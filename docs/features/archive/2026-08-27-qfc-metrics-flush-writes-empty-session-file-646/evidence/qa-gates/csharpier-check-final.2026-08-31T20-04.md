# QA Gate — CSharpier Check, Final (P2-T2)

Timestamp: 2026-09-01T12-53

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

## Verbatim Output

```
Checked 1566 files in 4843ms.
```

## Acceptance

| Condition | Required | Observed | Met |
|---|---|---|---|
| `EXIT_CODE` | `0` | `0` | Yes |

ACCEPTANCE: MET.

## Output Summary

The read-only, CI-parity format verification passes. CSharpier 1.2.6 (manifest-pinned via
`dotnet-tools.json` and invoked through `dotnet tool run`, never a global install) checked
1566 files and printed no `Error` line for any file. In check mode CSharpier emits one
`Error ...` line per non-compliant file before the summary; none was printed, and the exit
code is 0.

This is the read-only confirmation of the P2-T1 pass-2 fixpoint: the format gate is
independently observable here through the exit code, not only through the tree-comparison
that P2-T1 relies on.

The file count (1566) is unchanged from the P0-T7 baseline, confirming this change added no
file to CSharpier's scope. The evidence artifacts this item writes are outside that scope by
`.csharpierignore`, which excludes `**/evidence/**` and `*.cobertura.xml`, so they cannot
perturb this gate.
