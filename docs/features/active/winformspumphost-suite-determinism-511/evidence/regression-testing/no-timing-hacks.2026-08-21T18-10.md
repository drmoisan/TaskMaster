# P3-T8 — No Prohibited Timing Construct in the Added Lines

Timestamp: 2026-08-22T10-39

Command:
```
git diff --unified=0 c551eabab0aa0a6b1a284252811a2e1de819634e -- `
  QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs `
  QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs `
  QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs > coverage/p3-t8.diff

grep '^+[^+]' coverage/p3-t8.diff > coverage/p3-t8-added.txt
grep -cF 'Thread.Sleep'   coverage/p3-t8-added.txt
grep -cF 'Task.Delay'     coverage/p3-t8-added.txt
grep -cF 'SpinWait'       coverage/p3-t8-added.txt
grep -cF 'PumpTimeoutMs =' coverage/p3-t8-added.txt
```

The merge-base form `git diff <merge-base>` is used rather than `<merge-base>..HEAD`, because the
merge base currently equals `HEAD` (nothing is committed on this branch yet) and the two-dot form
would compare a commit with itself and see none of the edits.

EXIT_CODE: 0

Output Summary:

Diff scope: 109 added lines, **0** removed lines, across the three touched test files. The change is
purely additive.

| Literal | Count in added lines | Required |
| --- | --- | --- |
| `Thread.Sleep` | **0** | exactly 0 |
| `Task.Delay` | **0** | exactly 0 |
| `SpinWait` | **0** | exactly 0 |
| `PumpTimeoutMs =` | **0** | exactly 0 |

## Timeout constants retain their current values (file inspection)

| Constant | File | Line | Value |
| --- | --- | --- | --- |
| `PumpTimeoutMs` | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` | 38 | `internal const int PumpTimeoutMs = 60000;` |
| `PumpTimeoutMs` | `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 34 | `private const int PumpTimeoutMs = 60000;` |
| `PumpTimeoutMs` | `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` | 293 | `private const int PumpTimeoutMs = 60000;` |
| `TimeoutMs` | `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` | 24 | `private const int TimeoutMs = 30000;` |

None of these four declarations is in the diff. `QfcItemController.InitializationTests.cs`,
`QfcItemController.SeamFactoryTests.cs` and `WinFormsPumpHostTests.cs` are not touched by this
change at all; `QfcItemController.ViewerSetupTests.cs` is touched, but only at lines 436-438, and
its `PumpTimeoutMs` declaration at line 34 is unchanged.

Path note: `WinFormsPumpHostTests.cs` resides at `QuickFiler.Test/TestSupport/`, not under
`QuickFiler.Test/Controllers/`. Its line count is 443, matching the plan's do-not-touch budget entry,
and it shows a zero diff.

Acceptance: all four counts are exactly 0 and all four timeout constants retain their current values.
No sleep, retry, `SpinWait`, or raised timeout constant was introduced anywhere in this change.
