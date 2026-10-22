# Fail-Before Exception Dossier — Defect 1 (retry exhaustion reports success)

Timestamp: 2026-08-31T19-40

Scope: this dossier covers **defect 1 only**, the retry-exhaustion path. Defect 2, the mid-write success report, has a genuine failing pre-fix run and needs no exception; that run is recorded at `docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/evidence/regression-testing/p3-t2-midwrite-fail-before.md`.

## WhyFailingRunImpossible

A test asserting that `WriteTextFileAsync` returns `false` after exhausting its retry budget can only be written against the post-fix signature, because the pre-fix method returns the non-generic `Task` and therefore carries no value an assertion could read. The signature change from `Task` to `Task<bool>` is itself the fix for this defect, so any test capable of failing before the fix would first have to introduce the fix. There is no ordering of the two in which the test fails against unfixed source.

## Alternative proof

The pre-fix behavior of defect 1 is established by two artifacts, both produced before any part of the fix landed.

1. **Pre-change source record.** `docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/evidence/baseline/p1-t1-pre-change-loop.md` quotes `UtilitiesCS/To Depricate/FileIO2.cs` lines 63 through 88 verbatim and records the exhaustion branch: line 84 logs `$"Failed to write to {filepath} after {attempts} attempts."` with no exception argument, and line 85 then assigns `success = true`. Assigning that flag is what terminates the `while (!success)` loop at line 63, so the method returns normally after a write that never happened. The caller receives a completed `Task` and has no observable signal distinguishing it from a successful write.

2. **Pre-fix characterization run.** `docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/evidence/regression-testing/p3-t4-exhaustion-characterization.md` records a deterministic run against pre-fix source in which the always-failing open path invoked the writer factory exactly 100 times and the delay delegate exactly 99 times, and the method then returned normally with `await act.Should().NotThrowAsync();` passing. The full 100-attempt budget was consumed, every attempt failed, and the method still produced no failure signal of any kind. That is the defect, measured rather than asserted.

Together these establish, before the fix, both that the code assigns its success flag on the exhaustion path and that the exhausted path is observably indistinguishable from success at runtime.

## Post-fix counterpart

After Phase 4 and Phase 5, the same test method asserts `exhaustionResult.Should().BeFalse();` alongside the unchanged invocation-count assertions, and is recorded Passed in `docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/evidence/qa-gates/p5-t8-scoped-tests.md`. The pass-after evidence for defect 1 is therefore complete even though the fail-before evidence is necessarily this dossier rather than a failing run.
