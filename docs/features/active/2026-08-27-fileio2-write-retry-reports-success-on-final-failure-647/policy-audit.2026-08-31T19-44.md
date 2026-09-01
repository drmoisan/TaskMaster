# Policy Audit — Issue #647 (FileIO2 write retry reports success on final failure)

- Timestamp: 2026-08-31T19-44
- Feature folder: `docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647`
- Branch: `bug/fileio2-write-retry-reports-success-on-final-failure-647`
- Head reviewed: `8e773f350671c29f2ff34803df63ac60d70ed648`
- Base: `9b6aff2e886eb86af5dfc131ebee7a2ebe1a5b6c`
- Work mode: `full-bug` (marker read from `issue.md` line 12)
- Acceptance-criteria source: `spec.md` only

## Verdict

**PASS. Blocking findings: 0.** Non-blocking findings recorded in this artifact: 8 (N-1 through N-8).

## Scope Resolution

The audit scope is the full branch diff against the resolved base branch.

Base resolution was recomputed rather than accepted from the caller:

```
git merge-base HEAD origin/main -> 9b6aff2e886eb86af5dfc131ebee7a2ebe1a5b6c
git rev-parse origin/main       -> 9b6aff2e886eb86af5dfc131ebee7a2ebe1a5b6c
```

The merge-base equals the current `origin/main` tip and equals the base the caller supplied, so the supplied base is correct and current. `git diff --stat 9b6aff2e..HEAD` reports 58 changed paths: 5 source files and 53 paths under this feature folder. No path under `.claude/` appears in the branch diff; the two untracked agent-memory files and one modified agent-memory index shown by `git status` are uncommitted working-tree state, not part of the branch.

Changed languages in the branch diff: **C# only.** No `.py`, `.ps1`, `.psm1`, `.ts`, or `.tsx` file is changed, so the Python, PowerShell and TypeScript coverage gates have zero changed files on this branch.

## Rejected Scope Narrowing

None. No caller instruction attempted to narrow the diff scope to a plan, task, phase, or file subset, and no instruction asserted that any language with changed files should be excluded from the audit.

Two caller instructions were assessed and are recorded here for transparency because they touch evidence handling rather than diff scope:

1. "Do NOT create `artifacts/csharp/coverage.xml`; leave it absent." This does not narrow the audited diff. Its effect on the coverage gate is disclosed as N-2 below: the canonical path is absent, and coverage was verified instead from the same-session Cobertura document at `coverage/coverage.cobertura.xml`, which is hash-bound to the reviewed head (see Provenance). Coverage verification was performed, not skipped.
2. "Reconcile the recorded evidence instead" of re-running builds and tests. Every reconciled figure that could be re-derived from an on-disk artifact was re-derived independently; the figures that could not (baseline-run integers, MSBuild summaries) are reconciled from the recorded evidence and are labelled as such per row.

## Provenance of the Reviewed Tree

SHA-256 of the five footprint files at head matches, byte for byte, the ten post-format hashes recorded in `evidence/qa-gates/p6-t1-format.md`:

| Path | SHA-256 at head | Matches P6-T1 |
|---|---|---|
| `UtilitiesCS/To Depricate/FileIO2.cs` | `cc16bea4...54f0ba` | Yes |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | `4512823a...96a05a` | Yes |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | `71b6a200...d607b8` | Yes |
| `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` | `cf136e1c...e229e8` | Yes |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | `4b645c3e...b37461` | Yes |

Consequence: the analyzer build, the nullable build, the full-suite test run and the coverage document all observed exactly the tree under review. No re-run is required to bind the recorded gate results to this head.

