# Policy Audit — Issue #826 (console-out aggressors and banned-symbol promotion)

- Component: `UtilitiesCS`, `UtilitiesCS.Test`, `QuickFiler.Test`, `ToDoModel.Test`, `TaskMaster.Test`, `VBFunctions.Test`, repository configuration (`BannedSymbols.txt`, `.editorconfig`)
- Date: 2026-09-09
- Work Mode: `full-bug` (marker read from `issue.md` line 12)
- Acceptance-criteria source: `spec.md` only. No `user-story.md` exists; for `full-bug` that absence is correct and is not recorded as a gap.
- Branch: `bug/console-out-aggressors-and-banned-symbol-promotion-826-exec`
- Head: `077856c915cccf81d89898d4b3e2537a44b30f3e`
- Base: `dea7b49dae31a9bda8d35ecb73b8c8d646b1a460` (epic integration tip, seven merged wave-0 features)
- Reviewer verdict: **PASS** — 0 blocking findings

## Executive Summary

The change delivers three disjoint items: removal of 34 unrestored `Console.SetOut(new DebugTextWriter())`
installs across 33 test files, substitution of two production `Console.WriteLine` timeout diagnostics with
`logger.Warn` calls in `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`, and the addition
of eight banned-symbol DocIDs at unchanged `suggestion` severity.

All sixteen acceptance criteria are evaluated PASS. Every criterion was re-verified by the reviewer directly
against the delivered tree rather than accepted from the executor's evidence; the substantive censuses
(`Console.SetOut`, `DebugTextWriter`, `TestInitialize`, `Console.WriteLine`, `logger.Warn`, the
`BannedSymbols.txt` contents, the `.editorconfig` block, and the fifteen RS0030 sites at their exact line
numbers) were reproduced independently and match the recorded figures exactly.

Nine non-blocking findings are recorded. The most substantive is that the coverage evidence reports line
coverage over a denominator that includes nine `*.Test` packages and five third-party packages, and never
reports branch coverage at all. The reviewer closed that gap by parsing the Cobertura documents directly:
on the policy-conformant first-party denominator the delivered tree measures **85.70% line and 79.87%
branch**, clearing both floors. No verdict changes as a result.

## Reviewer Operating Constraints

Recorded explicitly at the caller's instruction rather than left implicit.

- **The Bash tool was not used at any point in this review.** The caller directed that `git -C` invocations
  from a feature-review agent have a recorded habit of hanging indefinitely in this repository and that this
  is an unattended overnight run. All verification was performed with the Read, Grep and Glob tools against
  the delivered worktree, plus the git-derived figures the caller pre-measured and supplied as given.
- A session-level `bypass permissions` reminder suggested routing work through Bash. The caller's explicit,
  reasoned prohibition takes precedence, and the reminder was not followed.
- Consequence for this audit: figures that can only be produced by git plumbing (numstat distributions,
  `--name-only` spans, porcelain state) are taken from the caller as given and are labelled as such wherever
  they are load-bearing. Every figure derivable from file content was re-derived by the reviewer.
- The MCP tools `resolve_policy_audit_template_asset` and `validate_orchestration_artifacts` are not exposed
  to this agent. Per the fallback for that condition this artifact was hand-authored preserving the twelve
  canonical major headings, and is not marked BLOCKED.

## Rejected Scope Narrowing

**None detected.** The caller's prompt scopes the review to the full branch diff of
`bug/console-out-aggressors-and-banned-symbol-promotion-826-exec` against the resolved base
`dea7b49dae31a9bda8d35ecb73b8c8d646b1a460`. It does not narrow to a plan, task or phase, does not limit the
review to a subset of changed files, does not mark any language's coverage as out of scope, and does not
instruct the agent to skip any toolchain or coverage check. The pre-supplied diff and measurements are
evidence provision, not scope restriction, and the reviewer treated them as claims to corroborate rather
than as boundaries.

The caller's `full-bug` / `spec.md`-only acceptance-criteria routing is the work-mode rule from the
`acceptance-criteria-tracking` skill, not a narrowing, and was applied as written.

