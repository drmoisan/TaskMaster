# Policy Compliance Audit — Issue #797 (Folder Settings never persist; User Email "Error Loading")

- Component: TaskMaster VSTO add-in — store settings dialog, Outlook store wrapper, and the shared `SmartSerializable<T>` serializer
- Date: 2026-09-07
- Timestamp label: 2026-09-07T22-40
- Work Mode: `full-bug` (marker read from `issue.md` line 12)
- Authoritative acceptance-criteria source: `spec.md` in this feature folder (per the acceptance-criteria-tracking skill, `full-bug` resolves to `spec.md` only). `user-story.md` is correctly absent; its absence is not a finding.
- Base branch / base commit: `origin/main` at `c431dc3297e864041d829e8d79b348960b8d8019` (recorded in `spec.md` line 6 and in `evidence/baseline/phase0-base-sha.2026-09-06T22-00.md`)
- Branch: `bug/folder-settings-never-persist-797`
- Reviewer inputs: the pre-generated branch patch `artifacts/797-source-review.patch` (82274 bytes, `git diff origin/main HEAD` restricted to `*.cs` and `*.csproj`), the feature folder documents, the committed evidence tree, and the two session Cobertura documents read directly from disk.

## Template provenance

The `policy-audit-template-usage` skill names the MCP tool `mcp__drm-copilot__resolve_policy_audit_template_asset` as the required template source. No MCP tool is exposed in this agent session, so the asset could not be resolved. This artifact is hand-authored preserving all twelve canonical major headings the skill enumerates, with the template instruction block omitted as the skill requires. The companion validator `mcp__drm-copilot__validate_orchestration_artifacts` is likewise unavailable and was not run; that is recorded rather than claimed as passed.

---

## Rejected Scope Narrowing

The caller supplied the branch diff in a form restricted by file type. The relevant caller text, verbatim:

> THE DIFF, PRE-GENERATED FOR YOU:
> C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a46162b1321fb4a50/artifacts/797-source-review.patch
> That file is `git diff origin/main HEAD` restricted to `*.cs` and `*.csproj`. It is 82274 bytes and is the complete and authoritative source footprint of this change against `main`.

and

> Do not run git. Do not run gh. Do not attempt to compute a diff yourself.

Justification for recording: restricting the supplied diff to two file extensions is a subset-of-changed-files restriction, which the scope invariant requires be recorded rather than silently accepted. The audit scope remains the full branch diff against the resolved base branch.

Disposition. The narrowing was not accepted as a scope limit, but the accompanying prohibition on running `git` removes the means by which this reviewer would independently enumerate changes outside the `*.cs` / `*.csproj` pathspec. The residual was therefore bounded by three independent, non-git means rather than left open:

1. The executor's own scope gate at `evidence/qa-gates/p5-t9-scope.2026-09-06T22-00.md` enumerates fifteen `.cs` / `.csproj` paths from an anchored diff **and** a porcelain listing taken with `--untracked-files=all`. That listing is an exact set match with the fifteen file diffs present in the supplied patch, confirming the patch is neither truncated nor filtered beyond its stated pathspec.
2. That same gate separately confirms zero working-set paths under the `.claude`, `.codex`, `.agents` and `config` trees, zero GitHub workflow files, zero repository-root files, and zero files with a `resx`, `config`, `props` or `targets` extension.
3. The terminal porcelain listing at `evidence/qa-gates/p6-t11-ac-status.2026-09-06T22-00.md` lines 92-98 shows only five `.claude/agent-memory` Markdown residuals, all of which predate execution and none of which was committed by this change.

The only files this change writes outside the supplied pathspec are therefore Markdown documents inside this feature folder: `spec.md` and `issue.md` (acceptance-criteria check-off only), the plan file (task check-off), and the timestamp-named evidence artifacts. None is a coverage language and none is a policy document under `.claude/rules/` or `.github/instructions/`.

Residual limitation, stated plainly: the base-branch resolution and the head commit identity were taken from the caller and from the executor's Phase 0 artifact and could not be recomputed here, because the directive prohibits running `git`. This is the single scope fact in this audit that rests on a supplied value rather than on direct measurement.

---

## Evidence Location Compliance

The evidence-location invariant requires agent-produced evidence to live under `<FEATURE>/evidence/<kind>/`.

