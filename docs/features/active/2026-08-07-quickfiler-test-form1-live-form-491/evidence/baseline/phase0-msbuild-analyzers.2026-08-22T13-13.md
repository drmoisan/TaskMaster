Timestamp: 2026-08-22T13-13
Command: & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fileLogger "/fileLoggerParameters:LogFile=coverage\msbuild\phase0-analyzers.log;Verbosity=normal"
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Error(s), 5 Warning(s) (all pre-existing `System.Reactive` packages.config migration advisories, unrelated to this change). `CoreCompile:` count in log: 53 (>= 1, real compilation occurred). `Skipping target "CoreCompile"` count in log: 0 (no up-to-date short-circuit). Elapsed time 00:00:26.69.
