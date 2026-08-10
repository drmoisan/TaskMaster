# utilitiescs-test-cs2002-duplicate-compile-entry (Issue #394)

- Issue: #394
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/394
- Type: bug
- Work Mode: full-bug
- Epic: build-ci-coverage-gate-fidelity
- Integration Branch: epic/build-ci-coverage-gate-fidelity-integration
- Branch: bug/utilitiescs-test-cs2002-duplicate-compile-entry-394
- Owner: drmoisan
- Last Updated: 2026-08-10
- Source: docs/features/potential/promoted/2026-07-20-utilitiescs-test-cs2002-duplicate-compile-entry.md

> Work-mode note: the promoted potential entry recorded `minor-audit`. The GitHub issue body
> carries the literal text "(not provided in potential file)" in every section, including
> `## Acceptance Criteria`. A `minor-audit` run requires an explicit, populated
> `## Acceptance Criteria` section in `issue.md` as its sole requirements source, so the
> minor-audit eligibility check fails and the lifecycle requires failing closed to the full
> path. Selected mode is therefore `full-bug`, with `spec.md` as the authoritative
> acceptance-criteria source per `acceptance-criteria-tracking`. The sections below are
> populated from the promoted potential entry, which is substantially more detailed than the
> GitHub issue body.

## Summary

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` contains two identical
`<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />` items, producing
compiler warning CS2002 ("Source file ... specified multiple times") on every build of the
test project.

The potential entry recorded the duplicate at lines 288 and 338 as of commit `443a1a52`. On the
epic integration base (`edf3d34c`) the duplicate is still present, but the line numbers have
shifted: the two occurrences are now at **line 304** and **line 356**. Both occurrences lie
inside the *same* `<ItemGroup>` (which spans lines 72-529), which refines the potential entry's
"two `<ItemGroup>` sections" hypothesis.

## Environment

- OS/version: Windows (any); also reproduces on the `windows-latest` CI runner
- Toolchain: MSBuild, `TaskMaster.sln`, Configuration=Debug, Platform=Any CPU
- Command/flags used: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
- Data source or fixture: none

## Steps to Reproduce

1. Build the solution or the test project:
   `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`.
2. Observe the compiler output for `UtilitiesCS.Test.csproj`.

## Expected Behavior

The build completes without CS2002; each source file appears exactly once in the project's
`<Compile>` item group.

## Actual Behavior

```
CSC : warning CS2002: Source file 'C:\...\UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times [UtilitiesCS.Test.csproj]
```

## Logs / Screenshots

- Observed in local toolchain runs on 2026-07-20 during PR #391 verification (analyzer build and
  `TreatWarningsAsErrors` rebuild).

## Impact / Severity

- Severity: **Low**.
- Build warning noise only. The duplicate entry does not currently fail any gate because CS2002
  is not promoted to an error in the affected configuration.
- It risks masking real warnings, and it would break the build if warning-promotion rules
  changed. This is a live consideration: sibling feature `csharp-toolchain-gate-fidelity-512`
  in this same epic is changing the `TreatWarningsAsErrors` gate.

## Suspected Cause / Notes

Likely a merge artifact. Two independently appended blocks of `OutlookObjects\Folder\*Tests.cs`
entries within the single large `<ItemGroup>` each carry the same file.

## Proposed Fix / Validation Ideas

- Remove one of the duplicate `<Compile>` items from `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
- Rebuild and confirm CS2002 no longer appears for that file.
- Confirm `PercentageFormatterTests` still runs with an unchanged test count via vstest.
- Sweep the remainder of the project file for any other duplicate `<Compile Include>` entries.

## Scope Constraints (from epic charter)

- Scope is `UtilitiesCS.Test/UtilitiesCS.Test.csproj` only.
- Do **not** modify `CLAUDE.md`, anything under `.claude/rules/`, or anything under `scripts/`.
  Those surfaces belong to sibling features in this epic; editing them would cause a fan-in
  conflict on the integration branch.
- Do **not** reformat, reorder, or otherwise churn the csproj. Remove the duplicate item and
  nothing else.
- `CLAUDE.md`'s documented `/p:Nullable=enable` type-check command is a known defect
  (issue #522, fixed by sibling feature 512) that produces roughly 200-414 spurious `CS86xx`
  errors on a clean `main` with no local change. Those are **not** a blocking finding for this
  change. Verify against CI's actual command instead:
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  (`.github/workflows/ci.yml` lines 103-116).

## Acceptance Criteria

The authoritative acceptance-criteria source for this `full-bug` feature is `spec.md`. The list
below mirrors it for convenience and must be kept consistent with it.

- [ ] Exactly one `<Compile Include>` item for `OutlookObjects\Folder\PercentageFormatterTests.cs`
      remains in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
- [ ] A pre-change build of `UtilitiesCS.Test.csproj` captures the CS2002 warning for that file
      as fail-before evidence.
- [ ] A post-change build of `UtilitiesCS.Test.csproj` emits no CS2002 for that file.
- [ ] `PercentageFormatterTests` still runs with an unchanged test count, verified via vstest,
      with the before and after counts recorded numerically.
- [ ] The rest of `UtilitiesCS.Test.csproj` is swept for other duplicate `<Compile Include>`
      entries; findings are reported and any found are fixed in the same change.
- [ ] The diff touches only `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (plus feature-folder
      documentation and evidence), with no reformatting or reordering.
</content>
</invoke>