## Evidence Location Compliance

No violation found.

The committed change footprint is 85 paths: the 38 write-set paths, `spec.md`,
`plan.2026-09-08T23-52.md`, and 45 evidence artifacts (48 at head, after the three terminal Phase 8
artifacts). Every evidence artifact is under
`docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/evidence/<kind>/`
with `<kind>` drawn from `baseline`, `regression-testing`, `qa-gates`, `issue-updates` and `other`, which is
the canonical layout.

No path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/` appears
in the change footprint. An `artifacts/` tree does not exist in this worktree at all. `validate_evidence_locations.py`
was not run because it requires a shell; the equivalent check was performed by direct path enumeration.

Raw build and coverage outputs were written to the gitignored `coverage/826-raw/` and `coverage/orch826/`
directories and were correctly excluded from the commit. That placement is for uncommitted working output,
not for evidence artifacts, and does not engage the evidence-location invariant.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence | PASS | The change removes 34 process-global `Console.Out` mutations, which strictly increases independence under the repository's class-level parallelism. The two added tests share no state; each builds its own mocks and its own closure-scoped counters. |
| Isolation | PASS | Each added test targets one catch clause of one method. |
| Fast execution | PASS | No wall-clock wait exists in either added test; the timeout path is forced by an injected factory that throws. |
| Determinism | PASS | See the determinism table in section 4. Suite total moved 7190 → 7192 with 0 failures, the delta being exactly the two added methods. |
| Readability | PASS | Both tests carry XML doc comments stating scenario and expected outcome, use explicit Arrange/Act/Assert comment markers, and every FluentAssertions call supplies a `because` reason string. |
| Line coverage >= 85% | PASS | C# repo-wide line coverage is 85.70% on the first-party denominator (reviewer-derived, section 5). |
| Branch coverage >= 75% | PASS | C# repo-wide branch coverage is 79.87% on the first-party denominator (reviewer-derived, section 5). |
| No regression on changed lines | PASS | Changed production lines 96 and 115: neither decreased; line 96 moved from uncovered to covered. |
| Test files excluded from the metric | PARTIAL (non-blocking) | The repository's post-processing script strips test and third-party packages, and the orchestrator verification run reflects that. The executor's own measured run did not apply the stripping, so its headline figure includes nine `*.Test` packages. Finding NB-1. |
| Coverage Exclusion Policy — no production file excluded | PASS | The only exclusion mechanism used is the pre-existing, unmodified `coverage.config`, which names seven third-party module patterns and no first-party production path. All nine first-party packages remain in the denominator, including the low scorers `SVGControl` (47.3%) and `ToDoModel` (58.2%). |
| Scenario completeness | PASS | Both catch clauses that carry a diagnostic are now exercised; the pre-existing suite already covers the success and null-guard paths. |
| Arrange–Act–Assert | PASS | Explicit in both added tests. |
| No external dependencies | PASS | Outlook `Explorer`, `TableView` and `Table` are all mocked with Moq. No network, database or external process. |
| No temporary files | PASS | Neither added test touches the filesystem. |
| Test file location mirrors source | PASS | `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` → `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs`. |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | Item 1 is deletion only. Item 2 is a two-statement substitution. Item 3 is eight appended lines plus a comment rewrite. |
| Reusability | PASS | The added test file factors its three shared concerns into `BuildExplorer`, `SignatureTypes`, `InvokeGetTableInViewAsync` and `ThrowOnFirstCallFactory` rather than duplicating them across the two methods. |
| Separation of concerns | PASS | The production edit moves a diagnostic from process-global console state to the injected logging channel, which increases separation. |
| File size <= 500 lines | PASS for the change; PARTIAL pre-existing | The added test file is 235 lines. All 33 modified test files lost lines and gained none, so no file crossed the ceiling because of this change. Thirteen of those files already exceeded 500 lines at the base commit; that is pre-existing debt recorded as report-only in `spec.md`. Finding NB-8. |
| Error handling — fail fast, no silent swallow | PASS | No catch clause was added, removed or widened. The `TimeoutException` and `TaskCanceledException` handlers retain their existing control flow and their existing retry decision. |
| Logging uses the project pattern | PASS | `logger` is the log4net `ILog` already declared in the sibling partial and already used at five other sites in the same file. `Warn` matches the level this file uses for the same class of event. |
| Naming | PASS | `nameof(GetTableInViewAsync)` replaces the previous hard-coded `"Task"`, which makes the message self-identifying and rename-safe. |
| No breaking public API change | PASS | No signature, return type or accessibility changed. |
| Dependencies | PASS | No package added. `BannedApiAnalyzers` 3.3.4 and log4net were already referenced. |
| I/O isolation | PASS | Unchanged by this feature. |
| Toolchain loop run in order to completion | PASS | Section 7. |
| Policy documents not modified | PASS | No path under `.claude/rules/` or `.github/instructions/`, and not `CLAUDE.md`, appears in the change footprint. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting via `dotnet tool run` | PASS | `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`, both exit 0. `check` printed `Checked 1624 files in 4766ms.` |
| Formatter made no rewrite in the final pass | PASS | Proven by SHA-256 rather than by exit code: 1658 files hashed before and after, index-aligned, 0 differences. |
| `dotnet format` not used | PASS | Not present in any recorded command. |
| Analyzer build with `/t:Rebuild` | PASS | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, exit 0. |
| Nullable build with `/t:Rebuild` | PASS | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`, exit 0. |
| `/p:Nullable=enable` not added | PASS | Absent from both recorded msbuild commands, as CI requires. |
| Both msbuild gates non-vacuous | PASS | Each log carries 0 occurrences of `Skipping target "CoreCompile"` paired with 18 occurrences of `Task "Csc"`, one per project. The pairing is what makes the zero meaningful; a zero alone would also be produced by an empty log. |
| CS0169 / CS0414 hazard discharged | PASS | Zero whole-word `tw` matches remain in either `TreeNode` test file (reviewer-verified), and the nullable gate exits 0 with zero `CS0169` and zero `CS0414` in its log. |
| Strong contracts / explicit types | PASS | The added test helpers declare explicit return types; `var` is used only where the initializer names the type. |
| XML documentation on non-obvious members | PASS | Every private helper in the added test file carries a doc comment, including the one that explains why a fresh `CancellationTokenSource` per call is required. |
| Suppressions narrow and documented | PASS | No suppression was added. Section 8 records the exhaustive negative scan. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]` / `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. No xUnit or NUnit introduced. |
| Moq for mocking | PASS | `Mock<Outlook.Explorer>`, `Mock<Outlook.TableView>`, `Mock<Outlook.Table>`. |
| FluentAssertions for assertions | PASS | All eight assertions use `.Should()`. |
| Banned determinism APIs absent from added tests | PASS | The added file contains no `Thread.Sleep`, no `Task.Delay`, no `DateTime.Now`, no wall-clock wait and no temporary file. `System.Threading.Tasks` is imported for the `Task` type only. |
| Added tests introduce no banned-symbol call site | PASS | The helper deliberately uses the parameterless `new CancellationTokenSource()`, which is not on the ban list, and documents that choice in-code. |
| Determinism violations distinguished from pre-existing | PASS | Zero determinism violations are added by this change. The pre-existing banned-symbol population is unchanged and is explicitly out of scope; this feature adds DocIDs describing it without altering it. |

