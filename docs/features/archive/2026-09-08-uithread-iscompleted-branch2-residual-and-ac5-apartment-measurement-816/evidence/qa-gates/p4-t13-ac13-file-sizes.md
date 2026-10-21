# P4-T13 — AC13 file-size projection

Timestamp: 2026-09-13T23-50

Command: `(Get-Content -LiteralPath <path>).Count` for each of the four paths below, measured
**after the final formatting pass** (P4-T2 and P4-T3), so the counts are the ones a reviewer will
see.

EXIT_CODE: 0

Output Summary:

| File | Pre-change (P0-T17) | Post-change | Headroom to 500 | At or below 500 |
|---|---|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | 293 | **306** | 194 | yes |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | 458 | **493** | 7 | yes |
| `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` | 460 | **464** | 36 | yes |
| `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs` | n/a (created by this delivery) | **164** | 336 | yes |

No recorded post-change count exceeds 500, so the FAIL condition is not met.

Per-file deltas:

- `UiThread.cs` grew by 13 lines: the seven-line explanatory comment replacing a one-line one, and
  the eight-line conjunctive condition replacing a one-line one, less the two removed lines.
- `UiThread_Tests.cs` grew by 35 lines: the positive twin plus its blank separator and its
  attribute. P1-T5 bounded that method at 36 source lines including the attribute and separator and
  required the file to end at or below 494; it ends at 493.
- `UiThreadInitContract_Tests.cs` grew by 4 lines: one for the apartment assertion added by P2-T2,
  and three for the multi-line form the tightened assertion takes after P2-T3.
- `UiThreadApartmentMeasurement_Tests.cs` is new, at 164 lines, holding both new test classes.

The remaining headroom on `UiThread_Tests.cs` is 7 lines, which is the tightest in the set. That is
the reason the plan placed only the positive twin there and put both negative cases and the
measurement in the new file.
