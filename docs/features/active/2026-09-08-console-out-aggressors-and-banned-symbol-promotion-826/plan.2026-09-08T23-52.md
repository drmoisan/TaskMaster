# 2026-09-08-console-out-aggressors-and-banned-symbol-promotion (Plan)

- **Issue:** #826
- **Parent (optional):** epic `review-residuals-2026-09-08` (child F826, wave 1, `depends_on: [825]`)
- **Owner:** drmoisan
- **Last Updated:** 2026-09-09 (date precision; no clock read was available to the revising agent, so
  the minute component is deliberately omitted rather than synthesized. The `yyyy-MM-ddTHH-mm`
  convention continues to govern evidence artifact filenames, which are written by the executor from
  an observed clock.)
- **Status:** Ready for preflight (revision rounds 1 through 5 applied)
- **Version:** 0.5
- **Work Mode:** `full-bug` (from `issue.md` line 12). Acceptance criteria come from `spec.md`
  section `## Acceptance Criteria` only. No `user-story.md` exists and none may be created.

**Fail-closed evidence rule:** every command-bearing task writes an evidence artifact carrying
`Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Every value in every artifact is an
observation made before that artifact is written; no artifact states a command's result in advance.
In the single case where those two requirements would conflict - P8-T21, the terminal commit task,
whose artifact is itself swept into the commit it describes - the artifact's `Command:` and
`EXIT_CODE:` name and carry the pre-commit `git status` invocation that P8-T21 runs and observes
before writing, and the terminal commit's own exit code is reported to the orchestrator in the
completion message. This is a scoping clause, not an exemption: `EXIT_CODE:` is still present and
still observed. Baseline and final-QC test artifacts additionally carry numeric coverage headline
values. A missing or incomplete artifact makes the task incomplete; the checklist box stays unchecked
and the verdict is BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** every evidence-producing task names its artifact path in the task text.
All evidence resolves under
`docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/evidence/<kind>/`.
Paths under `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`,
`artifacts/evidence/` and `artifacts/coverage/` are forbidden and are not used anywhere in this plan.

---

## Decisions and derivations this plan is built on

Each item below was re-derived against the tree in this worktree during plan authoring, except where
an item names a later dated revision round, in which case that round's re-derivation supersedes the
authoring-pass one. Line numbers quoted here are dated observations used to justify a decision; no
acceptance condition in this plan depends on a line number in a file that feature 825 owns.

- **D1. Item-2 edits are text-anchored, never line-anchored.** Feature 825 owns the timeout mechanics
  of `GetTableInViewAsync` in `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` and
  executes in wave 0, ahead of this feature. Drift is already demonstrated: `issue.md` lines 54 and 55
  cite the two diagnostics at lines 78 and 96; `spec.md` correction 2 supersedes that with 79 and 97;
  and a direct read of the file during plan authoring confirms 79 and 97. The issue-body pair is
  therefore already stale, and the spec's pair becomes stale the moment 825's edits land, which is why
  no acceptance condition here may cite either. Every item-2 edit is located by the literal statement
  text
  `Console.WriteLine` plus its enclosing catch clause type (`catch (TaskCanceledException)` for the
  first, `catch (TimeoutException)` for the second). This feature owns exactly those two statements
  and nothing else in that file.

- **D2. Both diagnostics are unreachable through the `timeoutSourceFactory` seam, as the tree stands.**
  The derivation, re-run during plan authoring, is:
  1. `GetTableInViewAsync` calls `TimeOutTask.RunWithTimeout(view.GetTable, token, timeoutMs, 1, false, timeoutSourceFactory)`
     (`OlTableExtensions.TableAccess.cs` lines 57 to 64). The `strict` argument is `false`.
  2. `view.GetTable` is a `Func<TResult>`, so the call binds the public overload
     `RunWithTimeout<TResult>(this Func<TResult>, CancellationToken, int, int, bool, Func<int,CancellationTokenSource>?)`
     at `UtilitiesCS/Threading/TimeOutTask.cs` line 21, which forwards to the private overload
     beginning at line 40 of the same file.
  3. That private overload catches `TaskCanceledException` itself (line 65). Inside that handler it
     calls `token.ThrowIfCancellationRequested()` (line 67); when `attempt < maxAttempts` it recurses,
     and otherwise it logs at line 82 and returns. It never rethrows `TaskCanceledException`.
  4. Its second handler, `catch (System.Exception e)` (line 85), rethrows only when `strict` is true.
     `GetTableInViewAsync` passes `strict: false`, so a `TimeoutException` raised inside the delegate
     is absorbed there and never leaves `RunWithTimeout`.
  5. The only exception that escapes is the `OperationCanceledException` produced by
     `token.ThrowIfCancellationRequested()`. `TaskCanceledException` derives from
     `OperationCanceledException`, not the reverse, so `catch (TaskCanceledException)` in
     `GetTableInViewAsync` does not catch it and it propagates past both handlers. The live test
     `GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException`
     (`UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` line 1324) pins exactly that
     escape with `ThrowAsync<OperationCanceledException>()`.

  Consequence: both catch bodies in `GetTableInViewAsync`, and therefore both lines this feature
  changes, are unreachable through the seam. This derivation is re-run at execution time in P1-T1
  because feature 825 owns `TimeOutTask.cs` and may change what escapes.

- **D3. AC7 and AC15 are resolved by a two-branch regression task, not by dropping or weakening AC7.**
  Spec AC7 demands a test that forces entry into `catch (TimeoutException)` through the
  `timeoutSourceFactory` seam. Against the tree as it stands that is unsatisfiable for the reason in
  D2. P2-T2 therefore carries two explicitly enumerated, mutually exclusive branches whose selection
  is decided by the `BRANCH:` value P1-T1 records, not by executor preference. Both branches are
  authorized by the task text, so neither is an unauthorized skip:
  - **Branch REACHABLE** - the P1-T1 measurement finds a path by which `TaskCanceledException` or
    `TimeoutException` escapes `RunWithTimeout`. The test is authored exactly as AC7 describes: it
    forces entry into `catch (TimeoutException)` through the seam, asserts the bounded retry occurred
    (`GetTable` call count 2), and contains no `Thread.Sleep`, no `Task.Delay`, no wall-clock wait and
    no temporary file.
  - **Branch UNREACHABLE** - the measurement confirms D2. The test instead pins the reachable
    observable contract of `GetTableInViewAsync` that the substitution must not disturb: with a
    factory returning a freshly constructed, already-cancelled `CancellationTokenSource` on every
    call and a non-cancelled outer token, the method returns `null` without invoking `GetTable` and
    without throwing. A fail-before exception dossier is written at
    `<FEATURE>/evidence/regression-testing/fail-before-exception.<timestamp>.md` carrying
    `WhyFailingRunImpossible:` plus the absence-of-reachability proof, re-derived against the
    post-825 tree. Under this branch the item-2 change is a statement-for-statement substitution of
    one uncovered line for one uncovered line, so it reduces coverage on no changed line and AC15's
    no-regression obligation for the changed lines is met by that identity. The numeric repository
    coverage figure is still captured from the step-4 run in P7-T4 and compared in P7-T6.

  Under either branch no acceptance condition requires observing the two diagnostics' emitted output.
  AC5 (zero `Console.WriteLine` in the file) and AC6 (two `logger.Warn` calls, one per catch, and no
  other change in that file's anchored diff) are static and are the primary pins for item 2.

- **D4. The factory must return a new source per call.** `TimeOutTask.cs` line 52 holds the timeout
  source in a `using var`, so it is disposed at the end of each attempt. A factory that returns the
  same instance on the retry attempt would hit `ObjectDisposedException` when the recursion reads
  `.Token`. Both branches of P2-T2 use a factory that constructs a new `CancellationTokenSource` on
  every invocation.

- **D5. RS0030 severity is not promoted, and the mechanism is stated rather than merely followed.**
  Running `/epic-run review-residuals-2026-09-08` authorizes edits to `.editorconfig` and to the root
  `BannedSymbols.txt` (epic.md lines 202 to 210). It does not authorize breaking the build to obtain
  them. `BannedSymbols.txt` is supplied to the compiler through `<AdditionalFiles>` by 16 of the 18
  projects in the solution, so a severity change is solution-wide. At `suggestion` the diagnostic is
  info-level and `TreatWarningsAsErrors` has nothing to promote. At `warning` every RS0030 report
  becomes a compiler warning, and toolchain step 3
  (`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`),
  mirrored by `.github/workflows/_build-nullable.yml`, would promote all of them to build errors on
  roughly 143 to 152 pre-existing usages. The constraint is the nullable gate, not the analyzer gate:
  `.github/workflows/_build-analyzers.yml` passes no `TreatWarningsAsErrors` and would not break.
  This feature therefore delivers the reachable subset - the eight added DocID lines (AC9) and the
  amended tracking comment (AC13), with the two documented exclusions held (AC12) - and leaves
  `dotnet_diagnostic.RS0030.severity = suggestion` at `.editorconfig` line 548 unchanged (AC11).

- **D6. Nothing is weakened to make a gate pass.** No coverage threshold, analyzer severity or policy
  requirement is lowered, weakened or deleted. No production file is added to any coverage exclusion
  list. No `[ExcludeFromCodeCoverage]` attribute is added. No `WarningsNotAsErrors`, no `NoWarn`, no
  path-scoped `.editorconfig` section, no second banned-symbols file and no
  `#pragma warning disable RS0030` sweep: the spec evaluated and rejected all of them. The only
  severity mutation this plan permits anywhere is the temporary, measured, reverted raise described
  in D7 channel 3, and P4-T3 proves the revert.

- **D7. AC10 needs a certified observation channel, in this order, each paired with a control.**
  Two prior measurements in this repository establish that a `suggestion`-severity (Roslyn info)
  diagnostic does not reach the msbuild console log at default verbosity or at `-v:m`, and that
  BannedApiAnalyzers silently ignores an unresolvable DocID. A clean build and an absent RS0030 are
  therefore each equally consistent with the ban working and with it being inert.
  1. **SARIF error log** - `/p:ErrorLog=<path>.sarif`. Roslyn's error log records info-severity
     diagnostics regardless of console verbosity. Hazard: a command-line `/p:ErrorLog=` is a global
     property and is not re-expanded per project, so a `/m` solution build has projects overwrite one
     another's log. Each relevant project is therefore built separately, with its own SARIF path and
     with `/p:BuildProjectReferences=false` after a warm solution build, so exactly one project
     compiles per invocation.
  2. **Detailed-verbosity file logger** - `/fl "/flp:LogFile=...;Verbosity=detailed"`, searched for
     the rule ID.
  3. **Last resort** - temporarily set the RS0030 severity to `warning`, run the analyzer gate only
     (which passes no `TreatWarningsAsErrors`, so warnings do not fail it), capture the diagnostics,
     then revert and prove the revert with a `git diff` anchored to `$Base`. Raising a severity to
     measure it and restoring it is not weakening a gate; leaving it raised is. AC11 still requires
     the committed `.editorconfig` to carry `dotnet_diagnostic.RS0030.severity = suggestion`
     unchanged, and P4-T3 is the unconditional gate that proves it.

  **Control (mandatory, same run).** A channel that reports zero for a symbol with known live usages
  has not been shown to carry info diagnostics at all, so a zero for the symbol under test would be
  uninformative and the observation void. The control sites are named rather than left to the
  executor to choose, and each was re-derived against the tree on 2026-09-09, in revision round 4, to
  be live code rather than a comment:
  - `QuickFiler.Test` project: `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs` line 25,
    `private DateTime now = DateTime.Now;` (`P:System.DateTime.Now`).
  - `QuickFiler` project: `QuickFiler/Controllers/EfcHomeControllerDependencies.cs` line 77,
    `MetricsNowFactory = metricsNowFactory ?? (() => DateTime.Now);` (`P:System.DateTime.Now`),
    registered at `QuickFiler.csproj` line 302. This is the only live, compiled `DateTime.Now` read
    in the `QuickFiler` project.
  - `UtilitiesCS` project: `UtilitiesCS/Threading/ApplicationIdleTimer.cs` lines 60, 140 and 236,
    three live `DateTime.Now` reads (`P:System.DateTime.Now`).

  Two traps are why these are named. First, the commented-out-code trap: all five `DateTime.Now`
  occurrences in `QuickFiler/Controllers/QfcHomeController.cs` are inside `//logger.Debug(...)`
  comments and produce no diagnostic, so that file is not a usable control. Second, the
  uncompiled-file trap: `QuickFiler/Legacy/QuickFileController.cs` carries three live `DateTime.Now`
  reads at lines 1010, 1013 and 1021 in its source text, but is named by no `<Compile Include>` item
  in `QuickFiler.csproj` - a legacy non-SDK project with explicit compile items and no wildcard
  include - nor in any other project file in the repository, so the compiler never sees it and it can
  produce no diagnostic either. Each of the three control sites named above was re-derived against the
  tree on 2026-09-09, in revision round 4, and is both live code and named by a `<Compile Include>`
  item in its own project:
  `Threading\ApplicationIdleTimer.cs` at `UtilitiesCS.csproj` line 1098,
  `Controllers\EfcHomeControllerDependencies.cs` at `QuickFiler.csproj` line 302, and
  `Helper Classes\MailItemInfoTests.cs` at `QuickFiler.Test.csproj` line 221.

