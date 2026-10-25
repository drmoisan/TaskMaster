# Coverage Comparison (P7-T5)

Timestamp: 2026-10-22T16-36
Command: pwsh -NoProfile -File SCRATCH\issue-1005-coverage-compare.ps1 -MergeBase <merge-base> (reads SCRATCH issue-1005-baseline-cobertura.xml and SCRATCH issue-1005-final-cobertura.xml, both post-processed Cobertura documents, and git diff -U0 <merge-base> per modified production file)
EXIT_CODE: 0
Output Summary: PASS, no shortfall. Repository line 86.30% -> 86.37% and branch 80.58% -> 80.66% (floors 80 / 75). ApplicationSettingsStore.cs 100% (7/7); AppAutoFileObjects.ScalarSettings.cs 100% (36/36). Every new or changed seam member in IDList, AppQuickFilerSettings, AppStagingFilenames, and the AppAutoFileObjects scalar partial is at 100% line coverage. Changed-line coverage is 100% in every modified production file, and no figure is below the baseline coverage of the lines it replaced.

Sources: baseline = `SCRATCH` `issue-1005-baseline-cobertura.xml` (P0-T7, merge-base code); final = `SCRATCH` `issue-1005-final-cobertura.xml` (P7-T4). Both are the post-processed Cobertura documents written by `Invoke-MSTestWithCoverage.ps1`; per-member figures come from Cobertura `method` elements, which the committed package-level JaCoCo projection does not carry. MERGE-BASE is recorded in `evidence/baseline/merge-base-scope.2026-10-22T16-06.md`.

## Repository figures

| Metric | Baseline | Final | Floor | Result |
| --- | --- | --- | --- | --- |
| Line | 86.30% (57542/66677) | 86.37% (57627/66720) | 80% | PASS, not below baseline |
| Branch | 80.58% (14036/17419) | 80.66% (14060/17431) | 75% | PASS, not below baseline |

## New files (line coverage)

| File | Final | Threshold | Result |
| --- | --- | --- | --- |
| `UtilitiesCS/HelperClasses/Settings/ApplicationSettingsStore.cs` | 100.00% (7/7) | 90% | PASS |
| `TaskMaster/AppGlobals/AppAutoFileObjects.ScalarSettings.cs` | 100.00% (36/36) | 90% | PASS |
| `UtilitiesCS/HelperClasses/Settings/ISettingsStore.cs` | not instrumented (interface only, no executable lines) | n/a | n/a |

## New or changed seam members (line coverage, final)

| File | Member | Covered / Lines | % |
| --- | --- | --- | --- |
| `ApplicationSettingsStore.cs` | `.ctor(ApplicationSettingsBase)` | 4/4 | 100 |
| `ApplicationSettingsStore.cs` | `get_Item(string)` | 1/1 | 100 |
| `ApplicationSettingsStore.cs` | `set_Item(string, object)` | 1/1 | 100 |
| `ApplicationSettingsStore.cs` | `Save()` | 1/1 | 100 |
| `IDList.cs` | `.ctor(IEnumerable<string>, ISettingsStore)` (new) | 9/9 | 100 |
| `IDList.cs` | `.ctor(ISettingsStore)` (new) | 9/9 | 100 |
| `IDList.cs` | `GetNextToDoID(string)` (changed) | 28/28 | 100 |
| `AppQuickFilerSettings.cs` | `.ctor()` (new) | 1/1 | 100 |
| `AppQuickFilerSettings.cs` | `.ctor(ISettingsStore)` (new) | 5/5 | 100 |
| `AppQuickFilerSettings.cs` | getters `MoveEntireConversation`, `SaveAttachments`, `SavePictures`, `SaveEmailCopy`, `HighConfidenceModeEnabled`, `HighConfidenceThreshold` | 1/1 each | 100 |
| `AppQuickFilerSettings.cs` | setters of the same six properties | 4/4 each | 100 |
| `AppStagingFilenames.cs` | `.ctor()` (new) | 1/1 | 100 |
| `AppStagingFilenames.cs` | `.ctor(ISettingsStore)` (new) | 5/5 | 100 |
| `AppStagingFilenames.cs` | getters of all ten properties | 1/1 to 5/5 each | 100 |
| `AppStagingFilenames.cs` | nine saving setters | 5/5 each | 100 |
| `AppStagingFilenames.cs` | `EmailInfoStagingFile` setter | 1/1 | 100 |
| `AppStagingFilenames.cs` | `Read(string)` (new helper), `InitProp` | 1/1, 4/4 | 100 |
| `AppAutoFileObjects.ScalarSettings.cs` | `.ctor(IApplicationGlobals, ISettingsStore)` (new) | 6/6 | 100 |
| `AppAutoFileObjects.ScalarSettings.cs` | getters of `LngConvCtPwr`, `Conversation_Weight`, `SmithWatterman_MatchScore`, `SmithWatterman_MismatchScore`, `SmithWatterman_GapPenalty`, `MaxRecents` | 1/1 each | 100 |
| `AppAutoFileObjects.ScalarSettings.cs` | setters of the same six properties | 4/4 each | 100 |
| `AppAutoFileObjects.cs` | `.ctor(IApplicationGlobals)` (changed to chain) | 1/1 | 100 |

