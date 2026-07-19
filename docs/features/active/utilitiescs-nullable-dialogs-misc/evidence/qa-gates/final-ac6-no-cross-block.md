# Final QC — AC6 No Cross-Block Verification

- Timestamp: 2026-07-19T12-45
- Task: [P7-T10]
- Command: `git diff --name-only <base>..HEAD` (base = dffadd5a)

## Changed source files (outside the feature docs directory)

Exactly the 14 in-scope cluster files, and nothing else:

```
UtilitiesCS/Dialogs/ActionButton.cs
UtilitiesCS/Dialogs/DelegateButton.cs
UtilitiesCS/Dialogs/DelegateButtonTemplate.cs
UtilitiesCS/Dialogs/FolderNotFoundViewer.cs
UtilitiesCS/Dialogs/FunctionButton.cs
UtilitiesCS/Dialogs/InputBox.cs
UtilitiesCS/Dialogs/InputBoxViewer.cs
UtilitiesCS/Dialogs/MyBox.cs
UtilitiesCS/Dialogs/MyBoxModeless.cs
UtilitiesCS/Dialogs/MyBoxViewer.cs
UtilitiesCS/Dialogs/NotImplementedDialog.cs
UtilitiesCS/Dialogs/YesNoToAll.cs
UtilitiesCS/Properties/AssemblyInfo.cs
UtilitiesCS/WindowsAPI/ExtraDeclarations.cs
```

All other changes are confined to
`docs/features/active/utilitiescs-nullable-dialogs-misc/` (plan, spec/user-story AC check-offs, and
evidence artifacts).

## Result

No file outside the 14-file cluster received a `#nullable enable` pragma or any nullable-related
edit. The 4 Designer siblings and every other file in the repository remain non-opted-in and
null-oblivious; because `#nullable enable` is lexical/per-file, they are not cross-blocked by this
change. The feature is independently mergeable under the per-file pragma architecture. AC6 is
satisfied.
