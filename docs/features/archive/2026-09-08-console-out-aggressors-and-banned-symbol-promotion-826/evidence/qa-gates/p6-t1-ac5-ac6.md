# AC5 and AC6 verification against the anchored diff (issue #826, [P6-T1])

Timestamp: 2026-09-09T19-45

Command, run as one `pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard:

```
@(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "Console.WriteLine").Count
@(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "Console.").Count
@(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "timed out on try").Count
git diff --numstat $Base -- $Tac
git diff $Base -- $Tac
```

with the changed lines then partitioned in-process, and the catch-ordering derivation computed from
`Select-String` line numbers. No line number is carried from the plan document.

EXIT_CODE: 0

## Gate figures

| Measure | Observed | Required |
|---|---|---|
| `Console.WriteLine` | 0 | 0 |
| `Console.` | 1 | 1 |
| `timed out on try` | 2 | 2 |
| anchored numstat | 2 added, 2 removed | exactly 2 added, 2 removed |
| removed lines containing `Console.WriteLine` | 2 | 2 |
| added lines containing `logger.Warn` | 2 | 2 |
| total changed lines in the anchored diff | 4 | — |

The surviving single `Console.` token is `var target = writer ?? Console.Out;` in `EnumerateTable`, which
is the issue #811 seam, is out of scope, and must remain.

## Catch-ordering derivation

The eleven `catch (` clauses in the file sit at lines 88, 113, 159, 185, 209, 228, 235, 256, 304, 311 and
329. The two `timed out on try` diagnostics sit at lines 96 and 115. For each diagnostic the nearest
preceding `catch (` is:

| Diagnostic line | Nearest preceding `catch (` | Clause text |
|---|---|---|
| 96 | 88 | `catch (TaskCanceledException)` |
| 115 | 113 | `catch (TimeoutException)` |

So one added `logger.Warn` sits inside `catch (TaskCanceledException)` — specifically in the `else`
branch of its `token.IsCancellationRequested` test — and the other sits inside `catch (TimeoutException)`.
That is what AC6 requires.

## Full anchored diff

Anchored on base `dea7b49dae31a9bda8d35ecb73b8c8d646b1a460`:

```
diff --git a/UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs b/UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
index 333eebb2..a7d6e404 100644
--- a/UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
+++ b/UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
@@ -93,7 +93,7 @@ namespace UtilitiesCS
                 }
                 else
                 {
-                    Console.WriteLine($"Task timed out on try {counter}");
+                    logger.Warn($"{nameof(GetTableInViewAsync)} timed out on try {counter}");
                     if (counter < 2)
                     {
                         table = await activeExplorer.GetTableInViewAsync(
@@ -112,7 +112,7 @@ namespace UtilitiesCS
             }
             catch (TimeoutException)
             {
-                Console.WriteLine($"Task timed out on try {counter}");
+                logger.Warn($"{nameof(GetTableInViewAsync)} timed out on try {counter}");
                 if (counter < 2)
                 {
                     // The caller's timeoutMs is propagated rather than a literal, so both attempts
```

The anchored diff shows no other changed line in that file. The deadline window, the retry counter, the
`timeoutSourceFactory` seam, the caught exception types, the control flow after each diagnostic, every
other `catch` clause and every `using` directive are therefore provably untouched — including the
sibling-owned timeout mechanics that feature 825 owns.

The two `if (counter < 2)` context lines immediately below each substitution are unchanged, which is
direct evidence that the bounded-retry control flow after each diagnostic survived the edit.

Output Summary: `Console.WriteLine` is 0 in the table-access file, `Console.` is 1, `timed out on try` is
2, the anchored numstat is exactly 2 added and 2 removed, the two removed lines are the two
`Console.WriteLine` calls, the two added lines are the two `logger.Warn` calls, and the catch-ordering
derivation places one in each of the two required clauses. AC5 and AC6 are satisfied.
