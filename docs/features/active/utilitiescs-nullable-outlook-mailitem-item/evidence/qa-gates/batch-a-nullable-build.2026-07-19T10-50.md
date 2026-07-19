# Batch A — Pragma-Only Nullable Build Verification (P1-T4)

- Timestamp: 2026-07-19T10-50
- Task: [P1-T4]
- Files opted in (Batch A, dead code): `MailItem/CaptureEmailAddressesModule2.cs`, `Item/ItemComparer.cs`
- Plan-literal Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`)
  - As established in P0-T4, the solution-wide TWAE Rebuild halts on 2 pre-existing out-of-scope vendored `SVGControl` CS0649 errors before reaching `UtilitiesCS`. This is a documented pre-existing epic-branch condition (flag-not-fix), not introduced by #371.
- Authoritative in-scope CS86xx Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false` (NO TWAE, NO `/p:Nullable=enable`; `warning CS86xx` count == TWAE `error CS86xx` count because UtilitiesCS.csproj is nullable-oblivious by default)
- EXIT_CODE (isolated authoritative build): 0

## Output Summary

- Errors: 0.
- CS86xx total across UtilitiesCS: 0.
- CS86xx in `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,Table}/`: **0** for the 2 opted-in Batch A files.
- No new diagnostics elsewhere (isolated UtilitiesCS build compiled clean; the two files are commented-out dead code, so the `#nullable enable` pragma is a no-op producing zero live diagnostics).
