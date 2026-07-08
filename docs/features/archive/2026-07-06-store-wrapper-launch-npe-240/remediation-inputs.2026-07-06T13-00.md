# Remediation Inputs — store-wrapper-launch-npe (Issue #240), Cycle 2 Re-Audit

- Timestamp: 2026-07-06T13-00
- Source artifacts: `policy-audit.2026-07-06T13-00.md`, `code-review.2026-07-06T13-00.md`, `feature-audit.2026-07-06T13-00.md`
- Prior cycle artifacts: `policy-audit.2026-07-06T12-15.md`, `code-review.2026-07-06T12-15.md`, `feature-audit.2026-07-06T12-15.md`, `remediation-inputs.2026-07-06T12-15.md`

## Finding 1 — RESOLVED (was Blocking): Test file exceeded the 500-line policy limit

- **Status: RESOLVED. No further action required.**
- Prior state: `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs` was 781 lines.
- Remediation applied (commit `9e3615b9`): split into three `partial class` files — `StoreWrapperController_Tests.cs` (181 lines), `StoreWrapperController_Tests.ButtonAndPopulate.cs` (396 lines), `StoreWrapperController_Tests.Launch.cs` (234 lines) — all <= 500 lines.
- Independent verification performed by this review cycle (not accepted on faith from executor evidence): `wc -l` on all three files; `grep -c "\[TestMethod\]"` summed to 39 pre- and post-split; `git diff dfbebb13..9e3615b9 -- UtilitiesCS/` produced no output (zero production diff); `dotnet tool run csharpier check` on all four changed `.cs` files (`EXIT_CODE 0`); a scoped `msbuild` analyzer rebuild of `UtilitiesCS.Test.csproj` (`EXIT_CODE 0`, zero diagnostics attributable to the split files); a targeted `vstest.console.exe` re-run of all 39 `StoreWrapperController_Tests` methods (39/39 passed).
- Evidence: `policy-audit.2026-07-06T13-00.md` §1.6, §2, §3, Appendix B.
- Owner/next step: none — closed.

## Finding 2 — Tracked, non-blocking-for-#240: Repo-wide C# coverage artifact absent

- **Status: remediation-required (systemic tracking item; not blocking for issue #240's own merge)** — unchanged from the prior cycle.
- No canonical `artifacts/csharp/coverage.xml` exists in this repository session (independently re-confirmed this cycle via `find`). Per the mandatory Coverage Verification Procedure, this is a FAIL for the "Repo-wide per language" gate for C# (C# has changed files on this branch).
- This condition pre-dates issue #240 and is not attributable to either of this issue's two commits (the original fix or this cycle's file-size split); issue #240's own new/changed-code coverage remains independently verified at 100%.
- Evidence: `policy-audit.2026-07-06T13-00.md` §1.2.2, §5, §8 item 2.
- Recommended action: unchanged from the prior cycle — track under the repository's existing `feature/csharp-coverage-uplift` initiative; produce a canonical, multi-project coverage merge (`artifacts/csharp/coverage.xml`) covering all first-party C# test projects, scoped to the testable denominator after applying the ratified COM/VSTO/WinForms exclusions.
- Owner/next step: repository maintainer / CI-infrastructure owner, not this issue's executor. This item is carried forward unchanged because it is outside the scope of both of issue #240's commits.

## Finding 3 — Informational: PR-context summary misclassifies changed C# files

- **Status: informational (process/tooling gap, not a code defect)** — unchanged from the prior cycle; independently re-confirmed against the refreshed `artifacts/pr_context.summary.txt`/`artifacts/pr_context.appendix.txt` for this cycle.
- `artifacts/pr_context.summary.txt`'s "Changed files overview" still reports "Core logic changes: 0 files" and buckets all changed `.cs`/`.csproj` files into "Docs/templates/agents/tooling: 42 files."
- Evidence: `policy-audit.2026-07-06T13-00.md` "PR-Context Artifact Reliability Note"; direct comparison against `git diff --stat`/`git diff --name-status`.
- Recommended action: unchanged — fix the PR-context summary generator's file-classification logic so `.cs`/`.csproj` core-logic files are not bucketed into "docs/templates/agents/tooling." Reviewers should continue to independently verify scope via `git diff`.
- Owner/next step: owner of the PR-context artifact generation tooling.

## Finding 4 — Documentation discrepancy: AC5 check-off vs. review verdict

- **Status: remediation-required (documentation reconciliation)** — unchanged from the prior cycle.
- `issue.md` AC5 remains checked `[x]`, but this cycle's independent verdict for AC5 remains PARTIAL (Finding 2 above is the specific cause).
- Evidence: `feature-audit.2026-07-06T13-00.md` "Acceptance Criteria Check-off" section.
- Recommended action: unchanged — maintainer should either (a) narrow AC5's wording to explicitly scope "repository line coverage" to the `UtilitiesCS` project/testable denominator actually measured, or (b) wait for Finding 2's canonical repo-wide artifact before treating AC5 as fully satisfied.
- Owner/next step: maintainer (Dan Moisan) / issue #240 owner.

## Remediation Priority Summary

| Finding | Severity | Blocking for #240 merge? | Status this cycle |
|---|---|---|---|
| 1. Test file 500-line limit | Was Blocking | N/A | **RESOLVED** |
| 2. Repo-wide C# coverage artifact absent | Systemic / FAIL per procedure | No (pre-existing, tracked separately) | Open, unchanged |
| 3. PR-context summary misclassification | Informational | No | Open, unchanged |
| 4. AC5 check-off discrepancy | Documentation | No (but should be reconciled) | Open, unchanged |

## Note on Handoff Scope

No new Blocking finding was identified in this cycle. Findings 2-4 are pre-existing, systemic, or informational items that are explicitly out of scope for issue #240's own executor (per Finding 2's and Finding 4's "Owner/next step" fields, both point to the repository maintainer/CI-infrastructure owner, not this issue's plan). This artifact does not, by itself, require opening a new atomic-planner remediation cycle scoped to issue #240; it restates the open, cross-cutting items for maintainer visibility and repository-wide tracking, consistent with the prior cycle's disposition.
