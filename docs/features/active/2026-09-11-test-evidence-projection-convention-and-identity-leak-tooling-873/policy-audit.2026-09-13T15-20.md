# Policy Audit — Issue #873, test-evidence projection convention and identity-leak tooling

- Component: `scripts/vscode` editor test entry points, `CLAUDE.md`, `TaskMaster/TaskMaster.csproj`, `.vscode/settings.json`, `.claude/agent-memory`
- Review timestamp: 2026-09-13T15-20
- Branch: `bug/test-evidence-projection-convention-and-identity-leak-tooling-873`
- Head commit: `5b1d5d93d0cf8b7b6a36aae68b24f7fd6086cd25`
- Resolved base: `origin/main` at `a5622ab9123a88bfa3ec5b8fccfdc613e74c4df5`
- Merge base: `a5622ab9123a88bfa3ec5b8fccfdc613e74c4df5`
- Work mode: `full-bug` (acceptance-criteria source is `spec.md` only)
- Review cycle: 1
- All paths in this document are repository-relative. No absolute host path, account token or host token appears anywhere in it.

## Executive Summary

Verdict: **PASS**. 0 Blocking findings, 11 Non-blocking findings.

The delivery adds two pure PowerShell part files (a package-level JaCoCo projection writer with a reconciliation assertion and a retention predicate; a test-result summary reader and formatter), wires both editor test entry points to an explicit results directory and log file name, records the committed-evidence convention in `CLAUDE.md`, and clears the named identifier leaks in one project file, one editor settings file and six agent-memory documents.

Every gate the repository imposes on this change set was run and recorded, and every recorded figure clears its floor:

- PowerShell analyzer: 16 diagnostics, identical tuple set and identical multiplicity to the Phase 0 baseline, 0 entries absent from it, and 0 diagnostics of any severity against the six files this delivery creates.
- PowerShell tests: 133 passed, 0 failed, 0 skipped over `tests/scripts/vscode`, against a Phase 0 baseline of 103 passed.
- C# format check: exit 0 over 1626 files.
- C# analyzer rebuild and C# nullable rebuild: exit 0, 0 warnings, 0 errors each, equal to the Phase 0 baseline for the same command, and re-validated after the `origin/main` merge changed C# compilation inputs.
- Largest PowerShell file in the change set: 498 lines against the 500-line ceiling. Helpers-file growth: exactly 1 line.
- 23 of 23 acceptance criteria are satisfied.

Two structural strengths are worth naming because they are the load-bearing parts of the design. First, the projection writer delegates the de-duplicating counting rule to `Get-CoberturaPackageLineSummary` and performs exactly one new arithmetic step, and an abstract-syntax-tree test proves the delegation rather than asserting it. Second, the reconciliation assertion is exact and enforces two independent equalities, and both are covered by their own negative test; the projection is committed in place of the document it summarises, so an unchecked arithmetic divergence would be undetectable after the raw document is discarded.

The Non-blocking findings concentrate on one asymmetry and one coupling in the coverage entry point: that path does not discard the raw test-result document although the plain path does, and its conditional discard of the raw coverage document is nested inside the success of the test-result summary write. Neither fails an acceptance criterion and neither produces a committable artifact, because both documents sit beneath the already-ignored repository coverage tree.

## Rejected Scope Narrowing

No caller instruction attempted to narrow this audit to a plan, task or phase, to a subset of changed files, or to mark any language with changed files as excluded. The scope evaluated is the full branch diff against the resolved base branch: 107 changed paths in the range `a5622ab9123a88bfa3ec5b8fccfdc613e74c4df5..5b1d5d93d0cf8b7b6a36aae68b24f7fd6086cd25`, of which 22 sit outside this feature folder.

One caller instruction bears on a gate threshold rather than on scope, and is recorded verbatim here with its disposition so the reader can check the reasoning:

> `CLAUDE.md` is first in the repository's policy-compliance order and governs on conflict. Its thresholds are: C# line 80, branch 75, PowerShell line 80, new code 90. The 85/75 figures that appear in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` are push-down-owned from an upstream repository and are not authoritative here. Judge against the CLAUDE.md figures. Where a measured figure clears both sets, say so.

Disposition: the instruction changed no verdict in this audit, because every measured figure clears both sets of thresholds. Both sets were applied and both results are recorded in section 5. The conflict between the two policy documents is real and unreconciled in the repository, and it is recorded as an open documentation gap in section 8 rather than resolved here.

## 1. General Unit Test Policy Compliance

Verdict: **PASS**.

### 1.1 Core Principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Every fixture is a here-string assigned to a script-scoped variable inside `BeforeAll` or the `It` block. No test file writes shared state to disk. The full-folder run of 16 containers passed with 0 failures. |
| Isolation | PASS | One behaviour per `It`. The projection, the reconciliation assertion, the retention predicate, each argument builder and the entry-point wiring are exercised in separate `Describe` blocks. |
| Fast execution | PASS | The whole `tests/scripts/vscode` folder run completes within a single Pester invocation; no test starts an external process or launches an executable. |
| Determinism | PASS | No `Start-Sleep`, no wall-clock read, no randomness, no retry. Every external boundary is reached through a mocked named seam. |
| Readability | PASS | Every `It` name states the behaviour; every test carries a comment naming the criterion it discharges and why the assertion is shaped as it is. |

### 1.2 Coverage Requirements

Verdict: **PASS**.

