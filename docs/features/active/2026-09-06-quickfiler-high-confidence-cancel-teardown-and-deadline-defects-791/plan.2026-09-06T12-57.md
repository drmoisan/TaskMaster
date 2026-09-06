# 2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects (Plan)

- **Issue:** #791
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-06T12-57
- **Status:** Ready for preflight
- **Version:** 1.0
- **Work Mode:** full-bug (resolved from `issue.md` line 12 and `spec.md` line 9)
- **Language in scope:** C# only (`QuickFiler`, `QuickFiler.Test`; legacy non-SDK projects with explicit `<Compile Include>` items)
- **Authoritative AC source:** `docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/spec.md` — "Acceptance Criteria", AC1 through AC6. `user-story.md` is narrative context only and is not an AC source or a check-off target.

**Fail-closed evidence rule:** Every baseline, regression, and QA artifact named by a task must exist with all required fields before that task may be checked off. A missing or field-incomplete artifact makes the outcome BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Each evidence-producing task names its exact artifact path. Work is not complete without the artifact.

---

## Plan-wide rules

**R1 — Evidence location (non-overridable).** Every evidence artifact is written under
`docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/evidence/<kind>/`
with `<kind>` in `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other`. Below, `<FEATURE>` abbreviates
`docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791`. No caller supplied a
non-canonical evidence path, so no `EVIDENCE_LOCATION_OVERRIDE_REJECTED` record is required by this plan.
`artifacts/csharp/coverage.xml` is a tool output document, not an evidence artifact; `.claude/hooks/enforce-evidence-locations.ps1`
lines 22-26 name `artifacts/csharp/` as an explicitly permitted path, and it is not in the forbidden prefix list at lines 64-74.

**R2 — Evidence artifact schema.** Every command-bearing task writes an artifact containing, at minimum, the literal field
lines `Timestamp:` (format `yyyy-MM-ddTHH-mm`), `Command:`, `EXIT_CODE:`, and `Output Summary:`. A task whose command is
expected to exit non-zero additionally writes `ExpectedExitCode: 1`.

**R3 — Evidence path hygiene.** No artifact may contain an absolute host path or a host account name. Replace a repository
root with `<repo-root>`, a user profile segment with `<user>`, and a machine name with `<host>`. This applies to tool
stdout, MSBuild logs, stack traces, Cobertura `filename` values, and TRX content alike. `QuickFiler.Test/QuickFiler.Test.csproj`
line 34 sets `<DebugType>full</DebugType>`, so Debug stack traces carry full source paths. TRX files carry `runUser` and
`computerName` attributes; never paste raw TRX content into an artifact — record only parsed counter values. The one
deliberate exception is the `vswhere`-resolved `vstest.console.exe` path that [P0-T6] is required to record; that value is
recorded in full because the task exists to pin it.

**R4 — Token-assertion case rule.** Every token-presence or token-absence assertion in this plan is case-sensitive.
Use `Select-String -CaseSensitive -SimpleMatch` or `git grep` without `-i`. PowerShell `-match` and a bare `Select-String`
are case-insensitive and must not be used for these gates.

**R5 — Named tests before phrase searches.** Where an acceptance condition can be carried by a named MSTest method, the
condition is stated as that method passing. Phrase searches are used only where no test can express the condition, and
every such literal is quoted verbatim in this document outside its command span.

**R6 — Base reference.** [P0-T2] records `BASE-SHA` (the commit at plan start) into
`<FEATURE>/evidence/baseline/p0-t2-branch-commit.md`. Every later `git diff` in this plan uses that recorded value as its
ref operand. No SHA is pinned as a literal expectation in this document.

**R7 — Scope pathspec for AC5.** AC5 says the branch diff "touches no file outside the Write Set". Read literally over the
whole tree it is unsatisfiable, because this plan is required to write evidence artifacts under `<FEATURE>/evidence/` and
to check off AC boxes in `spec.md`. AC5 is therefore evaluated over the source pathspec `'*.cs' '*.csproj'` only. Every
AC5 gate in this plan carries that pathspec, a `git add --intent-to-add` companion so newly created files are visible to
an anchored diff, and a `git status --porcelain --untracked-files=all` companion.

**R8 — 500-line ceiling.** `.claude/rules/general-code-change.md` caps every production and test file at 500 lines.
Baseline counts for every file this plan edits are captured by [P0-T12] and re-measured after the final format by [P3-T9].
The tightest files are named in the Decisions Record.

**R9 — MSBuild command forms.** The two gate builds use exactly the CLAUDE.md commands, with `/t:Rebuild` and without
`/p:Nullable=enable`. Iterative builds inside Phases 1 and 2 use `/t:Build` with no `/p:` gate switches; those builds
exist to produce test assemblies, not to run gates, and every source edit changes a timestamp so `CoreCompile` is not
skipped. A project-file build, if ever needed, must use `/p:Platform=AnyCPU`; the quoted `"/p:Platform=Any CPU"` form is a
solution-level alias only.

---

## Decisions Record

**D1 — `QfcDatamodel` is excluded from coverage measurement.** `QuickFiler/Controllers/QfcDatamodel.cs` line 25 carries
`[ExcludeFromCodeCoverage]` on the partial class declaration. The attribute applies to the whole type, so members declared
in `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` (which declares `public partial class QfcDatamodel` at line 12)
are excluded too. `QuickFiler/Controllers/QfcScanProgressBandMapper.cs` line 12 records the same fact in prose. Changed-line
coverage for those two files is therefore structurally unmeasurable rather than merely low. [P0-T11] turns this into a
decidable determination against the baseline Cobertura document instead of an assumption, and [P3-T8] compares only the
files that determination reports as measurable. Named-test evidence is the substitute for the unmeasurable files.

**D2 — Retargeting surface is larger than `spec.md` Test Strategy names.** `spec.md` lines 230-234 name four retargeting
obligations. Reading every deadline-dependent gate test against the AC1 design found three more that also encode the
superseded behaviour and will fail after the change. The complete set, re-derived in this pass, is recorded in the Citation
table and drives tasks [P1-T8] through [P1-T11]. Retargeting a test that AC3 does not name is permitted: AC3 requires the
named tests to exist and pass, and AC5 permits changes to test files under `QuickFiler.Test/Controllers`.

**D3 — `IFilerFormController.cs` line 11 declares `Task ActionCancelAsync();`.** An optional `trigger` parameter would not
satisfy that interface member (C# requires an exact signature match), and `QuickFiler/Interfaces/IFilerFormController.cs`
is outside the Write Set, so AC5 forbids changing it. The Logging Plan's "trigger (button vs. completion path)" discriminator
is therefore supplied by call-site logging inside the Write Set instead: the error path already logs at
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs` lines 167-168, and [P2-T11] adds one `log.Debug` line
immediately before the completion-path call at line 208. `ActionCancelAsync` keeps its zero-parameter signature.

**D4 — Token cancellation must stay before the first `await`.**
`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` lines 163-179
(`CancelClicked_WhenRaised_CancelsParentTokenSource`) raises the viewer's `CancelClicked` event and asserts the parent
token is cancelled by the time `Mock.Raise` returns. That holds today only because `ActionCancelAsync` reaches
`_parent?.TokenSource?.Cancel()` synchronously. The reordered method must keep the cancel stage ahead of the
`await _formViewer.UiSyncContext` marshal.

**D5 — Null-conditional access throughout the Cancel path.**
`QuickFiler.Test/Controllers/QfcFormControllerTests.cs` lines 392-403 (`ButtonCancel_Click_ShouldCancelAction`) awaits
`ActionCancelAsync()` against loose mocks in which `IQfcHomeController.KeyboardHandler` and `IQfcHomeController.DataModel`
both resolve to `null`. Every new dereference on the Cancel path must be null-conditional, and the awaited quiesce must be
captured into a local and awaited only when non-null, or that existing test starts throwing.

**D6 — No `_cancelTeardownStarted` flag.** Repeat invocation is already inert after the fix: the first pass nulls
`_parent`, `_groups`, `_formViewer` and `_parentCleanup` (`QuickFiler/Controllers/QfcFormController.SetupDisposal.cs`
lines 250-260), the unregister guard at lines 180-183 returns early, and `undoQueue?.CompleteAdding()` is already wrapped
against `ObjectDisposedException` at lines 223-230. Adding a flag would add state for a property the type already has. The
claim is pinned by an added test rather than asserted, and the extracted deactivate routine is null-guarded (see D7) so the
second pass cannot raise an ERROR line.

**D7 — Extraction invalidates its own sibling remark.**
`QuickFiler/Controllers/QfcFormController.Deactivate.cs` lines 22-25 state that no `_formViewer` null guard is written
"because this handler is reachable only through `_formViewer.FormDeactivated`". Calling the extracted routine from the
Cancel path makes that sentence false, so [P2-T8] both adds the guard and rewrites the remark.

**D8 — Bounds are gated by `deadlineEnabled`.** The scan cap and the time ceiling are evaluated inside the same
`deadlineEnabled` guard as the checkpoint. `Timeout.InfiniteTimeSpan` therefore continues to mean "no bound at all", which
is what `QfcStreamingDequeueConfidenceGateTests.Part2.cs` lines 271-312
(`DequeueAsync_DisabledSentinel_ReproducesUnboundedPreChangeBehavior`) pins. Production never passes the sentinel:
`QuickFiler/Controllers/QfcHomeController.cs` line 303 and `QuickFiler/Controllers/QfcHomeController.Iteration.cs` line 25
both pass `DefaultFirstBatchDeadline`.

**D9 — New gate bounds are exposed as internal get-only auto-properties, not private fields.** The Phase 1 seam adds the two
constructor parameters before the Phase 2 loop reads them. A `private readonly` field assigned and never read raises CS0414,
which `/p:TreatWarningsAsErrors=true` would promote to an error. An internal get-only auto-property has a compiler-generated
backing field read by its getter and raises no such warning, so the seam is warning-clean at every point in the plan.

**D10 — `QuiesceLoaderAsync` needs an injected log seam.** `spec.md` line 239 requires
`QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs` to observe the log. `QfcDatamodel` logs through `log4net`, and no
memory-appender convention exists anywhere in `QuickFiler.Test`; attaching one would mutate a process-global logger
repository and break test independence. The gate already establishes the alternative convention — an injected
`Action<string> debugLog` asserted directly (`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` lines 69, 131,
248, 258). [P1-T2] adds `internal Action<string> QuiesceDebugLog { get; set; }` to
`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, mirroring that convention. It is `internal`, the assembly already
grants `InternalsVisibleTo("QuickFiler.Test")` at `QuickFiler/Controllers/QfcHomeController.cs` line 15, and it widens no
public surface.

**D11 — `QfcHomeController.cs` headroom forces two guarded blocks, not three.** The file is 469 lines, leaving 31 lines to
the ceiling. Three separate `try`/`catch` blocks plus the `finally` measure out at roughly 505 lines. [P2-T12] therefore
uses two guarded blocks — one for the worker-completed detach, one covering the datamodel cleanup, the token-source
dispose and the field nulling — with `ParentCleanup` under `finally`. That satisfies AC2's two testable requirements (the
release callback runs under `finally`; every stage including any exception is logged) and both named tests, within the
ceiling.

