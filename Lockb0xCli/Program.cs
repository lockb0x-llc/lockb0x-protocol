namespace Lockb0xCli;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                ShowWelcome();
                return 0;
            }

            var command = args[0].ToLowerInvariant();

            return command switch
            {
                "--help" or "-h" => ShowHelp(),
                "--version" or "-v" => ShowVersion(),
                "status" => ShowStatus(),
                "info" => ShowInfo(),
                _ => ShowUnknownCommand(command)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    static void ShowWelcome()
    {
        Console.WriteLine("Welcome to Lockb0x Protocol CLI!");
        Console.WriteLine("Use --help to see available commands.");
    }

    static int ShowHelp()
    {
        Console.WriteLine("Lockb0x Protocol CLI - A command-line interface for the Lockb0x Protocol");
        Console.WriteLine();
        Console.WriteLine("Usage: lockb0x [command] [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  status    Show the current status of the Lockb0x Protocol");
        Console.WriteLine("  info      Display information about the Lockb0x Protocol");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --help, -h     Show this help message");
        Console.WriteLine("  --version, -v  Show version information");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  lockb0x status");
        Console.WriteLine("  lockb0x info");
        Console.WriteLine("  lockb0x --version");
        return 0;
    }

    static int ShowVersion()
    {
        Console.WriteLine("Lockb0x CLI v1.0.0");
        Console.WriteLine("Lockb0x Protocol Command Line Interface");
        Console.WriteLine("Runtime: .NET 8.0");
        return 0;
    }

    static int ShowStatus()
    {
        Console.WriteLine("Lockb0x Protocol Status: Ready");
        Console.WriteLine("CLI Version: 1.0.0");
        Console.WriteLine("Runtime: .NET 8.0");
        Console.WriteLine("System: " + Environment.OSVersion.Platform);
        return 0;
    }

    static int ShowInfo()
    {
        Console.WriteLine("=== Lockb0x Protocol Information ===");
        Console.WriteLine("A secure protocol implementation");
        Console.WriteLine("Repository: https://github.com/lockb0x-llc/lockb0x-protocol");
        Console.WriteLine("CLI Tool: Command-line interface for protocol operations");
        Console.WriteLine("Build: Release 1.0.0");
        Console.WriteLine("License: See repository for license information");
        return 0;
    }

    static int ShowUnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        Console.Error.WriteLine("Use --help to see available commands.");
        return 1;
    }
}
