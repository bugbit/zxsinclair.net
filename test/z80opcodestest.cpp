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

#define TESTEXPECTEDMEMORYMAX 10
#define TESTEXPECTEDMEMORYDATAMAX 10

struct TEstExpectedMemory
{
	z80_word address;
	z80_byte data[TESTEXPECTEDMEMORYDATAMAX];
};

struct TestExpected
{
	std::string testname;
	unsigned af, bc, de, hl, af_, bc_, de_, hl_, ix, iy, sp, pc;
	unsigned i, r, iff1, iff2, im;
	int halted;
	unsigned endtstates;
	struct TEstExpectedMemory memory[TESTEXPECTEDMEMORYMAX];
};

static std::ifstream testsin;
static std::ifstream testsexpe;
static Tz80_memory_default memory, memory_inittest;
static Tz80 z80(memory);
static std::string testname;
struct TestExpected testexpeop;

static bool runTest();
static bool readTest();
static bool readTestExpected();

void z80opcodestest()
{
	testsin.open("tests.in");
	testsexpe.open("tests.expected");
	// std::cout << std::hex << std::uppercase << std::setw(2) << std::setfill('0') << 12 << ' ' << 130 << std::endl;
	// return;
	while (runTest())
	{
		bool texpe = readTestExpected();

		std::cout << testname << ' ';
		if (!texpe)
		{
			std::cout << "error in read tests.expected, end test" << std::endl;

			return;
		}
		if (testname != testexpeop.testname)
		{
			std::cout << "tests.expected diferent name (" << testexpeop.testname << ") expected testerror in read tests.expected, end test" << std::endl;

			return;
		}
		if (z80.getInstrNotImp())
			std::cout << "not implement" << std::endl;
		else
			std::cout << "?" << std::endl;
	}
	testsin.close();
	testsexpe.close();
}

static bool runTest()
{
	z80.reset();
	for (int i = 0; i < 0x10000;)
	{
		memory.pokeByte(i++, 0xde);
		memory.pokeByte(i++, 0xad);
		memory.pokeByte(i++, 0xbe);
		memory.pokeByte(i++, 0xef);
	}
	memory.copyTo(memory_inittest);

	if (!readTest())
		return false;

	z80.instrfetch();

	return true;
}

static bool readTest()
{
	std::string line;
	z80_word i, r, iff1, iff2, im;
	int halted;
	unsigned endtstates;

	do
	{
		if (testsin.eof())
			return false;

		testsin >> line;
	} while (line == "\n");
	testname = line;

	auto regs = z80.getRegs();

	testsin >> std::hex >> regs.main.af.w >> regs.main.bc.w >> regs.main.de.w >> regs.main.hl.w >> regs.alternative.af.w >> regs.alternative.bc.w >> regs.alternative.de.w >> regs.alternative.hl.w >> regs.ix >> regs.iy >> regs.sp >> regs.pc;
	testsin >> std::hex >> i >> r;
	testsin >> iff1 >> iff2 >> im >> halted >> endtstates;

	for (;;)
	{
		unsigned address;

		testsin >> address;

		if (address >= 0x10000 || testsin.eof())
			break;

		for (;;)
		{
			unsigned byte;

			testsin >> byte;

			if (byte >= 0x100 || testsin.eof())
				break;

			memory.pokeByte(address++, byte);
		}
	}

	return true;
}

static bool readTestExpected()
{
	std::string line;
	// z80_word number;
	std::string numberstr, str;
	unsigned address;
	unsigned byte;

	do
	{
		if (testsexpe.eof())
			return false;

		testsexpe >> std::hex >> line;
	} while (line == "\n");

	testexpeop.testname = line;
	for (;;)
	{
		if (testsexpe.eof())
			return false;

		testsexpe >> numberstr >> str;

		if (str == "MR" || str == "MW" || str == "MC" || str == "PR" || str == "PW" || str == "PC")
		{
			testsexpe >> std::hex >> address;
			if (str == "MR" || str == "MW" || str == "PR" || str == "PW")
				testsexpe >> std::hex >> byte;
		}
		else
		{
			// testexpeop.af = number;
			std::istrstream(numberstr.c_str()) >> std::hex >> testexpeop.af;
			std::istrstream(str.c_str()) >> std::hex >> testexpeop.bc;
			testsexpe >> std::hex >> testexpeop.de >> testexpeop.hl >> testexpeop.af_ >> testexpeop.bc_ >> testexpeop.de_ >> testexpeop.hl_ >> testexpeop.ix >> testexpeop.iy >> testexpeop.sp >> testexpeop.pc;
			testsexpe >> std::hex >> testexpeop.i >> testexpeop.r >> testexpeop.iff1 >> testexpeop.iff2 >> testexpeop.im >> testexpeop.halted >> testexpeop.endtstates;

			break;
		}
	}

	struct TEstExpectedMemory *memoryexp = testexpeop.memory;

	for (;;)
	{
		if (testsexpe.eof())
		{
			memoryexp->address = 0;
			*(memoryexp->data) = 0;

			break;
		}

		unsigned address;

		testsin >> str;

		if (str == "")
		{
			memoryexp->address = 0;
			*(memoryexp->data) = 0;

			break;
		}

		std::istrstream(str.c_str()) >> std::hex >> address;

		if (address >= 0x10000 || testsin.eof())
		{
			memoryexp->address = 0;
			*(memoryexp->data) = 0;

			break;
		}

		memoryexp->address = address;

		z80_byte *data = memoryexp->data;

		for (;;)
		{
			if (testsexpe.eof())
				break;

			unsigned byte;

			testsin >> byte;

			if (byte >= 0x100 || testsin.eof())
				break;

			*data++ = byte;
		}

		memoryexp++;
	}

	return true;
}

void z80opcodestest2()
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