# Policy Compliance Audit — quickfiler-bug-family-446

- Reviewer: feature-review agent
- Timestamp: 2026-08-26T11-29
- Branch: `bug/quickfiler-bug-family-446` (epic child; PR target `epic/quickfiler-bug-family-integration`)
- Merge base: `61edc19befcf6c4e95b5acd32542f2dcdab41b78` (independently recomputed via `git merge-base HEAD origin/epic/quickfiler-bug-family-integration`; matches the caller-supplied SHA)
- Head: `fd746f558c210119fd625bbf1bb7f43d9e43e7d2` (11 commits)
- Work mode: `full-bug` (AC source: `spec.md` only)
- Audit scope: the full branch diff `61edc19b...fd746f55` — 161 files (13 C# source/test files, 4 promoted potential documents, 144 feature-folder documentation/evidence files)
- CI note: `ci.yml` triggers only on main/development; this PR receives zero CI checks. Local toolchain evidence is the only gate and was weighted accordingly.

## Rejected Scope Narrowing

None. The caller requested the full feature-vs-base audit and supplied no scope restriction. Two caller artifact-path constraints (do not create `artifacts/csharp/coverage.xml`; do not retain helper scripts under `evidence/`) are hygiene constraints consistent with this agent's own conventions, not scope narrowing, and were honored.

## 1. General Code Change Policy

| Item | Verdict | Evidence |
| --- | --- | --- |
| Bugfix Workflow: failing regression test first, per defect | PASS | Ten deliberately-RED tests across Phases 1 and 3, each with a fail-before TRX and pass-after TRX under `evidence/regression-testing/p1-*` and `p3-*`. Sampled independently: `p1-t9/p1-t9.trx` and `p1-t10/p1-t10.trx` record `outcome="Failed"` for the pre-fix gate stop-reason tests; the final run records all green. |
| Minimal, targeted fix; no opportunistic refactor widening | PASS | Production diff confined to 6 owned files named in `issue.md`. Deeper problems found during the work were routed to four new promoted potential documents (`docs/features/potential/promoted/2026-08-26-*.md`) instead of widening scope. |
| File size limit (500 lines) | PASS | All 13 changed `.cs` files measured with `awk END{print NR}`: max is 497 (`QfcHomeControllerIterationTests.cs`); production max is 480 (`QfcDatamodel.cs`). Pre-existing 827-line `QfcFormControllerTests.cs` untouched (verified absent from the diff). |
| Error handling: fail fast; no silent broad catch | PASS with note | Two broad `catch (System.Exception)` sites added (`QfcStreamingDequeueConfidenceGate.DequeueAsync` rejection-sink guard; `QfcDatamodel.QueueProcessing.TryReleaseRejectedHook`). Both are boundary guards that log with context via log4net and continue by documented design (a discarded candidate must not abort the batch scan). Compliant with the "clear boundary with added context" exception. See code review CR-2 for the redundancy note. |
| Logging via project pattern | PASS | log4net used in all new production error paths; no ad-hoc console output. |
| I/O isolation; no temp files in tests | PASS | New tests use Moq doubles, `FakeTimeProvider`, and injected seams. Grep of all 7 changed test files for `GetTempPath`/`GetTempFileName`: zero matches. |
| Supporting documents updated | PASS | `plan.2026-08-24T09-37.md` fully checked off (100 tasks); `spec.md` checkbox state maintained; 5 issue-update evidence records under `evidence/issue-updates/`. |

## 2. C# Code Change Policy

| Item | Verdict | Evidence |
| --- | --- | --- |
| CSharpier formatting (pinned, via `dotnet tool run`) | PASS | `evidence/qa-gates/p5-t9-clean-pass.2026-08-26T11-11.md`: scoped format then repo-wide `csharpier check .`, both EXIT_CODE 0; rewritten-file count 0 verified by SHA-256 digest comparison in the accepted pass. |
| Analyzer gate (`/t:Rebuild`, analyzers on) | PASS | `p5-t3-analyzer-build.2026-08-26T10-59.md` EXIT_CODE 0. Command string uses `/t:Rebuild`, not `/t:Build`. |
| Nullable/type-check gate (`TreatWarningsAsErrors`, no `/p:Nullable=enable`) | PASS | `p5-t4-nullable-build.2026-08-26T11-00.md` EXIT_CODE 0; command matches the CLAUDE.md-approved form; `/p:Nullable=enable` absent. |
| MSTest via vstest with coverage | PASS | `p5-t5-vstest.2026-08-26T11-01.md` EXIT_CODE 0 with `/InIsolation /EnableCodeCoverage`; TRX counters independently re-read from `evidence/qa-gates/p5-t5/p5-t5.trx`: total 6501, passed 6501, failed 0, error 0, timeout 0, aborted 0. |
| net481 constraints respected | PASS | New `QfcGateBatch` and `QfcDequeueBatch` declared as `readonly struct` with get-only properties and null-tolerant accessors; no `init`/`record` (CS0518 trap avoided). |

## 3. Unit Test Policy (General + C#)

| Item | Verdict | Evidence |
| --- | --- | --- |
| MSTest + Moq + FluentAssertions | PASS | All new tests use `[TestClass]`/`[TestMethod]`, Moq mocks, FluentAssertions with `because` messages. |
| Determinism: no banned APIs | PASS | Grep of the 7 changed test files for `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`: the only match is an XML doc comment naming `Task.Delay` as the thing being avoided. All async timing is driven by `FakeTimeProvider` / `CountingTimeProvider`. |
| Arrange–Act–Assert with documented intent | PASS | Sampled new tests carry AAA markers and scenario doc comments (some markers merged with the arrange comment after the [P3-T7] compaction; structure retained). |
| Independence / isolation / no external deps | PASS | No live COM: the `UndoItemProcessor` and `ScoringServiceFactory` seams keep Outlook and the WinForms dispatcher out of the unit under test. |
| Pre-existing tests as spec — modifications examined | PASS | `IterateQueueAsync_QueueEmpty` modification adjudicated legitimate (Section 6 and the feature audit); `DequeueAsync_BelowThresholdItemsAreDiscarded` byte-unchanged (hunk map verified: nearest hunks at base lines 285 and 324, method body at 298–310 untouched); `EmailMoveMonitorTests.cs` and the two overload-selection pin files absent from the diff. |

## 4. Coverage Verification (mandatory, per language with changed files)

Languages with changed files in the branch diff, from `git diff --numstat` and the regenerated `artifacts/pr_context.summary.txt`: **C# only** (13 `.cs` files). Zero `.ts/.tsx`, `.py`, `.ps1/.psm1` files changed on the branch, so TypeScript, Python and PowerShell have no changed files and therefore no coverage obligation on this branch.

Coverage evidence source: committed Cobertura artifacts `evidence/baseline/coverage-baseline.cobertura.xml` (Phase 0 baseline, same session) and `evidence/qa-gates/coverage-final.cobertura.xml` (accepted final pass). Figures below were independently re-parsed by this reviewer from those XMLs, not transcribed from executor prose. Per established adjudication practice, a committed feature-evidence Cobertura file is the canonical C# coverage artifact for review; `artifacts/csharp/coverage.xml` was deliberately not created.

| Language coverage row | Verdict |
| --- | --- |
| C# repo-wide line coverage 84.84% (root `line-rate` 0.848402) is below the 85% uniform floor | FAIL — dispositioned non-blocking, see disposition below |
| C# repo-wide branch coverage 78.75% (root `branch-rate` 0.787469) meets the 75% floor | PASS |
| C# changed-file line coverage: `QfcStreamingDequeueConfidenceGate.cs` 97.39% (112/115) against the 90% new/changed-code target | PASS |
| C# changed-file line coverage: `QfcHomeController.Iteration.cs` 100.00% (60/60) | PASS |
| C# changed-file line coverage: `QfcFormController.Actions.cs` 47.89% (102/213) is below the 90% target | FAIL — dispositioned non-blocking carve-out, see disposition below |
| C# changed-line no-regression coverage check (all three files improved against the same-session baseline: 96.77 to 97.39, 35.78 to 47.89, 80.36 to 100.00; repo-wide 84.7782 to 84.8402) | PASS |
| C# changed-file branch coverage: gate file 90.91%, iteration file 85.71%, against the 75% floor | PASS |
| C# changed-file branch coverage: `QfcFormController.Actions.cs` 45.24% (38/84) below the 75% floor | FAIL — same non-blocking disposition as its line row |

**Disposition of the repo-wide FAIL row (non-blocking).** The 84.84% figure is the unfiltered all-instrumented-package rate including vendored code; the filtered first-party denominator differs materially in this repository and historically clears the floor. The figure is pre-existing debt: it improved on this branch (+0.062 points against the same-session Phase 0 baseline), there is zero changed-line regression, and `spec.md` AC28 explicitly designates the repository-wide figure record-and-report for this change. No remediation task on this branch could responsibly move a repo-wide vendor-inflated figure.

**Disposition of the `QfcFormController.Actions.cs` FAIL rows (non-blocking carve-out — independently verified).** I re-derived the uncovered-line map from the final Cobertura: uncovered ranges are 29–160 (pre-existing `LoadItems`/`LoadItemsAsync` overloads bound to `TableLayoutPanel`/COM types), 241–258 (`ProcessUndoItemAsync`, the verbatim-extracted COM-and-dispatcher take branch), and 267–306 (`UndoDialog`, three modal `MessageBox.Show` calls). All are pre-existing untestable-without-seam code; every line this branch added or changed for the fix (the three seams and the rewritten `UndoConsumer`) is covered. The executor's carve-out line names only the `MessageBox.Show` calls; that understates the case — even a full `MessageBox` seam would lift the file only to roughly 67%, because the COM-bound loader overloads at 29–160 dominate the uncovered set. The carve-out conclusion is therefore correct and stronger than stated. The file improved 35.78 to 47.89 (line) and 35.37 to 45.24 (branch). Seam work for `UndoDialog` and the loaders is legitimate follow-up refactoring, out of minimal-bugfix scope per the Bugfix Workflow ("open a new issue instead of widening scope"); see the remediation-routing note in the code review (CR-1).

Coverage exclusions check: no `[ExcludeFromCodeCoverage]` attribute is added or moved by this branch (the `QfcDatamodel` and `FolderScoringService` exclusions pre-date the merge base and are within the ratified COM/VSTO exemption). No coverage-config exclude entry was added.

## 5. Evidence Location Compliance

- All executor evidence lives under `docs/features/active/quickfiler-bug-family-446/evidence/{baseline,regression-testing,qa-gates,issue-updates,other}/` — the canonical `<FEATURE>/evidence/<kind>/` layout. PASS.
- Branch diff scan for `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`: zero files. PASS.
- `validate_evidence_locations.py` does not exist in this repository (known port gap); the scan above was performed directly with `git diff --name-only`.
- No `.ps1`/`.py`/`.sh`/`.psm1` files anywhere in the branch diff (count: 0), so no helper-script residue and no spurious language-gate trigger.
- TRX hygiene: case-insensitive grep of the feature folder for the account name and the machine name returned zero matches; sampled TRX files re-parse as well-formed XML.

## 6. Adjudicated Items (caller-referred)

1. **AC28 unchecked — genuine spec self-contradiction; does NOT block merge.** Full reasoning in `feature-audit.2026-08-26T11-29.md` (AC28 row), summarized: the literal whole-type >= 90% reading is arithmetically unreachable from this change set. Even at 100% coverage of the owned files, `QfcFormController` peaks at (290+213)/708 = 71.0% and `QfcHomeController` at (259+60)/449 = 71.0%, because the remaining uncovered lines live in five sibling-owned partial declarations that AC18 forbids modifying and whose testability uplift is assigned to sibling epic children (442, 484, 444, 489). The spec's own Test Strategy states the blocking conditions are "change-scoped", so the checkbox wording is the internal outlier. The executor disposition (leave unchecked, record `REMEDIATION-REQUIRED: AC28 whole-type reading conflicts with AC18`, defer to maintainer) follows the ratified plan ([P5-T7]/[P5-T17]) and the acceptance-criteria-tracking skill exactly. A maintainer spec amendment is required at or before epic close; the blocking changed-file gate passed.
2. **Pre-existing test modified** (`IterateQueueAsync_QueueEmpty`) — legitimate. Base and head compared directly: the three `Verify` calls (dequeue Once, `CompleteAddingAsync` Once, `EnqueueAsync` Never) are preserved one-for-one through the `VerifyCompleteAdding`/`VerifyEnqueue` helpers with identical matchers and `Times` values. The arrangement had to change because production now calls `DequeueNextItemGroupWithOutcomeAsync`; the explicit `stop: QfcDequeueStop.SourceExhausted` converts the test from encoding the defect (any empty batch closes the queue) into pinning the corrected contract. The regression net is strengthened, not weakened: the new `IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding` asserts the previously-defective path now does NOT close the queue.
3. **`QfcFormController.Actions.cs` 47.89% carve-out** — legitimate, with the rationale correction recorded in Section 4. A `MessageBox` seam was technically available but insufficient alone and out of minimal-fix scope.
4. **Dead `using System.Diagnostics;`** — confirmed (line 4 of `QfcFormController.Actions.cs`; sole consumer `Stopwatch` removed by the Phase 3 rewrite; no other `System.Diagnostics` member used in the file). Severity Minor, non-blocking; the analyzer gate passes because IDE0005 is not error-severity in this repository. The executor's restraint (no unauthorized plan deviation) was correct; remove it on the next authorized touch of the file.

## 7. Verdict Summary

| Category | Verdict |
| --- | --- |
| General Code Change Policy | PASS |
| C# Code Change Policy | PASS |
| Unit Test Policy | PASS |
| C# repo-wide line coverage row | FAIL (non-blocking, dispositioned) |
| C# changed-file and no-regression coverage rows | PASS, except the `QfcFormController.Actions.cs` FAIL rows (non-blocking carve-out) |
| Evidence locations | PASS |
| Blocking findings | **0** |
