# Original reproduction test unchanged (issue 942)

Timestamp: 2026-09-30T07-42
Task: P2-T7 (creates this file); P3-T2 appends POST-FORMAT.
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; function Get-MethodText([string[]]$lines) { ... }; $base = @(git show 231e1c0b55105aeb626bf5a6e8d0266a567cacad:TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs); $now = Get-Content -LiteralPath "TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs" -Encoding UTF8; ... "METHOD_UNCHANGED=..."; "METHOD_LINES=..."' (the P2-T7 payload, run verbatim)
EXIT_CODE: 0

Output Summary:
- BASE_METHOD_SHA=19-3B-28-A7-14-24-D5-29-7C-F9-8E-63-49-B4-5D-54-B5-C7-17-D5-7D-55-5A-C8-03-DC-B7-D7-DB-F4-CD-E6
- NOW_METHOD_SHA=19-3B-28-A7-14-24-D5-29-7C-F9-8E-63-49-B4-5D-54-B5-C7-17-D5-7D-55-5A-C8-03-DC-B7-D7-DB-F4-CD-E6
- METHOD_UNCHANGED=True
- METHOD_LINES=32 (the `[TestMethod]` attribute line through the method's closing brace)
- Decoding note: the compared method span contains only ASCII characters and the file carries no BOM, so the console-code-page decoding of the `git show` output cannot alter the base side; no correction was applied.
