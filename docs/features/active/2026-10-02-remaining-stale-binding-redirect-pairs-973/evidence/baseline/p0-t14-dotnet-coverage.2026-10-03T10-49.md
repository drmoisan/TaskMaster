# P0-T14 dotnet-coverage global tool (issue #973)

Timestamp: 2026-10-03T10-49
Command: pwsh -NoProfile -Command 'if (Get-Command dotnet-coverage -ErrorAction SilentlyContinue) { "DOTNET-COVERAGE: present" } else { dotnet tool install --global dotnet-coverage | Out-Null; "DOTNET-COVERAGE: installed exit " + $LASTEXITCODE }; (dotnet-coverage --version)'
EXIT_CODE: 0
Output Summary: dotnet-coverage already present; version 18.10.0.

DOTNET-COVERAGE: present
18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
