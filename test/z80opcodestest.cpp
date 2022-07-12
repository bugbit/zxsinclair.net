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

static FILE *ftest;
// static std::ifstream testsexpe;
static Tz80_memory_default memory, memory_inittest;
static Tz80 z80(memory);
static std::string testname;
// struct TestExpected testexpeop;
static std::map<std::string, struct TestExpected *> testsexpes;

static void readTestsExpecteds();
static bool runTest();
static bool readTest();
static void printTestResult();
// static bool readTestExpected();

void z80opcodestest()
{
	readTestsExpecteds();
	ftest = fopen("tests.in", "r");
	// testsexpe.open("tests.expected");

	// std::string line;
	// int i = 0;

	// while (std::getline(testsexpe, line) && i < 100)
	// {
	// 	std::cout << line << std::endl;

	// 	i++;
	// }

	// std::cout << std::hex << std::uppercase << std::setw(2) << std::setfill('0') << 12 << ' ' << 130 << std::endl;
	// return;

	while (runTest())
	{
		printTestResult();
		// bool texpe = readTestExpected();

		// std::cout << testname << ' ';
		// if (!texpe)
		// {
		// 	std::cout << "error in read tests.expected, end test" << std::endl;

		// 	return;
		// }
		// if (testname != testexpeop.testname)
		// {
		// 	std::cout << "tests.expected diferent name (" << testexpeop.testname << ") expected testerror in read tests.expected, end test" << std::endl;

		// 	return;
		// }
	}
	fclose(ftest);
	// testsexpe.close();
}

static bool readTestExpected(FILE *f)
{
	char test_name[80];

	do
	{
		if (!fgets(test_name, sizeof(test_name), f))
		{
			if (feof(f))
				return false;

			fprintf(stderr, "error reading test description : %s\n",
					strerror(errno));
			return false;
		}

	} while (test_name[0] == '\n');

	char line[81];
	unsigned address, byte;

	for (;;)
	{
		if (!fgets(line, sizeof(line), f))
		{
			if (feof(f))
				return false;

			fprintf(stderr, "error reading test description : %s\n",
					strerror(errno));
			return false;
		}

		unsigned time;
		char type[10];
		int ret = sscanf(line, "%d %s %04x %02x", &time, type, &address, &byte);

		if (!strcmp(type, "MR") || !strcmp(type, "MW") || !strcmp(type, "MC") || !strcmp(type, "PR") || !strcmp(type, "PW") || !strcmp(type, "PC"))
		{
		}
		else
			break;
	}

	unsigned af, bc, de, hl, af_, bc_, de_, hl_, ix, iy, sp, pc;
	unsigned i, r, iff1, iff2, im;
	unsigned halted;
	unsigned end_tstates2;

	/* FIXME: how should we read/write our data types? */
	if (sscanf(line, "%x %x %x %x %x %x %x %x %x %x %x %x", &af, &bc,
			   &de, &hl, &af_, &bc_, &de_, &hl_, &ix, &iy, &sp, &pc) != 12)
	{
		fprintf(stderr, "first registers line corrupt\n");
		return false;
	}

	printf("%s\n", line);

	if (fscanf(f, "%x %x %u %u %u %d %d", &i, &r, &iff1, &iff2, &im,
			   &halted, &end_tstates2) != 7)
	{
		fprintf(stderr, "registers line corrupt\n");
		return false;
	}

	auto *test = new struct TestExpected();
	auto *memoryexp = test->memory;

	testsexpes[*new std::string(test_name)] = test;

	test->testname = test_name;
	test->af = af;
	test->bc = bc;
	test->de = de;
	test->hl = hl;
	test->af_ = af_;
	test->bc_ = bc_;
	test->de_ = de_;
	test->hl_ = hl_;
	test->i = i;
	test->r = r;
	test->iff1 = iff1;
	test->iff2 = iff2;
	test->im = im;
	test->halted = halted;
	test->endtstates = end_tstates2;

	while (1)
	{
		fgets(line, sizeof(line), f);

		auto len = strlen(line);

		if (len == 0 || *line == '\n')
			break;

		fseek(f, -len, SEEK_CUR);

		if (fscanf(f, "%x", &address) != 1)
			break;
		;

		if (address >= 0x10000)
			break;

		memoryexp->address = address;

		auto *data = memoryexp->data;

		while (1)
		{
			if (fscanf(f, "%x", &byte) != 1)
			{
				fprintf(stderr, "no data byte found in \n");
				return false;
			}

			if (byte >= 0x100)
				break;

			*data++ = byte;
		}
	}

	memoryexp->address = 0;
	*(memoryexp->data) = 0;

	return true;
}