- **D8. The AC10 sites under test are addressed by file plus a run-time line derivation, never by a
  literal line number.** `UtilitiesCS/Threading/TimeOutTask.cs` is sibling-owned by feature 825 and
  its line numbers will move. The five files under test, and the token whose line numbers are
  re-derived at observation time with `Select-String`, are:
  - `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs`, token `CancelAfter(`
    (one textual hit repository-wide during plan authoring);
  - `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs`, token `WaitOne(0)`;
  - `UtilitiesCS/Threading/TimeOutTask.cs`, token `new CancellationTokenSource(` with a non-empty
    argument (ten hits during plan authoring);
  - `QuickFiler/Controllers/QfcQueue.cs`, same token (two hits);
  - `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`, same token (one hit).

  The predicted total of 15 new diagnostics is a prediction. Spec AC10's pass condition is per site,
  not per file: every enumerated site must be observed, so a run reporting one of the ten
  `TimeOutTask.cs` sites would satisfy a per-file gate and still fail AC10. The pass condition is
  therefore that, for each of the five files, the re-derived line set minus the observed line set is
  empty, with the control firing in the same run. The observed total is recorded, not asserted. The
  site set re-derived against the tree on 2026-09-09, in revision round 4, is `TimeOutTask.cs` 53, 119,
  200, 274, 358, 436, 506, 588, 670 and 752; `QfcQueue.cs` 50 and 101; `ConversationHelper.cs` 295;
  `QfcQueueCoverageExpansionTests.cs` 169; and `BreadcrumbCoordinatorLifecycleTests.cs` 57 - 15 sites,
  so the completeness form is satisfiable. Those line numbers are recorded here as a dated
  satisfiability check only; P4-T2 re-derives them at observation time and asserts against the
  re-derived set, never against this list.

- **D9. The 33-file item-1 population is taken verbatim from the spec `### Write set` section.** It
  was re-derived during plan authoring: a repository-wide search of tracked `*.cs` for
  `Console.SetOut(` returns 38 occurrences across 35 files, matching spec correction 1 exactly. Two
  files are excluded: `TaskMaster/ThisAddIn.cs` line 103 (production, out of scope) and
  `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs` line 31
  (a commented-out call that installs nothing). The remainder is 33 files carrying 34 live install
  statements; `UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs`
  carries two, at lines 61 and 476, one per `[TestClass]`.

- **D10. Naming trap.** `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` is
  sibling-owned and is not in the population. Four other files whose names begin `QfcHomeController`
  are, as are two `QfcFormController` files. Every task in Phase 5 matches on the full filename.

- **D11. `ToDoModel.Test/Data Model/` contains a space.** Every tooling invocation and project
  reference that names a path under it is quoted.

- **D12. The two `TreeNode` files are the highest-likelihood failure mode (spec Risk 1, AC3).**
  Verified during plan authoring: in `ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs` the whole-word
  token `tw` occurs at line 16 (field declaration `private DebugTextWriter tw;`), lines 21 and 22
  (inside the commented-out `[ClassInitialize]` block), line 29 (assignment) and line 30
  (`Console.SetOut(tw);`). In `ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs` they
  are at lines 14, 19, 20, 27 and 28. Deleting only the call leaves CS0414; deleting the call and the
  assignment but keeping the field leaves CS0169. Both are compiler warnings, so the `.editorconfig`
  `suggestion` ceiling does not apply and `/p:TreatWarningsAsErrors=true` promotes them to build
  errors. The field, its assignment, the call and the orphaned commented-out block are deleted
  together. Both files retain their `[TestInitialize]` method, because it also constructs
  `this.mockRepository = new MockRepository(MockBehavior.Strict);` - these two files are in the
  delete-one-line group, not in the AC4 delete-the-method group.

- **D13. The AC4 partition was re-derived.** The ten AC4 files carry these `TestInitialize` token
  counts today, and every one drops to zero when its initializer is deleted, so the AC4 gate is both
  satisfiable and capable of failing: `VBFunctions.Test/ComputerInfo_Test.cs` 1,
  `UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs` 2, `UtilitiesCS.Test/Extensions/Frexp_Test.cs`
  2, `UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs` 2,
  `UtilitiesCS.Test/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew_Tests.cs` 2,
  `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs` 2,
  `UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs` 4 (two classes),
  `UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs` 2,
  `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs` 2,
  `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs` 2. In the last three
  the initializer body is the install plus one commented-out line; that orphaned comment is deleted
  with the method. The remaining 23 files are single-line deletions, and Risk 5 (deleting one line too
  many from an initializer that also constructs mocks) is discharged by requiring each task to read
  the initializer body before deleting anything.