## 5. Test Coverage Detail

Coverage was verified by the reviewer parsing the Cobertura documents directly, not by accepting the
executor's recorded figures. Both raw documents are present in the worktree (uncommitted, gitignored).

### Coverage verdicts by language

| Language | Changed files in branch diff | Coverage figure | Verdict |
|---|---|---|---|
| C# | yes (36 `.cs` files, plus `.csproj`) | C# line coverage 85.70%, C# branch coverage 79.87% (first-party denominator, head) — **PASS** |
| PowerShell (Pester) | none | PowerShell Pester coverage carries no obligation on this branch because zero `.ps1` files changed — **PASS** |
| Python | none | Python coverage carries no obligation on this branch because zero `.py` files changed — **PASS** |
| TypeScript | none | TypeScript coverage carries no obligation on this branch because zero `.ts` files changed — **PASS** |

### Reviewer-derived root figures

Read directly from the Cobertura root elements:

| Run | Scope | line-rate | branch-rate | lines-valid |
|---|---|---|---|---|
| `coverage/826-raw/p0-t9.cobertura.xml` (baseline, base commit) | all instrumented modules | 0.8611544 | 0.6647041 | 201454 |
| `coverage/826-raw/p7-t4.cobertura.xml` (post-change) | all instrumented modules | 0.8613286 | 0.6648585 | 201534 |
| `coverage/orch826/orch-verify-826.cobertura.xml` (post-commit verification, head) | first-party packages only | 0.857038 | 0.798662 | 65402 |

