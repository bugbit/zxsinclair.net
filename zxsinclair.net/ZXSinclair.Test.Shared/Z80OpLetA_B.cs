#region LICENSE
/*
    ZXSinclair Emulador ZX Computers make in .Net and .Net CORE
    Copyright (C) 2016 Oscar Hernandez Bano
    This file is part of ZEsarUX.
    ZEsarUX is free software: you can redistribute it and/or modify
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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZXSinclair.NetCore.Test
{
    [TestClass]
    public class Z80OpLetA_B
    {
        private CPU.Z80.Ops mOps = new CPU.Z80.Ops();

        [TestInitialize]
        public void SetUp()
        {
            mOps = new CPU.Z80.Ops();
            TestLDA_BMethod1();
            TestLDA_BMethod2();
            TestLDA_BMethod3();
        }

        [TestMethod]
        public void TestLDA_BMethod1()
        {
            for (var i = 0; i < 200000; i++)
                mOps.LdA_B_1();
        }

        [TestMethod]
        public void TestLDA_BMethod2()
        {
            for (var i = 0; i < 200000; i++)
                mOps.LdA_B_2();
        }

        [TestMethod]
        public void TestLDA_BMethod3()
        {
            for (var i = 0; i < 200000; i++)
                mOps.LdA_B_3();
        }
    }
}
