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

using System;
using System.Collections.Generic;
using System.Text;

namespace ZXSinclair.Machines
{
    public class Memory : IMemory
    {
        protected byte[] mBufferData;
        protected int mMask;

        public Memory(int argSize, int argMask)
        {
            mBufferData = new byte[argSize];
            mMask = argMask;
        }

        public Memory(int argSize) : this(argSize, argSize - 1) { }

        public byte ReadMemory(int argMemory) => mBufferData[argMemory % mMask];

        public virtual void WriteMemory(int argMemory, byte argData) => mBufferData[argMemory % mMask] = argData;
    }
}
