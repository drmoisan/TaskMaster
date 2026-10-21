# quickfiler-home-controller-metrics (Atomic Plan)

- **Issue:** #442 (also resolves #443, #451)
- **Parent:** epic `quickfiler-bug-family` (integration branch `epic/quickfiler-bug-family-integration`)
- **Owner:** drmoisan
- **Work Mode:** `full-bug`
- **Last Updated:** 2026-08-24T12-00
- **Status:** Ready for preflight (revision cycle 1 deltas B1-B9, A1, A2 applied)
- **Version:** 1.1
- **Plan path (fixed for all revisions):** `docs/features/active/quickfiler-home-controller-metrics-442/plan.2026-08-24T09-40.md`

> **Timestamp provenance.** No shell clock is reachable from the planning session. `2026-08-24T12-00`
> is derived from the session date and is known to be at or after the preflight cycle 1 result this
> revision responds to. The minute component is approximate, not clock-read.

---

## Authority and sources

- **Acceptance-criteria authority:** `docs/features/active/quickfiler-home-controller-metrics-442/spec.md`
  is the single authoritative acceptance-criteria source (work mode `full-bug`). It carries 25
  criteria, AC-1 through AC-25. `user-story.md` is deliberately absent (status NONE) and its absence
  is not a blocker.
- **Requirements record:** `docs/features/active/quickfiler-home-controller-metrics-442/issue.md`.
- **Design evidence:** `docs/features/active/quickfiler-home-controller-metrics-442/research/quickfiler-home-controller-metrics.research.2026-08-24T10-00.md`.
- **Policy:** `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`,
  `.claude/rules/plan-acceptance-gates.md`.

## Evidence location override rejections (non-overridable clause)

`spec.md` names two non-canonical evidence directories. Both are replaced with the canonical
`evidence/<kind>/` scheme from `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, and the
substitution is recorded here rather than silently applied:

- `EVIDENCE_LOCATION_OVERRIDE_REJECTED: docs/features/active/quickfiler-home-controller-metrics-442/evidence/regression/ replaced with docs/features/active/quickfiler-home-controller-metrics-442/evidence/regression-testing/`
- `EVIDENCE_LOCATION_OVERRIDE_REJECTED: docs/features/active/quickfiler-home-controller-metrics-442/evidence/coverage/ replaced with docs/features/active/quickfiler-home-controller-metrics-442/evidence/qa-gates/`

No evidence artifact may be written under `artifacts/`. No helper script (`.ps1`, `.py`, `.psm1`)
may be created under any `evidence/` directory; evidence directories hold Markdown artifacts and
tool-emitted result files only.

## Path and command conventions

- `FF` denotes `docs/features/active/quickfiler-home-controller-metrics-442`.
- `WS` denotes the repository root of the execution worktree. It is never a literal path written into
  this plan. The executor resolves it once, at the start of Phase 0, by running
  `git rev-parse --show-toplevel` from anywhere inside the worktree, and uses that value for `WS`
  everywhere below. This plan was prepared in a preparation-mode agent worktree that is not the
  worktree the plan will execute in, so any hard-coded root would be stale on arrival; resolving `WS`
  dynamically is the only correct behavior. Every command runs with the current directory set to
  `WS`. No absolute host path, user-account name, or machine name may be written into any plan task
  or any evidence artifact; use `WS`, `<repo-root>`, `<user-profile>`, `<user>`, and `<host>`.
- `TS` denotes the ISO-8601 timestamp `yyyy-MM-ddTHH-mm` captured at the moment the task runs.
  Substitute it into every evidence filename.
- `BASELINE_SHA` denotes the commit SHA recorded by task P0-T2.
- `MSBUILD` and `VSTEST` denote the absolute executable paths recorded by task P0-T3. Every msbuild
  and vstest task below resolves them inline through `vswhere.exe` so no manual substitution is
  needed.
- msbuild and vstest are invoked through `pwsh -NoProfile`. The payload is single-quoted and inner
  literals are double-quoted, so no variable expands in the parent shell. The Bash tool must not be
  used for msbuild or vstest: it mangles `/m` into `M:/` and produces MSB1008.
- Every evidence artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

## Ownership constraints (hard)

Owned production files, the only production files that may be written:

- `QuickFiler/Controllers/QfcHomeController.cs`
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs`
- `QuickFiler/Controllers/EfcHomeController.cs`
- `QuickFiler/Controllers/EfcHomeController.Metrics.cs`
- `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs`

Owned test files, the only test files that may be written:

- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`
- `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`

Forbidden to write (reading is expected): `QuickFiler/Controllers/QfcHomeController.Iteration.cs`,
`QuickFiler/Controllers/QfcCollectionController.cs`,
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs`,
`QuickFiler/Controllers/EfcFormController.cs`, `QuickFiler/QuickFiler.csproj`,
`QuickFiler.Test/QuickFiler.Test.csproj`, `QuickFiler/Interfaces/IFilerHomeController.cs`,
`QuickFiler/Controllers/IQfcHomeController.cs`,
`QuickFiler/Controllers/EfcHomeControllerDependencies.cs`,
`QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`.

**No new `.cs` file may be created.** Both projects are legacy non-SDK projects with explicit
`Compile Include` entries and both project files are unowned. The two owned test files are already
registered at `QuickFiler.Test/QuickFiler.Test.csproj:110` and `:133`.

## Settled design decisions (do not relitigate)

1. `QuickFileMetrics_WRITE(string filename)` is mandated by `QuickFiler/Interfaces/IFilerHomeController.cs:41`.
   It is implemented as guarded delegation. Removal is not an option.
2. The #442 fix replaces the `BlockingCollection`, `_metricsConsumers`, the never-started
   `System.Timers.Timer`, `TimedConsumerAsync`, `NonBlockingProducer`, and the unread static
   `_fileName` with a direct awaited append through the injectable `MetricsFileWriter` seam.
3. The writer receives `CancellationToken.None`, never `Token`, because the dispatcher continuation
   at `QfcFormController.EventHandlers.cs:228-231` is not awaited to completion.
4. The flush invariant is a happens-before condition: the writer Task completes before the Task
   returned by `WriteMetricsAsync` completes, with nothing deferred to a timer, a background
   consumer, or `Cleanup()`. It completes before `Globals` is nulled at `QfcHomeController.cs:391`.
5. #443 end-of-database path: `QfcHomeController.Metrics.cs:121` currently reads
   `Duration = StopWatch.Elapsed.Seconds;`, and `_stopWatchMoved` appears only on the commented-out
   line 120; task P4-T7 redirects line 121 to `_stopWatchMoved.Elapsed.TotalSeconds` and deletes
   the comment. The `MoveAndIterate` race is knowingly out of scope (CFN-1, feature 446).
6. `QfcCollectionController.cs:2284` trailing-null gets an owned-file mitigation only (null and
   whitespace filtering before the write). The root fix is CFN-2 for feature 468.
7. Culture invariance is in scope. The `"hh:mm"` 12-hour defect is out of scope (CFN-4).
8. Four tests break deliberately and are updated in the same change:
   `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine`,
   `QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract`,
   `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`, and
   `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay`.
9. No new time seam. Determinism comes from the existing EFC parameter seam and from
   reflection-injected stopwatches plus `StopWatch.IsRunning` on the QFC side. No wall-clock wait,
   no `Thread.Sleep`, no `Task.Delay` anywhere in test code.
10. `QuickFiler/Controllers/QfcHomeController.cs` is 487 lines against the 500-line cap. The design
    deletes roughly 33 lines, taking it to about 454. The cap is resolved by the design, not worked
    around.

## Planner-identified gate correction (must be executed)

`QuickFiler/Controllers/QfcHomeController.Metrics.cs:120` is the commented-out line
`//Duration = _stopWatchMoved.Elapsed.Seconds;`. AC-7 asserts that a search for `Elapsed.Seconds`
under `QuickFiler/Controllers/` returns no match. That gate is unsatisfiable unless the comment on
line 120 is deleted along with the live read on line 121. Task P4-T7 deletes both.

## Literals asserted by this plan

These literals are quoted here, outside every command span, because the plan instructs the executor
to create or remove them and the acceptance-gate rules exonerate a literal the plan quotes verbatim:

`_stopWatchMoved.Elapsed.TotalSeconds`, `OlEndTime.Subtract`, `Elapsed.Seconds`, `Stopwatch.StartNew`,
`int elapsedSeconds`, `NotImplementedException`, `RecipientSender`, `NonBlockingProducer`,
`TimedConsumerAsync`, `_metricsConsumers`, `_lockObject`, `_fileName`, `volatile`,
`MetricsFileWriter`, `CultureInfo.InvariantCulture`, `,Recipient,Sender,`, `,2.00,`, `,3,0.04,`,
`,90,1.50,`, `Thread.Sleep`, `Task.Delay`.

