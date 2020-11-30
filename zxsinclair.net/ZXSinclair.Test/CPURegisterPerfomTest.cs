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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZXSinclair.Test
{
    [TestClass]
    public class CPURegisterPerfomTest
    {
        [StructLayout(LayoutKind.Explicit)]
        public class RegsManager
        {
            [FieldOffset(0)]
            public ushort AF = 0;
            [FieldOffset(2)]
            public ushort BC = 0;
            [FieldOffset(1)]
            public byte A;
            [FieldOffset(0)]
            public byte F;
            [FieldOffset(3)]
            public byte B;

            /*
                Code IL:
                    // Method begins at RVA 0x2416
	                // Code size 13 (0xd)
	                .maxstack 8
                    // A = B;
                    IL_0000: ldarg.0
	                IL_0001: ldarg.0
	                IL_0002: ldfld uint8 ZXSinclair.Test.CPURegisterPerfomTest/RegsManager::B
	                IL_0007: stfld uint8 ZXSinclair.Test.CPURegisterPerfomTest/RegsManager::A
	                // }
	                IL_000c: ret
             */
            public void SetA_B() => A = B;

            public Action CreateA_B() => () => A = B;
        }

        public unsafe struct RegsNative
        {
            public const int AF = 0;
            public const int BC = 4;
            public const int A = 1;
            public const int F = 0;
            public const int B = 3;
            public fixed byte regs[4];

            public unsafe void LetR1R2(int r1, int r2) => regs[r1] = regs[r2];
            public unsafe void LetA_B() => regs[A] = regs[B];
            public unsafe int GetRW(int r)
            {
                short* ptr = (short*)(regs[r]);

                return *ptr;
            }
        }

        private RegsManager mRegsM;
        private RegsNative mRegsN;
        private int mTStates;
        private Action mLetA_B_Manager;
        private Action mLetA_B_Native1;
        private Action mLetA_B_Native2;

        [TestInitialize]
        public void SetUp()
        {
            mRegsM = new RegsManager();
            mRegsN = new RegsNative();
            mLetA_B_Manager = mRegsM.CreateA_B();
            mLetA_B_Native1 = mRegsN.LetA_B;
            mLetA_B_Native2 = () => mRegsN.LetR1R2(RegsNative.A, RegsNative.B);
            TestLDR1R2_ManagedMethod1();
            TestLDR1R2_ManagedMethod2();
            TestLDR1R2_NativeMethod1();
            TestLDR1R2_NativeMethod2();
            TestLDR1R2_NativeMethod3();
        }

        [TestMethod]
        public void TestLDR1R2_ManagedMethod1()
        {
            for (var i = 0; i < 500000; i++)
                LdA_B_Managed1();
        }

        [TestMethod]
        public void TestLDR1R2_ManagedMethod2()
        {
            for (var i = 0; i < 500000; i++)
                LdA_B_Managed2();
        }

        [TestMethod]
        public void TestLDR1R2_NativeMethod1()
        {
            for (var i = 0; i < 500000; i++)
                LdA_B_Native1();
        }

        [TestMethod]
        public void TestLDR1R2_NativeMethod2()
        {
            for (var i = 0; i < 500000; i++)
                LdA_B_Native2();
        }

        [TestMethod]
        public void TestLDR1R2_NativeMethod3()
        {
            for (var i = 0; i < 500000; i++)
                LdA_B_Native3();
        }

        private void LdA_B_Managed1()
        {
            mRegsM.SetA_B();
            mTStates += 2;
        }
        private void LdA_B_Managed2() => LdR1R2_Managed2(mLetA_B_Manager);
        private void LdA_B_Native1() => LDR1R2_Native1(RegsNative.A, RegsNative.B);
        private void LdA_B_Native2() => LdR1R2_Managed2(mLetA_B_Native1);
        private void LdA_B_Native3() => LdR1R2_Managed2(mLetA_B_Native2);
        private void LdR1R2_Managed2(Action argLd)
        {
            argLd.Invoke();
            mTStates += 2;
        }
        private void LDR1R2_Native1(int r1, int r2)
        {
            mRegsN.LetR1R2(r1, r2);
            mTStates += 2;
        }
    }
}
