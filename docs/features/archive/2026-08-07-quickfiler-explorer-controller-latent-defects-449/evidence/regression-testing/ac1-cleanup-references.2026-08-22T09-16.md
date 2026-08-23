# AC-1 — Residual `ExplConvView_Cleanup` References (Issue #449, [P3-T6])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command:
```
git grep -n -F "ExplConvView_Cleanup" -- "*.cs"
```
EXIT_CODE: 0

Full output (verbatim, complete — nothing elided):
```
QuickFiler/Legacy/QuickFileController.cs:673:                ExplConvView_Cleanup();
QuickFiler/Legacy/QuickFileController.cs:851:        public void ExplConvView_Cleanup()
QuickFiler/Notes/notes_interfaces.cs:58:        void ExplConvView_Cleanup();
```

## Result — three remaining hits, all in UNCOMPILED files

| # | File | Line | Kind | Compiled? |
| --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Legacy/QuickFileController.cs` | 673 | call site | **NO** |
| 2 | `QuickFiler/Legacy/QuickFileController.cs` | 851 | declaration | **NO** |
| 3 | `QuickFiler/Notes/notes_interfaces.cs` | 58 | duplicate interface declaration | **NO** |

**No hit resolves to a compiled file.** Specifically, and as AC-1 requires:

- **Zero** hits in `QuickFiler/Interfaces/IQfcExplorerController.cs` — the declaration formerly at
  line 12 was removed by [P3-T1].
- **Zero** hits in `QuickFiler/Controllers/QfcExplorerController.cs` — the `//PRIORITY:` comment and
  the four-line throwing implementation formerly at lines 60-64 were removed by [P3-T2].

Before this phase the same search returned **six** hits (five declarations or calls plus the
`//PRIORITY:` comment). Three were removed by Phases 3's two edits; the three above remain and are
deliberately retained.

## Proof that the three surviving hits are not compiled

Command: `grep -c 'Compile Include="Legacy' QuickFiler/QuickFiler.csproj`
EXIT_CODE: 1
Output: `0`

Command: `grep -c 'Compile Include="Notes' QuickFiler/QuickFiler.csproj`
EXIT_CODE: 1
Output: `0`

`QuickFiler/QuickFiler.csproj` contains **zero** `Compile Include` entries for either the `Legacy\` or
the `Notes\` directory. These are legacy non-SDK `packages.config` projects, which enumerate every
compiled source file explicitly rather than globbing, so a file with no `Compile Include` entry is not
passed to the compiler at all. Neither file participates in any build, so neither hit is a compile-time
reference and neither can break.

`QuickFiler/Notes/notes_interfaces.cs:52-59` declares a DUPLICATE `IQfcExplorerController` that still
carries the removed member. It is left **intentionally inconsistent** with the compiled contract: the
file is explicitly out of this issue's file set, is confirmed byte-identical to the merge base by
[P3-T3], and is not an edit target.

## No test-project reference

Command: `git grep -n --untracked -F "ExplConvView_Cleanup" -- QuickFiler.Test`
EXIT_CODE: 1
Output: (no match)

No file under `QuickFiler.Test` references the member — including the newly added, still-untracked
`QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs`, which `--untracked` brings into scope.
There is therefore no mock setup, no `Verify`, and no reflection assertion anywhere in the test suite
that the removal could have broken. This is the same fact the [P3-T7] dossier records as its
mock-setup proof.

## Output Summary

`git grep -n -F "ExplConvView_Cleanup" -- "*.cs"` returns **three** hits, EXIT_CODE 0, and every one
lies in an uncompiled file: `QuickFiler/Legacy/QuickFileController.cs:673` and `:851`, and
`QuickFiler/Notes/notes_interfaces.cs:58`. `QuickFiler/QuickFiler.csproj` carries zero
`Compile Include` entries for `Legacy\` or `Notes\`, which is the proof of non-compilation. **No hit
is in `QuickFiler/Interfaces/IQfcExplorerController.cs` or
`QuickFiler/Controllers/QfcExplorerController.cs`**, and no file under `QuickFiler.Test` references the
member. AC-1's search condition is satisfied.
