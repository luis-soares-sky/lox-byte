public enum OpCode : byte
{
    CONSTANT,
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

    public byte AddConstant(double value)
    {
        constants.Add(new(value));
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

    private int DisassembleInstruction(int offset)
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
