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

    int tstates;
    z80_byte n;
    z80_word bc, de, hl;

    printf("ld a,a\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_A_A);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_A_B);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_A_C);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_A_D);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_A_E);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_A_H);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_A_L);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_B_A);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_B_B);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_B_C);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_B_D);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_B_E);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_B_H);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_B_L);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_C_A);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_C_B);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_C_C);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_C_D);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_C_E);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_C_H);
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
    z80_pokemem_notime(0, Z80_OPCODES::LD_C_L);
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

    printf("ld d,a\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_D_A);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 1);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld d,b\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_D_B);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 2);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld d,c\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_D_C);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 3);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld d,d\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_D_D);
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

    printf("ld d,e\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_D_E);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 5);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld d,h\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_D_H);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 6);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld d,l\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_D_L);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 7);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld e,a\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_E_A);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 1);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld e,b\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_E_B);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 2);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld e,c\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_E_C);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 3);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld e,d\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_E_D);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 4);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld e,e\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_E_E);
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

    printf("ld e,h\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_E_H);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 6);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld e,l\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_E_L);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 7);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld h,a\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_H_A);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 1);
    assert(z80_l == 7);

    printf("ld h,b\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_H_B);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 2);
    assert(z80_l == 7);

    printf("ld h,c\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_H_C);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 3);
    assert(z80_l == 7);

    printf("ld h,d\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_H_D);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 4);
    assert(z80_l == 7);

    printf("ld h,e\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_H_E);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 5);
    assert(z80_l == 7);

    printf("ld h,h\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_H_H);
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

    printf("ld h,l\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_H_L);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 7);
    assert(z80_l == 7);

    printf("ld l,a\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_L_A);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 1);

    printf("ld l,b\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_L_B);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 2);

    printf("ld l,c\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_L_C);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 3);

    printf("ld l,d\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_L_D);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 4);

    printf("ld l,e\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_L_E);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 5);

    printf("ld l,h\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_L_H);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4);
    assert(z80_pc == 1);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 6);

    printf("ld l,l\n");
    z80_reset();

    tstates = z80_tstates;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_L_L);
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

    printf("ld a,n\n");
    z80_reset();

    tstates = z80_tstates;
    n = 205;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_A_N);
    z80_pokemem_notime(1, n);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4 + 3);
    assert(z80_pc == 2);
    assert(z80_a == 205);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld b,n\n");
    z80_reset();

    tstates = z80_tstates;
    n = 205;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_B_N);
    z80_pokemem_notime(1, n);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4 + 3);
    assert(z80_pc == 2);
    assert(z80_a == 1);
    assert(z80_b == 205);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld c,n\n");
    z80_reset();

    tstates = z80_tstates;
    n = 205;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_C_N);
    z80_pokemem_notime(1, n);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4 + 3);
    assert(z80_pc == 2);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 205);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld d,n\n");
    z80_reset();

    tstates = z80_tstates;
    n = 205;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_D_N);
    z80_pokemem_notime(1, n);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4 + 3);
    assert(z80_pc == 2);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 205);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld e,n\n");
    z80_reset();

    tstates = z80_tstates;
    n = 205;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_E_N);
    z80_pokemem_notime(1, n);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4 + 3);
    assert(z80_pc == 2);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 205);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ld h,n\n");
    z80_reset();

    tstates = z80_tstates;
    n = 205;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_H_N);
    z80_pokemem_notime(1, n);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4 + 3);
    assert(z80_pc == 2);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 205);
    assert(z80_l == 7);

    printf("ld l,n\n");
    z80_reset();

    tstates = z80_tstates;
    n = 205;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;
    z80_pokemem_notime(0, Z80_OPCODES::LD_L_N);
    z80_pokemem_notime(1, n);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4 + 3);
    assert(z80_pc == 2);
    assert(z80_a == 1);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 205);

    printf("ld a,(hl)\n");
    z80_reset();

    tstates = z80_tstates;
    n = 205;

    z80_a = 1;
    z80_b = 2;
    z80_c = 3;
    z80_d = 4;
    z80_e = 5;
    z80_h = 6;
    z80_l = 7;

    hl = z80_hl;

    z80_pokemem_notime(0, Z80_OPCODES::LD_A_M_HL_M);
    z80_pokemem_notime(hl, n);
    z80_instrfetch();
    assert(z80_tstates == tstates + 4 + 3);
    assert(z80_pc == 1);
    assert(z80_a == n);
    assert(z80_b == 2);
    assert(z80_c == 3);
    assert(z80_d == 4);
    assert(z80_e == 5);
    assert(z80_h == 6);
    assert(z80_l == 7);

    printf("ok\n");

    z80_freememory();
}