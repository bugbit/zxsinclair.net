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
#define ld_r_n(r)              \
    n = z80_readmempcandinc(); \
    r = n
#define ld_r_m_rr_m(r, rr) \
    n = z80_readmem(rr);   \
    r = n

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
    LD_L_A = 0x6F,
    LD_A_B = 0x78,
    LD_A_C = 0x79,
    LD_A_D = 0x7A,
    LD_A_E = 0x7B,
    LD_A_H = 0x7C,
    LD_A_L = 0x7D,
    LD_A_A = 0x7F,

    LD_A_N = 0x3E,
    LD_B_N = 0x06,
    LD_C_N = 0x0E,
    LD_D_N = 0x16,
    LD_E_N = 0x1E,
    LD_H_N = 0x26,
    LD_L_N = 0x2E,

    LD_A_M_HL_M = 0x7E,
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

extern z80_registers z80_regs;
extern int z80_tstates;

class Tz80_memory
{
public:
    inline Tz80_memory(z80_word size = 0xFFFF) { this->size = size; }
    inline virtual ~Tz80_memory() {}
    inline virtual z80_byte read(z80_word m) const
    {
        z80_tstates += 3;

        return 0;
    }
    inline virtual z80_byte read_notime(z80_word m) const { return 0; }
    inline virtual void poke(z80_word m, z80_byte data) { z80_tstates += 3; }
    inline virtual void poke_notime(z80_word m, z80_byte data) {}
    inline virtual z80_byte fetchopcode(z80_word m) const
    {
        z80_tstates += 4;

        return 0;
    }

protected:
    z80_word size;
};

class Tz80_memory_default : public Tz80_memory
{
public:
    inline Tz80_memory_default(int size = 0xFFFF) : Tz80_memory(size)
    {
        memory = new z80_byte[size];
    }
    inline virtual ~Tz80_memory_default()
    {
        delete[] memory;
    }
    inline virtual z80_byte read(z80_word m) const
    {
        z80_tstates += 3;

        return memory[m];
    }
    inline virtual z80_byte read_notime(z80_word m) const { return memory[m]; }
    inline virtual void poke(z80_word m, z80_byte data)
    {
        z80_tstates += 3;

        memory[m] = data;
    }
    inline virtual void poke_notime(z80_word m, z80_byte data) { memory[m] = data; }
    inline virtual z80_byte fetchopcode(z80_word m) const
    {
        z80_tstates += 4;

        return memory[m];
    }

protected:
    z80_byte *memory;
};

extern Tz80_memory *z80_memory;

inline void z80_freememory()
{
    if (z80_memory != NULL)
    {
        delete z80_memory;
        z80_memory = NULL;
    }
}

inline void z80_setmemory(Tz80_memory *m)
{
    z80_freememory();
    z80_memory = m;
}

inline int z80_creatememory_default()
{
    auto m = new Tz80_memory_default();

    if (m == NULL)
    {
        perror("not enough memory");

        return -1;
    }
    z80_setmemory(m);

    return 0;
}

inline void z80_reset()
{
    memset(&z80_regs, 0, sizeof(z80_regs));
    z80_tstates = 0;
}

inline z80_byte z80_readmem(z80_word m)
{
    assert(z80_memory);

    return z80_memory->read(m);
}

inline z80_byte z80_readmem_notime(z80_word m)
{
    assert(z80_memory);

    return z80_memory->read_notime(m);
}

inline z80_byte z80_readmempcandinc()
{
    return z80_readmem(z80_pc++);
}

inline void z80_pokemem(z80_word m, z80_byte data)
{
    assert(z80_memory);

    z80_memory->poke(m, data);
}

inline void z80_pokemem_notime(z80_word m, z80_byte data)
{
    assert(z80_memory);

    z80_memory->poke_notime(m, data);
}

inline z80_byte z80_fetchopcode(z80_word m)
{
    assert(z80_memory);

    return z80_memory->fetchopcode(m);
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