Test method names asserted by this plan:
`BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine`,
`BuildQuickFileMetricLines_RendersTwelveCommaSeparatedFields`,
`BuildQuickFileMetricLines_WithEmbeddedCommas_StillRendersTwelveFields`,
`BuildQuickFileMetricLines_WithMultipleMovedItems_PinsRealDivisionRounding`,
`BuildQuickFileMetricLines_WithNinetySeconds_RendersUntruncatedDuration`,
`BuildQuickFileMetricLines_UnderGermanCulture_RendersInvariantDecimalSeparator`,
`StopWatch_AfterControllerConstruction_IsRunning`,
`QuickFileMetricsWriteFilenameOnly_WithAbsentPrerequisites_DoesNotThrow`,
`QuickFileMetricsWriteFilenameOnly_WithPrerequisites_DelegatesToThreeArgumentOverload`,
`QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract`,
`TryBeginExecuteMoves_SecondCallBeforeReset_ReturnsFalse`,
`TryBeginExecuteMoves_AfterResetExecuteMovesState_ReturnsTrue`,
`WriteMetricsAsync_ReadsMovedStopwatchForDuration`,
`WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator`,
`WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`,
`WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce`,
`WriteMetricsAsync_CompletesWriterTaskBeforeReturning`,
`WriteMetricsAsync_PassesUncancelledTokenToWriter`,
`WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`,
`WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter`,
`NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay`.

---

### Phase 0 — Baseline capture and environment bootstrap

- [x] [P0-T1] Read, in the `policy-compliance-order` sequence, `CLAUDE.md`,
      `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`,
      `.claude/rules/csharp.md`, `.claude/rules/plan-acceptance-gates.md`,
      `.claude/rules/quality-tiers.md`, and `.claude/rules/tonality.md`. The C#-specific rule file
      is mandatory because every file this plan touches is C#. Write
      `FF/evidence/baseline/phase0-instructions-read.TS.md`
      carrying `Timestamp:`, `Policy Order:` (the sequence above, in order), and an explicit list of
      every file read with its repository-relative path. Acceptance: the artifact exists and its
      file list contains all seven paths.
- [x] [P0-T2] Record the git baseline and resolve `WS`. Run `git rev-parse --show-toplevel`,
      `git rev-parse HEAD`, `git rev-parse --abbrev-ref HEAD`, and `git status --porcelain`. The
      value returned by `git rev-parse --show-toplevel` is `WS` for every remaining task in this
      plan; do not read `WS` from any literal path written in this document. Write
      `FF/evidence/baseline/git-baseline.TS.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`,
      `Output Summary:`, plus an explicit `BASELINE_SHA: ` line holding the full 40-character HEAD
      SHA, a `BASELINE_BRANCH: ` line, and a `WS: ` line holding the literal redacted token
      `<repo-root>` rather than the resolved absolute path. The resolved absolute root is used
      in-session only and must never be written into this or any other committed artifact, because
      it carries the host user-account name. Acceptance: the artifact contains a 40-character
      `BASELINE_SHA:` value, its `WS:` line is exactly `<repo-root>`, and a case-insensitive search
      of the artifact for `Users` returns no match.
- [x] [P0-T3] Probe the toolchain. Resolve `msbuild.exe` and `vstest.console.exe` through
      `C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe`, and probe for
      `dotnet`, `dotnet-coverage`, `nuget`, and `gh` with `Get-Command`. Write
      `FF/evidence/baseline/toolchain-probe.TS.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`,
      `Output Summary:`, and one `MSBUILD:`, `VSTEST:`, `DOTNET:`, `DOTNET_COVERAGE:`, `NUGET:`,
      `GH:` line each holding either a resolved path or the literal `NOT_FOUND`. Any resolved path
      that falls under the host user profile must be written with its user-profile prefix replaced by
      the literal token `<user-profile>` (for example a repo-local SDK at
      `<user-profile>/.dotnet-sdk/dotnet.exe`); paths under `C:\Program Files` and
      `C:\Program Files (x86)` are machine paths and are recorded verbatim. Acceptance: the artifact
      exists, each of the six lines holds a path or `NOT_FOUND`, and a case-insensitive search of the
      artifact for `Users` returns no match. A `NOT_FOUND` value is recorded as a blocker for the
      tasks that consume it and execution continues; it is never a reason to halt the plan or to
      substitute a different route.
- [x] [P0-T4] Restore the CSharpier manifest tool. Run
      `pwsh -NoProfile -Command 'dotnet tool restore; Write-Host "EXIT_CODE=$LASTEXITCODE"'` from
      `WS`. The manifest is at the repository root file `dotnet-tools.json`, not under `.config/`.
      Write `FF/evidence/baseline/dotnet-tool-restore.TS.md`. Acceptance: `EXIT_CODE: 0` and
      `dotnet tool run csharpier --version` reports `1.2.6`.
- [x] [P0-T5] Restore NuGet packages for the solution. Run
      `pwsh -NoProfile -Command 'nuget restore "TaskMaster.sln"; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
      from `WS`. This worktree has no `packages/` directory, so every later msbuild and vstest task
      fails without this step. Write `FF/evidence/baseline/nuget-restore.TS.md`. Acceptance:
      `EXIT_CODE: 0` and the directory `packages` exists at `WS`.
- [x] [P0-T6] Capture the formatting baseline. Run
      `pwsh -NoProfile -Command 'dotnet tool run csharpier check .; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
      from `WS`. Write `FF/evidence/baseline/csharpier-check.TS.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:`, `Output Summary:` recording the number of files reported unformatted.
      Acceptance: the artifact exists and records a numeric unformatted-file count.
- [x] [P0-T7] Capture the analyzer baseline. Run
      `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
      from `WS`. `/t:Rebuild` is mandatory; a warm `/t:Build` skips `CoreCompile` and runs no
      analyzers. Do not add `/p:Nullable=enable`. Write
      `FF/evidence/baseline/msbuild-analyzers.TS.md` with the error and warning counts in
      `Output Summary:`. Acceptance: the artifact records numeric error and warning counts.
- [x] [P0-T8] Capture the nullable/type-check baseline. Run the same resolved msbuild with
      `"TaskMaster.sln" /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
      from `WS`. Do not add `/p:Nullable=enable`. Write
      `FF/evidence/baseline/msbuild-nullable.TS.md` with the error count in `Output Summary:`.
      Acceptance: the artifact records a numeric error count.
