# Remediation Plan — appointment-item-test-coverage-79

## Objective
Split `MeetingItemHelperTests.cs` (748 lines) into two partial class files each ≤ 500 lines to satisfy the 500-line file-size policy.

## Steps

- [x] [R-T1] Add `partial` keyword to class declaration in `MeetingItemHelperTests.cs` and truncate after `EmailHeader2_ShouldIncludeProjectedTextFields` test, appending closing braces.
- [x] [R-T2] Create `MeetingItemHelperTests.Part2.cs` with the remaining tests and all private helpers using `partial class`.
- [x] [R-T3] Add `MeetingItemHelperTests.Part2.cs` compile entry to `UtilitiesCS.Test.csproj`.
- [x] [R-T4] Run CSharpier format on both files — `EXIT_CODE: 0`.
- [x] [R-T5] Run analyzer-enabled MSBuild — `EXIT_CODE: 0`.
- [x] [R-T6] Run nullable-enforced MSBuild — `EXIT_CODE: 0`.
- [x] [R-T7] Run MSTest+coverage — `EXIT_CODE: 0`, 1169+ passed, 0 failed.
- [x] [R-T8] Verify both files are ≤ 500 lines.
- [x] [R-T9] Refresh final QA gate artifacts and re-run reduced audit.
