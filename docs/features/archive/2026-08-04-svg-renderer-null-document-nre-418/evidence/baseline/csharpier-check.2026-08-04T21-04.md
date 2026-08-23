# [P0-T6] Baseline Formatting State (csharpier check) — re-capture on VSTO-enabled host

Timestamp: 2026-08-04T21-04

Issue: #418
Plan: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md`
Task: `[P0-T6]`
Branch: `bug/svg-renderer-null-document-nre-418`
HEAD: `a5695656e711f98a8ae6ad334115c0f8666c509f`
csharpier version: `1.2.6`

## Command

```
dotnet tool run csharpier check .
```

EXIT_CODE: 0

## Output Summary

Files needing formatting: **0**.

Full tool output:
```
Checked 1462 files in 3939ms.
```

- csharpier reported no unformatted file and no parse error. Exit code 0 confirms zero formatting drift.
- The scan covers the whole repository from the root, which includes `SVGControl/SvgRenderer.cs` and
  every file under `SVGControl.Test/` (the files this plan is permitted to change).
- 1462 files were checked. This is the post-`ce0c91e6` file set at HEAD `a5695656`.
- Baseline formatting state is therefore clean; any formatting drift observed in Phase 2 is
  attributable to this change alone.
