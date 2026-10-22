# QA Gate — New Test-File Budget (P6-T8, AC-24)

Timestamp: 2026-08-27T20-54

`BASELINE_SHA` = `4f238289090e4c97ca505511a5a73e8092dce0f9`.

## Command 1 — working-tree status of the test project

Command: `git status --porcelain -- QuickFiler.Test/`

EXIT_CODE: 0

Output, verbatim:

```
 M QuickFiler.Test/QuickFiler.Test.csproj
 A QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs
 M QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs
 M QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs
 M QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs
 M QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs
```

Six entries. Exactly **one** carries the `A` (added) status:
`QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs`. The other five are `M`
(modified) — the project file plus the four existing test files this feature extended additively.

## Command 2 — added files against the baseline

Command: `git diff --name-only --diff-filter=A 4f238289090e4c97ca505511a5a73e8092dce0f9 -- QuickFiler.Test/`

EXIT_CODE: 0

Output, verbatim — exactly one path:

```
QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs
```

## Why the single-commit diff form is used

This command uses the single-commit form `git diff <BASELINE_SHA> -- <path>`, which compares
`BASELINE_SHA` against the WORKING TREE. The two-dot form `BASELINE_SHA..HEAD` is prohibited here for the
same reason it was at P1-T8: this plan's first commit is P9-T4, so at this point `HEAD == BASELINE_SHA`
and the two-dot form would print nothing whatever the working tree holds — an unfailable gate.

The `--diff-filter=A` result depends on the new file having an index entry, which P4-T8 established with
`git add -N`. Without that entry the file would be untracked and the command would report nothing, which
would have produced a false PASS rather than a false FAIL.

## Result

| Requirement | Observed | Verdict |
| --- | --- | --- |
| exactly one added file under `QuickFiler.Test/` | 1 | SATISFIED |
| that file is `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs` | yes | SATISFIED |

No third new test file was added. The spec's test-file budget ruling holds: the four hub-side and
lifetime-side assertions were placed in existing files with stated headroom
(`BreadcrumbCoordinatorUpgradeLifetimeTests.cs`, `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`,
`BreadcrumbMessengerHubTests.cs`, `BreadcrumbSelectorCoordinatorTests.cs`), and only the #502
coordinator-level test — which had no home, since the three topically natural files sit at 488, 489 and
478 lines — went into a new file. That single new file is matched by exactly one `<Compile Include>` line
in `QuickFiler.Test/QuickFiler.Test.csproj` (verified at P6-T4: 1 added, 0 deleted).

Notably, when `BreadcrumbSelectorCoordinatorTests.cs` overshot the 500-line cap at 531 lines during
Phase 5, the remedy was in-place compaction rather than a new file, precisely so this budget would hold.
