# Remediation Inputs — cycle 1 (R1), feature `winformspumphost-suite-determinism-511`

Timestamp: 2026-08-23T20-57
Canonical issue number for this feature is 511. Secondary: 571.
Branch: `bug/winformspumphost-suite-determinism-511-exec`
Feature folder: `docs/features/active/winformspumphost-suite-determinism-511`
Approved plan: `docs/features/active/winformspumphost-suite-determinism-511/plan.2026-08-21T18-10.md`

This document opens remediation cycle 1. It is the sole requirements input for the cycle's
remediation plan. It consolidates the six blocking findings recorded at the 2026-08-22T18-05 halt,
the two human-interaction decisions recorded at 2026-08-23T20-40, and four further orchestrator
scope decisions established by verification on 2026-08-23T20-57.

---

## Part 1 — Ground truth re-verified on 2026-08-23T20-57

Every claim below was re-measured in this worktree by the orchestrator before this document was
written. None of it is carried over on trust from a prior agent's summary.

| # | Claim | How it was verified | Result |
| --- | --- | --- | --- |
| 1 | Branch head is `edb2e63f`, remote in sync, rebased onto `main` `f85a36fa` | `git log --oneline`, `git rev-parse origin/<branch>` | confirmed |
| 2 | Diff vs `main` is exactly three code files, all under `QuickFiler.Test/` | `git diff --stat f85a36fa..HEAD -- '*.cs' '*.csproj'` | confirmed: +118 lines, 3 files |
| 3 | The two false comment blocks are present as described | `sed -n '80,100p'` Part2.cs; `sed -n '428,448p'` ViewerSetupTests.cs | confirmed |
| 4 | `p4-t2` holds exactly ten TRX | `find`/count | confirmed |
| 5 | Nine of the ten runs report `failed=0`; run 5 reports `failed=1` | TRX `ResultSummary/Counters` parsed directly | confirmed |
| 6 | The single failure is `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` | TRX `UnitTestResult[@outcome='Failed']` | confirmed — it is in `UtilitiesCS.Test`, an assembly this diff cannot reach |
| 7 | All four of this child's named tests pass in 10 of 10 runs | TRX per-test outcome scan | confirmed: `{Passed}` only, for all four |
| 8 | The committed distilled record matches the raw TRX | `determinism-ten-runs.2026-08-21T18-10.md` compared against the values re-derived in rows 5-7 | confirmed identical |

Row 8 is the finding that licenses the evidence disposition in Part 3, Decision 3: the committed
markdown is a faithful distillation, so the raw TRX carry no audit value that is not already in the
repository.

---

## Part 2 — Findings carried into this cycle

### Findings resolved by re-scoping the claim, requiring no code change

**A — the remedy is a measured no-op.** The inserted
`_ = await host.InvokeAsync(() => viewer.Handle)` forces a window handle that already exists.
`ItemViewer.InitializeComponent` runs `ISupportInitialize.EndInit()` on both WebView2 children,
which creates their handles, and WinForms creates a parent's handle when a child's is created.

**B — the observed defect is a different root cause.** The only genuine pre-fix failure was seven
expiries at the 60,000 ms `PumpTimeoutMs` under machine load. A missing handle makes
`Control.Invoke` throw immediately; it does not hang for sixty seconds.

**C — the post-fix evidence is statistically weak.** Thirty consecutive green tracked runs is about
a 1-in-4 outcome under the null hypothesis that the change has no effect.

A, B and C are **accepted as accurate**. They are addressed by correcting what this branch claims,
not by changing code. They are stated here so the remediation plan does not attempt to "fix" them
and so the feature review does not re-raise them as unaddressed.

### Findings requiring action in this cycle

**D — two inserted comment blocks assert the opposite of what was measured. (blocking)**

Locations, re-verified 2026-08-23T20-57:

- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`, the comment block
  immediately above the `viewer.Handle` read, currently claiming the viewer "can reach the act with
  no window handle" and that the two WebView2 children "are not dragged into handle creation".
- `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs`, the comment block
  immediately above its `viewer.Handle` read, currently claiming the children "stay handle-less".

Both are false, and both contradict the corrected assertions in
`QfcItemController.InitializationTests.Part3.cs`, which assert the children **are** handle-created.
`CLAUDE.md` C#6.3 requires comments to stay synchronized with behavior.

Required correction: rewrite both blocks to state the measured truth, namely that both WebView2
children, and therefore the parent `ItemViewer`, are already handle-created when construction
returns, via the Designer-emitted `ISupportInitialize.EndInit()` calls. The corrected comment
**must also state that the read is therefore redundant today and is retained deliberately as a
defensive measure**, so that the fixture does not silently depend on a third-party side effect this
repository neither controls nor observes. A comment that merely stops asserting a falsehood, but
still leaves a reader to infer the statement is load-bearing, does not discharge this finding.

**E — spec acceptance criterion 6 is unsatisfiable as worded. (blocking)**

AC 6 requires a test asserting "both WebView2 children remain handle-less". Measurement proves they
are handle-created at construction. Revise AC 6 to assert the measured inherited state.

Three further acceptance criteria are unchecked and must be reconciled against the re-scoped claim
rather than left dangling:

- **AC 3** — "the ten consecutive runs ... are all green". Nine of ten are suite-wide green; run 5
  carries one failure in a sibling-owned assembly. Revise per Decision F below and cite issue #594.
- **AC 8** — 21 pump-host call sites pass in the final run. Satisfied by the Phase 5 final run.
- **AC 13** — the five-step toolchain completes green in a single final pass with coverage captured.
  Satisfied by the Phase 5 final run.
- **AC 14** — `## Rollout & Follow-up` names the filed follow-up issue. The issue now exists: **#592**.

