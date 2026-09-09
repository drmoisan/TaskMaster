# QC loop final clean pass (Issue #824, task P5-T14)

Timestamp: 2026-09-09T16-16

EXIT_CODE: 0

## The five loop steps in order, with the final pass exit code and artifact for each

| # | Step | Command | `EXIT_CODE` | Artifact |
|---|---|---|---|---|
| 1 | Format | `dotnet tool run csharpier format .` | 0 | `evidence/qa-gates/csharpier-format.2026-09-09T15-58.md` |
| 2 | Format verify | `dotnet tool run csharpier check .` | 0 | `evidence/qa-gates/csharpier-check.2026-09-09T15-59.md` |
| 3 | Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | `evidence/qa-gates/msbuild-analyzers-final.2026-09-09T16-02.md` |
| 4 | Nullable | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 | `evidence/qa-gates/msbuild-nullable-final.2026-09-09T16-03.md` |
| 5 | Coverage-enabled tests | the Coverage Command Of Record, `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/post-change.cobertura.xml` | 0 | `evidence/qa-gates/coverage-post-change.2026-09-09T16-08.md` |

Supporting artifacts for the same pass:
`evidence/qa-gates/format-scope-gate.2026-09-09T16-00.md`,
`evidence/qa-gates/msbuild-analyzers-nonvacuity.2026-09-09T16-02.md`,
`evidence/qa-gates/msbuild-nullable-nonvacuity.2026-09-09T16-03.md`,
`evidence/qa-gates/ac11-named-tests.2026-09-09T16-09.md`,
`evidence/qa-gates/coverage-classes-post-change.2026-09-09T16-10.md`,
`evidence/qa-gates/coverage-delta.2026-09-09T16-12.md`,
`evidence/qa-gates/file-line-counts-final.2026-09-09T16-13.md`,
`evidence/qa-gates/static-gates-revalidated.2026-09-09T16-15.md`.

No step in this phase recorded `EXIT_CODE: SKIPPED`.

## Loop restarts

**One restart.** The first execution of P5-T1 reported `FORMAT_CHANGED_TREE=True`: CSharpier
reflowed the hand-written line wrapping of this feature's own new code in both source files. Per the
Phase 5 preamble the loop restarted at P5-T1. The second execution reported
`FORMAT_CHANGED_TREE=False`, and steps 2 through 5 then ran once each without a further restart.

No step after step 1 failed, and no step after step 1 modified a tracked file, so no further restart
was triggered.

## No step in the final pass modified a tracked file

This statement is supported by two observations, both required because neither alone covers the
whole pass.

**Citation 1 — for step 1, from P5-T1.** The final P5-T1 pass recorded
`FORMAT_CHANGED_TREE=False`, computed by comparing the complete `git diff HEAD` patch text captured
immediately before the formatter with the same patch text captured immediately after it. The
`---PORCELAIN---` listing captured in the same invocation enumerated 37 paths, every one of which
P5-T3 classified as satisfying a D6 class. A full-patch comparison is used rather than a name-list
comparison because the formatter can rewrite a file this plan had already changed, which would leave
the name list identical.

**Citation 2 — for steps 2 through 5, from P5-T13.** The re-run of the P4-T8 scope gate recorded in
`evidence/qa-gates/static-gates-revalidated.2026-09-09T16-15.md` was taken after all five steps
completed. It reports 49 paths in both the anchored name-status listing and the porcelain listing,
with zero paths outside the three D6 classes, and it re-confirms that the four owned-but-unmodified
files still produce an empty anchored diff and their exact baseline line counts, 489 and 299.

Both citations are required because the P5-T1 observation is captured immediately after the
formatter and therefore predates the analyzer build, the nullable build and the coverage run, while
the P5-T13 re-run postdates all five steps. The rise from 37 paths to 49 between the two is fully
accounted for by evidence artifacts written between them, each of which is D6 class 2; no source
file entered the footprint after step 1.

## Non-vacuity of the two msbuild gates

| Gate | `Skipping target "CoreCompile"` | `^\s*CoreCompile:` | `: error [A-Z]+[0-9]+:` |
|---|---|---|---|
| Analyzers (step 3) | 0 | 12 | 0 |
| Nullable (step 4) | 0 | 13 | 0 |

Both gates compiled. The differing `CoreCompile:` counts are expected per plan D14 and are not a
defect. `/t:Rebuild` was used on both and `/p:Nullable=enable` was added to neither.

## Result

The final pass completed all five steps with exit code 0 and no file modified by the formatter,
which is what AC11 requires.
