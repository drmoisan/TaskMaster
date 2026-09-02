# Production Diff Scope Verification (P1-T6)

Timestamp: 2026-09-01T12-40

Command: `git diff origin/main -- QuickFiler/Controllers/QfcHomeController.Metrics.cs`
EXIT_CODE: 0
`origin/main` = `8996b28746d32f9f5996a037e0ca76be78b7684d`

## Verbatim Diff

```
diff --git a/QuickFiler/Controllers/QfcHomeController.Metrics.cs b/QuickFiler/Controllers/QfcHomeController.Metrics.cs
index df2bf484..38d33fda 100644
--- a/QuickFiler/Controllers/QfcHomeController.Metrics.cs
+++ b/QuickFiler/Controllers/QfcHomeController.Metrics.cs
@@ -172,6 +172,10 @@ namespace QuickFiler.Controllers
             // no XML documentation and therefore no non-null element guarantee, so this filter
             // defends the interface contract rather than a known producer defect.
             var lines = strOutput.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
+            if (lines.Length == 0)
+            {
+                return;
+            }
 
             // CancellationToken.None, never the session Token: the dispatcher continuation that
             // carries this write is not awaited to completion, so a session cancellation can be
```

Command: `git diff --numstat origin/main -- QuickFiler/Controllers/QfcHomeController.Metrics.cs`
Output: `4	0	QuickFiler/Controllers/QfcHomeController.Metrics.cs`
(4 insertions, 0 deletions)

Command: `git diff -U0 origin/main -- QuickFiler/Controllers/QfcHomeController.Metrics.cs | grep "^@@"`
Output: `@@ -174,0 +175,4 @@ namespace QuickFiler.Controllers`
(exactly one hunk, a pure insertion of 4 lines after old line 174)

Command: `git diff origin/main -- QuickFiler/Controllers/QfcHomeController.Metrics.cs | grep -c "^-[^-]"`
Output: `0` (zero removed content lines)

## Acceptance — All Three Conditions

| # | Condition | Observed | Met |
|---|---|---|---|
| 1 | Zero removed (`-`) lines appear in the diff | `git diff --numstat` reports `0` deletions; the removed-content-line count is `0` | Yes |
| 2 | The only added (`+`) lines are the four guard lines from P1-T5 | `git diff --numstat` reports exactly `4` insertions, and the diff body shows them to be `if (lines.Length == 0)`, `{`, `return;`, `}` and nothing else | Yes |
| 3 | No hunk touches the `MetricsFileWriter` property declaration or the `if (!metricsWritten)` block | The single hunk spans old lines 172-177 only; see the span analysis below | Yes |

ACCEPTANCE: MET.

## Hunk Span Analysis for Condition 3

The diff contains exactly one hunk, `@@ -172,6 +172,10 @@`, covering old lines 172 through
177 inclusive. The two regions the plan's Hard Scope Boundary 1 places off-limits both sit
outside that span:

| Protected region (issue #647's delivered outcome) | Lines before change | Inside hunk span 172-177 |
|---|---|---|
| `MetricsFileWriter` delegate declaration, `Func<string, string[], string, CancellationToken, Task<bool>>` | 28-34 | No — 144 lines above the hunk |
| `if (!metricsWritten)` failure-logging branch | 185-191 | No — 8 lines below the hunk |

Both regions were additionally re-read directly in the post-change file and are byte-for-byte
unchanged. The declaration still reads, at lines 28-34:

```
        internal Func<
            string,
            string[],
            string,
            CancellationToken,
            Task<bool>
        > MetricsFileWriter { get; set; } = FileIO2.WriteTextFileAsync;
```

The `Task<bool>` return type is intact and the failure branch is intact; this change neither
altered the delegate signature nor the failure-handling branch. This is the evidence backing
AC6.

## Post-Fix Line Numbers

Re-derived directly from the changed file:

| Element | Pre-fix | Post-fix | Predicted by P1-T1 |
|---|---|---|---|
| Anchor A — `var lines = strOutput.Where(...)` | 174 | 174 | 174 (correct) |
| `if (lines.Length == 0)` | — | **175** | 176 (off by one) |
| `{` | — | **176** | 177 (off by one) |
| `return;` | — | **177** | 178 (off by one) |
| `}` | — | **178** | 179 (off by one) |
| `CancellationToken.None` comment | 176-178 | 180-182 | 180-182 (correct) |
| Anchor B — `bool metricsWritten = await MetricsFileWriter(` | 179 | **183** | 183 (correct) |
| `if (!metricsWritten)` | 185 | **189** | 189 (correct) |

**The four guard lines are 175, 176, 177, 178.** These are the line numbers P2-T7 uses to
locate the guard's per-line `hits` in the final Cobertura report.

P1-T1 predicted the guard at 176-179 on the assumption that the pre-existing blank line at
old 175 would be kept *above* the guard. It was placed *below* the guard instead, so the
guard occupies 175-178 and everything from the comment block onward lands exactly where
P1-T1 predicted. The placement decision is explained next; it is why re-deriving the numbers
after the edit, rather than trusting the prediction, was required.

## Why the Guard Hugs Anchor A

AC2 requires the guard to be "textually equivalent to the guard already present in
`QuickFiler/Controllers/EfcHomeController.Metrics.cs`". The EFC guard at lines 72-75 of that
file follows its computing statement (`var dataLines = BuildQuickFileMetricLines(...)`,
ending line 71) with **no blank line between them**, and is followed by a blank line before
the next statement. The QFC guard is placed the same way: directly after Anchor A, with the
file's pre-existing blank line now serving as the separator between the guard and the
`CancellationToken.None` comment block.

This placement has two consequences that both favor it:

1. It mirrors the EFC form structurally as well as textually, which is what AC2 asks for and
   what the issue's "Suspected Cause" section identifies as the asymmetry being closed.
2. It reuses the existing blank line rather than adding a new one, so the diff is exactly
   four added lines. Placing the guard below the blank line instead would have added a fifth,
   whitespace-only line and put the diff at 5 insertions rather than the 4 this task's
   acceptance condition specifies.

The `CancellationToken.None` comment remains adjacent to the `await MetricsFileWriter(...)`
statement it explains, as P1-T5 requires — nothing was interposed between that comment and
its statement.
