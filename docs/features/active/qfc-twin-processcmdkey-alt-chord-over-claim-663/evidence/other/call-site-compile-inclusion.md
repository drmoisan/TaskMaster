# Call-site compile-inclusion determination (issue #663)

Timestamp: 2026-09-01T00-25
Tree: `origin/main` @ `2b85134b42872e405602e6064e02dc9cda6c319b`
Purpose: establish, before any acceptance criterion is written, which `ProcessCmdKey` Alt-claiming sites are actually build inputs. The textual call-site count and the compiled call-site count differ, and the difference determines the scope decision.

## Why this matters

`QuickFiler/QuickFiler.csproj` is a legacy non-SDK project. Verified: it declares no wildcard `<Compile Include="*..." />` glob, no `EnableDefaultCompileItems`, and no `Microsoft.NET.Sdk` attribute.

Command: `git grep -n "Compile Include=\"\*\|EnableDefaultCompileItems\|Microsoft.NET.Sdk" -- QuickFiler/QuickFiler.csproj`
EXIT_CODE: 1
Output Summary: no matches. Every compiled file is therefore listed explicitly, and a source file absent from the item list is not compiled.

## Textual references to the Alt-claim predicate

Command: `git grep -n "IsAltKeyCommand" origin/main -- '*.cs'`
EXIT_CODE: 0
Output Summary: 4 production references (1 definition + 3 call sites) and 5 test references.

- Definition: `QuickFiler/Controllers/QfcFormKeyHandler.cs:18`
- Call site: `QuickFiler/Viewers/QfcFormViewer.cs:60`
- Call site: `QuickFiler/Viewers/QfcFormViewerDark.cs:43`
- Call site: `QuickFiler/Viewers/QfcFormViewerExpanded.cs:43`

A fourth QFC-surface site does not call the predicate and inlines the test instead:

Command: `git grep -n "HasFlag(Keys.Alt)" origin/main -- '*.cs'`
EXIT_CODE: 0
Output Summary: 4 hits — `QfcFormKeyHandler.cs:18`, `QuickFiler/Legacy/QfcFormLegacyViewer.cs:23`, `QuickFiler/Viewers/EfcViewer.cs:98`, `TaskVisualization/TaskViewer.cs:255`.

## Compile inclusion, per file

