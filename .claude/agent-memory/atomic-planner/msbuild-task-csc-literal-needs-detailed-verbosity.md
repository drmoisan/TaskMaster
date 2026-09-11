---
name: msbuild-task-csc-literal-needs-detailed-verbosity
description: The literal `Task "Csc"` needs detailed verbosity AND can never be attributed to a named project on one line — use the echoed `/out:obj\Debug\<Assembly>.dll` csc command line instead
metadata:
  type: feedback
---

An msbuild non-vacuity gate that asserts the literal `Task "Csc"` appears in the build log must
supply a detailed-verbosity log, or the assertion cannot pass on any run.

**Why:** MSBuild's default console verbosity is `normal`, which prints target names and the compiler
command line but not the task-started events that carry `Task "Csc"`. That message is a
detailed-verbosity event. A gate written against a plain console capture therefore fails on a
perfectly good build, which is the mirror image of the vacuous-gate defect it was written to prevent.

**How to apply:** attach a file logger scoped to a gitignored directory and grep that file, then
delete it after recording the occurrence count and byte size:

```
"/flp:LogFile=coverage\<gate-name>.msbuild.log;Verbosity=detailed"
```

In this repository `coverage/*` is gitignored at `.gitignore:144`, so the log never reaches a commit.
Pair the literal count with a second, cheap observation that is independent of verbosity: the
`LastWriteTimeUtc` of the affected project's output assembly must advance across the command. Either
observation alone proves compilation ran; together they survive a change in MSBuild's message text.

**Prefer the echoed csc command line outright, and NEVER try to attribute `Task "Csc"` to a named
project.** MSBuild prefixes a task-started line with the project INSTANCE ID, never with the project
path, so a count of lines carrying both `Task "Csc"` and `UtilitiesCS.csproj` is zero at every
verbosity — a gate on that pair can never pass. MSBuild echoes the full csc.exe command line under
each project's `CoreCompile` heading at NORMAL verbosity, and that single line carries
`/out:obj\Debug\<Assembly>.dll`, which names both the compiler invocation and the assembly it
produced. Confirmed against the committed normal-verbosity log
`docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/evidence/qa-gates/p7-t4-analyze.msbuild.txt`
line 932 (`/out:obj\Debug\VBFunctions.dll`); that same log carries zero `Task "Csc"` lines. Caught as
#825 R4 defect D-19 (2026-09-09).

Related: [[project-512-toolchain-gate-fidelity-plan-seams]],
[[project-663-qfc-alt-chord-plan-seams]],
[[project_825_etl_deadline_mechanics_plan_seams]].