- [x] [P0-T9] Capture the coverage baseline. Run
      `pwsh -NoProfile -File "scripts\vscode\Invoke-MSTestWithCoverage.ps1" -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml"`
      from `WS`. The run takes roughly 20 minutes; allow a timeout of at least 45 minutes. The
      runner raises two distinct run-related throws.
      `scripts/vscode/Invoke-MSTestWithCoverage.ps1:236` fires when the underlying
      dotnet-coverage/vstest process exits non-zero (tests failed or tooling failed).
      `scripts/vscode/Invoke-MSTestWithCoverage.ps1:341` calls
      `Assert-CoberturaLineCoverageThreshold`
      (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:459`), which throws when the
      repository-wide line-rate is under 80 percent. That second throw fires after
      `ConvertTo-KoverageCoberturaXml` at `:340` but before the `Set-Content` at `:344`, so when
      it fires, the Cobertura file left on disk is the raw dotnet-coverage output with absolute
      paths and third-party packages still present, not the post-processed form. When that occurs,
      read per-file line-rates by matching the raw absolute `filename` attributes for the five
      owned production files, and state in the artifact that the file on disk is
      un-post-processed. A non-zero exit caused by failing tests, and a non-zero exit caused by
      the 80 percent floor, are both baseline observations to record; only a run the tooling could
      not complete at all (vstest crashed, or no Cobertura file was produced) is a blocker. Write
      `FF/evidence/baseline/mstest-coverage.TS.md` with `Timestamp:`, `Command:`, `EXIT_CODE:` (as
      observed), and an `Output Summary:` recording: the passed and failed test counts, the
      repository-wide `line-rate` and `branch-rate` read from the root `coverage` element of
      `coverage\coverage.cobertura.xml` expressed as percentages to two decimals, the per-file
      line-rate for each of the five owned production files, aggregated across every `class` element
      sharing the same `filename` attribute so that compiler-generated async and lambda classes are
      not counted as separate denominators, and a `RunDisposition:` line reading exactly one of
      `CLEAN`, `TESTS_FAILED` (with the failed-test count), `COVERAGE_FLOOR_TRIPPED` (with the
      reported percentage), or `TOOLING_FAILURE`. Acceptance: the
      artifact records a numeric repository-wide line-rate percentage, five numeric per-file
      line-rate percentages, and a `RunDisposition:` line; a `TOOLING_FAILURE` disposition is a
      blocker requiring investigation, and this task is not complete until the recorded disposition
      is `CLEAN`, `TESTS_FAILED`, or `COVERAGE_FLOOR_TRIPPED`.
- [x] [P0-T10] Record the pre-change line count of each of the seven owned files (five production,
      two test) into `FF/evidence/baseline/owned-file-line-counts.TS.md` with `Timestamp:`,
      `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: the artifact lists seven paths, each
      with a numeric line count.
- [x] [P0-T11] Record the pre-fix defect-site census. Run `git grep -n` for each of these patterns
      and record every hit with file and line: `Elapsed.Seconds` under `QuickFiler/Controllers/`;
      `int elapsedSeconds` under `QuickFiler/`; `NotImplementedException` in
      `QuickFiler/Controllers/EfcHomeController.Metrics.cs`; `volatile` in
      `QuickFiler/Controllers/EfcHomeController.cs`; `Stopwatch.StartNew` in
      `QuickFiler/Controllers/EfcHomeController.cs`; `RecipientSender` under `QuickFiler.Test/`;
      and the alternation `NonBlockingProducer|TimedConsumerAsync|_metricsConsumers|_lockObject|_fileName`
      under `QuickFiler/Controllers/`. Write `FF/evidence/baseline/defect-site-census.TS.md`.
      Acceptance: the artifact records a non-zero hit count for `Elapsed.Seconds`, for
      `int elapsedSeconds`, for `NotImplementedException`, for `volatile`, for `RecipientSender`,
      and for the five-way alternation, and exactly one hit for `Stopwatch.StartNew`, at
      `QuickFiler/Controllers/EfcHomeController.cs:176` (the pre-existing `selectionStopwatch`
      call, unrelated to `_stopWatch`). This artifact
      is the pre-fix half of the grep-based acceptance criteria AC-7, AC-10, AC-12, AC-14, and
      AC-15.
- [x] [P0-T12] Resolve the two unverified points carried from research. Read
      `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` in full and confirm that
      `QuickFileMetrics_WRITE_WithNullList_SkipsBodyAndDoesNotThrow` and
      `QuickFileMetrics_WRITE_WithEmptyList_SkipsBodyAndDoesNotThrow` return through the
      `moved is null || moved.Count == 0` guard at `QuickFiler/Controllers/EfcHomeController.Metrics.cs:18-21`
      before reaching line 23, and therefore survive the `int` to `double` widening unchanged
      (assumption A-10). Read `QuickFiler/Controllers/QfcHomeController.cs` in full and list every
      member of the partial that consumes a type from `System.Collections.Concurrent` or
      `System.Timers` (assumption A-11). Write
      `FF/evidence/baseline/unverified-points-resolution.TS.md` recording both findings.
      Acceptance: the artifact states, for A-10, whether either test would break, and for A-11,
      the explicit list of consuming members or the statement that the only consumers are the
      members deleted by task P5-T10. That file is not owned; if A-10 resolves to "would break",
      record it as a blocker and stop before Phase 2.

### Phase 1 — EFC metrics regression tests, red state (#451)

- [x] [P1-T1] [expect-fail] In `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`,
      update the expected literal of `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine`
      (declared at line 35, literal at lines 56-60) so the concatenated substring `RecipientSender`
      is replaced by the separated substring `,Recipient,Sender,` and the expected line carries 12
      comma-separated fields. Acceptance: the file compiles and the literal
      `RecipientSender` no longer appears in that test method.
- [x] [P1-T2] [expect-fail] Add `BuildQuickFileMetricLines_RendersTwelveCommaSeparatedFields` to
      `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`. It invokes
      `BuildQuickFileMetricLines` with one moved item and asserts the single rendered line splits on
      the comma character into exactly 12 fields. Acceptance: the method exists, compiles, and is
      decorated `[TestMethod]`.
- [x] [P1-T3] [expect-fail] Add `BuildQuickFileMetricLines_WithEmbeddedCommas_StillRendersTwelveFields`
      to the same file. It embeds a comma in each of `ToRecipientsName`, `SenderName`, and
      `selectedFolder` and asserts the rendered line still splits into exactly 12 fields.
      Acceptance: the method exists, compiles, and is decorated `[TestMethod]`.
- [x] [P1-T4] [expect-fail] Add `BuildQuickFileMetricLines_WithMultipleMovedItems_PinsRealDivisionRounding`
      to the same file. It invokes `BuildQuickFileMetricLines` with `elapsedSeconds` of 8 and three
      moved items and asserts each rendered line contains the substring `,3,0.04,`. Under the
      current integer division `8 / 3` yields 2 and the line contains `,2,0.03,`; under real
      division it yields 2.6666… which renders as `3` and `0.04`. Acceptance: the method exists,
      compiles, and is decorated `[TestMethod]`.
- [x] [P1-T5] Add `BuildQuickFileMetricLines_WithNinetySeconds_RendersUntruncatedDuration` to the
      same file. It invokes `BuildQuickFileMetricLines` with `elapsedSeconds` of 90 and one moved
      item and asserts the rendered line contains the substring `,90,1.50,`. This test is a
      deliberate pin, not a regression: it passes both before and after the fix, because the 0-59
      truncation defect lives at `QuickFiler/Controllers/EfcHomeController.Metrics.cs:23` where the
      `TimeSpan` component is read, not inside `BuildQuickFileMetricLines`. The falsifiable half of
      AC-7 is carried by the `Elapsed.Seconds` search gate in task P2-T10. Acceptance: the method
      exists, compiles, is decorated `[TestMethod]`, and passes on the pre-fix source.
- [x] [P1-T6] [expect-fail] Add `BuildQuickFileMetricLines_UnderGermanCulture_RendersInvariantDecimalSeparator`
      to the same file. It sets `CultureInfo.CurrentCulture` to `de-DE` inside a `try` block whose
      `finally` restores the original culture, invokes `BuildQuickFileMetricLines` with
      `elapsedSeconds` of 120 and one moved item, and asserts the rendered line contains the
      substring `,2.00,` and splits into exactly 12 fields. Under `de-DE` the pre-fix source renders
      `,2,00,`. Acceptance: the method exists, compiles, is decorated `[TestMethod]`, and its
      `finally` block restores `CultureInfo.CurrentCulture`.
- [x] [P1-T7] [expect-fail] Add `StopWatch_AfterControllerConstruction_IsRunning` to the same file.
      It builds a controller through the existing `CreateController` helper and asserts
      `StopWatch.IsRunning` is `true`, following the `QfcHomeControllerRunAsyncTests.cs:303`
      precedent. If the helper's `EfcDataModel` (built through
      `FormatterServices.GetUninitializedObject` with `Mail` set to `null`) cannot reach the
      construction site at `QuickFiler/Controllers/EfcHomeController.cs:76`, supply a data model
      whose `Mail` is non-null for this one test. If that is not achievable without touching an
      unowned file, assert the `InitAsync` site at line 225 instead and record the limitation in
      task P7-T2. Acceptance: the method exists, compiles, is decorated `[TestMethod]`, and asserts
      `IsRunning`.
- [x] [P1-T8] [expect-fail] Delete `QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract`
      (lines 138-148) from the same file and add
      `QuickFileMetricsWriteFilenameOnly_WithAbsentPrerequisites_DoesNotThrow` in its place. The new
      test invokes the single-argument `QuickFileMetrics_WRITE` on a controller whose
      `_formController`, `DataModel`, or `DataModel.Mail` is absent and asserts the call does not
      throw. Acceptance: the deleted method name no longer appears in the file and the new method
      exists, compiles, and is decorated `[TestMethod]`.
- [x] [P1-T9] [expect-fail] Add `QuickFileMetricsWriteFilenameOnly_WithPrerequisites_DelegatesToThreeArgumentOverload`
      to the same file. It supplies the prerequisites and asserts the injected
      `metricsLineWriter` seam is invoked, proving delegation to the three-argument overload.
      Acceptance: the method exists, compiles, and is decorated `[TestMethod]`.
