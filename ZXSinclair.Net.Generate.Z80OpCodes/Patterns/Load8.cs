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

internal abstract class Load8Pattern : IPattern
{
    public bool Matches(Opcode opcode) => opcode.Kind is OpcodeKind.Instruction or OpcodeKind.Alias
        && opcode.Mnemonic == "LD" && opcode.Operands.Length == 2
        && Matches(opcode.Operands[0], opcode.Operands[1]);

    protected static bool ByteRegister(Operand operand) => operand.Kind is
        OperandKind.Register8 or OperandKind.IndexHigh or OperandKind.IndexLow;
    protected static bool PairMemory(Operand operand) => operand.Kind == OperandKind.MemoryPair
        && operand.Text is "(HL)" or "(BC)" or "(DE)";
    protected abstract bool Matches(Operand d, Operand s);
    protected abstract string Emit(Operand d, Operand s, EmitContext context);
    public string EmitBody(Opcode opcode, EmitContext context) =>
        Emit(opcode.Operands[0], opcode.Operands[1], context);
}

internal sealed class LoadRegisterRegister : Load8Pattern
{
    protected override bool Matches(Operand d, Operand s) => ByteRegister(d) && ByteRegister(s);
    protected override string Emit(Operand d, Operand s, EmitContext context) =>
        d == s ? "" : $"{Value(d, context)} = {Value(s, context)};";
}

internal sealed class LoadRegisterImmediate : Load8Pattern
{
    protected override bool Matches(Operand d, Operand s) => ByteRegister(d) && s.Kind == OperandKind.Immediate8;
    protected override string Emit(Operand d, Operand s, EmitContext context) =>
        $"{Value(d, context)} = ReadPc();";
}

internal sealed class LoadRegisterIndirect : Load8Pattern
{
    protected override bool Matches(Operand d, Operand s) => d.Kind == OperandKind.Register8
        && PairMemory(s) && (s.Text == "(HL)" || d.Text == "A");
    protected override string Emit(Operand d, Operand s, EmitContext context) =>
        $"{Value(d, context)} = {Value(s, context)};" + (s.Text == "(HL)" ? "" : $"\nRegisters.WZ = (ushort)({Address(s, context)} + 1);");
}

internal sealed class StoreIndirectRegister : Load8Pattern
{
    protected override bool Matches(Operand d, Operand s) => PairMemory(d)
        && s.Kind == OperandKind.Register8 && (d.Text == "(HL)" || s.Text == "A");
    protected override string Emit(Operand d, Operand s, EmitContext context) =>
        $"bus.Write({Address(d, context)}, {Value(s, context)});" + (d.Text == "(HL)" ? "" : $"\nRegisters.WZ = (ushort)((Registers.A << 8) | (({Address(d, context)} + 1) & 0xFF));");
}

internal sealed class StoreIndirectImmediate : Load8Pattern
{
    protected override bool Matches(Operand d, Operand s) => d.Kind == OperandKind.MemoryPair && d.Text == "(HL)" && s.Kind == OperandKind.Immediate8;
    protected override string Emit(Operand d, Operand s, EmitContext context) =>
        "var value = ReadPc();\nbus.Write(Registers.HL, value);";
}

internal sealed class LoadAccumulatorAbsolute : Load8Pattern
{
    protected override bool Matches(Operand d, Operand s) => d.Kind == OperandKind.Register8 && d.Text == "A" && s.Kind == OperandKind.AbsoluteMemory;
    protected override string Emit(Operand d, Operand s, EmitContext context) =>
        "var address = ReadPc16();\nRegisters.A = bus.Read(address);\nRegisters.WZ = (ushort)(address + 1);";
}

internal sealed class StoreAbsoluteAccumulator : Load8Pattern
{
    protected override bool Matches(Operand d, Operand s) => d.Kind == OperandKind.AbsoluteMemory && s.Kind == OperandKind.Register8 && s.Text == "A";
    protected override string Emit(Operand d, Operand s, EmitContext context) =>
        "var address = ReadPc16();\nbus.Write(address, Registers.A);\nRegisters.WZ = (ushort)((Registers.A << 8) | ((address + 1) & 0xFF));";
}

internal sealed class LoadRegisterIndexed : Load8Pattern
{
    protected override bool Matches(Operand d, Operand s) => d.Kind == OperandKind.Register8 && s.Kind == OperandKind.IndexedMemory;
    protected override string Emit(Operand d, Operand s, EmitContext context) =>
        $"var address = IndexedAddress<TIndex>();\n{Value(d, context)} = bus.Read(address);";
}

internal sealed class StoreIndexedRegister : Load8Pattern
{
    protected override bool Matches(Operand d, Operand s) => d.Kind == OperandKind.IndexedMemory && s.Kind == OperandKind.Register8;
    protected override string Emit(Operand d, Operand s, EmitContext context) =>
        $"var address = IndexedAddress<TIndex>();\nbus.Write(address, {Value(s, context)});";
}

internal sealed class StoreIndexedImmediate : Load8Pattern
{
    protected override bool Matches(Operand d, Operand s) => d.Kind == OperandKind.IndexedMemory && s.Kind == OperandKind.Immediate8;
    protected override string Emit(Operand d, Operand s, EmitContext context) =>
        "StoreIndexedImmediate<TIndex>();";
}

internal sealed class LoadAccumulatorSpecial : Load8Pattern
{
    protected override bool Matches(Operand d, Operand s) => d.Kind == OperandKind.Register8 && d.Text == "A" && s.Kind == OperandKind.SpecialRegister;
    protected override string Emit(Operand d, Operand s, EmitContext context) =>
        $"LoadAFromSpecial({Value(s, context)});";
}

internal sealed class StoreSpecialAccumulator : Load8Pattern
{
    protected override bool Matches(Operand d, Operand s) => d.Kind == OperandKind.SpecialRegister && s.Kind == OperandKind.Register8 && s.Text == "A";
    protected override string Emit(Operand d, Operand s, EmitContext context) =>
        $"bus.Internal(Registers.IR, 1);\n{Value(d, context)} = Registers.A;";
}
