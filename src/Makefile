CFLAGS = -o3 -g -I../common
LIBS = 
BUILD := ../../build/sierpinski

all: executable

# debug: CXXFLAGS += -DDEBUG -g
debug: CFLAGS += -DDEBUG -g -debug-compilation-dir='.'
debug: executable

executable: emcc ${CFLAGS} 