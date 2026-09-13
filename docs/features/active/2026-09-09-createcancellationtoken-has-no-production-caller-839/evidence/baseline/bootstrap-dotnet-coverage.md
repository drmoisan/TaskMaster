# Bootstrap: dotnet-coverage global tool (issue #839)

Timestamp: 2026-09-13T02-38
Command: pwsh -NoProfile -Command 'if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; "INSTALL_STEP_DONE=True"'
Command: pwsh -NoProfile -Command '"DOTNET_COVERAGE_RESOLVED=$([bool](Get-Command dotnet-coverage -ErrorAction SilentlyContinue))"; dotnet-coverage --version'
EXIT_CODE: 0

Output Summary:
- The install step printed INSTALL_STEP_DONE=True. The guard found dotnet-coverage already on PATH from a previous global install, so no download occurred.
- The separate probe invocation printed DOTNET_COVERAGE_RESOLVED=True.
- dotnet-coverage --version printed 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342.
- The recorded EXIT_CODE is that of the probe invocation.
