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
    public partial class Z80OpCodesTest
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
            //protected override void Sync()
            //{
            //    //base.Sync();
            //    mFinishToken.Cancel();
            //}
        }

        private MachineZ80Test mMachineTest;

        [TestInitialize]
        public void SetUp()
        {
            mMachineTest = new MachineZ80Test();
        }

        [TestMethod]
        public void LD_A_N_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_A_N);
            mMachineTest.Poke(1, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 2, "PC");
            Assert.IsTrue(pRegs.A == 205, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }

        [TestMethod]
        public void LD_B_N_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_B_N);
            mMachineTest.Poke(1, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 2, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 205, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }
        [TestMethod]
        public void LD_C_N_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_C_N);
            mMachineTest.Poke(1, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 2, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 205, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }
        [TestMethod]
        public void LD_D_N_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_D_N);
            mMachineTest.Poke(1, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 2, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 205, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }
        [TestMethod]
        public void LD_E_N_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_E_N);
            mMachineTest.Poke(1, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 2, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 205, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }
        [TestMethod]
        public void LD_H_N_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_H_N);
            mMachineTest.Poke(1, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 2, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 205, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }
        [TestMethod]
        public void LD_L_N_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_L_N);
            mMachineTest.Poke(1, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 2, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 205, "L");
        }
        [TestMethod]
        public void LD_A_M_HL_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_A_M_HL_M);
            mMachineTest.Poke(pRegs.HL, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 205, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }
        [TestMethod]
        public void LD_B_M_HL_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_B_M_HL_M);
            mMachineTest.Poke(pRegs.HL, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 205, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }
        [TestMethod]
        public void LD_C_M_HL_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_C_M_HL_M);
            mMachineTest.Poke(pRegs.HL, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 205, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }
        [TestMethod]
        public void LD_D_M_HL_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_D_M_HL_M);
            mMachineTest.Poke(pRegs.HL, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 205, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }
        [TestMethod]
        public void LD_E_M_HL_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_E_M_HL_M);
            mMachineTest.Poke(pRegs.HL, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 205, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }
        [TestMethod]
        public void LD_H_M_HL_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_H_M_HL_M);
            mMachineTest.Poke(pRegs.HL, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 205, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }
        [TestMethod]
        public void LD_L_M_HL_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_L_M_HL_M);
            mMachineTest.Poke(pRegs.HL, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 205, "L");
        }
        [TestMethod]
        public void LD_A_M_IX_D_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 100;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0xFFFF;

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_A_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.GetIX_d((sbyte)d), 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 205, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IX == 0xFFFF, "IX");
        }
        [TestMethod]
        public void LD_B_M_IX_D_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0x6776;

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_B_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.GetIX_d((sbyte)d), 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 205, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
        }
        [TestMethod]
        public void LD_C_M_IX_D_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0x6776;

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_C_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.GetIX_d((sbyte)d), 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 205, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
        }
        [TestMethod]
        public void LD_D_M_IX_D_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0x6776;

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_D_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.GetIX_d((sbyte)d), 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 205, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
        }
        [TestMethod]
        public void LD_E_M_IX_D_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0x6776;

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_E_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.GetIX_d((sbyte)d), 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 205, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
        }
        [TestMethod]
        public void LD_H_M_IX_D_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0x6776;

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_H_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.GetIX_d((sbyte)d), 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 205, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
        }
        [TestMethod]
        public void LD_L_M_IX_D_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0x6776;

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_L_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.GetIX_d((sbyte)d), 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 205, "L");
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
        }
        [TestMethod]
        public void LD_A_M_IY_D_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_A_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.GetIY_d((sbyte)d), 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 205, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
        }
        [TestMethod]
        public void LD_B_M_IY_D_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_B_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.GetIY_d((sbyte)d), 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 205, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
        }
        [TestMethod]
        public void LD_C_M_IY_D_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_C_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.GetIY_d((sbyte)d), 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 205, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
        }
        [TestMethod]
        public void LD_D_M_IY_D_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_D_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.GetIY_d((sbyte)d), 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 205, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
        }
        [TestMethod]
        public void LD_E_M_IY_D_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_E_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.GetIY_d((sbyte)d), 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 205, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
        }
        [TestMethod]
        public void LD_H_M_IY_D_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_H_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.GetIY_d((sbyte)d), 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 205, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
        }
        [TestMethod]
        public void LD_L_M_IY_D_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_L_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.GetIY_d((sbyte)d), 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 205, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
        }
        [TestMethod]
        public void LD_M_HL_M_A_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            var pHL = pRegs.HL;

            mMachineTest.Poke(0, OpCodes.LD_M_HL_M_A);
            mMachineTest.Poke(pRegs.HL, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(mMachineTest.PeekByte(pHL) == 1, "(HL)");
        }
        [TestMethod]
        public void LD_M_HL_M_B_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            var pHL = pRegs.HL;

            mMachineTest.Poke(0, OpCodes.LD_M_HL_M_B);
            mMachineTest.Poke(pRegs.HL, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(mMachineTest.PeekByte(pHL) == 2, "(HL)");
        }
        [TestMethod]
        public void LD_M_HL_M_C_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            var pHL = pRegs.HL;

            mMachineTest.Poke(0, OpCodes.LD_M_HL_M_C);
            mMachineTest.Poke(pRegs.HL, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(mMachineTest.PeekByte(pHL) == 3, "(HL)");
        }
        [TestMethod]
        public void LD_M_HL_M_D_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            var pHL = pRegs.HL;

            mMachineTest.Poke(0, OpCodes.LD_M_HL_M_D);
            mMachineTest.Poke(pRegs.HL, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(mMachineTest.PeekByte(pHL) == 4, "(HL)");
        }
        [TestMethod]
        public void LD_M_HL_M_E_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            var pHL = pRegs.HL;

            mMachineTest.Poke(0, OpCodes.LD_M_HL_M_E);
            mMachineTest.Poke(pRegs.HL, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(mMachineTest.PeekByte(pHL) == 5, "(HL)");
        }
        [TestMethod]
        public void LD_M_HL_M_H_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            var pHL = pRegs.HL;

            mMachineTest.Poke(0, OpCodes.LD_M_HL_M_H);
            mMachineTest.Poke(pRegs.HL, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(mMachineTest.PeekByte(pHL) == 6, "(HL)");
        }
        [TestMethod]
        public void LD_M_HL_M_L_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            var pHL = pRegs.HL;

            mMachineTest.Poke(0, OpCodes.LD_M_HL_M_L);
            mMachineTest.Poke(pRegs.HL, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(mMachineTest.PeekByte(pHL) == 7, "(HL)");
        }
        [TestMethod]
        public void LD_M_IX_D_M_A_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0x6776;

            var pIX_d = pRegs.GetIX_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_M_IX_D_M_A);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pIX_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
            Assert.IsTrue(mMachineTest.PeekByte(pIX_d) == 1, "(IX+d)");
        }
        [TestMethod]
        public void LD_M_IX_D_M_B_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0x6776;

            var pIX_d = pRegs.GetIX_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_M_IX_D_M_B);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pIX_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
            Assert.IsTrue(mMachineTest.PeekByte(pIX_d) == 2, "(IX+d)");
        }
        [TestMethod]
        public void LD_M_IX_D_M_C_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0x6776;

            var pIX_d = pRegs.GetIX_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_M_IX_D_M_C);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pIX_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
            Assert.IsTrue(mMachineTest.PeekByte(pIX_d) == 3, "(IX+d)");
        }
        [TestMethod]
        public void LD_M_IX_D_M_D_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0x6776;

            var pIX_d = pRegs.GetIX_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_M_IX_D_M_D);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pIX_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
            Assert.IsTrue(mMachineTest.PeekByte(pIX_d) == 4, "(IX+d)");
        }
        [TestMethod]
        public void LD_M_IX_D_M_E_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0x6776;

            var pIX_d = pRegs.GetIX_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_M_IX_D_M_E);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pIX_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
            Assert.IsTrue(mMachineTest.PeekByte(pIX_d) == 5, "(IX+d)");
        }
        [TestMethod]
        public void LD_M_IX_D_M_H_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0x6776;

            var pIX_d = pRegs.GetIX_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_M_IX_D_M_H);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pIX_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
            Assert.IsTrue(mMachineTest.PeekByte(pIX_d) == 6, "(IX+d)");
        }
        [TestMethod]
        public void LD_M_IX_D_M_L_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0x6776;

            var pIX_d = pRegs.GetIX_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_M_IX_D_M_L);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pIX_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
            Assert.IsTrue(mMachineTest.PeekByte(pIX_d) == 7, "(IX+d)");
        }

        [TestMethod]
        public void LD_M_IY_D_M_A_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            var pIY_d = pRegs.GetIY_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_M_IY_D_M_A);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pIY_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
            Assert.IsTrue(mMachineTest.PeekByte(pIY_d) == 1, "(IY+d)");
        }
        [TestMethod]
        public void LD_M_IY_D_M_B_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            var pIY_d = pRegs.GetIY_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_M_IY_D_M_B);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pIY_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
            Assert.IsTrue(mMachineTest.PeekByte(pIY_d) == 2, "(IY+d)");
        }
        [TestMethod]
        public void LD_M_IY_D_M_C_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            var pIY_d = pRegs.GetIY_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_M_IY_D_M_C);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pIY_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
            Assert.IsTrue(mMachineTest.PeekByte(pIY_d) == 3, "(IY+d)");
        }
        [TestMethod]
        public void LD_M_IY_D_M_D_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            var pIY_d = pRegs.GetIY_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_M_IY_D_M_D);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pIY_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
            Assert.IsTrue(mMachineTest.PeekByte(pIY_d) == 4, "(IY+d)");
        }
        [TestMethod]
        public void LD_M_IY_D_M_E_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            var pIY_d = pRegs.GetIY_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_M_IY_D_M_E);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pIY_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
            Assert.IsTrue(mMachineTest.PeekByte(pIY_d) == 5, "(IY+d)");
        }
        [TestMethod]
        public void LD_M_IY_D_M_H_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            var pIY_d = pRegs.GetIY_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_M_IY_D_M_H);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pIY_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
            Assert.IsTrue(mMachineTest.PeekByte(pIY_d) == 6, "(IY+d)");
        }
        [TestMethod]
        public void LD_M_IY_D_M_L_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            var pIY_d = pRegs.GetIY_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_M_IY_D_M_L);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pIY_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
            Assert.IsTrue(mMachineTest.PeekByte(pIY_d) == 7, "(IY+d)");
        }
        [TestMethod]
        public void LD_M_HL_M_N_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte n = 205;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            var pHL = pRegs.HL;

            mMachineTest.Poke(0, OpCodes.LD_M_HL_M_N);
            mMachineTest.Poke(1, n);
            mMachineTest.Poke(pRegs.HL, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 3 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 2, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(mMachineTest.PeekByte(pHL) == n, "(HL)");
        }
        [TestMethod]
        public void LD_M_IX_D_M_N_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;
            byte n = 205;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IX = 0x6776;

            var pIX_d = pRegs.GetIX_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_DD);
            mMachineTest.Poke(1, OpCodes.LD_M_IX_D_M_N);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(3, n);
            mMachineTest.Poke(pIX_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 4, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
            Assert.IsTrue(mMachineTest.PeekByte(pIX_d) == n, "(IX+d)");
        }
        [TestMethod]
        public void LD_M_IY_D_M_N_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            byte d = 201;
            byte n = 205;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.IY = 0x6776;

            var pIY_d = pRegs.GetIY_d((sbyte)d);

            mMachineTest.Poke(0, OpCodes.OPCODES_FD);
            mMachineTest.Poke(1, OpCodes.LD_M_IY_D_M_N);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(3, n);
            mMachineTest.Poke(pIY_d, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 4 + 3 + 5 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 4, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.IY == 0x6776, "IY");
            Assert.IsTrue(mMachineTest.PeekByte(pIY_d) == n, "(IX+d)");
        }
        [TestMethod]
        public void LD_A_M_BC_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_A_M_BC_M);
            mMachineTest.Poke(pRegs.BC, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 205, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }
        [TestMethod]
        public void LD_A_M_DE_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_A_M_DE_M);
            mMachineTest.Poke(pRegs.DE, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 7, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 205, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }
        [TestMethod]
        public void LD_A_M_NN_M_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            mMachineTest.Poke(0, OpCodes.LD_A_M_NN_M);
            mMachineTest.Poke(1, 0x1);
            mMachineTest.Poke(2, 0x40);
            mMachineTest.Poke(0x4001, 205);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 3 + 3 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 205, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }
        [TestMethod]
        public void LD_M_BC_M_A_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            var pBC = pRegs.BC;

            mMachineTest.Poke(0, OpCodes.LD_M_BC_M_A);
            mMachineTest.Poke(pRegs.BC, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(mMachineTest.PeekByte(pBC) == 1, "(BC)");
        }
        [TestMethod]
        public void LD_M_DE_M_A_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            var pDE = pRegs.DE;

            mMachineTest.Poke(0, OpCodes.LD_M_DE_M_A);
            mMachineTest.Poke(pRegs.DE, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(mMachineTest.PeekByte(pDE) == 1, "(BC)");
        }
        [TestMethod]
        public void LD_M_NN_M_A_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;

            var pNN = 0x4001;

            mMachineTest.Poke(0, OpCodes.LD_M_NN_M_A);
            mMachineTest.Poke(1, 0x1);
            mMachineTest.Poke(2, 0x40);
            mMachineTest.Poke(pNN, 0);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 3 + 3 + 3, "TState");
            Assert.IsTrue(pRegs.PC == 3, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(mMachineTest.PeekByte(pNN) == 1, "(NN)");
        }
        [TestMethod]
        public void LD_A_I_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            var n = (byte)0b11110111;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.I = n;
            pRegs.F = 0xFF;

            mMachineTest.Poke(0, OpCodes.OPCODES_ED);
            mMachineTest.Poke(1, OpCodes.LD_A_I);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 5, "TState");
            Assert.IsTrue(pRegs.PC == 2, "PC");
            Assert.IsTrue(pRegs.A == n, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue((pRegs.F & Flags.S) != 0, "Flags S");
            Assert.IsTrue((pRegs.F & Flags.Z) == 0, "Flags Z");
            Assert.IsTrue((pRegs.F & Flags.H) == 0, "Flags H");
            Assert.IsTrue((pRegs.F & Flags.PV) == 0, "Flags PV");
            Assert.IsTrue((pRegs.F & Flags.N) == 0, "Flags N");
            Assert.IsTrue((pRegs.F & Flags.C) != 0, "Flags C");
        }
        [TestMethod]
        public void LD_A_R_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            var n = (byte)0b11110111;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.R = n;
            pRegs.F = 0xFF;

            mMachineTest.Poke(0, OpCodes.OPCODES_ED);
            mMachineTest.Poke(1, OpCodes.LD_A_R);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 5, "TState");
            Assert.IsTrue(pRegs.PC == 2, "PC");
            Assert.IsTrue(pRegs.A == n + 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue((pRegs.F & Flags.S) != 0, "Flags S");
            Assert.IsTrue((pRegs.F & Flags.Z) == 0, "Flags Z");
            Assert.IsTrue((pRegs.F & Flags.H) == 0, "Flags H");
            Assert.IsTrue((pRegs.F & Flags.PV) == 0, "Flags PV");
            Assert.IsTrue((pRegs.F & Flags.N) == 0, "Flags N");
            Assert.IsTrue((pRegs.F & Flags.C) != 0, "Flags C");
        }
        [TestMethod]
        public void LD_I_A_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            var n = (byte)0b11110111;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.I = n;
            pRegs.F = 0xFF;

            mMachineTest.Poke(0, OpCodes.OPCODES_ED);
            mMachineTest.Poke(1, OpCodes.LD_I_A);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 5, "TState");
            Assert.IsTrue(pRegs.PC == 2, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.I == 1, "I");
        }
        [TestMethod]
        public void LD_R_A_Method()
        {
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;
            var n = (byte)0b11110111;

            pRegs.Reset();
            pRegs.A = 1;
            pRegs.B = 2;
            pRegs.C = 3;
            pRegs.D = 4;
            pRegs.E = 5;
            pRegs.H = 6;
            pRegs.L = 7;
            pRegs.R = n;
            pRegs.F = 0xFF;

            mMachineTest.Poke(0, OpCodes.OPCODES_ED);
            mMachineTest.Poke(1, OpCodes.LD_R_A);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4 + 5, "TState");
            Assert.IsTrue(pRegs.PC == 2, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
            Assert.IsTrue(pRegs.R == 1, "I");
        }
    }
}
