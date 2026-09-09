# Policy Audit — 2026-09-08-etl-deadline-mechanics-follow-ups (Issue #825)

- Component: UtilitiesCS / UtilitiesCS.Test (ETL deadline mechanics)
- Date: 2026-09-10T00-45
- Base commit: 96fd3dd86cff542226d192158f1c2f63d5eee926
- Head commit: 4c607bd9569f96516920bd7fe78fb0ab43a2c0f0 (per evidence/qa-gates/ac18-commit-language.md), plus one permitted residual plan check-off (1 file, +1/-1)
- Work mode: `full-bug` (from issue.md line 12). AC source is `spec.md` only. `user-story.md` is correctly absent and its absence is not reported as a gap.
- Policies applied, in order: CLAUDE.md; `.claude/rules/general-code-change.md`; `.claude/rules/general-unit-test.md`; `.claude/rules/quality-tiers.md`; `.claude/rules/tonality.md`.

## Tooling constraint governing this review (recorded explicitly)

The invoking agent directed that the **Bash tool not be used at all** in this review, because a
`git -C <path>` invocation from a feature-review agent has a recorded habit of hanging in this
repository and this was an unattended overnight run. This review therefore used only Read, Grep and
Glob.

Consequences, stated so the basis of each row is transparent:

- Every file-content fact below was read directly from the delivered tree at
  `C:/Users/DanMoisan/repos/TaskMaster-wt/rr0908-825` and is first-hand.
- Every **git-derived** fact — the committed footprint against the base, per-file numstat, the
  1011→966 line delta, and file LastWriteTime — is taken from the invoking agent's independent
  measurement and from the committed evidence artifacts, and was corroborated wherever a
  file-content check could corroborate it. Where corroboration was not possible without git, the row
  says so.
- This is a tooling constraint, not a scope narrowing. The audit scope remained the full branch diff
  against the resolved base.

## Rejected Scope Narrowing

None detected. The caller supplied the complete production diff, directed evaluation of all 35
acceptance criteria, and did not attempt to limit the audit to a plan, task, phase or file subset,
nor to mark any language's coverage as out of scope. No narrowing text is recorded here because none
was supplied.

## Evidence Location Compliance

All evidence produced by this feature is written under
`docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/<kind>/`, with kinds
`baseline`, `regression-testing`, `qa-gates`, `other` and `issue-updates`. Enumeration of the feature
folder returned 41 evidence artifacts and no file under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/coverage/` or `artifacts/evidence/`. The committed footprint (per the caller's
measurement, corroborated by `evidence/qa-gates/ac18-commit-language.md` reporting
`UnexpectedPaths: 0` over 59 paths) contains only the eleven `spec.md` Write Set paths and this
feature's own folder.

Verdict: **PASS**. No evidence-location violation. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` was
required.

## Executive Summary

The change is compliant with every policy examined. Its distinguishing property is that the feature
is entirely about timeouts, and it introduces **no new wall-clock dependency into test code**: every
new deadline is armed on an injected `TimeProvider` or neutralised by a never-cancelling source
supplied through the existing factory seam, and the one pre-existing wall-clock hazard in
`OlTableExtensions_Tests` was removed by the production change rather than tolerated with a
`[DoNotParallelize]` attribute.

Blocking findings: **0**.

Two pre-existing 500-line cap violations survive (`UtilitiesCS/Threading/TimeOutTask.cs` at 966
lines, `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` at approximately 1822
lines). Both were **reduced** by this feature, neither was introduced by it, and the feature is
explicit and correct in describing the TimeOutTask.cs change as a reduction and not a resolution. A
follow-up is recorded for TimeOutTask.cs but not for the test file; that omission is recorded as a
non-blocking finding.

## 1. General Unit Test Policy Compliance

