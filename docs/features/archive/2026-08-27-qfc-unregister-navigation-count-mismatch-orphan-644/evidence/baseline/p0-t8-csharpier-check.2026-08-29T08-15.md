# Baseline — CSharpier formatting gate ([P0-T8])

- Issue: #644
- Task: `[P0-T8]`
- Timestamp: 2026-08-29T08-15

Command: `dotnet tool run csharpier check .`
Working directory: repository root (`<repo-root>`)
EXIT_CODE: 0

This is the read-only verify form. It exits non-zero and names each unformatted file when the
tree has drift, so exit 0 with no named file is a genuine clean result rather than an
unobservable one.

Output:

```
Checked 1561 files in 5215ms.
```

## PRE-EXISTING FORMAT DRIFT SET

```
none
```

The command exited 0 and named **no unformatted file**, so the pre-existing format drift set is
empty. `[P4-T1]` consumes this empty set: because it is empty, the union that `[P4-T1]`'s
acceptance is evaluated against reduces to the before-listing, the six code paths of the change
footprint, and any path under `.claude/agent-memory/`. `[P4-T8]`'s conditional clause — that a
member of this set lying under `QuickFiler` or `QuickFiler.Test` is `REMEDIATION-REQUIRED` — is
vacuously satisfied for the same reason.

Output Summary: CSharpier 1.2.6, invoked through `dotnet tool run` so the manifest-pinned version
is used, checked 1561 files and found **no unformatted file**. EXIT_CODE 0. The
`PRE-EXISTING FORMAT DRIFT SET` is empty, so any file the `[P4-T1]` formatter rewrites is
attributable to this change rather than to pre-existing drift.
