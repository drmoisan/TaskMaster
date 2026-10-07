# quickfiler-utilities-coverage-and-filesize-residuals (Issue #933)

- Date captured: 2026-09-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-utilities-coverage-and-filesize-residuals/ (Issue #933)

- Issue: #933
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/933
- Last Updated: 2026-09-28
## Problem / Why

On 2026-09-11 the maintainer deferred sub-findings 1 to 3 of #727 out of the bug run, to be re-filed as refactor work. They were never re-filed. This entry carries the parts not tracked elsewhere. #623 already covers `QfcCollectionController.cs` and `QfcFormControllerTests.cs`. `QfcQueue.cs` is now 439 lines and no longer applies.

1. **Coverage debt:** `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` is at about 49% line coverage, which is below the floor.
2. **Missing from coverage report:** `StoreWrapperController` does not appear in the Cobertura report at all, even though only 2 of its members carry `[ExcludeFromCodeCoverage]`.
3. **500-line cap violations not covered by #623:**
   - `UtilitiesCS/Threading/TimeOutTask.cs`: about 1011 lines
   - `QuickFiler/Controllers/EfcFormController.cs`: about 1189 lines

## Proposed Behavior

- Add injectable seams where the Outlook and WinForms handlers block unit testing.
- Explain the `StoreWrapperController` report gap, then either fix the instrumentation or record why it is excluded.
- Split both oversized files into cohesive partial-class parts without changing any public API.

## Acceptance Criteria (early draft)

- [ ] `QfcFormController.EventHandlers.cs` line coverage is at least 80% on the testable denominator.
- [ ] `StoreWrapperController`'s absence from the coverage report is explained, and it is either instrumented or explicitly exempted under CLAUDE.md UT2.
- [ ] `TimeOutTask.cs` and `EfcFormController.cs` are each at most 500 lines.
- [ ] No public API change, and no test assertion is weakened.

## Constraints & Risks

- `QuickFiler.Test.csproj` enumerates every Compile Include explicitly.
- `TimeOutTask` is shared threading infrastructure. A split must not change its timing semantics.

## Test Conditions to Consider

- [ ] Whole-assembly runs are green before and after each split.
- [ ] Coverage does not regress on moved lines.

## Next Step

- [x] Promote to GitHub issue (feature request template)
- [ ] Create the active folder when scheduled

Source: re-filed from #727 sub-findings 1 to 3, per the maintainer decision recorded on #727 on 2026-09-11.
