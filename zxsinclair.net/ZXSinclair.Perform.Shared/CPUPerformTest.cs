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
using System.Text;
using ZXSinclair.Machines;
using ZXSinclair.Machines.Z80;

namespace ZXSinclair.Perform
{
    public class CPUPerformTest
    {
        public class MachineZ80Test : Machines.Z80.MachineZ80
        {
            private ROM mRom = new ROM(0xFFFF);

            public MachineZ80Test()
            {
                //mRom.BufferData[0] = 0x54; // LD D,H
                mRom.BufferData[0] = OpCodes.LD_A_N;
            }

            protected override IMemory[] CreateMemories()
            {
                return new[] { mRom };
            }

            public override byte PeekByte(int argAddress) => mRom.ReadMemory(argAddress & 0xFFFF);
            public override void Poke(int argAddress, byte argData) => mRom.WriteMemory(argAddress & 0xFFFF, argData);
            protected override void Sync()
            {
                //base.Sync();
                mFinishToken.Cancel();
            }
        }

        //private Z80.Cpu mZ80 = new Z80.Cpu { Ticks = new CPU.Ticks() };
        private MachineZ80Test mMachineTest = new MachineZ80Test();

        public void TestExecInstructionMethod()
        {
            mMachineTest.Reset();
            mMachineTest.SignalSync();
            mMachineTest.Start().Wait();
            //for (var i = 0; i < 224 * 312; i += 4)
            //    mZ80.ExecInstruction();
        }
    }
}
