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

public enum Z80OpCodesFD
{
    //NOP = 0
	ADD_IY_BC = 0x09,
	ADD_IY_DE = 0x19,
	LD_IY_nnnn = 0x21,
	LD_MM_nnnn_MM_IY = 0x22,
	INC_IY = 0x23,
	INC_IYH = 0x24,
	DEC_IYH = 0x25,
	LD_IYH_nn = 0x26,
	ADD_IY_IY = 0x29,
	LD_IY_MM_nnnn_MM = 0x2a,
	DEC_IY = 0x2b,
	INC_IYL = 0x2c,
	DEC_IYL = 0x2d,
	LD_IYL_nn = 0x2e,
	INC_MM_IY_PLUS_dd_MM = 0x34,
	DEC_MM_IY_PLUS_dd_MM = 0x35,
	LD_MM_IY_PLUS_dd_MM_nn = 0x36,
	ADD_IY_SP = 0x39,
	LD_B_IYH = 0x44,
	LD_B_IYL = 0x45,
	LD_B_MM_IY_PLUS_dd_MM = 0x46,
	LD_C_IYH = 0x4c,
	LD_C_IYL = 0x4d,
	LD_C_MM_IY_PLUS_dd_MM = 0x4e,
	LD_D_IYH = 0x54,
	LD_D_IYL = 0x55,
	LD_D_MM_IY_PLUS_dd_MM = 0x56,
	LD_E_IYH = 0x5c,
	LD_E_IYL = 0x5d,
	LD_E_MM_IY_PLUS_dd_MM = 0x5e,
	LD_IYH_B = 0x60,
	LD_IYH_C = 0x61,
	LD_IYH_D = 0x62,
	LD_IYH_E = 0x63,
	LD_IYH_IYH = 0x64,
	LD_IYH_IYL = 0x65,
	LD_H_MM_IY_PLUS_dd_MM = 0x66,
	LD_IYH_A = 0x67,
	LD_IYL_B = 0x68,
	LD_IYL_C = 0x69,
	LD_IYL_D = 0x6a,
	LD_IYL_E = 0x6b,
	LD_IYL_IYH = 0x6c,
	LD_IYL_IYL = 0x6d,
	LD_L_MM_IY_PLUS_dd_MM = 0x6e,
	LD_IYL_A = 0x6f,
	LD_MM_IY_PLUS_dd_MM_B = 0x70,
	LD_MM_IY_PLUS_dd_MM_C = 0x71,
	LD_MM_IY_PLUS_dd_MM_D = 0x72,
	LD_MM_IY_PLUS_dd_MM_E = 0x73,
	LD_MM_IY_PLUS_dd_MM_H = 0x74,
	LD_MM_IY_PLUS_dd_MM_L = 0x75,
	LD_MM_IY_PLUS_dd_MM_A = 0x77,
	LD_A_IYH = 0x7c,
	LD_A_IYL = 0x7d,
	LD_A_MM_IY_PLUS_dd_MM = 0x7e,
	ADD_A_IYH = 0x84,
	ADD_A_IYL = 0x85,
	ADD_A_MM_IY_PLUS_dd_MM = 0x86,
	ADC_A_IYH = 0x8c,
	ADC_A_IYL = 0x8d,
	ADC_A_MM_IY_PLUS_dd_MM = 0x8e,
	SUB_A_IYH = 0x94,
	SUB_A_IYL = 0x95,
	SUB_A_MM_IY_PLUS_dd_MM = 0x96,
	SBC_A_IYH = 0x9c,
	SBC_A_IYL = 0x9d,
	SBC_A_MM_IY_PLUS_dd_MM = 0x9e,
	AND_A_IYH = 0xa4,
	AND_A_IYL = 0xa5,
	AND_A_MM_IY_PLUS_dd_MM = 0xa6,
	XOR_A_IYH = 0xac,
	XOR_A_IYL = 0xad,
	XOR_A_MM_IY_PLUS_dd_MM = 0xae,
	OR_A_IYH = 0xb4,
	OR_A_IYL = 0xb5,
	OR_A_MM_IY_PLUS_dd_MM = 0xb6,
	CP_A_IYH = 0xbc,
	CP_A_IYL = 0xbd,
	CP_A_MM_IY_PLUS_dd_MM = 0xbe,
	shift_DDFDCB = 0xcb,
	POP_IY = 0xe1,
	EX_MM_SP_MM_IY = 0xe3,
	PUSH_IY = 0xe5,
	JP_IY = 0xe9,
	LD_SP_IY = 0xf9,
    
}