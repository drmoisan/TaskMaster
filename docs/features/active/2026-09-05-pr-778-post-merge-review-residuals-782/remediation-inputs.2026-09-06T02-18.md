# Remediation Inputs — Issue #782 (pr-778-post-merge-review-residuals)

- **Date:** 2026-09-06
- **Reviewer:** feature-review agent (re-audit, cycle 2)
- **Base:** `main` -> `origin/main` @ `77c6d31404e2bc2291aec7eb9561e393c20cdcae`
- **Head:** `refactor/pr-778-post-merge-review-residuals-782` @ `e053a4f2305502adb09afe6bcc9a26351804f6fe`
- **Companion artifacts:** `policy-audit.2026-09-06T02-18.md`, `code-review.2026-09-06T02-18.md`, `feature-audit.2026-09-06T02-18.md`

## Read this first

This document exists because two enumerated coverage triggers fire **mechanically** against the
feature-review contract, and because three new non-blocking accuracy nits were found. It does **not**
represent a no-go verdict.

- **Blocking findings: 0.**
- **Code defects requiring a fix before merge: 0.**
- **Acceptance criteria failing for a reason attributable to the delivery: 0.**
- **Overall review verdict: PASS. Recommendation: GO for pull request.**

The word "Blocking" appears nowhere in this document as a severity assignment. Every item below is
either procedural or a documentation nit.

R3 and R4 from cycle 1 are **closed**. Both were verified fixed by independent measurement, not by
reading the delivery's own assertions. They are not restated as open items.

## Status of the cycle-1 inputs

| Cycle-1 finding | Cycle-2 state | Basis |
|---|---|---|
| R1 — canonical C# coverage artifact absent | **Recurs, unchanged** | `artifacts/csharp/coverage.xml` and the `artifacts/csharp/` directory still do not exist. The finding is a property of scope decision SD1, which the remediation did not touch and was not asked to. Disposition recorded at `evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md`. |
| R2 — `UiThread.cs` modified-file coverage below the 80% trigger | **Recurs, unchanged** | Re-derived at the new head: 77.11% -> 76.83%, uncovered line set identical. The remediation changed no production file, so the figure could not move. Disposition recorded in the same document. |
| R3 — the message-pinning claim was false | **CLOSED** | The assertions now read `WithMessage(UiThread.DispatcherNotInitializedMessage)` at `UiThread_Tests.cs:144` and `WpfDispatcherYieldTests.cs:136`; the wildcard form is gone from both. The corrected claim is backed by an observed run, corroborated by the TRX at `TestResults/782-r1-p1t7` which this reviewer read directly: `outcome="Failed"`, 2 total, 1 passed, 1 failed. |
| R4 — the baseline coverage record named the wrong input document | **CLOSED** | Independently confirmed: aggregating `coverage/782-p0-baseline.cobertura.xml` returns exactly `LINES_COVERED=112359 LINES_VALID=132967 BRANCHES_COVERED=26496 BRANCHES_VALID=33480`, the figures the amendment attributes to the retained document. The discriminating observation the amendment uses — `Total tests: 6992` in the companion log versus 6997 in `p0-t6-vstest.md:71` — is the right kind of evidence, because it is a value the run wrote rather than mutable filesystem metadata. |

## R1 — Canonical C# coverage artifact absent (procedural, recommend accept again)

**Trigger:** "coverage artifact absent for any language that has changed files."

**Reason, as the contract requires it be stated:** coverage artifact absent for C#; coverage
verification is mandatory for all languages with changed files.

**Facts, re-derived at the new head.**

- `artifacts/csharp/coverage.xml` does not exist. The `artifacts/csharp/` directory does not exist.
- Deliberate and documented as scope decision SD1 in `spec.md` Constraint 11 and Non-Goals.
- Four raw Cobertura documents are present under `coverage/`, all git-ignored by `.gitignore:144`
  (`coverage/*` with only `coverage/.gitkeep` re-included): `782-p0-baseline` (18,144,506 bytes),
  `782-p7-final` (18,144,107), `782-r1-baseline` (18,144,083), `782-r1-final` (18,144,167).
- This reviewer aggregated all four independently. Every repo-wide, per-package, per-file, and
  changed-line figure in this cycle's policy audit was derived from them, under two selections. No
  coverage question was left unanswerable by the absence.
- The committed summary at `evidence/qa-gates/coverage-summary.2026-09-05T23-11.md` and the gate at
  `evidence/qa-gates/r-p4-t5-tests-coverage.md` reconcile exactly with that independent aggregation.

**Recommended disposition: ACCEPT, no remediation.** Unchanged from cycle 1, and for the same reason:
the rule exists to guarantee that coverage can be verified, and coverage was verified, independently
and from raw data rather than from a summary.

