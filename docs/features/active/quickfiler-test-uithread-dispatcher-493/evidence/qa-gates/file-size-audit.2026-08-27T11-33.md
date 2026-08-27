# File Size Audit After the Final Formatter Pass (P4-T3)

Timestamp: 2026-08-27T11-33
Task: [P4-T3]
Command: `(Get-Content <path>).Count` for each of the four in-scope C# paths, run from `<repo-root>` after the `P3-T1` formatter pass
EXIT_CODE: 0
Output Summary: All four measured line counts are at or below the 500-line ceiling in
`.claude/rules/general-code-change.md` § File Size Limit. The largest is
`QfcItemController.TestSupport.cs` at 440 lines, leaving 60 lines of headroom. Every value is a fresh
measurement of the formatted tree, not a restatement of a projection.

## Measurements

| Repo-relative path | Measured lines | Ceiling | At or below |
| --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | 278 | 500 | **yes** |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | 346 | 500 | **yes** |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 440 | 500 | **yes** |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 393 | 500 | **yes** |

## Movement against the Phase 0 baseline

Baseline counts are the values `P0-T11` recorded in
`<FEATURE>/evidence/baseline/file-inventory-baseline.2026-08-27T10-18.md`.

| Path | Baseline | Final | Change | Headroom now |
| --- | --- | --- | --- | --- |
| `QfcItemController.UiThreadDispatcherFixture.cs` | (did not exist) | 278 | new | 222 |
| `QfcItemController.UiThreadDispatcherFixtureTests.cs` | (did not exist) | 346 | new | 154 |
| `QfcItemController.TestSupport.cs` | 489 | 440 | −49 | 60 |
| `QfcItemController.InitializationTests.Part2.cs` | 418 | 393 | −25 | 107 |

Both owned files shrank, so this change relieved rather than consumed headroom. That matters
specifically for `QfcItemController.TestSupport.cs`, which entered at 489 of 500 — 11 lines of
headroom, not the 135 lines research §8 projected from `main`, because sibling epic features have
since added shared arrange helpers to its tail. Per § Decisions Record D2 these are measurements,
not restatements of the research projections, which is the reason the divergence changes no gate.

The two new files were measured **after** the `P3-T1` `csharpier format` pass, so the counts include
the formatter's rewrapping. `QfcItemController.UiThreadDispatcherFixtureTests.cs` grew from 337 to
346 lines in that pass, which is recorded in
`<FEATURE>/evidence/qa-gates/csharpier-format.2026-08-27T11-08.md`.
