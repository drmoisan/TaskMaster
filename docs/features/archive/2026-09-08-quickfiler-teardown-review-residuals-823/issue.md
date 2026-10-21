# quickfiler-teardown-review-residuals (Issue #823)

- Date captured: 2026-09-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-teardown-review-residuals/ (Issue #823)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #823
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/823
- Last Updated: 2026-09-08
- Work Mode: full-bug

## Summary

Standing residuals record for low-severity findings from the `bugs-2026-09-06` run's reviews of
items 810 and 812, batched here rather than filed one issue per finding. Six entries, none of which
individually warrants its own issue.

## Environment

- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8
- Python version: not applicable
- Command/flags used: findings surfaced during code review and execution of items 810 and 812
- Data source or fixture: not applicable

## Steps to Reproduce

Each entry below carries its own location. None is a runtime reproduction; all are review findings.

## Expected Behavior

Each entry states its own expectation.

## Actual Behavior

- **R1 — `StoreWrapperController` retry latch is per-controller, not per-store.** A retry budget
  scoped to the controller is shared across every store that controller handles, so one store's
  failures consume the allowance for the others. From item 812; advisory finding, not independently
  re-derived.

- **R2 — the QuickFiler throw in #818 moves rather than disappears.** The change relocated the
  failure point instead of eliminating it. Worth deciding explicitly whether the relocation is the
  intended end state or an intermediate one. From item 812; advisory.

- **R3 — `BreadcrumbPopupOwnerRegistry.Register` declares non-nullable parameters in a
  `#nullable enable` file, while its own documentation and tests treat null as ordinary input.** The
  signature and the contract disagree; one of them is wrong. From item 810, code review CR-2.

- **R4 — stale line-count comment in `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`.** The
  comment cites 480 lines for a file that item 810 reduced to 459. Trivial; fold into the next
  change that touches the file. From item 810, code review CR-3.

- **R5 — `QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores`
  failed once and passed in three other runs.** Flake watch only; not enough signal to diagnose. Do
  not stabilize it with a sleep, a retry, or a timing tolerance. From item 810.

- **R6 — plan-authoring guidance in `atomic-plan-contract` permits an unsatisfiable gate.** An
  exit-code expectation keyed to a single observation of a known-intermittent test cannot fail in
  one direction. The guidance should key such a gate to post-versus-baseline plus a newly-failing
  check instead. This is a guidance amendment, not a code defect, and `.claude/` content is
  pushed down from `drm-copilot`, so the fix belongs upstream.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: see the code-review and evidence artifacts under the item 810 and item 812 feature
  folders on `main`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low individually. Recorded together so they are not rediscovered by every subsequent review of this
subsystem, and so R5's flake has a place to accumulate observations before anyone diagnoses it.

## Suspected Cause / Notes

All six are child-reported from the `bugs-2026-09-06` run and were not independently re-derived.
Verify each before acting on it.

Append further low-severity QuickFiler findings here as checklist items rather than opening a new
issue for each, per the 2026-09-07 ruling that residuals be batched by blast radius and standalone
issues reserved for Medium severity or higher. The `UtilitiesCS.Test` counterpart to this record is
issue 817.

Two findings from the same reviews were filed separately because they are High rather than Low: the
`QfcHomeController` double ribbon release and the `ProgressViewer` suppressed null check.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: per entry; R3 needs a test that pins whichever of the signature or the
      documented contract is correct.
- [ ] Integration scenario to retest: not applicable to most entries.
- [ ] Manual verification notes: R6 must be fixed in `drm-copilot`, not here, or the next push-down
      reverts it.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