Reference: `.claude/rules/general-unit-test.md`, CLAUDE.md § General Unit Test Policy.

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence | PASS | The new class declares no `[ClassInitialize]`, `[TestInitialize]`, `[ClassCleanup]`, `[TestCleanup]` or `[AssemblyInitialize]` and no mutable static field. `SignatureTypes` and `CreateArmingBarrier` are pure factories returning fresh instances per call. |
| Isolation | PASS | Each of the four new tests targets one behaviour of `GetTableInViewAsync`: retry deadline value, arming clock, factory precedence, default path. |
| Fast execution | PASS | Whole-assembly run of 4919 tests in 31.87 s (`evidence/regression-testing/phase7-green.md`). No new test waits on wall time. |
| Determinism — banned APIs | PASS | `GetTableInViewAsyncClockTests.cs` contains zero occurrences of `Thread.Sleep`, `Task.Delay`, `DateTime.Now` and `Stopwatch`, and no retry loop or timing tolerance. Verified by direct grep of the file. |
| Determinism — banned APIs, edited files | PASS | `DfDeedleEtlTimeoutTests.cs` returns zero matches for the same four tokens. `OlTableExtensions_Tests.cs` likewise. |
| Determinism — pre-existing exception, correctly distinguished | PASS | `Task.Delay(200)` (line 36) and `Task.Delay(50)` (line 49) in `UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs` are **pre-existing** and were not added by this feature; the feature added only the comment at lines 10-15 that names them as the verified reason that class retains `[DoNotParallelize]`. The file's numstat is +6/-27, a net deletion. Documenting a pre-existing race, rather than removing the guard that contains it, is the conservative and correct handling. |
| Determinism — controllable clock | PASS | `FakeTimeProvider` and `ArmingBarrierTimeProvider` are injected through the new trailing `TimeProvider?` parameter. No production code under test reads wall-clock time for a deadline on this path after the change. |
| No external dependencies | PASS | All fixtures are Moq objects (`Mock<Outlook.Explorer>`, `Mock<Outlook.TableView>`, `Mock<Outlook.Table>`). No live Outlook, no network, no database. The phase-7 run excludes `TestCategory=LiveOutlook`, which is mandatory under this policy rather than convenient. |
| No temporary files | PASS | No `Path.GetTempFileName`, `Path.GetTempPath` or file creation in any new or edited test. |
| Arrange–Act–Assert | PASS | All four new tests carry explicit `// Arrange`, `// Act`, `// Assert` markers. |
| Clear failure messages | PASS | Every `.Should()` in the new file carries a `because` reason string; for example `recordedTimeouts[1].Should().Be(750, "the retry must arm on the caller's timeoutMs rather than on a literal 2000")`. |
| Documented intent | PASS | Every test and every private helper in the new file carries an XML doc comment stating the scenario and the mechanism. |
| Test file location mirrors source | PASS | `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs` mirrors `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`. No colocation in the production tree. |
| Scenario completeness | PASS | Positive (default path, factory precedence), negative/expiry (retry after injected throw), edge (never-advancing clock), state transition (first attempt versus retry). The pre-cancelled-token negative case is pinned by the pre-existing test at `OlTableExtensions_Tests.cs` line 1296, retained. |
| Coverage exclusion policy — no production path excluded | PASS | Grep for `ExcludeFromCodeCoverage` across `UtilitiesCS/OutlookObjects/Table` returns zero. `evidence/qa-gates/ac33-coverage-comparison.md` records that `coverage.config`'s `ModulePaths Exclude` block names only third-party modules (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest) and no first-party production module. `coverage.config`, `.editorconfig` and `.globalconfig` are outside the committed footprint, so none was weakened. |

## 2. General Code Change Policy Compliance

