# P6-T2 — Read-Only Format Check (governing terminating observation)

Timestamp: 2026-08-31T20-18
Command: dotnet tool run csharpier check .
EXIT_CODE: 0
Iteration: 1

Output Summary: the tool's final summary line, transcribed verbatim:

```
Checked 1565 files in 4764ms.
```

That line is recorded, not asserted over. The exit code is the gate.

## Why this exit code is the governing observation

`check` is read-only and returns a non-zero exit code when any target file is unformatted, so its exit code alone distinguishes a clean tree from a drifted one. It observes the same repository-wide CSharpier target set that P6-T1 wrote over — 1565 files under both invocations — so the format step's success is decided by one observation over one identical set rather than by inferring anything from the write-mode command's own exit code, which is 0 either way.

This task has no carried-blocker branch available to it. A read-only format check carries no pre-existing-blocker allowance anywhere in this plan, and none is needed: P0-T12 measured the branch head as formatter-clean and P6-T1 wrote over the whole target set, so an exit code of 0 is the only outcome that satisfies this gate. It is the observed outcome.

P7-T21 reads this artifact directly and requires exactly this: `EXIT_CODE:` 0, with no carried-blocker alternative.
