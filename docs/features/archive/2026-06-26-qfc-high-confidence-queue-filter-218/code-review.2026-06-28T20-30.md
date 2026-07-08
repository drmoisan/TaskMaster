# Code Review (Cycle 2 Exit Reaudit): qfc-high-confidence-queue-filter (Issue #218)

---

**Review Date:** 2026-06-28
**Reviewer:** feature-reviewer
**Feature Folder:** `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218`
**Feature Folder Selection Rule:** Explicit user-supplied feature folder matching canonical issue #218.
**Base Branch:** `main` (merge-base `1b8536b6e5fb0778aba528caa39853590185bcb7`)
**Head Branch:** `bug/qfc-high-confidence-queue-filter-218` at `27ca7717e7bf020ab5d2b5788fbdad6c1a1d0943`
**Review Type:** Remediation cycle 2 exit reaudit (full branch diff vs `main`)

---

## Executive Summary

This cycle-2 exit reaudit reviews the full branch diff against `main`, which now includes the maintainer production split `2637e4c1` (decomposing the oversized `QfcDatamodel` and `QfcHomeController` into cohesive partials/files) and the cycle-2 test-split completion `27ca7717` (trimming the 1370-line `QfcHomeControllerTests.cs` and wiring four split test files).

The issue #218 behavior is unchanged from cycle 1 and remains correct: remaining queued mail items are scored in the data-model path (`QfcRemainingQueueAdmission`/`TryQueueRemainingMailItemAsync`) before queue admission when high-confidence mode is enabled, and `QfcHomeController.RunAsync` no longer applies high-confidence filtering to only the initial GUI batch. No blocker or major correctness finding was identified in the issue #218 code path.

The cycle-2 work is mechanical and behavior-preserving: the production decomposition relocates existing code verbatim, and the test split preserves all 27 moved tests name- and body-equivalent to their canonical originals with the compiled active count preserved at 32 and zero duplicates. The full C# toolchain passes (CSharpier check exit 0 independently re-verified by this reviewer; analyzer and nullable builds exit 0; MSTest 4270 pass / 0 fail).

One Info-level documentation correction is raised: the changed-line-coverage evidence groups `EmailSorter.cs` under the COM/VSTO coverage exemption, but `EmailSorter` is pure, testable logic (it is not Outlook-Interop-bound). This does not change any verdict but should be corrected in the coverage-uplift follow-up tracking.

**What changed:**
The maintainer split `2637e4c1` extracted `EmailSorter` (relocated verbatim from `QfcDatamodel.cs` line 686 on `main`), `QfcDatamodel.FrameBuilding`/`QfcDatamodel.QueueProcessing`, and `QfcHomeController.Iteration`/`QfcHomeController.Metrics`, wiring them into `QuickFiler.csproj`. The cycle-2 commit `27ca7717` wired four split test files into `QuickFiler.Test.csproj`, trimmed `QfcHomeControllerTests.cs` to the 3 residual tests plus scaffolding, and added one null-mailItem admission test.

**Top 3 risks:**
1. Repository-wide C# coverage remains below 80% (raw 62.12%); dispositioned as a non-blocking authority-scoped exception with an open dependency on maintainer ratification under `feature/csharp-coverage-uplift`.
2. Aggregate changed-line coverage is 41.91%, concentrated entirely in pre-existing relocated code (`EmailSorter`, `QfcHomeController.Metrics`/`Iteration`); the issue #218 behavior subset is 100% covered.
3. Eight pre-existing banned-API sites (`DateTime.Now`, `Task.Delay`) were relocated into the new partials; the branch introduces none, and RS0030 is held at `suggestion` severity. Deferred to a follow-up time-seam migration.

