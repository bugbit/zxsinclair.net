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

using ZXSinclair.Net.Core.Z80;

namespace ZXSinclair.Net.Core.Tests;

public class Z80RegistersTests
{
    [Fact]
    public void PairsOverlapTheirBytes()
    {
        Assert.True(BitConverter.IsLittleEndian);
        var r = new Z80Registers
        {
            AF = 0x1234, BC = 0x5678, DE = 0x9ABC, HL = 0xDEF0,
            IX = 0x1122, IY = 0x3344, IR = 0x5566,
        };
        Assert.Equal(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0,
                0x11, 0x22, 0x33, 0x44, 0x55, 0x66 },
            new[] { r.A, r.F, r.B, r.C, r.D, r.E, r.H, r.L,
                r.IXH, r.IXL, r.IYH, r.IYL, r.I, r.R });
        r.A = r.B = r.D = r.H = r.IXH = r.IYH = r.I = 0xAA;
        r.F = r.C = r.E = r.L = r.IXL = r.IYL = r.R = 0xBB;
        Assert.All(new[] { r.AF, r.BC, r.DE, r.HL, r.IX, r.IY, r.IR },
            pair => Assert.Equal((ushort)0xAABB, pair));
    }

    [Fact]
    public void ExchangesOnlyAffectTheRequestedRegisterSet()
    {
        var r = new Z80Registers
        {
            AF = 1, BC = 2, DE = 3, HL = 4, AF_ = 5, BC_ = 6, DE_ = 7, HL_ = 8,
            IX = 9, IY = 10, SP = 11, PC = 12, WZ = 13, IR = 14,
        };
        r.ExchangeAF();
        Assert.Equal((ushort)5, r.AF);
        Assert.Equal((ushort)1, r.AF_);
        Assert.Equal((ushort)2, r.BC);
        r.Exx();
        Assert.Equal(new ushort[] { 5, 6, 7, 8, 1, 2, 3, 4, 9, 10, 11, 12, 13, 14 },
            new[] { r.AF, r.BC, r.DE, r.HL, r.AF_, r.BC_, r.DE_, r.HL_,
                r.IX, r.IY, r.SP, r.PC, r.WZ, r.IR });
    }

    [Theory]
    [InlineData(0x7F, 0)]
    [InlineData(0xFF, 0x80)]
    [InlineData(0x80, 0x81)]
    public void IncrementRPreservesBitSeven(byte initial, byte expected)
    {
        var r = new Z80Registers { R = initial, I = 0x42 };
        r.IncrementR();
        Assert.Equal(expected, r.R);
        Assert.Equal((byte)0x42, r.I);
    }

    [Fact]
    public void IndexSelectorsReturnReferencesToTheRightFields()
    {
        var r = new Z80Registers();
        IxRegister.Pair(ref r) = 0x1234;
        IyRegister.Pair(ref r) = 0x5678;
        IxRegister.High(ref r) = 0xAB;
        IyRegister.Low(ref r) = 0xCD;
        Assert.Equal((ushort)0xAB34, r.IX);
        Assert.Equal((ushort)0x56CD, r.IY);
        Assert.Equal((byte)0x34, IxRegister.Low(ref r));
        Assert.Equal((byte)0x56, IyRegister.High(ref r));
    }
}
