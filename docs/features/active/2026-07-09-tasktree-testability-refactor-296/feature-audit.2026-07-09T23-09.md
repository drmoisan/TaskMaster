# Feature Acceptance Audit — tasktree-testability-refactor (#296)

- Timestamp: 2026-07-09T23-09
- Branch: `feature/tasktree-testability-refactor-296` @ `b320336a`
- Work Mode: `full-feature` → AC sources: `spec.md` + `issue.md` (`user-story.md` intentionally absent for this refactor child)

## Scope and Baseline

Baseline is the merge-base `3f04d50f` with `epic/winforms-testability-refactor-integration`. Pre-change state: `TaskTreeController.cs` was 546 lines, bound to the concrete `TaskTreeForm`, with no `TaskTree.Test` project (0% coverage). The branch splits the controller into partials behind `ITaskTreeForm`/`ITreeVisual`, adds injectable seams, and adds a 37-test MSTest project. This audit evaluates the branch diff against the spec's acceptance criteria and Definition of Done. The three review artifacts (this file, policy-audit, code-review) share the `2026-07-09T23-09` timestamp.

## Acceptance Criteria Inventory

Source `issue.md` §Acceptance Criteria (8 items) and `spec.md` §Definition of Done (16 items, superset). Deduplicated to the distinct verifiable criteria:

1. `ITaskTreeForm` exists, derives from `IForm`, `TaskTreeForm` implements it.
2. `TaskTreeController` depends on `ITaskTreeForm`, not the concrete form.
3. Host-neutral logic separated from COM/WinForms interaction.
4. No production file in `TaskTree` exceeds 500 lines.
5. `TaskTree.Test` project exists, follows the repo MSTest pattern, and is in the solution.
6. No unit test constructs a live form/window or triggers a popup.
7. `TaskTree` project reaches >= 80% line coverage (new files >= 90%).
8. Full C# toolchain (csharpier → analyzers → nullable → MSTest) passes with no regression.
9. Single caller `RibbonController.cs::LoadTaskTree` compiles unchanged (invariant).

## Acceptance Criteria Evaluation

| # | Criterion | Verdict | Evidence |
|---|---|---|---|
| 1 | `ITaskTreeForm : IForm`; `TaskTreeForm` implements it | PASS | ITaskTreeForm.cs L19 `public interface ITaskTreeForm : IForm`; TaskTreeForm.cs L19 `: Form, ITaskTreeForm`; all facade members implemented |
| 2 | Controller depends on `ITaskTreeForm` | PASS | TaskTreeController.cs L21-33 ctor `(IApplicationGlobals, ITaskTreeForm, TreeOfToDoItems, Action<string>=null)`; field `_viewer` is `ITaskTreeForm` (L50). No `TreeLv`/`OlvToDoID`/`SplitContainer1` references in the controller |
| 3 | Host-neutral logic separated | PASS (structural) | Move/tree logic in TaskTreeController.MoveLogic.cs operates against `ITreeVisual` + data model; `ResolveRowStyle` extracted from `FormatRow`. Caveat: E4/E5/E6 leave decision-logic entangled inside exempt COM/live-control methods (policy finding, not an AC failure) |
| 4 | No production file > 500 lines | PASS | Independent awk count: max production file 312 (TaskTreeForm.Designer.cs); controller 206 + 295. #293-style hidden over-500 test file independently checked and absent (max test 447) |
| 5 | `TaskTree.Test` exists, MSTest pattern, in solution | PASS | Non-globbing csproj with explicit `<Compile Include>` for all 3 sources; unique ProjectGuid `{7C4E2B1A-...}`; three ProjectReferences; sln entry + Debug/Release Any CPU platform configs |
| 6 | No test constructs live form/popup | PASS | No `Form`/`Control` constructed; no `Show()`/`ShowDialog()`/`DoEvents`/`Thread.Sleep`/`Task.Delay`; no `[STATestClass]`/`*.StaTests.cs`. STA policy assessed and not exercised |
| 7 | >= 80% line coverage (new >= 90%) | PASS (literal) / caveated | 94.04% TaskTree.dll; controller 95.65%, move-logic 93.29% (final-coverage.md, coverage-delta.md). Caveat A: figure measured WITH the E4/E5/E6 exclusions this review finds Blocking. Caveat B: canonical `artifacts/csharp/coverage.xml` not committed → not independently recomputable. Caveat C: branch coverage (>=75%) not reported |
| 8 | Full toolchain green, no regression | PASS | final-format.md, final-analyzers.md, final-nullable.md, final-coverage.md all EXIT 0; nullable diagnostic set net-decreased from baseline |
| 9 | Single caller compiles unchanged | PASS | final-caller-unchanged.md: `git diff` of RibbonController.cs empty; ctor keeps interface-compatible 3-arg form + optional seam |

## Acceptance Criteria Check-off

All nine literal acceptance criteria evaluate PASS against their stated text; the executor's `[x]` marks in `issue.md` and `spec.md` are accurate to the literal criteria and are left checked. No criterion is un-checked by this review. AC7 is checked with the three recorded caveats.

Note: none of the acceptance criteria literally prohibits `[ExcludeFromCodeCoverage]` on a testable seam. The E4/E5/E6 Blockers are violations of the general-unit-test **Coverage Exclusion Policy** (a repository policy), not failures of a stated acceptance criterion. The feature meets its acceptance criteria while violating that policy; merge is gated on the policy violation, documented in policy-audit and remediation-inputs.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-07-09-tasktree-testability-refactor-296/issue.md`, `spec.md`
- Total AC items (deduplicated): 9
- Checked off (delivered, literal PASS): 9
- Remaining (unchecked): 0
- Items remaining: none

## Summary

The feature delivers all stated acceptance criteria: the `ITaskTreeForm`/`ITreeVisual` seams, the controller retarget, the file-size split, the wired MSTest project, the no-live-control test property, the toolchain green state, and the unchanged caller are all verified against branch evidence and independent inspection. File-size and STA/no-popup properties were re-verified independently (not taken from the evidence file) and hold; no hidden over-500 file exists.

However, three coverage exemptions (E4 `ActivateOlItem`, E5 `ActivateOlItemAsync`, E6 `HandleModelDropped`) are applied to testable seams rather than to irreducible COM/live-control wrappers, in violation of the general-unit-test Coverage Exclusion Policy, and they additionally leave the non-exempt `TreeLvActivateItem`/`Async` valid-type branches uncovered. These are Blocking and must be remediated with seams (typed dispatch / `IExplorerItemActivator` for E4/E5; extracted `RouteDrop` for E6) before merge. See `remediation-inputs.2026-07-09T23-09.md`.

Overall feature verdict: NOT READY TO MERGE — 3 Blocking policy findings (E4, E5, E6).
