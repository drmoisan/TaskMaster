# Phase 0 — Baseline Formatting Gate (issue #440, plan task P0-T10)

Timestamp: 2026-08-29T06-23

Command: `dotnet tool run csharpier check .` (run from the repository root through
`pwsh -NoProfile`, with the repository-local `.dotnet-sdk` on `PATH`)

EXIT_CODE: 0

## Output Summary

The tool printed its success-case `Checked` summary line verbatim:

```
Checked 1560 files in 4915ms.
```

- `BaselineCsharpierFileCount`: **1560**
- Files reported as needing reformatting: **none**. The exit code is 0 and the tool
  emitted no per-file report line, so the pre-existing-drift set is empty. P4-T2 may
  therefore be satisfied under its first branch (`EXIT_CODE: 0`).

## Ordering conditions this count depends on

- Taken **before** P0-T13 produces any coverage or test-result artifact, so it counts
  a tree carrying none of them. Global rule 8's `.cobertura.xml` naming rule is what
  keeps the P4-T2 count equal to this one.
- Taken **after** P0-T7 completed both its restore and its analyzer provisioning,
  which are the only writes this plan makes into the repository-root packages
  directory. Whatever those two steps placed there is already inside this count and
  will be inside the P4-T2 count alike, so the provisioning cannot move the two apart.