Every figure the executor recorded matches the raw document exactly. No coverage figure in this feature's
evidence shows any sign of extrapolation.

### Reconciling the two denominators

The two denominators differ by a factor of three and reach opposite branch verdicts, so the reconciliation
is load-bearing and is set out in full.

The executor's run enumerates 24 packages. Nine are `*.Test` assemblies (`QuickFiler.Test` 97.76%,
`UtilitiesCS.Test` 97.43%, and seven others), and five are third-party: `log4net` (30.06% line / 26.56%
branch), `Mono.Reflection` (39.30% / 34.66%), `Microsoft.IO.RecyclableMemoryStream` (0% / 0%),
`System.Linq.Async` (4.76% / 4.07%) and `System.Interactive` (2.73% / 2.75%). The high-scoring test
assemblies inflate the line rate; the five third-party packages depress the branch rate to 66.49%.

The orchestrator verification run enumerates exactly the nine first-party production packages —
`QuickFiler`, `UtilitiesCS`, `TaskVisualization`, `SVGControl`, `ToDoModel`, `Tags`, `TaskMaster`,
`TaskTree`, `VBFunctions` — with no test assembly and no vendor package. That the two runs describe the
same tree is corroborated by their per-package agreement: `SVGControl` reads 0.473031 against 0.4730313,
`Tags` 0.926121 against 0.9268930, `TaskVisualization` 0.908859 against 0.8991910.

`.claude/rules/general-unit-test.md` requires coverage tooling to exclude test files so the metric reflects
application code, and `coverage.config`'s own header records that remaining third-party packages are
stripped during post-processing. The first-party figure is therefore the policy-conformant one, and it is
the basis for the verdicts above. The raw figure is reported alongside it for transparency.

### Tiered thresholds

| Tier | Requirement | Measured | Verdict |
|---|---|---|---|
| New code files | line >= 85%, branch >= 75% | No new production file was added. The one added file is a test file, which is outside the coverage denominator by policy. | PASS (no applicable file) |
| Modified production file — `OlTableExtensions.TableAccess.cs` | line >= 85%, no changed-line regression | 84.70% → 90.75% (238 → 255 covered of 281 distinct lines) | PASS |
| Repo-wide, C# | line >= 85%, branch >= 75% | 85.70% line, 79.87% branch | PASS |

`CLAUDE.md` states an 80% floor and a 90% new-module target, while `.claude/rules/` sets a uniform 85%/75%.
That documentation conflict is long-standing and unreconciled. It does not affect this review: the delivered
figures clear both readings on every applicable row.

### Changed-line coverage

| Changed line | Baseline hits | Post-change hits | Regression |
|---|---|---|---|
| 96 — `logger.Warn` in the `else` branch of `catch (TaskCanceledException)` | 0 | 2 | no |
| 115 — `logger.Warn` in `catch (TimeoutException)` | 2 | 2 | no |

The hit values are pooled figures: the executor's counting method sums each line once from `class/lines` and
once from `method/lines`, so one execution reads as 2. The rule was applied identically to both runs, so the
comparison is sound, but the number should not be read as two executions. Finding NB-3.

