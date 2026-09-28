---
name: commandline-match-on-results-dir-token-kills-own-tool-shells
description: Killing a hung vstest by matching a results-directory token against Win32_Process CommandLine also matches the agent's own bash.exe and pwsh.exe tool wrappers and kills them
metadata:
  type: project
---

To stop a hung `vstest.console.exe`, do not select processes with
`Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -match "<token>" }` where `<token>`
is a results-directory name such as `p4-t8`. That token is also present in the agent's own
`bash.exe` and `pwsh.exe` tool-invocation command lines, so the filter kills the agent's live tool
shells along with the runner.

**Why:** observed on issue #871 P4-T8. A filter on `p4-t8` matched six `bash.exe` and two
`pwsh.exe` processes belonging to the current session in addition to the two intended runner
processes. Nothing belonging to a sibling item matched, so the blast radius was self-inflicted
rather than cross-item, but the session's in-flight tool calls died.

**How to apply:** select on the executable name first and only then narrow, e.g.
`Where-Object { $_.Name -in @("vstest.console.exe","testhost.exe") -and $_.CommandLine -match "<token>" }`.
Confirm the candidate list by printing PID, Name and CommandLine before any `Stop-Process`.
Separately, a build-lock held by the killed command must still be released explicitly — the
release script is file-based and does not notice the holder's death.

Related: [[project_killing_a_build_lock_waiter_by_script_name_hits_every_sibling]],
[[project_timedout_mstest_leaves_detached_runner]].
