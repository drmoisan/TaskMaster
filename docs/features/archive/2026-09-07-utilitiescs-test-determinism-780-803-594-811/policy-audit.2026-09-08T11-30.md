# Policy Audit — utilitiescs-test-determinism (Issue #811)

- Component: `UtilitiesCS`, `UtilitiesCS.Test`
- Date: 2026-09-08T11-30
- Branch: `bug/utilitiescs-test-determinism-780-803-594-811`
- Head commit: `3805ca89f0c72e2bb448726695f18f420bd260f7`
- Base commit (merge-base anchor): `bb1c7d4b60f7b782227956f36859314d5c47bb03`
- Work mode: `full-bug` (`issue.md` line 12) — acceptance-criteria source is `spec.md` only
- Reviewer: feature-review agent

## Executive Summary

Verdict: **PASS with one blocking process finding.**

The change repairs three distinct nondeterminism defects in `UtilitiesCS.Test` and its production
dependencies: a consumer-free 500 ms wall-clock cancellation window in
`DictionaryExtensions.TryAddValuesAsync`; an unguarded null-snapshot dereference on the
`DfDeedle.GetEmailDataInViewAsync` ETL path together with two mutable process-wide ETL delegate
statics; and four `Console.Out` capture races. All four gates of the mandatory toolchain pass on
the final tree, independently corroborated from the raw MSBuild logs in this worktree. Repository
line coverage rose and no changed file regressed.

Four of five acceptance criteria are met. AC4 (zero failures across ten consecutive full-suite
runs) is not met: run 7 of 10 reported one failure in
`MethodBodyReader_Tests.GetBodyCode_ReturnsConcatenatedInstructions`. This reviewer independently
confirmed the cause is an unsynchronised process-wide static in
`UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`, which this change does not touch, and
confirmed that the failure signature is uniquely explained by that mechanism (see section 8, F-1).
The AC4 shortfall is graded Non-blocking for merge.

One finding is graded **Blocking**: the `ILGlobals` race — the sole reason an acceptance criterion
is unmet — has no durable follow-up artifact. Two other follow-ups were filed as
`docs/features/potential/` entries by the same plan task, so the executor's stated reason for
omitting the third ("authoring it was not in this plan's write set") is contradicted by its own
write set. Without a durable entry the finding is lost at merge.

Blocking findings in this artifact: **1**.

## Scope Confirmation

The audit scope is the full branch diff against `bb1c7d4b`. The caller supplied two pre-generated
read-only artifacts because `git` invocation from a review agent hangs in this checkout:

- `coverage/review-811-source.patch` — unified diff of the 20 changed `.cs` / `.csproj` files
- `coverage/review-811-diffstat.txt` — diffstat of all 68 changed files

The diffstat confirms the full change set is 68 files: 20 source/project files, 46 feature-folder
documents and evidence artifacts, and 2 `docs/features/potential/` entries. The source patch is a
projection of the source subset, not a scope narrowing: the remaining 48 files were audited by
reading them directly. No caller instruction attempted to narrow the audit to a plan, task, phase,
or file subset, to mark a language as not applicable when it has changed files, or to skip a
toolchain or coverage check. No `## Rejected Scope Narrowing` section is therefore required.

Changed languages in the branch diff: **C# only**. No `.ps1`, `.py`, `.ts`, `.tsx`, `.yml` or
`.yaml` file appears in the diffstat.

## Evidence Location Compliance

