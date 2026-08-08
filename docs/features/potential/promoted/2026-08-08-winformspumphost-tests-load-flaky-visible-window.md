# winformspumphost-tests-load-flaky-visible-window (Issue #511)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/winformspumphost-tests-load-flaky-visible-window/ (Issue #511)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #511
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/511
- Last Updated: 2026-08-08
## Summary

Unit tests built on `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` start a real WinForms message pump (`Application.Run`) on a dedicated STA thread and construct real WinForms controls. They fail nondeterministically when the machine is under heavy CPU load, and they display a visible window during an otherwise headless test run.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1, MSTest via `vstest.console.exe` (VS18 test platform)
- Command/flags used: `./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .` (full-suite run with coverage)
- Data source or fixture: `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs`; affected suites include `QfcItemController_InitializationTests` (the `*ThroughThePumpHost*` cases) and `WpfDispatcherYieldTests`

## Steps to Reproduce

1. Drive the machine to sustained high CPU utilization (observed at approximately 96%).
2. Run the full MSTest suite with coverage using the command above.
3. Repeat several times and observe both the pass/fail outcome and the desktop.

## Expected Behavior

Unit tests are deterministic and headless. Per `.claude/rules/general-unit-test.md`, tests must produce the same result given the same inputs and must not depend on real wall-clock timing. No unit test should create a visible window on the developer's desktop.

## Actual Behavior

- The affected tests failed nondeterministically under load. During baseline capture for issue #438 on 2026-08-08, six attempts were required to obtain a clean full-suite baseline; the failures were confined to these suites and did not reproduce once the machine was idle.
- A visible window appeared during the run, because the host constructs a real WinForms control and pumps a real message loop.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no captured failure log is retained; the observation is recorded in the issue #438 execution report and in `.claude/agent-memory/atomic-executor/project_winformspumphost_tests_load_flaky.md`. A fresh capture under induced load should accompany the fix.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Nondeterministic tests are corrosive to an autonomous development workflow: they force repeated full-suite reruns (six were needed for one baseline), and they train reviewers and agents to retry a red suite rather than investigate it, which is exactly the habit that lets a real regression through. The visible window also makes the suite unsuitable for unattended or headless CI execution.

## Suspected Cause / Notes

Observed during orchestration of issue #438 on 2026-08-08. These tests and the host were not modified by that work; the finding is pre-existing and was left untouched to respect the minimal-fix boundary.

- `WinFormsPumpHost` runs `Application.Run(ApplicationContext)` on a dedicated STA background thread to supply a `WindowsFormsSynchronizationContext` that drains posted continuations. Its own documentation explains the motivation: awaiting `control.UiSyncContext` on a thread-pool MSTest thread hangs because nothing pumps the message loop.
- The design is deliberate and well documented, but it makes the tests dependent on real OS scheduling of a real message loop, which is what degrades under CPU contention.
- Comparable prior art exists in the repository: `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs` uses an analogous `StaDispatcherHost`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: replace the real pump with an injectable synchronization-context / dispatcher seam so the behavior under test is exercised against a controllable context rather than a live message loop, per the DI-seam preference in `.claude/rules/csharp.md` (interface seam first). Where a real pump is genuinely irreducible, the affected cases belong in an integration category that is not part of the unit suite.
- [ ] Integration scenario to retest: run the full suite repeatedly under induced high CPU load and confirm a stable pass rate, and confirm no window is created during a full run.
- [ ] Manual verification notes: confirm the retry rate meets the determinism budget in `.claude/rules/quality-tiers.md` for the applicable tier. Do not address this by adding retries, sleeps, or timing tolerances — `.claude/rules/csharp.md` prohibits masking flaky behavior that way.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
