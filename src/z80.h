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

#ifndef __Z80_H__

#define __Z80_H__

//8 bits
typedef unsigned char z80_byte;
//16 bits
typedef unsigned short z80_word;

typedef union
{
    // #if BIG_ENDIAN
    // big endian
    //  struct { z80_byte h,l; } s;
    //#else
    // litle endian
    struct
    {
        z80_byte l, h;
    } s;
    //#endif
    z80_word w;
} z80_register;

typedef struct
{
    z80_register af;
    z80_register bc;
    z80_register de;
    z80_register hl;
} z80_register_set;

typedef struct
{
    z80_register_set main;
    z80_register_set alternative;
    z80_register ir;
    z80_word ix;
    z80_word iy;
    z80_word sp;
    z80_word pc;
} z80_registers;

#endif