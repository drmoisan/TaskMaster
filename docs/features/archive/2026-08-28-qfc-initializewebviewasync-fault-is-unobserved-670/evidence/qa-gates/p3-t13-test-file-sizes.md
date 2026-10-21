# P3-T13 — Post-format line counts of the two touched test files

Timestamp: 2026-09-01T20-09
Command: `foreach ($p in @('QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs','QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs')) { [pscustomobject]@{ Path = $p; Lines = (Get-Content -LiteralPath $p).Count } }`
EXIT_CODE: 0

## Measured counts against the P0-T8 baseline

| File | P0-T8 baseline | Post-format | Added | Ceiling | Holds |
| --- | --- | --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | 398 | 498 | +100 | 500 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` | 209 | 261 | +52 | 500 | yes |

Both files are at or below the 500-line ceiling. The counts are taken **after** the P3-T12 formatter run, which is the measurement that matters: the ceiling applies to the formatted file, and a count taken against an unformatted draft can differ from the committed shape in either direction.

## `Part3.cs` and the 100-line budget

`Part3.cs` lands at exactly 100 added lines, which is exactly the budget the plan's section 2 set against the file's 102 lines of headroom. This was not the first drafted result and the path to it is recorded here rather than presented as a clean first pass.

The first drafts of the three spec-named tests brought the file to **510** lines — over the 500-line ceiling by 10 and over the budget by 12. Formatting reduced that to 501, still one over the ceiling. The plan anticipated exactly this contingency and prescribed the remedy: *"if the drafted bodies exceed it, compact the XML documentation comments rather than relocating a test, because relocating one would falsify its acceptance criterion."*

That instruction was followed literally. Only XML documentation comments were compacted — three of them, across two rounds — and no test was relocated, renamed, or removed, and no assertion, arrange step or teardown was altered. Relocating any of the three would have falsified its acceptance criterion, because AC4, AC5 and AC6 each name `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` specifically.

The compaction preserved each test's documented intent, as `.claude/rules/general-unit-test.md` requires: every one of the three retains a summary naming the issue, the path under test, and the reason for its arrangement.

## `InitializationTests.cs`

The +52 lines are the shared arrange helper `BuildGuardedWebViewTarget` (P3-T1) and the fourth test `InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink` (P3-T9). Both were placed here rather than in `Part3.cs` precisely because `Part3.cs` had no room for them; at 261 lines this file retains 239 lines of headroom.

Placing them here required no `QuickFiler.Test/QuickFiler.Test.csproj` edit, because this file is the primary partial of the same `[TestClass] public partial class QfcItemController_InitializationTests` and already carries a `<Compile Include>` entry. That is what allows AC1's requirement that `QuickFiler.Test.csproj` be unchanged to hold.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
