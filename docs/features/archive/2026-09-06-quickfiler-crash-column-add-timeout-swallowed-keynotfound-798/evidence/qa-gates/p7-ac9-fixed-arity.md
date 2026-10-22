# Phase 7 — AC9 fixed-arity constraint audit

Timestamp: 2026-09-07T03-21
Task: [P7-T7]
Issue: #798

Host-specific absolute paths are redacted to `<repo-root>`, `<user>` and `<host>` tokens. All
commands were executed with the working directory set to `<repo-root>`, anchored to base commit
c431dc32, against HEAD 4a29d7e79112fcaf359110c16c6933d9819165fa.

## Clause 1 — `UtilitiesCS.Test/Extensions/DfDeedle_Tests.cs` is untouched in both states

1. Command: git diff c431dc32 -- UtilitiesCS.Test/Extensions/DfDeedle_Tests.cs
   EXIT_CODE: 0
   Output: none (zero output lines).

2. Command: git status --porcelain --untracked-files=all -- UtilitiesCS.Test/Extensions/DfDeedle_Tests.cs
   EXIT_CODE: 0
   Output: none (zero output lines).

Verdict: PASS. The file is byte-identical to the base commit in the committed state and carries no
unstaged or untracked edit in the working state. Its reflective invocation of `Email2dArrayToDf` with
a three-element argument array is therefore unchanged, which is the constraint AC9 pins: the method's
arity is fixed, so a reflective call site written against three parameters still binds.

## Clause 2 — evidence source

Pass statuses below are read from the P6-T5 test run, which is the last run of the three affected
assemblies before this phase. No Phase 8 task is used as the evidence source for this Phase 7 gate.
P8-T3 in particular is the nullable build and produces no test result at all.

Source artifact:
`docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/regression-testing/p6-ac5-pass-after.md`

Underlying results file, queried directly for the two named tests:
`<repo-root>\coverage\trx\p6-ac5\<user>_<host>_2026-09-07_03_13_34_net481.trx`

Run-level counters read from that results file:

```
TOTAL=6570 PASSED=6570 FAILED=0 NOTEXECUTED=0
```

`NOTEXECUTED=0` establishes that neither test was filtered out or skipped by the run's
`/TestCaseFilter`, so the pass status below is a positive observation rather than an inference from
an aggregate.

## Clause 3 — pass status for the two named tests

| Fully-qualified test | Outcome | Duration | Role |
|---|---|---|---|
| `UtilitiesCS.Test.Extensions.DfDeedle_Tests.Email2dArrayToDf_ViaReflection_ValidData_ReturnsFrame` | Passed | 00:00:00.0230668 | Invokes `Email2dArrayToDf` reflectively with a three-element argument array |
| `UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.GetEmailDataFromTable_OneRow_ReturnsFrameWithExpectedFields` | Passed | 00:00:00.0327749 | Calls `GetEmailDataFromTable` directly |

Declaring classes were resolved from the results file's test definitions rather than assumed:
`Email2dArrayToDf_ViaReflection_ValidData_ReturnsFrame` resolves to
`className=UtilitiesCS.Test.Extensions.DfDeedle_Tests`, and
`GetEmailDataFromTable_OneRow_ReturnsFrameWithExpectedFields` resolves to
`className=UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests`.

Verdict: PASS on both.

## Conclusion

AC9 holds. The reflective test file is unmodified in both the committed and the working state, the
reflective three-argument call site still binds against `Email2dArrayToDf`, and the direct call site
into `GetEmailDataFromTable` still binds. Both tests passed in the P6-T5 run.

Output Summary: AC9 PASS. `git diff c431dc32` and `git status --porcelain --untracked-files=all`
against `UtilitiesCS.Test/Extensions/DfDeedle_Tests.cs` each produce zero output lines, so the file
is untouched in both states. Reading from the P6-T5 run, which reported
`TOTAL=6570 PASSED=6570 FAILED=0 NOTEXECUTED=0`,
`UtilitiesCS.Test.Extensions.DfDeedle_Tests.Email2dArrayToDf_ViaReflection_ValidData_ReturnsFrame`
and
`UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.GetEmailDataFromTable_OneRow_ReturnsFrameWithExpectedFields`
both record outcome `Passed`. The fixed-arity constraint on `Email2dArrayToDf` is preserved.
