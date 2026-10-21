# Policy Audit — issue #678, carry the folder predictor to the item controller

- Timestamp: 2026-09-01T23-35
- Feature folder: `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/`
- Branch under review: `bug/quickfiler-carry-folder-predictor-to-item-controller-678`
- Head: `d1f51e3a99cc5a98f622663df27abac7c8043f11`
- Base: `807fb0bb6e5e49f43efa6b256b05960bf078ca19`
- Work mode: `minor-audit` (marker read from `issue.md:13`)
- Diff form used: three-dot, `git diff 807fb0bb...HEAD`

## Base resolution and scope, re-derived

`git merge-base 807fb0bb6e5e49f43efa6b256b05960bf078ca19 HEAD` returns
`807fb0bb6e5e49f43efa6b256b05960bf078ca19` exactly, so the three-dot diff is non-degenerate and the
supplied pin is correct. The audit scope is the full branch diff against that base, not the scope of
the approved plan.

Footprint re-derived by this reviewer from `git diff --numstat 807fb0bb...HEAD`:

| Prefix | Changed paths |
|---|---:|
| `QuickFiler/` | 16 |
| `QuickFiler.Test/` | 19 |
| `docs/features/active/2026-08-28-...-678/` | 43 |
| Any other prefix | 0 |

Changed file extensions across the whole diff: 33 `.cs`, 2 `.csproj`, 41 `.md`, 2 `.xml`. No `.ps1`,
`.psm1`, `.py`, `.ts` or `.tsx` file is touched. Nothing under `UtilitiesCS/`, `.claude/` or
`CLAUDE.md` is touched.

Branch history relative to the base contains six commits, not two: `2ed1a8c7`, `9504d290`,
`a02ff703` (merge), `fc6784ac` (merge), `8782db56` (production plus tests) and `d1f51e3a` (evidence).
The two non-merge commits that carry the delivered change are `8782db56` and `d1f51e3a`; the earlier
two carry the feature-folder preparation. `issue.md` is wholly new relative to the base ref, so the
verdict register's "22 insertions and 22 deletions" figure describes a within-branch diff, not the
diff against the base. That does not weaken the register's claim, which this reviewer re-checked
directly against the head text.

## Rejected Scope Narrowing

None. The caller prompt supplied the base SHA and the feature folder, both of which are legitimate
scope sources, and explicitly directed a full-branch audit. The plan file
`plan.2026-08-31T21-12.md` was named as the approved plan, not as a scope limiter, and was not
treated as one. No language with changed files was excluded from evaluation.

