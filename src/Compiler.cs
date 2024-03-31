class Compiler
{

    public void Compile(string source)
    {
        Scanner scanner = new Scanner(source);

        int line = -1;
        while (true)
        {
            Token token = scanner.ScanToken();
            if (token.line != line)
            {
                Console.Write($"{token.line,4} ", token.line);
                line = token.line;
            }
            else
            {
                Console.Write("   | ");
            }
            Console.WriteLine($"{token.type,13} '{token.lexeme}'");

            if (token.type == TokenType.EOF) break;
        }
    }

}
