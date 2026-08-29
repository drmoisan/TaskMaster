# Preflight Round 2 — Issue #440

Timestamp: 2026-08-29T02-20
Reviewer: atomic-executor under `DIRECTIVE: PREFLIGHT VALIDATION ONLY`
Plan under review: `plan.2026-08-29T00-22.md` in this feature folder
Signal: `PREFLIGHT: REVISIONS REQUIRED`
Convergence: `CONVERGENCE: NO FURTHER ROUNDS EXPECTED`

The reviewer executed no plan task and modified no file. All 55 tasks remain unchecked.

## Round-1 closure

16 of the 17 round-1 defects are confirmed genuinely closed in the plan text: 1, 3, 4 (all three sub-cases), 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17. Defect 2 is closed in form but its prescribed literal is wrong; see defect A.

The reviewer re-derived and confirmed correct: the 5-line comment budget arithmetic (248 - 1 - 4 + 5 = 248); the at-least-4 coverage-span floor against the nine statement points in `LeftArrow()`; the three planner judgment calls the orchestrator asked it to scrutinize; the P3-T3 filter reaching all four router classes; scope containment at exactly three backticked source files; and tonality.

## Defects requiring revision

### A. P4-T3 and P4-T4 assert an unreliable log literal (blocking)

Both tasks require at least one occurrence of the literal `(Rebuild target)` as the positive half of the AC-14 non-vacuity proof.

**The reviewer's finding is correct in substance and wrong in its stated reason. The orchestrator verified this independently and the corrected reason is authoritative.**

The reviewer reported that MSBuild never emits `(Rebuild target)` and that a search returns zero matches. That is false. Measured against a real solution-scope normal-verbosity log committed in this repository, `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/regression-testing/p1-t5-expectfail-build.msbuild.txt`, the literal `(Rebuild target)` occurs 12 times. The reviewer's own probe used a project with no diagnostics, which is why it observed zero.

The real defect is narrower and still blocking. `(Rebuild target)` occurs only inside the terminal warning and error summary block, which MSBuild emits only when the build produced diagnostics. Observed at lines 11329 onward of that log, immediately after `Build FAILED.`. On a clean build with zero warnings and zero errors that block is absent and the count is 0, so an at-least-1 gate on it fails precisely when the build is cleanest.

`(Rebuild target(s))` is the correct literal because it appears in the per-project `Done Building Project` and ProjectStarted messages, which normal verbosity emits on every run irrespective of diagnostics. Observed at lines 11324 and 11325 of the same log.

Replacement — in P4-T3's acceptance, replace the sentence beginning "The artifact records two counts read from `coverage\logs\p4-t3-analyzer.msbuild.txt`" with:

> The artifact records two counts read from `coverage\logs\p4-t3-analyzer.msbuild.txt`, each counted with `Select-String -SimpleMatch`: the occurrences of the literal `Skipping target "CoreCompile"`, which must be 0, and the occurrences of the literal `(Rebuild target(s))`, which must be at least 1. The parenthesised `(s)` is part of the string MSBuild emits in its ProjectStarted and `Done Building Project` messages at normal verbosity, which appear on every run. The shorter spelling `(Rebuild target)` appears only inside the terminal warning and error summary block, which MSBuild emits only when the build produced diagnostics, so counting it would fail exactly when the build is clean. The second count is the positive half of the non-vacuity proof and is what fails if the log is empty or was never written (AC-14).

Apply the identical substitution in P4-T4, reading `coverage\logs\p4-t4-nullable.msbuild.txt`.

### B. Coverage output filenames escape `.csharpierignore` (blocking)

P0-T13 writes `coverage\coverage.cobertura.baseline440.xml` and P4-T5 writes `coverage\coverage.cobertura.final440.xml`. Verified by the orchestrator: `.csharpierignore` excludes coverage output by the glob `*.cobertura.xml`, which matches only a filename ending in `.cobertura.xml`, and it does not exclude the `coverage/` directory. Neither plan filename ends in `.cobertura.xml`.

Consequence at P4-T2: the tree carries two XML files the P0-T10 baseline count did not include, so the count-equality clause fails, and raw Cobertura output is reported as needing reformatting, which is neither exit code 0 nor the pre-existing-drift set. Both branches fail.

Replacement:
- P0-T13, first sentence: change `-CoverageOutput coverage\coverage.cobertura.baseline440.xml` to `-CoverageOutput coverage\baseline440.cobertura.xml`.
- P4-T5, first sentence: change `-CoverageOutput coverage\coverage.cobertura.final440.xml` to `-CoverageOutput coverage\final440.cobertura.xml`.
- Append to Global rule 8:

