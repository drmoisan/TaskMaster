# Batch E — Pragma-Only Nullable Build Verification (P5-T5)

- Timestamp: 2026-07-19T10-50
- Task: [P5-T5]
- Files opted in (Batch E, Attachment cluster): `Attachment/AttachmentSerializable.cs`, `Attachment/AttachmentHelper.cs`
- Upstream #364 dependency: `FilePathHelper` non-nullable `""`-default `FilePath`/`FolderPath`/`FileName` contract verified landed in P5-T1.
- Plan-literal Command: `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` (halts on out-of-scope SVGControl CS0649; see P0-T4).
- Authoritative in-scope CS86xx Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false`
- EXIT_CODE (isolated authoritative build): 0

## Annotations applied (annotation-only, faithful)

- `AttachmentSerializable` (POCO `IAttachment`): nullable POCO string props `FileName`/`DisplayName`/`PathName`/`ContentId`/`FileExtension`/`FilenameSeed` -> `string?` (genuinely null via parameterless ctor / deserialization; consumers reach them via the oblivious `IAttachment` interface so no cascade). COM mocking props `Application`/`Session` -> nullable; get-only `Parent`/`PropertyAccessor` -> nullable (never assigned). Lazy fields `_isImage` -> `Lazy<bool>?`, `_data` -> `Lazy<byte[]?>?`; `_a` -> `Attachment?` with `GetBytes(_a!)` in the lazy factory (justified: the lazy is created and evaluated only in the `(Attachment,bool)` ctor path where `_a` is set). `GetBytes` -> `byte[]?`; `AttachmentData` -> `byte[]?`; `TryFromSaveAsLoad`/`TryFromAccessor` -> `out byte[]?`; `TryFromContentIdAccessor` -> `out string?`. `AttachmentData` setter rebuilt as an explicit `new Lazy<byte[]?>` because `ToLazy<T>` has a `where T : class` constraint that `byte[]?` cannot satisfy (behavior-identical: null stays null).
- `AttachmentHelper`: `_attachmentInfo` -> `AttachmentSerializable?` (the setter uses `value as AttachmentSerializable`, which is nullable), `AttachmentInfo` getter uses `_attachmentInfo!` (invariant: assigned during construction/Init). `_attachment` given `= null!` deferred init. `_errorMessages` -> `List<string>?` (null until CheckParameters); `_filePathDelete`/`_folderPathDelete` -> `string?` (conditionally set). `Init`/`CheckParameters(4-arg)` `deleteFolderPath` -> `string?` (the 3-arg ctor passes `null`). `FilePathSave`/`FolderPathSave` left non-nullable `string`, forwarding to the #364 `FilePathHelperSave.FilePath`/`.FolderPath` non-nullable `""`-default contract as-is (no conflicting annotation, per P5-T3).

## Output Summary

- Errors: 0.
- CS86xx total across UtilitiesCS: 0.
- CS86xx in `UtilitiesCS/OutlookObjects/`: **0** for the 2 opted-in Batch E files.
- No new diagnostics elsewhere.
