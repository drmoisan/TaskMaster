# 2026-09-11-minor-audit-trio-gate-cts-tracker (Plan)

- **Issue:** #872
- **Work Mode:** minor-audit
- **Parent:** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12T10-26
- **Status:** Draft
- **Version:** 1.0
- **Phases:** 3 (Phase 0 baseline capture, Phase 1 delegated implementation, Phase 2 final QC loop)
- **Task counts (mechanical, one per `- [ ] [P#-T#]` line):** Phase 0 = 14, Phase 1 = 17, Phase 2 = 33, total = 64

## Requirements Source

The sole requirements source is `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md`
and specifically its `## Acceptance Criteria` section, which carries AC1 through AC12. This is a
minor-audit item: there is no spec.md and no user-story.md in this feature folder, their absence is
correct, and it is not a blocker. No acceptance criterion is inferred from any other section of that
file.

**Fail-closed evidence rule.** Every baseline command step and every final-QC command step writes its
own artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. If any required
baseline artifact, final-QC artifact or coverage-comparison artifact is missing or incomplete, the
verdict is BLOCKED or INCOMPLETE and never PASS. An approved-plan checkbox stays unchecked while its
artifact is absent or incomplete.

**Evidence location.** All evidence resolves under
`docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/` in the canonical
sub-folders baseline, regression-testing, qa-gates, issue-updates and other. No artifacts path is used
for evidence.

## Write Set

Every path below is a repository-relative path that the delivered code diff creates, modifies or
deletes. It reproduces the Write Set in issue.md unchanged.

- `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`
- `UtilitiesCS/Threading/ProgressPackage.cs`
- `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs`
- `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs`
- `UtilitiesCS/Threading/ProgressTrackerAsync.cs`
- `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`
- `UtilitiesCS/UtilitiesCS.csproj`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

The last four entries are the deletion group. The first two of them are deleted outright; the two
project files each lose exactly one Compile item.

The promotion record for this issue, which is the deletion of the potential-record markdown file and
the matching copy of it under the promoted sub-folder of the potential features directory, is
committed by the preparation run before Phase 0 and is not part of the delivered code diff.

This plan additionally writes its own bookkeeping and evidence inside the feature folder:

- `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md`
- `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/plan.2026-09-12T10-26.md`
- `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/`

## Scope Boundary

The following are read but never edited by this delivery. They are stated in prose without path
formatting so that no automated write-claim extractor reads an exclusion as a write.

- The production source of the high-confidence dequeue gate, file name
  QfcStreamingDequeueConfidenceGate.cs under the QuickFiler Controllers directory. Defect A is
  test-only; the dequeue gate's production source is correct as it stands. The executor reads it to
  transcribe the log line and changes nothing in it.
- The six consumer files named by research scope finding SF-1: the transform and folder-extraction
  partials of the email data miner, the OlFolder classifier group, the multiclass engine, the category
  classifier group, and the Bayesian performance measurement type. AC3 is a capability criterion. The
  residual leak at those nine call sites is promoted to a follow-up issue by the calling orchestrator,
  which owns that obligation; no task in this plan files it and no task in this plan edits those files.
- The progress tracker and progress tracker pane types, per scope finding SF-2. Both downstream
  viewers already catch ObjectDisposedException and document the borrowed-source contract.
- The QuickFiler test project file. No new test part file is created, so it needs no Compile item.
- The ProgressTracker report-and-viewer test file. Its only mention of the deleted type sits inside a
  code tag in an XML doc comment and is not compiled.
- The generated coverage summary at the repository root, file name coverage_output.txt. It names the
  deleted type and becomes stale after the deletion. It is generated, not compiled, and not edited.
- Agent memory under the dot-claude directory. It is tracked in this repository and the executor writes
  to it during a run, so every status, diff and grep gate in this plan is scoped to exclude it.

## Decisions Record

- **D1 — MSBuild and vstest resolution.** Neither msbuild.exe, vstest.console.exe nor vswhere.exe is on
  PATH in an agent worktree. Both are resolved through the explicit vswhere path shown in the Command
  Reference. The vswhere-resolved MSBuild is used rather than any repository build wrapper, because a
  wrapper that rewrites project HintPaths would dirty the Write Set.
- **D2 — `/t:Rebuild` only.** MSBuild's incremental up-to-date check does not invalidate on a
  command-line property change, so a warm `/t:Build` returns exit 0 with CoreCompile skipped on every
  project and runs no analyzer. Every analyzer and nullable gate in this plan uses `/t:Rebuild`.
- **D3 — The Nullable property is never supplied.** No project in this repository carries a Nullable
  element and there is no solution-wide opt-in. Supplying it conscripts every file that has never
  adopted the per-file pragma and produces roughly 195 errors in the UtilitiesCS project. The CI
  nullable workflow omits it deliberately and so does this plan.
- **D4 — Base anchor is self-anchored at Phase 0.** P0-T2 records the base commit into
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/base-commit.md`.
  No commit SHA is written into this plan. Every later git diff re-reads that file in its own command,
  because no shell variable survives between tasks.
- **D5 — Test-run population.** Every per-assembly test run in this plan, in Phase 0 and in Phase 2
  alike, uses the identical test-case filter pinned in the Command Reference. It excludes the
  LiveOutlook category, which starts an external Outlook process, and the four shell-icon test classes
  that stall vstest on this workstation through a Windows shell icon call. None of the excluded classes
  is touched by this delivery, so the AC11 delta arithmetic is unaffected by the exclusion. The filter
  string is identical in both phases, which is what makes the delta comparable.
- **D6 — Coverage runner population differs, and that is intended.** The repository coverage runner
  hard-codes its own LiveOutlook-only filter and offers no extension point, so its population is wider
  than D5's. The coverage run is used only for the per-file figure on the progress package source and
  for the repository-wide headline; it is never used as the source of the AC11 per-assembly counts.
- **D7 — Coverage-runner non-termination hazard.** The same shell-icon classes cannot be excluded from
  the coverage runner. This plan records no measured run time for that runner in this tree, so the
  hazard is treated as unquantified rather than as dormant. If the coverage run does not terminate
  within ten minutes, the executor halts, records the stall in the step artifact with `EXIT_CODE:` set
  to the observed value, and reports BLOCKED to the caller. It does not silently substitute a narrower
  run.
- **D8 — A green vstest run prints no failure line.** On this toolchain a fully green run prints
  `Test Run Successful.`, a `Total tests:` line and a `Passed:` line, and prints no `Failed:` line and
  no `Skipped:` line at all. No acceptance condition in this plan demands a zero-valued `Failed:` line,
  because that line is never emitted on the run the condition is meant to pass. The failure counters
  are transcribed as 0 and the transcription is corroborated by the success header and by the passed
  count equalling the total count. Two files in the UtilitiesCS test project carry a method-level
  Ignore attribute, and neither is compiled: the project declares explicit Compile items with no
  wildcard glob, and its items name only the copies of those two test classes that live under its
  dialogs sub-folder, which carry no Ignore attribute. The compiled test population therefore contains
  no ignored test, and the zero-skipped expectation in P0-T8, P0-T9, P2-T5 and P2-T6 is satisfiable.
  This is recorded because the two uncompiled files are easy to find by search and easy to mistake for
  live code, and a reviewer who reads them as compiled will conclude that those four acceptance
  conditions cannot pass. Verify compilation by looking for a Compile item naming the file, not by
  finding the file.
- **D9 — Known flaky test.** The dictionary-extensions test named
  TryAddValuesAsync_UpdatesExistingValue in the UtilitiesCS test assembly fails sporadically under
  high-worker coverage runs and is tracked as issue #780. If that test and only that test fails, the
  executor re-runs the same command once, records both runs in the same artifact, and takes the
  acceptance on the second run. Any other failing test is a real failure and restarts the Phase 2 loop.
- **D10 — Transient tool output is written to a git-ignored directory.** MSBuild file logs and vstest
  TRX files carry absolute host paths. Writing them under the feature folder would commit host paths
  into the repository and would require a sanitisation pass. They are therefore written under the
  repository-root TestResults directory, whose name matches a git-ignore directory class, and the
  acceptance-bearing values are transcribed into the canonical evidence artifact. The raw Cobertura
  coverage XML produced by P0-T10 and P2-T7 is routed to the same directory for a second reason: the
  maintainer decision recorded on issue 671 on 2026-09-11 moved this repository to projection-only
  coverage evidence, effective immediately, under which no new raw Cobertura XML and no new TRX file is
  added to git. Item 1 of that decision names the committed projection as the package-level JaCoCo XML
  used by item #646 together with the one-line first-party summary emitted by the first-party coverage
  report helper, and item 2 names a passed, failed, skipped and total count summary in place of a TRX.
  The two transient coverage paths are `TestResults/coverage/coverage-baseline.cobertura.xml` and
  `TestResults/coverage/coverage-postchange.cobertura.xml`; both resolve to the git-ignore pattern
  `[Tt]est[Rr]esult*/` on line 39 of `.gitignore`. This plan does not conform to that decision in full,
  and the gaps are stated here so that a reviewer does not read the citation above as full conformance.
  Of the two projection forms item 1 names, this plan commits only the one-line first-party summary,
  which P0-T10 and P2-T7 transcribe into their Markdown step artifacts; it commits no package-level
  JaCoCo XML, and producing that projection is not part of this delivery. Item 3 asks that raw output
  be discarded once its projection is written rather than retained under a git-ignored path, and this
  plan retains it: P0-T11 reads the baseline Cobertura document that P0-T10 writes and P2-T9 reads the
  post-change Cobertura document that P2-T7 writes, so each file must still exist when its reader runs,
  and no task in this plan removes either one. Item 4 asks that a test invocation set an explicit
  results directory and an explicit log file name so that the default account-and-host TRX name is
  never produced even transiently. Every vstest span in this plan sets both: an explicit
  `/ResultsDirectory:` and an explicit log file name supplied inside a quoted
  `/Logger:trx;LogFileName=` value that names the task which produced it. Item 4 is therefore
  discharged. The default TRX name that vstest composes from the account name and the host name is
  never produced, so no account or host token reaches a results directory and none can be transcribed
  into a committed artifact by P2-T14, P2-T15 or P2-T16. Because each span names a fixed file, a
  Phase 2 restart overwrites the TRX of the task it restarts rather than adding a second file to that
  results directory; the most-recent-write selection rule those three tasks state is retained and is
  inert while one file is present. The prohibition that governs the repository contents is discharged
  in full as well: no TRX, no MSBuild log and no raw coverage XML is committed.
- **D11 — AC5 is not verifiable by coverage.** The rebuild method in the subject-map orchestration
  partial carries the ExcludeFromCodeCoverage attribute. An excluded member emits no method element in
  the Cobertura report at all; it is absent rather than reported at zero, so no per-file coverage
  figure on that file can discriminate whether the AC5 change landed. AC5 is verified structurally by
  an anchored diff and by the two rebuild gates. It carries no test obligation, because the method
  installs a WindowsFormsSynchronizationContext and starts a long-running task and is not unit-testable
  without a host.
- **D12 — Shell discipline.** No task in this plan uses a command of the form that changes directory
  and then chains a second command, because every chained segment of a command line is checked
  independently against the shell allowlist. Repository-relative paths and `git -C` semantics are used
  instead. PowerShell payloads are quoted with outer single quotes and inner double quotes. There is no
  Python toolchain in this repository and no task runs poetry, pytest or a Python module.
- **D13 — Commit gating.** The two commit tasks stage production source, so the repository's
  pre-implementation commit gate applies to them and is satisfied by the orchestration state the
  calling orchestrator seeds. If that gate blocks a commit, the executor records the block in the step
  artifact and reports BLOCKED to the caller. It does not restructure the commit, split it into exempt
  pathspecs, or attempt any other route around the gate.
- **D14 — The MSTest runsettings file is not passed to any direct vstest span.** The six direct vstest
  spans in this plan previously appended the MSTest runsettings file under the vscode scripts
  directory. That file's entire content is an MSTest Parallelize block with ClassLevel scope and a
  worker count of zero, which runs test classes concurrently across every logical processor. Under that
  parallelism three tests in the QuickFiler zero-batch email-queue test class fail with a type
  initialization exception for the Deedle reflection type against netstandard 2.1: one race during
  concurrent class initialization poisons the type, and the CLR caches a failed static initializer for
  the process lifetime, so every later run in the same process reproduces it and the failure reads as
  deterministic. The same command with only that switch removed was measured at 1393 of 1393 passed and
  exit 0. The CI MSTest coverage workflow passes no settings file at all, so the defect is invisible to
  the merge gate and parity is with CI rather than with the repository runner. Retaining the switch
  would make the exit-zero and zero-failure acceptance conditions of P0-T9, P2-T6 and therefore AC11
  unsatisfiable by any work this plan performs, which is a gate that cannot pass rather than a gate that
  cannot fail. The switch is therefore removed from all six spans. Nothing else in those spans changes:
  the pinned test-case filter is byte-identical in Phase 0 and Phase 2, so the D5 comparability
  guarantee and the AC11 delta arithmetic are untouched, and parallelism affects which tests pass rather
  than how many are discovered. Neither the runner script nor the runsettings file is edited by this
  plan; both are outside the Write Set, and the underlying repository defect that the documented local
  coverage runner fails on the QuickFiler test assembly is reported to the caller rather than fixed
  here. That defect still reaches P0-T10 and P2-T7, which invoke the runner and cannot avoid the switch
  because the runner resolves the runsettings path internally and exposes no override parameter; D7
  governs a non-terminating run there and the caller owns the residual.

## Command Reference

These forms are pinned once and referenced by the task bodies. A span in this preamble is not itself an
acceptance condition; each task restates the command it runs. Every command in this plan, including
every pwsh command, is issued from the worktree root, so the repository-relative paths passed to them
resolve against that root.

Toolchain resolution, re-derived inside every task that needs it:

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\Current\Bin\MSBuild.exe' | Select-Object -First 1
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
```