> Every Cobertura document this plan writes, including the raw copy-aside taken on the Global rule 7 throwing branch, is named so that its filename ends in `.cobertura.xml`. `.csharpierignore` excludes coverage output by the glob `*.cobertura.xml` and does not exclude the `coverage/` directory, while CSharpier processes `*.xml` per CLAUDE.md C#1.1. A coverage file named otherwise is read by `csharpier check .` at P4-T2, which both raises the checked-file count above the P0-T10 baseline and reports the file as needing reformatting, so P4-T2 could pass under neither of its two branches.

### C. No gate can observe a repository path outside the two owned roots (blocking)

All three P3-T5 spans are pathspec-scoped to `UtilitiesCS` and `UtilitiesCS.Test`. AC-4 requires a QuickFiler keyboard-handler path absent from the diff; AC-9 requires two QuickFiler router files absent; AC-12 claims the diff touches exactly three repository files. No command in the plan can list a QuickFiler path, so those citations return the same result whatever the executor does.

Replacement — in P3-T5's command sentence, after the third span, add:

> ; and in the same task run `git diff --name-only b56400ab663a85b6039139d4548f408821e957ce -- . ":(exclude)docs" ":(exclude).claude"` and `git status --porcelain -- QuickFiler QuickFiler.Test`, which are the only two spans in this plan that can observe a repository path outside the two owned roots

And append to P3-T5's acceptance:

> The fourth command's output must name exactly the same three paths as the first. That span is what makes AC-12's exactly-three-repository-files claim, AC-4's absence of the QuickFiler keyboard-handler file and AC-9's absence of the two QuickFiler router files decidable: the first three spans are pathspec-scoped to `UtilitiesCS` and `UtilitiesCS.Test` and can never list a path outside them, so without this span those three absences are asserted against a command that cannot report them. The fifth command's output must be empty, which is what catches a file created rather than modified under either QuickFiler root, since a name-listing diff is blind to an untracked path. The two exclusions on the fourth span are load-bearing and are the only ones permitted: `.claude/agent-memory/` is tracked and already dirty at BASE from other agents' work, and `docs` carries this feature folder. The fourth span is a `git diff` rather than a `git status` because the repository-root `.dotnet-sdk` directory that P0-T5 creates is untracked and is not covered by `.gitignore`, so an unscoped status form would report it and could never pass.

### D. AC-8's Moq clause — resolved by the orchestrator amending the criterion

The reviewer found that AC-8 required Moq, that the file's `ModelWithSuggestion()` helper constructs `BreadcrumbStateModel` directly with no injectable seam, and that neither new test would therefore use Moq. It proposed leaving AC-8 permanently unchecked as PARTIAL, and noted the alternative of routing a new test through a Moq-provided `IFolderHierarchyProvider`.

The orchestrator adopted neither. Both options are wrong for the same reason: the unconditional Moq clause was inherited from the validation notes in `issue.md`, which were written against the superseded broad scope in which this issue was expected to change the router-level child-expansion path. Under the narrowed scope the clause has no referent — there is no collaborator on the walk-to-root path to mock — so shipping it unmet would record a false gap, and satisfying it literally would mean injecting a dependency the code under test does not take, contrary to the isolation requirement in `.claude/rules/general-unit-test.md`.

The orchestrator amended AC-8 in `spec.md` on 2026-08-29 to condition the mocking clause on a collaborator seam being present, to require FluentAssertions unconditionally, and to record the provenance of the change in the criterion itself. The amendment also notes that the mocked `IFolderHierarchyProvider` seam is exercised at the router level by the corrected test in `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`.

Consequence for the plan: P5-T8 keeps its normal full check-off form and needs no PARTIAL branch. The planner re-reads the amended AC-8 text and confirms P5-T8's evaluation clauses match it, adding a clause that records the router-level Moq seam.

### E. AC-14's fourth command is discharged by a different tool than the criterion names (minor)

AC-14 names `vstest.console.exe ... /EnableCodeCoverage`. P4-T5 runs the repository wrapper, which collects through `dotnet-coverage`. Global rule 5 gives a sound reason, but P5-T14 checks off AC-14 without recording the substitution.

Replacement — append to P5-T14's task text:

> The check-off additionally states that AC-14's fourth command is discharged by the repository wrapper `Invoke-MSTestWithCoverage.ps1` rather than by a bare `vstest.console.exe ... /EnableCodeCoverage`, because the wrapper is what supplies `/InIsolation` and the `TestCategory!=LiveOutlook` exclusion (Global rule 5), and that it collects through `dotnet-coverage` rather than through `/EnableCodeCoverage`.

## The `New-Item` step the planner added under round-1 defect 2

The reviewer tested the stated ground and found it false: the MSBuild file logger creates a missing log directory and exits 0. The added `New-Item -ItemType Directory -Force -Path coverage\logs` step is therefore unnecessary. It is harmless, since `coverage/*` is gitignored and `.txt` is not a CSharpier-processed extension, and the plan text does not assert the false rationale, so no plan edit is required. The false rationale must not be carried into any evidence artifact.