Reference: `.claude/rules/general-code-change.md`, CLAUDE.md § General Code Change Policy.

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | The whole of item 2 is one trailing optional parameter plus one resolved local. No new type, no new interface, no indirection layer. |
| Reusability | PASS | The deadline source is resolved **once** into `resolvedTimeoutSourceFactory` (TableAccess.cs lines 63-70) and reused by the primary call and, through re-resolution on the same inputs, by both retries. No copy-paste of the resolution expression. |
| Extensibility | PASS | Every added parameter is optional and trailing, so all callers binding by ordinary overload resolution stay source-compatible. The two exceptions — reflection binders and deleted members — are enumerated in spec.md and were both handled deliberately. |
| Separation of concerns | PASS | The clock is injected rather than read; deadline policy stays in `GetTableInViewAsync` and deadline mechanism stays in `TimeOutTask.RunWithTimeout`, whose signature is unchanged. |
| Fail fast, no silent error swallowing | PASS | No catch was widened. `catch (TimeoutException)` in `EtlAsync` (Etl.cs 129-135) still swallows and cancels — a deliberately pinned pre-existing contract, asserted by `EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource`. The failure is named at the consumer by the retained `InvalidOperationException` guard (DfDeedle.cs 186-193). `catch (System.Exception e)` in `RunWithTimeout` (TimeOutTask.cs 85-92) is neither narrowed nor widened. |
| Logging pattern | PASS | No `logger` level, message or call site changed. The one `logger.Warn` that disappears is the unreachable "attempts remaining" line inside a deleted inert overload, so no runtime log output changes. |
| **File size limit (500 lines)** | **PARTIAL — non-blocking, pre-existing, reduced** | See the dedicated section below. |
| Naming | PASS | `resolvedTimeoutSourceFactory`, `recordingFactory`, `acquisitionGate`, `gateAcquire`, `SignatureTypes`, `InvokeGetTableInViewAsync` are all descriptive. Test names state condition and expectation. |
| Comment why, not what | PASS | Every added comment states a reason: why a null provider is safe (TableAccess.cs 32-35), why `CancelAfter` must not be introduced on the provider-created source (same block), why an explicit factory still wins (60-62), why the caller's value is now propagated (118-121), why the 250 ms constant is unchanged (Etl.cs 84-93), why reflection is required (test file 74-80), why the gate is released inside the try (test file 194-196). |
| Dependencies | PASS | No new package. `Microsoft.Bcl.TimeProvider` 10.0.11 and `Microsoft.Extensions.TimeProvider.Testing` were already referenced. |
| I/O boundaries | PASS | No I/O introduced. |
| Public API breaking changes | PASS | `EtlAsync`'s first tuple element widens from `object[,]` to `object[,]?`. All five consumers were enumerated and checked: one production (DfDeedle.cs 176, nullable-enabled, guarded) and four test. See section 3 for the nullable-context analysis. |
| Mandatory toolchain loop | PASS | Recorded in section 7. |

### File size limit — detail

