# Batch F3b Nullable Gate — FolderTree.cs (P4-T6)

Timestamp: 2026-07-19T12-40

- csharpier format: EXIT 0. Full solution /t:Build: EXIT 0.
- Scoped gate `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false`: **zero CS86xx** for FolderTree.cs (AC1).

## Key annotation decisions (FolderTree.cs)
- `_roots` initialized `= new()` at the field (the parameterless ctor used by CreateAsync/FromRoots leaves it
  otherwise unset; the setting ctors overwrite it, so this is behavior-safe).
- `public event PropertyChangedEventHandler? PropertyChanged;` and `Child_PropertyChanged(object? sender, ...)`.
- COM tree-building derefs `node.Value.OlFolder!.Folders` forgiven (`OlFolder` is now nullable from F3a).
- `selections.Contains(node.RelativePath!)` (RelativePath is `string?`; Contains(null) is harmless, `!` suppresses
  the CS8604 without behavior change).
- ProgressTracker consumed at call sites only (external oblivious, not edited). No post-condition attributes;
  no record/init; file not split.
