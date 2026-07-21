# Feature Audit — utilitiescs-nullable-dialogs-misc (Issue #374)

- Date: 2026-07-19
- Reviewer: feature-review agent
- Work Mode: full-feature (AC sources: `spec.md` and `user-story.md`)
- Branch: `feature/utilitiescs-nullable-dialogs-misc-374` @ `9b09b1c9`

## Scope and Baseline

- Base branch: `origin/epic/utilitiescs-nullable-remediation-integration` (epic-child PR base, not `main`).
- Merge-base / base tip: `dffadd5a102884dd811ed5731477de18417594f1` (reviewer-confirmed `git merge-base` equals the base tip).
- Scope: full branch diff — 14 C# source files (12 `UtilitiesCS/Dialogs/` substantive + 2 verify-only
  misc: `WindowsAPI/ExtraDeclarations.cs`, `Properties/AssemblyInfo.cs`), plus feature docs, evidence
  artifacts, and agent-memory notes. No project/solution files, no Designer-generated files, no test
  source files, no workflow/benchmark files.
- Baseline evidence: `evidence/baseline/` (analyzers, csharpier, csproj-nullable-absent,
  file-inventory, nullable-pragma-gate, tests-coverage, upstream-precondition-363-batch-d).
- Independent verification performed by the reviewer: source diff read line-by-line; `grep`/`git`
  checks for pragma count (14), post-condition attributes (none), banned APIs (none), Designer files
  (none), csproj `<Nullable>` (none); Cobertura root line/branch-rate parse; and an independent
  isolated `UtilitiesCS.csproj` nullable rebuild (0 CS86xx).

## Acceptance Criteria Inventory

Source files (full-feature): `spec.md` (Definition of Done AC map) and `user-story.md` (Acceptance
Criteria). Both files carry the identical AC1–AC6 set, all currently marked `[x]`.

- AC1: All 14 in-scope files carry `#nullable enable` and compile with zero CS86xx under the per-file
  pragma with `TreatWarningsAsErrors`.
- AC2: No project- or solution-level `<Nullable>` element is introduced; `UtilitiesCS.csproj` retains none.
- AC3: No behavior change to dialog display, button-wrapper, or MyBox logic; existing `UtilitiesCS.Test/Dialogs/` tests still pass.
- AC4: No coverage regression on changed lines.
- AC5: Public signatures remain behavior-compatible; nullability annotations reflect actual null behavior and are consistent with the consumed `WinFormsExtensions.Clone<T>()` contract (#363).
- AC6: Non-remediated files (4 Designer-generated + all files outside the cluster) remain non-opted-in and are not cross-blocked; the change is independently mergeable under the per-file pragma architecture.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence and independent verification |
|---|---|---|
| AC1 | PASS | Reviewer counted exactly 14 `#nullable enable` additions in the diff (`git diff ... \| grep -c '^+#nullable enable'` = 14) covering all 14 named files. `evidence/qa-gates/final-nullable-pragma-gate.md` records isolated `UtilitiesCS.csproj` rebuild EXIT 0 with 0 CS86xx. Reviewer independently reproduced the isolated rebuild: 0 CS86xx; only pre-existing CS0168 (1) and CS0618 (14) surfaced, all in `EmailIntelligence/`/`Extensions/`, none in `Dialogs/`, none nullable-related. |
| AC2 | PASS | Reviewer `grep -c '<Nullable>' UtilitiesCS/UtilitiesCS.csproj` = 0; the only `nullable` hit in the csproj is a comment. No `.csproj/.props/.sln/.targets` file appears in the diff. Corroborates `evidence/qa-gates/final-ac2-csproj-check.md`. |
| AC3 | PASS | `evidence/qa-gates/final-tests-coverage.md`: 5702 passed / 0 failed, identical to baseline. Source diff contains only pragma + `?`/`!` edits — no statement/branch/logic change (reviewer line-by-line read; `evidence/qa-gates/final-signature-compat.md`). AsyncLocal invoker seams untouched. |
| AC4 | PASS | `evidence/qa-gates/final-coverage-delta.md`: per-file covered/total identical baseline vs post-change for all 14 files (delta 0.00%); cluster 93.10% (958/1029) in both runs. Reviewer independently parsed Cobertura root rates: baseline line 0.838032/branch 0.763485, post-change line 0.838187/branch 0.763759 — no regression (marginal improvement). |
| AC5 | PASS | `evidence/qa-gates/final-signature-compat.md` per-file table: all signature changes are additive nullability (`FolderAction?`, `FunctionButton<T>.Value` → `T?`, `InputBox.ShowDialog` → `string?`, `MyBox.ShowDialog<T>`/`FunctionButtonGroup<T>.Result` → `T?`, `MyBoxModeless.showAction` → `Action<MyBoxViewer>?`). Button-wrapper `.Button`/`.Name`/`.Delegate` kept non-null to match `Clone<T>() where T : Control` returning non-nullable `T`, avoiding false nullable propagation into `MyBox`. Existing callers compile and behave identically. |
| AC6 | PASS | `evidence/qa-gates/final-ac6-no-cross-block.md` and reviewer `git diff --name-only`: exactly the 14 cluster files received a pragma; no Designer sibling was edited (`git diff -- '*Designer.cs'` empty). Because `#nullable enable` is lexical/per-file, non-opted files (including the 4 Designer siblings) remain null-oblivious and are not cross-blocked; the change is independently mergeable. |

All six acceptance criteria are PASS with independent verification. No PARTIAL, FAIL, or UNVERIFIED
items.

## Acceptance Criteria Check-off

Both AC source files (`spec.md` Definition-of-Done AC map and `user-story.md` Acceptance Criteria)
already carry AC1–AC6 as `[x]`, applied by the atomic-executor during delivery. The reviewer
verified each check-off is supported by cited evidence (see the Evaluation table above) rather than
trusting the checkbox state. All six are correctly checked; no reviewer edit to the source files is
required and none was made.

### Acceptance Criteria Status
- Source: `docs/features/active/utilitiescs-nullable-dialogs-misc/spec.md`, `docs/features/active/utilitiescs-nullable-dialogs-misc/user-story.md`
- Total AC items: 6
- Checked off (delivered): 6
- Remaining (unchecked): 0
- Items remaining: none

## Summary

Feature #374 delivers per-file `#nullable enable` remediation of the `UtilitiesCS/Dialogs/` cluster
plus 2 verify-only misc files, annotation-only, with zero CS86xx under the per-file pragma, zero test
regressions (5702/0), zero changed-line coverage regression (cluster 93.10%), no project-level
`<Nullable>` element, and no cross-blocking of non-opted-in files. All six acceptance criteria are
independently verified as PASS. Feature-audit verdict: PASS. Zero blocking findings.
