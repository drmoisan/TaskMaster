# Final formatting step (issue #968, task P8-T1)

Timestamp: 2026-10-03T03-27
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); dotnet tool run csharpier format .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'
Canonical command: dotnet tool run csharpier format . (at the worktree root), bracketed by git -C WORKTREE status --porcelain --untracked-files=all and CMD-HASH on CS13 before and after
EXIT_CODE: 0
Output Summary:
- ITERATION: 1
- WORKTREE-LEAF: agent-a291a7fbabf9d0229 (format payload and both CMD-HASH payloads)
- CSHARPIER_EXIT_CODE: 0
- `Formatted 1640 files in 5265ms.` (a processed-file count, not a rewrite count)
- REWRITTEN-WRITESET: NONE (all thirteen CS13 hashes identical before and after; the values equal the P6-T1 after-hashes recorded in FEATURE/evidence/qa-gates/scoped-format.md)
- REWRITTEN-OTHER: NONE (porcelain before and after list the same four lines: ` M` FEATURE/plan.2026-10-02T05-42.md and `??` FEATURE/evidence/qa-gates/call-site-census.md, implementation-commit.md and prohibited-constructs-grep.md)
- Clean pass: both NONE; no D-13 format restart.
