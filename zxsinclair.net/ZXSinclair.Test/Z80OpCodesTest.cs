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
using ZXSinclair.Machines;
using ZXSinclair.Machines.Z80;

namespace ZXSinclair.Test
{
    [TestClass]
    public class Z80OpCodesTest
    {
        public class MachineZ80Test : MachineZ80
        {
            private RAM mRam = new RAM(0xFFFF);

            public MachineZ80Test()
            {
            }

            protected override IMemory[] CreateMemories()
            {
                return new[] { mRam };
            }

            public override byte PeekByte(int argAddress) => mRam.ReadMemory(argAddress & 0xFFFF);
            public override void Poke(int argAddress, byte argData) => mRam.WriteMemory(argAddress & 0xFFFF, argData);
            protected override void Sync()
            {
                //base.Sync();
                mFinishToken.Cancel();
            }
        }

        private MachineZ80Test mMachineTest;

        [TestInitialize]
        public void SetUp()
        {
            mMachineTest = new MachineZ80Test();
        }

        [TestMethod]
        public void LD_A_B_Method()
        {
            var pRegs = mMachineTest.Regs;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;

            mMachineTest.Poke(0, OpCodes.LD_A_B);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(pRegs.PC == 1);
            Assert.IsTrue(pRegs.A == 2);
            Assert.IsTrue(pRegs.B == 2);
            Assert.IsTrue(pRegs.C == 0);
            Assert.IsTrue(pRegs.D == 0);
            Assert.IsTrue(pRegs.E == 0);
            Assert.IsTrue(pRegs.H == 0);
            Assert.IsTrue(pRegs.L == 0);
        }
       
        [TestMethod]
        public void LD_A_C_Method()
        {
            var pRegs = mMachineTest.Regs;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;

            mMachineTest.Poke(0, OpCodes.LD_A_C);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(pRegs.PC == 1);
            Assert.IsTrue(pRegs.A == 3);
            Assert.IsTrue(pRegs.B == 2);
            Assert.IsTrue(pRegs.C == 3);
            Assert.IsTrue(pRegs.D == 0);
            Assert.IsTrue(pRegs.E == 0);
            Assert.IsTrue(pRegs.H == 0);
            Assert.IsTrue(pRegs.L == 0);
        }
    }
}
