# P9-T1 — Final QC step 1: formatting (#614; AC24 step 1)

Timestamp: 2026-08-26T19-00

Command 1: `dotnet tool run csharpier format .`
Command 2: `dotnet tool run csharpier check .`

Both invoked through `dotnet tool run` so the manifest-pinned CSharpier is used; no globally
installed CSharpier was invoked.

EXIT_CODE: 0 (format) and 0 (check)

## Output Summary

- `dotnet tool run csharpier format .` → `Formatted 1530 files in 4505ms.` EXIT_CODE 0.
  Note: CSharpier's "Formatted N files" is a PROCESSED count, not a rewrite count.
- `dotnet tool run csharpier check .` → `Checked 1530 files in 3944ms.` EXIT_CODE 0, i.e. zero
  files would be changed by formatting. This is CI parity.
- Non-rewrite proof: immediately after the repo-wide format pass, `git status --porcelain` reported
  only this change's own documentation and evidence paths and **no `.cs`, `.csproj`, `.xml`, or
  `packages.config` file at all**:

```
 M docs/features/active/2026-08-26-.../evidence/regression-testing/p1-t4-producer-companion-fail-before.2026-08-26T16-05.md
 M docs/features/active/2026-08-26-.../plan.2026-08-26T09-59.md
?? docs/features/active/2026-08-26-.../change-description.2026-08-26.md
?? docs/features/active/2026-08-26-.../evidence/qa-gates/
```

  (Every source file this change touched was formatted incrementally during Phases 1-8 and
  committed, so the repo-wide pass had nothing left to rewrite.)
- **No out-of-scope file was rewritten.** The Phase 0 baseline check also exited 0, so the repo was
  already format-clean before this change; the repo-wide pass therefore could only have rewritten
  files this change touched, and it rewrote none.