| File | Baseline | Head | Cap | Verdict |
|---|---|---|---|---|
| `UtilitiesCS/Threading/TimeOutTask.cs` | 1011 | 966 | 500 | Over cap. Reduced by 45. Pre-existing. |
| `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` | 1846 | ~1822 (numstat +15/-39) | 500 | Over cap. Reduced by 24. Pre-existing. |
| `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs` | n/a (new) | 289 | 500 | PASS |
| `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs` | ~228 | 263 | 500 | PASS |
| `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs` | 234 | 234 | 500 | PASS |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` | ~437 | 452 | 500 | PASS |

The head figure of 966 for TimeOutTask.cs was read directly (the file's last content line is 966).
It **contradicts spec.md**, which states 968 at line 179 and line 928 and a 43-line reduction at line
430. `issue.md` and `evidence/other/file-size-accounting.md` both carry the correct 966 and a 45-line
reduction. The 968 is an authoring-time estimate that no acceptance criterion turns on. Recorded as
non-blocking finding NB-3.

The requirement in AC18 — that nothing claims the cap violation is *resolved* — is satisfied. The
accounting artifact states `CapStillViolated: true` and "This is a reduction, not a resolution".
`evidence/qa-gates/ac18-commit-language.md` records `SoakLinesInMessage: 0` and
`Resolv500LinesInMessage: 0` and quotes the commit message's own "reduction ... not a resolution"
wording. I independently swept every source comment added by this feature and every artifact in the
feature folder and found no contrary claim.

## 3. Language-Specific Code Change Policy Compliance (C#)

Reference: CLAUDE.md § C# Code Change Policy.

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting, pinned version, via `dotnet tool run` | PASS | `evidence/baseline/csharpier-check.md` (1622 files, zero drift) and `evidence/qa-gates/qc-csharpier-check.md` (1623 files, the new file included). `evidence/baseline/dotnet-tool-restore.md` pins CSharpier 1.2.6 from the manifest. |
| No `dotnet format` | PASS | No `.csproj` was rewritten. `UtilitiesCS.Test.csproj` numstat is +1/-0, and `UtilitiesCS/UtilitiesCS.csproj` is absent from the footprint entirely. |
| .NET analyzers, `/t:Rebuild`, `EnableNETAnalyzers` + `EnforceCodeStyleInBuild` | PASS | `evidence/qa-gates/qc-build-analyzers.md`, exit 0, 0 warnings, 0 errors, `/t:Rebuild`. Non-vacuity proven at `evidence/qa-gates/ac32-non-vacuity.md`: `SkippingCoreCompileCount: 0` **together with** `CscInvocationsForWriteSetProjects: 4`. The second figure is what makes the first non-vacuous, and the artifact says so explicitly. |
| No analyzer severity weakened | PASS | `.editorconfig`, `.globalconfig` and `BannedSymbols.txt` are all absent from the committed footprint. No `#pragma warning disable` and no `[SuppressMessage]` appear in any added line of the diff. |
| Nullable type-check, `/t:Rebuild`, `TreatWarningsAsErrors`, no `/p:Nullable=enable` | PASS | `evidence/qa-gates/qc-build-nullable.md`, exit 0, 0 warnings, 0 errors. The command form matches the CI-parity form mandated by CLAUDE.md and correctly omits `/p:Nullable=enable`. |
| Nullable propagation of `object[,]?` to every consumer | PASS | Five consumers, all checked. **Production:** `DfDeedle.cs` carries `#nullable enable` at line 23; the retained guard at 186-193 throws before any dereference, so the flow state at 197 and 214 is non-null and no CS8602 arises. **Tests:** `OlTableExtensionsEtlClockTests.cs` and `DfDeedleEtlTimeoutTests.cs` carry no `#nullable` directive at all, and `OlTableExtensions_Tests.cs` enables only `#nullable enable annotations` in five narrow scoped regions, none of which covers its `EtlAsync` call site at line 969. Every test consumer is therefore in a warning-disabled nullable context and the widened element produces no diagnostic. This is why the gate reports 0 warnings, and it is a correct outcome under the repository's per-file opt-in model rather than a suppressed one. |
| Strong contracts, explicit types at public boundaries | PASS | `Func<int, CancellationTokenSource> resolvedTimeoutSourceFactory` is explicitly typed. `TimeProvider? timeProvider = null` is explicit. |
| Async / resource safety | PASS | The provider-created `CancellationTokenSource` is consumed by `RunWithTimeout` under `using var timeoutSource` (TimeOutTask.cs 52-54), so it is disposed on every path including the retry recursion. The pre-.NET 8 `CancelAfter` caveat is recorded in a comment at the creation site (TableAccess.cs 33-35) as spec.md Risks requires, and no `CancelAfter` call was introduced. |
| MSTest / Moq / FluentAssertions | PASS | New file uses `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`, `Moq` for all fixtures, and FluentAssertions for every assertion. No xUnit or NUnit introduced. |
| Legacy project compile-item discipline | PASS | Exactly one `<Compile Include="OutlookObjects\Table\GetTableInViewAsyncClockTests.cs" />` at csproj line 549, inserted in alphabetical neighbourhood; numstat +1/-0 proves no reorder or reformat, which is what the fan-in merge depends on. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest only | PASS | No xUnit/NUnit reference added. |
| Moq for mocks | PASS | All four new tests. |
| FluentAssertions preferred | PASS | Every assertion in the new file is FluentAssertions; no MSTest `Assert` fallback was needed. |
| `[DoNotParallelize]` present only with a documented, verified reason | PASS | Three classes retain it, each with a stated reason: the new `GetTableInViewAsyncClockTests` ("drives a real Task.Run gate"), `OlTableExtensionsEtlClockTests` (line 20), `DfDeedleEtlTimeoutTests` (lines 22-23), and `TimeOutTask_Tests` now carries the verified wall-clock-race reason at lines 10-15. `OlTableExtensions_Tests` had its attribute removed **after** the hazard was removed by the production change, in the order spec.md Risks mandates. |
| No stabilisation by timing tolerance or repeated runs | PASS | `evidence/other/ac21-justification.md` records `RunsObserved: 0` and `Justification: item 2 change`, and `evidence/regression-testing/phase7-green.md` explicitly disclaims being the justification. This is the correct discipline: the removal rests on a code change that eliminates the hazard, not on observation that it did not fire. |

## 5. Test Coverage Detail

Coverage artifacts: `evidence/baseline/coverage-baseline.cobertura.xml` and
`evidence/qa-gates/coverage-postchange.cobertura.xml`, both processed (test packages stripped by the
first-party allowlist before any figure was read).

