
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
    public partial class Z80OpCodesTest
    {


        [TestMethod]
        public void LD_A_A_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_A_A);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_A_B_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_A_B);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 2, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_A_C_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_A_C);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 3, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_A_D_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_A_D);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 4, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_A_E_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_A_E);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 5, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_A_H_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_A_H);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 6, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_A_L_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_A_L);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 7, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_B_A_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_B_A);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 1, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_B_B_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_B_B);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_B_C_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_B_C);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 3, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_B_D_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_B_D);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 4, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_B_E_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_B_E);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 5, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_B_H_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_B_H);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 6, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_B_L_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_B_L);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 7, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_C_A_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_C_A);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 1, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_C_B_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_C_B);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 2, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_C_C_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_C_C);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_C_D_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_C_D);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 4, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_C_E_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_C_E);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 5, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_C_H_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_C_H);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 6, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_C_L_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_C_L);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 7, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_D_A_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_D_A);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 1, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_D_B_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_D_B);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 2, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_D_C_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_D_C);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 3, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_D_D_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_D_D);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_D_E_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_D_E);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 5, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_D_H_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_D_H);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 6, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_D_L_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_D_L);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 7, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_E_A_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_E_A);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 1, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_E_B_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_E_B);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 2, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_E_C_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_E_C);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 3, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_E_D_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_E_D);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 4, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_E_E_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_E_E);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_E_H_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_E_H);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 6, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_E_L_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_E_L);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 7, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_H_A_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_H_A);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 1, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_H_B_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_H_B);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 2, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_H_C_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_H_C);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 3, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_H_D_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_H_D);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 4, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_H_E_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_H_E);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 5, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_H_H_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_H_H);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_H_L_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_H_L);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 7, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


        [TestMethod]
        public void LD_L_A_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_L_A);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 1, "L");
        }


        [TestMethod]
        public void LD_L_B_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_L_B);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 2, "L");
        }


        [TestMethod]
        public void LD_L_C_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_L_C);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 3, "L");
        }


        [TestMethod]
        public void LD_L_D_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_L_D);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 4, "L");
        }


        [TestMethod]
        public void LD_L_E_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_L_E);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 5, "L");
        }


        [TestMethod]
        public void LD_L_H_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_L_H);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 6, "L");
        }


        [TestMethod]
        public void LD_L_L_Method()
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

            mMachineTest.Poke(0, OpCodes.LD_L_L);
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, "TState");
            Assert.IsTrue(pRegs.PC == 1, "PC");
            Assert.IsTrue(pRegs.A == 1, "A");
            Assert.IsTrue(pRegs.B == 2, "B");
            Assert.IsTrue(pRegs.C == 3, "C");
            Assert.IsTrue(pRegs.D == 4, "D");
            Assert.IsTrue(pRegs.E == 5, "E");
            Assert.IsTrue(pRegs.H == 6, "H");
            Assert.IsTrue(pRegs.L == 7, "L");
        }


    }
}