- Files in the branch diff written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`: **zero**. Verified by inspecting every one of the fifteen file paths in `artifacts/797-source-review.patch`; none is under `artifacts/`.
- All twenty-eight committed evidence artifacts for this work item are under `<FEATURE>/evidence/` in the canonical kind subdirectories `baseline` (13), `regression-testing` (8), `qa-gates` (11), `issue-updates` (1) and `other` (1). Enumerated by directory listing.
- `spec.md` lines 585-591 states the same requirement and the delivery matches it.
- `validate_evidence_locations.py --root .` was not run, because the directive prohibits invoking the shell in this worktree. The scan was performed by direct path inspection of the diff and of the evidence tree instead, and found no violation. This substitution is recorded, not concealed.

Observation, not a violation. The raw Cobertura documents that back the coverage figures live at `coverage/plan797-baseline/coverage.cobertura.xml` and `coverage/plan797-final/coverage.cobertura.xml`. That directory is git-ignored, so those documents are not committed and a later reviewer cannot re-verify the figures without rerunning the measurement. This reviewer read both documents directly during this audit and independently confirmed every headline figure (see section 5). Two additional points weigh in favour of leaving them uncommitted: they carry absolute host paths including an account name in every `filename` attribute, and committing them would leave large blobs reachable in history.

---

## Executive Summary

The change is compliant. Every mandatory gate in the policy compliance order was executed in the required order, produced exit code 0 on a single clean final pass, and is evidenced. Seven of the eight acceptance criteria are delivered and verified by named automated tests; the eighth (AC3) requires a live Outlook restart, was honestly recorded as blocked with all nine procedure steps marked NOT PERFORMED, and its checkbox was correctly left unmarked in both requirement files.

The verdict is **PASS** with **zero blocking findings**. Six advisory findings are recorded in the companion code review; none of them alters an acceptance-criteria verdict and none requires remediation before merge. No remediation-inputs artifact is produced.

Two conditions are pre-existing rather than introduced and are reported as such: the shared serializer remains over the 500-line file cap (613 lines at baseline, 658 after this change), and the two-assembly-scoped repository line coverage sits well below both the 80 percent and the 85 percent documented floors, at 53.23 percent before this change and 53.26 percent after it.

| Section | Verdict |
|---|---|
| 1. General Unit Test Policy | PASS |
| 2. General Code Change Policy | PASS with one recorded pre-existing exception (file size) |
| 3. C# Code Change Policy | PASS |
| 4. C# Unit Test Policy | PASS |
| 5. Test Coverage Detail | PASS |
| 6. Test Execution Metrics | PASS |
| 7. Code Quality Checks | PASS |
| 10. Compliance Verdict | **PASS — 0 blocking findings** |

---

## 1. General Unit Test Policy Compliance

Reference: `.claude/rules/general-unit-test.md`.

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence — tests run in any order | PASS | Both new test classes and both modified controller test classes carry `[DoNotParallelize]` (`SmartSerializableSerializeGuardTests.cs` line 815 of the patch; `StoreWrapperControllerTests.cs` line 23; `StoreWrapperController_Tests.cs` line 14; `StoreWrapperTests.cs` line 14). Every log4net appender attachment is undone in a `finally` block through the `restore` delegate returned by `AttachRootMemoryAppender` and `AttachControllerMemoryAppender`, which also restores the previous logger level and the repository `Configured` flag. |
| Isolation — one unit per test | PASS | Each of the 25 new tests exercises a single member. The six `TrimStorePrefix_*` cases are pure-function cases over one static helper; the four `GetSmtpAddressFromStore_*` cases each pin one step of the fallback order. |
| Fast execution | PASS | 5262 tests total in the final scoped run; no test in the diff performs I/O, sleeps, or waits on a clock. The deferred timer is driven by `ManualFireTimerWrapper.FireElapsed()`, not by elapsed wall time. |
| Determinism | PASS | No `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, or real wall-clock wait appears anywhere in the patch. Verified by pattern search over `artifacts/797-source-review.patch`: zero matches for `Thread\.Sleep|Task\.Delay|DateTime\.Now`. |
| Readability and maintainability | PASS | Every new test carries an Arrange comment naming the issue and the criterion it serves, and every non-trivial FluentAssertions call supplies a `because` reason string. |
| **No temporary files in tests** | PASS | Zero matches for `Path.GetTempPath` or `GetTempFileName` in the patch. Both new serializer tests write through the injected `CreateStreamWriter` seam into a `MemoryStream` (`SmartSerializableSerializeGuardTests.cs`, `harness.SetCreateStreamWriter(...)`), and the two fake paths `X:\FakeAppData\TaskMaster\StoresWrapper.json` and `X:\FakeAppData\TaskMaster\GuardProbe.json` are never opened. |
| No external dependencies; mocks used at boundaries | PASS | Every Outlook COM object is a `Mock<T>` over the interop interface. No live Outlook process, no network, no database, no filesystem. |
| Scenario completeness (positive, negative, edge, error) | PASS | AC1 has a positive and a key-absent negative case. AC2 has both the empty-string and the null path. AC4 has the explicit-save case and the deferred-path pin. AC6 has four ordered fallback cases plus a null-root-folder safety case. AC7 has six boundary cases including null, empty and prefix-only. AC8 has both the populate path and the helper path. |
| Coverage tooling excludes test files | PASS | The Cobertura packages are the production assemblies; no test assembly appears as a measured package. |
| **Coverage Exclusion Policy — no production file excluded from measurement** | PASS | Zero occurrences of `ExcludeFromCodeCoverage` in the entire patch, verified by pattern search. The two new production files are both present in the post-change Cobertura document: `StoreWrapperController.Display.cs` appears as a measured `class` element with `line-rate="1"`, and `IJunkFolderSelectionSink.cs` legitimately emits no `class` element because an interface declaration produces no IL. No `exclude` entry matching a production source path was introduced. |
| Test file location mirrors production structure | PASS | `UtilitiesCS.Test/OutlookObjects/Store/` mirrors `UtilitiesCS/OutlookObjects/Store/`; `TaskMaster.Test/AppGlobals/` mirrors `TaskMaster/AppGlobals/`. No test file was placed in a production source tree. |
| Determinism infrastructure — no banned APIs | PASS | The deferred write is advanced by a manual-fire timer double injected through the existing `TimerFactory` seam, which is the repository's established fake-timer facility for this class. |

