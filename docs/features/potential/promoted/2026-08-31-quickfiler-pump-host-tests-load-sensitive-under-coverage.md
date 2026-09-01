# quickfiler-pump-host-tests-load-sensitive-under-coverage (Issue #711)

- Date captured: 2026-08-31
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-pump-host-tests-load-sensitive-under-coverage/ (Issue #711)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #711
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/711
- Last Updated: 2026-08-31
## Summary

Fourteen `QuickFiler.Test` pump-host and dispatcher tests time out at one minute each when the full suite is run under `/EnableCodeCoverage` on a loaded machine. Every one of them passes on re-run against an unchanged tree, so the failures are load-sensitive rather than a regression.

## Environment

- OS/version: Windows 11, .NET Framework 4.8.1
- Python version: not applicable
- Command/flags used: `vstest.console.exe` over all discovered `*.Test.dll` with `/EnableCodeCoverage /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /Settings:TaskMaster.runsettings`
- Data source or fixture: `QuickFiler.Test` pump-host and dispatcher fixtures

## Steps to Reproduce

1. Run the full discovered test set with `/EnableCodeCoverage` while the machine is under concurrent load.
2. Observe fourteen `QuickFiler.Test` tests fail, each after approximately one minute.
3. Re-run the same command against a byte-identical tree with the machine idle.
4. Observe all fourteen pass.

## Expected Behavior

Unit tests must be deterministic. `.claude/rules/general-unit-test.md` requires determinism and prohibits real wall-clock waits, and sets a determinism retry-rate budget per tier in `.claude/rules/quality-tiers.md`. A test whose outcome depends on host load does not meet that bar.

## Actual Behavior

The affected tests wait on a message pump or dispatcher with a wall-clock timeout. Under coverage instrumentation the instrumented code runs slower, and under concurrent load the pump does not reach its expected state inside the one-minute window, so the wait expires and the test is recorded Failed.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: each failure is recorded with an elapsed time of approximately 60 seconds and a timeout message rather than an assertion-failure message. The absence of an assertion message is the discriminator between this class and a real regression.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Severity is High because the failures are indistinguishable from a real regression at first sight, so every affected run costs an investigation, and because a coverage-enabled CI run is exactly the configuration that triggers them.

## Suspected Cause / Notes

Observed during the issue #647 toolchain run and recorded in that feature's `evidence/qa-gates/p6-t5-full-suite-vstest.md`. Two additional `UtilitiesCS.Test` failures in the same run showed the same signature and also cleared on re-run. The feature review for #647 recorded this as non-blocking pre-existing determinism debt and recommended promoting it.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: replace each wall-clock wait with a deterministic completion signal, or drive the pump through an injected virtual scheduler, per the determinism infrastructure section of `.claude/rules/general-unit-test.md`.
- [ ] Integration scenario to retest: the full discovered test set under `/EnableCodeCoverage`, run twice, asserting an identical result set both times.
- [ ] Manual verification notes: enumerate the fourteen tests by fully qualified name from the recorded evidence before starting, so the fix can be shown to cover all of them rather than the first few found.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
