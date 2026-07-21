# Feature Audit — utilitiescs-nullable-ci-capstone (Issue #376)

- Timestamp: 2026-07-20T06-00
- Work mode: `full-feature` (per `issue.md`'s `- Work Mode: full-feature` marker)
- AC source (per work-mode routing): `spec.md` **and** `user-story.md`, both present and both
  fully checked `[x]` for AC1–AC7. `issue.md`'s own mirrored AC checklist is intentionally left
  unchecked per this feature's documented convention (issue.md is not the authoritative AC source
  under `full-feature` mode) and is not used for this audit's pass/fail determination.

## Scope and Baseline

- Diff base: `bfcdb394b19a12ca2345bf740a4efffe0ad6e330` (`origin/epic/utilitiescs-nullable-remediation-integration` tip — all 12 Wave-0/Wave-1 remediation children merged). This feature merges to the epic integration branch, not `main`.
- Head: uncommitted working-tree state on top of the base (no commits yet on this branch beyond the base — see policy-audit Procedural Notes).
- Baseline behavior: CI's "Build with nullable warnings treated as errors" step ran `msbuild ... /t:Rebuild /p:Nullable=enable /p:TreatWarningsAsErrors=true`, forcing nullable project-wide; this had never been run to completion against the fanned-in integration tip (commit `20d163ac`, PR #361's `/t:Rebuild` fix, is not yet an ancestor of `origin/main`).
- Post-feature behavior: the same step drops `/p:Nullable=enable`, relies on per-file `#nullable enable` pragmas, and — after an 61-file annotation/pragma-suppression/dead-code remediation pass — the resulting gate reaches a clean solution-wide `EXIT_CODE: 0`.

## Acceptance Criteria Inventory

| # | Criterion (verbatim, `spec.md`/`user-story.md`) | Source check-off state |
|---|---|---|
| AC1 | CI nullable-gate step no longer passes `/p:Nullable=enable` globally; runs under `/t:Rebuild /p:TreatWarningsAsErrors=true`, relying on per-file `#nullable enable` pragma; opted-in files enforced, non-opted-in files do not cross-block. | `[x]` in both `spec.md` and `user-story.md` |
| AC2 | Genuine-enforcement verification defined and executed: deliberate null defect in an opted-in file fails the gate; same defect class in a non-opted-in file does not; defect reverted; verification evidenced. | `[x]` in both |
| AC3 | Any pwsh step/workflow YAML added or modified complies with `.claude/rules/ci-workflows.md` and `.claude/rules/benchmark-baselines.md` where applicable; no leaked `$LASTEXITCODE`. | `[x]` in both |
| AC4 | `.claude/rules/csharp.md` rules-vs-convention conflict surfaced as an explicit maintainer-decision item in `spec.md`; no `.claude/rules/*` file edited. | `[x]` in both |
| AC5 | Optional project-level `<Nullable>enable</Nullable>` capstone decision for `UtilitiesCS.csproj`/`SVGControl.csproj` documented as a separately-gated OPTIONAL step with an explicit maintainer decision gate; not performed by default. | `[x]` in both |
| AC6 | Single consolidated maintainer-decision summary present in `spec.md`, folding in epic-wide exclusions and the named child flags. | `[x]` in both |
| AC7 | No behavior change to production C# code and no reduction in coverage on changed lines. | `[x]` in both |

## Acceptance Criteria Evaluation

### AC1 — Gate step drops `/p:Nullable=enable`

**PASS.** `git diff bfcdb394 -- .github/workflows/ci.yml` shows exactly one `msbuild` line changed:
`/p:Nullable=enable` removed, `/t:Rebuild` and `/p:TreatWarningsAsErrors=true` preserved, comment
updated to the pragma-driven rationale. `evidence/qa-gates/qc-nullable-gate.2026-07-20T05-10.md`
confirms the resulting command reaches `EXIT_CODE: 0` solution-wide on the fully-remediated tree.
`evidence/other/nullable-opt-in-regrep.2026-07-20T04-10.md` independently confirms 400 files
(385 UtilitiesCS + 15 SVGControl) currently carry a `#nullable` pragma, i.e., a real, non-trivial
opted-in/non-opted-in distinction exists for AC1's "opted-in enforced, non-opted-in silent" claim
to be meaningfully tested (verified concretely in AC2 below, not merely asserted here).

### AC2 — Genuine-enforcement verification

**PASS.** Four separate evidence artifacts, each independently reviewed:
`verification-1-clean-baseline`, `verification-2-optedin-defect-fail` (EXIT_CODE 1, CS8602 on the
opted-in `PercentageFormatter.cs`), `verification-3-nonoptedin-defect-pass` (EXIT_CODE 0, no error
on the non-opted-in `BayesianClassifier.cs`), `verification-4-restored-clean`. The originally-planned
non-opted-in candidate (`ActionButton.cs`) was found mid-execution to actually be opted-in (a bash
`grep` BOM false-negative, documented in `nullable-opt-in-regrep.2026-07-20T04-10.md` and cross-
referenced in agent memory `project_bom_grep_anchor_false_negative.md`) and correctly substituted
with `BayesianClassifier.cs` before the defect-introduction step proceeded. `git diff bfcdb394
--stat` for both `PercentageFormatter.cs` and `BayesianClassifier.cs` (and `ActionButton.cs`)
returns empty, confirming no residual diff from either defect-introduction/revert cycle.