Minor observation, not a finding. `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs` sits one directory level shallower than a strict mirror of `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs` would place it. It matches the established local convention for this class's sibling test files — `SmartSerializableLoader_Tests.cs`, `SmartSerializableNonTyped_Tests.cs` and `SmartSerializableStatic_Tests.cs` are all in the same directory — so the General Code Change Policy's "match the existing style" rule is the governing one and is satisfied.

---

## 2. General Code Change Policy Compliance

Reference: `.claude/rules/general-code-change.md` and the CLAUDE.md embedded copy.

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | Each of the four root-cause fixes is the smallest expression of its remedy: one `CopyFrom` call for AC1, one shared guard method for AC2, one new public entry point for AC4, one interface plus one cast for AC5. |
| Reusability — no copy-paste | PASS | `TryGetSerializationPath` is factored once and consumed by both `Serialize()` and `SerializeNow()`, so the two entry points cannot diverge in their diagnostic. `TrimStorePrefix` is factored once and consumed by both label assignments. |
| Extensibility — public APIs extended, not broken | PASS | `SerializeNow()` is additive. `Serialize()` keeps its signature and its deferred behaviour for every existing caller, pinned by `Serialize_WithConfiguredPath_StillRequiresTimerFireToWrite`. `IJunkFolderSelectionSink` is a new interface, deliberately not added to `IOlObjects`, so no existing implementer or test stub is forced to change. |
| Separation of concerns | PASS | `TrimStorePrefix` is a pure static helper with no dependency on controller state. `BuildUserEmailUnavailableText` reads one property and formats a string. The COM-bound lookup stays in `StoreWrapper`; the rendering stays in the controller display partial. |
| Fail fast and explicitly; no silent error swallowing | PASS | This is the substance of the change. Two silent paths were removed: the serializer's empty-path early return now logs at error level, and the controller's reflection miss, previously a `logger.Warn`, is now a `logger.Error`. |
| Logging uses the project pattern | PASS | Every new diagnostic goes through the existing static log4net `logger` field on the owning type. No `Console.WriteLine` or ad-hoc output was introduced. |
| Comment **why**, not what | PASS | Every substantive edit carries a `// why: issue #797 AC<n>.` comment naming the defect mechanism. Examples: `AppOlObjects.StoreLoading.cs` lines 70-75, `SmartSerializable.cs` lines 487-494, `StoreWrapperController.Display.cs` lines 41-47. |
| Cohesive modules; small public surface | PASS | The AC5 seam is implemented **explicitly** (`void IJunkFolderSelectionSink.ApplyJunkFolderSelections(...)` at `AppOlObjects.JunkFolders.cs` line 54), so the public surface of `AppOlObjects` does not widen and the existing internal method keeps its accessibility. `TrimStorePrefix` is `internal`, not `public`. |
| No new external dependency | PASS | The patch adds no package reference. The `log4net` usings added to two test files resolve against a reference the test project already carries. |
| I/O isolated; domain logic testable without disk or network | PASS | All five new or changed production members are reachable through pre-existing injectable seams; no new production seam was required. |
| Treat existing tests as part of the spec | PASS | One expectation was changed, and it was declared in advance. See "Declared expectation change" below. |
| **500-line file cap** | PASS with one recorded pre-existing exception | Twelve of the thirteen C# Write Set paths are at or below 500 lines. See the file-size table below. |
| No policy document modified | PASS | Zero paths under `.claude/rules/` or `.github/instructions/` in the working set, confirmed by the P5-T9 scope gate. |
| No secrets or `.env` files created | PASS | No such file in the working set. |

### File size

Source: `evidence/qa-gates/p5-t8-file-sizes-postformat.2026-09-06T22-00.md`, taken after the formatter ran. The four newly created files' line counts are independently corroborated by the patch's own hunk headers (`@@ -0,0 +1,N @@`), which agree exactly: 29, 173, 252 and 352.

| Path | Lines | Within cap |
|---|---|---|
| `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs` | 90 | yes |
| `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` | 198 | yes |
| `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs` | 658 | **no — pre-existing** |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` | 302 | yes |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | 388 | yes |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs` | 173 | yes |
| `UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs` | 29 | yes |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs` | 402 | yes |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs` | 361 | yes |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs` | 252 | yes |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs` | 416 | yes |
| `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs` | 352 | yes |
| `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs` | 429 | yes |

Test files are counted against the cap on the same footing as production files, per the policy text; all seven test paths are within it.

