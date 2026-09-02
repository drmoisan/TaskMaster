# Policy Audit — Issue #646 (qfc-metrics-flush-writes-empty-session-file)

Timestamp: 2026-09-01T12-53

| Field | Value |
|---|---|
| Branch | `bug/qfc-metrics-flush-writes-empty-session-file-646` |
| HEAD | `0fe0668f146236c65aa93514fcb9756d366a6940` |
| Base branch | `origin/main` |
| Merge base | `8996b28746d32f9f5996a037e0ca76be78b7684d` (verified an ancestor of HEAD) |
| Branch diff | 31 files, 3223 insertions, 0 deletions (`git diff --shortstat origin/main...HEAD`) |
| Work mode | `minor-audit` (from the `- Work Mode:` marker in `issue.md`) |
| AC source | `issue.md` section `## Acceptance Criteria` only (AC1-AC8) |
| Blocking findings | **0** |

## Audit Scope Statement

The audited scope is the full branch diff against the resolved base branch `origin/main`, not
the scope of any plan, task, or phase. All 31 changed paths were enumerated and reviewed.

## Rejected Scope Narrowing

No caller instruction attempted to narrow the audit scope to a plan, task, phase, or file
subset, and no instruction attempted to suppress a toolchain or coverage check for a language
with changed files on the branch. Two strings in the feature folder resemble narrowing
directives and were assessed; neither is one:

| String | Location | Assessment |
|---|---|---|
| `DIRECTIVE: PREFLIGHT VALIDATION ONLY` | `plan.2026-08-31T20-04.md` line 414 (plan trailer) | Planner-to-executor handoff text governing the plan document's own validation, not the review's scope. Full branch audit performed regardless. |
| `- **Directive:** MINIMAL-AUDIT PLAN REQUIRED` | `plan.2026-08-31T20-04.md` line 10 | Selects the plan template shape for `minor-audit` work mode. It does not limit which files this review examines. |

The caller instruction that `artifacts/csharp/coverage.xml` must not be created is recorded and
was honoured, but it is not treated as scope narrowing: the C#/.NET coverage rows below carry
explicit verdicts and the absence of that artifact is itself recorded as a FAIL row rather than
suppressed.

## PR Context Artifacts

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` were absent at the
start of this review and were regenerated from `git diff origin/main...HEAD` at HEAD
`0fe0668f`. The `artifacts/` tree is excluded from version control by `.gitignore` line 57
(`artifacts/`), verified with `git check-ignore -v`, so regenerating them added no tracked path
and left the AC7 footprint unchanged. `git status --porcelain` returns empty after the
regeneration, which is the direct proof.

The regenerated summary classifies the two changed `.cs` files as C#. This is recorded
explicitly because the generator that normally produces this artifact has a recurring defect in
which C# changes are reported as documentation-only; the classification here was derived
mechanically from the branch diff rather than inherited.

## Evidence Location Compliance

`.claude/rules` and `.claude/skills/evidence-and-timestamp-conventions` require execution
evidence at `<FEATURE>/evidence/<kind>/`. The branch diff was scanned for files written under
the prohibited locations `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, and
`artifacts/coverage/`.

| Prohibited prefix | Paths found in branch diff | Verdict |
|---|---|---|
| `artifacts/baselines/` | 0 | PASS |
| `artifacts/qa/` | 0 | PASS |
| `artifacts/evidence/` | 0 | PASS |
| `artifacts/coverage/` | 0 | PASS |

All 25 evidence files are under
`docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/`
in the canonical `baseline/`, `qa-gates/`, `regression-testing/`, and `other/` subdirectories.
`validate_evidence_locations.py` does not exist in this repository, so the scan was performed
directly against the diff path list. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose:
no delegation instruction specified a non-canonical evidence path.

## Toolchain Compliance (CLAUDE.md C#, CUT3)

The mandated order is csharpier format, csharpier check, analyzer rebuild, nullable rebuild,
vstest with coverage. Each gate is evidenced with a command line, an exit code, and verbatim
summary output.

