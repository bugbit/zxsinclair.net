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

namespace ZXSinclair.Net.Generate.Z80OpCodes.Patterns;

internal sealed class NopPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic == "NOP" && opcode.Operands.Length == 0;
    public string EmitBody(Opcode opcode, EmitContext context) => "";
}


internal sealed class DecimalAdjustPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic == "DAA" && opcode.Operands.Length == 0;
    public bool WritesFlags(Opcode opcode) => true;
    public string EmitBody(Opcode opcode, EmitContext context) => "DecimalAdjust();";
}

internal sealed class ComplementPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic == "CPL" && opcode.Operands.Length == 0;
    public bool WritesFlags(Opcode opcode) => true;
    public string EmitBody(Opcode opcode, EmitContext context) => "Complement();";
}

internal sealed class NegatePattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic == "NEG" && opcode.Operands.Length == 0;
    public bool WritesFlags(Opcode opcode) => true;
    public string EmitBody(Opcode opcode, EmitContext context) => "Negate();";
}

internal sealed class SetCarryPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic == "SCF" && opcode.Operands.Length == 0;
    public bool WritesFlags(Opcode opcode) => true;
    public string EmitBody(Opcode opcode, EmitContext context) => "SetCarry();";
}

internal sealed class ComplementCarryPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic == "CCF" && opcode.Operands.Length == 0;
    public bool WritesFlags(Opcode opcode) => true;
    public string EmitBody(Opcode opcode, EmitContext context) => "ComplementCarry();";
}

internal sealed class HaltPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic == "HALT" && opcode.Operands.Length == 0;
    public string EmitBody(Opcode opcode, EmitContext context) => "Halt();";
}

internal sealed class DisableInterruptsPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic == "DI" && opcode.Operands.Length == 0;
    public string EmitBody(Opcode opcode, EmitContext context) => "Registers.IFF1 = Registers.IFF2 = false;";
}

internal sealed class EnableInterruptsPattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic == "EI" && opcode.Operands.Length == 0;
    public string EmitBody(Opcode opcode, EmitContext context) => "EnableInterrupts();";
}

internal sealed class InterruptModePattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic == "IM" && opcode.Operands.Length == 1
        && opcode.Operands[0] is { Kind: OperandKind.InterruptMode, Value: >= 0 and <= 2 };
    public string EmitBody(Opcode opcode, EmitContext context) =>
        $"Registers.IM = {opcode.Operands[0].Value};";
}
