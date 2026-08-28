# P4-T10 — Toolchain-loop attestation (remediation cycle 1)

Timestamp: 2026-08-28T04-18
Task: [P4-T10]
Command: (attestation over the recorded Phase 4 artifacts; no new command is executed by this task)
EXIT_CODE: 0

LoopIteration: 1
TotalLoopIterations: 1
RestartsTriggered: 0

## The completed pass

Phase 4 completed in **one** iteration, in the mandated order — format, lint, type-check, test.
No stage failed and no stage rewrote a file, so convention 11's restart rule was never triggered.

| # | Task | Stage | `EXIT_CODE:` | Artifact |
|---|---|---|---:|---|
| 1 | P4-T1 | Format (`csharpier format`) | 0 | `rem1-p4-t1-csharpier-format.2026-08-28T03-56.md` |
| 2 | P4-T2 | Format verification (`csharpier check`) | 0 | `rem1-p4-t2-csharpier-check.2026-08-28T03-57.md` |
| 3 | P4-T3 | Lint / analyzers (`msbuild /t:Rebuild` + analyzers) | 0 | `rem1-p4-t3-analyzer-build.2026-08-28T03-58.md` |
| 4 | P4-T4 | Lint non-vacuity proof | 0 | `rem1-p4-t4-analyzer-nonvacuity.2026-08-28T03-58.md` |
| 5 | P4-T5 | Type check / nullable (`msbuild /t:Rebuild` + `TreatWarningsAsErrors`) | 0 | `rem1-p4-t5-nullable-build.2026-08-28T03-59.md` |
| 6 | P4-T6 | Test, scoped (`vstest` over `QuickFiler.Test` with coverage) | 0 | `rem1-p4-t6-vstest-quickfiler.2026-08-28T04-01.md` |
| 7 | P4-T7 | Test, repository-wide with coverage | 0 | `rem1-p4-t7-repo-coverage.2026-08-28T04-16.md` |

All seven `EXIT_CODE:` values are `0`. No task in this pass recorded `SKIPPED`; `SKIPPED` is not a
valid outcome for any command-bearing task in this plan and none was used.

## No stage failed and no stage rewrote a file

**Explicit statement: in this pass, no stage failed and no stage rewrote any file.**

The rewrite claim is not an inference from the absence of an error message. P4-T1 measured it
directly, by SHA-256 manifest comparison over all 1850 tracked `*.cs`, `*.xml` and `packages.config`
files taken immediately before and immediately after the format command: 0 added, 0 removed, **0
changed**, with byte-identical aggregate manifest hashes on both sides
(`ac415e81b3d5ad61885fa1aac8063e2d79e3a2b3cea5145dce0c025b58024e44`). `git status --porcelain`
immediately after the format pass printed zero lines, agreeing independently.

The four later command stages — the two msbuild builds and the two test runs — do not write to tracked
source. The build stages write only to `obj/` and `bin/`, which are gitignored; the test stages write
only TRX files and, for the coverage runs, `coverage/coverage.cobertura.xml`, which is gitignored by
`.gitignore:144`. Between P4-T1 and P4-T9 the only tracked files that changed were this cycle's own
evidence artifacts and the plan's check-off marks.

## Restart history

**No restarts.** `RestartsTriggered: 0`. Because loop iteration 1 completed with every stage green and
no file rewritten, the pass was not restarted from P4-T1 at any point, and no restart is pending.

Two operational false starts occurred inside the pass and neither is a toolchain-stage failure or a
file rewrite, so neither triggered the restart rule. Both are recorded here rather than omitted:

1. **P4-T3, log filename.** A first msbuild invocation wrote its file log to a name whose timestamp had
   not come from a `date -u` reading. That log was deleted and the build re-run against a name taken
   from an actual reading. The build itself succeeded in both cases; the discarded one is not the
   recorded gate.
2. **P4-T7, argument quoting.** A first launch of the coverage runner had its `-CoverageOutput`
   backslash consumed by the shell, sending the document to a repo-root path. That run was killed, its
   process chain verified gone, and the one stray file it produced deleted before either recorded run.
   The two recorded runs both used correct quoting and both are reported in full in the P4-T7 artifact.

Neither event changed a tracked source file, and neither is a stage that "failed" in the sense
convention 11 governs: in both cases the underlying command succeeded and the defect was in how the
invocation was addressed, which was corrected before the gate figure was recorded.

## Gate results carried by this pass

| Gate | Baseline (P0-T5) | Final | Verdict |
|---|---|---|---|
| CSharpier unformatted files | 0 (empty set) | 0 (empty set) | equal |
| Analyzer warnings (deduplicated) | 5 | 5 | not greater |
| Analyzer errors | 0 | 0 | equal |
| `CoreCompile` skip occurrences | n/a | 0 | non-vacuous |
| Nullable build exit code | 0 | 0 | equal |
| `QuickFiler.Test` passed / failed / skipped | 1121 / 0 / 0 | 1122 / 0 / 0 | +1 test, exactly as expected |
| Repository-wide passed / failed / skipped | 6741 / 0 / 0 | 6742 / 0 / 0 | +1 test, exactly as expected |
| Coverage line rate (post-processed shape) | 0.851567 | 0.851617 | not lower |
| Coverage lines-valid | 63901 | 63902 | +1, the added production line |
| Added line 481 hits | n/a | 1 | covered |

## Acceptance

| P4-T10 condition | Result |
|---|---|
| The artifact shows one complete pass with every stage green | **Yes** — P4-T1 through P4-T7, all `EXIT_CODE: 0` |
| No restart pending | **Yes** — `RestartsTriggered: 0`, and the completed pass had no failure and no file rewrite |

Output Summary: Phase 4 completed in **one** toolchain-loop iteration with every stage green.
P4-T1 through P4-T7 all recorded `EXIT_CODE: 0`, in the mandated format, lint, type-check, test order,
and no task used `SKIPPED`. **No stage failed and no stage rewrote a file** — the rewrite claim is
measured, not inferred: P4-T1's SHA-256 manifest comparison over 1850 tracked files reported 0
rewritten with byte-identical aggregate hashes, corroborated by an empty porcelain. Convention 11's
restart rule was therefore never triggered and no restart is pending. Two operational false starts
inside the pass — a log filename whose timestamp had not come from a `date -u` reading, and a coverage
argument whose backslash the shell consumed — are recorded above; neither was a stage failure or a file
rewrite, both were corrected before any gate figure was recorded, and neither affected a recorded
result.
