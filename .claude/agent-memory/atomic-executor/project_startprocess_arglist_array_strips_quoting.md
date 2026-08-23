---
name: startprocess-arglist-array-strips-quoting
description: Detaching msbuild via `Start-Process -ArgumentList @(...)` silently drops the quoting on `/p:Platform=Any CPU`, so the gate dies with MSB1008 having compiled nothing; pass ONE pre-quoted argument string instead.
metadata:
  type: project
---

When launching `MSBuild.exe` (or `vstest.console.exe`) detached via
`Start-Process -PassThru -RedirectStandardOutput ...`, pass the arguments as a **single
pre-quoted string**, not as an array:

```powershell
# WRONG - dies with MSB1008, compiles nothing, exit code 1
$argList = @('TaskMaster.sln','/t:Rebuild','/m','/p:Configuration=Debug',
             '/p:Platform=Any CPU','/p:EnableNETAnalyzers=true')

# RIGHT
$argList = 'TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true'
```

**Why:** `Start-Process` joins the array elements with spaces to build the child's command
line and does NOT re-quote an element that contains a space. `/p:Platform=Any CPU` arrives
as two arguments, so MSBuild sees a second "project" and exits 1 with:

```
MSBUILD : error MSB1008: Only one project can be specified.
    Full command line: '"...MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug /p:Platform=Any CPU ...'
```

Measured 2026-08-23 on the #511 remediation cycle. The failure is quiet in the worst way: the
launcher itself succeeds, `$p.ExitCode` is a legitimate 1, and the log's own
`Full command line:` echo is the only place the missing quotes are visible. A gate that only
checked "did the process exit" would have recorded a red analyzer gate as a code defect.

**How to apply:** this is the exact same class of defect as
[[bash-tool-mangles-msbuild-switches]] one layer further in — the Bash tool mangles `/m` into
`M:/`, and `Start-Process -ArgumentList @(...)` mangles `"/p:Platform=Any CPU"`. Whenever a
plan mandates a detached long-run mechanic (`Start-Process -PassThru` + poll + take
`ExitCode` from the process object), build the argument line as one string and grep the log
for `Full command line:` on the first launch to confirm the quotes survived. Related:
[[project_pwsh_command_quoting_from_bash]], [[project_long_runs_need_detached_process]],
[[project_build_test_env]].
