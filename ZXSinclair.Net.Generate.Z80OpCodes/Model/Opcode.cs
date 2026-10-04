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
    /// <summary>Bytes read through PC after the opcode byte (immediates, offsets, addresses, port, DD/FD displacement).</summary>
    public int OperandBytes => Operands.Concat(InnerInstruction?.Operands ?? []).Sum(operand => operand.Kind switch
    {
        OperandKind.Immediate8 or OperandKind.RelativeOffset or OperandKind.PortImmediate => 1,
        OperandKind.Immediate16 or OperandKind.AbsoluteMemory => 2,
        // In DDFDCB the displacement precedes the opcode and is read by the prefix loop.
        OperandKind.IndexedMemory => Table == OpcodeTableKind.DDFDCB ? 0 : 1,
        _ => 0,
    });

    /// <summary>Prefix bytes (DD/FD CB d counts as three) plus the opcode and its operand bytes.</summary>
    public int Length => Table switch
    {
        OpcodeTableKind.Base => 0,
        OpcodeTableKind.DDFDCB => 3,
        _ => 1,
    } + 1 + OperandBytes;

    public string Comment => InnerInstruction is null
        ? Mnemonic + (Operands.Length == 0 ? "" : " " + string.Join(",", Operands.Select(o => o.Text)))
        : $"LD {CopyTo!.Text},{InnerInstruction.Mnemonic} {string.Join(",", InnerInstruction.Operands.Select(o => o.Text))}";
}
