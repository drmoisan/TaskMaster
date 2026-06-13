# Visual Studio Verification Checklist (supports AC8)

Timestamp: 2026-06-12T19-22

PostedAs: unknown

## Purpose

The CLI cannot reproduce the Visual Studio static-coverage `System.Security.VerificationException` (see
`evidence/regression-testing/exclusion-effect-not-cli-verifiable.2026-06-12T19-22.md`). The authoritative
acceptance for the exclusion's effect (AC8) is a user confirmation in Visual Studio. Perform the steps below
against `UtilitiesCS.Test`, with the edited repo-root `TaskMaster.runsettings` in place (it now carries the
`<DataCollector friendlyName="Code Coverage">` `ModulePaths/Exclude` block mirroring `coverage.config`).

Precondition:
- Confirm Visual Studio is auto-detecting the repo-root `TaskMaster.runsettings`
  (Test > Configure Run Settings > "Auto Detect runsettings Files" enabled, or "Select Solution Wide runsettings
  File" pointing at the repo-root `TaskMaster.runsettings`).
- Build `UtilitiesCS.Test` in Debug.

## Checklist

- [ ] (a) Run Tests, NO coverage:
      In Test Explorer, run the Deedle tests (`DfDeedle_COM_Tests`, `DfDeedle_Tests`,
      `DeedleTests.DeedleDoodles`) using "Run" (NOT "Analyze Code Coverage").
      EXPECTED: all listed Deedle tests are GREEN, and NO coverage is collected (no coverage results window/data).

- [ ] (b) Analyze Code Coverage:
      In Test Explorer, run the same Deedle tests (`DfDeedle_COM_Tests`, `DfDeedle_Tests`,
      `DeedleTests.DeedleDoodles`) using "Analyze Code Coverage for Selected Tests" (or "for All Tests").
      EXPECTED: all listed Deedle tests are GREEN, with NO `System.Security.VerificationException`
      ("Operation could destabilize the runtime"), because the root `TaskMaster.runsettings` `ModulePaths/Exclude`
      block prevents the Code Coverage collector from instrumenting `FSharp.Core`/`Deedle`.

## On completion

When both (a) and (b) pass, mark AC8 in
`docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/issue.md` as `[x]` and update
`evidence/issue-updates/ac8-vs-confirmation-pending.2026-06-12T19-22.md` to record the confirmation.
If (b) still throws `VerificationException`, the exclusion block did not take effect — re-verify VS is reading the
edited root `TaskMaster.runsettings` and that the seven `<ModulePath>` entries are present.
