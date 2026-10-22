# Code Review — Issue #839 (createcancellationtoken-has-no-production-caller)

- Timestamp: 2026-09-13T10-40
- Branch: bug/createcancellationtoken-has-no-production-caller-839
- Head: 36c65b1875f89c0e3d885dc52324b60959ae3dbb
- Merge base with main: 2405a829d6afd3b12eb7c228d57158a97cb4e2ca
- Files reviewed: `QuickFiler/Controllers/QfcHomeController.cs` (+1/-2), `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` (+71/-0)
- Review method: full read of both changed files in the item worktree; the caller's verbatim anchored diff; targeted reads of `QfcFormController.Actions.cs` (guards), `EfcHomeController.cs` (precedent), `RibbonController.cs` (dead caller), both `IQfcHomeController.cs` files, `scripts/vscode/TaskMaster.cli.runsettings`, `.github/workflows/_mstest-coverage.yml`, and every evidence projection under the feature folder. No shell or git command was run.

## Executive Summary

Verdict: **PASS**. Zero blocking findings. Four Low findings and three informational observations, none requiring a change on this branch.

The production change is one statement, placed at the only position that establishes the invariant for all three loaders (`this.Token` is read by the datamodel loader at line 89 and the queue loader at line 95 before the form-controller loader receives `_tokenSource` at line 103). The deleted comment (`//public QfcFormViewer FormViewer { get => _formViewer; }`) had no reader; deleting the adjacent blank line with it is what CSharpier would have produced anyway, confirmed by the format pass reporting `FORMAT_CHANGED_OWNED_PATCH=False`. The regression test is well constructed: it captures every token argument the loaders receive, asserts identity against the controller's own `TokenSource`, and pins the ordering rule through `CanBeCanceled` rather than through a positional assertion, so a fix inserted after the datamodel loader would fail it. It uses MSTest, Moq and FluentAssertions only, contains no banned waiting API, and documents its purpose and its inherited debt.

