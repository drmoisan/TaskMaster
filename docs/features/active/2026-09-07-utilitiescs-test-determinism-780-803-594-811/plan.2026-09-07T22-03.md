# 2026-09-07-utilitiescs-test-determinism-780-803-594 (Plan)

- **Issue:** #811
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-08T02-20
- **Status:** Approved for execution. Executor preflight returned PREFLIGHT: ALL CLEAR on round 3, after the round-1 delta (nine defects) and the round-2 delta (four defects) were applied. The MCP plan validator returns ok with no warnings in either channel.
- **Version:** 1.2
- **Work Mode:** full-bug
- **Base commit:** 04a54e681bd21e841e124c016df30672ee701b75 (origin/main at branch creation)
- **Branch:** bug/utilitiescs-test-determinism-780-803-594-811

**Fail-closed evidence rule:** Every evidence-producing task names its artifact path. If any required
baseline artifact, QA artifact, or coverage-comparison artifact is missing or incomplete, the verdict
is BLOCKED or INCOMPLETE, never PASS. An unchecked task may not be checked off without its artifact.

**Evidence accounting rule:** Each command-step artifact carries `Timestamp:`, `Command:`,
`EXIT_CODE:`, and `Output Summary:`. Artifacts for gates whose expected exit code is non-zero also
carry `ExpectedExitCode:`. One artifact per non-zero-capable gate.

**Requirements authority.** spec.md is the sole acceptance-criteria source for this `full-bug` item
and carries exactly five criteria, AC1 through AC5, under `## Acceptance Criteria` at spec.md line
291. user-story.md is absent and stays absent. The research artifact
`research/root-cause.2026-09-07T22-10.md` is authoritative for mechanism; spec.md is authoritative
for scope and acceptance; where they differ, spec.md wins.

---

## Write Set Under Change

The definitive list is spec.md `#### Files/modules to change:` (items 1 to 20). Repository-relative
paths, with the line count measured in this worktree at the base commit:

### Production (8), all carry `#nullable enable`

1. `UtilitiesCS/Extensions/DictionaryExtensions.cs` (282)
2. `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs` (474; 26 lines of headroom under the cap)
3. `UtilitiesCS/Extensions/DfDeedle.cs` (314)
4. `UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs` (276)
5. `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs` (122)
6. `UtilitiesCS/HelperClasses/PrettyPrint.cs` (680; PRE-EXISTING cap violation; zero net growth)
7. `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` (430)
8. `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs` (199)

### Tests (11), none carries a whole-file `#nullable enable`

9. `UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs` (296)
10. `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` (869; PRE-EXISTING cap violation; must end strictly below 869)
11. `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs` (NEW)
12. `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs` (NEW)
13. `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` (exactly 500; may not gain one line)
14. `UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs` (NEW; receives the moved helper)
15. `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` (276)
16. `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs` (408)
17. `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` (125)
18. `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` (1855; PRE-EXISTING cap violation; must end strictly below 1855)
19. `UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs` (119)

### Project file (1)

20. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (982; Compile items only; the 500-line cap does not reach `*.csproj`)

Total: 20 paths. Feature-folder artifacts under
`docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/` (written `<FEATURE>`
below) and the two follow-up entries under `docs/features/potential/` are additionally created or
updated by this plan and are outside the source write set.

Not touched, and every gate is written so that it cannot require touching them:
UtilitiesCS/Threading/TimeOutTask.cs (1011 lines; the inert `(int, int)` overloads stay),
UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs (its line-96 doc comment names `TableEtlInvoker` and
becomes stale; recorded as a follow-up in P8-T9, not edited), the roughly 24 test classes that install
a `DebugTextWriter` with no restore, any `.github/workflows/` file, any `packages.config`, any
`.runsettings`, `.editorconfig`, and `BannedSymbols.txt`.

---

## Decisions record (settled; not re-opened by the executor)

