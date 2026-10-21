# Feature Audit — breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers (#781)

- Review timestamp: 2026-09-05T17-29
- Work mode: `minor-audit`

## Scope and Baseline

| Item | Value |
|---|---|
| Resolved base branch | `main` (the only candidate branch; equals `origin/main`) |
| Merge base with HEAD | `a007f72e394ee3038c6c52bfdf91f007df96fd6c` |
| Branch under review | `bug/breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781` |
| Head SHA | `4f74aa39799dca7233bcf286dac90e8691eabd99` |
| Audit range | `a007f72e394ee3038c6c52bfdf91f007df96fd6c..4f74aa39799dca7233bcf286dac90e8691eabd99` |
| Paths in range | 44 |
| PR context freshness | Fresh. `artifacts/pr_context.summary.txt` records Head SHA `4f74aa39799dca7233bcf286dac90e8691eabd99`, equal to `git rev-parse HEAD`. Its changed-file classification is defective and was not used for scope; see policy audit finding F4. |
| Scope basis | The full feature-versus-base diff, recomputed with `git diff --numstat`. No caller narrowing was accepted. |

Production and test change surface:

| Path | Status | Lines |
|---|---|---|
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | modified, production | +24 / -17 |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` | added, test | +419 |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | modified, test | +3 / -95 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | modified, build configuration | +1 |

The remaining 40 paths are feature-folder documents and evidence (30), the promoted potential-feature
record (1), agent memory (9), and the deletion of one stale tracked orchestration state file.

## Acceptance Criteria Inventory

- Source file: `docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/issue.md`
- Source selection: `minor-audit` work mode, so the explicit `## Acceptance Criteria` section of
  `issue.md` is the sole authoritative source. `spec.md` and `user-story.md` do not exist in this
  feature folder, which is correct for this mode.
- Marker verified present: `- Work Mode: minor-audit`
- Heading verified present: `## Acceptance Criteria`
- Total AC items: 8 (AC1 through AC8), each appearing exactly once in checkbox form
- State on entry to this review: all 8 already marked `[x]` by the executor

## Acceptance Criteria Evaluation

### AC1 — guard proves ownership by owner-thread identity, no context reference comparison

**PASS.** `ThrowIfOffUiBoundary` now reads `Dispatcher owning = UiDispatcher;` and branches on
`!owning.CheckAccess()`. `UiDispatcher` is backed by `_uiDispatcher`, assigned
`Dispatcher.CurrentDispatcher` in the `ItemViewer` constructor
(`QuickFiler/Viewers/ItemViewer.cs` line 27), so the dispatcher is the one belonging to the
constructing thread. A grep of the changed file for
`ReferenceEquals(SynchronizationContext.Current` returns zero matches, and `using System.Threading;`
has been removed from the file. `Dispatcher.CheckAccess()` compares `Thread` object references,
which is a stricter form of owner-thread identity than the id comparison the AC permits as one
option.

### AC2 — guarded members succeed on the owning thread under any ambient context

**PASS.** All four guard sites route through the single private method: line 51
(`InitializeBreadcrumbPipeline`), lines 172 and 229 (both `ConfigureBreadcrumbDropDown` overloads),
and line 389 (`EnsureBreadcrumbResourceOwnership`). The last is reached from
`InitializeBreadcrumbPipeline` through `EnsureBreadcrumbLifecycle` (line 361), so the owning-thread
tests exercise it.

All three ambient shapes the AC enumerates are covered by a passing test that the reviewer executed
in this session:

| Ambient shape at the call site | Test | Result |
|---|---|---|
| `DispatcherSynchronizationContext` from a WPF dispatcher operation | `InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_...` and `ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow` | Passed |
| null | `InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow` | Passed |
| a different plain `SynchronizationContext` instance | `InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow` | Passed |

### AC3 — cross-thread call still throws `InvalidOperationException` naming the operation

**PASS.** The `throw` retains `InvalidOperationException` and the message still interpolates
`{operation}`, which every call site supplies via `nameof(...)`. Two tests assert the contract
directly, each with `.Where(error => error.Message.Contains("<member name>"))` and an explicit
`.Which.Should().NotBeOfType<ObjectDisposedException>()`:
`InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` and
`ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic`. Both passed in the reviewer's
run, and both also passed against the pre-fix guard, which confirms the fail-fast contract was
preserved rather than weakened by the change.

### AC4 — deterministic regression test reproducing the production shape

