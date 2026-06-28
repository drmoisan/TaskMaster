# Banned-API Baseline — Cycle 2 (Rebased Tree), Issue #218

Timestamp: 2026-06-28T17-31

Command: `Select-String -Path <8 touched production files> -Pattern 'DateTime\.Now','DateTime\.UtcNow','Random\.Shared','Thread\.Sleep','Task\.Delay'`

EXIT_CODE: 0

Banned set scanned: `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`. No matches for `DateTime.UtcNow`, `Random.Shared`, or `Thread.Sleep`. RS0030 (Microsoft.CodeAnalysis.BannedApiAnalyzers) is held at `suggestion` severity per `.claude/rules/csharp.md`, so these pre-existing call sites do not break the analyzer or nullable builds.

Matches (file:line on rebased tree):

Active (non-comment) code:
- QfcDatamodel.FrameBuilding.cs:43 — `await Task.Delay(5);`
- QfcDatamodel.QueueProcessing.cs:142 — `await Task.Delay(200);`
- QfcHomeController.cs:75 — `$"{DateTime.Now.ToString("mm:ss.fff")} "` (string-interpolation operand in active code)
- QfcHomeController.Metrics.cs:20 — `var now = DateTime.Now;`
- QfcHomeController.Metrics.cs:100 — `curDateText = DateTime.Now.ToString("MM/dd/yyyy");`
- QfcHomeController.Metrics.cs:102 — `curTimeText = DateTime.Now.ToString("hh:mm");`
- QfcHomeController.Metrics.cs:114 — `OlEndTime = DateTime.Now;`
- QfcHomeController.Metrics.cs:214 — `await Task.Delay(20);`

Commented-out code (no runtime effect):
- QfcDatamodel.cs:58, 65 — commented `logger.Debug($"{DateTime.Now...`
- QfcDatamodel.FrameBuilding.cs:54, 61, 76, 79 — commented logger lines
- QfcHomeController.cs:43, 262, 276, 281, 287 — commented logger lines
- QfcHomeController.Metrics.cs:21, 22 — commented lines

Output Summary: 8 active-code matches (DateTime.Now and Task.Delay), all pre-existing — carried verbatim from the original controllers by maintainer split 2637e4c1, not introduced by this remediation. RS0030 at suggestion severity means the build is not broken by them. Per-match disposition (removed-with-seam or precise deferred-finding) is performed in Phase 3 (P3-T1). Commented occurrences are not runtime code.
