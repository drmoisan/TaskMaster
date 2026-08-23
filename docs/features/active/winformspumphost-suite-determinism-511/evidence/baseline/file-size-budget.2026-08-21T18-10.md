# Baseline — File-Size Budget of the Three Touched Files

Timestamp: 2026-08-22T09-15

Command:

```
pwsh -NoProfile -Command "@('QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs','QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs','QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs') | ForEach-Object { $c = @(Get-Content -LiteralPath $_); Write-Output ('{0} = {1}' -f $_, $c.Count) }"
```

Run from the worktree root
`<repo-root>\.claude\worktrees\agent-ad37a256a0fb60243`. `Get-Content
-LiteralPath` was invoked once per file and the returned lines were counted, as the task specifies.

EXIT_CODE: 0

Output Summary:

| File | Pre-change line count | Plan-expected | Match | Headroom to 500 |
| --- | --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 409 | 409 | yes | 91 |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 467 | 467 | yes | 33 |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | 290 | 290 | yes | 210 |

All three recorded counts equal the values the task's acceptance condition names (409, 467, 290).
**No drift.** No count required verbatim recording as a deviation.

Two files named in Binding Constraint 5 as off-limits for additions were not measured here because
the task does not ask for them, and neither is touched by this child:
`QuickFiler.Test/Controllers/WinFormsPumpHostTests.cs` (443 per the plan) and
`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` (497 per the plan, three lines
of headroom).
