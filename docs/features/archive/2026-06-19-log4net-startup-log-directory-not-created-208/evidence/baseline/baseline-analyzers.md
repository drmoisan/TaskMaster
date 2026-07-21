# Baseline — .NET Analyzer Build (Issue #208, [P0-T3])

Timestamp: 2026-07-09T09-29

Command: msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true -m
(Run via the VS18 MSBuild.exe. Dash-prefixed switches are used because the Bash tool runs under
git-bash, which mangles slash-prefixed MSBuild switches into paths. `-m` enables parallel build.)

EXIT_CODE: 0

Output Summary: Build succeeded. 0 Error(s), 75 Warning(s) (per MSBuild summary). All warnings are
pre-existing and none are in the in-scope TaskMaster production sources touched by this fix. Warning
codes (parallel build double-reports, so raw grep counts exceed the 75 summary): CS8632 (nullable
annotation outside #nullable context, in UtilitiesCS.Test), CS0618 (obsolete IAsyncEnumerable
overloads), CS0108/CS0169/CS0067/CS0649/CS0168 (hiding/unused members), MSTEST0032. Baseline analyzer
state: clean build, no errors.
