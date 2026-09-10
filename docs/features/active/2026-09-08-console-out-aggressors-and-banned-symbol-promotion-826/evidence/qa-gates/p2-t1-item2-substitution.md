# Item-2 logger substitution (issue #826, [P2-T1])

Timestamp: 2026-09-09T19-13

Command: the two statement substitutions were made with the Edit tool, located by literal statement text
and enclosing catch clause and not by line number, then the following ran as one
`pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard:

```
& $dotnet tool run csharpier format UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
@(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "Console.WriteLine").Count
@(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "Console.").Count
@(Select-String -LiteralPath $Tac -CaseSensitive -SimpleMatch "timed out on try").Count
git diff --numstat $Base -- $Tac
```

EXIT_CODE: 0 (csharpier `format` reported `Formatted 1 files in 1122ms.` and exited 0)

## Edit performed

Both statements whose literal text was `Console.WriteLine($"Task timed out on try {counter}");` were
replaced with `logger.Warn($"{nameof(GetTableInViewAsync)} timed out on try {counter}");`. One sits in
the `else` branch of `catch (TaskCanceledException)`; the other sits in `catch (TimeoutException)`. No
new field and no new `using` was added: `logger` is the `log4net.ILog` declared in the sibling partial
`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs` of the same `public static partial class
OlTableExtensions`.

Nothing else was changed. No catch clause was added, removed or widened; the deadline window, the retry
counter, the `timeoutSourceFactory` seam, the caught exception types and the control flow after each
diagnostic are all exactly as found.

## Gate figures

| `-SimpleMatch` token | Observed | Required |
|---|---|---|
| `Console.WriteLine` | 0 | 0 |
| `Console.` | 1 | 1 |
| `timed out on try` | 2 | 2 |

The surviving single `Console.` token is the `var target = writer ?? Console.Out;` seam in
`EnumerateTable`, which is out of scope and must remain.

Anchored numstat against base `dea7b49dae31a9bda8d35ecb73b8c8d646b1a460`:

```
2	2	UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
```

Exactly 2 added and 2 removed lines, measured after the csharpier pass, so the substituted statements
were not wrapped by the formatter and the numstat is the post-format figure.

## Anchored diff

```
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

The second hunk's context lines show the substituted statement immediately follows
`catch (TimeoutException)`, and the first hunk's context lines show the other immediately follows the
`else` of the cancellation clause.

Output Summary: both diagnostics now leave `GetTableInViewAsync` through the log4net `logger` at `Warn`
level. All four acceptance figures hold: `Console.WriteLine` 0, `Console.` 1, `timed out on try` 2, and
an anchored numstat of exactly 2 added and 2 removed.