| # | Gate | Command | Exit | Evidence | Verdict |
|---|---|---|---|---|---|
| 1 | Format | `dotnet tool run csharpier format .` | 0 (both passes) | `evidence/qa-gates/csharpier-format.2026-08-31T20-04.md` | PASS |
| 2 | Format check | `dotnet tool run csharpier check .` | 0, 1566 files | `evidence/qa-gates/csharpier-check-final.2026-08-31T20-04.md` | PASS |
| 3 | Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0, 5 warnings / 0 errors | `evidence/qa-gates/msbuild-analyzer-rebuild.2026-08-31T20-04.md` | PASS |
| 4 | Type check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0, 5 warnings / 0 errors | `evidence/qa-gates/msbuild-nullable-rebuild.2026-08-31T20-04.md` | PASS |
| 5 | Test | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage` | 0, 1285/1285 passed | `evidence/qa-gates/vstest-coverage-run.2026-08-31T20-04.md` | PASS |

Supporting observations that make these gates meaningful rather than vacuous:

- **Restart rule honoured.** Format pass 1 rewrote one tracked file (CSharpier collapsed a
  three-line FluentAssertions chain in the new test onto one line). The loop restarted from the
  format step, as the General Code Change Policy section 8.1 requires. Pass 2 reached a
  fixpoint with no newly-modified path and no changed-line growth. The gates numbered 2 through
  5 then ran in one uninterrupted sequence with no further restart.
- **Non-vacuity of the two rebuilds.** Both used `/t:Rebuild`, not `/t:Build`, and both logs
  record 36 `csc.exe` command-line occurrences matching baseline. This is the check that
  distinguishes a real compile from MSBuild's incremental up-to-date skip, which returns exit 0
  having run no analyzers. The gates were capable of failing.
- **Nullable command fidelity.** The type-check command is character-for-character the CI step
  and correctly omits `/p:Nullable=enable`, which is a solution-wide opt-in this repository
  does not use. Neither changed file carries a `#nullable enable` pragma, so neither
  participates in nullable-flow analysis; the gate's actual reach here is the stronger general
  condition that the change introduces no C# compiler warning of any kind, since
  `TreatWarningsAsErrors` promotes all of them.
- **Gates remain valid at HEAD.** `git diff --name-only 10aaaf65 HEAD -- "*.cs"` returns empty,
  so no C# source changed after the commit these gates ran against. The one later commit
  (`0fe0668f`) touches zero `.cs` files. The two `.jacoco.xml` files it adds sit under
  `**/evidence/**`, which `.csharpierignore` excludes, so they cannot perturb the format gate.

## Coverage Verification

Languages with changed files in the branch diff: **C# only**. TypeScript, Python, and
PowerShell have zero changed files on this branch, so no coverage obligation arises for them
and no verdict is owed.

### Evidence basis

The committed coverage evidence is a package-level JaCoCo projection of the raw Cobertura the
runner emitted. The raw reports (52,131,269 bytes, 892,256 lines combined) were converted and
deleted after the three gates that read them had completed, following repository precedent
`d0955dc4`. This reviewer independently re-summed the `LINE` counters in both committed
projections:

| Report | Re-summed covered | Re-summed valid | Figure the substitution record claims | Match |
|---|---|---|---|---|
| Baseline | 48426 | 142226 | 48426 / 142226 | Exact |
| Final | 48436 | 142240 | 48436 / 142240 | Exact |

The first-party subset was also re-derived independently from the final projection
(`QuickFiler` + `UtilitiesCS` + `ToDoModel` + `TaskVisualization` + `Tags` + `SVGControl`):
14540 covered of 62121 valid, reproducing the recorded 23.4059% exactly. The substitution is
therefore lossless with respect to every counter any gate relied on, and the gate sequence is
auditable from the committed evidence. The substitution is assessed as **adequately recorded**,
not as missing evidence.

### Denominator statement

