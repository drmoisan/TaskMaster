# 2026-09-08-etl-deadline-mechanics-follow-ups (Plan)

- **Issue:** #825
- **Parent (optional):** epic review-residuals-2026-09-08, child F825
- **Owner:** drmoisan
- **Last Updated:** 2026-09-09
- **Status:** Awaiting executor preflight (revision round 6)
- **Version:** 1.5 — round 6 applied the two-defect fourth-preflight delta, D-31 and D-32, and two
  further instances the planner's own sweeps found. D-31 carried the Cobertura per-filename
  derivation rule already stated at P0-T9 and P8-T7 to the third reader of the same data, P8-T9,
  whose ChangedLinesCovered figure is otherwise sensitive to which of the two repeated line views
  and which class element is read. D-32 stopped P7-T1 writing the pre-change line number 1646 into
  a permanent source comment that P3-T9 and P4-T3 have already moved, and named the test by its
  method name instead. The sweep for that second class found two more permanent-source cases and
  closed both the same way: P6-T1's budget comment cited OlTableExtensions.Etl.cs lines 127-130
  while inserting itself above line 84, and P7-T4's reason comment cited TimeOutTask_Tests.cs lines
  27-37 and 40-50 while inserting itself above line 10, so each range moved by the height of the
  comment that cited it. Round 6 changed no task count, added no task and moved no identifier.
  Round 5 applied the six-defect third-preflight delta, D-25 through D-30:
  instruction scope widened to reach a third Item1 read at P5-T4; derived-figure rules added at
  P0-T9 and P8-T7 for Cobertura elements that carry no line counters; the test-assembly exclusion
  mechanism at P8-T7 corrected from the run-time module pattern to the first-party allowlist; the
  P9-T1 artifact requirement split into an artifact-bearing list and a count-or-search list; D7
  extended to sanitise the five MSBuild logs before commit; and P8-T8's three deleted tests recorded
  as contributing exactly zero. Round 4 applied the seven-defect confirming-preflight delta, D-18
  through D-24. Every one corrected an acceptance condition asserted over output a tool does not
  print on its success path, plus one single-branch satisfiability gap at P3-T3. Round 3 applied the
  seventeen-defect executor preflight delta. Round 2 applied the orchestrator's Branch A
  adjudication: the executor-side spec-amendment tasks were removed, Phase 3 opened with a read-only
  amended-spec verification, and Phase 3 was renumbered contiguously. Rounds 3, 4 and 5 changed no
  task count and added no task; round 4 added one decision entry, D10, and round 5 amended D7 rather
  than adding a task, so no identifier moved. The task count is unchanged at 108
  across ten phases, so every P#-T# cross-reference in this file is unchanged and still resolves.
- **Work Mode:** full-bug (sole acceptance-criteria source: spec.md, 35 criteria)

**Task-line convention.** Every task opens with its primary file path, then an em dash, then the
instruction. The path is the file the task creates, edits, or writes evidence to.

**Fail-closed evidence rule:** every baseline command step, every final-QC command step and the
coverage comparison produce their own artifact. A missing or field-incomplete artifact makes the
outcome BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** every evidence-producing task names its artifact path. Work is not
complete without the artifact on disk.

**Evidence location (non-overridable):** every artifact this plan produces is written under
docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/ with a kind
sub-folder drawn from baseline, regression-testing, qa-gates, issue-updates, other. No
artifacts/ path is used for evidence anywhere in this plan.

**Path-formatting rule inherited from spec.md:** in spec.md only the Write Set section uses
Markdown code spans for repository file paths; a downstream tool harvests backtick-delimited path
tokens to derive this feature's change footprint. Any task that edits spec.md outside the Write Set
writes file paths as plain prose, without backticks. This plan file is not harvested and uses plain
prose paths throughout.

---

## Design conflict adjudicated by the orchestrator on 2026-09-09 (Branch A, decided)

This section is not optional reading. It records a conflict between two acceptance criteria that
was measured directly against this worktree during plan authoring, and the decision that closed it.
The conflict is no longer open: the orchestrator adjudicated Branch A on 2026-09-09, independently
re-measured the finding below and confirmed it, and Phase 3 implements Branch A. The measurement and
the rejected alternative are retained verbatim so the choice stays reviewable.

**Who amended the acceptance criteria, and why not the executor.** All three acceptance-criteria
amendments were applied to spec.md during preparation by the orchestrator, before this plan is
handed to an executor. The AC35 amendment, which discharges the reachability observation through an
evidence artifact and a deferred epic handoff instead of an on-branch promotion, was committed as
13c27214. The AC6 and AC20 pair that Branch A requires was committed as 945659cd. Round 1 of this
plan had the
executor make the AC6 and AC20 amendments at its own Phase 3 tasks; that assignment was wrong and
has been removed. .claude/skills/acceptance-criteria-tracking/SKILL.md places authorship of
acceptance criteria with planning and scoping agents, not with executors, and an executor free to
rewrite the criterion it is judged against is not gated by that criterion. Phase 3 therefore opens
with a read-only verification task that confirms the executor is working against the amended spec,
so a stale checkout is caught before any source edit rather than after.

**The measurement.** UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs line 135
(GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder) drives
DfDeedle.GetEmailDataInViewAsync with an ArmingBarrierTimeProvider constructed at line 141 and
counts arming signals in a fixed order: it awaits barrier.Armed at line 158 for the 3000 ms
column-add deadline, re-arms at line 159, releases gate A at line 160, awaits barrier.Armed again at
line 164 for the 250 ms ETL hop deadline, and advances the fake clock by 250 ms at line 165. Table
acquisition at DfDeedle.cs line 148 runs before AddQfcColumnsAsync at line 168 and before EtlAsync
at line 172.

Threading the caller's TimeProvider into GetTableInViewAsync makes the table-acquisition deadline
create a timer on that same barrier, because TimeProviderTaskExtensions.CreateCancellationTokenSource
arms its cancellation through the provider's CreateTimer, and
UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs line 47 signals on every CreateTimer. The
acquisition timer therefore becomes the first signal, the column-add timer the second, and the ETL
hop timer the third. The test's barrier.Advance(250) then runs while only the 2000 ms and 3000 ms
timers are armed, fires nothing, and the awaited call never completes: the assertion at lines
168-172 sits inside the try, so the finally at lines 174-179 that releases the gates is never
reached. The failure mode is a hang, not a clean failure.

**Why the hazard cannot be dissolved by implementation choice.** The barrier's Armed signal is a
latch (TaskCompletionSource plus TrySetResult), so it loses a signal whenever two timers are armed
inside one await/re-arm window. Determinism is only recoverable by gating each production step that
arms a timer. There is no way to place the acquisition deadline under the caller's clock without
creating a timer on that clock; that is what "under the caller's clock" means.

**The conflict as it stood before the amendments.** spec.md AC6 required both that DfDeedle.cs line
148 passes its timeProvider to GetTableInViewAsync and that DfDeedleEtlTimeoutTests.cs does not
appear in this feature's diff while still passing. spec.md AC20 separately required
DfDeedleEtlTimeoutTests.cs to be absent from the diff. The measurement above shows those obligations
could not all hold. The spec's own Non-goals bullet asserting that the three DfDeedle-path tests
become deterministic with no edit at all was falsified for the line-135 test; it remains true for
DfDeedleEtlTimeoutTests.cs line 187 and for DfDeedle_COM_Tests.cs line 473, neither of which counts
arming signals. Exactly one test in that class consumes arming signals: barrier.Armed occurs at
lines 158 and 164 only, both inside the line-135 test.

**Resolution implemented by this plan (Branch A, adjudicated).** Thread the TimeProvider as spec.md
Proposed Fix item 2 requires, and update the line-135 test's ordering expectations deliberately,
adding a third test-owned gate on table acquisition so each timer is armed inside its own
await/re-arm window. The corresponding spec amendments are already on disk: AC6 and AC20 each carry
an inline Amended 2026-09-09 paragraph recording this measurement, the Write Set carries eleven
paths including UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs, the falsified Non-goals
bullet is replaced, and Test Strategy splits the no-change tests from the bounded-edit one. Phase 3
verifies that state and then makes the edit; no task in this plan amends an acceptance criterion.

**The rejected alternative (Branch B), recorded so the choice is reviewable.** Do not forward the
provider from DfDeedle.cs line 148. That keeps DfDeedleEtlTimeoutTests.cs out of the diff and
preserves the original AC20 verbatim, but it falsifies AC6's first clause and AC34's invariant
sentence, and it leaves the production table-acquisition deadline on the system clock, which is the
defect item 2 exists to close. The orchestrator rejected Branch B on those grounds: it costs more
acceptance criteria and abandons the substance of item 2. DfDeedleEtlTimeoutTests.cs is owned by no
sibling feature, so the Branch A edit creates no cross-feature collision.

---

## Other disagreements found between the relayed ground truth and spec.md

Both were measured directly against this worktree during plan authoring. Both are handled by the
acceptance conditions this plan authors; neither requires a spec amendment.

1. **spec.md AC11 as written is unsatisfiable.** It requires that a search of
   OlTableExtensions.Etl.cs for the null-forgiving return returns no hit. The literal
   `return (data!, columnDictionary);` appears twice in that file: at line 63, inside the
   synchronous ETL method, and at line 131, inside EtlAsync. Only line 131 is in scope; ETL's
   non-null tuple contract is a separate pre-existing latent condition documented at Etl.cs lines
   25-27, and changing it is outside this feature. Phase 5 therefore asserts AC11 by an occurrence
   count that drops from exactly 2 to exactly 1, together with an anchored diff showing the removed
   line inside EtlAsync. The count form is discriminating; the zero-hit form is not achievable.

2. **spec.md AC22's stated verification method cannot fail.** It says to verify by a search of the
   file for a CancellationTokenSource construction with a numeric argument returning no hit. That
   search already returns no hit before any change: the five occurrences in
   UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs are at lines 965, 1005, 1284,
   1328 and 1686 and every one is the no-argument constructor. The real 2000 ms source that the test
   at line 1646 arms is constructed inside TimeOutTask.cs line 53 by the default factory, not in the
   test file. Phase 3 therefore asserts AC22 by the substance the criterion names, that the test at
   line 1646 supplies a FakeTimeProvider, and retains the zero-hit search only as a corroborating
   guard, labelled as already-true.

---

## Decisions Record (command forms pinned once, referenced by task ID)

**D1 — vswhere-resolved MSBuild.** msbuild is not on PATH in this environment.
scripts/vscode/Invoke-VSBuild.ps1 must not be used, because it rewrites csproj HintPaths. Every
MSBuild task in this plan uses this two-line resolution followed by the command in the task text:

```powershell
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
```

**D2 — vswhere-resolved vstest.console.exe.** vstest.console.exe is not on PATH either. Scoped
pass/fail runs use:

```powershell
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~NodeName"
```

/InIsolation is mandatory for the Moq-based assemblies. vstest 18.x rejects OR inside
/TestCaseFilter; join clauses with a vertical bar.

**D3 — base-commit anchor for every git diff.** No bare git diff appears in this plan. P0-T2 writes
the pre-change commit into
docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/base-commit.md
as a line beginning `BaseCommit: ` followed by the 40-character sha. Every diff task re-derives it in
its own command, because no shell variable survives between tasks:

```powershell
$b = (((Select-String -Path 'docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/base-commit.md' -Pattern '^BaseCommit: [0-9a-f]{40}$').Line) -split ' ')[1]
```

origin/main is deliberately NOT used as the anchor: this feature branches from the epic integration
branch, whose own manifest commits under docs/features/epics/ would otherwise enter every diff and
falsify AC20.

**D4 — repo-wide gates exclude .claude/.** The .claude/agent-memory tree is tracked and the executor
writes to it during the run, so every repository-wide name-listing diff or status gate in this plan
carries the pathspec that excludes .claude/. The pathspec is spelled once here and reused verbatim:
the trailing operand `-- . ":(exclude).claude"`, which git applies recursively to that directory.
Tasks that name a narrower pathspec, such as `-- docs/features/`, do not need the exclusion because
that pathspec already scopes them away from .claude/.