Command: `git grep -n "Compile Include=\"Viewers" -- QuickFiler/QuickFiler.csproj`
EXIT_CODE: 0
Output Summary: 44 `Viewers\` compile items. `Viewers\QfcFormViewer.cs` is present at line 452 (with `.Designer.cs` at 455 and the `.resx` at 510). `Viewers\QfcFormViewerDark.cs` and `Viewers\QfcFormViewerExpanded.cs` are absent from the list.

Command: `git grep -n "QfcFormViewer" -- QuickFiler/QuickFiler.csproj`
EXIT_CODE: 0
Output Summary: 6 hits, all for `Interfaces\IQfcFormViewer.cs` and the `QfcFormViewer` triplet. No `QfcFormViewerDark` or `QfcFormViewerExpanded` item. This is the second, independent formulation and it agrees with the first.

Command: `git grep -c "Legacy" -- QuickFiler/QuickFiler.csproj`
EXIT_CODE: 1
Output Summary: zero matches. The entire `QuickFiler/Legacy/` folder is excluded from the build, not merely the one viewer file.

## Determination

| Site | File | In csproj | Compiled | Uses predicate | Handler invoked |
|---|---|---|---|---|---|
| 1 | `QuickFiler/Viewers/QfcFormViewer.cs:56-73` | yes (line 452) | YES | `IsAltKeyCommand` | `ToggleKeyboardDialogAsync()` (parameterless) |
| 2 | `QuickFiler/Viewers/QfcFormViewerDark.cs:41-53` | no | NO | `IsAltKeyCommand` | `KeyboardHandler_KeyDown(sender, e)` |
| 3 | `QuickFiler/Viewers/QfcFormViewerExpanded.cs:41-53` | no | NO | `IsAltKeyCommand` | `KeyboardHandler_KeyDown(sender, e)` |
| 4 | `QuickFiler/Legacy/QfcFormLegacyViewer.cs:21-33` | no (whole folder) | NO | inlines `HasFlag` | `QuickFileController.KeyboardHandler_KeyDown(sender, e)` |
| 5 | `TaskVisualization/TaskViewer.cs:253-265` | yes (`TaskVisualization.csproj:110`) | YES | inlines `HasFlag` | `TaskController.KeyboardHandler_KeyDown(sender, e)` |

**`QfcFormKeyHandler.IsAltKeyCommand` has exactly ONE compiled consumer: `QuickFiler/Viewers/QfcFormViewer.cs:60`.**

Sites 2, 3 and 4 have no runtime behavior to correct because they are not build inputs. Sites 2 and 3 additionally dereference `_keyboardHandler` at line 48 with no null guard, and no call site anywhere in the solution binds either type's `SetKeyboardHandler`, so the field would be permanently null were they ever compiled. That latent null dereference is a pre-existing property of uncompiled files and is not created, worsened, or repaired by this fix.

## Handler-contract divergence

The five sites do not share one contract, so one narrowing rule does not fit all of them.

- Site 1 calls `IQfcKeyboardHandler.ToggleKeyboardDialogAsync()` — the parameterless overload at `QuickFiler/Controllers/KeyboardHandler.cs:225`. Its body reads only `_kbdActive` and calls `ToggleOffNavigationAsync()` / `ToggleOnNavigationAsync()`. It never inspects the key data. A claim over an Alt+key chord therefore discards information the handler cannot use, which is precisely the #467 argument.
- Sites 2, 3 and 4 call a `KeyboardHandler_KeyDown(object, KeyEventArgs)` overload. The `IQfcKeyboardHandler` implementation at `QuickFiler/Controllers/KeyboardHandler.cs:114-131` dispatches on `e.KeyCode` through `KeyActions` and on `(char)e.KeyValue` through `CharActions`, gated on `KbdActive`. It does inspect the key data, so the #467 argument does not transfer to those sites unchanged.
- Site 5 calls `TaskVisualization.TaskController.KeyboardHandler_KeyDown` at `TaskVisualization/TaskController.Accelerator.cs:75`, which returns `bool` and implements a distinct accelerator model exercised by `TaskVisualization.Test/TaskControllerAcceleratorKeyboard.StaTests.cs`.

## Coverage posture of the candidate predicate hosts

Command: `git grep -n "ExcludeFromCodeCoverage" -- QuickFiler/Viewers/EfcViewer.cs QuickFiler/Controllers/QfcFormKeyHandler.cs QuickFiler/Viewers/QfcFormViewer.cs TaskVisualization/TaskViewer.cs`
EXIT_CODE: 0
Output Summary: 3 hits — `EfcViewer.cs:20`, `QfcFormViewer.cs:17`, `TaskViewer.cs:18`. `QfcFormKeyHandler.cs` has no such attribute.

`QfcFormKeyHandler` is the only candidate host that is coverage-measured. A predicate placed on `QfcFormViewer`, mirroring the EFC placement on `EfcViewer`, would sit inside an `[ExcludeFromCodeCoverage]` type and could not demonstrate the `>= 90%` new-method coverage the unit-test policy requires.

## Menu presence on the TaskVisualization surface

Command: `git grep -c "MenuStrip\|ToolStripMenuItem" -- TaskVisualization/TaskViewer.Designer.cs`
EXIT_CODE: 1
Output Summary: zero matches. `TaskViewer` declares no menu strip and no menu items, so it has no Alt mnemonic for `ProcessCmdKey` to swallow. The user-facing symptom described in issue #663 does not arise on that surface, which is why site 5 is out of scope on evidence rather than on project boundary alone.

## Negative evidence for the potential entry

SearchScope: `origin/main:docs/features/potential/` recursive, 146 tracked files, including `promoted/`.
SearchPatterns: `qfc-twin`, `processcmdkey`, `alt-chord` (case-insensitive).
SearchResult: `docs/features/potential/promoted/2026-08-07-efc-viewer-processcmdkey-swallows-alt-mnemonics.md` only. The QFC-twin potential entry named in the issue body, `docs/features/potential/2026-08-27-qfc-twin-processcmdkey-alt-chord-over-claim.md`, is absent from both `potential/` and `potential/promoted/`.