- **D1. Production timing is unchanged, byte-for-byte.** `TimeOutTask.cs:824` (`(this Task<TResult>, int millisecondsTimeout, int repeatAttempts)`) calls `task.TimeoutAfter(millisecondsTimeout)` at line 834, which binds to the `(this Task<TResult>, int, TimeProvider? timeProvider = null)` overload at line 862. That overload is not `async` and returns the task itself (line 873) or a proxy that a timer later faults; it never throws synchronously, so the `catch (TimeoutException)` at line 836 cannot fire and the retry at line 841 is dead. Replacing `.TimeoutAfter(ms, attempts)` with `.TimeoutAfter(ms, timeProvider)` where `timeProvider` is null is therefore exactly today's behaviour on the system clock. No deadline value changes: 250 ms x rowCount stays at `OlTableExtensions.Etl.cs:81`, 1000 ms stays at `DfDeedle.cs:208`, 3000 ms stays in `DfDeedle.QfcColumns.cs`.
- **D2. The AC2 citation.** The issue and AC2 cite `DfDeedle.cs:186`. Line 186 opens the `LogDfTiming(` call; the dereference `tableSnapshot.Item1.GetLength(0)` is on line 188 inside that call. The guard is inserted between line 185 and line 186, ahead of the whole call.
- **D3. `TryAddValuesAsync` has zero production call sites.** The only invocation in the repository is `UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs:244`; the other two hits are the definition (`DictionaryExtensions.cs:169`) and the test method name (line 237). `TryAddValues` (lines 123-136) is a bounded compare-and-swap loop that returns `false` when the key is absent; it cannot hang. The 500 ms window is deleted (AC1 first disjunct), not re-seamed.
- **D4. The RED test needs the seam first.** A test that passes `timeProvider:` to `GetEmailDataInViewAsync` cannot compile until the parameter exists. Phase 1 lands every seam with no behaviour change; Phase 2 adds the regression test, which then fails at assertion time with `NullReferenceException`; Phase 3 adds the guard. This is the declaration-then-red ordering, and it keeps the whole test assembly compiling at every task.
- **D5. Deterministic arming order in the RED test.** On the failing path two timers are armed on the injected clock: first the 3000 ms column-add deadline inside `AddQfcColumnsAsync` (`DfDeedle.QfcColumns.cs:127`), then the 250 ms ETL hop deadline (`OlTableExtensions.Etl.cs:245`). Whether the first timer is armed at all depends on a race between the pool thread running `AddQfcColumns` and the caller reaching `TimeoutAfter`. The test removes the race with two test-owned gates: `Columns.Add("SentOn")`, the first call `AddQfcColumns` makes (`DfDeedle.QfcColumns.cs:40`), blocks on gate A; `Table.GetNextRow()` blocks on gate B. The test awaits `Armed` (timer 1, deterministic because the adder is blocked), re-arms, releases gate A, awaits `Armed` (timer 2, deterministic because the row read is blocked), then advances the clock by 250 ms. Both gates are released in `finally` so the orphaned `Task.Run` bodies complete. `FakeTimeProvider` arms timers relative to its current time, so advancing before timer 2 exists would hang the test; this ordering is what prevents that.
- **D6. AC1 has no deterministic RED.** The original failure is load-dependent and cannot be reproduced without a sleep, which AC5 forbids. AC1's fail-before evidence is a `fail-before-exception` dossier (P4-T3) whose alternative proof is the structural count gate on `CancelAfter(` (1 before, 0 after), plus the contract-lock test that a pre-cancelled token still yields `TaskCanceledException`. The spec Test Strategy's "token cancelled after the work is observed to start" test is not authored: `TryAddValues` offers no observation point without a custom operator type, and the test would pass identically before and after the fix, so it has no discriminating power. This is a recorded deviation from the spec's Test Strategy prose; it changes no acceptance criterion.
- **D7. `DfDeedle` static seams.** `TableEtlInvoker` (`DfDeedle.cs:69-72`) and `StoreTableEtlInvoker` (`DfDeedle.cs:81-84`) are deleted and replaced by an optional trailing delegate parameter on `GetEmailDataInView(Explorer)` and on both `FromDefaultFolder` overloads. One `private static readonly` default delegate `DefaultTableEtl` replaces both (they are the same lambda `t => ((Outlook.Table)t).ETL()`). It is written in the same one-line-initializer shape as the statics it replaces so the per-file coverage accounting in P7-T6 sees the same line shape. `MessageBoxInvoker` (`DfDeedle.cs:54-60`) stays static per spec.md; its restored-in-`finally`, reader-free status is documented in the test-class header.
- **D8. `[DoNotParallelize]` disposition.** Removed from `StackGeek_Tests` (line 15), `PrettyPrint_Tests` (line 19) and `DASLFilterParserTests` (line 14), whose comments name the console redirect as the sole reason. Retained on `OlTableExtensions_Tests` (line 20) under spec.md Mitigation 4, with the comment at lines 17-19 rewritten so it no longer claims the console: the class is 1855 lines of COM-mock tests, ten of its tests drive `GetTableInViewAsync`/`RunWithTimeout` (a 2000 ms wall-clock window), and it has never been soaked under class-level parallelism. Removal after a soak is a follow-up (P8-T9).
- **D9. `OlTableExtensions.Etl.cs` headroom.** The change adds one signature line, two comment lines and removes one local, for a net of about +3 (474 to about 477). The dead `EtlAsyncOld` (lines 132-169) is NOT deleted: doing so would also delete its test at `OlTableExtensions_Tests.cs:986-1017` and widen the diff for no acceptance gain. Consequence: `EtlAsyncOld` retains `var attempts = 3;` (line 148), `.TimeoutAfter(milliseconds, attempts)` (line 158) and `DateTime.Now` (line 163), so every gate over those tokens in this file is an exact-count gate, never a zero-hit gate.
- **D10. `PrettyPrint.cs` zero-growth budget.** Adding `using System.IO;` costs +1 and each seamed overload wraps to two lines under CSharpier's 100-column width (+2). Three removals pay for it: line 6 `using System.Threading.Tasks;` (the token `Task` does not occur anywhere in the file), line 12 `using System.Text;` (duplicates line 5), line 18 `using Svg;` (the token `Svg` occurs only on line 18). If, and only if, the P5-T10 build reports a CS0103 or CS0246 in `PrettyPrint.cs`, line 18 is restored and line 15 `using System.Windows.Input;` (no `System.Windows.Input` type name occurs in the file; the two `Key` hits at lines 174 and 535 are `kvp.Key`/`x.Key` property reads) is removed instead, and the artifact records the substitution. The gate is the outcome: the file's line count is at most 680 after every task.
- **D11. AC4 is local evidence, not a CI gate.** `.github/workflows/_mstest-coverage.yml:86-92` discovers assemblies with a recursive `*.Test.dll` search (not a hard-coded list) and the job carries `timeout-minutes: 30`; ten consecutive full runs do not fit. The local gate enumerates the nine assemblies explicitly (no recursive discovery, so the dot-claude worktree problem never arises), extends the filter with the shell-icon exclusion that the #798 baseline recorded as `SHELL_ICON_EXCLUSION: REQUIRED` on this host (one of 23 shell-icon tests fails per run with an invalid Win32 icon handle, independent of this fix; CI runs them unfiltered), omits `/Settings:` so parallelism comes from `[assembly: Parallelize(Workers = 0, Scope = ClassLevel)]` at `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21` exactly as in CI, and is split into five two-run tasks so each fits inside one 600000 ms tool invocation. P0-T12 establishes once, before any source change, whether the CI-verbatim shape is locally viable.
- **D12. AC5 has no automated enforcement.** `BannedSymbols.txt` (7 entries) bans `Thread.Sleep` and `Task.Delay` but not `CancelAfter`, `TimeoutAfter` or `WaitOne`, and `.editorconfig:548` holds RS0030 at `suggestion`, so no toolchain step fails on a new sleep. P7-T10 is an explicit search over the anchored diff's added lines. The pre-existing tolerance at `OlTableExtensions_Tests.cs:960-963` (`Returns(120)` with the comment "so the timeout cannot fire under test-host contention") is retired in P6-T1.
- **D13. New test files must be registered.** `UtilitiesCS.Test.csproj` is a legacy non-SDK project with explicit `<Compile Include>` items (the `DfDeedle` family sits at lines 188-191, `TestHelpers\` at 74-76, `OutlookObjects\Table\OlTableExtensions_Tests.cs` at 538). An unregistered test file still builds and is silently absent from the assembly. Every task that creates a test file is immediately followed by the task that registers it, and P8-T1 carries a non-vacuity control naming every new test.
- **D14. The reflection call at `OlTableExtensions_Tests.cs:1124-1137`** binds `EtlByRowAsync` by an explicit four-type array (`Table`, `Dictionary<string, Func<object, string>>`, `Dictionary<string, int>`, `CancellationToken`), selecting the four-parameter overload at `OlTableExtensions.Etl.cs:171`. P1-T1 changes only the seven-parameter overload at line 229. P6-T2 re-checks the call after all edits, because a reflection binding is invisible to the compiler.
- **D15. Coverage measurement.** Baseline and final full-suite runs use the repository wrapper's own argument shape (`scripts/vscode/Invoke-MSTestWithCoverage.ps1:70-76`): `dotnet-coverage collect ... -- <vstest> <assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:...`, issued directly because the wrapper hard-codes its filter at line 76 and cannot take the shell-icon exclusion, and because its discovery at line 301 rejects every assembly under a dot-claude path when run from an agent worktree. Raw Cobertura documents stay under the gitignored `coverage/` directory (`.gitignore:144`) and are NOT committed; per-file figures are recorded in the artifacts. The repository-wide root `line-rate` is not reproducible run-to-run on this pipeline (the merged denominator moves), so it is reported under a two-branch comparability rule and the blocking coverage gates are per-file and per-changed-line.

---

## Plan-wide conventions

### C1. Shell preamble (every fenced PowerShell block)

Every command block is executed as one `pwsh -NoProfile -File coverage/plan811-helper.ps1` invocation.
The helper path is fixed, gitignored (`coverage/*`), rewritten in place for every task, registered
in no project file, asserted by no acceptance condition, and is not a deliverable. No shell variable
survives between tasks; every block begins with this preamble verbatim:

```powershell
    $ErrorActionPreference = 'Stop'
    $Root = (git rev-parse --show-toplevel)
    Set-Location -LiteralPath $Root
    if ((git rev-parse --abbrev-ref HEAD) -ne 'bug/utilitiescs-test-determinism-780-803-594-811') { throw 'wrong worktree' }
    $LASTEXITCODE = 0
    $Base = '04a54e681bd21e841e124c016df30672ee701b75'
    $Feature = 'docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811'
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
    $msbuild = & $vswhere -latest -products * -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    $dotnet = Join-Path $Root '.dotnet-sdk\dotnet.exe'
    $Assemblies = @(
      'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll', 'SVGControl.Test\bin\Debug\SVGControl.Test.dll',
      'Tags.Test\bin\Debug\Tags.Test.dll', 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll',
      'TaskTree.Test\bin\Debug\TaskTree.Test.dll', 'TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll',
      'ToDoModel.Test\bin\Debug\ToDoModel.Test.dll', 'UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll',
      'VBFunctions.Test\bin\Debug\VBFunctions.Test.dll')
    $FullFilter = 'TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser'
    $Blame = '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None'
```

Lines inside fenced blocks are indented four spaces so that no line begins at column 0 with `#`.
Backslash-bearing literals are written with single backslashes inside single-quoted PowerShell
strings; no doubled backslash appears anywhere in this document.

### C2. Toolchain commands (verbatim from CLAUDE.md; order is binding; restart from step 1 on any failure or rewrite)

1. `dotnet tool run csharpier format .` (invoked as `& $dotnet tool run csharpier format .`), verified with `dotnet tool run csharpier check .`
2. msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
3. msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
4. `vstest.console.exe` over the nine assemblies with coverage (D15 form for the coverage steps; CI-verbatim `/EnableCodeCoverage` form for AC4)

Facts that make a gate vacuous or unsatisfiable if ignored:

- `/t:Rebuild` is mandatory for the two solution gates. A warm `/t:Build` returns exit 0 with `CoreCompile` skipped and runs no analyzers.
- `/p:Nullable=enable` is never added. CI omits it; adding it conscripts every un-annotated file.
- `"/p:Platform=Any CPU"` is a solution-level alias. Project-file builds (used only to produce a test assembly for a scoped run, never as a gate) use `/p:Platform=AnyCPU` without the space.
- A successful msbuild prints the substring `error` in switch names and summary text. Build gates assert the exit code plus the verbatim summary line `0 Error(s)` read from a normal-verbosity file log written to `coverage/msbuild-<task>.log`, never the absence of the substring.
- `csharpier format` exits 0 whether or not it rewrote anything, and `Formatted N files in Mms.` is a processed-file count, not a rewrite count. Every format task records SHA-256 hashes of the 19 write-set `.cs` files before and after, defines rewritten-count as the number of hash differences, and is paired with `csharpier check .`, whose success line begins with the literal `Checked ` and ends with `ms.` (observed on this host: `Checked 1587 files in 7826ms.`).
- Every `git diff` is anchored to `$Base`, except P3-T3's numstat over spec.md, which is anchored to `HEAD` because spec.md does not exist at `$Base`. Every name-listing diff is paired with `git add --intent-to-add` for the three new test files and a `git status --porcelain --untracked-files=all` span in the same task.
- The dot-claude agent-memory tree is tracked and may be dirty from preparation agents. Every diff, status and name-listing gate is scoped with the pathspec exclusion written in plain prose as ":(exclude).claude".
- A bare `/Logger:trx` names the file after the account and machine. Every vstest invocation passes `"/Logger:trx;LogFileName=<task>.trx"` (double-quoted; an unquoted semicolon terminates the argument) and `/ResultsDirectory:coverage/trx/<task>` (gitignored, one directory per task). TRX and `.coverage` files are never committed: a TRX carries the account token in `runUser=` and the machine name in every `computerName=`.
- No artifact, plan line, or commit message contains an absolute host path, the account name, or the machine name. Tokens are derived at run time (`$acct = [regex]::Escape((Split-Path -Leaf $env:USERPROFILE))`, `$mach = [regex]::Escape($env:COMPUTERNAME)`) and never written out.
- Every token count in this plan is `@(Select-String -LiteralPath <path> -CaseSensitive -SimpleMatch '<token>').Count` unless the clause says "with regex", in which case `-Pattern` replaces `-SimpleMatch` and `-CaseSensitive` is still supplied. `Select-String` is case-insensitive by default, so the switch is load-bearing: without it `TimeProvider` also matches `timeProvider`.
- A test-scoped vstest run against UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll alone cannot reach the repository's only `[TestCategory("LiveOutlook")]` test (it lives in TaskMaster.Test), so scoped filters use `FullyQualifiedName~<Class>` joined with `|`; every nine-assembly run carries `$FullFilter`.

### C3. Reusable procedures (copied verbatim into the helper when a task names them)

**PROC-TRX** (read the counters and named results out of a TRX):

```powershell
    function Read-TrxCounters([string]$Path) {
      [xml]$trx = Get-Content -LiteralPath $Path -Raw
      $c = $trx.TestRun.ResultSummary.Counters
      [pscustomobject]@{ total=[int]$c.total; executed=[int]$c.executed; passed=[int]$c.passed; failed=[int]$c.failed; error=[int]$c.error; timeout=[int]$c.timeout; aborted=[int]$c.aborted; notExecuted=[int]$c.notExecuted }
    }
    function Read-TrxOutcome([string]$Path, [string]$ClassName, [string]$MethodName) {
      [xml]$trx = Get-Content -LiteralPath $Path -Raw
      $ns = @{ t = 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010' }
      $ut = Select-Xml -Xml $trx -Namespace $ns -XPath "//t:UnitTest[starts-with(t:TestMethod/@className,'$ClassName') and t:TestMethod/@name='$MethodName']"
      if (-not $ut) { return 'ABSENT' }
      $id = $ut.Node.GetAttribute('id')
      $r = Select-Xml -Xml $trx -Namespace $ns -XPath "//t:UnitTestResult[@testId='$id']"
      if (-not $r) { return 'NO-RESULT' }
      return $r.Node.GetAttribute('outcome')
    }
    function Read-TrxMessage([string]$Path, [string]$ClassName, [string]$MethodName) {
      [xml]$trx = Get-Content -LiteralPath $Path -Raw
      $ns = @{ t = 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010' }
      $ut = Select-Xml -Xml $trx -Namespace $ns -XPath "//t:UnitTest[starts-with(t:TestMethod/@className,'$ClassName') and t:TestMethod/@name='$MethodName']"
      if (-not $ut) { return 'ABSENT' }
      $id = $ut.Node.GetAttribute('id')
      $m = Select-Xml -Xml $trx -Namespace $ns -XPath "//t:UnitTestResult[@testId='$id']/t:Output/t:ErrorInfo/t:Message"
      if (-not $m) { return 'NO-MESSAGE' }
      return $m.Node.InnerText
    }
```

**PROC-COV** (per-file line map from a raw Cobertura document; both node axes merged by line number keeping the maximum `hits`, which is the repository helper's own dedup rule):

```powershell
    function Get-FileLineMap([xml]$Doc, [string]$Suffix) {
      $map = @{}
      foreach ($cls in $Doc.SelectNodes('//class')) {
        $fn = $cls.GetAttribute('filename')
        if (-not $fn.EndsWith($Suffix)) { continue }
        $nodes = @($cls.SelectNodes('./lines/line')) + @($cls.SelectNodes('./methods/method/lines/line'))
        foreach ($ln in $nodes) {
          $n = [int]$ln.GetAttribute('number'); $h = [int]$ln.GetAttribute('hits')
          if (-not $map.ContainsKey($n) -or $map[$n] -lt $h) { $map[$n] = $h }
        }
      }
      return $map
    }
    function Get-FileSummary([xml]$Doc, [string]$Suffix) {
      $m = Get-FileLineMap $Doc $Suffix
      $valid = $m.Count; $covered = @($m.Values | Where-Object { $_ -gt 0 }).Count
      "suffix=$Suffix valid=$valid covered=$covered uncovered=$($valid - $covered)"
    }
    function Get-MethodLineRate([xml]$Doc, [string]$Suffix, [string]$Method) {
      foreach ($cls in $Doc.SelectNodes('//class')) {
        if (-not $cls.GetAttribute('filename').EndsWith($Suffix)) { continue }
        foreach ($mth in $cls.SelectNodes('./methods/method')) {
          if ($mth.GetAttribute('name') -eq $Method) { return [double]$mth.GetAttribute('line-rate') }
        }
      }
      return 'ABSENT'
    }
```

The eight production suffixes, written with single backslashes exactly as the collector emits them:
`UtilitiesCS\Extensions\DictionaryExtensions.cs`, `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.Etl.cs`,
`UtilitiesCS\Extensions\DfDeedle.cs`, `UtilitiesCS\Extensions\DfDeedle.FrameUtilities.cs`,
`UtilitiesCS\OutlookObjects\Filter DASL\DASLFilterParser.cs`, `UtilitiesCS\HelperClasses\PrettyPrint.cs`,
`UtilitiesCS\OutlookObjects\Table\OlTableExtensions.TableAccess.cs`, `UtilitiesCS\ReusableTypeClasses\Other\StackGeek.cs`.

**PROC-CHANGED** (post-image added line numbers of one file from the anchored diff):

```powershell
    function Get-AddedLines([string]$Base, [string]$Path) {
      $out = @()
      foreach ($l in (git diff --unified=0 $Base -- $Path)) {
        if ($l -match '^@@ -\d+(,\d+)? \+(\d+)(,(\d+))? @@') {
          $start = [int]$matches[2]; $len = if ($matches[4]) { [int]$matches[4] } else { 1 }
          if ($len -gt 0) { $out += ($start..($start + $len - 1)) }
        }
      }
      return $out
    }
```

**PROC-HASH** (SHA-256 of the 19 write-set `.cs` files): `Get-FileHash -Algorithm SHA256` over
items 1 to 19 of the Write Set, printed as `<hash> <path>` lines; the three new files are skipped
while absent.

### C4. Literals this plan creates (quoted here so a search for them is a real assertion)

`TimeoutAfter(milliseconds, timeProvider)`, `TimeoutAfter(timeout, timeProvider)`,
`TimeoutAfter(1000, timeProvider)`, `timed out with a timeout of`, `TimeProvider? timeProvider = null`,
`DefaultTableEtl`, `was not produced`, `TextWriter? writer = null`, `Run(Console.Out)`,
`public static void Run(`, `not been soaked under class-level parallelism`,
`class ArmingBarrierTimeProvider`, `Extensions\DfDeedleEtlTimeoutTests.cs`,
`OutlookObjects\Table\OlTableExtensionsEtlClockTests.cs`, `TestHelpers\ArmingBarrierTimeProvider.cs`,
`using UtilitiesCS.Test.TestHelpers;`, `using Microsoft.Extensions.Time.Testing;`,
`MessageBoxInvoker is the one remaining static seam`.

New test methods (fully qualified; the nine names P7-T5 and P8-T1 assert, seven new plus two retained sentinels):

- `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_PreCancelledToken_ThrowsTaskCanceledAndLeavesValueUnchanged` (new)
- `UtilitiesCS.Test.Extensions.DfDeedleEtlTimeoutTests.GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder` (new)
- `UtilitiesCS.Test.Extensions.DfDeedleEtlTimeoutTests.GetEmailDataInViewAsync_ClockNeverAdvances_ReturnsOneRowFrame` (new)
- `UtilitiesCS.Test.OutlookObjects.Table.OlTableExtensionsEtlClockTests.EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource` (new)
- `UtilitiesCS.Test.OutlookObjects.Table.OlTableExtensionsEtlClockTests.EtlAsync_ClockNeverAdvances_ReturnsTransformedRows` (new)
- `UtilitiesCS.Test.ReusableTypeClasses.StackGeek_Tests.Run_WritesScenarioToSuppliedWriter` (new)
- `UtilitiesCS.Test.HelperClasses.PrettyPrint_Tests.PrettyPrint_NullWriter_WritesToConsoleWithoutThrowing` (new)
- `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` (retained, the #780 test)
- `UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` (retained, the #803 test)

Renamed test methods (behaviour now targets a supplied writer): `PrintTree_WritesIndentedTreeToSuppliedWriter`
(was `PrintTree_WritesIndentedTreeToConsole`), `DataFramePrettyHelpers_RenderRowsMarkdownAndWriterOutput`
(was `DataFramePrettyHelpers_RenderRowsMarkdownAndConsoleOutput`),
`EnumerateTable_WritesFormattedOutputToSuppliedWriterAndMovesToStart`
(was `EnumerateTable_WritesFormattedOutputAndMovesToStart`). `Main_RunsSampleScenarioWithoutThrowing` keeps its name.

---

### Phase 0 — Policy reads, toolchain bootstrap, and baselines

- [ ] [P0-T1] Read the policy files in the `policy-compliance-order` sequence and record the read in `<FEATURE>/evidence/baseline/phase0-instructions-read.md`: CLAUDE.md, then `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/tonality.md`, `.claude/rules/plan-acceptance-gates.md`, then `<FEATURE>/issue.md`, `<FEATURE>/spec.md`, `<FEATURE>/research/root-cause.2026-09-07T22-10.md`. Acceptance: the artifact carries `Timestamp:`, `Policy Order:` listing the nine paths in that order, the line `Work Mode: full-bug` read from issue.md line 12, the line `AC Source: spec.md lines 292-296 (AC1..AC5)`, and `user-story.md: ABSENT (expected)` after `Test-Path` on `<FEATURE>/user-story.md` returns `False`.

- [ ] [P0-T2] Record worktree identity and the anchor commit in `<FEATURE>/evidence/baseline/p0-t2-worktree-identity.md`: run the C1 preamble; record `git rev-parse HEAD`, `git rev-parse --abbrev-ref HEAD`, `git cat-file -t 04a54e681bd21e841e124c016df30672ee701b75`, `git merge-base HEAD 04a54e681bd21e841e124c016df30672ee701b75`, and `git status --porcelain --untracked-files=all -- . ":(exclude).claude"`. Acceptance: the branch name equals `bug/utilitiescs-test-determinism-780-803-594-811`; `cat-file -t` prints `commit`; the merge-base output equals `04a54e681bd21e841e124c016df30672ee701b75` (so the anchor is an ancestor of HEAD); the scoped porcelain lists only paths under `<FEATURE>/`; the artifact carries `BASE-SHA: 04a54e681bd21e841e124c016df30672ee701b75` on its own line and no host path.

- [ ] [P0-T3] Bootstrap the repo-local SDK and restore the tool manifest, recording `<FEATURE>/evidence/baseline/p0-t3-toolchain-bootstrap.md`: if `Test-Path .dotnet-sdk\dotnet.exe` is `False`, run `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1` (global.json pins 8.0.205 under `.dotnet-sdk`; a fresh agent worktree lacks it); then run `& $dotnet tool restore` at the worktree root. Acceptance: `& $dotnet --version` prints `8.0.205`; the tool-restore output contains the line `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier`; both commands exit 0; the artifact records which branch (install or already-present) applied.

- [ ] [P0-T4] Restore NuGet packages for the packages.config projects, recording `<FEATURE>/evidence/baseline/p0-t4-package-restore.md`: run `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1` (msbuild `/t:Restore /p:RestorePackagesConfig=true`; parameters `-SolutionPath`, `-Configuration`, `-Platform` default to TaskMaster.sln, Debug, Any CPU). Acceptance: exit 0; `Test-Path packages\Microsoft.Bcl.TimeProvider.10.0.11\lib` and `Test-Path packages\Microsoft.Extensions.TimeProvider.Testing.10.9.0\lib` both print `True` (the seam packages spec.md relies on; no package is added by this plan); the artifact records the count of directories directly under `packages\`.

- [ ] [P0-T5] Resolve the three external tools and record `<FEATURE>/evidence/baseline/p0-t5-tool-resolution.md`: `$vstest`, `$msbuild` (both from the C1 preamble) and `Get-Command dotnet-coverage`. Acceptance: `$vstest` and `$msbuild` are non-empty and `Test-Path` on each prints `True`; `dotnet-coverage --version` exits 0 and prints a version string beginning with a digit; if `Get-Command dotnet-coverage` fails, run `dotnet tool install --global dotnet-coverage` and re-check, recording that the install branch applied. Redact every resolved path to `<vs-install>` or `<user>` in the artifact.

- [ ] [P0-T6] Verify this plan's citations against the tree and record `<FEATURE>/evidence/baseline/p0-t6-citation-baseline.md`, using `(Get-Content -LiteralPath <path>).Count` for line counts and `@(Select-String -LiteralPath <path> -CaseSensitive -SimpleMatch '<token>').Count` for token counts (case-sensitive, per C2). Acceptance: every value below matches exactly; on any mismatch the artifact records `CITATION-MISMATCH: <path> <expected> <observed>` and execution stops with the plan returned for revision (this is the only halt this plan permits, and it precedes every source edit).
  - Line counts: `UtilitiesCS/Extensions/DictionaryExtensions.cs`=282; `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs`=474; `UtilitiesCS/Extensions/DfDeedle.cs`=314; `UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs`=276; `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs`=122; `UtilitiesCS/HelperClasses/PrettyPrint.cs`=680; `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`=430; `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs`=199; `UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs`=296; `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`=869; `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs`=500; `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs`=276; `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs`=408; `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs`=125; `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs`=1855; `UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs`=119; `UtilitiesCS/Threading/TimeOutTask.cs`=1011; the three new files ABSENT.
  - Token counts: in `DictionaryExtensions.cs`: `CancelAfter(`=1, `CreateLinkedTokenSource`=1. In `OlTableExtensions.Etl.cs`: `var attempts = 3;`=2, `TimeoutAfter(milliseconds, attempts)`=2, `TimeoutAfter(timeout, attempts)`=2, `timed out {attempts} times`=2, `DateTime.Now`=2, `TimeProvider`=0. In `DfDeedle.cs`: `TableEtlInvoker`=3, `TimeoutAfter(1000, 2)`=2, `TimeProvider`=0, `was not produced`=0. In `DfDeedle.FrameUtilities.cs`: `StoreTableEtlInvoker`=1. In `DASLFilterParser.cs`: `Console.WriteLine`=1, `TextWriter`=0. In `PrettyPrint.cs`: `Console.WriteLine`=2, `TextWriter`=0. In `OlTableExtensions.TableAccess.cs`: `Console.WriteLine`=5, `TextWriter`=0. In `StackGeek.cs`: `Console.WriteLine`=7, `public static void Run(`=0. In `DfDeedle_COM_Tests.cs`: `TableEtlInvoker`=14, `FakeTimeProvider`=0. In `DfDeedleQfcColumnTimeoutTests.cs`: `class ArmingBarrierTimeProvider`=1. In `StackGeek_Tests.cs`: `Console.SetOut`=3, `DoNotParallelize`=1. In `PrettyPrint_Tests.cs`: `Console.SetOut`=3, `DoNotParallelize`=2 (the line-14 comment mention and the line-19 attribute). In `DASLFilterParserTests.cs`: `Console.SetOut`=3, `DoNotParallelize`=1. In `OlTableExtensions_Tests.cs`: `Console.SetOut`=2, `DoNotParallelize`=1, `Returns(120)`=1, `cannot fire under test-host`=1, `redirects Console.Out`=1, `FakeTimeProvider`=0. In `NLogTraceWriter_Test.cs`: `Console.SetOut`=2, `originalOut`=3, `TestCleanup`=2. In `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: `Extensions\DfDeedleEtlTimeoutTests.cs`=0, `OutlookObjects\Table\OlTableExtensionsEtlClockTests.cs`=0, `TestHelpers\ArmingBarrierTimeProvider.cs`=0.

- [ ] [P0-T7] Baseline toolchain step 1 in `<FEATURE>/evidence/baseline/p0-t7-csharpier-check.md`: run `& $dotnet tool run csharpier check .` at the worktree root. Acceptance: the artifact carries `Timestamp:`, `Command: dotnet tool run csharpier check .`, `EXIT_CODE: <observed>`, `Output Summary:` quoting the summary line verbatim (it begins with `Checked ` and ends with `ms.`), and a `PRE-EXISTING DRIFT:` line listing every path the tool reported as unformatted, or `PRE-EXISTING DRIFT: none` when the exit code is 0. Expected on this host: exit 0 and `none` (observed one day earlier by #798).

- [ ] [P0-T8] Baseline toolchain step 2 in `<FEATURE>/evidence/baseline/p0-t8-msbuild-analyzers.md`: run `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:q "/flp:LogFile=coverage/msbuild-p0-t8.log;Verbosity=normal"`. Acceptance: `EXIT_CODE: 0`; the file log contains the line `    0 Error(s)` and the artifact quotes the `Warning(s)` and `Error(s)` lines verbatim and records `BASELINE_ANALYZER_WARNINGS: <n>`; `Test-Path UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` prints `True` afterwards.

- [ ] [P0-T9] Baseline toolchain step 3 in `<FEATURE>/evidence/baseline/p0-t9-msbuild-nullable.md`: run `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo /v:q "/flp:LogFile=coverage/msbuild-p0-t9.log;Verbosity=normal"`. Acceptance: `EXIT_CODE: 0`; the file log contains `    0 Error(s)`; the artifact states in one sentence that `/p:Nullable=enable` was not supplied and why (CLAUDE.md C#1.3).

- [ ] [P0-T10] Baseline toolchain step 4, full nine-assembly coverage run, in `<FEATURE>/evidence/baseline/p0-t10-vstest-coverage.md` (PROC-TRX): create `coverage/trx/p0-t10`, then run `dotnet-coverage collect --output coverage/p0-baseline.cobertura.xml --output-format cobertura --settings coverage.config -- $vstest $Assemblies /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/Logger:trx;LogFileName=p0-t10.trx" /ResultsDirectory:coverage/trx/p0-t10 $Blame "/TestCaseFilter:$FullFilter"`. Acceptance: `EXIT_CODE: <observed>` recorded (0 when no test failed; 1 when `PREEXISTING_FAILURE_SET:` is non-empty; the baseline is a capture and is not remediated); `coverage/trx/p0-t10/p0-t10.trx` exists and its counters are recorded as `total= executed= passed= failed= error= timeout= aborted= notExecuted=`; `failed` plus `error` plus `aborted` plus `timeout` equals 0 OR the artifact lists every failing test name under `PREEXISTING_FAILURE_SET:` (a failure of `TryAddValuesAsync_UpdatesExistingValue` or `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` here is the defect under repair and is recorded, not remediated); `coverage/p0-baseline.cobertura.xml` exists and the artifact records its root `line-rate`, `lines-covered`, `lines-valid`, `branch-rate`, `branches-covered`, `branches-valid` as numbers (`line-rate` is a fraction, not a percentage). This is the mandatory numeric coverage baseline. The run was measured at 54 s on this host for the wrapper form; invoke with the tool's 600000 ms timeout.

- [ ] [P0-T11] Baseline per-file coverage for the eight production files and the `Main` method in `<FEATURE>/evidence/baseline/p0-t11-per-file-coverage.md` (PROC-COV over `coverage/p0-baseline.cobertura.xml`): for each of the eight suffixes record the `Get-FileSummary` line (`valid=`, `covered=`, `uncovered=`); record `Get-MethodLineRate` for suffix `UtilitiesCS\ReusableTypeClasses\Other\StackGeek.cs` method `Main`; for `EtlAsync`, which is `async` and emits no `<method>` element in this pipeline's Cobertura output, record `ETLASYNC_SPAN: 66-130` (declaration line through closing brace, read from the baseline file) and `ETLASYNC_SPAN_RATE: <covered>/<valid>` computed over the `Get-FileLineMap` entries for suffix `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.Etl.cs` whose line number lies in that span. Acceptance: all eight suffixes match at least one `class` element (`valid` greater than 0); `Get-MethodLineRate` for `Main` is a number, not `ABSENT`; the `EtlAsync` span `valid` is greater than 0; the artifact records `BASELINE_UNCOVERED: <suffix>=<n>` for each of the eight, which P7-T6 compares against.

- [ ] [P0-T12] Probe the CI-verbatim AC4 command shape once against the pre-change tree and record `<FEATURE>/evidence/baseline/p0-t12-ci-shape-probe.md` (PROC-TRX): create `coverage/trx/p0-t12`, then run `& $vstest $Assemblies /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p0-t12.trx" /ResultsDirectory:coverage/trx/p0-t12 $Blame "/TestCaseFilter:$FullFilter"` (no `/Settings:`; parallelism from the assembly attribute, as in CI). Acceptance: `coverage/trx/p0-t12/p0-t12.trx` exists; counters recorded; wall-clock recorded as `PROBE_SECONDS: <n>`; the artifact records exactly one of `AC4_COMMAND_SHAPE: CI-VERBATIM` (every failing test, if any, is one of `TryAddValuesAsync_UpdatesExistingValue`, `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`, `Main_RunsSampleScenarioWithoutThrowing`, `DataFramePrettyHelpers_RenderRowsMarkdownAndConsoleOutput`, `PrintTree_WritesIndentedTreeToConsole`, `EnumerateTable_WritesFormattedOutputAndMovesToStart`, i.e. the three failure modes this item repairs) or `AC4_COMMAND_SHAPE: RUNSETTINGS-FALLBACK` (any other failure; the artifact lists the names, and every P8 run then adds `/Settings:TaskMaster.runsettings`, which carries the repo-root coverage module exclusions and class-level parallelism for every assembly, a heavier load than CI). `.coverage` files under `coverage/trx/p0-t12` are deleted after the counters are read.

### Phase 1 — Clock and delegate seams with no behaviour change

- [ ] [P1-T1] Edit `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs`: (a) add a trailing parameter `TimeProvider? timeProvider = null` to `EtlAsync` (signature lines 66-73) preceded by a two-line `//` comment stating that null resolves to the system clock and tests inject `FakeTimeProvider`; (b) delete `var attempts = 3;` at line 82; (c) in the `EtlByRowAsync` call at lines 98-106 replace the `attempts,` argument with `timeProvider,`; (d) line 114 becomes `.TimeoutAfter(milliseconds, timeProvider);`; (e) the log at lines 119-121 becomes `logger.Error($"{nameof(EtlAsync)} timed out with a timeout of {milliseconds} milliseconds. Canceling");` (drops the false retry count and the banned `DateTime.Now`); (f) in the private seven-parameter `EtlByRowAsync` at lines 229-237 replace `int attempts` with `TimeProvider? timeProvider`, and lines 245 and 259 become `.TimeoutAfter(timeout, timeProvider);`. `EtlAsyncOld` (lines 132-169) and the four-parameter `EtlByRowAsync` (line 171) are not edited (D9, D14). Acceptance, by `Select-String -SimpleMatch` counts on this file: `TimeoutAfter(milliseconds, timeProvider)`=1, `TimeoutAfter(timeout, timeProvider)`=2, `TimeoutAfter(milliseconds, attempts)`=1 (the retained `EtlAsyncOld` line), `TimeoutAfter(timeout, attempts)`=0, `var attempts = 3;`=1, `timed out {attempts} times`=1, `DateTime.Now`=1, `TimeProvider? timeProvider = null`=1, `timed out with a timeout of`=1; line count at most 480.

- [ ] [P1-T2] Edit `UtilitiesCS/Extensions/DfDeedle.cs` `GetEmailDataInViewAsync` (lines 139-144): add a trailing parameter `TimeProvider? timeProvider = null` with a two-line `//` comment; line 176 becomes `await AddQfcColumnsAsync(table, currentFolder!, token, 0, timeProvider: timeProvider);`; the `EtlAsync` call at lines 180-185 gains the named argument `timeProvider: timeProvider`; line 208 becomes `.TimeoutAfter(1000, timeProvider);`. No guard is added in this task. Acceptance by counts on this file: `TimeoutAfter(1000, timeProvider)`=1, `TimeoutAfter(1000, 2)`=1 (the retained commented-out line 214), `timeProvider: timeProvider`=2, `TimeProvider? timeProvider = null`=1, `was not produced`=0.

- [ ] [P1-T3] Edit `UtilitiesCS/Extensions/DfDeedle.cs` to replace the two ETL statics: delete lines 62-84 (`TableEtlInvoker` and `StoreTableEtlInvoker` with their doc comments); add, in their place, one `private static readonly Func<object, (object[,] data, Dictionary<string, int> columnInfo)> DefaultTableEtl = t => ((Outlook.Table)t).ETL();` with a doc comment stating it is the production ETL and that `object` avoids CS1769; change `GetEmailDataInView(Explorer activeExplorer)` (line 86) to `GetEmailDataInView(Explorer activeExplorer, Func<object, (object[,] data, Dictionary<string, int> columnInfo)>? etl = null)` and line 94 to `(object[,] data, Dictionary<string, int> columnInfo) = (etl ?? DefaultTableEtl)(table);`. `MessageBoxInvoker` (lines 54-60) is untouched. Acceptance by counts on this file: `TableEtlInvoker`=0, `DefaultTableEtl` at least 2 (the declaration and the use in `GetEmailDataInView`), `(etl ?? DefaultTableEtl)(table)`=1, `MessageBoxInvoker`=1 (the declaration; its `AddQfcColumns` readers live in the other partial); line count at most 320.

- [ ] [P1-T4] Edit `UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs`: `FromDefaultFolder(Store, ...)` (lines 125-130) gains a trailing `Func<object, (object[,] data, Dictionary<string, int> columnInfo)>? etl = null` and line 143 becomes `(var data, var columnInfo) = (etl ?? DefaultTableEtl)(table);`; `FromDefaultFolder(Stores, ...)` (lines 150-155) gains the same trailing parameter and the inner call at lines 160-165 forwards `etl: etl`. Acceptance by counts on this file: `StoreTableEtlInvoker`=0, `DefaultTableEtl`=1, `etl: etl`=1; line count at most 290.

- [ ] [P1-T5] Edit `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` to consume the new seams: add `using Microsoft.Extensions.Time.Testing;`; in `GetEmailDataInView_WithInjectedEtlResult_ReturnsPopulatedFrame` (line 371) delete the seam swap and `try`/`finally` at lines 399-414 and pass `etl: _ => (injectedData, injectedColInfo)` to `GetEmailDataInView`; in `FromDefaultFolder_Store_WithInjectedEtlResult_ReturnsPopulatedFrame` (line 729) delete lines 762-782's swap and pass the same delegate as `etl:`; in `FromDefaultFolder_Stores_FirstStoreHasData_ReturnsNonEmptyFrame` (line 790) delete lines 827-847's swap and pass `etl:`; in `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` (line 423) add the argument `timeProvider: new FakeTimeProvider()` to the call at lines 477-482 and never advance it (every deadline on the path is then armed on a clock that does not move; the assertions at lines 484-488 are unchanged); rewrite the class header lines 20-33 so line 26 no longer names `TableEtlInvoker` and lines 30-32 read that `MessageBoxInvoker is the one remaining static seam`, every test that swaps it restores it in a `finally`, and it has no reader on any parallel-phase path; rewrite the section-banner and arrangement comments at lines 367, 725, 735 and 796, and the header line 26, so they refer to the optional `etl` parameter written without a trailing colon (the `etl:` count below is therefore the three delegate arguments only) and not to `TableEtlInvoker` or `StoreTableEtlInvoker` (the substring search counts both); Assertions in the three converted tests are unchanged. Acceptance by counts on this file: `TableEtlInvoker`=0, `FakeTimeProvider`=1, `etl:`=3, `MessageBoxInvoker is the one remaining static seam`=1; line count strictly less than 869.

- [ ] [P1-T6] Move `ArmingBarrierTimeProvider` out of `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` into the new file `UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs` as a pure extraction: the new file declares namespace `UtilitiesCS.Test.TestHelpers`, `internal sealed class ArmingBarrierTimeProvider : TimeProvider`, and carries lines 39-85 of the source file verbatim except for the access modifier change from `private` to `internal` and the `using` directives it needs (`System`, `System.Threading`, `System.Threading.Tasks`, `Microsoft.Extensions.Time.Testing`); in the source file delete lines 39-85 and add `using UtilitiesCS.Test.TestHelpers;`. No member body changes. Acceptance: `class ArmingBarrierTimeProvider`=0 in the source file and =1 in the new file; `NewSignal` still resolves at the source file's former lines 95 and 103 (build in P1-T8 proves it); the source file's line count is at most 456 and strictly less than 500; the new file's line count is at most 70.

- [ ] [P1-T7] Register the moved helper in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: insert `<Compile Include="TestHelpers\ArmingBarrierTimeProvider.cs" />` immediately before the existing line 74 item `TestHelpers\ManualFireInnerTimer.cs`. Acceptance: `Select-String -SimpleMatch 'TestHelpers\ArmingBarrierTimeProvider.cs'` on the project file counts 1; the file's line count is 983.

- [ ] [P1-T8] Build the solution with the analyzer gate and record `<FEATURE>/evidence/qa-gates/p1-t8-seam-build.md`: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:q "/flp:LogFile=coverage/msbuild-p1-t8.log;Verbosity=normal"`. Acceptance: `EXIT_CODE: 0`; the log contains `    0 Error(s)`; the artifact records the warning count and that it does not exceed `BASELINE_ANALYZER_WARNINGS`. This proves the four in-solution callers named in spec.md (`QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:15,82-89`, `ToDoModel/Data Model/ID/IDList.cs:130,226`) compile unchanged against the optional parameters.

- [ ] [P1-T9] Scoped run of every class the seams touch, recording `<FEATURE>/evidence/regression-testing/p1-t9-seam-scoped-run.md` (PROC-TRX): create `coverage/trx/p1-t9`; run `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p1-t9.trx" /ResultsDirectory:coverage/trx/p1-t9 $Blame "/TestCaseFilter:FullyQualifiedName~DfDeedle_COM_Tests|FullyQualifiedName~DfDeedleQfcColumnTimeoutTests|FullyQualifiedName~OlTableExtensions_Tests|FullyQualifiedName~TimeOutTask_Tests"`. Acceptance: `EXIT_CODE: 0`; counters recorded; `failed`=0; `Read-TrxOutcome` returns `Passed` for `UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests` / `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` and for `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests` / `AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce` (proves the moved barrier still drives the #798 tests).

### Phase 2 — AC2 regression test, fails first

- [ ] [P2-T1] Create `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs` (namespace `UtilitiesCS.Test.Extensions`, `[TestClass]`, `[DoNotParallelize]` with a one-line comment that the class drives a real `Task.Run` gate and shares the `DfDeedle` logger like `DfDeedleQfcColumnTimeoutTests`, MSTest + Moq + FluentAssertions, `using UtilitiesCS.Test.TestHelpers;`, `using Microsoft.Extensions.Time.Testing;`) containing private builders that replicate `DfDeedle_COM_Tests` lines 425-476 (one-row strict `Table`, five columns `EntryID`/`MessageClass`/`SentOn`/`ConversationId`/`Triage`, `Row.BinaryToString(4)`, folder built with a `Triage` user-defined property and `Name` returning `Inbox`, strict `TableView` and `Explorer`, `ProgressTracker` over `SilentProgressTracker`) and two tests: (1) `GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder`, arranged per D5 with `var barrier = new ArmingBarrierTimeProvider(new FakeTimeProvider())`, gate A on `Columns.Add("SentOn")` (a `ManualResetEventSlim` waited inside the `Returns` callback), gate B on `GetNextRow()`; act: start the call with `timeProvider: barrier`, `await barrier.Armed`, `barrier.ReArm()`, `gateA.Set()`, `await barrier.Armed`, `barrier.Advance(250)`; assert `await FluentActions.Awaiting(() => call).Should().ThrowAsync<InvalidOperationException>()` with `.WithMessage("*Inbox*")`; `finally { gateA.Set(); gateB.Set(); }`; (2) `GetEmailDataInViewAsync_ClockNeverAdvances_ReturnsOneRowFrame` with an un-advanced `new FakeTimeProvider()`, no gate engaged, asserting `RowCount` is 1 and `ColumnKeys` contains `EntryId`, `MessageClass`, `ConversationId`. No sleep, delay, retry, or timed wait anywhere in the file. Acceptance: the file exists; `Select-String -SimpleMatch` counts on it: `[TestMethod]`=2, `ArmingBarrierTimeProvider`≥2, `Advance(250)`=1, `ThrowAsync<InvalidOperationException>`=1, `Thread.Sleep`=0, `Task.Delay`=0, `CancelAfter`=0, `WaitOne`=0, and with regex `\.Wait\(\s*(\d|TimeSpan)`=0 (the two gates are waited as `gate.Wait()` with no timeout argument, which that regex does not match); line count at most 300.

- [ ] [P2-T2] Register the new test file in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: insert `<Compile Include="Extensions\DfDeedleEtlTimeoutTests.cs" />` immediately after the item `Extensions\DfDeedleQfcColumnTimeoutTests.cs` (line 190 in the base tree; line 191 after P1-T7's insertion). Acceptance: `Select-String -SimpleMatch 'Extensions\DfDeedleEtlTimeoutTests.cs'` on the project file counts 1; line count 984.

- [ ] [P2-T3] Build the test project to produce the assembly, recording `<FEATURE>/evidence/regression-testing/p2-t3-red-build.md`: `& $msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:q "/flp:LogFile=coverage/msbuild-p2-t3.log;Verbosity=normal"`. Acceptance: `EXIT_CODE: 0`; the log contains `    0 Error(s)`; `Test-Path UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` prints `True` and its `LastWriteTime` is later than the task start.

- [ ] [P2-T4] [expect-fail] Run the new class and record the fail-before evidence in `<FEATURE>/evidence/regression-testing/p2-t4-ac2-fail-before.md` (PROC-TRX): create `coverage/trx/p2-t4`; run `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p2-t4.trx" /ResultsDirectory:coverage/trx/p2-t4 $Blame "/TestCaseFilter:FullyQualifiedName~DfDeedleEtlTimeoutTests"`. Acceptance: `EXIT_CODE: 1` with `ExpectedExitCode: 1`; counters `total`=2, `passed`=1, `failed`=1; `Read-TrxOutcome` for class `UtilitiesCS.Test.Extensions.DfDeedleEtlTimeoutTests` method `GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder` is `Failed` and the TRX text contains `System.NullReferenceException` inside that result's `ErrorInfo/Message` (FluentAssertions reports the found exception type); method `GetEmailDataInViewAsync_ClockNeverAdvances_ReturnsOneRowFrame` is `Passed`; the blame hang timeout did not fire (no `Sequence` file under `coverage/trx/p2-t4`); the artifact quotes the failure message verbatim after redaction.

### Phase 3 — AC2 guard, passes after

- [ ] [P3-T1] Insert the null-snapshot guard in `UtilitiesCS/Extensions/DfDeedle.cs` between the `EtlAsync` call (ends at the current line 185 region) and the `LogDfTiming(` call (D2): `if (tableSnapshot.data is null) { throw new InvalidOperationException($"The table snapshot for folder '{folderName}' was not produced: the table ETL timed out or was cancelled before returning any rows, so the email data frame cannot be built for this folder."); }` split across lines as CSharpier requires, preceded by a two-line `//` comment that `EtlAsync` swallows its `TimeoutException` and returns a null `data` through a null-forgiving suppression, so this is the first point the failure can be named. `folderName` is already in scope (line 166). The existing `ValidateRequiredEmailColumns` guard (line 195) is untouched and now runs after this one. Acceptance by counts on this file: `was not produced`=1, `tableSnapshot.data is null`=1, `#nullable enable`=1; line count at most 328.

- [ ] [P3-T2] Build and re-run the class, recording `<FEATURE>/evidence/regression-testing/p3-t2-ac2-pass-after.md` (PROC-TRX): `& $msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:q "/flp:LogFile=coverage/msbuild-p3-t2.log;Verbosity=normal"`, then `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p3-t2.trx" /ResultsDirectory:coverage/trx/p3-t2 $Blame "/TestCaseFilter:FullyQualifiedName~DfDeedleEtlTimeoutTests|FullyQualifiedName~DfDeedle_COM_Tests"`. Acceptance: build `EXIT_CODE: 0` with `    0 Error(s)`; test `EXIT_CODE: 0`; `failed`=0; `Read-TrxOutcome` is `Passed` for both `DfDeedleEtlTimeoutTests` methods and for `DfDeedle_COM_Tests` / `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`; the artifact names `p2-t4-ac2-fail-before.md` as the paired fail-before record.

- [ ] [P3-T3] Check off AC2 in `<FEATURE>/spec.md` line 293 (`- [ ] AC2:` becomes `- [x] AC2:`, text unchanged), citing `p1-t9-seam-scoped-run.md` (static seams replaced; `TableEtlInvoker`=0 in production and test files) and `p3-t2-ac2-pass-after.md` (guard). Acceptance: `Select-String -SimpleMatch '- [x] AC2:'` on spec.md counts 1 and `'- [ ] AC2:'` counts 0; no other line of spec.md changed (`git diff --numstat HEAD -- docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/spec.md` reports 1 insertion and 1 deletion (`HEAD` is the anchor because spec.md was created on this branch after `$Base` and is absent there; no commit occurs before P7-T12)).

### Phase 4 — AC1 wall-clock window deletion

- [ ] [P4-T1] Edit `UtilitiesCS/Extensions/DictionaryExtensions.cs` `TryAddValuesAsync` (lines 169-180): delete lines 176-177 (`CreateLinkedTokenSource` and `CancelAfter(500)`) and make the body `return await Task.Run(() => dictionary.TryAddValues(key, value), token);`, preceded by a two-line `//` comment that the body is a bounded compare-and-swap loop and cancellation is governed solely by the caller's token. Acceptance by counts on this file: `CancelAfter(`=0, `CreateLinkedTokenSource`=0, `TryAddValues(key, value), token)`=1; line count at most 282.

- [ ] [P4-T2] Add the contract-lock test to `UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs` after `TryAddValuesAsync_UpdatesExistingValue` (line 249): `TryAddValuesAsync_PreCancelledToken_ThrowsTaskCanceledAndLeavesValueUnchanged` — arrange a `ConcurrentDictionary<string, int>` with `["value"] = 8` and a `CancellationTokenSource` that is cancelled before the call; act `Func<Task> act = () => dictionary.TryAddValuesAsync("value", 2, cts.Token);`; assert `await act.Should().ThrowAsync<TaskCanceledException>()` and `dictionary["value"].Should().Be(8)`. `TryAddValuesAsync_UpdatesExistingValue` is retained unchanged. Acceptance by counts on this file: `TryAddValuesAsync_PreCancelledToken_ThrowsTaskCanceledAndLeavesValueUnchanged`=1, `TryAddValuesAsync_UpdatesExistingValue`=1, `[TestMethod]` increased by exactly 1 over the baseline count recorded in this task's own before-reading; line count at most 320.

- [ ] [P4-T3] Write the AC1 fail-before exception dossier `<FEATURE>/evidence/regression-testing/fail-before-exception.2026-09-08T00-00.md` (timestamp replaced by the actual ISO value at authoring; filename prefix `fail-before-exception.` is mandatory) with `Timestamp:`, `WhyFailingRunImpossible:` (the #780 failure is thread-pool scheduling latency above 500 ms under 24-worker coverage load; the only way to force it is a sleep or a deliberately starved pool, both forbidden by AC5 and the unit-test policy), and an alternative proof section containing: the P0-T6 count `CancelAfter(`=1 and `CreateLinkedTokenSource`=1 before P4-T1, the post-edit counts 0 and 0 measured by this task, and the statement that `TryAddValuesAsync` has zero production call sites (D3). Acceptance: the file exists at that path pattern; both `WhyFailingRunImpossible:` and `SearchScope:` lines are present (`SearchScope:` names `<FEATURE>/evidence/regression-testing/`, `SearchPatterns: fail-before-exception.*.md`, `SearchResult:` names this file).

- [ ] [P4-T4] Build and run the dictionary class, recording `<FEATURE>/evidence/regression-testing/p4-t4-ac1-pass-after.md` (PROC-TRX): `& $msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:q "/flp:LogFile=coverage/msbuild-p4-t4.log;Verbosity=normal"`, then `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p4-t4.trx" /ResultsDirectory:coverage/trx/p4-t4 $Blame "/TestCaseFilter:FullyQualifiedName~DictionaryExtensions_Tests"`. Acceptance: build exit 0 with `    0 Error(s)`; test `EXIT_CODE: 0`; `failed`=0; `Read-TrxOutcome` is `Passed` for class `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests` methods `TryAddValuesAsync_UpdatesExistingValue` and `TryAddValuesAsync_PreCancelledToken_ThrowsTaskCanceledAndLeavesValueUnchanged`; the duration of `TryAddValuesAsync_UpdatesExistingValue` read from its `UnitTestResult/@duration` is under one second.

- [ ] [P4-T5] Check off AC1 in `<FEATURE>/spec.md` line 292, citing `fail-before-exception.<timestamp>.md` and `p4-t4-ac1-pass-after.md`; the "passes deterministically under 24-worker parallel coverage runs" clause is finally evidenced by P8-T6 and this check-off records that dependency in the plan artifact `<FEATURE>/evidence/other/p4-t5-ac1-checkoff-note.md`. Acceptance: `'- [x] AC1:'` counts 1 and `'- [ ] AC1:'` counts 0 in spec.md; the note artifact exists and names P8-T6.

### Phase 5 — AC3 TextWriter seams

- [ ] [P5-T1] Edit `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs`: add `using System.IO;`; change `PrintTree(TreeNode<string> node, int level)` (line 97) to `PrintTree(TreeNode<string> node, int level, TextWriter? writer = null)`; line 99 writes through `(writer ?? Console.Out).WriteLine(...)`; the recursion at line 102 forwards `writer`. Acceptance by counts on this file: `TextWriter? writer = null`=1, `writer ?? Console.Out`=1, `PrintTree(child, level + 1, writer)`=1, `Console.WriteLine`=0; line count at most 126.

- [ ] [P5-T2] Edit `UtilitiesCS/HelperClasses/PrettyPrint.cs` with zero net line growth (D10): add `using System.IO;` after line 2; delete line 6 `using System.Threading.Tasks;`, line 12 `using System.Text;` (the namespace-scoped duplicate) and line 18 `using Svg;`; change line 25 to `public static void PrettyPrint(this DataFrame df, TextWriter? writer = null) => (writer ?? Console.Out).WriteLine(PrettyText(df));` and line 27 to `public static void PrettyPrint(this DataFrameRow row, TextWriter? writer = null) => (writer ?? Console.Out).WriteLine(Pretty(row));` (each wraps to two lines under CSharpier). Acceptance by counts on this file: `TextWriter? writer = null`=2, `writer ?? Console.Out`=2, `using Svg;`=0, `using System.Threading.Tasks;`=0, `using System.Text;`=1; line count at most 680 measured both now and again after P7-T1.

- [ ] [P5-T3] Edit `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`: add `using System.IO;`; change `EnumerateTable(this Outlook.Table table)` (line 381) to `EnumerateTable(this Outlook.Table table, TextWriter? writer = null)`; introduce `var target = writer ?? Console.Out;` and route the three writes at lines 423-425 through `target.WriteLine(...)`. Lines 78 and 96 (`Console.WriteLine($"Task timed out on try {counter}")` inside `GetTableInViewAsync`) are not edited; they are recorded as a follow-up in P8-T9. Acceptance by counts on this file: `TextWriter? writer = null`=1, `writer ?? Console.Out`=1, `Console.WriteLine`=2 (lines 78 and 96 retained), `target.WriteLine`=3; line count at most 436.

- [ ] [P5-T4] Edit `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs`: add `public static void Run(System.IO.TextWriter writer)` to class `GFG` carrying the body of `Main` (lines 175-191) with the four `Console.WriteLine` calls at lines 187-191 replaced by `writer.WriteLine(...)`; `Main(String[] args)` becomes a one-statement body `Run(Console.Out);`. The three `Console.WriteLine("Stack is empty...")` calls at lines 96, 127 and 137 are inside `pop`/`findMiddle`/`deleteMiddle` and are not part of the seam; they are untouched. Acceptance by counts on this file: `public static void Run(`=1, `Run(Console.Out)`=1, `writer.WriteLine`=4, `Console.WriteLine`=3; line count at most 208.

- [ ] [P5-T5] Edit `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs`: delete the comment block and attribute at lines 9-15 (`[DoNotParallelize]` and its console rationale); rewrite `Main_RunsSampleScenarioWithoutThrowing` (lines 146-168) to invoke `helper.InvokeMain(Array.Empty<string>())` inside `FluentActions.Invoking(...).Should().NotThrow()` with no `Console.Out` capture (it exercises the `Run(Console.Out)` default); add `Run_WritesScenarioToSuppliedWriter`: add `using UtilitiesCS;` to the file's directives, then `using var writer = new StringWriter(); GFG.Run(writer);` (the class is `internal`; `UtilitiesCS/Properties/AssemblyInfo.cs:19` grants `InternalsVisibleTo("UtilitiesCS.Test")`), assert `writer.ToString()` contains `Middle Element :` and `New Middle Element :`. Acceptance by counts on this file: `Console.SetOut`=0, `originalOut`=0, `DoNotParallelize`=0, `Run_WritesScenarioToSuppliedWriter`=1, `Main_RunsSampleScenarioWithoutThrowing`=1, `using UtilitiesCS;`=1; line count at most 285.

- [ ] [P5-T6] Edit `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs`: delete lines 14-19 (comment and `[DoNotParallelize]`); rename `DataFramePrettyHelpers_RenderRowsMarkdownAndConsoleOutput` (line 186) to `DataFramePrettyHelpers_RenderRowsMarkdownAndWriterOutput`, delete the `Console.Out` save/set/restore together with the `try` construct that exists only to host it (lines 194 and 196; the `try` and its opening brace at lines 198-199; the closing brace and `finally` block at lines 215-219), de-indenting the former `try` body one level, and pass `writer` to `frame.PrettyPrint(writer)` and `row.PrettyPrint(writer)`; assertions unchanged; add `PrettyPrint_NullWriter_WritesToConsoleWithoutThrowing` asserting `FluentActions.Invoking(() => { frame.PrettyPrint(); row.PrettyPrint(); }).Should().NotThrow()`. Acceptance by counts on this file: `Console.SetOut`=0, `originalOut`=0, `DoNotParallelize`=0, `PrettyPrint(writer)`=2, `PrettyPrint_NullWriter_WritesToConsoleWithoutThrowing`=1; line count at most 420.

- [ ] [P5-T7] Edit `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs`: delete lines 8-14 (comment and `[DoNotParallelize]`); rename `PrintTree_WritesIndentedTreeToConsole` (line 102) to `PrintTree_WritesIndentedTreeToSuppliedWriter`, delete the save/set/restore together with its `try` construct (lines 108-109; the `try` and its opening brace at lines 111-112; the closing brace and `finally` block at lines 115-119), de-indenting the former body one level, and call `parser.PrintTree(tree, 0, writer)`; the assertion at line 122 is unchanged. The null-writer path of `PrintTree` is already exercised by `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs:142`. Acceptance by counts on this file: `Console.SetOut`=0, `originalOut`=0, `DoNotParallelize`=0, `PrintTree(tree, 0, writer)`=1; line count at most 118.

- [ ] [P5-T8] Edit `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` for the console seam only: replace the comment at lines 17-19 with one stating that the console reason was removed by the `TextWriter` seam under #811 and the attribute is retained because the class has `not been soaked under class-level parallelism` and ten of its tests drive the 2000 ms `GetTableInViewAsync` window; keep `[DoNotParallelize]` at line 20 (D8); rename `EnumerateTable_WritesFormattedOutputAndMovesToStart` to `EnumerateTable_WritesFormattedOutputToSuppliedWriterAndMovesToStart`, delete the save/set/restore at lines 1636 and 1638-1646 keeping line 1641 as `mockTable.Object.EnumerateTable(output);`; assertions at lines 1648-1651 unchanged; after line 1650 add one `//` comment line and the single line `FluentActions.Invoking(() => mockTable.Object.EnumerateTable()).Should().NotThrow();` (the null-writer path resolving to `Console.Out`, asserted in the same test because the file is at its line ceiling and cannot take a second method); the existing `mockTable.Verify(t => t.MoveToStart(), Times.AtLeastOnce)` then covers both calls. No new test method is added to this file; the rewritten comment at lines 17-19 occupies at most three lines. Acceptance by counts on this file: `Console.SetOut`=0, `redirects Console.Out`=0, `not been soaked under class-level parallelism`=1, `DoNotParallelize`=1, `EnumerateTable(output)`=1, `EnumerateTable())`=1, `EnumerateTable_NullWriter`=0; line count at most 1848.

- [ ] [P5-T9] Edit `UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs`: delete the field at line 17, the save and install at lines 22-23, and the `[TestCleanup]` method at lines 53-57. `PrintLogCall` (lines 46-51) keeps writing to `Console` (whatever writer the process holds; no test asserts on it). Acceptance by counts on this file: `Console.SetOut`=0, `originalOut`=0, `TestCleanup`=0, `PrintLogCall`≥2; line count at most 112.

- [ ] [P5-T10] Build the solution with the analyzer gate and run the five converted classes plus the Triage caller, recording `<FEATURE>/evidence/regression-testing/p5-t10-ac3-pass-after.md` (PROC-TRX): `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:q "/flp:LogFile=coverage/msbuild-p5-t10.log;Verbosity=normal"` (this is where the D10 substitution rule is decided: a CS0103/CS0246 in `PrettyPrint.cs` triggers it, recorded here), then `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p5-t10.trx" /ResultsDirectory:coverage/trx/p5-t10 $Blame "/TestCaseFilter:FullyQualifiedName~StackGeek_Tests|FullyQualifiedName~PrettyPrint_Tests|FullyQualifiedName~DASLFilterParserTests|FullyQualifiedName~OlTableExtensions_Tests|FullyQualifiedName~NLogTraceWriter_Test|FullyQualifiedName~Triage_OlLogicTests"`. Acceptance: build exit 0 with `    0 Error(s)` (proves `TaskMaster/Ribbon/TryFunctionalityInConstruction.cs:51,66` and `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs:74` compile unchanged); test `EXIT_CODE: 0`; `failed`=0; `Read-TrxOutcome` is `Passed` for the three renamed tests, `Main_RunsSampleScenarioWithoutThrowing`, `Run_WritesScenarioToSuppliedWriter`, and `PrettyPrint_NullWriter_WritesToConsoleWithoutThrowing` (six names); `Select-String -SimpleMatch 'Console.SetOut'` over the five test files of this phase totals 0.

- [ ] [P5-T11] Check off AC3 in `<FEATURE>/spec.md` line 294, citing `p5-t10-ac3-pass-after.md`. Acceptance: `'- [x] AC3:'` counts 1 and `'- [ ] AC3:'` counts 0 in spec.md.

### Phase 6 — AC5 tolerance retirement and ETL clock tests

- [ ] [P6-T1] Edit `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` `EtlAsync_WithBinaryAndObjectFieldsAndProgress_ReturnsTransformedData` (line 947): add `using Microsoft.Extensions.Time.Testing;`; delete lines 960-963 (the comment and `mockTable.Setup(t => t.GetRowCount()).Returns(120);`), so `CreateTableWithColumns` (line 1780) supplies the natural row count of 1 and the deadline is 250 ms; add the trailing argument `timeProvider: new FakeTimeProvider()` to the `EtlAsync` call at lines 971-977 and never advance it. Assertions at lines 979-983 unchanged. Acceptance by counts on this file: `Returns(120)`=0, `cannot fire under test-host`=0, `FakeTimeProvider`=1, `using Microsoft.Extensions.Time.Testing;`=1.

- [ ] [P6-T2] Re-verify the reflection binding in `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` (originally lines 1124-1137) against the edited `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs`, recording `<FEATURE>/evidence/other/p6-t2-reflection-binding-check.md`: quote the `InvokeStaticAsync<IAsyncEnumerable<object[]>>("EtlByRowAsync", new[] { typeof(Outlook.Table), typeof(Dictionary<string, Func<object, string>>), typeof(Dictionary<string, int>), typeof(CancellationToken) }, ...)` span and the current signature line of the four-parameter private `EtlByRowAsync` in Etl.cs. Acceptance: the four-parameter overload's parameter list is `Table table, Dictionary<string, Func<object, string>>? objectConverters, Dictionary<string, int> columnDictionary, CancellationToken token` (unchanged); the seven-parameter overload's list ends `TimeProvider? timeProvider, ProgressTracker? progress = null`; the artifact records `REFLECTION_BINDING: UNCHANGED`.

- [ ] [P6-T3] Create `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs` (namespace `UtilitiesCS.Test.OutlookObjects.Table`, `[TestClass]`, `[DoNotParallelize]` with a one-line comment that it drives a real `Task.Run` gate, `using UtilitiesCS.Test.TestHelpers;`, `using Microsoft.Extensions.Time.Testing;`) with private builders replicating `OlTableExtensions_Tests.CreateTableWithColumns` (lines 1757-1793) and `CreateRowMock` (lines 1723-1750) for the three columns `MessageRecipients`, `Store`, `Subject` (so `Store`, a `BinaryToStringFields` member, selects the `EtlByRowAsync` branch) and two tests: (1) `EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource` — `GetNextRow()` blocks on a gate, `var barrier = new ArmingBarrierTimeProvider(new FakeTimeProvider())`, act: start `EtlAsync(CancellationToken.None, tokenSource, 0, null, converters, barrier)`, `await barrier.Armed`, `barrier.Advance(250)`, await the result; assert `data` is null and `tokenSource.IsCancellationRequested` is true; `finally { gate.Set(); }`; (2) `EtlAsync_ClockNeverAdvances_ReturnsTransformedRows` with an un-advanced `FakeTimeProvider`, asserting `columnInfo["Store"]` is 1, `data[0, 1]` equals the injected binary string and `tokenSource.IsCancellationRequested` is false. Acceptance: file exists; counts: `[TestMethod]`=2, `Advance(250)`=1, `Thread.Sleep`=0, `Task.Delay`=0, `CancelAfter`=0, `WaitOne`=0; line count at most 220.

- [ ] [P6-T4] Register the new test file in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: insert `<Compile Include="OutlookObjects\Table\OlTableExtensionsEtlClockTests.cs" />` immediately after the item `OutlookObjects\Table\OlTableExtensions_Tests.cs` (originally line 538). Acceptance: `Select-String -SimpleMatch 'OutlookObjects\Table\OlTableExtensionsEtlClockTests.cs'` counts 1; the project file's line count is 985.

- [ ] [P6-T5] Build the test project and run the two table classes, recording `<FEATURE>/evidence/regression-testing/p6-t5-etl-clock-pass.md` (PROC-TRX): `& $msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:q "/flp:LogFile=coverage/msbuild-p6-t5.log;Verbosity=normal"`, then `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p6-t5.trx" /ResultsDirectory:coverage/trx/p6-t5 $Blame "/TestCaseFilter:FullyQualifiedName~OlTableExtensions_Tests|FullyQualifiedName~OlTableExtensionsEtlClockTests"`. Acceptance: build exit 0 with `    0 Error(s)`; test `EXIT_CODE: 0`; `failed`=0; `Read-TrxOutcome` is `Passed` for both `OlTableExtensionsEtlClockTests` methods, for `EtlAsync_WithBinaryAndObjectFieldsAndProgress_ReturnsTransformedData`, and for the reflection test containing `EtlByRowAsync` at the originally cited lines (its method name is read from the file and recorded).

### Phase 7 — Final QA loop, coverage delta, audits, source commit

The loop is P7-T1 through P7-T5. If any step fails or P7-T1 rewrites a file, fix the cause and restart from P7-T1; the artifacts of the final clean pass carry `Toolchain pass: <n>`.

- [ ] [P7-T1] Toolchain step 1 in `<FEATURE>/evidence/qa-gates/p7-t1-csharpier-format.md`: record PROC-HASH over the 19 write-set `.cs` files and `git status --porcelain --untracked-files=all -- "*.cs" ":(exclude).claude"` before; run `& $dotnet tool run csharpier format .`; record both observations again after. Acceptance: `EXIT_CODE: 0`; `REWRITTEN_COUNT: <n>` defined as the number of files whose hash differs plus the number of porcelain entries that appeared, never read from the `Formatted N files` line; when `REWRITTEN_COUNT` is greater than 0 the loop restarts at P7-T1 and the final artifact shows `REWRITTEN_COUNT: 0`; `PrettyPrint.cs` line count at most 680 after the pass.

- [ ] [P7-T2] Toolchain step 1 verification in `<FEATURE>/evidence/qa-gates/p7-t2-csharpier-check.md`: run `& $dotnet tool run csharpier check .`. Acceptance: `EXIT_CODE: 0`; the summary line is quoted verbatim and begins with `Checked ` and ends with `ms.`.

- [ ] [P7-T3] Toolchain step 2 in `<FEATURE>/evidence/qa-gates/p7-t3-msbuild-analyzers.md`: run the P0-T8 command with `"/flp:LogFile=coverage/msbuild-p7-t3.log;Verbosity=normal"`. Acceptance: `EXIT_CODE: 0`; the log contains `    0 Error(s)`; the warning count does not exceed `BASELINE_ANALYZER_WARNINGS` from P0-T8, and any warning present is enumerated by diagnostic id.

- [ ] [P7-T4] Toolchain step 3 in `<FEATURE>/evidence/qa-gates/p7-t4-msbuild-nullable.md`: run the P0-T9 command with `"/flp:LogFile=coverage/msbuild-p7-t4.log;Verbosity=normal"`. Acceptance: `EXIT_CODE: 0`; the log contains `    0 Error(s)`.

- [ ] [P7-T5] Toolchain step 4, full nine-assembly coverage run, in `<FEATURE>/evidence/qa-gates/p7-t5-vstest-coverage.md` (PROC-TRX): create `coverage/trx/p7-t5`; run `dotnet-coverage collect --output coverage/p7-final.cobertura.xml --output-format cobertura --settings coverage.config -- $vstest $Assemblies /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/Logger:trx;LogFileName=p7-t5.trx" /ResultsDirectory:coverage/trx/p7-t5 $Blame "/TestCaseFilter:$FullFilter"`. Acceptance: `EXIT_CODE: 0`; counters recorded; `failed`+`error`+`aborted`+`timeout`=0 (any failure, including one of the retained sentinels, is a defect: fix and restart at P7-T1; no re-run-until-green is permitted and the artifact records `RERUN_COUNT: 0`); `total` is greater than the P0-T10 `total` by exactly 7 (the seven new tests in C4; a gap-closure test added by P7-T7 raises this expectation by its count and the artifact of the final pass states the adjusted figure); root `line-rate`, `lines-covered`, `lines-valid`, `branch-rate`, `branches-covered`, `branches-valid` recorded as numbers; `Read-TrxOutcome` is `Passed` for all nine C4 names.

- [ ] [P7-T6] Coverage delta in `<FEATURE>/evidence/qa-gates/p7-t6-coverage-delta.md` (PROC-COV over `coverage/p0-baseline.cobertura.xml` and `coverage/p7-final.cobertura.xml`, PROC-CHANGED against `$Base`): for each of the eight production suffixes record baseline and final `valid=`/`covered=`/`uncovered=`; for each, list every post-image added line (from `git diff --unified=0 $Base -- <file>`; all eight files are tracked, so no staging companion is needed here) with `hits=<n>` or `hits=non-executable` when the final map has no entry for that line; record `Get-MethodLineRate` for `StackGeek.cs` method `Run`, and the `EtlAsync` span rate per the P0-T11 rule with the final span read from the edited file (the line containing `> EtlAsync(` through the first later line that is exactly eight spaces followed by `}`); record the repository-wide comparison. Acceptance, every clause: (1) for each of the eight files `uncovered` final is at most `uncovered` baseline; (2) every added executable line reports `hits` greater than 0; (3) `Run` line-rate is at least 0.90 (new method, CLAUDE.md UT2); (4) the `EtlAsync` span rate final (covered divided by valid) is at least the P0-T11 `ETLASYNC_SPAN_RATE` (the expiry test newly covers the `catch` block), with both spans recorded; (5) repository-wide: exactly one of `COMPARABILITY: A` (final `lines-valid` within 1 percent of baseline `lines-valid`; then final `line-rate` is at least baseline minus 0.005) or `COMPARABILITY: B` (denominators differ by more than 1 percent; the two rates are recorded and not gated, with one sentence saying why) is named; (6) the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with the numeric headline for both documents.

- [ ] [P7-T7] Gap closure for P7-T6 in `<FEATURE>/evidence/qa-gates/p7-t7-gap-closure.md`: if every P7-T6 clause passed, record `GAP CLOSURE: NOT REQUIRED` and touch nothing. Otherwise add the smallest test that exercises each uncovered added line, in the write-set test file that owns the production file (`DfDeedleEtlTimeoutTests.cs` or `OlTableExtensionsEtlClockTests.cs` for the two seam files; `DictionaryExtensions_Tests.cs`, `StackGeek_Tests.cs`, `PrettyPrint_Tests.cs`, `DASLFilterParserTests.cs`, `OlTableExtensions_Tests.cs` for the rest; never `DfDeedleQfcColumnTimeoutTests.cs`), name each added test in the artifact, and restart the loop at P7-T1. Acceptance: the artifact exists and its final state records either `NOT REQUIRED` or the list of added fully-qualified test names together with a passing re-run of P7-T5 and P7-T6.

- [ ] [P7-T8] File-size audit in `<FEATURE>/evidence/qa-gates/p7-t8-line-cap.md`: `(Get-Content).Count` for the 19 write-set `.cs` files after the final format pass. Acceptance: every file other than the three pre-existing violations is at most 500; `UtilitiesCS/HelperClasses/PrettyPrint.cs` is at most 680; `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` is strictly less than 869; `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` is at most 1848 (the P5-T8 ceiling; P6-T1 is a further net −2); `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` is strictly less than 500; `UtilitiesCS/Threading/TimeOutTask.cs` is exactly 1011 (untouched); the project file is recorded as exempt from the cap.

- [ ] [P7-T9] Write-set scope audit in `<FEATURE>/evidence/qa-gates/p7-t9-write-set-diff.md`: run `git add --intent-to-add -- UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs`, then `git diff --name-only $Base -- "*.cs" "*.csproj" "*.config" "*.props" "*.targets" "*.runsettings" "*.yml" ":(exclude).claude"` and `git status --porcelain --untracked-files=all -- . ":(exclude).claude"`. Acceptance: the name-only list, sorted, equals exactly the 20 Write Set paths (no fewer: every one is touched; no more: no path outside the set); the porcelain output lists no path outside the 20 paths plus `<FEATURE>/` plus `docs/features/potential/`; `git diff --numstat $Base -- UtilitiesCS/Threading/TimeOutTask.cs UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` is empty.

- [ ] [P7-T10] AC5 timing-hack search in `<FEATURE>/evidence/qa-gates/p7-t10-ac5-timing-hack-search.md`: over `git diff $Base -- "*.cs" ":(exclude).claude"` keep only added lines (`^\+` and not `^\+\+\+`) and count, with `Select-String -SimpleMatch`, each of: `Thread.Sleep`, `Task.Delay`, `CancelAfter`, `WaitOne`, `Returns(120)`, `SpinWait`, `Stopwatch`; count with regex the shapes `new CancellationTokenSource\(\s*\d` (a numeric timeout), `\.Wait\(\s*(\d|TimeSpan)` (a timed wait), `catch \(` restricted to hunks of files under `UtilitiesCS.Test/` (a retry loop needs a catch; the new tests use `finally` only), and `Returns\(\s*\d{2,}\s*\)` on a line that also contains `GetRowCount` (a mock value chosen to outrun a deadline). Acceptance: every count is 0 (the production files' existing stopwatches are not added lines, so `Stopwatch` is 0 as well); the artifact lists each pattern with its count; a separate `RETIRED:` line records that `Returns(120)` and `cannot fire under test-host` count 0 in the final tree of `OlTableExtensions_Tests.cs`; a `RETAINED_GATES:` line records that `gate.Wait()` (no argument) occurrences in the two new test files are release-by-test gates, not timed waits, with their count.

- [ ] [P7-T11] Check off AC5 in `<FEATURE>/spec.md` line 296, citing `p7-t10-ac5-timing-hack-search.md`. Acceptance: `'- [x] AC5:'` counts 1 and `'- [ ] AC5:'` counts 0 in spec.md.

- [ ] [P7-T12] Source commit recording `<FEATURE>/evidence/other/p7-t12-source-commit.md`: `git add -A -- . ":(exclude).claude"`, then `git commit -m "fix(811): make UtilitiesCS.Test deterministic under parallel coverage (AC1-AC3, AC5)"` with a body listing the three defects; after the commit, write the artifact with `git rev-parse HEAD` on its own line as `SOURCE-HEAD: <sha>` (the artifact is therefore the one untracked path until P8-T12 commits it). Acceptance: exit 0; `git status --porcelain --untracked-files=all -- . ":(exclude).claude"` lists exactly one path, this task's artifact; `git status --porcelain -- "*.cs" "*.csproj"` is empty; `git diff --name-only $Base..HEAD -- "*.cs" "*.csproj"` lists exactly the 20 Write Set paths; the commit message contains no absolute path, account name or machine name.

### Phase 8 — AC4 ten-run gate, sanitisation, follow-ups, final commit

Every run in P8-T1 through P8-T5 uses this exact shape, with `$Shape` read from `p0-t12-ci-shape-probe.md` (`Select-String -Pattern '^AC4_COMMAND_SHAPE: (CI-VERBATIM|RUNSETTINGS-FALLBACK)$'`): base arguments `$Assemblies /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=<task>-run<k>.trx" /ResultsDirectory:coverage/trx/<task> $Blame "/TestCaseFilter:$FullFilter"`, plus `/Settings:TaskMaster.runsettings` only when `$Shape` is `RUNSETTINGS-FALLBACK`. Each pair task: (a) records `git rev-parse HEAD` and asserts it equals `SOURCE-HEAD` from P7-T12 and that `git status --porcelain -- "*.cs" "*.csproj"` is empty (the ten runs are consecutive on one unchanged tree); (b) runs the two runs back to back inside one `pwsh -NoProfile -File coverage/plan811-helper.ps1` invocation with the tool timeout set to 600000 ms (expected 2 to 6 minutes per pair; `PROBE_SECONDS` from P0-T12 times two is the estimate); (c) reads both TRX counters with PROC-TRX; (d) deletes every `*.coverage` file under its results directory after reading; (e) writes `<FEATURE>/evidence/regression-testing/<task>-ac4-runs.md` with `Timestamp:`, `Command:`, `EXIT_CODE:` (the second run's, and the first run's on a separate line), `Output Summary:`. If the invocation is cut off by the tool timeout, the missing TRX is `RUN_INCOMPLETE`; the task is re-run once as two single-run invocations of the same command, and the artifact records the split. Acceptance for each pair task: both TRX files exist; both `failed`=0, `error`=0, `aborted`=0, `timeout`=0, `executed`=`total`; both `total` values equal the P7-T5 `total`; both exit codes are 0.

- [ ] [P8-T1] AC4 runs 1 and 2 into `coverage/trx/p8-t1` (`p8-t1-run1.trx`, `p8-t1-run2.trx`), artifact `<FEATURE>/evidence/regression-testing/p8-t1-ac4-runs.md`. Acceptance: the pair acceptance above; additionally `Read-TrxOutcome` on `p8-t1-run1.trx` is `Passed` for all nine C4 names (the non-vacuity control: an unregistered or undiscovered test would read `ABSENT`, and `ABSENT` fails this clause).

- [ ] [P8-T2] AC4 runs 3 and 4 into `coverage/trx/p8-t2`, artifact `<FEATURE>/evidence/regression-testing/p8-t2-ac4-runs.md`. Acceptance: the pair acceptance.

- [ ] [P8-T3] AC4 runs 5 and 6 into `coverage/trx/p8-t3`, artifact `<FEATURE>/evidence/regression-testing/p8-t3-ac4-runs.md`. Acceptance: the pair acceptance.

- [ ] [P8-T4] AC4 runs 7 and 8 into `coverage/trx/p8-t4`, artifact `<FEATURE>/evidence/regression-testing/p8-t4-ac4-runs.md`. Acceptance: the pair acceptance.

- [ ] [P8-T5] AC4 runs 9 and 10 into `coverage/trx/p8-t5`, artifact `<FEATURE>/evidence/regression-testing/p8-t5-ac4-runs.md`. Acceptance: the pair acceptance.

- [ ] [P8-T6] Aggregate the ten runs into `<FEATURE>/evidence/regression-testing/p8-t6-ac4-ten-run.md`: a ten-row table (run, total, executed, passed, failed, error, aborted, timeout, wall-clock seconds read from each TRX's `Times/@start` and `@finish`), the `SOURCE-HEAD` all five pair tasks recorded, the `AC4_COMMAND_SHAPE` used, the filter used with the sentence that the shell-icon exclusion is a documented local environmental exclusion covered by CI and that the CI run on the pull request supplies the unfiltered single-run form (one CI run is not ten), and the local worker count (`[Environment]::ProcessorCount`) since `Workers = 0` resolves to it. Acceptance: exactly ten rows; every `failed`, `error`, `aborted`, `timeout` is 0; every `passed` equals its `total`; all ten `total` values are identical and equal the P7-T5 `total`; the five `SOURCE-HEAD` values are identical; the artifact contains no account name, machine name or absolute path (checked with the run-time-derived tokens, expected count 0); `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` present.

- [ ] [P8-T7] Check off AC4 in `<FEATURE>/spec.md` line 295, citing `p8-t6-ac4-ten-run.md`. Acceptance: `'- [x] AC4:'` counts 1 and `'- [ ] AC4:'` counts 0 in spec.md.

- [ ] [P8-T8] Host-token sanitisation sweep over `<FEATURE>/evidence/**` (contents and names) recording `<FEATURE>/evidence/qa-gates/p8-t8-sanitisation.md`: with `$acct` and `$mach` derived at run time (C2), count `@(Get-ChildItem -Recurse -File -Path $Feature/evidence | Select-String -Pattern "(?i)$acct").Count`, the same for `$mach`, the drive-rooted profile-path shape `[A-Za-z]:[^A-Za-z0-9]Users[^A-Za-z0-9]` (written without backslashes so this document stays free of them) as a single-quoted string, and the count of file or directory names under `$Feature/evidence` containing either token; rewrite any offending content to the `<user>`, `<machine>`, `<worktree>` tokens and record per-file substitution counts (the rewrite exits 0 either way, so the counts are the observation). Acceptance: all four final counts are 0; the artifact never writes either token's value; the sweep covers every artifact produced by P0-T1 through P8-T7, and the artifacts P8-T9 through P8-T11 produce are written from plan text only and are re-counted by P8-T12.

- [ ] [P8-T9] Author the two follow-up potential entries under `docs/features/potential/` using the bug-template heading set of `<FEATURE>/issue.md` lines 14-83 (Summary through Next Step, `- Status: Draft`, `- Date captured: <today>`): (1) `docs/features/potential/<yyyy-MM-dd>-etl-deadline-mechanics-follow-ups.md` covering the 250 ms-per-row budget at `OlTableExtensions.Etl.cs:81`, the residual 2000 ms `GetTableInViewAsync` window seamed by a factory rather than a `TimeProvider`, `EtlAsync`'s null-through-suppression tuple contract (nullable `data` plus rethrown `TimeoutException`), deletion of the inert `(int, int)` `TimeoutAfter` overloads at `TimeOutTask.cs:824` and `:924` (also recorded by #798 `evidence/other/followup-promotions.md` Finding 1), the stale `TableEtlInvoker` mention in the `DfDeedle.QfcColumns.cs:96` doc comment, and removal of `[DoNotParallelize]` from `OlTableExtensions_Tests` after a parallel soak; (2) `docs/features/potential/<yyyy-MM-dd>-console-out-aggressors-and-banned-symbol-promotion.md` covering the roughly 24 test classes that install a `DebugTextWriter` with no restore, the two production `Console.WriteLine` diagnostics at `OlTableExtensions.TableAccess.cs:78,96` that should use the logger, and promotion of RS0030 from `suggestion` to `warning` after legacy cleanup. Acceptance: both files exist; each contains the line `- [ ] Promote to GitHub issue (bug-report template)`; neither contains an absolute host path; the promotion itself is the orchestrator's step through the `feature-promotion-lifecycle` route and is not performed here.

- [ ] [P8-T10] Write the issue-update mirror `<FEATURE>/evidence/issue-updates/issue-811.<yyyy-MM-ddTHH-mm>.md` with `Timestamp:`, the exact text intended for the issue (the AC status summary in the `acceptance-criteria-tracking` format with `Source: spec.md`, `Total AC items: 5`, `Checked off (delivered): 5`, `Remaining (unchecked): 0`, the P8-T6 ten-row table, the two follow-up entries by path, and the closure pointers "#780, #803 and #594 are superseded by #811"), and the header `POSTING BLOCKED: GitHub posting is gated to the orchestrator by repository hooks` with `PostedAs: unknown`. Acceptance: the file exists; contains `Total AC items: 5` and `Checked off (delivered): 5`; contains no host token.

- [ ] [P8-T11] Final AC reconciliation in `<FEATURE>/evidence/other/p8-t11-ac-reconciliation.md`: read `<FEATURE>/spec.md` lines 292-296. Acceptance: `Select-String -SimpleMatch '- [x] AC'` over spec.md counts 5 and `'- [ ] AC'` counts 0; the artifact lists each AC with the evidence artifact that discharged it (AC1: fail-before dossier + p4-t4 + p8-t6; AC2: p1-t9 + p2-t4 + p3-t2; AC3: p5-t10; AC4: p8-t6; AC5: p7-t10) and the "Acceptance Criteria Status" block.

- [ ] [P8-T12] Final commit and clean-tree gate recording `<FEATURE>/evidence/other/p8-t12-final-commit.md`: first write the artifact with the P8-T8 four counts re-run over `$Feature` and `docs/features/potential/` (expected 0, 0, 0, 0) and the intended commit message; then `git add -A -- . ":(exclude).claude"`; then `git commit -m "docs(811): AC4 ten-run evidence, sanitisation, follow-ups, AC reconciliation"`. The resulting sha is reported in the executor's completion message, not written to any artifact, so nothing is written after the commit. Acceptance: exit 0; `git status --porcelain --untracked-files=all -- . ":(exclude).claude"` is empty; `git diff --name-only $Base..HEAD -- "*.cs" "*.csproj"` still lists exactly the 20 Write Set paths; `git ls-files -- $Feature/evidence | Measure-Object` counts at least 40 tracked artifacts and includes `p8-t6-ac4-ten-run.md`, `fail-before-exception.` (one file), and `p2-t4-ac2-fail-before.md`; no committed path under `$Feature/evidence` ends in `.trx`, `.coverage` or `.cobertura.xml`.

---

## Plan deviations recorded against spec.md prose (no acceptance criterion is changed)

- The "token cancelled after the work is observed to start" test (spec Test Strategy, AC1 bullet 3) is not authored; see D6.
- `EtlAsyncOld` is retained; spec.md offers deletion as an alternative to a documentation budget, and the budget is met without it (D9).
- `[DoNotParallelize]` is retained on `OlTableExtensions_Tests` under spec Mitigation 4 (D8); the console reason is removed from its comment and the retention is filed as a follow-up.
- `NLogTraceWriter_Test.cs` loses its `DebugTextWriter` install as well as its save/restore (P5-T9); the spec asks for the save/restore removal, and deleting the now-purposeless install is the smaller resulting file.
- Raw Cobertura documents are not committed (D15); numeric figures are recorded in the artifacts.

---

## Adversarial self-review

This section covers two passes. Round 1 (initial authoring) re-derived every citation the plan
relies on against this worktree with Read, Grep and Glob. Round 2 (this pass, the revision that
applied the nine-defect preflight delta) re-derived, against the current tree, every citation the
delta's edits touched, and re-read the sibling lines and clauses of each: `OlTableExtensions_Tests.cs`
lines 14-22, 958-978 and 1625-1656 plus a whole-file search for `EnumerateTable(`;
`OlTableExtensions.Etl.cs` lines 60-134 plus a whole-file search for `> EtlAsync(` and for lines
that are exactly eight spaces followed by `}`; `PrettyPrint_Tests.cs` lines 184-221;
`DASLFilterParserTests.cs` lines 100-125; `StackGeek_Tests.cs` lines 1-20 and 178-193 plus its
`using` directives; `DfDeedle_COM_Tests.cs` lines 1-19 plus a whole-file search for `TableEtlInvoker`;
the feature folder listing; `.editorconfig` for `IDE0005`; and a project-wide count of
`using UtilitiesCS;` in `UtilitiesCS.Test`. No source file has been edited between the two passes,
so the round-1 citations that the delta did not touch describe the same tree; executor preflight
round 1 independently confirmed every cited line range against it.

Round 2 findings recorded for the caller:

- The P5-T10 parenthetical was `(seven names)` in the delta and was applied verbatim, then reported as a miscount. The list it qualifies enumerates three renamed tests plus `Main_RunsSampleScenarioWithoutThrowing`, `Run_WritesScenarioToSuppliedWriter` and `PrettyPrint_NullWriter_WritesToConsoleWithoutThrowing`, which is six names. The orchestrator corrected line 371 to `(six names)` after confirming the enumeration directly.
- Defect 6's premise is narrower than stated: `StackGeek_Tests.cs` declares `namespace UtilitiesCS.Test.ReusableTypeClasses`, an enclosing-namespace lookup already resolves `UtilitiesCS.GFG` as `GFG` without a directive. The directive is nevertheless harmless and matches repository practice: 104 files under `UtilitiesCS.Test` already carry `using UtilitiesCS;` (including `DfDeedle_COM_Tests.cs` line 14 in a namespace that is likewise nested under `UtilitiesCS`), and `.editorconfig` has no `IDE0005` entry, so the redundant directive raises no build diagnostic and the `P7-T3` warning ceiling is unaffected.
- D13 previously stated that "P8-T6 carries a non-vacuity control naming every new test"; the control is in fact the `Read-TrxOutcome` clause of P8-T1, and P8-T6 aggregates counters only. D13 was not in the delta, so the orchestrator corrected the attribution to P8-T1 after confirming it against P8-T1 and P8-T6.
- The new single-line assertion `FluentActions.Invoking(() => mockTable.Object.EnumerateTable()).Should().NotThrow();` is 84 characters plus a 12-space indent (96 columns), so CSharpier keeps it on one line and the P5-T8 arithmetic (−9 +2 = −7, ceiling 1848) holds.

Round 1 findings that changed the plan:

- `EtlAsyncOld` (Etl.cs 132-169) and the commented-out line `DfDeedle.cs:214` retain the old tokens, so every zero-hit gate over `attempts`, `DateTime.Now` and `TimeoutAfter(1000, 2)` was rewritten as an exact-count gate.
- `Columns.Add("SentOn")` at `DfDeedle.QfcColumns.cs:40` is the first call inside `AddQfcColumns`, which gives the RED test a deterministic first gate; without it the number of `Armed` signals before the ETL timer is 1 or 2 depending on a race.
- `PrettyPrint.cs` has no `System.IO` using and both seamed overloads wrap under the 100-column width, so "zero net growth" needed three named removals, verified by token probes (no `Task`, no `Svg`, duplicate `System.Text` at line 12 versus line 5).
- The `Key` hits in `PrettyPrint.cs` (lines 174, 535) are property reads, not `System.Windows.Input.Key`, which is why line 15 is the substitution candidate and not a primary removal.
- `Triage_OlLogicTests.cs:142` already exercises the null-writer `PrintTree` path, so no extra null-writer test is added for DASL.
- `UtilitiesCS/Properties/AssemblyInfo.cs:19` grants `InternalsVisibleTo("UtilitiesCS.Test")`, so `GFG.Run` can be called directly from the test without reflection.
- `CreateTableWithColumns` (`OlTableExtensions_Tests.cs:1779-1780`) already derives `GetRowCount()` from the row count, so retiring `Returns(120)` is a pure deletion of four lines.
- `TimeOutTask_Tests.cs:197,210` still call the `(int, int)` overload; P1-T9 runs that class to prove the untouched overloads still bind.
- The dot-claude agent-memory tree is tracked; every git gate is pathspec-scoped with the exclusion.
- No project-file line count enters the 500-line audit; `UtilitiesCS.Test.csproj` is 982 and grows by exactly three items.

SELF-REVIEW: RE-DERIVED THIS PASS

Round 2 (this pass), one entry per citation the delta touched:

- `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` | 17-19 (comment naming `redirects Console.Out`), 20 (`[DoNotParallelize]`), 1635 (`var output = new StringWriter();`), 1636 (`var original = Console.Out;`), 1638-1640 (`try`, brace, `Console.SetOut(output);`), 1641 (`mockTable.Object.EnumerateTable();`, the only `EnumerateTable(` occurrence in the file), 1642-1646 (`}`, `finally` block), 1648-1651 (three `Contain` assertions and the `MoveToStart` verify); line count 1855
- `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` | 960-963 (comment and `Returns(120)`), 971-977 (`EtlAsync` call, one argument per line); P6-T1 net −2 confirmed
- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs` | 66 (the only line containing `> EtlAsync(`), 130 (first line after 66 that is exactly eight spaces followed by `}`; the intervening closers at 116 and 123 are twelve-space), 117-123 (`catch (TimeoutException)`), 132 (`EtlAsyncOld` opens; excluded from the span)
- `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs` | 186 (test name), 194 (`var originalOut = Console.Out;`), 195 (`using var writer`), 196 (`Console.SetOut(writer);`), 198-199 (`try`, `{`), 204-205 (`frame.PrettyPrint();`, `row.PrettyPrint();`), 215-219 (`}`, `finally`, `{`, `Console.SetOut(originalOut);`, `}`); line count 408
- `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` | 102 (test name), 108-109 (`var originalOut`, `Console.SetOut(writer);`), 111-112 (`try`, `{`), 114 (`parser.PrintTree(tree, 0);`), 115-119 (`}`, `finally`, `{`, `Console.SetOut(originalOut);`, `}`), 122 (assertion); line count 125
- `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` | 1-5 (the five `using` directives; no `using UtilitiesCS;`), 7 (`namespace UtilitiesCS.Test.ReusableTypeClasses`), 9-15 (comment and `[DoNotParallelize]`), 187-188 (`typeof(UtilitiesCS.StackObjectCS<int>).Assembly` and `GetType("UtilitiesCS.GFG", ...)`); line count 276
- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | 14 (`using UtilitiesCS;`), 26 (header mention), 367, 725, 735, 796 (comments naming `TableEtlInvoker` or `StoreTableEtlInvoker`), 399-400, 413, 762-763, 781, 827-828, 846 (the swap statements); `TableEtlInvoker` substring count 14
- `UtilitiesCS.Test` | 104 files carry `using UtilitiesCS;` (project-wide count)
- `.editorconfig` | no `IDE0005` entry
- `<FEATURE>/` | contains `issue.md`, `spec.md`, `plan.2026-09-07T22-03.md` only; `spec.md` is tracked at `HEAD` and dated to this branch's creation, so `HEAD` is a valid numstat anchor for P3-T3 (existence at `$Base` is denied by the caller's preflight and is not independently checkable from this planner's tool surface, which has no shell)
- `plan.2026-09-07T22-03.md` | the words `ten`, `eight new`, `P8-T6` searched document-wide; the surviving `ten` occurrences (D8 "ten of its tests", Phase 8 heading "ten-run", P8-T6 "ten rows") count tests in a class or AC4 runs and are unrelated to the C4 sentinel set

Round 1, retained (the delta did not touch these and no source file has changed since):

- `UtilitiesCS/Extensions/DictionaryExtensions.cs` | lines 123-136 (`TryAddValues`), 169-180 (`TryAddValuesAsync`), 176-177 (linked source and `CancelAfter(500)`); line count 282
- `UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs` | line 237 (test name), 244 (only invocation); line count 296
- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs` | 66-73 (`EtlAsync` signature), 81-82, 98-106, 114, 117-123, 129, 132-169 (`EtlAsyncOld`), 171 (four-parameter `EtlByRowAsync`), 229-237 (seven-parameter), 245, 259; line count 474
- `UtilitiesCS/Extensions/DfDeedle.cs` | 54-60 (`MessageBoxInvoker`), 62-84 (two statics), 86-102 (`GetEmailDataInView`), 94, 139-144, 166, 176, 180-185, 186-189 (call opens 186, dereference 188), 195, 204-208, 214 (commented-out `TimeoutAfter(1000, 2)`); line count 314
- `UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs` | 125-148 (`FromDefaultFolder(Store)`), 143, 150-165 (`Stores` overload); line count 276
- `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` | 23-86 (`AddQfcColumns`, first call `Columns.Add("SentOn")` at 40), 96 (doc comment naming `TableEtlInvoker`), 102-109 (`AddQfcColumnsAsync` with `timeProvider`), 119, 127, 158-168; line count 297
- `UtilitiesCS/Threading/TimeOutTask.cs` | 824-849 (`(int, int)` overload; catch at 836, retry at 841), 862-874 (`(int, TimeProvider?)` overload; `return task` at 873); line count 1011
- `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs` | 97-104 (`PrintTree`), 99; line count 122; no `TextWriter`
- `UtilitiesCS/HelperClasses/PrettyPrint.cs` | 1-18 (usings; duplicate `System.Text` at 5 and 12), 25, 27; token probes for `Task`, `Svg`, `Outlook`, `Input` types; line count 680
- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` | 1-12 (usings), 78, 96, 381-428 (`EnumerateTable`), 423-425; line count 430
- `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs` | 1-16 (usings, `class GFG` internal), 96, 127, 137, 173-192 (`Main`); line count 199
- `UtilitiesCS/Properties/AssemblyInfo.cs` | 19 (`InternalsVisibleTo("UtilitiesCS.Test")`)
- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | 20-33 (header), 197-211 (`MessageBoxInvoker` swap shape), 371, 399-414, 423-489 (the #803 test; `GetRowCount` 1 at 442, call at 477-482), 729, 762-782, 790, 827-847, 850-853 (`CreateProgressTracker`); `TableEtlInvoker` count 14; line count 869
- `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` | 8 (`using Microsoft.Extensions.Time.Testing`), 21-22 (`[DoNotParallelize]`), 39-85 (`ArmingBarrierTimeProvider`), 95, 103, 130-141 (`FireOneDeadlineAsync`), 266-293 (barrier usage), 500 (last line); line count exactly 500
- `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` | 1-13 (usings; no `Time.Testing`), 17-22 (comment and attribute), 946-984 (test with `Returns(120)` at 963, comment at 960-962), 986-1017 (`EtlAsyncOld` test), 1124-1137 (reflection binding), 1635-1651 (console capture), 1715-1716, 1723-1750, 1757-1793 (`effectiveRowCount` at 1779-1780), 1813-1822; line count 1855
- `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` | 9-17, 146-168; line count 276
- `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs` | 14-21, 185-220; line count 408
- `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` | 8-16, 101-123; line count 125
- `UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs` | 17, 19-29, 46-57; line count 119
- `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs` | 142 (`PrintTree(logicTree, 0)`)
- `UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs` | 197, 210 (`TimeoutAfter(100, 3)`)
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | 73-76 (`TestHelpers\` items), 188-191 (`DfDeedle` family), 538 (`OlTableExtensions_Tests.cs`); line count 982
- `UtilitiesCS.Test/Properties/AssemblyInfo.cs` | 18-21 (`Parallelize(Workers = 0, Scope = ClassLevel)`)
- `UtilitiesCS.Test/TestHelpers/ManualFireTimerWrapper.cs` | 4 (namespace `UtilitiesCS.Test.TestHelpers`)
- `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | 15, 82-89, 96-108
- `ToDoModel/Data Model/ID/IDList.cs` | 130 (named-argument call)
- `TaskMaster/Ribbon/TryFunctionalityInConstruction.cs` | 51, 66 (from repository grep)
- `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs` | 74 (from repository grep)
- `.github/workflows/_mstest-coverage.yml` | 86-92 (discovery), 99 (invocation); `timeout-minutes: 30` per spec.md
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 1-13 (parameters), 70-76 (argument shape, hard-coded filter), 296-307 (discovery, `\.claude\` rejection at 301), 340-345, 348
- `scripts/vscode/Invoke-Restore.ps1` | 1-10 (parameters), 36
- `scripts/vscode/Install-RepoDotNetSdk.ps1` | 1-11 (parameters)
- `global.json` | 1-12 (`8.0.205`, `.dotnet-sdk`)
- `TaskMaster.runsettings` | 1-30 (parallelize and module excludes); `scripts/vscode/TaskMaster.cli.runsettings` exists (Glob)
- `.gitignore` | 44, 57, 140-145 (`*.coverage`, `coverage/*`), 350
- `.editorconfig` | 546-548 (RS0030 at `suggestion`)
- `coverage.config` | exists at the repository root (Glob)
- `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/shell-icon-stall-probe.md` | verdict `SHELL_ICON_EXCLUSION: REQUIRED` and the filter extension
- `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/vstest-coverage-baseline.md` | command shape, 7023 tests, 54.5 s, nine-assembly list
- `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/csharpier-check-baseline.md` | `Checked 1587 files in 7826ms.`
- `<FEATURE>/issue.md` | 12 (`Work Mode: full-bug`), 14-83 (bug-template headings)
- `<FEATURE>/spec.md` | 13, 52-57, 66-72, 135, 139-143, 151-182, 259-288, 291-296 (AC1-AC5), 298-318, 325-329
- `<FEATURE>/research/root-cause.2026-09-07T22-10.md` | Sections 1-5 and Numeric Derivation Evidence (read in full)

---

## Planner Internal Review Record

The two write-set paths whose directory name contains a space (`UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs`, lines 97-104, and `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs`, lines 8-14 and 101-123) were re-derived in this pass and appear in the enumeration above; they are omitted from the `CITATION:` lines below only because the record format requires a space-free path token.

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: UtilitiesCS/Extensions/DictionaryExtensions.cs | lines 169-180; CancelAfter(500) at 177; TryAddValues 123-136
CITATION: UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs | EtlAsync 66-130 (sole "> EtlAsync(" line 66; eight-space closer 130); catch 117-123; EtlAsyncOld 132-169; EtlByRowAsync 171 and 229-263; TimeoutAfter at 114, 245, 259
CITATION: UtilitiesCS/Extensions/DfDeedle.cs | statics 62-84; GetEmailDataInView 86-102; GetEmailDataInViewAsync 139-219; LogDfTiming opens 186, dereference 188; TimeoutAfter(1000, 2) at 208 and comment 214
CITATION: UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs | FromDefaultFolder(Store) 125-148, StoreTableEtlInvoker read at 143; Stores overload 150-165
CITATION: UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs | AddQfcColumns 23-86 (Columns.Add("SentOn") at 40); AddQfcColumnsAsync 102-109; TimeoutAfter(3000, timeProvider) at 127; doc comment 96
CITATION: UtilitiesCS/Threading/TimeOutTask.cs | (int, int) overload 824-849; (int, TimeProvider?) overload 862-874
CITATION: UtilitiesCS/HelperClasses/PrettyPrint.cs | usings 1-18; PrettyPrint overloads 25 and 27; line count 680
CITATION: UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs | EnumerateTable 381-428; Console.WriteLine at 78, 96, 423-425
CITATION: UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs | GFG.Main 173-192; Console.WriteLine at 96, 127, 137, 187-191
CITATION: UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs | using UtilitiesCS at 14; header 20-33; seam comments 367, 725, 735, 796; seam swaps 399-414, 762-782, 827-847; failing test 423-489; line count 869
CITATION: UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs | ArmingBarrierTimeProvider 39-85; line count exactly 500
CITATION: UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs | attribute 17-20; Returns(120) 960-963; EtlAsync call 971-977; reflection 1124-1137; console capture 1635-1651 (sole EnumerateTable( call at 1641); builders 1723-1793; line count 1855
CITATION: UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs | usings 1-5 (no using UtilitiesCS); attribute 9-15; console capture 146-168; reflection 187-188
CITATION: UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs | attribute 14-19; console capture 185-220 (try 198-199; finally 215-219)
CITATION: UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs | field 17; save/install 22-23; TestCleanup 53-57
CITATION: UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs | TryAddValuesAsync_UpdatesExistingValue 236-249; only invocation 244
CITATION: UtilitiesCS.Test/UtilitiesCS.Test.csproj | TestHelpers items 74-76; DfDeedle items 188-191; OlTableExtensions_Tests item 538; line count 982
CITATION: UtilitiesCS.Test/Properties/AssemblyInfo.cs | Parallelize(Workers = 0, Scope = ClassLevel) at 18-21
CITATION: .github/workflows/_mstest-coverage.yml | recursive discovery 86-92; invocation 99
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | argument shape 70-76; discovery and dot-claude rejection 296-307
CITATION: docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/spec.md | Acceptance Criteria 291-296; Files/modules to change 151-182
CITATION: docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/research/root-cause.2026-09-07T22-10.md | Sections 1 to 5
AC-INVENTORY: AC1 AC2 AC3 AC4 AC5
AC-MAPPING: AC1 | IMPLEMENTATION: P4-T1 | TESTS: P4-T2,P4-T4,P8-T1 | EVIDENCE: fail-before-exception dossier (P4-T3), p4-t4-ac1-pass-after.md, p8-t6-ac4-ten-run.md, check-off P4-T5
AC-MAPPING: AC2 | IMPLEMENTATION: P1-T1,P1-T2,P1-T3,P1-T4,P3-T1 | TESTS: P1-T5,P2-T1,P2-T4,P3-T2 | EVIDENCE: p1-t9-seam-scoped-run.md, p2-t4-ac2-fail-before.md, p3-t2-ac2-pass-after.md, check-off P3-T3
AC-MAPPING: AC3 | IMPLEMENTATION: P5-T1,P5-T2,P5-T3,P5-T4 | TESTS: P5-T5,P5-T6,P5-T7,P5-T8,P5-T9,P5-T10 | EVIDENCE: p5-t10-ac3-pass-after.md, check-off P5-T11
AC-MAPPING: AC4 | IMPLEMENTATION: P0-T12,P7-T12 | TESTS: P8-T1,P8-T2,P8-T3,P8-T4,P8-T5 | EVIDENCE: p8-t1..p8-t5-ac4-runs.md, p8-t6-ac4-ten-run.md, check-off P8-T7
AC-MAPPING: AC5 | IMPLEMENTATION: P6-T1,P6-T3 | TESTS: P6-T5,P7-T10 | EVIDENCE: p6-t5-etl-clock-pass.md, p7-t10-ac5-timing-hack-search.md, check-off P7-T11
UNRESOLVED-GAPS: NONE
