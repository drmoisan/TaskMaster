# Policy Audit — Issue #638 (EFC unguarded archive-root read)

- **Component:** `QuickFiler` / `QuickFiler.Test`
- **Feature folder:** `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638`
- **Branch:** `bug/efc-unguarded-archive-root-read-crashes-ui-thread-638`
- **Base / merge base:** `ecdb1c84ba8541ab67042985919cfed4df768c01` (re-derived with `git merge-base`; matches the caller-supplied ref)
- **Head:** `af1b36e2d93c6beeeb98bbe4998d752e1ebfd732`
- **Commits under review:** `f07b6299`, `254fd56d`, `0b063741`, `a8cb1499`, `af1b36e2`
- **Work mode:** `full-bug` (marker read from `issue.md:8`) — sole AC source is `spec.md`
- **Audit date:** 2026-08-29T13-06
- **Reviewer:** feature-review agent

Assumptions recorded because evidence was not directly obtainable:

1. The MCP policy-audit template asset could not be resolved: no `mcp__drm-copilot__*` tool is
   exposed to this agent session. The canonical major headings required by
   `.claude/skills/policy-audit-template-usage/SKILL.md` are reproduced verbatim below and the
   artifact is authored to that structure. The same limitation prevents running
   `mcp__drm-copilot__validate_orchestration_artifacts`.
2. The reviewing directive prohibits `gh`, so the existence of GitHub issues #696, #697 and #698 on
   the remote is asserted from the branch's own recorded evidence rather than from the GitHub API.

## Executive Summary

**Overall verdict: PASS. Blocking findings: 0.**

The change is a minimal, well-bounded bug fix. Three unguarded reads of
`Globals.Ol.ArchiveRootPath` inside `EmailFilerConfig` object initializers in
`QuickFiler/Controllers/EfcDataModel.cs` are routed through a new private
`TryGetArchiveRoot(out string)` helper whose `catch` is narrowed to `InvalidOperationException`.
`MoveToFolderAsync` degrades to `return false`; the two `Open*` methods report through a new
injectable `Action<string>` diagnostic seam and return. Eleven MSTest regression tests were added in
a new file registered in the legacy non-SDK test project.

Every gate mandated by `CLAUDE.md` § "C# Toolchain" is evidenced and independently corroborated:
CSharpier check exit 0 with zero `Was not formatted.` lines; both `msbuild /t:Rebuild` gates at
`0 Error(s)` with zero `Skipping target "CoreCompile"` occurrences; a full nine-assembly vstest run
with 6870 of 6870 passing and zero executed `LiveOutlook` tests. The SHA-256 hashes recorded after
the accepted toolchain pass match the two footprint files at `HEAD` byte-for-byte, so the gated tree
state is the reviewed tree state.

Repository-wide C# line coverage is 85.33 percent and branch coverage is 79.31 percent, both
independently re-read from `coverage/coverage.cobertura.xml` root attributes. Changed-line coverage
was independently recomputed from the same Cobertura file and reproduces the recorded 93.10 percent
exactly, including the identity of the two uncovered lines. One coverage row is recorded FAIL: the
modified production file `QuickFiler/Controllers/EfcDataModel.cs` sits at 66.20 percent line
coverage against the 85 percent modified-file floor. That row is dispositioned non-blocking on
evidence set out in section 5.

## Rejected Scope Narrowing

None detected. The caller directive anchors the audit to the full branch diff against
`ecdb1c84ba8541ab67042985919cfed4df768c01`, names every changed file category, and explicitly
requires coverage, toolchain and hygiene verification rather than excluding any of them. The
directive's statement that no `user-story.md` exists is a correct consequence of the `full-bug`
work mode and is not a narrowing of audit scope. No instruction was received that limits the audit
to a plan, task, phase, or file subset, or that marks any language as out of scope.

The audit scope used is the full two-dot-equivalent diff
`git diff ecdb1c84ba8541ab67042985919cfed4df768c01...HEAD`: 38 files, 3 of them source
(`QuickFiler/Controllers/EfcDataModel.cs`, `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`,
`QuickFiler.Test/QuickFiler.Test.csproj`), the remaining 35 feature-folder documents and evidence.

## Evidence Location Compliance

