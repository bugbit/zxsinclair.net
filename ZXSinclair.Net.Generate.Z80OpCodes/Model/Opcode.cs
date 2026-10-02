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

namespace ZXSinclair.Net.Generate.Z80OpCodes.Model;

internal sealed record SourceLocation(string File, int Line)
{
    public override string ToString() => $"{File}:{Line}";
}

internal sealed record Instruction(string Mnemonic, Operand[] Operands);

internal sealed record Opcode(OpcodeTableKind Table, byte Byte, string Mnemonic,
    Operand[] Operands, OpcodeKind Kind, SourceLocation Source,
    Operand? CopyTo = null, Instruction? InnerInstruction = null)
{
    public string Comment => InnerInstruction is null
        ? Mnemonic + (Operands.Length == 0 ? "" : " " + string.Join(",", Operands.Select(o => o.Text)))
        : $"LD {CopyTo!.Text},{InnerInstruction.Mnemonic} {string.Join(",", InnerInstruction.Operands.Select(o => o.Text))}";
}