Base anchor, re-derived inside every task that runs a git diff:

```
$b = ((Select-String -Path 'docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/base-commit.md' -Pattern 'BaseCommit: ' -SimpleMatch | Select-Object -First 1).Line -split ' ')[-1]
```

Pinned test-case filter for every per-assembly run, per D5. It is one argument value on one line:

```
TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests
```

C# toolchain order, restarting from the first stage on any failure or any file rewrite:

1. `dotnet tool run csharpier format .`, verified with `dotnet tool run csharpier check .`
2. MSBuild on TaskMaster.sln with `/t:Rebuild`, EnableNETAnalyzers and EnforceCodeStyleInBuild
3. MSBuild on TaskMaster.sln with `/t:Rebuild` and TreatWarningsAsErrors
4. vstest.console.exe over the affected test assemblies, plus the coverage runner

## Literals This Plan Instructs The Executor To Create

These tokens do not exist in the tree today. They are quoted here verbatim so that a later search for
them is a real assertion rather than a search that cannot match.

- `_ownsCancelSource` — the private ownership field on the progress package type.
- `_ownsCancelSource = cancelSource is null;` — the assignment form at each construction site.
- `public class ProgressPackage : IDisposable` — the amended type declaration.
- `using var tokenSource = new CancellationTokenSource();` — the AC5 release shape.
- `DequeueAsync_ZeroAcceptedAndCapReached_LogsScanCapBoundAndStopDecision` — the AC1 test name.
- `DequeueAsync_ZeroAcceptedAndCeilingReached_LogsCeilingBoundNotScanCapBound` — the AC2 test name.
- `Dispose_WhenPackageConstructedTheSource_ReleasesIt` — the first AC4 test name.
- `Dispose_WhenCallerSuppliedTheSource_LeavesItUsable` — the second AC4 test name.
- `Dispose_OnSpawnedChild_DoesNotReleaseTheParentsSource` — the third AC4 test name.

None of the six field tokens below is asserted against the tree. Each is produced at run time by an
interpolated segment of the scan-bound message the gate composes, and the two bound values are the
string literals the gate assigns to its bound local. Three of the six do also occur as contiguous text
in the compiled C# sources of this repository, which is incidental and is not the basis of any
assertion here: the stop decision token is the literal tail of the gate's interpolated message at line
357 of the gate source; the zero-accepted token occurs inside an assertion in each of two existing
QuickFiler test part files; and the cutoff token occurs inside two assertions in one of them. The
remaining three occur in no compiled source. All six additionally occur in this plan file and in the
research artifact under this feature folder, both of which are markdown documents that no compiler
reads. The tests
assert all six against the captured message string and never against the tree, and they are quoted here
verbatim so that a reader can check them against the emitted line: `Accepted=0`, `Scanned=4`,
`Cutoff=900`, `Bound=scan-cap`, `Bound=zero-acceptance-ceiling` and `Decision=stop`. The log-line
selector the two new tests apply is the four-word phrase Zero-acceptance scan bound reached, which the
gate source carries on one line inside the interpolated message it composes. The sibling checkpoint
message opens with the phrase Zero-acceptance checkpoint, so the two share their first word and a
one-word filter does not discriminate between them; the two new tests apply the full four-word phrase.
Both phrases contain spaces and are therefore stated in prose rather than as whitespace-free tokens.

---

### Phase 0 — Baseline capture

Phase 0 runs no formatter and edits no source. A baseline captured after a write-mode formatter has
already repaired pre-existing drift is not a baseline, so the formatter is not invoked before P0-T5.
The two rebuild gates precede the test runs because vstest.console.exe never compiles: a test assembly
must already exist on disk before it is run.

- [ ] [P0-T1] Read the repository policy documents in the required order and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/phase0-instructions-read.md`.
  The order is the standing instructions file CLAUDE.md at the repository root, then the rules files
  general-code-change.md, general-unit-test.md, quality-tiers.md, csharp.md, tonality.md and
  plan-acceptance-gates.md under the dot-claude rules directory. Those seven paths are named in plain
  prose rather than in path formatting because they are read and never written by this delivery.
  Acceptance: the artifact exists and carries `Timestamp:`, a `Policy Order:` line, and an explicit
  bulleted list naming all seven files read, in the order above.

- [ ] [P0-T2] Record the base commit anchor into
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/base-commit.md`.

  ```
  git rev-parse HEAD
  git status --porcelain --untracked-files=all -- . ":(exclude).claude"
  ```

  Acceptance: the artifact exists and carries `Timestamp:`, `Command:`, `EXIT_CODE: 0`,
  `Output Summary:`, and a line of the exact form `BaseCommit: ` followed by the forty-character SHA
  printed by the first command. The artifact also records the porcelain output verbatim; a non-empty
  porcelain result is recorded rather than suppressed, and any pre-existing modification to a Write Set
  path is a BLOCKED condition reported to the caller.

- [ ] [P0-T3] Restore the pinned dotnet tool manifest and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/dotnet-tool-restore.md`.

  ```
  dotnet tool restore
  ```

  Acceptance: `EXIT_CODE: 0`, and the artifact carries a `CSharpierVersion:` line whose value is read
  from the tools manifest at the repository root. The success-case output prints a restored-tool line
  naming csharpier and a `Restore was successful.` line; the artifact quotes both in `Output Summary:`.
  The exit code alone is not the observation, because the command exits 0 whether or not it installed
  anything.

- [ ] [P0-T4] Restore NuGet packages for the packages.config projects and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/restore.md`.

  ```
  pwsh -File scripts/vscode/Invoke-Restore.ps1
  ```

  Acceptance: `EXIT_CODE: 0`, and the artifact carries `Timestamp:`, `Command:`, `Output Summary:` and
  a `PackagesDirectoryPresent:` line recording whether the repository-root packages directory exists on
  disk after the run, plus the installed package count printed by NuGet. An unbootstrapped worktree
  produces CS0006 reference errors in the later rebuild gates, so this task must complete before P0-T6.

