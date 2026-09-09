# claude-md-cut3-names-uninvoked-coverage-command (Issue #828)

- Date captured: 2026-09-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/claude-md-cut3-names-uninvoked-coverage-command/ (Issue #828)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #828
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/828
- Last Updated: 2026-09-09
## Summary

`CLAUDE.md` section CUT3 (C# Toolchain Command Selection), step 4, specifies
`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` as the C# test toolchain command. The
coverage route actually in use in this repository is `dotnet-coverage collect` wrapping
`vstest.console.exe`, with `/EnableCodeCoverage` deliberately omitted from the inner invocation. A
policy document that names a command the repository deliberately does not run produces false PARTIAL
verdicts against any acceptance criterion that cites it.

## Environment

- OS/version: Windows 11 Pro 10.0.26200 (the mismatch is platform-independent; it is a document/route divergence, not a runtime defect)
- Python version: not applicable; the affected surface is C# toolchain policy text and PowerShell coverage scripting
- Command/flags used: `CLAUDE.md` CUT3 step 4 as written, `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`, versus the implemented route in `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
- Data source or fixture: `scripts/vscode/TaskMaster.cli.runsettings`, `coverage.config`

## Steps to Reproduce

1. Read `CLAUDE.md` section CUT3, step 4, and note the command it names.
2. Read `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and observe that the implemented route is an outer `dotnet-coverage collect` wrapping an inner `vstest.console.exe` that is never given `/EnableCodeCoverage`.
3. Read `scripts/vscode/TaskMaster.cli.runsettings` and observe it carries the MSTest parallelization only and no coverage data collector.
4. Observe that a reviewer checking an acceptance criterion which cites CUT3 step 4 finds the delivery did not run the command the policy names.

## Expected Behavior

The toolchain command named in `CLAUDE.md` CUT3 step 4 matches the coverage route the repository
actually runs, so a reviewer citing it evaluates the delivery against a command that is genuinely
invoked.

## Actual Behavior

CUT3 step 4 names a `vstest.console.exe ... /EnableCodeCoverage` invocation that the repository
deliberately never issues. The omission of `/EnableCodeCoverage` is load-bearing rather than
accidental: enabling the built-in Code Coverage collector alongside the outer `dotnet-coverage`
instrumentation conflicts.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: the corroborating in-code comment at `scripts/vscode/Invoke-MSTestWithCoverage.ps1` lines 19-26, in the `.DESCRIPTION` block of `Resolve-RunSettingsPath`:

> The CLI runsettings (TaskMaster.cli.runsettings) lives alongside this script in scripts/vscode and
> is resolved deterministically from the script directory. It carries the MSTest parallelization only
> and no coverage data collector, so the inner vstest invocation never activates the Code Coverage
> collector; instrumentation comes solely from the outer dotnet-coverage --settings coverage.config
> path.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The mismatch is what made issue #809's AC6 read as PARTIAL on wording alone: the reviewer found the
delivery had not run the command `CLAUDE.md` names, recomputed AC6's measurable clauses from raw
Cobertura, and found they pass. Every future item citing CUT3 step 4 inherits the same false PARTIAL.

## Suspected Cause / Notes

The policy text predates the migration to the `dotnet-coverage collect` route and was not updated
when the wrapping changed. The in-code comment cited above records the reasoning for the current
arrangement, so the implementation is correct and the document is stale, not the reverse.

**Ownership caveat — this cannot be a drive-by edit.** Whether the correction belongs in this
repository or upstream in `drm-copilot` must be decided first. Everything under `.claude/` other than
`agent-memory/` is pushed down from that governance repository with no templating, so a change made
here would be overwritten by the next push-down. `CLAUDE.md` itself sits at the repository root
rather than under `.claude/`, so its ownership should be confirmed before the edit is made. Do not
simply edit `CLAUDE.md` here without resolving that question.

## Proposed Fix / Validation Ideas

- [x] Reword CUT3 step 4 to name the `dotnet-coverage collect` route, and cite `scripts/vscode/Invoke-MSTestWithCoverage.ps1` as the implementation of record.
- [ ] Unit coverage areas: none; this is a documentation correction with no runtime surface.
- [ ] Integration scenario to retest: confirm the reworded command matches what `scripts/vscode/Invoke-MSTestWithCoverage.ps1` issues, including the deliberate absence of `/EnableCodeCoverage` on the inner invocation.
- [ ] Manual verification notes: confirm ownership of `CLAUDE.md` (this repository versus `drm-copilot`) before editing, per the caveat above.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

**Origin.** Raised by the delivery of issue #815 under its acceptance criterion AC14, which requires
this mismatch to be handed off rather than fixed in that change. Issue #815's `spec.md` Non-Goal 1
places that feature under a hard constraint not to modify `CLAUDE.md`, and the
`review-residuals-2026-09-08` epic manifest records the same constraint for a sibling feature.
Bundling the correction would have put a `CLAUDE.md` edit inside a change whose only code surface is
three PowerShell files.
