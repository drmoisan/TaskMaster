# P0-T9: Baseline Nullable / TreatWarningsAsErrors Rebuild

Timestamp: 2026-09-03T11-38

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /v:normal /fl "/flp:LogFile=coverage\logs\p0-t9-nullable-baseline.txt;Verbosity=normal"
EXIT_CODE: 0

Output Summary:
"Build succeeded. 0 Warning(s) 0 Error(s)." Time Elapsed 00:00:13.97. This is the
CI-literal command with no `/p:Nullable=enable` added (no project in this repo opts
into it solution-wide; forcing it produces ~195 pre-existing errors and diverges from
CI, per CLAUDE.md C#1.3). The raw log at coverage/logs/p0-t9-nullable-baseline.txt
(3,319,460 bytes, gitignored per `coverage/*`) records a genuine `/t:Rebuild` of the
whole solution with zero errors.