All evidence artifacts this change produced are under
`docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/evidence/<kind>/`
(`baseline`, `qa-gates`, `regression-testing`, `issue-updates`, `other`). The diffstat contains no
path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`.

| Check | Result |
|---|---|
| Files written to `artifacts/baselines/` | 0 |
| Files written to `artifacts/qa/` | 0 |
| Files written to `artifacts/evidence/` | 0 |
| Files written to `artifacts/coverage/` | 0 |
| Evidence under `<FEATURE>/evidence/<kind>/` | 33 files across 5 canonical kinds |

Verdict: **PASS**.

Working files produced during execution (TRX, Cobertura XML, MSBuild logs, the review patch) live
under the gitignored `coverage/` directory and do not appear in the branch diff. That is correct:
`coverage/` is a scratch location, not an evidence location, and nothing there is cited as
committed evidence except by value transcribed into a committed `<FEATURE>/evidence/` artifact.

## 1. General Unit Test Policy Compliance

Reference: `.claude/rules/general-unit-test.md`.

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence — tests run in any order | PASS | Three `[DoNotParallelize]` attributes removed only after the shared-state dependency was eliminated by a `TextWriter` parameter; the two new classes carry `[DoNotParallelize]` and document why. |
| Isolation — one unit of behavior per test | PARTIAL | One exception: `OlTableExtensions_Tests.EnumerateTable_WritesFormattedOutputToSuppliedWriterAndMovesToStart` performs two Acts (seam path and null-writer path). See section 8, F-3. |
| Fast execution | PASS | Full nine-assembly suite 51.3 s to 73.9 s over ten runs; the new deadline tests are clock-driven, not wall-clock. |
| Determinism | PASS | Every new deadline test drives `FakeTimeProvider` or `ArmingBarrierTimeProvider`. No test advances wall time. |
| Readability / maintainability | PASS | Every new test method and helper carries an XML doc comment stating the scenario and the reason for the technique. |
| Line coverage >= 85% | PASS | Repository line-rate 86.0424% on the final Cobertura document, re-derived by this reviewer from the `<coverage>` root element. |
| Branch coverage >= 75% | FAIL | Repository branch-rate 66.3978%. Pre-existing: baseline branch-rate was 66.3058%. This change improved it by +0.092 percentage points. See section 8, F-2. |
| No regression on changed lines | PASS | All 38 added executable lines report `hits > 0`; no per-file uncovered count rose across the eight production files. |
| No coverage exclusion of production paths | PASS | No `exclude` entry and no `[ExcludeFromCodeCoverage]` attribute is added by this change. |
| Scenario completeness (positive, negative, edge, error) | PASS | Positive: green-clock paths on both new classes. Negative/error: deadline expiry throwing `InvalidOperationException`, pre-cancelled token throwing `TaskCanceledException`. Edge: null `TextWriter` and null `TimeProvider` defaults both exercised. |
| Arrange–Act–Assert structure | PASS | All new tests carry explicit `// Arrange` / `// Act` / `// Assert` markers. |
| No external dependencies | PASS | Every boundary is a Moq mock of an Outlook COM interface. No live Outlook, network, or database. |
| No temporary files in tests | PASS | Zero filesystem writes in the added test code; assertions target in-memory `StringWriter` instances. |
| Test files mirror source tree | PASS | `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs`, `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs` and `UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs` all sit under the test project mirroring their production counterparts. No test file was placed in a production source tree. |
| Banned APIs in test code (`Thread.Sleep`, `Task.Delay`, timed waits, wall-clock reads) | PASS | Zero occurrences across the 737 added lines; independently confirmed against the diff. The three surviving `gate.Wait` references are untimed `ManualResetEventSlim.Wait()` method groups passed as delegates and released in `finally`. |
| Fake timers / `FakeTimeProvider` for async time | PASS | `Microsoft.Extensions.Time.Testing.FakeTimeProvider` is used in five tests; `ArmingBarrierTimeProvider` wraps it to enforce arm-before-advance ordering. |

## 2. General Code Change Policy Compliance

Reference: `.claude/rules/general-code-change.md` and CLAUDE.md.

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | The three defects share one technique (make the wall clock or the writer injectable, or delete it when it guards nothing). No new abstraction layer is introduced. |
| Reusability | PASS | `ArmingBarrierTimeProvider` was moved out of `DfDeedleQfcColumnTimeoutTests` into `UtilitiesCS.Test/TestHelpers/` and is now shared by three classes rather than duplicated. |
| Extensibility / no breaking API change | PASS | Every new parameter is optional and trailing. Two `internal static` fields were removed; all readers and writers were in-repo and are updated in the same change. |
| Separation of concerns | PASS | The `TextWriter` seam separates rendering from the process-wide console; the `TimeProvider` seam separates deadline policy from the system clock. |
| Bugfix workflow — failing regression test first | PASS | `evidence/regression-testing/p2-t4-ac2-fail-before.md` and `fail-before-exception.2026-09-08T09-50.md` record the RED state for AC2; `p3-t2-ac2-pass-after.md` records GREEN after the fix. AC1's RED artifact is documented as a contract-shape test rather than a load reproduction, with the reason stated in `spec.md` (a load-dependent failure cannot be made deterministically RED without a sleep, which AC5 forbids). |
| Bugfix workflow — minimal targeted fix | PASS | Production edits are confined to eight files; each is a deletion of a consumer-free guard, an optional parameter, or a new guard clause. |
| Bugfix workflow — open a new issue rather than widen scope | PARTIAL | Two of three uncovered defects were filed as `docs/features/potential/` entries. The third (`ILGlobals`) was not. See section 8, F-1. |
| Full toolchain in order, restart on change | PASS | `Toolchain pass: 3` is recorded on all four P7 gate artifacts, meaning the final clean pass followed two earlier passes that changed files. |
| File size limit 500 lines | PASS | See section 7. No new violation; all three pre-existing violations held or shrank. |
| Fail fast and explicitly | PASS | The new `InvalidOperationException` in `DfDeedle.GetEmailDataInViewAsync` names the folder and the failure mode, replacing an unattributed `NullReferenceException` thrown one statement later. |
| Established logging pattern | PASS | The corrected `EtlAsync` timeout log keeps the existing `logger.Error` call and removes a retry count that was never consumed plus a banned `DateTime.Now` format call. |
| Naming | PASS | `PascalCase` types and public members, `camelCase` locals and parameters throughout the added code. |
| Comment why, not what | PASS | Comments on `timeProvider` parameters state the production-invariance rationale; the retained `[DoNotParallelize]` on `OlTableExtensions_Tests` states the new reason after the console reason ceased to apply. |
| No new dependencies | PASS | `Microsoft.Bcl.TimeProvider` and `Microsoft.Extensions.TimeProvider.Testing` were already referenced. The only project-file change is three `<Compile Include>` items. |
| I/O boundaries isolated | PASS | `Console.Out` moves from an implicit process-wide dependency to an explicit parameter on four production members. |

