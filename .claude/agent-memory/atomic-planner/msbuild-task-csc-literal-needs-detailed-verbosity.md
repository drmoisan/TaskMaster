---
name: msbuild-task-csc-literal-needs-detailed-verbosity
description: The literal `Task "Csc"` that proves CoreCompile ran is emitted only at MSBuild detailed verbosity, so a non-vacuity gate asserting it over a default-verbosity console run cannot pass
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

Related: [[project-512-toolchain-gate-fidelity-plan-seams]],
[[project-663-qfc-alt-chord-plan-seams]].