**The serializer exception.** `SmartSerializable.cs` was 613 lines at the base commit and is 658 lines after the change: a delta of **+45 lines**, a 7.3 percent increase on an already over-cap file, leaving it 158 lines over the 500-line limit. The delta is independently corroborated by the patch's two hunk headers for that file (`@@ -439,11 +439,35 @@` is +24 and `@@ -453,6 +477,27 @@` is +21, summing to +45, which reconciles 613 to 658 exactly). The overage is pre-existing and is declared out of this work item by design decision D5 (`spec.md` lines 411-418), by Non-Goals item 1 (lines 190-197) and by risk 5 (lines 643-645), on the stated ground that splitting a shared reusable-type-classes file during a parallel run would create merge contention with concurrently running sibling work items. This is recorded as a pre-existing exception, not a new violation class. One wording note is carried into the code review: `spec.md` line 195 describes the addition as "a small number of lines", which understates a +45 delta.

**Declared expectation change.** `PopulateWithCurrent_NullCurrent_SetsErrorLoadingText` in `StoreWrapperController_Tests.ButtonAndPopulate.cs` previously asserted `act.Should().Throw<NullReferenceException>()`. It now asserts `act.Should().NotThrow()` plus four specific rendered label values. This was declared in advance as design decision D6 (`spec.md` lines 420-432) and is required by AC8. The reviewer confirms it is a strengthening, not a weakening: the original pinned only an exception type, while the replacement pins four exact strings. The test name always described the fixed behaviour, so the change also removes a pre-existing contradiction between the name and the assertion.

---

## 3. Language-Specific Code Change Policy Compliance (C#)

Reference: the C# Code Change Policy embedded in CLAUDE.md.

| Requirement | Verdict | Evidence |
|---|---|---|
| Formatting via `dotnet tool run csharpier format .`, verified with `check` | PASS | `evidence/qa-gates/p5-t10-clean-pass.2026-09-06T22-00.md` steps 1 and 2, both exit 0. The check subcommand exited 0 over 1605 files. Invoked through `dotnet tool run`, so the manifest-pinned version was used. |
| `dotnet format` not used | PASS | Absent from every recorded command. |
| No hand-formatting against the formatter | PASS | The final pass rewrote zero Write Set files, proven by SHA-256 hashing all thirteen Write Set C# files immediately before and after the formatter run and comparing: `FORMAT-REWRITTEN-COUNT=0`. |
| Analyzer build with `/t:Rebuild`, `EnableNETAnalyzers` and `EnforceCodeStyleInBuild` | PASS | `p5-t10` step 3, exit 0. The command uses `/t:Rebuild`, not `/t:Build`, so `CoreCompile` was not skipped and the gate was genuinely capable of failing. |
| Nullable build with `/t:Rebuild` and `TreatWarningsAsErrors=true` | PASS | `p5-t10` step 4, exit 0. Character-for-character the CI command shape. |
| `/p:Nullable=enable` deliberately **not** added | PASS | Absent from the recorded command, as CLAUDE.md requires. |
| Per-file nullable opt-in honoured | PASS | The new production partial `StoreWrapperController.Display.cs` opens with `#nullable enable` on line 1, matching its sibling `StoreWrapperController.cs` line 1, so the relocated and new members remain inside the same nullable analysis context they were in before the split. `StoreWrapper.cs` continues to use `string?` annotations. |
| Toolchain run in the exact order, restarting on any change | PASS | One restart was performed and its cause recorded: the first `csharpier format` run rewrote files, which the loop rule requires be treated as a restart trigger. The restarted pass completed all six steps with exit code 0 and no file changes. |
| Strong contracts and explicit APIs | PASS | `IJunkFolderSelectionSink` documents the parameter order as part of the contract in XML comments, and a test pins it. |
| Null-safety by default | PASS | The AC8 fix converts four unguarded dereferences to the null-conditional form, matching the form already used in the adjacent block of the same method. |
| Prefer fixing diagnostics over suppressing them | PASS | Exactly one suppression is introduced, `#pragma warning disable CS0067` around the `PropertyChanged` event of the test-only probe type. It is narrowly scoped to a single member, carries an in-code rationale, and is unavoidable because `ISmartSerializable<T>` derives from `INotifyPropertyChanged` and the probe never raises the event. |
| XML documentation on non-obvious public APIs | PASS | `IJunkFolderSelectionSink`, `SerializeNow`, `TryGetSerializationPath`, `RefreshUserEmailAddress`, `LastSmtpLookupError`, `TrimStorePrefix`, `BuildUserEmailUnavailableText` and both new partial class declarations all carry `<summary>` blocks. |
| Non-SDK-style compile entries added for new files | PASS | Four new files, four hand-added `<Compile Include=...>` entries: two in `UtilitiesCS.csproj` (lines 1731 and 1739 of the patch) and two in `UtilitiesCS.Test.csproj` (lines 1144 and 1152). The risk that a missing entry silently omits a test file was mitigated as `spec.md` risk 3 directs — all 25 new test names appear in the run output, and the test total rose from 5237 to 5262. |
| `TaskMaster.Test.csproj` claimed but unmodified | PASS | The Write Set claim is conservative and every scope gate is written as a subset test, not an equality test (`spec.md` lines 264-269, `p5-t9` lines 82-86). An unmodified claimed file is not a finding. |