The defect is correctly characterised throughout as latent: `RibbonController.LoadQuickFiler()` (RibbonController.cs:97-110) is the only production caller of `Init()`, and a repository-wide word-bounded grep for `LoadQuickFiler` in `.cs`, `.xml`, `.vb`, `.ps1` and `.json` files returned only that declaration plus archived coverage documents.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | QuickFiler.Test/Controllers/QfcHomeControllerTests.cs | 223-233 (`Init_CreatesTokenSourceBeforeAnyLoaderObservesIt`) | `_controller.Cleanup()` runs after the seven assertions with no `try`/`finally`. If any assertion throws, the `CancellationTokenSource` created by `Init()` and the real `QfcFormViewer` are not cleaned up for that test instance. | When the dead synchronous path is removed (remedy d) this test goes with it; if it is retained, wrap the assertions in `try { ... } finally { _controller.Cleanup(); }`. No change required on this branch. | AC7 requires only that `Cleanup()` is called after the assertions, which it is. The sibling `Init_InitializesCorrectly` has never cleaned up at all, so the new test is strictly better than the file's existing pattern. A leaked source with no timer is GC-collectible; the impact is bounded to a failing run. | Reviewer read; spec.md:166, 217. |
| Low | QuickFiler/Controllers/QfcHomeController.cs | 86-88 (`Init()`) | After the fix, a second `Init()` call on the same instance allocates a new `CancellationTokenSource` and overwrites `_tokenSource` without disposing the previous one. Before the fix this path allocated nothing. | None on this branch. Note it in the remedy (d) follow-up, which deletes `Init()`; if `Init()` were ever retained, guard with `_tokenSource?.Dispose();` before the factory call or reject re-entry. | `Init()` is a one-shot initializer returning `this` for chaining; `InitAsync` has the same overwrite-without-dispose shape (line 118) and `EfcHomeController` calls the identical factory on every construction path (lines 62, 126, 162). This is a pre-existing pattern the fix aligns with, not a new class of hazard, and the path has had no live caller since 2024-09-27 (spec.md:238). | Reviewer read of QfcHomeController.cs:86-107, 109-151, 466-470; reviewer grep of EfcHomeController.cs. |
| Low | QuickFiler.Test/Controllers/QfcHomeControllerTests.cs | 112-163 (`Init_InitializesCorrectly`) | This pre-existing test now allocates a `CancellationTokenSource` through `Init()` and never disposes it (it has no `Cleanup()` call). | Optional: add a trailing `_controller.Cleanup()` in a later change. AC8 explicitly permitted this edit and the plan's Decision D2 declined it to keep the test-file hunk a pure insertion with zero deleted lines. | Deliberate, documented trade-off; the AC8 gate (`,0 +` hunk, numstat deleted 0) is stronger evidence of non-tampering than a cosmetic disposal fix would be worth. GC-collectible. | test-file-gates.md:44-50; plan Decision D2. |
| Low | docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence | coverage-baseline.md, coverage-comparison.md | Only line figures were transcribed; no branch figure appears in any projection although the Cobertura document carries `branch-rate`. | Future plans that discard raw Cobertura should transcribe `branch-rate` alongside `line-rate` in the same projection. No action on this branch. | The inserted statement is a straight-line call with no conditional, so this diff cannot move any branch figure; the gap is in reporting, not in the change. | coverage-comparison.md:10-16 (line metrics only). |
| Info | scripts/vscode/TaskMaster.cli.runsettings (unchanged on this branch; outside the Write Set) | lines 3-8 | The file's entire content is `<MSTest><Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest>`. Under it, three `QfcInitEmailQueueZeroBatchTests` fail with a Deedle `TypeInitializationException`; without it the same assembly passes 1393/1393 and 1394/1394. The CI workflow passes no settings file (`_mstest-coverage.yml:99`), so the repository's documented local runner (`Invoke-MSTestWithCoverage.ps1`, which appends the file at its inner vstest call) diverges from CI. | File a separate item to remove or scope the parallelisation element and re-align the local runner with CI. Not a finding against this branch: the plan's Decision D16 stopped passing the file, both sides of every comparison were measured without it, and neither file is in this item's Write Set. | Confirmed by the reviewer reading the runsettings file and the CI workflow; the executor's superseded RED baseline record and the isolating sibling observation are described at baseline-quickfiler-tests.md:32-42. | TaskMaster.cli.runsettings:3-8; _mstest-coverage.yml:99; Invoke-MSTestWithCoverage.ps1:33, 48, 69. |
| Info | docs/features/active/* (other feature folders; outside this footprint) | evidence trees of #501, #498, #468, #446 and others | More than 100 `.trx` files are tracked under other active feature folders' evidence trees (Glob `docs/features/**/*.trx` in the item worktree truncated at 100 results; the executor counted 332), contrary to the issue-671 projections-only decision. None is in this branch's footprint and none was written by this run (newest write time precedes this session per init-token-source-fail-before.md:49-52). | File a separate cleanup item scoped to those feature folders. Pre-existing; not a finding against this branch. | This branch's own evidence is projections-only (no `.trx`, `.xml` or `.coverage` in the 43-path diff). | Reviewer Glob; init-token-source-fail-before.md:39-54. |
| Info | docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/spec.md | line 33 and line 236 | The spec cites `IQfcHomeController.Init()` at "IQfcHomeController.cs line 12". Two files carry that name: `QuickFiler/Controllers/IQfcHomeController.cs` declares `IQfcHomeController Init();` at line 12 (the intended one); `QuickFiler/Interfaces/IQfcHomeController.cs` is a different interface in a different namespace and declares no `Init()`. | When filing the remedy (d) follow-up, name `QuickFiler/Controllers/IQfcHomeController.cs` explicitly so the interface-member removal targets the right file. No spec edit required by this review. | Avoids a future executor removing a member from the wrong interface. | Reviewer read of both files. |

No finding in this table is blocking. No `remediation-inputs` artifact is produced.

## Detailed Review

### Production change: `QfcHomeController.Init()` (QfcHomeController.cs:86-107)

- Placement is correct and load-bearing. `this.Token` is consumed at line 89 (datamodel loader) and line 95 (queue loader) before `this._tokenSource` is consumed at line 103 (form-controller loader). Any later insertion point would leave the first two holding `default(CancellationToken)` (`CanBeCanceled == false`), which is the quieter version of the same defect the spec describes (spec.md:104).
- The fix establishes the invariant that the three early-return guards in `QfcFormController.Actions.cs` (lines 38, 75, 131, `|| _tokenSource is null`) protect, without touching the guards. The spec's inverse constraint (guards must stay; removing them relocates the failure into `QfcCollectionController` and `QfcItemController`) is sound and was respected.
- The synchronous path is now structurally identical to `InitAsync` (lines 117-118 assign token and source before any loader runs) and to `EfcHomeController` (factory called at lines 62, 126 and 162). Consistency with the precedent was verified by grep.
- Disposal ownership is unchanged and adequate: `Cleanup()` disposes and nulls the source at lines 390-391, and `Cleanup` is passed to the form controller as `parentCleanup` at line 101.
- No nullable directive, no signature change, no analyzer suppression, no new dependency.

### Deleted lines (QfcHomeController.cs, base lines 465-466)

- `//public QfcFormViewer FormViewer { get => _formViewer; }` was a commented-out property with no reader. Its deletion is the only way to add a statement while staying at or under the 500-line ceiling without removing executable code. Choosing this candidate over the commented debug log at line 41 keeps every line number between 41 and 465 stable, which is why the coverage gate can name line 88 on both sides.
- The blank-line deletion is formatter-canonical; the executor's hand edit matched CSharpier's output exactly (final-format.md:29).

