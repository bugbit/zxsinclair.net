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

void z80opcodestest()
{
    z80_creatememory_default();

    printf("ld a,a\n");
    z80_reset();

    auto tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_A_A);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld a,b\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_A_B);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 2);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld a,c\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_A_C);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 3);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld a,d\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_A_D);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 4);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld a,e\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_A_E);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 5);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld a,h\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_A_H);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 6);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld a,l\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_A_L);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 7);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld b,a\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_B_A);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 1);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld b,b\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_B_B);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld b,c\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_B_C);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 3);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld b,d\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_B_D);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 4);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld b,e\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_B_E);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 5);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld b,h\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_B_H);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 6);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld b,l\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_B_L);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 7);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld c,a\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_C_A);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 1);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld c,b\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_C_B);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 2);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld c,c\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_C_C);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld c,d\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_C_D);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 4);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld c,e\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_C_E);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 5);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld c,h\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_C_H);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 6);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld c,l\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem(0, Z80_OPCODES::LD_C_L);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 7);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    z80_freememory_default();
}