- [x] [P1-T10] [expect-fail] Build and run the EFC scoped suite to record the red state. Run
      `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"; $vt = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vt "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcHomeControllerMetricsTests" /Logger:trx "/ResultsDirectory:TestResults\p1-t10"; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
      from `WS`. This is a build-for-test step, not the analyzer gate; the analyzer gate uses
      `/t:Rebuild` and runs in Phase 6. Write
      `FF/evidence/regression-testing/efc-metrics-red.TS.md` with `Timestamp:`, `Command:`,
      `EXIT_CODE:`, `ExpectedExitCode: 1`, and an `Output Summary:` that names each failing test and
      quotes its verbatim failure message. Acceptance: the artifact records the eight tests from
      P1-T1, P1-T2, P1-T3, P1-T4, P1-T6, P1-T7, P1-T8, and P1-T9 as failing, records
      `BuildQuickFileMetricLines_WithNinetySeconds_RendersUntruncatedDuration` as passing, and the
      TRX file exists under `TestResults\p1-t10`.

### Phase 2 — EFC metrics minimal fixes (#451)

- [x] [P2-T1] Add `using System.Globalization;` to
      `QuickFiler/Controllers/EfcHomeController.Metrics.cs` in the existing using block.
      Acceptance: the directive is present and the solution compiles.
- [x] [P2-T2] In `QuickFiler/Controllers/EfcHomeController.cs`, replace the two-statement
      construct-then-nothing pattern at lines 76 and 225 with the single form
      `_stopWatch = Stopwatch.StartNew();`. `System.Diagnostics` is already imported at line 3.
      Acceptance: a search for `Stopwatch.StartNew` in that file returns exactly three hits, at
      lines 76, 176, and 225 — line 176 is the pre-existing `selectionStopwatch` call, unrelated to
      `_stopWatch` — and the solution compiles.
- [x] [P2-T3] In `QuickFiler/Controllers/EfcHomeController.Metrics.cs` line 23, change the argument
      from the 0-59 `Seconds` component to `TotalSeconds`. Acceptance: a search for
      `Elapsed.Seconds` in that file returns no match and the solution compiles.
- [x] [P2-T4] Widen the two `int elapsedSeconds` parameters at
      `QuickFiler/Controllers/EfcHomeController.Metrics.cs:35` and `:57` to `double elapsedSeconds`.
      Both members are `internal`, so no public API breaks. Acceptance: a search for
      `int elapsedSeconds` under `QuickFiler/` returns no match and the solution compiles.
- [x] [P2-T5] Insert the missing comma field separator between the interpolated
      `ToRecipientsName` at the end of `QuickFiler/Controllers/EfcHomeController.Metrics.cs:80` and
      the interpolated `SenderName` at the start of line 81. Acceptance:
      `BuildQuickFileMetricLines_RendersTwelveCommaSeparatedFields` passes.
- [x] [P2-T6] Wrap `ToRecipientsName`, `SenderName`, and `selectedFolder` in
      `QfcCollectionController.xComma(...)` in the same interpolated block, so all four free-text
      fields are sanitized as the QFC writer already does. `xComma` is `public static` and is
      already called from line 79, so no unowned file is written. Acceptance:
      `BuildQuickFileMetricLines_WithEmbeddedCommas_StillRendersTwelveFields` passes.
- [x] [P2-T7] Pass `CultureInfo.InvariantCulture` to the two numeric format calls at
      `QuickFiler/Controllers/EfcHomeController.Metrics.cs:73` and `:74`. Do not change the date or
      time format calls. Acceptance:
      `BuildQuickFileMetricLines_UnderGermanCulture_RendersInvariantDecimalSeparator` passes.
- [x] [P2-T8] Implement `QuickFileMetrics_WRITE(string filename)` at
      `QuickFiler/Controllers/EfcHomeController.Metrics.cs:26-29` as guarded delegation to the
      three-argument overload. Derive `selectedFolder` from `_formController.SelectedFolder` and
      `moved` from the owned `internal static SelectMoveMetricsItems(...)` at
      `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:111`, mirroring
      `ExecuteMovesCoreAsync` at `ExecuteMoves.cs:66-72`. Return early without throwing when
      `_formController`, `DataModel`, or `DataModel.Mail` is absent, following the existing silent
      no-op precedent at `EfcHomeController.Metrics.cs:18-21`. Do not add a seam and do not change
      the signature; `QuickFiler/Interfaces/IFilerHomeController.cs:41` mandates it. Acceptance: a
      search for `NotImplementedException` in
      `QuickFiler/Controllers/EfcHomeController.Metrics.cs` returns no match and both tests from
      P1-T8 and P1-T9 pass.
- [x] [P2-T9] Build and run the EFC scoped suite green. Use the same command as P1-T10 with
      `"/ResultsDirectory:TestResults\p2-t9"`. Write `FF/evidence/regression-testing/efc-metrics-green.TS.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` recording the passed,
      failed, and skipped counts. Acceptance: `EXIT_CODE: 0`, zero failed, zero skipped, and every
      test named in P1-T1 through P1-T9 passing.
- [x] [P2-T10] Record the EFC post-fix search census. Run `git grep -n` for `Elapsed.Seconds` under
      `QuickFiler/Controllers/EfcHomeController.Metrics.cs`, for `int elapsedSeconds` under
      `QuickFiler/`, for `NotImplementedException` in
      `QuickFiler/Controllers/EfcHomeController.Metrics.cs`, for `RecipientSender` under
      `QuickFiler.Test/`, and for `Stopwatch.StartNew` in
      `QuickFiler/Controllers/EfcHomeController.cs`. Write
      `FF/evidence/qa-gates/efc-search-census.TS.md`. Acceptance: the first four searches each
      return zero hits and the fifth returns exactly three hits, at lines 76, 176, and 225 (line
      176 being the pre-existing `selectionStopwatch` call, unrelated to `_stopWatch`).

### Phase 3 — EFC re-entrancy guard (#451 defect 3)

- [x] [P3-T1] Add `TryBeginExecuteMoves_SecondCallBeforeReset_ReturnsFalse` to
      `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`. It calls
      `TryBeginExecuteMoves()` twice in sequence on one controller and asserts the first returns
      `true` and the second returns `false`. The assertion is deliberately sequential; a genuinely
      concurrent assertion on a compare-and-swap is not deterministic and must not be attempted.
      Acceptance: the method exists, compiles, and is decorated `[TestMethod]`.
- [x] [P3-T2] Add `TryBeginExecuteMoves_AfterResetExecuteMovesState_ReturnsTrue` to the same file.
      It calls `TryBeginExecuteMoves()`, then `ResetExecuteMovesState()`, then
      `TryBeginExecuteMoves()` again, and asserts the third call returns `true`. Acceptance: the
      method exists, compiles, and is decorated `[TestMethod]`.
- [x] [P3-T3] Run both re-entrancy tests against the pre-change `volatile` primitive using the
      P1-T10 command shape with `"/TestCaseFilter:FullyQualifiedName~TryBeginExecuteMoves"` and
      `"/ResultsDirectory:TestResults\p3-t3"`. Write
      `FF/evidence/regression-testing/efc-reentrancy-pin.TS.md`. Acceptance: `EXIT_CODE: 0` and
      both tests recorded as passing. They pass before the change by design; they exist to pin the
      observable contract across the primitive swap.
- [x] [P3-T4] Write the fail-before exception dossier
      `FF/evidence/regression-testing/fail-before-exception.TS.md` for root cause RC-6. It must
      carry `Timestamp:`, `WhyFailingRunImpossible:` explaining that the non-atomic read-then-write
      at `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:48-57` is only observable under a
      genuine data race, which `.claude/rules/general-unit-test.md` forbids reproducing
      deterministically, and an alternative-proof section citing the source form before and after
      the change plus the two pinning tests from P3-T1 and P3-T2. Acceptance: the artifact exists
      and carries a non-empty `WhyFailingRunImpossible:` value.
- [x] [P3-T5] Change `_isExecuting` at `QuickFiler/Controllers/EfcHomeController.cs:389` from
      `private volatile bool` to `private int`, rewrite `TryBeginExecuteMoves` at
      `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:48-57` to return the result of an
      `Interlocked.CompareExchange` against the field, and rewrite `ResetExecuteMovesState` at
      `:59-62` as `Interlocked.Exchange(ref _isExecuting, 0)`. Acceptance: a search for `volatile`
      in `QuickFiler/Controllers/EfcHomeController.cs` returns no match and the solution compiles.
- [x] [P3-T6] Re-run the two re-entrancy tests after the primitive change using the P3-T3 command
      shape with `"/ResultsDirectory:TestResults\p3-t6"`. Write
      `FF/evidence/regression-testing/efc-reentrancy-green.TS.md`. Acceptance: `EXIT_CODE: 0` and
      both tests recorded as passing.

### Phase 4 — QFC stopwatch, duration, and culture (#443)

- [x] [P4-T1] [expect-fail] Add `WriteMetricsAsync_ReadsMovedStopwatchForDuration` to
      `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`. Build the controller through
      the existing `BuildLooseMetricsController()` helper, set `_stopWatchMoved` by reflection to a
      stopped stopwatch whose internal elapsed-tick field is itself set by reflection to a fixed,
      explicit non-zero value — the deterministic construction that P7-T3 names — never by
      start/stop wall-clock timing, which does not guarantee a non-zero elapsed value, set
      `_stopWatch` by reflection to a freshly constructed stopwatch, await `WriteMetricsAsync`, and
      verify the
      mocked `GetMoveDiagnostics` was called with a `duration` argument matched by
      `It.Is<double>(d => d > 0)`. The pre-fix source reads the fresh stopwatch and passes zero.
      Acceptance: the method exists, compiles, and is decorated `[TestMethod]`.
- [x] [P4-T2] [expect-fail] Add `WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator`
      to the same file. It sets `CultureInfo.CurrentCulture` to `de-DE` inside a `try` whose
      `finally` restores the original culture, sets `_stopWatchMoved` by reflection, awaits
      `WriteMetricsAsync`, and asserts the `durationMinutesText` argument captured from the mocked
      `GetMoveDiagnostics` contains no comma character. The assertion is independent of the actual
      elapsed value, so it is deterministic without any clock read. Acceptance: the method exists,
      compiles, is decorated `[TestMethod]`, and its `finally` restores `CultureInfo.CurrentCulture`.
- [x] [P4-T3] Update `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` (declared at line
      328, currently setting only `_stopWatch` at line 332) so it also sets `_stopWatchMoved` by
      reflection. Without this the test dereferences a null field once line 121 reads the moved
      stopwatch. Record the disposition in task P7-T1. Acceptance: the method sets `_stopWatchMoved`
      and compiles.
- [x] [P4-T4] [expect-fail] Build and run the QFC scoped suite to record the red state. Use the
      P1-T10 command shape with
      `"/TestCaseFilter:FullyQualifiedName~QfcHomeControllerMetricsTests"` and
      `"/ResultsDirectory:TestResults\p4-t4"`. Write
      `FF/evidence/regression-testing/qfc-stopwatch-red.TS.md` with `ExpectedExitCode: 1` and an
      `Output Summary:` naming each failing test with its verbatim failure message. Acceptance: the
      artifact records `WriteMetricsAsync_ReadsMovedStopwatchForDuration` and
      `WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator` as failing, and the
      TRX file exists under `TestResults\p4-t4`.
- [x] [P4-T5] Add `using System.Globalization;` to
      `QuickFiler/Controllers/QfcHomeController.Metrics.cs`. Acceptance: the directive is present
      and the solution compiles.
- [x] [P4-T6] Change `QuickFiler/Controllers/QfcHomeController.Metrics.cs:42` to read
      `_stopWatchMoved.Elapsed.TotalSeconds`. Acceptance: the line reads the total-seconds form and
      the solution compiles.
- [x] [P4-T7] Change `QuickFiler/Controllers/QfcHomeController.Metrics.cs:121` to read
      `_stopWatchMoved.Elapsed.TotalSeconds`, and delete the commented-out line 120 which reads
      `//Duration = _stopWatchMoved.Elapsed.Seconds;`. Deleting the comment is required: AC-7
      asserts a search for `Elapsed.Seconds` under `QuickFiler/Controllers/` returns no match, and a
      commented occurrence is still a match. Acceptance: a search for `Elapsed.Seconds` under
      `QuickFiler/Controllers/` returns no match and
      `WriteMetricsAsync_ReadsMovedStopwatchForDuration` passes.
