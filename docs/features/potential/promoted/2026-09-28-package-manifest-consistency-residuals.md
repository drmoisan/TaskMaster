# package-manifest-consistency-residuals (Issue #929)

- Date captured: 2026-09-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/package-manifest-consistency-residuals/ (Issue #929)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #929
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/929
- Last Updated: 2026-09-28
## Summary
Consolidates #912 with the remaining parts of #914. PRs #920 and #921 fixed 8 of the 10 stale binding redirects and the `Invoke-ProjectConsistencyRepair` fallback defect. All remaining items break one invariant: a project's files and its own `packages.config` must agree, and the repair workflow that maintains them must be runnable.

1. **#912:** `QuickFiler.Test/QuickFiler.Test.csproj` lines 8 and 535 import `altcover.8.6.45` build assets. No `packages.config` declares `altcover`, so the imports are silently skipped by their `Exists()` guard.
2. **#914 sub-item 3 residual:** two binding redirects in `SVGControl/app.config` are still stale:
   - `Fizzler` redirects to 1.3.0.0, but the reference and the restored package are 1.3.1.0.
   - `System.Runtime.CompilerServices.Unsafe` redirects to 6.0.2.0, but the reference and the restored package are 6.0.3.0.
3. **#914 comment item 1:** `.github/workflows/dependabot-repair.yml:51` passes the deprecated `app-id` input to `actions/create-github-app-token@v3`. It should pass `client-id`, and the runbook should say to store the App's Client ID.
4. **#914 sub-item 1 (verification only, requires the maintainer):** AC18, AC19 and AC20 of #911 can only be exercised after a GitHub App credential is provisioned. The runbook is at `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md`. The secret store is also unconfirmed: a Dependabot-triggered `workflow_run` may read only Dependabot secrets. None of this is a merge gate for items 1 to 3.

## Environment
- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable (.NET Framework 4.8.1 packages.config projects, GitHub Actions)
- Command/flags used: `nuget restore TaskMaster.sln` from cold; static inspection on `main` at `177b6d78e`
- Data source or fixture: `QuickFiler.Test/QuickFiler.Test.csproj`, `QuickFiler.Test/packages.config`, `SVGControl/app.config`, `SVGControl/packages.config`, `.github/workflows/dependabot-repair.yml`

## Steps to Reproduce
1. `git grep -n altcover -- "*.csproj" "*packages.config"` finds two imports and no manifest entry.
2. Compare the `SVGControl/app.config` `bindingRedirect newVersion` values for `Fizzler` and `System.Runtime.CompilerServices.Unsafe` with the referenced assembly versions.
3. Read `dependabot-repair.yml:51`.

## Expected Behavior
- Every `..\packages\` import corresponds to a manifest entry.
- Every binding redirect names the assembly version that actually ships.
- The repair workflow uses the non-deprecated input.

## Actual Behavior
As listed in the Summary.

## Logs / Screenshots
- [x] Attached minimal logs or snippet
- Snippet: run https://github.com/drmoisan/TaskMaster/actions/runs/36281113978 fails at `Mint an installation token` with the deprecation warning for `app-id` (see the #914 comment of 2026-09-26).

## Impact / Severity
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes
- The altcover imports are left over from an evaluation whose manifest entry was removed.
- The #911 repair pass reconciles redirects only for packages it upgraded, so the pre-existing drift in `SVGControl` was left in place.

## Proposed Fix / Validation Ideas
- [ ] Delete both altcover `<Import>` elements. AltCover is not used by the coverage route, which is `dotnet-coverage`.
- [ ] Correct the two `SVGControl/app.config` redirects, preferably by running `scripts/dependencies/Repair-PackageManifestConsistency.ps1` rather than editing by hand.
- [ ] Switch `app-id` to `client-id` in the workflow, update the runbook, and confirm with actionlint.
- [ ] Add a verifier test case to the #911 consistency verifier that detects "an import whose package is absent from the manifest". Use the altcover case as the fixture shape, held in memory without temporary files.
- [ ] Rebuild the solution after a cold restore to confirm it still succeeds.
- [ ] Record AC18 to AC20 as deferred to the maintainer and cite the runbook. Do not mark them passed.

## Next Step
- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Consolidates: #912, #914 (sub-items 1, 3 residual, and the `client-id` comment). #914 sub-item 2 moved to the coverage-runner item. #914 sub-item 4 is fixed by PR #920.