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
        new StoreSpecialAccumulator());

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
