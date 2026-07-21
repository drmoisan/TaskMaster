# Batch F3d Nullable Gate — FolderPredictor partial pair (P4-T12)

Timestamp: 2026-07-19T13-20

- csharpier format EXIT 0; full /t:Build EXIT 0.
- Scoped gate (UtilitiesCS Rebuild, TreatWarningsAsErrors, BuildProjectReferences=false): **zero CS86xx** for
  both FolderPredictor.cs (974 lines, not split) and FolderPredictor.IFolderSearchHandler.cs (AC1, AC7 — both
  partial-class parts remediated together in the single task P4-T11).

## Key annotation decisions
- Nullable fields `Regex? _regex`, `List<string>? _folderList` (both genuinely null-until-set, null-checked).
- The navigation-only ctor `FolderPredictor(Outlook.Application olApp)` does not populate `_globals`/`Suggestions`;
  `_globals = null!` (partial-init idiom) and `_suggestions = null!` field initializer document this while keeping
  both non-null-typed so `IFolderSearchHandler.Suggestions` (non-null, decided in P1-T2) is satisfied and the many
  globals-dependent members stay clean. This preserves the pre-existing contract (globals-dependent members require
  a globals-providing ctor).
- Nullable-returning: all four `GetFolder` overloads (`Folder?`), `InputFoldername`/`InputFoldernameAsync`
  (`string?`/`Task<string?>`), `CreateFolder` (`MAPIFolder?`), `CreateFolderAsync` (`Task<object?>`).
- Nullable optional params: `string? defaultValue = null`, `List<string>? emailSearchRoots = null`,
  `IEnumerable<...>? exclusions = null` (FindFolder matches the P1-decided IFolderSearchHandler.FindFolder shape).
- COM/root-lookup derefs forgiven with `!` (GetFolder(root)!.Folders, _regex!.IsMatch, _folderList!.Add) — no new
  runtime guards. No post-condition attributes; no record/init.
