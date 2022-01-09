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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXSinclair.Machines.Z80
{
    // http://www.z80.info/zip/z80cpu_um.pdf
    // http://www.z80.info/zip/z80-documented.pdf
    // https://worldofspectrum.org/z88forever/dn327/z80undoc.htm
    // https://worldofspectrum.org/faq/resources/documents.htm
    // http://biblioteca.museo8bits.es/index.php
    // https://trastero.speccy.org/cosas/Libros/Libros.htm
    // https://www.imd.guru/retropedia/desarrollo/z80/opcodes.html
    // https://wiki.speccy.org/cursos/ensamblador/lenguaje_1
    // https://worldofspectrum.org/faq/reference/48kreference.htm#Contention

    public partial class MachineZ80 : Machine
    {
        protected static readonly byte[] mTablePV;
        protected static readonly byte[] mTableZS53;

        protected Regs mRegs = new Regs();
        protected bool IFF1;
        protected bool IFF2;
        protected Action[] mOpCodesDD;
        protected Action[] mOpCodesFD;
        protected Action[] mOpCodesED;

        static MachineZ80()
        {
            mTablePV = CreateTablePV();
            mTableZS53 = CreateTableZS53();
        }

        private static byte[] CreateTablePV()
        {
            var pTable = new byte[0x100];

            for (var pByte = 0x00; pByte <= 0xFF; pByte++)
                pTable[(byte)pByte] = Flags.GetParity((byte)pByte);

            return pTable;
        }

        private static byte[] CreateTableZS53()
        {
            var pTable = new byte[0x100];

            for (var pByte = 0x00; pByte <= 0xFF; pByte++)
                pTable[(byte)pByte] = (byte)(pByte & (uint)(Flags.S | Flags.F3 | Flags.F5));
            pTable[0] |= Flags.Z;

            return pTable;
        }

        public MachineZ80() : base()
        {
            mTSatesFetchOpCode = 4;
            mTSatesReadMem = mTSatesWriteMem = 3;
            mTSatesCounterFrame = mTSatesToFrame = 224 * 312;

            mOpCodesDD = new Action[256];
            mOpCodesFD = new Action[256];
            mOpCodesED = new Action[256];

            Parallel.For(0, 256, i => mOpCodesDD[i] = NOP);
            Parallel.For(0, 256, i => mOpCodesFD[i] = NOP);
            Parallel.For(0, 256, i => mOpCodesED[i] = NOP);
            FillTableOpCodesDD();
            FillTableOpCodesFD();
            FillTableOpCodesED();
        }

        public Regs Regs => mRegs;

        public override void Reset()
        {
            base.Reset();
            mRegs.Reset();
            IFF1 = IFF2 = false;
        }

        protected override byte FetchOpCode() => PeekByte(mRegs.GetPCAndInc());

        protected byte ReadMemBytePCAndInc() => ReadMemByte(mRegs.GetPCAndInc());
        protected byte ReadMemByteNotTStatesPCAndInc() => ReadMemByteNotTStates(mRegs.GetPCAndInc());
        protected int ReadMemWordPCAndINC()
        {
            var pWord = (int)ReadMemBytePCAndInc();

            pWord += ReadMemBytePCAndInc() << 8;

            return pWord;
        }

        protected override void ExecOpCode(int argOpCode)
        {
            mRegs.RefreshR();

            base.ExecOpCode(argOpCode);
        }

        protected override void FillTableOpCodes()
        {
            var qR_R1 = from r in OpCodes.Rs from r1 in OpCodes.Rs select (r << 3) | r1;

            Parallel.ForEach(qR_R1, r_r1 => mOpCodes[OpCodes.LD_r_r1 | r_r1] = mRegs.CreateLDR_R1(r_r1));
            FillTableOpCodes
            (
                new Dictionary<byte, Action>
                {
                    [OpCodes.LD_A_N] = LD_A_N,
                    [OpCodes.LD_B_N] = LD_B_N,
                    [OpCodes.LD_C_N] = LD_C_N,
                    [OpCodes.LD_D_N] = LD_D_N,
                    [OpCodes.LD_E_N] = LD_E_N,
                    [OpCodes.LD_H_N] = LD_H_N,
                    [OpCodes.LD_L_N] = LD_L_N,
                    [OpCodes.LD_A_M_HL_M] = LD_A_M_HL_M,
                    [OpCodes.LD_B_M_HL_M] = LD_B_M_HL_M,
                    [OpCodes.LD_C_M_HL_M] = LD_C_M_HL_M,
                    [OpCodes.LD_D_M_HL_M] = LD_D_M_HL_M,
                    [OpCodes.LD_E_M_HL_M] = LD_E_M_HL_M,
                    [OpCodes.LD_H_M_HL_M] = LD_H_M_HL_M,
                    [OpCodes.LD_L_M_HL_M] = LD_L_M_HL_M,
                    [OpCodes.LD_M_HL_M_A] = LD_M_HL_M_A,
                    [OpCodes.LD_M_HL_M_B] = LD_M_HL_M_B,
                    [OpCodes.LD_M_HL_M_C] = LD_M_HL_M_C,
                    [OpCodes.LD_M_HL_M_D] = LD_M_HL_M_D,
                    [OpCodes.LD_M_HL_M_E] = LD_M_HL_M_E,
                    [OpCodes.LD_M_HL_M_H] = LD_M_HL_M_H,
                    [OpCodes.LD_M_HL_M_L] = LD_M_HL_M_L,
                    [OpCodes.LD_M_HL_M_N] = LD_M_HL_M_N,
                    [OpCodes.LD_A_M_BC_M] = LD_A_M_BC_M,
                    [OpCodes.LD_A_M_DE_M] = LD_A_M_DE_M,
                    [OpCodes.LD_A_M_NN_M] = LD_A_M_NN_M,
                    [OpCodes.LD_M_BC_M_A] = LD_M_BC_M_A,
                    [OpCodes.LD_M_DE_M_A] = LD_M_DE_M_A,
                    [OpCodes.LD_M_NN_M_A] = LD_M_NN_M_A,
                    [OpCodes.OPCODES_DD] = () => ExecInstruction(mOpCodesDD),
                    [OpCodes.OPCODES_FD] = () => ExecInstruction(mOpCodesFD),
                    [OpCodes.OPCODES_ED] = () => ExecInstruction(mOpCodesED),
                }
            ); ;
        }

        protected void LD_A_N()
        {
            var n = ReadMemBytePCAndInc();

            mRegs.A = n;
        }
        protected void LD_B_N()
        {
            var n = ReadMemBytePCAndInc();

            mRegs.B = n;
        }
        protected void LD_C_N()
        {
            var n = ReadMemBytePCAndInc();

            mRegs.C = n;
        }
        protected void LD_D_N()
        {
            var n = ReadMemBytePCAndInc();

            mRegs.D = n;
        }
        protected void LD_E_N()
        {
            var n = ReadMemBytePCAndInc();

            mRegs.E = n;
        }
        protected void LD_H_N()
        {
            var n = ReadMemBytePCAndInc();

            mRegs.H = n;
        }
        protected void LD_L_N()
        {
            var n = ReadMemBytePCAndInc();

            mRegs.L = n;
        }

        // LD A,(HL)
        protected void LD_A_M_HL_M()
        {
            var n = ReadMemByte(mRegs.HL);

            mRegs.A = n;
        }

        // LD B,(HL)
        protected void LD_B_M_HL_M()
        {
            var n = ReadMemByte(mRegs.HL);

            mRegs.B = n;
        }

        // LD C,(HL)
        protected void LD_C_M_HL_M()
        {
            var n = ReadMemByte(mRegs.HL);

            mRegs.C = n;
        }

        // LD D,(HL)
        protected void LD_D_M_HL_M()
        {
            var n = ReadMemByte(mRegs.HL);

            mRegs.D = n;
        }

        // LD E,(HL)
        protected void LD_E_M_HL_M()
        {
            var n = ReadMemByte(mRegs.HL);

            mRegs.E = n;
        }

        // LD H,(HL)
        protected void LD_H_M_HL_M()
        {
            var n = ReadMemByte(mRegs.HL);

            mRegs.H = n;
        }

        // LD L,(HL)
        protected void LD_L_M_HL_M()
        {
            var n = ReadMemByte(mRegs.HL);

            mRegs.L = n;
        }

        // LD (HL),A
        protected void LD_M_HL_M_A() => PokeMemByte(mRegs.HL, mRegs.A);

        // LD (HL),B
        protected void LD_M_HL_M_B() => PokeMemByte(mRegs.HL, mRegs.B);

        // LD (HL),C
        protected void LD_M_HL_M_C() => PokeMemByte(mRegs.HL, mRegs.C);

        // LD (HL),D
        protected void LD_M_HL_M_D() => PokeMemByte(mRegs.HL, mRegs.D);

        // LD (HL),E
        protected void LD_M_HL_M_E() => PokeMemByte(mRegs.HL, mRegs.E);

        // LD (HL),H
        protected void LD_M_HL_M_H() => PokeMemByte(mRegs.HL, mRegs.H);

        // LD (HL),L
        protected void LD_M_HL_M_L() => PokeMemByte(mRegs.HL, mRegs.L);

        // LD (HL),N
        protected void LD_M_HL_M_N()
        {
            var n = ReadMemBytePCAndInc();

            PokeMemByte(mRegs.HL, n);
        }

        // LD (IX+d),N
        protected void LD_M_IX_D_M_N()
        {
            var d = (sbyte)ReadMemBytePCAndInc();
            var n = ReadMemByteNotTStatesPCAndInc();

            AddCycles(5);

            PokeMemByte(mRegs.GetIX_d(d), n);
        }

        // LD (IY+d),N
        protected void LD_M_IY_D_M_N()
        {
            var d = (sbyte)ReadMemBytePCAndInc();
            var n = ReadMemByteNotTStatesPCAndInc();

            AddCycles(5);

            PokeMemByte(mRegs.GetIY_d(d), n);
        }

        // LD A,(BC)
        protected void LD_A_M_BC_M()
        {
            var n = ReadMemByte(mRegs.BC);

            mRegs.A = n;
        }

        // LD A,(DE)
        protected void LD_A_M_DE_M()
        {
            var n = ReadMemByte(mRegs.DE);

            mRegs.A = n;
        }

        // LD A,(NN)
        protected void LD_A_M_NN_M()
        {
            var nn = ReadMemWordPCAndINC();
            var n = ReadMemByte(nn);

            mRegs.A = n;
        }

        // LD (BC),A
        protected void LD_M_BC_M_A() => PokeMemByte(mRegs.BC, mRegs.A);

        // LD (DE),A
        protected void LD_M_DE_M_A() => PokeMemByte(mRegs.DE, mRegs.A);

        // LD (nn),A
        protected void LD_M_NN_M_A()
        {
            var nn = ReadMemWordPCAndINC();

            PokeMemByte(nn, mRegs.A);
        }
    }
}