## 3. Language-Specific Code Change Policy Compliance (C#)

Reference: `.claude/rules/csharp.md` and CLAUDE.md sections C#1 to C#7.

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting, via `dotnet tool run` | PASS | `dotnet tool run csharpier check .` exit 0, `Checked 1616 files in 7454ms.` The file count rose by exactly 3, matching the three new `.cs` files. |
| Analyzer gate with `/t:Rebuild` | PASS | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exit 0. Independently corroborated: `coverage/msbuild-p7-t3.log` lines 11388-11389 read `0 Warning(s)` and `0 Error(s)`. |
| Nullable / warnings-as-errors gate with `/t:Rebuild` | PASS | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` exit 0. Independently corroborated: `coverage/msbuild-p7-t4.log` lines 11312-11313 read `0 Warning(s)` and `0 Error(s)`. `/p:Nullable=enable` is correctly not supplied, matching `.github/workflows/_build-nullable.yml`. |
| `/t:Rebuild` rather than `/t:Build` | PASS | Both gate commands are recorded verbatim with `/t:Rebuild`, so `CoreCompile` ran and neither gate is vacuous. |
| Nullable annotations on touched production files | PASS | All eight production files carry `#nullable enable`; the new `TimeProvider?`, `TextWriter?` and `Func<...>?` parameters and the `is null` guard produced no `CS86xx` diagnostic under warnings-as-errors. |
| Strong contracts / explicit public API | PASS | The removed `TableEtlInvoker` / `StoreTableEtlInvoker` mutable statics are replaced by an explicit optional `etl` parameter; the surviving default is `private static readonly DefaultTableEtl`, which narrows the public surface. |
| Composition over inheritance | PASS | `ArmingBarrierTimeProvider` derives from `TimeProvider` because that is the framework extension point, and it forwards every member to a composed `FakeTimeProvider` rather than reimplementing behavior. |
| Exceptions — fail fast, no broad catch added | PASS | No new `catch (Exception)` is introduced. The pre-existing `catch (TimeoutException)` in `EtlAsync` is retained deliberately and is now covered by a test that documents the swallow-and-cancel contract. |
| Public surface minimal, `internal` preferred | PASS | `ArmingBarrierTimeProvider` is `internal sealed`. `GFG.Run(TextWriter)` is `public` because `GFG.Main` already was and the test asserts on it. |
| XML docs where behavior is non-obvious | PASS | Both new test classes, the shared helper, and `GFG.Run` carry doc comments or explanatory comments. |
| DI seam preference order | PASS | Injectable delegate (`Func<object,(object[,],Dictionary<string,int>)>? etl`) is preference 2 and is correct for a single call path; `TimeProvider` is the explicitly named time seam. Constructor injection is not available because all touched members are `static`, so a trailing optional parameter is the smallest viable seam. |
| Banned symbols posture | PASS (improved) | One `DateTime.Now` call was removed from the `EtlAsync` timeout log line. No banned symbol is added. |
| No broad refactor across unrelated projects | PASS | Two projects touched, both named in `spec.md` item list 1-20. The write set matches the spec exactly. |
| No weakened assertions | PASS | The retired `Returns(120)` row-count tolerance is replaced by an un-advanced `FakeTimeProvider`, which constrains the deadline rather than widening it. Assertions on the affected test are unchanged. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest as the framework | PASS | `Microsoft.VisualStudio.TestTools.UnitTesting` with `[TestClass]` / `[TestMethod]` in all new and modified test files. No xUnit or NUnit reference is introduced. |
| Moq for mocking | PASS | `Mock<Outlook.Table>`, `Mock<Outlook.Row>`, `Mock<Outlook.Columns>`, `Mock<Outlook.Explorer>`, `Mock<MAPIFolder>`, `Mock<UserDefinedProperties>` throughout the two new classes, with `MockBehavior.Strict` where the interaction set is closed. |
| FluentAssertions for assertions | PASS | `Should().Be`, `Should().BeNull`, `Should().Contain`, `FluentActions.Awaiting(...).Should().ThrowAsync<InvalidOperationException>().WithMessage("*Inbox*")`, `FluentActions.Invoking(...).Should().NotThrow()`. No MSTest `Assert` call is added. |
| `[TestClass]` / `[TestMethod]` attributes | PASS | Present on every added class and method. |
| Arrange–Act–Assert | PASS | Explicit section comments in all added tests. |
| No external dependencies | PASS | No live COM, no network, no filesystem. |
| Repo-wide line coverage >= 80% (CLAUDE.md UT2) | PASS | 86.0424%. |
| New module/class/method >= 90% (CLAUDE.md UT2) | PASS | `StackGeek.GFG.Run` line rate 1.0, measured by method element and reported as a number rather than absent, so the measurement is not vacuous. |
| Coverage regression on changed lines is blocking | PASS | Zero uncovered added executable lines; zero per-file regressions. |
| Deterministic test rules (no PATH, profile, cwd, clock dependence) | PASS | Clocks are injected; no ambient state is read. |

