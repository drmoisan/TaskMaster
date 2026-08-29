# [P1-T4] Pre-change tree facts (Issue 638)

Timestamp: 2026-08-29T12-25

Command: `(Get-Content -LiteralPath 'QuickFiler/Controllers/EfcDataModel.cs').Count`

`Measure-Object -Line` was deliberately not used: it reports a different figure for a file
with a trailing newline.

EXIT_CODE: 0

Output Summary:

PRECHANGE_EFCDATAMODEL_LINE_COUNT: 423

HEADROOM_TO_CAP: 77

The four re-derived facts from [P1-T1] through [P1-T4], each against the working tree of
this worktree at branch head `f07b6299`:

## [P1-T1] The three unguarded archive-root read sites

`Select-String -Path 'QuickFiler/Controllers/EfcDataModel.cs' -SimpleMatch 'OlAncestor = Globals.Ol.ArchiveRootPath'`
returned exactly three matches:

```
LINE 289: OlAncestor = Globals.Ol.ArchiveRootPath,
LINE 310: OlAncestor = Globals.Ol.ArchiveRootPath,
LINE 328: OlAncestor = Globals.Ol.ArchiveRootPath,
```

Line numbers 289, 310 and 328 match the plan's Verified Facts exactly. They sit inside the
`EmailFilerConfig` object initializers of `MoveToFolderAsync(string, bool, bool, bool, bool)`,
`OpenOlFolderAsync(string)` and `OpenFsFolderAsync(string)` respectively.

## [P1-T2] The ordering sentinel

`Select-String -Path 'QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs' -SimpleMatch 'SpecialFoldersAccessCount.Should().Be(2)'`
returned exactly one match:

```
LINE 217: probe.FileSystem.SpecialFoldersAccessCount.Should().Be(2);
```

Line number 217 matches the plan exactly. This test pins the archive-root guard's placement
strictly **after** the OneDrive `SpecialFolders` read in all three methods.

## [P1-T3] The new test file is not yet registered

`Select-String -Path 'QuickFiler.Test/QuickFiler.Test.csproj' -SimpleMatch 'EfcDataModelArchiveRootTests.cs'`
returned **0** matches.

`Select-String -Path 'QuickFiler.Test/QuickFiler.Test.csproj' -SimpleMatch 'Controllers\EfcDataModelTests.cs'`
returned exactly one match:

```
LINE 115: <Compile Include="Controllers\EfcDataModelTests.cs" />
```

Line number 115 matches the plan exactly and is the insertion anchor for [P3-T2].

## [P1-T4] Pre-change file size

`QuickFiler/Controllers/EfcDataModel.cs` is 423 lines, matching the plan's Verified Facts.
The 500-line cap in `.claude/rules/general-code-change.md` therefore leaves 77 lines of
headroom for the Phase 2 seam and the Phase 4 guard.
