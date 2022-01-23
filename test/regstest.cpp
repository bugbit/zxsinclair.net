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

void endiantest()
{
    printf("endiantest\n");

    z80_register r;

    r.w = 0x1234;

    assert(r.s.l = 0x34);
    assert(r.s.h = 0x12);

    z80_a = 0x34;
    z80_f = 0x12;

    assert(z80_af == r.w);

    z80_c = 0x34;
    z80_b = 0x12;

    assert(z80_bc == r.w);

    z80_e = 0x34;
    z80_d = 0x12;

    assert(z80_de == r.w);

    z80_l = 0x34;
    z80_h = 0x12;

    assert(z80_hl == r.w);

    printf("ok\n");
}