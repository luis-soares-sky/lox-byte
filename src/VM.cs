enum InterpretResult
{
    OK,
    COMPILE_ERROR,
    RUNTIME_ERROR,
}

class VM
{
    Chunk chunk;

    List<Value> stack = new List<Value>();
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
        stack.Add(value);
    }

    public Value Pop()
    {
        var value = stack[stack.Count - 1];
        stack.RemoveAt(stack.Count - 1);
        return value;
    }

    public Value Peek(int distance)
    {
        return stack[stack.Count - 1 - distance];
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

            InterpretResult? opResult = null;

            byte instruction = ReadByte();
            switch ((OpCode)instruction)
            {
                case OpCode.CONSTANT:
                    Value constant = ReadConstant();
                    Push(constant);
                    break;
                case OpCode.NIL:
                    Push(Value.Nil());
                    break;
                case OpCode.TRUE:
                    Push(Value.Boolean(true));
                    break;
                case OpCode.FALSE:
                    Push(Value.Boolean(false));
                    break;
                case OpCode.EQUAL:
                    Push(Value.Boolean(Pop() == Pop()));
                    break;
                case OpCode.GREATER:
                    opResult = BinaryOp((a, b) => Value.Boolean(a > b));
                    break;
                case OpCode.LESS:
                    opResult = BinaryOp((a, b) => Value.Boolean(a < b));
                    break;
                case OpCode.ADD:
                    opResult = BinaryOp((a, b) => Value.Number(a + b));
                    break;
                case OpCode.SUBTRACT:
                    opResult = BinaryOp((a, b) => Value.Number(a - b));
                    break;
                case OpCode.MULTIPLY:
                    opResult = BinaryOp((a, b) => Value.Number(a * b));
                    break;
                case OpCode.DIVIDE:
                    opResult = BinaryOp((a, b) => Value.Number(a / b));
                    break;
                case OpCode.NOT:
                    Push(Value.Boolean(Pop().IsFalsy));
                    break;
                case OpCode.NEGATE:
                    if (!Peek(0).IsNumber)
                    {
                        RuntimeError("Operand must be a number.");
                        return InterpretResult.RUNTIME_ERROR;
                    }
                    Push(Value.Number(-Pop().AsNumber));
                    break;
                case OpCode.RETURN:
                    Console.WriteLine(Pop());
                    return InterpretResult.OK;
            }

            if (opResult != null)
            {
                return (InterpretResult)opResult;
            }
        }
    }

    private InterpretResult? BinaryOp(Func<double, double, Value> callback)
    {
        if (!Peek(0).IsNumber || !Peek(1).IsNumber)
        {
            RuntimeError("Operands must be numbers.");
            return InterpretResult.RUNTIME_ERROR;
        }
        double b = Pop().AsNumber;
        double a = Pop().AsNumber;
        Push(callback(a, b));
        return null;
    }

    private byte ReadByte()
    {
        return chunk.code[ip++];
    }

    private Value ReadConstant()
    {
        return chunk.constants[ReadByte()];
    }

    private void RuntimeError(string message)
    {
        Console.WriteLine(message);
        Console.WriteLine($"[line {chunk.lines[ip]}] in script");
        stack.Clear();
    }
}
