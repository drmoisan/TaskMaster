# Coverage Baseline (P0-T5)

- Timestamp: 2026-07-19T10-50
- Task: [P0-T5]
- Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot UtilitiesCS.Test -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/baseline/coverage-baseline.2026-07-19T10-50.cobertura.xml`
  - Driver wraps `dotnet-coverage collect --output-format cobertura --settings coverage.config -- <vstest.console.exe> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`.
  - Scope note: `-SearchRoot UtilitiesCS.Test` scopes discovery to `UtilitiesCS.Test.dll` — the assembly every batch test task in this plan targets — for an apples-to-apples baseline/final delta. `coverage.config` module excludes prevent the Deedle/FSharp instrumentation flakiness that would otherwise fail DataFrame tests.
- EXIT_CODE: 0
- Cobertura XML: `evidence/baseline/coverage-baseline.2026-07-19T10-50.cobertura.xml`

## Output Summary

- Tests: Total 4511, Passed 4511, Failed 0. Total time 25.39s.
- Overall (Cobertura root `<coverage>`): line-rate **0.65299** (65.30%), branch-rate **0.613274** (61.33%); lines-covered 67625 / lines-valid 103562; branches-covered 15690 / branches-valid 25584. (Root overall spans all UtilitiesCS production code instrumented by this test assembly.)
- Targeted in-scope `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,Table}/` production line coverage (deduped per line across partial classes, test files excluded): **87.07%** (3319 covered / 3812 valid) across 28 files with executable lines (the 2 dead files `CaptureEmailAddressesModule2.cs` and `ItemComparer.cs` have no executable lines).

### Per-file in-scope baseline (line %, covered/valid)

| % | covered/valid | file |
|---|---|---|
| 94.7 | 177/187 | Attachment/AttachmentHelper.cs |
| 97.1 | 136/140 | Attachment/AttachmentSerializable.cs |
| 93.6 | 176/188 | Conversation/ConversationHelper.Formatting.cs |
| 85.3 | 209/245 | Conversation/ConversationHelper.cs |
| 88.2 | 90/102 | Item/OlItemPseudoInterface.cs |
| 80.4 | 78/97 | Item/OlItemSummary.cs |
| 82.5 | 174/211 | Item/OutlookItem.cs |
| 80.0 | 196/245 | Item/OutlookItemExtensions.cs |
| 80.2 | 190/237 | Item/OutlookItemFlaggable.cs |
| 100.0 | 18/18 | Item/OutlookItemFlaggableTry.cs |
| 100.0 | 100/100 | Item/OutlookItemTry.cs |
| 95.9 | 71/74 | Item/OutlookItemTryGet.cs |
| 94.7 | 36/38 | MailItem/CidImageResolver.cs |
| 82.7 | 115/139 | MailItem/EmailDetails.cs |
| 100.0 | 12/12 | MailItem/EmailDetailsWrapper.cs |
| 91.6 | 76/83 | MailItem/ItemInfo.cs |
| 100.0 | 25/25 | MailItem/MailItemExtensions.cs |
| 100.0 | 167/167 | MailItem/MailItemHelper.Html.cs |
| 63.2 | 120/190 | MailItem/MailItemHelper.Loading.cs |
| 81.6 | 93/114 | MailItem/MailItemHelper.Properties.cs |
| 74.6 | 88/118 | MailItem/MailItemHelper.Serialization.cs |
| 97.3 | 179/184 | MailItem/MailItemHelper.cs |
| 100.0 | 18/18 | MailItem/MailResolution.cs |
| 91.2 | 271/297 | Table/OlTableExtensions.Etl.cs |
| 100.0 | 48/48 | Table/OlTableExtensions.RowTransforms.cs |
| 79.6 | 215/270 | Table/OlTableExtensions.TableAccess.cs |
| 96.1 | 172/179 | Table/OlTableExtensions.cs |
| 80.2 | 69/86 | Table/OlToDoTable.cs |

This per-file table is the reference for the P10-T6 changed-line no-regression check (AC4).
