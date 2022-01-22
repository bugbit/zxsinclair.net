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
    switch (data)
    {
    case Z80_OPCODES::LD_A_A:
        ld_r_r1(z80_a, z80_a);
        break;
    case Z80_OPCODES::LD_A_B:
        ld_r_r1(z80_a, z80_b);
        break;
    case Z80_OPCODES::LD_A_C:
        ld_r_r1(z80_a, z80_c);
        break;
    case Z80_OPCODES::LD_A_D:
        ld_r_r1(z80_a, z80_d);
        break;
    case Z80_OPCODES::LD_A_E:
        ld_r_r1(z80_a, z80_e);
        break;
    case Z80_OPCODES::LD_A_H:
        ld_r_r1(z80_a, z80_h);
        break;
    case Z80_OPCODES::LD_A_L:
        ld_r_r1(z80_a, z80_l);
        break;
    case Z80_OPCODES::LD_B_A:
        ld_r_r1(z80_b, z80_a);
        break;
    case Z80_OPCODES::LD_B_B:
        ld_r_r1(z80_b, z80_b);
        break;
    case Z80_OPCODES::LD_B_C:
        ld_r_r1(z80_b, z80_c);
        break;
    case Z80_OPCODES::LD_B_D:
        ld_r_r1(z80_b, z80_d);
        break;
    case Z80_OPCODES::LD_B_E:
        ld_r_r1(z80_b, z80_e);
        break;
    case Z80_OPCODES::LD_B_H:
        ld_r_r1(z80_b, z80_h);
        break;
    case Z80_OPCODES::LD_B_L:
        ld_r_r1(z80_b, z80_l);
        break;
    case Z80_OPCODES::LD_C_A:
        ld_r_r1(z80_c, z80_a);
        break;
    case Z80_OPCODES::LD_C_B:
        ld_r_r1(z80_c, z80_b);
        break;
    case Z80_OPCODES::LD_C_C:
        ld_r_r1(z80_c, z80_c);
        break;
    case Z80_OPCODES::LD_C_D:
        ld_r_r1(z80_c, z80_d);
        break;
    case Z80_OPCODES::LD_C_E:
        ld_r_r1(z80_c, z80_e);
        break;
    case Z80_OPCODES::LD_C_H:
        ld_r_r1(z80_c, z80_h);
        break;
    case Z80_OPCODES::LD_C_L:
        ld_r_r1(z80_c, z80_l);
        break;

    case Z80_OPCODES::NOP:
    default:
        nop();
        break;
    }
}