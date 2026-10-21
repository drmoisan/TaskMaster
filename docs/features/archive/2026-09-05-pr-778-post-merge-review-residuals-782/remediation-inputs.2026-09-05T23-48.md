# Remediation Inputs — Issue #782 (pr-778-post-merge-review-residuals)

- **Date:** 2026-09-05
- **Reviewer:** feature-review agent
- **Base:** `main` -> `origin/main` @ `77c6d31404e2bc2291aec7eb9561e393c20cdcae`
- **Head:** `refactor/pr-778-post-merge-review-residuals-782` @ `4ed2f790e96d8c22abd36514db3848b71e073912`

## Read this first

This document exists because two enumerated coverage triggers fire **mechanically** against the
feature-review contract. It does **not** represent a no-go verdict.

- **Blocking findings: 0.**
- **Code defects requiring a fix before merge: 0.**
- **Acceptance criteria failing for a reason attributable to the delivery: 0.**
- **Overall review verdict: PASS. Recommendation: GO for pull request.**

Both items below are **procedural**. Each carries a recommended disposition, and for each the
recommendation is that a maintainer accept it rather than that an executor change code. Two further
items (R3, R4) are Should-fix improvements carried from the code review; neither blocks.

Companion artifacts:

- `policy-audit.2026-09-05T23-48.md`
- `code-review.2026-09-05T23-48.md`
- `feature-audit.2026-09-05T23-48.md`

## R1 — Canonical C# coverage artifact absent (procedural, recommend accept)

**Trigger:** "coverage artifact absent for any language that has changed files."

**Reason, as the contract requires it be stated:** coverage artifact absent for C#; coverage
verification is mandatory for all languages with changed files.

**Facts.**

- `artifacts/csharp/coverage.xml` does not exist. The `artifacts/csharp/` directory does not exist.
- This is deliberate and documented as scope decision SD1 in `spec.md` Constraint 11 and Non-Goals.
- Equivalent raw evidence is present on disk: `coverage/782-p0-baseline.cobertura.xml` (18,144,506
  bytes) and `coverage/782-p7-final.cobertura.xml` (18,144,107 bytes).
- This reviewer re-derived **every** repo-wide, per-package, per-file, and changed-line figure
  directly from those two documents. No coverage question was left unanswerable by the absence.
- A committed package-level summary in JaCoCo counter form exists at
  `evidence/qa-gates/coverage-summary.2026-09-05T23-11.md`; its per-package rows reconcile exactly
  with this reviewer's independent aggregation.

**Recommended disposition: ACCEPT, no remediation.** The rule exists to guarantee that coverage can
be verified. Coverage was verified, independently and from raw data rather than from a summary. Note
also that one stated reason for SD1 — that producing the artifact "would force a FAIL verdict" — is
not a legitimate reason to omit it, and this reviewer recorded the FAIL regardless. The omission is
acceptable on the strength of the substitute evidence, not on the strength of that rationale.

**If a maintainer prefers remediation instead:** convert `coverage/782-p7-final.cobertura.xml` to
JaCoCo and write it to `artifacts/csharp/coverage.xml`. Expect the repo-wide row to read FAIL at
84.51% against the 85% floor either way; the conversion changes the artifact's presence, not the
verdict.

## R2 — `UiThread.cs` modified-file coverage below the 80% trigger floor (procedural, recommend waive)

**Trigger:** "coverage regression below policy threshold (< 80% ... for modified files)."

**Facts.**

| Metric | Baseline | Head | Floor |
|---|---|---|---|
| Line | 77.11% (64/83) | 76.83% (63/82) | 85% uniform / 80% trigger |
| Branch | 65.00% (13/20) | 65.00% (13/20) | 75% uniform |

**The decisive measurement, which no delivery artifact records.** This reviewer compared the covered
and uncovered line sets between the two Cobertura documents rather than comparing percentages:

```
BASELINE uncovered (19): 28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120
POST     uncovered (19): 28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120
IDENTICAL SETS: True
```

**Not one line transitioned from covered to uncovered.** The uncovered residue is unchanged in both
membership and line number. The -0.28 point movement is the arithmetic consequence of removing one
covered line from a file whose uncovered count is fixed at 19: the covered three-line wrapped
`throw` collapsed to a single line when routed through the shared constant. All 7 changed executable
production lines on the branch are covered.

The residue sits entirely in members the diff never touched:

| Lines | Member | Why uncovered |
|---|---|---|
| 28-34 | `Init` parameter handling | pre-existing |
| 67-76 | `ThreadMonitor` construction inside `Initialize()` | requires a live UI thread; constructs and shows a hidden WinForms `SyncContextForm` |
| 118-120 | lazy `UiSyncContext` accessor | pre-existing |

