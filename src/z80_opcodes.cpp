/*
    ZXSinclair Emulador ZX Computers
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

#include "pch.h"
#include "z80.h"

void z80_execopcode(z80_byte data)
{
    z80_byte n;

    switch (data)
    {
    case Z80_OPCODES::LD_A_A:
        // LD A,A
        ld_r_r1(z80_a, z80_a);
        break;
    case Z80_OPCODES::LD_A_B:
        // LD A,B
        ld_r_r1(z80_a, z80_b);
        break;
    case Z80_OPCODES::LD_A_C:
        // LD A,C
        ld_r_r1(z80_a, z80_c);
        break;
    case Z80_OPCODES::LD_A_D:
        // LD A,D
        ld_r_r1(z80_a, z80_d);
        break;
    case Z80_OPCODES::LD_A_E:
        // LD A,E
        ld_r_r1(z80_a, z80_e);
        break;
    case Z80_OPCODES::LD_A_H:
        // LD A,H
        ld_r_r1(z80_a, z80_h);
        break;
    case Z80_OPCODES::LD_A_L:
        // LD A,L
        ld_r_r1(z80_a, z80_l);
        break;
    case Z80_OPCODES::LD_B_A:
        // LD B,A
        ld_r_r1(z80_b, z80_a);
        break;
    case Z80_OPCODES::LD_B_B:
        // LD B,B
        ld_r_r1(z80_b, z80_b);
        break;
    case Z80_OPCODES::LD_B_C:
        // LD B,C
        ld_r_r1(z80_b, z80_c);
        break;
    case Z80_OPCODES::LD_B_D:
        // LD B,D
        ld_r_r1(z80_b, z80_d);
        break;
    case Z80_OPCODES::LD_B_E:
        // LD B,E
        ld_r_r1(z80_b, z80_e);
        break;
    case Z80_OPCODES::LD_B_H:
        // LD B,H
        ld_r_r1(z80_b, z80_h);
        break;
    case Z80_OPCODES::LD_B_L:
        // LD B,L
        ld_r_r1(z80_b, z80_l);
        break;
    case Z80_OPCODES::LD_C_A:
        // LD C,A
        ld_r_r1(z80_c, z80_a);
        break;
    case Z80_OPCODES::LD_C_B:
        // LD C,B
        ld_r_r1(z80_c, z80_b);
        break;
    case Z80_OPCODES::LD_C_C:
        // LD C,C
        ld_r_r1(z80_c, z80_c);
        break;
    case Z80_OPCODES::LD_C_D:
        // LD C,D
        ld_r_r1(z80_c, z80_d);
        break;
    case Z80_OPCODES::LD_C_E:
        // LD C,E
        ld_r_r1(z80_c, z80_e);
        break;
    case Z80_OPCODES::LD_C_H:
        // LD C,H
        ld_r_r1(z80_c, z80_h);
        break;
    case Z80_OPCODES::LD_C_L:
        // LD C,L
        ld_r_r1(z80_c, z80_l);
        break;
    case Z80_OPCODES::LD_D_A:
        // LD D,A
        ld_r_r1(z80_d, z80_a);
        break;
    case Z80_OPCODES::LD_D_B:
        // LD D,B
        ld_r_r1(z80_d, z80_b);
        break;
    case Z80_OPCODES::LD_D_C:
        // LD D,C
        ld_r_r1(z80_d, z80_c);
        break;
    case Z80_OPCODES::LD_D_D:
        // LD D,D
        ld_r_r1(z80_d, z80_d);
        break;
    case Z80_OPCODES::LD_D_E:
        // LD D,E
        ld_r_r1(z80_d, z80_e);
        break;
    case Z80_OPCODES::LD_D_H:
        // LD D,H
        ld_r_r1(z80_d, z80_h);
        break;
    case Z80_OPCODES::LD_D_L:
        // LD D,L
        ld_r_r1(z80_d, z80_l);
        break;
    case Z80_OPCODES::LD_E_A:
        // LD E,A
        ld_r_r1(z80_e, z80_a);
        break;
    case Z80_OPCODES::LD_E_B:
        // LD E,B
        ld_r_r1(z80_e, z80_b);
        break;
    case Z80_OPCODES::LD_E_C:
        // LD E,C
        ld_r_r1(z80_e, z80_c);
        break;
    case Z80_OPCODES::LD_E_D:
        // LD E,D
        ld_r_r1(z80_e, z80_d);
        break;
    case Z80_OPCODES::LD_E_E:
        // LD E,E
        ld_r_r1(z80_e, z80_e);
        break;
    case Z80_OPCODES::LD_E_H:
        // LD E,H
        ld_r_r1(z80_e, z80_h);
        break;
    case Z80_OPCODES::LD_E_L:
        // LD E,L
        ld_r_r1(z80_e, z80_l);
        break;
    case Z80_OPCODES::LD_H_A:
        // LD H,A
        ld_r_r1(z80_h, z80_a);
        break;
    case Z80_OPCODES::LD_H_B:
        // LD H,B
        ld_r_r1(z80_h, z80_b);
        break;
    case Z80_OPCODES::LD_H_C:
        // LD H,C
        ld_r_r1(z80_h, z80_c);
        break;
    case Z80_OPCODES::LD_H_D:
        // LD H,D
        ld_r_r1(z80_h, z80_d);
        break;
    case Z80_OPCODES::LD_H_E:
        // LD H,E
        ld_r_r1(z80_h, z80_e);
        break;
    case Z80_OPCODES::LD_H_H:
        // LD H,H
        ld_r_r1(z80_h, z80_h);
        break;
    case Z80_OPCODES::LD_H_L:
        // LD H,L
        ld_r_r1(z80_h, z80_l);
        break;
    case Z80_OPCODES::LD_L_A:
        // LD L,A
        ld_r_r1(z80_l, z80_a);
        break;
    case Z80_OPCODES::LD_L_B:
        // LD L,B
        ld_r_r1(z80_l, z80_b);
        break;
    case Z80_OPCODES::LD_L_C:
        // LD L,C
        ld_r_r1(z80_l, z80_c);
        break;
    case Z80_OPCODES::LD_L_D:
        // LD L,D
        ld_r_r1(z80_l, z80_d);
        break;
    case Z80_OPCODES::LD_L_E:
        // LD L,E
        ld_r_r1(z80_l, z80_e);
        break;
    case Z80_OPCODES::LD_L_H:
        // LD L,H
        ld_r_r1(z80_l, z80_h);
        break;
    case Z80_OPCODES::LD_L_L:
        // LD L,L
        ld_r_r1(z80_l, z80_l);
        break;

    case Z80_OPCODES::LD_A_N:
        // LD A,N
        ld_r_n(z80_a);
        break;
    case Z80_OPCODES::LD_B_N:
        // LD B,N
        ld_r_n(z80_b);
        break;
    case Z80_OPCODES::LD_C_N:
        // LD C,N
        ld_r_n(z80_c);
        break;
    case Z80_OPCODES::LD_D_N:
        // LD D,N
        ld_r_n(z80_d);
        break;
    case Z80_OPCODES::LD_E_N:
        // LD E,N
        ld_r_n(z80_e);
        break;
    case Z80_OPCODES::LD_H_N:
        // LD H,N
        ld_r_n(z80_h);
        break;
    case Z80_OPCODES::LD_L_N:
        // LD L,N
        ld_r_n(z80_l);
        break;

    case Z80_OPCODES::LD_A_M_HL_M:
        // LD A,(HL)
        ld_r_m_rr_m(z80_a, z80_hl);
        break;

    case Z80_OPCODES::NOP:
    // NOP
    default:
        // NOP
        nop();
        break;
    }
}