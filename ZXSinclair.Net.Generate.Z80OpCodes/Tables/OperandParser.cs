#region LICENSE
/*
    ZXSinclair Emulador ZX Computers make in .Net and .Net CORE
    Copyright (C) 2016 Oscar Hernandez Bano
    This file is part of ZXSincalir.Net.
    ZXSincalir.Net is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/
#endregion

using System.Globalization;
using ZXSinclair.Net.Generate.Z80OpCodes.Model;

namespace ZXSinclair.Net.Generate.Z80OpCodes.Tables;

internal static class OperandParser
{
    public static Operand Parse(string text, string mnemonic, int position, SourceLocation source)
    {
        var condition = position == 0 && mnemonic is "JP" or "JR" or "CALL" or "RET";
        if (condition && text is "NZ" or "Z" or "NC" or "C" or "PO" or "PE" or "P" or "M")
            return new(OperandKind.Condition, text);
        if (mnemonic == "shift" && text is "CB" or "DD" or "ED" or "FD" or "DDFDCB")
            return new(OperandKind.PrefixTable, text);
        if (mnemonic == "RST")
        {
            if (int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var restart)
                && restart is >= 0 and <= 0x38 && (restart & 7) == 0)
                return new(OperandKind.RestartAddress, text, restart);
            throw new GeneratorException(source, $"Invalid RST address '{text}'.");
        }
        if (mnemonic == "IM")
        {
            if (text is "0" or "1" or "2")
                return new(OperandKind.InterruptMode, text, int.Parse(text, CultureInfo.InvariantCulture));
            throw new GeneratorException(source, $"Invalid IM mode '{text}'.");
        }
        if (mnemonic is "BIT" or "SET" or "RES" && position == 0)
        {
            if (text.Length == 1 && text[0] is >= '0' and <= '7')
                return new(OperandKind.Bit, text, text[0] - '0');
            throw new GeneratorException(source, $"Invalid bit number '{text}'.");
        }
        if (mnemonic == "OUT" && position == 1 && text == "0")
            return new(OperandKind.Constant, text, 0);
        var kind = text switch
        {
            "A" or "B" or "C" or "D" or "E" or "H" or "L" or "F" => OperandKind.Register8,
            "I" or "R" => OperandKind.SpecialRegister,
            "AF" or "BC" or "DE" or "HL" or "SP" or "AF'" => OperandKind.Register16,
            "REGISTER" => OperandKind.IndexPair,
            "REGISTERH" => OperandKind.IndexHigh,
            "REGISTERL" => OperandKind.IndexLow,
            "(HL)" or "(BC)" or "(DE)" or "(SP)" => OperandKind.MemoryPair,
            "(REGISTER+dd)" => OperandKind.IndexedMemory,
            "nn" => OperandKind.Immediate8,
            "nnnn" => OperandKind.Immediate16,
            "(nnnn)" => OperandKind.AbsoluteMemory,
            "offset" => OperandKind.RelativeOffset,
            "(C)" when mnemonic is "IN" or "OUT" => OperandKind.PortC,
            "(nn)" when mnemonic is "IN" or "OUT" => OperandKind.PortImmediate,
            _ => throw new GeneratorException(source, $"Unknown operand '{text}' in {mnemonic}."),
        };
        return new(kind, text);
    }
}
