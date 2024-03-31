#define DEBUG_TRACE_EXECUTION

Chunk chunk = new();

byte constant = chunk.AddConstant(1.2);
chunk.Write((byte)OpCode.CONSTANT, 123);
chunk.Write(constant, 123);

constant = chunk.AddConstant(3.4);
chunk.Write((byte)OpCode.CONSTANT, 123);
chunk.Write(constant, 123);

chunk.Write((byte)OpCode.ADD, 123);

constant = chunk.AddConstant(5.6);
chunk.Write((byte)OpCode.CONSTANT, 123);
chunk.Write(constant, 123);

chunk.Write((byte)OpCode.DIVIDE, 123);

chunk.Write((byte)OpCode.NEGATE, 123);
chunk.Write((byte)OpCode.RETURN, 123);

chunk.Disassemble("test chunk");

Console.WriteLine("");

VM vm = new VM();

vm.interpret(chunk);
