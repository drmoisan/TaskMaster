# Final QC — AC5 Signature Compatibility Review

- Timestamp: 2026-07-19T12-45
- Task: [P7-T9]
- Source: `git diff <base>..HEAD -- '*.cs'` (64 insertions, 36 deletions across 14 files; net additive)

Every source change is one of: a `#nullable enable` pragma (non-executable), a `?` nullability
annotation on a type/parameter/return, or a runtime-neutral `!` null-forgiving operator on an
existing expression. No statement, branch, guard, or logic was added or removed. Per-file
public-signature review:

| File | Public-signature change | Assessment |
|---|---|---|
| DelegateButtonTemplate.cs | none (pragma only) | compatible |
| FolderNotFoundViewer.cs | `FolderAction` → `string?` | additive nullability; property already assigned only in `*_Click` handlers |
| MyBoxViewer.cs | none public; `_map` (private) → nullable, `!` derefs | compatible; internal-only |
| InputBoxViewer.cs | none (pragma only) | compatible |
| ActionButton.cs | none — `Name`/`Button`/`Delegate` kept non-null (runtime-neutral `!` getters); fields private | compatible |
| DelegateButton.cs | none — same as ActionButton | compatible |
| FunctionButton.cs | `Value` → `T?`; `Name`/`Button`/`Delegate`/`ButtonClicked`/`ButtonClickedAsync` kept non-null | additive nullability on `Value` only |
| InputBox.cs | `ShowDialog(...)` → `string?` | additive; matches documented "or null if cancelled" |
| NotImplementedDialog.cs | none (pragma only) | compatible |
| MyBox.cs | both `ShowDialog<T>(...)` → `T?`; `FunctionButtonGroup<T>.Result` → `T?` | additive unconstrained-generic nullability, consistent with `WinFormsExtensions.Clone<T>()` |
| MyBoxModeless.cs | 5-arg `showAction` → `Action<MyBoxViewer>?` | additive; matches documented null-defaulting behavior |
| YesNoToAll.cs | none (pragma only) | compatible |
| AssemblyInfo.cs | none (pragma only) | compatible |
| ExtraDeclarations.cs | none (pragma only) | compatible |

## Result

All public-signature changes are limited to additive nullability annotations that reflect the actual
runtime null behavior, consistent with the consumed `WinFormsExtensions.Clone<T>()` contract from
issue #363 (`Clone<T>() where T : Control` returns non-nullable `T`; the button wrappers' `.Button`
properties were kept non-null to match, avoiding a false nullable propagation into `MyBox`). An
existing caller that compiles today continues to compile and behaves identically. AC5 is satisfied.
