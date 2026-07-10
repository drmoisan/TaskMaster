# Code Review — Issue #293 (tagcontroller-testability-refactor)

- Feature folder: `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/`
- Base branch: `epic/winforms-testability-refactor-integration` @ merge-base `3f04d50f6544f084323e5d7a9a563facb9d579df`
- Head: `55a4835659f977a0dce9e1f5f872b121b659167d`
- Review timestamp: 2026-07-09T22-52
- Scope: full branch diff vs merge-base.

## Executive Summary

The refactor is well-structured and matches the repository's I/O-boundary and DI-seam guidance. The 877-line `TagController` is decomposed into a single public partial class (`TagController.cs` 435 + `TagController.Rendering.cs` 327) plus a host-neutral `TagSelectionModel` (224, zero WinForms references), an `ITagViewer : IForm` seam, an `IUserPrompt` dialog seam with a thin `[ExcludeFromCodeCoverage]` production adapter, and an extracted testable `LauncherAutoAssign`. Constructor injection preserves production call sites via implicit upcast and defaulted optional parameters. The maintainer-ratified STA refinement is honored exactly: STA control construction is confined to two dedicated `*.StaTests.cs` files that never show a window, use no message pump, and dispose every control.

One Blocking finding: the new test file `Tags.Test/TagControllerSeamTests.cs` is 579 lines, over the 500-line file-size limit that applies to test code. Two Low observations and one Info observation are recorded for author awareness. The pre-existing `RemoveControls`/`PrefixItem` defects called out in the spec Non-Goals are report-only and correctly out of scope.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocking | `Tags.Test/TagControllerSeamTests.cs` | whole file (579 lines) | New test file exceeds the 500-line file-size limit, which applies to test code. | Split into two or more cohesive `<= 500`-line test files (e.g. by method group: dialog-seam tests vs navigation/rendering-seam tests). | `.claude/rules/general-code-change.md` File Size Limit explicitly includes "test code"; CLAUDE.md §4.1 / §C#5.1. No listed exception applies (not a throwaway script, language fixture, or Markdown doc). | `awk 'END{print NR}'` = 579; `wc -l` = 579 |
| Low | `Tags/LauncherAutoAssign.cs` | `AutoFindAsync` (L81-91) | `try { return Task.Run(...); } catch (System.Exception) { throw; }` catches and rethrows with no added context and cannot catch faults from the returned task (they surface on await, not on `Task.Run` construction). | Remove the redundant try/catch; return `Task.Run(() => AutoFind(objItem))` directly. | General Code Change Policy §3 / C#4.1 discourage broad catches that add no context. Behavior is unchanged, so this is a clarity nit, not a defect. | file read L81-91 |
| Low | `Tags/TagController.Rendering.cs` | `RemoveControls` (L94-102) | `bool unused = _colColorbox.Remove(i);` passes the loop index `i` to a by-object `Remove`, an index/element confusion. | Leave as-is for #293 (spec Non-Goal marks it report-only, latent because `_colColorbox` is empty in current flows); track for a follow-up fix. | Spec `## Non-Goals` explicitly defers this pre-existing defect; the collection is empty in current flows so it is latent. Recorded here for traceability. | spec.md Non-Goals; file read L94-102 |
| Info | `Tags/WinFormsUserPrompt.cs` / `Tags/TagViewer.cs` / `Tags/TagLauncher.cs` | class-level `[ExcludeFromCodeCoverage]` | Three class-level exemptions on irreducible host-bound wiring. | No action. | Registers E1/E3/E5; each is live dialog UI, `Form`-derived, or live-form/globals wiring that no seam can cover. Ratified by the epic STA refinement; testable logic was extracted out from under these. | `grep ExcludeFromCodeCoverage Tags/` |

## Design and Best-Practice Assessment

- Separation of concerns: PASS. `TagSelectionModel` is pure host-neutral logic (only compile-time `OlCategoryColor` and Moq-friendly `IAutoAssign`/`IPrefix` interfaces; zero `System.Windows.Forms`). Controller orchestrates model + `ITagViewer` + `IUserPrompt`.
- Seam design: PASS. `ITagViewer : IForm` exposes intent-named events/properties/methods rather than raw controls; `IUserPrompt` wraps `MessageBox`/`InputBox`; `_drawFocus` isolates the only HWND-forcing draw. Constructor defaults (`prompt ?? new WinFormsUserPrompt()`, `drawFocus ?? DrawFocusDefault`) preserve production behavior.
- Contracts / fail-fast: PASS. `ResolvePrefix` throws `ArgumentException` on unknown key; `Select_Ctrl_By_Position` throws `ArgumentOutOfRangeException` out of bounds. Guard clauses precede use.
- Public-API preservation: PASS. `TagLauncher.Viewer` stays typed `TagViewer`; `SetController(TagController)` retained on the interface; `CheckBoxController._parent` stays concrete; call sites compile via implicit upcast.
- Naming/formatting: PASS. CSharpier-clean per executor evidence; PascalCase/camelCase observed; XML docs on new public types.
- Test quality: PASS. Deterministic; Moq `ITagViewer`/`IUserPrompt`/`IAutoAssign`; `FakeTagViewer` backs `OptionControls` with a tracked list; STA tests raise clicks via reflection `Control.InvokeOnClick` with no pump.
- STA-refinement conformance: PASS. Only `TagControllerRendering.StaTests.cs` and `CheckBoxControllerWiring.StaTests.cs` use `[STATestClass]`/`[STATestMethod]`; both construct only unshown `CheckBox` controls, never `Show()`/`ShowDialog()`, dispose every control.

## Overall Recommendation

One change required before merge: split `Tags.Test/TagControllerSeamTests.cs` to bring every test file under 500 lines. After that split (and a re-run of the toolchain to confirm the split file set still compiles and passes), no other changes are required. The two Low observations are optional maintainability follow-ups; the `RemoveControls`/`PrefixItem` defects are report-only per the spec Non-Goals.