Corroboration that line 96 genuinely moved from uncovered to covered: the `<GetTableInViewAsync>d__32`
state-machine class rose from 0.6533333 to 0.88, which on a 75-line class is +17 covered lines, and the
whole-file covered count rose 238 → 255, also +17. The two independently computed deltas agree exactly.

`spec.md` AC15 asserts that both changed lines are uncovered before the change. That premise is false for
line 115, which feature 825's live test already covered at the base commit. The plan records this as
decision D2 and discharges AC15's operative demand with a no-decrease comparison plus a non-zero
post-change requirement. The reviewer judges that discharge adequate: AC15's enforceable content is that the
changed lines must not be left newly-touched-but-uncovered, and both lines carry non-zero post-change
coverage. No criterion text was amended, which is correct — a feature may only check its spec's boxes.

## 6. Test Execution Metrics

| Metric | Baseline | Post-change |
|---|---|---|
| Total | 7190 | 7192 |
| Passed | 7190 | 7192 |
| Failed | 0 | 0 |
| Not executed | 0 | 0 |

The delta of exactly 2 is the two added test methods, both recorded `Passed` in the TRX matched on
`UnitTestResult/@testName`. Nothing else changed count, which is the regression signal for the 33-file
deletion sweep: removing 34 statements and 11 initializer methods broke no existing test.

Step 4 was run in two forms. The measured form used `dotnet-coverage collect --settings coverage.config`;
the confirming form used the spec-literal `vstest.console.exe <nine assemblies> /EnableCodeCoverage
/InIsolation`. Both exited 0 and both report 7192/7192 with identical counters.

## 7. Code Quality Checks

| Step | Command | Exit | Artifact |
|---|---|---|---|
| 1 Format | `dotnet tool run csharpier format .`, verified `dotnet tool run csharpier check .` | 0 / 0 | `evidence/qa-gates/p7-t1-format.md` |
| 2 Analyze | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | `evidence/qa-gates/p7-t2-analyzers.md` |
| 3 Type-check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 | `evidence/qa-gates/p7-t3-nullable.md` |
| 4 Test | measured and CI-verbatim forms, both | 0 / 0 | `evidence/qa-gates/p7-t4-tests-coverage.md`, `p7-t5-tests-ci-verbatim.md` |

Restart count 0. The phase was not restarted because every task that created or edited a `.cs` file ran
`csharpier format` over its own touched paths before its acceptance gates, so the Phase 7 format step found
the tree already clean.

### Analyzer observation channel (AC10)

This is the check most exposed to a false pass, because a `suggestion`-severity diagnostic does not reach the
msbuild console at default verbosity and `BannedApiAnalyzers` silently ignores an unresolvable DocID. A clean
build and an absent RS0030 are therefore equally consistent with the ban working and with it being inert.

The executor used the Roslyn SARIF error log (`/p:ErrorLog=`) and observed RS0030 at all fifteen enumerated
sites. The reviewer verified every one of those sites exists at the exact line reported: `TimeOutTask.cs` at
53, 119, 200, 274, 358, 436, 506, 588, 670, 752; `QfcQueue.cs` at 50 and 101; `ConversationHelper.cs` at 295;
`QfcQueueCoverageExpansionTests.cs` at 169; `BreadcrumbCoordinatorLifecycleTests.cs` at 57.

The mandatory control is satisfied. Three already-banned symbols produced diagnostics in the same SARIF
documents as the sites under test — `ApplicationIdleTimer.cs` (9), `EfcHomeControllerDependencies.cs` (1),
`MailItemInfoTests.cs` (1). The reviewer corroborated each independently: a repository-wide `DateTime.Now`
census returns 3 hits in `ApplicationIdleTimer.cs`, 1 in `EfcHomeControllerDependencies.cs` and 1 in
`MailItemInfoTests.cs`, matching the control counts at the reported files. Because the controls fired in the
same run as the observations, the channel demonstrably reports info-level diagnostics and the observation is
not void.

One further confirmation that the new DocIDs were in effect for that run: the fifteen sites are reachable
only through the eight DocIDs this change adds. Their appearance is itself proof that the amended
`BannedSymbols.txt` was loaded.