**D5 — coverage runner.** Full-suite coverage uses
`pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput <path>`.
The -SearchRoot argument is always the repository root. The script requires PowerShell 7, discovers
every test assembly under bin\Debug\, already excludes .claude\ worktree builds, and hardcodes
/TestCaseFilter:TestCategory!=LiveOutlook. Baseline and post-change runs use the identical command
shape so the two figures are comparable.

**D6 — known environmental hazard on this workstation.** Four UtilitiesCS.Test shell-icon classes
(HelperClasses.ShellUtilities_Tests, HelperClasses.ShellUtilitiesStatic_Tests,
HelperClasses.SysImageListHelperTests, EmailIntelligence.OSBrowser_Tests) have stalled full local
vstest runs through SHGetFileInfo, and DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue
is a known flaky test tracked as issue #780. Invoke-MSTestWithCoverage.ps1 has no extension point for
an extra /TestCaseFilter clause. If a full-suite run in this plan does not terminate, the executor
records the observation in the artifact, marks the task BLOCKED, and reports it; it must not be
recorded as PASS and must not be worked around by narrowing the search root, which would change the
coverage denominator.

**D7 — build logs use a .txt extension and are sanitised before they are committed.** .gitignore
line 84 ignores files ending in .log, so an MSBuild file log written into the evidence folder with
that extension would never be committed. Every /flp:LogFile= target in this plan ends in .txt.
Every such log carries absolute host paths: MSBuild echoes the csc command line at detailed
verbosity, and each /reference:, /analyzer:, /analyzerconfig: and /additionalfile: argument spells
out an absolute root, so the log carries the session worktree root and the main checkout root and
therefore the host user account name. Before P9-T4 commits, and as the last action of each task
that writes one, the executor rewrites every occurrence of the worktree root to the literal
<repo-root> and every occurrence of the main checkout root to the literal <main-checkout-root>,
case-insensitively, in each of the five logs this plan produces:
evidence/baseline/build-analyzers.txt, evidence/baseline/build-nullable.txt,
evidence/other/ac8-createcancellationtokensource-proof.txt,
evidence/qa-gates/qc-build-analyzers.txt and evidence/qa-gates/qc-build-nullable.txt. That is the
convention the repository's own committed logs already follow; see
docs/features/active/2026-08-25-itemviewer-surface-defects-489/evidence/qa-gates/phase0-analyzer-build.2026-08-27T23-22.msbuild.txt
line 57. The rewrite runs after every count this plan reads from a log, because the tokens counted
(`Skipping target "CoreCompile"`, `/out:obj\Debug\UtilitiesCS.dll`, `/out:obj\Debug\UtilitiesCS.Test.dll`
and `CS1061`) carry no absolute path and are unaffected by it. P9-T4 additionally confirms that
none of the five committed logs contains the token `C:\Users\`.

**D8 — TRX and raw coverage stay out of the evidence folder.** TRX files are not gitignored and carry
the host machine and user name in two casings. No task in this plan writes a TRX into the evidence
folder; scoped runs pass no logger, and the coverage runner's Cobertura XML is the only machine
artifact committed.

**D9 — a red coverage run produces no readable artifact.** scripts/vscode/Invoke-MSTestWithCoverage.ps1
throws at lines 235-237 when the coverage exit code is non-zero, and that throw precedes the Cobertura
post-processing at lines 334-344, so a run with any failing test leaves no processed XML for P0-T8 or
P8-T6 to read. If a run in this plan is red, the executor records the failing test names in the
artifact and re-runs once. If the same test fails on the re-run and it is
DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue, the executor records it as the
flake tracked by issue #780, re-runs a third time and marks the task BLOCKED if it does not go green.
Any other repeated failure marks the task BLOCKED and is reported. A BLOCKED coverage task halts the
plan; it is never recorded as PASS, and a raw unprocessed Cobertura file is not an acceptable
substitute for the processed one.

**D10 — a green vstest run prints no `Failed:` and no `Skipped:` line.** A fully passing
vstest.console.exe run on this toolchain prints exactly `Test Run Successful.`, `Total tests: N`,
`Passed: N` and `Total time: ...`. The `Failed:` and `Skipped:` lines are emitted only when their
counters are non-zero, and `Passed:` is likewise omitted when it is zero. D8 forbids a TRX, so no
second source exists. Every task in this plan that records `TestsFailed:`, `TestsPassed:` or
`TestsSkipped:` therefore transcribes the counters the run printed and records each omitted counter
as 0 with the annotation `(omitted category, transcribed per D10)`. On a green run `TestsFailed: 0`
and `TestsSkipped: 0` are that transcription, corroborated by the printed `Test Run Successful.`
header and by `Passed:` equalling `Total tests:`. At P2-T4, whose run is red, `Failed:` is printed
and `TestsPassed:` is `Total tests:` minus that figure. Recording UNVERIFIED, or recording a value
the captured output does not support, fails the task.

---

### Phase 0 — Policy Reads, Toolchain Bootstrap and Baseline Capture

- [x] [P0-T1] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/phase0-instructions-read.md — read, in the order defined by .claude/skills/policy-compliance-order/SKILL.md, the files CLAUDE.md, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md, .claude/rules/quality-tiers.md, .claude/rules/csharp.md, .claude/rules/tonality.md and .claude/rules/plan-acceptance-gates.md, then write this artifact.
      Acceptance: the artifact exists and carries a `Timestamp:` line in yyyy-MM-ddTHH-mm form, a
      `Policy Order:` line naming that skill's order, and exactly seven bullet lines each naming one
      of the seven files above.

- [x] [P0-T2] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/base-commit.md — record the pre-change anchor by running `git rev-parse HEAD`, `git rev-parse --abbrev-ref HEAD` and `git status --porcelain --untracked-files=all` with the D4 exclusion pathspec.
      Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`,
      one line matching the regular expression `^BaseCommit: [0-9a-f]{40}$`, and one
      `WorktreeStatusLines:` line whose value is the integer count of porcelain output lines. This
      sha is the D3 anchor for every diff in this plan.

- [x] [P0-T3] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/restore.md — restore NuGet packages by running `pwsh -File scripts/vscode/Invoke-Restore.ps1`, which invokes the vswhere-resolved MSBuild with `/t:Restore /p:RestorePackagesConfig=true`, then write this artifact.
      Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`,
      and a `PackagesDirectoryPresent: true` line proving
      packages/Microsoft.Bcl.TimeProvider.10.0.11/lib/net462/Microsoft.Bcl.TimeProvider.dll exists on
      disk after the run. That file did not exist before this task; UtilitiesCS/UtilitiesCS.csproj
      line 97 names it as the HintPath and UtilitiesCS/packages.config line 28 pins the version.

- [x] [P0-T4] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/dotnet-tool-restore.md — restore the local tool manifest by running `dotnet tool restore` from the repository root, then write this artifact.
      Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`,
      and a `CSharpierVersion: 1.2.6` line read from the tool manifest at the repository root,
      dotnet-tools.json. The manifest is at the repository root and not under .config/; there is no
      .config/dotnet-tools.json in this tree. CSharpier 1.2.6 requires a subcommand, so the bare-path
      invocation form does not run.

- [x] [P0-T5] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/csharpier-check.md — capture the formatter baseline by running `dotnet tool run csharpier check .` from the repository root, then write this artifact.
      Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, a
      `CheckedFiles:` line carrying the integer CSharpier prints on its `Checked N files in Xms.`
      summary line, and an `UnformattedFileList:` line naming every file the check reported or the
      value none. A non-zero exit code halts the plan and is reported: P8-T1 runs a repository-wide
      mutating `csharpier format .`, which would repair pre-existing drift anywhere in the tree, and
      every file it repaired would then enter the branch diff and falsify the P9-T4 Write Set
      accounting, which admits only the eleven Write Set paths and this feature's own folder.

- [x] [P0-T6] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/build-analyzers.md — capture the analyzer-gate baseline using the D1 resolution followed by `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` with a detailed file log written to the sibling build-analyzers.txt in the same folder.
      Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, a
      `WarningCount:` and an `ErrorCount:` integer read from the MSBuild summary, and the sibling
      build-analyzers.txt exists. /t:Rebuild is mandatory: MSBuild's up-to-date check does not
      invalidate on a command-line property change, so a warm /t:Build returns exit 0 with CoreCompile
      skipped and runs no analyzers.