The measured `line-rate` of 0.3405 is a single-assembly unfiltered figure. Only
`QuickFiler.Test.dll` was executed, and the 15-package denominator includes eight vendored
third-party assemblies plus the `QuickFiler.Test` assembly itself. It is not this repository's
policy denominator, which is nine first-party packages with no `*.Test` assembly, and it is not
quoted here as a repository figure.

### C# / .NET coverage verdicts

| # | C# / .NET coverage measure | Floor | Observed | Verdict |
|---|---|---|---|---|
| C1 | C# repository-wide line coverage from the canonical artifact `artifacts/csharp/coverage.xml` | >= 85% | The canonical artifact is absent, so no repository-wide C# line coverage figure exists to evaluate | **FAIL** |
| C2 | C# repository-wide branch coverage from the canonical artifact | >= 75% | The canonical artifact is absent, and the committed run emitted zero `condition-coverage` occurrences, so no C# branch coverage figure exists to evaluate | **FAIL** |
| C3 | C# line coverage of the changed production file `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | >= 85% | 77.60% final against 77.05% baseline (97 of 125 measured lines); improved, but under the floor | **FAIL** |
| C4 | C# new-code line coverage of the four added guard lines | >= 90% | 100% — 3 of 3 instrument-measured lines at `hits=1`; the closing brace emits no sequence point | **PASS** |
| C5 | C# new-branch coverage of the added guard condition | both outcomes exercised | Both exercised — true outcome by the new test, false outcome by the two pre-existing non-empty-array tests | **PASS** |
| C6 | C# no-regression on changed lines coverage | no line moves covered to uncovered | Zero pre-existing lines were changed; every line at `hits=1` in baseline remains `hits=1` | **PASS** |
| C7 | C# no-regression on the like-for-like measured coverage denominator | final not below baseline | 0.3405230596 final against 0.3404862683 baseline, from the identical invocation over the identical assembly set | **PASS** |

Non-C# languages, recorded for completeness. These carry no verdict because they have zero
changed files on this branch: TypeScript (`coverage/lcov.info`), Python
(`artifacts/python/lcov.info`), PowerShell (`artifacts/pester/powershell-coverage.xml`).

### Disposition of the four FAIL rows

C1, C2, and C3 are recorded as FAIL because the measurements they name are either absent or
below the stated floor, and this reviewer does not soften a below-floor or unevidenced row into
a PASS. All three are assessed as **non-blocking**, for reasons that are structural rather than
discretionary:

1. **C1 and C2 are measurement-provisioning gaps, not code defects.** Producing the canonical
   repository-wide artifact requires a full-suite coverage pass across all test assemblies.
   That work would change no line of the delivered code and would alter none of C4, C5, C6, or
   C7. The gap predates this branch and is unaffected by it.
2. **C2's branch figure was never measured rather than measured at zero.** The run carried
   `branch-rate="1"` at the root with zero `condition-coverage` occurrences, verified on the
   source before deletion. Reading the projection's zero BRANCH counters as zero branch
   coverage would be a misreading of the instrument. The one new branch this change introduces
   is separately shown fully exercised at C5.
3. **C3's shortfall is entirely pre-existing and structurally untouchable on this branch.** The
   28 uncovered lines in the changed file are the Outlook-interop `WriteMoveToCalendar` path
   and the writer-failure logging branch, both of which predate this change. This change moved
   the file's coverage up by 0.55 points and added no uncovered line. Raising the file above
   85% would require either new tests against COM-bound code or a refactor, and AC7 forbids
   modifying any file other than the two owned ones — so no remediation is available within
   this branch's mandate.
4. **The substantive coverage obligations are met.** C4 clears the 90% new-code floor at 100%,
   C5 shows both new branch outcomes exercised, and C6 shows no changed-line regression. These
   are the rows that measure what this change actually did.

No `remediation-inputs` artifact is produced, because no finding requires a code change on this
branch. The C1 and C2 provisioning gap is reported to the caller for separate scheduling.

## Cross-Language Policy Compliance

