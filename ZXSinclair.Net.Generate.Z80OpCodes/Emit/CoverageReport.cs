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

namespace ZXSinclair.Net.Generate.Z80OpCodes.Emit;

internal static class CoverageReport
{
    public static void Write(IEnumerable<GeneratedDispatch> dispatches, TextWriter output, bool verbose)
    {
        foreach (var dispatch in dispatches)
        {
            var opcodes = dispatch.Opcodes;
            var implemented = opcodes.Count(o => o.Implemented);
            var prefixes = opcodes.Count(o => o.Opcode.Kind == OpcodeKind.Prefix);
            var holes = opcodes.Count(o => o.Opcode.Kind == OpcodeKind.Hole);
            var pending = opcodes.Count(o => !o.Implemented && o.Opcode.Kind != OpcodeKind.Prefix && o.Opcode.Kind != OpcodeKind.Hole);
            var pendingHoles = opcodes.Count(o => !o.Implemented && o.Opcode.Kind == OpcodeKind.Hole);
            var aliases = opcodes.Count(o => o.Opcode.Kind == OpcodeKind.Alias);
            output.WriteLine($"{dispatch.Table.ToString().ToLowerInvariant(),-7}: {implemented} implemented / {pending} pending / {prefixes} prefixes / {holes} holes ({pendingHoles} pending) / {aliases} aliases");
            if (verbose)
                foreach (var item in opcodes.Where(o => !o.Implemented && o.Opcode.Kind != OpcodeKind.Prefix))
                    output.WriteLine($"  0x{item.Opcode.Byte:X2} {item.Opcode.Comment} [{item.Opcode.Kind}]");
        }
    }
}