**Recommended disposition: WAIVE.** Against the four-part precedent test for a sub-floor modified
file: no changed-line regression (satisfied), residue pre-existing and untouched (satisfied), at or
above 80% (not satisfied), improved versus baseline (not satisfied in percentage terms only). The two
unsatisfied legs are both artefacts of the same denominator arithmetic, and the underlying intent of
both — "did any line get worse?" — is satisfied exactly, with proof.

**If a maintainer prefers remediation instead:** raising `UiThread.cs` above 80% requires covering the
`ThreadMonitor` block at lines 67-76 inside `Initialize()`. That is host-bound WinForms code with UI
thread affinity, and covering it would mean either a seam extraction on production code or a host
harness — both production behaviour changes well outside a Refactor's scope, and the same class of
change this delivery already carved out to issues #787 and #788. **Recommendation: do not remediate
in this branch.** If pursued, promote it as its own entry.

## R3 — Message constant text is unpinned (Should-fix, non-blocking)

Carried from code review finding **CR-1**.

No test asserts the value of `UiThread.DispatcherNotInitializedMessage`. Both message assertions use
the wildcard `WithMessage("*UiThread.Init()*")`, which would still match if the removed tail "before
yielding folder tree work" were restored. `spec.md` AC10 and
`evidence/other/code-review.2026-09-05T23-00.md` entry (b) both state the removal "is pinned by" that
assertion; it is not.

**Recommended fix (small, low risk).** In `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`
and `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, change the assertion to
`.WithMessage(UiThread.DispatcherNotInitializedMessage)`. The constant is `internal` and
`UtilitiesCS/Properties/AssemblyInfo.cs` grants `InternalsVisibleTo("UtilitiesCS.Test")`, so it is
reachable; the literal contains no `*` or `?`, so it behaves as an exact match. Optionally extend the
same assertion to the two C26 tests, which currently assert only the exception type (finding CR-3).
Then correct the two "pinned by" sentences.

**Recommended disposition: fix in this branch if convenient, otherwise promote.** It does not block.

## R4 — Baseline coverage figures not reproducible from the recorded document (Should-fix, non-blocking)

Carried from code review finding **CR-2 / EV-1**.

`evidence/baseline/p0-t7-coverage.md` records re-measured baseline figures of 112,355 lines covered
and 26,500 branches covered. Re-aggregating `coverage/782-p0-baseline.cobertura.xml` — the output
path that artifact's own command names — with that artifact's own pinned all-descendant `.//line`
selection yields **112,359 and 26,496**, the values the artifact labels "superseded" and declares
invalid as a baseline side. The file's `CreationTime` and `LastWriteTime` are both
`2026-09-05 19:26:55`, while the artifact carries `Timestamp: 2026-09-05T21-59`.

**Impact on any verdict: none.** Head reads 112,363 and 26,500. Against either candidate baseline the
conclusion is the same — line coverage improved and branch coverage improved or held. No regression
exists on any reading.

**Recommended fix.** Amend `evidence/baseline/p0-t7-coverage.md` to state that the re-measurement's
output document was not retained, that the retained document is the 19:26 collection taken at the
re-anchored base `736c2cf2` (committed 19:17, before the first production edit at 20:37), and that it
yields 112,359 / 26,496. Remove the instruction declaring those figures invalid as a baseline side,
since they are the only reproducible ones. Alternatively, re-run the baseline collection so the
document matches the recorded figures.

**Recommended disposition: amend the artifact.** It does not block.

## Items explicitly NOT requiring remediation

| Item | Why not |
|---|---|
| Repo-wide C# line coverage 84.51% below the 85% floor | Pre-exists on `origin/main` at 84.50% and improves. Reflects the unreconciled CLAUDE.md 80% versus `.claude/rules` 85% documentation conflict, which is a repository-level matter. |
| AC-U1 unchecked | Requires a pull request that does not yet exist. Correctly open; not a delivery defect. |
| C03 omitted | Discharged through AC2's omission branch with a full measured record, bisect, and mechanism, and promoted as issue #788. This is the correct handling of a withdrawn item. |
| Three `spec.md` passages still describing the C03 re-arm | Already enumerated and disclosed by the delivery's own code-review artifact. Accepted as a recorded decision. |
| Test files in `<Project>.Test/` rather than a `tests/` tree | Repository-wide pre-existing convention; not introduced by this branch. |
| Nits CR-4 through CR-7 | Minor test-hygiene observations, several consistent with existing repository precedent. Recorded in the code review; none blocks. |

## Handoff

No atomic remediation plan is requested. R1 and R2 are maintainer acceptance decisions rather than
executor work, and R3 and R4 are small optional improvements. If a maintainer elects to act on R3 and
R4, they are a single small task each and do not warrant a phased plan.

**One action is required before merge, and it is not remediation:** the pull request must close
**only #782**. The seven auto-close candidates in `artifacts/pr_context.summary.txt` (#394, #449,
#476, #493, #508, #584, #778) are prose scrapes from this delivery's own artifacts and must not be
carried into the PR body.
