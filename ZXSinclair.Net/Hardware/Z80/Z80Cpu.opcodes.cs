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

namespace ZXSinclair.Net.Hardware.Z80;

public partial class Z80Cpu
{

    public void LD_M_BC_M_A()
    {
        var r = Regs;

        WriteMemory(r.BC, r.A);
    }

    public void LD_M_BC_M_B()
    {
        var r = Regs;

        WriteMemory(r.BC, r.B);
    }

    public void LD_M_BC_M_C()
    {
        var r = Regs;

        WriteMemory(r.BC, r.C);
    }

    public void LD_M_BC_M_D()
    {
        var r = Regs;

        WriteMemory(r.BC, r.D);
    }

    public void LD_M_BC_M_E()
    {
        var r = Regs;

        WriteMemory(r.BC, r.E);
    }

    public void LD_M_BC_M_H()
    {
        var r = Regs;

        WriteMemory(r.BC, r.H);
    }

    public void LD_M_BC_M_L()
    {
        var r = Regs;

        WriteMemory(r.BC, r.L);
    }

    public void LD_M_DE_M_A()
    {
        var r = Regs;

        WriteMemory(r.DE, r.A);
    }

    public void LD_M_DE_M_B()
    {
        var r = Regs;

        WriteMemory(r.DE, r.B);
    }

    public void LD_M_DE_M_C()
    {
        var r = Regs;

        WriteMemory(r.DE, r.C);
    }

    public void LD_M_DE_M_D()
    {
        var r = Regs;

        WriteMemory(r.DE, r.D);
    }

    public void LD_M_DE_M_E()
    {
        var r = Regs;

        WriteMemory(r.DE, r.E);
    }

    public void LD_M_DE_M_H()
    {
        var r = Regs;

        WriteMemory(r.DE, r.H);
    }

    public void LD_M_DE_M_L()
    {
        var r = Regs;

        WriteMemory(r.DE, r.L);
    }

    public void LD_M_HL_M_A()
    {
        var r = Regs;

        WriteMemory(r.HL, r.A);
    }

    public void LD_M_HL_M_B()
    {
        var r = Regs;

        WriteMemory(r.HL, r.B);
    }

    public void LD_M_HL_M_C()
    {
        var r = Regs;

        WriteMemory(r.HL, r.C);
    }

    public void LD_M_HL_M_D()
    {
        var r = Regs;

        WriteMemory(r.HL, r.D);
    }

    public void LD_M_HL_M_E()
    {
        var r = Regs;

        WriteMemory(r.HL, r.E);
    }

    public void LD_M_HL_M_H()
    {
        var r = Regs;

        WriteMemory(r.HL, r.H);
    }

    public void LD_M_HL_M_L()
    {
        var r = Regs;

        WriteMemory(r.HL, r.L);
    }