## PR context artifacts

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` are absent from this
worktree. The reviewer's write permissions for this task are confined to the feature folder, so the
artifacts were not regenerated. Scope and evidence were derived instead from the authoritative
sources: the resolved base SHA and the three-dot `git diff`, enumerated above. This substitution is
recorded as an assumption; it does not narrow scope, because the git diff is the broader of the two
sources.

## Evidence Location Compliance

The branch diff was scanned for files written under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/` or `artifacts/coverage/`. **Zero matches.** All execution evidence is written
under `docs/features/active/2026-08-28-...-678/evidence/<kind>/` using the canonical kinds
`baseline/`, `qa-gates/`, `regression-testing/`, `issue-updates/` and `other/`. No
`EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose during this review.

`validate_evidence_locations.py` does not exist in this repository; the scan above was performed
directly against `git diff --name-only 807fb0bb...HEAD`.

Verdict: **PASS**.

## Coverage Verification

Languages with changed files in the branch diff: **C# only**. No other coverage language has a
changed file, so no other language row is required.

### Artifact availability

The canonical path `artifacts/csharp/coverage.xml` does not exist in this worktree. The measurement
substrate used instead is the post-processed Cobertura document at `coverage/coverage.cobertura.xml`,
written by the final MSTest pass (file mtime 2026-09-01 23:15 local), together with the committed
package-level summaries at `evidence/baseline/coverage-baseline.jacoco.xml` and
`evidence/qa-gates/coverage-post-change.jacoco.xml`. The Cobertura document's root attributes were
read directly by this reviewer and reproduce the executor's headline figures character for character.

### Repository-wide figures, independently read

| Side | line-rate | Line % | lines-covered | lines-valid | branch-rate | Branch % |
|---|---:|---:|---:|---:|---:|---:|
| Baseline (committed summary) | 0.853973 | 85.3973 | 55001 | 64406 | 0.794239 | 79.4239 |
| Post-change (read from the live Cobertura root element) | 0.854119 | 85.4119 | 55083 | 64491 | 0.794494 | 79.4494 |

Both floors are cleared on both readings: 85.4119 clears the 85 percent line floor of
`.claude/rules/general-unit-test.md` and the 80 percent floor of `CLAUDE.md`; 79.4494 clears the 75
percent branch floor.

### Language rows

| Language | Changed files | Repo-wide line | Repo-wide branch | Verdict |
|---|---:|---:|---:|---|
| C# repository-wide coverage | 33 `.cs`, 2 `.csproj` | 85.4119 % | 79.4494 % | **PASS** |
| C# new-file coverage, `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 1 | 28.00 % (28/100) | see note | **FAIL** — dispositioned non-blocking below |
| C# new-file coverage, `QuickFiler/Controllers/QfcCollectionController.CarrierLoad.cs` | 1 | no row emitted; the class-level exclusion attribute on the base part covers this part | — | **PASS** |
| C# modified-file coverage, all eleven remaining production paths | 11 | every changed-line rate 100 %, no per-file reduction unexplained by a deletion in that file | — | **PASS** |
| TypeScript coverage | 0 changed files | — | — | **PASS** (vacuous: the branch diff contains zero `.ts` and `.tsx` files, so no obligation arises) |
| Python coverage | 0 changed files | — | — | **PASS** (vacuous: the branch diff contains zero `.py` files, so no obligation arises) |
| PowerShell and Pester coverage | 0 changed files | — | — | **PASS** (vacuous: the branch diff contains zero `.ps1` and `.psm1` files, so no obligation arises) |

### Independent per-member and per-file reproduction

The reviewer parsed `coverage/coverage.cobertura.xml` directly, deduplicating line numbers across
`classes/class/lines/line` and `methods/method/lines/line` so method rows cannot double-count field
initialisers. Every figure below was produced by this reviewer, not copied from the executor:

| Unit | Covered / total | Rate |
|---|---:|---:|
| `QuickFiler\Controllers\QfcQueue.Enqueue.cs` (whole file) | 28 / 100 | 28.00 % |
| `QuickFiler\Controllers\QfcQueue.cs` (whole file, post-change) | 157 / 312 | 50.32 % |
| `QfcQueue.ItemControllerFactory` production default, lines 33-55 | 11 / 11 | 100.00 % |
| `QfcQueue.ResolveCarriedHandler`, lines 142-166 | 14 / 14 | 100.00 % |
| `QfcQueue.EnqueueAsync`, lines 67-139 | 0 / 46 | 0.00 % |
| `QfcQueue.LoadControllersViewersAsync`, lines 169-212 | 0 / 24 | 0.00 % |

All six match the executor's `evidence/qa-gates/coverage-delta.md` exactly.

### Disposition of the sub-floor new-file row

`QuickFiler/Controllers/QfcQueue.Enqueue.cs` is an added file at 28.00 percent line coverage, below
the 90 percent new-code threshold and below the 85 percent uniform floor. The row is recorded as
**FAIL** and dispositioned **non-blocking**, on four independently checked grounds:

1. **The shortfall is relocated pre-existing code, not new code.** Of the file's 100 measured lines,
   70 belong to `EnqueueAsync` and `LoadControllersViewersAsync`, which were moved out of
   `QfcQueue.cs` by this change. The 25 lines that are genuinely new (`ItemControllerFactory`
   production default and `ResolveCarriedHandler`) measure 25 / 25, that is 100 percent.
2. **The two relocated members were at zero at the base ref.** The reviewer verified this
   independently of the executor's arithmetic: `git grep` at `807fb0bb` finds every
   `EnqueueAsync` reference in `QuickFiler.Test/` to be a Moq setup or verification on the
   `IQfcQueue` interface (`QfcHomeControllerIterationTests.cs:133`, `:175`, `:282`), and no test
   constructs a concrete `QfcQueue` and calls `EnqueueAsync`. `LoadControllersViewersAsync` is
   private and has no reference of any kind in the test project. Neither member was reachable, so
   neither could have been covered. This is therefore not a regression on changed lines.
