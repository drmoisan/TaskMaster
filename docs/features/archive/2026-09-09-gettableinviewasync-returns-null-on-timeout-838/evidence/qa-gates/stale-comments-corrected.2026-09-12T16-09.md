# P4-T13 — Both stale prose comments are corrected

Timestamp: 2026-09-13T03-20

Command: the plan's fixed search-gate form applied to the literal `pre-existing latent` in `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` and to the literal `making the returned table null` in `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs`.

EXIT_CODE: 0

```
LATENT_COUNT=0
MAKING_NULL_COUNT=0
```

Output Summary: both acceptance clauses hold, both counts being exactly 0. P0-T21 recorded a pre-change count of 1 for each literal in its named carrying file through this same search form, so both gates have a demonstrated non-zero before-state.

Both searches are scoped to explicit file paths, which is mandatory rather than tidy: both phrases also occur in `spec.md` inside acceptance criterion 9's own text and in the plan file, so an unscoped search for either could never reach zero and the gate would be unsatisfiable however correct the source.

The two comments described behaviour the change removes. The production comment called the null-on-cancellation-or-timeout path a pre-existing latent condition; it is replaced by a comment stating that a null local means the shared helper absorbed its own retry budget and that the failure has to be reported here, which is what the guard below it now does. The test comment stated that advancing the fake clock past the budget would make the returned table null; it now states that doing so would surface a `TimeoutException` from the acquisition, which is why the clock is never advanced. P3-T5 also established that the comment correction touched no assertion: the clock test file still carries exactly the 10 lines containing `.Should()` that P0-T22 recorded.
