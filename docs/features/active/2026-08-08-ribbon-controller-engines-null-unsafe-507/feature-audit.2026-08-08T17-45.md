# Feature Audit — ribbon-controller-engines-null-unsafe (#507)

Timestamp: 2026-08-08T17-45
Work Mode: `minor-audit`
AC Source: `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/issue.md`,
`## Acceptance Criteria` section only (AC1-AC6), per `minor-audit` work-mode routing.
`spec.md`/`user-story.md` are intentionally absent for `minor-audit` and are not treated as a
finding.

## Scope and Baseline

- Base: `main`, merge base `003c5715055d7d1933db68a742531332756e30b2`.
- Branch: `bug/ribbon-controller-engines-null-unsafe-507`, head `e589fad7`.
- Diff evaluated: `git diff 003c5715055d7d1933db68a742531332756e30b2...HEAD`.
- Production surface: one line in `TaskMaster/Ribbon/RibbonController.Intelligence.cs`. Test
  surface: two new `[TestMethod]`s appended to `TaskMaster.Test/Ribbon/RibbonControllerTests.cs`.
  Remainder of the diff is feature-folder evidence/docs and agent-memory housekeeping.

## Acceptance Criteria Inventory

| ID | Criterion (verbatim from `issue.md`) |
|---|---|
| AC1 | `RibbonController.Engines` returns `null` instead of throwing `NullReferenceException` when `Globals` has not been assigned (i.e. before `SetGlobals` has run). |
| AC2 | The change is confined to `TaskMaster/Ribbon/RibbonController.Intelligence.cs`; no other production file is modified. |
| AC3 | A deterministic MSTest regression test in `TaskMaster.Test` covers the unassigned-`Globals` case, fails against the pre-fix source, and passes after the fix. |
| AC4 | When `Globals` is assigned, `Engines` continues to return the value of `Globals.Engines` (no behavior regression for the assigned path). |
| AC5 | The full C# toolchain passes in a single clean pass, in order: `csharpier .`, msbuild with `EnableNETAnalyzers`/`EnforceCodeStyleInBuild`, the nullable gate as enforced by `.github/workflows/ci.yml`, and `vstest.console.exe` with `/EnableCodeCoverage`. (AC text was corrected in-branch to name the CI-enforced nullable command; rationale in `evidence/qa-gates/phase2-orchestrator-ci-gate-reconciliation.md`.) |
| AC6 | No pre-existing test regresses; the MSTest pass/fail counts are no worse than the recorded Phase 0 baseline. |

## Acceptance Criteria Evaluation

### AC1 — PASS

`TaskMaster/Ribbon/RibbonController.Intelligence.cs:204` reads
`internal IAppItemEngines Engines => Globals?.Engines;` (was `Globals.Engines;`). Verified directly
in the diff and by the regression test
`Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing`
(`TaskMaster.Test/Ribbon/RibbonControllerTests.cs`), which constructs a bare `RibbonController()`
(leaving `Globals` at its default `null`) and asserts `controller.Engines` does not throw and is
`null`. Confirmed passing post-fix and failing pre-fix (`evidence/regression-testing/phase1-expect-fail-engines-unassigned.md`,
`evidence/regression-testing/phase1-post-fix-engines-tests.md`).

Caveat (does not change the verdict, but is material context): AC1 is scoped strictly to the
property boundary. Independent verification in this audit (see `code-review.2026-08-08T17-45.md`,
Blocking finding 2) found that all 11 real production call sites of `Engines`
(in `TaskMaster/Ribbon/RibbonViewer.cs`) dereference the property result without a null check, so
the reachable `NullReferenceException` the issue describes still occurs for those callers — it is
relocated from `RibbonController.get_Engines()` to the call site, not eliminated. AC1's literal text
("`Engines` returns `null` instead of throwing") is true and verified at the property boundary;
whether that is a complete fix for the issue's described symptom is a design/scope caveat, not an
AC1 failure — the criterion says nothing about caller behavior.

### AC2 — PASS

`git diff --name-only 003c5715055d7d1933db68a742531332756e30b2...HEAD` (this review's own
independent execution) shows exactly one production `.cs`/`.csproj`/`.props`/`.targets` file
touched: `TaskMaster/Ribbon/RibbonController.Intelligence.cs`. `TaskMaster.Test/Ribbon/RibbonControllerTests.cs`
is test code, not production code, and is explicitly the vehicle for AC3, so its modification does
not violate AC2. `TaskMaster/Ribbon/RibbonViewer.cs` is confirmed absent from the diff, independently
and via `evidence/qa-gates/phase2-git-status-scope-check.md` and `evidence/qa-gates/phase2-ribbonviewer-guard.md`.

### AC3 — PASS

