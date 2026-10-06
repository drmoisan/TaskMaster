# P5-T8 AC8 check-off

Timestamp: 2026-10-06T18-36
Command: Grep tool over spec.md `^- \[x\] AC8 ` and `^- \[[ x]\] AC[0-9]+ ` (count); Grep over the named artifact; live Greps for the csproj Include version and the redirect pair version
EXIT_CODE: 0
Output Summary: AC8 met and checked off. The artifact records the GetAssemblyName command, the placeholder-composed absolute DLL path and the printed `ASSEMBLY-VERSION: 10.0.0.12`, which equals the expected value. Every AC7 Include (5) and every AC9 newVersion (15) carries 10.0.0.12 character for character.

Artifacts read:
- evidence/other/system-linq-asyncenumerable-assembly-version.md: EXIT_CODE 0. The command calls `[System.Reflection.AssemblyName]::GetAssemblyName`. The DLL path reads `<execution-worktree-root>\packages\System.Linq.AsyncEnumerable.10.0.12\lib\net462\System.Linq.AsyncEnumerable.dll`, which is the absolute path required by the shared no-host-path rule. The printed lines are `ASSEMBLY-NAME: System.Linq.AsyncEnumerable`, `ASSEMBLY-VERSION: 10.0.0.12` and `PUBLIC-KEY-TOKEN: b03f5f7f11d50a3a`.
- P3-T8 to P3-T12 Include Greps (EXIT_CODE 0); live Grep `Include="System\.Linq\.AsyncEnumerable, Version=10\.0\.0\.12,` matches in the five projects.
- P3-T13 CMD-PAIR-COUNT with VERSION (EXIT_CODE 0); live multiline Grep `oldVersion="0\.0\.0\.0-10\.0\.0\.12" newVersion="10\.0\.0\.12"` after the System.Linq.AsyncEnumerable identity: 15 files, 1 each.

SPEC-LINE: `- [x] AC8 (assembly version read, not assumed).` (criterion text unchanged)
