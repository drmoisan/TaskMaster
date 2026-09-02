# P2-T2 — CSharpier check (verify, read-only), remediation cycle 1

Timestamp: 2026-09-02T01-32

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

## Output Summary

```
Checked 1575 files in 4937ms.
```

This is a read-only check command, so its exit code is a real signal: CSharpier `check` exits
non-zero when any file needs formatting and 0 when none does.

## Acceptance clauses

| # | Clause | Result |
|---|---|---|
| 1 | `EXIT_CODE:` is recorded | PASS — 0 |
| 2 | the reported set of files needing formatting contains no path under `QuickFiler/` or `QuickFiler.Test/` | PASS — the set is empty, so it contains no such path |
| 3 | that set is either empty with exit 0, or a subset of `R_BASELINE_FORMAT_DRIFT` restricted to paths P2-T1 restored | PASS via the first branch — the set is **empty** and the exit code is **0** |

The reported set is empty. CSharpier prints one `Error ---------------------- <path>` block per
non-conforming file before the summary line; the captured output contains no such block, only
the summary line, which is consistent with the exit code of 0.

The second branch of clause 3 does not apply and no `REMEDIATION-REQUIRED:` line is written.
That branch exists for the case where P2-T1 had to restore an out-of-prefix path to its
base-ref content, leaving that path non-conforming and a zero exit unreachable without
editing outside the footprint. P2-T1 restored no path, because it rewrote no path outside the
two permitted prefixes, so the conflict that branch handles did not arise.

`R_BASELINE_FORMAT_DRIFT` from P0-T5 was itself the empty set, so the whole tree was
CSharpier-clean at baseline and is CSharpier-clean now.
