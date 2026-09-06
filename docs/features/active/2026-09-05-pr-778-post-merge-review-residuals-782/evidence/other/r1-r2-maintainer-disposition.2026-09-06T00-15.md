# Maintainer disposition of findings R1 and R2 — issue #782

Timestamp: 2026-09-06T00-15

This record exists so the dispositions of R1 and R2 survive in the delivery's own evidence rather
than only in the reviewer's input document. It is written by task [P3-T7] of the remediation plan
`remediation-plan.2026-09-06T00-15.md`.

## The review verdict this disposition sits inside

The feature review recorded in `remediation-inputs.2026-09-05T23-48.md` returned **PASS** with:

- blocking findings: **0**;
- code defects requiring a fix before merge: **0**;
- acceptance criteria failing for a reason attributable to the delivery: **0**;
- recommendation: **GO for pull request**.

R1 and R2 are procedural coverage triggers that fire mechanically against the feature-review
contract. Neither is a defect in the delivered code. R3 and R4 are the two Should-fix items this
remediation acts on; they are recorded elsewhere in this plan's evidence and are not restated here.

## R1 — canonical C# coverage artifact absent

**Disposition: ACCEPT, no remediation.**

The reviewer's grounds, quoted from `remediation-inputs.2026-09-05T23-48.md`:

> The rule exists to guarantee that coverage can be verified. Coverage was verified, independently
> and from raw data rather than from a summary.

The supporting facts the reviewer recorded are that `artifacts/csharp/coverage.xml` does not exist
and the `artifacts/csharp/` directory does not exist; that equivalent raw evidence is present as
`coverage/782-p0-baseline.cobertura.xml` and `coverage/782-p7-final.cobertura.xml`; that the reviewer
re-derived every repo-wide, per-package, per-file, and changed-line figure directly from those two
documents, leaving no coverage question unanswerable by the absence; and that the committed
package-level summary at `evidence/qa-gates/coverage-summary.2026-09-05T23-11.md` reconciles exactly
with the reviewer's independent aggregation.

`artifacts/csharp/coverage.xml` is deliberately not produced under scope decision SD1, documented in
`spec.md` Constraint 11 and in the Non-Goals section. No task in this remediation produces it.

### The reviewer's qualification on the SD1 rationale, recorded in full

The reviewer recorded, at `remediation-inputs.2026-09-05T23-48.md:47-51`, that one stated reason for
SD1 — that producing the artifact "would force a FAIL verdict" — **is not a legitimate reason to omit
it**, and that the reviewer recorded the FAIL regardless. The acceptance therefore rests on the
strength of the substitute raw evidence and not on that rationale.

That qualification is reproduced here rather than paraphrased away, because an acceptance recorded
without it would read as an endorsement of a rationale the reviewer explicitly rejected.

## R2 — `UiThread.cs` modified-file line coverage below the 80% trigger floor

**Disposition: WAIVE.**

The reviewer's decisive measurement, quoted from `remediation-inputs.2026-09-05T23-48.md`:

> BASELINE uncovered (19): 28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120
> POST     uncovered (19): 28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120
> IDENTICAL SETS: True

The uncovered line set is identical in both membership and line number between the baseline and the
head. Not one line transitioned from covered to uncovered. The recorded movement from 77.11% to
76.83% is the arithmetic consequence of removing one covered line from a file whose uncovered count
is fixed at 19: the covered three-line wrapped `throw` collapsed to a single line when routed through
the shared constant. All seven changed executable production lines on the branch are covered, and
branch coverage is unchanged at 65.00%.

### Why raising the file above the floor is promoted rather than performed here

Raising `UiThread.cs` above the 80% trigger floor requires covering the `ThreadMonitor` construction
block at lines 67-76 inside `Initialize()`. That is host-bound WinForms code with UI-thread affinity
which constructs and shows a hidden `SyncContextForm`. Covering it would require either a seam
extraction on production code or a host harness, both of which are production behaviour changes
outside this delivery's scope and the same class of change already carved out to issues #787 and
#788. The reviewer's own recommendation is not to remediate in this branch and to promote it as its
own entry if pursued.

No task in this remediation changes `UtilitiesCS/Threading/UiThread.cs` or adds coverage for its
`ThreadMonitor` block.

## No file was changed for either item

**No file was changed for R1 and no file was changed for R2.** Both are recorded dispositions only.
The complete set of files this remediation changes is the two `UtilitiesCS.Test` assertion files,
`spec.md`, three artifacts under this feature's `evidence/` subtree, the new evidence artifacts this
plan writes, and the plan file itself. None of them is `UtilitiesCS/Threading/UiThread.cs`, and none
of them is under `artifacts/`.
