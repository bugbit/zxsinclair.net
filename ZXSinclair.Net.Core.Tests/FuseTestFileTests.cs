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

using ZXSinclair.Net.Fuse;

namespace ZXSinclair.Net.Core.Tests;

public class FuseTestFileTests
{
    [Fact]
    public void LoadsTheWholeSuite()
    {
        var inputs = FuseTestFile.LoadInputs();
        var expected = FuseTestFile.LoadExpected();

        Assert.Equal(1335, inputs.Count);
        Assert.Equal(1335, expected.Count);
        Assert.All(inputs, test => Assert.True(expected.ContainsKey(test.Base.Name), test.Base.Name));
        Assert.Equal(12691, expected.Values.Sum(t => t.Events.Length));
    }

    [Fact]
    public void ParsesAnInputCase()
    {
        var tests = FuseTestFile.ParseInputs(new[]
        {
            "01",
            "0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000",
            "00 00 0 0 0 0     1",
            "0000 01 12 34 -1",
            "-1",
        });
        var test = Assert.Single(tests);

        Assert.Equal("01", test.Base.Name);
        Assert.Equal(1, test.Base.Line2.endtstates);
        Assert.Equal((ushort)0, Assert.Single(test.Base.Memories).Address);
        Assert.Equal(new byte[] { 0x01, 0x12, 0x34 }, test.Base.Memories[0].Data);
    }

    [Theory]
    [InlineData("0000 0000 0000", "00 00 0 0 0 0 1")]
    [InlineData("0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 zzzz", "00 00 0 0 0 0 1")]
    [InlineData("0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000", "00 00 x 0 0 0 1")]
    public void MalformedLinesThrowWithTheTestName(string line1, string line2)
    {
        var ex = Assert.Throws<FormatException>(() => FuseTestFile.ParseInputs(new[] { "bad", line1, line2, "-1" }));

        Assert.Contains("Test 'bad'", ex.Message);
    }

    [Fact]
    public void TruncatedFileThrows()
    {
        Assert.Throws<FormatException>(() => FuseTestFile.ParseInputs(new[] { "bad", "0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000" }));
    }
}
