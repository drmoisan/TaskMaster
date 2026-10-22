# [P4-T6] Coverage comparison and changed-file enumeration

Timestamp: 2026-09-06T01-55

Command:

```powershell
git status --porcelain --untracked-files=all -- '*.cs'
```

The four counters on each side are read from the key lines of the two artifacts themselves —
`evidence/remediation-baseline/r-p0-t10-tests-coverage.md` for the baseline side and
`evidence/qa-gates/r-p4-t5-tests-coverage.md` for the final side — rather than re-derived here, so
this task compares what those artifacts record.

EXIT_CODE: 0

Output Summary: the denominators are equal on both sides, neither covered counter decreased, and the
changed-`.cs` enumeration lists exactly the two test files this remediation edits.

## Counter comparison

| Counter | Baseline ([P0-T10]) | Final ([P4-T5]) | Relation | Required |
|---|---|---|---|---|
| lines covered | 112351 | 112351 | equal | final >= baseline |
| lines valid | 132961 | 132961 | equal | equal |
| branches covered | 26498 | 26498 | equal | final >= baseline |
| branches valid | 33480 | 33480 | equal | equal |

`FINAL-LINES-VALID` equals `BASELINE-LINES-VALID` and `FINAL-BRANCHES-VALID` equals
`BASELINE-BRANCHES-VALID`, so the two sides share a denominator and are comparable.
`FINAL-LINES-COVERED` is greater than or equal to `BASELINE-LINES-COVERED`, and
`FINAL-BRANCHES-COVERED` is greater than or equal to `BASELINE-BRANCHES-COVERED`. All four
acceptance relations hold.

Because the denominators are equal, the fallback clause in the task text — recording line and branch
percentages for both sides and comparing on those instead — is not reached and is not used.

The equality across all four counters is the expected outcome and not a coincidence. This remediation
changes no production code at all, and the two files it does change are test files that the derived
coverage configuration excludes from measurement.

## Changed `.cs` enumeration

```text
 M UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs
 M UtilitiesCS.Test/Threading/UiThread_Tests.cs
```

Exactly two paths, both under `UtilitiesCS.Test`. No other `.cs` path is listed, modified, staged, or
untracked.

## Consequences recorded

- **No production `.cs` file is changed by this remediation.** The only production file it touched at
  any point was `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, mutated temporarily by
  [P1-T5] and reverted by [P1-T8], whose artifact records three independent checks that the revert is
  complete. That file does not appear in the enumeration above.
- **Changed-line coverage is NOT APPLICABLE for this remediation.** The metric applies to changed
  production lines, and there are none.
- **Both changed files are excluded from the coverage denominator** by the derived configuration's
  `<ModulePath>.*\.Test\.dll$</ModulePath>` exclusion, which removes every `*.Test.dll` module from
  measurement. A change confined to those two files therefore cannot move any first-party counter,
  which is what the table above shows.

## Why the porcelain enumeration is used here rather than a diff

At the time this task runs, the two edits are uncommitted. An anchored `git diff --name-only` sees
tracked committed changes and would report them, but it cannot see an untracked path, and the
remediation creates many untracked evidence files. The porcelain status sees both. Its complementary
weakness is that it goes empty once the change is committed, which is why [P5-T5] repeats the
enumeration after the commit using an anchored diff against the `REMEDIATION-BASE-SHA` recorded in
[P0-T11]. Both are required; neither alone is correct in both states.