## 5. Test Coverage Detail

Coverage was verified from pre-existing artifacts. This reviewer re-derived the repository figures
directly from the `<coverage>` root elements of `coverage/p0-baseline.cobertura.xml` and
`coverage/p7-final.cobertura.xml` rather than accepting the transcribed values.

| Language | Metric | Value | Threshold | Verdict |
|---|---|---|---|---|
| C# | repository line coverage | 86.0424% (172626 / 200629) | >= 85% | PASS |
| C# | repository branch coverage | 66.3978% (21489 / 32364) | >= 75% | FAIL |
| C# | new-file line coverage | all added executable lines covered (38 / 38) | >= 85% new-code floor | PASS |
| C# | modified-file line coverage, no regression | 0 of 8 production files regressed | no regression | PASS |
| PowerShell | coverage | zero changed `.ps1` files in the branch diff | not evaluated | N/A |
| Python | coverage | zero changed `.py` files in the branch diff | not evaluated | N/A |
| TypeScript | coverage | zero changed `.ts` / `.tsx` files in the branch diff | not evaluated | N/A |

Repository-wide movement, baseline to final:

| Metric | Baseline | Final | Delta |
|---|---|---|---|
| `line-rate` | 0.8601092896174863 | 0.8604239666249645 | +0.0003147 |
| `lines-valid` | 200385 | 200629 | +244 (0.1218%) |
| `branch-rate` | 0.6630576006929406 | 0.6639784946236559 | +0.0009209 |
| `branches-valid` | 32326 | 32364 | +38 |

The denominator moved 0.1218%, inside the 1% comparability band, so the rate comparison is
directly meaningful (comparability branch A). Both rates improved.

Per-file uncovered line counts, baseline to final:

| Production file | Baseline uncovered | Final uncovered | Verdict |
|---|---|---|---|
| `UtilitiesCS/Extensions/DictionaryExtensions.cs` | 7 | 7 | PASS |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs` | 26 | 12 | PASS (improved) |
| `UtilitiesCS/Extensions/DfDeedle.cs` | 2 | 0 | PASS (improved) |
| `UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs` | 52 | 52 | PASS |
| `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs` | 3 | 3 | PASS |
| `UtilitiesCS/HelperClasses/PrettyPrint.cs` | 65 | 65 | PASS |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` | 55 | 55 | PASS |
| `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs` | 0 | 0 | PASS |

`EtlAsync` method-span line rate rose from 0.7000 to 0.9796. The previously uncovered
`catch (TimeoutException)` block is now exercised by
`OlTableExtensionsEtlClockTests.EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource`.

Coverage-artifact path note: the canonical path `artifacts/csharp/coverage.xml` does not exist in
this worktree. Verification was performed against `coverage/p7-final.cobertura.xml` (a valid
Cobertura document with a `<coverage>` root, read directly by this reviewer) plus the committed
transcriptions in `<FEATURE>/evidence/qa-gates/p7-t5-vstest-coverage.md` and `p7-t6-coverage-delta.md`.
The substance of the coverage gate is verified; only the canonical file location is absent. This is
recorded as a procedural deviation, not a coverage failure. See section 8, F-5.

