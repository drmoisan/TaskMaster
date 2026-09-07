# QA Gate — Build After the SR-1 Partial Split (P2-T3)

Timestamp: 2026-08-27T20-21

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

Output Summary:

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:19.43
```

- Error count: **0**
- Warning count: 5, all the same pre-existing `System.Reactive` `packages.config` advisory recorded at
  baseline. No new warning was introduced by the split.
- Count of lines matching `Skipping target "CoreCompile"`: **0** — the gate is non-vacuous.

## First attempt failed; recorded rather than omitted

The first run of this command returned `EXIT_CODE: 1` with 2 errors, both
`error CS0246: The type or namespace name 'FolderRow' could not be found`, at
`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs(22,50)` and `(43,27)`.

Cause: `FolderRow` is declared in namespace `UtilitiesCS`
(`UtilitiesCS/OutlookObjects/Folder/FolderRow.cs:31`, namespace declared at `:2`), not in the
path-implied namespace `UtilitiesCS.OutlookObjects.Folder`. The new partial part's initial using list
carried only the latter. `BreadcrumbSelectorState` genuinely is in
`UtilitiesCS.OutlookObjects.Folder` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs:21`,
namespace at `:5`), so both usings are required.

Remedy: added `using UtilitiesCS;` to
`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs`. This is within P2-T1's own instruction
to include "the usings the moved members require" and changes no moved member body. The command was
then re-run and is the run recorded above.

## What this proves

The four members — `SetSuggestions`, the `SuggestionsUpgrade` property, `PopulateSuggestionsAsync` and
`AddItems` — now compile from the new partial part, and the primary file no longer declares them, so
the type does not declare any member twice. The single `<Compile Include>` line added by P2-T2 is
sufficient to bring the new file into the compilation.

Acceptance: `EXIT_CODE: 0` and an error count of 0. PASS.
