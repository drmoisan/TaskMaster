# Batch 3 Pragma Verification (P4-T3)

Timestamp: 2026-07-19T10-54

Batch 3 opted-in files (2, Recipient cluster):
1. UtilitiesCS/OutlookObjects/Recipient/RecipientInfo.cs — `string?` fields (`_name/_address/_html`) and
   props (`Name/Address/Html`) per the #371 ItemInfo/EmailDetails field-nullability pattern (Equals/
   GetHashCode already use `?? ""`). Ctor params left non-null; all callers pass non-null.
2. UtilitiesCS/OutlookObjects/Recipient/RecipientStatic.cs (774 lines — pre-existing 500-line breach, NOT split):
   - `GetGlobalAddressList` return `Outlook.AddressList?` (has `return null`).
   - `GetSenderAddress(MailItem)` local `string? address = null`.
   - `ToResolvedRecipient(this AddressEntry, ...)` return `Recipient?` (has `return default`).
   - `GetInfo(this Recipient, SegmentStopWatch? sw = null)`.
   - `ExtractNameFromAddress` return tuple `(string? FirstName, string? LastName, string? DomainName)`
     (has `return (null, null, null)` and `(x, null, y)`).

## Trustworthy isolated CS86xx gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168 -p:BuildProjectReferences=false`

EXIT_CODE: 0

Output Summary: Build succeeded. 0 errors, 0 CS86xx, 15 pre-existing out-of-scope warnings. All
annotations are additive nullability only. `IsNullOrEmpty` (non-refining on net481) is used only on
string-typed values; no new runtime guard was added and existing guards (`?? ""`, `is null`,
try/catch fallbacks) are preserved. `RecipientStatic.cs` was NOT split (pre-existing breach flagged
in spec.md item 6 / AC8). The `Recipient`-overload of `ToResolvedRecipient` stays non-null (it never
returns null); only the `AddressEntry` overload became `Recipient?`.
