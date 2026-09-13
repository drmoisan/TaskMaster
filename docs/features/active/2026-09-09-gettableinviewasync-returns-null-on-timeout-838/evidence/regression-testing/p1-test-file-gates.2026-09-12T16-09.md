# P1-T1 — New failure-contract test file created with the first test

Timestamp: 2026-09-13T02-49

Command: a single pwsh payload testing the file for existence and then applying the plan's fixed search-gate form, `@(Select-String -LiteralPath <file> -SimpleMatch -CaseSensitive -Pattern "<literal>").Count`, to five literals in `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncFailureContractTests.cs`, exiting 0 only when all five required values hold.

EXIT_CODE: 0

```
FILE_EXISTS=True
TESTMETHOD_COUNT=1
DONOTPARALLELIZE_COUNT=0
TEST1_COUNT=1
FACTORY_COUNT_ASSERT=1
TABLEREAD_COUNT_ASSERT=1
```

Output Summary: the file exists and carries exactly one `[TestMethod]`, no `[DoNotParallelize]`, one declaration of `GetTableInViewAsync_RunWithTimeoutExhaustsRetries_ThrowsTimeoutException`, one occurrence of `factoryInvocations.Should().Be(2` and one of `tableReadInvocations.Should().Be(0`. All five acceptance clauses hold.

The last two gates are mandatory rather than descriptive, because acceptance criterion 2 is the presence of those two count assertions and not the pass of the test that carries them; a test that omitted them would satisfy every other gate in this plan. Both literals were false before this task and are true after it, so neither needs a Phase 0 positive control, and each is written as a single statement with no reason argument so it stays on one line and the gate literal survives formatting.

The file mirrors `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs`: the reflective invocation helper has the shape that file uses at its lines 81 through 102, the parameter-type array has the same six elements as its lines 55 through 64, and the explorer builder has the shape of its lines 31 through 49. Reflection is required rather than preferred, because the return type involves an embedded interop type and a direct await from the test assembly is rejected with a compiler error. The injected factory returns a fresh source constructed through the parameterless constructor and cancelled explicitly, so no timed constructor and no timer call appears, which is what P4-T15 gates.
