public enum OpCode : byte
{
    CONSTANT,
    NIL,
    TRUE,
    FALSE,
    EQUAL,
    GREATER,
    LESS,
    ADD,
    SUBTRACT,
    MULTIPLY,
    DIVIDE,
    NOT,
    NEGATE,
    RETURN
}

struct Chunk
{
    public List<byte> code = new List<byte>();
    public List<int> lines = new List<int>();
    public List<Value> constants = new List<Value>(); // @todo: should be List<Value>()

    public Chunk() { }

    public void Write(byte value, int line)
    {
        code.Add(value);
        lines.Add(line);
    }

    public byte AddConstant(Value value)
    {
        constants.Add(value);
        return (byte)(constants.Count - 1);
    }

    public void Disassemble(string name)
    {
        Console.WriteLine($"== {name} ==");

        for (int offset = 0; offset < code.Count;)
        {
            offset = DisassembleInstruction(offset);
        }
    }

    public int DisassembleInstruction(int offset)
    {
        Console.Write($"{offset:D4} ");

        if (offset > 0 && lines[offset] == lines[offset - 1])
        {
            Console.Write("   | ");
        }
        else
        {
            Console.Write($"{lines[offset],4} ");
        }

        byte instruction = code[offset];
        switch ((OpCode)instruction)
        {
            case OpCode.CONSTANT: return ConstantInstruction("OP_CONSTANT", offset);
            case OpCode.NIL: return SimpleInstruction("OP_NIL", offset);
            case OpCode.TRUE: return SimpleInstruction("OP_TRUE", offset);
            case OpCode.FALSE: return SimpleInstruction("OP_FALSE", offset);
            case OpCode.EQUAL: return SimpleInstruction("OP_EQUAL", offset);
            case OpCode.GREATER: return SimpleInstruction("OP_GREATER", offset);
            case OpCode.LESS: return SimpleInstruction("OP_LESS", offset);
            case OpCode.ADD: return SimpleInstruction("OP_ADD", offset);
            case OpCode.SUBTRACT: return SimpleInstruction("OP_SUBTRACT", offset);
            case OpCode.MULTIPLY: return SimpleInstruction("OP_MULTIPLY", offset);
            case OpCode.DIVIDE: return SimpleInstruction("OP_DIVIDE", offset);
            case OpCode.NOT: return SimpleInstruction("OP_NOT", offset);
            case OpCode.NEGATE: return SimpleInstruction("OP_NEGATE", offset);
            case OpCode.RETURN: return SimpleInstruction("OP_RETURN", offset);
        }

        Console.WriteLine($"Unknown opcode {instruction}");
        return offset + 1;
    }

    private int SimpleInstruction(string name, int offset)
    {
        Console.WriteLine($"{name}");
        return offset + 1;
    }

    private int ConstantInstruction(string name, int offset)
    {
        byte constantIndex = code[offset + 1];
        Console.Write($"{name,-16} {constantIndex,4} '");
        Console.Write(constants[constantIndex]);
        Console.WriteLine("'");
        return offset + 2;
    }
}
