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

namespace ZXSinclair.Machines.Z80
{
    public partial class OpCodes
    {
        // M_HL_M = (HL)
        // M_IX_D_M = (IX+d)

        public const int R_A = 0b111;
        public const int R_B = 0b000;
        public const int R_C = 0b001;
        public const int R_D = 0b010;
        public const int R_E = 0b011;
        public const int R_H = 0b100;
        public const int R_L = 0b101;

        public static readonly int[] Rs = { R_A, R_B, R_C, R_D, R_E, R_H, R_L };
        public static readonly char[] Car_Rs = { 'A', 'B', 'C', 'D', 'E', 'H', 'L' };

        public const int LD_r_r1 = 0b01000000;
        public const int LD_r_r1_msk = 0b11000000;

        public const int LD_r_n = 0b00000110;
        public const int LD_r_n_mask = 0b11000111;

        public const int LD_r_m_hl_m = 0b01000110;
        public const int LD_r_m_hl_m_mask = 0b11000111;

        public const int LD_r_m_ix_d_m = 0b01000110;
        public const int LD_r_m_ix_d_m_mask = 0b11000111;

        public const int LD_r_m_iy_d_m = 0b01000110;
        public const int LD_r_m_iy_d_m_mask = 0b11000111;

        public const int LD_m_hl_m_r = 0b01110000;
        public const int LD_m_hl_m_r_mask = 0b11111000;

        public const int LD_m_ix_d_m_r = 0b01110000;
        public const int LD_m_ix_d_m_r_mask = 0b11111000;

        public const int LD_m_iy_d_m_r = 0b01110000;
        public const int LD_m_iy_d_m_r_mask = 0b11111000;

        public const int LD_m_hl_m_n = 0b00110110;
        public const int LD_m_hl_m_n_mask = 0b00110110;

        public const int LD_m_ix_d_m_n = 0b00110110;
        public const int LD_m_ix_d_m_n_mask = 0b00110110;

        public const int LD_m_iy_d_m_n = 0b00110110;
        public const int LD_m_iy_d_m_n_mask = 0b00110110;

        public const int LD_a_m_bc_m = 0b00001010;
        public const int LD_a_m_bc_m_mask = 0b00001010;

        public const int LD_a_m_de_m = 0b00011010;
        public const int LD_a_m_de_m_mask = 0b00011010;

        public const int LD_a_m_nnn_m = 0b00111010;
        public const int LD_a_m_nnn_m_mask = 0b00111010;

        public const int LD_m_bc_m_a = 0b00000010;
        public const int LD_m_bc_m_a_mask = 0b00000010;

        public const int LD_m_de_m_a = 0b00010010;
        public const int LD_m_de_m_a_mask = 0b00010010;

        public const int LD_m_nn_m_a = 0b00110010;
        public const int LD_m_nn_m_a_mask = 0b00110010;

        public const int LD_a_i = 0b01010111;
        public const int LD_a_i_mask = 0b01010111;

        public const int LD_a_r = 0b01011111;
        public const int LD_a_r_mask = 0b01011111;

        public const int LD_i_a = 0b01000111;
        public const int LD_i_a_mask = 0b01000111;

        public const int LD_r_a = 0b01000111;
        public const int LD_r_a_mask = 0b01000111;

        public const int NOP = 0x00;

        public const int LD_B_N = 0x06;

        public const int LD_C_N = 0x0E;

        public const int LD_D_N = 0x16;

        public const int LD_E_N = 0x1E;

        public const int LD_H_N = 0x26;

        public const int LD_L_N = 0x2E;

        public const int LD_A_N = 0x3E;

