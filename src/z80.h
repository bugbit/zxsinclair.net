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

#define z80_a z80_regs.main.af.s2.a
#define z80_b z80_regs.main.bc.s.h
#define z80_c z80_regs.main.bc.s.l
#define z80_d z80_regs.main.de.s.h
#define z80_e z80_regs.main.de.s.l
#define z80_h z80_regs.main.hl.s.h
#define z80_l z80_regs.main.hl.s.l
#define z80_bc z80_regs.main.bc.w
#define z80_de z80_regs.main.de.w

#define ld_r_r1(r, r1) r = r1

enum Z80_OPCODES
{
    LD_A_A = 0x7F
};

// 8 bits
typedef unsigned char z80_byte;
// 16 bits
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
    struct
    {
        z80_byte a, f;
    } s2;
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

typedef struct
{
    z80_byte *memory;
    z80_byte (*read)(z80_word m);
    void (*poke)(z80_word m, z80_byte data);
    z80_byte (*fetchopcode)(z80_word m);
} z80_memory_bank;

extern z80_registers z80_regs;
extern int z80_tstates;
extern z80_byte *z80_memory_ptr;
extern z80_memory_bank *z80_memory;
extern int z80_memory_blank_sll;
extern int z80_memory_blank_count;
extern int z80_memory_blank_mod;
extern void (*z80_freememory)();

int z80_creatememory_default();

inline void z80_freememory_default()
{
    if (z80_memory_ptr != NULL)
    {
        delete z80_memory_ptr;
        z80_memory_ptr = NULL;
    }
    if (z80_memory != NULL)
    {
        delete[] z80_memory;
        z80_memory = NULL;
    }
}

inline void z80_reset()
{
    memset(&z80_regs, 0, sizeof(z80_regs));
    z80_tstates = 0;
}

inline z80_byte z80_readmem_default(z80_word m)
{
    return z80_memory_ptr[m];
}

inline void z80_pokemem_default(z80_word m, z80_byte data)
{
    z80_memory_ptr[m] = data;
}

inline z80_byte z80_readmem(z80_word m)
{
    return z80_memory[(m >> z80_memory_blank_sll) % z80_memory_blank_mod].read(m);
}

inline void z80_pokemem(z80_word m, z80_byte data)
{
    z80_memory[(m >> z80_memory_blank_sll) % z80_memory_blank_mod].poke(m, data);
}

inline z80_byte z80_fetchopcode(z80_word m)
{
    return z80_memory[(m >> z80_memory_blank_sll) % z80_memory_blank_mod].fetchopcode(m);
}

void z80_execopcode(z80_byte data);

inline void z80_execopcode()
{
    auto data = z80_fetchopcode(z80_regs.pc++);

    z80_tstates += 4;

    z80_execopcode(data);
}

#endif