**F — the absolute-zero gate spans a sibling-owned assembly. (resolvable)**

Plan task P4-T2 requires each of ten TRX to record "a failed count of exactly 0" across all nine
assemblies. Run 5's single failure is
`UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`,
which this child's three-file `QuickFiler.Test/` diff cannot reach or fix.

Required correction: narrow P4-T2's zero condition to the classes this child owns, per the ratified
repository precedent that a child's absolute-zero gate is scoped to the classes it owns and the
residual is promoted as its own defect. The residual is already promoted as **#594**. The narrowed
condition is satisfied by the measured evidence in Part 1 rows 5-7 and requires **no re-run**.

---

## Part 3 — Orchestrator scope decisions established 2026-08-23T20-57

These four decisions are made by the orchestrator on verified evidence. They are inputs to the
plan, not open questions.

### Decision 1 — the pull request targets `main`, not the epic integration branch

Verified: `origin/epic/quickfiler-suite-determinism-foundation-integration` is at `e7b4824a` and is
a **strict ancestor** of `origin/main` (`f85a36fa`). `git log integration..main` returns 7 commits;
`git log main..integration` returns 0. PR #595 already merged the integration branch into `main`,
and PR #596 merged the final epic-status document. The epic is closed, recorded as
`deliver_three_children_descope_511`.

Consequences, all of which the plan must honour:

- Opening a PR against the integration branch would produce an **85-file diff carrying 7 unrelated
  `main` commits**, and would merge into a branch that is already fully merged.
- Against `main` the PR shows exactly this child's own commits and 63 files.
- `.github/workflows/ci.yml` triggers `pull_request` on `[main, development]` only. Targeting the
  integration branch yields **zero checks**; targeting `main` runs the real CI workflow. The prior
  checkpoint recorded `ci_gate.applicable: false` on the integration-branch premise. That premise
  is now false: **CI is applicable and is a real gate.**
- `epic_mode` is set to `false` with this rationale recorded. The epic-mode merge-on-green step no
  longer applies, because there is no live integration branch to merge into.

### Decision 2 — the defensive handle read is retained, not deleted

Finding A establishes the statement is a no-op today. The alternative to correcting its comment is
deleting the statement. It is **retained**, because the maintainer's recorded HI-1 decision is to
"keep the fixture hardening", and because the new Part3.cs regression test now pins the inherited
state loudly: if a future WebView2 change makes the children handle-less, that test fails and the
retained read keeps the two named tests working. The plan must not delete the statement. It must
make the comment tell the truth about it, including its present redundancy.

### Decision 3 — raw vstest artifacts are gitignored at the evidence root, then deleted

The worktree carries roughly 1.2 GB of untracked raw output under the evidence tree: 56 `.trx` and
42 `.coverage` files. `.gitignore:140` already excludes `*.coverage` repository-wide; `*.trx` is
**not** excluded, so a `git add -A` would stage roughly half a gigabyte of TRX.

Disposition, in this order:

1. Create `docs/features/active/winformspumphost-suite-determinism-511/evidence/.gitignore`
   excluding `*.trx`, `*.coverage`, and the `vstest.console.exe` per-run scratch directories. This
   closes the `git add -A` hazard for the **new** TRX that Phase 5 will produce, and it lives inside
   the feature's own documentation tree, so it does not touch repository-root configuration and does
   not disturb the three-code-file scope lock.
2. Retain the existing raw artifacts for the duration of this cycle so the re-audit can spot-check
   them.
3. Delete every raw `.trx` and `.coverage` under the evidence tree as the **final** task of the
   cycle, before the pull request is opened.

Justification for deletion: Part 1 row 8 establishes that the committed distilled markdown reproduces
the raw data faithfully. Repository policy rejects committed raw machine coverage artifacts. The
distilled record is the evidence of record.

### Decision 4 — the follow-up issues are already filed; do not re-file them

Verified against GitHub on 2026-08-23T20-57:

| Residual | Issue | State |
| --- | --- | --- |
| Load-induced 60 s `PumpTimeoutMs` cascade — the genuine #511 defect | **#592** | OPEN |
| Three pre-existing `UtilitiesCS.Test` flakes blocking any suite-wide zero gate | **#594** | OPEN |
| Repository-wide analyzer version skew (plan task P6-T20) | **#597** | OPEN |

