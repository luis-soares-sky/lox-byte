enum InterpretResult
{
    OK,
    COMPILE_ERROR,
    RUNTIME_ERROR,
}

class VM
{
    Chunk chunk;

    Stack<Value> stack = new Stack<Value>();
    int ip = 0;

    public InterpretResult interpret(string source)
    {
        Chunk chunk = new Chunk();
        Compiler compiler = new Compiler(new Scanner(source), ref chunk);
        if (!compiler.Compile())
        {
            return InterpretResult.COMPILE_ERROR;
        }

        this.chunk = chunk;
        this.ip = 0;
        return Run();
    }

    public void Push(Value value)
    {
        stack.Push(value);
    }

    public Value Pop()
    {
        return stack.Pop();
    }

    private InterpretResult Run()
    {
        while (true)
        {
#if DEBUG_TRACE_EXECUTION
            Console.Write("          ");
            foreach (Value value in stack)
            {
                Console.Write($"[ {value} ]");
            }
            Console.WriteLine("");
            chunk.DisassembleInstruction(ip);
#endif

            byte instruction = ReadByte();
            switch ((OpCode)instruction)
            {
                case OpCode.CONSTANT:
                    Value constant = ReadConstant();
                    Push(constant);
                    break;
                case OpCode.ADD:
                    BinaryOp((a, b) => a + b);
                    break;
                case OpCode.SUBTRACT:
                    BinaryOp((a, b) => a - b);
                    break;
                case OpCode.MULTIPLY:
                    BinaryOp((a, b) => a * b);
                    break;
                case OpCode.DIVIDE:
                    BinaryOp((a, b) => a / b);
                    break;
                case OpCode.NEGATE:
                    Value value = Pop();
                    value.value = -value.value;
                    Push(value);
                    break;
                case OpCode.RETURN:
                    Console.WriteLine(Pop());
                    return InterpretResult.OK;
            }
        }
    }

    private void BinaryOp(Func<double, double, double> callback)
    {
        double b = Pop().value;
        double a = Pop().value;
        Push(new(callback(a, b)));
    }

    private byte ReadByte()
    {
        return chunk.code[ip++];
    }

    private Value ReadConstant()
    {
        return chunk.constants[ReadByte()];
    }
}