| Language | Changed files on this branch | Coverage figure | Verdict |
|---|---|---|---|
| C# line coverage: repo-wide production denominator 56029/65402 = 85.6686 percent, threshold 85 percent — **PASS** | 11 | 85.6686 percent | PASS |
| C# branch coverage: repo-wide 13486/16892 = 79.8366 percent, threshold 75 percent — **PASS** | 11 | 79.8366 percent | PASS |
| C# new-and-changed-code line coverage: 20/22 = 90.9091 percent, threshold 90 percent (CLAUDE.md) and 85 percent (`.claude/rules`) — **PASS** | 11 | 90.9091 percent | PASS |
| PowerShell coverage: zero changed `.ps1` files on this branch, so no Pester coverage artifact is required or expected — **PASS** | 0 | not measured, none required | PASS |
| Python coverage: zero changed `.py` files on this branch — **PASS** | 0 | not measured, none required | PASS |
| TypeScript coverage: zero changed `.ts` files on this branch — **PASS** | 0 | not measured, none required | PASS |

### The four decided gates (UtilitiesCS package)

| Gate | Content | Verdict |
|---|---|---|
| A | `LinesValid` 43287 → 43246, a reduction of 41 | PASS |
| B | `LinesCovered` 39010 ≥ 39014 − 41 = 38973, exceeding the floor by 37 | PASS |
| C | `BranchesValid` unchanged at 11161; `BranchesCovered` 9363 → 9368 | PASS |
| D | No file this feature did not shrink has a negative signed `LinesCovered` delta | PASS |

### Deletion reconciliation (independently re-checked)

DeletionSum 55 (32 from the two inert `TimeoutAfter` overloads in TimeOutTask.cs, 23 from
`EtlAsyncOld` in Etl.cs, 0 from the three deleted tests, which live in a package stripped before
measurement) minus AdditionSum 14 (10 in TableAccess.cs, 4 in DfDeedle.cs, 0 in DfDeedle.QfcColumns.cs)
= **41**, which equals the Gate A reduction of 41 exactly. **Residual 0.** The arithmetic closes.

I re-performed this arithmetic and confirm it. I also confirm the reconciliation is the right shape:
reconciling the deletion sum alone against the total could not close, because this feature both adds
and deletes measurable lines, and the artifact says so.

### The two uncovered changed lines

Post-change TableAccess.cs lines 103 and 104, the argument lines `timeoutSourceFactory,` and
`timeProvider` inside the `TaskCanceledException` retry recursion. The artifact attributes this to
line-level instrumentation placing the sequence point on the call's first argument line after
CSharpier wrapped the expression. I accept that explanation: the enclosing statement is covered, and
this is a measurement artefact of a wrapped call rather than an untested branch. It costs 2 of 22
changed lines and still leaves the figure above the 90 percent floor.

Repo-wide raw rates are reported informationally only and correctly are **not** used as a
no-regression gate. Both raw rates in fact rose (line 0.856211 → 0.856686; branch 0.79807 →
0.798366), but the artifact is explicit that this is an outcome and not the gate, which is the
correct treatment when covered code is deleted.

## 6. Test Execution Metrics

| Run | Command scope | Result | Artifact |
|---|---|---|---|
| Fail-before, AC7 | Single test by `/TestCaseFilter`, against the pre-change production file | EXIT 1, 0 passed, 1 failed, with the message "found 2000 (difference of 1250)" | `evidence/regression-testing/ac7-fail-before.md` |
| Phase 3 green | New tests plus the three DfDeedle-path tests | Green | `evidence/regression-testing/phase3-green.md` |
| Phase 4 green | After deleting the inert overloads and dead method | Green | `evidence/regression-testing/phase4-green.md` |
| Phase 5 green | After the nullable tuple contract change | Green | `evidence/regression-testing/phase5-green.md` |
| Phase 7 green | Whole assembly, `TestCategory!=LiveOutlook` | 4919 total, 4919 passed, 0 failed, 31.87 s | `evidence/regression-testing/phase7-green.md` |

The RED-first discipline is genuinely proven, not asserted. The fail-before record shows the count
half of the assertion passing (the factory recorded exactly two invocations, proving the retry branch
was entered) while only the **value** half failed on 2000 versus 750. That distinguishes a real
defect-encoding failure from a test that failed for an unrelated reason, which is the property a
fail-before record has to establish.

