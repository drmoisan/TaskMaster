# Batch C — Pragma-Only Nullable Build Verification (P3-T8)

- Timestamp: 2026-07-19T10-50
- Task: [P3-T8]
- Files opted in (Batch C, small COM-bound leaves): `MailItem/MailResolution.cs`, `MailItem/MailItemExtensions.cs`, `Item/OlItemPseudoInterface.cs`, `Item/OlItemSummary.cs`, `Table/OlToDoTable.cs`
- Plan-literal Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`) — solution build halts on pre-existing out-of-scope SVGControl CS0649 (see P0-T4).
- Authoritative in-scope CS86xx Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false` (NO TWAE, NO `/p:Nullable=enable`)
- EXIT_CODE (isolated authoritative build): 0

## Annotations applied (annotation-only, faithful to actual null behavior)

- `MailResolution.TryResolveMailItem(object)` return `MailItem` -> `MailItem?` (returns null when input is not a readable MailItem); local `MailItem? olMail`.
- `MailItemExtensions.ToMIME` return `byte[]` -> `byte[]?` (`PropertyAccessor.GetProperty(...) as byte[]` can be null); local `byte[]? mimeContent`. `TryMoveAsync` return `Task<object>` -> `Task<object?>` (returns null on failure). Extension `this`/`folder` param nullability unchanged (pre-existing defensive `is null` guard produced no diagnostic).
- `OlItemPseudoInterface.cs`: no CS86xx (all COM-oblivious paths); pragma-only.
- `OlItemSummary.ExtractSummary(MailItem)`: local `MailItem? OlMail` consuming the new `MailResolution.TryResolveMailItem` `MailItem?` contract.
- `OlToDoTable.cs`: `GetToDoTable` return `Outlook.Table` -> `Outlook.Table?`; nullable locals `MAPIFolder? folder`, `UserDefinedProperty? field`, `Items? items`, `object? itemObj`, `string? entryId`, `string? value`. The `dynamic item = itemObj;` line is byte-unchanged (see maintainer-flags P3-T6). No new runtime guards; existing try/catch guards preserved.

## Output Summary

- Errors: 0.
- CS86xx total across UtilitiesCS: 0.
- CS86xx in `UtilitiesCS/OutlookObjects/`: **0** for the 5 opted-in Batch C files.
- No new diagnostics elsewhere; no cascade from the `MAPIFolder? folder`/return-type changes (the compiler proves `folder` non-null after the return-in-catch try block).
