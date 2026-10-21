# P7-T2 — Toolchain step 1 verification (CSharpier check)

Timestamp: 2026-09-08T10-06
Task: [P7-T2]
Command: dotnet tool run csharpier check .
EXIT_CODE: 0
Toolchain pass: 3

## Output Summary

Summary line, verbatim:

```
Checked 1616 files in 7454ms.
```

The line begins with `Checked ` and ends with `ms.`, as the acceptance condition requires. Exit
code 0 with no path reported as unformatted.

This is the read-only CI-parity check that pairs with the P7-T1 write-mode format pass: `format`
exits 0 whether or not it rewrote anything, while `check` exits non-zero when any file would be
rewritten. Its passing here independently confirms the `REWRITTEN_COUNT: 0` observation P7-T1
recorded from file hashes.

The processed-file count is 1616, three higher than the 1613 P0-T7 recorded, which is exactly the
three new `.cs` files this plan created
(`DfDeedleEtlTimeoutTests.cs`, `OlTableExtensionsEtlClockTests.cs`, `ArmingBarrierTimeProvider.cs`).

## Acceptance evaluation

- `EXIT_CODE: 0`. PASS
- The summary line is quoted verbatim, begins with `Checked ` and ends with `ms.`. PASS
