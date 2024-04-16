class Compiler
{
    enum Precedence
    {
        NONE,
        ASSIGNMENT, // =
        OR,         // or
        AND,        // and
        EQUALITY,   // == !=
        COMPARISON, // < > <= >=
        TERM,       // + -
        FACTOR,     // * /
        UNARY,      // ! -
        CALL,       // . ()
        PRIMARY,
    }

    struct ParseRule(Action? prefix, Action? infix, Precedence precedence)
    {
        public readonly Action? prefix = prefix;
        public readonly Action? infix = infix;
        public readonly Precedence precedence = precedence;
    }

    readonly ParseRule[] rules;

    readonly Scanner scanner;
    readonly Chunk chunk;

    Token previous;
    Token current;
    bool hadError;
    bool panicMode;

    public Compiler(Scanner scanner, ref Chunk chunk)
    {
        this.scanner = scanner;
        this.chunk = chunk;

        rules = new ParseRule[41]; // amount of enums
        rules[(int)TokenType.NONE] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.LEFT_PAREN] = new ParseRule(Grouping, null, Precedence.NONE);
        rules[(int)TokenType.RIGHT_PAREN] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.LEFT_BRACE] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.RIGHT_BRACE] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.COMMA] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.DOT] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.MINUS] = new ParseRule(Unary, Binary, Precedence.TERM);
        rules[(int)TokenType.PLUS] = new ParseRule(null, Binary, Precedence.TERM);
        rules[(int)TokenType.SEMICOLON] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.SLASH] = new ParseRule(null, Binary, Precedence.FACTOR);
        rules[(int)TokenType.STAR] = new ParseRule(null, Binary, Precedence.FACTOR);
        rules[(int)TokenType.BANG] = new ParseRule(Unary, null, Precedence.NONE);
        rules[(int)TokenType.BANG_EQUAL] = new ParseRule(null, Binary, Precedence.EQUALITY);
        rules[(int)TokenType.EQUAL] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.EQUAL_EQUAL] = new ParseRule(null, Binary, Precedence.EQUALITY);
        rules[(int)TokenType.GREATER] = new ParseRule(null, Binary, Precedence.COMPARISON);
        rules[(int)TokenType.GREATER_EQUAL] = new ParseRule(null, Binary, Precedence.COMPARISON);
        rules[(int)TokenType.LESS] = new ParseRule(null, Binary, Precedence.COMPARISON);
        rules[(int)TokenType.LESS_EQUAL] = new ParseRule(null, Binary, Precedence.COMPARISON);
        rules[(int)TokenType.IDENTIFIER] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.STRING] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.NUMBER] = new ParseRule(Number, null, Precedence.NONE);
        rules[(int)TokenType.AND] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.CLASS] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.ELSE] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.FALSE] = new ParseRule(Literal, null, Precedence.NONE);
        rules[(int)TokenType.FOR] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.FUN] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.IF] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.NIL] = new ParseRule(Literal, null, Precedence.NONE);
        rules[(int)TokenType.OR] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.PRINT] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.RETURN] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.SUPER] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.THIS] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.TRUE] = new ParseRule(Literal, null, Precedence.NONE);
        rules[(int)TokenType.VAR] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.WHILE] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.ERROR] = new ParseRule(null, null, Precedence.NONE);
        rules[(int)TokenType.EOF] = new ParseRule(null, null, Precedence.NONE);
    }

    public bool Compile()
    {
        Advance();
        Expression();
        Consume(TokenType.EOF, "Expect end of expression.");
        EndCompiler();

        return !hadError;
    }

    private void Advance()
    {
        previous = current;

        while (true)
        {
            current = scanner.ScanToken();
            if (current.type != TokenType.ERROR) break;

            ErrorAtCurrent(current.lexeme);
        }
    }

    private void Consume(TokenType type, string message)
    {
        if (current.type == type)
        {
            Advance();
            return;
        }

        ErrorAtCurrent(message);
    }

    private void EmitByte(byte b)
    {
        chunk.Write(b, previous.line);
    }

    private void EmitBytes(byte b1, byte b2)
    {
        EmitByte(b1);
        EmitByte(b2);
    }

    private void EmitReturn()
    {
        EmitByte((byte)OpCode.RETURN);
    }

    private byte MakeConstant(Value value)
    {
        int constant = chunk.AddConstant(value);
        if (constant > byte.MaxValue)
        {
            Error("Too many constants in one chunk.");
            return 0;
        }
        return (byte)constant;
    }

    private void EmitConstant(Value value)
    {
        EmitBytes((byte)OpCode.CONSTANT, MakeConstant(value));
    }

    private void EndCompiler()
    {
        EmitReturn();

#if DEBUG_PRINT_CODE
        if (!hadError)
        {
            chunk.Disassemble("code");
        }
#endif
    }

    private void Binary()
    {
        TokenType operatorType = previous.type;
        ParseRule rule = GetRule(operatorType);
        ParsePrecedence(rule.precedence + 1);

        switch (operatorType)
        {
            case TokenType.BANG_EQUAL: EmitBytes((byte)OpCode.EQUAL, (byte)OpCode.NOT); break;
            case TokenType.EQUAL_EQUAL: EmitByte((byte)OpCode.EQUAL); break;
            case TokenType.GREATER: EmitByte((byte)OpCode.GREATER); break;
            case TokenType.GREATER_EQUAL: EmitBytes((byte)OpCode.LESS, (byte)OpCode.NOT); break;
            case TokenType.LESS: EmitByte((byte)OpCode.LESS); break;
            case TokenType.LESS_EQUAL: EmitBytes((byte)OpCode.GREATER, (byte)OpCode.NOT); break;
            case TokenType.PLUS: EmitByte((byte)OpCode.ADD); break;
            case TokenType.MINUS: EmitByte((byte)OpCode.SUBTRACT); break;
            case TokenType.STAR: EmitByte((byte)OpCode.MULTIPLY); break;
            case TokenType.SLASH: EmitByte((byte)OpCode.DIVIDE); break;
            default: return;
        }
    }

    private void Literal()
    {
        switch (previous.type)
        {
            case TokenType.FALSE: EmitByte((byte)OpCode.FALSE); break;
            case TokenType.NIL: EmitByte((byte)OpCode.NIL); break;
            case TokenType.TRUE: EmitByte((byte)OpCode.TRUE); break;
        }
    }

    private void Grouping()
    {
        Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
    }

    private void Number()
    {
        if (double.TryParse(previous.lexeme, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
        {
            EmitConstant(Value.Number(result));
        }
        else
        {
            EmitConstant(Value.Nil());
        }
    }

    private void Unary()
    {
        TokenType operatorType = previous.type;

        // Compile the operand.
        ParsePrecedence(Precedence.UNARY);

        // Emit the operator instruction.
        switch (operatorType)
        {
            case TokenType.BANG: EmitByte((byte)OpCode.NOT); break;
            case TokenType.MINUS: EmitByte((byte)OpCode.NEGATE); break;
            default: return; // Unreachable.
        }
    }

    private void ParsePrecedence(Precedence precedence)
    {
        Advance();

        Action? prefixRule = GetRule(previous.type).prefix;
        if (prefixRule == null)
        {
            Error("Expect expression.");
            return;
        }

        prefixRule();

        while (precedence <= GetRule(current.type).precedence)
        {
            Advance();
            GetRule(previous.type).infix?.Invoke();
        }
    }

    private ParseRule GetRule(TokenType type)
    {
        return rules[(int)type];
    }

    private void Expression()
    {
        ParsePrecedence(Precedence.ASSIGNMENT);
    }

    private void ErrorAtCurrent(string message)
    {
        ErrorAt(current, message);
    }

    private void Error(string message)
    {
        ErrorAt(previous, message);
    }

    private void ErrorAt(Token token, string message)
    {
        if (panicMode) return;
        panicMode = true;

        Console.Write($"[line {token.line}] Error");

        if (token.type == TokenType.EOF)
        {
            Console.Write(" at end");
        }
        else if (token.type == TokenType.ERROR)
        {
            // Nothing.
        }
        else
        {
            Console.Write($" at {token.lexeme}");
        }

        Console.WriteLine($": {message}");
        hadError = true;
    }

}
