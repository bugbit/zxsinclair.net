﻿#region LICENSE
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
            mMachineTest.Poke(1, OpCodes.LD_A_M_HL_M);
            mMachineTest.Poke(2, d);
            mMachineTest.Poke(pRegs.IX + d, 205);
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
            Assert.IsTrue(pRegs.IX == 0x6776, "IX");
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
            mMachineTest.Poke(pRegs.IX + d, 205);
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
            mMachineTest.Poke(pRegs.IX + d, 205);
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
            mMachineTest.Poke(pRegs.IX + d, 205);
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
            mMachineTest.Poke(pRegs.IX + d, 205);
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
            mMachineTest.Poke(pRegs.IX + d, 205);
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
            mMachineTest.Poke(pRegs.IX + d, 205);
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
            mMachineTest.Poke(pRegs.IY + d, 205);
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
            mMachineTest.Poke(pRegs.IY + d, 205);
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
            mMachineTest.Poke(pRegs.IY + d, 205);
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
            mMachineTest.Poke(pRegs.IY + d, 205);
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
            mMachineTest.Poke(pRegs.IY + d, 205);
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
            mMachineTest.Poke(pRegs.IY + d, 205);
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
            mMachineTest.Poke(pRegs.IY + d, 205);
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
    }
}
