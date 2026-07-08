# Feature Audit: QuickFiler banned-API time/delay seams (Issue #222)

**Audit Date:** 2026-06-28
**Feature Folder:** `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222`
**Base Branch:** `main`
**Head Branch:** `TaskMaster-wt-2026-06-28-18-49`
**Work Mode:** `full-bug`
**Audit Type:** Re-audit (cycle 2) after maintainer authority decision 222-COV-001 resolving prior finding R1

---

## Scope and Baseline

- **Base branch:** `main` (commit `86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a`)
- **Head branch/commit:** `TaskMaster-wt-2026-06-28-18-49` (commit `d4075e02509f7340e747894dc9bff0ae1c1e1197`)
- **Merge base:** `86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt` and `git diff 86b555bf..d4075e02`
  - Feature evidence: `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222/evidence/**`
  - Additional evidence: `evidence/qa-gates/ac-traceability.md`
  - Maintainer authority decision: `coverage-policy-exception.md` (222-COV-001)
- **Feature folder used:** `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222`
- **Requirements source:** `spec.md` (`## Acceptance Criteria`)
- **Work mode resolution note:** `issue.md` line "- Work Mode: full-bug". Per acceptance-criteria-tracking, `full-bug` => `spec.md` only is the authoritative AC source.
- **Cycle-2 delta:** The only commit since cycle 1 (`e4893265..d4075e02`) is `d4075e02 docs(#222): record authority coverage-policy exception (222-COV-001)`. No production or test code changed (`git diff --stat e4893265..d4075e02 -- '*.cs' '*.csproj' '*.config'` is empty); all code-level AC verdicts are re-verified and unchanged from cycle 1. The sole substantive change is the resolution of AC7's repo-wide sub-criterion by maintainer authority.
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
| 1 | All 8 banned-API sites removed/replaced with seams | PASS | Diff shows all 8 sites converted to `TimeProvider.Delay`/`TimeProvider.GetLocalNow().LocalDateTime`; independent grep at head returns only commented-out references; sweep reports 0 active matches | `grep -nE "DateTime\.Now\|Task\.Delay" QuickFiler/Controllers/<4 files>` | `p3-banned-api-sweep.md` |
| 2 | No new banned-API; RS0030 not suppressed; policy files unchanged | PASS | Diff adds no RS0030/NoWarn/#pragma/SuppressMessage; BannedSymbols.txt/.editorconfig/.globalconfig/csharp.md/CLAUDE.md not in diff | `git diff 86b555bf..d4075e02 -- <policy files>` -> empty | `p3-policy-unchanged.md` |
| 3 | Behavior preserved (5/200/20 ms; mm:ss.fff, MM/dd/yyyy, hh:mm) | PASS | Durations and format strings preserved verbatim in diff; default `TimeProvider.System`; tests assert exact formats/durations via fake clock | diff inspection; `final-tests.md` | Durations 5/200/20 ms unchanged |
| 4 | Seams via construction paths; interfaces unchanged | PASS | `internal TimeProvider` property + optional `LaunchAsync` param; `IQfcDatamodel`/`IQfcHomeController` untouched | diff inspection | LaunchAsync optional param is source-compatible (Info finding) |
| 5 | Every touched file <= 500 lines | PASS | Independently verified: max 456 (QfcHomeController.cs); Metrics 234; QfcDatamodel 438; FrameBuilding 154; QueueProcessing 146; test files 421/276 | `awk 'END{print NR}' <each file>` | `final-line-counts.md` |
| 6 | MSTest+Moq+FluentAssertions tests; no live COM/temp files | PASS | 5 new tests use MSTest/Moq/FluentAssertions/FakeTimeProvider; uninitialized object + loose mocks; no temp files | `vstest.console.exe ... /EnableCodeCoverage` (evidence) | `final-tests.md` 186/186 pass |
| 7 | >= 90% new code; no regression on changed lines; repo-wide >= 80% | PASS | New testable code 100% (6/6 changed lines); no regression (Metrics.cs class +14.51; package +0.72). Repo-wide floor: verification deferred to PR CI and pre-existing below-floor figure ratified as a legacy COM/VSTO condition by maintainer authority decision 222-COV-001 under the CLAUDE.md testable-denominator framework | `coverage-comparison.md`; `final-tests.md` per-line hit counts; `coverage-policy-exception.md` | See AC7 disposition note below |
| 8 | Toolchain passes in order | PASS | csharpier/analyzer/nullable/vstest all EXIT_CODE 0 (committed evidence) | see Appendix B of policy audit | `final-format/analyzer/nullable/tests.md` |

