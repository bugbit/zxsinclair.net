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



using ZXSinclair.Net.Generate.Z80OpCodes.Model;
using static ZXSinclair.Net.Generate.Z80OpCodes.Emit.OperandEmitter;

namespace ZXSinclair.Net.Generate.Z80OpCodes.Patterns;

internal sealed class ExchangeRegistersPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Mnemonic == "EXX" && opcode.Operands.Length == 0
        || opcode.Mnemonic == "EX" && opcode.Operands.Length == 2
        && ((opcode.Operands[0].Text == "DE" && opcode.Operands[1].Text == "HL")
            || (opcode.Operands[0].Text == "AF" && opcode.Operands[1].Text == "AF'"));
    public string EmitBody(Opcode opcode, EmitContext context) => opcode.Mnemonic == "EXX"
        ? "Registers.Exx();" : opcode.Operands[0].Text == "AF"
        ? "Registers.ExchangeAF();" : "(Registers.DE, Registers.HL) = (Registers.HL, Registers.DE);";
}

internal sealed class ExchangeStackPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Mnemonic == "EX" && opcode.Operands.Length == 2
        && opcode.Operands[0] is { Kind: OperandKind.MemoryPair, Text: "(SP)" }
        && (opcode.Operands[1] is { Kind: OperandKind.Register16, Text: "HL" }
            || opcode.Operands[1].Kind == OperandKind.IndexPair);
    public string EmitBody(Opcode opcode, EmitContext context)
    {
        var target = Value(opcode.Operands[1], context);
        return $"{target} = ExchangeStack({target});";
    }
}

internal sealed class BlockLoadPattern : IPattern
{
    public bool WritesFlags(Opcode opcode) => true;
    public bool Matches(Opcode opcode) => opcode.Table == OpcodeTableKind.ED
        && opcode.Operands.Length == 0 && opcode.Mnemonic is "LDI" or "LDD" or "LDIR" or "LDDR";
    public string EmitBody(Opcode opcode, EmitContext context) =>
        $"{(opcode.Mnemonic.Length == 4 ? "BlockLoadRepeat" : "BlockLoad")}({(opcode.Mnemonic[2] == 'I' ? 1 : -1)});";
}

internal sealed class BlockComparePattern : IPattern
{
    public bool WritesFlags(Opcode opcode) => true;
    public bool Matches(Opcode opcode) => opcode.Table == OpcodeTableKind.ED
        && opcode.Operands.Length == 0 && opcode.Mnemonic is "CPI" or "CPD" or "CPIR" or "CPDR";
    public string EmitBody(Opcode opcode, EmitContext context) =>
        $"{(opcode.Mnemonic.Length == 4 ? "BlockCompareRepeat" : "BlockCompare")}({(opcode.Mnemonic[2] == 'I' ? 1 : -1)});";
}