Also verified: **#511 and #571 are both already CLOSED as `NOT_PLANNED`** (2026-08-23T19:07),
superseded by #592, with the premise-correction comments already posted to both.

The plan must therefore **not** file duplicates, and must **not** carry any closing keyword for
#511 or #571. Plan tasks P6-T1 and P6-T20, which instruct `gh issue create`, are discharged by
#592 and #597 respectively and must be recorded as already-satisfied with the issue number cited,
not re-executed.

Two residuals remain unfiled and are to be promoted through the MCP promotion lifecycle by the
orchestrator, outside the plan's execution scope:

- the orchestrator checkpoint `blocked_reason` enum cannot express a substantive halt;
- repository-wide host-identifier sanitization of the roughly 146 + 157 files outside this feature.

---

## Part 4 — Remaining unexecuted work from the approved plan

32 of 74 plan tasks are unchecked. The plan is otherwise complete through Phase 4.

- **P4-T2** — narrow per Finding F; satisfied by existing measured evidence; **no re-run**.
- **Phase 5 (P5-T1 … P5-T10)** — the final QC loop, entirely unexecuted. Mandatory and
  unconditional: `dotnet tool restore`, scoped `csharpier format`, repo-wide `csharpier check`,
  `msbuild /t:Rebuild` with analyzers, `msbuild /t:Rebuild` with `TreatWarningsAsErrors`, the full
  nine-assembly `vstest.console.exe` run, the coverage capture, the coverage delta check, the
  500-line re-audit, and the clean-pass attestation. `SKIPPED` is not a valid outcome for any of
  them.
- **Phase 6 (P6-T1 … P6-T21)** — acceptance-criteria check-off, the `## Rollout & Follow-up`
  update, the AC status summary, and the commits. Adjusted by Decision 4: the two `gh issue create`
  tasks are already discharged.

Note for Phase 5 and the re-audit: the ten-run determinism evidence in Phase 4 was produced from a
binary whose source differs from the post-remediation source **by comments only**. That is why no
re-run of Phase 4 is required. This must be stated explicitly in the evidence rather than left for
a reviewer to infer.

---

## Part 5 — Exit criteria for this cycle

The cycle exits when the re-audit (`code-review`, `feature-audit`, `policy-audit`) reports a
combined blocking count of zero. Specifically:

1. Both comment blocks state the measured truth, including the present redundancy of the read.
2. Spec AC 6 is revised to the measured inherited state; AC 3 is revised to the owned-class scope
   citing #594; AC 8, AC 13 and AC 14 are satisfied and checked off with cited evidence.
3. P4-T2's zero condition is narrowed to owned classes and recorded as satisfied on existing
   evidence.
4. The Phase 5 toolchain completes green in a single final pass with numeric coverage recorded.
5. The evidence `.gitignore` exists and the raw `.trx` / `.coverage` files are removed.
6. No artifact claims this branch fixes #511 or #571, and no closing keyword for either appears
   anywhere in the branch or in the pull-request body.
7. The spec's `## Scope & Non-Goals` "In scope" bullets no longer assert the falsified premise
   (see Part 6).

---

## Part 6 — Addendum: the spec's Scope section also asserts the falsified premise

Added 2026-08-23T21-10 by the orchestrator after reading `spec.md` lines 90-137. This is a gap in
Part 2's Finding E, which addressed only the `## Acceptance Criteria` section. The following
`## Scope & Non-Goals` "In scope" bullets are contradicted by the measurement and must be revised
in the same cycle:

1. **"Deterministic creation of the `ItemViewer` window handle on the pump thread inside the shared
   pump harness, so that `Control.Invoke` has an existing handle before any act."** The handle
   already exists at construction. Revise to state that the harness makes the *inherited* handle
   state explicit and independent of a third-party side effect, rather than creating a handle that
   would otherwise be absent.

2. **"#571 in full: both named intermittent failures."** Not delivered. #571 is closed as
   `NOT_PLANNED`, superseded by #592. Revise to state that this feature does not fix #571 and that
   the real cause is tracked as #592.

3. **"#511's load-flakiness half: removing the handle race removes one whole class of the
   load-induced failures reported in #511."** There is no handle race; Finding A and Finding B
   falsify this. Revise to state that the load-induced failures are a 60-second `PumpTimeoutMs`
   expiry tracked as #592, which this feature does not address.

The two bullets that remain accurate and should be retained are the regression tests in
`QfcItemController.InitializationTests.Part3.cs` and the empirical determinism record.

Also revise the `## Out of scope / non-goals` bullet that refers to #511's visible-window half
"requiring its own issue" so that it names the issue that now exists (**#592**) rather than
describing one as still to be filed.

The plan must treat this addendum as part of Finding E. A revision that corrects AC 6 but leaves
the Scope section asserting that this feature fixes #571 would leave the specification internally
contradictory, and the feature audit would correctly raise it.
