using System.CommandLine;

namespace Lockb0xCli;

public class Program
{
    public static int Main(string[] args)
    {
        var rootCommand = new RootCommand("Lockb0x CLI");
        rootCommand.Add(new Command("init", "Initialize Lockb0x configuration."));
        rootCommand.Add(new Command("create", "Create a Codex Entry from a file."));
        rootCommand.Add(new Command("sign", "Sign a Codex Entry."));
        rootCommand.Add(new Command("anchor", "Anchor a Codex Entry on Stellar."));
        rootCommand.Add(new Command("certify", "Certify a Codex Entry."));
        rootCommand.Add(new Command("verify", "Verify a Codex Entry."));
        rootCommand.Add(new Command("revision", "Show revision history for a Codex Entry."));
        return rootCommand.Parse(args).Invoke();
    }
}
