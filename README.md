# Password Brute Force Cracker

C# Final Task - Multi-threaded password brute force application

## Version History

| Commit | Description |
|--------|-------------|
| v1.0 | Added Models: PasswordHasher, PasswordGenerator, BruteForceResult |
| v1.1 | Added Services: CombinationGenerator, PasswordValidator, BruteForceEngine, PerformanceLogger |
| v1.2 | Added UI: MainForm WinForms interface |
| v1.3 | Added Program entry point and project file |

## How to Run
1. Install .NET 6 SDK
2. Open terminal in project folder
3. Type: dotnet run

## Features
- SHA256 hashing with static salt
- Random password length 4 or 5 characters
- Brute force search length 1 to 6
- Single-thread and multi-thread modes
- Max CPU cores minus 1 threads
- Stops all threads when password found
- Performance comparison log