- **D14. The project-file registration is exactly one added line.** `UtilitiesCS.Test.csproj` is a
  legacy non-SDK project with explicit `<Compile Include>` items; the `OutlookObjects\Table\` family
  sits at lines 402 to 405 and 543 to 545 in the tree observed during plan authoring. An unregistered
  test file still builds and is silently absent from the assembly. AC8 requires exactly one added
  line, zero removed and no reordering or reformatting, so the new entry is inserted adjacent to the
  existing `OutlookObjects\Table\` entries and nothing else in the file is touched.

- **D15. Coverage measurement and the `lines-valid` comparability rule.** The repository-wide
  Cobertura root `line-rate` is not reproducible run-to-run on this pipeline because the merged
  denominator moves. P7-T6 therefore reports under a two-branch comparability rule: if baseline and
  final `lines-valid` differ by 5 percent or less, the root `line-rate` comparison is the reported
  figure; otherwise the run is recorded `NOT COMPARABLE` and the blocking comparison falls back to
  the per-file figure for `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` and to
  the per-changed-line hit counts. Item 1 is expected to move no coverage figure at all: all 33 files
  compile into `*.Test.dll` assemblies, which the coverage pipeline excludes from instrumentation, so
  neither numerator nor denominator moves.

- **D16. AC16 needs an allow-list, not a bare write-set equality.** An anchored
  `git diff --name-only` run near the end of execution necessarily also lists this feature's own
  `spec.md` (AC check-offs), this plan file (task check-offs) and the evidence artifacts the plan
  requires. AC16's real content is that no out-of-scope file is touched. P8-T1 therefore compares the
  anchored diff against an explicit allow-list of the 38 write-set paths plus `<FEATURE>/spec.md`,
  `<FEATURE>/plan.2026-09-08T23-52.md` and the prefix `<FEATURE>/evidence/`, and additionally asserts
  the absence of `<FEATURE>/issue.md`, of any path under `<FEATURE>/research/`, of `CLAUDE.md`, of any
  path under `.claude/rules/` or `.github/instructions/` or `docs/features/epics/`, and of every
  sibling-owned path the spec enumerates, notably `UtilitiesCS/Threading/TimeOutTask.cs` and
  `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`.

- **D17. `.claude/agent-memory` is tracked and may be written mid-run by the executor and by sibling
  agents.** Every `git diff`, `git status` and name-listing gate in this plan is scoped with the
  pathspec exclusion written in plain prose as ":(exclude).claude". This is a rule, not a snapshot: it
  applies to every such gate in every phase, including any added during a revision round.

- **D18. Format-drift halt.** CI enforces the format check on `main`, so the tree is expected to be
  csharpier-clean at `$Base`. If P0-T6 records a non-clean baseline, the executor halts and returns to
  the orchestrator. Repairing pre-existing drift inside this feature would put files outside the write
  set into the anchored diff and make AC16 unsatisfiable, and scoping the final format pass to avoid
  that would leave `csharpier check .` failing, so neither response is available inside this feature.

---

## Plan-wide conventions

### C1. Shell discipline

Only `git`, `pwsh` and `poetry run` invocations run without a prompt in this environment, and every
`&&` or `|` segment of a command line is checked independently. Therefore:

- No `cd`, `grep`, `sed`, `cat`, `ls`, `cp`, `mv` or `mkdir` is used through the shell. File reading,
  writing and editing use the Read, Grep, Glob, Write and Edit tools.
- Every fenced `powershell` block below is executed as one `pwsh -NoProfile -Command` invocation whose
  payload is enclosed in outer single quotes. Consequently **no single-quote character appears inside
  any block**; every PowerShell string literal in this plan uses double quotes.
- **No `|` character appears in any block.** Pipelines are replaced by array subexpressions and
  indexing, for example `@(& $vswhere ...)[0]` and `@(Select-String ...).Count`.
- No `.ps1` helper script is created anywhere by this plan. The repository PowerShell batch budget is
  at its cap.
- Inside a `Select-String -Pattern` value, `\x22` denotes a literal double quote and `\x5C` a literal
  backslash. `-SimpleMatch` cannot use those escapes, so any token containing a quote is asserted with
  `-Pattern`.
- `Select-String` is case-insensitive by default. `-CaseSensitive` is supplied on every count gate in
  this plan and is load-bearing.
- Evidence artifacts are written with the Write tool from the transcribed console output. They are
  never produced by shell redirection.

### C2. Preamble (prepended verbatim to every fenced block)

```powershell
    $ErrorActionPreference = "Stop"
    $Root = (git rev-parse --show-toplevel)
    Set-Location -LiteralPath $Root
    $LASTEXITCODE = 0
    $Feature = "docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826"
    $Raw = "coverage/826-raw"
    $null = New-Item -ItemType Directory -Force -Path $Raw
    $BaseRef = "$Feature/evidence/baseline/base-ref.md"
    $Base = ""
    if (Test-Path -LiteralPath $BaseRef) { $Base = ([regex]::Match((Get-Content -LiteralPath $BaseRef -Raw), "BaseCommit: ([0-9a-f]{40})")).Groups[1].Value }
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $msbuild = @(& $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe")[0]
    $vstest = @(& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe")[0]
    $dotnet = Join-Path $Root ".dotnet-sdk\dotnet.exe"
    $Tac = "UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs"
    $Assemblies = @(
      "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll", "SVGControl.Test\bin\Debug\SVGControl.Test.dll",
      "Tags.Test\bin\Debug\Tags.Test.dll", "TaskMaster.Test\bin\Debug\TaskMaster.Test.dll",
      "TaskTree.Test\bin\Debug\TaskTree.Test.dll", "TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll",
      "ToDoModel.Test\bin\Debug\ToDoModel.Test.dll", "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll",
      "VBFunctions.Test\bin\Debug\VBFunctions.Test.dll")
    $FullFilter = "TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
```

Every line inside a fenced block is indented four spaces so no line begins at column 0 with a `#`
character. `$Base` is re-derived from the P0-T2 artifact in every block, because no shell variable
survives between tasks. The `Test-Path` guard is load-bearing: `$ErrorActionPreference = "Stop"`
makes `Get-Content -LiteralPath` on a missing file a terminating error, so an unguarded read would
kill the preamble of the very task that creates `base-ref.md`. `$Base` is empty only in P0-T2, which
creates `base-ref.md`, and in P0-T3, which does not reference it. Any task that references `$Base`,
whether in a fenced block or in its acceptance text, halts and returns to the orchestrator if `$Base`
is empty. P3-T1 is covered by this rule and is named because it is the only task whose `$Base`
reference sits outside a fenced block: an empty `$Base` would degrade its anchored
`git diff --numstat` to an unanchored worktree-versus-index comparison, which reads 8 added and 0
removed for a just-edited unstaged file and would therefore pass without the base anchor having been
applied.

### C3. Base anchor

`$Base` is the commit that is `HEAD` immediately before the first edit of this feature, recorded by
P0-T2. Because the item branch is cut from the epic integration branch and no other agent commits to
it, `$Base` is the merge base for AC16's purposes. No SHA is pinned in this document. If the upstream
branch advances mid-execution, the executor stops at the next phase boundary and returns to the
orchestrator for a fresh reconciliation rather than re-anchoring on its own.

### C4. Toolchain (this exact order; restart from step 1 on any failure or auto-fix)

1. `dotnet tool run csharpier format .`, verified with `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <the nine test assemblies> /EnableCodeCoverage`

Facts that make one of these gates vacuous or unsatisfiable if ignored:

- `/t:Rebuild` is mandatory on both msbuild gates. MSBuild's up-to-date check does not invalidate on a
  command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
  project and runs no analyzers.
- `/p:Nullable=enable` is never added to step 3. It is not in the CI command, no project carries a
  `<Nullable>` element, and adding it conscripts every file that never adopted the pragma.
- Both msbuild gates write a detailed-verbosity file log under the gitignored `coverage/826-raw/`
  directory. AC14 is asserted from that log as a pair: the count of the regex
  `Skipping target \x22CoreCompile\x22` is 0 **and** the count of the regex `Task \x22Csc\x22` is at
  least 1. The second half is what makes the first non-vacuous; a count of zero for both would be
  equally consistent with a log that recorded nothing. The pair rule is not confined to AC14: every
  task in this plan that asserts a zero count read from an msbuild log - `Skipping target
  "CoreCompile"` in P0-T7, P0-T8, P5-T11, P7-T2 and P7-T3, and the CS0169 and CS0414 counts in P0-T8,
  P5-T11 and P7-T3 - asserts `Task "Csc"` at least 1 in the same task and from the same log, because
  a log recording no compilation at all would otherwise satisfy each of those zeros.
- A successful msbuild prints the substring `error` inside switch names and summary text, so build
  gates assert the exit code plus the verbatim summary token `` ` 0 Error(s)` `` read from the log,
  never the absence of the substring `error`. The leading space in that token is load-bearing:
  `0 Error(s)` without it is a substring of `10 Error(s)`, so a build reporting 10 errors would
  satisfy a "count at least 1" gate. MSBuild prints the summary line with leading whitespace, which
  is what makes the leading-space form discriminating.
- `csharpier format .` rewrites files and still exits 0, and its `Formatted N files` line is a
  processed-file count, not a rewrite count. The Phase 7 format task therefore records SHA-256 hashes
  of every tracked `.cs` file plus the untracked file P2-T2 creates, before and after, defines
  `$rewritten` as the number of hash differences, and is paired with `csharpier check .`, whose clean
  run exits 0 and prints a line beginning with the literal token `Checked `.
- Neither `BannedSymbols.txt` nor `.editorconfig` is reformatted by step 1. CSharpier 1.2.6 processes
  `*.cs`, `*.xml` and `packages.config` only, and neither file carries one of those shapes. Neither
  is named in `.csharpierignore`, which lists `**/evidence/**`, `*.cobertura.xml`, `*.coverage`,
  `*.coveragexml`, `*.trx`, `*.csproj`, `*.props` and `*.targets`; the exemption rests on the
  file-type restriction, not on that ignore file.
- A bare `/Logger:trx` names the output file after the account and the machine. Every vstest
  invocation passes `"/Logger:trx;LogFileName=<task>.trx"` (double-quoted, because an unquoted
  semicolon terminates the argument) and `/ResultsDirectory:coverage/826-raw/<task>`.
- Raw msbuild logs, SARIF documents, TRX files, `.coverage` files and raw Cobertura documents are
  never committed. They live under `coverage/826-raw/`, which `.gitignore` line 144 (`coverage/*`)
  excludes. They embed absolute host paths, the account name in `runUser=` and the machine name in
  `computerName=`. Committed evidence artifacts carry only sanitized extracts: counts, rule IDs,
  repository-relative paths and line numbers, with the repository root written as `<repo-root>`.
- Every task that creates or edits a `.cs` file runs `& $dotnet tool run csharpier format` over
  exactly the paths it touched, before that task's own acceptance gates, so the Phase 7 step-1 pass
  finds the tree already clean and no restart is triggered. Paths under `ToDoModel.Test/Data Model/`
  are double-quoted. The substituted `logger.Warn` statements are 93 and 89 columns wide - a
  73-character statement at 20-space and 16-space indentation respectively, measured against the
  current tree - and CSharpier's width is its 100-column default because the repository carries no
  `.csharpierrc`, so formatting does not wrap them and P2-T1's 2-added, 2-removed numstat is measured
  post-format.

### C5. Long-run fallback

If a single nine-assembly vstest invocation exceeds one tool invocation window, the run is split per
assembly into separate invocations writing separate Cobertura documents under
`coverage/826-raw/<task>/`, and the documents are merged with `dotnet-coverage merge` before the
figures are read. This branch is authorized by this plan; the evidence artifact records which form was
used and the per-assembly result counts either way.

---

### Phase 0 — Policy reads, toolchain bootstrap, base anchor, baselines

- [x] [P0-T1] Read, in this order, `CLAUDE.md`, `.claude/rules/general-code-change.md`,
  `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`,
  `.claude/rules/csharp.md`, then this feature's `spec.md` in full and its research record
  `docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/research/console-out-and-banned-symbol-residuals.2026-09-08T23-58.md`,
  then `docs/features/epics/review-residuals-2026-09-08/epic.md` (read only; this plan performs no
  edit to it). Write
  `<FEATURE>/evidence/baseline/phase0-instructions-read.md` with `Timestamp:`, `Policy Order:` and the
  explicit list of files read.
  **Acceptance:** the artifact exists and lists all nine paths above under `Policy Order:`.

- [x] [P0-T2] Record the base anchor before any edit. Run the block below and write
  `<FEATURE>/evidence/baseline/base-ref.md` containing the literal line `BaseCommit: <40-hex sha>`, the
  branch name, and the porcelain output.
  ```powershell
      git rev-parse HEAD
      git rev-parse --abbrev-ref HEAD
      git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826"
      git status --porcelain --untracked-files=all -- "docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826"
  ```
  The single-span form is not usable here. By the time this task runs, P0-T1 has written the untracked
  artifact `<FEATURE>/evidence/baseline/phase0-instructions-read.md` and has checked itself off in this
  tracked plan file, and neither is covered by `:(exclude).claude`, so a single scoped span can never
  be empty on any run.
  **Acceptance:** the artifact contains one line matching `BaseCommit: ` followed by 40 hexadecimal
  characters; the first porcelain span (which excludes `.claude` and this feature's folder) is empty,
  proving no file outside this feature is dirty at the base anchor; and every entry in the second span
  is either this plan file or a path under `<FEATURE>/evidence/`, which are the only paths Phase 0 has
  written so far.

- [x] [P0-T3] Bootstrap the C# toolchain in this worktree. `.dotnet-sdk` does not exist here, so
  `global.json` cannot be satisfied by a globally installed SDK and every `dotnet` invocation would
  fail until this runs.
  ```powershell
      pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1
      & (Join-Path $Root ".dotnet-sdk\dotnet.exe") tool restore
      & (Join-Path $Root ".dotnet-sdk\dotnet.exe") tool run csharpier --version
      pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1
      dotnet-coverage --version
  ```
  Write `<FEATURE>/evidence/baseline/toolchain-bootstrap.md` with `Timestamp:`, `Command:`,
  `EXIT_CODE:` and `Output Summary:` for each of the five commands.
  **Acceptance:** all five commands exit 0 and the csharpier version reported is `1.2.6`.

- [x] [P0-T4] Upstream 825 precondition halt-gate. Confirm the two statements this feature owns
  survived feature 825 byte-identical and that the seam parameter still exists.
  ```powershell
      @(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "Console.WriteLine").Count
      @(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "Task timed out on try").Count
      @(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "timeoutSourceFactory").Count
      @(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "catch (TaskCanceledException)").Count
      @(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "catch (TimeoutException)").Count
  ```
  Write `<FEATURE>/evidence/baseline/upstream-825-precondition.md` with the five counts and the line
  numbers reported for the first two tokens.
  **Acceptance:** the first count is 2, the second is 2, the third is at least 1, and the fourth and
  fifth are each 1. If any of these does not hold, the executor stops, records `HALT: 825 RECONCILE`
  in the artifact, and returns to the orchestrator without editing the file; the spec forbids
  reconstructing a missing statement.

- [x] [P0-T5] Capture the pre-change census that makes the Phase 5 and Phase 3 gates capable of
  failing.
  ```powershell
      $cs = @(git ls-files "*.cs")
      $so = @(Select-String -LiteralPath $cs -CaseSensitive -SimpleMatch "Console.SetOut(")
      $so.Count
      @([System.Collections.Generic.HashSet[string]]::new([string[]]$so.Path)).Count
  ```
  The distinct-file figure uses a `HashSet` rather than `Sort-Object -Unique` because C1 forbids the
  `|` character in every block. Record also, per
  file, the `DebugTextWriter` count for each of the 33 write-set files, the `TestInitialize` count for
  each of the 33 write-set files, the total line count of `BannedSymbols.txt`, and the `.editorconfig`
  counts of the `-SimpleMatch` tokens `#181` and `dotnet_diagnostic.RS0030.severity = suggestion`.
  Write `<FEATURE>/evidence/baseline/pre-change-census.md`.
  **Acceptance:** the artifact records `Console.SetOut(` occurrences 38 across 35 files;
  `BannedSymbols.txt` line count 7; `.editorconfig` `#181` count 3 and
  `dotnet_diagnostic.RS0030.severity = suggestion` count 1; a non-zero `DebugTextWriter` count for
  every one of the 33 files; and a non-zero `TestInitialize` count for every one of the ten AC4 files,
  and a recorded `TestInitialize` count for each of the remaining 23 write-set files. The census spans
  all 33 files rather than only the ten AC4 files because P5-T1 and P5-T6 both gate on a
  `TestInitialize` count being unchanged from this census, and their eight files are not among the
  ten. Any deviation from the first three figures is recorded verbatim and reported to the
  orchestrator before Phase 5 begins, because the Phase 5 gates are stated against them.

- [x] [P0-T6] Baseline format state (toolchain step 1, read-only form).
  ```powershell
      & $dotnet tool run csharpier check .
      $LASTEXITCODE
  ```
  Write `<FEATURE>/evidence/baseline/baseline-format.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`
  and `Output Summary:` including the literal `Checked ` summary line.
  **Acceptance:** `EXIT_CODE: 0` and the output contains a line beginning with the token `Checked `.
  If the exit code is non-zero, the executor records the unformatted file list, writes
  `HALT: PRE-EXISTING FORMAT DRIFT` in the artifact and returns to the orchestrator, per D18.

- [x] [P0-T7] Baseline analyzer build (toolchain step 2).
  ```powershell
      & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:LogFile=coverage/826-raw/p0-t7-analyzers.log;Verbosity=detailed"
      $LASTEXITCODE
      @(Select-String -LiteralPath "coverage/826-raw/p0-t7-analyzers.log" -CaseSensitive -SimpleMatch " 0 Error(s)").Count
      @(Select-String -LiteralPath "coverage/826-raw/p0-t7-analyzers.log" -CaseSensitive -Pattern "Skipping target \x22CoreCompile\x22").Count
      @(Select-String -LiteralPath "coverage/826-raw/p0-t7-analyzers.log" -CaseSensitive -Pattern "Task \x22Csc\x22").Count
  ```
  Write `<FEATURE>/evidence/baseline/baseline-analyzers.md` with all four figures.
  **Acceptance:** `EXIT_CODE: 0`, ` 0 Error(s)` count at least 1, `Skipping target "CoreCompile"` count
  0, and `Task "Csc"` count at least 1.

- [x] [P0-T8] Baseline nullable build (toolchain step 3).
  ```powershell
      & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl "/flp:LogFile=coverage/826-raw/p0-t8-nullable.log;Verbosity=detailed"
      $LASTEXITCODE
      @(Select-String -LiteralPath "coverage/826-raw/p0-t8-nullable.log" -CaseSensitive -SimpleMatch " 0 Error(s)").Count
      @(Select-String -LiteralPath "coverage/826-raw/p0-t8-nullable.log" -CaseSensitive -SimpleMatch "CS0169").Count
      @(Select-String -LiteralPath "coverage/826-raw/p0-t8-nullable.log" -CaseSensitive -SimpleMatch "CS0414").Count
      @(Select-String -LiteralPath "coverage/826-raw/p0-t8-nullable.log" -CaseSensitive -Pattern "Skipping target \x22CoreCompile\x22").Count
      @(Select-String -LiteralPath "coverage/826-raw/p0-t8-nullable.log" -CaseSensitive -Pattern "Task \x22Csc\x22").Count
  ```
  Write `<FEATURE>/evidence/baseline/baseline-nullable.md` with all six figures.
  **Acceptance:** `EXIT_CODE: 0`, ` 0 Error(s)` count at least 1, CS0169 count 0, CS0414 count 0,
  `Skipping target "CoreCompile"` count 0, and `Task "Csc"` count at least 1, which is what makes the
  CS0169, CS0414 and Skipping-target zeros non-vacuous.

- [x] [P0-T9] Baseline measured test and coverage run (toolchain step 4, measured form). This is the
  run whose numbers AC15 compares against; P7-T5 is the confirming CI-verbatim run.
  ```powershell
      dotnet-coverage collect --output coverage/826-raw/p0-t9.cobertura.xml --output-format cobertura -- $vstest $Assemblies /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/Logger:trx;LogFileName=p0-t9.trx" /ResultsDirectory:coverage/826-raw/p0-t9 /TestCaseFilter:$FullFilter
      $LASTEXITCODE
      $x = [xml](Get-Content -LiteralPath "coverage/826-raw/p0-t9.cobertura.xml" -Raw)
      $x.coverage.GetAttribute("line-rate")
      $x.coverage.GetAttribute("lines-valid")
      $x.coverage.GetAttribute("lines-covered")
  ```
  Then, in the same task, derive the two changed-line hit counts without hard-coding a line number:
  read `$Tac` with `Select-String -CaseSensitive -SimpleMatch "Task timed out on try"` to obtain the
  two current line numbers, and for each look up the `hits` attribute of the `<line number="N">`
  element under every `<class>` whose `filename` attribute ends with
  `OlTableExtensions.TableAccess.cs`, aggregating across classes because an async method is split
  across state-machine classes. Write
  `<FEATURE>/evidence/baseline/baseline-tests-coverage.md` with `Timestamp:`, `Command:`,
  `EXIT_CODE:` and an `Output Summary:` carrying: total, passed, failed and notExecuted counts read
  from the TRX `ResultSummary/Counters` attributes of those names; `notExecuted` is the TRX spelling
  of the skipped count; the numeric root `line-rate`, `lines-valid` and `lines-covered`;
  the per-file line rate for `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`; and
  the two changed-line hit counts labelled `BaselineChangedLineHits:`.
  If no `<line number="N">` element exists for a changed line under any `<class>` whose `filename`
  ends with `OlTableExtensions.TableAccess.cs`, record that line's hit count as `0` and annotate it
  `no line element emitted`. Both runs apply this rule identically, so P7-T6 compares like for like.
  **Acceptance:** `EXIT_CODE: 0`, failed count 0, and the artifact carries numeric (not placeholder)
  values for `line-rate`, `lines-valid`, `lines-covered` and `BaselineChangedLineHits:`.

---

### Phase 1 — Item-2 reachability re-measurement against the post-825 tree

- [x] [P1-T1] Re-run the D2 derivation against the tree as it stands now, because feature 825 owns
  `UtilitiesCS/Threading/TimeOutTask.cs` and may have changed what escapes it. Perform these five
  mechanical steps with the Read and Grep tools and record each with its current line citation:
  1. In `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`, locate the
     `TimeOutTask.RunWithTimeout(` call inside `GetTableInViewAsync` and record its full argument
     list, in particular the value of the `strict` argument.
  2. In `UtilitiesCS/Threading/TimeOutTask.cs`, identify the public overload whose first parameter is
     `this Func<TResult>` and record the private overload it forwards to.
  3. In that private overload, enumerate every `throw` and every rethrow, and record for each whether
     it is guarded by `strict`.
  4. Determine whether `TaskCanceledException` or `TimeoutException` can leave that overload when
     `strict` is `false`, and record which exception type does leave it.
  5. Confirm against `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` that the test
     asserting the escape of the cancellation exception still exists, and record its method name and
     current line number.

  Write `<FEATURE>/evidence/other/item2-branch-reachability.<timestamp>.md` containing the five steps
  with citations, and exactly one line that is either `BRANCH: REACHABLE` or `BRANCH: UNREACHABLE`.
  **Acceptance:** the artifact exists, carries all five numbered steps each with a file path and a
  current line number, and contains exactly one line matching `BRANCH: REACHABLE` or exactly one line
  matching `BRANCH: UNREACHABLE` and not both.

- [x] [P1-T2] Write the fail-before exception dossier at
  `<FEATURE>/evidence/regression-testing/fail-before-exception.<timestamp>.md`. A failing run is
  structurally impossible under both branches of P2-T2: AC7 itself requires that reverting the item-2
  production edit alone must not be what makes the test pass or fail, so the test pins a branch rather
  than the substitution and cannot be red before the fix. The dossier carries `Timestamp:`,
  `WhyFailingRunImpossible:` in one to three sentences, the `BRANCH:` value copied from P1-T1, and an
  alternative proof section. Under `BRANCH: UNREACHABLE` the alternative proof is the
  absence-of-reachability derivation re-stated from P1-T1 with its post-825 line citations. Under
  `BRANCH: REACHABLE` the alternative proof is the pre-change count observed by P0-T4 -
  `Console.WriteLine` in the table-access file is 2 - together with a statement naming P6-T1 as the
  task that asserts the post-change count of 0. The dossier records the observed pre-change figure
  only; the post-change figure is not observable when this task runs and must not be written here in
  advance.
  The dossier also records `SearchScope:` `<FEATURE>/evidence/regression-testing/`,
  `SearchPatterns:` `fail-before-exception.*.md`, and `SearchResult:` naming this file.
  **Acceptance:** the dossier exists at the stated path, its filename begins with
  `fail-before-exception.`, and it contains `WhyFailingRunImpossible:`, the same `BRANCH:` value as
  the P1-T1 artifact, and a non-empty alternative proof section.

---

### Phase 2 — Item 2: logger substitution, regression test, project registration

- [x] [P2-T1] In `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`, replace each of
  the two statements whose literal text is `Console.WriteLine($"Task timed out on try {counter}");`
  with `logger.Warn($"{nameof(GetTableInViewAsync)} timed out on try {counter}");`. Locate each by its
  literal text and its enclosing catch clause: one sits in the `else` branch of
  `catch (TaskCanceledException)`, the other in `catch (TimeoutException)`. Do not use a line number.
  No new field and no new `using` is required: `logger` is the `log4net.ILog` declared at
  `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs` line 25 in the sibling partial of the same
  `public static partial class OlTableExtensions`, and the same file already calls `logger.Warn` three
  times. Change nothing else: no catch clause is added, removed or widened; the deadline window, the
  retry counter, the `timeoutSourceFactory` seam, the caught exception types and the control flow
  after each diagnostic are all left exactly as found.
  ```powershell
      @(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "Console.WriteLine").Count
      @(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "Console.").Count
      @(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "timed out on try").Count
      git diff --numstat $Base -- $Tac
  ```
  Write `<FEATURE>/evidence/qa-gates/p2-t1-item2-substitution.md`.
  **Acceptance:** `Console.WriteLine` count 0; `Console.` count 1 (the `writer ?? Console.Out` seam in
  `EnumerateTable`, which is out of scope and must remain); `timed out on try` count 2; and the
  anchored numstat for that path reads exactly 2 added and 2 removed lines.

- [x] [P2-T2] Create `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs`
  as an MSTest class using Moq and FluentAssertions, following the mocking pattern established at
  `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` lines 1266 to 1321 (mock
  `Outlook.Table`, `Outlook.TableView` and `Outlook.Explorer`; set up `CurrentView` to return the
  table view; inject a `Func<int, CancellationTokenSource>` through the existing `timeoutSourceFactory`
  parameter). A new file is required because `OlTableExtensions_Tests.cs` is at its stated line
  ceiling. Per D4 the injected factory constructs a **new** `CancellationTokenSource` on every
  invocation. The test contains no `Thread.Sleep`, no `Task.Delay`, no wall-clock wait, no temporary
  file and no external service, and asserts nothing about `Console.Out`. Both branches below are
  authorized by this task text; select the one whose name equals the `BRANCH:` value recorded by
  P1-T1, and record the selected branch in the artifact.
  - **BRANCH: REACHABLE** - author
    `GetTableInViewAsync_TimeoutSourceForcesTimeout_RetriesOnceAndReturnsTable`, which forces entry
    into `catch (TimeoutException)` in `GetTableInViewAsync` through the `timeoutSourceFactory` seam
    and asserts the bounded retry occurred with a `GetTable` call count of 2.
  - **BRANCH: UNREACHABLE** - author
    `GetTableInViewAsync_PreCancelledTimeoutSource_ReturnsNullWithoutInvokingGetTable`, which supplies
    a factory returning a freshly constructed, already-cancelled `CancellationTokenSource` on every
    call together with a non-cancelled outer `CancellationToken`, and asserts that the method returns
    `null`, that the `GetTable` call count is 0, and that no exception is thrown. This pins the
    reachable observable contract of `GetTableInViewAsync` that the substitution must not disturb.

  Write `<FEATURE>/evidence/regression-testing/p2-t2-regression-test.md` recording the selected
  branch, the file path, the fully qualified test method name and the file's line count.
  **Acceptance:** the file exists, is under 500 lines, contains exactly one `[TestClass]` and exactly
  one `[TestMethod]` whose name is the one named by the selected branch, contains zero occurrences of
  each of the `-SimpleMatch` tokens `Thread.Sleep`, `Task.Delay` and `Console.`, and the artifact
  records a `BRANCH:` value identical to the one in the P1-T1 artifact.

- [x] [P2-T3] Register the new test file in the legacy non-SDK project by inserting exactly one line
  into `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, adjacent to the existing
  `OutlookObjects\Table\` compile items, with the same indentation and element shape as its
  neighbours. The literal this task creates is the file name token
  `OlTableExtensionsTimeoutDiagnosticsTests.cs` inside a `Compile Include` element. Remove nothing,
  reorder nothing and reformat nothing.
  ```powershell
      @(Select-String -LiteralPath "UtilitiesCS.Test/UtilitiesCS.Test.csproj" -CaseSensitive -SimpleMatch "OlTableExtensionsTimeoutDiagnosticsTests.cs").Count
      git diff --numstat $Base -- "UtilitiesCS.Test/UtilitiesCS.Test.csproj"
      git diff --numstat $Base -- "*.csproj"
  ```
  Write `<FEATURE>/evidence/qa-gates/p2-t3-csproj-registration.md`.
  **Acceptance:** the token count is 1; the anchored numstat for `UtilitiesCS.Test.csproj` reads
  exactly 1 added and 0 removed lines; and the anchored numstat over `*.csproj` lists exactly that one
  project file and no other.

- [x] [P2-T4] Build the `UtilitiesCS.Test` project and run the new test class in isolation, so a
  compile or registration failure surfaces here rather than in the final QA loop. This is an interim
  scoped check, not a toolchain-loop pass.
  ```powershell
      & $msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /fl "/flp:LogFile=coverage/826-raw/p2-t4-testproj.log;Verbosity=detailed"
      $LASTEXITCODE
      & $vstest "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation "/Logger:trx;LogFileName=p2-t4.trx" /ResultsDirectory:coverage/826-raw/p2-t4 /TestCaseFilter:FullyQualifiedName~OlTableExtensionsTimeoutDiagnosticsTests
      $LASTEXITCODE
  ```
  Write `<FEATURE>/evidence/regression-testing/p2-t4-scoped-run.md` with both exit codes and the TRX
  counters.
  **Acceptance:** both exit codes are 0; the TRX `ResultSummary/Counters` reports `executed` 1,
  `passed` 1 and `failed` 0; and the TRX contains a `UnitTest` whose `TestMethod/@name` equals the
  test method name recorded by P2-T2. A count of 0 executed proves the compile item did not take
  effect and fails this task.

---

### Phase 3 — Item 3: banned symbols and the tracking comment, at unchanged severity

- [x] [P3-T1] Append exactly eight lines to `BannedSymbols.txt`, each in the same
  DocID-semicolon-message shape as the seven pre-existing lines, with a message naming
  `TimeProvider`. The eight lines this task creates are, verbatim:

  The four-space indentation in the block below is this document's fenced-block convention and is
  **not** part of the file content. Each of the eight lines is appended with no leading whitespace; a
  DocID with leading whitespace does not resolve and BannedApiAnalyzers ignores it silently.
  ```text
      M:System.Threading.CancellationTokenSource.CancelAfter(System.Int32);Do not call CancelAfter. Inject a time abstraction (System.TimeProvider) and use FakeTimeProvider in tests.
      M:System.Threading.CancellationTokenSource.CancelAfter(System.TimeSpan);Do not call CancelAfter. Inject a time abstraction (System.TimeProvider) and use FakeTimeProvider in tests.
      M:System.Threading.CancellationTokenSource.#ctor(System.Int32);Do not construct a deadline CancellationTokenSource. Inject a time abstraction (System.TimeProvider) and use FakeTimeProvider in tests.
      M:System.Threading.CancellationTokenSource.#ctor(System.TimeSpan);Do not construct a deadline CancellationTokenSource. Inject a time abstraction (System.TimeProvider) and use FakeTimeProvider in tests.
      M:System.Threading.WaitHandle.WaitOne(System.Int32);Do not use a timed WaitOne. Inject a time abstraction (System.TimeProvider) and use FakeTimeProvider in tests.
      M:System.Threading.WaitHandle.WaitOne(System.TimeSpan);Do not use a timed WaitOne. Inject a time abstraction (System.TimeProvider) and use FakeTimeProvider in tests.
      M:System.Threading.WaitHandle.WaitOne(System.Int32,System.Boolean);Do not use a timed WaitOne. Inject a time abstraction (System.TimeProvider) and use FakeTimeProvider in tests.
      M:System.Threading.WaitHandle.WaitOne(System.TimeSpan,System.Boolean);Do not use a timed WaitOne. Inject a time abstraction (System.TimeProvider) and use FakeTimeProvider in tests.
  ```
  The parameterless `CancellationTokenSource.#ctor()` is not a candidate and must not be listed: 157
  parameterless constructions exist across 80 files and none carries a deadline. `TimeoutAfter` must
  not be listed: it is a repository-local extension method in `UtilitiesCS/Threading/TimeOutTask.cs`,
  two of whose overloads accept a `TimeProvider` and are the documented determinism seam, so banning
  it would ban the remedy every existing message points callers toward. The parameterless
  `WaitHandle.WaitOne()` overload must not be listed: it is a deterministic handshake on a signal
  rather than a wall-clock deadline, 12 of the 13 current call sites use it as the repository's own
  cross-thread determinism idiom, and adding it would contribute 12 more unfixable entries to the
  backlog that already blocks promotion. Do not modify the seven pre-existing lines.
  **Acceptance:** `(Get-Content -LiteralPath "BannedSymbols.txt").Count` is 15;
  and for each of these eight `-SimpleMatch` tokens,
  `@(Select-String -LiteralPath "BannedSymbols.txt" -CaseSensitive -SimpleMatch <token>).Count` is 1
  and the matched line also contains the token `TimeProvider`:
  `M:System.Threading.CancellationTokenSource.CancelAfter(System.Int32);`,
  `M:System.Threading.CancellationTokenSource.CancelAfter(System.TimeSpan);`,
  `M:System.Threading.CancellationTokenSource.#ctor(System.Int32);`,
  `M:System.Threading.CancellationTokenSource.#ctor(System.TimeSpan);`,
  `M:System.Threading.WaitHandle.WaitOne(System.Int32);`,
  `M:System.Threading.WaitHandle.WaitOne(System.TimeSpan);`,
  `M:System.Threading.WaitHandle.WaitOne(System.Int32,System.Boolean);`,
  `M:System.Threading.WaitHandle.WaitOne(System.TimeSpan,System.Boolean);`; the anchored
  `git diff --numstat $Base -- BannedSymbols.txt` reads exactly 8 added and 0 removed lines; the
  counts of the `-SimpleMatch` tokens `TimeoutAfter` and `WaitHandle.WaitOne;` in the file are each 0;
  and `@(Select-String -LiteralPath "BannedSymbols.txt" -CaseSensitive -Pattern "^\s").Count` is 0,
  proving no appended line carries leading whitespace.
  Evidence: `<FEATURE>/evidence/qa-gates/p3-t1-banned-symbols.md`.

- [x] [P3-T2] Re-measure the textual surface, then amend the `.editorconfig` tracking comment. First
  measure, so that no unverified figure is written into the repository:
  ```powershell
      $cs = @(git ls-files "*.cs")
      @(Select-String -LiteralPath $cs -CaseSensitive -SimpleMatch "DateTime.Now").Count
      @(Select-String -LiteralPath $cs -CaseSensitive -SimpleMatch "DateTime.UtcNow").Count
      @(Select-String -LiteralPath $cs -CaseSensitive -SimpleMatch "Random.Shared").Count
      @(Select-String -LiteralPath $cs -CaseSensitive -SimpleMatch "Thread.Sleep").Count
      @(Select-String -LiteralPath $cs -CaseSensitive -SimpleMatch "Task.Delay").Count
  ```
  Then replace the two comment lines that currently sit immediately above
  `dotnet_diagnostic.RS0030.severity = suggestion` in the `BannedApiAnalyzers` block with a comment
  that states the promotion precondition inline, records the re-measured surface, and no longer cites
  the closed tracking issue. The literals this task creates inside that comment block are the tokens
  `TreatWarningsAsErrors`, `_build-nullable.yml` and `2026-09-08`, and the five symbol names measured
  above. Write the measured figures, not the spec's recorded ones, and label them
  `Verified textual surface, 2026-09-08 (re-measured at implementation time)`. Do not write the
  characters `181` anywhere in the block, and do not write the token `dotnet_diagnostic.` anywhere in
  the block. Do not change the severity value on line 548 or on any other line.
  **Acceptance:** in the region of `.editorconfig` running from the line containing the
  `-SimpleMatch` token `BannedApiAnalyzers 3.3.4` through the line containing
  `dotnet_diagnostic.RS0030.severity`, the count of `-SimpleMatch` `181` is 0, and the counts of
  `-SimpleMatch` `TreatWarningsAsErrors`, `_build-nullable.yml` and `2026-09-08` are each at least 1;
  the whole-file count of `-SimpleMatch` `#181` is 2, down from the 3 recorded by P0-T5; and the
  whole-file count of `-SimpleMatch` `dotnet_diagnostic.RS0030.severity = suggestion` is 1.
  Evidence: `<FEATURE>/evidence/qa-gates/p3-t2-editorconfig-comment.md`, which also records the five
  re-measured counts.

- [x] [P3-T3] Prove the severity is untouched by the item-3 edit.
  ```powershell
      git diff $Base -- .editorconfig
  ```
  From that output, count the changed lines (those beginning with a single `+` or `-`, excluding the
  `+++` and `---` file headers) that contain the `-SimpleMatch` token `dotnet_diagnostic.`.
  Write `<FEATURE>/evidence/qa-gates/p3-t3-severity-unchanged.md` with the full anchored diff of
  `.editorconfig` and that count.
  **Acceptance:** the count is 0, and `.editorconfig` still contains exactly one line equal to
  `dotnet_diagnostic.RS0030.severity = suggestion`.

---

### Phase 4 — AC10: positive observation of RS0030 with a working control

- [x] [P4-T1] Certify an observation channel, in the D7 order, and stop at the first channel whose
  control fires. Run a warm solution build first so that `/p:BuildProjectReferences=false` is valid,
  then build each of the three relevant projects separately with its own SARIF path, because a
  command-line `/p:ErrorLog=` is a global property and a `/m` solution build would have projects
  overwrite one another's log.
  ```powershell
      & $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
      & $msbuild UtilitiesCS\UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/p:ErrorLog=$Root\coverage\826-raw\utilitiescs.sarif"
      & $msbuild QuickFiler\QuickFiler.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/p:ErrorLog=$Root\coverage\826-raw\quickfiler.sarif"
      & $msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/p:ErrorLog=$Root\coverage\826-raw\quickfilertest.sarif"
      @(Select-String -LiteralPath "coverage/826-raw/utilitiescs.sarif" -CaseSensitive -SimpleMatch "RS0030").Count
      @(Select-String -LiteralPath "coverage/826-raw/quickfiler.sarif" -CaseSensitive -SimpleMatch "RS0030").Count
      @(Select-String -LiteralPath "coverage/826-raw/quickfilertest.sarif" -CaseSensitive -SimpleMatch "RS0030").Count
      $ctl = @(
        @("coverage/826-raw/utilitiescs.sarif", "*ApplicationIdleTimer.cs"),
        @("coverage/826-raw/quickfiler.sarif", "*EfcHomeControllerDependencies.cs"),
        @("coverage/826-raw/quickfilertest.sarif", "*MailItemInfoTests.cs"))
      foreach ($c in $ctl) {
        $j = ConvertFrom-Json (Get-Content -LiteralPath $c[0] -Raw)
        $rs = @(@($j.runs[0].results).Where({ $_.ruleId -eq "RS0030" }))
        $c[0]
        $j.version
        $rs.Count
        $uris = @($rs.ForEach({ $loc = $_.locations[0]; if ($null -ne $loc.physicalLocation) { $loc.physicalLocation.artifactLocation.uri } else { $loc.resultFile.uri } }))
        $uris
        @($uris.Where({ $_ -like $c[1] })).Count
      }
  ```
  Verify additionally that each SARIF is scoped to its own project, by confirming that every RS0030
  result location in it lies under that project's directory; if a SARIF is found to have been
  overwritten by a referenced project, record it and re-run that project's build alone. A Roslyn error
  log can carry rule metadata for rules that produced no result, so the three `-SimpleMatch "RS0030"`
  counts above are recorded only and are not diagnostic counts. The location shape differs by SARIF
  version - `locations[0].resultFile.uri` in v1, `locations[0].physicalLocation.artifactLocation.uri`
  in v2 - so the block probes for both and additionally emits each document's `version` value, and
  the artifact records that version together with the location shape it implies. If the three
  control counts are not all at least 1, the channel has not been shown to carry info-severity
  diagnostics: record `CHANNEL: SARIF VOID` and proceed to channel 2, a detailed-verbosity file logger
  searched for `RS0030`. If channel 2's control is also void, proceed to channel 3: temporarily set
  the RS0030 severity in `.editorconfig` to `warning`, run only
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  (which passes no `TreatWarningsAsErrors`, so warnings do not fail it), capture the diagnostics, and
  restore the value immediately in the same task. Record `CHANNEL: SARIF`, `CHANNEL: DETAILED-LOG` or
  `CHANNEL: SEVERITY-RAISE` in
  `<FEATURE>/evidence/qa-gates/p4-t1-rs0030-channel.md`, together with the exact command used, the
  three control counts and the control site paths from D7.
  **Acceptance:** the artifact records exactly one `CHANNEL:` value and, for the recorded channel, at
  least one RS0030 diagnostic whose own result location is
  `UtilitiesCS/Threading/ApplicationIdleTimer.cs`, at least one whose result location is
  `QuickFiler/Controllers/EfcHomeControllerDependencies.cs`, and at least one whose result location
  is `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs`. Each control count is a count of RS0030
  results at that file, never a count of textual occurrences of the file name: with
  `EnableNETAnalyzers` and `EnforceCodeStyleInBuild` set, a control file is expected to carry
  unrelated IDE diagnostics whose result locations name it, so a file-name search would report at
  least 1 whether or not the channel carries RS0030 and could certify a void channel. The artifact
  records, per control file, the RS0030-scoped count, the extracted result-location URIs, and the
  emitted SARIF `version` value together with the location shape that version implies. A channel for
  which any of the three RS0030-scoped control counts is 0
  must not be recorded as the certified channel.

- [x] [P4-T2] Through the channel certified by P4-T1, observe RS0030 at the five files under test.
  First re-derive the expected line numbers at observation time, so that no line number is carried
  from this document:
  ```powershell
      Select-String -LiteralPath "QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs" -CaseSensitive -SimpleMatch "CancelAfter("
      Select-String -LiteralPath "QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs" -CaseSensitive -SimpleMatch "WaitOne(0)"
      Select-String -LiteralPath "UtilitiesCS/Threading/TimeOutTask.cs" -CaseSensitive -Pattern "new CancellationTokenSource\([^)]"
      Select-String -LiteralPath "QuickFiler/Controllers/QfcQueue.cs" -CaseSensitive -Pattern "new CancellationTokenSource\([^)]"
      Select-String -LiteralPath "UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs" -CaseSensitive -Pattern "new CancellationTokenSource\([^)]"
  ```
  Then, from the certified channel's output for the run that produced the P4-T1 controls, extract
  every RS0030 diagnostic with its repository-relative file path and line number. Write
  `<FEATURE>/evidence/qa-gates/p4-t2-rs0030-observation.md` with the channel name, the exact command,
  the full extracted list, the observed total, and a per-file reconciliation table comparing the
  re-derived line numbers with the observed ones. Write repository-relative paths only; the raw SARIF
  and log documents stay under `coverage/826-raw/` and are not committed.
  **Acceptance:** for each of the five files under test, the set of re-derived line numbers minus the
  set of observed RS0030 line numbers is empty, so every re-derived site produced a diagnostic; and
  the set of observed line numbers minus the set of re-derived line numbers is recorded verbatim
  rather than asserted, because a diagnostic at a line the token search did not match is information,
  not a failure. The three P4-T1 control counts are recorded again from the same run, each measured as
  the number of RS0030 diagnostics whose own result location is the control file rather than the
  number of textual occurrences of the file name, and each is at least 1; the observed total is
  recorded. The observed total is not asserted to equal 15; the
  predicted figure is a prediction and the pass condition is per-site observation with a live control.
  If any re-derived site produces no diagnostic, the executor records that site's file path, line
  number and source text in the artifact, leaves AC10 unchecked, and reports the discrepancy to the
  orchestrator rather than passing this task.

- [x] [P4-T3] Prove the severity is at `suggestion` in the tree, unconditionally, whichever channel
  P4-T1 certified. This gate is what makes the channel-3 branch safe: raising a severity to measure it
  is permitted, leaving it raised is not.
  ```powershell
      @(Select-String -LiteralPath ".editorconfig" -CaseSensitive -SimpleMatch "dotnet_diagnostic.RS0030.severity = suggestion").Count
      @(Select-String -LiteralPath ".editorconfig" -CaseSensitive -SimpleMatch "dotnet_diagnostic.RS0030.severity = warning").Count
      git diff $Base -- .editorconfig
  ```
  From the anchored diff, count changed lines (leading single `+` or `-`, excluding the `+++` and
  `---` headers) containing the `-SimpleMatch` token `dotnet_diagnostic.`. Write
  `<FEATURE>/evidence/qa-gates/p4-t3-severity-restored.md` recording the two counts, the changed-line
  count, and the `CHANNEL:` value copied from P4-T1.
  **Acceptance:** the `suggestion` count is 1, the `warning` count is 0, and the changed-line count
  containing `dotnet_diagnostic.` is 0.

---

### Phase 5 — Item 1: remove the 34 unrestored console-writer installs across the 33 test files

Before deleting anything in any task of this phase, read the enclosing initializer body in full.
Risk 5 is deleting one line too many from an initializer that also constructs a `MockRepository`,
mocks or fixtures; in the 23 delete-one-line files, delete **only** the single install statement.

- [x] [P5-T1] Delete the single `Console.SetOut(new DebugTextWriter());` statement from each of these
  six `UtilitiesCS.Test/NewtonsoftHelpers` files, leaving their initializer methods and every other
  statement intact: `WrapperScoDictionaryTest.cs`, `WrapperScDictionaryTest.cs`,
  `ScoDictionaryConverterTests.cs`, `ScDictionaryConverter_Tests.cs`, `PeopleScoConverter_Tests.cs`,
  `FilePathHelperConverterTests.cs`.
  **Acceptance:** across those six paths, `@(Select-String -LiteralPath <the six paths> -CaseSensitive -SimpleMatch "Console.SetOut(").Count`
  is 0 and `@(... -SimpleMatch "DebugTextWriter").Count` is 0, while
  `@(... -SimpleMatch "TestInitialize").Count` is unchanged from the P0-T5 census for those six files.
  Evidence: `<FEATURE>/evidence/qa-gates/p5-t1-newtonsoft-sweep.md`.

- [x] [P5-T2] Delete the single install statement from each of these six `UtilitiesCS.Test`
  EmailIntelligence files, leaving their initializer methods and every other statement intact:
  `EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs`,
  `EmailIntelligence/EmailParsingSorting/MinedMailInfoTests.cs`,
  `EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs`,
  `EmailIntelligence/Bayesian/BayesianSerializationHelper_Tests.cs`,
  `EmailIntelligence/Bayesian/BayesianPerformanceMeasurement_Tests.cs`,
  `EmailIntelligence/Bayesian/BayesianClassifierTests.cs`.
  Do not touch `EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs`: its
  `Console.SetOut` occurrence is commented out, installs nothing, and is one of the two files AC1
  expects to still match after the change.
  **Acceptance:** across those six paths, `Console.SetOut(` count 0 and `DebugTextWriter` count 0;
  `BayesianClassifierTests_UnfinishedStubs.cs` still has `Console.SetOut(` count 1.
  Evidence: `<FEATURE>/evidence/qa-gates/p5-t2-emailintelligence-sweep.md`.

- [x] [P5-T3] Delete the single install statement from `UtilitiesCS.Test/Threading/AppGlobalsConverterTests.cs`
  and `UtilitiesCS.Test/Threading/AppGlobalsConverterTests_Unfinished.cs`, leaving their initializer
  methods and every other statement intact.
  **Acceptance:** across those two paths, `Console.SetOut(` count 0 and `DebugTextWriter` count 0.
  Evidence: `<FEATURE>/evidence/qa-gates/p5-t3-threading-sweep.md`.

- [x] [P5-T4] Delete the single install statement from each of these six `QuickFiler.Test/Controllers`
  files, matching on the full filename: `QfcHomeControllerTests.cs`,
  `QfcHomeControllerRunAsyncTests.cs`, `QfcHomeControllerPropertyTests.cs`,
  `QfcHomeControllerIterationTests.cs`, `QfcFormControllerTests.cs`, `QfcFormControllerSeamTests.cs`.
  `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` is sibling-owned by another child of
  this epic, is not in the population, and must not be opened or edited by this task.
  **Acceptance:** across those six paths, `Console.SetOut(` count 0 and `DebugTextWriter` count 0; and
  both spans below are empty, the first proving the tracked sibling-owned file is unmodified and
  undeleted relative to the base anchor, the second proving it was not deleted and recreated as an
  untracked file:
  ```powershell
      git diff --name-only $Base -- "QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs"
      git status --porcelain --untracked-files=all -- "QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs"
  ```
  The anchored diff alone is sufficient for the two states this gate must detect, because the path is
  tracked and this plan never creates it, so a modification or a deletion is visible to it; the
  porcelain companion is added because it costs one line and catches the one remaining state the diff
  misses, a delete-then-recreate-as-untracked.
  Evidence: `<FEATURE>/evidence/qa-gates/p5-t4-quickfiler-sweep.md`.

- [x] [P5-T5] Delete the single install statement from
  `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs`, leaving its initializer method and
  every other statement intact. The directory name contains a space, so quote the path in every
  invocation.
  **Acceptance:** for that path, `Console.SetOut(` count 0 and `DebugTextWriter` count 0.
  Evidence: `<FEATURE>/evidence/qa-gates/p5-t5-todomodel-people-sweep.md`.

- [x] [P5-T6] In `ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs` and
  `ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs`, delete all four elements
  together: the `private DebugTextWriter tw;` field declaration, the `tw = new DebugTextWriter();`
  assignment, the `Console.SetOut(tw);` call, and the orphaned commented-out `[ClassInitialize]` block
  that also references `tw`. Keep the `[TestInitialize]` method and its
  `this.mockRepository = new MockRepository(MockBehavior.Strict);` statement: these two files are in
  the delete-one-line group, not in the AC4 delete-the-method group. Deleting only the call leaves
  CS0414 and deleting only the call and the assignment leaves CS0169; both are compiler warnings that
  `/p:TreatWarningsAsErrors=true` promotes to build errors, which is why all four elements go
  together.
  **Acceptance:** for each of the two paths,
  `@(Select-String -LiteralPath <path> -CaseSensitive -Pattern "\btw\b").Count` is 0,
  `-SimpleMatch "DebugTextWriter"` count is 0, `-SimpleMatch "Console.SetOut("` count is 0, and
  `-SimpleMatch "TestInitialize"` count is 2 (unchanged from the P0-T5 census).
  Evidence: `<FEATURE>/evidence/qa-gates/p5-t6-treenode-sweep.md`.

- [x] [P5-T7] Delete the whole initializer method, together with its attribute and its install
  statement, from each of these six files, in which the install is the method's only executable
  statement: `VBFunctions.Test/ComputerInfo_Test.cs`,
  `UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs`, `UtilitiesCS.Test/Extensions/Frexp_Test.cs`,
  `UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs`,
  `UtilitiesCS.Test/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew_Tests.cs`,
  `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs`. Read each method body first and confirm the
  install is its only statement before deleting the method.
  **Acceptance:** across those six paths, `-SimpleMatch "TestInitialize"` count 0,
  `-SimpleMatch "Console.SetOut("` count 0 and `-SimpleMatch "DebugTextWriter"` count 0.
  Evidence: `<FEATURE>/evidence/qa-gates/p5-t7-empty-initializers.md`.

- [x] [P5-T8] In `UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs`,
  delete both initializer methods, one in each of the file's two `[TestClass]` types, together with
  their attributes and their install statements. This is the only file in the population carrying two
  live install statements.
  **Acceptance:** for that path, `-SimpleMatch "TestInitialize"` count 0 (down from 4),
  `-SimpleMatch "Console.SetOut("` count 0 (down from 2) and `-SimpleMatch "DebugTextWriter"` count 0;
  and `-SimpleMatch "[TestClass]"` count is unchanged at 2.
  Evidence: `<FEATURE>/evidence/qa-gates/p5-t8-obsolete-bayesian.md`.

- [x] [P5-T9] Delete the whole initializer method, its attribute, its install statement **and** the
  orphaned commented-out line that formed the rest of its body, from each of these three files:
  `UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs`,
  `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs`,
  `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs`. Only the comment
  inside the deleted method body is removed; comments outside the method are left alone.
  **Acceptance:** across those three paths, `-SimpleMatch "TestInitialize"` count 0,
  `-SimpleMatch "Console.SetOut("` count 0, `-SimpleMatch "DebugTextWriter"` count 0, and, across
  those three paths, `-SimpleMatch "mockRepository = new MockRepository"` count 0, which is capable of
  failing because the only current match in each file is the commented residue this task deletes.
  Re-derived against the tree on 2026-09-09, in revision round 4, that single match sits at
  `BayesianClassifierSharedTests.cs` line 24, `BayesianClassifierGroupTests.cs` line 25 and
  `AngleSharpParsedEmailBodyTests.cs` line 22; the surviving `//    this.mockRepository.VerifyAll();`
  lines and the `//private MockRepository mockRepository;` declaration at
  `BayesianClassifierGroupTests.cs` line 19 do not match the token, so leaving them in place does not
  make this gate unsatisfiable.
  Evidence: `<FEATURE>/evidence/qa-gates/p5-t9-comment-only-initializers.md`.

- [x] [P5-T10] Verify the whole item-1 sweep at repository scope.
  ```powershell
      $cs = @(git ls-files "*.cs")
      $so = @(Select-String -LiteralPath $cs -CaseSensitive -SimpleMatch "Console.SetOut(")
      $so.Count
      $so.Path
  ```
  Write `<FEATURE>/evidence/qa-gates/p5-t10-sweep-verification.md` recording the count, the distinct
  file list, the per-file `DebugTextWriter` count for all 33 write-set files, and the per-file
  `TestInitialize` count for the ten AC4 files.
  **Acceptance:** the repository-wide `Console.SetOut(` occurrence count is 2 and the distinct file
  list is exactly `TaskMaster/ThisAddIn.cs` and
  `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs`; the
  `DebugTextWriter` count is 0 in every one of the 33 write-set files; and the `TestInitialize` count
  is 0 in every one of the ten AC4 files. The four out-of-scope files that legitimately retain the
  `DebugTextWriter` token - `UtilitiesCS/HelperClasses/Logging/DebugTextWriter.cs`,
  `UtilitiesCS.Test/HelperClasses/DebugTextLogger_Tests.cs`, `UtilitiesCS.Test/DeedleTests.cs`,
  `UtilitiesCS.Test/Extensions/DeedleTests.cs` - plus `TaskMaster/ThisAddIn.cs` are recorded as
  expected retentions and are not asserted to be zero.

- [x] [P5-T11] Interim nullable build to discharge the CS0169/CS0414 hazard (spec Risk 1) as early as
  possible. This is an interim diagnostic build, not a toolchain-loop pass; the loop itself runs in
  Phase 7.
  ```powershell
      & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl "/flp:LogFile=coverage/826-raw/p5-t11-nullable.log;Verbosity=detailed"
      $LASTEXITCODE
      @(Select-String -LiteralPath "coverage/826-raw/p5-t11-nullable.log" -CaseSensitive -SimpleMatch "CS0169").Count
      @(Select-String -LiteralPath "coverage/826-raw/p5-t11-nullable.log" -CaseSensitive -SimpleMatch "CS0414").Count
      @(Select-String -LiteralPath "coverage/826-raw/p5-t11-nullable.log" -CaseSensitive -SimpleMatch " 0 Error(s)").Count
      @(Select-String -LiteralPath "coverage/826-raw/p5-t11-nullable.log" -CaseSensitive -Pattern "Skipping target \x22CoreCompile\x22").Count
      @(Select-String -LiteralPath "coverage/826-raw/p5-t11-nullable.log" -CaseSensitive -Pattern "Task \x22Csc\x22").Count
  ```
  Write `<FEATURE>/evidence/qa-gates/p5-t11-interim-nullable.md`.
  **Acceptance:** exit code 0, CS0169 count 0, CS0414 count 0, ` 0 Error(s)` count at least 1,
  `Skipping target "CoreCompile"` count 0 and `Task "Csc"` count at least 1, the last of which is what
  makes the two zero CS-code counts non-vacuous.

---

### Phase 6 — Item-2 and item-3 write-set verification

- [x] [P6-T1] Verify AC5 and AC6 against the anchored diff, without reference to any line number.
  ```powershell
      @(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "Console.WriteLine").Count
      @(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "Console.").Count
      @(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "timed out on try").Count
      git diff --numstat $Base -- $Tac
      git diff $Base -- $Tac
  ```
  From the full anchored diff, count the removed lines containing `Console.WriteLine` and the added
  lines containing `logger.Warn`. Separately, from the current file, obtain the line numbers of the
  two `timed out on try` occurrences and of every `catch (` occurrence, and confirm that the nearest
  preceding `catch (` for the first is `catch (TaskCanceledException)` and for the second is
  `catch (TimeoutException)`. Write `<FEATURE>/evidence/qa-gates/p6-t1-ac5-ac6.md` with the anchored
  diff, the four counts and the catch-ordering derivation.
  **Acceptance:** `Console.WriteLine` count 0; `Console.` count 1; `timed out on try` count 2; the
  anchored numstat reads exactly 2 added and 2 removed lines; the removed lines containing
  `Console.WriteLine` number 2 and the added lines containing `logger.Warn` number 2; and the
  catch-ordering derivation places one added `logger.Warn` inside `catch (TaskCanceledException)` and
  one inside `catch (TimeoutException)`. The anchored diff shows no other changed line in that file,
  so the deadline window, retry counter, `timeoutSourceFactory` seam, caught exception types, control
  flow after each diagnostic, other catch clauses and `using` directives are all provably untouched.

- [x] [P6-T2] Verify AC8 and the no-other-project-file constraint.
  ```powershell
      git diff --numstat $Base -- "UtilitiesCS.Test/UtilitiesCS.Test.csproj"
      git diff $Base -- "UtilitiesCS.Test/UtilitiesCS.Test.csproj"
      git diff --name-only $Base -- "*.csproj" "*.props" "*.targets"
      git status --porcelain --untracked-files=all -- "*.csproj" "*.props" "*.targets"
  ```
  Write `<FEATURE>/evidence/qa-gates/p6-t2-ac8-project-file.md` with all four outputs.
  **Acceptance:** the numstat reads exactly 1 added and 0 removed lines; the anchored diff shows a
  single added line whose text contains the token `OlTableExtensionsTimeoutDiagnosticsTests.cs` and
  shows no reordered or reformatted existing entry; the name-listing diff lists exactly
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj` and nothing else; and the porcelain companion span lists
  no untracked or modified project, props or targets file other than that one.

- [x] [P6-T3] Verify AC9 and AC12 against the final state of `BannedSymbols.txt`.
  ```powershell
      (Get-Content -LiteralPath "BannedSymbols.txt").Count
      @(Select-String -LiteralPath "BannedSymbols.txt" -CaseSensitive -SimpleMatch "TimeoutAfter").Count
      @(Select-String -LiteralPath "BannedSymbols.txt" -CaseSensitive -SimpleMatch "WaitHandle.WaitOne;").Count
      @(Select-String -LiteralPath "BannedSymbols.txt" -CaseSensitive -SimpleMatch "M:System.Threading.").Count
      git diff --numstat $Base -- "BannedSymbols.txt"
  ```
  Additionally assert, for each of these eight `-SimpleMatch` tokens,
  `@(Select-String -LiteralPath "BannedSymbols.txt" -CaseSensitive -SimpleMatch <token>).Count` is 1
  and the matched line also contains the token `TimeProvider`:
  `M:System.Threading.CancellationTokenSource.CancelAfter(System.Int32);`,
  `M:System.Threading.CancellationTokenSource.CancelAfter(System.TimeSpan);`,
  `M:System.Threading.CancellationTokenSource.#ctor(System.Int32);`,
  `M:System.Threading.CancellationTokenSource.#ctor(System.TimeSpan);`,
  `M:System.Threading.WaitHandle.WaitOne(System.Int32);`,
  `M:System.Threading.WaitHandle.WaitOne(System.TimeSpan);`,
  `M:System.Threading.WaitHandle.WaitOne(System.Int32,System.Boolean);`,
  `M:System.Threading.WaitHandle.WaitOne(System.TimeSpan,System.Boolean);`. Write
  `<FEATURE>/evidence/qa-gates/p6-t3-ac9-ac12.md`.
  **Acceptance:** line count 15; `TimeoutAfter` count 0; `WaitHandle.WaitOne;` count 0; each of the
  eight DocID counts is 1 with a `TimeProvider` message; and the anchored numstat reads exactly 8
  added and 0 removed lines, which is what proves the seven pre-existing lines are unchanged.

---

### Phase 7 — Final QA toolchain loop

Run steps 1 to 4 in order. If any step fails, or if step 1 rewrites any file - that is, if P7-T1's
`$rewritten` count is non-zero, whether the rewritten file is inside the write set or outside it -
fix the cause and restart this phase from P7-T1. The phase is complete only when all four steps pass
in a single uninterrupted pass. The C4 convention that every `.cs`-touching task formats the paths it
touched is what makes a zero `$rewritten` the expected outcome here rather than a surprise.

- [x] [P7-T1] Toolchain step 1. Record SHA-256 hashes of every tracked `.cs` file plus the untracked
  `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs` immediately
  before and immediately after `format`, define `$rewritten` as the number of hash differences, and
  pair the write-mode command with the read-only `check` form, whose clean run prints a line beginning
  with the literal token `Checked `. The hash set is repository-wide rather than write-set-scoped
  because the restart rule in this phase's preamble is triggered by a rewrite of any file, and a
  write-set-scoped hash set could not detect one outside it.
  ```powershell
      $cs = @(git ls-files "*.cs") + @("UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs")
      $before = @(Get-FileHash -Algorithm SHA256 -LiteralPath $cs)
      & $dotnet tool run csharpier format .
      $LASTEXITCODE
      $after = @(Get-FileHash -Algorithm SHA256 -LiteralPath $cs)
      $rewritten = 0
      for ($i = 0; $i -lt $before.Count; $i++) { if ($before[$i].Hash -ne $after[$i].Hash) { $rewritten++ } }
      $rewritten
      & $dotnet tool run csharpier check .
      $LASTEXITCODE
      git diff --name-only $Base -- . ":(exclude).claude"
      git status --porcelain --untracked-files=all -- . ":(exclude).claude"
  ```
  `Get-FileHash -LiteralPath` accepts a string array and preserves input order, so the two hash arrays
  are index-aligned and the loop compares each file with itself.
  Write `<FEATURE>/evidence/qa-gates/p7-t1-format.md` with both exit codes, the `Formatted` line, the
  `Checked ` line, the `$rewritten` count derived from the hash comparison, and both git spans.
  **Acceptance:** both exit codes are 0; the `check` output contains a line beginning with `Checked `;
  `$rewritten` is 0, proving the write-mode command changed nothing on this pass; and every path in
  the name-listing diff and in the porcelain companion span is inside the P8-T1 allow-list. If
  `$rewritten` is non-zero the phase restarts from P7-T1, whatever the rewritten files were. If any
  rewritten file lies outside the write set, the executor additionally records that path in the
  artifact, halts, and reports it to the orchestrator before restarting, because repairing
  pre-existing drift inside this feature would put an out-of-scope file into the anchored diff and
  make AC16 unsatisfiable.

- [x] [P7-T2] Toolchain step 2.
  ```powershell
      & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:LogFile=coverage/826-raw/p7-t2-analyzers.log;Verbosity=detailed"
      $LASTEXITCODE
      @(Select-String -LiteralPath "coverage/826-raw/p7-t2-analyzers.log" -CaseSensitive -SimpleMatch " 0 Error(s)").Count
      @(Select-String -LiteralPath "coverage/826-raw/p7-t2-analyzers.log" -CaseSensitive -Pattern "Skipping target \x22CoreCompile\x22").Count
      @(Select-String -LiteralPath "coverage/826-raw/p7-t2-analyzers.log" -CaseSensitive -Pattern "Task \x22Csc\x22").Count
      @(Select-String -LiteralPath "coverage/826-raw/p7-t2-analyzers.log" -CaseSensitive -SimpleMatch "RS0030").Count
  ```
  Write `<FEATURE>/evidence/qa-gates/p7-t2-analyzers.md` with all five figures.
  **Acceptance:** exit code 0; ` 0 Error(s)` count at least 1; `Skipping target "CoreCompile"` count 0;
  `Task "Csc"` count at least 1. The RS0030 count is recorded, not asserted: at `suggestion` severity
  the diagnostic is info-level and may be absent from this log entirely, which is exactly why AC10 is
  satisfied through the certified channel in Phase 4 and not from this log.

- [x] [P7-T3] Toolchain step 3.
  ```powershell
      & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl "/flp:LogFile=coverage/826-raw/p7-t3-nullable.log;Verbosity=detailed"
      $LASTEXITCODE
      @(Select-String -LiteralPath "coverage/826-raw/p7-t3-nullable.log" -CaseSensitive -SimpleMatch " 0 Error(s)").Count
      @(Select-String -LiteralPath "coverage/826-raw/p7-t3-nullable.log" -CaseSensitive -SimpleMatch "CS0169").Count
      @(Select-String -LiteralPath "coverage/826-raw/p7-t3-nullable.log" -CaseSensitive -SimpleMatch "CS0414").Count
      @(Select-String -LiteralPath "coverage/826-raw/p7-t3-nullable.log" -CaseSensitive -Pattern "Skipping target \x22CoreCompile\x22").Count
      @(Select-String -LiteralPath "coverage/826-raw/p7-t3-nullable.log" -CaseSensitive -Pattern "Task \x22Csc\x22").Count
  ```
  `/p:Nullable=enable` is not added. Write `<FEATURE>/evidence/qa-gates/p7-t3-nullable.md`.
  **Acceptance:** exit code 0; ` 0 Error(s)` count at least 1; CS0169 count 0; CS0414 count 0;
  `Skipping target "CoreCompile"` count 0; `Task "Csc"` count at least 1. This is the AC3 gate.

- [x] [P7-T4] Toolchain step 4, measured form. This run supplies the post-change numbers AC15
  compares against P0-T9.
  ```powershell
      dotnet-coverage collect --output coverage/826-raw/p7-t4.cobertura.xml --output-format cobertura -- $vstest $Assemblies /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/Logger:trx;LogFileName=p7-t4.trx" /ResultsDirectory:coverage/826-raw/p7-t4 /TestCaseFilter:$FullFilter
      $LASTEXITCODE
      $x = [xml](Get-Content -LiteralPath "coverage/826-raw/p7-t4.cobertura.xml" -Raw)
      $x.coverage.GetAttribute("line-rate")
      $x.coverage.GetAttribute("lines-valid")
      $x.coverage.GetAttribute("lines-covered")
  ```
  Derive the two changed-line hit counts exactly as in P0-T9, except that the current line numbers are
  obtained with `Select-String -CaseSensitive -SimpleMatch "timed out on try"` against the post-change
  file, and the `hits` values are aggregated across every `<class>` whose `filename` ends with
  `OlTableExtensions.TableAccess.cs`. Write
  `<FEATURE>/evidence/qa-gates/p7-t4-tests-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`
  and an `Output Summary:` carrying the TRX total, passed, failed and notExecuted counts, the numeric root
  `line-rate`, `lines-valid` and `lines-covered`, the per-file line rate for
  `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`, and the two hit counts labelled
  `PostChangeChangedLineHits:`.
  If no `<line number="N">` element exists for a changed line under any `<class>` whose `filename`
  ends with `OlTableExtensions.TableAccess.cs`, record that line's hit count as `0` and annotate it
  `no line element emitted`. Both runs apply this rule identically, so P7-T6 compares like for like.
  **Acceptance:** exit code 0; TRX failed count 0; the test named by P2-T2 appears in the TRX with
  outcome `Passed`; and every numeric field above is present and numeric.

- [x] [P7-T5] Toolchain step 4, confirming CI-verbatim form. P7-T4 is the measured run; this run
  confirms the CI-shaped invocation passes and that the two runs agree on result counts.
  ```powershell
      & $vstest $Assemblies /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p7-t5.trx" /ResultsDirectory:coverage/826-raw/p7-t5 /TestCaseFilter:$FullFilter
      $LASTEXITCODE
  ```
  `/InIsolation` and the `TestCaseFilter` are recorded environmental necessities on this host, not
  scope reductions of the gate: the filter excludes the shell-icon and live-Outlook classes that hang
  or require a live Outlook process locally and that CI runs unfiltered. Write
  `<FEATURE>/evidence/qa-gates/p7-t5-tests-ci-verbatim.md` with the exact command, the exit code, the
  TRX counters, and the literal filter string used.
  **Acceptance:** exit code 0; TRX failed count 0; and the TRX total, passed and notExecuted counts
  equal the corresponding P7-T4 counts.

- [x] [P7-T6] Coverage comparison and AC15 record. Compare P0-T9 against P7-T4 under the D15
  comparability rule and record the outcome.
  Write `<FEATURE>/evidence/qa-gates/p7-t6-coverage-delta.md` with: baseline and post-change
  `line-rate`, `lines-valid` and `lines-covered`; the absolute percentage difference in `lines-valid`;
  the comparability verdict `COMPARABLE` or `NOT COMPARABLE`; the baseline and post-change per-file
  line rate for `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`; the
  `BaselineChangedLineHits:` and `PostChangeChangedLineHits:` pairs; the `BRANCH:` value from P1-T1;
  and an explicit statement that no coverage exclusion, no threshold change and no
  `[ExcludeFromCodeCoverage]` attribute was added anywhere in this change.
  **Acceptance:** each post-change changed-line hit count is greater than or equal to its baseline
  counterpart, so no changed line lost coverage; the `>= 85%` line-coverage floor from
  `.claude/rules/general-unit-test.md` is reported against the measured post-change figure; and either
  the verdict is `COMPARABLE` and the post-change root `line-rate` is not lower than the baseline root
  `line-rate`, or the verdict is `NOT COMPARABLE` and the post-change per-file line rate for the
  table-access file is not lower than its baseline value. Under `BRANCH: UNREACHABLE` the equal-hits
  case is the expected outcome and is recorded explicitly as the identity argument: one uncovered line
  was substituted for one uncovered line, so coverage was reduced on no changed line. Item 1 is
  expected to move no figure at all, because all 33 files compile into `*.Test.dll` assemblies that
  the pipeline excludes from instrumentation.

- [x] [P7-T7] Record the loop-closure attestation. Write
  `<FEATURE>/evidence/qa-gates/p7-t7-toolchain-attestation.md` naming the four commands in order, the
  artifact path for each, the exit code for each, and the number of times the phase was restarted.
  **Acceptance:** the artifact names all four steps in the order format, analyzer build, nullable
  build, test; each cites an artifact from P7-T1 through P7-T5 with exit code 0; and it states that
  the final pass completed without any step failing or rewriting a file.

---

### Phase 8 — Scope gate, acceptance check-off, commit

- [x] [P8-T1] AC16 scope gate (measured). Stage the new test file with an intent-to-add so the
  name-listing diff can see it, then enumerate the anchored diff and the porcelain companion span.
  ```powershell
      git add --intent-to-add "UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs"
      git diff --name-only $Base -- . ":(exclude).claude"
      git status --porcelain --untracked-files=all -- . ":(exclude).claude"
  ```
  Compare the union of both spans against the allow-list, which is: the 33 item-1 test files named in
  Phase 5; `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`;
  `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs`;
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj`; `BannedSymbols.txt`; `.editorconfig`;
  `<FEATURE>/spec.md`; `<FEATURE>/plan.2026-09-08T23-52.md`; and any path beginning
  `<FEATURE>/evidence/`. Write `<FEATURE>/evidence/qa-gates/p8-t1-ac16-write-set.md` with both spans
  and the set difference in each direction.
  **Acceptance:** the set difference "observed minus allow-list" is empty; and the observed set
  contains no entry for `CLAUDE.md`, for `<FEATURE>/issue.md`, for any path under
  `<FEATURE>/research/`, for any path under `.claude/rules/`, `.github/instructions/` or
  `docs/features/epics/`, or for any sibling-owned path the spec enumerates under "Explicitly excluded
  systems", specifically including `UtilitiesCS/Threading/TimeOutTask.cs` and
  `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`.

- [x] [P8-T2] Check off AC1 in `spec.md` by changing its `- [ ]` to `- [x]`, citing
  `<FEATURE>/evidence/qa-gates/p5-t10-sweep-verification.md`.
  **Acceptance:** the AC1 bullet in `spec.md` reads `- [x]`, and the cited artifact records a
  repository-wide `Console.SetOut(` occurrence count of 2 across exactly the two named files.

- [x] [P8-T3] Check off AC2 in `spec.md`, citing
  `<FEATURE>/evidence/qa-gates/p5-t10-sweep-verification.md`.
  **Acceptance:** the AC2 bullet reads `- [x]`, and the cited artifact records a `DebugTextWriter`
  count of 0 for each of the 33 write-set files.

- [x] [P8-T4] Check off AC3 in `spec.md`, citing `<FEATURE>/evidence/qa-gates/p5-t6-treenode-sweep.md`
  and `<FEATURE>/evidence/qa-gates/p7-t3-nullable.md`.
  **Acceptance:** the AC3 bullet reads `- [x]`; the first artifact records a whole-word `tw` count of
  0 in both `TreeNode` files; and the second records exit code 0 with CS0169 count 0 and CS0414
  count 0.

- [x] [P8-T5] Check off AC4 in `spec.md`, citing
  `<FEATURE>/evidence/qa-gates/p5-t7-empty-initializers.md`,
  `<FEATURE>/evidence/qa-gates/p5-t8-obsolete-bayesian.md` and
  `<FEATURE>/evidence/qa-gates/p5-t9-comment-only-initializers.md`.
  **Acceptance:** the AC4 bullet reads `- [x]`, and the three artifacts together record a
  `TestInitialize` count of 0 for all ten named files.

- [x] [P8-T6] Check off AC5 in `spec.md`, citing `<FEATURE>/evidence/qa-gates/p6-t1-ac5-ac6.md`.
  **Acceptance:** the AC5 bullet reads `- [x]`, and the artifact records `Console.WriteLine` count 0
  and `Console.` count 1 for the table-access file.

- [x] [P8-T7] Check off AC6 in `spec.md`, citing `<FEATURE>/evidence/qa-gates/p6-t1-ac5-ac6.md`.
  **Acceptance:** the AC6 bullet reads `- [x]`, and the artifact records the 2-added, 2-removed
  anchored numstat, two removed `Console.WriteLine` lines, two added `logger.Warn` lines, and the
  catch-ordering derivation placing one in each catch clause.

- [x] [P8-T8] Check off AC7 in `spec.md`, citing
  `<FEATURE>/evidence/other/item2-branch-reachability.<timestamp>.md`,
  `<FEATURE>/evidence/regression-testing/p2-t2-regression-test.md`,
  `<FEATURE>/evidence/regression-testing/fail-before-exception.<timestamp>.md` and
  `<FEATURE>/evidence/qa-gates/p7-t4-tests-coverage.md`. If `BRANCH: UNREACHABLE` was recorded, the
  check-off note in `spec.md` must state, in one sentence, that the AC7 test pins the reachable
  observable contract rather than the unreachable `catch (TimeoutException)` branch, and must name the
  reachability artifact; AC7 is not marked satisfied without that statement.
  **Acceptance:** the AC7 bullet reads `- [x]`; the named test appears in the P7-T4 TRX with outcome
  `Passed`; the test file contains zero occurrences of `Thread.Sleep` and zero of `Task.Delay`; and,
  under `BRANCH: UNREACHABLE`, the check-off note carries the required one-sentence statement.

- [x] [P8-T9] Check off AC8 in `spec.md`, citing
  `<FEATURE>/evidence/qa-gates/p6-t2-ac8-project-file.md`.
  **Acceptance:** the AC8 bullet reads `- [x]`, and the artifact records 1 added and 0 removed lines
  for `UtilitiesCS.Test/UtilitiesCS.Test.csproj` and no other project file in the name-listing diff.

- [x] [P8-T10] Check off AC9 in `spec.md`, citing `<FEATURE>/evidence/qa-gates/p6-t3-ac9-ac12.md`.
  **Acceptance:** the AC9 bullet reads `- [x]`, and the artifact records all eight DocID counts as 1
  with a `TimeProvider` message and an 8-added, 0-removed anchored numstat.

- [x] [P8-T11] Check off AC10 in `spec.md`, citing
  `<FEATURE>/evidence/qa-gates/p4-t1-rs0030-channel.md` and
  `<FEATURE>/evidence/qa-gates/p4-t2-rs0030-observation.md`.
  **Acceptance:** the AC10 bullet reads `- [x]`; the channel artifact records exactly one `CHANNEL:`
  value with all three control counts at least 1; and the observation artifact records, for each of
  the five files under test, an empty set difference of re-derived line numbers minus observed RS0030
  line numbers. If the controls were void on every channel, AC10 is left unchecked, the artifacts record
  the void result, and the executor reports the blocker rather than downgrading AC10 to an absence
  check.

- [x] [P8-T12] Check off AC11 in `spec.md`, citing
  `<FEATURE>/evidence/qa-gates/p3-t3-severity-unchanged.md` and
  `<FEATURE>/evidence/qa-gates/p4-t3-severity-restored.md`.
  **Acceptance:** the AC11 bullet reads `- [x]`; both artifacts record a `suggestion` count of 1 and a
  count of 0 for anchored-diff changed lines containing `dotnet_diagnostic.`.

- [x] [P8-T13] Check off AC12 in `spec.md`, citing `<FEATURE>/evidence/qa-gates/p6-t3-ac9-ac12.md`.
  **Acceptance:** the AC12 bullet reads `- [x]`, and the artifact records a `TimeoutAfter` count of 0
  and a `WaitHandle.WaitOne;` count of 0 in `BannedSymbols.txt`.

- [x] [P8-T14] Check off AC13 in `spec.md`, citing
  `<FEATURE>/evidence/qa-gates/p3-t2-editorconfig-comment.md`.
  **Acceptance:** the AC13 bullet reads `- [x]`, and the artifact records a region `181` count of 0
  together with region counts of at least 1 for `TreatWarningsAsErrors`, `_build-nullable.yml` and
  `2026-09-08`, plus the five re-measured symbol counts written into the comment.

- [x] [P8-T15] Check off AC14 in `spec.md`, citing
  `<FEATURE>/evidence/qa-gates/p7-t1-format.md`, `<FEATURE>/evidence/qa-gates/p7-t2-analyzers.md`,
  `<FEATURE>/evidence/qa-gates/p7-t3-nullable.md`, `<FEATURE>/evidence/qa-gates/p7-t4-tests-coverage.md`,
  `<FEATURE>/evidence/qa-gates/p7-t5-tests-ci-verbatim.md` and
  `<FEATURE>/evidence/qa-gates/p7-t7-toolchain-attestation.md`.
  **Acceptance:** the AC14 bullet reads `- [x]`; all four steps are recorded with exit code 0 in a
  single pass; and both msbuild artifacts record `Skipping target "CoreCompile"` count 0 together with
  `Task "Csc"` count at least 1.

- [x] [P8-T16] Check off AC15 in `spec.md`, citing
  `<FEATURE>/evidence/baseline/baseline-tests-coverage.md` and
  `<FEATURE>/evidence/qa-gates/p7-t6-coverage-delta.md`. If `BRANCH: UNREACHABLE` was recorded by
  P1-T1, the check-off note in `spec.md` must state, in one sentence, that the two changed lines
  remain uncovered because the `catch (TimeoutException)` branch is unreachable through the seam as
  the tree stands, that the no-regression obligation is met by the one-uncovered-line-for-one-
  uncovered-line substitution identity rather than by new coverage, and must name the reachability
  artifact; AC15 is not marked satisfied without that statement.
  **Acceptance:** the AC15 bullet reads `- [x]`; the delta artifact records both numeric coverage
  figures, the comparability verdict, the changed-line hit pair with no decrease, and the explicit
  statement that no exclusion, threshold change or `[ExcludeFromCodeCoverage]` attribute was added;
  and, under `BRANCH: UNREACHABLE`, the check-off note carries the required one-sentence statement.

- [x] [P8-T17] Check off AC16 in `spec.md`, citing
  `<FEATURE>/evidence/qa-gates/p8-t1-ac16-write-set.md`.
  **Acceptance:** the AC16 bullet reads `- [x]`, and the artifact records an empty "observed minus
  allow-list" set difference and the explicit absence of every named out-of-scope path.

- [x] [P8-T18] Write the acceptance-criteria status summary at
  `<FEATURE>/evidence/issue-updates/ac-status-summary.<timestamp>.md`, listing all 16 criteria with
  their final state and the artifact path that establishes each. Record `PostedAs: unknown` unless the
  orchestrator posts it, and mirror nothing into `issue.md` (this feature must not edit `issue.md`).
  **Acceptance:** the artifact lists exactly 16 rows, AC1 through AC16, each with a state and at least
  one artifact path, and its states agree with the check-off state of each bullet in `spec.md`.

- [x] [P8-T19] Commit every change and every evidence artifact, then verify a clean tree.
  ```powershell
      git add -- . ":(exclude).claude"
      git status --porcelain --untracked-files=all -- . ":(exclude).claude"
      git commit -m "fix(826): remove unrestored console-writer installs, route timeout diagnostics through log4net, extend BannedSymbols"
      git status --porcelain --untracked-files=all -- . ":(exclude).claude"
  ```
  Do not use `git add -A`: a queued sibling promotion's untracked file would be swept onto this
  branch. Write `<FEATURE>/evidence/qa-gates/p8-t19-final-git-state.md` with the pre-commit porcelain
  span, the commit subject and the post-commit porcelain span. No absolute host path, account name or
  machine name appears in the commit message or in any committed artifact.
  **Acceptance:** the post-commit porcelain span for the scoped pathspec, as observed in the shell at
  the moment the block runs, is empty, and the raw logs, SARIF, TRX, `.coverage` and Cobertura
  documents under `coverage/826-raw/` are absent from the commit because `.gitignore` line 144
  excludes them. This is not a terminal clean-tree claim: writing this task's own artifact and
  checking this task off both happen after that observation, so they reappear in P8-T20's span and are
  committed by P8-T21, which carries the terminal clean-tree gate.

- [x] [P8-T20] Confirming write-set gate over the committed tree.
  ```powershell
      git diff --name-only $Base HEAD -- . ":(exclude).claude"
      git status --porcelain --untracked-files=all -- . ":(exclude).claude"
  ```
  Write `<FEATURE>/evidence/qa-gates/p8-t20-committed-write-set.md`.
  **Acceptance:** every path listed is inside the P8-T1 allow-list, and the listed set includes all 38
  write-set paths, so the change is neither wider nor narrower than the spec's write set. The
  porcelain span is not asserted empty here, and what it lists is the residual P8-T19 left behind
  rather than anything this task has yet written: at the moment this block runs, P8-T19's artifact
  `p8-t19-final-git-state.md` is untracked and this plan file carries P8-T19's check-off, while
  `p8-t20-committed-write-set.md` and this task's own check-off do not exist yet and therefore do not
  appear. The span is recorded verbatim and every entry must be either this plan file or a path under
  `<FEATURE>/evidence/`. P8-T21 clears it, together with this task's own artifact and check-off.

- [x] [P8-T21] Commit the terminal residual left by P8-T19 and P8-T20: the
  `p8-t20-committed-write-set.md` artifact and this plan file's check-offs for P8-T19 and P8-T20.
  This task is ordered write-then-commit, not commit-then-write, and the ordering is what makes its
  acceptance reachable. Perform these steps in exactly this order:
  1. Run `git status --porcelain --untracked-files=all -- . ":(exclude).claude"` on its own, then
     write `<FEATURE>/evidence/qa-gates/p8-t21-terminal-clean-tree.md` carrying `Timestamp:`,
     `Command:` naming that status invocation, `EXIT_CODE:` carrying that invocation's observed exit
     code, `TerminalCommitCommand:` naming the three lines of the block below, the porcelain span that
     invocation printed, the commit subject this task will use, and an `Output Summary:` listing the
     paths that span reported. Every field in this artifact is an observation made before the artifact
     is written. The terminal commit's own exit code and post-commit porcelain span are not recorded
     here, because writing them back would create a fresh uncommitted file and the task would not
     terminate; they are reported to the orchestrator in the completion message instead.
  2. Check this task off in this plan file, changing its `- [ ]` to `- [x]`. This is the last
     documentation edit this plan makes; every subsequent step only reads.
  3. Run the block, whose `git add` therefore sweeps the artifact from step 1 and the check-off from
     step 2 along with the P8-T19 and P8-T20 residual.
  ```powershell
      git add -- . ":(exclude).claude"
      git commit -m "docs(826): record the committed write-set gate and close the plan checklist"
      git status --porcelain --untracked-files=all -- . ":(exclude).claude"
  ```
  Report the observed post-commit porcelain span to the orchestrator in the completion message rather
  than writing it back into the artifact; writing it back would create a fresh uncommitted file and
  the task would not terminate. Do not use `git add -A`, for the reason given in P8-T19, and do not
  amend the P8-T19 commit.
  **Acceptance:** the observed post-commit porcelain span for the scoped pathspec is empty. Because
  the artifact and the check-off were both written before the `git add`, no path this plan authors
  remains uncommitted, and the repository convention of a clean worktree at completion is met without
  a residual fixpoint.
  **Terminal commit result (reported, not written back):** the `git commit` exit code observed in
  step 3 is reported to the orchestrator in the completion message. A non-zero code, or a
  `nothing to commit` message, means step 1 or step 2 was skipped; in that case the executor reverts
  step 2's check-off to `- [ ]`, records the failure in the completion message, and returns to the
  orchestrator rather than reporting this task complete.

---

## Acceptance-criteria traceability

The 16 criteria are taken from `spec.md` section `## Acceptance Criteria` and are neither extended nor
reduced by this plan. The executor checks each off in `spec.md` per the `acceptance-criteria-tracking`
skill as it is verified, one criterion per task, in P8-T2 through P8-T17.

| AC | Implementation task(s) | Verification task | Evidence artifact |
|---|---|---|---|
| AC1 | P5-T1 to P5-T9 | P5-T10 | `evidence/qa-gates/p5-t10-sweep-verification.md` |
| AC2 | P5-T1 to P5-T9 | P5-T10 | `evidence/qa-gates/p5-t10-sweep-verification.md` |
| AC3 | P5-T6 | P5-T11, P7-T3 | `evidence/qa-gates/p5-t6-treenode-sweep.md`, `evidence/qa-gates/p7-t3-nullable.md` |
| AC4 | P5-T7, P5-T8, P5-T9 | P5-T10 | `evidence/qa-gates/p5-t7-empty-initializers.md`, `evidence/qa-gates/p5-t8-obsolete-bayesian.md`, `evidence/qa-gates/p5-t9-comment-only-initializers.md` |
| AC5 | P2-T1 | P6-T1 | `evidence/qa-gates/p6-t1-ac5-ac6.md` |
| AC6 | P2-T1 | P6-T1 | `evidence/qa-gates/p6-t1-ac5-ac6.md` |
| AC7 | P1-T1, P1-T2, P2-T2 | P2-T4, P7-T4 | `evidence/other/item2-branch-reachability.<timestamp>.md`, `evidence/regression-testing/fail-before-exception.<timestamp>.md`, `evidence/regression-testing/p2-t2-regression-test.md` |
| AC8 | P2-T3 | P6-T2 | `evidence/qa-gates/p6-t2-ac8-project-file.md` |
| AC9 | P3-T1 | P6-T3 | `evidence/qa-gates/p6-t3-ac9-ac12.md` |
| AC10 | P3-T1 | P4-T1, P4-T2 | `evidence/qa-gates/p4-t1-rs0030-channel.md`, `evidence/qa-gates/p4-t2-rs0030-observation.md` |
| AC11 | none (deliberate no-change) | P3-T3, P4-T3 | `evidence/qa-gates/p3-t3-severity-unchanged.md`, `evidence/qa-gates/p4-t3-severity-restored.md` |
| AC12 | P3-T1 | P6-T3 | `evidence/qa-gates/p6-t3-ac9-ac12.md` |
| AC13 | P3-T2 | P3-T2 | `evidence/qa-gates/p3-t2-editorconfig-comment.md` |
| AC14 | P7-T1 to P7-T5 | P7-T7 | `evidence/qa-gates/p7-t7-toolchain-attestation.md` |
| AC15 | P2-T2 | P7-T6 | `evidence/baseline/baseline-tests-coverage.md`, `evidence/qa-gates/p7-t6-coverage-delta.md` |
| AC16 | none (scope constraint) | P8-T1, P8-T20 | `evidence/qa-gates/p8-t1-ac16-write-set.md`, `evidence/qa-gates/p8-t20-committed-write-set.md` |

## Out of scope for this plan

No task in this plan edits `CLAUDE.md`, any file under `.claude/rules/` or `.github/instructions/`,
`docs/features/epics/review-residuals-2026-09-08/epic.md`, this feature's `issue.md`, or this
feature's research record. No task edits any sibling-owned path the spec enumerates, in particular
`UtilitiesCS/Threading/TimeOutTask.cs` and
`QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`, and no task changes anything in
`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` other than the two statements this
feature owns. The three report-only findings the spec records (the stale `Directory.Build.props`
claim in CLAUDE.md, the thirteen oversized item-1 test files, and the two SVGControl projects that do
not reference `BannedSymbols.txt`) are carried forward as follow-ups and no action is taken on them
here.
