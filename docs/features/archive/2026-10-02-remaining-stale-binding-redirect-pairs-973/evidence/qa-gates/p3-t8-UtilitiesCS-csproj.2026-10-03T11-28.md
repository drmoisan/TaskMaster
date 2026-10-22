# P3-T8 EDIT-CSPROJ UtilitiesCS/UtilitiesCS.csproj (issue #973)

Timestamp: 2026-10-03T11-28
Command: Edit tool on UtilitiesCS/UtilitiesCS.csproj (old_string the three-line System.Linq.Async Reference element read by Grep `Include="System.Linq.Async,` -A 2 at 398-400; new_string those lines plus the nine spec Part C lines with VERSION 10.0.0.12); Grep `Include="System.Linq.AsyncEnumerable, Version=10.0.0.12,` count; Grep `<Aliases>SystemLinqAsyncEnumerable</Aliases>` count; Grep `CS0121` count; Grep `CS0433` count; CMD-LINECOUNT; CMD-CRCOUNT; git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- UtilitiesCS/UtilitiesCS.csproj
EXIT_CODE: 0
Output Summary: aliased Reference element and its comment inserted after line 400; every EDIT-CSPROJ gate holds; LINECOUNT 1335 to 1344, CRCOUNT 1334 to 1343 (+9 each); numstat 9/0.

Before: LINECOUNT 1335, CRCOUNT 1334
After: LINECOUNT 1344, CRCOUNT 1343
Include Version=10.0.0.12: 1; Aliases: 1; CS0121: 1; CS0433: 1
NUMSTAT: 9	0	UtilitiesCS/UtilitiesCS.csproj
