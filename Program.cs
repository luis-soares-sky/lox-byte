Chunk chunk = new();

byte constant = chunk.AddConstant(1.2);
chunk.Write((byte)OpCode.CONSTANT, 123);
chunk.Write(constant, 123);

chunk.Write((byte)OpCode.RETURN, 123);

chunk.Disassemble("test chunk");