`evidence/regression-testing/phase1-expect-fail-engines-unassigned.md` records the new test failing
against the pre-fix source with `System.NullReferenceException: Object reference not set to an
instance of an object` at `RibbonController.get_Engines()` — matching the issue's documented
observed-failure signature exactly. `evidence/regression-testing/phase1-post-fix-engines-tests.md`
(not separately re-quoted here; referenced by `evidence/qa-gates/phase2-final-vstest-coverage.md`,
which shows the full suite, including this test, passing post-fix) confirms the pass after the fix.
The test is deterministic (no I/O, no timing dependency, no external state) and MSTest-based.

### AC4 — PASS

Verified by direct code inspection: `Globals?.Engines` evaluates to `Globals.Engines` whenever
`Globals` is non-null (the null-conditional operator is a no-op guard, not a value transform).
Additionally verified by the second new test,
`Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines`, which sets `Globals.Engines` via reflection to
a distinguishable `Moq`-backed `IAppItemEngines` instance and asserts `controller.Engines` returns
the exact same reference (`BeSameAs`), which is a stronger check than a null/non-null comparison and
correctly rules out a false-positive null-to-null pass.

### AC5 — PASS

Verified via `evidence/qa-gates/phase2-orchestrator-ci-gate-reconciliation.md`, which reproduces
`.github/workflows/ci.yml`'s exact enforced nullable command (`/t:Rebuild`, `/p:TreatWarningsAsErrors=true`,
no `/p:Nullable=enable`) with the change applied: `EXIT_CODE=0`, 0 errors, 0 `CS8603`. Combined with
`evidence/qa-gates/phase2-final-csharpier.md` (0 reformatted), `evidence/qa-gates/phase2-final-msbuild-analyzers.md`
(0 errors), and `evidence/qa-gates/phase2-final-vstest-coverage.md` (0 failed), all four stages pass
in a single clean pass. The AC text's self-correction (naming the CI-enforced command rather than
`CLAUDE.md`'s `/p:Nullable=enable` command) is itself accurate and is corroborated independently in
this audit: the `CLAUDE.md`/`ci.yml` divergence is real, pre-existing (195 + 219 errors already red
on `main` under the forced flag, per the same evidence artifact), and correctly out of scope for a
minor-audit single-line bugfix. See `policy-audit.2026-08-08T17-45.md` § 2 for the informational
disposition of that divergence.

### AC6 — PASS

`evidence/baseline/phase0-baseline-vstest-coverage.md` records the Phase 0 baseline: 6294 total,
6294 passed, 0 failed. `evidence/qa-gates/phase2-final-vstest-coverage.md` records the post-fix run:
6296 total, 6296 passed, 0 failed (+2 exactly matching the two new tests). A separate orchestrator
re-verification (`evidence/qa-gates/phase2-orchestrator-ci-gate-reconciliation.md`) recorded
6295/6295 passed, 0 failed, on an independent run with an added `TestCategory!=LiveOutlook` filter.
All three counts satisfy AC6's literal text ("no pre-existing test regresses"; "no worse than
baseline") because every recorded run shows `total == passed` and `failed == 0`; the one-test
discrepancy between the two post-fix runs (6296 vs 6295) does not indicate a regression in either
direction and is noted as an evidence-hygiene item in `policy-audit.2026-08-08T17-45.md` § 5. The
repo-wide coverage-percentage swing (74.43% -> 61.66% raw) investigated in the same evidence file is
a denominator artifact, not a test regression, and is separately dispositioned in the policy audit's
coverage section.

## Acceptance Criteria Status

- Source: `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/issue.md`
- Total AC items: 6
- Checked off (delivered): 6 (AC1-AC6 were already checked `[x]` in `issue.md` prior to this
  review; this audit independently verified all 6 as PASS and confirms the existing check-off state
  is correct. No new check-offs were required.)
- Remaining (unchecked): 0
- Items remaining: none

## Findings Carried from Code Review / Policy Audit

Two Blocking findings apply to this feature despite all 6 ACs evaluating PASS (the ACs, as
literally worded, do not cover file-size limits or caller-side null-safety beyond the property
boundary):

1. `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` exceeds the repository's 500-line file-size
   cap (513 lines; baseline was 452). See `code-review.2026-08-08T17-45.md`.
2. `Engines` returning `null` does not eliminate the reachable `NullReferenceException` for any of
   the 11 real production call sites in `RibbonViewer.cs`; the crash relocates rather than resolves.
   See `code-review.2026-08-08T17-45.md`.

Full remediation guidance: `remediation-inputs.2026-08-08T17-45.md`.

## Verdict

All 6 acceptance criteria PASS on their literal text and are backed by concrete evidence. The
feature does not merge cleanly against full repository policy due to 2 Blocking findings unrelated
to AC wording (file-size limit; caller-side hazard). Recommend remediation before merge.