- [x] [P4-T8] Change `QuickFiler/Controllers/QfcHomeController.Metrics.cs:123` to
      `OlStartTime = OlEndTime.Subtract(_stopWatchMoved.Elapsed);`, removing the reconstruction from
      the truncated integer cast. The read must remain positioned before the
      `Duration /= emailsLoaded` division at line 129, which the current line position already
      satisfies. Acceptance: a search for `OlEndTime.Subtract` in that file returns exactly one hit
      whose text contains `_stopWatchMoved.Elapsed`, and a search for the cast form `(int)Duration`
      in that file returns no match.
- [x] [P4-T9] Pass `CultureInfo.InvariantCulture` to the four numeric format calls at
      `QuickFiler/Controllers/QfcHomeController.Metrics.cs:53`, `:56`, `:132`, and `:135`. Do not
      change the date or time format calls; the `"hh:mm"` defect is CFN-4 and out of scope.
      Acceptance: `WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator` passes.
- [x] [P4-T10] Build and run the QFC scoped suite green. Use the P4-T4 command shape with
      `"/ResultsDirectory:TestResults\p4-t10"`. Write
      `FF/evidence/regression-testing/qfc-stopwatch-green.TS.md`. Acceptance: `EXIT_CODE: 0`, zero
      failed, zero skipped, and both tests from P4-T1 and P4-T2 plus the updated test from P4-T3
      recorded as passing.
- [x] [P4-T11] Record the QFC #443 post-fix search census. Run `git grep -n` for `Elapsed.Seconds`
      under `QuickFiler/Controllers/` and for `OlEndTime.Subtract` in
      `QuickFiler/Controllers/QfcHomeController.Metrics.cs`. Write
      `FF/evidence/qa-gates/qfc-stopwatch-search-census.TS.md`. Acceptance: the first search returns
      zero hits and the second returns exactly one hit containing `_stopWatchMoved.Elapsed`.

### Phase 5 — QFC metrics flush redesign (#442)

- [x] [P5-T1] Declare the writer seam in `QuickFiler/Controllers/QfcHomeController.Metrics.cs` as
      `internal Func<string, string[], string, CancellationToken, Task> MetricsFileWriter { get; set; } = FileIO2.WriteTextFileAsync;`
      with an XML doc comment recording that it mirrors the EFC precedent at
      `EfcHomeControllerDependencies.cs:78`. Parameter order is filename, lines, folder root,
      cancellation token. Do not yet change `WriteMetricsAsync`; this task only makes the seam
      exist so the Phase 5 regression tests compile and fail for the right reason. Acceptance: the
      solution compiles and the identifier `MetricsFileWriter` is present in that file.
- [x] [P5-T2] [expect-fail] Add `WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce` to
      `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`. It builds a controller through
      `BuildLooseMetricsController()` whose mocked `GetMoveDiagnostics` returns a known non-empty
      array, assigns a capturing delegate to `MetricsFileWriter` that returns
      `Task.CompletedTask`, awaits `WriteMetricsAsync`, and asserts the capture list holds exactly
      one invocation carrying the supplied filename, the `MyDocuments` folder root, and the expected
      lines. Extend `BuildLooseMetricsController()` with an optional diagnostics-array parameter
      rather than duplicating the helper. Acceptance: the method exists, compiles, and is decorated
      `[TestMethod]`.
- [x] [P5-T3] [expect-fail] Add `WriteMetricsAsync_CompletesWriterTaskBeforeReturning` to the same
      file. Its injected delegate yields once and then sets a boolean flag; the test asserts the
      flag is `true` immediately after awaiting `WriteMetricsAsync`. This is the happens-before half
      of the flush invariant. Use `Task.Yield`, never `Task.Delay` and never `Thread.Sleep`.
      Acceptance: the method exists, compiles, and is decorated `[TestMethod]`.
- [x] [P5-T4] [expect-fail] Add `WriteMetricsAsync_PassesUncancelledTokenToWriter` to the same file.
      It cancels the controller's `TokenSource` before awaiting `WriteMetricsAsync` and asserts the
      captured `CancellationToken` reports `IsCancellationRequested` as `false`. Acceptance: the
      method exists, compiles, and is decorated `[TestMethod]`.
- [x] [P5-T5] [expect-fail] Add `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` to the
      same file. Its mocked `GetMoveDiagnostics` returns an array whose trailing element is `null`,
      mirroring the allocation defect at `QfcCollectionController.cs:2284`, and it asserts the lines
      reaching the writer contain no null and no whitespace-only entry. Acceptance: the method
      exists, compiles, and is decorated `[TestMethod]`.
- [x] [P5-T6] Add `WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter` to the same file.
      It supplies a `SpecialFolders` collection with no `MyDocuments` entry and asserts the writer
      is never invoked, guarding the guard at
      `QuickFiler/Controllers/QfcHomeController.Metrics.cs:114`. This test passes both before and
      after the fix by design. Acceptance: the method exists, compiles, is decorated `[TestMethod]`,
      and passes on the pre-fix source.
- [x] [P5-T7] [expect-fail] Build and run the QFC scoped suite to record the red state. Use the
      P4-T4 command shape with `"/ResultsDirectory:TestResults\p5-t7"`. Write
      `FF/evidence/regression-testing/qfc-flush-red.TS.md` with `ExpectedExitCode: 1` and an
      `Output Summary:` naming each failing test with its verbatim failure message. Acceptance: the
      artifact records the four tests from P5-T2 through P5-T5 as failing with an empty capture
      list, records `WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter` as passing, and
      the TRX file exists under `TestResults\p5-t7`.
- [x] [P5-T8] Replace `QuickFiler/Controllers/QfcHomeController.Metrics.cs:153-154` with a filtered
      awaited write: drop every null and whitespace-only entry from the `GetMoveDiagnostics` result,
      then `await MetricsFileWriter(filename, lines, myDocuments, CancellationToken.None)`. The
      token must be `CancellationToken.None`, never the controller's `Token`, because the dispatcher
      continuation at `QfcFormController.EventHandlers.cs:228-231` is not awaited to completion and a
      session cancellation can be raised while the write is in flight. Acceptance: the four tests
      from P5-T2 through P5-T5 pass and the identifier `_fileName` no longer appears in that file.
- [x] [P5-T9] Delete both `NonBlockingProducer` overloads and the unreachable consumer-scheduling
      block from `QuickFiler/Controllers/QfcHomeController.Metrics.cs:190-232`. Acceptance: a search
      for `NonBlockingProducer` in that file returns no match and the solution compiles.
- [x] [P5-T10] Delete `_metrics`, `_metricsConsumers`, `_lockObject`, and `_fileName` at
      `QuickFiler/Controllers/QfcHomeController.cs:353-358`, and delete `TimedConsumerAsync` at
      `:362-386`. Acceptance: a search for the alternation
      `NonBlockingProducer|TimedConsumerAsync|_metricsConsumers|_lockObject|_fileName` under
      `QuickFiler/Controllers/` returns no match and the solution compiles.
