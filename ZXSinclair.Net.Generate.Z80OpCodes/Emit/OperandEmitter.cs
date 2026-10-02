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
using ZXSinclair.Net.Generate.Z80OpCodes.Patterns;

namespace ZXSinclair.Net.Generate.Z80OpCodes.Emit;

// These are expression translations, not instruction bodies. Patterns control evaluation
// order, read/write cycles, address caching and WZ according to each instruction spec.
internal static class OperandEmitter
{
    public static string Value(Operand operand, EmitContext context) => operand.Kind switch
    {
        OperandKind.Register8 or OperandKind.SpecialRegister or OperandKind.Register16 =>
            "Registers." + (operand.Text == "AF'" ? "AF_" : operand.Text),
        OperandKind.IndexPair => "TIndex.Pair(ref Registers)",
        OperandKind.IndexHigh => "TIndex.High(ref Registers)",
        OperandKind.IndexLow => "TIndex.Low(ref Registers)",
        OperandKind.Immediate8 => "ReadPc()",
        OperandKind.Immediate16 => "ReadPc16()",
        OperandKind.RelativeOffset => "(sbyte)ReadPc()",
        OperandKind.MemoryPair or OperandKind.IndexedMemory or OperandKind.AbsoluteMemory =>
            $"bus.Read({Address(operand, context)})",
        OperandKind.PortC or OperandKind.PortImmediate => Address(operand, context),
        OperandKind.Condition => Condition(operand.Text),
        OperandKind.Bit or OperandKind.RestartAddress or OperandKind.InterruptMode or OperandKind.Constant =>
            operand.Value!.Value.ToString(CultureInfo.InvariantCulture),
        _ => throw new GeneratorException($"Cannot emit value for operand '{operand.Text}'."),
    };

    public static string Address(Operand operand, EmitContext context) => operand.Kind switch
    {
        OperandKind.MemoryPair => "Registers." + operand.Text[1..^1],
        OperandKind.IndexedMemory => context.IndexedCB ? "address" : "IndexedAddress<TIndex>()",
        OperandKind.AbsoluteMemory => "ReadPc16()",
        OperandKind.PortC => "Registers.BC",
        OperandKind.PortImmediate => "(ushort)((Registers.A << 8) | ReadPc())",
        _ => throw new GeneratorException($"Operand '{operand.Text}' has no address expression."),
    };

    private static string Condition(string condition)
    {
        var (flag, set) = condition switch
        {
            "NZ" => ("Z", false), "Z" => ("Z", true), "NC" => ("C", false), "C" => ("C", true),
            "PO" => ("PV", false), "PE" => ("PV", true), "P" => ("S", false), "M" => ("S", true),
            _ => throw new GeneratorException($"Unknown condition '{condition}'."),
        };
        return $"(Registers.F & Z80Flags.{flag}) {(set ? "!=" : "==")} 0";
    }
}
