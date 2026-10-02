# P0-T9 type-check baseline
Timestamp: 2026-10-01T06-38
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\logs\p0-t9-nullable.msbuild.log;Verbosity=detailed" (plus /nodeReuse:false)
EXIT_CODE: 0
Output Summary:
  msbuild process exit code: 0
  Log contains "Build succeeded.": True
  Log has line matching ^\s*0 Error\(s\)\s*$ : True
  Warning(s) count: 0
  Token /out:obj\Debug\QuickFiler.Test.dll occurrences: 2 (non-vacuity control, at least 1)
  Lines containing Skipping target CoreCompile: 0
  Logger: msbuild file logger Verbosity=detailed (log is git-ignored under coverage\logs\)
  Note: /nodeReuse:false added to the command (not a plan-named switch; does not alter compilation).