---

## 4. Language-Specific Unit Test Policy Compliance (C#)

Reference: the C# Unit Test Policy embedded in CLAUDE.md.

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest is the framework | PASS | Every new test carries `[TestMethod]`; the two new test types carry `[TestClass]` or extend an existing `[TestClass]` partial. `Microsoft.VisualStudio.TestTools.UnitTesting` is the only test framework namespace imported. No xUnit or NUnit reference appears. |
| Moq for mocking | PASS | `Mock<OutlookFolder>`, `Mock<NameSpace>`, `Mock<OutlookRecipient>`, `Mock<AddressEntry>`, `Mock<ExchangeUser>`, `Mock<OutlookApplication>`, `Mock<IApplicationGlobals>` and `Mock<ISmartSerializableNonTyped>` are all used. |
| FluentAssertions for assertions | PASS | Every assertion in the 25 new tests uses `.Should()`. No bare MSTest `Assert` call was introduced. |
| MSTest attributes from the correct namespace | PASS | Confirmed by the using directives in both new test files. |
| Test with coverage via `vstest.console.exe <assembly-paths> /EnableCodeCoverage` | PASS | `p5-t10` steps 5 and 6, both exit 0, invoking vstest over two explicitly named assemblies rather than by directory discovery, with the `/InIsolation` switch CI uses. |
| No test can trigger UX or a live Outlook worker | PASS | Every Outlook object is a Moq double. `MyBox.ShowDialog` is not reachable from any new test: the AC5 tests drive `PersistJunkFolderSelections` on the UtilitiesCS side against doubles and never enter `AppOlObjects.LoadJunkPotential` or `LoadJunkCertain`, which are the only members in the touched files that raise a dialog. The one live-host criterion, AC3, was not automated. |

---

## 5. Test Coverage Detail

### Coverage authority applied

Plan rule R8 (`plan.2026-09-06T22-00.md` lines 356-368) governs and resolves a genuine conflict between two policy documents that this reviewer confirms is real and unreconciled in the repository: CLAUDE.md, rank 1 in the policy compliance order, names an 80 percent repository-wide line floor and a 90 percent floor for new code, while `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` name 85 percent line and 75 percent branch uniformly across tiers.

Every coverage run for this change is scoped to two test assemblies, `UtilitiesCS.Test` and `TaskMaster.Test`, which is a narrower denominator than the full-suite denominator either floor is written against. Under R8 the two binding gates are (a) no regression between the same-scope baseline and the post-change figure, and (b) the 90 percent changed-line requirement CLAUDE.md sets for new and changed code. The 80 and 85 percent absolute figures are recorded as observations against a non-comparable denominator and are not asserted as gates here.

### Independent verification performed by this reviewer

The executor's figures were not taken on trust. Both Cobertura documents were read directly and the root `coverage` element attributes transcribed:

- Baseline `coverage.cobertura.xml`: `lines-covered="44426" lines-valid="83466" line-rate="0.5322646347015552" branches-covered="10877" branches-valid="24323" branch-rate="0.44718990256136165"`.
- Post-change `coverage.cobertura.xml`: `lines-covered="44489" lines-valid="83537" line-rate="0.5325664076995822" branches-covered="10928" branches-valid="24371" branch-rate="0.4484017890115301"`.

Every figure in `evidence/qa-gates/p5-t7-coverage-delta.2026-09-06T22-00.md` reconciles exactly with these values. The internal arithmetic also reconciles: 44426/83466 = 53.2265 percent; 44489/83537 = 53.2566 percent; the `lines-valid` delta of 71 is 0.085 percent of the baseline against an R9 tolerance of 4173; and 92/101 = 91.089 percent, with 101 + 38 relocated = 139 and 92 + 38 = 130 matching the pre-exclusion aggregate.

Additionally, this reviewer located each new or changed production member in the post-change Cobertura document and read its own `line-rate` and `branch-rate` directly, which is stronger evidence for the new-code floor than a file-level aggregate:

| Member | Line rate | Branch rate | Criterion |
|---|---|---|---|
| `SmartSerializable.TryGetSerializationPath` | 1.00 | 1.00 | AC2 |
| `SmartSerializable.SerializeNow` | 1.00 | 1.00 | AC4 |
| `StoreWrapper.RefreshUserEmailAddress` | 1.00 | 1.00 | AC6 |
| `StoreWrapper.GetSmtpAddressFromStore` | 0.8710 | 0.9444 | AC6 |
| `StoreWrapperController.BuildUserEmailUnavailableText` | 1.00 | 1.00 | AC6 |
| `StoreWrapperController.TrimStorePrefix` | 1.00 | 1.00 | AC7 |
| `StoreWrapperController` class in `StoreWrapperController.Display.cs` | 1.00 | 0.9324 | AC6, AC7, AC8 |

