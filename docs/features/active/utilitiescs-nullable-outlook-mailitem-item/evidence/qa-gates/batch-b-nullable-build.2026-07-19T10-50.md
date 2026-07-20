# Batch B — Pragma-Only Nullable Build Verification (P2-T3)

- Timestamp: 2026-07-19T10-50
- Task: [P2-T3]
- File opted in (Batch B, host-neutral leaf, NON-exempt coverage): `MailItem/CidImageResolver.cs`
- Plan-literal Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`) — solution build halts on pre-existing out-of-scope SVGControl CS0649 (see P0-T4).
- Authoritative in-scope CS86xx Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false` (NO TWAE, NO `/p:Nullable=enable`)
- EXIT_CODE (isolated authoritative build): 0

## Annotation applied

- Added `#nullable enable` whole-file pragma.
- Single CS8602 surfaced at `BuildContentIdMap`'s `map[attachment.ContentId] = attachment;` (the loop element `attachment` acquired a maybe-null flow state from the pre-existing defensive `attachment?.ContentId` guard on the line above). Fix: null-forgiving `attachment!.ContentId` on the key, justified by the preceding `!string.IsNullOrEmpty(attachment?.ContentId)` guard (which returns false unless `attachment` and its `ContentId` are both non-null). Annotation-only; no new runtime guard; existing guards unchanged. Public parameter signatures (`html`, `attachments`, `virtualHost`) unchanged — the tests exercise them with non-null inputs only, so the tested contract stays behavior-compatible.

## Output Summary

- Errors: 0.
- CS86xx total across UtilitiesCS: 0.
- CS86xx in `UtilitiesCS/OutlookObjects/`: **0** for `CidImageResolver.cs`.
- No new diagnostics elsewhere.