**The cycle-1 qualification stands and is restated.** One stated reason for SD1 — that producing the
artifact "would force a FAIL verdict" — is **not** a legitimate reason to omit it, and this reviewer
recorded the FAIL regardless. The acceptance rests on the strength of the substitute evidence, not on
that rationale. The `evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md` document
reproduces this qualification in full rather than paraphrasing it away, which is the correct handling.

**If remediation is preferred instead:** convert `coverage/782-r1-final.cobertura.xml` to JaCoCo and
write it to `artifacts/csharp/coverage.xml`. Expect the repo-wide row to read FAIL at 84.50% against
the 85% floor either way; the conversion changes the artifact's presence, not the verdict.

## R2 — `UiThread.cs` modified-file coverage below the 80% trigger (procedural, recommend waive again)

**Trigger:** "coverage regression below policy threshold for modified files."

**Facts, re-derived at the new head from the raw Cobertura with a class-level selection.**

| Metric | Baseline | Head | Floor |
|---|---|---|---|
| Line | 77.11% (64/83) | 76.83% (63/82) | 85% uniform, 80% trigger |
| Branch | 65.00% (13/20) | 65.00% (13/20) | 75% uniform |

**The decisive measurement, reproduced independently this cycle:**

```text
BASELINE uncovered (19): 28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120
HEAD     uncovered (19): 28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120
IDENTICAL SETS: True
```

Not one line moved from covered to uncovered. The -0.28 point movement is arithmetic: a covered
three-line wrapped `throw` collapsed to a single line when routed through the shared constant, so
numerator and denominator each fell by one against a residue fixed at 19.

**Recommended disposition: WAIVE.** Unchanged from cycle 1. Raising the file above the floor requires
covering the `ThreadMonitor` construction block at lines 67-76 inside `Initialize()`, which is
host-bound WinForms code with UI-thread affinity that constructs and shows a hidden `SyncContextForm`.
Covering it requires either a production seam extraction or a host harness — the same class of change
already carved out to #787 and #788. Do not remediate in this branch.

## N1 — Absolute host path in two committed artifacts (nit, recommend fix, non-blocking)

**Locations.**

```text
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/plan.2026-09-05T15-47.md:42
**Worktree root.** All other paths are relative to `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-05T10-47`.

docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/research/research.2026-09-05T16-10.md:6
- Research root (worktree): `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-05T10-47`
```

**This finding corrects a cycle-1 error by this reviewer.** `policy-audit.2026-09-05T23-48.md` row 2.11
recorded PASS with the evidence "Evidence artifacts substitute `<worktree>` for host paths and
explicitly decline to reproduce vstest-generated TRX filenames." That sentence is true of the 90
changed files under `evidence/`, where the substitution is complete. It is not true of the criterion
as stated, which covers artifacts generally, and the plan and research documents are artifacts of this
delivery. Cycle 2 records the row as FAIL and states the correction rather than silently re-scoping the
criterion to fit the evidence.

**Why it is not blocking.** The prohibition is a reviewer convention rather than repository policy: no
file under `.claude/rules/` and no section of `CLAUDE.md` states it. `git grep -l` over `docs/**` at the
base commit returns **827 committed documents** already carrying the same path, so two more occurrences
do not change the repository's exposure, and gating the pull request on a standard `origin/main` does
not meet would be disproportionate.

**Recommended fix, if elected.** Replace the absolute path with `<worktree-root>` on both lines. Two
documentation lines, no `.cs` file touched, no toolchain pass required.

## N2 — The R1/R2 disposition record asserts maintainer authority that no record supports (nit)

**Location.** `evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md`, title line: "Maintainer
disposition of findings R1 and R2 — issue #782".

**What this reviewer looked for and did not find.**

- `artifacts/orchestration/orchestrator-state.json` carries a `remediation_disposition` object with
  `decided_at: "2026-09-06T00:20:00Z"` and **no actor field**. Its
  `rationale_for_fixing_two_non_blocking_findings` value is written in the orchestrator's voice.
- The same file's `human_interaction` key is **`null`**.
- The document's own body attributes itself accurately: "It is written by task [P3-T7] of the
  remediation plan `remediation-plan.2026-09-06T00-15.md`."

The recorded decider is therefore the orchestrator, not the maintainer. It is entirely possible a human
ratified this in session and it was simply not logged; this finding states what the artifacts support,
not what did or did not happen.

**Why it matters beyond wording.** `CLAUDE.md` UT2 requires that the COM/VSTO coverage exemption "be
ratified by the project maintainer". R2 is adjacent to that class of decision. A committed document
titled as a maintainer disposition could later be cited as the ratification it is not.

**Recommended fix, if elected.** Either retitle to name the actual decider — for example "Disposition of
findings R1 and R2" with a line stating the orchestrator decided them and citing
`artifacts/orchestration/orchestrator-state.json` — or add a one-line maintainer ratification record.
Either resolves it. One documentation line.