- [x] [P5-T11] Delete `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay` (declared
      at line 401, body at 404-416) from
      `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`. Its body never calls
      `NonBlockingProducer`; it exercises the time provider's delay directly, and after P5-T9 that
      seam has no production call site, so the test would assert only that the fake time provider
      works. Deletion also recovers line budget for the 500-line cap. Record the disposition in task
      P7-T1. Acceptance: the method name no longer appears in the file and the file compiles.
- [x] [P5-T12] Remove the now-unused `using System.Collections.Concurrent;` at
      `QuickFiler/Controllers/QfcHomeController.cs:2` and `using System.Timers;` at `:11`, using the
      A-11 finding recorded by task P0-T12 to confirm no other member of the partial consumes those
      namespaces. Re-evaluate `using System.Linq;` at `:7` the same way and remove it only if the
      analyzer pass reports it unused. The compiler and analyzer are the authority: a removal that
      produces a missing-type error must be reverted in this task, not deferred. Acceptance: the
      solution compiles with zero new errors under the analyzer command from P0-T7.
- [x] [P5-T13] Build and run the QFC scoped suite green. Use the P4-T4 command shape with
      `"/ResultsDirectory:TestResults\p5-t13"`. Write
      `FF/evidence/regression-testing/qfc-flush-green.TS.md`. Acceptance: `EXIT_CODE: 0`, zero
      failed, zero skipped, and every test named in P5-T2 through P5-T6 recorded as passing.
- [x] [P5-T14] Record the #442 post-fix search census. Run `git grep -n` for the alternation
      `NonBlockingProducer|TimedConsumerAsync|_metricsConsumers|_lockObject|_fileName` under
      `QuickFiler/Controllers/`. Write `FF/evidence/qa-gates/qfc-flush-search-census.TS.md`
      recording the pre-fix hit count from the P0-T11 census alongside the post-fix count.
      Acceptance: the post-fix count is zero and the recorded pre-fix count is greater than zero.
- [x] [P5-T15] Measure and, if necessary, compact
      `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` so its line count is at most
      499. The file began at 421 lines with roughly 79 lines of headroom; P5-T11 recovers roughly 25
      more. If the count exceeds 499, shorten the new test methods by reusing
      `BuildLooseMetricsController()` rather than relocating any test into the EFC file, which is
      the wrong home. Fallback disposition: if the count still exceeds 499 after shortening the new
      test methods, record the residual overage in the artifact as a blocker and consolidate the
      duplicated arrange code of the existing tests into `BuildLooseMetricsController()` until the
      count is at most 499; relocating any test into the EFC file and creating a new file both
      remain forbidden, and Phase 6 must not start while the count exceeds 499. Write
      `FF/evidence/qa-gates/qfc-test-file-size.TS.md` recording the measured
      line count. Acceptance: the recorded line count is at most 499.

### Phase 6 — Final QC toolchain loop

The four steps below run in the stated order. If any step fails, or if the formatter modifies any
file, restart this phase from P6-T1. This phase is unconditional: every task executes its stated
command and records its artifact. `EXIT_CODE: SKIPPED` is not a valid outcome for any task in this
phase.

- [x] [P6-T1] Format the changed files. Run
      `pwsh -NoProfile -Command 'dotnet tool run csharpier format "QuickFiler\Controllers\QfcHomeController.cs" "QuickFiler\Controllers\QfcHomeController.Metrics.cs" "QuickFiler\Controllers\EfcHomeController.cs" "QuickFiler\Controllers\EfcHomeController.Metrics.cs" "QuickFiler\Controllers\EfcHomeController.ExecuteMoves.cs" "QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs" "QuickFiler.Test\Controllers\EfcHomeControllerMetricsTests.cs"; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
      from `WS`. The mutating pass is scoped to the seven owned files so it cannot rewrite an
      unowned file and break the ownership gates in Phase 7. Write
      `FF/evidence/qa-gates/csharpier-format.TS.md`. Acceptance: `EXIT_CODE: 0` and the artifact
      records which of the seven files, if any, were rewritten, determined by comparing each file's
      SHA-256 before and after the command rather than by reading the tool's processed-file count.
- [x] [P6-T2] Verify formatting repository-wide, read-only. Run
      `pwsh -NoProfile -Command 'dotnet tool run csharpier check .; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
      from `WS`. Write `FF/evidence/qa-gates/csharpier-check.TS.md`. Acceptance: `EXIT_CODE: 0`.
- [x] [P6-T3] Run the analyzer gate. Use the exact command from P0-T7. Write
      `FF/evidence/qa-gates/msbuild-analyzers.TS.md` recording the error and warning counts.
      Acceptance: `EXIT_CODE: 0` and the error count is zero.
- [x] [P6-T4] Run the nullable/type-check gate. Use the exact command from P0-T8. Write
      `FF/evidence/qa-gates/msbuild-nullable.TS.md` recording the error count. Acceptance:
      `EXIT_CODE: 0` and the error count is zero.
- [x] [P6-T5] Run the full coverage-enabled test suite. Use the exact command from P0-T9 and allow
      a timeout of at least 45 minutes. Write `FF/evidence/qa-gates/mstest-coverage.TS.md` with
      `Timestamp:`, `Command:`, `EXIT_CODE:` as observed, and an `Output Summary:` recording the
      passed, failed, and skipped test counts, the repository-wide `line-rate` and `branch-rate` as
      percentages to two decimals, and the per-file line-rate for each of the five owned production
      files aggregated across every `class` element sharing the same `filename` attribute.
      Acceptance: zero failed tests and the artifact records a numeric repository-wide line-rate and
      five numeric per-file line-rates. Record the same four-member `RunDisposition:` line defined
      by P0-T9. A `COVERAGE_FLOOR_TRIPPED` disposition does not by itself fail this task; the
      zero-failed-tests condition and the numeric-line-rate conditions are the gate.
- [x] [P6-T6] Compute the coverage delta. Write `FF/evidence/qa-gates/coverage-delta.TS.md`
      comparing the P0-T9 baseline against the P6-T5 result: the baseline repository-wide line-rate,
      the post-change repository-wide line-rate, their signed difference, and for each of the five
      owned production files the baseline line-rate, the post-change line-rate, and their signed
      difference. Additionally record the measured line-rate for the newly added or changed members
      named in the spec's Test Strategy: `BuildQuickFileMetricLines`, `SelectMoveMetricsItems`,
      `TryBeginExecuteMoves`, `ResetExecuteMovesState`, the implemented single-argument
      `QuickFileMetrics_WRITE`, and `WriteMetricsAsync`. Per the spec, the repository-wide figure is
      a record-and-report obligation and not a blocking threshold, because no merge-base baseline
      existed at spec time; the change-scoped figures are the gating ones. The repository-wide
      line-rate and its signed difference are recorded values only and carry no pass/fail
      condition; the per-file and per-member figures carry the gate. Acceptance: the artifact
      records every value named above as a number, and each of the six named members reports at
      least 90.00 percent line coverage or carries a named, member-specific justification for why
      the residual lines are unreachable without a live Outlook process. If either the P0-T9 or
      the P6-T5 run recorded `COVERAGE_FLOOR_TRIPPED`, state in the artifact which of the two
      Cobertura documents was un-post-processed and confirm that both sides of every comparison
      were read using the same `filename` form.
- [x] [P6-T7] Audit file sizes after formatting. Record the post-format line count of each of the
      seven owned files into `FF/evidence/qa-gates/owned-file-line-counts.TS.md` alongside the
      P0-T10 pre-change counts. Acceptance: every one of the seven counts is at most 499, and
      `QuickFiler/Controllers/QfcHomeController.cs` is at or below its pre-change count of 487.
- [x] [P6-T8] Audit test determinism. Run `git grep -nE` for the alternation
      `Thread\.Sleep|Task\.Delay|DateTime\.Now|Path\.GetTempPath|GetTempFileName` restricted to
      `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` and
      `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`. Write
      `FF/evidence/qa-gates/test-determinism.TS.md`. Acceptance: the search returns zero hits.
- [x] [P6-T9] Confirm the loop closed in a single clean pass. Write
      `FF/evidence/qa-gates/toolchain-loop.TS.md` recording, in order, the timestamps and exit codes
      of P6-T1 through P6-T5 for the final pass, and the number of files the formatter rewrote in
      that pass. Acceptance: the recorded order is format, then check, then analyzers, then nullable,
      then coverage-enabled test; every recorded exit code for P6-T2, P6-T3, and P6-T4 is zero; the
      formatter rewrote zero files in the final pass; and P6-T5 recorded zero failed tests. If any
      of those conditions does not hold, restart this phase from P6-T1 before completing this task.

### Phase 7 — Closure, ownership gates, and acceptance-criteria check-off

