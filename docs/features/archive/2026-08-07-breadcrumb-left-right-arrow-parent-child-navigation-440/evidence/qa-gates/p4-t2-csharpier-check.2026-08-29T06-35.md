# Phase 4 — Formatting Verification, Unscoped (issue #440, plan task P4-T2)

Timestamp: 2026-08-29T06-35

Command: `dotnet tool run csharpier check .` (run from the repository root)

EXIT_CODE: 0

## Output Summary

```
Checked 1560 files in 4734ms.
```

- Checked-file count: **1560**
- `BaselineCsharpierFileCount` recorded by P0-T10: **1560**
- The two counts are **equal**.

The counts are equal because this change adds no repository source file, and because
every artifact written between the two runs is one CSharpier does not read:

- the Cobertura documents are named `*.cobertura.xml` per Global rule 8 and are
  excluded by the `.csharpierignore` glob of that shape;
- the TRX files are excluded by the `*.trx` glob;
- the msbuild file-logger output written later in this phase is `.txt`, which
  CSharpier does not process;
- everything under this feature folder's `evidence/` subtree is excluded by the
  `**/evidence/**` glob.

The counts are also unaffected by the P0-T7 analyzer provisioning, because that task
is the only one in this plan that writes into the repository-root packages directory
and it completed before P0-T10. No other msbuild invocation in this plan names the
Restore target, and the test wrapper contains no restore and no build step, so that
directory is identical at both runs.

## Gate result

The gate's first branch is satisfied: `EXIT_CODE: 0`. The tool reported no file as
needing reformatting, which is consistent with the empty pre-existing-drift set that
P0-T10 recorded. None of the three owned paths appears in a reported set, because the
reported set is empty.
