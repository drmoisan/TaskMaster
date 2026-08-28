# Phase 2 — deletion of the three orphaned `EfcViewer3.*` files

Timestamp: 2026-08-28T00-19
Task: [P2-T4]
Command: `git rm QuickFiler/Viewers/EfcViewer3.cs QuickFiler/Viewers/EfcViewer3.Designer.cs QuickFiler/Viewers/EfcViewer3.resx`; then `ls` on the three paths, `git diff --name-only 002335989830ba9f3ad802858ef0b794f6281750 -- QuickFiler/QuickFiler.csproj`, and a repository-wide search for the token `EfcViewer3`
EXIT_CODE: 0

## Deletion

All three paths are absent from the working tree. `ls` reports `No such file or directory` for each:

- `QuickFiler/Viewers/EfcViewer3.cs`
- `QuickFiler/Viewers/EfcViewer3.Designer.cs`
- `QuickFiler/Viewers/EfcViewer3.resx`

Their presence and sizes at `BASELINE_SHA` (2474, 32101 and 5817 bytes) are recorded in `[P0-T16]`.

## `QuickFiler/QuickFiler.csproj` is untouched

```
git diff --name-only 002335989830ba9f3ad802858ef0b794f6281750 -- QuickFiler/QuickFiler.csproj
```

produces **no output lines**.

No project edit was required because the three files carried no `Compile Include`, no
`EmbeddedResource`, and no `DependentUpon` entry: `[P0-T16]` records
`grep -c 'EfcViewer3' QuickFiler/QuickFiler.csproj` returning **0** at baseline. This is what removes all
contention with feature #501, which adds its own line to that project file.

## No residual references

A repository-wide search over `*.cs`, `*.csproj` and `*.resx` for the token `EfcViewer3` now returns
**zero matching files**. Nothing referenced the deleted type, which is consistent with its having no
project entry and therefore never having been compiled.

The `[ExcludeFromCodeCoverage]` attribute that `EfcViewer3.cs:17` carried on
`public partial class EfcViewer3 : Form` is removed with the file. That is a *removal* of an exemption,
not an addition, so constraint C5 — no new coverage exemption anywhere in the diff — is unaffected.
`[P10-T11]` audits only the four surviving files, whose exemption counts are unchanged.

Output Summary: The three orphaned `EfcViewer3.*` files are deleted from the working tree with `git rm`.
`QuickFiler/QuickFiler.csproj` shows no diff against BASELINE_SHA, and a repository-wide search for the
token `EfcViewer3` returns zero files.