        public const int LD_B_B = 0x40;
        public const int LD_B_C = 0x41;
        public const int LD_B_D = 0x42;
        public const int LD_B_E = 0x43;
        public const int LD_B_H = 0x44;
        public const int LD_B_L = 0x45;
        public const int LD_B_M_HL_M = 0x46;
        public const int LD_B_A = 0x47;
        public const int LD_C_B = 0x48;
        public const int LD_C_C = 0x49;
        public const int LD_C_D = 0x4A;
        public const int LD_C_E = 0x4B;
        public const int LD_C_H = 0x4C;
        public const int LD_C_L = 0x4D;
        public const int LD_C_M_HL_M = 0x4E;
        public const int LD_C_A = 0x4F;
        public const int LD_D_B = 0x50;
        public const int LD_D_C = 0x51;
        public const int LD_D_D = 0x52;
        public const int LD_D_E = 0x53;
        public const int LD_D_H = 0x54;
        public const int LD_D_L = 0x55;
        public const int LD_D_M_HL_M = 0x56;
        public const int LD_D_A = 0x57;
        public const int LD_E_B = 0x58;
        public const int LD_E_C = 0x59;
        public const int LD_E_D = 0x5A;
        public const int LD_E_E = 0x5B;
        public const int LD_E_H = 0x5C;
        public const int LD_E_L = 0x5D;
        public const int LD_E_M_HL_M = 0x5E;
        public const int LD_E_A = 0x5F;
        public const int LD_H_B = 0x60;
        public const int LD_H_C = 0x61;
        public const int LD_H_D = 0x62;
        public const int LD_H_E = 0x63;
        public const int LD_H_H = 0x64;
        public const int LD_H_L = 0x65;
        public const int LD_H_M_HL_M = 0x66;
        public const int LD_H_A = 0x67;
        public const int LD_L_B = 0x68;
        public const int LD_L_C = 0x69;
        public const int LD_L_D = 0x6A;
        public const int LD_L_E = 0x6B;
        public const int LD_L_H = 0x6C;
        public const int LD_L_L = 0x6D;
        public const int LD_L_M_HL_M = 0x6E;
        public const int LD_L_A = 0x6F;

        public const int LD_A_B = 0x78;
        public const int LD_A_C = 0x79;
        public const int LD_A_D = 0x7A;
        public const int LD_A_E = 0x7B;
        public const int LD_A_H = 0x7C;
        public const int LD_A_L = 0x7D;
        public const int LD_A_M_HL_M = 0x7E;
        public const int LD_A_A = 0x7F;
        public const int LD_M_HL_M_A = 0x77;
        public const int LD_M_HL_M_B = 0x70;
        public const int LD_M_HL_M_C = 0x71;
        public const int LD_M_HL_M_D = 0x72;
        public const int LD_M_HL_M_E = 0x73;
        public const int LD_M_HL_M_H = 0x74;
        public const int LD_M_HL_M_L = 0x75;
        public const int LD_M_HL_M_N = 0x36;
        public const int LD_A_M_BC_M = 0x0A;
        public const int LD_A_M_DE_M = 0x1A;
        public const int LD_A_M_NN_M = 0x3A;
        public const int LD_M_BC_M_A = 0x2;
        public const int LD_M_DE_M_A = 0x12;
        public const int LD_M_NN_M_A = 0x32;

        public const int OPCODES_DD = 0xDD;

        public const int LD_A_M_IX_D_M = 0x7E;
        public const int LD_B_M_IX_D_M = 0x46;
        public const int LD_C_M_IX_D_M = 0x4E;
        public const int LD_D_M_IX_D_M = 0x56;
        public const int LD_E_M_IX_D_M = 0x5E;
        public const int LD_H_M_IX_D_M = 0x66;
        public const int LD_L_M_IX_D_M = 0x6E;
        public const int LD_M_IX_D_M_A = 0x77;
        public const int LD_M_IX_D_M_B = 0x70;
        public const int LD_M_IX_D_M_C = 0x71;
        public const int LD_M_IX_D_M_D = 0x72;
        public const int LD_M_IX_D_M_E = 0x73;
        public const int LD_M_IX_D_M_H = 0x74;
        public const int LD_M_IX_D_M_L = 0x75;
        public const int LD_M_IX_D_M_N = 0x36;

        public const int OPCODES_FD = 0xFD;

        public const int LD_A_M_IY_D_M = 0x7E;
        public const int LD_B_M_IY_D_M = 0x46;
        public const int LD_C_M_IY_D_M = 0x4E;
        public const int LD_D_M_IY_D_M = 0x56;
        public const int LD_E_M_IY_D_M = 0x5E;
        public const int LD_H_M_IY_D_M = 0x66;
        public const int LD_L_M_IY_D_M = 0x6E;
        public const int LD_M_IY_D_M_A = 0x77;
        public const int LD_M_IY_D_M_B = 0x70;
        public const int LD_M_IY_D_M_C = 0x71;
        public const int LD_M_IY_D_M_D = 0x72;
        public const int LD_M_IY_D_M_E = 0x73;
        public const int LD_M_IY_D_M_H = 0x74;
        public const int LD_M_IY_D_M_L = 0x75;
        public const int LD_M_IY_D_M_N = 0x36;

        public const int OPCODES_ED = 0xED;

        public const int LD_A_I = 0x57;
        public const int LD_A_R = 0x5F;
        public const int LD_I_A = 0x47;
        public const int LD_R_A = 0x4F;
    }
}