    public override void ExecOpCode(byte opcode)
    {
        switch (opcode)
        {
			// 0x00 NOP
			case (byte)Z80OpCodes.NOP:
			Nop();
			break;
			// 0x01 LD BC,nnnn
			case (byte)Z80OpCodes.LD_BC_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x02 LD (BC),A
			case (byte)Z80OpCodes.LD_MM_BC_MM_A:
			LD_M_BC_M_A();
			break;
			// 0x03 INC BC
			case (byte)Z80OpCodes.INC_BC:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x04 INC B
			case (byte)Z80OpCodes.INC_B:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x05 DEC B
			case (byte)Z80OpCodes.DEC_B:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x06 LD B,nn
			case (byte)Z80OpCodes.LD_B_nn:
			Regs.SetB_n(ReadMemory(Regs.GetPCAndInc()));
			break;
			// 0x07 RLCA
			case (byte)Z80OpCodes.RLCA:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x08 EX AF,AF'
			case (byte)Z80OpCodes.EX_AF_AF_:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x09 ADD HL,BC
			case (byte)Z80OpCodes.ADD_HL_BC:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x0a LD A,(BC)
			case (byte)Z80OpCodes.LD_A_MM_BC_MM:
			Regs.SetA_n(Read_M_BC_M());
			break;
			// 0x0b DEC BC
			case (byte)Z80OpCodes.DEC_BC:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x0c INC C
			case (byte)Z80OpCodes.INC_C:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x0d DEC C
			case (byte)Z80OpCodes.DEC_C:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x0e LD C,nn
			case (byte)Z80OpCodes.LD_C_nn:
			Regs.SetC_n(ReadMemory(Regs.GetPCAndInc()));
			break;
			// 0x0f RRCA
			case (byte)Z80OpCodes.RRCA:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x10 DJNZ offset
			case (byte)Z80OpCodes.DJNZ_offset:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x11 LD DE,nnnn
			case (byte)Z80OpCodes.LD_DE_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x12 LD (DE),A
			case (byte)Z80OpCodes.LD_MM_DE_MM_A:
			LD_M_DE_M_A();
			break;
			// 0x13 INC DE
			case (byte)Z80OpCodes.INC_DE:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x14 INC D
			case (byte)Z80OpCodes.INC_D:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x15 DEC D
			case (byte)Z80OpCodes.DEC_D:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x16 LD D,nn
			case (byte)Z80OpCodes.LD_D_nn:
			Regs.SetD_n(ReadMemory(Regs.GetPCAndInc()));
			break;
			// 0x17 RLA
			case (byte)Z80OpCodes.RLA:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x18 JR offset
			case (byte)Z80OpCodes.JR_offset:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x19 ADD HL,DE
			case (byte)Z80OpCodes.ADD_HL_DE:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x1a LD A,(DE)
			case (byte)Z80OpCodes.LD_A_MM_DE_MM:
			Regs.SetA_n(Read_M_DE_M());
			break;
			// 0x1b DEC DE
			case (byte)Z80OpCodes.DEC_DE:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x1c INC E
			case (byte)Z80OpCodes.INC_E:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x1d DEC E
			case (byte)Z80OpCodes.DEC_E:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x1e LD E,nn
			case (byte)Z80OpCodes.LD_E_nn:
			Regs.SetE_n(ReadMemory(Regs.GetPCAndInc()));
			break;
			// 0x1f RRA
			case (byte)Z80OpCodes.RRA:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x20 JR NZ,offset
			case (byte)Z80OpCodes.JR_NZ_offset:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x21 LD HL,nnnn
			case (byte)Z80OpCodes.LD_HL_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x22 LD (nnnn),HL
			case (byte)Z80OpCodes.LD_MM_nnnn_MM_HL:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x23 INC HL
			case (byte)Z80OpCodes.INC_HL:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x24 INC H
			case (byte)Z80OpCodes.INC_H:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x25 DEC H
			case (byte)Z80OpCodes.DEC_H:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x26 LD H,nn
			case (byte)Z80OpCodes.LD_H_nn:
			Regs.SetH_n(ReadMemory(Regs.GetPCAndInc()));
			break;
			// 0x27 DAA
			case (byte)Z80OpCodes.DAA:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x28 JR Z,offset
			case (byte)Z80OpCodes.JR_Z_offset:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x29 ADD HL,HL
			case (byte)Z80OpCodes.ADD_HL_HL:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x2a LD HL,(nnnn)
			case (byte)Z80OpCodes.LD_HL_MM_nnnn_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x2b DEC HL
			case (byte)Z80OpCodes.DEC_HL:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x2c INC L
			case (byte)Z80OpCodes.INC_L:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x2d DEC L
			case (byte)Z80OpCodes.DEC_L:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x2e LD L,nn
			case (byte)Z80OpCodes.LD_L_nn:
			Regs.SetL_n(ReadMemory(Regs.GetPCAndInc()));
			break;
			// 0x2f CPL
			case (byte)Z80OpCodes.CPL:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x30 JR NC,offset
			case (byte)Z80OpCodes.JR_NC_offset:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x31 LD SP,nnnn
			case (byte)Z80OpCodes.LD_SP_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x32 LD (nnnn),A
			case (byte)Z80OpCodes.LD_MM_nnnn_MM_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x33 INC SP
			case (byte)Z80OpCodes.INC_SP:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x34 INC (HL)
			case (byte)Z80OpCodes.INC_MM_HL_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x35 DEC (HL)
			case (byte)Z80OpCodes.DEC_MM_HL_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x36 LD (HL),nn
			case (byte)Z80OpCodes.LD_MM_HL_MM_nn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x37 SCF
			case (byte)Z80OpCodes.SCF:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x38 JR C,offset
			case (byte)Z80OpCodes.JR_C_offset:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x39 ADD HL,SP
			case (byte)Z80OpCodes.ADD_HL_SP:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x3a LD A,(nnnn)
			case (byte)Z80OpCodes.LD_A_MM_nnnn_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x3b DEC SP
			case (byte)Z80OpCodes.DEC_SP:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x3c INC A
			case (byte)Z80OpCodes.INC_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x3d DEC A
			case (byte)Z80OpCodes.DEC_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x3e LD A,nn
			case (byte)Z80OpCodes.LD_A_nn:
			Regs.SetA_n(ReadMemory(Regs.GetPCAndInc()));
			break;
			// 0x3f CCF
			case (byte)Z80OpCodes.CCF:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x40 LD B,B
			case (byte)Z80OpCodes.LD_B_B:
			break;
			// 0x41 LD B,C
			case (byte)Z80OpCodes.LD_B_C:
			Regs.SetB_C();
			break;
			// 0x42 LD B,D
			case (byte)Z80OpCodes.LD_B_D:
			Regs.SetB_D();
			break;
			// 0x43 LD B,E
			case (byte)Z80OpCodes.LD_B_E:
			Regs.SetB_E();
			break;
			// 0x44 LD B,H
			case (byte)Z80OpCodes.LD_B_H:
			Regs.SetB_H();
			break;
			// 0x45 LD B,L
			case (byte)Z80OpCodes.LD_B_L:
			Regs.SetB_L();
			break;
			// 0x46 LD B,(HL)
			case (byte)Z80OpCodes.LD_B_MM_HL_MM:
			Regs.SetB_n(Read_M_HL_M());
			break;
			// 0x47 LD B,A
			case (byte)Z80OpCodes.LD_B_A:
			Regs.SetB_A();
			break;
			// 0x48 LD C,B
			case (byte)Z80OpCodes.LD_C_B:
			Regs.SetC_B();
			break;
			// 0x49 LD C,C
			case (byte)Z80OpCodes.LD_C_C:
			break;
			// 0x4a LD C,D
			case (byte)Z80OpCodes.LD_C_D:
			Regs.SetC_D();
			break;
			// 0x4b LD C,E
			case (byte)Z80OpCodes.LD_C_E:
			Regs.SetC_E();
			break;
			// 0x4c LD C,H
			case (byte)Z80OpCodes.LD_C_H:
			Regs.SetC_H();
			break;
			// 0x4d LD C,L
			case (byte)Z80OpCodes.LD_C_L:
			Regs.SetC_L();
			break;
			// 0x4e LD C,(HL)
			case (byte)Z80OpCodes.LD_C_MM_HL_MM:
			Regs.SetC_n(Read_M_HL_M());
			break;
			// 0x4f LD C,A
			case (byte)Z80OpCodes.LD_C_A:
			Regs.SetC_A();
			break;
			// 0x50 LD D,B
			case (byte)Z80OpCodes.LD_D_B:
			Regs.SetD_B();
			break;
			// 0x51 LD D,C
			case (byte)Z80OpCodes.LD_D_C:
			Regs.SetD_C();
			break;
			// 0x52 LD D,D
			case (byte)Z80OpCodes.LD_D_D:
			break;
			// 0x53 LD D,E
			case (byte)Z80OpCodes.LD_D_E:
			Regs.SetD_E();
			break;
			// 0x54 LD D,H
			case (byte)Z80OpCodes.LD_D_H:
			Regs.SetD_H();
			break;
			// 0x55 LD D,L
			case (byte)Z80OpCodes.LD_D_L:
			Regs.SetD_L();
			break;
			// 0x56 LD D,(HL)
			case (byte)Z80OpCodes.LD_D_MM_HL_MM:
			Regs.SetD_n(Read_M_HL_M());
			break;
			// 0x57 LD D,A
			case (byte)Z80OpCodes.LD_D_A:
			Regs.SetD_A();
			break;
			// 0x58 LD E,B
			case (byte)Z80OpCodes.LD_E_B:
			Regs.SetE_B();
			break;
			// 0x59 LD E,C
			case (byte)Z80OpCodes.LD_E_C:
			Regs.SetE_C();
			break;
			// 0x5a LD E,D
			case (byte)Z80OpCodes.LD_E_D:
			Regs.SetE_D();
			break;
			// 0x5b LD E,E
			case (byte)Z80OpCodes.LD_E_E:
			break;
			// 0x5c LD E,H
			case (byte)Z80OpCodes.LD_E_H:
			Regs.SetE_H();
			break;
			// 0x5d LD E,L
			case (byte)Z80OpCodes.LD_E_L:
			Regs.SetE_L();
			break;
			// 0x5e LD E,(HL)
			case (byte)Z80OpCodes.LD_E_MM_HL_MM:
			Regs.SetE_n(Read_M_HL_M());
			break;
			// 0x5f LD E,A
			case (byte)Z80OpCodes.LD_E_A:
			Regs.SetE_A();
			break;
			// 0x60 LD H,B
			case (byte)Z80OpCodes.LD_H_B:
			Regs.SetH_B();
			break;
			// 0x61 LD H,C
			case (byte)Z80OpCodes.LD_H_C:
			Regs.SetH_C();
			break;
			// 0x62 LD H,D
			case (byte)Z80OpCodes.LD_H_D:
			Regs.SetH_D();
			break;
			// 0x63 LD H,E
			case (byte)Z80OpCodes.LD_H_E:
			Regs.SetH_E();
			break;
			// 0x64 LD H,H
			case (byte)Z80OpCodes.LD_H_H:
			break;
			// 0x65 LD H,L
			case (byte)Z80OpCodes.LD_H_L:
			Regs.SetH_L();
			break;
			// 0x66 LD H,(HL)
			case (byte)Z80OpCodes.LD_H_MM_HL_MM:
			Regs.SetH_n(Read_M_HL_M());
			break;
			// 0x67 LD H,A
			case (byte)Z80OpCodes.LD_H_A:
			Regs.SetH_A();
			break;
			// 0x68 LD L,B
			case (byte)Z80OpCodes.LD_L_B:
			Regs.SetL_B();
			break;
			// 0x69 LD L,C
			case (byte)Z80OpCodes.LD_L_C:
			Regs.SetL_C();
			break;
			// 0x6a LD L,D
			case (byte)Z80OpCodes.LD_L_D:
			Regs.SetL_D();
			break;
			// 0x6b LD L,E
			case (byte)Z80OpCodes.LD_L_E:
			Regs.SetL_E();
			break;
			// 0x6c LD L,H
			case (byte)Z80OpCodes.LD_L_H:
			Regs.SetL_H();
			break;
			// 0x6d LD L,L
			case (byte)Z80OpCodes.LD_L_L:
			break;
			// 0x6e LD L,(HL)
			case (byte)Z80OpCodes.LD_L_MM_HL_MM:
			Regs.SetL_n(Read_M_HL_M());
			break;
			// 0x6f LD L,A
			case (byte)Z80OpCodes.LD_L_A:
			Regs.SetL_A();
			break;
			// 0x70 LD (HL),B
			case (byte)Z80OpCodes.LD_MM_HL_MM_B:
			LD_M_HL_M_B();
			break;
			// 0x71 LD (HL),C
			case (byte)Z80OpCodes.LD_MM_HL_MM_C:
			LD_M_HL_M_C();
			break;
			// 0x72 LD (HL),D
			case (byte)Z80OpCodes.LD_MM_HL_MM_D:
			LD_M_HL_M_D();
			break;
			// 0x73 LD (HL),E
			case (byte)Z80OpCodes.LD_MM_HL_MM_E:
			LD_M_HL_M_E();
			break;
			// 0x74 LD (HL),H
			case (byte)Z80OpCodes.LD_MM_HL_MM_H:
			LD_M_HL_M_H();
			break;
			// 0x75 LD (HL),L
			case (byte)Z80OpCodes.LD_MM_HL_MM_L:
			LD_M_HL_M_L();
			break;
			// 0x76 HALT
			case (byte)Z80OpCodes.HALT:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x77 LD (HL),A
			case (byte)Z80OpCodes.LD_MM_HL_MM_A:
			LD_M_HL_M_A();
			break;
			// 0x78 LD A,B
			case (byte)Z80OpCodes.LD_A_B:
			Regs.SetA_B();
			break;
			// 0x79 LD A,C
			case (byte)Z80OpCodes.LD_A_C:
			Regs.SetA_C();
			break;
			// 0x7a LD A,D
			case (byte)Z80OpCodes.LD_A_D:
			Regs.SetA_D();
			break;
			// 0x7b LD A,E
			case (byte)Z80OpCodes.LD_A_E:
			Regs.SetA_E();
			break;
			// 0x7c LD A,H
			case (byte)Z80OpCodes.LD_A_H:
			Regs.SetA_H();
			break;
			// 0x7d LD A,L
			case (byte)Z80OpCodes.LD_A_L:
			Regs.SetA_L();
			break;
			// 0x7e LD A,(HL)
			case (byte)Z80OpCodes.LD_A_MM_HL_MM:
			Regs.SetA_n(Read_M_HL_M());
			break;
			// 0x7f LD A,A
			case (byte)Z80OpCodes.LD_A_A:
			break;
			// 0x80 ADD A,B
			case (byte)Z80OpCodes.ADD_A_B:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x81 ADD A,C
			case (byte)Z80OpCodes.ADD_A_C:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x82 ADD A,D
			case (byte)Z80OpCodes.ADD_A_D:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x83 ADD A,E
			case (byte)Z80OpCodes.ADD_A_E:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x84 ADD A,H
			case (byte)Z80OpCodes.ADD_A_H:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x85 ADD A,L
			case (byte)Z80OpCodes.ADD_A_L:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x86 ADD A,(HL)
			case (byte)Z80OpCodes.ADD_A_MM_HL_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x87 ADD A,A
			case (byte)Z80OpCodes.ADD_A_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x88 ADC A,B
			case (byte)Z80OpCodes.ADC_A_B:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x89 ADC A,C
			case (byte)Z80OpCodes.ADC_A_C:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x8a ADC A,D
			case (byte)Z80OpCodes.ADC_A_D:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x8b ADC A,E
			case (byte)Z80OpCodes.ADC_A_E:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x8c ADC A,H
			case (byte)Z80OpCodes.ADC_A_H:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x8d ADC A,L
			case (byte)Z80OpCodes.ADC_A_L:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x8e ADC A,(HL)
			case (byte)Z80OpCodes.ADC_A_MM_HL_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x8f ADC A,A
			case (byte)Z80OpCodes.ADC_A_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x90 SUB A,B
			case (byte)Z80OpCodes.SUB_A_B:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x91 SUB A,C
			case (byte)Z80OpCodes.SUB_A_C:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x92 SUB A,D
			case (byte)Z80OpCodes.SUB_A_D:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x93 SUB A,E
			case (byte)Z80OpCodes.SUB_A_E:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x94 SUB A,H
			case (byte)Z80OpCodes.SUB_A_H:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x95 SUB A,L
			case (byte)Z80OpCodes.SUB_A_L:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x96 SUB A,(HL)
			case (byte)Z80OpCodes.SUB_A_MM_HL_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x97 SUB A,A
			case (byte)Z80OpCodes.SUB_A_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x98 SBC A,B
			case (byte)Z80OpCodes.SBC_A_B:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x99 SBC A,C
			case (byte)Z80OpCodes.SBC_A_C:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x9a SBC A,D
			case (byte)Z80OpCodes.SBC_A_D:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x9b SBC A,E
			case (byte)Z80OpCodes.SBC_A_E:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x9c SBC A,H
			case (byte)Z80OpCodes.SBC_A_H:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x9d SBC A,L
			case (byte)Z80OpCodes.SBC_A_L:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x9e SBC A,(HL)
			case (byte)Z80OpCodes.SBC_A_MM_HL_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x9f SBC A,A
			case (byte)Z80OpCodes.SBC_A_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa0 AND A,B
			case (byte)Z80OpCodes.AND_A_B:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa1 AND A,C
			case (byte)Z80OpCodes.AND_A_C:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa2 AND A,D
			case (byte)Z80OpCodes.AND_A_D:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa3 AND A,E
			case (byte)Z80OpCodes.AND_A_E:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa4 AND A,H
			case (byte)Z80OpCodes.AND_A_H:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa5 AND A,L
			case (byte)Z80OpCodes.AND_A_L:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa6 AND A,(HL)
			case (byte)Z80OpCodes.AND_A_MM_HL_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa7 AND A,A
			case (byte)Z80OpCodes.AND_A_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa8 XOR A,B
			case (byte)Z80OpCodes.XOR_A_B:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa9 XOR A,C
			case (byte)Z80OpCodes.XOR_A_C:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xaa XOR A,D
			case (byte)Z80OpCodes.XOR_A_D:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xab XOR A,E
			case (byte)Z80OpCodes.XOR_A_E:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xac XOR A,H
			case (byte)Z80OpCodes.XOR_A_H:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xad XOR A,L
			case (byte)Z80OpCodes.XOR_A_L:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xae XOR A,(HL)
			case (byte)Z80OpCodes.XOR_A_MM_HL_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xaf XOR A,A
			case (byte)Z80OpCodes.XOR_A_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb0 OR A,B
			case (byte)Z80OpCodes.OR_A_B:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb1 OR A,C
			case (byte)Z80OpCodes.OR_A_C:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb2 OR A,D
			case (byte)Z80OpCodes.OR_A_D:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb3 OR A,E
			case (byte)Z80OpCodes.OR_A_E:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb4 OR A,H
			case (byte)Z80OpCodes.OR_A_H:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb5 OR A,L
			case (byte)Z80OpCodes.OR_A_L:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb6 OR A,(HL)
			case (byte)Z80OpCodes.OR_A_MM_HL_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb7 OR A,A
			case (byte)Z80OpCodes.OR_A_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb8 CP B
			case (byte)Z80OpCodes.CP_B:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb9 CP C
			case (byte)Z80OpCodes.CP_C:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xba CP D
			case (byte)Z80OpCodes.CP_D:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xbb CP E
			case (byte)Z80OpCodes.CP_E:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xbc CP H
			case (byte)Z80OpCodes.CP_H:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xbd CP L
			case (byte)Z80OpCodes.CP_L:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xbe CP (HL)
			case (byte)Z80OpCodes.CP_MM_HL_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xbf CP A
			case (byte)Z80OpCodes.CP_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xc0 RET NZ
			case (byte)Z80OpCodes.RET_NZ:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xc1 POP BC
			case (byte)Z80OpCodes.POP_BC:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xc2 JP NZ,nnnn
			case (byte)Z80OpCodes.JP_NZ_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xc3 JP nnnn
			case (byte)Z80OpCodes.JP_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xc4 CALL NZ,nnnn
			case (byte)Z80OpCodes.CALL_NZ_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xc5 PUSH BC
			case (byte)Z80OpCodes.PUSH_BC:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xc6 ADD A,nn
			case (byte)Z80OpCodes.ADD_A_nn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xc7 RST 00
			case (byte)Z80OpCodes.RST_00:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xc8 RET Z
			case (byte)Z80OpCodes.RET_Z:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xc9 RET
			case (byte)Z80OpCodes.RET:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xca JP Z,nnnn
			case (byte)Z80OpCodes.JP_Z_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xcb shift CB
			case (byte)Z80OpCodes.shift_CB:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xcc CALL Z,nnnn
			case (byte)Z80OpCodes.CALL_Z_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xcd CALL nnnn
			case (byte)Z80OpCodes.CALL_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xce ADC A,nn
			case (byte)Z80OpCodes.ADC_A_nn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xcf RST 8
			case (byte)Z80OpCodes.RST_8:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xd0 RET NC
			case (byte)Z80OpCodes.RET_NC:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xd1 POP DE
			case (byte)Z80OpCodes.POP_DE:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xd2 JP NC,nnnn
			case (byte)Z80OpCodes.JP_NC_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xd3 OUT (nn),A
			case (byte)Z80OpCodes.OUT_MM_nn_MM_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xd4 CALL NC,nnnn
			case (byte)Z80OpCodes.CALL_NC_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xd5 PUSH DE
			case (byte)Z80OpCodes.PUSH_DE:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xd6 SUB nn
			case (byte)Z80OpCodes.SUB_nn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xd7 RST 10
			case (byte)Z80OpCodes.RST_10:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xd8 RET C
			case (byte)Z80OpCodes.RET_C:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xd9 EXX
			case (byte)Z80OpCodes.EXX:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xda JP C,nnnn
			case (byte)Z80OpCodes.JP_C_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xdb IN A,(nn)
			case (byte)Z80OpCodes.IN_A_MM_nn_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xdc CALL C,nnnn
			case (byte)Z80OpCodes.CALL_C_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xdd shift DD
			case (byte)Z80OpCodes.shift_DD:
			InstrfetchDD();
			break;
			// 0xde SBC A,nn
			case (byte)Z80OpCodes.SBC_A_nn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xdf RST 18
			case (byte)Z80OpCodes.RST_18:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xe0 RET PO
			case (byte)Z80OpCodes.RET_PO:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xe1 POP HL
			case (byte)Z80OpCodes.POP_HL:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xe2 JP PO,nnnn
			case (byte)Z80OpCodes.JP_PO_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xe3 EX (SP),HL
			case (byte)Z80OpCodes.EX_MM_SP_MM_HL:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xe4 CALL PO,nnnn
			case (byte)Z80OpCodes.CALL_PO_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xe5 PUSH HL
			case (byte)Z80OpCodes.PUSH_HL:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xe6 AND nn
			case (byte)Z80OpCodes.AND_nn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xe7 RST 20
			case (byte)Z80OpCodes.RST_20:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xe8 RET PE
			case (byte)Z80OpCodes.RET_PE:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xe9 JP HL
			case (byte)Z80OpCodes.JP_HL:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xea JP PE,nnnn
			case (byte)Z80OpCodes.JP_PE_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xeb EX DE,HL
			case (byte)Z80OpCodes.EX_DE_HL:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xec CALL PE,nnnn
			case (byte)Z80OpCodes.CALL_PE_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xed shift ED
			case (byte)Z80OpCodes.shift_ED:
			InstrfetchED();
			break;
			// 0xee XOR A,nn
			case (byte)Z80OpCodes.XOR_A_nn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xef RST 28
			case (byte)Z80OpCodes.RST_28:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xf0 RET P
			case (byte)Z80OpCodes.RET_P:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xf1 POP AF
			case (byte)Z80OpCodes.POP_AF:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xf2 JP P,nnnn
			case (byte)Z80OpCodes.JP_P_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xf3 DI
			case (byte)Z80OpCodes.DI:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xf4 CALL P,nnnn
			case (byte)Z80OpCodes.CALL_P_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xf5 PUSH AF
			case (byte)Z80OpCodes.PUSH_AF:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xf6 OR nn
			case (byte)Z80OpCodes.OR_nn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xf7 RST 30
			case (byte)Z80OpCodes.RST_30:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xf8 RET M
			case (byte)Z80OpCodes.RET_M:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xf9 LD SP,HL
			case (byte)Z80OpCodes.LD_SP_HL:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xfa JP M,nnnn
			case (byte)Z80OpCodes.JP_M_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xfb EI
			case (byte)Z80OpCodes.EI:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xfc CALL M,nnnn
			case (byte)Z80OpCodes.CALL_M_nnnn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xfd shift FD
			case (byte)Z80OpCodes.shift_FD:
			InstrfetchFD();
			break;
			// 0xfe CP nn
			case (byte)Z80OpCodes.CP_nn:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xff RST 38
			case (byte)Z80OpCodes.RST_38:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;

            default:
                Nop();
                break;
        }
    }
}