- [ ] [P0-T5] Capture the CSharpier baseline in read-only check mode and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/csharpier-check.md`.

  ```
  dotnet tool run csharpier check .
  ```

  Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, a
  `CheckedFiles:` line transcribing the file count from the single summary line the tool prints in the
  form `Checked ` followed by a count and an elapsed time, and an `UnformattedFileList:` line naming
  every file the tool reported as not formatted, or the word none. The exit code is recorded as
  observed and is not asserted to be zero here; pre-existing formatter drift is a fact about the base
  tree that P0-T14 evaluates. The check subcommand is read-only and does not repair drift, which is
  why it and not the format subcommand is used for the baseline.

- [ ] [P0-T6] Capture the analyzer rebuild baseline and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/build-analyzers.md`.

  ```
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\Current\Bin\MSBuild.exe' | Select-Object -First 1
  & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults/msbuild/p0-t6-analyzers.txt;Verbosity=detailed"
  ```

  Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, a
  `WarningCount:` line and an `ErrorCount:` line, both transcribed from the MSBuild summary block.
  The counts are read from the summary lines that end in `Warning(s)` and `Error(s)` using a
  start-anchored match on the whole line, because a bare substring search for a zero-valued count also
  matches a ten-valued one. The artifact additionally records a non-vacuity observation: the detailed
  file log contains at least one line carrying the literal `/out:obj\Debug\UtilitiesCS.dll` and at
  least one carrying `/out:obj\Debug\UtilitiesCS.Test.dll`, which are csc command lines MSBuild echoes
  under each project's CoreCompile heading. That observation is what distinguishes a real compilation
  from a build whose CoreCompile was skipped as up to date, and the exit code cannot distinguish them.
  Per D10 the detailed log stays in the git-ignored TestResults directory and is not committed.

- [ ] [P0-T7] Capture the nullable rebuild baseline and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/build-nullable.md`.

  ```
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\Current\Bin\MSBuild.exe' | Select-Object -First 1
  & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=TestResults/msbuild/p0-t7-nullable.txt;Verbosity=detailed"
  ```

  Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, a
  `WarningCount:` line and an `ErrorCount:` line read by the same start-anchored match as P0-T6, and
  the same two non-vacuity log observations. Per D3 the command line contains no Nullable property.

- [ ] [P0-T8] Capture the baseline executed-test count for the UtilitiesCS test assembly and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/tests-utilitiescs.md`.

  ```
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/Logger:trx;LogFileName=p0-t8-baseline-utilitiescs.trx" /ResultsDirectory:TestResults\vstest\p0-t8
  ```

  Acceptance: `EXIT_CODE: 0`, and the artifact carries `Timestamp:`, `Command:`, `Output Summary:`, a
  `TotalTests:` line and a `Passed:` line transcribed from the `Total tests:` and `Passed:` lines the
  run prints, plus `Failed: 0` and `Skipped: 0` transcribed per D8 with the D8 reason stated. The
  artifact also records that the printed success header reads `Test Run Successful.` and that the
  passed count equals the total count. This `TotalTests:` value is the AC11 baseline for this assembly.

- [ ] [P0-T9] Capture the baseline executed-test count for the QuickFiler test assembly and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/tests-quickfiler.md`.

  ```
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/Logger:trx;LogFileName=p0-t9-baseline-quickfiler.trx" /ResultsDirectory:TestResults\vstest\p0-t9
  ```

  Acceptance: `EXIT_CODE: 0`, and the artifact carries the same field set as P0-T8, including a
  `TotalTests:` line. This value is the AC11 baseline for the QuickFiler test assembly.

- [ ] [P0-T10] Capture the repository-wide coverage baseline in Cobertura format, writing the raw XML
  to the transient git-ignored path `TestResults/coverage/coverage-baseline.cobertura.xml` and the step
  artifact to
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/coverage-baseline.md`.
  Per D10 the raw XML is transient tool output written outside the tracked tree and is never committed.
  It is read by P0-T11 and must therefore still exist on disk when P0-T11 runs. No task in this plan
  removes it and the executor does not remove it: it stays in the git-ignored directory for the
  remainder of the run, exactly as the MSBuild file logs and the vstest TRX files already do.

  ```
  pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput TestResults/coverage/coverage-baseline.cobertura.xml
  pwsh -Command '"TransientXml: " + (Test-Path -LiteralPath "TestResults/coverage/coverage-baseline.cobertura.xml" -PathType Leaf); "FeatureFolderXml: " + @(Get-ChildItem -LiteralPath "docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872" -Recurse -File -Filter "coverage-baseline.cobertura.xml").Count'
  ```

  Acceptance: `EXIT_CODE: 0` on the runner; the second span prints `TransientXml: True`, asserting that
  the transient XML exists at the ignored path, and `FeatureFolderXml: 0`, asserting that no file named
  `coverage-baseline.cobertura.xml` exists anywhere under the feature folder (the second span is a
  filesystem name enumeration by the PowerShell `-Filter` wildcard, not a text search, so no regex
  engine is involved); and the artifact carries `Timestamp:`,
  `Command:`, `Output Summary:` and numeric `LineRate:`, `LinesCovered:`, `LinesValid:`, `BranchRate:`,
  `BranchesCovered:`, `BranchesValid:` and `TestsPassed:` lines, all read from the root coverage element
  of the emitted XML and from the run's printed totals. The success-case output prints a line beginning
  `First-party coverage: ` carrying the same line and branch figures, and a final line beginning
  `Done. Coverage artifact: `; the artifact quotes both and records that the two figure sets reconcile.
  The `Output Summary:` field itself also carries the first-party line figure and the first-party branch
  figure as numbers, so the reduced audit can read the coverage headline from the summary without
  resolving a named field. Placeholder values are prohibited. Per D7, if the run has not terminated
  within ten minutes, halt, record the observed state and report BLOCKED.

- [ ] [P0-T11] Derive the per-file coverage baseline for `UtilitiesCS/Threading/ProgressPackage.cs` and
  write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/coverage-baseline-progresspackage.md`.

  ```
  pwsh -Command '. ./scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1; [xml]$x = Get-Content -Raw "TestResults/coverage/coverage-baseline.cobertura.xml"; $c = @($x.SelectNodes("//class") | Where-Object { $_.GetAttribute("filename") -like "*Threading\ProgressPackage.cs" }); "ClassElements: " + $c.Count; foreach ($n in $c) { $s = Get-CoberturaClassLineSummary -ClassNode $n; "LineRateAttribute: " + $n.GetAttribute("line-rate"); "TotalLines: " + $s.TotalLines; "CoveredLines: " + $s.CoveredLines }'
  ```

  Acceptance: `EXIT_CODE: 0`, and the artifact carries `Timestamp:`, `Command:`, `Output Summary:`, a
  `ClassElements:` line, and numeric `TotalLines:`, `CoveredLines:` and `LineRateAttribute:` lines. The
  derivation is stated in the artifact: figures come from the helper function named
  Get-CoberturaClassLineSummary, which de-duplicates by line number, because a Cobertura class element
  repeats every line under its method tree and again in its class-level rollup and a descendant-axis
  count therefore double-counts. Where more than one class element resolves to that filename, the
  per-class line maps are merged by line number with a repeated line number resolved by the maximum
  hits value, and the merge is stated even when the class-element count is one. This is the AC12
  baseline and it is captured before any Phase 1 edit, so it describes the pre-change file.

- [ ] [P0-T12] Capture the Compile-item count baseline for the two project files and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/compile-item-counts.md`.

  ```
  pwsh -Command '"UtilitiesCS.csproj Include: " + @(Select-String -Path "UtilitiesCS/UtilitiesCS.csproj" -Pattern "<Compile Include=" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.csproj Element: " + @(Select-String -Path "UtilitiesCS/UtilitiesCS.csproj" -Pattern "<Compile" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.Test.csproj Include: " + @(Select-String -Path "UtilitiesCS.Test/UtilitiesCS.Test.csproj" -Pattern "<Compile Include=" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.Test.csproj Element: " + @(Select-String -Path "UtilitiesCS.Test/UtilitiesCS.Test.csproj" -Pattern "<Compile" -SimpleMatch -CaseSensitive).Count'
  ```

  Acceptance: `EXIT_CODE: 0`, and the artifact records four numbers under the field names
  `UtilitiesCsIncludeCount:`, `UtilitiesCsElementCount:`, `UtilitiesCsTestIncludeCount:` and
  `UtilitiesCsTestElementCount:`. The expected baseline is 492 for both counts on
  `UtilitiesCS/UtilitiesCS.csproj` and 477 for both counts on
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. These two expectations were 491 and 476 when this plan
  was authored and were each raised by one when the mandated reconciliation merged the current main
  branch into this item's branch: that merge added exactly one Compile item to each of the two project
  files. The research artifact under this feature folder still records 491 and 476, which is correct as
  a measurement of the tree at its own timestamp and is deliberately not rewritten; the figures above
  are the operative ones. If an observed number differs from the expected value,
  the observed number is recorded as the operative baseline and the divergence is reported to the
  caller before Phase 1 begins. The two patterns agreeing establishes that every Compile element uses
  the Include attribute and that no Update or Remove form exists, so a single attribute-form count is a
  sound basis for the AC7 comparison.

