# lockb0x-protocol

A secure protocol implementation with a command-line interface.

## Lockb0x CLI

The repository includes a C# command-line interface (CLI) application for interacting with the Lockb0x Protocol.

### Building the CLI

```bash
cd Lockb0xCli
dotnet build
```

### Running the CLI

```bash
cd Lockb0xCli
dotnet run
```

Or after building, you can run the executable directly:

```bash
cd Lockb0xCli/bin/Debug/net8.0
./lockb0x
```

### Available Commands

- `--help`, `-h`: Show help information
- `--version`, `-v`: Show version information  
- `status`: Show the current status of the Lockb0x Protocol
- `info`: Display information about the Lockb0x Protocol

### Examples

```bash
# Show help
dotnet run -- --help

# Show version
dotnet run -- --version

# Check protocol status
dotnet run -- status

# Get protocol information
dotnet run -- info
```

## Requirements

- .NET 8.0 SDK or later