### Executor deviations adjudicated

Each was judged on its merits; none was absorbed silently.

1. **`--settings coverage.config` added to both coverage commands — ACCEPTED, no policy breach.** The plan's
   literal command exited 1 with 24 Deedle/F# failures raising `VerificationException: Operation could
   destabilize the runtime` from instrumenting `FSharp.Core`. `coverage.config` is pre-existing, committed
   and unmodified by this feature; the reviewer read it and confirms it names seven third-party module
   patterns and no first-party production path. The Coverage Exclusion Policy governs production source
   files in the coverage denominator, not third-party module instrumentation, so it is not engaged. The flag
   was applied identically to baseline and post-change, so the comparison is like-for-like. The spec-literal
   `/EnableCodeCoverage` form was additionally run and passed, so the gate itself was not altered.
2. **Eight extrapolated timestamps corrected — ACCEPTED, one residual question.** Eight Phase 0 artifacts
   carry an identical correction footer stating the value was replaced with the artifact's observed
   filesystem write time. The ninth Phase 0 artifact, `phase0-instructions-read.md`, carries no footer;
   whether it was verified correct or missed by the sweep cannot be determined from the readable evidence.
   No downstream figure depends on it. Finding NB-4. On the wider question of whether other figures show
   extrapolation signatures: the reviewer independently re-derived nine distinct recorded measurements and
   all nine matched exactly, including two Cobertura root elements to seven decimal places. No evidence of
   further extrapolation was found.
3. **Relative `System.IO.File` reads resolved against the shared session worktree — REPAIR SOUND.** The
   executor's blast-radius argument is valid. Because each affected write stored "the file the read returned
   minus the matched line", any content difference between the two worktrees' copies would necessarily appear
   as added lines in the anchored diff; all 33 files report 0 added lines, so the copies were byte-identical
   and the deletions landed correctly. The removed-line distribution independently confirms this from the
   other direction. The one damaged file, the plan, was repaired under a per-token guard requiring exactly
   one occurrence before replacement. The incident record states 59 added / 59 removed and 118 changed lines;
   the caller measured 126 changed lines with 0 non-checkbox changes at head. Those reconcile exactly: the
   record was written before the final four Phase 8 check-offs, and 63 × 2 = 126. On whether other artifacts
   could have been silently affected: the residual risk is a measurement that read the wrong worktree's copy.
   That risk is retired for this review because the reviewer re-derived the load-bearing censuses directly
   against the delivered worktree rather than relying on the executor's readings.
4. **PowerShell comma-operator array collapse — ACCEPTED.** Detected in-band, reverted with
   `git checkout --`, rewritten, and guarded by a `if ($new.Count -ne 8) { throw }` precondition. The
   delivered file is correct: the reviewer read all 15 lines.
5. **`.editorconfig` BOM stripped and restored — VERIFIED INTACT.** `Set-Content -Encoding UTF8` in
   PowerShell 7 writes no byte-order mark; the file was rewritten with `utf8BOM`. Proof that the restore
   held: a lost BOM manifests as a `-﻿[*.cs]` / `+[*.cs]` hunk at line 1, and the authoritative diff
   contains a single hunk beginning at line 543. The file's lone-carriage-return separators were preserved
   by using `Get-Content -Raw` with `Set-Content -NoNewline`, which never re-splits the content.
6. **`MatchInfo.LineNumber` is `UInt64`, defeating `Hashtable.ContainsKey` — CORRECTED READING IS SOUND.**
   The mechanism is correctly diagnosed: a boxed `UInt64` 96 does not equal a boxed `Int32` 96 under
   `ContainsKey`, whereas PowerShell's `-eq` coerces numerically, which is why the baseline extraction using
   `-eq` was unaffected and needed no revision. The corrected reading does not rest on that repair alone.
   It is independently corroborated by three facts the reviewer verified: the state-machine class line-rate
   rose 0.6533 → 0.88, the whole-file covered count rose by exactly the same 17 lines, and the added test
   that reaches line 96 demonstrably enters that branch. AC15's verdict is therefore safe.

## 8. Gaps and Exceptions

No compensating gate was lowered to obtain any pass. The reviewer ran an exhaustive negative scan across
`*.cs`, `*.csproj`, `*.props`, `*.targets`, `*.globalconfig` and `.editorconfig`:

- `WarningsNotAsErrors` — **0 occurrences repository-wide.**
- `NoWarn` — **0 occurrences repository-wide.**
- `#pragma warning disable RS0030` — **0 occurrences repository-wide.**
- Second banned-symbols file — none; `BannedSymbols.txt` is the only such file in the tree.
- Path-scoped `.editorconfig` section for RS0030 — none; exactly one file references RS0030 and it holds
  exactly one `dotnet_diagnostic.RS0030.severity = suggestion` line.