#### Coverage Evidence Checklist

- C# baseline coverage artifact: `coverage/coverage.cobertura.xml`, the post-processed document written by the Phase 6 run, with the first-party headline transcribed into `docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p6-t2-default-output-run.md`
- C# post-change coverage artifact: `coverage/coverage.cobertura.xml`, the same document; this delivery changes zero compiled C# files, so the baseline and post-change measurements are the same measurement
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/baseline/p0-t12-powershell-coverage-baseline.jacoco.xml`
- PowerShell post-change coverage artifact: `docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t7-new-code-coverage.jacoco.xml`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 1 | 7222 | 7222 passed, 0 failed | 85.71% line / 79.87% branch | 85.71% line / 79.87% branch | 85.71% line |
| PowerShell | 12 | 133 | 133 passed, 0 failed | 76.96% line | 78.57% line | 92.50% line |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | 0 | N/A | N/A | N/A | N/A |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.71% line / 79.87% branch (56066/65416 lines, 13495/16896 branches). Post-change: 85.71% line / 79.87% branch (56066/65416 lines, 13495/16896 branches). Change: no movement; zero compiled C# files changed on this branch. New/changed-code coverage: 85.71%. Disposition: PASS. Evidence: docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p6-t2-default-output-run.md
- PowerShell: Baseline: 76.96% line (551/716 line elements). Post-change: 78.57% line (627/798 line elements). Change: +1.61% line. New/changed-code coverage: 92.50%. Disposition: PASS. Evidence: docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t7-new-code-coverage.md
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Figure Provenance

The C# figures are the first-party headline the Phase 6 default-output run printed, transcribed verbatim into `evidence/regression-testing/p6-t2-default-output-run.md` at line 136. The same run's projection reconciled exactly against the raw document's root attributes at 56066 covered and 65416 valid, which is an independent check on the numerator and the denominator. The second Phase 6 run printed 56071/65416 lines and 13500/16896 branches, a five-line movement in the covered count against an identical denominator, recorded in that artifact as known run-to-run collector variance with no file changed between the two runs. The lower of the two figures is the one carried above.

The C# baseline and post-change figures are identical because they are the same measurement. This delivery changes exactly one C# artifact, the publish-destination property in `TaskMaster/TaskMaster.csproj`, which is a publish-time ClickOnce property that contributes no compiled line. The New Code Coverage field carries the first-party repo-wide figure rather than an independent percentage, because a zero-line changed denominator admits none.

The PowerShell baseline figure of 76.96% is the folder-wide measurement over `scripts/vscode` recorded in `evidence/baseline/p0-t12-powershell-test-baseline.md`, derived by counting JaCoCo `line` elements whose covered-instruction attribute exceeds zero against the total count of `line` elements. The post-change figure of 78.57% is derived arithmetic, not a measured run: the two new part files contribute 82 line elements of which 76 are covered, per `evidence/qa-gates/p7-t7-new-code-coverage.md`, and 551 plus 76 over 716 plus 82 is 627/798, which is 78.57%. The derivation is stated so a reader is not left to infer it, and its direction is the operative point: the two new files raise the folder-wide figure rather than lowering it. Section 5 records what was not measured and why that is judged non-blocking.

### 1.3 Coverage Exclusion Policy

Verdict: **PASS**. No production file is excluded from coverage measurement by this delivery. No `exclude` entry matching a production source path is added anywhere. The only exclusion-shaped construct the delivery touches is `ConvertTo-DerivedCoverageSettingsXml`, which is pre-existing and adds the test-assembly instrumentation exclusion `.*\.Test\.dll$` — a test path, which the policy permits.

### 1.4 Scenario Completeness

Verdict: **PASS**. Every new pure function carries positive, negative, boundary and error-path tests.

| Unit | Positive | Negative | Boundary | Error handling |
|---|---|---|---|---|
| `ConvertTo-JacocoPackageProjection` | exact multi-package shape; three-package document order | missing packages node throws the reused wording | empty `classes` element; document with no branch data | reused throw wording proven byte-identical to the first-party helper's |
| `Assert-JacocoProjectionReconciliation` | summed counters equal the root attributes | covered-total mismatch throws naming both figures | valid-total mismatch throws naming both figures | thrown message asserted, not just the throw |
| `Test-RawCoverageDocumentRetained` | repository coverage directory retains | unrelated directory discards | subdirectory of the coverage tree discards; output path with no parent returns false | guard clause exercised |
| `Get-TrxRunSummary` | namespaced document read correctly | failing fixture yields exactly the two failed names | zero result elements yields an empty collection, not null | missing result-summary node throws a specific message |
| `Format-TrxRunSummary` | derivation statement present in the rendered text | — | not-executed and inconclusive figures carried verbatim | — |
| `Get-VsTestArgumentList` | both switches present, order pinned by index | — | — | — |
| `Get-DotnetCoverageArgumentList` | both switches present | — | both indices greater than the separator index | — |

A specific strength: the namespace handling is proven rather than assumed. `Get-TrxRunSummary` resolves every node through a local-name predicate, and a companion test asserts that an unprefixed XPath over the identical fixture selects zero nodes. Without that second test the first would pass for a reader that happened to work on a namespace-free fixture, and a namespace regression would surface as a run reporting zero of everything rather than as a failure.

### 1.5 Test Structure and External Dependencies

Verdict: **PASS**.

- Arrange–Act–Assert is followed in every test; fixtures are declared in `BeforeAll` or at the head of the `It`, the call under test is a single statement, and the assertions follow.
- No unit test depends on a network, a database, a remote API or an external process. Every executable boundary is reached through a mocked named wrapper function.
- Temporary files: independently verified, not accepted on the executor's assertion. A scan of all seven Write Set test files for `Set-Content`, `Out-File`, `Add-Content`, `New-Item`, `Remove-Item`, `Copy-Item`, `Move-Item`, `New-TemporaryFile`, `[IO.File]`, `[System.IO.File]`, `GetTempPath`, `TEMP` and `Get-Content` returned 31 hits, every one of which is a `Mock` declaration or a `Should -Invoke` assertion against a mock. Zero real filesystem calls, zero temporary files, zero fixtures loaded from a path. This reproduces the result claimed in `evidence/qa-gates/p7-t13-no-temporary-files-review.md`.

### 1.6 Test File Location

Verdict: **PASS**. Every test file lives under `tests/scripts/vscode/`, mirroring the production structure under `scripts/vscode/`, and every file is named `*.Tests.ps1`. No test file was created or moved into the production source tree.

## 2. General Code Change Policy Compliance

Verdict: **PASS**.

### 2.1 Design Principles

| Principle | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | The projection writer is a single loop that serialises four integers per package. The retention decision is a pure path-arithmetic predicate rather than a filesystem probe. |
| Reusability | PASS | The counting rule is not re-derived; `Get-CoberturaPackageLineSummary` is called once per package. The summary reader is a single part file dot-sourced by both entry points rather than duplicated in each. |
| Extensibility | PASS | Both new part files expose advanced functions with named, attributed parameters. The two builder parameters were added as named mandatory parameters and every in-repository call site was updated in the same delivery. |
| Separation of concerns | PASS | All five new functions are pure. The conditional discard is deliberately split into a predicate in the part file and the `Remove-Item` at the entry point, so the decision is unit-testable with no file existing. |

### 2.2 File Size Limit

Verdict: **PASS**. Twelve PowerShell files measured after the final format step; the largest is 498 lines.

| File | Lines | Against 500 |
|---|---|---|
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | 498 | within |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1` | 495 | within |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 471 | within |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 438 | within |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1` | 268 | within |
| `scripts/vscode/Invoke-MSTest.ps1` | 262 | within |
| `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` | 197 | within |
| `tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1` | 193 | within |
| `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` | 150 | within |
| `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` | 146 | within |
| `tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1` | 119 | within |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` | 106 | within |