## 6. Test Execution Metrics

| Metric | Value |
|---|---|
| Assemblies in the gate | 9 |
| Tests per run | 7162 |
| Runs executed for AC4 | 10 |
| Runs with zero failures | 9 |
| Runs with failures | 1 (run 7: 7161 passed, 1 failed) |
| Wall clock per run | 51.3 s to 73.9 s |
| Class-level parallel workers | 24 (`Workers = 0` resolving to `ProcessorCount`) |
| Tracked-test assertions across the ten runs | 130 (13 tests x 10 runs), all `Passed` |
| Source commit under test for all ten runs | `03b7bd57cacfc902a8b9f4e917ace627ffcac464`, identical across all five pair tasks |
| Tree dirty during the runs | No (`git status --porcelain -- "*.cs" "*.csproj"` entry count 0) |

Filter deviation, restated: the local runs extend `TestCategory!=LiveOutlook` with
`FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser`.
That exclusion is a documented workstation-environmental issue carried forward from the #798
baseline, not a property of this change, and CI runs those classes unfiltered. Ten local filtered
runs and one unfiltered CI run are not equivalent to ten unfiltered runs; the evidence artifact
states both facts rather than conflating them, which this reviewer confirms is the honest framing.

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| Format (csharpier check) | `dotnet tool run csharpier check .` | PASS — exit 0, 1616 files checked |
| Lint (.NET analyzers) | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | PASS — exit 0, 0 warnings, 0 errors |
| Type check (nullable, warnings as errors) | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | PASS — exit 0, 0 warnings, 0 errors |
| Test | `vstest.console.exe <nine assemblies> /EnableCodeCoverage /InIsolation` | PARTIAL — 9 of 10 runs clean; run 7 failed one test outside the write set |

File size cap (500 lines), verified against the plan's requirement that no pre-existing violation
grow:

| File | Baseline | Final | Verdict |
|---|---|---|---|
| `UtilitiesCS/HelperClasses/PrettyPrint.cs` | 680 | 680 | PASS — held exactly, pre-existing violation not enlarged |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | 869 | 848 | PASS — reduced by 21, pre-existing violation |
| `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` | 1855 | 1846 | PASS — reduced by 9, pre-existing violation |
| `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` | 500 | 453 | PASS — helper extracted to `TestHelpers/` |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs` | 475 | 476 | PASS — under cap |
| Largest new file | n/a | `OlTableExtensionsEtlClockTests.cs` at 234 | PASS |
| `UtilitiesCS/Threading/TimeOutTask.cs` | 1011 | 1011 | PASS — untouched pre-existing violation |

`PrettyPrint.cs` held at exactly 680 because three unused `using` directives
(`System.Threading.Tasks`, a duplicated inner `System.Text`, and `Svg`) were removed to pay for the
two added `TextWriter?` parameter lines and one added `System.IO` directive. That is an incidental
cleanup, but it is confined to the same file and is the mechanism by which a pre-existing violation
was prevented from growing. It compiles clean under the analyzer and nullable gates, which
establishes the removed directives were genuinely unused.

Three pre-existing 500-line violations are correctly reported as pre-existing, not as newly
introduced. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` at 988 lines is not subject to the cap: the
General Code Change Policy applies it to production code, test code and reusable script files.

## 8. Gaps and Exceptions

### F-1 — BLOCKING — The `ILGlobals` static race has no durable follow-up artifact

The AC4 ten-run gate failed on run 7 with
`UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.MethodBodyReader_Tests.GetBodyCode_ReturnsConcatenatedInstructions`.
This reviewer verified the mechanism independently by reading the source rather than accepting the
executor's derivation:

- `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs:123` reassigns `singleByteOpCodes` to a
  freshly allocated all-default `OpCode[0x100]`, then fills it in a reflection loop at line 137.
  There is no lock, no `Lazy<T>`, and no `volatile`. Lines 117-118 declare both tables as plain
  mutable `public static` fields.
- `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs:105` reads
  `ILGlobals.singleByteOpCodes[(int)value]` with no synchronisation.
- Exactly two test classes call `LoadOpCodes()`: `ILGlobals_Tests` (lines 15, 26, 37, 47) and
  `MethodBodyReader_Tests` (line 364, inside the private `CreateReader` helper that every test in
  that class routes through). Neither class carries `[DoNotParallelize]`, and both are therefore
  parallel-eligible under `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`
  at `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`.
