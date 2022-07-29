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
//{{BEFORE}}
    public void ExecOpCodeED(byte opcode)
    {
        switch (opcode)
        {
			// 0x40 IN B,(C)
			case (byte)Z80OpCodesED.IN_B_MM_C_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x41 OUT (C),B
			case (byte)Z80OpCodesED.OUT_MM_C_MM_B:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x42 SBC HL,BC
			case (byte)Z80OpCodesED.SBC_HL_BC:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x43 LD (nnnn),BC
			case (byte)Z80OpCodesED.LD_MM_nnnn_MM_BC:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			case 0x44:
			case 0x4c:
			case 0x54:
			case 0x5c:
			case 0x64:
			case 0x6c:
			case 0x74:
			// 0x7c NEG
			case (byte)Z80OpCodesED.NEG:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			case 0x45:
			case 0x4d:
			case 0x55:
			case 0x5d:
			case 0x65:
			case 0x6d:
			case 0x75:
			// 0x7d RETN
			case (byte)Z80OpCodesED.RETN:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			case 0x46:
			case 0x4e:
			case 0x66:
			// 0x6e IM 0
			case (byte)Z80OpCodesED.IM_0:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x47 LD I,A
			case (byte)Z80OpCodesED.LD_I_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x48 IN C,(C)
			case (byte)Z80OpCodesED.IN_C_MM_C_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x49 OUT (C),C
			case (byte)Z80OpCodesED.OUT_MM_C_MM_C:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x4a ADC HL,BC
			case (byte)Z80OpCodesED.ADC_HL_BC:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x4b LD BC,(nnnn)
			case (byte)Z80OpCodesED.LD_BC_MM_nnnn_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x4f LD R,A
			case (byte)Z80OpCodesED.LD_R_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x50 IN D,(C)
			case (byte)Z80OpCodesED.IN_D_MM_C_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x51 OUT (C),D
			case (byte)Z80OpCodesED.OUT_MM_C_MM_D:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x52 SBC HL,DE
			case (byte)Z80OpCodesED.SBC_HL_DE:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x53 LD (nnnn),DE
			case (byte)Z80OpCodesED.LD_MM_nnnn_MM_DE:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			case 0x56:
			// 0x76 IM 1
			case (byte)Z80OpCodesED.IM_1:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x57 LD A,I
			case (byte)Z80OpCodesED.LD_A_I:
			LD_A_I();
			break;
			// 0x58 IN E,(C)
			case (byte)Z80OpCodesED.IN_E_MM_C_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x59 OUT (C),E
			case (byte)Z80OpCodesED.OUT_MM_C_MM_E:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x5a ADC HL,DE
			case (byte)Z80OpCodesED.ADC_HL_DE:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x5b LD DE,(nnnn)
			case (byte)Z80OpCodesED.LD_DE_MM_nnnn_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			case 0x5e:
			// 0x7e IM 2
			case (byte)Z80OpCodesED.IM_2:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x5f LD A,R
			case (byte)Z80OpCodesED.LD_A_R:
			LD_A_R();
			break;
			// 0x60 IN H,(C)
			case (byte)Z80OpCodesED.IN_H_MM_C_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x61 OUT (C),H
			case (byte)Z80OpCodesED.OUT_MM_C_MM_H:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x62 SBC HL,HL
			case (byte)Z80OpCodesED.SBC_HL_HL:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x63 LD (nnnn),HL
			case (byte)Z80OpCodesED.LD_MM_nnnn_MM_HL:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x67 RRD
			case (byte)Z80OpCodesED.RRD:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x68 IN L,(C)
			case (byte)Z80OpCodesED.IN_L_MM_C_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x69 OUT (C),L
			case (byte)Z80OpCodesED.OUT_MM_C_MM_L:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x6a ADC HL,HL
			case (byte)Z80OpCodesED.ADC_HL_HL:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x6b LD HL,(nnnn)
			case (byte)Z80OpCodesED.LD_HL_MM_nnnn_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x6f RLD
			case (byte)Z80OpCodesED.RLD:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x70 IN F,(C)
			case (byte)Z80OpCodesED.IN_F_MM_C_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x71 OUT (C),0
			case (byte)Z80OpCodesED.OUT_MM_C_MM_0:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x72 SBC HL,SP
			case (byte)Z80OpCodesED.SBC_HL_SP:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x73 LD (nnnn),SP
			case (byte)Z80OpCodesED.LD_MM_nnnn_MM_SP:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x78 IN A,(C)
			case (byte)Z80OpCodesED.IN_A_MM_C_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x79 OUT (C),A
			case (byte)Z80OpCodesED.OUT_MM_C_MM_A:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x7a ADC HL,SP
			case (byte)Z80OpCodesED.ADC_HL_SP:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0x7b LD SP,(nnnn)
			case (byte)Z80OpCodesED.LD_SP_MM_nnnn_MM:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa0 LDI
			case (byte)Z80OpCodesED.LDI:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa1 CPI
			case (byte)Z80OpCodesED.CPI:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa2 INI
			case (byte)Z80OpCodesED.INI:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa3 OUTI
			case (byte)Z80OpCodesED.OUTI:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa8 LDD
			case (byte)Z80OpCodesED.LDD:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xa9 CPD
			case (byte)Z80OpCodesED.CPD:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xaa IND
			case (byte)Z80OpCodesED.IND:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xab OUTD
			case (byte)Z80OpCodesED.OUTD:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb0 LDIR
			case (byte)Z80OpCodesED.LDIR:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb1 CPIR
			case (byte)Z80OpCodesED.CPIR:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb2 INIR
			case (byte)Z80OpCodesED.INIR:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb3 OTIR
			case (byte)Z80OpCodesED.OTIR:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb8 LDDR
			case (byte)Z80OpCodesED.LDDR:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xb9 CPDR
			case (byte)Z80OpCodesED.CPDR:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xba INDR
			case (byte)Z80OpCodesED.INDR:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xbb OTDR
			case (byte)Z80OpCodesED.OTDR:
			// not implement
			#if Z80_OPCODES_TEST
			instrNotImp=true;
			#endif
			break;
			// 0xfb slttrap
			case (byte)Z80OpCodesED.slttrap:
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
