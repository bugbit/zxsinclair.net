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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXSinclair.Machines.Z80;

namespace ZXSinclair.Test
{
    [TestClass]
    public class FlagsTest
    {
        [TestMethod]
        public void ParityMethod()
        {
            byte[] pBytes =
            {
                0b10,
                0b101,
                0b11100,
                0b11,
                0b1101,
                0b11011,
                0b10101011
            };
            bool pPar = false;

            foreach (var pByte in pBytes)
            {
                var pPV = Flags.GetParity(pByte);

                Assert.IsTrue((!pPar && pPV == 0) || (pPar && pPV != 0), $"Byte = {Convert.ToString(pByte, 2)} If Par = {pPar} PV = {pPV}");
                pPar = !pPar;
            }
        }
    }
}