## Changed-line coverage per modified production file

Added or modified lines come from `git diff -U0 <merge-base>` (whole file for new files); "instrumented" counts those lines that carry a Cobertura line entry. Replaced lines are the removed or modified lines of the same diff, measured in the baseline Cobertura document.

| File | Added/modified lines | Instrumented | Covered | Changed-line % | Replaced lines instrumented | Replaced covered (baseline) | Baseline % | Result |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `ToDoModel/Data Model/ID/IDList.cs` | 36 | 15 | 15 | 100.00 | 2 | 2 | 100.00 | PASS |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` | 39 | 24 | 24 | 100.00 | 18 | 18 | 100.00 | PASS |
| `TaskMaster/AppGlobals/AppStagingFilenames.cs` | 61 | 42 | 42 | 100.00 | 28 | 7 | 25.00 | PASS |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | 1 | 1 | 1 | 100.00 | 33 | 33 | 100.00 | PASS |
| `TaskMaster/AppGlobals/AppAutoFileObjects.ScalarSettings.cs` (new) | 88 | 36 | 36 | 100.00 | 0 | 0 | n/a | PASS |
| `UtilitiesCS/HelperClasses/Settings/ApplicationSettingsStore.cs` (new) | 38 | 7 | 7 | 100.00 | 0 | 0 | n/a | PASS |
| `UtilitiesCS/HelperClasses/Settings/ISettingsStore.cs` (new) | 25 | 0 | 0 | n/a | 0 | 0 | n/a | n/a |

## Per-file context (not a gate)

| File | Baseline | Final |
| --- | --- | --- |
| `ToDoModel/Data Model/ID/IDList.cs` | 24.36% (38/156) | 26.04% (44/169) |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` | 100.00% (30/30) | 100.00% (36/36) |
| `TaskMaster/AppGlobals/AppStagingFilenames.cs` | 30.77% (20/65) | 100.00% (79/79) |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | 45.66% (184/403) | 40.81% (151/370) |

The `AppAutoFileObjects.cs` file figure falls because the six fully covered scalar properties (33 covered lines) moved to `AppAutoFileObjects.ScalarSettings.cs`, which is at 100%; no line that remains in `AppAutoFileObjects.cs` lost coverage, and its one changed line is covered. `IDList.cs` existing Outlook-bound and deserializing constructors remain uncovered as at baseline; their source is unchanged apart from sharing the new field initializer, whose lines are covered.

## No exclusion added

No `[ExcludeFromCodeCoverage]` was added (`parallel-constraints-scan.2026-10-22T16-28.md`: 0 added lines) and `coverage.config` is unchanged.

Result: PASS. No shortfall; not remediation-required.
