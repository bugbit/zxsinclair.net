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

void endiantest();

//8 bits
typedef unsigned char z80_byte;
//16 bits
typedef unsigned short z80_int;

typedef union {
// #if BIG_ENDIAN
// big endian
//  struct { z80_byte h,l; } s;
//#else
// litle endian
  struct { z80_byte l,h; } s;
//#endif

  z80_int x;
} z80_registro;

int main()
{
    endiantest();
    z80_registro a,b;

    a.x=0x1234;
    b.s.l=0x34;
    b.s.h=0x12;

    assert(a.x==b.x);

    return 0;
}