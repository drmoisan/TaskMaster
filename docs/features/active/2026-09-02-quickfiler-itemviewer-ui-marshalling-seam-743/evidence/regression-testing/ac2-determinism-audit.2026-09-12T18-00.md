# AC2 determinism audit over the seam test file (P3-T7)

Task: [P3-T7]
Both commands were run from the item worktree root via Set-Location inside one pwsh invocation; inner quoting of the plan spans was inverted to single quotes where wrapped, semantics identical. In the regular expression `|` is alternation; a literal pipe would be written `\|` and none is intended.

## Command 1 — banned-construct match list

Timestamp: 2026-09-13T03-29
Command: `pwsh -Command 'Select-String -Path QuickFiler.Test\Controllers\QfcItemController.SeamMarshallingTests.cs -Pattern "Thread\.Sleep|Task\.Delay|Stopwatch|DateTime\.Now|DateTime\.UtcNow|Environment\.TickCount|\bwhile\b" | ForEach-Object { $_.LineNumber.ToString() + ": " + $_.Line.Trim() }'`
EXIT_CODE: 0
Output Summary: the match list is empty (no line printed).

Verbatim match list:

```
(empty)
```

## Command 2 — timeout-attribute count

Timestamp: 2026-09-13T03-29
Command: `pwsh -Command 'Select-String -Path QuickFiler.Test\Controllers\QfcItemController.SeamMarshallingTests.cs -SimpleMatch -Pattern "[Timeout(" | Measure-Object | Select-Object -ExpandProperty Count'`
EXIT_CODE: 0
Output Summary: `5`

## Structural-assertion statement

Each of the five tests asserts a structural property and none asserts an elapsed duration: test 1 asserts that the viewer object is not assignable to the concrete viewer type and that the item-number tip was built; test 2 asserts the two tip-detail collection counts, the two non-empty control groups and a single `DescendantControls()` call; test 3 asserts by reflection the first parameter type of the member; test 4 asserts `OperationCanceledException`; test 5 asserts exactly one `InvokeAsync(Action)` call on the injected dispatcher double and one `ItemNumberText` assignment. The single `[Timeout(SeamTimeoutMs)]` on each test converts a genuine deadlock into a failure and is not an assertion over time. Every `await` in the tests completes inline through the ambient-context reference-equality branch of the UtilitiesCS awaiter, so no wait, poll, sleep, retry or wall-clock read exists in the file (the `for` loop in `BuildHostedLabels` is a fixed-count constructor loop, not a polling loop).
