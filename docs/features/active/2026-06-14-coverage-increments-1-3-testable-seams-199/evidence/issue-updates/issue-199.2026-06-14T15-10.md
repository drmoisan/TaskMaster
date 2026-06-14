# Issue #199 Update Mirror — Phase 5 (Authorized Seams to Close AC1/AC3 Gaps)

- Timestamp: 2026-06-14T15-10
- PostedAs: unknown (local mirror; not posted to GitHub by this executor run)

## Intended update text

Phase 5 (maintainer-authorized scope change, option B per remediation-inputs.2026-06-14T15-10.md)
added two minimal production test seams to close the two previously-deferred Flag-and-Stop coverage
gaps. No runtime behavior changed.

### Production seams added (exactly two, both authorized)

1. `UtilitiesCS/Properties/AssemblyInfo.cs`: `[assembly: InternalsVisibleTo("ToDoModel.Test")]`,
   exposing the existing internal `MyBox.DialogInvoker` seam to ToDoModel.Test. `MyBox` still
   defaults to the real dialog in production.
2. `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`: extracted the `MatchBestSpecialFolder`
   matching logic into a pure `internal static` helper; the instance method delegates to it with
   byte-for-byte identical semantics.

### Tests added (12, all passing)

- `ToDoModel.Test/Data Model/Project/ProjectEntryDialogBranchesTests.cs` (3): malformed-ID dialog
  branch (via MyBox.DialogInvoker stub), CompareTo length tie-break (shorter/longer comparand).
- `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs` (9):
  positive, longest-match, case sensitivity, trailing separator, no-match, null/empty collection,
  empty path, null-path throws.

Full P5 run: 185 tests, 185 passed, 0 failed.

### AC outcomes

- AC3 (Increment 3 / MatchBestSpecialFolder): FULLY DELIVERED. New static helper 100% covered.
- AC1 (Increment 1 / ProjectEntry): malformed-ID dialog branch and CompareTo length tie-break now
  covered. Residual flag-and-stop: the change-confirmation branch (SetProjectId -> ChangeId)
  remains uncovered because ChangeId commits via the ProjectID property setter's RAW un-seamed
  MessageBox.Show, which would require a THIRD production seam beyond the two authorized for
  Phase 5. Recorded in evidence/other/p5-projectentry-changeconfirm-gap.2026-06-14T15-10.md;
  needs separate maintainer direction.

### Toolchain (final pass)

- csharpier: clean (exit 0).
- analyzers + code style: build succeeded, 0 errors.
- nullable + warnings-as-errors: 0 first-party errors (vendored SVGControl/UtilitiesSwordfish
  errors are pre-existing artifacts of solution-wide Nullable=enable, excluded from the analyzer stack).
- MSTest + coverage: 185/185 passed.

### Coverage delta

Coverage strictly increased vs the prior #199 state on the named seams; new-code coverage on the
new static helper is 100% (>= 90%); no regression on changed lines. Production-change boundary
verified: only the two authorized seams changed; no [ExcludeFromCodeCoverage]/coverage.config/
runsettings/pipeline change.