## 7. Code Quality Checks

| Stage | Command | Result | Artifact |
|---|---|---|---|
| Restore | `nuget restore` | `PackagesDirectoryPresent: true` | `evidence/baseline/restore.md` |
| Tool restore | `dotnet tool restore` | CSharpier 1.2.6 pinned | `evidence/baseline/dotnet-tool-restore.md` |
| Format | `dotnet tool run csharpier format .` | Applied; restarted the phase once as the loop requires | `evidence/qa-gates/qc-csharpier-format.md` |
| Format check | `dotnet tool run csharpier check .` | 1623 files, clean | `evidence/qa-gates/qc-csharpier-check.md` |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT 0, 0 warnings, 0 errors | `evidence/qa-gates/qc-build-analyzers.md` + `.txt` |
| Nullable | `msbuild TaskMaster.sln /t:Rebuild /m ... /p:TreatWarningsAsErrors=true` | EXIT 0, 0 warnings, 0 errors | `evidence/qa-gates/qc-build-nullable.md` + `.txt` |
| Test | `vstest.console.exe ... /InIsolation` | 4919/4919 | `evidence/regression-testing/phase7-green.md` |

Both MSBuild gates use `/t:Rebuild`, which is the form CLAUDE.md mandates precisely because a warm
`/t:Build` returns exit 0 with `CoreCompile` skipped. Non-vacuity is proven positively rather than by
the absence of a skip message.

### Log hygiene

