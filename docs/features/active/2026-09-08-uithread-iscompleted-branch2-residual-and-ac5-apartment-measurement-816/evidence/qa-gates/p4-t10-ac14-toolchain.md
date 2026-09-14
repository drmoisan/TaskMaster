# P4-T10 — AC14 toolchain projection

Timestamp: 2026-09-13T23-47

The four CLAUDE.md toolchain commands were run in order in Phase 4, in a single clean pass with no
restart. Each is recorded verbatim below with its exit code.

## 1. Formatting

```
dotnet tool run csharpier format .
```

Exit code: **0** (task P4-T2)

Verified read-only by:

```
dotnet tool run csharpier check .
```

Exit code: **0** (task P4-T3)

## 2. Analyzers

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

Exit code: **0** (task P4-T4; the MSBuild executable is resolved through vswhere and a detailed file
logger is attached, neither of which changes the gate)

## 3. Type check

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

Exit code: **0** (task P4-T5)

## 4. Tests

```
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p4-t6-run1.trx" /ResultsDirectory:coverage\trx\p4-t6 "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
```

Exit code: **0**

**Which run supplies AC14's fourth command:** the first of P4-T6, P4-T7 and P4-T8 whose recorded
exit code is zero. That is **P4-T6, repetition 1**. All three repetitions recorded exit code 0, so
the requirement that at least one recorded a zero exit code is met; the first is named here for
determinacy.

## Non-vacuity observations for both msbuild commands

| Command | Task | `Skipping target "CoreCompile"` count | `Task "Csc"` count |
|---|---|---|---|
| Analyzers | P4-T4 | **0** | **36** |
| Type check | P4-T5 | **0** | **36** |

Cited from `evidence/qa-gates/p4-t4-msbuild-analyzers.md` and
`evidence/qa-gates/p4-t5-msbuild-nullable.md`.

Both artifacts state, and this projection repeats, that the skipped-compile count is an invariant
restatement rather than a condition that can fail: under `/t:Rebuild` MSBuild cannot emit that
message. The observation that can fail, and that carries the non-vacuity claim, is the compile-task
count, which is 36 on each command — one Csc invocation per project configuration actually compiled.

## Formatting-check result

The formatting check reported **zero** lines matching `Was not formatted`, over 1634 files checked.
Cited from `evidence/qa-gates/p4-t3-csharpier-check.md`.

## The last formatting step modified no file

The four SHA-256 content hashes captured immediately before and immediately after
`dotnet tool run csharpier format .` are **equal, pair for pair**, for all four owned C# files:

| File | Pair equal |
|---|---|
| `UtilitiesCS\Threading\UiThread.cs` | yes |
| `UtilitiesCS.Test\Threading\UiThread_Tests.cs` | yes |
| `UtilitiesCS.Test\Threading\UiThreadInitContract_Tests.cs` | yes |
| `UtilitiesCS.Test\Threading\UiThreadApartmentMeasurement_Tests.cs` | yes |

Cited from `evidence/qa-gates/p4-t2-csharpier-format.md`, which carries the eight hash values. The
formatter therefore modified no file, so no subsequent restart from step one was required and none
was performed.

## FAIL determination

No recorded exit code is non-zero. The format check reported no diff. Neither msbuild log shows a
skipped compile. The last formatting step modified no file. No FAIL condition of AC14 is met.