- Within a class, `ClassLevel` scope serialises the tests, so `MethodBodyReader_Tests` cannot race
  itself. The second participant must be `ILGlobals_Tests`.

The observed failure output corroborates the mechanism uniquely. The reported body was:

```
0000 : nop
0001 :  1879067923
0006 : stloc.0
0007 : br.s 0009
0009 : ldloc.0
0010 : ret
```

Only the entry at IL offset 0001 — `ldstr`, opcode `0x72` — degraded, while `nop` (0x00),
`stloc.0` (0x0A), `br.s` (0x2B), `ldloc.0` (0x06) and `ret` (0x2A) all resolved correctly. That is
the signature of a partially filled table, not of a null or wholly-uninitialised one. The two
spaces where the opcode name belongs are the empty `Name` of `default(OpCode)`;
`default(OpCode).OperandType` is `InlineBrTarget` (value 0), which drives the reader down the
numeric-operand branch and stores a raw metadata token instead of calling
`module.ResolveString(...)`. The printed value 1879067923 lies in the `0x70……` range, which is the
String metadata-token range, so the token was read correctly and merely not resolved. No other
mechanism explains a single-entry degradation with a correctly-ranged unresolved string token.

The defect is pre-existing and outside this item's 20-path write set. That is not the finding. The
finding is that it has no durable record. The executor filed two follow-ups as
`docs/features/potential/` entries in task P8-T9
(`2026-09-08-etl-deadline-mechanics-follow-ups.md` and
`2026-09-08-console-out-aggressors-and-banned-symbol-promotion.md`) and confirmed both in P8-T12,
but wrote for the third: "it is reported to the orchestrator rather than filed here, because
authoring it was not in this plan's write set"
(`evidence/issue-updates/issue-811.2026-09-08T10-44.md` lines 77-78). That justification is
contradicted by the same plan's own conduct: `docs/features/potential/` demonstrably was in the
write set, because two entries were written there. This reviewer confirmed by search that no file
under `docs/features/potential/` mentions `ILGlobals`, `SDIL` or `MethodBodyReader`.

Impact: the sole reason an acceptance criterion is unmet exists only as prose inside a feature
folder that is archived at merge. The `mstest-coverage` required check will continue to fail
intermittently on unrelated pull requests through this mechanism, and the diagnosis will have to be
redone from scratch.

Required remediation: author
`docs/features/potential/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race.md` recording
the mechanism, the two racing classes, the observed signature, and the candidate fixes (a `lock`
around `LoadOpCodes`, a `Lazy<OpCode[]>` pair, static-constructor initialisation, or
`[DoNotParallelize]` on both classes as an interim stopgap), then promote it to a GitHub issue.

### F-2 — Non-blocking — Repository branch coverage is below the 75% floor

Repository branch-rate is 66.3978% against the >= 75% uniform threshold in
`.claude/rules/quality-tiers.md`. Recorded as **FAIL** because the rule states a hard floor and no
maintainer exemption is recorded in `issue.md` or `spec.md`.

Disposition: **non-blocking, no remediation demanded of this change.** The shortfall is
pre-existing (baseline 66.3058%), this change improved it by +0.092 percentage points, and closing
an 8.6-point repository-wide branch gap is not achievable inside a three-defect determinism bugfix.
Note also the unreconciled documentation conflict already known in this repository: CLAUDE.md UT2
states an 80% line floor with no branch floor, while `.claude/rules/general-unit-test.md` and
`.claude/rules/quality-tiers.md` state 85% line and 75% branch. The change clears the CLAUDE.md
floor and the rules-file line floor; only the rules-file branch floor is missed, and only
repository-wide.

### F-3 — Non-blocking — One test performs two Acts

`OlTableExtensions_Tests.EnumerateTable_WritesFormattedOutputToSuppliedWriterAndMovesToStart`
asserts on the supplied-writer path and then invokes the null-writer path in the same method, with
the in-code justification "this file is at its line ceiling". This is a documented, deliberate
trade against `.claude/rules/general-unit-test.md` isolation. It is acceptable given the
1846-line pre-existing violation the plan was required not to enlarge, but the cleaner resolution
is a small separate test file, which would also reduce the offending file. Recorded for follow-up,
not for remediation now.

### F-4 — Non-blocking — `MessageBoxInvoker` remains a mutated process-wide static

