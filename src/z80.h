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
#define z80_f z80_regs.main.af.s2.f
#define z80_i z80_regs.ir.s3.i
#define z80_r z80_regs.ir.s3.r
#define z80_bc z80_regs.main.bc.w
#define z80_de z80_regs.main.de.w
#define z80_hl z80_regs.main.hl.w
#define z80_af z80_regs.main.af.w
#define z80_pc z80_regs.pc

#define nop()
#define ld_r_r1(r, r1) r = r1

enum Z80_OPCODES
{
    NOP = 0x0,
    LD_B_B = 0x40,
    LD_B_C = 0x41,
    LD_B_D = 0x42,
    LD_B_E = 0x43,
    LD_B_H = 0x44,
    LD_B_L = 0x45,
    LD_B_A = 0x47,
    LD_C_B = 0x48,
    LD_C_C = 0x49,
    LD_C_D = 0x4A,
    LD_C_E = 0x4B,
    LD_C_H = 0x4C,
    LD_C_L = 0x4D,
    LD_C_A = 0x4F,
    LD_D_B = 0x50,
    LD_D_C = 0x51,
    LD_D_D = 0x52,
    LD_D_E = 0x53,
    LD_D_H = 0x54,
    LD_D_L = 0x55,
    LD_D_A = 0x57,
    LD_E_B = 0x58,
    LD_E_C = 0x59,
    LD_E_D = 0x5A,
    LD_E_E = 0x5B,
    LD_E_H = 0x5C,
    LD_E_L = 0x5D,
    LD_E_A = 0x5F,
    LD_H_B = 0x60,
    LD_H_C = 0x61,
    LD_H_D = 0x62,
    LD_H_E = 0x63,
    LD_H_H = 0x64,
    LD_H_L = 0x65,
    LD_H_A = 0x67,
    LD_L_B = 0x68,
    LD_L_C = 0x69,
    LD_L_D = 0x6A,
    LD_L_E = 0x6B,
    LD_L_H = 0x6C,
    LD_L_L = 0x6D,
    LD_L_M_HL_M = 0x6E,
    LD_L_A = 0x6F,
    LD_A_B = 0x78,
    LD_A_C = 0x79,
    LD_A_D = 0x7A,
    LD_A_E = 0x7B,
    LD_A_H = 0x7C,
    LD_A_L = 0x7D,
    LD_A_M_HL_M = 0x7E,
    LD_A_A = 0x7F,
};

// 8 bits
typedef unsigned char z80_byte;
// 16 bits
typedef unsigned short z80_word;

typedef union
{
#if __BYTE_ORDER == __BIG_ENDIAN
    // big endian
    struct
    {
        z80_byte h, l;
    } s;
    struct
    {
        z80_byte f, a;
    } s2;
    struct
    {
        z80_byte r, i;
    } s3;
#else
    // litle endian
    struct
    {
        z80_byte l, h;
    } s;
    struct
    {
        z80_byte a, f;
    } s2;
    struct
    {
        z80_byte i, r;
    } s3;
#endif
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

inline void z80_refreshr()
{
    int r = z80_r;

    // Seven bits of this 8-bit register are automatically incremented after each instruction fetch.
    z80_r = (z80_byte)((r + 1) & 0x7F | (r & 0x80));
}

void z80_execopcode(z80_byte data);

inline void z80_instrfetch()
{
    auto data = z80_fetchopcode(z80_regs.pc++);

    z80_tstates += 4;
    /*
    Memory Refresh (R) Register. The Z80 CPU contains a memory refresh counter,
enabling dynamic memories to be used with the same ease as static memories. Seven bits
of this 8-bit register are automatically incremented after each instruction fetch. The eighth
bit remains as programmed, resulting from an LD R, A instruction. The data in the refresh
counter is sent out on the lower portion of the address bus along with a refresh control sig-
nal while the CPU is decoding and executing the fetched instruction. This mode of refresh
is transparent to the programmer and does not slow the CPU operation. The programmer
can load the R register for testing purposes, but this register is normally not used by the
programmer. During refresh, the contents of the I Register are placed on the upper eight
bits of the address bus.
    */
    z80_refreshr();

    z80_execopcode(data);
}

#endif