**PR readiness recommendation:** **Go for this bug remediation cycle.** The three cycle-1 blocking findings are cleared to an authorized non-blocking disposition; the residual items are pre-existing, out-of-scope coverage and time-seam follow-ups.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` | `TryQueueAsync` (lines 1-58) | The high-confidence queue-admission decision is centralized behind a focused, fully covered seam (33/33 lines), including the null-mailItem guard newly covered by the P4-T2 test. | Keep the seam focused; no change required. | Implements issue #218 behavior with deterministic, COM-free tests; public `IQfcDatamodel` surface unchanged. | `changed-line-coverage-final-cycle2-218.md`; `focused-pass-after-cycle2-218.md`. |
| Info | `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` | Whole file (287 lines, trimmed) + 4 split files | The 1370-line test file was split into five cohesive files (all <= 500 lines) with all 27 moved tests verified name- and body-equivalent to the canonical originals, zero duplicates, and the compiled active count preserved at 32. | No change required. | Mechanical, behavior-preserving split; preserves the existing tests as part of the spec. | `test-split-equivalence-cycle2-218.md`; `test-split-verification-cycle2-218.md`; `test-split-build-cycle2-218.md`. |
| Info | `QuickFiler/Controllers/EmailSorter.cs` | Whole file (85 lines); coverage evidence | The changed-line-coverage evidence groups `EmailSorter` (49 uncovered lines) under the COM/VSTO testable-denominator exemption, but `EmailSorter` is pure testable logic (sort-key arithmetic, triage dictionary; takes a `DateTime` parameter, no Outlook-Interop dependency). It is relocated pre-existing code (was `QfcDatamodel.cs` line 686 on `main`), not new behavior. | Correct the exemption rationale: classify `EmailSorter` as "relocated pre-existing untested code, out of scope for issue #218" and track its coverage under `feature/csharp-coverage-uplift`. Do not add out-of-scope tests in this cycle (prohibited by the inputs). | The exemption framing is inaccurate for `EmailSorter`; the non-blocking disposition is unaffected because the branch introduces no new uncovered behavior, but the rationale should be precise. | Reviewer `git grep "class EmailSorter" main` (matched `QfcDatamodel.cs:686`); `EmailSorter.cs` source inspection; `changed-line-coverage-final-cycle2-218.md`. |
| Info | `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcDatamodel.FrameBuilding.cs` | Metrics:20,100,102,114,214; QueueProcessing:142; FrameBuilding:43; QfcHomeController.cs:75 | Eight active banned-API sites (`DateTime.Now`, `Task.Delay`) appear as additions in the diff but were verified verbatim on `main` and relocated by split `2637e4c1`. | Defer to a `System.TimeProvider` time-seam migration cycle when RS0030 is promoted from `suggestion` to `warning`; do not refactor production seams within this mechanical-split remediation. | Branch introduces zero new banned-API usage; `.claude/rules/csharp.md` classifies legacy call-site migration as follow-up work. | Reviewer `git show main:QuickFiler/Controllers/QfcHomeController.cs` / `QfcDatamodel.cs` banned-API grep; `banned-api-sweep-cycle2-218.md`. |

No Blocker or Major code findings were identified.

## Implementation Audit

### C# implementation audit

#### What changed well

- The queue-admission decision runs in the remaining-queue path behind a focused, fully covered `QfcRemainingQueueAdmission` seam.
- The public `IQfcDatamodel` interface was not changed.
- Scoring reuses the existing `FolderScoringService` path instead of duplicating classifier logic.
- The oversized `QfcDatamodel.cs` (790) and `QfcHomeController.cs` (739) files were decomposed into cohesive single-responsibility files, all <= 500 lines, using `partial class` to preserve type identity and the public surface.
- The test split preserved each test verbatim and re-created only the scaffolding each split class requires.

#### Type safety and API notes

- Reviewer nullable build passed with `TreatWarningsAsErrors=true` (exit 0).
- New seams are internal and do not expand the public API.
- `TryQueueRemainingMailItemAsync` accepts `MailItem` and returns a boolean signalling admission.

#### Error handling and logging

- Cancellation still flows through `ThrowIfCancellationRequested`.
- Existing exception logging in the remaining-queue loop is unchanged.
- No new broad catch was added; the only `catch` in the relocated `EmailSorter.GetSortKey` is a narrow `KeyNotFoundException` that logs and rethrows (fail-fast, pre-existing).

## Test Quality Audit

The issue #218 tests are focused and deterministic, using Moq and delegate seams to assert scoring, admission, rejection, hook behavior, and the null-item guard without live Outlook COM. The cycle-2 test split was verified equivalent before trimming: each of the 27 moved tests was compared name-and-body against the compiled canonical original, with all 27 EQUIVALENT and zero divergence.

### Reviewed test and QA artifacts

- `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` - queue-admission behavior for enabled, equal-threshold, below-threshold, disabled, and null-item modes.
- `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs` - initial GUI load behavior under high-confidence mode.
- `test-split-equivalence-cycle2-218.md` - 27/27 moved tests EQUIVALENT to canonical originals.
- `test-split-verification-cycle2-218.md` - all six files <= 500 lines; 32 compiled active tests; zero duplicates.
- `final-mstest-coverage-cycle2-218.md` - full MSTest run, 4270 passed, 0 failed.
- `focused-pass-after-cycle2-218.md` - 7/7 focused issue #218 tests pass after the split.

### Quality assessment prompts

- **Determinism:** Tests use mocks and local delegates instead of live Outlook COM.
- **Isolation:** Each model test targets one queue-admission behavior; each split class is independent.
- **Speed:** Full suite of 4270 tests executed in 42.86 s.
- **Diagnostics:** FluentAssertions and descriptive test names make expected behavior clear.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Diff inspection found no credentials or secret material in changed C# files. |
| No unsafe subprocess or command construction | PASS | Issue #218 code path does not add subprocess execution. |
| Input validation at boundaries | PASS | Queue helper checks cancellation and handles null mail item defensively (now test-covered). |
| Error handling remains explicit | PASS | Cancellation and existing exception logging are preserved; relocated `KeyNotFoundException` handler rethrows. |
| Configuration / path handling is safe | N/A | No new file-path or configuration-loading logic was added. |

## Research Log

External research was not required. The review used repository policy files, PR context artifacts, the branch diff, issue #218 feature artifacts, cycle-2 evidence, and local verification commands (`git diff`/`git show`/`git grep`, on-disk line counts, and a CSharpier check).

## Verdict

The issue #218 implementation and the cycle-2 mechanical split are acceptable from a code-review standpoint. Behavior is covered by focused tests, the production decomposition preserves the public surface, the test split is verified equivalent, and the C# toolchain passes. No Blocker or Major finding was identified. The residual coverage and banned-API items are pre-existing, out-of-scope follow-ups dispositioned as non-blocking exceptions in `policy-audit.2026-06-28T20-30.md`.

**Overall code-review verdict: GO (no blocking code findings).**