3. **No repository policy floor is breached.** Repository-wide line 85.4119 and branch 79.4494 both
   clear their floors. The combined `QfcQueue` surface improved from 158/381 (41.47 percent) at the
   base ref to 185/412 (44.90 percent) after the change.
4. **The bodies are host-bound.** `EnqueueAsync` clones a `TableLayoutPanel` through the UI-idle
   marshal and hooks an `EmailMoveMonitor`; `LoadControllersViewersAsync` dequeues a real
   `ItemViewer` through `AddAsync`. `.claude/rules/general-unit-test.md` prohibits a test that
   requires a live window, and `AC20` prohibits adding an exclusion attribute.

No remediation-inputs artifact is produced for this row. Under the repository's own criteria — no
regression on changed lines, an improved per-surface rate, and both repository-wide floors cleared —
the correct disposition is a recorded failing row rather than a remediation trigger.

## Toolchain gates (C# Code Change Policy, CUT3)

| # | Gate | Command | Executor result | Reviewer verification |
|---|---|---|---|---|
| 1 | Format verify | `dotnet tool run csharpier check .` | EXIT 0, `Checked 1574 files in 4846ms.` | **Re-run by this reviewer**: `Checked 1574 files in 4737ms.`, exit 0. Confirmed. |
| 2 | Analyzer build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT 0, 5 Warning(s), 0 Error(s), `CoreCompile:` ran 63 times | Not re-run (full solution rebuild). Command string matches the policy text verbatim, uses `/t:Rebuild` not `/t:Build`, and the recorded `CoreCompile` count of 63 proves the gate was not vacuous. Attested. |
| 3 | Nullable build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | EXIT 0, 5 Warning(s), 0 Error(s), `CoreCompile:` ran 71 times | Not re-run. Command matches the policy text verbatim, omits `/p:Nullable=enable` as the policy requires, and uses `/t:Rebuild`. `CoreCompile` ran 71 times, so the gate was not vacuous. Attested. |
| 4 | Test with coverage | `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .` | EXIT 0, Total 6946, Passed 6946, Failed 0, Skipped 0 | Not re-run. Corroborated: the Cobertura document the run produced exists on disk with a mtime inside the recorded execution window, and its root attributes reproduce the reported percentages exactly. Attested. |

The five commands of the final pass ran in sequence with no source edit between them, per
`evidence/qa-gates/final-toolchain-pass.md`. Two loop restarts are recorded, both triggered by a gate
finding and both followed by a full restart from step 1. Neither gate was reinterpreted or waived.

Verdict: **PASS**.

## Policy-by-policy findings

