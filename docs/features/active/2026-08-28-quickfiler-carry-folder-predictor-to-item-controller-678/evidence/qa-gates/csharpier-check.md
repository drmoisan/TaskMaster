# P2-T2 — CSharpier check (verify, read-only)

Timestamp: 2026-09-01T23-46

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

The command was run unconditionally.

## Output Summary

The run produced exactly one non-empty output line, reproduced verbatim:

```
Checked 1574 files in 4574ms.
```

The file count matches the 1574 the P2-T1 `format` run reported, so both commands saw the same file
set.

## Acceptance conditions

### 1. `EXIT_CODE:` is recorded

`EXIT_CODE: 0`, above. This is a read-only check command, so its exit code is a real signal:
`csharpier check` exits 1 when any file needs formatting and 0 when none does. The observed 0
therefore distinguishes a clean tree from a drifting one and is not the constant-0 outcome the
write-mode `format` command gives.

### 2. The reported set of files needing formatting contains no path under `QuickFiler/` or `QuickFiler.Test/`

**The reported set is empty**, so it trivially contains no such path. CSharpier emits one
`Error ./<path> - Was not formatted.` block per drifting file before its summary line; the captured
output contains no such block and no path of any kind. Every one of the 35 files this change touches
under those two prefixes is CSharpier-clean.

### 3. The set is either empty, in which case the exit code must be 0, or a subset of `BASELINE_FORMAT_DRIFT`

**The set is empty and the exit code is 0.** The first arm of the condition is satisfied.

The second arm, which would have required a `REMEDIATION-REQUIRED:` line recording a conflict between
AC19 and AC23 for paths P2-T1 restored, **is not reached**. That arm exists for the case where P2-T1
rewrote a file outside the `QuickFiler/` and `QuickFiler.Test/` prefixes and then restored it, leaving
that file reported as needing formatting while AC23 forbids editing it. P2-T1 rewrote no path at all,
restored no path, and `BASELINE_FORMAT_DRIFT` recorded by P0-T5 is itself the empty set, so no such
conflict exists and none is reported.

No `REMEDIATION-REQUIRED:` line is written, because writing one would assert a conflict that does not
exist.