AC2's first clause is "`DfDeedle_COM_Tests` no longer mutates process-wide static seams in a way
another class can observe". The two ETL delegate statics are gone, but `DfDeedle.MessageBoxInvoker`
survives and is still swapped by four tests in `DfDeedle_COM_Tests` (lines 200-213, 225-238,
264-283, 320-336), each restoring in `finally`. `spec.md` settles this deliberately, and this
reviewer verified the exposure argument independently: the only production readers are
`DfDeedle.QfcColumns.cs:27,170,194`, and only three test classes reach those paths —
`DfDeedle_COM_Tests` itself (whose tests are serialised by `ClassLevel` scope),
`DfDeedleQfcColumnTimeoutTests` and `DfDeedleEtlTimeoutTests`, both `[DoNotParallelize]`. The
residual exposure therefore depends on the same MSTest ordering assumption that `spec.md` Risk 3
flags as documentation-sourced and not run-verified — the assumption AC3's seam work was
specifically intended to stop relying on. It is an unenforced invariant rather than an observed
defect; no test or analyzer prevents a future edit from removing `[DoNotParallelize]` from either
reader class.

### F-5 — Non-blocking — Canonical coverage artifact path and PR context artifacts absent

`artifacts/csharp/coverage.xml` does not exist; `artifacts/pr_context.summary.txt` and
`artifacts/pr_context.appendix.txt` do not exist. Under this reviewer's standing procedure the
absence of a canonical coverage artifact for a changed language is a FAIL, so it is recorded as
one, with a **non-blocking** disposition: the underlying Cobertura document
(`coverage/p7-final.cobertura.xml`) exists and was read directly by this reviewer, so every
coverage figure in section 5 is independently verified rather than merely transcribed. PR context
artifacts could not be regenerated because `git` invocation from a review agent hangs in this
checkout; the caller pre-generated an equivalent unified diff and a full 68-file diffstat, which
were used instead. Both deviations are procedural.

### F-6 — Non-blocking — AC4 literal condition not met

Recorded here for completeness; the substantive analysis is in `feature-audit.2026-09-08T11-30.md`.
AC4 requires zero failures on ten consecutive runs and 9 of 10 were clean. The criterion is
correctly left unchecked in `spec.md` and correctly reported as "not delivered" in the issue-update
artifact. No re-run-until-green was attempted, which is the correct discipline for a gate whose
entire purpose is to detect intermittency. This reviewer does not require the AC to be forced
green.

## 9. Summary of Changes

68 files changed, 4177 insertions, 351 deletions.

Production (`UtilitiesCS`, 8 files):

1. `Extensions/DictionaryExtensions.cs` — deleted the linked `CancellationTokenSource` and
   `CancelAfter(500)` from `TryAddValuesAsync`; the caller token now flows directly to `Task.Run`.
   Verified: `TryAddValuesAsync` has zero production call sites (the only invocations anywhere are
   two test methods), so the deletion cannot change production behavior. It also removes a per-call
   undisposed timer allocation.
2. `Extensions/DfDeedle.cs` — added trailing `TimeProvider? timeProvider = null` to
   `GetEmailDataInViewAsync` and forwarded it to `AddQfcColumnsAsync`, `EtlAsync` and the 1000 ms
   dataframe-transform deadline; added a null-snapshot guard throwing `InvalidOperationException`
   ahead of the `LogDfTiming` dereference; replaced the mutable `TableEtlInvoker` static with an
   optional `etl` parameter and a `private static readonly DefaultTableEtl`.
3. `Extensions/DfDeedle.FrameUtilities.cs` — replaced `StoreTableEtlInvoker` with an optional `etl`
   parameter on both `FromDefaultFolder` overloads, the `Stores` overload forwarding to the `Store`
   overload.
4. `OutlookObjects/Table/OlTableExtensions.Etl.cs` — added trailing `TimeProvider? timeProvider`
   to `EtlAsync` and `EtlByRowAsync`; converted three `TimeoutAfter(ms, attempts)` calls to
   `TimeoutAfter(ms, timeProvider)`; deleted the inert `attempts` local; corrected the timeout log
   text and removed its `DateTime.Now`.
5. `OutlookObjects/Filter DASL/DASLFilterParser.cs` — `PrintTree` gained `TextWriter? writer = null`,
   forwarded on recursion.
6. `HelperClasses/PrettyPrint.cs` — both `PrettyPrint` overloads gained `TextWriter? writer = null`
   at zero net line growth.
7. `OutlookObjects/Table/OlTableExtensions.TableAccess.cs` — `EnumerateTable` gained
   `TextWriter? writer = null` routing its three writes.
8. `ReusableTypeClasses/Other/StackGeek.cs` — extracted `GFG.Main`'s body into
   `public static void Run(TextWriter)`; `Main` now calls `Run(Console.Out)`.