## Policy Compliance Order Applied

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/quality-tiers.md`, and the C# sections of `CLAUDE.md` (C#1–C#7, CUT1–CUT3)

## Gate Results

| # | Gate | Authority | Verdict | Evidence |
|---|---|---|---|---|
| G1 | CSharpier format (`dotnet tool run csharpier format .`) | CLAUDE.md C#1.1 | PASS | `evidence/qa-gates/p6-t1-format.md`: exit 0, `REWRITTEN_FILE_COUNT: 0`, ten SHA-256 hashes reproduced above |
| G2 | CSharpier check (`dotnet tool run csharpier check .`) | CLAUDE.md C#1.1 | PASS | `evidence/qa-gates/p6-t2-format-check.md` exit 0; closure re-run in `p6-t8-loop-closure.md` exit 0, `Checked 1565 files` |
| G3 | Analyzer build (`/t:Rebuild` + `EnableNETAnalyzers` + `EnforceCodeStyleInBuild`) | CLAUDE.md C#1.2 | PASS | `evidence/qa-gates/p6-t3-analyzer-build.md`: exit 0, 0 errors, 5 warnings; baseline `p0-t13` 0 errors / 5 warnings; 36 `csc.exe` invocations confirm a real compile |
| G4 | Nullable / `TreatWarningsAsErrors` build (`/t:Rebuild`) | CLAUDE.md C#1.3 | PASS | `evidence/qa-gates/p6-t4-nullable-build.md`: exit 0, 0 errors, 5 warnings; baseline `p0-t14` identical; scan for `(warning\|error) <LETTERS><DIGITS>` returned zero matches |
| G5 | Test run (`vstest.console.exe` over all discovered `*.Test.dll`, `/EnableCodeCoverage /InIsolation`) | CLAUDE.md CUT3.4 | PASS | `evidence/qa-gates/p6-t5-full-suite-vstest.md`: 9 assemblies, 6899 total, 6899 passed, 0 failed, exit 0 |
| G6 | `/t:Rebuild` used rather than `/t:Build`, and `/p:Nullable=enable` not added | CLAUDE.md C#1.2, C#1.3 | PASS | Commands transcribed verbatim in `p6-t3`/`p6-t4`; both use `/t:Rebuild`; `p6-t4` records that `/p:Nullable=enable` was not added and that per-file opt-in via the line-1 pragma governs `FileIO2.cs` |
| G7 | C# framework, mocking and assertion libraries | CLAUDE.md CUT1, CUT2 | PASS | New tests use `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting` and FluentAssertions (`Should()`); no xUnit or NUnit introduced; no mock needed, so Moq is not used in the new tests |
| G8 | 500-line file limit | `.claude/rules/general-code-change.md` | PASS | Measured at head: FileIO2.cs 293, QfcHomeController.Metrics.cs 227, AppOlObjects.cs 494, FileIO2_Tests.cs 335, QfcHomeControllerMetricsTests.cs 454. All at or under 500. See N-9 in the code review for the 6-line margin on AppOlObjects.cs |
| G9 | Test-file location (`tests/` tree mirroring production) | `.claude/rules/general-unit-test.md` | PASS | Both changed test files live in the repository's established `<Project>.Test/` mirror trees; no test file was colocated into a production source tree |
| G10 | Banned test APIs and filesystem/temp-file prohibition | `.claude/rules/general-unit-test.md`, CLAUDE.md UT4 | PASS | `evidence/qa-gates/p5-t10-banned-api-audit.md`: 0 occurrences of `Thread.Sleep`, `Task.Delay`, `GetTempPath`, `CreateDirectory`, `File.Create`, `File.WriteAllText`, `new FileStream(` across both changed test files. Independently confirmed by reading both files at head |
| G11 | Determinism infrastructure (no wall-clock waits in tests) | `.claude/rules/general-unit-test.md` | PASS | Every timing branch is driven through the injected `Func<int, CancellationToken, Task>` seam returning `Task.CompletedTask`; the 99-delay exhaustion test runs in 2 ms |
| G12 | Coverage exclusion policy (no production path excluded) | `.claude/rules/general-unit-test.md` | PASS | No `.csproj`, `coverage.config`, `.editorconfig` or `TaskMaster.runsettings` change is in the diff; no `[ExcludeFromCodeCoverage]` attribute was added. `UtilitiesCS\To Depricate\FileIO2.cs` is present in the coverage document's denominator, verified by parsing the Cobertura class element |
| G13 | Bugfix workflow: failing regression test before the fix | CLAUDE.md Bugfix Workflow | PASS | Defect 2 has a genuine failing pre-fix run (`evidence/regression-testing/p3-t2-midwrite-fail-before.md`, exit 1, `Expected midWriteDelayCalls to be 0, but found 1`) and a matching pass-after run. Defect 1 carries the exception dossier `evidence/regression-testing/fail-before-exception.2026-08-31T19-40.md` plus a pre-fix characterization run; the signature change is itself the fix, so a failing pre-fix assertion on the return value is unconstructible. Anticipated by spec Risk 5 |
| G14 | Minimal targeted fix; no opportunistic widening | CLAUDE.md Bugfix Workflow | PASS | Footprint independently verified: `git diff --name-only 9b6aff2e..HEAD` yields the 5 named source files plus feature-folder paths and nothing else. Three deferred items were left untouched and recorded for promotion |
| G15 | Toolchain loop completes with all stages passing on one unchanged tree | CLAUDE.md "Run the full toolchain (no shortcuts)" | PASS | `evidence/qa-gates/p6-t8-loop-closure.md`: `FINAL_ITERATION: 1`, seven cited artifacts all at iteration 1, six command-bearing artifacts all exit 0. Literal-restart deviation disclosed as N-5 |
| G16 | Policy documents unmodified | This agent's hard constraints | PASS | No path under `.claude/rules/`, `.github/instructions/`, or `CLAUDE.md` appears in the branch diff |

## Coverage Verification

Coverage document parsed: `coverage/coverage.cobertura.xml` (Cobertura, root element `<coverage>`, produced by the P6-T6 run recorded at `evidence/qa-gates/p6-t6-full-suite-coverage.md`). Root attributes read directly:

```
line-rate="0.852919" branch-rate="0.792754"
lines-covered="54835" lines-valid="64291"
branches-covered="13063" branches-valid="16478"
```

These are byte-identical to the six figures the executor recorded, so the recorded repository-wide figures are independently confirmed rather than transcribed.

### Language coverage rows

| Language | Changed files on branch | Coverage measured | Verdict |
|---|---|---|---|
| C#/.NET coverage | 5 | line coverage 85.29% repository-wide (54835/64291); branch coverage 79.28% repository-wide (13063/16478); changed method `FileIO2.WriteTextFileAsync` line coverage 95.00% (38/40) | **PASS** |
| Python coverage | 0 | zero changed files on this branch, so no Python coverage figure exists to evaluate | zero changed files |
| PowerShell coverage | 0 | zero changed files on this branch, so no PowerShell coverage figure exists to evaluate | zero changed files |
| TypeScript coverage | 0 | zero changed files on this branch, so no TypeScript coverage figure exists to evaluate | zero changed files |

**C#/.NET coverage row, stated explicitly: PASS.** Repository-wide line coverage is 85.29% and repository-wide branch coverage is 79.28%; the changed method `FileIO2.WriteTextFileAsync` reaches 95.00% line coverage. Assessment against both governing authorities:

- **CLAUDE.md § UT2** sets a repository-wide floor of 80% on the testable denominator, with a maintainer-ratified COM/VSTO/WinForms exemption. The measured 85.29% clears the 80% floor by 5.29 points on the full first-party denominator, without invoking the exemption at all. UT2 also requires new modules, classes and methods to target 90%: the one new method this change adds, the `internal static` seam overload, measures 38 of 39 lines covered (97.44%), which clears 90%.
- **`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`** set a uniform 85% line and 75% branch floor across T1–T4 for branch-capable languages. C# is branch-capable. Measured 85.29% line clears 85% by 0.29 points; measured 79.28% branch clears 75% by 4.28 points.

Both authorities are satisfied by the measured figures.

### Changed-method verification, re-derived independently

Parsing every `<line>` element of the `UtilitiesCS.FileIO2` class element and selecting the two `WriteTextFileAsync` spans (69–74 public forwarder, 83–150 internal seam overload):

```
span lines 40   covered 38   rate 0.950000
zero-hit lines  74, 101
```

This reproduces the executor's figures exactly, including the identity of the two zero-hit lines. Line 74 is the public overload's forwarding expression; line 101 is the wrapped right operand of the production-default writer-factory coalescing expression. Both are among the three lines the plan enumerated as permitted.

### Changed-file coverage rows

| Changed production file | Line coverage at head | Verdict | Disposition |
|---|---|---|---|
| `UtilitiesCS/To Depricate/FileIO2.cs` | 88.32% (121/137), up from 84.13% (106/126) at baseline | PASS | Clears 85%; +15 covered lines, zero regression |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 77.05% (94/122) | FAIL | Dispositioned Non-blocking — see N-3 |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 29.58% (71/240) | FAIL | Dispositioned Non-blocking — see N-4 |

Changed test files are correctly outside the coverage denominator: neither `FileIO2_Tests.cs` nor `QfcHomeControllerMetricsTests.cs` appears as a `filename` in the Cobertura document.

### No-regression on changed lines

`FileIO2.cs` covered lines rose from 106 to 121 and no previously covered line went dark. In `QfcHomeController.Metrics.cs`, the single pre-change flush statement was reflowed into six lines (179–184) that are all covered at head, so the changed statement did not regress; the six lines added after it (186–191) are new and uncovered. In `AppOlObjects.cs` the surrounding region measured zero hits before and after, so the added lines did not displace covered ones.

### Repository-wide delta

Baseline 0.853296 to head 0.852919, a shortfall of 0.000377 (0.038 percentage points). Absolute covered lines rose from 54820 to 54835. Discovered assembly count is 9 at both ends, so the denominator did not move because of a discovery difference. The shortfall is arithmetically explained: the change adds 46 lines to the denominator and 15 to the numerator. Both governing floors remain cleared at head.

## Evidence Location Compliance

All evidence this feature produced is under `docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/evidence/<kind>/`, using the canonical kinds `baseline/`, `qa-gates/` and `regression-testing/`.

A scan of the branch diff for paths under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/` returned **zero matches**. The only `artifacts/` subtree present in this worktree is `artifacts/orchestration/`, which is not in the branch diff.

`validate_evidence_locations.py` is not present in this repository, so the scan was performed directly against `git diff --name-only 9b6aff2e..HEAD`. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose: no delegation instruction specified a non-canonical evidence path.

Verdict: **PASS**, 0 violations.

## Non-blocking Findings

**N-1 — PR context artifacts absent.** `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` do not exist in this worktree; the only `artifacts/` subtree is `artifacts/orchestration/`. The caller's hard constraints forbid writing under `artifacts/`, so they were not regenerated. Scope and evidence were derived instead from `git diff` against the independently recomputed merge-base, which is a strictly stronger source than the summary. Operational consequence for downstream tooling: any termination hook that reads the changed-language set from `pr_context.summary.txt` will see an empty set and will therefore skip its language checks rather than enforce them. Non-blocking.

**N-2 — Canonical C# coverage artifact absent at `artifacts/csharp/coverage.xml` by explicit caller instruction.** The default rule for an absent coverage artifact is a FAIL with a remediation trigger. That rule is not applied here because an equivalent same-session Cobertura document exists at `coverage/coverage.cobertura.xml`, was produced by the recorded P6-T6 run, and is bound to the reviewed head by the five matching SHA-256 hashes. It was parsed directly and every repository-wide, per-file and per-method figure was re-derived from it. Coverage verification was therefore performed on real measured data; only the artifact's path deviates. Non-blocking, disclosed rather than waived.

**N-3 — `QuickFiler/Controllers/QfcHomeController.Metrics.cs` measures 77.05% line coverage (94/122), below both the 85% uniform per-file expectation and CLAUDE.md § UT2's 80% floor, and it regressed.** Reconstructing the baseline from the diff (one covered flush line replaced by six covered lines, plus six new uncovered lines) gives approximately 89/111 = 80.18% before the change. The uncovered residue is the new failure-log block at lines 186–191, which this change introduced. Recorded as a FAIL row and dispositioned **Non-blocking** on four grounds: (a) no acceptance criterion places a per-file coverage obligation on this file — AC20 scopes the change-scoped coverage obligation to `FileIO2.cs` and the repository-wide figure; (b) no previously covered line regressed, and the changed flush statement itself is fully covered; (c) both governing repository-wide floors are cleared at head; (d) the uncovered lines are a `logger.Error` call on a static log4net field, so a test that executed them could assert nothing, and mandating one would produce coverage without assertion power rather than a real check. The proportionate remedy is a follow-up issue introducing an injectable logging seam on `QfcHomeController`, not a coverage-only test. See code-review finding C-3. No remediation-inputs artifact is produced.

**N-4 — `TaskMaster/AppGlobals/AppOlObjects.cs` measures 29.58% line coverage (71/240), and the 33 lines this change adds are uncovered.** The entire enclosing region measured zero hits before the change as well. `AppOlObjects` constructs Outlook global objects directly from `Microsoft.Office.Interop.Outlook.Application` with no injectable seam, which places it inside the maintainer-ratified exemption in CLAUDE.md § UT2 categories (a) and (c). Recorded as a FAIL row and dispositioned **Non-blocking** under that ratified exemption: the file is host-bound, the added lines cannot be reached without a live Outlook process, and no previously covered line regressed. No remediation-inputs artifact is produced.

**N-5 — The toolchain-loop restart at P6-T5 was taken as an in-place re-invocation rather than a restart from step 1.** The first P6-T5 invocation exited 1 with 14 failed tests, all one-minute timeouts in `QuickFiler.Test` pump-host and dispatcher fixtures. CLAUDE.md's loop rule says to restart from step 1 when any step fails. The executor instead re-invoked the byte-identical command, which passed 6899 of 6899. The substantive requirement is met: the five footprint hashes prove no tracked file changed between the failing and passing runs, so steps 1 through 4 would have re-executed as no-ops on an identical tree, and the recorded exit-0 results for those steps already apply to this head. Recorded as a literal-form deviation with the substance satisfied. Non-blocking.

**N-6 — Recorded evidence timestamps run ahead of the actual clock.** Examples: `p6-t6-full-suite-coverage.md` records `2026-08-31T20-50` while `coverage/coverage.cobertura.xml` has an mtime of 19:23; `p8-t4-commit.md` records `2026-08-31T21-10` while the commit `8e773f35` is dated `2026-08-31 19:32:56 -0400`. The drift grows monotonically from roughly +1h15m to +1h40m and is not a clean timezone offset. Relative ordering across artifacts is preserved, so every sequencing argument in the evidence still holds; only the absolute values are unreconcilable against the tree. Non-blocking, but the timestamps should not be cited as wall-clock facts.

**N-7 — Three deferred follow-ups are recorded as promotion requests only, and the promotions are still owed.** `evidence/qa-gates/p8-t3-promotion-requests.md` records `BLOCKER: no promotion MCP tool and no gh CLI are available to this executor`, and names the orchestrator as the party that performs the promotions. The three entries are `narrow-fileio2-retryable-exception-set` (bug, full-bug), `supported-async-text-writer-for-to-depricate-migration` (feature, full-feature), and `remove-unnecessary-interlocked-increment-in-fileio2` (feature, minor-audit). Until they are promoted they exist only in a feature folder that disappears at merge. Non-blocking; action owed by the orchestrator before this feature folder is archived.

**N-8 — Only the four-stage C# toolchain was run, against the seven-stage loop described in `.claude/rules/general-code-change.md`.** Stages 4 (architecture-boundary tests), 6 (contract/schema compatibility) and 7 (integration tests) were not run. CLAUDE.md is authority #1 and defines the C# loop as exactly four stages (format, analyze, type-check, test), and this repository provides no C# architecture-boundary, contract-schema, or integration harness that those stages could invoke. Recorded so the divergence between the two policy documents remains visible. Non-blocking.

## Summary

- Blocking findings: **0**
- Non-blocking findings in this artifact: **8** (N-1 through N-8)
- Toolchain: all four C# stages pass on a tree that is hash-identical to the reviewed head
- Coverage: C#/.NET row PASS at 85.29% line, 79.28% branch, 95.00% on the changed method
- Evidence locations: canonical, 0 violations
- Recommendation: **GO**