**AC7 disposition note (cycle-2 resolution).** AC7 has three sub-criteria: (a) new/changed code >= 90%; (b) no regression on changed lines; (c) repo-wide floor (>= 80%) maintained. Sub-criteria (a) and (b) are independently verified PASS from committed per-line evidence: 100% of testable changed lines covered (6/6), with the 3 uncovered changed lines formally exempt (COM/VSTO lifecycle + unreachable defensive branch, dossiers ratified), and no regression (Metrics.cs class +14.51 points; QuickFiler package +0.72). Sub-criterion (c) — the absolute repo-wide figure — is not independently measured during this no-mutation review because the canonical `artifacts/csharp/coverage.xml` is absent. The repository owner has issued maintainer authority decision 222-COV-001 (Option C): defer repo-wide floor verification to the PR CI coverage run and accept the current repo-wide figure as a pre-existing legacy COM/VSTO/WinForms condition that issue #222 does not introduce or regress, under the CLAUDE.md testable-denominator exemption framework, tracked on `feature/csharp-coverage-uplift`. CLAUDE.md explicitly authorizes this maintainer-ratified exemption, and it matches resolution paths 2-3 enumerated for finding R1 in `remediation-inputs.2026-06-28T19-57.md`. On that basis AC7 is evaluated PASS: the change satisfies the criteria within its control (new-code 100%, no regression), and the repo-wide "maintained" sub-criterion is satisfied as a ratified accepted exception with verification deferred to PR CI. This audit notes transparently that the absolute repo-wide percentage was not independently re-measured locally; the PASS rests on the no-regression evidence and the maintainer authority decision, not on a fresh repo-wide measurement.

---

## Summary

**Overall Feature Readiness:** READY (Go)

**Criteria summary:**
- **PASS:** 8 criteria (AC1-AC8)
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Cycle-1 vs cycle-2:** Cycle 1 evaluated AC7 as PARTIAL (repo-wide coverage floor not demonstrable; canonical artifact absent) and produced `remediation-inputs.2026-06-28T19-57.md` with finding R1. The maintainer subsequently issued authority decision 222-COV-001, which resolves R1 via one of its own enumerated resolution paths (defer to PR CI; ratify the pre-existing below-floor figure under the testable-denominator framework). With no code changed since cycle 1, all other AC verdicts are re-confirmed PASS and AC7 now evaluates PASS.

**Remaining gaps preventing PASS:** None blocking. Two Info-level follow-ups remain for PR review: confirm the PR CI repo-wide coverage figure per 222-COV-001, and confirm the maintainer has recorded explicit approval for the two new first-party Microsoft TimeProvider packages.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file(s) if represented as markdown checkboxes and not already checked.
- Criteria evaluated as **PARTIAL**, **FAIL**, or **UNVERIFIED** must remain unchecked.

AC1-AC6 and AC8 were already checked `[x]` in `spec.md`; this review confirms those check-offs. AC7 was unchecked to `[ ]` by the cycle-1 review (it was PARTIAL then). This cycle evaluates AC7 as PASS on the basis of the maintainer authority decision 222-COV-001 plus the within-scope 100%/no-regression coverage evidence, and therefore **re-checks AC7 to `[x]`** in `spec.md`. All 8 criteria are now checked.

### AC Status Summary

- Source: `spec.md` (`## Acceptance Criteria`)
- Total AC items: 8
- Checked off (delivered): 8 (AC1-AC8)
- Remaining (unchecked): 0
- Items remaining: none

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 8 | 8 (AC1-AC8) | 0 | AC7 re-checked this cycle after 222-COV-001 resolved repo-wide sub-criterion |
