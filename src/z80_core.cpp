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

z80_registers z80_regs;
int z80_tstates;
void (*z80_freememory)() = z80_freememory_default;
z80_byte *z80_memory_ptr;
z80_memory_bank *z80_memory;
int z80_memory_blank_sll;
int z80_memory_blank_count;
int z80_memory_blank_mod;
;

int z80_creatememory_default()
{
    z80_byte *m = new z80_byte[65535];

    if (m == NULL)
    {
        perror("not enough memory");

        return -1;
    }

    z80_memory_bank *b = new z80_memory_bank();

    if (b == NULL)
    {
        delete[] m;
        perror("not enough memory");

        return -1;
    }

    z80_memory_blank_sll = 0;
    z80_memory_blank_count = 1;
    z80_memory_blank_mod = 2;
    b->memory = m;
    b->read = z80_readmem_default;
    b->poke = z80_pokemem_default;
    b->fetchopcode = z80_readmem_default;
    z80_memory_ptr = m;
    z80_memory = b;

    return 0;
}