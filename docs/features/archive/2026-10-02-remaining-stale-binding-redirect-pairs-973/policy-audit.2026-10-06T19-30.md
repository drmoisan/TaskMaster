# Policy Audit: remaining-stale-binding-redirect-pairs (Issue #973)

- Timestamp: 2026-10-06T19-30
- Branch: bug/remaining-stale-binding-redirect-pairs-973
- Head: 9b0d5421707aa9a192afc6ad3700a4e9667f0744 (last entry of the worktree reflog; commit "docs(973): record evidence and acceptance check-offs for the binding-redirect sweep")
- Base: merge base 993fdd01566dee82e5f37acb761a600feaaa1454 (evidence/baseline/p0-t2-anchors.2026-10-03T10-41.md MERGE-BASE; the caller's diff file was cut against the same SHA; origin/main at the executor's anchor was f8ea1b5dcc6514bc0088bc80965c188bfd717557)
- Work mode: full-bug (issue.md line 12); acceptance-criteria source: spec.md only (AC1 to AC23, spec.md lines 357-379)
- Reviewer: feature-review, no-Bash mode (caller directive). Every check was made with Read, Grep and Glob against the item worktree, the caller-supplied branch diff file, the committed evidence under evidence/, the gitignored raw coverage documents left in the worktree (coverage/baseline-973.cobertura.xml, coverage/final-973.cobertura.xml, artifacts/pester/powershell-coverage.xml) and the worktree reflog as the head reference and clock. Where a check needs a shell it is recorded as such with the reason.
- Timestamp derivation: no shell clock was readable in this session. The label above is assigned, not read: it is later than the head commit's reflog epoch 1791326875 (2026-10-06T22:47:55Z, 18:47:55 at the recorded -0400 offset) and later than every label in the feature folder (latest 2026-10-06T18-47, evidence/other/p5-t25-commit-c.2026-10-06T18-47.md). The executor's labels were cross-checked against the reflog: the P3-T24 census label 18-19 matches the record commit epoch 1791325191 (18:19:51 -0400), the P4-T3 label 18-22 precedes its record commit epoch 1791325460 (18:24:20 -0400) by two minutes, and the P5-T25 label 18-47 matches epoch 1791326875 (18:47:55 -0400); the evidence labels are clock-derived.

## Executive Summary

