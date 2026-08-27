# [P0-T16] Baseline formatting check

Timestamp: 2026-08-27T09-45
Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

## Output (verbatim)

```
Checked 1540 files in 5376ms.
```

CSharpier 1.2.6 emits one warning line per unformatted file before its summary line. No such line was
emitted, and the exit code is 0.

```
BaselineUnformattedFiles = 0
```

## Acceptance evaluation

- The artifact records `Command:`, `EXIT_CODE:`, and an `Output Summary:` containing a numeric
  `BaselineUnformattedFiles`. PASS.
- `BaselineUnformattedFiles` is `0`, so the conditional clause requiring every unformatted path to be
  listed does not apply.
- The explicit environment-halt branch is **not taken**. That branch fires only when
  `BaselineUnformattedFiles` is non-zero and a listed path lies outside this feature's seven owned
  `.cs` paths. With a zero baseline there is no such path, so `[P4-T2]`'s repository-wide absolute `0`
  is reachable: `[P4-T1]` formats only the owned paths, and every other file is already formatted.

`BLOCKED: pre-existing unformatted files outside this feature's ownership` is **not** written.

Output Summary: `BaselineUnformattedFiles = 0` across 1540 checked files; exit code 0; no halt; the
`[P4-T2]` repository-wide zero gate is satisfiable.