- [x] [P7-T1] Write `FF/evidence/other/pr-body-statements.TS.md` holding the exact statements the
      PR body must carry, for the `pr-author` skill to consume when the epic authors the pull
      request: that the EFC metrics row moves from 11 fields to 12; that EFC durations change from
      zero to real values; that all durations become untruncated and culture-invariant; that a
      repository-wide search for the session-metrics settings key found no in-repo reader, only the
      three writers and three settings-plumbing declarations enumerated in the spec's Data / API /
      Config Impact section; that the `int` to `double` widening changes `##0` rounding for
      multi-item EFC moves, pinned by
      `BuildQuickFileMetricLines_WithMultipleMovedItems_PinsRealDivisionRounding`; and the
      disposition of each of the four deliberately broken tests, naming
      `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine` as updated,
      `QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract` as deleted and replaced,
      `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` as updated, and
      `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay` as deleted. Acceptance:
      the artifact exists and carries all six statements plus the four named dispositions.
- [x] [P7-T2] Write `FF/evidence/qa-gates/efc-stopwatch-site-reachability.TS.md` recording which of
      the two EFC construction sites the P1-T7 test actually exercises, and, if
      `QuickFiler/Controllers/EfcHomeController.cs:76` proved unreachable from a fixture without a
      live Outlook mail item, naming that blocker explicitly and citing the three-hit
      `Stopwatch.StartNew` search result from P2-T10 (hits at lines 76, 176, and 225, of which
      lines 76 and 225 are the `_stopWatch` sites) as the covering evidence for that site.
      Acceptance: the artifact names the exercised site and either records that both sites are test-
      reachable or names the blocker for the unreachable one.
- [x] [P7-T3] Write `FF/evidence/other/coverage-boundaries.TS.md` recording the two coverage
      boundaries the spec declares rather than papers over: that QFC seconds truncation is not
      asserted numerically because a stopwatch cannot be given an arbitrary elapsed value without
      reflection into its internal tick field or a prohibited wall-clock wait, so the truncation fix
      is asserted on the EFC side where the elapsed value is a plain parameter; and that
      `OlStartTime` is not asserted because the calendar lookup returns null in every unit fixture,
      so the change at `QuickFiler/Controllers/QfcHomeController.Metrics.cs:123` is verified by the
      P4-T11 search census instead. Acceptance: the artifact records both boundaries with their
      reasons.
- [x] [P7-T4] Promote CFN-4, the 12-hour `"hh:mm"` format defect at
      `QuickFiler/Controllers/QfcHomeController.Metrics.cs:31` and `:110` and
      `QuickFiler/Controllers/EfcHomeController.Metrics.cs:68`, to its own GitHub issue through the
      promotion lifecycle, then write the resulting issue number into the CFN-4 section of
      `FF/spec.md` in place of the placeholder. If the `GH:` line of the P0-T3 toolchain probe
      recorded `NOT_FOUND`, do not halt: write
      `FF/evidence/issue-updates/cfn4-promotion-blocked.TS.md` carrying the exact issue title and
      body that must be filed, name the blocker, and record `PROMOTION BLOCKED` in the CFN-4 section
      instead. Acceptance: the CFN-4 section of `FF/spec.md` carries either a GitHub issue number or
      the literal `PROMOTION BLOCKED` together with the path of the blocker artifact.
- [x] [P7-T5] Write `FF/evidence/issue-updates/cross-feature-notes-handoff.TS.md` recording that
      CFN-1 and CFN-3 are directed to feature 446 and CFN-2 to feature 468, each with its file and
      line reference and its recommended remedy as stated in the spec's Cross-Feature Notes section,
      and that none of the three is fixed in this feature's diff. Acceptance: the artifact names all
      three notes with their owning feature number and states that none is fixed here.
- [ ] [P7-T6] Run the forbidden-file ownership gate. From `WS`, run
      `git diff --name-only BASELINE_SHA -- QuickFiler/Controllers/QfcHomeController.Iteration.cs QuickFiler/Controllers/QfcFormController.EventHandlers.cs QuickFiler/Controllers/QfcCollectionController.cs QuickFiler/Controllers/EfcFormController.cs QuickFiler/Interfaces/IFilerHomeController.cs QuickFiler/Controllers/IQfcHomeController.cs QuickFiler/Controllers/EfcHomeControllerDependencies.cs QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`.
      The two-dot form is deliberately omitted so the comparison includes uncommitted working-tree
      changes. Write `FF/evidence/qa-gates/ownership-gate.TS.md`. Acceptance: the command produces
      no output lines.
      **DOCUMENTED DEVIATION — deliberately left unchecked.** The gate produces one output line
      against the merge base: `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`, changed at
      line 64 from `SetField(controller, "_isExecuting", true)` to pass `1`. That write was
      unavoidable once [P3-T5] and AC-14 made `_isExecuting` a `private int`, because
      `FieldInfo.SetValue` rejects a boxed `System.Boolean` for an `System.Int32` field and no
      production-side change can alter that. The parent epic-orchestrator ratified the write after
      verifying that no epic sibling holds a claim on the file. This task is NOT checked off and
      the forbidden-list gate is NOT reported clean. Full reasoning:
      `FF/evidence/qa-gates/ownership-gate.2026-08-27T14-03.md`.
- [x] [P7-T7] Run the project-file and new-source gate. From `WS`, run
      `git diff --name-only BASELINE_SHA -- "*.csproj" "*.props" "*.targets"`,
      `git diff --name-only --diff-filter=A BASELINE_SHA -- "*.cs"`, and
      `git ls-files --others --exclude-standard -- "*.cs"`. The third command is required because
      `git diff` never lists untracked files and this plan's only commit (P7-T35) runs after this
      task, so a forbidden newly created `.cs` file would still be untracked here and invisible to
      the first two commands. Write `FF/evidence/qa-gates/project-file-gate.TS.md`. Acceptance: all
      three commands produce no output lines.
- [x] [P7-T8] Record the full changed-file inventory. From `WS`, run
      `git diff --name-only BASELINE_SHA -- . ":(exclude).claude/agent-memory"`
      and
      `git status --porcelain -- . ":(exclude).claude/agent-memory"`.
      Both commands cover the whole worktree and exclude only `.claude/agent-memory`, which holds
      428 tracked files this feature does not own and which the executing agent writes to during
      the run. Every other path in the tree remains observable, so a write outside the owned
      surface fails this gate. Write
      `FF/evidence/qa-gates/changed-file-inventory.TS.md` listing every path. Acceptance: every
      listed path is one of the five owned production files, one of the two owned test files, or a
      path under `docs/features/active/quickfiler-home-controller-metrics-442/`.
- [x] [P7-T9] Check off AC-1 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/regression-testing/efc-metrics-red.TS.md`,
      `FF/evidence/regression-testing/qfc-stopwatch-red.TS.md`,
      `FF/evidence/regression-testing/qfc-flush-red.TS.md`, and
      `FF/evidence/regression-testing/fail-before-exception.TS.md`. Acceptance: the AC-1 checkbox
      reads `[x]` and every root cause RC-1 through RC-9 is covered by a named red observation or by
      the fail-before exception dossier.
- [x] [P7-T10] Check off AC-2 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointer is recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointer
      `FF/evidence/regression-testing/qfc-flush-green.TS.md` and the test name
      `WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce`. Acceptance: the AC-2 checkbox reads
      `[x]`.
- [x] [P7-T11] Check off AC-3 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/regression-testing/qfc-flush-green.TS.md` and
      `FF/evidence/qa-gates/qfc-flush-search-census.TS.md`, and the test name
      `WriteMetricsAsync_CompletesWriterTaskBeforeReturning`. Acceptance: the AC-3 checkbox reads
      `[x]`.
- [x] [P7-T12] Check off AC-4 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointer is recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointer
      `FF/evidence/regression-testing/qfc-flush-green.TS.md` and the test name
      `WriteMetricsAsync_PassesUncancelledTokenToWriter`. Acceptance: the AC-4 checkbox reads `[x]`.
- [x] [P7-T13] Check off AC-5 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointer is recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointer
      `FF/evidence/regression-testing/qfc-flush-green.TS.md` and the test name
      `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`. Acceptance: the AC-5 checkbox
      reads `[x]`.
- [x] [P7-T14] Check off AC-6 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointer is recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointer
      `FF/evidence/regression-testing/qfc-stopwatch-green.TS.md` and the test name
      `WriteMetricsAsync_ReadsMovedStopwatchForDuration`. Acceptance: the AC-6 checkbox reads `[x]`.