Overall verdict: REMEDIATION_REQUIRED. 2 Blocking findings: B-1 (AC17, the invariant-trace criterion, is unsatisfiable as written because its Azure.Core clause presumes a requester that is not deployed to the add-in output directory; remediability class autonomous, by a spec amendment under the in-item precedent of orchestrator-ruled criterion amendments) and B-2 (AC18, the manual designer and add-in check, is pending the maintainer's runbook run; remediability class human_decision_required, as the caller designated). 1 finding of class autonomous, so the verdict is REMEDIATION_REQUIRED rather than HALT_NON_REMEDIABLE. Non-blocking findings and observations are detailed in code-review.2026-10-06T19-30.md. AC1 to AC16 and AC19 to AC23 verified PASS against the diff, the worktree and the evidence; AC17 is PARTIAL and AC18 is PENDING MANUAL, both left unchecked. The delivered change itself is correct and complete over its full diff: 137 stale redirect entries corrected, 13 dead ADAL blocks deleted, the System.Linq.AsyncEnumerable 10.0.12 package installed behind an aliased Reference in five projects with 15 redirects pointed at the deployed version, the regression test emptied and two guards added, six unused Graph directives removed, CategoryClassifierGroup.cs split under the 500-line limit with the type's member set and coverage identical, and the CLAUDE.md premise corrected.

| Area | Verdict | Evidence summary |
|---|---|---|
| General Unit Test Policy | PASS | The edited Pester It and the two new It blocks are deterministic reads of tracked files with explicit non-vacuity guards; AAA markers and because-texts throughout; no temporary file, TestDrive or sleep; 153/153 PowerShell tests and 7361/7361 C# tests pass |
| General Code Change Policy | PASS | Bugfix workflow honoured: the gate was observed red with exactly the 15 pairs before any config edit, red again after the sweep alone (negative control naming only System.Linq.AsyncEnumerable), green after the install; every touched or added file at or under 500 lines (test file 402, CategoryClassifierGroup.cs 442, new partial 106); toolchain single pass |
| C# Code Change Policy | PASS | csharpier check exit 0 (1638 files, baseline 1637 plus the new partial); analyzer /t:Rebuild 0 errors 0 warnings, SKIP_CORECOMPILE_LINES 0; TreatWarningsAsErrors /t:Rebuild 0 errors 0 warnings, SKIP_CORECOMPILE_LINES 0; CS0121/CS0433/MSB3277 0; no /p:Nullable=enable |
| C# Unit Test Policy | PASS | MSTest route 7361/7361; first-party lines 85.35%, branches 79.74% (floors 80/75 per CLAUDE.md, 85/75 per .claude/rules, both met); equal denominators to baseline |
| PowerShell Code Change and Test Policy | PASS | PoshQC format rewrote nothing (16 of 16 hashes identical), analyze ok, test 153/153 with the changed suite at 16/16; one test file changed, zero production PowerShell files |
| Coverage (C#) | PASS | Repo-wide 85.35% lines / 79.74% branches against 85.36% / 79.75% at baseline with identical denominators (65855 lines, 17078 branches); the new partial file reads 100% lines / 87.5% branches; the split preserved the type aggregate exactly (229/318 lines, 45/58 branches at both stages) |
| Coverage (PowerShell) | FAIL on the canonical artifact, PASS on changed lines | artifacts/pester/powershell-coverage.xml reads 0 covered of 9294 lines and instruments only .claude/ and .codex/ (pre-existing instrument defect of the bundled route, recurring); zero production PowerShell lines changed; the module under test is byte-identical; the scripts/ figure comes from the CI Pester job |
| Evidence hygiene | PASS | 0 host paths in the feature folder (reviewer Grep, case-insensitive drive-letter and /c/Users patterns; executor HOST-PATH-RESIDUALS 0); no raw trx, Cobertura or .coverage document committed; no file under artifacts/ in the diff |
| Acceptance criteria | 21 of 23 PASS | AC17 PARTIAL (B-1), AC18 PENDING MANUAL (B-2); see feature-audit.2026-10-06T19-30.md |

## Rejected Scope Narrowing

No scope narrowing was detected in the caller prompt. The following caller statements were evaluated and accepted as tooling constraints, factual inputs or rulings rather than as narrowing:

- "BINDING FIRST DIRECTIVE: do NOT use the Bash tool at all in this run." A tooling constraint. The audit scope remains the full branch diff against the merge base; every one of the 34 paths in the supplied diff file was read, the post-change state of every corrected family was re-read from the worktree, and the C# and PowerShell toolchain evidence was read in full.
- "Branch diff against merge base 993fdd015..., excluding the feature folder and .claude/agent-memory." The supplied diff file omits two path families. The feature folder was reviewed directly from the worktree (spec, issue, plan, runbook, three research records, every evidence file cited in this audit). The .claude/agent-memory/ paths are Markdown agent-memory notes (the 23 inherited at plan start are listed in evidence/baseline/p0-t2-anchors.2026-10-03T10-41.md; evidence/qa-gates/p4-t12-footprint.2026-10-06T18-34.md confirms no other path under .claude/ is in the diff); they carry no code, no coverage obligation and no evidence-location obligation, and this review did not re-enumerate additions after plan start (no shell). This is a recorded limitation, not an accepted narrowing: no language and no coverage check was skipped because of it.
- "KNOWN OPEN ITEMS (evaluate them; do not assume my conclusions)" and "AC18 is manual (runbook); classify it as human_decision_required / pending-manual, not autonomous." Rulings on how two criteria are classified, not on what is audited. Both criteria and every other criterion were evaluated over the full diff; the AC17 conclusion below is the reviewer's own, reached from three agreeing observations.
- "Under the maintainer related-defect directive, related defects ... are remediated inside this item, so classify them autonomous where an agent can fix them." A classification rule, applied to the findings in code-review.2026-10-06T19-30.md; one caller-stated defect (item 4, unquoted plan pathspecs) was found on inspection to be an executor-side command-hygiene matter and not a plan-text defect, and is reported as such.

## Evidence Location Compliance

- Branch diff scan for files under artifacts/baselines/, artifacts/qa/, artifacts/evidence/ or artifacts/coverage/: none. The 34 paths of the supplied diff are 15 app.config, 5 packages.config, 5 csproj, 6 .cs, CLAUDE.md, tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 and docs/features/potential/promoted/2026-10-02-remaining-stale-binding-redirect-pairs.md; the executor's footprint capture against the same base (evidence/qa-gates/p4-t12-footprint.2026-10-06T18-34.md) lists the same 33 code paths plus the promoted record, the feature folder and the agent-memory notes.
- All executor evidence lives under docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/{baseline,regression-testing,qa-gates,other,issue-updates}/ (Glob listing in this review: 100-plus Markdown files; no non-Markdown file).
- validate_evidence_locations.py --root .: not run (Bash was forbidden for this review). The manual scan above substitutes; no violation observed.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required; the caller supplied no non-canonical evidence path.
- PR context artifacts (artifacts/pr_context.summary.txt, artifacts/pr_context.appendix.txt): absent in the review worktree (Read of the exact path failed). The session checkout's pair belongs to another branch (bug/sort-email-latent-logic-defects-959, generated 2026-10-06 22:16:20 UTC) and was not used for scope. Regeneration was not possible without a shell or the collection tool. Scope was derived from the caller-supplied diff file, the committed footprint evidence and the files on disk, three agreeing sources.
- Raw coverage documents: coverage/baseline-973.cobertura.xml and coverage/final-973.cobertura.xml exist locally in the worktree (gitignored, not committed); their class nodes for the six changed C# files were read for this review. The canonical path artifacts/csharp/coverage.xml is absent in both checkouts; the committed JaCoCo package projection plus one-line summary (evidence/baseline/mstest-coverage-baseline.md, evidence/qa-gates/mstest-coverage-projection.md) are the forms CLAUDE.md "Committed Test Evidence Format" requires, and the standing ruling treats executor-committed feature-folder coverage evidence as the present artifact.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Each It block in tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 enumerates the repository tree itself (Get-ChildItem on $script:RepoRoot) and builds its own records; nothing is shared between the main It (lines 275-317), It (a) (lines 319-367) and It (b) (lines 369-399) beyond the imported modules; order independence holds by construction |
| Isolation | PASS | The main It targets the detector's debt and unverifiable sets; It (a) targets the range half of each corrected redirect plus the version pin; It (b) targets the Aliases child of the five Reference elements; each failure message names the offending records (`-Because` joins) |
| Fast execution | PASS | File reads of 17 configs and 17 csproj per It; the whole dependencies suite of 153 tests completes inside one PoshQC run (evidence/qa-gates/poshqc-test.md) |
| Determinism | PASS | Inputs are tracked files only; no network, clock, randomness or environment variable; the fail-before, negative-control and pass-after runs produced the exact predicted messages (15 pairs; netstandard plus System.Linq.AsyncEnumerable; 0 failures) |
| Readability | PASS | Descriptive It titles; Arrange / Act / Assert comments (lines 320, 341, 360, 370, 378, 392); every assertion carries a `-Because` text naming the observed set |

### 1.2 Coverage

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 6 (5 modified, 1 added) | 7361 | 7361 passed, 0 failed | 85.36% lines / 79.75% branches | 85.35% lines / 79.74% branches | 100% lines / 87.5% branches (the one added file) |
| PowerShell | 1 (test file; 0 production files) | 153 (16 in the changed suite) | 153 passed, 0 failed | 0.00% lines (bundled artifact reading; see the per-language block) | 0.00% lines (same artifact and reading) | N/A (zero production PowerShell lines added or changed) |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |

Files Changed counts source files only. The remaining changed paths are 15 app.config, 5 packages.config, 5 csproj, CLAUDE.md and the promoted Markdown record.

Coverage source statement (C#): the figures above are read from the committed projections and summaries (evidence/baseline/mstest-coverage-baseline.md at P0-T19; evidence/qa-gates/mstest-coverage-projection.md and mstest-test-results-summary.md at P4-T9; evidence/qa-gates/p4-t10-coverage-comparison.2026-10-06T18-30.md at P4-T10), each carrying the first-party summary line and the package-level JaCoCo projection, and were cross-checked at the class-node level against the local raw documents coverage/baseline-973.cobertura.xml and coverage/final-973.cobertura.xml. Both runs used the DIRECT route with the identical four-class shell-icon exclusion (spec Planner Amendment 4), so they are comparable.

Verdict lines:

- C# coverage verdict: PASS (repo-wide first-party lines 85.35% and branches 79.74% from the committed post-change projection; above the CLAUDE.md floors of 80% lines and 75% branches and above the 85% / 75% floors in .claude/rules; 0.01 percentage points below the baseline 85.36% / 79.75% with identical denominators, inside the 0.10-point tolerance the spec sets for collector run-to-run variance).
- C# new-file coverage: PASS. UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.ConditionalEngine.cs reads line-rate 1 (49 of 49 instrumented lines) and branch-rate 0.875 (21 of 24) at its class node (coverage/final-973.cobertura.xml line 181702), above the 85% / 75% new-file thresholds. The spec classifies the file as moved members rather than a new module; the figure satisfies the stricter reading as well.
- C# changed-production-file coverage: PASS on the no-regression limb. The only edits to the five modified files are deleted `using` directives, one `partial` modifier and a verbatim region move, none of which carries a sequence point, so no changed line can regress. Per-file readings (class nodes, baseline then final): ManagerAsyncLazy.cs 91.74% / 100% unchanged; StoreWrapper.cs 91.41% / 70% unchanged; FolderMinimalWrapper.cs 91.80% / 90% unchanged; Triage_OlLogic.cs 68.97% / 54.17% unchanged; CategoryClassifierGroup.cs 72.01% / 77.59% at baseline (229/318 lines, 45/58 branches) and 66.91% / 70.59% after the split (180/269 lines, 24/34 branches). The per-file drop in CategoryClassifierGroup.cs is the arithmetic of moving the fully covered region out: original 180/269 plus new file 49/49 equals the baseline 229/318 lines, and 24/34 plus 21/24 equals the baseline 45/58 branches. The type aggregate is identical at both stages.
- C# per-file floor rows: FAIL for CategoryClassifierGroup.cs (66.91% lines after the split; 72.01% before) and Triage_OlLogic.cs (68.97% lines, 54.17% branches, unchanged) against the 85% / 75% per-file floors, both pre-existing and neither moved by a changed line; FAIL for StoreWrapper.cs branches (70%, unchanged). Disposition: non-blocking (no changed-line regression; the type aggregate for CategoryClassifierGroup is unchanged; the spec excludes any refactor or test addition for the moved members). Recorded as code-review CR-3 with a follow-up recommendation.
- C# package-level corroboration: UtilitiesCS LINE missed 4603 / covered 38905 after (4599 / 38909 before) and BRANCH 1861 / 9432 after (1859 / 9434 before); QuickFiler LINE 2294 / 10460 after (2293 / 10461 before); every other package identical. The five covered lines and two covered branches that moved lie outside the CategoryClassifierGroup type (whose aggregate is unchanged) and outside every changed line; this is the run-to-run variance the spec's tolerance was written for.
- PowerShell coverage verdict on the canonical artifact: FAIL (artifacts/pester/powershell-coverage.xml in the worktree, written by the bundled PoshQC test route at 18:23 on 2026-10-06, reads 0 covered of 9294 lines at its report-level counter and its 13 packages are all under .claude/hooks, .claude/lib/* and .codex/; it instruments no file under scripts/, so it cannot report on scripts/dependencies/BindingRedirectVerification.psm1 or any other production script). This is the pre-existing instrument defect of the bundled route recorded at #441, #565 and #928 (promotion candidate P-4); disposition non-blocking because the artifact measures no file this branch changes and the production module is unchanged.
- PowerShell changed-line coverage gate: PASS by vacuity (the only PowerShell file in the diff is the test file tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1, which is outside the denominator by policy; zero production PowerShell lines were added or changed; scripts/dependencies/BindingRedirectVerification.psm1 hashed 51d6664b281cad0b6c8cd01e78c6bc8491a75862 before and after the final format pass, evidence/qa-gates/poshqc-format.md, and no scripts/ path is in the diff).
- PowerShell repo-wide figure for scripts/: produced by the CI Pester job (.github/workflows/_pester.yml, 80% line floor over scripts/dependencies, scripts/hygiene and scripts/vscode together); the executor recorded COVERAGE-MEASUREMENT deferred to CI in every Pester artifact; the module under test is unchanged by this item, so the figure cannot move. No in-session direct-Pester measurement was possible without a shell.
- TypeScript and Python: zero files changed on this branch; no verdict is owed.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/baseline/mstest-coverage-baseline.md` (committed one-line first-party summary and JaCoCo package projection; raw document coverage/baseline-973.cobertura.xml present locally, gitignored; canonical artifacts/csharp/coverage.xml absent in the worktree)
- C# post-change coverage artifact: `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/mstest-coverage-projection.md` with `evidence/qa-gates/p4-t10-coverage-comparison.2026-10-06T18-30.md` (same forms; raw document coverage/final-973.cobertura.xml present locally, gitignored; canonical artifacts/csharp/coverage.xml absent in the worktree)
- TypeScript baseline coverage artifact: none consulted (zero TypeScript files changed on this branch)
- TypeScript post-change coverage artifact: none consulted (zero TypeScript files changed on this branch)
- PowerShell baseline coverage artifact: `artifacts/pester/powershell-coverage.xml` (worktree, gitignored; the P0-T10 baseline run's document was overwritten by later runs of the same tool; the surviving document reads 0 covered of 9294 and instruments no scripts/ file; FAIL row above)
- PowerShell post-change coverage artifact: `artifacts/pester/powershell-coverage.xml` (worktree, gitignored; written by the P4-T3 run at 2026-10-06 18:23; 0 covered of 9294; FAIL row above; the scripts/ figure comes from the CI Pester job)
- Python baseline coverage artifact: none consulted (zero Python files changed on this branch)
- Python post-change coverage artifact: none consulted (zero Python files changed on this branch)
- Per-language comparison summary: the per-language comparison block of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.36% lines (56212/65855) / 79.75% branches (13620/17078). Post-change: 85.35% lines (56207/65855) / 79.74% branches (13618/17078). Change: -0.01% lines (-5 covered) / -0.01% branches (-2 covered), with lines-valid and branches-valid identical at both stages and the moved type's aggregate unchanged (229/318 lines, 45/58 branches). New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/baseline/mstest-coverage-baseline.md, evidence/qa-gates/mstest-coverage-projection.md, evidence/qa-gates/p4-t10-coverage-comparison.2026-10-06T18-30.md, class nodes at lines 56465, 148741, 164776, 179613, 180896 and 181702 of coverage/final-973.cobertura.xml and the same first five of coverage/baseline-973.cobertura.xml.
- PowerShell: Baseline: 0.00% lines. Post-change: 0.00% lines. Change: none measurable (the canonical bundled artifact reads 0 covered of 9294 lines at both stages and instruments only .claude/ and .codex/ scripts; zero production PowerShell lines changed; the changed-line gate passes by vacuity and the scripts/ figure is produced by the CI Pester job). Disposition: FAIL. Evidence: artifacts/pester/powershell-coverage.xml (worktree, gitignored), evidence/qa-gates/poshqc-test.md, evidence/qa-gates/poshqc-format.md (module hash identical before and after).
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Artifact consulted | State | Disposition |
|---|---|---|---|
| C# | Committed projections and summaries under evidence/baseline and evidence/qa-gates; raw Cobertura class nodes read locally | Present; canonical artifacts/csharp/coverage.xml absent in the worktree (recurring observation O-2) | PASS |
| PowerShell | artifacts/pester/powershell-coverage.xml (bundled PoshQC route) | Present but reads zero and omits scripts/; instrument defect, recurring (P-4) | FAIL on the artifact; PASS by vacuity on changed lines; scripts/ figure from CI |
| TypeScript | none | zero files changed | no verdict owed |
| Python | none | zero files changed | no verdict owed |

Coverage exclusion policy check (.claude/rules/general-unit-test.md): the branch adds no coverage-config exclude entry and no ExcludeFromCodeCoverage attribute (reads of the six .cs paths and the five csproj hunks; the csproj hunks are one Reference element each plus one Compile Include). Not Blocking.

### 1.3 Scenario completeness

| Scenario | Verdict | Evidence |
|---|---|---|
| Positive flows | PASS | Pass-after run: main It, It (a) and It (b) green with every redirect of the 16 corrected names bounded at a csproj-declared version and every aliased Reference present (evidence/regression-testing/binding-redirect-gate-pass-after.md, 16/16; final P4-T3 run 153/153) |
| Negative flows | PASS | Fail-before run: the main It lists exactly the 15 stale pairs, It (a) 152 inconsistent records, It (b) an empty carrier set (binding-redirect-gate-fail-before.md); negative control after the sweep alone: the count assertion passes and the unverifiable assertion names only netstandard and System.Linq.AsyncEnumerable, It (a) exactly the 15 System.Linq.AsyncEnumerable records (binding-redirect-gate-fail-before-unverifiable.md) |
| Edge cases | PASS | It (a) asserts non-vacuity twice (record count greater than zero; every one of the 16 names observed) and pins the version as well as the range, so a config whose range excludes its own newVersion or whose newVersion no csproj declares fails; It (b) asserts the exact carrier set, so a sixth project or a dropped alias fails; the in-memory fixture Describe blocks (lines 100-242) are unchanged and still pin the detector's contract |
| Error handling | PASS | Parser errors propagate from ConvertFrom-AppConfigText and ConvertTo-ReferenceVersionMap (module unchanged); the tests add no catch |
| Concurrency | PASS | No concurrent construct exists in these file-reading Pester tests; no shared mutable state introduced |
| State transitions | PASS | The three recorded gate states (red with 15 pairs; red with the install half only; green) are the transitions the Bugfix Workflow requires and each was observed |

### 1.4 Arrange-Act-Assert

PASS. The main It keeps its Arrange / Act / Assert structure (lines 277-317); It (a) carries Arrange (320), Act (341) and Assert (360) comments; It (b) carries Arrange (370), Act (378) and Assert (392); every Should carries a `-Because` string.

### 1.5 External dependencies and temporary files

PASS. The tests read tracked repository files through ReadAllText and write nothing (file header comment, lines 8-9); no $TestDrive, Set-Content, New-TemporaryFile, network or process call was added (reviewer read of the three hunks; the executor's P1-T5 verification agrees). C# tests were not changed.

### 1.6 Test file location

PASS. tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 mirrors scripts/dependencies/BindingRedirectVerification.psm1 (the tests/ tree layout the rules file requires); no new test file was added and none was colocated with production source.

### 1.7 Determinism infrastructure

PASS. No clock, randomness, sleep, retry or timing construct was added; the tests depend only on tracked file content.

## 2. General Code Change Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| Before making changes (plan, spec) | PASS | spec.md (418 lines, version 1.1 with five planner amendments and a 15-item scope amendment log) and plan.2026-10-02T22-16.md (revision 1.6, 107 of 108 tasks checked; the unchecked task is P5-T17, the AC17 check-off) exist; six preflight rounds and two clearance records under evidence/other/ |
| Bugfix workflow step 1 (failing regression test first) | PASS | The literal edits to the main It were made first (P1-T1, P1-T2) and the gate observed red before any config, manifest or project edit with the 15 pairs listed verbatim and a precondition porcelain over the three file kinds empty (binding-redirect-gate-fail-before.md); the two new It blocks were red in the same run |
| Bugfix workflow step 2 (minimal targeted fix) | PASS | Config edits are attribute-value changes and whole-block deletions only (per-file numstat 9/9 to 14/18, every file w/crlf with BOM 239,187,191, p4-t12-footprint); five packages.config at 1/0; five csproj at 9/0 (UtilitiesCS 10/0 with the Compile Include); .cs edits limited to six deleted directives, one `partial` and a verbatim move; CLAUDE.md 1/1 |
| Bugfix workflow step 3 (verify locally, toolchain in order) | PASS | PowerShell format, analyze, test and C# format, analyze, type-check, test each closed on iteration 1 with no file rewritten between steps (evidence/qa-gates/p4-t11-toolchain-loop-closure.2026-10-06T18-32.md; csharpier scoped format hash sets identical; PoshQC format hash sets identical) |
| Design principles (simplicity, reusability, extensibility, separation) | PASS | The aliased Reference is the smallest install that deploys the assembly without putting a second System.Linq.AsyncEnumerable type in scope (research 2); the range guard and alias guard reuse the file's existing enumeration and parser; the split moves one self-contained region into a partial with the base list left on the original declaration |
| Classes, functions, APIs | PASS | No public API changed; CategoryClassifierGroup keeps its 36 member declarations (35 distinct texts) across two partial declarations with identical accessibility (evidence/other/category-classifier-group-split-census.md) |
| Error handling | PASS | No catch added; no production statement changed |
| Logging | PASS | No logging changed; the moved ConditionLog and GetOlItemString members keep their logger.Debug calls verbatim |
| File size limit (500 lines) | PASS | Reviewer CR-anchored line counts: BindingRedirectVerification.Tests.ps1 402, CategoryClassifierGroup.cs 442, CategoryClassifierGroup.ConditionalEngine.cs 106 (each equal to the executor's LINECOUNT and CRCOUNT); the other four .cs files lost one line each (305, 269, 355, 187 per graph-usings-grep-pass-after.md) |
| Naming | PASS | New file named by concern (ConditionalEngine) beside the original; test variables descriptive (correctedName, inconsistent, missingName, withoutAlias, carrier) |
| Public APIs and compatibility | PASS | Every correction widens the set of requests that bind and narrows none; the System.Linq.Async and System.Interactive.Async References stay at 7.0.0.0 with lib\net48 HintPaths; no source file declares extern alias SystemLinqAsyncEnumerable (compile gates prove the alias is inert to name lookup) |
| Dependencies | PASS | One package added, System.Linq.AsyncEnumerable 10.0.12, the declared dependency of the already-installed System.Linq.Async 7.0.1 (nuspec floor 10.0.6); installed only in the five projects that install System.Linq.Async; the four net462 dependencies are already present (spec Constraints) |
| I/O boundaries | PASS | No I/O introduced |
| Documentation updates | PASS | CLAUDE.md premise corrected (verified against Directory.Build.props, which sets only RxUseUnsupportedPackagesConfig, and Directory.Build.targets, which only toggles SignManifests, SignAssembly and the manifest certificate for the TaskMaster project; Grep `<Nullable` over every csproj, props, targets, vbproj and fsproj in the worktree: 0); the csproj comment beside each aliased Reference states the CS0121 / CS0433 reason |

## 3. Language-Specific Code Change Policy Compliance

Languages in scope: C# (six .cs files, five csproj) and PowerShell (one test file). Configuration files (app.config, packages.config) are covered by the C# toolchain through the csproj changes and by the Pester gate.

| Item | Verdict | Evidence |
|---|---|---|
| C# formatting (csharpier via dotnet tool run) | PASS | evidence/qa-gates/csharpier-check.md: scoped format of the six files rewrote none (hash sets identical), repo-wide check `Checked 1638 files`, CSHARPIER-EXIT 0, baseline count 1637 plus one |
| C# linting (analyzer rebuild, /t:Rebuild, EnableNETAnalyzers, EnforceCodeStyleInBuild) | PASS | evidence/qa-gates/msbuild-analyzers.md: MSBUILD_EXIT_CODE 0, ERRORS 0, WARNINGS 0 (baseline 0), SKIP_CORECOMPILE_LINES 0, USING_DIAG_LINES 0, UtilitiesCS csc echoed twice and the new file named twice in the log |
| C# type checking (TreatWarningsAsErrors rebuild, no /p:Nullable=enable) | PASS | evidence/qa-gates/msbuild-treatwarningsaserrors.md: exit 0, 0 errors, 0 warnings, SKIP_CORECOMPILE_LINES 0; command text matches CLAUDE.md character for character |
| C# nullable annotations | PASS | The new partial file carries `#nullable enable` on line 1 (after the BOM), as do the five Part F files; no directive added or removed elsewhere |
| C# alias compile proof | PASS | CS0121_LINES 0 and CS0433_LINES 0 in both rebuild logs; MSB3277_LINES 0; the Part C fallback was not triggered (evidence/qa-gates/p4-t7-part-c-fallback.2026-10-06T18-26.md) |
| C# deployment proof | PASS | System.Linq.AsyncEnumerable.dll present in bin\Debug of UtilitiesCS, QuickFiler, ToDoModel, TaskMaster and UtilitiesCS.Test after the Rebuild, absent in all five at the P0-T18 negative control (evidence/qa-gates/system-linq-asyncenumerable-bin-presence.md) |
| C# XML docs and comments | PASS | No public member added; the csproj comment explains the alias; the moved region keeps its pre-existing comments verbatim (AC23 requires the verbatim move) |
| C# analyzer suppressions | PASS | None added (reads of the six .cs paths; no #pragma, SuppressMessage or ExcludeFromCodeCoverage line in any hunk) |
| PowerShell formatting (PoshQC format via MCP) | PASS | evidence/qa-gates/poshqc-format.md: ok true, 16 of 16 hashes identical before and after, FORMAT-REWROTE none, test file 402 lines / 402 CR |
| PowerShell analysis (PoshQC analyze via MCP) | PASS | evidence/qa-gates/poshqc-analyze.md: ok true, no diagnostic |
| PowerShell coding standards (StrictMode, approved verbs, no global state, under 500 lines) | PASS | Set-StrictMode -Version Latest at line 1; no new function; test-scoped variables only; 402 lines |
| PowerShell change budget | PASS | 0 production files and 1 test file (direct mode; spec Constraints) |

## 4. Language-Specific Unit Test Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| C# MSTest, Moq, FluentAssertions | PASS | No C# test changed; the existing suite (7361 tests, nine assemblies) passed 7361/7361 at baseline and after the change under the repository runsettings |
| C# repo-wide coverage floors | PASS | 85.35% lines (floor 80% per CLAUDE.md, 85% per rules), 79.74% branches (floor 75%) |
| C# new module/class/method >= 90% | PASS on the applicable limb | No new type or member exists; the new file holds moved members and reads 100% lines / 87.5% branches at its class node |
| C# no regression on changed lines | PASS | No changed line carries a sequence point; denominators identical; type aggregate identical |
| PowerShell Pester v5, Describe/It, one behaviour per It | PASS | Two It blocks added to the existing Describe; each asserts one property set with named failure output |
| PowerShell line coverage (>= 85% rules, 80% CI floor) | FAIL on the canonical bundled artifact (0 covered of 9294; instruments no scripts/ file; pre-existing instrument defect, non-blocking); the production module is unchanged and the scripts/dependencies figure comes from the CI Pester job |
| PowerShell changed-line regression | PASS by vacuity (no production PowerShell line changed) |
| Prohibited behaviors (sleeps, retries, timing hacks, weakened assertions) | PASS | The main It's assertions were strengthened (count form with a diagnostic `-Because`, an added `-Because` on the unverifiable assertion), not weakened; the examined-count guard and the Fizzler/Unsafe exclusion are unchanged (lines 313-314, 317); no sleep or retry in the three hunks |

## 5. Test Coverage Detail

| File | Change type | Coverage observation | Disposition |
|---|---|---|---|
| UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs | Modified production (+1 / -98: two `using` deletions, `partial`, 93-line region and two blank lines removed) | Class node 72.01% lines (229/318) / 77.59% branches (45/58) at baseline; 66.91% (180/269) / 70.59% (24/34) after; the difference equals the moved region's 49/49 lines and 21/24 branches; no changed line has a sequence point | PASS on the no-regression limb; per-file floor FAIL pre-existing (CR-3) |
| UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.ConditionalEngine.cs | Added (106 lines; moved region) | Class node line-rate 1 (49/49), branch-rate 0.875 (21/24), complexity 26 (baseline type complexity 70 = 44 + 26) | PASS |
| UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs | Modified production (-1 `using`) | 91.74% / 100% at both stages | PASS |
| UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs | Modified production (-1 `using`) | 68.97% / 54.17% at both stages | PASS on the no-regression limb; per-file floor FAIL pre-existing (CR-3) |
| UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs | Modified production (-1 `using`) | 91.80% / 90% at both stages | PASS |
| UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs | Modified production (-1 `using`) | 91.41% / 70% at both stages | PASS on the no-regression limb; branch floor FAIL pre-existing (CR-3) |
| tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 | Modified test (+85 / -19) | Outside the denominator by policy; 16/16 Passed after, 3 of 16 Failed before as predicted | Not measured |
| 15 app.config, 5 packages.config, 5 csproj, CLAUDE.md | Configuration and documentation | Not source; proven by the Pester gate (configs, csproj Aliases), the two Rebuilds (csproj, packages) and the deployment probe | Not measured |

Package-level projection after the change: UtilitiesCS LINE 4603 missed / 38905 covered, BRANCH 1861 / 9432; QuickFiler LINE 2294 / 10460, BRANCH 699 / 2518; TaskMaster LINE 802 / 2477, BRANCH 211 / 519; other packages identical to baseline (evidence/qa-gates/mstest-coverage-projection.md).

## 6. Test Execution Metrics

| Run | Scope | Total | Passed | Failed | Source |
|---|---|---|---|---|---|
| PowerShell baseline (P0-T10) | tests/scripts/dependencies, nine suites | 151 | 151 | 0 | evidence/baseline/poshqc-test-baseline.2026-10-03T10-48.md |
| Gate fail-before (P1-T6, after the test edits, before any config edit) | nine suites | 153 | 150 | 3 (expected: main It with 15 pairs, It (a), It (b)) | evidence/regression-testing/binding-redirect-gate-fail-before.md |
| Negative control (P2-T17, after the sweep and ADAL deletion, before the install) | nine suites | 153 | 150 | 3 (expected: unverifiable assertion naming System.Linq.AsyncEnumerable, It (a) with 15 records, It (b)) | evidence/regression-testing/binding-redirect-gate-fail-before-unverifiable.md |
| Gate pass-after (P3-T14) | nine suites | 153 | 153 | 0 | evidence/regression-testing/binding-redirect-gate-pass-after.md |
| PowerShell final (P4-T3) | nine suites | 153 | 153 | 0 | evidence/qa-gates/poshqc-test.md |
| C# baseline (P0-T19, DIRECT route) | nine test assemblies | 7361 | 7361 | 0 | evidence/baseline/mstest-coverage-baseline.md |
| C# final (P4-T9, DIRECT route) | nine test assemblies | 7361 | 7361 | 0 | evidence/qa-gates/mstest-test-results-summary.md |

Figures compared: the dependencies suite grew by exactly the two added It blocks (151 to 153; the changed file 14 to 16); the C# total is unchanged (no C# test changed); error, timeout, aborted and notExecuted each 0 at both C# stages; SEQUENCE_FILES 0; FAILED-SET empty.

## 7. Code Quality Checks

| Check | Command or method | Result | Verdict |
|---|---|---|---|
| Confidentiality masking scan | Grep over the feature folder, case-insensitive, drive-letter and /c/Users path patterns | 0 hits (agrees with HOST-PATH-RESIDUALS 0 in p4-t12-footprint and p5-t25-commit-c) | PASS |
| Raw document scan | Glob over the feature folder for non-Markdown files; diff file list for .xml, .trx, .coverage | 0 files; none in the diff | PASS |
| Suppression scan (added lines) | Read of the six .cs paths and the five csproj hunks | No new #pragma, SuppressMessage, ExcludeFromCodeCoverage or analyzer suppression | PASS |
| Workflow change scan | Diff file list | No .github/, scripts/ or runsettings path changed; csproj changes are one Reference element each plus one Compile Include | PASS |
| Post-state redirect census | Grep over */app.config | 0 Microsoft.IdentityModel.Clients.ActiveDirectory identities; 15 System.Linq.AsyncEnumerable blocks all at 0.0.0.0-10.0.0.12 / 10.0.0.12 (SVGControl and SVGControl.Test carry none); TaskMaster/app.config netstandard block at lines 38-39 unchanged (0.0.0.0-2.1.0.0 / 2.0.0.0) | PASS |
| Directive census | Grep `^using Microsoft\.Graph[.;]` type cs over the worktree | 0 hits (6 in 5 files at base per P0-T20) | PASS |
| CLAUDE.md census | Grep over CLAUDE.md line 211 | stale clause 0; "neither root build file sets one" 1; the 195-errors sentence, "CI omits it deliberately" and the closing sentence each 1 | PASS |
| Line-ending check | Grep `\r$` counts on the new partial file, the original file and the test file | 106, 442 and 402, each equal to the file's line count (CRLF throughout) | PASS |
| Tonality scan | Read of spec.md, issue.md, the plan's revision log, the runbook and the committed evidence | Neutral, factual wording; no humor, hyperbole or metaphor | PASS |

## 8. Gaps and Exceptions

- B-1 (Blocking, autonomous): AC17's Azure.Core clause asserts that the traced request Azure.Core 1.50.0.0 from Microsoft.Kiota.Authentication.Azure 2.1.2 redirects to an assembly that exists in the add-in's output directory. Three observations agree that the requester is not deployed there: the executor's Test-Path probes (Azure.Core.dll and Microsoft.Kiota.Authentication.Azure.dll absent from TaskMaster\bin\Debug, present only in UtilitiesCS\bin\Debug; evidence/qa-gates/p5-t17-ac17-checkoff.2026-10-06T18-36.md); the caller-reported System.Reflection.Metadata read of UtilitiesCS.dll, whose AssemblyReferences list contains no Microsoft.Graph, Microsoft.Kiota or Azure entry; and this reviewer's Grep over UtilitiesCS/**/*.cs and TaskMaster/**/*.{cs,csproj} for Microsoft.Graph, Azure., Kiota, GraphServiceClient and TokenCredential, which returns zero occurrences (before Part F the only occurrences were the six `using` directives, which bind nothing and emit no assembly reference). The compiler emits an assembly reference only for assemblies whose types are used, and MSBuild copies a ProjectReference's dependencies from the referenced assembly's metadata, so the Graph, Kiota and Azure family is never copied beside TaskMaster.dll and no Azure.Core request arises in the add-in process. The premise entered the spec through research 1 section 5.2 ("deployed beside TaskMaster.dll") and the Proposed Fix trace step 1. The redirect value itself is correct in every config and the detector invariant holds; the System.Linq.AsyncEnumerable half of AC17 is observed. Remedy: a spec amendment rewording AC17's Azure.Core clause to the observable facts (redirect value and range in TaskMaster/app.config and in every test-host config, the requester deployed only to UtilitiesCS and test outputs, and the add-in process issuing no such request), recorded in the Scope Amendment Log, then re-verification and check-off. Recorded in remediation-inputs.2026-10-06T19-30.md.
- B-2 (Blocking, human_decision_required): AC18 requires the maintainer to run runbooks/verify-designer-and-addin-load.runbook.md (Visual Studio designer load of PictureBoxSVG and an Outlook add-in start with a log inspection) and to write evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md. No designer or add-in observation was made (evidence/other/p5-t18-ac18-pending-manual.2026-10-06T18-43.md). No agent route exists. Recorded in remediation-inputs.2026-10-06T19-30.md.
- Canonical C# coverage artifact path absent: committed projections and the local raw documents used, per the standing ruling (observation O-2, recurring).
- Canonical PowerShell coverage artifact reads zero and instruments no scripts/ file (FAIL row, non-blocking; recurring P-4).
- PR context artifact pair absent in the worktree: scope verified from three agreeing sources instead.
- Plan-text nit (CR-1, non-blocking): P0-T20's Grep `ConditionalEngine` over UtilitiesCS.csproj expected 0 but matches the pre-existing Interfaces\IGlobals\IConditionalEngine.cs Compile item (count 1 at base); the fact it stands for was verified by the narrower Grep. No gate consumed the literal count.
- Caller item 4 corrected (observation O-1): the plan quotes every wildcard pathspec (every `-- '*.cs'`, `'*.md'`, `'*.ps1'` occurrence single-quoted; zero unquoted `-- *.` forms in the plan). The unquoted forms appear only in executor-issued commands (P4-T10 and P4-T11 Command fields; one discarded P4-T12 `*.md` capture), and each result was corroborated by a quoted re-capture in P4-T12 and by the commit C porcelain. No plan-text remedy is owed.

## 9. Summary of Changes

- Part A: 137 bindingRedirect entries for the 15 pair names corrected in 14 app.config files to oldVersion "0.0.0.0-<corrected>" / newVersion <corrected> (Azure.Core 1.63.0.0; Microsoft.Bcl.Memory, Microsoft.Bcl.Numerics, Microsoft.Extensions.Diagnostics.Abstractions 10.0.0.12; Microsoft.Identity.Client and .Extensions.Msal 4.90.1.0; the seven Microsoft.IdentityModel assemblies and System.IdentityModel.Tokens.Jwt 8.23.0.0; System.ClientModel 1.16.0.0).
- Part B: 13 Microsoft.IdentityModel.Clients.ActiveDirectory dependentAssembly blocks deleted (UtilitiesCS/app.config joins the write set).
- Part C: System.Linq.AsyncEnumerable 10.0.12 added to five packages.config files; an aliased Reference (Version 10.0.0.12, lib\net462 HintPath, Aliases SystemLinqAsyncEnumerable, explanatory comment) added to five csproj files; 15 System.Linq.AsyncEnumerable redirects set to 0.0.0.0-10.0.0.12 / 10.0.0.12; the DLL deployed to the five bin\Debug folders.
- Part D: the main It's literals emptied to `$expectedDebt = @()` and `@('netstandard')`, the count assertion with a diagnostic `-Because`, a `-Because` on the unverifiable assertion; new It (a) range-and-version guard over the 16 corrected names with non-vacuity assertions; new It (b) alias guard over the raw csproj text with the exact five-project carrier set.
- Part E: netstandard block untouched.
- Part F: six `using Microsoft.Graph.*` directives deleted from five UtilitiesCS files.
- Part G: CLAUDE.md line 211 first sentence reworded to name Directory.Build.props and Directory.Build.targets and what each sets; conclusion and historical sentences unchanged.
- Part H: CategoryClassifierGroup made partial; the 93-line `#region IConditionalEngine Implementation` block moved verbatim into CategoryClassifierGroup.ConditionalEngine.cs (106 lines, `#nullable enable`, CRLF, BOM); Compile Include added after the original item; original 539 to 442 lines.
- Evidence: 100-plus Markdown files under the feature folder (baseline, regression-testing, qa-gates, other, issue-updates); no raw document committed.

## 10. Compliance Verdict

REMEDIATION_REQUIRED. Every policy area evaluates PASS over the full branch diff except the per-file and canonical-artifact coverage rows recorded above, all of which are pre-existing and non-blocking. Two blocking findings remain: B-1 (AC17, class autonomous; a spec amendment under the in-item precedent of orchestrator-ruled criterion amendments, Planner Amendments 4 and 5, then re-verification and check-off) and B-2 (AC18, class human_decision_required; the maintainer's runbook run). Because one blocking finding is autonomous the verdict is REMEDIATION_REQUIRED; once B-1 is closed the item halts on B-2 until the maintainer records the designer-load evidence.

## Appendix A: Test Inventory

| Suite or class | Test | Status after change | AC |
|---|---|---|---|
| Repository binding redirects (issue 953) | reports exactly the recorded known-debt set and unverifiable set over every app.config against every csproj Reference (literals emptied, count assertion) | Failed before (15 pairs), Failed after the sweep alone (System.Linq.AsyncEnumerable unverifiable), Passed after the install | AC1, AC2, AC3, AC4, AC6, AC9, AC15 |
| Repository binding redirects (issue 953) | bounds every corrected redirect range at its newVersion across the repository app.config files (new) | Failed before (152 records), Failed after the sweep alone (15 records), Passed after | AC5, AC4, AC9 |
| Repository binding redirects (issue 953) | carries an Aliases child on every System.Linq.AsyncEnumerable project Reference (new) | Failed before (empty carrier set), Passed after | AC10, AC7 |
| Repository binding redirects (issue 953) | Fizzler redirect It (lines 246-273) | Passed, unchanged | AC16 |
| Find-StaleBindingRedirect and ConvertTo-ReferenceVersionMap in-memory fixtures | 12 tests (lines 100-242) | Passed, unchanged | AC16 |
| Other dependencies suites | AnalyzerItemRepair 13, ConsistencyVerifier 14, DependabotConfig 17, PackageCompatibility 8, PackageGraph 32, ProjectConsistency 18, Repair-PackageManifestConsistency 31, RepositoryTreeConsistency 4 | Passed, counts equal to baseline | AC13 |
| C# nine test assemblies | 7361 tests | Passed, count equal to baseline | AC11, AC14, AC20, AC23 |

PowerShell test count: 151 at baseline, 153 after (two added, none removed). C# test count: 7361 at both stages.

## Appendix B: Toolchain Commands Reference

| Step | Command (as recorded in evidence/qa-gates/) | Exit | Iteration |
|---|---|---|---|
| PS1 | mcp__drm-copilot__run_poshqc_format workspace_root=<worktree> scan_folders=["scripts/dependencies","tests/scripts/dependencies"] (poshqc-format.md) | ok | 1 |
| PS2 | mcp__drm-copilot__run_poshqc_analyze, same folders (poshqc-analyze.md) | ok | 1 |
| PS3 | mcp__drm-copilot__run_poshqc_test workspace_root=<worktree> scan_folders=["tests/scripts/dependencies"] (poshqc-test.md; JUnit 153/0; the bundled PoshQC route's Pester document is the FAIL-row artifact above) | ok | 1 |
| 1 | dotnet tool run csharpier format <six .cs paths>; dotnet tool run csharpier check . (csharpier-check.md; Checked 1638 files) | 0 | 1 |
| 2 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (msbuild-analyzers.md) | 0 | 1 |
| 3 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (msbuild-treatwarningsaserrors.md) | 0 | 1 |
| 4 | dotnet-coverage collect --output coverage\final-973.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-973.config -- vstest.console.exe <nine test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\973\final" "/Logger:trx;LogFileName=final-973.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (DIRECT route per spec Planner Amendment 4; mstest-coverage-projection.md; PASS 7361/7361) | 0 | 1 |
| 4b | pwsh Test-Path probes of System.Linq.AsyncEnumerable.dll in five bin\Debug folders (system-linq-asyncenumerable-bin-presence.md; five True) | 0 | 1 |

Reviewer commands: none (Bash forbidden). All verification by Read, Grep and Glob against the item worktree, the caller-supplied diff file, the gitignored raw coverage documents and the worktree reflog.