Tests (`UtilitiesCS.Test`, 11 files) and one project file: three new files
(`DfDeedleEtlTimeoutTests.cs`, `OlTableExtensionsEtlClockTests.cs`,
`TestHelpers/ArmingBarrierTimeProvider.cs`), the `ArmingBarrierTimeProvider` move out of
`DfDeedleQfcColumnTimeoutTests.cs`, four capture-and-assert tests converted to test-owned writers,
`[DoNotParallelize]` removed from three classes and retained with a corrected rationale on the
fourth, the `NLogTraceWriter_Test` console save/restore removed, the `Returns(120)` timing
tolerance retired, and three `<Compile Include>` items added.

Documentation: 46 feature-folder and evidence artifacts, plus two `docs/features/potential/`
follow-up entries.

## 10. Compliance Verdict

| Policy | Verdict |
|---|---|
| CLAUDE.md standing instructions | PASS |
| `.claude/rules/general-code-change.md` | PASS |
| `.claude/rules/general-unit-test.md` | PARTIAL — repository branch coverage below floor (pre-existing, improved); one test with two Acts |
| `.claude/rules/csharp.md` | PASS |
| `.claude/rules/quality-tiers.md` | PARTIAL — line floor met, branch floor missed repository-wide (pre-existing, improved) |
| `.claude/rules/tonality.md` | PASS — all authored prose in the feature folder is factual, measured, and free of humor, hyperbole and decorative metaphor; deviations are stated plainly, including the sections the executor titled "stated honestly" |
| Evidence location invariant | PASS |
| Scope invariant (full branch diff vs base) | PASS |

**Overall: PASS with one blocking process finding (F-1).**

Blocking findings in this artifact: **1**.

## Appendix A: Test Inventory

New test classes and methods:

| File | Method | Purpose |
|---|---|---|
| `Extensions/DfDeedleEtlTimeoutTests.cs` | `GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder` | RED-first AC2 regression: deadline expiry must produce a folder-naming `InvalidOperationException`, not an `NullReferenceException` |
| `Extensions/DfDeedleEtlTimeoutTests.cs` | `GetEmailDataInViewAsync_ClockNeverAdvances_ReturnsOneRowFrame` | Deterministic green path on an un-advanced clock |
| `Extensions/DfDeedleEtlTimeoutTests.cs` | `GetEmailDataInView_NoEtlArgument_UsesProductionDefaultDelegate` | Covers the `etl ?? DefaultTableEtl` default branch left by the removed static |
| `OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs` | `EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource` | Documents the surviving swallow-and-cancel contract; covers the previously uncovered `catch (TimeoutException)` |
| `OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs` | `EtlAsync_ClockNeverAdvances_ReturnsTransformedRows` | Deterministic green row path |
| `OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs` | `EtlAsync_NoBinaryOrObjectFields_UsesGetArrayBranchOnControlledClock` | Covers the `GetArray` branch's `TimeoutAfter` |
| `Extensions/DictionaryExtensions_Tests.cs` | `TryAddValuesAsync_PreCancelledToken_ThrowsTaskCanceledAndLeavesValueUnchanged` | Locks the surviving AC1 cancellation contract |
| `HelperClasses/PrettyPrint_Tests.cs` | `PrettyPrint_NullWriter_WritesToConsoleWithoutThrowing` | Null-writer default of both overloads |
| `ReusableTypeClasses/StackGeek_Tests.cs` | `Run_WritesScenarioToSuppliedWriter` | The `TextWriter` seam, asserting on a test-owned writer |

Renamed or converted tests: `DataFramePrettyHelpers_RenderRowsMarkdownAndConsoleOutput` to
`...AndWriterOutput`; `PrintTree_WritesIndentedTreeToConsole` to `...ToSuppliedWriter`;
`EnumerateTable_WritesFormattedOutputAndMovesToStart` to `...ToSuppliedWriterAndMovesToStart`;
`Main_RunsSampleScenarioWithoutThrowing` retained and narrowed to the null-writer default.

New shared helper: `TestHelpers/ArmingBarrierTimeProvider.cs`, `internal sealed`, moved out of
`DfDeedleQfcColumnTimeoutTests` rather than duplicated.

## Appendix B: Toolchain Commands Reference

```
dotnet tool run csharpier format .
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe <nine assemblies> /EnableCodeCoverage /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook"
```

Template note: the MCP asset `mcp__drm-copilot__resolve_policy_audit_template_asset` is not
available in this agent session, so this artifact was hand-authored preserving the twelve canonical
major headings required by `.claude/skills/policy-audit-template-usage/SKILL.md`. The MCP validator
`mcp__drm-copilot__validate_orchestration_artifacts` is likewise unavailable and was not run.
