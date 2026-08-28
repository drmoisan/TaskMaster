---
name: unquoted-backslash-in-bash-arg-silently-redirects-output
description: An unquoted Windows path argument like coverage\coverage.cobertura.xml passed through the Bash tool loses its backslash, so a tool writes to a joined repo-root path instead — no error, wrong artifact location.
metadata:
  type: project
---

Any Windows-style path passed as a **bash command argument** must be single-quoted. Unquoted,
bash consumes the backslash as an escape character and the argument arrives joined:
`coverage\coverage.cobertura.xml` became `coveragecoverage.cobertura.xml`, a repo-root path.

**Why:** Observed on 2026-08-28 running
`pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 ... -CoverageOutput coverage\coverage.cobertura.xml`.
The runner echoed `Coverage output: <repo-root>\coveragecoverage.cobertura.xml` and proceeded
normally — **exit 0, no warning**. The failure is silent and only visible if you read the tool's
own echo of the resolved path. It would have written a 10 MB coverage document outside the
gitignored `coverage/` directory, dirtying the tree, and left a stray
`*.effective-coverage.config` at the repository root. This is the same backslash-eating class as
[[bash-heredoc-collapses-doubled-backslashes]] and
[[tool-layer-collapses-double-backslash-in-file-content]], but it bites **command arguments**, not
file content, so those two memories do not cover it.

**How to apply:** When a plan's command text contains a Windows path with a backslash
(`coverage\x.xml`, `QuickFiler.Test\bin\Debug\...`, `/flp:LogFile=docs\features\...`), single-quote
the whole argument in the Bash tool. Then verify from the tool's own output that the resolved path
is the intended one before letting the run finish — do not assume exit 0 means the path was right.
MSBuild `/flp:LogFile=...` and vstest `/ResultsDirectory:...` arguments survived in practice
because they were already inside double quotes; the bare argument did not.
