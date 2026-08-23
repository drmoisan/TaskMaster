Timestamp: 2026-08-22T13-13
Command: & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fileLogger "/fileLoggerParameters:LogFile=coverage\msbuild\phase3-nullable.log;Verbosity=normal"
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Error(s), 5 Warning(s) (pre-existing System.Reactive advisories). `CoreCompile:` count: 65 (>= 1). `Skipping target "CoreCompile"` count: 0. The `Command:` line above contains no `Nullable=enable` property.