### CLAUDE.md and `.claude/rules/general-code-change.md`

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity, reusability, separation of concerns | PASS | The carry is a single added member threaded through existing seams. `ResolveCarriedHandler` and `ProjectPredeterminedFolder` are small `internal static` pure helpers, correctly separated from the host-bound code that calls them. |
| Fail fast, no silent error swallowing | PASS | No new catch block. The adoption path at `QfcItemController.FolderHandling.cs:68-77` adds no exception handling and leaves the existing handlers intact. |
| Logging via the project pattern | PASS | The new adoption log at `:71-75` uses `logger.Debug` and mirrors the shape of the two existing `Probability debug` lines in the same method. |
| Comment why, not what | PASS | Every non-obvious decision carries an in-code rationale: why the enqueue parameter is required rather than optional (`IQfcQueue.cs:34-40`), why the projection is duplicated (`QfcItemController.FolderHandling.cs:215-221`), why matching is by `EntryID` (`QfcQueue.Enqueue.cs:39-46`), why two members were relocated (`QfcQueue.Enqueue.cs:14-22`, `QfcCollectionController.CarrierLoad.cs:9-21`). |
| 500-line file limit | PASS for this change; pre-existing overage remains | No changed file crossed the limit. Three files remain over it and all three are smaller than at the base ref: `QuickFiler/Controllers/QfcCollectionController.cs` 2446 -> 2336, `QuickFiler/Controllers/QfcQueue.cs` 610 -> 505, `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` 827 -> 792. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` moved 499 -> 500, which is at the cap and does not exceed it. Recorded as finding NB-6. |
| Public API stability | PASS with a documented breaking change | `IQfcQueue.EnqueueAsync` gained a required third parameter. The rationale is recorded in the interface doc comment: an optional parameter cannot be named in a Moq expression tree (CS0854). The only production call site, `QfcHomeController.Iteration.cs:35`, is updated. This is a repository-internal interface with no external consumer. |
| No new dependency | PASS | No package reference added; both `.csproj` edits are `<Compile Include=...>` entries for the new partial parts. |
| I/O boundaries | PASS | The two new helpers are pure and testable without COM. |

### `.claude/rules/general-unit-test.md` and `CLAUDE.md` UT sections

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence, isolation, determinism | PASS | Every new test constructs its own doubles. No shared mutable state, no clock read, no sleep, no retry. |
| No temporary files | PASS | Reviewer grep of the changed test files finds no `Path.GetTempFileName`, `Path.GetTempPath` or `File.Create`. |
| No live external dependency | PASS | `MailItem` is always a Moq double. The one place a concrete `QfcQueue` is built, `QfcQueuePurePathsTests.NewQueue`, passes a null home controller and mocked globals. |
| Arrange-Act-Assert with documented intent | PASS | Every added test carries an XML summary naming its criterion, and explicit `// Arrange`, `// Act`, `// Assert` markers. |
| Coverage exclusion policy | See note | No exclusion attribute was added or removed anywhere in the diff; the reviewer confirmed a zero net change on that token across the three-dot diff. The standing conflict between the ratified host-bound exemption in `CLAUDE.md` and the no-exclusion rule in `.claude/rules/general-unit-test.md` is pre-existing and is not created, widened or relied upon by this change. |
| Test files in a mirroring `tests/` tree | Pre-existing repository convention | This repository colocates C# tests in sibling `*.Test` projects rather than a `tests/` tree. The change follows the established convention. Not a defect introduced here. |

### `.claude/rules/quality-tiers.md`

Uniform thresholds apply: line >= 85 percent, branch >= 75 percent. Both are met repository-wide.
The tier-dependent gates (property-test density, mutation score, golden tests, contract tests) have
no established harness in this repository and none is required by the acceptance criteria of a
`minor-audit` bug fix.

### C# Code Change Policy and C# Unit Test Policy

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | Every added test method carries `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. No xUnit or NUnit reference. |
| Moq for mocking | PASS | All doubles are `Mock<T>`, including the delegate-typed predictor factory seam. |
| FluentAssertions for assertions | PASS | All assertions use `.Should()`. MSTest `Assert` is not used in the added code. |
| Strong contracts, explicit types at boundaries | PASS | The carried member is declared as the narrow `IFolderSearchHandler` seam, not the concrete `FolderPredictor`, matching AC1. |
| Nullable discipline | PASS | The new member is documented as nullable with the reason stated (`QfcHighConfidencePreFilter.cs:143-147`); the nullable build reports zero `CS86` diagnostics. |
| Narrow suppression with rationale | PASS | The single `#pragma warning disable CS0618` in `QfcQueue.Enqueue.cs:169` is relocated verbatim from `QfcQueue.cs` with its original justification comment intact. It is not new. |

### `.claude/rules/tonality.md`

PASS. The evidence artifacts and in-code comments are factual and measured. The coverage artifact
states its own limitation explicitly ("the baseline per-line hit map was not retained") rather than
overstating what it proves, which is the behaviour the rule asks for.

## Findings summary

- Blocking: **0**
- Non-blocking: **8** (NB-1 through NB-8, enumerated in `code-review.2026-09-01T23-35.md`)

## Overall policy verdict

**PASS.** No blocking policy violation was found. One acceptance criterion, AC20, fails one of its
four clauses; that failure is recorded as a failing coverage row above and dispositioned non-blocking
against repository policy floors, all of which are met.
