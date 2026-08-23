# User Story: Remove duplicate CS2002-triggering `<Compile>` entry (Issue #394)

- **Issue:** #394
- **Work Mode:** full-bug (a user story is optional for this mode; included because the epic
  planner for `build-ci-coverage-gate-fidelity` explicitly requested one)
- **Last Updated:** 2026-08-10T14-20

## Note on scope

This is a one-line build-configuration fix with no end-user-facing behavior. The "user" below is
the developer or CI consumer who reads build output, not an application end user. This document is
intentionally brief and should not be read as implying more product value than the change delivers.

## Story

As a developer or CI log reviewer building `UtilitiesCS.Test.csproj`,
I want the project file to list `PercentageFormatterTests.cs` exactly once,
so that build output no longer contains a spurious CS2002 warning that adds noise and makes it
marginally harder to notice genuinely new warnings.

## Acceptance (see spec.md for the authoritative, verifiable list)

- The duplicate `<Compile Include>` item for `PercentageFormatterTests.cs` is removed; exactly one
  remains.
- A `/t:Rebuild` build of `UtilitiesCS.Test.csproj` no longer emits CS2002 for that file.
- `PercentageFormatterTests` still reports 7 tests via vstest, unchanged from before the fix.

The full, objectively verifiable acceptance-criteria checklist for this feature lives in `spec.md`
per `acceptance-criteria-tracking` (`full-bug` mode: `spec.md` is the sole authoritative AC source).
This document does not duplicate or supersede that list.

## Out of scope

- No change in end-user-visible product behavior.
- No change to test outcomes, compiled output, or any other project file.
- Does not address the unrelated `System.Linq` `<Private>` duplication anomaly noted during the
  duplicate sweep (see `spec.md`, Scope & Non-Goals).