- [ ] [P0-T13] Pin the source facts the later comparisons depend on and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/pinned-source-facts.md`.

  ```
  pwsh -Command '"TestMethodCount: " + @(Select-String -Path "UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs" -Pattern "TestMethod" -SimpleMatch -CaseSensitive).Count; "DataRowCount: " + @(Select-String -Path "UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs" -Pattern "DataRow" -SimpleMatch -CaseSensitive).Count; "CoverageAttributeCount: " + @(Select-String -Path "UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs" -Pattern "[ExcludeFromCodeCoverage]" -SimpleMatch -CaseSensitive).Count; "Part4Lines: " + (Get-Content -Path "QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs").Count; "ProgressPackageTestsLines: " + (Get-Content -Path "UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs").Count; "ProgressPackageLines: " + (Get-Content -Path "UtilitiesCS/Threading/ProgressPackage.cs").Count; "SubjectMapOrchestrationLines: " + (Get-Content -Path "UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs").Count'
  ```

  Acceptance: `EXIT_CODE: 0`, and the artifact records all seven values. Three of them are hard
  expectations and a divergence on any of them is reported to the caller before Phase 1 begins: 9 test
  methods and 0 data rows in `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, so its
  executed-test count equals its method count and the AC11 removal term is minus nine; and 4 coverage
  attributes in `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs`, which is the
  value P2-T12 compares against. The four line counts are recorded as measured and are the comparison
  base for P2-T17; their expected approximate values are 347 for
  `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`, 120 for
  `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs`, 150 for
  `UtilitiesCS/Threading/ProgressPackage.cs` and 274 for
  `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs`. A difference of one line
  on any of the four is attributable to the trailing-newline counting convention of the measuring tool
  and is not a divergence; a difference greater than one is. The artifact additionally
  quotes the two Compile lines to be removed verbatim, each with four leading spaces and in the
  single-line self-closing form: the line at `UtilitiesCS/UtilitiesCS.csproj` line 971 reads
  `<Compile Include="Threading\ProgressTrackerAsync.cs" />` and the line at
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj` line 496 reads
  `<Compile Include="Threading\ProgressTrackerAsync_Tests.cs" />`. It also records the neighbours that
  must not be touched: line 970 and line 972 of the first file, and line 495 and line 497 of the
  second. A divergence between an observed value and an expected value is recorded and reported before
  Phase 1 begins.

- [ ] [P0-T14] Evaluate the Phase 0 halt gate and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/phase0-gate.md`.
  Acceptance: the artifact tabulates the recorded `EXIT_CODE:` of P0-T5, P0-T6, P0-T7, P0-T8, P0-T9 and
  P0-T10 and states, per row, whether it is zero. If every row is zero the artifact records
  `PHASE0_GATE: GREEN` and Phase 1 proceeds. If any row is non-zero the artifact records
  `PHASE0_GATE: RED`, names every failing row with its observed value, and the executor halts and
  reports BLOCKED to the caller without beginning Phase 1. This gate exists because an admitted red
  baseline would make every Phase 2 exit-zero demand unmeetable by any work this plan performs, so the
  divergence must be resolved by the caller before implementation starts rather than surfacing as a
  false Phase 2 failure.

---

### Phase 1 — Delegated implementation

The three defects are independent and are implemented as three task groups. No gate runs between
P1-T10 and P1-T13, and that span deliberately carries one state that does not compile: once P1-T11
has deleted the dormant tracker source, the test project still names the dormant tracker's test
source in a Compile item and that test source constructs the deleted type, so the test assembly does
not compile until P1-T12 removes the item. Each Compile item is removed immediately before the file
it names is deleted, which is what confines the span to that single non-compiling state, and P1-T14
is the first gate after it.

- [ ] [P1-T1] Record the implementation handoff to the small-path C# implementation engineer persona
  csharp-typed-engineer and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/other/implementation-handoff.md`.
  Acceptance: the artifact exists and carries `Timestamp:`, the engineer persona name, the eight Write
  Set paths reproduced verbatim, the Scope Boundary restated, and the completion criteria for the
  handoff, which are that tasks P1-T2 through P1-T17 are each complete and that no file outside the
  Write Set has been modified. Tests use MSTest, Moq and FluentAssertions; no test creates a temporary
  file, sleeps, waits on a wall clock or touches an external process.

- [ ] [P1-T2] In `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs` add one
  test method named `DequeueAsync_ZeroAcceptedAndCapReached_LogsScanCapBoundAndStopDecision` that
  drives the gate to the item-cap bound with a debugLog delegate injected. It reuses the existing
  arrangement of the neighbouring cap test in the same file: a counting take delegate over a ten-item
  candidate queue, a score loader returning 100, a threshold of 0.90, a fake time provider that is
  never advanced, a source-active delegate returning true, and a maximum scan without acceptance of 4.
  It additionally passes a string-collecting delegate as the debugLog argument. Acceptance: the test
  filters the captured log list on lines containing the four-word phrase that opens the scan-bound
  message, which is Zero-acceptance scan bound reached, and asserts that exactly one line matches, then
  asserts that the single matching line contains `Accepted=0`, `Scanned=4`, `Cutoff=900`,
  `Bound=scan-cap` and `Decision=stop`. The full four-word phrase is required rather than its first
  word alone, because the checkpoint message opens with the same first word. In this arrangement the
  fake clock is never advanced, so the checkpoint interval never elapses and no checkpoint line is
  emitted; the filter is a guard that keeps the single-match assertion correct under a future
  arrangement in which both lines are emitted. The filter is applied before the field assertions
  because the launch line this run does emit carries `Cutoff=900` and a scan-cap bound field spelled
  ScanCap, so an unfiltered field assertion could be satisfied by the wrong line.
  No acceptance condition
  asserts an exact total count of captured log lines, because that total is not established. This test
  is expected to pass immediately; Defect A is test-only and the production log line already emits
  every field asserted here. Evidence: this task adds no artifact of its own; P1-T16 is its run.

- [ ] [P1-T3] In `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs` add one
  test method named `DequeueAsync_ZeroAcceptedAndCeilingReached_LogsCeilingBoundNotScanCapBound` that
  drives the gate to the time-ceiling bound with a debugLog delegate injected. It reuses the existing
  arrangement of the neighbouring ceiling test in the same file: a take delegate returning null, a
  score loader returning 950 that is never invoked, a threshold of 0.90, a fake time provider, a
  source-active delegate returning true, and a zero-acceptance ceiling of 120 seconds, with the
  maximum scan without acceptance left at its default. It starts the dequeue without awaiting, asserts
  the returned task is not complete, advances the fake clock by 121 seconds, then awaits. Acceptance:
  the test filters the captured log list on lines containing the four-word phrase that opens the
  scan-bound message, which is Zero-acceptance scan bound reached, asserts that exactly one line
  matches, asserts that the single matching line contains `Bound=zero-acceptance-ceiling`, and asserts
  that it does not contain `Bound=scan-cap`. The full four-word phrase is required rather than its
  first word alone, because the checkpoint message opens with the same first word. In this arrangement
  the checkpoint line is not emitted: the loop iteration that follows the clock advance evaluates the
  two bounds before the checkpoint interval and returns at the bound, so the checkpoint branch is never
  reached. The filter is therefore a guard that keeps the single-match assertion correct under a future
  arrangement in which both lines are emitted, rather than a filter against a line present today.
  The negative assertion is the discriminating one: a regression that collapsed the two bounds to one
  value would emit the item-cap token and would still satisfy a presence-only assertion. Evidence:
  this task adds no artifact of its own; P1-T16 is its run.

- [ ] [P1-T4] In `UtilitiesCS/Threading/ProgressPackage.cs` add the ownership mechanism. Add a private
  boolean field `_ownsCancelSource`. At each of the two construction sites inside the two
  InitializeAsync overloads, immediately alongside the null-coalescing construction of the
  cancellation token source, add the assignment `_ownsCancelSource = cancelSource is null;`. Change the
  type declaration to `public class ProgressPackage : IDisposable` and add a public Dispose method that
  releases the held source only when the ownership field is true and then clears the field. Dispose
  must not set the held source reference to null, because the public getter's observable behaviour for
  existing callers would change and the deterministic probe the P1-T6 through P1-T8 tests rely on would
  be removed. Acceptance: the ownership field is assigned in exactly two places, both inside
  InitializeAsync overloads and both in the assignment form rather than a conditional set, and it is
  never assigned inside the public CancelSource property setter. The assignment form is required so
  that a second InitializeAsync call with an injected source clears a previously claimed ownership. The
  setter exclusion is required because the spawn-child path copies the parent's source through that
  setter using an object initializer, so a setter that claimed ownership would make every child claim
  its parent's source. P2-T13 verifies both structurally.

- [ ] [P1-T5] In `UtilitiesCS/Threading/ProgressPackage.cs` add an XML doc comment to each of the two
  static tuple factory methods stating that the cancellation token source in the returned tuple is
  transferred to the caller and that the caller owns its release. Acceptance: both static factory
  methods carry a summary element whose text states the transfer, and no executable statement in either
  method is changed. This records in code the residual that the Scope Boundary describes, and it is the
  only in-Write-Set way to record it. This task adds no dispose call inside either factory: disposing
  there would release the source the factory is contractually returning.

- [ ] [P1-T6] In `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs` add one test method named
  `Dispose_WhenPackageConstructedTheSource_ReleasesIt`. It awaits the tracker overload of
  InitializeAsync with a null cancelSource argument, a non-null injected progressTracker argument
  constructed from a locally created source, and an explicit stopWatch argument, then captures the
  source the package constructed from the public getter and disposes the package. Acceptance: the test
  asserts that reading the captured source's Token property after disposal throws
  ObjectDisposedException. The overload is disambiguated by naming the progressTracker parameter,
  because a null third argument is otherwise ambiguous between the two overloads. A non-null tracker
  and an explicit stop watch are both required so that no dispatcher is touched and no background task
  is started, which is what keeps the test headless and deterministic. The probe is the token getter
  and never a timer or a finalizer.

- [ ] [P1-T7] In `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs` add one test method named
  `Dispose_WhenCallerSuppliedTheSource_LeavesItUsable`. It awaits the tracker overload of
  InitializeAsync with a caller-created cancelSource argument, a non-null injected progressTracker
  argument and an explicit stopWatch argument, then disposes the package. Acceptance: the test asserts
  that reading the caller's source Token property after disposal does not throw and that calling Cancel
  on that source sets IsCancellationRequested to true. A caller-supplied source belongs to that caller
  and must never be released by the package.

- [ ] [P1-T8] In `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs` add one test method named
  `Dispose_OnSpawnedChild_DoesNotReleaseTheParentsSource`. It awaits the tracker overload of
  InitializeAsync with a null cancelSource argument, a non-null injected progressTracker argument and
  an explicit stopWatch argument so that the parent constructs and owns a source, spawns a child, and
  disposes only the child. Acceptance: the test asserts that reading the parent's source Token property
  after the child is disposed does not throw. Ownership is never transferred by assignment, so a child
  that received the reference through the property setter owns nothing and its disposal is a no-op on
  the source.

- [ ] [P1-T9] In `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs` convert the
  local cancellation token source construction inside the rebuild method from a plain local declaration
  to a using declaration, so that the line reads
  `using var tokenSource = new CancellationTokenSource();`. Acceptance: the change is a one-line
  replacement at the construction site and nothing else in the method moves. A using declaration lowers
  to a try and finally with no catch, so it releases on the completing path and on the faulting path
  alike and does not swallow any exception raised by the awaited long-running task. The release point is
  the end of the method scope, which is strictly after the last use of the token, which is the token
  argument to the task factory call, and strictly after the last use of the tracker that holds the
  source, which is the progress report of 100. No explicit dispose call is placed before the progress
  report of 100: doing so would release the source while the root progress viewer is still open and
  would silently make its cancel button inert. P2-T12 verifies the ordering structurally. Per D11 this
  criterion carries no test obligation and no coverage obligation.

- [ ] [P1-T10] In `UtilitiesCS/UtilitiesCS.csproj` remove the single Compile item naming the dormant
  tracker source. The line to remove is the single-line self-closing element at line 971 whose text is
  `<Compile Include="Threading\ProgressTrackerAsync.cs" />` with four leading spaces. Acceptance:
  exactly one line is removed, the count of lines matching the literal `<Compile Include=` in that file
  falls by exactly one relative to the P0-T12 baseline, and the two neighbouring Compile items at the
  former line 970 and line 972 are unchanged. No other Compile item may be dropped.

- [ ] [P1-T11] Delete `UtilitiesCS/Threading/ProgressTrackerAsync.cs` from the working tree.
  Acceptance: the path does not exist on disk after the task. The type has no construction site outside
  its own declaration file and the test file deleted by P1-T13; the only other occurrence in the tree
  is prose inside a code tag in an XML doc comment in the ProgressTracker report-and-viewer test file,
  which is not a compile-time dependency, and there is no cref-form reference to the type anywhere.

- [ ] [P1-T12] In `UtilitiesCS.Test/UtilitiesCS.Test.csproj` remove the single Compile item naming the
  dormant tracker's test source. The line to remove is the single-line self-closing element at line 496
  whose text is `<Compile Include="Threading\ProgressTrackerAsync_Tests.cs" />` with four leading
  spaces. Acceptance: exactly one line is removed, the count of lines matching the literal
  `<Compile Include=` in that file falls by exactly one relative to the P0-T12 baseline, and the two
  neighbouring Compile items at the former line 495 and line 497 are unchanged.

- [ ] [P1-T13] Delete `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` from the working tree.
  Acceptance: the path does not exist on disk after the task. Its removal takes with it the nine test
  methods pinned by P0-T13 and one class-level parallelization attribute scoped to that class alone.

- [ ] [P1-T14] Run a progress rebuild of the solution to confirm the tree compiles after the three
  defect groups, and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/other/p1-t14-interim-build.md`.

  ```
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\Current\Bin\MSBuild.exe' | Select-Object -First 1
  & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults/msbuild/p1-t14-interim.txt;Verbosity=detailed"
  ```

  Acceptance: `EXIT_CODE: 0`, and the artifact carries `Timestamp:`, `Command:`, `Output Summary:` and
  start-anchored `ErrorCount: 0`. This is a progress gate, not an acceptance-bearing gate: it runs
  before the formatter, so its result is superseded by P2-T3. It exists so that a compile break is
  found here rather than after the Phase 2 loop has begun. It also produces the assemblies that P1-T15
  and P1-T16 require, because vstest.console.exe never compiles.

