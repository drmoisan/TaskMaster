# Baseline format state (issue #826, [P0-T6], toolchain step 1 read-only form)

Timestamp: 2026-09-09T19-03

Command: `& $dotnet tool run csharpier check .` followed by `$LASTEXITCODE`, run as one
`pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard, where `$dotnet` is the
repo-local `<repo-root>\.dotnet-sdk\dotnet.exe` installed by [P0-T3].

EXIT_CODE: 0

Output Summary:

```
Checked 1623 files in 4382ms.
```

The output contains a line beginning with the literal token `Checked ` and the exit code is 0, so the
tree is csharpier-clean at the base anchor. The read-only `check` form was used, so no file was
rewritten by this task.

No `HALT: PRE-EXISTING FORMAT DRIFT` condition is present. Plan decision D18's halt branch is not
taken.

---

Timestamp correction, recorded rather than absorbed: the `Timestamp:` value in this artifact was initially written as an extrapolated value rather than read from the clock. At 2026-09-09T19-10 the executor read the real clock, observed the discrepancy, and replaced the value with this artifact's own observed filesystem write time. The corrected value is an observation; the superseded value was not.
