Timestamp: 2026-09-01T00-21
Command: pwsh -NoProfile -Command 'if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; dotnet-coverage --version'
EXIT_CODE: 0
Output Summary: dotnet-coverage was already installed as a global tool. `dotnet-coverage --version` prints 18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3.