- [ ] [P1-T15] Run the three new disposal tests in the UtilitiesCS test assembly and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/regression-testing/p1-t15-utilitiescs-scoped.md`.

  ```
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~Dispose_WhenPackageConstructedTheSource_ReleasesIt|FullyQualifiedName~Dispose_WhenCallerSuppliedTheSource_LeavesItUsable|FullyQualifiedName~Dispose_OnSpawnedChild_DoesNotReleaseTheParentsSource" "/Logger:trx;LogFileName=p1-t15-scoped-utilitiescs.trx" /ResultsDirectory:TestResults\vstest\p1-t15
  ```

  Acceptance: the artifact carries `Timestamp:`, `Command:` and `Output Summary:`; `EXIT_CODE: 0` and
  the artifact records `TotalTests: 3` and `Passed: 3`. A run whose
  total is anything other than 3 is a failure and not a pass: vstest reports a zero-match filter
  without an obvious error, so the total is asserted explicitly. Filter clauses are joined with the
  vertical bar because this version of the test platform rejects the word OR inside a test case filter.

- [ ] [P1-T16] Run the two new log-assertion tests in the QuickFiler test assembly and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/regression-testing/p1-t16-quickfiler-scoped.md`.

  ```
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~DequeueAsync_ZeroAcceptedAndCapReached_LogsScanCapBoundAndStopDecision|FullyQualifiedName~DequeueAsync_ZeroAcceptedAndCeilingReached_LogsCeilingBoundNotScanCapBound" "/Logger:trx;LogFileName=p1-t16-scoped-quickfiler.trx" /ResultsDirectory:TestResults\vstest\p1-t16
  ```

  Acceptance: the artifact carries `Timestamp:`, `Command:` and `Output Summary:`; `EXIT_CODE: 0` and
  the artifact records `TotalTests: 2` and `Passed: 2`. A run whose
  total is anything other than 2 is a failure and not a pass.

- [ ] [P1-T17] Record the fail-before exception for Defect B and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/regression-testing/fail-before-exception.2026-09-12T10-26.md`.
  Acceptance: the artifact carries `Timestamp:`, a `WhyFailingRunImpossible:` field of one to three
  sentences stating that the three AC4 tests call a Dispose method that does not exist on the progress
  package before P1-T4, so the test assembly does not compile and the tests cannot be observed to fail
  at run time; an absence-of-test proof recording that the four pre-existing tests in
  `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs` make no disposal assertion; and a
  `SearchScope:`, `SearchPatterns:` and `SearchResult:` triple covering the regression-testing evidence
  folder. It also records that Defect A carries no fail-before obligation, because the gate's
  production source already emits every asserted field and the defect is a missing assertion rather
  than a behavioural fault, and that Defect C carries none, because it is a deletion.

---

### Phase 2 — Final QC loop and acceptance verification

The four toolchain stages run in the order P2-T1, P2-T2, P2-T3, P2-T4, then the test stage P2-T5
through P2-T7. If any of those stages fails, or if P2-T1 rewrites any file, the loop restarts from
P2-T1 and every restarted step overwrites its artifact with the values of the pass that completed the
phase, recording the superseded pass in the same artifact. Every command task in this phase is
unconditional: none carries an in-scope or out-of-scope branch and none has a skipped completion path.

- [ ] [P2-T1] Run the formatter over the tree and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/qc-csharpier-format.md`.

  ```
  $b = ((Select-String -Path 'docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/base-commit.md' -Pattern 'BaseCommit: ' -SimpleMatch | Select-Object -First 1).Line -split ' ')[-1]
  git add --intent-to-add -- . ":(exclude).claude" ":(exclude)docs/features/potential"
  git diff --numstat $b -- . ":(exclude).claude" ":(exclude)docs/features/potential"
  dotnet tool run csharpier format .
  git add --intent-to-add -- . ":(exclude).claude" ":(exclude)docs/features/potential"
  git diff --numstat $b -- . ":(exclude).claude" ":(exclude)docs/features/potential"
  ```

  Acceptance: `EXIT_CODE: 0` for the formatter, and the artifact carries `Timestamp:`, `Command:`,
  `Output Summary:`, a `FormattedFileCount:` line transcribed from the single summary line the tool
  prints in the form `Formatted ` followed by a count and an elapsed time, and a `ChangedFileCount:`
  line holding the number of paths whose insertion or deletion figure differs between the two numstat
  runs. The falsifiable observation is that before-and-after comparison and not the exit code: the
  format subcommand exits 0 both when it changed nothing and when it repaired drift, so the exit code
  is identical on a clean run and a repairing one. The processed count is likewise not the observation;
  it counts files visited, not files repaired, so using it as a restart trigger would never terminate.
  The restart trigger is a non-zero `ChangedFileCount:`. A porcelain status is not usable here because
  every Write Set file is already modified before the formatter runs and would already be listed. The
  intent-to-add span precedes each numstat because a numstat enumerates tracked changes only, and any
  file this plan created and has not yet committed would otherwise be invisible to both runs. The
  anchor is the base commit recorded by P0-T2; an unanchored diff compares the worktree against the
  index and passes vacuously once the change is committed. Every span here excludes the agent-memory
  tree, which the executor writes to during the run, and the potential features directory. The
  intent-to-add spans must carry the second exclusion for the same reason the P2-T32 porcelain span
  carries it: intent-to-add is a staging operation, and in a parallel run other items queue their own
  untracked promotion files in that directory, so an unscoped span places another item's bookkeeping in
  this worktree's index. The same exclusion on the two numstat spans is inert for the comparison,
  because a path excluded from both runs contributes the same figure to each, and it keeps all four
  spans identical in scope.