The ceiling applies to test code as well as production code, and both files nearest the limit are test files. `Invoke-MSTest.RunSettings.Tests.ps1` stood at 496 lines before the change and would have breached the ceiling had the two new arguments been added at each affected call site in the existing line-continuation style; the conversion of those call sites to splatting from hashtables declared in the containing setup block is what absorbed them, at a net cost of 2 lines for 101 added and 99 removed. The helpers file grew by exactly 1 line, the dot-source of the new projection part file, against a post-format baseline of 470.

### 2.3 Mandatory Toolchain Loop

Verdict: **PASS**. The applicable stages for a PowerShell change are format, lint and unit tests; type checking is explicitly skipped for PowerShell by the rule. Architecture-boundary, contract-schema and integration stages have no applicable tooling for this change set. The project-file edit additionally brings the C# format, analyzer and nullable gates into play, and all three were run.

The loop restarted once, correctly. The new-code coverage gate failed on its first measurement at 82.50% for the projection part file against a 90 floor. The remediation added two tests to a file already inside the Write Set, and the loop then restarted from the format step: format, analyze, test, C# format check, C# analyzer rebuild and C# nullable rebuild were all re-run, and each artifact carries a recorded second pass. This is the behaviour the General Code Change Policy requires when a step changes files, and the first failing measurement was recorded rather than discarded.

### 2.4 Error Handling and Logging

Verdict: **PASS**. Both new part files fail fast with specific messages. The missing-packages condition reuses the wording the first-party helper already throws rather than introducing a second wording for the same condition, and the reconciliation failure names both the expected and the observed totals so a failure is diagnosable from the message alone. One deliberate non-fatal path exists: a missing, unreadable or result-summary-less test-result document is reported with `Write-Warning` rather than thrown, because a genuine test failure is already surfaced by the exit-code check upstream. That choice is documented in a comment at both call sites and is covered by a test that asserts the warning is emitted and that neither the summary write nor the discard occurs.

### 2.5 Naming, Docs and Comments

Verdict: **PASS**. All five new functions use approved verbs and singular nouns, and the analyzer reports no naming diagnostic against any file this delivery creates. Every new function carries a comment-based help block stating its synopsis, its description, its parameters and its outputs. Comments explain why rather than what: the `GetAttribute` calls carry a comment recording that `Set-StrictMode -Version Latest` makes a missing XML attribute throw on bare property access, and the equality-rather-than-containment choice in the retention predicate is stated with its reason.

### 2.6 I/O Boundaries

Verdict: **PASS**. All five new functions are pure. The projection writer operates on an already-parsed document and returns a string; the caller persists it. The summary reader takes the document as a string and returns an object; the caller persists the rendered text. `Split-Path` and `Join-Path` in the retention predicate are path arithmetic and require neither path to exist.

## 3. Language-Specific Code Change Policy Compliance

Verdict: **PASS**.

