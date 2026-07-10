# Feature Audit — Issue #293 (tagcontroller-testability-refactor)

- Feature folder: `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/`
- Work mode: `full-feature`
- AC source: `spec.md` `### Acceptance Criteria` and `issue.md` `## Acceptance Criteria` (identical 9-item set). `user-story.md` waived per epic #295 (recorded in issue.md "AC Source Exception").
- Review timestamp: 2026-07-09T22-52

## Scope and Baseline

- Base branch (resolved): `epic/winforms-testability-refactor-integration`
- Merge-base SHA: `3f04d50f6544f084323e5d7a9a563facb9d579df`
- Head SHA: `55a4835659f977a0dce9e1f5f872b121b659167d`
- Baseline for regression comparison: merge-base `3f04d50f` and the executor Phase 0 baseline coverage (`evidence/baseline/baseline-coverage.md`, Tags.dll 67.28%).
- The `### Acceptance Criteria` section is present in `spec.md` (9 criteria) and mirrored in `issue.md`. `user-story.md` is intentionally absent (refactor child of epic #295); this is the correct, documented configuration for this feature.

## Acceptance Criteria Inventory

Nine criteria (verbatim, from `spec.md` `### Acceptance Criteria`, mirrored in `issue.md`):

1. `ITagViewer` interface exists, derives from `IForm`, and exposes the members `TagController` requires; `TagViewer` implements it.
2. `TagController` depends on `ITagViewer`, not the concrete `TagViewer`.
3. Host-neutral business logic is separated from COM/WinForms interaction.
4. No resulting production file exceeds 500 lines.
5. Unit tests cover the named methods and related logic without constructing real WinForms objects; seams are introduced where required.
6. `TagController` (and extracted logic) reaches `>= 80%` line coverage.
7. The `Tags` project as a whole reaches `>= 80%` line coverage (epic #295 goal; includes `TagLauncher` and `CheckBoxController` coverage as needed).
8. No unit test constructs a live form/window or triggers a popup requiring human interaction.
9. Full C# toolchain (csharpier -> analyzers -> nullable -> MSTest) passes with no regression.

## Acceptance Criteria Evaluation

| # | Verdict | Evidence and reasoning |
|---|---|---|
| 1 | PASS | `Tags/ITagViewer.cs` L17 declares `public interface ITagViewer : IForm` and exposes the command intent events, state properties, and option-panel methods `TagController` consumes; `SetController(TagController)` retained. `Tags/TagViewer.cs` L19 declares `public partial class TagViewer : Form, ITagViewer`. |
| 2 | PASS | `Tags/TagController.cs` constructor signatures take `ITagViewer viewerInstance` and the field `_viewer` is `ITagViewer`; no concrete `TagViewer` dependency in the controller. Analyzer/nullable builds pass (steps 2-3). |
| 3 | PASS | `Tags/TagSelectionModel.cs` contains the pure selection/search/filter/prefix logic with zero `System.Windows.Forms` references (only compile-time `OlCategoryColor` and Moq-friendly interfaces). Dialogs isolated behind `IUserPrompt`; focus draw behind `Action<CheckBox> _drawFocus`. |
| 4 | PASS | AC is production-scoped. All production files are `<= 435` lines (independently counted: TagController.cs 435, TagController.Rendering.cs 327, TagViewer.cs 167, TagSelectionModel.cs 224, CheckBoxController.cs 257, LauncherAutoAssign.cs 112, TagLauncher.cs 169, ITagViewer.cs 59, IUserPrompt.cs 21, WinFormsUserPrompt.cs 25). Note: this AC concerns production files only and is satisfied. A separate repo-policy file-size violation exists for a **test** file (`TagControllerSeamTests.cs` 579), recorded as Blocking in the policy audit and code review; it does not change this production-scoped AC verdict. |
| 5 | PASS | Named methods (`TryGetAutoAssignment`, `AddColorCategory`, `GetUserInputCategory`, `OptionsPanel_PreviewKeyDown`/`_KeyDown`, keyboard handlers, `LauncherAutoAssign`) covered via mocked `ITagViewer`/`IUserPrompt`/`IAutoAssign` and injected no-op `_drawFocus`. Determinism scan confirms no live WinForms object is constructed outside the two sanctioned STA files. 64 tests. |
| 6 | PASS | `coverage-delta.md`: `TagController` 95.10% and `TagController.Rendering` 89.71% line coverage, both `>= 80%`. New modules `TagSelectionModel` 97.50% and `LauncherAutoAssign` 93.33% (`>= 90%`). |
| 7 | PASS | `final-coverage.md` / `coverage-delta.md`: Tags.dll line coverage 92.63% (704/760), `>= 80%` floor met; baseline 67.28% -> +25.35pp, no regression on changed lines. Verified for internal consistency (704/760 = 92.63%). Canonical `artifacts/csharp/coverage.xml` is gitignored working-tree output; committed evidence markdown is authoritative; independent regeneration not performed (no build toolchain on PATH). |
| 8 | PASS | `determinism-scan.md` and independent grep: no `new TagViewer(`, `.Show()`/`.ShowDialog()`, `MessageBox`/`InputBox`, `[STAThread]`, `DoEvents`, `Thread.Sleep`/`Task.Delay`, or temp-file API in `Tags.Test`. Unshown `CheckBox` controls appear only in the two sanctioned `*.StaTests.cs` files, never a `Form`, never shown — permitted by the maintainer-ratified STA refinement. |
| 9 | PASS (evidence-based) | `final-qa-summary.md`: all four toolchain steps EXIT_CODE 0 in one clean final pass (csharpier 1331 files clean; analyzers 0 errors/0 Tags warnings; nullable 0 warnings/0 errors; 64/64 tests). Independent re-run not performed (no msbuild/vstest on PATH); executor evidence relied upon per the evidence-verification model. |

## Summary

All nine acceptance criteria evaluate to **PASS**. The refactor delivers the `ITagViewer` seam, controller-on-interface dependency, host-neutral logic extraction, production file-size compliance, deterministic seam-based tests, and coverage above the 80%/90% thresholds, with the maintainer-ratified STA refinement honored exactly and the Coverage Exemption Register applied with no exemption on any testable seam.

The feature deliverables are complete. However, the branch is **not ready to merge** due to one repository-policy finding that is orthogonal to the acceptance criteria: the new test file `Tags.Test/TagControllerSeamTests.cs` (579 lines) exceeds the 500-line file-size limit, which applies to test code. This is captured as a Blocking finding in `policy-audit.2026-07-09T22-52.md` and `code-review.2026-07-09T22-52.md`, with remediation guidance in `remediation-inputs.2026-07-09T22-52.md`. AC4 itself remains PASS because it is scoped to production files.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/spec.md` and `.../issue.md`
- Total AC items: 9 (identical set in both files)
- Checked off (delivered): 9
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

All nine criteria in both `spec.md` `### Acceptance Criteria` and `issue.md` `## Acceptance Criteria` were already marked `[x]` by the executor and are confirmed PASS in this audit; no check-off change was required. The `## Definition of Done`, `## Seeded Test Conditions`, and issue `## Test Conditions to Consider` checklists were likewise already complete and are consistent with the delivered work. No criterion text was modified.
