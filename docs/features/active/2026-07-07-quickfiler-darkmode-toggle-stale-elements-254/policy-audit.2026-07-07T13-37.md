# Policy Compliance Audit — Issue #254 (QuickFiler dark/light toggle stale mail labels)

- Timestamp: 2026-07-07T13-37
- Reviewer: feature-reviewer
- Work Mode: minor-audit (from `issue.md`)
- Base branch (resolved): `main` @ `026de853fb756ca9fac47c3885ff9b4d14c961a2` (merge-base, recomputed via `git merge-base HEAD origin/main` — matches supplied value)
- Head: `TaskMaster-wt-2026-07-07-12-28` @ `57bcebec9b0fc2d0bcc7f24281d080d7d2b06b68`
- Scope: full branch diff vs merge-base (not a plan/task subset)

## Executive Summary

The branch delivers a minimal, targeted bug fix for issue #254. The single production change is a
narrow defensive guard in `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs` that wraps the
`MailRead()` read-state probe in a `try/catch (COMException)` so a stale/moved Outlook `MailItem`
cannot abort the per-item renderer before the sender/subject labels are re-themed. It is accompanied
by a new MSTest regression class (3 test methods giving full branch coverage of the changed block),
a one-line `.csproj` compile registration, and feature documentation/evidence.

Overall verdict: **PASS**. No blocking findings. The full C# toolchain (CSharpier, analyzers,
nullable, MSTest) is recorded as clean in the committed evidence, changed-line coverage is 100% with
no regression, and the change respects the narrow-catch and 500-line policy constraints. One
non-blocking code-review observation is recorded (potential residual `NullReferenceException` path;
see the code review).

## Changed Files In Scope (full branch diff)

C# (the only language with changed code files):
- `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs` — modified (+19/-1), production.
- `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.MailLabelThemingTests.cs` — added (+156/-0), test.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — modified (+1/-0), compile registration.

Non-code (documentation / feature evidence / agent memory), not subject to toolchain/coverage gates:
- `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/**` (issue.md, plan.md,
  research/, evidence/) and two `.claude/agent-memory/**` markdown files.

## Rejected Scope Narrowing

None. The caller supplied the full branch scope and did not attempt to narrow the audit to a plan,
task, phase, or file subset. The caller-supplied merge-base was independently recomputed and matched.

Observation (not a caller narrowing): the auto-generated `artifacts/pr_context.summary.txt` "Changed
files overview" originally reported `Core logic changes: 0 files` and classified the three C# changes
as docs/tooling. This is the known pr-context C#-misclassification. It was corrected in place during
this review so downstream language detection identifies C# as a changed language; the audit scope was
derived from the git diff, not the summary overview.

## 1. Section-by-Section Policy Compliance

### 1.1 General Code Change Policy — PASS

- Simplicity / minimal change: the fix touches one production method and adds only a defensive probe
  evaluation; no opportunistic refactor. PASS.
- Separation of concerns: the guard sits at the UI-boundary renderer where the COM probe is invoked;
  it does not leak into domain logic. PASS.
- Error handling — fail fast, no broad catch: the catch is narrowed to
  `System.Runtime.InteropServices.COMException`; unrelated exceptions still propagate. This complies
  with the "avoid broad `catch (Exception)`" rule. PASS.
- Comment "why, not what": a multi-line `// why (issue #254)` comment documents the stale-`MailItem`
  rationale and the deliberate narrowness of the catch. PASS.
- File size limit (500 lines): `Theme.Rendering.cs` post-change is ~120 lines; the new test file is
  156 lines. Both are under 500. PASS.

### 1.2 C# Code Change Policy + Toolchain — PASS (from committed evidence)

The four-stage C# toolchain results are recorded in the committed feature evidence. This agent
verifies from existing artifacts rather than re-running.

| Stage | Command | Evidence | Result |
|---|---|---|---|
| Format (CSharpier) | `dotnet tool run csharpier check .` | `evidence/qa-gates/qc-csharpier.2026-07-07T13-18.md` | EXIT 0 — PASS |
| Analyze (.NET analyzers) | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | `evidence/qa-gates/qc-analyzers.2026-07-07T13-18.md` | EXIT 0 — PASS |
| Type-check (nullable) | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | `evidence/qa-gates/qc-nullable.2026-07-07T13-18.md` | EXIT 0 — PASS |
| Test (MSTest) | `vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /EnableCodeCoverage` | `evidence/qa-gates/qc-tests-coverage.2026-07-07T13-28.md` | EXIT 0 — 4661/4661 PASS |

### 1.3 General + C# Unit Test Policy — PASS

- Framework: MSTest (`[TestClass]`/`[TestMethod]`), FluentAssertions for assertions — compliant with
  CUT1/CUT2. PASS.
- Isolation / determinism: tests use handle-less WinForms doubles via the big constructor and an
  injected `Func<bool>` read-state probe. No live Outlook/COM, no dispatcher, no network, no temp
  files. Deterministic. PASS.
- Arrange–Act–Assert with descriptive names and a class-level docstring stating intent. PASS.
- Scenario completeness for the changed block: probe returns true (read colors), probe returns false
  (unread colors), probe throws COMException (defaults to unread, does not throw) — positive, negative,
  and error paths covered. PASS.
