# Batch I — Pragma-Only Nullable Build Verification (P9-T7)

- Timestamp: 2026-07-19T10-50
- Task: [P9-T7]
- Files opted in (Batch I, OlTableExtensions partial-class group, 4 files verified as ONE unit): `Table/OlTableExtensions.cs`, `Table/OlTableExtensions.Etl.cs`, `Table/OlTableExtensions.RowTransforms.cs`, `Table/OlTableExtensions.TableAccess.cs`
- Upstream contract gate: P9-T1 `ArrayExtensions.ToStringArray`/`SliceRow`/`To2D` (#363) VERIFIED landed (live at `UtilitiesCS/Extensions/ArrayExtensions.cs` lines 17/42/56/81, 102, 118; non-nullable `string[,]`/`string[]` returns). Also depends on Batch H (`using static UtilitiesCS.ConvHelper;`), already complete.
- Authoritative in-scope CS86xx Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false`
- EXIT_CODE (isolated authoritative build): 0

## Key decisions (annotation-only, faithful)

- `OlTableExtensions.cs`: `LogTableTiming` `details` -> `string?`; `RunTableRetry<T>` -> `T?` (returns `default(T)`); nullable `object[,]? data` local in `ExtractData2`.
- `OlTableExtensions.Etl.cs`: the `ETL`/`EtlAsync`/`EtlAsyncOld` tuple returns made `(object[,]? data, Dictionary<string, int>? columnInfo)` (they genuinely return `(null, null)` on a null table and a null `data`); `GetObjectFields` -> `(IEnumerable<string>?, IEnumerable<int>?)`; optional `objectConverters`/`progress` params nullable throughout; `objFields`/`objIndices` nullable where they flow from `GetObjectFields`; `EtlPrepAsync` tuple's `objFields`/`objIndices` nullable; `rows![i]` (the `rows is not null` check is redundant on the non-null param but introduces a maybe-null flow state).
- `OlTableExtensions.RowTransforms.cs`: `objFields`/`objIndices`/`objectConverters` params made nullable consistent with the `.Etl.cs` call-site contract (the existing `is not null` guards make them safe). No CS86xx originated here.
- `OlTableExtensions.TableAccess.cs`: `GetTableInViewAsync` -> `Task<Outlook.Table?>`; the three `TryGetTableAsync` overloads (Store/MAPIFolder/Conversation) -> `Task<object?>`; `GetTable(this Store)` -> `Outlook.Table?`; nullable locals (`Outlook.Table? table`, `Outlook.TableView? view` from `as`, `MAPIFolder? folder`); `timeoutSourceFactory` default-null param nullable.
- Cross-batch reconciliation (same feature): the already-opted-in `ConversationHelper.Formatting.cs` consumes the now-nullable `ETL` tuple; its three `(object[,] data, ...) = table.ETL()` deconstructions made nullable with a justified `data!`/`columnInfo!` before `ToDataFrame` (`GetInfoDf`/`GetDataFrame`/`GetDataFrameAsync` assume a resolved table). The oblivious `Extensions/DfDeedle.cs` ETL consumer is unaffected (nullable-oblivious, no cross-block).

## Output Summary

- Errors: 0.
- CS86xx total across UtilitiesCS: 0.
- CS86xx in `UtilitiesCS/OutlookObjects/`: **0** across all 4 `OlTableExtensions` partial-class files verified together as one unit (consistent cross-file call-graph nullability), plus the re-touched `ConversationHelper.Formatting.cs`.
- No new diagnostics elsewhere.
