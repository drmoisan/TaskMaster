# QA Gate — The #462 Must-Pass Test Files Were Not Edited (P1-T8)

Timestamp: 2026-08-27T20-18

`BASELINE_SHA` = `4f238289090e4c97ca505511a5a73e8092dce0f9` (recorded by P0-T3).

## Why the single-commit diff form is used

Both commands below use the single-commit form `git diff --numstat BASELINE_SHA -- <path>`, which
compares `BASELINE_SHA` against the WORKING TREE. The two-dot form `BASELINE_SHA..HEAD` is prohibited
here: this plan's first commit is P9-T4, so at this point `HEAD == BASELINE_SHA` and the two-dot form
would print nothing whatever the working tree holds — a gate that cannot fail. P6-T8 uses the same
single-commit form for the same reason.

## Command 1

Command: `git diff --numstat 4f238289090e4c97ca505511a5a73e8092dce0f9 -- QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs`
EXIT_CODE: 0
Output: **empty** — `git diff --numstat` emits no row for an unchanged path.

Interpretation: 0 added lines and 0 deleted lines. The file that owns
`PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose` (AC-20) and the shared
`CoordinatorHarness` / `ControlledHost` helpers is byte-identical to `BASELINE_SHA`.

Required bound: 0 added and 0 deleted. Observed: 0 and 0. **SATISFIED.**

## Command 2

Command: `git diff --numstat 4f238289090e4c97ca505511a5a73e8092dce0f9 -- QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`
EXIT_CODE: 0
Output:

```
74	0	QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs
```

Interpretation: 74 added lines, **0 deleted lines**. Every change to this file is additive: the two
test methods authored by P1-T1 and P1-T3. No existing line was edited or removed, so
`SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired` (AC-21) at its original position is
untouched.

Required bound: 0 deleted lines. Observed: 0. **SATISFIED.**

Both results satisfy their stated bounds. Corroborating behavioural evidence: P1-T7 ran both named
tests as part of a 48-test surface and both passed.
