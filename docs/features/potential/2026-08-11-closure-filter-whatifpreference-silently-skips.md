# closure-filter-whatifpreference-silently-skips (Potential Bug)

- Date captured: 2026-08-11
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

`Remove-CoberturaExemptClosureCoverage` declares `[CmdletBinding(SupportsShouldProcess = $true)]` and guards its
only mutation with a single `$PSCmdlet.ShouldProcess(...)` call. A session in which `$WhatIfPreference` is `$true`
therefore skips the filter silently, and `ConvertTo-KoverageCoberturaXml` continues to a normal-looking coverage
document whose exempt closure lines were never removed. The failure is silent: no warning, no non-zero exit, and a
plausible rate.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (PowerShell 7 / Pester 5.6.1)
- Command/flags used: `./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
- Data source or fixture: any raw `dotnet-coverage` Cobertura document containing an exempt-member closure class

## Steps to Reproduce

1. In a PowerShell session, set `$WhatIfPreference = $true` (or invoke any ancestor of the coverage pipeline with `-WhatIf`).
2. Run the canonical coverage runner over the repository.
3. Inspect the post-processed `coverage\coverage.cobertura.xml` for a closure class whose declaring member carries `[ExcludeFromCodeCoverage]`.

## Expected Behavior

Either the filter runs (it is a pure in-memory XML transform that changes nothing on disk by itself, so there is no
state change for `-WhatIf` to protect), or the pipeline fails loudly rather than emitting a document that silently
retains lines the filter exists to remove.

## Actual Behavior

The `ShouldProcess` guard returns `$false`, the mutation is skipped, and the pipeline emits a coverage document whose
denominator still includes every exempt closure line. Because the resulting rate is plausible, nothing downstream
detects the omission.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no captured log; identified by code review of issue #457 (finding CR-3), recorded in
  `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/code-review.2026-08-11T01-33.md`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The trigger requires a non-default session preference, so it is unlikely in ordinary CI use. The severity comes from
the failure mode rather than the likelihood: a silently unfiltered denominator is exactly the class of invisible
coverage error that the `build-ci-coverage-gate-fidelity` epic exists to eliminate.

## Suspected Cause / Notes

`SupportsShouldProcess` was adopted under a genuine constraint, not by preference. PSScriptAnalyzer raises
`PSUseShouldProcessForStateChangingFunctions` (Warning) against a `Remove-` verb declared with a bare
`[CmdletBinding()]`, and `run_poshqc_analyze` exits non-zero on a Warning, so the analyze gate in #457 could not pass
without it. The tension is between an analyzer heuristic keyed on the verb and a function that changes no external
state. Options worth evaluating:

- Rename to an approved verb that does not trip the heuristic, keeping `[CmdletBinding()]` bare.
- Retain `SupportsShouldProcess` but make a skipped filter observable rather than silent.
- Narrowly suppress the rule with an in-code rationale.

Files to inspect: `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`,
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a Pester case that sets `$WhatIfPreference = $true` and asserts the chosen behavior (filter still applied, or a loud failure) rather than a silent no-op
- [ ] Integration scenario to retest: full coverage run, confirming `lines-valid` matches the filtered figure
- [ ] Manual verification notes: confirm `run_poshqc_analyze` still reports no new diagnostic on the module after the change

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Source: code review of issue #457 (finding CR-3), epic `build-ci-coverage-gate-fidelity` wave 1. Deliberately not
absorbed into #457, whose production surface is fixed at two files and exactly two edits by spec AC 13.