- [ ] [P2-T2] Verify formatting in read-only mode and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/qc-csharpier-check.md`.

  ```
  dotnet tool run csharpier check .
  ```

  Acceptance for AC8: `EXIT_CODE: 0`, and the artifact carries `CheckedFiles:` transcribed from the
  single summary line the tool prints in the form `Checked ` followed by a count and an elapsed time,
  and `UnformattedFileList: none`. The success-case output of this subcommand is that summary line with
  no per-file not-formatted line, which is the observation beyond the exit code. The checked count must
  be at least one, because a run that visited no file would also exit 0 and would report no unformatted
  file.

- [ ] [P2-T3] Run the analyzer rebuild gate and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/qc-build-analyzers.md`.

  ```
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\Current\Bin\MSBuild.exe' | Select-Object -First 1
  & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults/msbuild/p2-t3-analyzers.txt;Verbosity=detailed"
  ```

  Acceptance for AC9: `EXIT_CODE: 0`; the artifact records a start-anchored `ErrorCount: 0` and quotes
  the `Build succeeded` line; the artifact records a `WarningCount:` value that does not exceed the
  P0-T6 baseline warning count; and the artifact records the non-vacuity observation that the detailed
  file log contains at least one line carrying the literal `/out:obj\Debug\UtilitiesCS.dll` and at
  least one carrying `/out:obj\Debug\UtilitiesCS.Test.dll`. The error count is matched on the whole
  summary line rather than as a bare substring, because a zero-valued count is a substring of a
  ten-valued one. The non-vacuity observation is what discharges AC9's requirement that compilation
  actually ran rather than being skipped as up to date; per D2 the exit code of a warm build cannot
  discharge it. Per D10 the log is not committed.

- [ ] [P2-T4] Run the nullable rebuild gate and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/qc-build-nullable.md`.

  ```
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\Current\Bin\MSBuild.exe' | Select-Object -First 1
  & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=TestResults/msbuild/p2-t4-nullable.txt;Verbosity=detailed"
  ```

  Acceptance for AC10: `EXIT_CODE: 0`; the artifact records a start-anchored `ErrorCount: 0` and quotes
  the `Build succeeded` line; the artifact records the same two non-vacuity log observations as P2-T3;
  and the artifact records explicitly that the command line contains no Nullable property, per D3.
  Under this gate a warning is promoted to an error, so a zero error count is the operative signal.

- [ ] [P2-T5] Run the UtilitiesCS test assembly and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/qc-tests-utilitiescs.md`.

  ```
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/Logger:trx;LogFileName=p2-t5-final-utilitiescs.trx" /ResultsDirectory:TestResults\vstest\p2-t5
  ```

  Acceptance: `EXIT_CODE: 0`, the printed success header reads `Test Run Successful.`, and the artifact
  records `TotalTests:` and `Passed:` transcribed from the run with the two values equal, plus
  `Failed: 0` and `Skipped: 0` transcribed per D8 with the D8 reason stated. The filter string is
  byte-identical to the P0-T8 filter string, which is what makes the AC11 comparison valid. D9 governs
  a single sporadic failure of the named dictionary-extensions test.

- [ ] [P2-T6] Run the QuickFiler test assembly and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/qc-tests-quickfiler.md`.

  ```
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/Logger:trx;LogFileName=p2-t6-final-quickfiler.trx" /ResultsDirectory:TestResults\vstest\p2-t6
  ```

  Acceptance: `EXIT_CODE: 0`, the printed success header reads `Test Run Successful.`, and the artifact
  records `TotalTests:` and `Passed:` with the two values equal, plus `Failed: 0` and `Skipped: 0`
  transcribed per D8. The filter string is byte-identical to the P0-T9 filter string.

- [ ] [P2-T7] Capture post-change coverage in Cobertura format, writing the raw XML to the transient
  git-ignored path `TestResults/coverage/coverage-postchange.cobertura.xml` and the step artifact to
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/qc-coverage-postchange.md`.
  Per D10 the raw XML is transient tool output written outside the tracked tree and is never committed.
  It is read by P2-T9 and must therefore still exist on disk when P2-T9 runs. No task in this plan
  removes it and the executor does not remove it: it stays in the git-ignored directory for the
  remainder of the run, exactly as the MSBuild file logs and the vstest TRX files already do.

  ```
  pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput TestResults/coverage/coverage-postchange.cobertura.xml
  pwsh -Command '"TransientXml: " + (Test-Path -LiteralPath "TestResults/coverage/coverage-postchange.cobertura.xml" -PathType Leaf); "FeatureFolderXml: " + @(Get-ChildItem -LiteralPath "docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872" -Recurse -File -Filter "coverage-postchange.cobertura.xml").Count'
  ```

  Acceptance: `EXIT_CODE: 0` on the runner; the second span prints `TransientXml: True`, asserting that
  the transient XML exists at the ignored path, and `FeatureFolderXml: 0`, asserting that no file named
  `coverage-postchange.cobertura.xml` exists anywhere under the feature folder (a filesystem name
  enumeration by the PowerShell `-Filter` wildcard, not a text search, so no regex engine is involved);
  and the artifact carries numeric `LineRate:`,
  `LinesCovered:`, `LinesValid:`, `BranchRate:`, `BranchesCovered:`, `BranchesValid:` and
  `TestsPassed:` lines read from the root coverage element and the run's printed totals, quoting the
  line beginning `First-party coverage: ` and the line beginning `Done. Coverage artifact: `. The
  `Output Summary:` field itself also carries the first-party line figure and the first-party branch
  figure as numbers, so the reduced audit can read the coverage headline from the summary without
  resolving a named field.
  Placeholder values are prohibited. The artifact additionally records the repository-wide line rate
  against the P0-T10 value and notes that deleting the dormant tracker removes a production file from
  the denominator, so an upward movement in the headline percentage is expected and is the stated
  purpose of the dormant-code issue. Per D7 a non-terminating run is halted at ten minutes and reported
  BLOCKED.

- [ ] [P2-T8] Verify the AC11 per-assembly test-count deltas and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac11-test-count-delta.md`.
  Acceptance for AC11: the artifact tabulates, for each of the two assemblies, the P0 baseline
  `TotalTests:` value, the P2 value, and their difference; the UtilitiesCS test assembly difference is
  exactly minus six and the QuickFiler test assembly difference is exactly plus two; and both runs
  recorded `Failed: 0`. The artifact states the arithmetic: minus nine for the nine test methods
  removed with the dormant tracker's test class, pinned by P0-T13 with a data-row count of zero so the
  executed count equals the method count, plus three for the tests added by P1-T6, P1-T7 and P1-T8,
  giving minus six; and plus two for the tests added by P1-T2 and P1-T3. A difference other than the
  stated one fails this task.

- [ ] [P2-T9] Derive post-change per-file coverage for `UtilitiesCS/Threading/ProgressPackage.cs`,
  compare it against the P0-T11 baseline, and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac12-progresspackage-coverage.md`.

  ```
  $b = ((Select-String -Path 'docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/base-commit.md' -Pattern 'BaseCommit: ' -SimpleMatch | Select-Object -First 1).Line -split ' ')[-1]
  git diff -U0 $b -- UtilitiesCS/Threading/ProgressPackage.cs
  pwsh -Command '. ./scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1; [xml]$x = Get-Content -Raw "TestResults/coverage/coverage-postchange.cobertura.xml"; $c = @($x.SelectNodes("//class") | Where-Object { $_.GetAttribute("filename") -like "*Threading\ProgressPackage.cs" }); "ClassElements: " + $c.Count; foreach ($n in $c) { $s = Get-CoberturaClassLineSummary -ClassNode $n; "LineRateAttribute: " + $n.GetAttribute("line-rate"); "TotalLines: " + $s.TotalLines; "CoveredLines: " + $s.CoveredLines; foreach ($k in ($s.LineMap.Keys | Sort-Object)) { "Line " + $k + " hits " + $s.LineMap[$k].Hits } }'
  ```

  Acceptance for AC12: the artifact records `BaselineCoveredLines:`, `BaselineTotalLines:`,
  `PostChangeCoveredLines:` and `PostChangeTotalLines:`, and the post-change covered-to-total ratio is
  greater than or equal to the baseline ratio. The artifact additionally enumerates the added and
  changed line numbers of that file, read from the plus-side hunk headers of the anchored unified diff
  with zero context, and for each one records either its hits value from the post-change line map or,
  where the line is absent from the line map, its text and a one-line statement of why it is
  non-executable. The artifact must show a hits value greater than zero for the ownership assignment
  line inside the tracker overload of InitializeAsync and for every line of the Dispose body; those are
  the minimum floor and a zero hits value on any of them fails this task. The ownership assignment line
  inside the pane overload is recorded with its observed hits value and is deliberately excluded from
  the floor: the pane overload is reached only through CreateAsTuplePaneAsync, whose production call
  sites all read the application-globals progress tracker and are host-bound, so no unit test executes
  it. AC4 fixes the three new tests to the tracker overload and AC11 pins the UtilitiesCS executed-test
  delta at exactly plus three, so a fourth test covering the pane overload is outside this delivery and
  the uncovered line is a known residual rather than a gate failure. The diff is
  anchored to the base commit recorded by P0-T2 rather than being left unanchored, because an
  unanchored diff compares the worktree against the index and would pass vacuously after a commit. The
  derivation matches P0-T11 exactly, including the de-duplicating helper and the merge-by-line-number
  rule, so the two figures are comparable. Line numbers are not compared across the change: the file
  grows, so a per-line-number comparison would be meaningless and only the aggregate ratio and the
  new-line floor are load-bearing. The artifact additionally records the line count and covered-line
  count of the Dispose method body taken from the post-change line map, and their ratio is at least
  0.90, which is the repository floor for a newly added method. Both branches of the ownership test are
  exercised: the owned path by P1-T6 and by the parent in P1-T8, and the not-owned path by P1-T7 and by
  the child in P1-T8.

