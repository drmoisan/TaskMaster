# Phase 5 — Evidence Content Sanitisation (P5-T9)

Timestamp: 2026-09-03T03-35
Task: [P5-T9]
Command: for every in-scope file under the feature evidence tree, read the content, count case-insensitive occurrences of the local account token and the machine-name token, replace each with its redaction literal, and write the file back only when at least one substitution was made.
EXIT_CODE: 0

## Token derivation

Both tokens are derived at run time and neither value is written into this artifact, into the plan,
or into the P5-T10 artifact. Only the derivation expressions and the replacement literals are
recorded:

| Role | Derivation expression | Replacement literal |
|---|---|---|
| local account token | `Split-Path -Leaf $env:USERPROFILE` | `REDACTED-ACCOUNT` |
| machine-name token | `$env:COMPUTERNAME` | `REDACTED-MACHINE` |

Matching is case-insensitive in both directions, which matters because a TRX renders the account
token with different casing in different attributes.

## Scope

Every file under `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/`,
enumerated recursively, whose extension is `.trx`, `.cobertura.xml` or `.md`. Files scanned: **55**.

Both TRX documents and Cobertura documents are in scope because each carries the tokens inside its
CONTENT rather than only in its name. A TRX carries the account token in the `runUser=` attribute of
its `TestRun` element and the machine-name token in the `computerName=` attribute of every
`UnitTestResult` element; a Cobertura document can carry both inside the absolute source paths it
records. A name-only check would see none of that.

## Capture-time sweeps

This sweep was run not only here but immediately before every commit in this cycle, because a commit
made with the tokens still present writes them into git history permanently, where a final
working-tree sweep cannot reach them. The per-commit sweeps and this terminal sweep together mean no
commit on this branch has ever carried either token.

| Sweep point | Files scanned | Files rewritten | Account substitutions | Machine substitutions |
|---|---|---|---|---|
| Before the Phase 1 commit | 22 | 4 | 466 | 232 |
| Before the Phase 2 commit | 31 | 1 | 22 | 12 |
| Before the Phase 3 commit | 41 | 3 | 87 | 45 |
| **This terminal sweep (P5-T9)** | **55** | **1** | **276** | **137** |
| Totals across all sweeps | — | 9 | 851 | 426 |

## Per-file substitution counts for this sweep

The count rows below, not the exit code, are the required observation. This task rewrites tracked
files and exits 0 whether it substituted anything or nothing, so an exit code alone records no
outcome.

| File (relative to the feature folder) | Account subs | Machine subs |
|---|---|---|
| evidence/baseline/base-ref.2026-09-02T12-04.md | 0 | 0 |
| evidence/baseline/coverage-baseline.2026-09-02T12-04.cobertura.xml | 0 | 0 |
| evidence/baseline/coverage-baseline.2026-09-02T12-04.md | 0 | 0 |
| evidence/baseline/csharpier-check.2026-09-02T12-04.md | 0 | 0 |
| evidence/baseline/file-line-counts.2026-09-02T12-04.md | 0 | 0 |
| evidence/baseline/msbuild-analyzer.2026-09-02T12-04.md | 0 | 0 |
| evidence/baseline/msbuild-nullable.2026-09-02T12-04.md | 0 | 0 |
| evidence/baseline/nuget-restore.2026-09-02T12-04.md | 0 | 0 |
| evidence/baseline/phase0-instructions-read.2026-09-02T12-04.md | 0 | 0 |
| evidence/baseline/ribbon-tests.2026-09-02T12-04.md | 0 | 0 |
| evidence/baseline/scope-and-write-set.2026-09-02T12-04.md | 0 | 0 |
| evidence/baseline/toolchain-bootstrap.2026-09-02T12-04.md | 0 | 0 |
| evidence/baseline/p0-t8/p0-t8.trx | 0 | 0 |
| evidence/issue-updates/ac-status.2026-09-02T12-04.md | 0 | 0 |
| evidence/other/manual-verification-clear-spam-manager.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/build-after-callsite.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/build-after-finding1.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/build-after-gate-class.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/build-after-gate-tests.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/build-after-race-fix.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/build-before-race-fix.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/callsite-edit-scope.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/coordinator-size-contingency.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/coverage-delta.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/coverage-final.2026-09-02T12-04.cobertura.xml | 0 | 0 |
| evidence/qa-gates/csharpier-check-final.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/csharpier-format.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/csharpier-xml-format.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/file-line-counts.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/footprint-scope.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/gate-class-constraints.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/msbuild-analyzer.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/msbuild-nullable.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/no-new-exemption.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/partial-keyword-edit.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/race-fix-structure.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/toolchain-loop-closure.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/vstest-coverage-run.2026-09-02T12-04.md | 0 | 0 |
| evidence/qa-gates/xml-edit-scope.2026-09-02T12-04.md | 0 | 0 |
| **evidence/qa-gates/p4-t3/p4-t3.trx** | **276** | **137** |
| evidence/regression-testing/coordinator-fixture-after-fix.2026-09-02T12-04.md | 0 | 0 |
| evidence/regression-testing/fail-before-exception.2026-09-02T12-04.md | 0 | 0 |
| evidence/regression-testing/fail-before-finding1.2026-09-02T12-04.md | 0 | 0 |
| evidence/regression-testing/fail-before-finding3.2026-09-02T12-04.md | 0 | 0 |
| evidence/regression-testing/gate-tests.2026-09-02T12-04.md | 0 | 0 |
| evidence/regression-testing/pass-after-finding1.2026-09-02T12-04.md | 0 | 0 |
| evidence/regression-testing/pass-after-finding3.2026-09-02T12-04.md | 0 | 0 |
| evidence/regression-testing/ribbon-fixtures-after-finding1.2026-09-02T12-04.md | 0 | 0 |
| evidence/regression-testing/p1-t2/p1-t2.trx | 0 | 0 |
| evidence/regression-testing/p1-t7/p1-t7.trx | 0 | 0 |
| evidence/regression-testing/p1-t8/p1-t8.trx | 0 | 0 |
| evidence/regression-testing/p2-t8/p2-t8.trx | 0 | 0 |
| evidence/regression-testing/p3-t11/p3-t11.trx | 0 | 0 |
| evidence/regression-testing/p3-t12/p3-t12.trx | 0 | 0 |
| evidence/regression-testing/p3-t5/p3-t5.trx | 0 | 0 |

The six TRX documents that report zero here were rewritten by the earlier capture-time sweeps before
their commits; only `p4-t3.trx` was produced after the last of those sweeps, which is why it is the
single file rewritten by this terminal sweep. Every markdown artifact reports zero in every sweep,
because they were authored with placeholders rather than literal paths.

## Post-sweep verification

- Files and directories anywhere under the evidence tree whose NAME contains either token,
  compared case-insensitively: **0**.
- Case-insensitive occurrences of either token in the CONTENT of every in-scope file: **0**.

Output Summary: 55 files scanned, **1 file rewritten**, **276 account-token substitutions** and
**137 machine-name-token substitutions** in this terminal sweep. All substitutions were in
`evidence/qa-gates/p4-t3/p4-t3.trx`; every other file was already clean from the three capture-time
sweeps that preceded each commit. After the sweep, zero name occurrences and zero content
occurrences of either token remain anywhere under the evidence tree.
