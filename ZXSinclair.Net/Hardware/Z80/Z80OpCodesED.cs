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

public enum Z80OpCodesED
{
    //NOP = 0
	IN_B_MM_C_MM = 0x40,
	OUT_MM_C_MM_B = 0x41,
	SBC_HL_BC = 0x42,
	LD_MM_nnnn_MM_BC = 0x43,
	NEG = 0x7c,
	RETN = 0x7d,
	IM_0 = 0x6e,
	LD_I_A = 0x47,
	IN_C_MM_C_MM = 0x48,
	OUT_MM_C_MM_C = 0x49,
	ADC_HL_BC = 0x4a,
	LD_BC_MM_nnnn_MM = 0x4b,
	LD_R_A = 0x4f,
	IN_D_MM_C_MM = 0x50,
	OUT_MM_C_MM_D = 0x51,
	SBC_HL_DE = 0x52,
	LD_MM_nnnn_MM_DE = 0x53,
	IM_1 = 0x76,
	LD_A_I = 0x57,
	IN_E_MM_C_MM = 0x58,
	OUT_MM_C_MM_E = 0x59,
	ADC_HL_DE = 0x5a,
	LD_DE_MM_nnnn_MM = 0x5b,
	IM_2 = 0x7e,
	LD_A_R = 0x5f,
	IN_H_MM_C_MM = 0x60,
	OUT_MM_C_MM_H = 0x61,
	SBC_HL_HL = 0x62,
	LD_MM_nnnn_MM_HL = 0x63,
	RRD = 0x67,
	IN_L_MM_C_MM = 0x68,
	OUT_MM_C_MM_L = 0x69,
	ADC_HL_HL = 0x6a,
	LD_HL_MM_nnnn_MM = 0x6b,
	RLD = 0x6f,
	IN_F_MM_C_MM = 0x70,
	OUT_MM_C_MM_0 = 0x71,
	SBC_HL_SP = 0x72,
	LD_MM_nnnn_MM_SP = 0x73,
	IN_A_MM_C_MM = 0x78,
	OUT_MM_C_MM_A = 0x79,
	ADC_HL_SP = 0x7a,
	LD_SP_MM_nnnn_MM = 0x7b,
	LDI = 0xa0,
	CPI = 0xa1,
	INI = 0xa2,
	OUTI = 0xa3,
	LDD = 0xa8,
	CPD = 0xa9,
	IND = 0xaa,
	OUTD = 0xab,
	LDIR = 0xb0,
	CPIR = 0xb1,
	INIR = 0xb2,
	OTIR = 0xb3,
	LDDR = 0xb8,
	CPDR = 0xb9,
	INDR = 0xba,
	OTDR = 0xbb,
	slttrap = 0xfb,
    
}