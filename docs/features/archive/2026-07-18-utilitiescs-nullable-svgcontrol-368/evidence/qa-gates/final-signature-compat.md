# Final QC — AC5 Signature Compatibility Review

Timestamp: 2026-07-19T05-00

Command: `git diff --stat SVGControl/` followed by `git diff SVGControl/<file>.cs` per file
(reviewed in full for all 12 remediated files).

`git diff --stat` summary: 12 files changed, 89 insertions(+), 49 deletions(-). No file outside
`SVGControl/`'s 12 remediation targets shows a diff (confirmed via `git status --short SVGControl/`
listing exactly these 12 paths; `PathInternal.cs`, `RelativePath.cs`, `ValueStringBuilder.cs` are
absent from the modified list, confirming verify-only status held).

## Per-file confirmation

| File | Public signature changes | Nature |
|---|---|---|
| `ButtonSVG.cs` | `ObjectToByteArray(Object obj)` -> `(object? obj)`; private `GetStringForValue(object value)` -> `(object? value)` | Additive nullable parameter annotation only; existing guards unchanged |
| `PictureBoxSVG.cs` | None beyond pragma | No signature change |
| `ToggleSwitch.cs` | None beyond pragma | No signature change |
| `SVGParser.cs` | None beyond pragma | No signature change |
| `SvgRenderer.cs` | `Render()` -> `Bitmap?`; `Document` property -> `SvgDocument?`; `GetSvgDocument(byte[])` -> `SvgDocument?` return; `PublicKeyTokensEqual(byte[] a, byte[] b)` -> `(byte[]? a, byte[]? b)`; `ResolveByNameAndKey` -> `Assembly?` return; `PropertyChanged` event -> nullable | All additive nullable annotations reflecting actual null-return/null-state behavior; existing guards (`a == null \|\| b == null`) unchanged; justified `!` used only at internal call sites, never on public signatures |
| `SvgImageSelector.cs` | `AboluteImagePath` -> `string?`; `ResourceName` -> `ISvgResource?`; `Render()` -> `Bitmap?`; `PropertyChanged` event -> nullable | Additive nullable annotations reflecting actual behavior (Batch C, the central judgment-call file); `ImagePath` getter return type unchanged (`string`, resolved via justified `!`, not a signature change) |
| `ISvgResource.cs` | Interface `Name`/`Data` -> `string?`/`byte[]?`; `SvgResource.Name`/`.Data` -> matching nullable | Additive; required to avoid a CS8766 interface-implementation nullability mismatch after the class members needed to be nullable (parameterless constructor never assigns them); `SvgResource` remains a plain class |
| `SvgResourceConverter.cs` | None (return type is `object`, unchanged; internal expression uses `!`) | No signature change |
| `DropDownEditor.cs` | Private field `_editorService` -> `IWindowsFormsEditorService?` | Private-member-only annotation; public `EditValue`/`GetEditStyle` signatures unchanged |
| `SvgOptionsConverter.cs` | None beyond pragma (local variable retyped, not a signature) | No signature change |
| `SvgOptionsConverter2.cs` | None beyond pragma (local variables retyped, not signatures) | No signature change |
| `SVGFileNameEditor.cs` | Private fields `_appPath`/`_ofd` only | Private-member-only annotation; public `EditValue`/`InitializeDialog` signatures unchanged |

## Conclusion

Every public-signature change across the 12 remediated files is limited to additive nullability
annotations (`?` on parameters, return types, properties, fields, or events) that reflect actual,
pre-existing null behavior of the underlying member. No parameter was added or removed, no method
was renamed, no access modifier changed, and no overload was introduced or removed. This satisfies
AC5.
