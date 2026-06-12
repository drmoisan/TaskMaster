# Cycle 2 End-State Summary (F3 / AC20)

Timestamp: 2026-06-12T17:14Z

## Outcome
The single in-scope finding F3 (AC20: LcppnFolderPredictor_Tests.cs over the 500-line cap)
is remediated by splitting the file by behavior into two cohesive test files, each <= 500
lines, preserving every test case and LcppnFolderPredictor strict coverage.

## Verified criteria
- Both resulting test files are <= 500 lines:
  - LcppnFolderPredictor_Tests.cs = 316 lines
  - LcppnFolderPredictor_Classify_Tests.cs = 287 lines
  (split-verification.2026-06-12T16-45.md)
- All 21 original test cases preserved (14 config/validation/training/untrain/build in
  File A incl. two [DataTestMethod]s; 9 Classify_* in File B); none dropped or renamed.
  LcppnFolderPredictor scoped test run 33/33 passed (21 in-scope predictor cases + 10
  serialization). (p1-test / final-toolchain artifacts)
- LcppnFolderPredictor strict coverage >= 90%: post-change 97.71% line / 97.58% block,
  identical to baseline, no regression on changed lines. UtilitiesCS.dll module line
  coverage 85.46% (>= 80% floor). (coverage-delta.2026-06-12T16-45.md)
- Containment held: zero diff to ManagerAsyncLazy.cs (and the shared Manager value type),
  Triage.cs, SpamBayes.cs, CategoryClassifierGroup.cs, MulticlassEngine.cs; only the two
  test files and UtilitiesCS.Test.csproj changed; no production .cs modified.
  (containment.2026-06-12T16-45.md)
- Full C# toolchain in a single final pass: CSharpier EXIT 0, analyzers EXIT 0 (build
  succeeded), nullable/TWAE EXIT 0 (0W/0E). Tests 3903/3904; the single failure is the
  documented out-of-scope pre-existing flake (passes 1/1 in isolation), not introduced by
  this work and explicitly excluded by the cycle-2 inputs. (final-toolchain.2026-06-12T16-45.md)

## Linked artifacts
- evidence/qa-gates/split-verification.2026-06-12T16-45.md (P1-T5)
- evidence/qa-gates/final-toolchain.2026-06-12T16-45.md (P2-T1)
- evidence/qa-gates/coverage-delta.2026-06-12T16-45.md (P2-T2)
- evidence/qa-gates/containment.2026-06-12T16-45.md (P2-T3)
- artifacts/csharp/coverage.xml (canonical post-change coverage)