- [x] [P7-T15] Check off AC-7 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/qa-gates/qfc-stopwatch-search-census.TS.md` and
      `FF/evidence/regression-testing/efc-metrics-green.TS.md`, and the test name
      `BuildQuickFileMetricLines_WithNinetySeconds_RendersUntruncatedDuration`. Acceptance: the AC-7
      checkbox reads `[x]`.
- [x] [P7-T16] Check off AC-8 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointer is recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointer
      `FF/evidence/qa-gates/qfc-stopwatch-search-census.TS.md`. Acceptance: the AC-8 checkbox reads
      `[x]`.
- [x] [P7-T17] Check off AC-9 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/regression-testing/efc-metrics-green.TS.md`,
      `FF/evidence/qa-gates/efc-search-census.TS.md`, and
      `FF/evidence/qa-gates/efc-stopwatch-site-reachability.TS.md`, and the test name
      `StopWatch_AfterControllerConstruction_IsRunning`. Acceptance: the AC-9 checkbox reads `[x]`.
- [x] [P7-T18] Check off AC-10 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/qa-gates/efc-search-census.TS.md` and
      `FF/evidence/qa-gates/msbuild-nullable.TS.md`. Acceptance: the AC-10 checkbox reads `[x]`.
- [x] [P7-T19] Check off AC-11 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/regression-testing/efc-metrics-green.TS.md` and
      `FF/evidence/other/pr-body-statements.TS.md`, and the test name
      `BuildQuickFileMetricLines_WithMultipleMovedItems_PinsRealDivisionRounding`. Acceptance: the
      AC-11 checkbox reads `[x]`.
- [x] [P7-T20] Check off AC-12 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/regression-testing/efc-metrics-green.TS.md` and
      `FF/evidence/qa-gates/efc-search-census.TS.md`, and the test names
      `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine` and
      `BuildQuickFileMetricLines_RendersTwelveCommaSeparatedFields`. Acceptance: the AC-12 checkbox
      reads `[x]`.
- [x] [P7-T21] Check off AC-13 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointer is recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointer
      `FF/evidence/regression-testing/efc-metrics-green.TS.md` and the test name
      `BuildQuickFileMetricLines_WithEmbeddedCommas_StillRendersTwelveFields`. Acceptance: the AC-13
      checkbox reads `[x]`.
- [x] [P7-T22] Check off AC-14 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/regression-testing/efc-reentrancy-green.TS.md` and
      `FF/evidence/regression-testing/fail-before-exception.TS.md`, and the test names
      `TryBeginExecuteMoves_SecondCallBeforeReset_ReturnsFalse` and
      `TryBeginExecuteMoves_AfterResetExecuteMovesState_ReturnsTrue`. Acceptance: the AC-14 checkbox
      reads `[x]`.
- [x] [P7-T23] Check off AC-15 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/qa-gates/efc-search-census.TS.md` and
      `FF/evidence/regression-testing/efc-metrics-green.TS.md`, and the test names
      `QuickFileMetricsWriteFilenameOnly_WithAbsentPrerequisites_DoesNotThrow` and
      `QuickFileMetricsWriteFilenameOnly_WithPrerequisites_DelegatesToThreeArgumentOverload`.
      Acceptance: the AC-15 checkbox reads `[x]`.
- [x] [P7-T24] Check off AC-16 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/regression-testing/efc-metrics-green.TS.md` and
      `FF/evidence/regression-testing/qfc-stopwatch-green.TS.md`, and the test names
      `BuildQuickFileMetricLines_UnderGermanCulture_RendersInvariantDecimalSeparator` and
      `WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator`. Acceptance: the AC-16
      checkbox reads `[x]`.
- [x] [P7-T25] Check off AC-17 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointer is recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointer
      `FF/evidence/qa-gates/test-determinism.TS.md`. Acceptance: the AC-17 checkbox reads `[x]`.
- [x] [P7-T26] Check off AC-18 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointer is recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointer
      `FF/evidence/other/pr-body-statements.TS.md`. Acceptance: the AC-18 checkbox reads `[x]` and
      the referenced artifact names all four deliberately broken tests with their dispositions.
- [ ] [P7-T27] Check off AC-19 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/qa-gates/ownership-gate.TS.md` and
      `FF/evidence/qa-gates/changed-file-inventory.TS.md`. Acceptance: the AC-19 checkbox reads
      `[x]`.
      **DOCUMENTED DEVIATION — deliberately left unchecked.** AC-19's first sentence requires the
      diff to list only the five owned production files, the two owned test files and paths under
      the feature folder. It lists an eighth source path,
      `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`, the one-line parent-ratified write
      recorded at [P7-T6]. AC-19's second sentence, that the six named forbidden files are
      unmodified, IS satisfied. A partially satisfied criterion is left unchecked. Full reasoning:
      `FF/evidence/qa-gates/ownership-gate.2026-08-27T14-03.md` and
      `FF/evidence/qa-gates/acceptance-criteria-status.2026-08-27T14-32.md`.
- [x] [P7-T28] Check off AC-20 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointer is recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointer
      `FF/evidence/qa-gates/project-file-gate.TS.md`. Acceptance: the AC-20 checkbox reads `[x]`.
- [x] [P7-T29] Check off AC-21 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/qa-gates/owned-file-line-counts.TS.md` and
      `FF/evidence/qa-gates/qfc-test-file-size.TS.md`. Acceptance: the AC-21 checkbox reads `[x]`.
- [x] [P7-T30] Check off AC-22 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/baseline/mstest-coverage.TS.md`, `FF/evidence/qa-gates/mstest-coverage.TS.md`,
      and `FF/evidence/qa-gates/coverage-delta.TS.md`. Acceptance: the AC-22 checkbox reads `[x]`.
- [x] [P7-T31] Check off AC-23 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/qa-gates/csharpier-check.TS.md`,
      `FF/evidence/qa-gates/msbuild-analyzers.TS.md`,
      `FF/evidence/qa-gates/msbuild-nullable.TS.md`,
      `FF/evidence/qa-gates/mstest-coverage.TS.md`, and
      `FF/evidence/qa-gates/toolchain-loop.TS.md`. Acceptance: the AC-23 checkbox reads `[x]`.
- [x] [P7-T32] Check off AC-24 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointer is recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointer
      `FF/evidence/other/pr-body-statements.TS.md`. Acceptance: the AC-24 checkbox reads `[x]`.
- [x] [P7-T33] Check off AC-25 in `FF/spec.md`. The only edit made to `FF/spec.md` is flipping that criterion's checkbox from `- [ ]` to `- [x]`; the criterion text is not touched, and the evidence pointers are recorded in `FF/evidence/qa-gates/acceptance-criteria-status.TS.md`, the artifact P7-T34 produces, not in `FF/spec.md`. For that status artifact, record the evidence pointers
      `FF/evidence/issue-updates/cross-feature-notes-handoff.TS.md` and the CFN-4 disposition
      written by task P7-T4. Acceptance: the AC-25 checkbox reads `[x]` if and only if CFN-4 carries
      a GitHub issue number; if CFN-4 carries `PROMOTION BLOCKED`, leave the AC-25 checkbox unchecked
      and record the outstanding item in `FF/evidence/issue-updates/cfn4-promotion-blocked.TS.md`.
- [x] [P7-T34] Verify the acceptance-criteria state. Count the `[x]` checkboxes in the
      `## Acceptance Criteria` section of `FF/spec.md` and write
      `FF/evidence/qa-gates/acceptance-criteria-status.TS.md` per
      `.claude/skills/acceptance-criteria-tracking/SKILL.md`, listing all 25 criteria with their
      checked state and evidence pointer. Acceptance: the artifact lists exactly 25 criteria and
      records 25 checked, or 24 checked with AC-25 named as the single outstanding item together
      with its blocker artifact path.
- [x] [P7-T35] Commit every source, test, and evidence change on the feature branch with a message
      naming issues #442, #443, and #451, then run
      `git status --porcelain -- . ":(exclude).claude/agent-memory"`
      from `WS`. The command covers the whole worktree and excludes only `.claude/agent-memory`,
      which holds 428 tracked files this feature does not own and which the executing agent writes
      to during the run. Every other path in the tree remains observable, so a write outside the
      owned surface fails this gate. Write no artifact after this task.
      Acceptance: the scoped `git status --porcelain` command produces no output lines.

---

## Validator status

The `mcp__drm-copilot__validate_orchestration_artifacts` MCP tool is not present in this planning
session's tool surface. **VALIDATOR NOT RUN.** No validator result is claimed for this plan. The
structural self-check performed instead confirms: every phase heading uses the exact form
`### Phase N — Title` with an ATX `###`, an em-dash surrounded by single spaces, and no token
between the phase number and the em-dash; every task line begins `- [ ] [P#-T#]`; task identifiers
are digit-only and sequential by appearance within their phase; no column-zero `#` character appears
inside any fenced or indented block; and the file uses LF line endings throughout.

## Preflight

- Directive for handoff: `DIRECTIVE: PREFLIGHT VALIDATION ONLY`
- Required signal: `PREFLIGHT: ALL CLEAR` or `PREFLIGHT: REVISIONS REQUIRED`
- Every revision iteration updates this same file,
  `docs/features/active/quickfiler-home-controller-metrics-442/plan.2026-08-24T09-40.md`. No
  timestamped sibling plan file is created.
