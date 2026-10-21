# P8-T6 — Toolchain Completed in ONE Clean Pass (AC-29)

Timestamp: 2026-08-26T11-26

Command: `pwsh -NoProfile -Command 'Select-String -Path "evidence/qa-gates/p8-t1-csharpier-format.md","evidence/qa-gates/p8-t2-csharpier-check.md","evidence/qa-gates/p8-t3-analyzer-rebuild.md","evidence/qa-gates/p8-t4-nullable-rebuild.md","evidence/qa-gates/p8-t5-coverage-test.md" -Pattern "^EXIT_CODE:" | ForEach-Object { "{0}: {1}" -f (Split-Path $_.Path -Leaf), $_.Line }; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

**Final pass number: 3. Passes 1 and 2 were superseded; pass 3 is a single CLEAN pass in which
`P8-T1` rewrote ZERO files and `P8-T2` through `P8-T5` each met their PRIMARY acceptance condition with
no degradation.**

### The four toolchain outcomes — final pass (pass 3)

| Step | Task | Command class | Result | Degradation used |
|---|---|---|---|---|
| 1 (mutating) | `P8-T1` | `dotnet tool run csharpier format <16 files>` | `EXIT_CODE: 0`; **0 files rewritten** (SHA-256 before/after) | n/a — task has no degradation branch |
| 1 (verify) | `P8-T2` | `dotnet tool run csharpier check .` | `EXIT_CODE: 0`; 1525 files checked; no unformatted file | **NONE** |
| 2 | `P8-T3` | analyzer Rebuild (`/t:Rebuild`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`) | `EXIT_CODE: 0`; 0 errors, 5 warnings | **NONE** |
| 3 | `P8-T4` | nullable Rebuild (`/t:Rebuild`, `TreatWarningsAsErrors`) | `EXIT_CODE: 0`; 0 errors, 5 warnings | **NONE** |
| 4 | `P8-T5` | full-suite vstest with Cobertura coverage | `EXIT_CODE: 0`; 6514/6514 passed, 0 failed; line rate 84.83% | **NONE** |

The verification command above re-read the `EXIT_CODE:` line from each of the five artifacts on disk:
all five report `0`.

### Pass history and why two restarts occurred

| Pass | What ran | Outcome | Why it restarted |
|---:|---|---|---|
| 1 | `P8-T1` to `P8-T5` | All five clean: 0 rewrites, exit 0 everywhere, 6511/6511 passed, line rate 84.81% | `P8-T7` then measured `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` changed-line coverage at **89.56%**, below the task's 90.00 percent floor. `P8-T7` directs the executor to add tests and re-run, so three tests were appended to the owned file `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs`, covering the three newly authored guard branches that were uncovered (`WithFilingTarget` early return, `GetActiveChild` invalid-request null, `TryExpandActiveSegment` no-affordance false). Changing a source file invalidates pass 1. |
| 2 | `P8-T1` only | `EXIT_CODE: 0`, but **1 file rewritten** (`BreadcrumbStateModelTests.cs`) — CSharpier reflowed the appended tests | The restart rule fires as soon as step 1 rewrites a file, so steps 2 to 4 were not run for this pass. Running them would have measured a tree that step 1 had just changed. |
| 3 | `P8-T1` to `P8-T5` | All five clean: 0 rewrites, exit 0 everywhere, 6514/6514 passed, line rate 84.83% | Terminal. No restart trigger fired. |

CSharpier is idempotent, which is why pass 3 rewrote zero files and the loop terminated rather than
oscillating.

### Restart analysis for the final pass

The task's restart condition has two triggers. Neither fired on pass 3:

1. **Did `P8-T1` rewrite any file?** No. The rewrite count was determined by comparing a SHA-256 hash of
   each of the 16 target files taken immediately before the command with one taken immediately after.
   Zero hashes changed (`REWRITTEN_COUNT: 0`). CSharpier's own `Formatted 16 files` line is a PROCESSED
   count, not a rewrite count, and was deliberately not used for this determination.
2. **Did any of `P8-T2` through `P8-T5` fail its acceptance?** No. All four met their PRIMARY acceptance
   condition absolutely.

`P8-T7` was also re-run against the pass-3 coverage artifact, as its own text directs, and now passes:
every per-file changed-line figure is at or above 90.00 percent or is a `NOT APPLICABLE` row, and
repository-wide line coverage rose above the baseline.

### Degradation accounting

**No permitted baseline-comparison degradation was used by any of the four gates on any pass, and none
was available.** Every degradation branch in `P8-T2`, `P8-T3`, `P8-T4` and `P8-T5` is gated on the
corresponding Phase 0 baseline having been non-zero or non-empty, and every one of those baselines is
clean:

| Gate | Baseline artifact | Baseline value | Degradation available? |
|---|---|---|---|
| `P8-T2` | `evidence/baseline/p0-t12-csharpier-check.md` | `EXIT_CODE: 0`, unformatted set EMPTY | NO |
| `P8-T3` | `evidence/baseline/p0-t13-analyzer-rebuild.md` | `EXIT_CODE: 0`, 0 errors / 5 warnings | NO |
| `P8-T4` | `evidence/baseline/p0-t14-nullable-rebuild.md` | `EXIT_CODE: 0`, 0 errors | NO |
| `P8-T5` | `evidence/baseline/p0-t15-coverage-test.md` | `BASELINE_FAILURE_SET` EMPTY, exit 0 | NO |

No `ExpectedExitCode:` is declared in any of the five pass artifacts, because every observed exit code
is 0.

**AC-29 disposition: SATISFIED** — the mandatory toolchain loop (format, lint/analyze, type-check,
test) completed with a final pass in which every step was clean and step 1 rewrote zero files.
