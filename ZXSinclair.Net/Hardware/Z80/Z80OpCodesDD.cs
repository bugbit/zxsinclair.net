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

public enum Z80OpCodesDD
{
    //NOP = 0
	ADD_IX_BC = 0x09,
	ADD_IX_DE = 0x19,
	LD_IX_nnnn = 0x21,
	LD_MM_nnnn_MM_IX = 0x22,
	INC_IX = 0x23,
	INC_IXH = 0x24,
	DEC_IXH = 0x25,
	LD_IXH_nn = 0x26,
	ADD_IX_IX = 0x29,
	LD_IX_MM_nnnn_MM = 0x2a,
	DEC_IX = 0x2b,
	INC_IXL = 0x2c,
	DEC_IXL = 0x2d,
	LD_IXL_nn = 0x2e,
	INC_MM_IX_PLUS_dd_MM = 0x34,
	DEC_MM_IX_PLUS_dd_MM = 0x35,
	LD_MM_IX_PLUS_dd_MM_nn = 0x36,
	ADD_IX_SP = 0x39,
	LD_B_IXH = 0x44,
	LD_B_IXL = 0x45,
	LD_B_MM_IX_PLUS_dd_MM = 0x46,
	LD_C_IXH = 0x4c,
	LD_C_IXL = 0x4d,
	LD_C_MM_IX_PLUS_dd_MM = 0x4e,
	LD_D_IXH = 0x54,
	LD_D_IXL = 0x55,
	LD_D_MM_IX_PLUS_dd_MM = 0x56,
	LD_E_IXH = 0x5c,
	LD_E_IXL = 0x5d,
	LD_E_MM_IX_PLUS_dd_MM = 0x5e,
	LD_IXH_B = 0x60,
	LD_IXH_C = 0x61,
	LD_IXH_D = 0x62,
	LD_IXH_E = 0x63,
	LD_IXH_IXH = 0x64,
	LD_IXH_IXL = 0x65,
	LD_H_MM_IX_PLUS_dd_MM = 0x66,
	LD_IXH_A = 0x67,
	LD_IXL_B = 0x68,
	LD_IXL_C = 0x69,
	LD_IXL_D = 0x6a,
	LD_IXL_E = 0x6b,
	LD_IXL_IXH = 0x6c,
	LD_IXL_IXL = 0x6d,
	LD_L_MM_IX_PLUS_dd_MM = 0x6e,
	LD_IXL_A = 0x6f,
	LD_MM_IX_PLUS_dd_MM_B = 0x70,
	LD_MM_IX_PLUS_dd_MM_C = 0x71,
	LD_MM_IX_PLUS_dd_MM_D = 0x72,
	LD_MM_IX_PLUS_dd_MM_E = 0x73,
	LD_MM_IX_PLUS_dd_MM_H = 0x74,
	LD_MM_IX_PLUS_dd_MM_L = 0x75,
	LD_MM_IX_PLUS_dd_MM_A = 0x77,
	LD_A_IXH = 0x7c,
	LD_A_IXL = 0x7d,
	LD_A_MM_IX_PLUS_dd_MM = 0x7e,
	ADD_A_IXH = 0x84,
	ADD_A_IXL = 0x85,
	ADD_A_MM_IX_PLUS_dd_MM = 0x86,
	ADC_A_IXH = 0x8c,
	ADC_A_IXL = 0x8d,
	ADC_A_MM_IX_PLUS_dd_MM = 0x8e,
	SUB_A_IXH = 0x94,
	SUB_A_IXL = 0x95,
	SUB_A_MM_IX_PLUS_dd_MM = 0x96,
	SBC_A_IXH = 0x9c,
	SBC_A_IXL = 0x9d,
	SBC_A_MM_IX_PLUS_dd_MM = 0x9e,
	AND_A_IXH = 0xa4,
	AND_A_IXL = 0xa5,
	AND_A_MM_IX_PLUS_dd_MM = 0xa6,
	XOR_A_IXH = 0xac,
	XOR_A_IXL = 0xad,
	XOR_A_MM_IX_PLUS_dd_MM = 0xae,
	OR_A_IXH = 0xb4,
	OR_A_IXL = 0xb5,
	OR_A_MM_IX_PLUS_dd_MM = 0xb6,
	CP_A_IXH = 0xbc,
	CP_A_IXL = 0xbd,
	CP_A_MM_IX_PLUS_dd_MM = 0xbe,
	shift_DDFDCB = 0xcb,
	POP_IX = 0xe1,
	EX_MM_SP_MM_IX = 0xe3,
	PUSH_IX = 0xe5,
	JP_IX = 0xe9,
	LD_SP_IX = 0xf9,
    
}