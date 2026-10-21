# P4-T1 — Scoped CSharpier Format Pass

Timestamp: 2026-09-03T11-34
Command: dotnet tool run csharpier format <four in-scope files, absolute paths>
(invoked via the item worktree's pinned .dotnet-sdk/dotnet.exe by absolute path)
EXIT_CODE: 0
Output Summary: "Formatted 4 files in 6074ms." (PROCESSED count, not a rewritten count.)

Before hashes (SHA256):
- QfcHomeController.Metrics.cs: 0F48D72116F0504563FAA75F3606CD0C1E8692565F554B64ADB492900916F396
- EfcHomeController.Metrics.cs: 08D1C18E4E280CE1E2EEAE317F29107D2E8E70486E16CB18CEDA9642BE8ECF58
- QfcHomeControllerMetricsTests.cs: 13A924399533AF5AF629E639D829AA3CD24EA97A4EB029ED4D95B6274EFB8F36
- EfcHomeControllerMetricsTests.cs: 994940F30DF03D279378253F3CA84626AE1297344E5F7BB5D78B7399F375342C

After hashes (SHA256):
- QfcHomeController.Metrics.cs: 0F48D72116F0504563FAA75F3606CD0C1E8692565F554B64ADB492900916F396
- EfcHomeController.Metrics.cs: 08D1C18E4E280CE1E2EEAE317F29107D2E8E70486E16CB18CEDA9642BE8ECF58
- QfcHomeControllerMetricsTests.cs: 13A924399533AF5AF629E639D829AA3CD24EA97A4EB029ED4D95B6274EFB8F36
- EfcHomeControllerMetricsTests.cs: 994940F30DF03D279378253F3CA84626AE1297344E5F7BB5D78B7399F375342C

RewrittenCount: 0 (all four before/after hashes are identical; the four in-scope files were
already CSharpier-clean after the P1/P2 edits, consistent with the P0-T13 baseline).