**PASS.** `InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext`
installs a plain `SynchronizationContext`, constructs the viewer inside
`Dispatcher.CurrentDispatcher.Invoke((Action)(...))`, and then calls
`InitializeBreadcrumbPipeline` on the same thread outside that operation. It asserts non-vacuity
before acting, by requiring that the viewer's captured context is not reference-equal to the ambient
context.

Determinism requirements are met: no sleep, timer, wall-clock wait, or temporary file appears in the
file; the same-thread `Dispatcher.Invoke(Action)` fast path runs the callback inline at
`DispatcherPriority.Send` and needs no message pump; the ambient context is restored in a `finally`.
The reviewer observed the test fail against the pre-fix guard and pass against the fixed guard.

### AC5 — old reference-comparison tests replaced; #488 D3 and D5 behaviours intact

**PASS.** Both named tests are deleted from
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`
(`InitializeBreadcrumbPipeline_AmbientContextNull_ThrowsBoundaryDiagnostic` and
`InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic`, 94 lines removed),
and the suite now asserts the thread-identity contract through the new class.

The D3 and D5 tests are unchanged and were re-executed by the reviewer:
`InitializeBreadcrumbPipeline_SecondDifferentProvider_ThrowsInvalidOperationException` (D3) and
`InitializeBreadcrumbPipeline_AfterViewerDisposed_ThrowsObjectDisposedException` (D5) both Passed,
as did the four other retained tests in the class. Removing the two deleted tests is correct rather
than a coverage retreat: both asserted the defective behaviour and could not have been retained
without failing.

### AC6 — XML documentation states the thread-identity contract and why context equality was unsuitable

**PASS, with a documentation-accuracy note.** The rewritten `<remarks>` states that ownership is
proved through `Dispatcher.CheckAccess()` on the constructor-captured dispatcher, and explains why
the previous test was unsuitable: a WPF dispatcher operation installs a
`DispatcherSynchronizationContext` for the duration of its callback, so a viewer constructed inside
one captures a context that is never the UI thread's ambient context again. Both clauses the AC
requires are present. The `EnsureBreadcrumbResourceOwnership` statement-order comment was updated in
the same commit and remains accurate: it now reasons about the owning dispatcher being null rather
than about `UiSyncContext` being null, preserving the FIRST STATEMENT / FIRST ACTION argument it
documents.

Note, recorded as code-review finding CR-4 and not affecting this verdict: the supporting sentence
"Managed thread ids are unique among live threads" describes a mechanism the code does not use.
`Dispatcher.CheckAccess()` compares `Thread` object references, so id recycling is not a
consideration at all. The AC's required content is present; the supporting argument understates the
guard.

### AC7 — scope limited to `QuickFiler/Viewers/ItemViewer*.cs` and their tests

**PASS.** Independently verified by the reviewer with
`git diff --numstat a007f72e..4f74aa39`, not by reading the executor's scope-boundary artifact. The
C# change surface is exactly four paths, listed in the Scope and Baseline section above. Zero paths
under `QuickFiler/Controllers/` appear in the branch diff, so `QfcCollectionController.cs`,
`QfcItemController.FolderHandling.cs`, and `QfcItemController.ViewerSetup.cs` — the three files AC7
names explicitly — are unchanged.

One non-code path outside the write set is present in the branch: the deletion of the tracked,
gitignore-matched `artifacts/orchestration/orchestrator-state.json`. It is not a source or test file
and does not bear on AC7's subject, which is the fix's code scope; it is recorded separately as
policy-audit finding F5 and code-review finding CR-6.

### AC8 — full C# toolchain in one consecutive pass, >= 90% new-code line coverage, canonical coverage artifact produced

**PASS, with a recorded deviation on the coverage clause.**

Toolchain clause — satisfied and independently corroborated:

| Stage | Command | Exit |
|---|---|---|
| Format | `dotnet tool run csharpier check .` | 0, zero files needing formatting |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0, 3 warnings equal to baseline, 0 errors, 36 `csc.exe` invocations so the gate was live |
| Nullable and type check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0, 3 warnings equal to baseline, 0 errors, `/p:Nullable=enable` correctly omitted and `/t:Rebuild` correctly used |
| Test | `vstest.console.exe ... /EnableCodeCoverage /InIsolation` | 0; repository-wide suite 6997 of 6997 passed |

No stage modified a file, so the four stages completed in one consecutive pass without a restart.
The reviewer re-ran the format stage over the two changed directories (exit 0) and the test stage
over the four affected classes (33 of 33 passed).

Canonical artifact clause — satisfied. `artifacts/csharp/coverage.xml` exists, 12,732,521 bytes,
mtime 2026-09-05 17:10, Cobertura root
`line-rate="0.848316" branch-rate="0.791421" lines-covered="54920" lines-valid="64740"`.

Coverage clause — **deviation**. No percentage exists for the changed production lines. The reviewer
independently enumerated every `<class>` element in the baseline and post-change Cobertura documents
and found zero whose `filename` ends `ItemViewer.Breadcrumb.cs` in either. The cause is
`[ExcludeFromCodeCoverage]` on the `ItemViewer` partial class declaration at
`QuickFiler/Viewers/ItemViewer.cs` line 20, which applies to the whole type. The attribute predates
this branch (the query returns zero on the baseline document as well) and is the maintainer-ratified
CLAUDE.md UT2(b) WinForms form-derived exemption.

The verdict is PASS rather than PARTIAL because the substitute evidence is branch-complete and was
independently verified. The rewritten method has exactly three reachable outcomes, and a named,
passing test exists for each:

| Outcome of the rewritten guard | Covering test | Reviewer-observed pre-fix result |
|---|---|---|
| Null owner: returns without effect | `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` | Failed |
| Non-null owner, `CheckAccess()` true: returns | `..._ConstructedInsideDispatcherOperation_...`, `..._OwningThreadNullAmbientContext_...`, `..._OwningThreadDifferentPlainContext_...`, `ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_...` | Failed (all four) |
| Non-null owner, `CheckAccess()` false: throws | `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic`, `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` | Passed (corroborating) |

Five of the seven tests were observed by the reviewer to move from Failed to Passed across the guard
swap, so they discriminate on the changed behaviour rather than merely execute it.

The deviation is recorded so a maintainer can overrule. If a measured number is required rather than
branch-complete behavioural evidence, the only remedy is to extract the guard logic out of the
`UserControl`-derived type into a host-neutral class, which is a separate work item and not a defect
in this change.

Separately, the repository-wide line coverage of 84.8316% is below the 85% uniform floor in
`.claude/rules/quality-tiers.md` and above the 80% floor in `CLAUDE.md`. That row is recorded FAIL
with a non-blocking disposition in policy-audit section 5.2 and finding F3. It is pre-existing at
the merge base (0.848347) and no `QuickFiler` class differs between the two documents, so it does
not bear on AC8, which concerns this change's own new code.

## Summary

| Verdict | Count | Items |
|---|---|---|
| PASS | 8 | AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8 |
| PARTIAL | 0 | — |
| FAIL | 0 | — |
| UNVERIFIED | 0 | — |

All eight acceptance criteria pass. AC6 and AC8 carry recorded notes that do not change their
verdicts: AC6 a documentation-accuracy note (code-review CR-4), AC8 the coverage-measurability
deviation (policy-audit F2).

The defect reported in issue #781 is fixed and the fix is proven rather than asserted. The reviewer
restored the pre-fix production file from the merge base and reproduced the reported failure mode in
five of the seven new tests, then restored the file and confirmed by SHA-256 that the working tree
was returned to the committed content byte for byte.

Blocking findings: 0. Remediation is not required and no `remediation-inputs` artifact is emitted.

**Recommendation: GO for PR.**

Residual items owed, none blocking this PR:

1. Correct the causal attribution and the `coverage.config` citation in
   `evidence/qa-gates/coverage-delta.2026-09-05T10-49.md` (code-review CR-1, CR-2).
2. Promote the `UtilitiesCS.UiThread.SynchronizationContextAwaiter.IsCompleted` observation from
   `issue.md` prose to a GitHub issue before the feature folder is archived (policy-audit F6,
   code-review CR-8).
3. Mention the `artifacts/orchestration/orchestrator-state.json` deletion in the PR body
   (policy-audit F5, code-review CR-6).
4. Optionally remove the unused `DrainableSynchronizationContext.Drain()` and restate the
   `ThrowIfOffUiBoundary` `<remarks>` thread-id sentence (code-review CR-3, CR-4).

## Acceptance Criteria Check-off

All eight criteria were already marked `[x]` in `issue.md` by the executor before this review began.
This review verified each independently against the branch diff and against tests it executed
itself, and confirms every check-off is warranted. No checkbox was changed by this review, because
none required changing: there is no criterion whose recorded state disagrees with the reviewer's
evaluation.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/issue.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none
