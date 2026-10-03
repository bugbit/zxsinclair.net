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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ZXSinclair.Net.Core.Z80;

[StructLayout(LayoutKind.Explicit)]
public struct Z80Registers
{
    [FieldOffset(0)] public ushort AF;
    [FieldOffset(0)] public byte F;
    [FieldOffset(1)] public byte A;
    [FieldOffset(2)] public ushort BC;
    [FieldOffset(2)] public byte C;
    [FieldOffset(3)] public byte B;
    [FieldOffset(4)] public ushort DE;
    [FieldOffset(4)] public byte E;
    [FieldOffset(5)] public byte D;
    [FieldOffset(6)] public ushort HL;
    [FieldOffset(6)] public byte L;
    [FieldOffset(7)] public byte H;
    [FieldOffset(8)] public ushort AF_;
    [FieldOffset(10)] public ushort BC_;
    [FieldOffset(12)] public ushort DE_;
    [FieldOffset(14)] public ushort HL_;
    [FieldOffset(16)] public ushort IX;
    [FieldOffset(16)] public byte IXL;
    [FieldOffset(17)] public byte IXH;
    [FieldOffset(18)] public ushort IY;
    [FieldOffset(18)] public byte IYL;
    [FieldOffset(19)] public byte IYH;
    [FieldOffset(20)] public ushort SP;
    [FieldOffset(22)] public ushort PC;
    [FieldOffset(24)] public ushort WZ;
    [FieldOffset(26)] public ushort IR;
    [FieldOffset(26)] public byte R;
    [FieldOffset(27)] public byte I;
    [FieldOffset(28)] public bool IFF1;
    [FieldOffset(29)] public bool IFF2;
    [FieldOffset(30)] public byte IM;
    [FieldOffset(31)] public bool Halted;
    [FieldOffset(32)] public bool EiPending;
    [FieldOffset(33)] public byte Q;
    [FieldOffset(34)] public bool SpecialLoadPending;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IncrementR() => R = (byte)((R & 0x80) | ((R + 1) & 0x7F));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExchangeAF() => (AF, AF_) = (AF_, AF);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Exx()
    {
        (BC, BC_) = (BC_, BC);
        (DE, DE_) = (DE_, DE);
        (HL, HL_) = (HL_, HL);
    }
}
