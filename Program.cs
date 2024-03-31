#define DEBUG_TRACE_EXECUTION

VM vm = new VM();

if (args.Length == 0)
{
    RunREPL();
}
else if (args.Length == 1)
{
    RunFile(args[1]);
}
else
{
    Console.WriteLine("Usage: cslox [path]");
    Environment.Exit(64);
}

void RunREPL()
{
    while (true)
    {
        Console.Write("> ");
        string? line = Console.ReadLine();
        if (line != null)
        {
            vm.interpret(line);
        }
    }
}

void RunFile(string filePath)
{
    string source = "";
    try
    {
        source = File.ReadAllText(filePath);
    }
    catch
    {
        Console.WriteLine($"Could not open file \"{filePath}\".");
        Environment.Exit(74);
    }

    switch (vm.interpret(source))
    {
        case InterpretResult.COMPILE_ERROR: Environment.Exit(65); break;
        case InterpretResult.RUNTIME_ERROR: Environment.Exit(70); break;
    }
}
