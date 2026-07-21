# Batch I — Pragma-Only Nullable Build Verification (P9-T7)

- Timestamp: 2026-07-19T10-50
- Task: [P9-T7]
- Files opted in (Batch I, OlTableExtensions partial-class group, 4 files verified as ONE unit): `Table/OlTableExtensions.cs`, `Table/OlTableExtensions.Etl.cs`, `Table/OlTableExtensions.RowTransforms.cs`, `Table/OlTableExtensions.TableAccess.cs`
- Upstream contract gate: P9-T1 `ArrayExtensions.ToStringArray`/`SliceRow`/`To2D` (#363) VERIFIED landed (live at `UtilitiesCS/Extensions/ArrayExtensions.cs` lines 17/42/56/81, 102, 118; non-nullable `string[,]`/`string[]` returns). Also depends on Batch H (`using static UtilitiesCS.ConvHelper;`), already complete.
- Authoritative in-scope CS86xx Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false`
- EXIT_CODE (isolated authoritative build): 0

## Key decisions (annotation-only, faithful)

- `OlTableExtensions.cs`: `LogTableTiming` `details` -> `string?`; `RunTableRetry<T>` -> `T?` (returns `default(T)`); nullable `object[,]? data` local in `ExtractData2`.
- `OlTableExtensions.Etl.cs`: the `ETL`/`EtlAsync`/`EtlAsyncOld` and `EtlPrepAsync` public tuple return types were kept NON-nullable (behavior-compatible) so that their nullable-enabled out-of-scope consumers (`Extensions/DfDeedle.cs` `TableEtlInvoker`/`StoreTableEtlInvoker`/`EtlAsync`/`EtlPrepAsync` seams and `DfDeedle.FrameUtilities.cs`) continue to compile per the spec's public-signature-compatibility requirement. The genuine `(null, null)` error path and nullable `object[,]? data` local flow are expressed with a justified `!` at the tuple-return sites (`return (null!, null!)`, `return (data!, columnDictionary)`, `objectConverters!/objFields!/objIndices!`), documenting the pre-existing "callers assume non-null" contract. `GetObjectFields` -> `(IEnumerable<string>?, IEnumerable<int>?)` (private, internal-only); optional `objectConverters`/`progress` params nullable throughout; `objFields`/`objIndices` nullable on the internal `EtlByRow`/`EtlRow` params where they flow from `GetObjectFields`; `rows![i]` (the `rows is not null` check is redundant on the non-null param but introduces a maybe-null flow state).
- `OlTableExtensions.RowTransforms.cs`: `objFields`/`objIndices`/`objectConverters` params made nullable consistent with the `.Etl.cs` call-site contract (the existing `is not null` guards make them safe). No CS86xx originated here.
- `OlTableExtensions.TableAccess.cs`: `GetTableInViewAsync` -> `Task<Outlook.Table?>`; the three `TryGetTableAsync` overloads (Store/MAPIFolder/Conversation) -> `Task<object?>`; `GetTable(this Store)` -> `Outlook.Table?`; nullable locals (`Outlook.Table? table`, `Outlook.TableView? view` from `as`, `MAPIFolder? folder`); `timeoutSourceFactory` default-null param nullable.
- Cross-batch reconciliation (same feature): because the `ETL` public tuple was kept non-null, the already-opted-in `ConversationHelper.Formatting.cs` consumers deconstruct into non-null `(object[,] data, ...)` unchanged and call `data.ToDataFrame(...)` without a `!`. Behavior-compatibility was verified against the nullable-enabled `Extensions/DfDeedle.cs`/`DfDeedle.FrameUtilities.cs` consumers: total UtilitiesCS CS86xx == 0 (no regression in any out-of-scope nullable-enabled file). An initial attempt to make the ETL tuples nullable was reverted after the full-solution nullable gate (P10-T3) surfaced 7 CS86xx it would have introduced in DfDeedle/FrameUtilities, which would have violated the public-signature-compatibility requirement.

## Output Summary

- Errors: 0.
- CS86xx total across UtilitiesCS: 0.
- CS86xx in `UtilitiesCS/OutlookObjects/`: **0** across all 4 `OlTableExtensions` partial-class files verified together as one unit (consistent cross-file call-graph nullability), plus the re-touched `ConversationHelper.Formatting.cs`.
- No new diagnostics elsewhere.
