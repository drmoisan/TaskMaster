# Phase 1 — Fail-Before Evidence for the Two New Regression Tests (issue #440, plan task P1-T4)

Timestamp: 2026-08-29T06-30

This task is tagged `[expect-fail]` in the plan. A failing run is the required
outcome for this task only.

Command:

```
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~LeftArrow_RepeatedOnThreeSegmentChain_WalksToRootThenReportsUnhandled|FullyQualifiedName~LeftArrow_WalkFromAnOpenLeafExpansion_ClearsTheExpansionAndStillReachesTheRoot" "/Logger:trx;LogFileName=p1-t4.trx" "/ResultsDirectory:coverage\trx\p1-t4"
```

EXIT_CODE: 1
ExpectedExitCode: 1

## Output Summary

Run summary, verbatim:

```
Total tests: 2
     Failed: 2
Test Run Failed.
```

The runner prints no `Passed:` line when the passed count is zero, so the passed
count is 0, derived from a total of 2 with 2 failures. The total is exactly 2, which
confirms the filter named the two methods precisely and matched no other test; the
still-passing tests in the same class could not contaminate this run.

- `FailBeforeTotalTests`: 2
- `FailBeforePassedTests`: 0
- `FailBeforeFailedTests`: 2

## Failing methods and the assertion that failed in each

### 1. `LeftArrow_RepeatedOnThreeSegmentChain_WalksToRootThenReportsUnhandled`

Failed at `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs`
line 162, which is the **second** Left press:

```
model.LeftArrow().Should().BeTrue();
```

Reported failure message, verbatim:

```
Expected model.LeftArrow() to be True, but found False.
```

The first Left press passes, because the row is still leaf-anchored and the pre-fix
guard's `activeIndex.Value == row.Chain.Count - 1` conjunct holds. The second press
fails, because after the first press the active index is 1 while `Chain.Count - 1` is
2, so the conjunct is false and control falls through to `TryCollapseLeaf()`, which
returns false. This is exactly the defect the spec describes.

### 2. `LeftArrow_WalkFromAnOpenLeafExpansion_ClearsTheExpansionAndStillReachesTheRoot`

Failed at the same file, line 189, which is likewise the **second** Left press:

```
model.LeftArrow().Should().BeTrue();
```

Reported failure message, verbatim:

```
Expected model.LeftArrow() to be True, but found False.
```

The Arrange `RightArrow()` and the first Left press both pass, so the failure isolates
the same one-step limit rather than any expansion-clearing behavior.

## Verdict

Both tests fail against the pre-fix tree for the intended reason, at the intended
press, with the intended assertion. Fail-before evidence is established. Paired with
the P3-T1 pass-after run this is the fail-before / pass-after evidence for AC-1 and
AC-8.

## Redaction note

The captured console output contained absolute worktree paths in the stack traces and
a third-party licensing banner carrying a vendor sales mailbox address. Neither is
reproduced here. The raw TRX remains under the gitignored `coverage/` tree at
`coverage\trx\p1-t4\p1-t4.trx` and is not copied under this feature folder.