### Regression test: `Init_CreatesTokenSourceBeforeAnyLoaderObservesIt` (QfcHomeControllerTests.cs:165-234)

- Arrangement mirrors the sibling `Init_InitializesCorrectly` (five loader replacements) and additionally captures the token arguments, which the sibling never did; that omission is why the defect went undetected.
- Assertions (lines 224-230) cover exactly what AC3 enumerates: not-null source; form-controller token equals source token; `TokenSource` is the same instance; datamodel and queue tokens equal the source token; both are cancellable. The `CanBeCanceled` assertions are the ordering pin.
- Fail-before evidence shows the first assertion failing with `Expected capturedSource not to be <null>.` at line 224 against the unfixed assembly (init-token-source-fail-before.md:27, 31); scoped and whole-assembly pass-after runs show all seven passing on the fixed assembly.
- Determinism: no timers, no wall clock, no filesystem, no Outlook interop object created by the test. The `MockBehavior.Strict` repository in `Setup()` is pre-existing; `Init()` does not call anything on `Globals` that the strict mock would reject (the run proves this).
- Declared UT4 exception: `Init()` constructs a real `QfcFormViewer` (QfcHomeController.cs:91). The test's doc comment declares this as inherited debt shared with the sibling test; spec.md:170-172 declares it per UT5. The reviewer confirms it is pre-existing (the sibling test at line 148 constructs the same viewer) and introduces no new category of behaviour.
- Naming: descriptive and behaviour-oriented. Doc comment names #839, the ordering rule and the expected failure mode of a wrong insertion point.

### Test-file hygiene

- Pure insertion: one hunk `@@ -164,0 +165,71 @@`, numstat 71/0 (test-file-gates.md:44-48), so `Init_InitializesCorrectly`, `InitAsync_InitializesCorrectly` and every commented-out legacy block are byte-identical to the base. The reviewer's read of the file is consistent with that: the sibling test retains its five loaders and four `Assert.AreEqual` calls (lines 121-162).
- File length 346, under the 500-line ceiling (file-size-audit.md:11; reviewer read).

### Evidence quality

- Every artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:` and an output summary; the single expected-failure artifact carries `ExpectedExitCode: 1`.
- Transport adaptations (`Set-Location -LiteralPath "REPO-ROOT"` prefix; the `[char]92` separator normalisation in the Cobertura filename predicate; the single-operand `-replace ":"`; the `[char]34` quote construction) are each explained with a probe result and an assertion that semantics are unchanged. The reviewer accepts these as semantics-preserving; each is applied identically to both sides of every comparison.
- The Cobertura per-file method (de-duplicate by line number across all class elements sharing the filename, union of class-level and method-level views, max hits per line) matches the repository helper's documented rule and avoids the double-count trap.
- The msbuild gates record `CSC_TASK_LINES=18`, proving `CoreCompile` ran on every project under `/t:Rebuild`; neither diagnostic gate is vacuous.
- Both the analyzer and nullable gates omit `/p:Nullable=enable`, as CLAUDE.md requires.
- The executor's superseded RED baseline was overwritten in place with a GREEN record that explains the supersession and corrects its own earlier root-cause attribution (baseline-quickfiler-tests.md:32-42). This is the right handling: the earlier record's inference was recorded, then falsified by varying the one variable it had not varied.
- Sanitisation: zero account-name or machine-name hits across 34 evidence files, with a positive control of 19578 hits in a gitignored msbuild log (evidence-sanitization.md:9-18). The reviewer's independent grep of the whole feature folder for account and path patterns also returned zero hits.

### Out-of-radius observations (not findings against this branch)

See the Info rows in the findings table: the runsettings parallelism divergence from CI, the tracked `.trx` files under other feature folders, and the ambiguous `IQfcHomeController.cs` citation.

## Verdict

PASS. Ready to merge. Zero blocking findings.