### 3.1 PowerShell (`.claude/rules/powershell.md`)

| Requirement | Verdict | Evidence |
|---|---|---|
| Format via PoshQC `Invoke-Formatter` | PASS | `evidence/qa-gates/p7-t1-final-format.md`, run twice |
| Lint via PSScriptAnalyzer with repo settings | PASS | `evidence/qa-gates/p7-t2-final-analyze.md`: 16 diagnostics, 0 absent from the Phase 0 baseline tuple set |
| Type checking skipped for PowerShell | PASS | Rule-mandated skip |
| Test via Pester 5.x | PASS | Pester 5.6.1; `evidence/qa-gates/p7-t3-final-test.md` |
| Toolchain order format then analyze then test | PASS | Task order P7-T1, P7-T2, P7-T3, re-run in the same order on pass 2 |
| PowerShell 7+ compatibility | PASS | Analyzer settings enforce it; 0 compatibility diagnostics |
| Advanced functions with `CmdletBinding` | PASS | All five new functions carry `[CmdletBinding()]`, `[OutputType(...)]` and `[Parameter(Mandatory = $true)]` |
| Avoid hard-coded paths | PASS | The results directory and log file name are defaulted parameters resolved against the repository root, overridable by a caller |
| Keep scripts under 500 lines | PASS | Section 2.2 |

Per-batch change budget: **PASS**. The rule caps a batch at 3 production files and 3 test files. The plan's batch declaration and the change set agree on the per-phase counts: Phase 1 touched 2 production files and 1 test file; Phase 2 touched 1 and 1; Phase 3 touched 1 production file and 3 test files; Phase 4 touched 1 production file and 3 test files; Phase 5 touched no PowerShell file; the Phase 7 remediation touched 1 test file. No batch exceeded either cap. The shared argument-builder test file was repaired in the same batch as each builder signature change that broke it, so no batch gate ran a test path that the same batch had just broken.

The batch-open artifacts at `evidence/qa-gates/p2-t1-batch-open.md`, `p3-t1-batch-open.md`, `p4-t1-batch-open.md` and `p7-t1-batch-open.md` record the reset of the batch-budget hook state rather than an enumeration of the batch's own file counts. The reset is a mechanical precondition for the hook, not evidence of compliance; compliance is established here from the per-phase file counts in the change set, which are checkable independently of what the artifacts assert.

Design seams: **PASS**. The wrapper-function seam is the pattern used throughout. `Invoke-VsTestExe`, `Invoke-VsWhereExe` and `Invoke-DotnetCoverageExe` each accept a single array parameter named `VsTestArgs`, `VsWhereArgs` and `DotnetCoverageArgs` respectively, none named `Args`, and each splats into the resolved executable.

Mocking rules: **PASS**. No test mocks `vstest.console.exe`, `dotnet-coverage` or `vswhere.exe` directly. Every executable is reached through its wrapper and the wrapper is what the tests mock. Mock signature parity holds where a mock declares a parameter block: the `Invoke-VsTestExe` mock declares `param([string]$VsTestPath, [string[]]$VsTestArgs)`, matching production exactly, and the `Invoke-DotnetCoverageCollection` mock declares all seven production parameters including the two the delivery adds.

### 3.2 C# (`CLAUDE.md` C# Code Change Policy)

Verdict: **PASS**. One C# artifact changed: `TaskMaster/TaskMaster.csproj`, one line added and one removed, replacing an absolute user-profile publish destination with the repository-relative value `publish\` that `UtilitiesCS.Test/UtilitiesCS.Test.csproj` already uses for the same property. No `.cs` file changed.

