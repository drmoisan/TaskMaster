# Batch 1 Pragma Verification (P2-T6)

Timestamp: 2026-07-19T10-54

Batch 1 opted-in files (5):
1. UtilitiesCS/OutlookObjects/Calendar/Calendar.cs — `GetCalendar` return `Folder?`; local `Folder? foundCalendar = null`.
2. UtilitiesCS/OutlookObjects/Category/CreateCategory.cs — `CreateCategoryModule.CreateCategory` return `Category?`; local `Category? objCategory = null`.
3. UtilitiesCS/OutlookObjects/Com/ComType.cs — `TypeInformation.GetTypeName` return `string?` (already `return null`).
4. UtilitiesCS/OutlookObjects/Explorer/ExplorerActions.cs — `GetCurrentItem` and `Readable` returns `object?`.
5. UtilitiesCS/OutlookObjects/MailResolution.cs (root; class `MailResolution_ToRemove`) — `TryResolveMailItemDep` return `MailItem?`; local `MailItem? OlMail = null`.

## Trustworthy isolated CS86xx gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168 -p:BuildProjectReferences=false`

EXIT_CODE: 0

Output Summary: Build succeeded. 0 errors, 0 CS86xx, 15 pre-existing out-of-scope warnings. All
five Batch 1 files reached zero CS86xx with additive nullability-annotation return-type changes and
`?`-annotated locals only. No new runtime guard added; existing guards (`?? ""`, `IsNullOrEmpty`,
COM enumeration) preserved.

## Deviations from the plan's suggested annotation
- P2-T1: the plan named method `FindCalendar`; the actual method in Calendar.cs is `GetCalendar`.
  Annotated the actual method (`GetCalendar` return `Folder?`, local `Folder? foundCalendar`),
  which is the return-nullability method the task intends. Substance unchanged.
- The out-of-scope sibling `UtilitiesCS/OutlookObjects/MailItem/MailResolution.cs` (#371) was NOT
  touched, per plan. Only the root `MailResolution.cs` was opted in.

Full-solution mandated command note: same as Batch 0 — recorded once at P12-T3; per-batch signal is
the isolated build above (SVGControl ProjectReference blocks the full-solution UtilitiesCS compile).
