Timestamp: 2026-08-22T13-13
Command: & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fileLogger "/fileLoggerParameters:LogFile=coverage\msbuild\phase0-nullable.log;Verbosity=normal"
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Error(s), 5 Warning(s) (pre-existing `System.Reactive` packages.config migration advisories; these are MSBuild task warnings, not C# compiler warnings, so `TreatWarningsAsErrors` does not promote them). `CoreCompile:` count in log: 56 (>= 1). `Skipping target "CoreCompile"` count in log: 0. No `Nullable=enable` property appears in the Command line above; this is the exact command from CLAUDE.md / ci.yml, which omits it deliberately.