Every one of the seven clears both the 85 percent line figure and the 75 percent branch figure at member level, and six of the seven are at 100 percent line coverage. The single member below 100 percent, `GetSmtpAddressFromStore`, is at 87.10 percent line and 94.44 percent branch, both comfortably above every documented floor.

### Coverage verdicts by language

Languages with changed files in the branch diff: C# only. The fifteen changed paths are ten `.cs` files, two `.csproj` files and three feature-folder Markdown documents.

- **C# coverage: PASS.** No-regression gate met (post-change 53.26 percent is not below the baseline 53.23 percent, on a comparable denominator under rule R9) and the changed-line gate met (91.09 percent over 101 executable changed lines, against CLAUDE.md's 90 percent requirement for new and changed code). Verified directly by this reviewer against both Cobertura documents and against per-member rates, as tabulated above. The canonical path `artifacts/csharp/coverage.xml` is not populated in this worktree; the substituting artifacts are the two session Cobertura documents named above, which were read in full during this audit, together with the committed delta report at `evidence/qa-gates/p5-t7-coverage-delta.2026-09-06T22-00.md`.
- **PowerShell coverage: PASS.** Zero changed `.ps1` files in the branch diff, so no Pester coverage obligation arises. The one PowerShell file used during execution, `coverage/plan797-helpers.ps1`, was created and deleted within the agent session, lived in a git-ignored directory throughout, and was never committed, which is exactly the throwaway-script exemption the General Code Change Policy grants.
- **Python coverage: PASS.** Zero changed `.py` files in the branch diff, so no Python coverage obligation arises.
- **TypeScript coverage: PASS.** Zero changed `.ts` files in the branch diff, so no TypeScript coverage obligation arises.

### Repository-wide floors, recorded as observations

Both the baseline and the post-change figures sit below CLAUDE.md's 80 percent repository-wide line floor and below the 85 percent line and 75 percent branch figures in `.claude/rules/general-unit-test.md`. Under the two-assembly scope this plan measures, the post-change document reads 53.26 percent line and 44.84 percent branch. This condition is pre-existing: the baseline was already at 53.23 percent line and 44.72 percent branch before any change here. This change neither created nor resolved it, and it moved both figures marginally upward. It is recorded as an observation against a non-comparable denominator, not raised as a finding.

One further observation. The `UtilitiesCS` package-level line rate moved from 0.884099 to 0.884058, a decrease of 0.004 percentage points. That is inside the known cross-session nondeterminism band for this repository's C# coverage constants and is not treated as a regression; the binding same-scope document-level comparison moved upward.

### The two zero-coverage rows, examined

`evidence/qa-gates/p5-t7-coverage-delta.2026-09-06T22-00.md` reports two rows that warrant scrutiny rather than acceptance.

1. `IJunkFolderSelectionSink.cs` is reported NOT APPLICABLE. This reviewer confirms the basis: the file declares only an interface, an interface declaration emits no IL, and consequently no `class` element for it exists in either Cobertura document. Rule R10 directs that such a file be reported as not applicable rather than as a zero. This is the correct signal for a declaration-only asset, not a coverage gap, and it does **not** constitute exclusion of a production file from measurement.
2. `AppOlObjects.JunkFolders.cs` is reported at 1 executable changed line, 0 covered. This reviewer confirms the file itself is measured (21 of 69 lines in the post-change document) and carries no class-level exclusion attribute. The single uncovered line is the explicit interface implementation's forwarding expression at line 57 of that file. It is uncovered because driving it would write to the `.NET` user settings store, which no unit test may do. The contract the forwarder carries — that the junk-certain path is supplied first — is pinned instead on the UtilitiesCS side by `PersistJunkFolderSelections_PassesJunkCertainPathFirst` against a recording double, and this reviewer verified the forwarder's argument order by code read: parameters are passed positionally in the same order, and the internal method routes `junkCertainRelativePath` to `WriteJunkCertainSetting` and `junkPotentialRelativePath` to `WriteJunkPotentialSetting`. Recorded as an advisory in the code review, not a blocking gap.

---

## 6. Test Execution Metrics

Source: `evidence/qa-gates/p5-t5-vstest.2026-09-06T22-00.md`.

| Metric | Value |
|---|---|
| Total tests | 5262 |
| Passed | 5262 |
| Failed | 0 |
| Skipped | 0 |
| Exit code | 0 |
| Baseline total | 5237 |
| Tests added by this change | 25 |
| Fail-before tests recorded | 16 |
| Fail-before tests that pass after | 16 of 16, enumerated in full rather than sampled |
| Pre-existing failures in the Write Set | NONE |

Red-first evidence. `evidence/regression-testing/p1-t18-fail-before.2026-09-06T22-00.md` records sixteen named tests failing before their fixes, and `p5-t5` enumerates all sixteen as passing after, in full rather than by sample. The correspondence is complete and no name is carried under a pre-existing-failure exclusion.

