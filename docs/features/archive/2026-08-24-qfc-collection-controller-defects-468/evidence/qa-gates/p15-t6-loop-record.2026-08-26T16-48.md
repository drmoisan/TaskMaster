# [P15-T6] Final QA loop — restart-discipline record (AC-24)

Timestamp: 2026-08-26T16-48

Command:

```
# no command of its own; this artifact records the loop's control flow.
# Verification that no source file changed during the loop:
git status --porcelain | grep -E '\.(cs|csproj)$' | wc -l
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

**One pass. Five steps. Zero failures. Zero files rewritten. No restart was required.**

The General Code Change Policy requires the loop to restart from step 1 whenever any step fails or
changes a file. Neither condition arose, so the single pass recorded below is the final pass.

## Pass 1 — the only pass

| Step | Task | Command | Result | Files rewritten |
|---|---|---|---|---|
| 1. Format | P15-T1 | `dotnet tool run csharpier format <10 owned paths>` | `EXIT_CODE: 0`, `Formatted 9 files` (processed, not changed) | **0**, proven by SHA-256 on all 10 paths before and after |
| 1a. Format verify | P15-T2 | `dotnet tool run csharpier check .` | `EXIT_CODE: 0`, `Checked 1530 files`, zero reported unformatted | 0 (read-only) |
| 2. Lint | P15-T3 | `Invoke-VSBuild.ps1 -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild` | `EXIT_CODE: 0`, 0 errors, 5 warnings, 0 analyzer diagnostics, 18 projects compiled | 0 |
| 3. Type check | P15-T4 | `Invoke-VSBuild.ps1 -Target Rebuild -TreatWarningsAsErrors` | `EXIT_CODE: 0`, 0 errors, 0 `CS86xx`, 18 projects compiled | 0 |
| 4. Test | P15-T5 | `Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput <path>` | `EXIT_CODE: 0`, 6581 total, 6581 passed, 0 failed, 0 skipped; line-rate 84.9435%, branch-rate 78.9377% | 0 |

The steps ran in the mandated order: format, then lint, then type check, then test. No step was
skipped and none recorded `EXIT_CODE: SKIPPED`.

## Restart-condition evaluation

The loop restarts if **either** condition holds at any step. Both were evaluated after every step:

### Condition A — did any step fail?

| Step | Exit code | Failed? |
|---|---|---|
| P15-T1 | 0 | no |
| P15-T2 | 0 | no |
| P15-T3 | 0 | no |
| P15-T4 | 0 | no |
| P15-T5 | 0 | no |

No step failed. The one measurement that could have been mistaken for a failure — CSharpier's
`Formatted 9 files` — is a processed count, not a change count, and the SHA-256 comparison in
`p15-t1-format.2026-08-26T16-43.md` shows all ten hashes identical across the run.

### Condition B — did any step change a file?

Measured two independent ways:

1. **SHA-256 before and after the format step**, for all ten owned paths. All ten identical.
   Recorded in full in `p15-t1-format.2026-08-26T16-43.md`.
2. **`git status --porcelain` filtered to `.cs` and `.csproj` paths**, evaluated after the last step
   of the pass: **0 matching entries.** No source or project file in the whole tree is modified.

The working tree after the pass contains only Markdown evidence artifacts written by this phase, the
coverage XML written by P15-T5, the plan file with its checkbox updates, and the untracked
`.claude/state/` directory. None of those is an input to any of the five steps, so none of them could
invalidate the pass.

## Why one pass is sufficient here rather than lucky

The owned file set entered Phase 15 already formatter-clean, analyzer-clean, and green, because each
of Phases 1 through 13 ran its own format, analyzer, nullable, and suite gates before committing.
Phase 15 is a whole-tree re-verification against the merged tree — including sibling feature 498's
changes, which had never been built together with this feature's changes before the integration
merges. That is the risk Phase 15 exists to retire, and it is retired: 18 projects recompiled from
clean with analyzers loaded, and 6,581 tests passed with zero failures.

## Acceptance verification

| Clause | Status |
|---|---|
| the artifact enumerates every pass performed | met — one pass, tabulated step by step |
| the final pass completed all five steps with no failure | met — five exit codes, all `0` |
| the final pass completed all five steps with no file rewritten | met — 0 rewrites by SHA-256; 0 `.cs`/`.csproj` entries in `git status --porcelain` |
