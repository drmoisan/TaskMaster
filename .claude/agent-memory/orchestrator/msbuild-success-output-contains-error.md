---
name: msbuild-success-output-contains-error
description: A SUCCESSFUL msbuild run prints the word error ~35 times via /errorreport:prompt plus its own "0 Error(s)" summary, so a no-line-contains-error gate is unsatisfiable; match the diagnostic form instead
metadata:
  type: project
---

Measured 2026-09-01 against a recorded SUCCESSFUL analyzer build in this repo
(`docs/features/active/itemviewer-surface-defects-489/evidence/qa-gates/rem1-p4-t3-analyzer-build.2026-08-28T03-57.msbuild.txt`,
whose tail reads `0 Error(s)`): 35 lines match `error` case-insensitively. 34 of
them are the `/errorreport:prompt` token that appears on every Csc command line;
the 35th is MSBuild's own `0 Error(s)` summary. `Select-String` is
case-insensitive by default, so it matches regardless.

An acceptance condition of the form "exit code 0 and no console line containing
`error`" is therefore UNSATISFIABLE. It reads like a strict gate and can never
pass, which halts a correct executor.

**Use instead:**
- require a summary line matching `^\s*0 Error\(s\)$`, and
- require no line matching the MSBuild diagnostic form `: error [A-Z]+[0-9]+:`.
- for warnings, match `: warning [A-Z]+[0-9]+:` rather than the bare word.

**Companion trap, same family.** `Task "Csc"` is emitted only at DETAILED
verbosity, so asserting it over a default-verbosity console capture is likewise
unsatisfiable. Attach a file logger —
`"/flp:LogFile=coverage\<name>.msbuild.log;Verbosity=detailed"` — grep that log,
record the count, then delete the log. `coverage/.gitkeep` is tracked, so the
directory exists in a fresh checkout. Note that no detailed-verbosity MSBuild log
existed anywhere in this repo's recorded evidence as of 2026-09-01, so the
`Task "Csc"` literal is reasoned rather than measured; give the task a recovery
branch that records the actual task-started line if the count comes back zero.

**How to apply:** this is the general "observe the success-case output before
asserting over it" rule with a concrete instance. For any tool, read a recorded
SUCCESSFUL run before writing an assertion about what its output does not
contain. Related: [[msbuild-analyzer-gate-vacuous-without-rebuild]],
[[msbuild-non-vacuity-which-pattern-to-count]].
