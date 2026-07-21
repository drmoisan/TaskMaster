# Examples / Dead-Duplicate Pragma Verification (P11-T3)

Timestamp: 2026-07-19T10-54

P11-T1 — UtilitiesCS/Examples/MSDemoConv.cs opted in (annotation-only, default maintainer-decision
behavior; exclude/delete alternatives remain a maintainer flag in spec.md Maintainer Decisions item 2):
`Outlook.MailItem?`/`Outlook.Folder?` locals for the `... as Outlook.MailItem`/`as Outlook.Folder`
casts, with justified `!` at the demo's own derefs (`mailItem!.Parent`, `folder!.Store`, `inFolder!.Name`).
Resolves the CS8600/CS8602 sites; no new runtime guard.

P11-T2 — UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNewBackup.cs receives NO pragma and is
unmodified:
- `git status` for the backup file is clean (unmodified).
- `grep -c "#nullable"` = 0 (no pragma).
- `UtilitiesCS.csproj` `<Compile Include>` contains only the live `EmailIntelligence\People\PeopleScoDictionaryNew.cs`
  (plus `WrapperPeopleScoDictionaryNew.cs` and the interface); the backup file is NOT in the compile set,
  so a pragma on it would be a no-op that cannot emit CS86xx.
- Effective compiled hand-written opt-in set is 37 files (44 − 6 Designer − 1 dead uncompiled backup).
  The exclude/delete decision remains a maintainer flag (spec.md Maintainer Decisions item 1).

## Trustworthy isolated CS86xx gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168 -p:BuildProjectReferences=false`

EXIT_CODE: 0

Output Summary: Build succeeded. 0 errors, 0 CS86xx for `MSDemoConv.cs`; the backup file emitted no
diagnostics (not compiled). 15 pre-existing out-of-scope warnings.
