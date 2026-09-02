Timestamp: 2026-08-31T10:43:37-04:00
Command (analyzers): `pwsh -NoProfile -Command '<resolved MSBuild> TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true'`
EXIT_CODE (analyzers): 0
Command (nullable): `pwsh -NoProfile -Command '<resolved MSBuild> TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true'`
EXIT_CODE (nullable): 0
NULLABLE_OPT_IN_PROPERTY: absent
Output Summary: Both builds succeeded with `(Rebuild target(s))` observed. Each reported 5 warnings and 0 errors.
