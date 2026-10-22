# Unowned-File Byte Identity (P4-T1)

Timestamp: 2026-08-27T11-26
Task: [P4-T1]
Command: `sha256sum QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs UtilitiesCS/Threading/UiThread.cs` (run from `<repo-root>`)
EXIT_CODE: 0
Output Summary: Both recomputed SHA-256 values equal the values `P0-T11` recorded for the same paths.
Both files are byte-identical to their Phase 0 state, so neither was modified by any task in this
plan — including the repository-wide `csharpier check` in `P3-T2`, which is read-only, and the
file-scoped `csharpier format` in `P3-T1`, whose argument list names neither path.

## Cited baseline artifact

Resolved per § Conventions from the stem `file-inventory-baseline`:
`<FEATURE>/evidence/baseline/file-inventory-baseline.2026-08-27T10-18.md`

## Hash comparison

| Path | Recorded by `P0-T11` | Recomputed now | Equal |
| --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | `a3c35259f1c5e5d2ed8d8a3e5ba923a964e2b164abe9d9ac7b6b32ec30644e4b` | `a3c35259f1c5e5d2ed8d8a3e5ba923a964e2b164abe9d9ac7b6b32ec30644e4b` | **yes** |
| `UtilitiesCS/Threading/UiThread.cs` | `87b4fde609398c59346557fb688ba192639ebc888104d74fea35d24dd18bdeaa` | `87b4fde609398c59346557fb688ba192639ebc888104d74fea35d24dd18bdeaa` | **yes** |

Both the recorded and the recomputed value are quoted for each path, as the acceptance condition
requires.

## Supplementary line counts

| Path | Lines | Figure the spec states |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 497 | 497 (AC-6) |
| `UtilitiesCS/Threading/UiThread.cs` | 163 | 163 (spec § Proposed Fix item 5) |

Both match, which independently corroborates the hash comparison.

`UtilitiesCS/Threading/UiThread.cs` being unchanged means the conditional permission in
`issue.md` § Constraints to edit that file was not exercised, per § Decisions Record D3.
