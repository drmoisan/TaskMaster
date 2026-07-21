# Batch F — Pragma-Only Nullable Build Verification (P6-T5)

- Timestamp: 2026-07-19T10-50
- Task: [P6-T5]
- Files opted in (Batch F): `MailItem/ItemInfo.cs`, `MailItem/EmailDetails.cs`, `MailItem/EmailDetailsWrapper.cs`
- Plan-literal Command: `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` (halts on out-of-scope SVGControl CS0649; see P0-T4).
- Authoritative in-scope CS86xx Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false`
- EXIT_CODE (isolated authoritative build): 0

## Annotations applied (annotation-only, faithful)

- `ItemInfo` (POCO `IItemInfo`): all reference-type auto-properties made nullable to reflect the parameterless-ctor / deserialization state — string props -> `string?`; `AttachmentsInfo` -> `IAttachment[]?`; `FolderInfo` -> `IFolderWrapper?`; `Sender` -> `IRecipientInfo?`; `CcRecipients`/`ToRecipients` -> `IRecipientInfo[]?`; `Tokens` -> `string[]?`; `Sw` -> `SegmentStopWatch?`. This mirrors the Batch G `MailItemHelper` decisions (both implement the oblivious `IItemInfo`). `Equals(IItemInfo? other)` param nullable; the `Sender.Equals`/`Sender.GetHashCode` derefs use a justified `!` (the type's equality contract assumes a set `Sender`, preserving the original assume-non-null behavior); `RecipientsEquivalent`/`GetRecipientsHashCode` params -> nullable (they already null-guard internally).
- `EmailDetails` (static extensions): `Details(MailItem,...)`/`Details(MailItemHelper,...)`/`GetEmailFolderPath` `dictRemap` params -> `IScoDictionaryNew<string,string>?` (default-null / null-checked). No other CS86xx (COM-oblivious).
- `EmailDetailsWrapper` (`IEmailDetailsWrapper` thin delegator seam): `Details` `dictRemap` and `GetInfo` `sw` default-null params -> nullable. The seam over the static `EmailDetails` extension methods is preserved exactly; the delegated `GetSenderInfo`/`GetSenderName`/etc. resolve to out-of-scope oblivious extension methods, so wrapper return types are unchanged.

## Output Summary

- Errors: 0.
- CS86xx total across UtilitiesCS: 0.
- CS86xx in `UtilitiesCS/OutlookObjects/`: **0** for the 3 opted-in Batch F files.
- No new diagnostics elsewhere. Note: the `EmailDetails.Details(MailItemHelper)` overload derefs `helper.FolderInfo`/`.Sender`/`.ToRecipients`/`.CcRecipients`/`.AttachmentsInfo`, which are still oblivious (MailItemHelper opts in at Batch G); any derefs those surface once MailItemHelper is nullable are reconciled in Batch G, and the final P10-T3 gate re-verifies all 30 files.