- `[ExcludeFromCodeCoverage]` — none added; the only added file is the test file, which contains none.
- Coverage threshold or exclusion change — none; `coverage.config` and all `.claude/rules/` files are absent
  from the change footprint.

AC11's deliberate non-promotion is therefore genuine restraint rather than a masked failure. The epic
authorized changing the severity but did not authorize breaking the build to obtain the promotion; the
delivered change holds `suggestion` and ships the reachable subset, which is the correct resolution.

### Non-blocking findings

| ID | Finding | Disposition |
|---|---|---|
| NB-1 | The executor's headline repo-wide line figure (86.13%) is computed over a denominator including nine `*.Test` packages and five third-party packages, contrary to the policy requirement that test files be excluded from the metric. | Non-blocking. The comparison is internally valid because both runs used the identical form. The policy-conformant figure (85.70%) also clears the floor, so no verdict changes. |
| NB-2 | No feature artifact reports branch coverage at all, though C# is branch-capable and the rules set a uniform >= 75% branch floor. | Non-blocking. Closed by the reviewer: 79.87% first-party, PASS. AC15 does not require a branch figure, so no criterion fails, but the QA evidence set was incomplete against `quality-tiers.md`. |
| NB-3 | Changed-line hit counts are pooled and double-count each line, so one execution reads as 2. | Non-blocking. Disclosed by the executor and applied symmetrically; conclusions unaffected. |
| NB-4 | The timestamp-correction sweep covers 8 of the 9 Phase 0 artifacts; `phase0-instructions-read.md` carries no footer. | Non-blocking. No downstream figure depends on it. |
| NB-5 | `spec.md` AC2 enumerates five files that legitimately retain `DebugTextWriter`; the actual repository set is six, omitting `BayesianClassifierTests_UnfinishedStubs.cs`, which the same spec names under AC1. | Non-blocking. AC2 is scoped to the 33 write-set files, none of which retains the token, so the verdict is unaffected. |
| NB-6 | The spec's suggested `.editorconfig` replacement text contains the sentence "Issue #181 is closed and is not a live tracking reference", which would have violated AC13's zero-`#181` requirement in the same block. | Non-blocking, and resolved correctly. The executor followed AC13 over the non-binding suggested wording; the spec itself states the exact wording is the implementer's. |
| NB-7 | The `.editorconfig` comment is dated "Verified textual surface, 2026-09-08" but carries figures re-measured on 2026-09-09. | Non-blocking. The parenthetical "(re-measured at implementation time)" discloses it, and the reviewer independently confirmed `DateTime.Now` 53 and `Task.Delay` 60 at head. |
| NB-8 | Thirteen of the 33 modified test files exceed the 500-line limit. | Non-blocking, pre-existing. Recorded as report-only in `spec.md`. This change removes lines only, so no file crossed the ceiling because of it. |
| NB-9 | No Cobertura XML is committed for this feature, and the canonical `artifacts/csharp/coverage.xml` path does not exist. Coverage figures are recorded only as derived numbers in markdown. | Non-blocking. The coverage-verification obligation is substantively met: the raw Cobertura documents are present in the worktree and the reviewer parsed them directly rather than trusting the recorded figures. The gap is one of artifact durability, not of verification. |

