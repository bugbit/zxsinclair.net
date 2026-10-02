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

internal enum OperandKind
{
    Register8, SpecialRegister, Register16, IndexPair, IndexHigh, IndexLow,
    MemoryPair, IndexedMemory, Immediate8, Immediate16, AbsoluteMemory,
    RelativeOffset, Condition, PortC, PortImmediate, Bit, RestartAddress,
    InterruptMode, Constant, PrefixTable,
}

internal sealed record Operand(OperandKind Kind, string Text, int? Value = null);