- No temp files, no external dependencies, no weakened assertions. PASS.

### 1.4 Architecture Boundaries (No-COM rules) — PASS

The production change reduces reliance on a COM fault path (it defends against a COM exception at an
existing UI boundary); it introduces no new VSTO/Interop reference, no `[ComVisible(true)]`, and no
new Outlook event dependency. PASS.

## 2. Coverage Verification (mandatory per language with changed files)

Coverage is verified from committed canonical evidence (`evidence/qa-gates/qc-tests-coverage.<TS>.md`
and `evidence/qa-gates/coverage-comparison.<TS>.md`), both derived from `dotnet-coverage collect`
Cobertura over `UtilitiesCS.Test` + `QuickFiler.Test`. Coverage generation was not re-run.

| Language | Changed code files | Coverage evidence | Line cov (relevant) | New/changed-line coverage | Verdict |
|---|---|---|---|---|---|
| C# | 2 `.cs` (+1 `.csproj`) | `qc-tests-coverage` / `coverage-comparison` Cobertura | UtilitiesCS module 87.93% | 100% (14/14) | PASS |
| TypeScript | 0 | — | — | — | N/A (zero changed files) |
| Python | 0 | — | — | — | N/A (zero changed files) |
| PowerShell | 0 | — | — | — | N/A (zero changed files) |

- C# coverage verdict: **PASS**. Changed-line coverage on `Theme.Rendering.cs` is 100% (14/14
  executable lines in the changed block, all three branches exercised: try→read, try→unread,
  catch→default-unread). The module containing the change (`UtilitiesCS`) is at 87.93% line coverage,
  above the CLAUDE.md 80% testable-denominator floor and the 85% uniform-tier floor. No regression on
  changed lines (they went from uncovered to fully covered; delta table shows flat-to-positive on
  every scope). New/changed-code coverage 100% exceeds the 90% new-code floor.
- Pre-existing repo-wide observation (not attributable to this feature, not blocking): the
  two-assembly aggregate line-rate is 64.28% and branch-rate 33.12%, unchanged by this change (delta
  +0.00 pp line). This aggregate is dominated by COM/VSTO/WinForms and Outlook-Interop event classes
  that CLAUDE.md formally exempts from the 80% floor via the testable-denominator rule. The tension
  with the `.claude/rules/general-unit-test.md` uniform 85%/75% floors is a pre-existing,
  exemption-covered repository condition; under the governing CLAUDE.md policy (authority #1) the
  C# coverage gate for this feature is satisfied.
- Coverage-artifact-path note: the raw Cobertura XML is not committed at `artifacts/csharp/coverage.xml`
  (that path is itself a non-canonical evidence location under the Evidence Location Invariant). The
  numeric coverage is captured in committed canonical evidence, which satisfies the mandatory coverage
  verification for C#.

## 3. Regression Test Evidence — PASS

- Fail-before: `evidence/regression-testing/fail-before.2026-07-07T13-16.md` — the
  `WhenReadProbeThrows` test fails before the fix (EXIT 1). Satisfies AC3 fail-before.
- Pass-after: `evidence/regression-testing/pass-after.2026-07-07T13-18.md` — all `Theme_MailLabelTheming`
  tests pass after the fix (EXIT 0).
- #251 no-regression: `qc-tests-coverage` confirms `QfcCollectionControllerDarkModeTests` (including
  the cleanup-unsubscribe regression tests) all pass. Satisfies AC4 no-regression.

## 4. Evidence Location Compliance — PASS

- Manual diff scan for files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`,
  or `artifacts/coverage/`: none found. All feature evidence is under the canonical
  `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/<kind>/` tree
  (baseline/, qa-gates/, regression-testing/).
- The repository does not contain a `validate_evidence_locations.py` script; the manual scan is the
  fallback evidence for this section.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none (no caller instruction specified a non-canonical evidence
  path).

## 5. Workflow-Modifying Policy Rule (`modified-workflow-needs-green-run`) — Not triggered

The branch diff modifies no path under `.github/workflows/**`, `scripts/benchmarks/**`, or
`.github/actions/**`. The rule does not fire; no green-run evidence requirement applies.

## 6. Benchmark Baseline Provenance — Not applicable

No benchmark baseline files were added or modified.

## 7. Overall Policy Verdict

| Policy area | Verdict |
|---|---|
| General Code Change | PASS |
| C# Code Change + Toolchain (format/analyze/nullable/test) | PASS |
| General + C# Unit Test | PASS |
| Architecture boundaries (No-COM) | PASS |
| C# coverage (changed-line 100%, no regression) | PASS |
| Regression evidence (fail-before / pass-after / #251 intact) | PASS |
| Evidence location compliance | PASS |
| Workflow-modifying rule | Not triggered |

Remediation required: **No**.

## Appendix B — Command Reference (verification, check-only)

- `git merge-base HEAD origin/main` — confirm merge-base `026de853…`.
- `git diff --name-status <merge-base> HEAD` — enumerate branch diff.
- `git diff --numstat <merge-base> HEAD -- '*.cs' '*.csproj'` — changed C# line counts.
- Toolchain/coverage results read from committed evidence under
  `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/` (not re-run).