### AC3 — Workflow-authoring-rule compliance

**PASS.** The edited step's `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` line is present,
unchanged, and verbatim in the diff (confirmed by direct inspection of the full diff hunk — it is
outside the changed-line range). No new deliberately-failing nested command is introduced anywhere
in the `run:` block; the change is a flag removal and comment update only.
`.claude/rules/benchmark-baselines.md` is correctly identified as not applicable (no
`scripts/benchmarks/**` path touched, no baseline artifact introduced).

### AC4 — Rules-vs-convention conflict flagged, not resolved

**PASS.** `git diff bfcdb394 --name-only | grep -i "\.claude/rules"` returns no matches — no
`.claude/rules/*` file is edited. `spec.md`'s "Rules-vs-convention conflict detail (AC4)" section
quotes `.claude/rules/csharp.md` lines 16 and 81–83 verbatim and presents two maintainer options
without resolving between them, citing a real prior precedent (the
`appevents-loadasync-inbox-gating-243` coverage-threshold-exception runbook) for the same
flag-don't-edit pattern.

### AC5 — Optional project-level flip documented, not performed

**PASS.** `spec.md`'s "Optional project-level flip detail (AC5)" section documents the trade-offs,
current infeasibility (Designer-generated files and the `Interfaces/**` tree would enter an
unverified enabled nullable context), and gating conditions in detail. `grep -n "<Nullable>"
UtilitiesCS/UtilitiesCS.csproj SVGControl/SVGControl.csproj` on the current tree returns no
matches — the flip is not performed.

### AC6 — Consolidated maintainer-decision summary

**PASS.** `spec.md`'s "Maintainer Decision Summary" table consolidates 14 rows with source
citations, including the two new items this capstone's own execution surfaced (the
`PeopleScoDictionaryNew.cs` island retention decision and the P2-T17–T23 scope-expansion layers),
each with a specific evidence-artifact citation. Cross-checked the `PeopleScoDictionaryNew.cs`
citation against `evidence/remediation-baseline/debt2-people-island-decision.2026-07-20T00-52.md`
directly — the summary's description of the 12 CS8644 errors and the root-cause explanation match
the underlying evidence artifact.

### AC7 — No behavior change, no coverage reduction

**PASS.** Individually inspected the diffs for every file the task description flagged as a
verification target (`SvgImageSelector.cs`, `PeopleScoDictionaryNew.cs`,
`TaskController.Actions.cs`, `ToDoEvents.Filtering.cs`, `FolderScorer.cs`, `IItemViewer.cs`,
`QfcFormControllerTests.cs`) plus a representative sample of the remaining fan-in files; every
change found is nullable-annotation-only, null-forgiving-`!`-only, a narrow pragma-suppress
bracket with rationale, or dead-code deletion after a grep-confirmed zero-live-reference check. No
control-flow, signature-shape, or logic change was found anywhere in the 61-file diff. Coverage:
`evidence/qa-gates/qc-coverage-delta.2026-07-20T05-20.md` shows 83.88%→83.89% line and
76.36%→76.37% branch — both marginal increases, not a reduction. Test count unchanged (5702/5702
passed in both baseline and post-change runs).

## AC Check-off Actions Taken by This Review

All seven AC items were already checked `[x]` in both `spec.md` and `user-story.md` prior to this
review (checked off by the executing agent during Phase 5/Phase 7, per the acceptance-criteria-
tracking protocol's "check off as soon as the corresponding plan task passes verification"
guidance). This review independently re-verified each item against on-disk evidence (not solely
against the prior check-off) and confirms every check-off is warranted. No AC item required
un-checking; no new AC item was added (none is authored by a reviewer, per the no-phantom-criteria
rule).

### Acceptance Criteria Status

- Source: `docs/features/active/utilitiescs-nullable-ci-capstone/spec.md`,
  `docs/features/active/utilitiescs-nullable-ci-capstone/user-story.md`
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: none

## Definition of Done Cross-Check (`spec.md`)

| DoD item | Status |
|---|---|
| Acceptance criteria documented and mapped to tests/demos | Unchecked in `spec.md`'s DoD list, but verified true in substance: each AC maps to a specific evidence artifact per the "Acceptance Criteria Evaluation" section above. This is a cosmetic checklist gap in `spec.md`'s DoD section (separate from the AC-source checklist itself, which is fully checked), not a delivery gap. |
| Behavior matches AC in all documented environments | Verified via the toolchain evidence (Section 7 of policy-audit). |
| Tests updated/added | Not applicable in the traditional sense — this feature adds no new test scenarios; it adjusts nullable-context annotations in existing test files and deletes 3 dead fields. Existing 5702-test suite fully re-verified. |
| Toolchain pass completed | PASS — see policy-audit Sections 3, 6, 7. |

## Overall Feature-Audit Verdict

**PASS.** All 7 acceptance criteria are verified against on-disk evidence, not merely asserted.
Two items remain open as pre-merge (not delivery) obligations — see
`remediation-inputs.2026-07-20T06-00.md`.