## N3 — `user-story.md` AC-U2 names the withdrawn C03 behavior as delivered (nit)

**Location.** `user-story.md`, AC-U2, currently checked `[x]`:

> AC-U2: The delivery introduces no production behavior change other than the text of the
> `InvalidOperationException` message and the retry-after-failed-initialization behavior of
> `UiThread.Init()`, both of which are stated in the specification's Behavioral Contract.

**Facts.** C03 was withdrawn at commit `92c43665` after a measured regression, and
`UtilitiesCS/Threading/UiThread.cs`'s `Init()` is byte-identical to its `pre-782-base` form — the
branch diff for that file touches only the `Dispatcher` property region. `spec.md` handles the
withdrawal correctly and at length in its Behavioral Contract, and `spec.md` AC2 routes C03 through its
omission branch explicitly. Only AC-U2 was not updated.

**Why it is a nit and not a Should-fix.** As a proposition the AC still holds: "no production behavior
change other than A and B" is satisfied by delivering only A. The trailing clause "both of which are
stated in the specification's Behavioral Contract" is also literally true, since B is stated there as
withdrawn. A reader who follows the pointer finds the full explanation. This is staleness relative to
final scope, not a false claim.

**Recommended fix, if elected.** Reword AC-U2 to name only the message text, or append "the latter
withdrawn under SD18 and promoted as #788". One line in an AC source document; no AC state change.

## Informational findings — no action requested

These are recorded so a future reader does not rediscover them. None is a defect of this delivery and
none requires a change on this branch.

| ID | Observation |
|---|---|
| N4 | The delivery's pinned SD22 `.//line` Cobertura selection double-counts: a `<class>` carries both a class-level `<lines>` block and per-method `<lines>` blocks over the same source lines. The proof is internal to the document — its own root attribute `lines-valid="83068"` is smaller than the 132961 the `.//line` form reports over a strict subset of packages. The impact is 0.0021 points (84.4992% versus a class-level 84.5013%) and both sides of every comparison use the same selection, so no figure the delivery states is materially wrong. Future work should prefer `classes/class/lines/line` de-duplicated by line number. |
| N5 | `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` has two changed lines and is absent from every Cobertura document, baseline and head alike, because `RibbonViewer` carries `[ExcludeFromCodeCoverage]` on the `RibbonViewer.cs` partial. That attribute is pre-existing on `origin/main` at `RibbonViewer.cs:32` and falls under the ratified COM/VSTO exemption in `CLAUDE.md` UT2. The changed lines were verified behavior-preserving by inspection instead: `var dispatcher = UiThread.Dispatcher;` throws when the static is unset on the base commit as well as at head, so `dispatcher != null` was already unreachable-false before the branch. |
| N6 | `artifacts/pr_context.summary.txt` reports `Core logic changes: 0 files` while 15 `.cs` and 1 `.csproj` file changed. The three bucket counts sum to 110, exactly the `.md` count, so all 16 code files are absent from every bucket rather than misfiled. Consequence, simulated by dot-sourcing the hook: `Get-ChangedLanguageSet` returns an **empty** language set from this summary, so `.claude/hooks/validate-feature-review-coverage.ps1` performs only its artifact-path checks. This is a recurring generator defect, not a delivery defect. |
| N7 | The `Close candidates` author-asserted list in the same summary holds 22 entries scraped from prose, including the non-issues `#ISO-8601`, `#S2-1`, and `#S3-1` through `#S4-2`, plus eight unrelated real issues (#394, #449, #476, #493, #508, #584, #778, #780). The only issue this branch closes is **#782**. #787 and #788 must remain open. |
| N8 | `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs` opens with `#nullable enable annotations` rather than `#nullable enable`. This is the established idiom in the assembly (17 occurrences of each of the enable and restore forms) and is consistent, but it means the file receives annotation syntax without `CS86xx` flow analysis, so the nullable gate is a no-op over it. `ResolveDispatcherField` assigns a possibly-null `FieldInfo` to a non-nullable local without a diagnostic; the subsequent `Should().NotBeNull(because: ...)` makes the runtime behavior safe, so this is a gate-coverage note rather than a defect. |

## Handoff

No remediation is required for merge. If the delivery elects to close N1, N2, and N3, the same
reasoning that justified fixing R3 and R4 applies — this delivery exists to remove accuracy defects
from audit artifacts, and all three are accuracy defects in its own artifacts. All three are
documentation-only, total four edited lines across three files, touch no `.cs` file, and therefore
require no toolchain pass and cannot move any coverage counter. They can be handled in one commit.

If they are instead accepted as-is, record the acceptance in the delivery's evidence with the actual
decider named, which is itself the substance of N2.