- [ ] [P2-T10] Verify the AC7 Compile-item counts and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac7-compile-item-counts.md`.

  ```
  pwsh -Command '"UtilitiesCS.csproj Include: " + @(Select-String -Path "UtilitiesCS/UtilitiesCS.csproj" -Pattern "<Compile Include=" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.csproj Element: " + @(Select-String -Path "UtilitiesCS/UtilitiesCS.csproj" -Pattern "<Compile" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.Test.csproj Include: " + @(Select-String -Path "UtilitiesCS.Test/UtilitiesCS.Test.csproj" -Pattern "<Compile Include=" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.Test.csproj Element: " + @(Select-String -Path "UtilitiesCS.Test/UtilitiesCS.Test.csproj" -Pattern "<Compile" -SimpleMatch -CaseSensitive).Count; "TrackerReferences: " + @(Select-String -Path "UtilitiesCS/UtilitiesCS.csproj","UtilitiesCS.Test/UtilitiesCS.Test.csproj" -Pattern "ProgressTrackerAsync" -SimpleMatch -CaseSensitive).Count'
  ```

  Acceptance for AC7: each of the four counts is exactly one lower than the corresponding P0-T12
  baseline value, and `TrackerReferences:` is 0. A fall of more than one in either file means a sibling
  Compile item was dropped and fails this task. A fall of zero means the item was not removed.

- [ ] [P2-T11] Verify the AC6 deletions and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac6-deletions.md`.

  ```
  $b = ((Select-String -Path 'docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/base-commit.md' -Pattern 'BaseCommit: ' -SimpleMatch | Select-Object -First 1).Line -split ' ')[-1]
  git add --intent-to-add -- . ":(exclude).claude" ":(exclude)docs/features/potential"
  git status --porcelain --untracked-files=all -- UtilitiesCS/Threading/ProgressTrackerAsync.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
  git diff --name-status $b -- UtilitiesCS/Threading/ProgressTrackerAsync.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
  ```

  Acceptance for AC6: neither path exists on disk; the name-status diff prints exactly two lines and
  each begins with the deletion status letter D; and the porcelain span is recorded verbatim. The
  staging and porcelain spans accompany the name-listing diff because a name-listing diff enumerates
  tracked changes only and is blind to an untracked path, and the two mechanisms are complementary
  rather than redundant: porcelain goes empty once the change is committed and the anchored diff does
  not. The intent-to-add span excludes the potential features directory for the reason P2-T32 gives:
  intent-to-add is a staging operation, and an unscoped span places another item's queued promotion
  file in this worktree's index. The diff carries the base commit recorded by P0-T2 as its ref
  operand.

- [ ] [P2-T12] Verify AC5 structurally and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac5-using-declaration.md`.

  ```
  $b = ((Select-String -Path 'docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/base-commit.md' -Pattern 'BaseCommit: ' -SimpleMatch | Select-Object -First 1).Line -split ' ')[-1]
  git diff -U0 $b -- UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs
  pwsh -Command '$p = "UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs"; "Using: " + (@(Select-String -Path $p -Pattern "using var tokenSource = new CancellationTokenSource();" -SimpleMatch -CaseSensitive) | ForEach-Object { $_.LineNumber }); "Report: " + (@(Select-String -Path $p -Pattern "progress.Report(100);" -SimpleMatch -CaseSensitive) | ForEach-Object { $_.LineNumber }); "Attribute: " + (@(Select-String -Path $p -Pattern "[ExcludeFromCodeCoverage]" -SimpleMatch -CaseSensitive).Count); "ExplicitDispose: " + @(Select-String -Path $p -Pattern "tokenSource.Dispose()" -SimpleMatch -CaseSensitive).Count'
  ```

  Acceptance for AC5: the anchored diff shows exactly one added line and exactly one removed line for
  that file; the added line is the using declaration whose text this plan quotes in the literals
  section; the recorded `Using:` and `Report:` line numbers both fall inside the rebuild method body,
  whose opening and closing line numbers the artifact records, so the end-of-scope release point is the
  method's closing brace and is strictly after the progress report of 100; and the file contains zero
  occurrences of the literal `tokenSource.Dispose()`, which is the check that can fail if an explicit
  release is inserted ahead of the report and silently makes the root progress viewer's cancel button
  inert; and the recorded `Attribute:` count is exactly 4, matching the
  `CoverageAttributeCount:` value pinned by P0-T13, so no coverage attribute was added or removed. Per
  D11 no coverage figure is asserted for this file, because the
  rebuild method carries the coverage-exclusion attribute and an excluded member is absent from the
  report rather than reported at zero. The diff carries the base commit recorded by P0-T2 as its ref
  operand.

- [ ] [P2-T13] Verify AC3 structurally and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac3-ownership-structure.md`.

  ```
  pwsh -Command '$p = "UtilitiesCS/Threading/ProgressPackage.cs"; "Declaration: " + @(Select-String -Path $p -Pattern "public class ProgressPackage : IDisposable" -SimpleMatch -CaseSensitive).Count; "Assignments: " + @(Select-String -Path $p -Pattern "_ownsCancelSource = cancelSource is null;" -SimpleMatch -CaseSensitive).Count; Select-String -Path $p -Pattern "_ownsCancelSource" -SimpleMatch -CaseSensitive | ForEach-Object { "Line " + $_.LineNumber + ": " + $_.Line.Trim() }; Select-String -Path $p -Pattern "public CancellationTokenSource? CancelSource" -SimpleMatch -CaseSensitive | ForEach-Object { "PropertyStart: " + $_.LineNumber }; Select-String -Path $p -Pattern "public async Task<ProgressPackage> InitializeAsync(" -SimpleMatch -CaseSensitive | ForEach-Object { "InitializeAsyncStart: " + $_.LineNumber }'
  ```

  Acceptance for AC3: `Declaration:` is 1; `Assignments:` is 2; the artifact enumerates every line
  containing the ownership field with its line number and trimmed text; both assignment line numbers
  fall inside an InitializeAsync overload body, which the artifact establishes by recording the two
  InitializeAsync start line numbers and the start and end line numbers of each overload body; and no
  enumerated line number falls inside the public CancelSource property declaration block, whose start
  and end line numbers the artifact records. The setter exclusion is the discriminating check: the
  spawn-child path assigns the parent's source through that setter, so a setter that claimed ownership
  would make every child claim its parent's source, and a check that only counted the assignments would
  not detect it. The artifact also records that the two static tuple factory methods each carry the
  ownership-transfer doc comment added by P1-T5 and that neither disposes the package it constructs.
  The artifact restates that AC3 is a capability criterion and names the Scope Boundary entry that
  records the residual and its owner.

- [ ] [P2-T14] Verify the three AC4 tests by name and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac4-disposal-tests.md`.
  Acceptance for AC4: the TRX produced by P2-T5 under the results directory for that task contains a
  unit test result with outcome Passed for each of `Dispose_WhenPackageConstructedTheSource_ReleasesIt`,
  `Dispose_WhenCallerSuppliedTheSource_LeavesItUsable` and
  `Dispose_OnSpawnedChild_DoesNotReleaseTheParentsSource`, and the artifact transcribes the three test
  names with their outcomes. The artifact additionally records, by reading
  `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs`, that each of the three tests passes a non-null
  progressTracker argument and an explicit stopWatch argument and probes release through the token
  getter rather than through a timer, and that the four pre-existing tests in that file are unchanged.
  A named test and its node identity are used rather than a phrase search because a test name is stable
  under reformatting. Where the results directory holds more than one TRX file because the Phase 2 loop
  restarted, the file with the most recent last-write timestamp is the operative one; the artifact
  records that file's name and the count of TRX files present, so a third party re-running the
  selection obtains the same file.

- [ ] [P2-T15] Verify the AC1 test by name and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac1-scan-cap-log.md`.
  Acceptance for AC1: the TRX produced by P2-T6 under the results directory for that task contains a
  unit test result with outcome Passed for
  `DequeueAsync_ZeroAcceptedAndCapReached_LogsScanCapBoundAndStopDecision`, and the artifact transcribes
  the name and outcome. The artifact additionally records, by reading
  `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`, that the test injects a
  debugLog delegate, that it filters the captured list on the full four-word opening phrase of the
  scan-bound message before asserting fields, and that it asserts the five field tokens `Accepted=0`,
  `Scanned=4`, `Cutoff=900`, `Bound=scan-cap` and `Decision=stop`. Where the results directory holds
  more than one TRX file because the Phase 2 loop restarted, the file with the most recent last-write
  timestamp is the operative one; the artifact records that file's name and the count of TRX files
  present, so a third party re-running the selection obtains the same file.

