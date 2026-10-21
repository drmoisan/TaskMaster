Timestamp: 2026-09-03T12-06
Command: if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage } ; dotnet-coverage --version
EXIT_CODE: 0
Output Summary: dotnet-coverage was already available (Get-Command succeeded, install skipped); `dotnet-coverage --version` printed 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342, exit 0.
