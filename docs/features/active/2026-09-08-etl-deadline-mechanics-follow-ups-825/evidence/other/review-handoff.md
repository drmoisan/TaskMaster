# Review Handoff Index

Timestamp: 2026-09-09T17-37

## Read this first

**plan.2026-09-08T23-51.md, the section "Design conflict adjudicated by the orchestrator on
2026-09-09 (Branch A, decided)".** It records a measured conflict between AC6 and AC20, the
adjudication that closed it, and the rejected alternative retained so the choice stays reviewable.
A reviewer who reads the AC6 and AC20 amendments without that section will not be able to judge
whether the bounded edit to DfDeedleEtlTimeoutTests.cs was necessary.

Then read evidence/other/plan-deviations.md, which records every point at which execution departed
from the literal plan text, with the measurement that forced each departure.

## Baseline

- evidence/baseline/phase0-instructions-read.md — the seven policy files read, in the required order.
- evidence/baseline/base-commit.md — the D3 anchor sha used by every diff in this plan.
- evidence/baseline/restore.md — NuGet restore; proves the TimeProvider package is on disk.
- evidence/baseline/dotnet-tool-restore.md — local tool manifest restore; pins CSharpier 1.2.6.
- evidence/baseline/csharpier-check.md — formatter baseline, 1622 files, zero drift.
- evidence/baseline/build-analyzers.md — analyzer-gate baseline, 0 warnings, 0 errors.
- evidence/baseline/build-analyzers.txt — the detailed MSBuild log for that build, sanitised.
- evidence/baseline/build-nullable.md — nullable-gate baseline, 0 warnings, 0 errors.
- evidence/baseline/build-nullable.txt — the detailed MSBuild log for that build, sanitised.
- evidence/baseline/coverage-baseline.md — repository-wide coverage before the change.
- evidence/baseline/coverage-baseline.cobertura.xml — the processed Cobertura it reads.
- evidence/baseline/coverage-baseline-by-file.md — per-file coverage for the five edited production files.
- evidence/baseline/pinned-source-facts.md — the eleven measured facts every later transition is expressed against.

## Regression testing

- evidence/regression-testing/ac7-fail-before.md — the retry-literal test failing against the pre-change file, with the recorded 2000.
- evidence/regression-testing/phase3-green.md — the same test passing, plus the three other new tests and the three DfDeedle-path tests.
- evidence/regression-testing/phase4-green.md — green after deleting the inert overloads and the dead method.
- evidence/regression-testing/phase5-green.md — green after the EtlAsync nullable tuple contract change.
- evidence/regression-testing/phase7-green.md — whole-assembly green run after removing DoNotParallelize.

## QA gates

- evidence/qa-gates/ac6-ac20-amended-spec-verification.md — read-only proof the executor worked against the amended spec.
- evidence/qa-gates/ac26-ac27-boundary.md — the ownership boundary with sibling feature 826.
- evidence/qa-gates/ac11-etlasync-tuple.md — anchored diff proving the removed suppression is the one inside EtlAsync.
- evidence/qa-gates/ac19-historical-records.md — the historical TableEtlInvoker record survives untouched.
- evidence/qa-gates/ac23-attributes-retained.md — the two classes that keep DoNotParallelize are untouched.
- evidence/qa-gates/qc-csharpier-format.md — the format pass, including the pass that restarted the phase.
- evidence/qa-gates/qc-csharpier-check.md — the read-only format verification, 1623 files.
- evidence/qa-gates/qc-build-analyzers.md — the analyzer gate.
- evidence/qa-gates/qc-build-analyzers.txt — its detailed MSBuild log, sanitised.
- evidence/qa-gates/qc-build-nullable.md — the nullable gate.
- evidence/qa-gates/qc-build-nullable.txt — its detailed MSBuild log, sanitised.
- evidence/qa-gates/ac32-non-vacuity.md — proof neither MSBuild gate was vacuous.
- evidence/qa-gates/qc-coverage-postchange.md — repository-wide coverage after the change.
- evidence/qa-gates/coverage-postchange.cobertura.xml — the processed Cobertura it reads.
- evidence/qa-gates/ac33-coverage-comparison.md — the four decided gates, the testable denominator, and the deletion reconciliation.
- evidence/qa-gates/ac33-changed-line-coverage.md — changed-line coverage per file and overall.
- evidence/qa-gates/ac20-docs-boundary.md — the documentation boundary.
- evidence/qa-gates/ac28-sibling-ownership.md — the sibling-owned file exclusion.
- evidence/qa-gates/ac18-commit-language.md — commit language, clean tree, Write Set accounting and the D7 confirming check.

## Other

- evidence/other/ac8-createcancellationtokensource-proof.md — the compile-time proof of the TimeProvider extension member.
- evidence/other/ac8-createcancellationtokensource-proof.txt — the MSBuild log that proof reads, sanitised.
- evidence/other/ac7-mechanism-note.md — the exception-injection mechanism note spec.md Test Strategy requires.
- evidence/other/file-size-accounting.md — the TimeOutTask.cs line accounting, recorded as a reduction.
- evidence/other/ac21-justification.md — why DoNotParallelize was removed, with RunsObserved at 0.
- evidence/other/ac34-value-trace.md — the four-step reviewer trace of one accepted timeout value.
- evidence/other/ac35-reachability-observation.md — the reachability observation and the deferred epic handoff. **Sibling feature 826 depends on this one.**
- evidence/other/plan-deviations.md — every deviation from the plan text, with its measurement.
- evidence/other/review-handoff.md — this index.

## Issue updates

- evidence/issue-updates/ac-status-summary.md — the acceptance-criteria status summary, 35 of 35 delivered.

## Note for sibling feature 826

evidence/other/ac35-reachability-observation.md states the end state of the escaping exception
explicitly. In summary: after this change, on the ordinary timeout path, nothing escapes
`TimeOutTask.RunWithTimeout` — it returns null — so neither `Console.WriteLine` diagnostic in
`GetTableInViewAsync` has become reachable through that path. This feature changed which clock arms
the deadline and which millisecond value the retry uses; it did not change the exception behaviour of
`RunWithTimeout`, whose signatures, `strict` semantics and retry behaviour are unchanged.