Excluded test classes. Four shell-icon test classes are excluded from every local run in this plan by test-case filter: `HelperClasses.ShellUtilities_Tests`, `HelperClasses.ShellUtilitiesStatic_Tests`, `HelperClasses.SysImageListHelperTests` and `EmailIntelligence.OSBrowser_Tests`. The stated reason is that they stall vstest on this workstation, which is an environmental condition unrelated to this change and independently documented in this repository. None of the four is in the Write Set, none touches any changed file, and CI covers them. The exclusion is applied identically to the baseline and the post-change runs, so the coverage comparison is not distorted by it.

Known intermittent failure. `UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` is tracked as a timing flake under issue 803 and is outside this Write Set. It passed in this run as it did at baseline and is not raised here.

---

## 7. Code Quality Checks

| Check | Command | Exit code |
|---|---|---|
| Format (apply) | `dotnet tool run csharpier format .` | 0 |
| Format (verify) | `dotnet tool run csharpier check .` | 0 (1605 files, none unformatted) |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 |
| Nullable / type check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 |
| Tests | vstest over the two named assemblies with `/InIsolation` | 0 |
| Coverage | vstest coverage collection, converted to Cobertura | 0 |

Loop discipline. Exactly one restart was performed, its trigger recorded (the first formatter run rewrote files), and the restarted pass completed all six steps in a single uninterrupted sequence with every exit code at 0 and no step changing files. The `FORMAT-REWRITTEN-COUNT=0` SHA-256 comparison is a genuine proof that the final pass was clean rather than an assertion that it was.

Scope gate. Fifteen source and project paths in the working set, every one a member of the Write Set, confirmed by the union of an anchored diff listing and a porcelain listing taken with `--untracked-files=all`. This reviewer independently confirmed the same fifteen paths by enumerating the file diffs in the supplied patch, and confirmed the post-image of five production files by reading them directly from the working tree and comparing them against the patch text.

Artifact hygiene. No absolute host path, account name or machine name appears in any evidence artifact authored by this execution. The vstest results file was deliberately not committed because it carries `runUser` and `computerName` attributes, and only sanitized counts were transcribed. Two pre-existing documents in the feature folder — `issue.md` and `research/research-folder-settings-persistence.md` — contain the reporter's own mailbox address, which is maintainer-authored bug-report content that predates this change and was not introduced by it.

---

## 8. Gaps and Exceptions

1. **AC3 is not automatable and was not automated.** It requires a live Outlook process, a real user profile and a full process teardown and restart. The handoff at `evidence/other/p6-t1-ac3-manual-verification.2026-09-06T22-00.md` records `AC3-RESULT: BLOCKED-MANUAL`, marks all nine procedure steps NOT PERFORMED with individual reasons, fabricates no observation, and leaves the checkbox unmarked in both `spec.md` and `issue.md`. Plan task P6-T4 is correspondingly left unchecked, which is the plan's own conditional branch behaving as designed. This is honest non-verification, correctly recorded, and is not a defect. It is carried in the feature audit as UNVERIFIED and handed to the maintainer.
2. **Pre-existing file-size overage, deliberately not resolved.** `SmartSerializable.cs` remains 158 lines over the 500-line cap and grew by 45 lines in this change. Declared out of scope by D5 with a stated rationale. Recorded, with the delta reported as the caller directed.
3. **Repository-wide coverage below both documented floors.** Pre-existing under the two-assembly measurement scope; neither created nor resolved by this change. Recorded as an observation under R8 rather than asserted as a failed gate.
4. **The 80-versus-85 percent floor conflict between CLAUDE.md and `.claude/rules/` is unreconciled in the repository.** This is a standing documentation defect independent of this work item. R8 resolves it for this change by naming the two gates that are measurable against the scope actually used. Reconciling the two documents is a separate concern and is not raised as a finding against this change.
5. **Base and head commit identity rest on supplied values.** The directive prohibits running `git`, so the merge base could not be recomputed and the head SHA could not be confirmed current. Mitigated by three independent non-git corroborations of the patch's completeness, enumerated under "Rejected Scope Narrowing" above.
6. **The MCP policy-audit template and its validator were unavailable.** This artifact is hand-authored preserving the canonical heading structure; the validator was not run.
7. **Raw Cobertura documents are not committed.** They are git-ignored, so a later reviewer cannot re-verify the coverage figures without rerunning the measurement. This reviewer read them during this audit and confirmed every figure; the confirmation is recorded in section 5.

---

## 9. Summary of Changes

Fifteen paths: seven production C# files (five modified, two created), six test C# files (four modified, two created), and two project files. Three feature-folder Markdown documents were also written for check-off and evidence.