static void readTestsExpecteds()
{
	FILE *f = fopen("tests.expected", "r");

	while (readTestExpected(f))
		;

	fclose(f);
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
	unsigned af, bc, de, hl, af_, bc_, de_, hl_, ix, iy, sp, pc;
	unsigned i, r, iff1, iff2, im;
	unsigned halted;
	unsigned end_tstates2;
	unsigned address;
	char test_name[80];

	do
	{

		if (!fgets(test_name, sizeof(test_name), ftest))
		{

			if (feof(ftest))
				return false;

			fprintf(stderr, "error reading test description : %s\n",
					strerror(errno));
			return false;
		}

	} while (test_name[0] == '\n');

	testname = test_name;

	/* FIXME: how should we read/write our data types? */
	if (fscanf(ftest, "%x %x %x %x %x %x %x %x %x %x %x %x", &af, &bc,
			   &de, &hl, &af_, &bc_, &de_, &hl_, &ix, &iy, &sp, &pc) != 12)
	{
		fprintf(stderr, "first registers line corrupt\n");
		return false;
	}

	auto &regs = z80.getRegs();

	regs.main.af.w = af;
	regs.main.bc.w = bc;
	regs.main.de.w = de;
	regs.main.hl.w = hl;
	regs.alternative.af.w = af_;
	regs.alternative.bc.w = bc_;
	regs.alternative.de.w = de_;
	regs.alternative.hl.w = hl_;
	regs.ix = ix;
	regs.iy = iy;
	regs.sp = sp;
	regs.pc = pc;

	if (fscanf(ftest, "%x %x %u %u %u %d %d", &i, &r, &iff1, &iff2, &im,
			   &halted, &end_tstates2) != 7)
	{
		fprintf(stderr, "registers line corrupt\n");
		return false;
	}

	regs.ir.s3.i = i;
	regs.ir.s3.r = r;
	/* IFF1 = iff1;
	IFF2 = iff2;
	IM = im;
	*end_tstates = end_tstates2; */

	while (1)
	{

		if (fscanf(ftest, "%x", &address) != 1)
		{
			fprintf(stderr, "no address found \n");

			return false;
		}

		if (address >= 0x10000)
			break;

		while (1)
		{

			unsigned byte;

			if (fscanf(ftest, "%x", &byte) != 1)
			{
				fprintf(stderr, "no data byte found in \n");
				return false;
			}

			if (byte >= 0x100)
				break;

			memory.poke_notime(address++, byte);
		}
	}

	return true;
}

static void printTestResult()
{
	std::cout << testname << std::endl;

	if (z80.getInstrNotImp())
		printf("not implement\n");
	else
	{
		auto regs = z80.getRegs();
		// temporal
		unsigned iff1 = 0, iff2 = 0, im = 0;
		int halted = 0;

		printf("%04x %04x %04x %04x %04x %04x %04x %04x %04x %04x %04x %04x\n",
			   regs.main.af.w, regs.main.bc.w, regs.main.de.w, regs.main.hl.w, regs.alternative.af.w, regs.alternative.bc.w, regs.alternative.de.w, regs.alternative.hl.w, regs.ix, regs.iy, regs.sp, regs.pc);
		printf("%02x %02x %d %d %d %d %d\n", regs.ir.s3.i, regs.ir.s3.r, iff1, iff2, im, halted, z80.getTStates().getTStates());
	}

	std::cout << std::endl;
}

/*
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

		std::string line;

		std::getline(testsexpe, line);

		break;

		auto car = testsexpe.peek();

		if (car == '\r' || car == '\n')
		{
			memoryexp->address = 0;
			*(memoryexp->data) = 0;

			break;
		}

		unsigned address;

		testsexpe >> str;

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

			testsexpe >> byte;

			if (byte >= 0x100 || testsin.eof())
				break;

			*data++ = byte;
		}

		memoryexp++;
	}

	return true;
}
*/
/*

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

*/