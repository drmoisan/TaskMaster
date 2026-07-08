# Feature Audit: QuickFiler banned-API time/delay seams (Issue #222)

**Audit Date:** 2026-06-28
**Feature Folder:** `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222`
**Base Branch:** `main`
**Head Branch:** `TaskMaster-wt-2026-06-28-18-49`
**Work Mode:** `full-bug`
**Audit Type:** Initial acceptance review

---

## Scope and Baseline

- **Base branch:** `main` (commit `86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a`)
- **Head branch/commit:** `TaskMaster-wt-2026-06-28-18-49` (commit `e48932654a6a9b90e94f23f3a87f6f617727ffcc`)
- **Merge base:** `86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt` and `git diff 86b555bf..e4893265`
  - Feature evidence: `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222/evidence/**`
  - Additional evidence: `evidence/qa-gates/ac-traceability.md`
- **Feature folder used:** `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222`
- **Requirements source:** `spec.md` (`## Acceptance Criteria`)
- **Work mode resolution note:** `issue.md` line "- Work Mode: full-bug". Per acceptance-criteria-tracking, `full-bug` => `spec.md` only is the authoritative AC source.
- **Scope note:** The PR-context summary misclassifies all changes as docs ("Core logic changes: 0 files"). This audit uses the verified `git diff` scope, which includes 5 C# production files, 2 C# test files, and 6 build/config files.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `spec.md` — only source (work mode `full-bug`)

### Acceptance criteria

1. All 8 active banned-API sites in the four target files are removed and replaced with injected seams.
2. No new banned-API usages introduced; RS0030 not suppressed globally and policy files not weakened.
3. Production behavior preserved: delays remain 5/200/20 ms; timestamp formats and semantics unchanged.
4. Seams injected through `QfcHomeController` and `QfcDatamodel` construction paths without breaking public `IQfcDatamodel` / home-controller surfaces.
5. Every touched file remains <= 500 lines.
6. Focused MSTest+Moq+FluentAssertions tests prove time-dependent output uses the injected clock and delayed paths await the injected delay (Moq-verifiable), with no live Outlook COM and no temp files.
7. New/changed code targets >= 90% coverage; coverage on changed lines not reduced; repo-wide floor (>= 80%) maintained.
8. C# toolchain passes in order: csharpier -> analyzer build -> nullable build (TreatWarningsAsErrors) -> vstest with coverage.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | All 8 banned-API sites removed/replaced with seams | PASS | Diff shows all 8 sites converted to `TimeProvider.Delay`/`TimeProvider.GetLocalNow().LocalDateTime`; sweep reports 0 active matches | `git diff 86b555bf..e4893265 -- QuickFiler/Controllers/*.cs` | `p3-banned-api-sweep.md` |
| 2 | No new banned-API; RS0030 not suppressed; policy files unchanged | PASS | Diff adds no RS0030/NoWarn/#pragma/SuppressMessage; BannedSymbols.txt/.editorconfig/csharp.md not in diff | `git diff ... \| grep -iE 'RS0030\|NoWarn\|pragma\|SuppressMessage'` -> none | `p3-policy-unchanged.md` |
| 3 | Behavior preserved (5/200/20 ms; mm:ss.fff, MM/dd/yyyy, hh:mm) | PASS | Durations and format strings preserved verbatim in diff; default `TimeProvider.System`; tests assert exact formats/durations via fake clock | diff inspection; `final-tests.md` | Durations: 5/200/20 ms unchanged |
| 4 | Seams via construction paths; interfaces unchanged | PASS | `internal TimeProvider` property + optional `LaunchAsync` param; `IQfcDatamodel`/`IQfcHomeController` untouched | diff inspection | LaunchAsync optional param is source-compatible (Info finding) |
| 5 | Every touched file <= 500 lines | PASS | Independently verified: max 456 (QfcHomeController.cs); test files 421/276 | `awk 'END{print NR}' <each file>` | `final-line-counts.md` |
| 6 | MSTest+Moq+FluentAssertions tests; no live COM/temp files | PASS | 5 new tests use MSTest/Moq/FluentAssertions/FakeTimeProvider; uninitialized object + loose mocks; no temp files | `vstest.console.exe ... /EnableCodeCoverage` (evidence) | `final-tests.md` 186/186 pass |
| 7 | >= 90% new code; no regression on changed lines; repo-wide >= 80% | PARTIAL | New testable code 100% (6/6); no regression (Metrics.cs +14.5pts); repo-wide floor NOT demonstrable (canonical `artifacts/csharp/coverage.xml` absent; single-assembly run only) | `ls artifacts/csharp/coverage.xml` -> absent | `coverage-comparison.md` states repo-wide "NOT MEASURABLE"; remediation trigger |
| 8 | Toolchain passes in order | PASS | csharpier/analyzer/nullable/vstest all EXIT_CODE 0 (committed evidence) | see Appendix B of policy audit | `final-format/analyzer/nullable/tests.md` |

---

## Summary

**Overall Feature Readiness:** NEEDS REVISION

**Criteria summary:**
- **PASS:** 7 criteria (AC1-AC6, AC8)
- **PARTIAL:** 1 criterion (AC7)
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**

1. AC7 repo-wide coverage: the canonical `artifacts/csharp/coverage.xml` is absent and the >= 80% repo-wide floor is not demonstrable from the committed single-assembly run. New/changed-code coverage (100% testable) and no-regression sub-criteria are met; only the repo-wide floor sub-criterion is unverified.

**Recommended follow-up verification steps:**

1. Generate the canonical cobertura/JaCoCo coverage artifact at `artifacts/csharp/coverage.xml`, or confirm repo-wide C# coverage via the PR CI coverage run, and record the figure against the 80% floor (applying the CLAUDE.md testable-denominator exemption framework if below floor due to pre-existing COM-bound code).
2. Confirm maintainer approval is recorded for the new `Microsoft.Bcl.TimeProvider` / `Microsoft.Extensions.TimeProvider.Testing` dependencies.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file(s) if represented as markdown checkboxes and not already checked.
- Criteria evaluated as **PARTIAL**, **FAIL**, or **UNVERIFIED** must remain unchecked.

AC1-AC6 and AC8 (PASS) were already checked `[x]` in `spec.md` by the executor; this review confirms those check-offs. AC7 is evaluated PARTIAL; it had been marked `[x]` by the executor, and this review **unchecked it to `[ ]`** in `spec.md` to comply with the acceptance-criteria-tracking rule that PARTIAL/FAIL/UNVERIFIED items must remain unchecked. The repo-wide coverage sub-criterion is unverified pending the canonical artifact / CI confirmation.

### AC Status Summary

- Source: `spec.md` (`## Acceptance Criteria`)
- Total AC items: 8
- Checked off (delivered): 7 (AC1-AC6, AC8 — all reviewer-confirmed PASS)
- Remaining (unchecked): 1 (AC7)
- Items remaining: AC7 — "New/changed code targets >= 90% coverage; coverage on changed lines not reduced; repo-wide floor (>= 80%) maintained." (PARTIAL: new-code 100% testable and no-regression met; repo-wide floor unverified.)

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 8 | 7 (AC1-6, AC8) | 1 (AC7, PARTIAL) | Checkbox-backed; reviewer unchecked AC7 |