**D12 — `ButtonCancel_Click_ActionThrows_DoesNotRethrow` is driven from the click handler's own body.** After [P2-T10]
every teardown stage is individually caught, so `ActionCancelAsync` no longer offers a throw source. The test instead nulls
the private `_formViewer` field so `SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext)` at
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs` line 74 raises `NullReferenceException` inside the handler's
own `try`. Today line 80 rethrows it and the raise escapes; after [P2-T11] it is logged and swallowed. The test is
false-before and true-after against exactly the line the fix changes.

**D13 — `dotnet-coverage`, not `/EnableCodeCoverage`.** AC4 requires Cobertura XML at `artifacts/csharp/coverage.xml`.
`vstest.console.exe /EnableCodeCoverage` writes a binary `.coverage` file, and the two collectors conflict, so the coverage
runs use `dotnet-coverage collect --output-format cobertura -- <vstest> ...` exactly as the most recent completed C# feature
did (`<repo-root>/docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p4-t5-tests-coverage.md`
lines 21-35 for the command, and lines 80-87 plus
`.../evidence/remediation-baseline/r-p0-t10-tests-coverage.md` lines 85-108 for the aggregation snippet and its observed
success-case output `LINES_COVERED=112351 LINES_VALID=132961 BRANCHES_COVERED=26498 BRANCHES_VALID=33480`).

**D14 — Comparability, not a repository-wide rate.** The repository-wide Cobertura `line-rate` is not a stable gate on this
harness. The coverage comparison in [P3-T8] is made on the four first-party counters produced by one pinned aggregation
over both documents, exactly as issue #782 did, with `lines-valid` equality as the comparability precondition.

**D15 — Assembly discovery excludes `\.claude\` by construction.** The nine first-party test assemblies are named
explicitly on every run command. A path that is never enumerated cannot be loaded, so no worktree under a `.claude`
segment can enter a run.

---

## Citation table (re-derived against the current tree in this pass)

| Repository-relative path | Locator re-derived |
|---|---|
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 262 lines; `DefaultFirstBatchDeadline` :56; nine-parameter ctor :111-125; `_cutoff` :129; scan loop :168-237; zero-accept deadline branch :172-180; empty-queue wait :183-196; `scanned++` :205; `LogDeadlineExpiry` :242-250; `LogScore` :252-260 |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 133 lines; `QfcDequeueStop` :30-40 with `DeadlineExpired` :38-39; `QfcDequeueBatch` struct :49-81; `IQfcDatamodel` :83-132; `void Cleanup();` :131 |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 480 lines; `[ExcludeFromCodeCoverage]` :25; `Cleanup()` :75-91 with unguarded `_globals.Ol.App` :79 and `_moveMonitor.UnhookAll()` :80; `RemainingEmailLoader` :130; `Worker_DoWork` :175-213 with `e.Result = await RemainingEmailLoader(_token);` :191; `LoadRemainingEmailsToQueueAsync` :307-348; `TryQueueRemainingMailItemAsync` :350-361 with the admission construction :355-359 |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 298 lines; `public partial class QfcDatamodel` :12; `_remainingLoadActive` :23; `DequeueWithHighConfidenceGateWithOutcomeAsync` :177-200 with the gate construction :184-194; `WaitForQueue` :289-296 |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 408 lines; `ButtonCancel_Click` :70-82 with `throw;` :80; `ActionCancelAsync` :84-94; OK-path keyboard reset :125-128; `MoveAndIterate` error-path cancel :169 and completion-path cancel :208 |
| `QuickFiler/Controllers/QfcFormController.Deactivate.cs` | 60 lines; remark "a null-viewer branch would be unreachable code" :24; `FormViewer_Deactivated` :26-58; `_formViewer.IsWebView2Focused` :28; per-item catch :45-56 |
| `QuickFiler/Controllers/QfcHomeController.cs` | 469 lines; `InternalsVisibleTo("QuickFiler.Test")` :15; `Worker_RunWorkerCompleted` subscription :91 and :131; `RunAsync` outcome call :300-305; `Worker_RunWorkerCompleted` :343-368; `Cleanup()` :370-379; `_tokenSource` :442 |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | `IterateQueueAsync` :12-65; `CompleteAddingAsync` only under `SourceExhausted` :39-48 (unmodified by this plan) |
| `QuickFiler/Interfaces/IFilerFormController.cs` | `Task ActionCancelAsync();` :11 |
| `QuickFiler/Interfaces/IQfcCollectionController.cs` | `ItemGroups` :17; `UnregisterNavigation()` :109; `Cleanup()` :116 |
| `QuickFiler/Interfaces/IQfcKeyboardHandler.cs` | `KbdActive` :11; `ToggleKeyboardDialog()` :12 |
| `QuickFiler/Interfaces/IQfcFormViewer.cs` | `UiSyncContext` :17; `Worker` :18; `IsWebView2Focused` :64; `ParkFocusOffWebView2()` :70 |
| `QuickFiler/Controllers/IQfcHomeController.cs` | `IQfcDatamodel DataModel { get; }` :11 |
| `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` | three-delegate ctor :14-24; `TryQueueAsync` :26-38 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | 477 lines; `CreateGate` reflection helper :27-92 with the exact nine-type array :56-71 and the fail-closed assert :74-77; second `CreateGate` overload :94-121; `DequeueBatchAsync` :138-158; `DequeueAsync_UsesDequeueTimeScoreSelection_AndLogsScoreContext` :160-179 (filtered `ContainSingle`, unaffected); `DequeuePastDeadlineQualifiersAsync` :448-475 (first candidate qualifies, deadline inert, unaffected) |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` | 465 lines; `DeadlineConfigurations` :30; `CreateLowYieldGate` :37-70; **breaks:** `DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline` :76-121, `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReturnsEmptyListAtTheBound` :124-144, `DequeueAsync_AfterDeadlineReturn_StopsTakingAndLeavesUnscannedCandidates` :205-228, `DequeueAsync_DeadlineExpiry_EmitsOneExpiryLineAndKeepsPerCandidateLogging` :346-385 (total-count assertion `logs.Should().HaveCount(4, ...)` :384); **unaffected:** :150-200, :233-265, :271-312, :319-339, :392-422, :429-463 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` | 280 lines; `Scored` helper :32-36; **breaks:** `DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns` :92-127, `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop` :174-208; **unaffected:** :43-86, :134-165, :215-238, :246-... |
| `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | 413 lines; **breaks:** `DequeueNextItemGroupWithOutcomeAsync_DeadlineExpiredGate_ReportsDeadlineExpiredStop` :201-260 |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | 477 lines; `IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding` :394-406; `IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce` :412-424 |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.Part2.cs` | 101 lines; documents that the base part carries the only `[TestClass]` and the shared `ArrangeIterate` / `VerifyCompleteAdding` helpers :12-21 |
| `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` | 248 lines; construction seam `CreateController` :60-70; `SetPrivateField` :72-73; `InjectGroups` :79-92; register-guard setup :47-58 |
| `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` | 496 lines; `CancelClicked_WhenRaised_CancelsParentTokenSource` :162-179 |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | 792 lines; loose-mock setup :89-100; `CreateQfcFormController` :75-87; `ButtonCancel_Click_ShouldCancelAction` :392-403 |
| `QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs` | 345 lines; `Cleanup_ExecutesCorrectly` :79-103 (verifies `_datamodel.Cleanup()` once and `ParentCleanup.Invoke()` once) |
| `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` | 255 lines; `CreateUninitializedDatamodel` :35-36; `SetPrivateField` :38-45; bounded-condition helper `WaitForState` with its rationale :47-57 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | insertion-ordered `<Compile Include>` list; gate parts :165-167; `QfcHomeControllerIterationTests.cs` :169; `QfcQueuePurePathsTests.cs` :117 |
| `QuickFiler.Test/packages.config` | `Microsoft.Extensions.TimeProvider.Testing` 10.9.0 :85-89; `Moq` 4.20.72 :112; `FluentAssertions` 8.10.0 :8; `MSTest.TestFramework` 4.4.0 :120 |
| `coverage.config` | module excludes :12-22; carries no `.*\.Test\.dll$` entry, so the derived config appends one |
| `.gitignore` | `artifacts/` :57; `coverage/*` :144 with `!coverage/.gitkeep` :145 |
| `.github/workflows/_mstest-coverage.yml` | assembly discovery :86-96; run switches `/EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` :99 |
| `.claude/hooks/enforce-evidence-locations.ps1` | permitted `artifacts/csharp/` :22-26; forbidden prefixes :64-74 |

---

### Phase 0 — Baseline capture and toolchain bootstrap

- [ ] [P0-T1] Read, in the `policy-compliance-order` sequence, `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, and `.claude/rules/tonality.md`, then write `<FEATURE>/evidence/baseline/phase0-instructions-read.md` containing the literal field lines `Timestamp:`, `Policy Order:`, and an explicit list of the five files read with their line counts. Acceptance: the artifact exists and contains all five paths and the three field lines.

- [ ] [P0-T2] Record the branch and base commit into `<FEATURE>/evidence/baseline/p0-t2-branch-commit.md`, including the literal field lines `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, and the two derived lines `BASE-BRANCH: <name>` and `BASE-SHA: <40-hex>`. Acceptance: both derived lines are present and `BASE-SHA` is a 40-character hexadecimal value.

```powershell
git rev-parse --abbrev-ref HEAD
git rev-parse HEAD
git status --porcelain --untracked-files=all
```

- [ ] [P0-T3] Restore NuGet packages for the solution and record `<FEATURE>/evidence/baseline/p0-t3-nuget-restore.md`. The worktree has no `packages/` directory, so `QuickFiler.Test`'s 70-plus `..\packages\` HintPaths and its five `<Analyzer Include>` items are unresolvable until this runs; CS0006 is an error, not a warning. Acceptance: the directory `packages/Moq.4.20.72` exists after the command and the artifact records the EXIT_CODE of the restore.

```powershell
msbuild TaskMaster.sln /t:Restore /m /p:RestorePackagesConfig=true /p:Configuration=Debug "/p:Platform=Any CPU"
Test-Path 'packages/Moq.4.20.72'
```

- [ ] [P0-T4] Restore the manifest-pinned dotnet tools and record `<FEATURE>/evidence/baseline/p0-t4-dotnet-tool-restore.md`. The repository-local SDK marker directory `.dotnet-sdk/sdk/8.0.205` already exists, so `global.json` resolves. Acceptance: the artifact records `EXIT_CODE: 0` and `dotnet tool run csharpier --version` prints `1.2.6`.

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet tool restore
dotnet tool run csharpier --version
```

- [ ] [P0-T5] Resolve `dotnet-coverage` and record `<FEATURE>/evidence/baseline/p0-t5-dotnet-coverage.md`. Run `dotnet-coverage --version` first; only if that probe exits non-zero, run `dotnet tool install --global dotnet-coverage` and re-probe. Acceptance: the artifact records a final `dotnet-coverage --version` invocation with `EXIT_CODE: 0` and the printed version string, and states which of the two branches was taken.

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet-coverage --version
```

- [ ] [P0-T6] Resolve `vstest.console.exe` through `vswhere` and record the full resolved path into `<FEATURE>/evidence/baseline/p0-t6-vstest-resolution.md` as `VSTEST-PATH: <resolved path>`. This is the one artifact exempted from R3's path reduction, because pinning the resolved path is the task's purpose. Acceptance: `VSTEST-PATH` names an existing file and the artifact records `EXIT_CODE: 0`.

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
$vstest
Test-Path $vstest
```

- [ ] [P0-T7] Capture the CSharpier baseline into `<FEATURE>/evidence/baseline/p0-t7-csharpier-check.md`, recording the verbatim printed line and the derived line `BASELINE-CSHARPIER-CHECKED-FILES: <N>`. The success-case output of this command on a clean tree is the single line `Checked <N> files in <M>ms.` with exit 0, observed at `<repo-root>/docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p4-t2-format-check.md` line 18. If the check reports drift, the artifact must list every drifting path as a disclosed pre-existing set. Acceptance: the artifact records `EXIT_CODE:`, the printed line, and the `BASELINE-CSHARPIER-CHECKED-FILES` numeral.

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet tool run csharpier check .
```

- [ ] [P0-T8] Capture the analyzer-build baseline into `<FEATURE>/evidence/baseline/p0-t8-msbuild-analyzers.md` using exactly the CLAUDE.md analyzer command. Acceptance: the artifact records `EXIT_CODE:` and an `Output Summary:` giving the warning and error counts from the MSBuild summary.

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

- [ ] [P0-T9] Capture the nullable-build baseline into `<FEATURE>/evidence/baseline/p0-t9-msbuild-nullable.md` using exactly the CLAUDE.md nullable command. `/p:Nullable=enable` must not be added and `/t:Build` must not be substituted. Acceptance: the artifact records `EXIT_CODE:` and the warning and error counts.

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

- [ ] [P0-T10] Run `QuickFiler.Test` alone and record its pass/fail counts into `<FEATURE>/evidence/baseline/p0-t10-quickfiler-tests.md` as the derived lines `BASELINE-QFT-TOTAL:`, `BASELINE-QFT-PASSED:`, `BASELINE-QFT-FAILED:`, read from the TRX `ResultSummary/Counters` element. Do not paste TRX content (R3). Acceptance: all three derived lines are present and `BASELINE-QFT-FAILED` is recorded whatever its value.

```powershell
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p0-t10' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook'
```

- [ ] [P0-T11] Run the full nine-assembly suite under `dotnet-coverage` and record `<FEATURE>/evidence/baseline/p0-t11-coverage.md` with the derived lines `BASELINE-LINES-COVERED:`, `BASELINE-LINES-VALID:`, `BASELINE-BRANCHES-COVERED:`, `BASELINE-BRANCHES-VALID:`, the two derived percentages, and `BASELINE-TOTAL-TESTS:`. The four counters are aggregated from `coverage\791-baseline.cobertura.xml` by the pinned all-descendant `.//line` selection over the nine first-party packages, whose observed success-case output form is `LINES_COVERED=<n> LINES_VALID=<n> BRANCHES_COVERED=<n> BRANCHES_VALID=<n>`. Record `BASELINE_FLOOR: MET` or `BASELINE_FLOOR: NOT MET` against the 80 percent line floor and continue either way; a pre-existing repository floor never halts this plan. Acceptance: the four `BASELINE-` counter lines and the `BASELINE_FLOOR` line are present and numeric.

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
$derived = 'coverage\791-effective-coverage.config'
[xml]$cfg = Get-Content -LiteralPath 'coverage.config'
$excl = $cfg.Configuration.CodeCoverage.ModulePaths.Exclude
$node = $cfg.CreateElement('ModulePath'); $node.InnerText = '.*\.Test\.dll$'
$null = $excl.AppendChild($node); $cfg.Save((Join-Path (Get-Location) $derived))
dotnet-coverage collect --output coverage\791-baseline.cobertura.xml --output-format cobertura --settings coverage\791-effective-coverage.config -- $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p0-t11' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

- [ ] [P0-T12] Determine, from `coverage\791-baseline.cobertura.xml`, which Write Set production files are measurable, and write `<FEATURE>/evidence/baseline/p0-t12-coverage-measurability.md`. For each of the seven production paths, query for a `class` element whose `filename` attribute ends with a directory separator followed by that file's name, and record one line per file of the form `MEASURABLE: <path>` or `UNMEASURABLE: <path>`. The trailing-name match must be separator-anchored: an unanchored `QfcDatamodel.cs` suffix also selects `IQfcDatamodel.cs`. Acceptance: exactly seven `MEASURABLE:`/`UNMEASURABLE:` lines are present, one per Write Set production path, and the artifact records the class-element counts the determination was made from.

```powershell
$doc = New-Object System.Xml.XmlDocument
$doc.Load((Resolve-Path -LiteralPath 'coverage\791-baseline.cobertura.xml').Path)
$names = @('QfcStreamingDequeueConfidenceGate.cs','IQfcDatamodel.cs','QfcDatamodel.QueueProcessing.cs','QfcDatamodel.cs','QfcFormController.EventHandlers.cs','QfcFormController.Deactivate.cs','QfcHomeController.cs')
foreach ($n in $names) {
    $hit = 0
    foreach ($c in $doc.SelectNodes('//class')) {
        $f = $c.GetAttribute('filename')
        if ($f.EndsWith('\' + $n) -or $f.EndsWith('/' + $n)) { $hit++ }
    }
    "$n classElements=$hit"
}
```

- [ ] [P0-T13] Record the baseline line count of every file this plan edits or creates into `<FEATURE>/evidence/baseline/p0-t13-line-counts.md`, one `<path> = <count>` line per file, covering the seven production paths, the six existing test paths (`QfcStreamingDequeueConfidenceGateTests.cs`, `.Part2.cs`, `.Part3.cs`, `QfcQueuePurePathsTests.cs`, `QfcHomeControllerIterationTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`), and a `CEILING: 500` line. Acceptance: every listed path has a numeric count and the artifact names the three tightest files.

- [ ] [P0-T14] Record the pre-change status of the seven deadline-dependent tests named in D2 into `<FEATURE>/evidence/baseline/p0-t14-deadline-test-inventory.md`, one line per test of the form `BASELINE-PASS: <FullyQualifiedName>`, derived from the [P0-T10] TRX. This is the set that Phase 1 deliberately turns red and Phase 2 turns green again; recording it now is what makes the Phase 2 no-newly-failing comparison meaningful. Acceptance: seven `BASELINE-PASS:` lines are present.

```powershell
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p0-t14' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~QfcStreamingDequeueConfidenceGateTests|FullyQualifiedName~QfcQueuePurePathsTests'
```

---

### Phase 1 — Declaration seams and failing regression tests

Phase 1 is test-first. Tasks [P1-T1] through [P1-T3] add only type-level declarations, because the new tests name types and
members that do not exist yet and a missing declaration reddens the whole `QuickFiler.Test` assembly at compile time rather
than producing a targeted failure. No behaviour changes in Phase 1.

- [ ] [P1-T1] In `QuickFiler/Interfaces/IQfcDatamodel.cs`, add the enum member `ScanCapReached` to `QfcDequeueStop` with an XML doc recording that it reports a bounded zero-acceptance exit and is treated exactly as `DeadlineExpired` is; update the `DeadlineExpired` XML doc at line 38 to record that issue #791 made the first-batch deadline advisory and that the member is retained for compatibility; and declare `Task QuiesceLoaderAsync(TimeSpan timeout);` on `IQfcDatamodel` with an XML doc stating that it cancels, awaits the loader against the supplied bound, never throws for the timeout case, and returns when the loader completes or the bound expires. Acceptance: `QuickFiler.csproj` compiles and a case-sensitive search of `QuickFiler/Interfaces/IQfcDatamodel.cs` finds the single-line token `ScanCapReached` and the single-line token `QuiesceLoaderAsync`.

- [ ] [P1-T2] In `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, add `internal Action<string> QuiesceDebugLog { get; set; }` with an XML doc naming it the injected diagnostic seam that mirrors the gate's `debugLog` parameter (D10), and add `public Task QuiesceLoaderAsync(TimeSpan timeout) => throw new NotImplementedException("Issue #791: the quiesce body is supplied by [P2-T4].");` as the declaration-only seam. Acceptance: the solution compiles and a case-sensitive search of that file finds the single-line token `QuiesceDebugLog`.

- [ ] [P1-T3] In `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`, add `internal static readonly int DefaultMaxScanWithoutAcceptance = 250;` and `internal static readonly TimeSpan DefaultZeroAcceptanceCeiling = TimeSpan.FromSeconds(120);` with XML docs recording that both are implementation quality bounds with a constructor test seam and no settings surface, add the two optional parameters `int? maxScanWithoutAcceptance = null, TimeSpan? zeroAcceptanceCeiling = null` to the end of the wide constructor's parameter list, and store them in the internal get-only auto-properties `MaxScanWithoutAcceptance` and `ZeroAcceptanceCeiling` (D9). Do not change `DequeueAsync`. Acceptance: the solution compiles and the wide constructor has exactly eleven parameters.

- [ ] [P1-T4] Build the solution so the seam declarations are available to the test project, and record `<FEATURE>/evidence/regression-testing/p1-t4-seam-build.md`. Acceptance: `EXIT_CODE: 0`.

```powershell
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

- [ ] [P1-T5] Widen the fail-closed reflection helper in `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` lines 27-92: add `int? maxScanWithoutAcceptance = null, TimeSpan? zeroAcceptanceCeiling = null` to both `CreateGate` overloads, add `typeof(int?)` and `typeof(TimeSpan?)` to the constructor type array, add the two arguments to the `constructor.Invoke` array, and update the assertion message at line 76 from "nine-parameter" to "eleven-parameter". Keep the helper fail-closed: the `constructor.Should().NotBeNull(...)` guard must remain. Acceptance: `QuickFiler.Test` compiles and the file remains at or below 500 lines (baseline 477, budget at most 12 added lines).

- [ ] [P1-T6] Create `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs` declaring `public partial class QfcStreamingDequeueConfidenceGateTests` in namespace `QuickFiler.Controllers.Tests` with no `[TestClass]` attribute (the base part carries the only one; repeating it is CS0579), containing the seven AC1 tests named in `spec.md` lines 222-228: `DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance`, `DequeueAsync_ZeroAcceptedAndSourceDrained_ReportsSourceExhausted`, `DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached`, `DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling`, `DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts`, `DequeueAsync_Launch_LogsCutoffQuantityAndBounds`, and `DequeueAsync_NonEmptyPrefix_UnchangedByCheckpoint`. All seven use `FakeTimeProvider` as the clock, drive the gate through the widened `CreateGate` helper, and assert through the injected `debugLog` delegate rather than a log4net appender. The two logging tests assert on these exact single-line literals, which [P2-T2] will introduce: `High-confidence dequeue launch` and `Zero-acceptance checkpoint`. `DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached` injects `maxScanWithoutAcceptance: 4` over ten candidates and asserts the stop reason is `QfcDequeueStop.ScanCapReached`, exactly four takes occurred, and six candidates remain in the source. Acceptance: the file compiles once [P1-T7] wires it, contains exactly seven `[TestMethod]` attributes, and is at or below 500 lines.

- [ ] [P1-T7] Add four `<Compile Include>` entries to `QuickFiler.Test/QuickFiler.Test.csproj` for `Controllers\QfcStreamingDequeueConfidenceGateTests.Part4.cs`, `Controllers\QfcFormControllerCancelTeardownTests.cs`, `Controllers\QfcHomeControllerCleanupTests.cs`, and `Controllers\QfcDatamodelTeardownTests.cs`. The project is legacy `packages.config` and its item list is insertion-ordered, not alphabetical; append the four entries adjacent to the existing gate entries at lines 165-167. Acceptance: `QuickFiler.Test.csproj` contains exactly four new `<Compile Include>` lines and the four named files are compiled once they exist.

- [ ] [P1-T8] Retarget the four superseded tests in `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs`, preserving each test's intent against the new behaviour rather than deleting it: `DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline` (lines 76-121) becomes `DequeueAsync_LowYieldStream_ContinuesPastDefaultDeadlineToTheQualifier`, asserting the qualifier at position 40 is returned and that the scan runs to source exhaustion; `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReturnsEmptyListAtTheBound` (lines 124-144) becomes `DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesToSourceExhaustion`, asserting the source is drained rather than three candidates taken; `DequeueAsync_AfterDeadlineReturn_StopsTakingAndLeavesUnscannedCandidates` (lines 205-228) becomes `DequeueAsync_AfterScanCapReached_StopsTakingAndLeavesUnscannedCandidates`, replacing the 4-second deadline with `maxScanWithoutAcceptance: 4` so its existing take-count and residual-source assertions stay exactly 4 and 6; and `DequeueAsync_DeadlineExpiry_EmitsOneExpiryLineAndKeepsPerCandidateLogging` (lines 346-385) becomes `DequeueAsync_CheckpointExpiry_EmitsCheckpointLineAndKeepsPerCandidateLogging`, replacing the total-count assertion `logs.Should().HaveCount(4, ...)` at line 384 with per-category counts so the assertion is not brittle against the added launch line. Acceptance: the four old method names are absent from the file under a case-sensitive search, the four new names are present, the six unaffected tests listed in the Citation table are unchanged, and the file is at or below 500 lines.

- [ ] [P1-T9] Retarget the two superseded tests in `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs`: `DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns` (lines 92-127) keeps its name and its "no invocation after the method returns" intent but bounds the run with an injected `maxScanWithoutAcceptance` instead of the 3-second deadline, so the expected report sequence follows the cap; and `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop` (lines 174-208) becomes `DequeueAsync_ZeroAcceptedAndCapReached_ReportsScanCapReachedStop`, asserting `QfcDequeueStop.ScanCapReached` and an empty accepted list. Acceptance: the old method name `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop` is absent under a case-sensitive search, the new name is present, and the file is at or below 500 lines.

- [ ] [P1-T10] Retarget `DequeueNextItemGroupWithOutcomeAsync_DeadlineExpiredGate_ReportsDeadlineExpiredStop` in `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` lines 201-260 to `DequeueNextItemGroupWithOutcomeAsync_ScanCapReachedGate_ReportsScanCapReachedStop`, preserving its purpose — that the datamodel projects the gate's stop reason verbatim rather than folding it into quantity satisfaction — by driving the gate to its scan cap and asserting `QfcDequeueStop.ScanCapReached`. Acceptance: the old method name is absent under a case-sensitive search, the new name is present, and the file is at or below 500 lines.

- [ ] [P1-T11] Add `IterateQueueAsync_EmptyBatchWithScanCapReached_DoesNotCompleteAdding` to `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` as a sibling of the existing `IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding` pin at lines 394-406, using the same `ArrangeIterate(stop: ...)` and `VerifyCompleteAdding(queue, Times.Never, ...)` helpers. This is the AC6 pin that `#446` AC-6 is preserved: the new stop reason must not be routed into the `SourceExhausted` branch. The file is 477 lines; the addition must not take it past 500. Acceptance: the new method name is present under a case-sensitive search and the file is at or below 500 lines.

- [ ] [P1-T12] Create `QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs` with `[TestClass] public class QfcFormControllerCancelTeardownTests` in namespace `QuickFiler.Controllers.Tests`, modelled on the seam pattern of `QfcFormControllerDeactivateTests.cs` lines 36-92 (mock viewer, mock home controller, mock collection controller injected into `_groups` by private-field reflection, a `Control.ControlCollection` and an empty exclusion list so the register/unregister guard is satisfied). It contains the eight tests: `ActionCancelAsync_ResetsKbdActive_WhenKeyboardDialogActive`, `ActionCancelAsync_DoesNotToggle_WhenInactive`, `ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors`, `ActionCancelAsync_UnregistersHandlersBeforeGroupsCleanup`, `ActionCancelAsync_AwaitsLoaderQuiesceBeforeGroupsCleanup`, `ActionCancelAsync_GroupsCleanupThrows_StillInvokesParentCleanup`, `ButtonCancel_Click_ActionThrows_DoesNotRethrow`, and `ActionCancelAsync_CalledTwice_InvokesParentCleanupOnce`. Ordering is asserted through a shared invocation-order `List<string>` populated by `Callback` handlers, comparing the first index of each marker. `ButtonCancel_Click_ActionThrows_DoesNotRethrow` nulls the private `_formViewer` field so the throw originates at `QfcFormController.EventHandlers.cs` line 74 inside the handler's own `try` (D12). `ActionCancelAsync_AwaitsLoaderQuiesceBeforeGroupsCleanup` sets up `IQfcDatamodel.QuiesceLoaderAsync` to return a completed `Task` — the same shape the timeout path returns, which `QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs` pins independently — and asserts both that the quiesce marker precedes the groups-cleanup marker and that both later stages still ran. Acceptance: the file compiles, contains exactly eight `[TestMethod]` attributes, and is at or below 500 lines.

- [ ] [P1-T13] Create `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` with `[TestClass] public class QfcHomeControllerCleanupTests` in namespace `QuickFiler.Controllers.Tests`, constructing the controller through the public `QfcHomeController(IApplicationGlobals, System.Action)` constructor and injecting `_datamodel`, `_formViewer` and `_tokenSource` by private-field reflection, as `QfcHomeControllerPropertyTests.cs` lines 84-95 already does. It contains `Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup` (the datamodel mock throws; assert `Cleanup()` does not throw and the parent cleanup delegate ran exactly once) and `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` (assert that reading `cts.Token` after `Cleanup()` throws `ObjectDisposedException`, and that the viewer mock's `Worker` getter was read at least once, which is the observable proof the detach path executed). Acceptance: the file compiles, contains exactly two `[TestMethod]` attributes, and is at or below 500 lines.

- [ ] [P1-T14] Create `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs` with `[TestClass] public class QfcDatamodelTeardownTests` in namespace `QuickFiler.Controllers.Tests`, carrying its own `CreateUninitializedDatamodel` and `SetPrivateField` helpers following the existing duplication convention documented at `QfcDatamodelLivenessTests.cs` lines 18-24. It contains the four tests named in `spec.md` line 239 plus one capture pin: `TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing` (both `_masterQueue` and `_moveMonitor` null; assert the call does not throw and returns `false`), `QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout` (`_remainingLoadTask` injected as a completed task, `TimeProvider` a `FakeTimeProvider` never advanced; assert the returned task completes and `QuiesceDebugLog` captured a line containing `Loader quiesce completed`), `QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs` (`_remainingLoadTask` injected as a never-completing `TaskCompletionSource` task; start the call, advance the fake clock past the bound, await, and assert `QuiesceDebugLog` captured a line containing `Loader quiesce timed out`), `Cleanup_CalledTwice_DoesNotThrow` (an uninitialized instance whose `_globals` and `_moveMonitor` are null and whose `_tokenSource` and `_worker` are set; assert two successive `Cleanup()` calls do not throw), and `Worker_DoWork_CapturesRemainingLoadTask` (drive `InitEmailQueue(0, worker)` with an injected `RemainingEmailLoader`, then assert `_remainingLoadTask` becomes non-null using a bounded event-driven condition wait carrying the same rationale comment as `QfcDatamodelLivenessTests.cs` lines 47-57). Acceptance: the file compiles, contains exactly five `[TestMethod]` attributes, and is at or below 500 lines.

- [ ] [P1-T15] Build the solution with the new and retargeted tests in place and record `<FEATURE>/evidence/regression-testing/p1-t15-test-build.md`. Acceptance: `EXIT_CODE: 0`, proving every new test compiles against the Phase 1 seams.

```powershell
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

- [ ] [P1-T16] [expect-fail] Run the gate and datamodel-projection tests and record `<FEATURE>/evidence/regression-testing/p1-t16-gate-fail-before.md` with `ExpectedExitCode: 1`. The artifact must enumerate, by fully qualified name, every failing test and state for each whether it is one of the seven new Part4 tests, one of the six retargeted tests, or the `QfcQueuePurePathsTests` retarget. Acceptance: `EXIT_CODE: 1`, and the recorded failure set includes `DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance`.

```powershell
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p1-t16' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~QfcStreamingDequeueConfidenceGateTests|FullyQualifiedName~QfcQueuePurePathsTests'
```

- [ ] [P1-T17] [expect-fail] Run `QfcFormControllerCancelTeardownTests` and record `<FEATURE>/evidence/regression-testing/p1-t17-cancel-teardown-fail-before.md` with `ExpectedExitCode: 1`, enumerating each failing test by fully qualified name and its failure message reduced per R3. Acceptance: `EXIT_CODE: 1` and `ActionCancelAsync_UnregistersHandlersBeforeGroupsCleanup` appears in the failure set.

```powershell
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p1-t17' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~QfcFormControllerCancelTeardownTests'
```

- [ ] [P1-T18] [expect-fail] Run `QfcHomeControllerCleanupTests` and record `<FEATURE>/evidence/regression-testing/p1-t18-home-cleanup-fail-before.md` with `ExpectedExitCode: 1`. Acceptance: `EXIT_CODE: 1` and both `Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup` and `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` appear in the failure set.

```powershell
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p1-t18' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~QfcHomeControllerCleanupTests'
```

- [ ] [P1-T19] [expect-fail] Run `QfcDatamodelTeardownTests` and record `<FEATURE>/evidence/regression-testing/p1-t19-datamodel-teardown-fail-before.md` with `ExpectedExitCode: 1`. The artifact must record that `TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing` fails with an `ArgumentException` whose message names a delegate over a null instance — the exact failure mode the issue log records at `issue.md` lines 64-69 — and that the two `QuiesceLoaderAsync` tests fail with `NotImplementedException` from the [P1-T2] seam. Acceptance: `EXIT_CODE: 1` and all five test names appear in the failure set with their exception types recorded.

```powershell
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p1-t19' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~QfcDatamodelTeardownTests'
```

- [ ] [P1-T20] Write `<FEATURE>/evidence/regression-testing/p1-t20-expected-red-inventory.md` consolidating the four fail-before artifacts into one list of every test that is red at the end of Phase 1, each tagged `NEW`, `RETARGETED`, or `SEAM-BLOCKED`. This is the set Phase 2 must turn green and nothing else. Acceptance: the inventory's count equals the sum of the failure counts recorded by [P1-T16] through [P1-T19], and every entry carries one of the three tags.

---

### Phase 2 — Production implementation

- [ ] [P2-T1] Rewrite the zero-acceptance branch of `DequeueAsync` in `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` lines 172-180 as a checkpoint plus a bounded exit, keeping every other statement of the loop unchanged. Inside the existing `deadlineEnabled && accepted.Count == 0` guard (D8), evaluate the two bounds against the run origin before evaluating the checkpoint against a separate checkpoint origin; when either bound is reached, return `new QfcGateBatch(accepted, QfcDequeueStop.ScanCapReached, scanned)`; when the checkpoint interval elapses, log and reset the checkpoint origin and continue. The bound check must sit ahead of `MailItem mailItem = _tryTakeNext();` so a capped scan cannot take an extra item. Acceptance: `DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance`, `DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached`, `DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling`, `DequeueAsync_ZeroAcceptedAndSourceDrained_ReportsSourceExhausted` and `DequeueAsync_NonEmptyPrefix_UnchangedByCheckpoint` all pass.

- [ ] [P2-T2] Add the two logging helpers to `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`: a launch helper invoked once at the top of `DequeueAsync` emitting a line whose first token sequence is `High-confidence dequeue launch` and which carries the cutoff in per-mille and as a fraction, the requested quantity, the checkpoint interval, the scan cap and the ceiling; and a checkpoint helper replacing `LogDeadlineExpiry` (lines 242-250) emitting a line whose first token sequence is `Zero-acceptance checkpoint` and which carries accepted, scanned, the cutoff, elapsed time, the remaining cap and ceiling, and the decision. Add a third line for the bounded exit whose first token sequence is `Zero-acceptance scan bound reached`. All three route through both `_debugLog?.Invoke(message)` and `logger.Debug(message)`, exactly as the existing helpers do. Acceptance: `DequeueAsync_Launch_LogsCutoffQuantityAndBounds` and `DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts` pass, and `DequeueAsync_UsesDequeueTimeScoreSelection_AndLogsScoreContext` (which uses a filtered `ContainSingle`) still passes.

- [ ] [P2-T3] Verify that no other production consumer routes the new stop reason into the queue-closing branch: `QuickFiler/Controllers/QfcHomeController.Iteration.cs` lines 39-48 must remain byte-identical, so `CompleteAddingAsync` stays reachable only under `SourceExhausted`. Acceptance: `IterateQueueAsync_EmptyBatchWithScanCapReached_DoesNotCompleteAdding` and `IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce` both pass, and the anchored diff below lists no entry for `QuickFiler/Controllers/QfcHomeController.Iteration.cs`.

```powershell
git add --intent-to-add -- '*.cs' '*.csproj'
git status --porcelain --untracked-files=all -- 'QuickFiler/Controllers/QfcHomeController.Iteration.cs'
git diff --name-only $BaseSha -- 'QuickFiler/Controllers/QfcHomeController.Iteration.cs'
```

- [ ] [P2-T4] In `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, add the private field `_remainingLoadTask` holding the loader task, and replace the [P1-T2] `NotImplementedException` seam with the real `QuiesceLoaderAsync(TimeSpan timeout)` body: cancel the token source, snapshot `_remainingLoadTask` into a local, return immediately when the local is null or already completed, otherwise `await Task.WhenAny(loader, TimeProvider.Delay(timeout, CancellationToken.None))` and emit exactly one outcome line through both `QuiesceDebugLog` and `logger.Info`, containing the literal `Loader quiesce completed` on the completion path and the literal `Loader quiesce timed out` on the bound path. The method must never throw for the timeout case. Acceptance: `QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout` and `QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs` pass, and a case-sensitive search of that file finds no occurrence of `NotImplementedException`.

- [ ] [P2-T5] Relocate `TryQueueRemainingMailItemAsync` from `QuickFiler/Controllers/QfcDatamodel.cs` lines 350-361 into `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, snapshotting `_masterQueue` and `_moveMonitor` into locals and returning `false` when either local is null or when cancellation is requested, before any delegate is constructed over them. The `QfcRemainingQueueAdmission` three-delegate constructor shape is unchanged. Acceptance: `TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing` passes and `TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsBelowThresholdCandidate` in `QfcDatamodelTests.cs` still passes.

- [ ] [P2-T6] In `QuickFiler/Controllers/QfcDatamodel.cs`, replace line 191 so `Worker_DoWork` captures the loader task into `_remainingLoadTask` before awaiting it, and delete the method body relocated by [P2-T5]. The `finally` that clears `_remainingLoadActive` at lines 193-200 is unchanged. Acceptance: `Worker_DoWork_CapturesRemainingLoadTask` passes and `DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle` in `QfcDatamodelLivenessTests.cs` still passes.

- [ ] [P2-T7] Null-guard `QfcDatamodel.Cleanup()` at `QuickFiler/Controllers/QfcDatamodel.cs` lines 75-91 so the `_globals.Ol.App.NewMailEx` unsubscribe at line 79 and the `_moveMonitor.UnhookAll()` call at line 80 cannot dereference a null field, and add one comment recording that a second Cancel, or a Cancel after a partially failed launch, reaches this method with those fields already released. Acceptance: `Cleanup_CalledTwice_DoesNotThrow` passes.

- [ ] [P2-T8] In `QuickFiler/Controllers/QfcFormController.Deactivate.cs`, extract the body of `FormViewer_Deactivated` (lines 26-58) into `internal void ParkFocusAndCancelSelectors()`, leaving the event handler as a one-line delegation, add a null guard so the `IsWebView2Focused` read at line 28 is null-conditional, and rewrite the `<remarks>` block at lines 22-25, whose claim that a null-viewer branch is unreachable becomes false the moment the Cancel path calls the routine (D7). The per-item boundary catch at lines 45-56 keeps its exact shape. Acceptance: all six tests in `QfcFormControllerDeactivateTests.cs` still pass, and a case-sensitive search of `QuickFiler/Controllers/QfcFormController.Deactivate.cs` returns zero matches for the single-line literal `a null-viewer branch would be unreachable code`.

- [ ] [P2-T9] In `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, add the teardown support members: `internal static readonly TimeSpan LoaderQuiesceBound` with the bound value and an XML doc recording that it is a caller-supplied constant rather than a setting; a private `RunTeardownStage(string stage, System.Action body)` helper that runs one stage, logs its completion at DEBUG and logs any escaping exception at ERROR with the stage name so a throwing stage cannot skip a later one; a private `ResetKeyboardActive()` that toggles the keyboard dialog only when `_parent?.KeyboardHandler?.KbdActive == true`, mirroring the OK path at lines 125-128; and a private `UnregisterCancelPathHandlers()` that calls `_groups?.UnregisterNavigation()` and then `UnregisterFormEventHandlers()`. Every dereference is null-conditional (D5). Acceptance: the solution compiles and `ButtonCancel_Click_ShouldCancelAction` in `QfcFormControllerTests.cs` still passes.

- [ ] [P2-T10] Rewrite `ActionCancelAsync` at `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` lines 84-94 to the ordered teardown, keeping its zero-parameter signature (D3): log entry at INFO; cancel the parent token source; marshal to the UI sync context when it is non-null; reset the keyboard-active flag; call `ParkFocusAndCancelSelectors()` while the item groups still exist; unregister navigation and form handlers before any row is removed; hide the form; capture `_parent?.DataModel?.QuiesceLoaderAsync(LoaderQuiesceBound)` into a local and await it only when non-null; clean up the groups; and invoke `Cleanup()` from a `finally` so the ribbon release callback runs whichever stage threw. The token cancel must remain ahead of the first `await` (D4). Acceptance: `ActionCancelAsync_ResetsKbdActive_WhenKeyboardDialogActive`, `ActionCancelAsync_DoesNotToggle_WhenInactive`, `ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors`, `ActionCancelAsync_UnregistersHandlersBeforeGroupsCleanup`, `ActionCancelAsync_AwaitsLoaderQuiesceBeforeGroupsCleanup`, `ActionCancelAsync_GroupsCleanupThrows_StillInvokesParentCleanup` and `ActionCancelAsync_CalledTwice_InvokesParentCleanupOnce` all pass, and `CancelClicked_WhenRaised_CancelsParentTokenSource` in `QfcFormControllerSeamTests.cs` still passes.

- [ ] [P2-T11] In `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, remove the `throw;` at line 80 from `ButtonCancel_Click` so an `async void` handler no longer converts a teardown failure into an unhandled Outlook UI-thread exception, keeping the `logger.Error(ex.Message, ex)` above it, and add one `log.Debug` line immediately before the completion-path `await ActionCancelAsync();` at line 208 that names the completion path, supplying the Logging Plan's trigger discriminator without changing the method signature (D3). Acceptance: `ButtonCancel_Click_ActionThrows_DoesNotRethrow` passes.

- [ ] [P2-T12] Rewrite `QfcHomeController.Cleanup()` at `QuickFiler/Controllers/QfcHomeController.cs` lines 370-379 as two guarded blocks under one `finally` (D11): the first detaches `Worker_RunWorkerCompleted` from `_formViewer?.Worker` through a local, before the viewer reference is dropped; the second runs `_datamodel?.Cleanup()`, disposes `_tokenSource`, and nulls `Globals`, `_formViewer`, `_explorerController`, `_formController` and `_keyboardHandler`; each block logs any escaping exception at ERROR with its stage name; and the `finally` invokes `ParentCleanup?.Invoke()` and logs the release at INFO. Acceptance: `Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup` and `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` pass, `Cleanup_ExecutesCorrectly` in `QfcHomeControllerPropertyTests.cs` still passes, and `QuickFiler/Controllers/QfcHomeController.cs` is at or below 500 lines.

- [ ] [P2-T13] Build the solution and record `<FEATURE>/evidence/regression-testing/p2-t13-post-fix-build.md`. Acceptance: `EXIT_CODE: 0`.

```powershell
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

- [ ] [P2-T14] Run every test named in the [P1-T20] inventory and record `<FEATURE>/evidence/regression-testing/p2-t14-pass-after.md`. Acceptance: `EXIT_CODE: 0` and the artifact records a passed count equal to the [P1-T20] inventory count, with `Failed: 0`.

```powershell
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p2-t14' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~QfcStreamingDequeueConfidenceGateTests|FullyQualifiedName~QfcQueuePurePathsTests|FullyQualifiedName~QfcFormControllerCancelTeardownTests|FullyQualifiedName~QfcHomeControllerCleanupTests|FullyQualifiedName~QfcDatamodelTeardownTests|FullyQualifiedName~QfcHomeControllerIterationTests'
```

- [ ] [P2-T15] Run the whole `QuickFiler.Test` assembly and record `<FEATURE>/evidence/regression-testing/p2-t15-quickfiler-suite.md` with the derived lines `POST-QFT-TOTAL:`, `POST-QFT-PASSED:`, `POST-QFT-FAILED:` and a `NEWLY-FAILING:` line listing every test failing here that was not failing in the [P0-T10] baseline. Acceptance: `NEWLY-FAILING: NONE` and `POST-QFT-FAILED` is less than or equal to `BASELINE-QFT-FAILED` from [P0-T10].

```powershell
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p2-t15' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook'
```

- [ ] [P2-T16] Record the pre-format line count of every file this plan has edited or created into `<FEATURE>/evidence/qa-gates/p2-t16-file-size-interim.md`, one `<path> = <count>` line per file alongside the [P0-T13] baseline count for the same path. Acceptance: every listed count is at or below 500, and any file within ten lines of the ceiling is named explicitly with its remaining headroom.

---

### Phase 3 — Final QA loop, coverage, and acceptance-criteria closure

- [ ] [P3-T1] Run the CSharpier formatter over the repository and record `<FEATURE>/evidence/qa-gates/p3-t1-csharpier-format.md`. `format` rewrites tracked source and still exits 0 after rewriting, so the exit code alone cannot distinguish a clean run from a repairing one; the artifact must therefore record the verbatim printed line of the form `Formatted <N> files in <M>ms.` and, as the distinguishing observation, the `git status --porcelain --untracked-files=all` path set and the `git diff --stat` output anchored to `BASE-SHA`, captured before and after the run, with the two derived lines `PATH_SETS_IDENTICAL:` and `DIFFSTAT_IDENTICAL:`. Acceptance: `EXIT_CODE: 0` and both derived comparison lines are recorded with their values.

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
git add --intent-to-add -- '*.cs' '*.csproj'
$before = @(git status --porcelain --untracked-files=all)
$beforeStat = @(git diff --stat $BaseSha)
dotnet tool run csharpier format .
$after = @(git status --porcelain --untracked-files=all)
$afterStat = @(git diff --stat $BaseSha)
```

- [ ] [P3-T2] Run the read-only CSharpier check and record `<FEATURE>/evidence/qa-gates/p3-t2-csharpier-check.md` with the verbatim printed line and the derived line `FINAL-CSHARPIER-CHECKED-FILES: <N>`. The success-case output on a clean tree is the single line `Checked <N> files in <M>ms.` with exit 0. Record the delta against `BASELINE-CSHARPIER-CHECKED-FILES` from [P0-T7]; four new `.cs` files are added by this plan, so a delta of 4 is the expected observation. Acceptance: `EXIT_CODE: 0`. The exit code is the gate here, because `check` is read-only and returns non-zero on drift.

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet tool run csharpier check .
```

- [ ] [P3-T3] Run the analyzer gate and record `<FEATURE>/evidence/qa-gates/p3-t3-msbuild-analyzers.md`, comparing its warning and error counts against [P0-T8]. Acceptance: `EXIT_CODE: 0` and the error count is 0.

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

- [ ] [P3-T4] Run the nullable gate and record `<FEATURE>/evidence/qa-gates/p3-t4-msbuild-nullable.md`, comparing its warning and error counts against [P0-T9]. `/p:Nullable=enable` must not be added and `/t:Build` must not be substituted. Acceptance: `EXIT_CODE: 0` and the error count is 0.

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

- [ ] [P3-T5] Run the full nine-assembly suite under `dotnet-coverage`, writing the Cobertura document to `artifacts/csharp/coverage.xml`, and record `<FEATURE>/evidence/qa-gates/p3-t5-tests-coverage.md` with the derived lines `FINAL-LINES-COVERED:`, `FINAL-LINES-VALID:`, `FINAL-BRANCHES-COVERED:`, `FINAL-BRANCHES-VALID:`, the two derived percentages, and `FINAL-TOTAL-TESTS:` / `FINAL-FAILED-TESTS:`. The four counters are aggregated by the same pinned all-descendant `.//line` selection over the same nine first-party package names that [P0-T11] used, so the two sides are produced by one collector, one configuration, one selection and one filter. `artifacts/` is git-ignored at `.gitignore` line 57, so the document is a local tool output rather than committed evidence; the acceptance below is on-disk existence and the recorded counters, not on `git ls-files`. Acceptance: `EXIT_CODE: 0`, `FINAL-FAILED-TESTS: 0`, `artifacts/csharp/coverage.xml` exists, and all four `FINAL-` counter lines are numeric.

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
New-Item -ItemType Directory -Force -Path 'artifacts\csharp' | Out-Null
dotnet-coverage collect --output artifacts\csharp\coverage.xml --output-format cobertura --settings coverage\791-effective-coverage.config -- $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p3-t5' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

- [ ] [P3-T6] Record the toolchain loop closure into `<FEATURE>/evidence/qa-gates/p3-t6-loop-closure.md`, listing [P3-T1] through [P3-T5] in order with each artifact path and each recorded exit code, and stating whether any step failed or rewrote a file. Acceptance: the artifact records all five steps as passing in one uninterrupted pass, or, if any step failed or changed files, records the restart and the subsequent clean pass; the checklist box for this task stays unchecked until a clean pass is recorded.

- [ ] [P3-T7] Write `<FEATURE>/evidence/qa-gates/p3-t7-changed-line-coverage.md` comparing coverage on the changed production lines. Restrict the comparison to the paths [P0-T12] reported as `MEASURABLE:`; for each path reported `UNMEASURABLE:`, record `CHANGED-LINE-COVERAGE: NOT MEASURABLE` with the `[ExcludeFromCodeCoverage]` citation (D1) and name the passing tests that exercise those changed lines as the substitute evidence. For each measurable path, derive the changed line numbers from `git diff --unified=0 $BaseSha -- <path>` and record each changed line's `hits` value from `artifacts/csharp/coverage.xml`, using a de-duplicated per-line map that merges `./lines/line` with `./methods/method/lines/line` keyed by line number and resolved by maximum `hits`. Where a diff hunk's added and removed line counts are unequal, no one-to-one baseline mapping exists; record such lines as `baseline=none` and exclude them from the regression count rather than attributing borrowed coverage. Acceptance: every changed production line in the measurable set is recorded with a post-change `hits` value, the count of changed lines with `hits = 0` is stated, and the count of changed lines whose post-change `hits` is lower than their baseline `hits` is `0`.

- [ ] [P3-T8] Write `<FEATURE>/evidence/qa-gates/p3-t8-coverage-delta.md` comparing the four [P0-T11] baseline counters against the four [P3-T5] final counters. Record the comparability precondition first: `FINAL-LINES-VALID` and `BASELINE-LINES-VALID` must be compared and their relation stated, because the denominator grows when new production lines are added and the two sides are only directly comparable when it does not. When the denominators differ, compare the two derived percentages instead and state that the percentage comparison is the one used. Acceptance: the artifact records baseline coverage, post-change coverage, and the new/changed-code coverage determination from [P3-T7], and states explicitly whether the repository-wide first-party line percentage decreased.

- [ ] [P3-T9] Record the post-format line count of every file this plan edited or created into `<FEATURE>/evidence/qa-gates/p3-t9-file-size-audit.md`, one `<path> = <count>` line per file alongside its [P0-T13] baseline. This audit runs after the final format because CSharpier can change line counts. Acceptance: every count is at or below 500 and the artifact states the smallest remaining headroom across all listed files.

- [ ] [P3-T10] Write `<FEATURE>/evidence/qa-gates/p3-t10-scope-boundary.md` enumerating the changed source set under the R7 pathspec and asserting the AC5 boundary. The artifact must list the anchored-diff output and the porcelain output side by side, because neither alone is correct in both states: an anchored diff cannot see an untracked path, and porcelain status goes empty once the change is committed. Acceptance: the enumerated set contains only the seven Write Set production paths, the four new and five modified test paths under `QuickFiler.Test/Controllers`, and `QuickFiler.Test/QuickFiler.Test.csproj`; and none of `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler/Controllers/QfcHomeController.Iteration.cs`, `TaskMaster/Ribbon/RibbonController.cs`, `TaskMaster/Properties/Settings.Designer.cs`, `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` appears in either output.

```powershell
git add --intent-to-add -- '*.cs' '*.csproj'
git diff --name-only $BaseSha -- '*.cs' '*.csproj'
git status --porcelain --untracked-files=all -- '*.cs' '*.csproj'
```

- [ ] [P3-T11] Check off AC1 in `spec.md` line 255 by changing its `- [ ]` to `- [x]`, citing the [P2-T14] pass-after artifact and the [P1-T16] fail-before artifact. Acceptance: exactly one AC checkbox changes in this task and the AC1 line carries `- [x]`.

- [ ] [P3-T12] Check off AC2 in `spec.md` line 256, citing [P1-T17], [P1-T18], [P1-T19] and [P2-T14], and recording that the live-Outlook confirmation is human-interaction exception HI-1 performed per `<FEATURE>/runbooks/live-outlook-cancel-teardown-verification.runbook.md` and does not gate the automated review. Acceptance: exactly one AC checkbox changes in this task and the AC2 line carries `- [x]`.

- [ ] [P3-T13] Check off AC3 in `spec.md` line 257 by writing `<FEATURE>/evidence/qa-gates/p3-t13-ac3-test-inventory.md` first, listing every test name that `spec.md` Test Strategy names alongside the file it now lives in and its pass result from [P2-T14], and confirming that fail-before and pass-after evidence exists for `DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance` and `TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing`. Acceptance: every Test Strategy test name maps to an existing file and a passing result, and exactly one AC checkbox changes in this task.

- [ ] [P3-T14] Check off AC4 in `spec.md` line 258, citing [P3-T1] through [P3-T6] for the toolchain order, [P3-T5] for `artifacts/csharp/coverage.xml`, and [P3-T7] and [P3-T8] for the coverage determinations. Acceptance: exactly one AC checkbox changes in this task and the AC4 line carries `- [x]`.

- [ ] [P3-T15] Check off AC5 in `spec.md` line 259, citing [P3-T10] and recording the R7 pathspec reading and its rationale so a reviewer does not read the narrower evaluation as an unstated relaxation. Acceptance: exactly one AC checkbox changes in this task and the AC5 line carries `- [x]`.

- [ ] [P3-T16] Check off AC6 in `spec.md` line 260, citing the supersession statements already present at `spec.md` lines 103-105 and lines 213-214 (which this plan does not modify) and the [P2-T3] evidence that `QuickFiler/Controllers/QfcHomeController.Iteration.cs` is unmodified, plus the passing `IterateQueueAsync_EmptyBatchWithScanCapReached_DoesNotCompleteAdding`. Acceptance: exactly one AC checkbox changes in this task and the AC6 line carries `- [x]`.

- [ ] [P3-T17] Update `spec.md` Status to `Implemented` and add an "Outcome" note under Rollout & Follow-up recording the three deviations this plan makes from the spec's own prose, each with its reason: the `ActionCancelAsync` trigger discriminator is a call-site log rather than a parameter (D3); `QfcDatamodel.QuiesceDebugLog` is an added internal test seam (D10); and the retargeting surface is seven tests rather than the four Test Strategy names (D2). Acceptance: the Status line reads `Implemented` and all three deviations are recorded by name.

- [ ] [P3-T18] Update `issue.md` with the outcome and mirror it to `<FEATURE>/evidence/issue-updates/issue-791.<timestamp>.md` per the evidence conventions, including the literal field lines `Timestamp:`, the exact text intended, and `PostedAs:`. Acceptance: both the local `issue.md` update and the mirror artifact exist and carry the same text.

- [ ] [P3-T19] Write `<FEATURE>/evidence/qa-gates/p3-t19-ac-status-summary.md` listing AC1 through AC6 with their final checkbox state and the artifact path that justifies each. Acceptance: six rows are present, each naming at least one existing artifact path, and every row's checkbox state matches the corresponding line in `spec.md`.

---

SELF-REVIEW: RE-DERIVED THIS PASS

Every citation below was read directly from this worktree in this authoring pass, and the sibling
region of each edited citation was re-checked. The sibling sweep is what produced D2 (three
deadline-dependent gate tests that `spec.md` Test Strategy does not name), D3 (`IFilerFormController.cs`
line 11 forbids an optional `trigger` parameter), D4 (`QfcFormControllerSeamTests.cs` requires the token
cancel to precede the first `await`), D5 (`QfcFormControllerTests.cs` loose mocks resolve
`KeyboardHandler` and `DataModel` to null), D7 (`QfcFormController.Deactivate.cs` line 24 remark becomes
false), and D11 (`QfcHomeController.cs` has 31 lines of headroom, which the three-block form exceeds).

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs | 262 lines; zero-acceptance deadline branch at lines 172-180; nine-parameter constructor at lines 111-125; DefaultFirstBatchDeadline at line 56; LogDeadlineExpiry at lines 242-250
CITATION: QuickFiler/Interfaces/IQfcDatamodel.cs | 133 lines; enum QfcDequeueStop at lines 30-40 with DeadlineExpired at lines 38-39; interface IQfcDatamodel at lines 83-132
CITATION: QuickFiler/Controllers/QfcDatamodel.cs | 480 lines; [ExcludeFromCodeCoverage] at line 25; Cleanup() at lines 75-91; Worker_DoWork at lines 175-213; TryQueueRemainingMailItemAsync at lines 350-361
CITATION: QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | 298 lines; public partial class QfcDatamodel at line 12; _remainingLoadActive at line 23; gate construction at lines 184-194
CITATION: QuickFiler/Controllers/QfcFormController.EventHandlers.cs | 408 lines; ButtonCancel_Click at lines 70-82 with throw; at line 80; ActionCancelAsync at lines 84-94; completion-path cancel at line 208
CITATION: QuickFiler/Controllers/QfcFormController.Deactivate.cs | 60 lines; unreachable-code remark at line 24; FormViewer_Deactivated at lines 26-58
CITATION: QuickFiler/Controllers/QfcHomeController.cs | 469 lines; InternalsVisibleTo at line 15; Cleanup() at lines 370-379; Worker_RunWorkerCompleted subscription at lines 91 and 131
CITATION: QuickFiler/Controllers/QfcHomeController.Iteration.cs | CompleteAddingAsync reachable only under SourceExhausted at lines 39-48
CITATION: QuickFiler/Interfaces/IFilerFormController.cs | Task ActionCancelAsync(); at line 11
CITATION: QuickFiler/Interfaces/IQfcCollectionController.cs | UnregisterNavigation() at line 109; ItemGroups at line 17
CITATION: QuickFiler/Interfaces/IQfcFormViewer.cs | UiSyncContext at line 17; Worker at line 18; IsWebView2Focused at line 64; ParkFocusOffWebView2 at line 70
CITATION: QuickFiler/Controllers/IQfcHomeController.cs | IQfcDatamodel DataModel { get; } at line 11
CITATION: QuickFiler/Controllers/QfcScanProgressBandMapper.cs | prose confirming QfcDatamodel is [ExcludeFromCodeCoverage] at line 12
CITATION: QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs | 477 lines; fail-closed nine-type constructor lookup at lines 53-77
CITATION: QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs | 465 lines; four superseded tests at lines 76-121, 124-144, 205-228, 346-385; total-count assertion at line 384
CITATION: QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs | 280 lines; two superseded tests at lines 92-127 and 174-208
CITATION: QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | 413 lines; superseded test at lines 201-260
CITATION: QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs | 477 lines; DeadlineExpired pin at lines 394-406; SourceExhausted control at lines 412-424
CITATION: QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs | 248 lines; construction seam at lines 60-70; group injection at lines 79-92
CITATION: QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs | CancelClicked_WhenRaised_CancelsParentTokenSource at lines 162-179
CITATION: QuickFiler.Test/Controllers/QfcFormControllerTests.cs | loose-mock setup at lines 89-100; ButtonCancel_Click_ShouldCancelAction at lines 392-403
CITATION: QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs | Cleanup_ExecutesCorrectly at lines 79-103
CITATION: QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs | uninitialized-object seam at lines 35-45; bounded condition wait rationale at lines 47-57
CITATION: QuickFiler.Test/QuickFiler.Test.csproj | gate Compile Include entries at lines 165-167; QfcHomeControllerIterationTests.cs at line 169
CITATION: QuickFiler.Test/packages.config | Microsoft.Extensions.TimeProvider.Testing 10.9.0 at lines 85-89; Moq 4.20.72 at line 112
CITATION: coverage.config | module excludes at lines 12-22 with no Test.dll entry
CITATION: .gitignore | artifacts/ at line 57; coverage/* at line 144
CITATION: .github/workflows/_mstest-coverage.yml | assembly discovery at lines 86-96; run switches at line 99
CITATION: .claude/hooks/enforce-evidence-locations.ps1 | artifacts/csharp/ permitted at lines 22-26; forbidden prefixes at lines 64-74
CITATION: docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/spec.md | Acceptance Criteria AC1 through AC6 at lines 255-260; Write Set at lines 141-164; Test Strategy at lines 219-252
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6
AC-MAPPING: AC1 | IMPLEMENTATION: P2-T1, P2-T2, P2-T3 | TESTS: P1-T6, P1-T8, P1-T9, P1-T10, P1-T11 | EVIDENCE: <FEATURE>/evidence/regression-testing/p1-t16-gate-fail-before.md and <FEATURE>/evidence/regression-testing/p2-t14-pass-after.md
AC-MAPPING: AC2 | IMPLEMENTATION: P2-T4, P2-T5, P2-T6, P2-T7, P2-T8, P2-T9, P2-T10, P2-T11, P2-T12 | TESTS: P1-T12, P1-T13, P1-T14 | EVIDENCE: <FEATURE>/evidence/regression-testing/p1-t17-cancel-teardown-fail-before.md, p1-t18-home-cleanup-fail-before.md, p1-t19-datamodel-teardown-fail-before.md and p2-t14-pass-after.md
AC-MAPPING: AC3 | IMPLEMENTATION: P1-T6, P1-T12, P1-T13, P1-T14 | TESTS: P2-T14, P3-T13 | EVIDENCE: <FEATURE>/evidence/qa-gates/p3-t13-ac3-test-inventory.md
AC-MAPPING: AC4 | IMPLEMENTATION: P3-T1, P3-T2, P3-T3, P3-T4, P3-T5 | TESTS: P3-T5, P3-T6 | EVIDENCE: <FEATURE>/evidence/qa-gates/p3-t5-tests-coverage.md, p3-t7-changed-line-coverage.md and p3-t8-coverage-delta.md
AC-MAPPING: AC5 | IMPLEMENTATION: P2-T3 | TESTS: P3-T10 | EVIDENCE: <FEATURE>/evidence/qa-gates/p3-t10-scope-boundary.md
AC-MAPPING: AC6 | IMPLEMENTATION: P2-T3 | TESTS: P1-T11, P3-T16 | EVIDENCE: <FEATURE>/evidence/qa-gates/p3-t10-scope-boundary.md and <FEATURE>/evidence/regression-testing/p2-t14-pass-after.md
UNRESOLVED-GAPS: NONE
DIRECTIVE: PREFLIGHT VALIDATION ONLY
PREFLIGHT: REQUESTED — validation-only preflight has NOT been run by this planner, because no atomic-executor delegation tool and no MCP plan validator are present in this planner's tool surface. The orchestrator must obtain one of the two exact signals, `PREFLIGHT: ALL CLEAR` or `PREFLIGHT: REVISIONS REQUIRED`, and a passing `mcp__drm-copilot__validate_orchestration_artifacts` run with `artifact_type: "plan"` before execution begins. This plan is not self-approved.
CONVERGENCE: FURTHER ROUNDS LIKELY — the plan asserts three literals it also creates (`High-confidence dequeue launch`, `Zero-acceptance checkpoint`, `Loader quiesce timed out`), which acceptance-gate rule G5 evaluates against the tracked tree, and three files land within twenty lines of the 500-line ceiling, so a reviewer is likely to request tightening on at least one of those axes.
