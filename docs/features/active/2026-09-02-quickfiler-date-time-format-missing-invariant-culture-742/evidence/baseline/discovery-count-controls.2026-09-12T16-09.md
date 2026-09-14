# Baseline Discovery-Count Controls (issue #742, [P0-T9])

Timestamp: 2026-09-14T02-05

All eleven commands below were run against the unmodified tree, before [P1-T1] created or modified
any Write Set file. Branch HEAD at capture time: `6d6da116f9084db5da0b2f25d80941c41c5fbf61`.

Zero-match reading convention: `git grep -c PATTERN -- path` prints no line at all, and exits `1`,
when `path` has zero matching lines; it never prints a `0`.

Every command below was executed with `git -C <worktree>` from the item worktree
`C:/Users/DanMoisan/repos/TaskMaster-wt/bugs-2026-09-11-item-742`. The `-C` form is an invocation
detail of this execution environment and does not change any pattern or pathspec.

---

## Control 1 — uncultured `ToString` sweep across the five production files

Command: `git grep -c '[.]ToString[(]@\?"[^"]*[:/.\-][^"]*"[)]' -- QuickFiler/Controllers/QfcHomeController.Metrics.cs QuickFiler/Controllers/EfcHomeController.Metrics.cs QuickFiler/Controllers/QfcItemController.ViewerSetup.cs QuickFiler/Controllers/QfcCollectionController.cs QuickFiler/Controllers/EfcItemController.cs`

EXIT_CODE: 0

Output Summary:

```
QuickFiler/Controllers/EfcHomeController.Metrics.cs:4
QuickFiler/Controllers/EfcItemController.cs:2
QuickFiler/Controllers/QfcCollectionController.cs:3
QuickFiler/Controllers/QfcHomeController.Metrics.cs:4
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:1
```

Per-file distribution 4 / 4 / 1 / 3 / 2 (14 matching lines total), exactly as the plan and
`spec.md` state. The `@\?` escape is load-bearing: `git grep` runs in basic-regular-expression mode,
in which a bare `?` is a literal character rather than an optional-atom quantifier.

## Control 2 — interpolated specifier token

Command: `git grep -c '[{]now:' -- QuickFiler/Controllers/QfcHomeController.Metrics.cs`

EXIT_CODE: 0

Output Summary: `QuickFiler/Controllers/QfcHomeController.Metrics.cs:1` — expected 1.

## Control 3 — pre-existing InvariantCulture sites, QfcHomeController.Metrics.cs

Command: `git grep -c 'CultureInfo[.]InvariantCulture' -- QuickFiler/Controllers/QfcHomeController.Metrics.cs`

EXIT_CODE: 0

Output Summary: `QuickFiler/Controllers/QfcHomeController.Metrics.cs:4` — expected 4.

## Control 4 — pre-existing InvariantCulture sites, EfcHomeController.Metrics.cs

Command: `git grep -c 'CultureInfo[.]InvariantCulture' -- QuickFiler/Controllers/EfcHomeController.Metrics.cs`

EXIT_CODE: 0

Output Summary: `QuickFiler/Controllers/EfcHomeController.Metrics.cs:2` — expected 2.

## Control 5 — `Compile Include` count in the test project file

Command: `git grep -c -F 'Compile Include' -- QuickFiler.Test/QuickFiler.Test.csproj`

EXIT_CODE: 0

Output Summary: `QuickFiler.Test/QuickFiler.Test.csproj:176` — expected 176 per the coordinator
amendment dated 2026-09-14, which re-derived this figure after items 871, 873 and 877 each added an
unrelated `<Compile Include>` entry to this file. The plan body's original figure of 172 is
superseded.

## Control 6 — new test file not yet referenced by the project file

Command: `git grep -c -F 'QuickFilerInvariantCultureIssue742Tests' -- QuickFiler.Test/QuickFiler.Test.csproj`

EXIT_CODE: 1

Output Summary: prints no line — expected: prints no line, exits 1.

## Control 7 — no `System.Globalization` using directive, QfcItemController.ViewerSetup.cs

Command: `git grep -c -F 'using System.Globalization;' -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`

EXIT_CODE: 1

Output Summary: prints no line — expected: prints no line, exits 1.

## Control 8 — no `System.Globalization` using directive, QfcCollectionController.cs

Command: `git grep -c -F 'using System.Globalization;' -- QuickFiler/Controllers/QfcCollectionController.cs`

EXIT_CODE: 1

Output Summary: prints no line — expected: prints no line, exits 1.

## Control 9 — no `System.Globalization` using directive, EfcItemController.cs

Command: `git grep -c -F 'using System.Globalization;' -- QuickFiler/Controllers/EfcItemController.cs`

EXIT_CODE: 1

Output Summary: prints no line — expected: prints no line, exits 1.

## Control 10 — self-referential uncultured test oracle, both occurrences

Command: `git grep -c -F 'expectedLocal.ToString("MM/dd/yyyy") + "," + expectedLocal.ToString("HH:mm") + ",";' -- QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`

EXIT_CODE: 0

Output Summary: `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:2` — expected 2, the
identical expected-value construction in the two sibling test methods.

## Control 11 — no InvariantCulture in the metrics test file

Command: `git grep -c -F 'CultureInfo.InvariantCulture' -- QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`

EXIT_CODE: 1

Output Summary: prints no line — expected: prints no line, exits 1.

---

Acceptance: this artifact exists, records all eleven commands with `Timestamp:`, `Command:`,
`EXIT_CODE:` and `Output Summary:`, every observed figure equals the figure stated by the task
(with control 5 read against the coordinator's amended value of 176), and it was written before
[P1-T1], which is the first task in this plan that creates or modifies any Write Set file.

This artifact is the baseline evidence artifact required by the Acceptance Criteria item covering
the pre-fix discovery-count figures.