PASS. No file in the branch diff is written under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/` or `artifacts/coverage/`. All 33 evidence artifacts on the branch live under
`<FEATURE>/evidence/<kind>/` with `<kind>` drawn from the canonical set
(`baseline`, `qa-gates`, `regression-testing`, `remediation-baseline`, `other`), as required by
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.

`validate_evidence_locations.py` is not present in this repository, so the scan was performed
directly over `git diff --name-only` output against the merge base. `artifacts/csharp/coverage.xml`
exists on disk but is gitignored (`.gitignore:57`) and therefore contributes no branch-diff entry.

No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose: this agent wrote its three artifacts to
the feature folder root in the review worktree, as directed, and wrote no evidence elsewhere.

The branch also cured one earlier non-canonical path itself: the spec's Correction Log entry at
2026-08-29T10-05 records replacing a non-canonical coverage sub-directory reference with the
canonical `evidence/baseline/` and `evidence/qa-gates/` locations. No helper script
(`.ps1`, `.py`, `.sh`) exists anywhere under the feature folder's `evidence/` tree; verified by a
recursive extension search returning zero matches.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence — tests run in any order | PASS | Every test constructs its own `Mock<IOlObjects>`, `Mock<IApplicationGlobals>`, `ConcurrentDictionary` and `EfcDataModel`. No static or shared mutable state; no `[ClassInitialize]`/`[AssemblyInitialize]`. |
| Isolation — one unit of behavior per test | PASS | Each of the 11 tests exercises exactly one method-and-condition pair; names encode both. |
| Fast execution | PASS | No I/O, no COM, no waits. The whole 11-test filter run is a sub-second segment of the 44.9-second nine-assembly suite. |
| Determinism | PASS | No wall-clock assertion, no RNG, no ambient environment read. Grep for `Thread.Sleep`, `Task.Delay`, `Path.GetTemp`, `File.Create` over the new test file returns zero matches. |
| Readability and maintainability | PASS | Every test carries a Scenario/Expected-outcome XML summary and explicit `// Arrange`, `// Act`, `// Assert` markers. Shared arrangement is factored into six named private helpers. |
| Arrange-Act-Assert structure | PASS | All 11 tests. |
| Clear failure messages | PASS | FluentAssertions throughout (`Should().BeFalse()`, `Should().ContainSingle()`, `Should().NotContain(...)`, `Should().ThrowAsync<T>()`). |
| No external dependencies | PASS | Strict Moq seams over `IApplicationGlobals`, `IOlObjects` and `IFileSystemFolderPaths`. No network, database, filesystem, live Outlook or external process. |
| No temporary files | PASS | Zero filesystem writes in the new test file. |
| Scenario completeness (positive, negative, edge, error) | PASS | Positive: `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`. Negative: both `InvalidOperationException` throw conditions. Edge: two `MoveToFolderAsync` early-guard orderings plus two `Open*` OneDrive-missing paths. Error: `COMException` propagation. |
| Test file location | PASS (with recorded deviation) | Tests are placed at `QuickFiler.Test/Controllers/`, not in a `tests/` mirror tree. `.claude/rules/general-unit-test.md` § "Test File Location" mandates a `tests/` mirror; `spec.md` records decision D1 resolving the conflict in favour of `CLAUDE.md` § C#, which ranks above `.claude/rules/` per `.claude/skills/policy-compliance-order/SKILL.md`, and in favour of the General Code Change Policy § 7.1 "match existing style". Every C# test in this repository lives in a `<Project>.Test` sibling; the `tests/` tree holds only PowerShell Pester files. The deviation is pre-existing, repository-wide, and documented rather than silent. |
| Coverage thresholds | See section 5 | Repo-wide C# line coverage PASS; modified-file line coverage FAIL, dispositioned non-blocking. |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix Workflow — failing regression test first | PASS | `evidence/regression-testing/p3-t15-regression-fail-before.md`: exit 1, 11 tests, 5 failed, each failure message naming `InvalidOperationException`, run before the Phase 4 fix. `evidence/regression-testing/p5-t1-regression-pass-after.md`: exit 0, 11 of 11 pass. |
| Bugfix Workflow — minimal targeted fix | PASS | One production file, +65/-3 lines, one new private helper and one new internal seam. No opportunistic refactor. Deeper design problems were routed to follow-up issues rather than widening scope. |
| Bugfix Workflow — verify locally before review | PASS | Section 7 records the four-step toolchain, run twice with the second pass clean end to end. |
| Simplicity first | PASS | A `try`/`catch` extracted into a `bool`-returning helper with an `out` parameter — the simplest shape that lets a `try` guard a read that syntactically sits inside an object initializer. |
| Reusability | PASS | The single helper serves all three call sites; no copy-pasted `try`. |
| Extensibility | PASS | `UserDiagnosticAction` is a settable delegate seam, matching `EfcHomeController.MoveFailureMessageAction`. |
| Separation of concerns | PASS | The helper isolates the COM-backed read and its documented failure mode from the filing workflow. |
| Fail fast and explicitly | PASS | The catch is exactly `InvalidOperationException`. `COMException` propagates, pinned by `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates`. No broad `catch (Exception)` was introduced. |
| Logging via the project pattern | PASS | `logger.Warn(message, ex)` on the existing static log4net logger, mirroring the adjacent OneDrive guard's `logger.Warn`. No `Console.WriteLine`. |
| No silent error swallowing | PASS | Each guarded failure emits one `Warn` entry, and the `Open*` paths additionally surface a user-facing message. The pre-fix behavior — a hidden form, no message, and one log entry several frames away — is what the change removes. |
| File size <= 500 lines | PASS | `QuickFiler/Controllers/EfcDataModel.cs` is 485 lines at head (423 at the merge base), measured with `awk END{print NR}`. `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` is 389 lines. |
| Module cohesion | PASS | The helper and its message constant belong to the class that performs the reads. |
| Comment why, not what | PASS | The helper's XML doc explains why `InvalidOperationException` is absorbed and why other failures are not. The `TestableEfcDataModel` doc explains why the two-argument `ConversationResolver` constructor is required and why the five-argument one would break the single-read invariant. |
| No breaking public API change | PASS | `git diff` shows no signature line altered. `EfcDataModel` is `internal`; the new member is `internal`. |
| Approved dependencies only | PASS | No package reference added. `QuickFiler.Test.csproj` gains exactly one `<Compile Include>` line. |
| I/O boundaries | PASS | Domain logic is testable without network or filesystem; the 11 tests prove it. |
| Existing tests treated as spec | PASS | `evidence/other/p5-t3-untouched-tests.md` shows the union of `git diff --name-only` and `git status --porcelain -uall` over both test projects is exactly the new file plus the one-line `.csproj` edit. All six protected test files are unmodified; independently re-verified against the branch diff. |
| Docs updated | PASS | `spec.md` status set to Implemented, AC check-off recorded, Rollout & Follow-up carries the three follow-up issue numbers, and a Correction Log documents three amendments. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting via `dotnet tool run` | PASS | `evidence/qa-gates/p6-t2-csharpier-check.md`: `dotnet tool run csharpier check .`, exit 0, `Checked 1561 files in 4097ms.`, zero `Error <path> - Was not formatted.` lines. Baseline was 1560 files with zero unformatted. |
| `dotnet format` not used | PASS | No evidence artifact or command record references `dotnet format`. |
| Analyzer gate with `/t:Rebuild` | PASS | `evidence/qa-gates/p6-t3-msbuild-analyzers.md`: exit 0, `0 Error(s)`, `5 Warning(s)` (unchanged from the `[P0-T10]` baseline), zero occurrences of `Skipping target "CoreCompile"` in the tee'd log against 86 `CoreCompile` invocations. |
| Type-check gate with `/t:Rebuild`, no `/p:Nullable=enable` | PASS | `evidence/qa-gates/p6-t4-msbuild-nullable.md`: exit 0, `0 Error(s)`, `5 Warning(s)`, zero `Skipping target "CoreCompile"`. The recorded command contains neither `/p:Nullable=enable` nor `/t:Build`, matching `CLAUDE.md` § C#1.3 and `.github/workflows/ci.yml`. |
| Toolchain order and restart-on-change | PASS | `evidence/qa-gates/p6-t6-loop-closure.md`: the loop ran twice; pass 1's format step rewrote the new test file (line-ending normalization) and forced a restart; pass 2 was a fixpoint and steps 2-5 all ran after it, with non-decreasing timestamps. |
| Gated tree equals reviewed tree | PASS | SHA-256 hashes recorded immediately after the test step — `995BB645...B2A5E2A` for `EfcDataModel.cs` and `A20BCF32...987794EB` for the new test file — reproduce exactly against the working tree at `HEAD` (independently recomputed during this review). |
| Strong contracts, explicit APIs | PASS | `TryGetArchiveRoot(out string)` documents its contract, its absorbed exception and its non-absorbed exceptions in XML doc comments. |
| Null-safety | PASS | The helper assigns `archiveRoot = null` on the failure branch and every caller checks the `bool` before use, so no null flows into `EmailFilerConfig.OlAncestor`. Neither changed file opts into `#nullable enable`, consistent with the repository's per-file opt-in model. |
| Resource safety / async | PASS | No new disposable, no new `async` method, no `async void` added. |
| Public surface minimal | PASS | The seam is `internal`, the helper and the message constant are `private`. |
| Naming conventions | PASS | `PascalCase` members, `camelCase` locals, descriptive names. |
| XML documentation on non-obvious behavior | PASS | Both new members and the message constant carry XML doc comments. |
| No new suppressions | PASS | No `#pragma warning disable`, no `[SuppressMessage]`, no `[ExcludeFromCodeCoverage]` added. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]` / `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. No xUnit or NUnit reference. |
| Moq for mocking | PASS | `Mock<IOlObjects>`, `Mock<IApplicationGlobals>`, `Mock<IFileSystemFolderPaths>`, all `MockBehavior.Strict`. |
| FluentAssertions for assertions | PASS | Every assertion is FluentAssertions; `Moq.VerifyGet` is used for call-count pinning, which FluentAssertions does not express. |
| No new test dependency | PASS | `QuickFiler.Test.csproj` diff is one `<Compile Include>` line; no `PackageReference` or `Reference` added. |
| New test file registered in the legacy project | PASS | `<Compile Include="Controllers\EfcDataModelArchiveRootTests.cs" />` added at `QuickFiler.Test/QuickFiler.Test.csproj:116`. Confirmed effective, not merely present: 11 tests named `EfcDataModelArchiveRootTests` appear in the full-suite TRX. |
| No `[TestCategory("LiveOutlook")]` in the change | PASS | Grep over the new test file returns zero `TestCategory` occurrences. The suite run executed zero `LiveOutlook` tests. |
| Strict mocks fail loudly on unexpected reads | PASS | `MockBehavior.Strict` on all three seams, so an unconfigured member access throws rather than returning a default. |

