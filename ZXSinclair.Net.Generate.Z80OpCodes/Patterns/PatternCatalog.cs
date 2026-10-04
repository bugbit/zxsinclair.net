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

internal sealed class PatternCatalog(params IPattern[] patterns)
{
    public static PatternCatalog Default { get; } = new(
        new NopPattern(),
        new EdHolePattern(),
        new LoadRegisterRegister(),
        new LoadRegisterImmediate(),
        new LoadRegisterIndirect(),
        new StoreIndirectRegister(),
        new StoreIndirectImmediate(),
        new LoadAccumulatorAbsolute(),
        new StoreAbsoluteAccumulator(),
        new LoadRegisterIndexed(),
        new StoreIndexedRegister(),
        new StoreIndexedImmediate(),
        new LoadAccumulatorSpecial(),
        new StoreSpecialAccumulator(),
        new LoadPairImmediate(),
        new LoadPairAbsolute(),
        new StoreAbsolutePair(),
        new LoadStackPointer(),
        new PushPair(),
        new PopPair(),
        new JumpAbsolutePattern(),
        new JumpRegister(),
        new JumpRelativePattern(),
        new DecrementJump(),
        new CallPattern(),
        new ReturnPattern(),
        new ReturnConditionalPattern(),
        new ReturnFromInterruptPattern(),
        new RestartPattern(),
        new Alu8Pattern(),
        new IncDec8Register(),
        new IncDec8Memory(),
        new DecimalAdjustPattern(),
        new ComplementPattern(),
        new NegatePattern(),
        new SetCarryPattern(),
        new ComplementCarryPattern(),
        new HaltPattern(),
        new DisableInterruptsPattern(),
        new EnableInterruptsPattern(),
        new InterruptModePattern(),
        new Add16Pattern(),
        new AdcSbc16Pattern(),
        new IncDec16Pattern(),
        new RotateAccumulatorPattern(),
        new RotateRegisterPattern(),
        new RotateMemoryPattern(),
        new RotateMemoryCopyPattern(),
        new RotateDigitPattern(),
        new BitTestRegisterPattern(),
        new BitTestMemoryPattern(),
        new SetResRegisterPattern(),
        new SetResMemoryPattern(),
        new SetResMemoryCopyPattern(),
        new ExchangeRegistersPattern(),
        new ExchangeStackPattern(),
        new BlockLoadPattern(),
        new BlockComparePattern(),
        new InImmediatePattern(),
        new OutImmediatePattern(),
        new InRegisterPattern(),
        new OutRegisterPattern(),
        new BlockIoPattern());

    public IPattern? Resolve(Opcode opcode)
    {
        IPattern? match = null;
        foreach (var pattern in patterns)
        {
            if (!pattern.Matches(opcode)) continue;
            if (match is not null)
                throw new GeneratorException(opcode.Source, $"Multiple patterns match {opcode.Table} 0x{opcode.Byte:X2} ({opcode.Comment}).");
            match = pattern;
        }
        return match;
    }
}
