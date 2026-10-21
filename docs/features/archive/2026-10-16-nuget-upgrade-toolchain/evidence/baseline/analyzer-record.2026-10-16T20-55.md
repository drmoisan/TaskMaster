Timestamp: 2026-10-16T20-55
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /verbosity:minimal
EXIT_CODE: 1
Output Summary: CS0006: stale Meziantou.Analyzer.3.0.296 paths. Outlook also locked existing native binaries. Compiler/tests/coverage were unavailable before correcting compilation.
Raw Log: analyzers.2026-10-16T20-47.log
