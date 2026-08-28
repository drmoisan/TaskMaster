# P11-T13 — Toolchain-loop iteration history

Timestamp: 2026-08-28T02-34
Command: (reads the eleven P11-T2 through P11-T12 artifacts written during this phase; no new command is required by this task)
EXIT_CODE: 0

## Iterations required: 1

The loop restarts from P11-T2 whenever any stage fails **or rewrites a file**. Neither happened, so
one iteration was sufficient and no restart occurred.

## Iteration 1 — every stage, in the order run

| # | Task | Stage | Command | Result | File rewritten? |
|---|---|---|---|---|---|
| 1 | P11-T2 | Format | `dotnet tool run csharpier format .` | exit 0, `Formatted 1547 files` | **No** — 0 of 1868 hashed files changed SHA-256; porcelain over the 19-directory project set returned 0 lines |
| 2 | P11-T3 | Format check | `dotnet tool run csharpier check .` | exit 0, `Checked 1547 files`, `FinalUnformattedSet:` empty | No — read-only |
| 3 | P11-T4 | Lint / analyze | `msbuild TaskMaster.sln /t:Rebuild … /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal /fl` | exit 0, `Build succeeded.`, 5 warnings, 0 errors | No — writes only gitignored `bin/`, `obj/` and the evidence log |
| 4 | P11-T5 | Lint non-vacuity | `Select-String -SimpleMatch 'Skipping target "CoreCompile"' … | Measure-Object` | count 0 | No — read-only |
| 5 | P11-T6 | Type check | `msbuild TaskMaster.sln /t:Rebuild … /p:TreatWarningsAsErrors=true` | exit 0, 5 warnings, 0 errors, 0 CS86xx | No — writes only gitignored `bin/` and `obj/` |
| 6 | P11-T7 | Test (scoped) | `vstest QuickFiler.Test.dll /EnableCodeCoverage /InIsolation` | exit 0, 1121 passed / 0 failed / 0 skipped | No |
| 7 | P11-T8 | Test (repo-wide, coverage) | `Invoke-MSTestWithCoverage.ps1 -SearchRoot .` — run 1 and the clause-(a) re-execution run 2 | exit 0 both runs, 6741 passed / 0 failed / 0 skipped; line rate 0.851599 and 0.851567 at `lines-valid=63901` | No tracked file — `coverage/coverage.cobertura.xml` is gitignored |
| 8 | P11-T9 | Coverage, new member | Cobertura read of `CbxPictures_CheckedChanged` | `NewMemberLineRate: 1.0` | No — read-only |
| 9 | P11-T10 | Exclusion-attribute recount | `git grep -n -E "\[(System\.Diagnostics\.CodeAnalysis\.)?ExcludeFromCodeCoverage\]" -- "*.cs" | Measure-Object` | 261, equal to baseline | No — read-only |
| 10 | P11-T11 | Line-count audit | `(Get-Content -LiteralPath <path>).Count` over 30 paths | all three parts satisfied | No — read-only |
| 11 | P11-T12 | Test (unfiltered guard) | `vstest QuickFiler.Test.dll /InIsolation` | exit 0, 1121 passed / 0 failed; guard test `Passed` | No |

**No row records a skipped stage.** Every stage in the table executed its command and recorded a
result; `EXIT_CODE: SKIPPED` appears in none of the eleven artifacts.

## Why no restart was triggered

The two restart conditions were evaluated explicitly at each stage rather than assumed:

- **No stage failed.** Every command-bearing stage exited `0`, and every gate's own acceptance
  condition — which in four cases is deliberately not the exit code — was met.
- **No stage rewrote a file.** The format stage is the only one that can rewrite tracked source, and
  it was measured by SHA-256 over an 1868-file superset of CSharpier's target set: the before and
  after manifests are byte-identical. The measurement is deliberately not keyed on CSharpier's
  "Formatted N files" line, which is a processed count rather than a rewritten count and would never
  let such a loop terminate. Independently, `git status --porcelain` over the full nineteen-directory
  C# project set returned zero lines immediately after the format pass, and the working tree was
  clean before it because P10-T18 had committed every source change — so a rewrite anywhere in the
  project set would have shown.

## The one re-execution, and why it is not a loop restart

P11-T8 was executed twice. That second execution is **not** a loop restart and does not make this a
two-iteration loop: it is the re-run that P11-T8's own clause (a) mandates when the observed
`lines-valid` diverges from the baseline by more than 5 percent, performed inside that single task.
The first run did not fail — it exited `0` — and it rewrote no tracked file. Both runs are recorded
in the P11-T8 artifact, and they agreed on the denominator to the line (63901 both times) and on the
line rate to within 0.000032, which is what established that the divergence from the baseline
`lines-valid=82070` is a document-shape change rather than the parallelism-sensitive merge
instability the clause was written to detect.

## Final iteration state

Iteration 1 is also the final iteration. It shows **P11-T2 through P11-T12 all passing, with no file
rewritten** — the exact condition this task's acceptance requires of the final recorded iteration.

Output Summary: The Phase 11 toolchain loop completed in **one iteration** with **zero restarts**.
All eleven stages — format, format check, analyzer, analyzer non-vacuity, nullable/type check,
scoped test, repo-wide coverage test, new-member coverage, exclusion-attribute recount, line-count
audit and the unfiltered guard run — executed their commands and passed, and no row records a skipped
stage. No restart was triggered because no stage failed and no stage rewrote a file: the format pass
left all 1868 hashed files byte-identical and the post-format porcelain over the nineteen-directory
C# project set returned zero lines. P11-T8's second execution is the re-run its own clause (a)
mandates, not a loop restart; both of its runs exited `0` on the identical denominator.