## 5. Test Coverage Detail

Coverage source: `coverage/coverage.cobertura.xml` (post-processed by
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`), plus the canonical review artifact
`artifacts/csharp/coverage.xml`. Both were read directly during this review, not taken on trust.

Root attributes independently re-read: `line-rate="0.853335"`, `branch-rate="0.79311"`,
`lines-covered="54802"`, `lines-valid="64221"`, 9 `<package>` elements. The canonical JaCoCo-shaped
artifact carries `<counter type="LINE" missed="9419" covered="54802"/>`, which recomputes to
85.33 percent and agrees with the Cobertura root.

| Language | Artifact | Scope | Figure | Floor | Verdict |
|---|---|---|---|---|---|
| C# | `artifacts/csharp/coverage.xml` (present) | repo-wide line coverage | 85.33% | 85% | PASS |
| C# | `coverage/coverage.cobertura.xml` | repo-wide branch coverage | 79.31% | 75% | PASS |
| C# | `coverage/coverage.cobertura.xml` | changed-line coverage on `EfcDataModel.cs` | 93.10% (27 of 29) | 90% | PASS |
| C# | `coverage/coverage.cobertura.xml` | modified-file line coverage, `QuickFiler/Controllers/EfcDataModel.cs` | 66.20% (188 of 284) | 85% | FAIL, dispositioned non-blocking below |
| C# | branch-diff scan | new production source files added by this change | none exist; the one added file is a test file | 85% | PASS |
| TypeScript | branch-diff scan | zero changed files on this branch | no measurement required | 85% | PASS |
| Python | branch-diff scan | zero changed files on this branch | no measurement required | 85% | PASS |
| PowerShell | branch-diff scan | zero changed files on this branch | no measurement required | 85% | PASS |

C# is the only language with changed files in the branch diff. The diff contains three source files,
all `.cs` or `.csproj`, and 35 Markdown documents. No `.ts`, `.tsx`, `.py`, `.ps1` or `.psm1` file
appears anywhere in the diff, which is why the three non-C# rows above record no measurement
obligation rather than a waived one.

### Independent recomputation of the changed-line figure

The recorded 93.10 percent was reproduced from scratch during this review. Parsing
`coverage/coverage.cobertura.xml` for every `<class>` whose `filename` resolves to
`QuickFiler/Controllers/EfcDataModel.cs` yields exactly one merged class element carrying 284
`<line>` entries. Intersecting the 65 post-image line numbers named by the `+` hunks of
`git diff -U0` against the merge base with that line set leaves 29 executable lines, 27 of them with
`hits > 0`: **93.10 percent**, uncovered lines **366** and **390**. Every figure and both line
numbers match `evidence/qa-gates/p7-t2-coverage-changed-lines.md` exactly.

Lines 366 and 390 are `OlAncestor = olAncestor,` on the success branch of `OpenOlFolderAsync` and
`OpenFsFolderAsync`. Reaching them requires constructing a real `EmailFiler` against a live Outlook
folder, which `.claude/rules/general-unit-test.md` prohibits. The equivalent line on the move path,
339, is covered.

### AC17 remediation adjudication

The plan's `[P7-T3]` correctly refused to compare a `raw` merge-base figure against a
`koverage-processed` post-change figure: the raw file carried 14 packages and `lines-valid=82363`
including every `.Test` assembly, versus 9 packages and `lines-valid=64221` post-processed. That is
a denominator difference, not a change effect, and the resulting `+14.63` delta was an artefact.

The remediation is sound. `evidence/remediation-baseline/ac17-commensurable-baseline.md` records a
fresh merge-base run in a separate detached worktree at `ecdb1c84`, with `packages/` and
`.dotnet-sdk/` copied from the feature worktree so the analyzer set and SDK are identical, a
`/t:Rebuild` before the harness, exit 0, 6859 of 6859 passing, and post-processing reached. It
yields `line-rate=0.852636`, `branch-rate=0.792376`, `lines-covered=54735`, `lines-valid=64195`
across 9 packages. Four independent corroborations were checked:

1. **Mode equality.** Both figures are `koverage-processed`; the clause `[P7-T3]` could not satisfy
   is now satisfied on its own terms.
2. **Package-count equality.** 9 packages in both runs, so the assembly allowlist selected the same
   set and the denominators are constructed identically.
3. **Denominator arithmetic.** `64221 - 64195 = 26`, which equals the count of newly added
   executable lines in `EfcDataModel.cs` derived independently above (29 changed executable lines
   minus the 3 pre-existing lines that were only re-pointed at the local). The denominator grew by
   exactly what the change adds and by nothing else.
4. **Numerator arithmetic.** `54802 - 54735 = +67`. Of that, 26 are the new lines; the remaining 41
   are lines that were executable and uncovered at the merge base and are now reached by the 11 new
   tests through `EfcDataModel`'s guard and early-return paths. The sign and magnitude are consistent
   with 11 added tests that drive previously unexercised branches.

The `+0.07`-point movement on both line and branch rates is inside the run-to-run noise band this
repository has previously measured for C# repo-wide figures, and the remediation artifact says so
explicitly, claiming only "not lowered" rather than a measured improvement. That is the claim AC17
requires and the claim the evidence supports. The original `[P7-T3]` artifact was left unedited with
an appended, clearly delimited `RESOLVED` section, which conforms to the remediation-reconciliation
rule in `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. **AC17's closure is sound.**

### Disposition of the modified-file FAIL row

`QuickFiler/Controllers/EfcDataModel.cs` reads 66.20 percent line coverage (188 of 284), below the
85 percent modified-file floor. Recorded FAIL. Dispositioned **non-blocking**, on five grounds:

1. **The shortfall is pre-existing and the change improves it.** The change adds 26 executable lines,
   all covered. Baseline denominator was therefore 258 and baseline numerator 161 or 162 (161 if the
   pre-change line 289 was uncovered, 162 if covered), giving a merge-base figure of 62.40 to 62.79
   percent. Head is 66.20 percent, an improvement of 3.4 to 3.8 points. The exact baseline numerator
   cannot be pinned because the merge-base Cobertura file was produced in a detached worktree that
   was not retained; the bound above is derived arithmetically and holds either way.
2. **Zero regression on changed lines.** All 26 newly added executable lines are covered, and of the
   three pre-existing lines the change touched, one is covered and two (366, 390) sit on a
   COM-dependent success branch that no test could reach before or after.
3. **The residual uncovered mass is COM-bound and outside the defect.** The 96 uncovered lines fall
   in eight ranges: 181-185 and 189-221 (`FolderHelper` / `InitFolderHandlerAsync`, driven by
   `FolderPredictor`), 248-254 (`TryGetFirstInSelection`), 345-346, 362-371 and 386-395
   (`EmailFiler` construction and its `SortAsync` / `OpenOlFolderAsync` /
   `OpenFileSystemFolderAsync` calls), 406-419 (the `MAPIFolder` overload of `MoveToFolderAsync`)
   and 451-481 (`PackageItems`, `FindMatches`, `RefreshSuggestions`, which dereference
   `MailItem` and `FolderPredictor`). Every one of these requires a live Outlook object.
4. **Raising the file to 85 percent would violate the Bugfix Workflow.** `CLAUDE.md` § Bugfix
   Workflow step 2 requires changing only what is needed and directs deeper design problems to new
   issues. Covering the ranges above needs new injectable seams across `EmailFiler`, `FolderPredictor`
   and the `MAPIFolder` overload — a refactor several times the size of the fix.
5. **The governing acceptance criterion is change-scoped and it passes.** `spec.md` AC17 sets the
   blocking clause at >= 90 percent on changed lines with no regression, and demotes the repo-wide
   figure to record-and-report. Changed-line coverage is 93.10 percent and repo-wide is 85.33 percent,
   which clears both the 80 percent floor in `CLAUDE.md` § UT2 and the 85 percent floor in
   `.claude/rules/general-unit-test.md`.

No remediation-inputs artifact is produced, because no finding in this audit blocks.

## 6. Test Execution Metrics

| Metric | Value | Source |
|---|---|---|
| Test assemblies discovered | 9 | `evidence/qa-gates/p6-t5-vstest-coverage.md` |
| Total tests executed | 6870 | same |
| Passed | 6870 | same |
| Failed | 0 | same; corroborated per-namespace from the TRX (`QuickFiler.` = 0, `TaskMaster.` = 0, all others = 0) |
| Skipped | 0 | no `Skipped:` summary line emitted |
| `LiveOutlook` tests executed | 0 | TRX `TestDefinitions` intersected with the executed `testId` set |
| Baseline failure carve-outs consumed | 0 | `BASELINE_FAILURE_SET: none` in `evidence/baseline/p0-t12-direct-harness-baseline.md` |
| New tests added | 11 | all 11 present in the TRX |
| Merge-base test total | 6859 | `evidence/remediation-baseline/ac17-commensurable-baseline.md` |
| Arithmetic check | 6859 + 11 = 6870 | consistent |
| Fail-before run | exit 1; 11 tests, 6 passed, 5 failed, each naming `InvalidOperationException` | `evidence/regression-testing/p3-t15-regression-fail-before.md` |
| Pass-after run | exit 0; 11 of 11 passed | `evidence/regression-testing/p5-t1-regression-pass-after.md` |
| Test filter parity with CI | `/InIsolation` and `/TestCaseFilter:"TestCategory!=LiveOutlook"`, matching `.github/workflows/_mstest-coverage.yml:83` | `evidence/qa-gates/p6-t5-vstest-coverage.md` |

The assembly-discovery rule recorded in the evidence rejects `\.claude\` only in the
worktree-relative path, not the absolute path. That is the correct form: an absolute-path test would
match every candidate when the review worktree itself sits under a `.claude` directory and would
silently produce an empty assembly list that vstest reports as zero failures. Nine assemblies were
in fact discovered, so the filter did not over-reject.

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Format | `dotnet tool run csharpier check .` | exit 0, 1561 files checked, 0 unformatted | PASS |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0, `0 Error(s)`, `5 Warning(s)` (baseline-equal), 0 `Skipping target "CoreCompile"` | PASS |
| Type check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | exit 0, `0 Error(s)`, `5 Warning(s)` (baseline-equal), 0 `Skipping target "CoreCompile"` | PASS |
| Test | `vstest.console.exe <9 assemblies> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` | exit 0, 6870 of 6870 passed | PASS |
| Loop closure | SHA-256 of both footprint files before and after | equal; and equal to the working tree at `HEAD` | PASS |
| File-size cap | `awk END{print NR}` | 485 and 389, both <= 500 | PASS |
| Redaction discipline | source read plus `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress` | the constant names the rule only; no path, no mailbox address, no exception message interpolated | PASS |
| Host-identity hygiene of branch content | recursive case-insensitive grep for user-profile roots, the account name and worktree names over the feature folder and both source files | zero matches | PASS |
| Guard ordering | direct source read at `EfcDataModel.cs:311-330`, `:349-360`, `:374-384` | `MailInfo is null` first; OneDrive `SpecialFolders` second; archive-root guard third in `MoveToFolderAsync`, and after the OneDrive guard in both `Open*` methods | PASS |
| Ordering pinned from the untouched side | `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:217` | `probe.FileSystem.SpecialFoldersAccessCount.Should().Be(2)` still present and the file unmodified in the diff | PASS |
| Catch breadth | `EfcDataModel.cs:287` | `catch (InvalidOperationException ex)` only; no `catch (Exception)`, no `catch (COMException)` | PASS |

The 5 analyzer/compiler warnings are pre-existing and equal in count to the `[P0-T10]` and
`[P0-T11]` merge-base baselines, so the change introduces no new diagnostic.

## 8. Gaps and Exceptions

All items below are non-blocking. Each is recorded so it is not lost.

- **G1 — Modified-file line coverage below the 85 percent floor.** Recorded FAIL in section 5 with a
  five-part non-blocking disposition. Severity: Minor. Pre-existing debt, improved by this change.
- **G2 — `tests/` mirror-tree deviation.** `.claude/rules/general-unit-test.md` requires test files
  in a `tests/` mirror. This repository places all C# tests in `<Project>.Test` siblings. The
  deviation is repository-wide and pre-existing; `spec.md` decision D1 records the resolution and its
  authority chain. Severity: Informational. Not introduced by this change.
- **G3 — AC12's phrase "unmodified production code" is imprecise.** The fail-before run at
  `[P3-T15]` executed against production code that already carried the Phase-2 diagnostic seam
  declaration; only the Phase-4 guard was absent. This is structurally necessary — the tests assign
  `dataModel.UserDiagnosticAction`, so they cannot compile without the seam — and the seam is
  behaviourally inert until Phase 4 adds its three invocation sites, which the branch diff confirms.
  The five recorded failures are therefore genuine proof of the unguarded read. Severity:
  Informational, wording only.
- **G4 — Remote existence of issues #696, #697 and #698 was not queried.** The directive prohibits
  `gh`. Verification was limited to what the directive assigns: the three numbers are recorded in
  `spec.md` § Rollout & Follow-up (two places: the post-fix task list and the Links list), and the
  three sections of `evidence/other/p8-t2-followup-issue-dossier.md` correspond one-to-one to the
  three non-goals (a), (b) and (c) in `spec.md` § Scope & Non-Goals. Both hold. Severity:
  Informational.
- **G5 — The promotion records for #696-#698 are not on this branch.** The dossier's `RESOLVED`
  appendix states the promoted records are retained under `docs/features/potential/promoted/`; that
  directory in the review worktree contains no entry for these three, and the working tree is clean.
  The records were created in whichever worktree the orchestrator ran the promotion tools. This does
  not affect AC20, whose text requires the issue numbers to be recorded in the spec, which they are.
  Severity: Informational. Worth confirming before the feature folder is archived.
- **G6 — Spec-internal tension on the redaction invariant.** `spec.md` § Boundaries states that no
  user-visible message may interpolate the destination folder path, while § Error handling
  deliberately preserves the pre-existing `EfcHomeController` message "Cannot move to folderpath
  {selectedFolder}", which does interpolate it. That message is pre-existing, unmodified by this
  change, pinned by an untouched test, and carries an archive-relative stem rather than a mailbox
  address. AC4 is scoped to the new seam's diagnostic, which is clean. Severity: Informational.
- **G7 — Region placement.** `TryGetArchiveRoot` and `ArchiveRootUnavailableMessage` are declared
  inside the `#region Public Properties` block, although both are private and one is a method.
  Severity: Trivial. Detailed in the code review.

No policy document under `.claude/rules/` or `.github/instructions/` was modified by this change or
by this review. No secret or `.env` file was created.

## 9. Summary of Changes

| File | Change | Lines |
|---|---|---|
| `QuickFiler/Controllers/EfcDataModel.cs` | Added `internal Action<string> UserDiagnosticAction` seam defaulting to `MessageBox.Show`; added `private const string ArchiveRootUnavailableMessage`; added `private bool TryGetArchiveRoot(out string)` with a narrow `catch (InvalidOperationException)` and a `logger.Warn`; routed the three `OlAncestor` assignments through it, returning `false` in `MoveToFolderAsync` and reporting-then-returning in both `Open*` methods | +65 / -3 (423 to 485) |
| `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` | New file: 11 MSTest tests, 6 private arrangement helpers, one `TestableEfcDataModel` fixture subclass | +389 (new) |
| `QuickFiler.Test/QuickFiler.Test.csproj` | One `<Compile Include>` registration | +1 |
| Feature folder documents and evidence | `issue.md`, `spec.md`, `plan.2026-08-29T07-41.md`, one research artifact, 33 evidence artifacts | +3969 |

Change footprint matches AC18 exactly. `QuickFiler/Controllers/EfcFormController.cs`,
`TaskMaster/AppGlobals/AppOlObjects.cs`, `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs` and
`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs` are all absent from the branch diff, confirmed by
`git diff --name-only` against the merge base.

## 10. Compliance Verdict

**PASS.**

| Section | Verdict |
|---|---|
| 1. General Unit Test Policy | PASS |
| 2. General Code Change Policy | PASS |
| 3. C# Code Change Policy | PASS |
| 4. C# Unit Test Policy | PASS |
| 5. Test Coverage Detail | PASS overall; one FAIL row (modified-file line coverage) dispositioned non-blocking |
| 6. Test Execution Metrics | PASS |
| 7. Code Quality Checks | PASS |
| 8. Gaps and Exceptions | 7 non-blocking items recorded |
| Evidence Location Compliance | PASS |
| Rejected Scope Narrowing | none detected |

Blocking findings: **0**. No remediation-inputs artifact is required or produced.

## Appendix A: Test Inventory

New tests, all in `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`, namespace
`QuickFiler.Test.Controllers`:

| # | Test | Category | Pre-fix | Post-fix |
|---|---|---|---|---|
| 1 | `MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing` | Regression | FAIL | PASS |
| 2 | `MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing` | Regression | FAIL | PASS |
| 3 | `OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns` | Regression | FAIL | PASS |
| 4 | `OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns` | Regression | FAIL | PASS |
| 5 | `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress` | Redaction | FAIL | PASS |
| 6 | `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` | Invariant | PASS | PASS |
| 7 | `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot` | Ordering | PASS | PASS |
| 8 | `MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot` | Ordering | PASS | PASS |
| 9 | `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates` | Catch breadth | PASS | PASS |
| 10 | `OpenOlFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot` | Ordering | PASS | PASS |
| 11 | `OpenFsFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot` | Ordering | PASS | PASS |

Protected existing tests, verified unmodified in the branch diff:
`QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs`,
`QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs`,
`QuickFiler.Test/Controllers/EfcDataModelTests.cs`,
`QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`,
`QuickFiler.Test/Controllers/EfcFormControllerTests.cs`,
`TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs`.

## Appendix B: Toolchain Commands Reference

Commands mandated by `CLAUDE.md` § "C# Toolchain (run in this exact order)", all evidenced on this
branch. Executable paths are recorded in the evidence as unresolved `vswhere` expressions because the
resolved paths are absolute.

1. `dotnet tool restore` (once per worktree) — `evidence/baseline/p0-t6-dotnet-tool-restore.md`
2. `dotnet tool run csharpier format .` — `evidence/qa-gates/p6-t1-csharpier-format.md`
3. `dotnet tool run csharpier check .` — `evidence/qa-gates/p6-t2-csharpier-check.md`
4. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — `evidence/qa-gates/p6-t3-msbuild-analyzers.md`
5. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` — `evidence/qa-gates/p6-t4-msbuild-nullable.md`
6. `vstest.console.exe <assembly list> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` — `evidence/qa-gates/p6-t5-vstest-coverage.md`

Read-only commands run by this review, all non-mutating: `git rev-parse`, `git merge-base`,
`git status --short`, `git diff --numstat`, `git diff -U0`, `git diff --name-only`, `git show`,
`git check-ignore`, `awk END{print NR}`, `sha256sum`, `grep`, and a read-only Python parse of
`coverage/coverage.cobertura.xml`. No source file, policy document or acceptance criterion was
modified.
