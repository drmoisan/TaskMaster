# [P1-T6] Analyzer gate

Timestamp: 2026-08-26T08-45

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild`

Emitted MSBuild command line (host paths replaced with `<WS>`):

```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" <WS>\TaskMaster.sln /t:Rebuild /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /m
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

**Exit code 0, 0 errors, 5 warnings, 18 projects compiled, 0 `CoreCompile` skips, 0 analyzer
diagnostics. Byte-identical outcome to the P0-T12 baseline. No new diagnostic, and specifically no
"assigned but never used" diagnostic for `_itemTlpToMove`.**

### Result counts, against the P0-T12 baseline

| Metric | P0-T12 baseline | P1-T6 | New? |
|---|---|---|---|
| Exit code | 0 | **0** | — |
| Errors | 0 | **0** | none |
| Warnings | 5 | **5** | none |
| Analyzer diagnostics (any of the five wired analyzers) | 0 | **0** | none |
| Distinct projects that executed `CoreCompile` | 18 | **18** | — |
| `Skipping target "CoreCompile"` occurrences | 0 | **0** | — |
| Wall time | 00:00:19.26 | 00:00:17.60 | — |

### Non-vacuity proof

`grep -c 'Skipping target "CoreCompile"'` over the build log returns **0**, and 18 distinct
`/out:` compile targets appear. `/t:Rebuild` was used, never `/t:Build`: a warm `/t:Build` returns
exit 0 having skipped `CoreCompile` on every project and run no analyzers, which would make this
gate incapable of failing.

### The five warnings are the identical baseline set

Distinct warning text — exactly one, unchanged from P0-T12:

```
warning : The project contains a packages.config file, which is not supported by System.Reactive
v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the
RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.)
```

Emitting projects — the identical five: `QuickFiler.csproj`, `TaskMaster.csproj`,
`ToDoModel.csproj`, `UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj`.

A scan for any compiler or analyzer diagnostic code — `grep -oE 'warning (CS|CA|MA|RCS|S|IDE|AsyncFixer|RS)[0-9]+'`
— returns **no match**. The five warnings are MSBuild target warnings from a `.targets` file, not
diagnostics attributable to source. **No new diagnostic relative to the P0-T12 baseline.**

### Specific check: `_itemTlpToMove`

The plan calls this out because removing `CacheTlpForMove` (which wrote `_itemTlpToMove` at
`<MERGE_BASE>:867`) could leave the field written but never read, tripping an "assigned but never
used" diagnostic. Measured:

`grep -ci "_itemTlpToMove"` over the P1-T6 build log returns **0** — the identifier appears in no
diagnostic at all.

The field's post-removal site inventory in `QfcCollectionController.cs` is:

```
 69:        private TableLayoutPanel _itemTlpToMove;                        declaration
693:            _itemTlpToMove = _formViewer.L1v0L2L3v_TableLayout;         sole writer
813:            if (_itemTlpToMove is not null)                             reader
814:                _itemTlpToMove.Dispose();                               reader
```

The sole remaining writer at `:693` is inside the **live** member `CacheMoveObjects`:

```csharp
        public void CacheMoveObjects()
        {
            _itemTlpToMove = _formViewer.L1v0L2L3v_TableLayout;
            CacheItemGroupsForMove();
        }
```

This is the line the plan cites as `:900` at the pre-removal baseline; it is `:693` after the
241-line renumbering. The field is both written and read, so no such diagnostic can arise. This
confirms `research/qfc-collection-controller-defects.md` §7.1's prediction and closes the
"#468 <-> `_itemTlpToMove`" overlap row in §8.1.

### Acceptance verification

- `EXIT_CODE: 0`.
- Non-zero `CoreCompile` project count: **18**.
- No new diagnostic relative to the P0-T12 baseline — identical error count (0), identical warning
  count (5), identical warning text, identical emitting projects.
- No "assigned but never used" diagnostic for `_itemTlpToMove`.

Result: PASS. Toolchain step 2 (Linting) is green; the loop proceeds to step 3 (P1-T7, nullable).
