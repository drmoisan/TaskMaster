# Git Baseline — 2026-08-10-cobertura-coverage-arithmetic-441

Timestamp: 2026-08-10T22-30

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
git rev-parse --abbrev-ref HEAD
git rev-parse HEAD
git rev-parse edf3d34c
git merge-base --is-ancestor edf3d34c HEAD; 'ancestor-check-exit=' + $LASTEXITCODE
git status --porcelain
```

EXIT_CODE: 0

Output Summary:

```
bug/cobertura-coverage-arithmetic-441
a5e336e5ae3443d4197caf5f87036fae1d538f89
edf3d34cb9cd455bd3c1d9f5ee363b825632073c
ancestor-check-exit=0
--- porcelain ---
 M docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/plan.2026-08-10T14-07.md
?? docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/evidence/
--- end porcelain ---
```

## Assessment

- **Branch:** `bug/cobertura-coverage-arithmetic-441` — matches the plan's required branch exactly.
  No branch is created, switched, or renamed by this plan.
- **HEAD:** `a5e336e5ae3443d4197caf5f87036fae1d538f89`. Recorded as an observation only; it is never
  used as a later expectation.
- **Base commit:** `edf3d34c` resolves to `edf3d34cb9cd455bd3c1d9f5ee363b825632073c`.
  `git merge-base --is-ancestor edf3d34c HEAD` exited **0**, confirming the base is an ancestor of
  HEAD, so `git diff ... edf3d34c` gates are well-defined.
- **Working-tree state at baseline:** the plan file
  `plan.2026-08-10T14-07.md` is already ` M` (line-ending normalization performed by
  `epic-orchestrator` before delegation) and the feature `evidence/` directory is untracked because
  this run created it. Both source files under `scripts/` and `tests/` are clean at baseline.
