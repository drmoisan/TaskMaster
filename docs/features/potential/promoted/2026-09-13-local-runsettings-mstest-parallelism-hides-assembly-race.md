# local-runsettings-mstest-parallelism-hides-assembly-race (Issue #877)

- Date captured: 2026-09-13
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/local-runsettings-mstest-parallelism-hides-assembly-race/ (Issue #877)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #877
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/877
- Last Updated: 2026-09-13
## Summary

The local coverage runsettings file enabled MSTest class-level parallelism at `Workers 0` (one worker per processor, 24 on the development host), which causes an assembly-resolution race in three `QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests` tests. CI is unaffected because it invokes vstest directly without a settings file, so the defect was invisible to CI and only surfaced in local coverage runs.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runner: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (local coverage runner)
- Command/flags used: local coverage run via `Invoke-MSTestWithCoverage.ps1`, which appends `/Settings:<runsettings path>` at line 76
- Data source or fixture: `QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests`

## Steps to Reproduce

1. Run the QuickFiler test suite locally through `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which passes `scripts/vscode/TaskMaster.cli.runsettings` (containing `<Workers>0</Workers>`, `<Scope>ClassLevel</Scope>`) as `/Settings:`.
2. Observe three `QfcInitEmailQueueZeroBatchTests` tests fail with `TypeInitializationException` for `Deedle.Reflection`, then for `FrameUtils`, then `FileNotFoundException: netstandard, Version=2.1.0.0`, at 9ms, under 1ms, and 1ms respectively.
3. Re-run the same suite without the settings file: all 1394 of 1394 tests pass, exit code 0.

## Expected Behavior

Local coverage runs should produce the same pass/fail result as CI for the same test suite and configuration.

## Actual Behavior

With the settings file (`Workers 0`, `ClassLevel` parallelism) applied, the same suite produced 3 failures and exit code 1, measured in one worktree minutes apart from the passing run. The failures are an assembly-resolution race during static initialization, not assertion failures: the CLR caches a failed static initializer for the process lifetime, so repeated runs under parallelism look deterministically red even though the underlying cause is a race.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `TypeInitializationException` for `Deedle.Reflection` (9ms), then for `FrameUtils` (<1ms), then `FileNotFoundException: netstandard, Version=2.1.0.0` (1ms), all in `QfcInitEmailQueueZeroBatchTests`.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Because only the local parallel path fails and CI never takes it, the defect was invisible to CI by construction. It blocked three items of a twelve-item parallel run in Phase 0 before being diagnosed.

## Suspected Cause / Notes

- `scripts/vscode/TaskMaster.cli.runsettings` contained only an MSTest `Parallelize` block with `Workers 0` and `Scope ClassLevel`.
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 76 appends `/Settings:<that file>` to every local coverage run, so every local run enabled that parallelism.
- `.github/workflows/_mstest-coverage.yml` line 99 invokes vstest directly with no `/Settings:` argument, so CI never enables parallelism and never exercises this race.
- An earlier theory attributing the failures to a Deedle `netstandard 2.1` packaging gap was retracted: the run behind that theory removed `dotnet-coverage` while keeping the settings file in both arms, so it never varied the variable that mattered.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: `QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests` under the local runsettings parallel path.
- [x] Integration scenario to retest: local coverage run with and without the runsettings file, comparing pass/fail counts.
- [x] Manual verification notes: fix already implemented and verified. Commit `95e27de44` on branch `chor/local-runsettings-disable-class-parallelism` changes `scripts/vscode/TaskMaster.cli.runsettings` `<Workers>0</Workers>` to `<Workers>1</Workers>` (one line). This disables class-level parallelism for local coverage runs while leaving CI's behavior unchanged (CI never referenced the file). Re-measurement with `Workers 1` is expected to match the no-settings-file passing result (1394 of 1394, exit 0).

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