| Requirement | Verdict | Evidence |
|---|---|---|
| `dotnet tool run csharpier check .` | PASS | exit 0, 1626 files checked; `evidence/qa-gates/p7-t4-final-csharpier-check.md` |
| Analyzer rebuild with `/t:Rebuild` | PASS | exit 0, 0 warnings, 0 errors; `evidence/qa-gates/p7-t5-final-msbuild-analyzer.md` |
| Nullable rebuild with `/t:Rebuild` | PASS | exit 0, 0 warnings, 0 errors; `evidence/qa-gates/p7-t6-final-msbuild-nullable.md` |
| No `/p:Nullable=enable` added | PASS | Neither msbuild command carries it, matching the workflow the policy names |
| `dotnet format` not used | PASS | Absent from every recorded command |
| Element text is XML markup, so no angle-bracket placeholder | PASS | The replacement is the literal `publish\`; the file re-parses as XML |

The two rebuild gates were re-run after the `origin/main` merge changed C# compilation inputs, and returned results identical to the Phase 0 baselines. That re-validation is recorded in `evidence/qa-gates/p6-post-merge-csharp-revalidation.md` and closes the gap the Phase 7 disclosure identified.

## 4. Language-Specific Unit Test Policy Compliance

Verdict: **PASS**.

### 4.1 PowerShell Testing Standards

| Requirement | Verdict | Evidence |
|---|---|---|
| Pester 5.x | PASS | Pester 5.6.1 recorded in both the baseline and final test artifacts |
| Tests mirror code structure | PASS | `tests/scripts/vscode/` mirrors `scripts/vscode/` |
| `*.Tests.ps1` naming | PASS | All seven Write Set test files |
| `Describe`/`Context`/`It`, one behaviour per `It` | PASS | Read across all seven files |
| No external dependencies | PASS | Section 1.5 |
| Deterministic under Terminal and Test Explorer | PASS | Every path is resolved from `$PSScriptRoot`; no ambient PATH or working-directory assumption; no environment variable read |
| Line coverage threshold | PASS | Section 5 |
| No coverage regression on changed lines | PASS | Section 5 |

Fixture mechanism: **PASS**, and it is the only compliant one available given that temporary files are prohibited with no approved exception. Every fixture is a here-string assigned to a script-scoped variable and cast to `[xml]` where a document is needed. Test-only helper functions are declared inside `BeforeAll` rather than at file scope, which is required because Pester 5 runs each `It` in a child scope of the containing block.

Allowlist handling: **PASS**. No test mocks `Get-KoverageProjectAllowlist`. The one test that reaches a function carrying that allowlist as a parameter default supplies an explicit `-ProjectNames @('Alpha.Core')` value with a comment recording that the default would otherwise perform a recursive repository scan, which a unit test must not do.

### 4.2 C# Unit Test Standards

Verdict: **PASS**. No C# test file changed, so MSTest, Moq and FluentAssertions selection is not exercised by this delivery. The existing C# suite was run end to end twice as part of the Phase 6 observation: 7222 tests, 7222 passed, 0 failed on each run.

## 5. Test Coverage Detail

- PowerShell coverage verdict: PASS. New-code line coverage measures 92.86% and 92.50%, each above the 90 new-code floor and above the 85 and 80 line floors.
- C# coverage verdict: PASS. First-party line coverage measures 85.71% and branch coverage 79.87%, clearing the `CLAUDE.md` floors of 80 and 75 and also the 85 and 75 figures carried by the two rules files.

### 5.1 New-code figures, per file

| File | Line elements | Covered | Percent | Against the 90 floor |
|---|---|---|---|---|
| `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` | 42 | 39 | 92.86% | clears |
| `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` | 40 | 37 | 92.50% | clears |

Source: `evidence/qa-gates/p7-t7-new-code-coverage.md`, pass 2, paired with the emitted JaCoCo document at `evidence/qa-gates/p7-t7-new-code-coverage.jacoco.xml`. The aggregate percentage the runner exposes was correctly not used, because an aggregate across both files cannot render the per-file verdict the floor requires.

The residual uncovered lines are recorded rather than implied. In the summary part file, lines 56, 70 and 132 remain unexercised. In the projection part file, line 121 is the no-root-element throw and remains unexercised; lines 192 and 194 are the retained-directory arithmetic of `Test-RawCoverageDocumentRetained`, which is exercised by three tests in `tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1` that this gate's fixed two-file run path excludes. Those two lines are therefore covered by the delivery's suite but invisible to this measurement, which understates the projection file's true figure rather than overstating it.

### 5.2 Changed-line regression

No regression is demonstrable on any changed line. The three modified production files gained 63, 92 and 1 lines and lost 3, 5 and 0. The added regions are directly exercised: the argument-builder additions by the two new results-directory test files, the summary write and the discard sequencing by the call-order test that captures order through the wrapper seams, the non-fatal warning branch by its own test, and the projection wiring by two abstract-syntax-tree assertions that bind the projection call's document argument to the variable the post-processor assigns. The passed count over the same test folder rose from 103 to 133.

### 5.3 What was not measured, and why it is judged non-blocking

No post-change folder-wide figure over `scripts/vscode` was captured, and no repo-wide PowerShell figure was captured at any point. The Phase 0 folder-wide baseline stood at 76.96%, from 551 covered of 716 JaCoCo line elements. That figure sits below the `CLAUDE.md` repo-wide floor of 80 and it did so before this delivery began.

Three observations bear on the disposition, and each is checkable.

First, the direction of movement is upward, not downward. The two new part files add 82 line elements of which 76 are covered, so the same folder-wide computation over the post-change file set yields 627/798, or 78.57%. The two files this delivery adds cannot have lowered the folder-wide figure, because each is covered well above the folder's prevailing rate.

Second, the shortfall is attributable to files this delivery does not touch. The Phase 0 JaCoCo document enumerates twelve production files in `scripts/vscode`. The executed container list from the final test run enumerates sixteen test files, and none of them targets `Invoke-Restore.ps1`, `Sync-PackageReferences.ps1` or `TestProcessCleanup.ps1`. `TestProcessCleanup.ps1` is measured in the baseline document as wholly uncovered: every one of its `line` elements carries a covered-instruction count of zero. None of those three files is created or modified by this delivery, and none is in its Write Set.

Third, the figure quoted is neither a per-file figure nor a repo-wide one. It is folder-scoped to `scripts/vscode`, and the repository contains PowerShell production files outside that folder which no measurement in this delivery's evidence covers.

The judgement is that the absent post-change folder-wide figure is non-blocking. Every figure that was measured clears every applicable floor; the arithmetic shows the unmeasured figure moved upward; and the remaining shortfall is a pre-existing gap in three untested scripts that this delivery is not responsible for and could not close without exceeding its declared footprint. The correct remedy is a separate item that adds test files for those three scripts, and the correct reading of this delivery's obligation is the per-new-file floor, which it clears on both files. The gap is recorded as an open item in section 8 so it is visible rather than absorbed.

## 6. Test Execution Metrics

| Metric | Baseline | Final | Source |
|---|---|---|---|
| PowerShell tests passed | 103 | 133 | `evidence/baseline/p0-t12-powershell-test-baseline.md`, `evidence/qa-gates/p7-t3-final-test.md` |
| PowerShell tests failed | 0 | 0 | same |
| PowerShell tests skipped | 0 | 0 | same |
| PowerShell test containers executed | 13 | 16 | `evidence/qa-gates/p7-t3-final-test.md` |
| PowerShell analyzer diagnostics | 16 | 16 | `evidence/baseline/p0-t11-powershell-analyze-baseline.md`, `evidence/qa-gates/p7-t2-final-analyze.md` |
| PowerShell analyzer entries absent from baseline | — | 0 | `evidence/qa-gates/p7-t2-final-analyze.md` |
| C# tests executed | — | 7222 | `evidence/regression-testing/p6-t2-default-output-run.md` |
| C# tests failed | — | 0 | same |

All seven test files the delivery creates or repairs appear by name in the executed container list, on both passes. The container list was byte-identical across the two passes, so the count movement from 131 to 133 is exactly the two tests the coverage remediation added.

The analyzer comparison is a diagnostic-set comparison by the tuple of rule name, file leaf name and severity, not an exit code. That is the correct gate shape here: the MCP analyzer tool exits 1 on any warning and `Invoke-MSTestWithCoverage.Helpers.ps1` already carried an unsuppressed `PSUseSingularNouns` warning at Phase 0, so an absolute exit-zero demand would be unsatisfiable for a reason this delivery does not cause. Zero diagnostics of any severity are reported against any of the six files the delivery creates.

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Format check | `mcp__drm-copilot__run_poshqc_format` over both script folders | clean on both passes | PASS |
| Lint check | `mcp__drm-copilot__run_poshqc_analyze` paired with a direct enumerating `Invoke-ScriptAnalyzer` run | 16 diagnostics, 0 absent from baseline | PASS |
| Type check | `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` | exit 0, 0 warnings, 0 errors | PASS |
| Unit tests | `Invoke-Pester` over `tests/scripts/vscode` | 133 passed, 0 failed, 0 skipped | PASS |
| Confidentiality masking scan | case-insensitive token search over the change set | zero account, host and organization matches in the corrected files | PASS |
| Suppression scan (added lines) | search for analyzer suppressions in the six created files | none added | PASS |
| Workflow change scan | inspection of the change set for `.github/workflows/**` | no workflow file changed | PASS |

The confidentiality masking scan was reproduced independently rather than accepted from the evidence artifacts. A case-insensitive search of `.claude/agent-memory/` for the account and host tokens returns five matching files, and none of them is among the five this delivery corrects. `TaskMaster/TaskMaster.csproj` carries `<PublishUrl>publish\</PublishUrl>` at line 37 with no `OneDrive` segment, no drive letter and no account token. `.vscode/settings.json` carries `${workspaceFolder}/.vscode/excel-pq-symbols` and the target directory exists and contains `excel-pq-symbols.json`. The five corrected agent-memory files use `<account>`, `<host>`, `<user>` and `<repo-root>` placeholders throughout.

The five remaining agent-memory files that still match the account or host token are out of this delivery's declared scope. `spec.md` states that explicitly, cites R7.1 as establishing that the five named files are not the complete population, and assigns the remainder to the repository-wide sweep item #602. Those five files are not in the change set and this delivery neither introduced nor could cure them.

New raw-document commitment: **zero**. The change set contains no `.trx` document and no `.cobertura.xml` document at all, which is the stronger statement than a naming check and is the one the evidence supports. Verified independently by a glob over the feature folder for `*.trx`, `*.cobertura.xml` and `*.coverage`, which returned no files, and corroborated by the changed-path union scan at `evidence/regression-testing/p6-t4-default-name-scan.md`, which records `UNION_TRX_COUNT: 0` and `UNION_COBERTURA_COUNT: 0` over 195 post-merge paths. The two JaCoCo documents committed under the feature folder's evidence tree are projections, which is exactly the form the new `CLAUDE.md` convention permits, so the delivery complies with the rule it authors.

## 8. Gaps and Exceptions

### 8.1 Non-blocking findings

Eleven findings, all Non-blocking. Full detail, location and recommendation for each are in `code-review.2026-09-13T15-20.md`; the summary here names them so this document stands alone.

1. The coverage entry point does not discard the raw test-result document after writing its summary, although the plain entry point does. Verified from the post-run directory listing in `evidence/regression-testing/p6-t3-external-output-run.md`, which retains `mstest-coverage-run.trx`.
2. The conditional discard of the raw coverage document is nested inside the success of the test-result summary write, adding an undeclared fourth precondition to an invariant stated purely in terms of directory identity.
3. The terminal `Done. Coverage artifact:` line names a path that the preceding discard may have removed.
4. The exact-shape projection assertion normalises line endings on both sides, a documented deviation from AC1's "exactly equal to a here-string literal" wording.
5. Two test suites avoid real `Remove-Item` and `New-Item` calls incidentally rather than by mocking them, so a future move of the discard outside its current guard would make them touch the real filesystem.
6. The coverage entry-point tests mock ten cmdlets, which makes them closer to wiring assertions than behaviour tests.
7. No post-change folder-wide or repo-wide PowerShell line-coverage figure was measured; the reasoning and disposition are in section 5.3.
8. `artifacts/pr_context.summary.txt` asserts eight auto-close issues including `#602`, `#646`, `#662`, `#718` and a malformed `#UTF-8`, where `spec.md` closes only `#671` and `#728` and explicitly sequences `#602` after this item.
9. `evidence/qa-gates/p7-t16-delivery-commit.md` records `PATHSPEC_COUNT: 24` while its own prose names 23 pathspecs; the enumerated block contains 24.
10. The projection part file's header comment states thirty lines of helpers headroom where `spec.md` cites R5 as measuring 29. The comment carries the correct figure; 500 minus 470 is 30.
11. `artifacts/pr_context.summary.txt` classifies `TaskMaster/TaskMaster.csproj` under documentation and tooling rather than as a C# change. This audit evaluated C# on its own merits regardless, which is why section 3.2 exists.

### 8.2 Documented policy conflict, unresolved in the repository

`CLAUDE.md` states a repository-wide line floor of 80 and a new-module floor of 90. `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state a uniform line floor of 85 and a branch floor of 75 across all tiers. The two are not reconciled anywhere in the repository. This audit applied both and records that every measured figure clears both: C# at 85.71% line and 79.87% branch clears 80/75 and 85/75; the two new PowerShell part files at 92.86% and 92.50% clear 90, 85 and 80. No verdict in this document depends on which document governs. Reconciling the two is outside this delivery's scope and is named here so the conflict is visible rather than silently resolved by whichever document a future reviewer happens to read first.

### 8.3 Accepted deviations claimed by the delivery

Three deviations are claimed in the delivery's own documents, and each is judged legitimate.

- Both msbuild gates are evaluated baseline-relative rather than against absolute zero. This is the correct reading: "no new diagnostics" has no meaning without a recorded prior count, and the pre-change state of a whole-solution rebuild is not this delivery's to repair. In the event both gates recorded exit 0 with zero errors and zero warnings, so the distinction did not matter.
- The PowerShell analyzer gate is a diagnostic-set comparison rather than an exit code, for the reason given in section 6.
- The Phase 6 flakiness-attribution rule was applied once. The first end-to-end attempt aborted on three `QuickFiler.Test` failures whose root cause was a failed bind of `netstandard, Version=2.1.0.0`; the executor recorded and reported them rather than modifying a test outside the Write Set. That attribution is confirmed correct: the defect was repaired on `main` by item #877 and the re-run passed 7222 of 7222 with no Write Set file modified to make it pass. Recording the aborted attempt alongside the successful one, rather than erasing it, is the right handling of an audit trail.

### 8.4 Out-of-scope items, correctly sequenced

The historical sweep over already-tracked raw evidence documents is #602 and must run after this item so a fresh test run does not reintroduce the prefix the sweep removes. The further agent-memory occurrences under a broader pattern belong to the same item. The obligation to have the atomic-plan contract cite the new convention cannot be delivered here because that contract file is push-down owned; it is recorded as an upstream follow-up in the Rollout section of `spec.md`. Building a redaction sweep as executable code is materially larger scope and belongs to the sweep item; the delivery correctly delivered the obligation as rule text instead and says so.

## 9. Summary of Changes

107 paths changed in the range against the resolved base, of which 22 sit outside this feature folder.

| Category | Count | Detail |
|---|---|---|
| PowerShell production files | 5 | 2 created, 3 modified |
| PowerShell test files | 7 | 4 created, 3 repaired by forced signature change |
| Repository instruction file | 1 | `CLAUDE.md`, new convention section and two toolchain-step amendments |
| Project file | 1 | `TaskMaster/TaskMaster.csproj`, publish-destination element |
| Editor settings | 1 | `.vscode/settings.json`, Power Query symbols directory |
| Agent-memory documents | 6 | 5 token substitutions plus 1 rule-text addition |
| Promotion rename | 1 | inherited from the pre-Phase-0 preparation commit |
| Feature folder documents and evidence | 85 | issue, spec, user story, plan, research, 66 evidence artifacts |

Created production files: `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` (197 lines, three pure functions), `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` (150 lines, two pure functions).

Modified production files: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (+92/-5), `scripts/vscode/Invoke-MSTest.ps1` (+63/-3), `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (+1/-0).

Created test files: `Invoke-MSTestWithCoverage.Projection.Tests.ps1` (495), `Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1` (268), `Invoke-MSTest.TrxSummary.Tests.ps1` (193), `Invoke-MSTest.ResultsDirectory.Tests.ps1` (119).

Repaired test files: `Invoke-MSTest.RunSettings.Tests.ps1` (+101/-99, splatting conversion), `Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` (+10/-3, mock parameter block), `Invoke-MSTest.Main.Tests.ps1` (exact-array assertion widened from four to six elements).

Governance paths not touched, which AC18 requires and which the change set confirms: no path under `.claude/rules`, `.claude/skills`, `.claude/agents`, `.claude/hooks` or `.claude/lib`; no `.claude/settings.json`; neither `config/blast-radius.json` nor `config/orchestration-routing.json`; no `.gitignore` entry. The only paths under `.claude/` in the change set are the six agent-memory documents.

## Evidence Location Compliance

Verdict: **PASS**. Zero violations.

Every evidence artifact this delivery produces sits beneath `docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/<kind>/` with a canonical kind: `baseline` (19 artifacts), `qa-gates` (19), `regression-testing` (30) and `issue-updates` (1). No artifact is written under a `coverage` kind, and the new-code coverage capture is deliberately placed under `qa-gates` with that reasoning stated in the artifact itself.

No file in the change set sits under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`. A directory listing of `artifacts/` in this worktree returns thirteen files — four PR body and receipt pairs, three Pester outputs, two PR context artifacts and one orchestration checkpoint — and none of them appears in the change set, in the 100-path Phase 7 inventory, or in the 195-path post-merge union. The `validate_evidence_locations.py` scanner was not invoked, because the required tooling was unavailable in this review context; the equivalent check was performed by enumerating `artifacts/` directly and cross-checking both recorded path unions, and both methods agree on zero violations.

No non-canonical evidence path was supplied to this review, so no `EVIDENCE_LOCATION_OVERRIDE_REJECTED` record is required.

## 10. Compliance Verdict

**PASS** — 0 Blocking findings, 11 Non-blocking findings, 23 of 23 acceptance criteria satisfied.

| Policy area | Verdict |
|---|---|
| General Unit Test Policy | PASS |
| General Code Change Policy | PASS |
| PowerShell Code Standards | PASS |
| C# Code Change Policy | PASS |
| PowerShell Unit Test Standards | PASS |
| Coverage thresholds, C# | PASS |
| Coverage thresholds, PowerShell | PASS |
| Evidence location conventions | PASS |
| Tonality | PASS |
| File size ceiling | PASS |
| Change budget | PASS |

Remediation is not required. None of the eleven Non-blocking findings prevents merge, none fails an acceptance criterion, and none produces a committable artifact. Findings 1, 2 and 3 concern the coverage entry point's post-run sequence and are the ones most worth acting on, either in a follow-up item or before merge at the maintainer's discretion; the recommendations are in `code-review.2026-09-13T15-20.md`.

Tonality: **PASS**. The delivery's twelve production and test files, its plan and its 69 evidence artifacts were read and carry no humour, no hyperbole and no decorative metaphor. Statements are matched to their evidence: where a measurement failed it is recorded as failed with its diagnosis, where a figure is derived rather than measured the derivation is stated, and where an observation is complementary rather than load-bearing the artifact says which it is.

## Appendix A: Test Inventory

| Test file | Lines | State | Tests exercised |
|---|---|---|---|
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1` | 495 | created | Projection shape, missed derivation, empty-package boundary, zero-branch uniformity, document order, reused throw wording, reconciliation positive, reconciliation covered-total negative, reconciliation valid-total negative, retention-predicate guard clause, counting-rule delegation by abstract syntax tree |
| `tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1` | 193 | created | Namespaced read, unprefixed-XPath-selects-zero companion, skipped derivation, derivation statement in the rendered text, verdict and failed names, empty failed-name collection, missing result-summary throw |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1` | 268 | created | Coverage-builder switch membership, both switches after the separator by index, entry-point results-directory default by abstract syntax tree, retention true for the coverage directory, false for an unrelated directory, false for a subdirectory, discard ordering by captured call order, projection built from post-processed content, reconciliation call present |
| `tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1` | 119 | created | Plain-builder switch membership and order by index, entry-point results-directory default by abstract syntax tree, non-fatal summary path emits one warning and writes and deletes nothing |
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | 498 | repaired | Runsettings resolution, both builders' argument arrays, both wrapper seams, derived-settings lifecycle across success and failure, entry-point happy path, threshold-throw persistence, worktree exclusion, four isolated error paths |
| `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` | 146 | repaired | Runsettings resolution positive and negative, splatting seam, four entry-point guards, no-execute short circuit, exact six-element argument array, defaulted search root and configuration, nonzero exit code |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` | 106 | repaired | Three worktree-exclusion discovery cases |

Production files under test: `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1`, `scripts/vscode/Invoke-MSTest.TrxSummary.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `scripts/vscode/Invoke-MSTest.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`.

## Appendix B: Toolchain Commands Reference

PowerShell, in the order the rule mandates:

1. Format: `mcp__drm-copilot__run_poshqc_format` with `scan_folders` set to `["scripts/vscode", "tests/scripts/vscode"]`
2. Lint: `mcp__drm-copilot__run_poshqc_analyze` with the same `scan_folders`, paired with a direct `Invoke-ScriptAnalyzer -Path <folder> -Recurse` run that enumerates each diagnostic
3. Type check: the rule directs this step to be skipped for PowerShell and to proceed to testing
4. Test: `mcp__drm-copilot__run_poshqc_test` with the same `scan_folders`, paired with a direct `Invoke-Pester` run over `tests/scripts/vscode` with `Run.PassThru` and an explicit exit statement

C#, in the order `CLAUDE.md` mandates:

1. Format: `dotnet tool run csharpier check .`
2. Analyze: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. Type check: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. Test: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /ResultsDirectory:coverage\test-results /Logger:trx;LogFileName=mstest-run.trx`

The fourth C# step is the one this delivery amended. Both `CLAUDE.md` toolchain listings now carry the explicit results-directory switch and the explicit log-file-name form, which is what stops the test console producing its default account-and-host-and-timestamp file name.

Coverage capture, PowerShell: `Invoke-Pester` with `CodeCoverage.Enabled`, `CodeCoverage.Path` set to the two new part files, `CodeCoverage.OutputFormat` set to JaCoCo and `CodeCoverage.OutputPath` set beneath the feature folder's `evidence/qa-gates/` directory.

Coverage capture, C#: the repository coverage entry point `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, whose first-party headline line is the figure quoted in this audit.