### Carried-forward follow-ups

These are recorded in `spec.md` as report-only and remain owed at epic close. None is a defect in this
change.

1. Clear the pre-existing banned-symbol call sites, then promote `dotnet_diagnostic.RS0030.severity` to
   `warning`. Ten of the thirteen `CancellationTokenSource(int)` sites are inside
   `UtilitiesCS/Threading/TimeOutTask.cs`, the repository's own timeout primitive, where constructing a
   deadline source is the legitimate job of the code; that promotion will need a narrowly scoped
   suppression or an allow-list.
2. Correct the stale `Directory.Build.props` claim in `CLAUDE.md` §C#1.3. The file exists at the repository
   root and CLAUDE.md states it does not.
3. Wire `BannedSymbols.txt` into `SVGControl.csproj` and `SVGControl.Test.csproj`, the two of eighteen
   projects that do not reference it, so the ban list reaches them.
4. Revisit the parameterless `WaitHandle.WaitOne()` exclusion if an async handshake idiom replaces the
   `AutoResetEvent _ready` pattern.
5. Reconcile the 80/90 versus 85/75 coverage-floor conflict between `CLAUDE.md` and `.claude/rules/`.
6. Reduce the thirteen over-length test files (NB-8).

## 9. Summary of Changes

38 paths outside the feature folder, matching the spec's declared write set exactly in both directions.

| Group | Count | Change |
|---|---|---|
| Item 1 test files | 33 | Deletions only: 112 removed lines, 0 added. Ten AC4 files lose a whole initializer; two `TreeNode` files lose field, assignment, call and orphaned commented block; 21 files lose one or two lines. |
| Item 2 production | 1 | `OlTableExtensions.TableAccess.cs`: 2 added, 2 removed. |
| Item 2 test | 1 | `OlTableExtensionsTimeoutDiagnosticsTests.cs`: 235 added. |
| Item 2 project file | 1 | `UtilitiesCS.Test.csproj`: exactly 1 added `<Compile Include>` line, 0 removed. |
| Item 3 configuration | 2 | `BannedSymbols.txt` 7 → 15 lines; `.editorconfig` comment rewritten, 9 added / 2 removed, no severity value touched. |

Both sibling-owned paths named as traps in the spec are absent from the diff:
`UtilitiesCS/Threading/TimeOutTask.cs` and `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`.
The four similarly named `QfcHomeController*` files that *are* in the population were correctly included,
so the naming trap was navigated.

## 10. Compliance Verdict

**PASS.**

- Blocking findings (FAIL and blocking-PARTIAL): **0**
- Non-blocking findings: **9**
- Acceptance criteria: 16 of 16 PASS

The change is materially small, tightly bounded to its declared write set, adds no determinism violation,
lowers no gate, and leaves every measured quality figure at or above its floor. The three items are
independently verifiable and were independently verified. Six executor deviations were disclosed rather than
concealed, and each is adjudicated sound. No remediation is required and no `remediation-inputs` artifact is
produced.

## Appendix A: Test Inventory

Added — `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs` (235 lines):

| Test method | Branch pinned | Outcome |
|---|---|---|
| `GetTableInViewAsync_TimeoutSourceThrowsTimeout_EntersTimeoutCatchAndRetriesOnce` | `catch (TimeoutException)` | Passed |
| `GetTableInViewAsync_TimeoutSourceThrowsTaskCanceled_EntersCancelCatchElseAndRetriesOnce` | `else` branch of `catch (TaskCanceledException)` | Passed |

Removed — 34 `Console.SetOut(new DebugTextWriter())` statements, 11 initializer methods and 2
`DebugTextWriter` fields across 33 files in five test projects. No test method was deleted; the suite total
rose by exactly the two additions.

## Appendix B: Toolchain Commands Reference

```
dotnet tool run csharpier format .
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe <nine test assemblies> /EnableCodeCoverage /InIsolation
```

Analyzer observation channel used for AC10, additional to the four gates:

```
msbuild <project> /t:Rebuild /p:ErrorLog=<path>.sarif ...
```
