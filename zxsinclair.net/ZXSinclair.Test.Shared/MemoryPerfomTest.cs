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
using System.Text;

namespace ZXSinclair.Test
{
    [TestClass]
    public class MemoryPerfomTest
    {
        unsafe struct MemoryNative
        {
            public fixed byte memorynative[4096];
        }

        private byte[] mMemoryManaged;
        private MemoryNative mMemoryNative;
        private unsafe byte* mMemN;

        [TestInitialize]
        public unsafe void SetUp()
        {
            fixed (byte* ptr = &mMemoryNative.memorynative[0])
            {
                mMemN = ptr;
                unsafe
                {
                    var ptr1 = ptr;

                    for (var i = 0; i < 4096; i++)
                        *ptr1++ = (byte)(i % 255);
                }
            }
            mMemoryManaged = new byte[4096];
            for (var i = 0; i < 4096; i++)
                mMemoryManaged[i] = (byte)(i % 255);
            TestPerfomNative1Method();
            TestPerfomNative2Method();
            TestPerfomManagedMethod();
        }

        [TestMethod]
        public void TestPerfomNative1Method()
        {
            for (var i = 0; i < 500000; i++)
                for (var j = 0; j < 4096; j++)
                    ReadMemoryNative1(j);
        }

        [TestMethod]
        public void TestPerfomNative2Method()
        {
            for (var i = 0; i < 500000; i++)
                for (var j = 0; j < 4096; j++)
                    ReadMemoryNative1(j);
        }

        [TestMethod]
        public void TestPerfomManagedMethod()
        {
            for (var i = 0; i < 500000; i++)
                for (var j = 0; j < 4096; j++)
                    ReadMemoryMamaged(j);
        }

        private unsafe byte ReadMemoryNative1(int argMem)
        {
            return mMemN[argMem];
        }

        private unsafe byte ReadMemoryNative2(int argMem)
        {
            byte* ptr = mMemN + argMem;

            return *ptr;
        }

        private byte ReadMemoryMamaged(int argMem)
        {
            return mMemoryManaged[argMem];
        }
    }
}