All five committed MSBuild logs use a `.txt` extension (a `.log` would be gitignored and never
committed) and each contains **zero** lines carrying the token `C:\Users\`. The caller independently
confirmed zero occurrences of that token anywhere in the feature folder. The three-rewrite
sanitisation — worktree root, main checkout root, user profile root, applied in that order because
the shorter string is a prefix of the longer — is correct and the third rewrite is a recorded
deviation (D1) rather than an undisclosed change.

## 8. Gaps and Exceptions

All findings in this section are **non-blocking**.

- **NB-1 — `TimeOutTask.cs` remains 466 lines over the 500-line cap.** Pre-existing; reduced by 45
  this feature; correctly described as a reduction. Splitting it requires a new production compile
  item in `UtilitiesCS/UtilitiesCS.csproj`, which is deliberately outside the Write Set. Recorded as
  deferred follow-up 2 in `evidence/other/ac35-reachability-observation.md`. Disposition: accept,
  and hold the epic to filing the follow-up.
- **NB-2 — `OlTableExtensions_Tests.cs` remains approximately 1322 lines over the cap.** Pre-existing;
  reduced by 24 this feature. The repository's file-size rule applies to test code identically. No
  deferred follow-up currently names this file; the four recorded follow-ups do not cover it.
  Disposition: accept for this feature, recommend the epic add a fifth follow-up to split it.
- **NB-3 — `spec.md` states the post-change TimeOutTask.cs size as 968 lines (lines 179 and 928) and
  the reduction as 43 lines (line 430).** Measured head value is 966 and the reduction is 45.
  `issue.md` and `evidence/other/file-size-accounting.md` carry the correct figures. This is a stale
  authoring-time estimate in the AC source document; no acceptance criterion turns on it and no claim
  of resolution is affected. Disposition: accept, correct at next spec touch.
- **NB-9 — Bash was not used in this review.** Git-derived rows (footprint, numstat, the 1011→966
  delta, LastWriteTime) rest on the caller's independent measurement plus committed evidence rather
  than on my own git invocation. Every such row is corroborated by a file-content read where one
  exists. Disposition: recorded; the constraint was a deliberate hang-avoidance measure for an
  unattended run.

## 9. Summary of Changes

Production (5 files):

- `OlTableExtensions.TableAccess.cs` — trailing optional `TimeProvider? timeProvider = null`; deadline
  source resolved once with an explicitly supplied factory still winning; the `TimeoutException` retry
  now propagates the caller's `timeoutMs` instead of a literal 2000; both recursions forward
  `timeProvider`. Both `Console.WriteLine` diagnostics, the `counter` parameter and both `catch`
  blocks survive byte-identical.
- `OlTableExtensions.Etl.cs` — `EtlAsync`'s first tuple element widened to `object[,]?`; the
  null-forgiving suppression at the return deleted; a ten-line comment recording why the 250 ms
  per-row budget is unchanged and how it would be measured; `EtlAsyncOld` deleted.
- `TimeOutTask.cs` — the two inert `(int, int)` `TimeoutAfter` overloads deleted (45 lines).
- `DfDeedle.cs` — forwards `timeProvider` to `GetTableInViewAsync`; consumer comment and two
  `Item1` references updated to `data`.
- `DfDeedle.QfcColumns.cs` — one stale doc-comment name corrected.

Test (6 files): one new file with four regression tests; four reflective binding sites updated; one
bounded timer-ordering update; three tests deleted with the code they covered; two class comments
corrected; one `[DoNotParallelize]` removed and one documented; one compile item added.

## 10. Compliance Verdict

**PASS.**

| Policy | Verdict |
|---|---|
| CLAUDE.md standing instructions | PASS |
| `.claude/rules/general-code-change.md` | PASS with two pre-existing, reduced, non-blocking file-size violations |
| `.claude/rules/general-unit-test.md` | PASS |
| `.claude/rules/quality-tiers.md` (uniform 85/75) | PASS |
| `.claude/rules/tonality.md` | PASS |

Tonality: I swept every artifact in the feature folder and every source comment added by this
feature. All are factual, measured and neutral. Claims are matched to their evidence — the coverage
artifact distinguishes derived from read figures and says which; the reachability artifact separates
what was traced from what was not; the AC21 justification states `RunsObserved: 0` rather than
implying a soak. No hyperbole, no humour, no decorative metaphor, and no claim of certainty beyond
the evidence was found. Where a claim is weaker than it might appear, the artifact says so — for
example, `ac33-changed-line-coverage.md` labels the two uncovered lines a measurement artefact and
explains the mechanism instead of asserting full coverage.

Blocking findings: **0**. Remediation inputs are **not** required and no
`remediation-inputs.<timestamp>.md` was produced.

## Appendix A: Test Inventory

New, in `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs` (289 lines,
`[DoNotParallelize]`):

1. `GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000`
2. `GetTableInViewAsync_InjectedClock_ArmsAcquisitionDeadlineOnInjectedProvider`
3. `GetTableInViewAsync_ExplicitFactorySupplied_TakesPrecedenceOverTimeProvider`
4. `GetTableInViewAsync_NoTimeProviderSupplied_UsesSystemClockAndCompletes`

Deleted (with the code they covered):

- `OlTableExtensions_Tests.EtlAsyncOld_WithBinaryAndObjectFields_ReturnsTransformedData`
- `TimeOutTask_Tests.TimeoutAfter_GenericTask_WithRepeatAttempts_ReturnsResult`
- `TimeOutTask_Tests.TimeoutAfter_NonGenericTask_WithRepeatAttempts_CompletesSuccessfully`

Updated in place: four reflective binding sites in `OlTableExtensions_Tests.cs` (lines ~1215, 1273,
1306, 1637) each gained `typeof(TimeProvider)` and a corresponding argument; the site at 1637 also
gained `new FakeTimeProvider()`. One doc comment in `OlTableExtensionsEtlClockTests.cs`. One bounded
timer-ordering update in `DfDeedleEtlTimeoutTests.cs`.

`OlTableExtensions_Tests.cs` retains 82 `[TestMethod]` declarations and gained none.

## Appendix B: Toolchain Commands Reference

```
nuget restore TaskMaster.sln
dotnet tool restore
dotnet tool run csharpier format .
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /InIsolation
```

The AC8 proof build deliberately scoped to `UtilitiesCS/UtilitiesCS.csproj` and omitted
`"/p:Platform=Any CPU"`, with the reason recorded: that project defaults `$(Platform)` to `AnyCPU`
and conditions its Debug property group on `Debug|AnyCPU`, so the solution-form platform name would
match no property group and fail before compilation, producing no `CS1061` and therefore no
refutation. That reasoning is correct and the full-solution gates do use the CI-parity form.

Note on template provenance: `.claude/skills/policy-audit-template-usage/SKILL.md` requires the
template be resolved through `mcp__drm-copilot__resolve_policy_audit_template_asset`. That MCP tool
is not available in this agent's tool set. This artifact was hand-authored preserving all twelve
canonical major headings rather than being marked BLOCKED, which is the established fallback in this
repository.
