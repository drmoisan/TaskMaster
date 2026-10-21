# AC8 — Compile-Time Proof of TimeProviderTaskExtensions.CreateCancellationTokenSource

Timestamp: 2026-09-09T16-42

Command: & $msbuild UtilitiesCS\UtilitiesCS.csproj /t:Rebuild /m /p:Configuration=Debug /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /flp:LogFile=docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/other/ac8-createcancellationtokensource-proof.txt;Verbosity=detailed

EXIT_CODE: 0

MemberProven: TimeProviderTaskExtensions.CreateCancellationTokenSource
CS1061Count: 0
UsingSystemThreadingTasksPresent: true
PackagesDirectoryPresentAtP0T3: true

Output Summary: P1-T1 added the settling call site
`_ = TimeProvider.System.CreateCancellationTokenSource(TimeSpan.FromMilliseconds(1));` as the first
statement of GetTableInViewAsync in
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs. The project-scoped rebuild
reported "Build succeeded", 0 Warning(s) and 0 Error(s) in 00:00:03.18. The captured log carries
8290 lines and zero occurrences of the token `CS1061`, so the member is available on the
`Microsoft.Bcl.TimeProvider` 10.0.11 assembly this project references, proven by compilation rather
than by reading the shipped XML documentation. The log also carries two lines containing the token
`/out:obj\Debug\UtilitiesCS.dll`, which is the csc.exe command line MSBuild echoes under this
project's CoreCompile heading, proving the compilation ran rather than being skipped. A
`Task "Csc"` search was deliberately not used: that line carries the project instance id rather than
the project path and cannot be attributed to a named project on its own.

The solution platform name `Any CPU` was deliberately not passed. UtilitiesCS/UtilitiesCS.csproj
line 9 defaults $(Platform) to `AnyCPU` and its Debug property group at line 22 is conditioned on
`Debug|AnyCPU`, so `/p:Platform=Any CPU` would match no property group, leave OutputPath unset and
fail before compilation, producing no CS1061 and therefore no refutation. That failure did not
occur. The evidence/other/ directory was created before the invocation, because MSBuild's file
logger does not create intermediate directories and an absent directory part terminates the build
with MSB1029; no MSB1029 occurred.

## Confounder exclusions

Both confounders that would produce the same CS1061 as an absent member are recorded as excluded,
even though the observed CS1061 count is zero, because the plan requires them whenever the count is
read as evidence.

UsingSystemThreadingTasksPresent is true:
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs still contains the line
`using System.Threading.Tasks;` at line 9, with `#nullable enable` at line 1.
TimeProviderTaskExtensions lives in that namespace, so its absence would produce the same
diagnostic as an absent member.

PackagesDirectoryPresentAtP0T3 is true: the artifact
evidence/baseline/restore.md written by P0-T3 carries `PackagesDirectoryPresent: true`, recording
that packages/Microsoft.Bcl.TimeProvider.10.0.11/lib/net462/Microsoft.Bcl.TimeProvider.dll exists on
disk. An unrestored packages tree would produce the same diagnostic through an unresolved reference.

## Outcome

The member is available. The P1-T4 fallback branch is not taken, and Phase 3 implements the
clock-derived factory named in spec.md Proposed Fix item 2 rather than the in-repo provider-driven
fallback.

## D7 sanitisation

The sibling ac8-createcancellationtokensource-proof.txt was sanitised as the last action of this
task, after every count above was taken. Three rewrites were applied in order: the worktree root to
`<repo-root>`, the main checkout root to `<main-checkout-root>`, and, as the deviation recorded in
full at evidence/baseline/build-analyzers.md, the user profile root to `<user-profile-root>`. The
count of lines containing `C:\Users\` in this log is now 0.
