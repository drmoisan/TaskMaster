# bayesian-staging-asynclazy-null-guard (Issue #131)

- Date captured: 2026-04-14
- Author: Dan Moisan
- Status: Validated locally in `docs/features/active/2026-04-14-bayesian-staging-asynclazy-null-guard-131/`

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #131
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/131
- Last Updated: 2026-04-14
- Work Mode: minor-audit

## Summary

Building the category classifier crashes when Bayesian staging data is missing because `CategoryClassifierGroup.LoadStagingData` forwards a null or empty `MinedMailInfo[]` into `ThrowIfNullOrEmpty()` instead of surfacing the missing mining prerequisite.

## Environment

- OS/version: Windows workstation, local development environment
- Python version: not applicable
- Command/flags used: ribbon action `Build Category Classifier`
- Data source or fixture: missing `<AppData>\Bayesian` staging data

## Steps to Reproduce

1. Open TaskMaster without existing Bayesian staging files under `%LocalAppData%\TaskMaster\Bayesian`.
2. Trigger the `Build Category Classifier` ribbon action without first running `Continue Mining` or `Scrape and Mine`.
3. Observe the category-classifier build path.

## Expected Behavior

The build should stop cleanly and explain that Bayesian mining must be run before category classifiers can be built.

## Actual Behavior

The build throws an unhandled `ArgumentNullException` from `LoadStagingData` after `EmailDataMiner.Load<MinedMailInfo[]>(folderPath)` returns null or an empty collection.

## Acceptance Criteria

- [x] `Build Category Classifier` no longer crashes when staged Bayesian data is missing.
- [x] The user sees an actionable warning that tells them to run `Continue Mining` or `Scrape and Mine` before building category classifiers.
- [x] The dead `EmailDataMiner` local in `CategoryClassifierGroup.BuildClassifiersAsync` is removed or otherwise accounted for.
- [x] MSTest regression coverage verifies the missing-staging-data path in `UtilitiesCS.Test`.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet:
  `System.ArgumentNullException: collection cannot be null or empty. Called from LoadStagingData`

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

`ContinueMiningAsync` and `ScrapeAndMineAsync` are already separate ribbon actions that populate the Bayesian staging folder. `BuildCategoryClassifierAsync` expects that staging data to exist but previously handled the missing-data case as an unhandled exception instead of a user-facing prerequisite failure.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas
- [x] Integration scenario to retest
- [x] Manual verification notes

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
