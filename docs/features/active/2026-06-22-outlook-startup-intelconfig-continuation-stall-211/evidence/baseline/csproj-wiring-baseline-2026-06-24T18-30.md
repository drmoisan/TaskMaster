# csproj-Wiring Baseline (Issue #211 PostLoad/LoadInboxes attribution probe)

Timestamp: 2026-06-24T18-30

Command:
- `grep -c "<Compile Include=" TaskMaster/TaskMaster.csproj`
- `grep -c "<Compile Include=" TaskMaster.Test/TaskMaster.Test.csproj`
- `grep -n "StartupInboxAttributionProbe" TaskMaster/TaskMaster.csproj TaskMaster.Test/TaskMaster.Test.csproj`
- `grep -nE "<Compile Include=\"[^\"]*\*\*" TaskMaster/TaskMaster.csproj TaskMaster.Test/TaskMaster.Test.csproj`

EXIT_CODE: 0 (count commands); 1 (no-match greps, as expected)

Output Summary:
- `TaskMaster/TaskMaster.csproj` uses explicit `<Compile Include>` items (31 items); no glob/wildcard (`**`) Compile entry found.
- `TaskMaster.Test/TaskMaster.Test.csproj` uses explicit `<Compile Include>` items (28 items); no glob/wildcard (`**`) Compile entry found.
- No `StartupInboxAttributionProbe` Compile item exists in either project yet (grep exit 1 = no match).
- Conclusion: both projects require explicit `<Compile Include>` wiring for any new `.cs` file (P1-T2, P1-T6, P2-T1).
