# Batch G — Pragma-Only Nullable Build Verification (P7-T9)

- Timestamp: 2026-07-19T10-50
- Task: [P7-T9]
- Files opted in (Batch G, MailItemHelper partial-class group, 5 files verified as ONE unit): `MailItem/MailItemHelper.cs`, `MailItem/MailItemHelper.Html.cs`, `MailItem/MailItemHelper.Loading.cs`, `MailItem/MailItemHelper.Properties.cs`, `MailItem/MailItemHelper.Serialization.cs`
- Upstream contracts verified landed: #363 `LazyExtension.ToLazy`/`.ToLazyValue`/`.ToLazyTry` (P7-T1); #364 `Initializer.GetOrLoad` `ref T`/`T?` overloads (P7-T2).
- Plan-literal Command: `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` (halts on out-of-scope SVGControl CS0649; see P0-T4).
- Authoritative in-scope CS86xx Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false`
- EXIT_CODE (isolated authoritative build): 0

## Key decisions (annotation-only, faithful)

- The 33 `Lazy<T>`/`LazyTry<T>` backing fields made nullable (`Lazy<T>?`), eliminating the 125 CS8618 "uninitialized non-nullable field" diagnostics that arose because `InitLazyFields`/`InitializeSafeDefaults` set the fields in a separate method the compiler cannot see through (net481 has no `[MemberNotNull]`). The getters already use `_x?.Value ?? default`, so this is consistent.
- The four lazy-backed public properties WITHOUT a `??` fallback annotated nullable per P7-T6: `Sender` -> `IRecipientInfo?`, `FolderInfo` -> `IFolderWrapper?`, `AttachmentsInfo` -> `IAttachment[]?`, `Globals` -> `IApplicationGlobals?`. Their setters use `value?.ToLazy()` (the property value is now nullable and `ToLazy<T>` has a `where T : class` constraint). No new `??` guard added.
- `MailItemHelper.Html.cs`: the interior `#nullable enable`/`#nullable disable` region (former lines 107/144) normalized to a single whole-file pragma (both interior directives removed); `_emailHeader`'s existing `?` annotation reconciled.
- `_item` given `= null!` deferred init so `ref _item` matches `GetOrLoad<MailItem>` (the strict overload infers `T=MailItem` from the loader) and so the pervasive `_item.*` reflection/COM accesses stay clean. `ResolveMail` return `MailItem` -> `MailItem?` (consuming `Initializer.GetOrLoad`'s `T?` contract); `ResolveMailAsync` -> `Task<MailItem?>`.
- `InitializeSafeDefaults` `new Lazy<T>(() => null)` for the three nullable-yielding fields replaced with a direct `_x = null` (behavior-identical: the getter returns null either way). Provably-redundant `?.` on non-null receivers (`OlRecipients`, `AttachmentsHelper` — non-null via `?? Array.Empty` getters; `_item.Recipients` — a non-null COM collection) removed in the collection factories so their `Lazy<T[]>` factory lambdas produce non-null. `PropertyChanged` event -> `PropertyChangedEventHandler?`; `Sw` -> `SegmentStopWatch?`; `LogMailItemTiming` `details` -> `string?`; discard arrays holding nullable members -> `object?[]`; `_entryId!/_storeId!` passed to `GetOrLoad`'s `params object[] dependencies` (compile-time only; the runtime values, including null, still flow to the dependency null-check).
- Cross-batch reconciliation (same feature): the already-opted-in `EmailDetails.Details(MailItemHelper)` overload derefs of the now-nullable `helper.FolderInfo`/`.Sender` use a justified `!` (the details projection assumes a resolved helper). `CidImageResolver.RewriteCidReferences`/`BuildContentIdMap` `attachments` params made `IReadOnlyCollection<IAttachment>?` (they already null-guard internally) so `MailItemHelper.Html.cs`'s `AttachmentsInfo` (nullable) flows without a new guard.

## Output Summary

- Errors: 0.
- CS86xx total across UtilitiesCS: 0.
- CS86xx in `UtilitiesCS/OutlookObjects/`: **0** across all 5 `MailItemHelper` partial-class files verified together as one unit (no inconsistent CS8618/definite-assignment diagnostics between files), plus the re-touched `EmailDetails.cs` and `CidImageResolver.cs`.
- No new diagnostics elsewhere.