- [ ] [P2-T16] Verify the AC2 test by name and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac2-ceiling-log.md`.
  Acceptance for AC2: the TRX produced by P2-T6 under the results directory for that task contains a
  unit test result with outcome Passed for
  `DequeueAsync_ZeroAcceptedAndCeilingReached_LogsCeilingBoundNotScanCapBound`, and the artifact
  transcribes the name and outcome. The artifact additionally records, by reading
  `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`, that the test filters
  the captured list on the full four-word opening phrase of the scan-bound message, that it asserts
  `Bound=zero-acceptance-ceiling` present and `Bound=scan-cap` absent, and states that the absence
  assertion is the one that makes a collapse of the two bounds to a single value detectable. Where the
  results directory holds more than one TRX file because the Phase 2 loop restarted, the file with the
  most recent last-write timestamp is the operative one; the artifact records that file's name and the
  count of TRX files present, so a third party re-running the selection obtains the same file.

- [ ] [P2-T17] Audit file sizes after the final formatter pass and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/file-size-audit.md`.

  ```
  pwsh -Command 'foreach ($p in @("QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs","UtilitiesCS/Threading/ProgressPackage.cs","UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs","UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs","UtilitiesCS/UtilitiesCS.csproj","UtilitiesCS.Test/UtilitiesCS.Test.csproj")) { $p + ": " + (Get-Content -Path $p).Count }'
  ```

  Acceptance: each of the four C# source files reports a line count of 500 or fewer. The audit runs
  after P2-T1 in the completing pass, because a size measured before the formatter runs is not the size
  the repository limit applies to. The two project files are measured and recorded for completeness;
  the 500-line limit does not apply to them. The projected count for the QuickFiler test part file is
  between 448 and 458 against a base of 347, which leaves margin; if the measured count exceeds 500 the
  task fails and the executor reports BLOCKED rather than adding a new part file, because a new part
  file would require editing the QuickFiler test project file, which is outside the Write Set.

- [ ] [P2-T18] Check off AC1 in
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md` by changing the AC1
  checkbox from unchecked to checked. Acceptance: exactly one checkbox changes in this task; the AC1
  line matches the literal `- [x] AC1 ` including the trailing space, which does not match the AC10,
  AC11 or AC12 lines; and the artifact
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac1-scan-cap-log.md`
  exists and is complete before the box is flipped.

- [ ] [P2-T19] Check off AC2 in
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md`. Acceptance: exactly
  one checkbox changes; the AC2 line matches the literal `- [x] AC2 `; and
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac2-ceiling-log.md`
  exists and is complete.

- [ ] [P2-T20] Check off AC3 in
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md`. Acceptance: exactly
  one checkbox changes; the AC3 line matches the literal `- [x] AC3 `; and
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac3-ownership-structure.md`
  exists and is complete.

- [ ] [P2-T21] Check off AC4 in
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md`. Acceptance: exactly
  one checkbox changes; the AC4 line matches the literal `- [x] AC4 `; and
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac4-disposal-tests.md`
  exists and is complete.

- [ ] [P2-T22] Check off AC5 in
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md`. Acceptance: exactly
  one checkbox changes; the AC5 line matches the literal `- [x] AC5 `; and
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac5-using-declaration.md`
  exists and is complete.

- [ ] [P2-T23] Check off AC6 in
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md`. Acceptance: exactly
  one checkbox changes; the AC6 line matches the literal `- [x] AC6 `; and
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac6-deletions.md`
  exists and is complete.

- [ ] [P2-T24] Check off AC7 in
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md`. Acceptance: exactly
  one checkbox changes; the AC7 line matches the literal `- [x] AC7 `; and
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac7-compile-item-counts.md`
  exists and is complete.

- [ ] [P2-T25] Check off AC8 in
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md`. Acceptance: exactly
  one checkbox changes; the AC8 line matches the literal `- [x] AC8 `; and
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/qc-csharpier-check.md`
  exists and is complete.

- [ ] [P2-T26] Check off AC9 in
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md`. Acceptance: exactly
  one checkbox changes; the AC9 line matches the literal `- [x] AC9 `; and
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/qc-build-analyzers.md`
  exists and is complete.

- [ ] [P2-T27] Check off AC10 in
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md`. Acceptance: exactly
  one checkbox changes; the AC10 line matches the literal `- [x] AC10 `; and
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/qc-build-nullable.md`
  exists and is complete.

- [ ] [P2-T28] Check off AC11 in
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md`. Acceptance: exactly
  one checkbox changes; the AC11 line matches the literal `- [x] AC11 `; and
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac11-test-count-delta.md`
  exists and is complete.

- [ ] [P2-T29] Check off AC12 in
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md`. Acceptance: exactly
  one checkbox changes; the AC12 line matches the literal `- [x] AC12 `; and
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac12-progresspackage-coverage.md`
  exists and is complete.

- [ ] [P2-T30] Tick the three Evidence Checklist boxes in
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md` for baseline,
  targeted verification and end-state. Acceptance: all three lines are checked, and the artifact
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/issue-updates/ac-status-summary.md`
  exists carrying `Timestamp:`, a twelve-row table of acceptance criterion identifier, verdict and
  evidence path, and `PostedAs: unknown` where no GitHub update was made. These three boxes are the
  evidence checklist and are not acceptance criteria, which is why they are ticked in a separate task
  from the twelve single-criterion check-offs.

- [ ] [P2-T31] Reconcile the acceptance-criteria state and write
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/qa-gates/ac-reconciliation.md`.

  ```
  pwsh -Command '$p = "docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/issue.md"; "Checked: " + @(Select-String -Path $p -Pattern "- [x] AC" -SimpleMatch -CaseSensitive).Count; "Unchecked: " + @(Select-String -Path $p -Pattern "- [ ] AC" -SimpleMatch -CaseSensitive).Count'
  ```

  Acceptance: `Checked:` is exactly 12 and `Unchecked:` is exactly 0. A count other than twelve checked
  means a criterion was flipped twice or not at all and fails this task.

- [ ] [P2-T32] Commit the code change and the evidence with explicit pathspecs and write nothing
  further in this task.

  ```
  git add -- QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs UtilitiesCS/Threading/ProgressPackage.cs UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs UtilitiesCS/UtilitiesCS.csproj UtilitiesCS.Test/UtilitiesCS.Test.csproj
  git add -A -- UtilitiesCS/Threading/ProgressTrackerAsync.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
  git add -- docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872
  git commit -m "fix(872): assert scan-bound log, add progress package source ownership, remove dormant tracker"
  git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)TestResults" ":(exclude)docs/features/potential"
  ```

  Acceptance: the commit succeeds and the porcelain span produces zero output lines. The terminal
  evidence artifact P2-T33 writes does not exist yet at this point, so it cannot appear in the span;
  that is why P2-T33 carries its own commit. The span excludes the agent-memory tree, which the
  executor writes to during the run, and the git-ignored results directory. The commit message contains
  no angle bracket, dollar sign or backtick character, and the commit uses a single message argument.
  The porcelain span excludes the potential features directory. The promotion record for this issue is
  committed by the preparation run before Phase 0 begins, so it is not staged here; and in a parallel
  run other items queue their own untracked promotion files in that directory, which this delivery
  must neither stage nor be failed by. Staging them would sweep another item's bookkeeping onto this
  branch.

- [ ] [P2-T33] Write the terminal evidence artifact
  `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/other/final-commit.md`
  and commit it, leaving the worktree clean.

  ```
  git rev-parse HEAD
  git add -- docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/other/final-commit.md docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/plan.2026-09-12T10-26.md
  git commit -m "chore(872): record terminal evidence for the minor-audit trio"
  git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)TestResults" ":(exclude)docs/features/potential"
  ```

  Acceptance: the artifact carries `Timestamp:`, the head SHA produced by P2-T32, the eight Write Set
  paths with their final state of created, modified or deleted, and an index of every evidence artifact
  this plan produced; the second commit succeeds; and the final porcelain span produces at most one
  output line, which if present names only
  docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/plan.2026-09-12T10-26.md. Any
  other path in that span fails this task. The single tolerated line is this task's own check-off: the
  executor marks a task complete only after its verification passes, so the check-off of P2-T33 is
  necessarily written after this span runs and no commit inside this plan can capture it. The staging
  span includes the plan file so that the check-off of P2-T32 is committed here.
  This task exists as a separate commit because an artifact written after the first commit would
  otherwise leave the terminal state as a worktree with an uncommitted evidence file. No TRX, no
  MSBuild log and no raw coverage XML is committed by either commit task, per D10.

---

## Acceptance Criteria Traceability

| AC | Implementation task | Verification task | Evidence artifact |
|---|---|---|---|
| AC1 | P1-T2 | P2-T6, P2-T15 | evidence/qa-gates/ac1-scan-cap-log.md |
| AC2 | P1-T3 | P2-T6, P2-T16 | evidence/qa-gates/ac2-ceiling-log.md |
| AC3 | P1-T4, P1-T5 | P2-T13 | evidence/qa-gates/ac3-ownership-structure.md |
| AC4 | P1-T6, P1-T7, P1-T8 | P2-T5, P2-T14 | evidence/qa-gates/ac4-disposal-tests.md |
| AC5 | P1-T9 | P2-T12 | evidence/qa-gates/ac5-using-declaration.md |
| AC6 | P1-T11, P1-T13 | P2-T11 | evidence/qa-gates/ac6-deletions.md |
| AC7 | P1-T10, P1-T12 | P2-T10 | evidence/qa-gates/ac7-compile-item-counts.md |
| AC8 | P2-T1 | P2-T2 | evidence/qa-gates/qc-csharpier-check.md |
| AC9 | P1-T14 | P2-T3 | evidence/qa-gates/qc-build-analyzers.md |
| AC10 | P1-T14 | P2-T4 | evidence/qa-gates/qc-build-nullable.md |
| AC11 | P1-T2, P1-T3, P1-T6, P1-T7, P1-T8, P1-T13 | P2-T5, P2-T6, P2-T8 | evidence/qa-gates/ac11-test-count-delta.md |
| AC12 | P1-T4 | P2-T7, P2-T9 | evidence/qa-gates/ac12-progresspackage-coverage.md |
