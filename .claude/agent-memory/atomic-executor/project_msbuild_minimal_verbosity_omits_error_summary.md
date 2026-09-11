---
name: msbuild-minimal-verbosity-omits-error-summary
description: MSBuild prints the "Build succeeded / N Warning(s) / N Error(s)" block only at normal verbosity or above, so a gate asserting the literal `0 Error(s)` reads nothing at /v:m or /v:q
metadata:
  type: project
---

MSBuild emits the `Build succeeded.` / `N Warning(s)` / `N Error(s)` summary block only at
verbosity **normal or above**. At `/v:m` (minimal) and `/v:q` (quiet) the block is absent entirely,
even though the build ran and exit code 0 is returned.

**Why:** Nearly every C# plan in this repo writes an acceptance condition of the form "records
`EXIT_CODE: 0` and quotes the summary line `0 Error(s)` together with the reported warning count."
If you add `/v:m` to keep the console output small, the run succeeds but the summary line the gate
demands was never printed, so the observation cannot be made and the task cannot be checked off
honestly. Observed on 2026-09-07 executing issue #798 Phase 0: `/v:m` produced only the per-project
`Project -> path.dll` lines and then the exit code; a `/flp:Verbosity=minimal` file log had the same
gap (2,495 bytes, no summary).

**How to apply:** Keep the console quiet and put the summary in a file log at normal verbosity:
`... /nologo /v:q "/flp:LogFile=<scratch>\build.log;Verbosity=normal"`, then read the tail of that
log. A normal-verbosity solution-wide log is roughly 3 MB for this solution, which greps fine and
stays outside the repo so `.gitignore`'s `*.log` rule is irrelevant. Do not lower the file logger to
minimal to save space — that reintroduces the gap. Related: [[msbuild-log-has-two-absolute-path-leak-classes]].