- [x] [P0-T7] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/build-nullable.md — capture the nullable-gate baseline using the D1 resolution followed by `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` with a detailed file log written to the sibling build-nullable.txt in the same folder.
      Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`,
      `WarningCount:`, `ErrorCount:`, and the sibling build-nullable.txt exists. Do not add
      /p:Nullable=enable: no project in this repository carries a Nullable element and there is no
      Directory.Build.props, so the property conscripts files that never adopted the pragma and the
      gate cannot pass.

- [x] [P0-T8] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/coverage-baseline.md — capture the repository-wide coverage baseline by running the D5 command with its coverage output pointed at the sibling coverage-baseline.cobertura.xml in the same folder, then write this artifact.
      Acceptance: the Cobertura XML exists; the artifact carries `Timestamp:`, `Command:`,
      `EXIT_CODE:`, `Output Summary:`, and six numeric lines read from the root coverage element of
      that XML, namely `LineRate:`, `LinesCovered:`, `LinesValid:`, `BranchRate:`,
      `BranchesCovered:`, `BranchesValid:`, plus `TestsPassed:` and `TestsFailed:` read from the run
      summary. All eight values are integers or decimals; the string UNVERIFIED is not an acceptable
      value. D6 applies if the run does not terminate. D9 applies if the run is red. D10 governs how
      the counters vstest omits are recorded.

- [x] [P0-T9] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/coverage-baseline-by-file.md — record the per-file coverage baseline for the five production files this feature edits by reading the class elements of the sibling coverage-baseline.cobertura.xml whose filename attribute ends in OlTableExtensions.TableAccess.cs, OlTableExtensions.Etl.cs, TimeOutTask.cs, DfDeedle.cs or DfDeedle.QfcColumns.cs.
      The counters are derived, not read: a Cobertura class element carries line-rate, branch-rate,
      complexity, name and filename only, and no line counters. Derive each file's figures with
      Get-CoberturaClassLineSummary, declared at scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
      line 158, whose TotalLines and CoveredLines are deduplicated by line number. A direct count of
      the line descendants of a class element double-counts, because each class carries both a
      methods/method/lines tree and a class-level lines block repeating the same line numbers. The
      helper is dot-sourced from that file, which itself dot-sources the sibling PackageRate,
      ClosureFilter and Threshold files.
      Acceptance: the artifact carries `Timestamp:` and exactly five `File:` blocks, each with
      `LinesCovered:` and `LinesValid:` integers aggregated over every class element sharing that
      filename. Aggregation by filename is required because async state machines split a single
      source file across several class elements, so reading one class element understates the file.

- [x] [P0-T10] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/pinned-source-facts.md — measure and pin the pre-change source facts that this plan's later acceptance conditions are expressed as transitions from.
      Acceptance: the artifact carries `Timestamp:` and exactly these eleven measured integer lines,
      each matching its stated expectation: `TimeOutTaskLines: 1011`;
      `OlTableExtensionsTestsLines: 1846`; `EtlCsLines: 476`; `TableAccessCsLines: 432`;
      `DfDeedleCsLines: 319`; `DfDeedleQfcColumnsCsLines: 297`; `EtlCsDataBangCount: 2`, the
      occurrences of the token `data!`; `EtlCsBudgetExpressionCount: 2`, the occurrences of the token
      `250 * rowCount`; `TimeOutTaskCatchTimeoutCount: 3`, the occurrences of the token
      `catch (TimeoutException)`; `TableAccessLiteral2000ArgumentCount: 1`, the lines matching the
      regular expression `^\s+2000,$`; and `OlTableExtensionsTestsFakeTimeProviderCount: 1`, the
      occurrences of the token `new FakeTimeProvider()` in
      UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs, whose single pre-change
      occurrence is at line 974. A measured value that disagrees with its stated expectation halts
      the plan and is reported, because every later acceptance condition depends on it.

- [x] [P0-T11] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — confirm the acceptance-criteria inventory is intact and check nothing off.
      Acceptance: the count of lines in that file matching the regular expression
      `^- \[ \] \*\*AC[0-9]+\*\*` is exactly 35 and the count matching `^- \[x\] \*\*AC[0-9]+\*\*` is
      exactly 0. This establishes the starting state that the Phase 1 through Phase 9 check-off tasks
      move.

### Phase 1 — Compile-Time Proof of CreateCancellationTokenSource

- [x] [P1-T1] UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs — add the settling call site as the first statement of GetTableInViewAsync, immediately after the opening brace at line 39: a single discard statement invoking CreateCancellationTokenSource on TimeProvider.System with a one-millisecond TimeSpan.
      Acceptance: the file contains exactly one line matching the regular expression
      `_ = TimeProvider\.System\.CreateCancellationTokenSource\(` and the file's line count is exactly
      one greater than the TableAccessCsLines value pinned by P0-T10.

- [x] [P1-T2] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/other/ac8-createcancellationtokensource-proof.md — compile the settling call site using the D1 resolution followed by `& $msbuild UtilitiesCS\UtilitiesCS.csproj /t:Rebuild /m /p:Configuration=Debug /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` with a detailed file log written to the sibling ac8-createcancellationtokensource-proof.txt in the same folder.
      The solution platform name `Any CPU` is deliberately not passed to this project-scoped build.
      UtilitiesCS/UtilitiesCS.csproj line 9 defaults `$(Platform)` to `AnyCPU` and its Debug property
      group at line 22 is conditioned on `Debug|AnyCPU`, so `/p:Platform=Any CPU` matches no property
      group, leaves `OutputPath` unset and fails before compilation. That failure produces no `CS1061`
      and is not a refutation; the task is BLOCKED if it occurs.
      Create docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/other/
      before invoking MSBuild. MSBuild's file logger does not create intermediate directories: a
      `/flp:logfile=` target whose directory part does not exist terminates the build with MSB1029,
      and no task before this one writes into evidence/other/. An MSB1029 failure is neither a
      refutation nor the platform-mismatch case above; it is corrected by creating the directory and
      re-running.
      Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, a
      `MemberProven: TimeProviderTaskExtensions.CreateCancellationTokenSource` line, and a
      `CS1061Count: 0` line obtained by counting occurrences of the token `CS1061` in the captured
      log. The captured log must also contain at least one line containing the token
      `/out:obj\Debug\UtilitiesCS.dll`, which is the csc.exe command line MSBuild echoes under this
      project's CoreCompile heading, proving the compilation ran rather than being skipped. A
      `Task "Csc"` search is not used: that line carries the project instance id rather than the
      project path, so it cannot be attributed to a named project on its own. A prose assertion of
      availability, or a citation of the package XML alone, does not satisfy this task.
      A non-zero CS1061Count may be read as refutation of the member's availability only after two
      confounders are excluded and both exclusions are recorded in this artifact as
      `UsingSystemThreadingTasksPresent: true` and `PackagesDirectoryPresentAtP0T3: true`. The first
      records that UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs still contains
      the line `using System.Threading.Tasks;`, which is at line 9 today with `#nullable enable` at
      line 1; TimeProviderTaskExtensions lives in that namespace, so its absence produces the same
      CS1061 as an absent member. The second records that
      docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/restore.md
      written by P0-T3 carries `PackagesDirectoryPresent: true`, because an unrestored packages tree
      produces the same CS1061 through an unresolved Microsoft.Bcl.TimeProvider reference. If either
      recorded value is false this task is BLOCKED and reported, not refuted: the P1-T4 fallback
      branch must not be taken on a BLOCKED result.

- [x] [P1-T3] UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs — remove the settling call site so the real call site added in Phase 3 is the only one.
      Acceptance: the file contains zero lines matching
      `_ = TimeProvider\.System\.CreateCancellationTokenSource\(` and the file's line count equals the
      TableAccessCsLines value pinned by P0-T10 exactly.

- [x] [P1-T4] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC8 by changing that one criterion's unchecked box to a checked box without altering its text.
      Acceptance: the file contains exactly one line matching `^- \[x\] \*\*AC8\*\*` and exactly 34
      lines matching `^- \[ \] \*\*AC[0-9]+\*\*`, and the artifact named in P1-T2 exists on disk. If
      P1-T2 recorded a non-zero CS1061Count together with both of its confounder exclusions true,
      this task does not run: the fallback named in spec.md Proposed Fix, an in-repo provider-driven
      factory that constructs a plain CancellationTokenSource and cancels it from a timer created on
      the provider, is implemented in Phase 3 instead and recorded in the same artifact. A BLOCKED
      P1-T2 result is not a refutation and does not open the fallback branch; it halts the plan.
      If the fallback branch is taken, AC8 as written is refuted rather than satisfied and cannot be
      checked off. The executor then writes
      docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/other/ac8-refuted-fallback.md
      recording the CS1061 evidence, both confounder exclusions and the fallback implemented in Phase
      3, leaves AC8 unchecked, and reports the plan outcome as INCOMPLETE with AC8 named as the single
      unmet criterion. P9-T1 then reads 34 checked and 1 unchecked, and P9-T2 records
      `Checked off (delivered): 34` and `Remaining (unchecked): 1`. This branch is not expected: the
      research read the member's declaration at line 199 of the shipped
      Microsoft.Bcl.TimeProvider.xml.

### Phase 2 — Regression Test Red for the Retry Literal

- [x] [P2-T1] UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs — create this file with one TestClass named GetTableInViewAsyncClockTests in namespace UtilitiesCS.Test.OutlookObjects.Table, carrying DoNotParallelize with a comment immediately above it recording the verified reason (the class drives a real Task.Run gate, the reason recorded for OlTableExtensionsEtlClockTests at line 20 of its own file), and one TestMethod named GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000.
      The test supplies a timeoutSourceFactory that appends every ms argument it receives to a list,
      throws TimeoutException on its first invocation and returns a never-cancelling
      CancellationTokenSource on every later invocation; it calls GetTableInViewAsync on a mocked
      Explorer whose TableView returns a mocked Table, passing CancellationToken.None, counter 0,
      timeoutMs 750 and that factory; and it asserts the recorded list has exactly two entries and
      that the second entry is 750.
      Acceptance: the file exists, contains exactly one occurrence of the token
      `GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000`, and has at most 500 lines.

- [x] [P2-T2] UtilitiesCS.Test/UtilitiesCS.Test.csproj — add exactly one compile item for the new test file, inserted immediately after the existing entry naming OlTableExtensionsEtlClockTests.cs so the fan-in merge is a clean union, changing nothing else.
      Acceptance: the file contains exactly one line containing the token
      `GetTableInViewAsyncClockTests.cs`; the count of lines containing the token `Compile Include` is
      exactly 471, one greater than the 470 present before this task; and, using the D3 anchor,
      `git diff $b --numstat -- UtilitiesCS.Test/UtilitiesCS.Test.csproj` reports exactly 1 added line
      and exactly 0 removed lines.

- [x] [P2-T3] UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll — build the test project against the pre-change production signature using the D1 resolution followed by `& $msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Rebuild /m /p:Configuration=Debug`.
      The solution platform name `Any CPU` is deliberately not passed to this project-scoped build.
      UtilitiesCS.Test/UtilitiesCS.Test.csproj line 11 defaults `$(Platform)` to `AnyCPU` and its Debug
      property group at line 47 is conditioned on `Debug|AnyCPU`, so `/p:Platform=Any CPU` matches no
      property group, leaves `OutputPath` unset and fails before compilation; the assembly write-time
      assertion below could not then hold. That failure is BLOCKED and reported, not a build failure of
      the test code.
      Acceptance: the command exits 0 and this assembly exists with a write time later than the
      timestamp recorded by P0-T2. The new test compiles against the pre-change signature because it
      uses only the timeoutSourceFactory parameter, which already exists at
      UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs line 37.

- [x] [P2-T4] [expect-fail] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/regression-testing/ac7-fail-before.md — run the new test against the pre-change production file using the D2 resolution with a test-case filter naming GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000, then write this artifact.
      Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 1`,
      `ExpectedExitCode: 1`, `Output Summary:`, `TestsPassed: 0`, `TestsFailed: 1`, and a
      `FailureReason:` line recording that the second value the factory received was 2000 rather than
      750. That figure is the literal at
      UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs line 106, which the
      TimeoutException catch opened at line 95 passes in place of the caller's timeoutMs. The factory
      is invoked at UtilitiesCS/Threading/TimeOutTask.cs line 52, outside the try opened at line 61,
      so its throw reaches the await inside the try at TableAccess.cs line 55 and enters that catch.
      D10 governs how the counters vstest omits are recorded.

- [x] [P2-T5] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/other/ac7-mechanism-note.md — record the exception-injection mechanism note required by spec.md Test Strategy.
      Acceptance: the artifact carries `Timestamp:`, names UtilitiesCS/Threading/TimeOutTask.cs line
      52 as the invocation point outside the try at line 61, names
      UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs line 95 as the entered catch,
      and records `SubstituteMechanismUsed: false`, meaning the factory-throw mechanism spec.md names
      was used unchanged rather than replaced.

### Phase 3 — Item 2: Thread TimeProvider and Fix the Retry Literal

- [x] [P3-T1] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/ac6-ac20-amended-spec-verification.md — verify, read-only, that the executor is working against the amended spec.md before any source edit in this phase, then write this artifact. This task makes no edit to spec.md: the AC6, AC20 and AC35 amendments were applied during preparation by the orchestrator, for the reason recorded in the adjudicated design conflict section above.
      Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`,
      and these five measured integer lines, each matching its stated expectation, every one counted
      over docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md:
      `AmendmentMarkerCount: 4`, the occurrences of the token `Amended 2026-09-09`;
      `NoEditAtAllCount: 0`, the occurrences of the token `no edit at all`;
      `WriteSetDfDeedleEtlTimeoutTestsCount: 1`, the lines matching the regular expression
      "^- `UtilitiesCS\.Test/Extensions/DfDeedleEtlTimeoutTests\.cs`", which is the Write Set's
      backticked-path bullet form; `TotalAcCount: 35`, the lines matching
      `^- \[[ x]\] \*\*AC[0-9]+\*\*`, which is box-state independent and therefore the inventory
      assertion; and `CheckedAcCount:`, the lines matching `^- \[x\] \*\*AC[0-9]+\*\*`, whose
      expected value is 1 because P1-T4 has already checked off AC8, or 0 when the P1-T4 fallback
      branch was taken and that task did not run. Any other checked count halts. The box-state
      figures are stated as a transition from Phase 1 rather than as the pre-Phase-1 zero, because
      P0-T11 already pinned the pre-Phase-1 state and Phase 1 moves it.
      A measured value that disagrees with its stated expectation means
      the working tree does not carry the amended spec: this task is then BLOCKED and reported, and
      no later task in this phase runs. The executor does not repair the disagreement by editing
      spec.md; acceptance criteria in this feature are authored by planning agents only.

- [x] [P3-T2] UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs — add the trailing optional parameter to GetTableInViewAsync, positioned after timeoutSourceFactory on line 37, declared as a nullable TimeProvider defaulting to null. Place the explanatory comment on the lines immediately above the method declaration, not inside the parameter list, recording that a null provider resolves to the system clock so production timing is unchanged, and that a CancelAfter call must never be introduced on a provider-created source because on pre-.NET 8 runtimes it does not terminate the original delay timer.
      Acceptance: the file contains exactly one line matching the regular expression
      `TimeProvider\? timeProvider = null`, and the line number of that match is exactly one greater
      than the line number of the single line matching `timeoutSourceFactory = null`. The comment
      sits above the declaration rather than above the new parameter because a comment line inside
      the parameter list would separate the two parameters and falsify the adjacency assertion. The
      shortened `timeoutSourceFactory = null` token replaces the full parameter declaration so the
      asserted literal carries no angle brackets, which the plan-acceptance gate skips.

- [x] [P3-T3] UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs — resolve the deadline source once inside GetTableInViewAsync, before the try that opens at pre-change line 55 and which P3-T2's parameter and comment lines have already moved down, as a local factory equal to the supplied timeoutSourceFactory when it is non-null and otherwise to a clock-derived factory calling CreateCancellationTokenSource on the provider coalesced with TimeProvider.System with a TimeSpan.FromMilliseconds argument, and pass that resolved local to TimeOutTask.RunWithTimeout in place of the raw parameter.
      Acceptance: the file contains exactly one line matching `CreateCancellationTokenSource\(`; the
      argument the RunWithTimeout call passes is the resolved local rather than timeoutSourceFactory;
      and an explicitly supplied factory still wins, which P3-T11 proves by test.
      On the P1-T4 fallback branch this task instead implements the in-repo provider-driven factory
      spec.md Proposed Fix names, a plain CancellationTokenSource cancelled from a timer created on
      the coalesced provider, and its acceptance reads instead: the file contains exactly one line
      matching `CreateTimer\(` and zero lines matching `CreateCancellationTokenSource\(`; the
      argument the RunWithTimeout call passes is the resolved local rather than timeoutSourceFactory;
      an explicitly supplied factory still wins, which P3-T11 proves by test; and the executor
      records the branch taken in the artifact P1-T4 names.

- [x] [P3-T4] UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs — propagate the new parameter through the TaskCanceledException retry recursion at pre-change lines 82-87, which already forwards timeoutMs and timeoutSourceFactory, by adding the provider as the trailing argument.
      Acceptance: the recursion inside the TaskCanceledException catch passes five arguments and the
      fifth is timeProvider; and the single Console.WriteLine inside the TaskCanceledException catch,
      at pre-change line 79, is byte-identical to its pre-change text including its twenty leading
      spaces. It is located by content and by enclosing catch block rather than by line number,
      because P3-T2 and P3-T3 add lines above it and it no longer sits at line 79.

- [x] [P3-T5] UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs — replace the literal timeout argument in the TimeoutException retry recursion at pre-change lines 103-108 with timeoutMs, add the provider as the trailing argument, and replace the rationale comment at pre-change lines 100-102 with one recording that the caller's value is now propagated so both attempts are governed by the same caller-visible deadline on the same caller-supplied clock.
      Acceptance: the file contains zero lines matching the regular expression `^\s+2000,$`, down from
      the TableAccessLiteral2000ArgumentCount value of 1 pinned by P0-T10; the file still contains
      exactly one line matching `int timeoutMs = 2000`; and the single Console.WriteLine inside the
      TimeoutException catch, at pre-change line 97, is byte-identical to its pre-change text
      including its sixteen leading spaces. It is located by content and by enclosing catch block
      rather than by line number, because P3-T2, P3-T3 and P3-T4 add lines above it and it no longer
      sits at line 97.

- [x] [P3-T6] UtilitiesCS/Extensions/DfDeedle.cs — change line 148 so it passes the provider as a named third argument to GetTableInViewAsync, completing the pattern already used at line 168 for AddQfcColumnsAsync and at lines 172-178 for EtlAsync.
      Acceptance: the file contains zero occurrences of the token `GetTableInViewAsync(token, 0);`,
      which is the pre-change two-argument call at line 148, and the count of lines in that file
      containing the token `timeProvider: timeProvider` is exactly 3, up from the two at lines 168 and
      177. The single-line form `GetTableInViewAsync(token, 0, timeProvider: timeProvider)` is
      deliberately not asserted: that statement is 113 columns at this indentation, there is no
      .csharpierrc in the repository, and CSharpier's default 100-column print width breaks it into a
      five-line call at P8-T1, after which a single-line regex returns zero matches.

- [x] [P3-T7] UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs — add a third test-owned gate to BuildExplorer by inserting a leading Action parameter named onGetTable before onAddSentOnColumn, invoking it inside the GetTable setup at line 119 before returning the table mock, and updating the doc comment at lines 66-71 to describe three gates.
      Adding the parameter breaks all three existing BuildExplorer call sites, at lines 142, 190 and
      218, so this task also updates each of the three to pass a leading argument: an empty lambda at
      the call sites on lines 190 and 218, and an empty lambda placeholder at the line-142 call site,
      which P3-T8 replaces with the third gate's Wait. The call-site update is in scope deliberately:
      the acceptance below counts call sites file-wide, an edit confined to the declaration and the
      GetTable setup cannot reach them, and the file does not compile without it. The declaration at
      line 72 is not a call site and is not counted.
      Acceptance: the file contains exactly one line matching `System\.Action onGetTable`, exactly one
      line matching `onGetTable\(\);`, and exactly three call sites of BuildExplorer each passing four
      arguments.

- [x] [P3-T8] UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs — update the timer-ordering expectations in GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder by adding a third ManualResetEventSlim for table acquisition, passing its Wait as the new first gate, awaiting the barrier and re-arming once for the 2000 ms acquisition deadline before releasing that gate, then once more for the 3000 ms column-add deadline before releasing gate A, then once more for the 250 ms ETL hop deadline before advancing 250 ms, and releasing all three gates in the existing finally; renumber the two timer comments to 1, 2 and 3 in the new order.
      Acceptance: the test body contains exactly three occurrences of the token `await barrier.Armed;`,
      exactly two occurrences of `barrier.ReArm();`, exactly one occurrence of `barrier.Advance(250);`,
      and a finally block that sets exactly three gates. The two other tests in the class pass an
      empty lambda for the new gate and are otherwise unchanged. Each gate makes exactly one timer
      arm inside each await window, which is what the latch-based barrier requires.

- [x] [P3-T9] UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs — update the four reflection binding sites, each inside the test method declared at line 1238, 1267, 1324 or 1646, so each parameter-type array passed to InvokeAsyncResult gains a TimeProvider entry as its last element and each call gains a corresponding trailing argument; the three sites inside the tests declared at 1238, 1267 and 1324 pass null and the site inside GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot, declared at 1646 with its argument list at 1662-1677, passes a new FakeTimeProvider.
      Acceptance: the file contains exactly four occurrences of the token `typeof(TimeProvider),`;
      the occurrences of the token `new FakeTimeProvider()` number exactly one greater than the
      OlTableExtensionsTestsFakeTimeProviderCount value pinned by P0-T10, the added one being at the
      1646 site; and, using the D3 anchor,
      `git diff $b -- UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` produces zero
      lines matching the regular expression `^\+.*\[TestMethod\]`.

- [x] [P3-T10] UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs — add GetTableInViewAsync_InjectedClock_ArmsAcquisitionDeadlineOnInjectedProvider, which constructs an ArmingBarrierTimeProvider over a FakeTimeProvider, gates the mocked GetTable on a ManualResetEventSlim, starts the call without awaiting it, awaits the barrier, sets the gate inside the try, awaits the call, and asserts the returned table is the mock; the gate is set a second time in a finally so an orphaned Task.Run body cannot outlive the test on the failure path.
      The gate must be set inside the try before the call is awaited: releasing it only in the finally
      leaves the awaited call blocked on a gate nothing sets, and because the assertion sits inside the
      try the failure mode is a hang rather than a failure. The fake clock is not advanced, because
      advancing past timeoutMs cancels the acquisition and RunWithTimeout then returns default, making
      the returned table null.
      Acceptance: the file contains exactly one occurrence of the token
      `GetTableInViewAsync_InjectedClock_ArmsAcquisitionDeadlineOnInjectedProvider`, exactly one
      occurrence of `new ArmingBarrierTimeProvider(`, and one occurrence of
      `using UtilitiesCS.Test.TestHelpers;`, which is required because ArmingBarrierTimeProvider is
      declared internal in that namespace at
      UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs line 19. The awaited barrier cannot
      complete unless a timer was created on the injected provider, which is the empirical proof the
      criterion requires.

- [x] [P3-T11] UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs — add GetTableInViewAsync_ExplicitFactorySupplied_TakesPrecedenceOverTimeProvider, which supplies both a recording timeoutSourceFactory returning a never-cancelling source and an ArmingBarrierTimeProvider, and after the call completes asserts the factory was invoked exactly once and the barrier's Armed task is not completed, proving the provider created no timer.
      Acceptance: the file contains exactly one occurrence of the token
      `GetTableInViewAsync_ExplicitFactorySupplied_TakesPrecedenceOverTimeProvider` and exactly one
      occurrence of the token `Armed.IsCompleted`.

- [x] [P3-T12] UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs — add GetTableInViewAsync_NoTimeProviderSupplied_UsesSystemClockAndCompletes, which omits both optional parameters, uses a mocked GetTable that returns immediately, and asserts the returned table is the mock and the mocked GetTable was called exactly once.
      Acceptance: the file contains exactly one occurrence of the token
      `GetTableInViewAsync_NoTimeProviderSupplied_UsesSystemClockAndCompletes`; zero lines matching
      the regular expression `Thread\.Sleep|Task\.Delay|DateTime\.Now|Stopwatch`; and at most 500
      lines.

- [x] [P3-T13] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/regression-testing/phase3-green.md — rebuild with the D1 resolution followed by `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`, then run two D2 scoped runs against UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll, the first filtered to GetTableInViewAsyncClockTests, OlTableExtensions_Tests and OlTableExtensionsEtlClockTests, the second filtered to DfDeedleEtlTimeoutTests and DfDeedle_COM_Tests, then write this artifact.
      Acceptance: the artifact carries `Timestamp:`, three `Command:` lines, the first of which is the
      solution rebuild above, three `EXIT_CODE: 0` lines,
      `Output Summary:`, `TestsFailed: 0` for both runs, and named per-test result lines showing
      GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000,
      GetTableInViewAsync_InjectedClock_ArmsAcquisitionDeadlineOnInjectedProvider,
      GetTableInViewAsync_ExplicitFactorySupplied_TakesPrecedenceOverTimeProvider,
      GetTableInViewAsync_NoTimeProviderSupplied_UsesSystemClockAndCompletes,
      GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder,
      GetEmailDataInViewAsync_ClockNeverAdvances_ReturnsOneRowFrame and
      GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform each passing. D10 governs
      how the counters vstest omits are recorded.

- [x] [P3-T14] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/ac26-ac27-boundary.md — verify the ownership boundary by running, with the D3 anchor, `git diff $b -- UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` and filtering its output for lines matching the regular expression `^[+-][^+-].*Console\.WriteLine`, then write this artifact.
      Acceptance: that filter produces zero lines;
      UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs still contains exactly one
      line matching `catch \(TaskCanceledException\)`, exactly one matching
      `catch \(TimeoutException\)`, exactly one matching `int counter,`, and exactly two matching
      `Console\.WriteLine`; and the artifact records `Timestamp:`, `Command:`, `EXIT_CODE: 0` and all
      five counts. The `^[+-][^+-]` anchor excludes the diff's own header lines, so a passing result
      is not an artefact of the header carve-out.

- [x] [P3-T15] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC4.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC4\*\*`, and the P3-T2 parameter-position
      assertion passed.

- [x] [P3-T16] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC5.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC5\*\*`, and the phase3-green.md artifact
      records GetTableInViewAsync_InjectedClock_ArmsAcquisitionDeadlineOnInjectedProvider passing.

- [x] [P3-T17] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC6 in the form the orchestrator amended it into during preparation and P3-T1 verified on disk.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC6\*\*`; the phase3-green.md artifact records
      both DfDeedleEtlTimeoutTests tests and the DfDeedle_COM_Tests test passing; and, using the D3
      anchor, `git diff --name-only $b -- UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` produces
      zero lines while `git status --porcelain --untracked-files=all -- UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`
      also produces zero lines. AC6's bounded-edit clause is additionally gated: using the D3 anchor,
      `git diff $b -- UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs` produces zero added and
      zero removed lines matching the regular expression `\.Should\(\)`, which is the machine-checkable
      form of "adding no assertion and removing none". That file carries five such lines before the
      change, at lines 170, 203, 205, 224 and 225, and the timer-ordering update touches gate,
      re-arm and BuildExplorer call-site lines only.

- [x] [P3-T18] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC7.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC7\*\*`; the fail-before artifact written by
      P2-T4 exists and records `EXIT_CODE: 1`; and phase3-green.md records
      GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000 passing.

- [x] [P3-T19] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC9.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC9\*\*`, and
      UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs contains zero lines matching
      the regular expression `^\s+2000,$`.

- [x] [P3-T20] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC10.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC10\*\*`, and phase3-green.md records
      GetTableInViewAsync_ExplicitFactorySupplied_TakesPrecedenceOverTimeProvider passing.

- [x] [P3-T21] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC22, which must complete before AC21 is checked off at P7-T7 because spec.md Risks requires the wall-clock hazard to be shown removed before the attribute is removed.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC22\*\*`;
      UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs contains occurrences of the
      token `new FakeTimeProvider()` numbering exactly one greater than the
      OlTableExtensionsTestsFakeTimeProviderCount value pinned by P0-T10. The added one is the
      argument P3-T9 introduced inside the test method
      GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot, whose declaration and
      InvokeAsyncResult argument list are at pre-change lines 1646 and 1662-1677 and have moved by the
      lines P3-T9 added at the three earlier binding sites; the check-off report records the
      re-derived numbers. The pinned one is
      the pre-existing argument at line 974, which sits inside
      EtlAsync_WithBinaryAndObjectFieldsAndProgress_ReturnsTransformedData and is unrelated to
      GetTableInViewAsync, so it must not be mistaken for the criterion's subject. The corroborating
      guard the criterion names holds, namely zero lines matching the regular expression
      `new CancellationTokenSource\([0-9]` in that file. That guard was already true before this
      feature and is recorded as corroboration only, not as the discriminating check.

- [x] [P3-T22] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC26.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC26\*\*`, and the artifact written by P3-T14
      records zero matching added or removed Console.WriteLine lines.

- [x] [P3-T23] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC27.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC27\*\*`, and the artifact written by P3-T14
      records the counter count and both catch counts as one, one and one.

### Phase 4 — Item 4: Delete the Inert Overloads and the Dead Method

- [x] [P4-T1] UtilitiesCS/Threading/TimeOutTask.cs — delete both inert integer-repeat TimeoutAfter overloads, the generic one at lines 824-849 and the non-generic one at lines 924-940, leaving the proxy-returning provider overloads at lines 862 and 949 and their doc comments untouched.
      Acceptance: the file contains zero lines matching `int repeatAttempts`; the count of lines
      matching `catch \(TimeoutException\)` is exactly 1, down from the TimeOutTaskCatchTimeoutCount
      value of 3 pinned by P0-T10, and the surviving occurrence is the one at pre-change line 290
      outside any TimeoutAfter method; and the file's line count is strictly less than the
      TimeOutTaskLines value of 1011 pinned by P0-T10 and strictly greater than 500.

- [x] [P4-T2] UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs — delete EtlAsyncOld at lines 134-171, whose call at line 160 is the only production caller of a deleted overload.
      Acceptance: the file contains zero occurrences of the token `EtlAsyncOld`; the count of
      occurrences of the token `250 * rowCount` is exactly 1, down from the
      EtlCsBudgetExpressionCount value of 2 pinned by P0-T10, and the surviving occurrence is inside
      EtlAsync; and the count of occurrences of the token `data!` is still exactly 2, unchanged,
      because EtlAsyncOld returned its tuple without a suppression.

- [x] [P4-T3] UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs — delete the test EtlAsyncOld_WithBinaryAndObjectFields_ReturnsTransformedData at lines 984-1015 together with its TestMethod attribute line.
      Acceptance: the file contains zero occurrences of the token `EtlAsyncOld` and its line count is
      strictly less than the OlTableExtensionsTestsLines value of 1846 pinned by P0-T10.

- [x] [P4-T4] UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs — delete the two test callers of the removed overloads, TimeoutAfter_GenericTask_WithRepeatAttempts_ReturnsResult at lines 190-201 and TimeoutAfter_NonGenericTask_WithRepeatAttempts_CompletesSuccessfully at lines 203-215.
      Acceptance: the file contains zero occurrences of the token `WithRepeatAttempts` and its line
      count is strictly less than 217, the count the file carries before this task.

- [x] [P4-T5] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/other/file-size-accounting.md — record the file-size accounting the acceptance criteria require.
      Acceptance: the artifact carries `Timestamp:`, `LinesBefore: 1011`, a measured `LinesAfter:`
      integer strictly between 500 and 1011, `CapLimit: 500`, `CapStillViolated: true`, and the
      verbatim sentence "This is a reduction, not a resolution, of the 500-line cap violation." No
      other artifact, code comment, commit message or PR body in this feature may state that the cap
      violation is resolved.

- [x] [P4-T6] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/regression-testing/phase4-green.md — rebuild with the D1 resolution followed by `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`, then run a D2 scoped run against UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll filtered to TimeOutTask_Tests and OlTableExtensions_Tests, then write this artifact.
      Acceptance: the artifact carries `Timestamp:`, two `Command:` lines, the first of which is the
      solution rebuild above, two `EXIT_CODE: 0` lines, `Output Summary:`,
      `TestsFailed: 0`, and a `TestsPassed:` integer. TimeOutTask_Tests is one partial class spanning
      UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs,
      UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs,
      UtilitiesCS.Test/Threading/TimeOutTask_InternalCoverageTests.cs and
      UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs, so this filter covers all four
      files. D10 governs how the counters vstest omits are recorded.

- [x] [P4-T7] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC3.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC3\*\*`, and
      UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs contains exactly one occurrence of the
      token `250 * rowCount`, inside EtlAsync.

- [x] [P4-T8] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC15.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC15\*\*`, and
      UtilitiesCS/Threading/TimeOutTask.cs contains zero lines matching `int repeatAttempts` and
      exactly one line matching `catch \(TimeoutException\)`.

- [x] [P4-T9] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC16.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC16\*\*`, and
      `git grep -n -- EtlAsyncOld -- "*.cs"` produces zero output lines. git grep is used rather than
      a filesystem scan so sibling worktrees under .claude/worktrees are not searched.

- [x] [P4-T10] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC17.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC17\*\*`;
      UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs contains zero occurrences of the token
      `WithRepeatAttempts`; and the artifact written by P4-T6 records `TestsFailed: 0`.

- [x] [P4-T11] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC18.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC18\*\*`, and the artifact written by P4-T5
      exists and carries `CapStillViolated: true`.

### Phase 5 — Item 3: The EtlAsync Nullable Tuple Contract

- [x] [P5-T1] UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs — change the declared return type of EtlAsync at line 66 so its first tuple element is nullable, and delete the null-forgiving suppression on the return statement at line 131.
      Acceptance: the file contains exactly one occurrence of the token `object[,]? data,`; the count
      of occurrences of the token `data!` is exactly 1, down from the EtlCsDataBangCount value of 2
      pinned by P0-T10; and the surviving occurrence is at the return of the synchronous ETL method,
      whose non-null contract is a pre-existing latent condition documented at Etl.cs lines 25-27 and
      is out of scope for this feature.

- [x] [P5-T2] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/ac11-etlasync-tuple.md — verify by anchored diff that the removed suppression is the one inside EtlAsync, by running with the D3 anchor `git diff $b -- UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs`, then write this artifact.
      Acceptance: the diff contains exactly one removed line whose text after the leading minus is
      `            return (data!, columnDictionary);` and exactly one added line whose text after the
      leading plus is `            return (data, columnDictionary);`; the artifact records
      `Timestamp:`, `Command:`, `EXIT_CODE: 0` and both counts.

- [x] [P5-T3] UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs — keep the swallow path in EtlAsync unchanged: the TimeoutException catch at lines 119-125, its logger.Error and its tokenSource.Cancel stay verbatim, and the LogTableTiming call at lines 127-130 that sits between the swallow and the return is untouched.
      Acceptance: the file contains exactly one line matching `tokenSource\.Cancel\(\);`, inside
      EtlAsync; the anchored diff for this file contains exactly one removed line matching
      `tokenSource\.Cancel`, which is the EtlAsyncOld occurrence at pre-change line 167 that P4-T2
      deleted, and zero added lines matching it; and the anchored diff contains zero removed lines
      matching `LogTableTiming`.

- [x] [P5-T4] UtilitiesCS/Extensions/DfDeedle.cs — change the three Item1 reads to the tuple's data name so the flow analysis the guard at line 182 establishes carries to both consumers under this file's nullable context opened at line 23, and rewrite the stale sentence at lines 180-181 that describes a null-forgiving suppression so it describes a nullable tuple element; the guard at lines 182-189 and its InvalidOperationException message are not touched.
      Two of the three reads are on line 193, inside the LogDfTiming payload. The third is on line
      210, inside the lambda argument to Email2dArrayToDf. Line 210 is in scope deliberately: the
      acceptance below is file-wide, and an edit confined to line 193 cannot reach it.
      The two tableSnapshot.Item2 reads, at lines 200 and 210, are not renamed. Only the first tuple
      element is read through the guard, so only it needs the name the guard establishes flow state
      for.
      Acceptance: the file contains zero occurrences of the token `tableSnapshot.Item1`; the file
      still contains exactly two occurrences of the token `tableSnapshot.Item2`; and the anchored
      diff for this file contains zero removed lines matching `is null` and zero removed lines
      matching `The table snapshot for folder`.

- [x] [P5-T5] UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs — update the doc comment on EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource at lines 99-103 so it no longer describes a null-forgiving suppression, leaving the test body and both assertions at lines 144-145 unchanged.
      Acceptance: the file contains zero occurrences of the token `null-forgiving`; the anchored diff
      for this file contains zero added or removed lines matching `data\.Should\(\)\.BeNull\(\)` and
      zero matching `IsCancellationRequested`; and every changed line in that diff begins with three
      slashes of doc-comment syntax.

- [x] [P5-T6] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/regression-testing/phase5-green.md — rebuild with the D1 resolution followed by `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`, then run a D2 scoped run against UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll filtered to OlTableExtensionsEtlClockTests, OlTableExtensions_Tests and the DfDeedle classes, then write this artifact.
      Acceptance: the artifact carries `Timestamp:`, two `Command:` lines, the first of which is the
      solution rebuild above, two `EXIT_CODE: 0` lines, `Output Summary:`,
      `TestsFailed: 0`, and a named result line showing
      EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource passing. D10 governs how the
      counters vstest omits are recorded.

- [x] [P5-T7] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC11, recording in the check-off report that the criterion's literal zero-hit wording is discharged by the two-to-one occurrence-count transition proven by P5-T1 and P5-T2, because the second occurrence at Etl.cs line 63 belongs to the out-of-scope synchronous ETL method.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC11\*\*`, and the artifact written by P5-T2
      exists.

- [x] [P5-T8] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC12.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC12\*\*`, and the P5-T3 and P5-T4 diff
      assertions both passed.

- [x] [P5-T9] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC13.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC13\*\*`, and the artifact written by P5-T6
      records EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource passing with the P5-T5
      body-unchanged assertion satisfied.

- [x] [P5-T10] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC14.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC14\*\*`, and
      UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs contains zero occurrences
      of the token `null-forgiving`.

### Phase 6 — Items 1 and 5: Budget Rationale and the Stale Doc Comment

- [x] [P6-T1] UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs — add the budget rationale comment immediately above the per-row budget expression inside EtlAsync, without altering that expression or its numeric literal, recording three facts: that the value rests on no recorded measurement; that no measurement is obtainable in this environment because there is no benchmark harness in any project file and the fixtures are Moq objects whose GetRowCount, GetNextRow and GetArray return in microseconds; and the live-Outlook capture route by name, namely the LogTableTiming EtlAsync-complete payload carrying rowCount and elapsedMs, together with the log4net root level ALL set at TaskMaster/log4net.config line 4.
      The comment names that payload by the LogTableTiming method name and carries no line-number
      citation into UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs. The executor reads the
      payload at pre-change lines 127-130 of that file, but this task inserts its comment block above
      the budget expression at pre-change line 84, so those four lines move down by the height of the
      comment as it is written, and P8-T1's format pass can move them again. A line number for this
      file written into the comment would therefore be stale before this task finishes, and unlike the
      artifact re-derivations at P3-T21, P6-T3 and P8-T13 it would persist in permanent source after
      this feature merges. The TaskMaster/log4net.config line 4 citation is unaffected and may be
      written: that file is outside this feature's Write Set and no task in this plan edits it.
      Acceptance: the file contains exactly one occurrence of the token `250 * rowCount`; the comment
      block immediately above it contains all three of the tokens `no recorded measurement`,
      `LogTableTiming` and `elapsedMs`; that comment block contains zero lines matching the regular
      expression `lines? [0-9]+-[0-9]+`; and, using the D3 anchor,
      `git diff $b -- UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs` produces exactly one
      removed line matching the regular expression `250 \* rowCount` and zero added lines matching it.
      Exactly one removed line, not zero: P4-T2 deleted EtlAsyncOld, whose pre-change line 149 carried
      a byte-identical copy of the expression, so the zero-removed form cannot hold at this point in
      the plan. The zero-added half is what proves the surviving expression inside EtlAsync and its
      numeric literal were not rewritten.

- [x] [P6-T2] UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs — correct the stale doc comment at line 96 by replacing the name TableEtlInvoker with DefaultTableEtl, which is declared at DfDeedle.cs lines 67-70, retaining the CS1769 rationale sentence unchanged.
      Acceptance: the file contains zero occurrences of the token `TableEtlInvoker`, at least one
      occurrence of the token `DefaultTableEtl`, and exactly one occurrence of the token `CS1769`.

- [x] [P6-T3] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/ac19-historical-records.md — confirm the historical record this feature must not rewrite is untouched, by running with the D3 anchor `git diff $b -- UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs` and counting added or removed lines matching TableEtlInvoker, then write this artifact.
      Acceptance: that count is zero, so the historically accurate past-tense mention of
      TableEtlInvoker survives. It sits at pre-change line 212 and P3-T7 and P3-T8 add lines above it,
      so the artifact records the line number re-derived at the time of this check rather than 212. The
      artifact records `Timestamp:`, `Command:`, `EXIT_CODE: 0` and the count.

- [x] [P6-T4] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC1.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC1\*\*`, and the P6-T1 one-removed,
      zero-added assertion for the budget expression passed.

- [x] [P6-T5] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC2.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC2\*\*`, and the three tokens named in P6-T1
      are all present in the comment block.

- [x] [P6-T6] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC19.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC19\*\*`, and the three P6-T2 counts hold.

### Phase 7 — Item 6: DoNotParallelize Reconciliation

This phase runs last among the editing phases. Its first edit is gated on the AC22 check-off
completed at P3-T21, which established that no test in OlTableExtensions_Tests still arms a real
timed source on the system clock.

- [x] [P7-T1] UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs — replace the class comment at lines 18-20 with one recording what changed: the class contains four tests calling GetTableInViewAsync, not ten; GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot now supplies a FakeTimeProvider so no wall-clock deadline governs any test in the class; and the attribute is therefore removed.
      The replacement comment names that test by its method name and carries no line-number
      citation. Its declaration sat at pre-change line 1646, but P3-T9 added a type entry and a
      trailing argument at each of the three earlier binding sites and P4-T3 deleted the EtlAsyncOld
      test above it, so 1646 is no longer where it sits. Writing that number into permanent source
      would reproduce the defect this task exists to correct: the comment being replaced is wrong
      precisely because it states a fact about the file that the file no longer carries. Lines 18-20
      are themselves unmoved, because every edit this plan makes to this file before this task is
      below them.
      Acceptance: the file contains zero lines matching the token `soak`, down from one before this
      feature; the replacement comment contains the token `FakeTimeProvider` and the token
      `GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot`; the replacement
      comment contains zero lines matching the regular expression `line [0-9]`; and the comment
      asserts no population of tests driving the 2000 ms window.

- [x] [P7-T2] UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs — remove the DoNotParallelize attribute, leaving the TestClass attribute that follows it in place. The attribute sits at pre-change line 21 with TestClass at pre-change line 22; P7-T1 has already replaced the three-line class comment above them, so both are located by content rather than by line number.
      Acceptance: the file contains zero occurrences of the token `[DoNotParallelize]` and exactly one
      occurrence of the token `[TestClass]`.

- [x] [P7-T3] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/other/ac21-justification.md — record the justification for the attribute removal.
      Acceptance: the artifact carries `Timestamp:`, `RunsObserved: 0`, a
      `Justification: item 2 change` line, the token `FakeTimeProvider`, and a sentence naming
      UtilitiesCS/Threading/TimeOutTask.cs line 63 as the Task.Run the removed wall-clock deadline
      used to govern. The zero value of RunsObserved is the machine-checkable form of the criterion
      forbidding a soak: the justification is the code change, not observed green runs.

- [x] [P7-T4] UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs — add the verified reason comment on the lines immediately above the DoNotParallelize attribute at line 10 and below the TestClass attribute at line 9, naming the two wall-clock races in the class: the 200-millisecond delay raced against a 10-millisecond timeout, and the 50-millisecond delay raced against a zero timeout.
      The comment names each race by its Task.Delay literal and its timeout argument and carries no
      line-number citation. The executor finds the two tests at pre-change lines 27-37 and 40-50,
      declared under TestMethod attributes at 26 and 39, and P4-T4's deletions at pre-change lines
      190-215 sit below both and move neither. This task's own comment lines are inserted between the
      TestClass attribute at line 9 and the DoNotParallelize attribute at line 10, which is above both
      ranges, so writing either range into the comment makes it stale by the height of the comment
      itself, and P8-T1's format pass can move both again. As at P7-T1, that stale figure would
      persist in permanent source after this feature merges rather than in an artifact a later task
      re-derives.
      Acceptance: the file still contains exactly one occurrence of the token `[DoNotParallelize]`;
      the comment lines immediately preceding it contain both of the tokens `Task.Delay(200)` and
      `Task.Delay(50)`; those comment lines contain zero lines matching the regular expression
      `lines? [0-9]+-[0-9]+`; and the line number of the DoNotParallelize occurrence is strictly
      greater than the line number of the TestClass occurrence.

- [x] [P7-T5] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/ac23-attributes-retained.md — verify the two classes that keep their attributes are untouched, then write this artifact.
      Acceptance: UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs contains
      exactly one occurrence of `[DoNotParallelize]` at line 21 with its reason comment at line 20
      intact; UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs contains exactly one occurrence
      of `[DoNotParallelize]` at line 24 with its reason comment at lines 22-23 intact; the anchored
      diff for each of those two files contains zero added or removed lines matching
      `DoNotParallelize`; and UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsRetryTests.cs
      still contains zero occurrences of `[DoNotParallelize]`.

- [x] [P7-T6] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/regression-testing/phase7-green.md — rebuild with the D1 resolution followed by `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`, then run a D2 run of the whole UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll assembly with a test-case filter excluding the LiveOutlook category, then write this artifact.
      Acceptance: the artifact carries `Timestamp:`, two `Command:` lines, the first of which is the
      solution rebuild above, two `EXIT_CODE: 0` lines, `Output Summary:`,
      `TestsFailed: 0` and `TestsSkipped: 0`, both transcribed per D10, and a `TestsPassed:` integer
      read from the printed `Passed:` line. The LiveOutlook exclusion is
      mandatory: the repository's only test in that category constructs a real Outlook Application and
      polls a live store, which is an external-process dependency the unit-test policy forbids. D6
      applies if the run does not terminate. D10 governs how the counters vstest omits are recorded.

- [x] [P7-T7] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC21, strictly after the AC22 check-off completed at P3-T21.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC21\*\*`; the AC22 line already matches
      `^- \[x\] \*\*AC22\*\*`; and
      UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs contains zero occurrences of
      `[DoNotParallelize]` and zero occurrences of the token `soak`.

- [x] [P7-T8] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC23.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC23\*\*`, and the artifact written by P7-T5
      records all five of its assertions passing.

- [x] [P7-T9] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC24.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC24\*\*`, and
      UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs contains both of the tokens `Task.Delay(200)`
      and `Task.Delay(50)` on comment lines immediately above its single DoNotParallelize attribute.

- [x] [P7-T10] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC25.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC25\*\*`; the artifact written by P7-T3
      carries `RunsObserved: 0`; and every artifact under
      docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/ contains zero
      lines containing the token `soak`. The commit-message check is not repeated here: no task before
      P9-T4 creates a commit, so at this point `git log $b..HEAD` names an empty range and returns zero
      lines whatever the executor has written. P9-T4 performs that check against the commit it makes.

### Phase 8 — Final QC Toolchain Loop, Coverage and Boundary Gates

Run tasks T1 through T6 in this exact order. If any of them fails, or if the formatter reports that
it changed a tracked file, restart this phase from T1. Do not proceed past a failing step.

- [x] [P8-T1] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/qc-csharpier-format.md — format the tree by running `dotnet tool run csharpier format .` from the repository root, then write this artifact.
      Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, a
      `FormattedFileCount:` line carrying the integer CSharpier prints on its summary line, and a
      `ChangedFileCount:` line derived from an anchored numstat taken immediately before and
      immediately after the format, namely `git diff --numstat $b -- . ":(exclude).claude"` with the
      D3 anchor and the D4 exclusion, counting the paths whose insertion or deletion figure differs
      between the two runs. A porcelain status is not usable for this observation: every file this
      plan edits is already modified and therefore already listed before the format runs, and P0-T5
      has excluded pre-existing drift in every file that is still clean, so a status-set comparison
      reports zero however much content the formatter rewrote. The numstat form compares content and
      is therefore falsifiable. Each of the two numstat runs is preceded by the staging span
      `git add --intent-to-add -- . ":(exclude).claude"`, because a numstat enumerates tracked
      changes only and UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs, which
      P2-T1 created and no task has yet committed, would otherwise be invisible to both runs and its
      reflow undetectable. A non-zero ChangedFileCount is expected on the first pass and restarts
      this phase: P3-T6 writes DfDeedle.cs line 148 as a 113-column single-line call and P3-T7 and
      P3-T8 take the three BuildExplorer call sites in
      UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs to 105, 102 and 102 columns, and
      CSharpier's 100-column default print width rewraps all four.
      The exit code alone is not the observation: it is 0 both when the formatter changed nothing and
      when it repaired drift, so the before-and-after tree observation is what makes this step
      falsifiable. FormattedFileCount is a processed count, not a repaired count, so it is recorded
      but never used as a restart trigger; a non-zero ChangedFileCount is the restart trigger.
      This task additionally re-runs the P3-T14 boundary filter after the format: using the D3 anchor,
      `git diff $b -- UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` filtered for
      lines matching the regular expression `^[+-][^+-].*Console\.WriteLine` must still produce zero
      lines, and the artifact records `ConsoleWriteLineDiffLinesAfterFormat: 0`. P3-T14 ran before any
      formatter pass, so without this re-check a reflow introduced by `csharpier format .` would
      falsify AC26 after its check-off at P3-T22. A non-zero count restarts this phase and is reported.

- [x] [P8-T2] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/qc-csharpier-check.md — verify formatting by running `dotnet tool run csharpier check .` from the repository root, then write this artifact.
      Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`,
      and a `CheckedFiles:` line whose integer is greater than or equal to the CheckedFiles figure
      recorded by P0-T5, since this feature adds one C# file and deletes none.

- [x] [P8-T3] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/qc-build-analyzers.md — run the analyzer gate using the D1 resolution followed by `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` with a detailed file log written to the sibling qc-build-analyzers.txt in the same folder.
      Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`,
      `WarningCount:` and `ErrorCount: 0`. /t:Rebuild is mandatory; a warm /t:Build returns exit 0
      having skipped CoreCompile on every project, which would make this gate vacuous.

- [x] [P8-T4] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/qc-build-nullable.md — run the nullable gate using the D1 resolution followed by `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` with a detailed file log written to the sibling qc-build-nullable.txt in the same folder.
      Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`,
      `WarningCount:` and `ErrorCount: 0`. Do not add /p:Nullable=enable.

- [x] [P8-T5] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/ac32-non-vacuity.md — demonstrate non-vacuity of both MSBuild gates from the two sibling txt logs written by P8-T3 and P8-T4, then write this artifact.
      Acceptance: for each of the two logs the artifact records `SkippingCoreCompileCount: 0`, counted
      as lines containing the token `Skipping target "CoreCompile"`, and a
      `CscInvocationsForWriteSetProjects:` integer of at least 2, counted as the number of log lines
      containing the token `/out:obj\Debug\UtilitiesCS.dll` plus the number containing the token
      `/out:obj\Debug\UtilitiesCS.Test.dll`. Those are the csc.exe command lines MSBuild echoes under
      each project's CoreCompile heading, and they are the only single-line evidence that names both
      the compiler invocation and the project it compiled. A count of lines carrying both
      `Task "Csc"` and a project file name must not be substituted: MSBuild prefixes a task-start
      line with the project instance id and never with the project path, so those two tokens never
      appear on one line and that count is zero whatever the build did. The second figure is
      what makes the first non-vacuous: a zero count of skip messages proves nothing unless
      compilation is shown to have run.

- [x] [P8-T6] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/qc-coverage-postchange.md — run the full coverage-enabled suite with the D5 command, pointing its coverage output at the sibling coverage-postchange.cobertura.xml in the same folder, then write this artifact.
      Acceptance: the Cobertura XML exists; the artifact carries `Timestamp:`, `Command:`,
      `EXIT_CODE:`, `Output Summary:`, the same six numeric root-element lines P0-T8 recorded, plus
      `TestsPassed:` and `TestsFailed: 0`. All eight values are numeric. D6 applies if the run does not
      terminate. D9 applies if the run is red. D10 governs how the counters vstest omits are recorded.

- [x] [P8-T7] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/ac33-coverage-comparison.md — compare baseline and post-change coverage counters read from evidence/baseline/coverage-baseline.cobertura.xml and evidence/qa-gates/coverage-postchange.cobertura.xml, then write this artifact.
      Acceptance: the artifact carries `Timestamp:`, the six baseline values, the six post-change
      values, and these four decided gates. Every package-level and class-level figure below is
      derived, not read: a Cobertura package element carries line-rate, branch-rate, complexity and
      name only, and a class element adds filename and nothing more. Derive each package figure with
      Get-CoberturaPackageLineSummary, declared at
      scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1, and each per-filename figure with
      Get-CoberturaClassLineSummary, declared at
      scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 line 158. Both deduplicate by line
      number, and the same pair produces the root-element attributes this artifact also records, so
      the package sums reconcile with the root totals by construction. A hand-rolled count of line
      descendants double-counts and does not reconcile.
      Gate A: LinesValid post is less than or equal to baseline
      for the `UtilitiesCS` package element only. The repository-wide figure is reported
      informationally and is not gated because it aggregates eight further production assemblies this
      feature does not touch, so a movement in any of them would be attributed to this change. Test
      assemblies are not in the processed figure:
      scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 line 6 builds a first-party allowlist that
      drops every project whose assembly name ends in .Test (lines 22-47), and
      ConvertTo-KoverageCoberturaXml at line 405 strips every package outside that allowlist from
      both the numerator and the denominator. The run-time module pattern `.*\.Test\.dll$` that the
      runner appends at lines 99-112 does not do this work: it has been present since 2026-07-24 and
      the two raw Cobertura artifacts committed under
      docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/
      on 2026-09-07 still carry instrumented UtilitiesCS.Test, QuickFiler.Test and seven further
      .Test packages. Test assemblies are therefore instrumented and then removed during
      post-processing, so a raw Cobertura is not interchangeable with a processed one for this
      figure, and every processed Cobertura committed under docs/features/ carries production
      packages only. Gate B: LinesCovered post for the `UtilitiesCS`
      package is greater than or equal to that package's baseline LinesCovered minus its reduction in
      LinesValid. Gate C: the same two comparisons for BranchesValid and BranchesCovered on the
      `UtilitiesCS` package. Gate D: for each of the five edited production files, the signed per-filename
      LinesCovered delta, aggregated over all class elements sharing that filename, is recorded, and a
      negative delta on any file this feature did not shrink is a failure. Raw LineRate and BranchRate
      are reported informationally only. A raw-rate no-regression gate is not used and must not be
      substituted: deleting fully covered lines lowers the aggregate rate by arithmetic even when every
      surviving line keeps its coverage.
      The artifact additionally carries `ProductionLinesCovered:` and `ProductionLinesValid:` integers
      and a `TestableDenominatorLineRate:` decimal. The production-only aggregate is defined
      mechanically as the sum of the Get-CoberturaPackageLineSummary results over every package
      element in the Cobertura file whose name does not end in .Test or .Tests; no per-class
      judgment is exercised. That filter is expected to
      select every package in the file, because the first-party allowlist has already stripped every
      .Test package from the processed Cobertura during post-processing;
      it is retained as a mechanical guard, and the artifact records
      `ModulesFilteredOut:`, whose expected value is 0. The artifact records that
      CLAUDE.md § UT2 permits exemption through exactly two mechanisms, an [ExcludeFromCodeCoverage]
      attribute in source and an assembly-level exclude in coverage.config, and that each contributes
      zero first-party exclusions in this tree: no [ExcludeFromCodeCoverage] attribute appears on any
      in-scope production file, and coverage.config's ModulePaths excludes only the third-party modules
      Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing and MSTest, to which the
      runner adds `.*\.Test\.dll$` at run time. No first-party production module is excluded by
      either mechanism. The testable
      denominator therefore equals the production-only denominator, and the artifact asserts that
      identity rather than deriving an exemption list. It then carries either
      `TestableDenominatorFloorMet: true` when that rate is at or above 0.80, or, when it is below,
      `NotLoweredVersusBaseline:` computed against the same figure derived from the baseline Cobertura.

- [x] [P8-T8] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/ac33-coverage-comparison.md — append the deletion attribution to the same artifact.
      Acceptance: the artifact carries a `DeletionAttribution:` block naming the two TimeoutAfter
      overloads and EtlAsyncOld with the LinesValid reduction each contributed, and naming the three
      deleted tests EtlAsyncOld_WithBinaryAndObjectFields_ReturnsTransformedData,
      TimeoutAfter_GenericTask_WithRepeatAttempts_ReturnsResult and
      TimeoutAfter_NonGenericTask_WithRepeatAttempts_CompletesSuccessfully with a recorded
      contribution of exactly 0 each, because all three live in UtilitiesCS.Test and that package is
      stripped from the processed Cobertura by the first-party allowlist before any figure in this
      plan is read; an `AdditionAccounting:` block naming the added production statements and
      stating the LinesValid each contributed; and a reconciliation showing that the deletion sum
      minus the addition sum equals the `UtilitiesCS`-package LinesValid delta recorded by Gate A,
      with a residual of exactly zero. Every per-construct figure is obtained by differencing the
      baseline and post-change per-filename line-number sets for the file that construct lived in,
      using Get-CoberturaClassLineSummary; a count of deleted source lines is not a substitute,
      because a comment, a declaration and a closing brace carry no line element. A reconciliation
      of the deletion sum alone against the total cannot close, because the total is deletions net of
      additions and this feature adds a parameter, a resolved local and a comment block. If the
      reconciliation does not close, the artifact records the residual and this task fails.

- [x] [P8-T9] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/ac33-changed-line-coverage.md — compute changed-line coverage by taking the added-line numbers for each edited production file from `git diff $b --unified=0` using the D3 anchor, intersecting each file's set with the line elements of evidence/qa-gates/coverage-postchange.cobertura.xml aggregated by filename, and reporting covered over total, then write this artifact.
      The per-filename line data is derived, not read directly. Each class element repeats every
      line number twice, once under methods/method/lines and once in the class-level lines rollup,
      and a single source file is split across several class elements because an async method
      compiles to its own state machine class. Build each file's line map with
      Get-CoberturaClassLineSummary, declared at
      scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 line 158, over every class element whose
      filename attribute ends in that file's name, and merge the resulting maps by line number,
      resolving a line number that appears in more than one map by taking the maximum hits value.
      That is the same derivation P0-T9 and P8-T7 use, and it is required here for the same reason:
      a line read from one class element, or from one of the two repeated views, can carry a hits
      value that a sibling entry for the same line contradicts, and
      UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs carries most of this
      feature's changed lines inside an async method, which is exactly the case that produces more
      than one class element for one filename. A line is counted in ChangedLinesCovered: when its
      merged hits value is greater than zero.
      Acceptance: the artifact carries `Timestamp:`, one `File:` block per edited production file with
      `ChangedLines:`, `ChangedLinesCovered:` and a `ChangedLineRate:` that is a decimal when
      `ChangedLines:` is greater than zero and the literal `n/a` when it is zero, and an overall
      `NewAndChangedCodeRate:` decimal greater than or equal to 0.90, computed as the sum of
      `ChangedLinesCovered:` over the sum of `ChangedLines:` across the files whose `ChangedLines:` is
      greater than zero. Two files are expected to report `ChangedLines: 0`:
      UtilitiesCS/Threading/TimeOutTask.cs, which P4-T1 only deletes from, and
      UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs, whose single added line is the P6-T2 doc comment.
      A comment or declaration line carries no line element, so the intersection correctly leaves
      those two files empty; a rate of zero must not be recorded for them, because it would lower the
      overall figure by arithmetic rather than by coverage. No ExcludeFromCodeCoverage
      attribute exists on any in-scope production file, so every changed line is measurable.

- [x] [P8-T10] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/ac20-docs-boundary.md — enforce the documentation boundary by running, with the D3 anchor, an intent-to-add over the worktree with the D4 exclusion, then `git diff --name-only $b -- docs/features/` and `git status --porcelain --untracked-files=all -- docs/features/`, then write this artifact.
      Acceptance: every path either command reports begins with
      docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/; zero reported paths begin
      with docs/features/epics/ or docs/features/potential/ or name any other active feature folder;
      and the artifact records `Timestamp:`, both `Command:` lines, both `EXIT_CODE:` values and the
      full reported path list. The intent-to-add span is required because a name-listing diff
      enumerates tracked changes only and would otherwise report none of the evidence files this plan
      creates.

- [x] [P8-T11] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/ac28-sibling-ownership.md — enforce the sibling-owned file exclusion by running, with the D3 anchor, first the staging span `git add --intent-to-add -- . ":(exclude).claude"` so files created but not yet tracked enter the name listing, then `git diff --name-only $b` with the D4 exclusion pathspec, and comparing the union of both outputs against the sibling-owned list in spec.md Non-goals, then write this artifact.
      Acceptance: the artifact records both `Command:` lines and both `EXIT_CODE:` values, the
      intent-to-add span having run before the name-listing diff so an untracked sibling-owned file
      cannot escape the listing; zero reported paths match any of
      QuickFiler/Controllers/QfcItemController.FolderHandling.cs,
      QuickFiler/Controllers/QfcHomeController.cs, UtilitiesCS/Threading/ProgressViewer.cs,
      UtilitiesCS/OutlookObjects/Store/StoreWrapperController, QuickFiler/Viewers/Breadcrumb,
      UtilitiesCS/NewtonsoftHelpers/SDIL Reader/, UtilitiesCS.Test/Properties/AssemblyInfo.cs,
      UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs, .editorconfig or
      BannedSymbols.txt; and the artifact records `Timestamp:`, both exit codes as 0, the full
      reported path list and the ten-entry exclusion list checked against it.

- [x] [P8-T12] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/other/ac34-value-trace.md — write the reviewer trace of one accepted timeout value.
      Acceptance: the artifact carries `Timestamp:` and four numbered steps, each citing a post-change
      file and line: the accept point in
      UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs where timeoutMs is declared;
      the resolution point where the deadline factory is derived from the coalesced provider; the
      throw point at UtilitiesCS/Threading/TimeOutTask.cs line 52 where the factory is invoked outside
      the try that follows; and the retry point in TableAccess.cs where the caller's timeoutMs is now
      passed instead of the literal. It also records that TimeOutTask.RunWithTimeout signatures, its
      strict semantics and its retry behaviour are unchanged, and that GetTableInViewAsync still
      passes one maximum attempt and non-strict mode, so the two-deep retry layering and the total
      attempt count are unchanged.

- [x] [P8-T13] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/other/ac35-reachability-observation.md — write the reachability observation and the deferred follow-up handoff.
      Acceptance: the artifact carries `Timestamp:` and records the traced conclusion that
      RunWithTimeout returns null rather than throwing on the ordinary timeout path, so neither catch
      block in GetTableInViewAsync and neither retry recursion is entered by that path; it names
      UtilitiesCS/Threading/TimeOutTask.cs lines 52, 60, 63, 65, 67, 69, 82 and 94, which Phase 4's
      deletions at pre-change lines 824-940 leave unmoved, and four TableAccess.cs sites cited by the
      line numbers the file carries after Phase 3 and re-derived when this artifact is written: the try
      opening the RunWithTimeout await, the TaskCanceledException catch, the TimeoutException catch and
      the `return table!`. Their pre-change numbers were 55, 71, 95 and 118; Phase 3's additions move
      all four. It records that this is a pre-existing condition this feature does not fix; it
      records `PromotionWrittenOnThisBranch: false`; and it carries a `DeferredHandoff:` block
      addressed to the epic listing all four follow-ups from spec.md Rollout and stating they are filed
      after this feature merges. No file outside
      docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/ is created or modified by
      this task and no promotion tool is called.

- [x] [P8-T14] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC20 in the form the orchestrator amended it into during preparation and P3-T1 verified on disk. Its implementation is the documentation-boundary enforcement performed at P8-T10.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC20\*\*`, and the artifact written by P8-T10
      records zero out-of-folder documentation paths.

- [x] [P8-T15] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC28.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC28\*\*`, and the artifact written by P8-T11
      records zero matches against the ten-entry exclusion list.

- [x] [P8-T16] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC29.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC29\*\*`; all four new tests are in
      UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs; and the P2-T2 numstat
      assertion of exactly one added and zero removed lines in
      UtilitiesCS.Test/UtilitiesCS.Test.csproj still holds when re-run against the D3 anchor.

- [x] [P8-T17] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC30.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC30\*\*`, and, using the D3 anchor,
      `git diff $b -- UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` produces zero
      lines matching the regular expression `^\+.*\[TestMethod\]`.

- [x] [P8-T18] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC31.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC31\*\*`, and
      UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs contains zero lines
      matching the regular expression `Thread\.Sleep|Task\.Delay|DateTime\.Now|Stopwatch` and zero
      lines matching the regular expression `for \(|while \(`.

- [x] [P8-T19] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC32.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC32\*\*`; the artifacts written by P8-T2,
      P8-T3, P8-T4, P8-T5 and P8-T6 all exist; and P8-T5 records SkippingCoreCompileCount as zero for
      both logs with a CscInvocationsForWriteSetProjects value of at least 2.

- [x] [P8-T20] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC33.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC33\*\*`; the artifacts written by P8-T7,
      P8-T8 and P8-T9 all exist; Gates A through D all pass; NewAndChangedCodeRate is at least
      0.90; and the P8-T7 artifact carries `TestableDenominatorLineRate:` with either
      `TestableDenominatorFloorMet: true` or `NotLoweredVersusBaseline: true`.

- [x] [P8-T21] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC34.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC34\*\*`, and the artifact written by P8-T12
      exists and carries all four numbered steps with post-change line citations.

- [x] [P8-T22] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — check off AC35.
      Acceptance: exactly one line matches `^- \[x\] \*\*AC35\*\*`; the artifact written by P8-T13
      exists and carries `PromotionWrittenOnThisBranch: false`; and the P8-T10 boundary check reports
      zero paths under docs/features/potential/.

### Phase 9 — Reconciliation, Commit and Handoff

- [x] [P9-T1] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md — reconcile the acceptance-criteria state against the evidence on disk.
      Acceptance: the count of lines matching `^- \[x\] \*\*AC[0-9]+\*\*` is exactly 35 and the count
      matching `^- \[ \] \*\*AC[0-9]+\*\*` is exactly 0. For each of the 19 criteria whose check-off
      task names an evidence artifact, that artifact exists under
      docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/; a checked box
      whose named artifact is absent is reverted to unchecked and this task fails. Those 19 are AC5
      (P3-T16), AC6 (P3-T17), AC7 (P3-T18), AC8 (P1-T4), AC10 (P3-T20), AC11 (P5-T7), AC13 (P5-T9),
      AC17 (P4-T10), AC18 (P4-T11), AC20 (P8-T14), AC23 (P7-T8), AC25 (P7-T10), AC26 (P3-T22), AC27
      (P3-T23), AC28 (P8-T15), AC32 (P8-T19), AC33 (P8-T20), AC34 (P8-T21) and AC35 (P8-T22). The
      remaining sixteen criteria are checked off against a file-content count, a search, or a prior
      task's assertion rather than against an artifact, and are AC1 (P6-T4), AC2 (P6-T5), AC3
      (P4-T7), AC4 (P3-T15), AC9 (P3-T19), AC12 (P5-T8), AC14 (P5-T10), AC15 (P4-T8), AC16 (P4-T9),
      AC19 (P6-T6), AC21 (P7-T7), AC22 (P3-T21), AC24 (P7-T9), AC29 (P8-T16), AC30 (P8-T17) and AC31
      (P8-T18). For each of those sixteen this task re-runs the count or search its check-off task
      named and records the re-derived value; a value that no longer holds reverts that box to
      unchecked and fails this task. The two lists are disjoint and together cover all 35 criteria.
      On the P1-T4 fallback
      branch the two expected counts are 34 and 1 instead, AC8 is the single unchecked criterion, and
      the artifact required in its place is
      docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/other/ac8-refuted-fallback.md.

- [x] [P9-T2] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/issue-updates/ac-status-summary.md — write the acceptance-criteria status summary.
      Acceptance: the artifact carries `Timestamp:` and the four required lines in the
      acceptance-criteria-tracking format, namely a Source line naming spec.md,
      `Total AC items: 35`, `Checked off (delivered): 35` and `Remaining (unchecked): 0`, plus an
      `Items remaining:` line whose value is none. On the P1-T4 fallback branch the three lines read
      `Total AC items: 35`, `Checked off (delivered): 34` and `Remaining (unchecked): 1`, the
      `Items remaining:` value is AC8, and the plan outcome reported is INCOMPLETE.

- [x] [P9-T3] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/issue.md — record the delivered outcome, the three spec amendments to AC6, AC20 and AC35 with their measured reasons, and the four deferred follow-ups.
      Acceptance: issue.md contains the token `Amended 2026-09-09`, the token
      `DfDeedleEtlTimeoutTests`, a status line recording delivery, and a sentence recording all three
      of these facts: that the AC6, AC20 and AC35 amendments were made during preparation by the
      orchestrator and not by the executor; the measured reason for each, namely for AC6 and AC20
      that threading the provider inserts the table-acquisition timer as the first arming signal on
      the latch-based barrier the test at DfDeedleEtlTimeoutTests.cs line 135 consumes in a fixed
      order, and for AC35 the substitution of an evidence artifact plus a deferred epic handoff for
      an on-branch promotion; and that no acceptance criterion was amended during execution, which
      P3-T1 verified on disk before any Phase 3 source edit. Its `- Work Mode: full-bug` line is
      unchanged.

- [ ] [P9-T4] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/ac18-commit-language.md — commit every source and evidence change with explicit pathspecs covering UtilitiesCS/, UtilitiesCS.Test/ and this feature folder, using a message body that describes the TimeOutTask.cs change as a reduction of the 500-line cap violation and never as a resolution of it, then write this artifact recording the commit sha and the language checks.
      Acceptance: the command span `git status --porcelain --untracked-files=all -- . ":(exclude).claude"`
      produces zero output lines; `git log -1 --pretty=%B` produces zero lines containing the token
      `soak` and zero lines matching the regular expression `resolv.*500`; and, using the D3 anchor,
      the command span `git diff --name-only $b -- . ":(exclude).claude"` satisfies both directions of
      the Write Set accounting. Direction one: every path it reports is either one of the eleven
      backticked spec.md Write Set paths or a path under
      docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/, and nothing else.
      Direction two: every one of the eleven Write Set paths appears in the report, which holds
      because this plan edits all eleven, UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs
      among them since the Write Set gained it on 2026-09-09. The porcelain span is the companion the
      name-listing diff requires: the diff enumerates tracked changes only, and the zero-line
      porcelain result is what proves nothing was left untracked and therefore unreported. Each of
      the five committed MSBuild logs D7 names —
      docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/build-analyzers.txt,
      the sibling build-nullable.txt, evidence/other/ac8-createcancellationtokensource-proof.txt,
      evidence/qa-gates/qc-build-analyzers.txt and the sibling qc-build-nullable.txt — contains zero
      lines containing the token `C:\Users\`, which is the confirming check D7's sanitisation
      requires and the reason the commit does not leak the host account name; a non-zero count
      fails this task, and it is corrected by re-running the D7 rewrite rather than by amending the
      commit alone. The
      artifact records `Timestamp:`, all three `Command:` lines, all three `EXIT_CODE:` values, the
      commit sha, the full reported path list and the five per-log `C:\Users\` counts.

- [ ] [P9-T5] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/other/review-handoff.md — write the review handoff index listing every artifact this plan produced with its path and one-line purpose, naming the adjudicated design conflict section of this plan as the first item a reviewer must read.
      Acceptance: the artifact carries `Timestamp:` and one bullet per artifact path written by Phases
      0 through 9, and every listed path exists on disk.

- [ ] [P9-T6] docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/other/review-handoff.md — commit this artifact and anything else P9-T5 touched, using explicit pathspecs under this feature's folder.
      Acceptance: the command span `git status --porcelain --untracked-files=all -- . ":(exclude).claude"`
      produces zero output lines, and the command span
      `git diff --name-only HEAD~1 HEAD -- . ":(exclude).claude"` lists at least one path and only
      paths under docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/. The
      porcelain span is the companion the name-listing diff requires, and it runs after the commit so
      its zero-line result proves the artifact reached the commit rather than remaining untracked and
      invisible to the diff. This second commit
      exists because P9-T5 writes an artifact after the clean-tree commit, and a plan whose terminal
      state is a dirty worktree is not complete.
      One residual is permitted and expected: this plan file's own check-off for P9-T6, which is
      written after the commit P9-T6 makes and therefore cannot be inside it. The executor records it
      in its final report as `UncommittedResidual: plan.2026-09-08T23-51.md (P9-T6 check-off only)` and
      confirms by
      `git diff --numstat HEAD -- docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/plan.2026-09-08T23-51.md`
      that it is one insertion and one deletion in one file. No other path may be dirty. The porcelain
      span above runs before that check-off, so its zero-line result remains the gate.
