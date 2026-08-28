---
name: 442-review-residuals
description: "#442/#443/#451 metrics review 2026-08-27: PASS/0 blocking; AC-19 stays unchecked (ratified 1-line forbidden-test write); residuals CR-1 empty-lines write guard, CR-2 FileIO2 silent-failure promotion owed, CR-3 date-separator culture gap adjacent #645, PA-2 agent-memory paths outside AC-19"
metadata:
  type: project
---

quickfiler-home-controller-metrics-442 (issues #442/#443/#451, epic quickfiler-bug-family) reviewed 2026-08-27T14-35: 0 blocking, 24/25 AC PASS, AC-19 FAIL by design (parent-ratified one-line write to plan-forbidden `EfcHomeControllerTests.cs` — `bool`→`int` for the Interlocked guard field; AC-19/[P7-T6]/[P7-T27] deliberately unchecked, must stay unchecked).

**Why:** the deviation record + non-claim is the adjudicated state; re-flagging it as Blocking in later cycles would relitigate a settled ratification.

**How to apply:** residuals to check at epic fan-in / PR review:
- CR-1: `QfcHomeController.Metrics.cs:179` invokes `MetricsFileWriter` on an empty lines array (creates zero-content file); EFC guards `Length==0`, QFC does not.
- CR-2: `FileIO2.WriteTextFileAsync` swallows final IOException failure (`success=true` after 100 retries) — promotion to issue owed; module is deprecation-marked.
- CR-3: date/time separators (`MM/dd/yyyy`, `hh:mm`, `HH:mm:ss`) still CurrentCulture; recommend widening #645 (verified OPEN).
- PA-2: PR body should enumerate the two `.claude/agent-memory/orchestrator/**` diff paths beside the ratified deviation; the changed-file inventory excluded them by pathspec.
- Post-442 baseline of record: repo line 85.1255%, branch 79.2096%; changed-line 39/39; `QuickFileMetrics_WRITE` 88.37% (ten Interop lines, 39/49 unchanged both sides).
- Dual-verdict coverage rows (honest FAIL vs 85 floor + non-blocking disposition on 76.23%/80.00% modified files) passed the hook; artifact-absent C# row written FAIL/procedural per [[deletion-only PR with absent canonical C# artifact]] precedent.
