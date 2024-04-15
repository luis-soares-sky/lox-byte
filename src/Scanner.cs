using System.ComponentModel;
using System.Text.RegularExpressions;

enum TokenType
{
    NONE,

    // Single-character tokens.
    LEFT_PAREN, RIGHT_PAREN,
    LEFT_BRACE, RIGHT_BRACE,
    COMMA, DOT, MINUS, PLUS,
    SEMICOLON, SLASH, STAR,

    // One or two character tokens.
    BANG, BANG_EQUAL,
    EQUAL, EQUAL_EQUAL,
    GREATER, GREATER_EQUAL,
    LESS, LESS_EQUAL,

    // Literals.
    IDENTIFIER, STRING, NUMBER,

    // Keywords.
    AND, CLASS, ELSE, FALSE,
    FOR, FUN, IF, NIL, OR,
    PRINT, RETURN, SUPER, THIS,
    TRUE, VAR, WHILE,

    // Special.
    ERROR, EOF,
}

struct Token
{
    public TokenType type;
    public string lexeme;
    public int start;
    public int length;
    public int line;
}

class Scanner(string source)
{
    string source = source;
    int start = 0;
    int current = 0;
    int line = 1;

    public Token ScanToken()
    {
        SkipWhitespace();

        start = current;

        if (IsAtEnd) return MakeToken(TokenType.EOF);

        char c = Advance();

        if (IsAlpha(c)) return MakeIdentifier();
        if (IsDigit(c)) return MakeNumber();

        switch (c)
        {
            case '(': return MakeToken(TokenType.LEFT_PAREN);
            case ')': return MakeToken(TokenType.RIGHT_PAREN);
            case '{': return MakeToken(TokenType.LEFT_BRACE);
            case '}': return MakeToken(TokenType.RIGHT_BRACE);
            case ';': return MakeToken(TokenType.SEMICOLON);
            case ',': return MakeToken(TokenType.COMMA);
            case '.': return MakeToken(TokenType.DOT);
            case '-': return MakeToken(TokenType.MINUS);
            case '+': return MakeToken(TokenType.PLUS);
            case '/': return MakeToken(TokenType.SLASH);
            case '*': return MakeToken(TokenType.STAR);
            case '!': return MakeToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
            case '=': return MakeToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
            case '<': return MakeToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
            case '>': return MakeToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
            case '"': return MakeString();
        }

        return ErrorToken("Unexpected character.");
    }

    private bool IsAtEnd => current >= source.Length;

    private char Advance()
    {
        return source[current++];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd) return false;
        if (source[current] != expected) return false;
        current++;
        return true;
    }

    private char Peek()
    {
        if (current >= source.Length) return '\0';
        return source[current];
    }

    private char PeekNext()
    {
        if (IsAtEnd) return '\0';
        return source[current + 1];
    }

    private bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
                c == '_';
    }

    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private Token MakeIdentifier()
    {
        while (IsAlpha(Peek()) || IsDigit(Peek())) Advance();
        return MakeToken(GetIdentifierType());
    }

    private TokenType GetIdentifierType()
    {
        switch (source[start])
        {
            case 'a': return CheckKeyword(1, "nd", TokenType.AND);
            case 'c': return CheckKeyword(1, "lass", TokenType.CLASS);
            case 'e': return CheckKeyword(1, "lse", TokenType.ELSE);
            case 'f':
                if (start + 1 < source.Length)
                    switch (source[start + 1])
                    {
                        case 'a': return CheckKeyword(2, "lse", TokenType.FALSE);
                        case 'o': return CheckKeyword(2, "r", TokenType.FOR);
                        case 'u': return CheckKeyword(2, "n", TokenType.FUN);
                    }
                break;
            case 'i': return CheckKeyword(1, "f", TokenType.IF);
            case 'n': return CheckKeyword(1, "il", TokenType.NIL);
            case 'o': return CheckKeyword(1, "r", TokenType.OR);
            case 'p': return CheckKeyword(1, "rint", TokenType.PRINT);
            case 'r': return CheckKeyword(1, "eturn", TokenType.RETURN);
            case 's': return CheckKeyword(1, "uper", TokenType.SUPER);
            case 't':
                if (start + 1 < source.Length)
                    switch (source[start + 1])
                    {
                        case 'h': return CheckKeyword(2, "is", TokenType.THIS);
                        case 'r': return CheckKeyword(2, "ue", TokenType.TRUE);
                    }
                break;
            case 'v': return CheckKeyword(1, "ar", TokenType.VAR);
            case 'w': return CheckKeyword(1, "hile", TokenType.WHILE);
        }
        return TokenType.IDENTIFIER;
    }

    private TokenType CheckKeyword(int offset, string partial, TokenType type)
    {
        int s = start + offset;
        int e = s + partial.Length;
        if (e <= source.Length && source[s..e] == partial) return type;
        return TokenType.IDENTIFIER;
    }

    private Token MakeNumber()
    {
        while (IsDigit(Peek())) Advance();

        // Look for a fractional part.
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            // Consume the ".".
            Advance();

            while (IsDigit(Peek())) Advance();
        }

        return MakeToken(TokenType.NUMBER);
    }

    private Token MakeString()
    {
        while (Peek() != '"' && !IsAtEnd)
        {
            if (Peek() == '\n') line++;
            Advance();
        }

        if (IsAtEnd) return ErrorToken("Unterminated string.");

        // The closing quote.
        Advance();
        return MakeToken(TokenType.STRING);
    }

    private Token MakeToken(TokenType type) => new Token()
    {
        type = type,
        lexeme = source[start..current],
        start = start,
        length = current - start,
        line = line,
    };

    private Token ErrorToken(string message) => new Token()
    {
        type = TokenType.ERROR,
        lexeme = message,
        start = start,
        length = 0,
        line = line,
    };

    private void SkipWhitespace()
    {
        while (true)
        {
            char c = Peek();
            switch (c)
            {
                case ' ':
                case '\r':
                case '\t':
                    Advance();
                    break;
                case '\n':
                    line++;
                    Advance();
                    break;
                case '/':
                    if (PeekNext() == '/')
                    {   // A comment goes until the end of the line.
                        while (Peek() != '\n' && !IsAtEnd) Advance();
                    }
                    else
                    {
                        return;
                    }
                    break;
                default:
                    return;
            }
        }
    }
}