| Policy | Requirement | Observed | Verdict |
|---|---|---|---|
| General Code Change — file size | No file over 500 lines | Production file 231 lines; test file 477 lines (`wc -l` and `awk NR` agree) | PASS |
| General Code Change — design | Simplicity first, mirror existing style | Four-line early return mirroring the EFC sibling; no indirection added | PASS |
| General Code Change — error handling | Fail fast, no silent error swallowing | The guard is a no-content short circuit, not an error path; the writer-failure logging branch is untouched | PASS |
| General Code Change — I/O boundaries | Domain logic testable without filesystem | The writer is an injectable delegate; the new test touches no filesystem | PASS |
| General Unit Test — framework | MSTest, Moq, FluentAssertions | `[TestMethod]`, `Mock<IQfcCollectionController>`, `.Should().BeFalse(...)` | PASS |
| General Unit Test — determinism | No `Thread.Sleep`, `Task.Delay`, wall-clock waits | None present; the stub returns `Task.FromResult(true)` | PASS |
| General Unit Test — no temp files | Creation of temp files in tests prohibited | None; the writer delegate is replaced with an in-memory flag capture | PASS |
| General Unit Test — AAA structure | Arrange, Act, Assert | Present and visually separated | PASS |
| General Unit Test — documented intent | Descriptive name plus summary | 6-line XML doc comment plus a self-describing method name | PASS |
| General Unit Test — scenario completeness | Boundary and negative cases | The empty-array boundary is the case added; the non-empty side is held by two pre-existing tests | PASS |
| General Unit Test — coverage exclusions | No production path excluded from measurement | No exclusion added; `.csharpierignore` changes none and `coverage.config` is unmodified | PASS |
| Bugfix workflow | Failing regression test first, then minimal fix | RED at exit 1 with a genuine 346 ms assertion failure, then the four-line fix, then GREEN at exit 0 | PASS |
| Tonality | Professional, factual, no hyperbole | Evidence artifacts are measured and specific throughout | PASS |
| Policy documents | Not modified by this branch | No path under `.claude/rules/` or `.github/instructions/` in the diff | PASS |

### Test file location

`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` mirrors
`QuickFiler/Controllers/QfcHomeController.Metrics.cs` under this repository's established
`<Project>.Test/` convention rather than the `tests/` tree named in
`.claude/rules/general-unit-test.md`. The change adds a method to a file that already existed
at that path; it neither creates nor moves a test file, and General Code Change Policy section
7.1 directs matching the existing repository style. Recorded as a pre-existing repository-wide
convention divergence, not a finding against this branch.

### Tier-dependent gates

`.claude/rules/quality-tiers.md` requires a `quality-tiers.yml` at the repository root mapping
every project to a tier. That file does not exist in this worktree, so the tier of `QuickFiler`
cannot be resolved and the tier-dependent gates (property-test density, mutation score, golden
tests) cannot be evaluated against a declared tier. This is a pre-existing repository condition
that this branch neither introduced nor was capable of changing under AC7. The uniform gates,
which do not depend on tier, are all evaluated above.

## Coverage Floor Documentation Conflict

`CLAUDE.md` UT2 states a repository-wide floor of 80% with 90% for new modules.
`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state a uniform 85%
line and 75% branch floor across T1-T4. The two are unreconciled in the repository. This audit
reports against the stricter 85%/75% pair for the repository-wide and modified-file rows, and
against the 90% new-code floor for the guard lines, which is the stricter reading on both
counts. The delivered result at C4 (100%) clears every variant of the new-code floor. The
conflict is noted so the C3 verdict is read against the correct authority.

## Summary

| Category | PASS | FAIL |
|---|---|---|
| Toolchain gates | 5 | 0 |
| Coverage rows | 4 | 3 |
| Cross-language policy | 14 | 0 |
| Evidence location | 4 | 0 |

**Blocking findings: 0.** The three FAIL coverage rows are measurement-provisioning and
pre-existing-shortfall conditions with explicit non-blocking dispositions recorded above. No
finding on this branch requires a code change.
