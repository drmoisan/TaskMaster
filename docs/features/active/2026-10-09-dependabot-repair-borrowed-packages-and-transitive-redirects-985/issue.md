# dependabot-repair-borrowed-packages-and-transitive-redirects (Issue #985)

- Date captured: 2026-10-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/dependabot-repair-borrowed-packages-and-transitive-redirects/ (Issue #985)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #985
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/985
- Last Updated: 2026-10-09
- Work Mode: full-bug

## Summary

Grouped NuGet Dependabot pull requests (PR #984) still fail CI after the `dependabot-repair` workflow (issue #911) runs successfully, because two classes of manifest inconsistency are invisible to both Dependabot and the repair script: (1) test projects that hint-path a package they do not declare in their own `packages.config` (borrowed from a sibling production project), and (2) `app.config` binding redirects for assemblies that a project receives only transitively (no `packages.config` entry and no direct `<Reference>`). All failures must be remediated automatically, with no manual step on any future Dependabot PR.

## Environment

- OS/version: GitHub Actions `windows-latest` (CI) and Windows 11 (local)
- Python version: n/a (PowerShell 7 repair scripts, .NET Framework 4.8.1 / packages.config projects)
- Command/flags used: CI workflow `ci.yml` on branch `dependabot/nuget/QuickFiler.Test/all-nuget-updates-2542c001c9`; repair workflow `.github/workflows/dependabot-repair.yml` (workflow_run trigger) invoking `scripts/dependencies/Repair-PackageManifestConsistency.ps1` without `-CandidateUpgrade`
- Data source or fixture: PR #984 (AngleSharp, log4net 3.4.0 -> 3.5.0, Microsoft.Web.WebView2 1.0.4191.47 -> 1.0.4258.31, analyzer bumps); CI run #1059 (id 37959420911) on repair commit fff92fe83

## Steps to Reproduce

1. Let Dependabot open a grouped NuGet update that bumps Microsoft.Web.WebView2 and log4net (PR #984).
2. Let `dependabot-repair` run after CI; it repairs `<Analyzer Include>` paths and pushes commit fff92fe83 as `taskmaster-dependabot-repair[bot]`.
3. Observe CI run #1059 on that commit.

## Expected Behavior

After the repair workflow runs, every required CI check (build-analyzers, build-nullable, mstest-coverage, pester, format-check, actionlint, hygiene) passes on the Dependabot branch with no human edit. Every project that compiles against a package declares that package in its own `packages.config`, and every `bindingRedirect newVersion` equals an assembly version actually referenced in the solution.

## Actual Behavior

- build-analyzers, build-nullable, mstest-coverage fail: `UtilitiesCS.Test` and `QuickFiler.Test` hint-path `packages\Microsoft.Web.WebView2.1.0.4191.47\...` without declaring WebView2 in their own `packages.config`; after the production projects move to 1.0.4258.31 nothing restores 4191.47, producing `CS0012` / `CS0234` / `CS0246` (for example `UtilitiesCS.Test\HelperClasses\ThemeHelpers\ThemeTests.cs(284,29): error CS0012: The type 'WebView2' is defined in an assembly that is not referenced`).
- The same latent pattern exists on main for `ObjectListView.Official 2.9.1` in `QuickFiler.Test` and `TaskTree.Test`.
- pester fails: `BindingRedirectVerification.Tests.ps1` reports `log4net|3.4.0.0` because Tags.Test, TaskTree.Test, TaskVisualization.Test, ToDoModel.Test and VBFunctions.Test keep `newVersion="3.4.0.0"` while every other project moved to 3.5.0.0; those five projects have no log4net `packages.config` entry or `<Reference>`, so neither Dependabot nor the repair script edits them, and the repair's redirect pass does not run from the workflow_run trigger because `-CandidateUpgrade` is not supplied (see `dependabot-repair.yml` lines 87-94).

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `Expected 0, because every bindingRedirect newVersion must equal a csproj Reference version (issue 973 emptied the recorded known-debt set; a new stale pair is fixed, not recorded); observed: log4net|3.4.0.0, but got 1.`

## Impact / Severity

- [x] Blocker
- [ ] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

Dependabot's packages.config updater and `Repair-PackageManifestConsistency.ps1` both act only on packages a project declares in its own `packages.config`. Borrowed hint paths and transitive binding redirects fall outside that set. The verifier (`ConsistencyVerifier.psm1` `Find-OrphanedHintPath`) already detects borrowed hint paths but is not a failing gate on main.

## Proposed Fix / Validation Ideas

- [ ] Declare Microsoft.Web.WebView2 in QuickFiler.Test and UtilitiesCS.Test `packages.config`, and ObjectListView.Official in QuickFiler.Test and TaskTree.Test `packages.config`, at the versions their production siblings declare.
- [ ] Add a redirect-sync pass to the repair script that sets every `bindingRedirect newVersion` (and the upper bound of `oldVersion`) to the assembly version referenced elsewhere in the solution, covering transitive redirects; run it from the workflow_run trigger.
- [ ] Make orphaned (borrowed) hint paths a failing check on main (Pester gate) so the pattern cannot recur.
- [ ] Pester coverage for the new pass with in-memory fixtures (no temp files).
- [ ] After merge, `@dependabot recreate` on PR #984 and confirm all required checks pass with no human edit.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
