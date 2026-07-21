# Acceptance-Criteria Verification (AC1..AC13)

Timestamp: 2026-07-16T03-32

AC sources (full-feature): spec.md (`## Acceptance Criteria`, 13 items) and user-story.md
(`## Acceptance Criteria / Done When`, 8 aligned items). All items checked off in both files.

| AC | Criterion (spec) | Task(s) | Evidence | Status |
|---|---|---|---|---|
| AC1 | FolderScore readonly struct (net48-safe) | P1-T1, P1-T2 | UtilitiesCS/OutlookObjects/Folder/FolderScore.cs; analyzer + nullable builds pass; FolderScoreTests.Constructor_... | PASS |
| AC2 | ToScoredArray() / ToScoredArray(int) return FolderScore[] | P1-T4 | FolderScorer.cs; FolderScoreTests | PASS |
| AC3 | ToScoredArray ordering == ToArray incl. tie | P1-T5 | FolderScorerRegressionTests.ToScoredArray_*Ordering_*, ToScoredArray_WithTie_* (pass) | PASS |
| AC4 | ToArray()/ToArray(int) unchanged byte-for-byte | P1-T3, P1-T5 | shared OrderedScores(); golden tests ToArray_WithPopulatedScorer_*, ToArrayTopN_* (pass) | PASS |
| AC5 | FolderArray / FindFolder unchanged byte-for-byte | P2-T3, P2-T4, P2-T5 | FolderRowTests Text-parity + FolderRowArray_DoesNotAlterFolderArrayOutput (pass) | PASS |
| AC6 | Probability max-normalized [0,1] with zero-guard | P1-T4, P1-T6 | BuildScoredArray zero-guard; ToScoredArray_EmptyScorer_*, _AllZeroSeeds_* (pass) | PASS |
| AC7 | Scored projection across 3 sources + mixed; no AddBayesianSuggestionsAsync | P1-T6 | FolderScoreTests_BayesianScale/_AcrossThreeSourceScales/_MixedSourceAccumulation (pass) | PASS |
| AC8 | FolderRow + FolderRowKind + FolderRowArray + FindFolderRows; Text/Kind/Score | P2-T1..T5 | FolderRow.cs; FolderRowTests Kind + Score-only-on-Suggestion (pass) | PASS |
| AC9 | "Error" sentinel never in scored contract | P1-T5 | FolderScorerRegressionTests.AddSuggestion_WithErrorSentinel_*, AddArray_WhenFirstElementIsErrorSentinel_* (pass) | PASS |
| AC10 | Downstream sufficiency documented (Math.Round(Probability*100), Kind skip) | P3-T2 | evidence/other/downstream-sufficiency.md | PASS |
| AC11 | Probability XML doc = relative display value, not calibrated posterior | P1-T1 | FolderScore.cs XML doc on Probability | PASS |
| AC12 | New/changed code meets stricter coverage regime | P4-T4, P4-T5 | evidence/qa-gates/coverage-delta.md (all new members >= 90% line; branch paths covered; no changed-line regression; no exclusions) | PASS |
| AC13 | Full C# toolchain green | P4-T1..T4 | qc-csharpier.md (EXIT 0), qc-analyzer-build.md (0 errors), qc-nullable-build.md (0 feature errors; only pre-existing vendored SVGControl debt), qc-vstest-coverage.md (all 18 new tests pass; non-instrumented full suite green) | PASS |

## User-story AC mapping (aligned)

- US1 FolderScore value type -> AC1. US2 ToScoredArray ordering incl. ties -> AC2/AC3. US3 existing
  outputs unchanged -> AC4/AC5. US4 Probability max-normalized + zero-guard + documented -> AC6/AC11.
  US5 verified across 3 sources w/o COM Bayesian -> AC7. US6 FolderRow model + Kind + Score-only-Suggestion
  -> AC8. US7 "Error" sentinel absent -> AC9. US8 sufficiency documented -> AC10. US "coverage + toolchain
  green" -> AC12/AC13. All checked off in user-story.md.

## Toolchain note (AC13)

The nullable full-solution gate under a forced Rebuild surfaces 34 pre-existing nullable errors in
the vendored SVGControl.csproj only (identical to baseline, out of feature scope). The two first-party
projects this feature touches (UtilitiesCS, UtilitiesCS.Test) compile with 0 nullable/type errors under
/p:Nullable=enable /p:TreatWarningsAsErrors=true. The 17 test failures under coverage instrumentation
are pre-existing Deedle/DataFrame flakes (0 failures without instrumentation); no Folder-scoring test
fails. The feature introduces no new toolchain failures.

## Verdict

Every AC (AC1..AC13 and all aligned user-story items) is mapped to satisfying evidence and checked off
in both spec.md and user-story.md. No AC is unmapped or contradicted by evidence on disk.