| Path | Change | Purpose |
|---|---|---|
| `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs` | modify | AC1 — the fresh-build branch adopts the loader's disk configuration |
| `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` | modify | AC5 — explicit implementation of the new typed sink |
| `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs` | modify | AC2 shared path guard; AC4 `SerializeNow` explicit-save entry point |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` | modify | AC6 — ordered SMTP fallback chain, captured failure reason, retry entry point |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | modify | AC4 save-path switch; AC5 reflection removal; D4 partial split |
| `UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs` | create | AC5 — the typed seam |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs` | create | D4 relief; AC6 retry, AC7 trim, AC8 guards |
| `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs` | create | AC2 and AC4 tests |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs` | create | AC6, AC7 and AC8 tests |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs` | modify | AC5 tests and double retarget |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs` | modify | AC8 — the declared D6 inversion |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs` | modify | AC6 fallback-order tests |
| `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs` | modify | AC1 positive and negative tests |
| `UtilitiesCS/UtilitiesCS.csproj` | modify | two compile entries for the new production files |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | modify | two compile entries for the new test files |

---

## 10. Compliance Verdict

**PASS. Zero blocking findings.**

Every mandatory gate in the policy compliance order was run in the required order and passed on a single clean final pass with independently corroborated evidence. The coverage gates that R8 identifies as binding are both met and were re-verified by this reviewer against the raw Cobertura documents rather than accepted from the executor's report. No production file was excluded from coverage measurement, no coverage-exclusion attribute was introduced, no policy document was modified, no evidence artifact was written outside the canonical location, and no temporary file, sleep or wall-clock wait appears in any test.

Six advisory findings are recorded in `code-review.2026-09-07T22-40.md`. None is blocking, none changes an acceptance-criteria verdict, and no remediation-inputs artifact is produced.

The one outstanding item is AC3, which is handed to the maintainer for manual verification by Outlook restart with a nine-step procedure. It is unverified, not failed.

---

## Appendix A: Test Inventory

25 tests added. All 25 passed in the final run and appear in the results file.

**AC1 — `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs` (2)**
1. `LoadStoresAsync_WhenConfigDeserializesToNull_FreshWrapperAdoptsLoaderDiskConfiguration`
2. `LoadStoresAsync_WhenConfigKeyIsAbsent_FreshWrapperKeepsEmptyDiskPath`

**AC2 and AC4 — `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs` (4)**
3. `Serialize_WithEmptyDiskPath_LogsErrorAndArmsNoTimer`
4. `Serialize_WithNullDiskPath_LogsErrorAndArmsNoTimer`
5. `SerializeNow_WithConfiguredPath_WritesWithoutFiringTimer`
6. `Serialize_WithConfiguredPath_StillRequiresTimerFireToWrite`

**AC5 — `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs` (2 new, 1 retargeted)**
7. `PersistJunkFolderSelections_PassesJunkCertainPathFirst`
8. `PersistJunkFolderSelections_WhenGlobalsAreNotTheTypedSink_LogsErrorAndDoesNotInvoke`
- retargeted: `PersistJunkFolderSelections_WhenApplyMethodIsMissing_DoesNotThrow`

**AC6 — `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs` (5)**
9. `GetSmtpAddressFromStore_WhenPrimarySmtpAddressIsPresent_ReturnsIt`
10. `GetSmtpAddressFromStore_WhenPrimarySmtpThrows_FallsBackToAddressEntryAddress`
11. `GetSmtpAddressFromStore_WhenPrimaryAndAddressEntryFail_FallsBackToDisplayName`
12. `GetSmtpAddressFromStore_WhenEveryFallbackFails_ReturnsNullAndCapturesReason`
13. `RefreshUserEmailAddress_WhenRootFolderIsNull_ReturnsNullAndDoesNotThrow`

**AC6, AC7 and AC8 — `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs` (12)**
14. `PopulateWithCurrent_WhenUserEmailIsNull_RetriesLookupAndRendersAddress`
15. `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_DoesNotRetryLookup`
16. `PopulateWithCurrent_WhenRetryFails_RendersSpecificMessageWithReason`
17. `TrimStorePrefix_WithLeadingStorePrefix_RemovesIt`
18. `TrimStorePrefix_WithNoLeadingBackslash_ReturnsInputUnchanged`
19. `TrimStorePrefix_WithSingleLeadingBackslash_ReturnsInputUnchanged`
20. `TrimStorePrefix_WithEmptyString_ReturnsEmptyString`
21. `TrimStorePrefix_WithNull_ReturnsNull`
22. `TrimStorePrefix_WithOnlyTheStorePrefix_ReturnsEmptyString`
23. `PopulateWithCurrent_RendersInboxAndRootFolderWithoutStorePrefix`
24. `PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow`
25. `GetRelativeFsPath_WithNullCurrent_ReturnsPlaceholderAndDoesNotThrow`

**Modified — `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs`**
- `PopulateWithCurrent_NullCurrent_SetsErrorLoadingText` — assertion inverted under the declared D6 expectation change.

---

## Appendix B: Toolchain Commands Reference

```text
dotnet tool run csharpier format .
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /InIsolation
```

Two constraints were honoured and are restated for the record: `/t:Rebuild` rather than `/t:Build`, because MSBuild's up-to-date check does not invalidate on a command-line property change and a warm `/t:Build` would skip `CoreCompile` and return exit 0 without running the gate; and `/p:Nullable=enable` deliberately omitted from the nullable step, because no project in this repository carries a `<Nullable>` element and forcing it conscripts every file that has never adopted the per-file pragma.
