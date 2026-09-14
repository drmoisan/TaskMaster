# P0-T17 — Pre-change line counts of the touched C# files (baseline)

Timestamp: 2026-09-13T23-11

Command: `(Get-Content -LiteralPath <path>).Count` for each of the three paths below.

EXIT_CODE: 0

Output Summary:

| File | Recorded count | Expected | Headroom to the 500-line limit |
|---|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | **293** | 293 | 207 |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | **458** | 458 | 42 |
| `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` | **460** | 460 | 40 |

Every recorded count equals its expected value, so the FAIL condition is not met. The write-set
sizing in this plan and in the specification is derived from these three numbers: the thin headroom
on the two test files is why the two negative predicate cases and the measurement go into one new
file rather than into either existing one, and why the positive twin in `UiThread_Tests.cs` is
bounded at 36 source lines by P1-T5.

`UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs` has no pre-change count because
it does not yet exist; P1-T1 creates it. Its post-change count is recorded by P4-T13 alongside the
post-change counts of these three files.
