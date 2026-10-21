# Phase 6 — Final file-size verification, after formatting

Timestamp: 2026-09-09T14-08
Task: [P6-T13]

Run after the `[P6-T1]` `csharpier format .` pass, which is the last write to any `.cs` file in this
plan. `[P6-T2]` confirmed the formatter reached a fixed point at exit 0 with zero files needing
formatting, so no further reformatting can change these counts.

Command: the `[P0-T13]` line-count command, re-run unchanged.
EXIT_CODE: 0

Verbatim output:

```text
QuickFiler/Controllers/QfcHomeController.cs 500
QuickFiler/Controllers/EfcHomeController.cs 447
UtilitiesCS/Threading/ProgressViewer.cs 136
UtilitiesCS/Threading/ProgressPane.cs 107
QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs 192
QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs 495
UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs 486
UtilitiesCS.Test/Threading/ProgressPane_Tests.cs 328
```

## Acceptance check

| File | Baseline | Final | Plan budget | 500 ceiling | Headroom to 500 |
|---|---|---|---|---|---|
| `QuickFiler/Controllers/QfcHomeController.cs` | 498 | **500** | exactly 500 | **at the ceiling** | 0 |
| `QuickFiler/Controllers/EfcHomeController.cs` | 445 | **447** | 447 | under | 53 |
| `UtilitiesCS/Threading/ProgressViewer.cs` | 92 | **136** | at most 140 | under | 364 |
| `UtilitiesCS/Threading/ProgressPane.cs` | 61 | **107** | at most 115 | under | 393 |
| `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` | 159 | **192** | at most 200 | under | 308 |
| `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs` | 459 | **495** | at most 495 | under | 5 |
| `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs` | 352 | **486** | at most 499 | under | 14 |
| `UtilitiesCS.Test/Threading/ProgressPane_Tests.cs` | 192 | **328** | at most 350 | under | 172 |

**No file exceeds 500 lines**, and `QuickFiler/Controllers/QfcHomeController.cs` is at most 500 — it
sits at exactly 500, which `.claude/rules/general-code-change.md` line 49 permits because the rule
forbids *exceeding* 500. Every file is also within its own plan budget.

The three tightest files are `QfcHomeController.cs` at the ceiling with zero headroom,
`EfcHomeControllerLifecycleTests.cs` at 495 with 5 lines of headroom and exactly at its plan budget,
and `ProgressViewer_Tests.cs` at 486 with 14 lines of headroom.

`QfcHomeController.cs` held at exactly 500 through the formatting pass because the three inserted
statements are at most 94 characters including indentation, below CSharpier's 100-character print
width, so none was split into a continuation line. CSharpier did reflow two of the new test bodies —
`EfcHomeControllerLifecycleTests.cs` rose from 492 to 495 and `ProgressPane_Tests.cs` from 324 to
328 — and both remained within budget.

No file was split to meet a budget. Splitting is forbidden: a ninth file would require a `.csproj`
`Compile Include` entry and would falsify AC19 and AC20. Where the first draft of
`ProgressViewer_Tests.cs` overshot at 536 lines, it was brought within budget by factoring repeated
scaffolding into shared private helpers, with no assertion removed or weakened;
`[P4-T13]` records that decision in full.

Output Summary: all eight Write Set files are within their plan budgets and **no file exceeds 500
lines**. `QuickFiler/Controllers/QfcHomeController.cs` is at exactly 500, at most 500 as required.
