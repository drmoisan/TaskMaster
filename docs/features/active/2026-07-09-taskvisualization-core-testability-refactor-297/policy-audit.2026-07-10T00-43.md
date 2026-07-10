# Policy Compliance Audit — #297 TaskVisualization Core Testability Refactor (Remediation Pass 1 Re-Audit)

- Timestamp: 2026-07-10T00-43
- Branch under review: `feature/taskvisualization-core-testability-refactor-297` (remediation head `8587ae92`)
- Base branch: `epic/winforms-testability-refactor-integration`
- Merge-base: `3f04d50f6544f084323e5d7a9a563facb9d579df` (recomputed via `git merge-base HEAD origin/epic/winforms-testability-refactor-integration`)
- Work mode (from `issue.md`): `full-feature` -> AC sources are `spec.md` and `user-story.md` (`user-story.md` intentionally absent per documented refactor-child policy).
- Review type: focused re-audit confirming the single prior Blocking finding is resolved and no new issue was introduced. The audit scope remains the full branch diff against the resolved base; the prior cycle audited the full feature and this cycle confirms the remediation delta plus the previously-verified invariants.

## Executive Summary

The prior review's single Blocking finding — `SetFlag(Taskname)` and `Shortcut_ReadingNews` uncovered because the `_active.TaskSubject = value` write raised `MissingMethodException` over a Moq proxy — is resolved by the prescribed fix. An optional-with-default `Action<string> setActiveTaskSubject` seam was added to both `TaskController` constructors (default `v => _active.TaskSubject = v`), the `SetFlag` `Taskname` case routes through `_setActiveTaskSubject(value)`, and two new tests exercise both methods through a capturing delegate. No new `[ExcludeFromCodeCoverage]` was introduced, `FlagTasks.cs` remains unchanged, and no file exceeds the 500-line limit. No new policy violation was detected in the remediation delta.

Overall verdict: PASS. Zero Blocking findings.

## Rejected Scope Narrowing

None. The caller framed this as a "focused re-audit" of the prior Blocking finding, which describes the re-review purpose rather than narrowing the audit scope to a prohibited subset. The caller explicitly requested confirmation of all 14 spec ACs, STA policy, #298 contract stability, and full-toolchain evidence. The audit was conducted against the full branch diff versus the resolved merge-base. No instruction to skip a language, mark coverage informational, or limit to a file subset was present.

## Evidence Location Compliance

- Scanned the full branch diff for files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`: NONE present. Result: PASS.
- All feature evidence is written under the canonical `<FEATURE>/evidence/<kind>/` tree (`evidence/baseline/`, `evidence/qa-gates/`, `evidence/other/`).
- `scripts/dev_tools/validate_evidence_locations.py` is not present in this repository; the scan was performed via `git diff --name-only` against the merge-base. No violation found.
- Note: coverage evidence text references a copy at `artifacts/csharp/coverage.xml`. That is the designated canonical C# coverage-artifact path in the reviewer coverage table, not a prohibited evidence-baseline path; it is gitignored and therefore absent from the review worktree checkout.

## Section 1 — Coverage Verification (per language with changed files)

Languages with changed source files in the branch diff: C# only (`*.cs`, `*.csproj`). No `*.ts`, `*.py`, or `*.ps1` source files are in the diff, so those languages have zero changed files and are not evaluated.

### 1.2.1 Per-language coverage rows

| Language | Baseline | Post-change | Change | New/changed-code coverage | Disposition | Evidence |
|---|---|---|---|---|---|---|
| C# (CSharp) coverage | 0.00% line (controller class-level exempt at baseline) | package 85.36% line / 78.28% branch; refactored-core aggregate 88.95% line | +85.36 line (package) | changed partial `TaskController.Actions.cs` 98.39% line / 91.30% branch; new helpers 100% line | PASS | `evidence/qa-gates/remediation-297-setactivetasksubject-seam.2026-07-10T00-36.md`, `evidence/qa-gates/coverage-comparison.2026-07-10T00-01.md` |

C# coverage verdict: PASS. Package line coverage 85.36% clears the 80% (CLAUDE.md testable-denominator) and 85% (general-unit-test) line floors; package branch coverage 78.28% clears the 75% branch floor; the changed partial (98.39% line / 91.30% branch) and the two new helper classes (100% line) clear the >=90% new/changed-code line target.

Coverage-artifact status: The C# coverage collector ran during execution (`vstest.console.exe ... /Settings:coverage.runsettings`, exit 0, 106/106 tests) and the parsed Cobertura figures are recorded per-file and per-line in the cited evidence. The raw Cobertura artifact is gitignored and therefore not present in the review worktree for independent reparse; the coverage-verdict hook parses `artifacts/csharp/coverage.xml` with a JaCoCo reader that returns null for Cobertura, so no automated repo-wide numeric floor is enforced by the hook for C#. The verdict above rests on the documented executor evidence corroborated by the independent source/test structural confirmation in Section 3.

Comparison line (numeric): Baseline: 0.00% line. Post-change: 85.36% line / 78.28% branch (package); 88.95% line (refactored core). Disposition: PASS (both floors met; changed code above the 90% line target).

## Section 2 — Prior Blocking Finding Disposition

Prior Blocking-1: `TaskController.Actions.cs` `SetFlag` `Taskname` case + `Shortcut_ReadingNews` uncovered.

Resolution confirmed (independent source inspection):

1. Seam on both constructors — `TaskController.cs:43` (6-arg ctor param `Action<string> setActiveTaskSubject = null`), forwarded at `:58`; `TaskController.cs:105` (11-arg ctor param), forwarded at `:119`. `InitializeSeams` (`:165`) applies the production default at `:177`: `_setActiveTaskSubject = setActiveTaskSubject ?? (v => _active.TaskSubject = v);`. Field declared at `:286`.
2. `SetFlag` `Taskname` case routes through the seam — `TaskController.Actions.cs:386` `_setActiveTaskSubject(value);` followed by `:387` `_viewer.TaskNameText = value;`.
3. Two new tests — `TaskControllerActionsTests.cs:76` `SetFlag_Taskname_WritesSubjectAndFacade` (injects `setActiveTaskSubject: v => captured.Add(v)`, asserts captured value and `TaskNameText` facade write) and `:200` `Shortcut_ReadingNews_SetsAllFlagsAndFocusesDuration` (injects the capturing delegate, asserts Context/Projects on `Active`, captured `"READ: Original Subject"`, `DurationText = "15"`, and `view.Mock.Verify(v => v.FocusDuration(), Times.Once)`). Both use `MoqTaskViewer` (Moq-backed `ITaskViewer`, `InvokeRequired => false`, non-`Form`).
4. Both methods COVERED (not exempted, not skipped) — no `[ExcludeFromCodeCoverage]` exists on either method or in `TaskController.Actions.cs`; the prior "not unit-tested here" skip comments were removed; the executor evidence records per-line hits=1 for `Shortcut_ReadingNews` (lines 299-306) and the `SetFlag` `Taskname` case (lines 384-389, including 386). The seam replaces the interop write with a delegate the test controls, so the two methods are now reachable in a unit test without the `MissingMethodException`.

Disposition: RESOLVED.

## Section 3 — General Code Change Policy

- Simplicity / reusability: the seam mirrors the existing `_showWarning` and `_mailItemHelperFactory` seam pattern (same optional-with-default shape), so no new abstraction style was introduced. PASS.
- Separation of concerns / seam-first: the fix uses an injectable delegate (the policy-preferred seam tier) rather than an exemption. PASS.
- File-size limit (500 lines): all changed production and test files are at or below 500 lines (Section 5). PASS.
- Public API compatibility: the new constructor parameter is trailing optional-with-default; `FlagTasks.cs` (sole constructing caller) compiles unedited and is absent from the diff. PASS.
- Error handling / logging: unchanged; no new broad catch or ad-hoc output introduced. PASS.

## Section 4 — Unit Test Policy (MSTest / Moq / FluentAssertions)

- Framework: MSTest `[TestMethod]`; assertions via FluentAssertions (`.Should().Equal(...)`); mocking via Moq (`Mock<ITaskViewer>`, `VerifySet`, `Verify`). PASS.
- Independence / determinism: both new tests construct a fresh `MoqTaskViewer` and controller per test; no shared mutable state, no wall-clock or RNG dependence, no `Thread.Sleep`/`Task.Delay`, no temp files, no network. The `DateTime.Now` occurrences are confined to the pre-existing `MoqOlToDo` builder and are not asserted (documented in `TaskControllerFixtures.cs:20`). PASS.
- Isolation / no live form or popup: the new tests use the Moq-backed non-`Form` viewer and the capturing delegate; no `Form`-derived construction, no `ShowDialog`/`Show`, no `MessageBox` popup. PASS.
- Arrange-Act-Assert and intent documentation: both tests have a rationale comment and clear AAA structure. PASS.

## Section 5 — File-Size Compliance (independent count via `wc -l`)

| File | Lines | Limit | Result |
|---|---|---|---|
| `TaskVisualization/TaskController.cs` | 330 | 500 | PASS |
| `TaskVisualization/TaskController.Actions.cs` | 490 | 500 | PASS |
| `TaskVisualization/TaskController.Accelerator.cs` | 500 | 500 | PASS (at limit; untouched by remediation) |
| `TaskVisualization/TaskController.ControlMaps.cs` | 296 | 500 | PASS |
| `TaskVisualization/TaskController.ControlRelationships.cs` | 259 | 500 | PASS |
| `TaskVisualization/TaskController.Flags.cs` | 181 | 500 | PASS |
| `TaskVisualization/TaskViewer.cs` | 456 | 500 | PASS |
| `TaskVisualization.Test/TaskControllerActionsTests.cs` | 452 | 500 | PASS |
| `TaskVisualization.Test/TaskControllerFixtures.cs` | 195 | 500 | PASS |
| `TaskVisualization.Test/TaskControllerAccelerator.StaTests.cs` | 353 | 500 | PASS |
| `TaskVisualization.Test/StaControlHarness.cs` | 221 | 500 | PASS |

`TaskController.Accelerator.cs` is exactly 500 lines and was last modified in the original feature commit `82b207ff`; the remediation commit `8587ae92` did not touch it (verified via `git log -- TaskController.Accelerator.cs` and the remediation commit stat). No file exceeds 500 lines.

## Section 6 — Exemption Inventory Compliance

- No `[ExcludeFromCodeCoverage]` exists in `TaskController.Actions.cs` or `TaskController.cs` (the two production files the remediation touched). PASS.
- The remediation commit `8587ae92` added and removed zero `[ExcludeFromCodeCoverage]` attributes; the only exemption-inventory edit was prose retiring the "not-exempt-but-uncovered residue" note for the now-covered `SetFlag(Taskname)` case. PASS.
- `ApplyChanges` method-level exemption (`TaskController.Flags.cs:35`, previously adjudicated PERMITTED) is unchanged; the file was untouched by the remediation. PASS.
- Remaining production exemptions are the ratified handle/pump/focus and COM/VSTO/WinForms set: `TaskController.Accelerator.cs` (lines 45, 60, 284, 292, 301 — PostMessage/handle/focus residue), `TaskViewer.cs:18` (Form-derived), `TagPromptService.cs:18` (WinForms dialog adapter), `FlagChangeGroup.cs`, `FlagTasks.cs`, and the sibling #298-owned `AutoAssign*`/`AutoCreate*`/`EditFilter*`/`ManageFilters` classes. No exemption was broadened by the remediation. PASS.

## Section 7 — Toolchain Evidence (evidence-verification model; not rerun)

Per the C# toolchain order in CLAUDE.md, using the documented remediation evidence (`evidence/qa-gates/remediation-297-setactivetasksubject-seam.2026-07-10T00-36.md`):

| Stage | Command | Result (evidence) | Row Verdict |
|---|---|---|---|
| Format (csharpier) | `dotnet tool run csharpier format .` then `csharpier check` | exit 0; 44 files checked clean; only the 5 intended files changed | PASS |
| Analyzers | `MSBuild ... /p:EnableNETAnalyzers /p:EnforceCodeStyleInBuild` | exit 0; 0 errors; only pre-existing baseline warnings | PASS |
| Nullable / TWAE | `MSBuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` (incremental Debug-scoped gate) | exit 0; 0 errors; forced-Rebuild 84 errors confined to vendored `SVGControl`/`UtilitiesSwordfish` (known repo-wide debt) | PASS |
| Tests (MSTest incl. STA) | `vstest.console.exe TaskVisualization.Test.dll /InIsolation /Settings:coverage.runsettings` | exit 0; 106/106 passed (104 pre-existing + 2 new); STA `[STATestClass]`/`[STATestMethod]` classes executed | PASS |

The toolchain was not rerun in this review; verdicts rest on the recorded execution evidence corroborated by source inspection. The nullable forced-Rebuild residue matches the established repo pattern (vendored assemblies excluded from the touched-code gate).

## Appendix A — Base Resolution and Scope

- Base branch resolved per `pr-base-branch-merge-base`: `origin/epic/winforms-testability-refactor-integration`.
- Merge-base recomputed independently: `3f04d50f6544f084323e5d7a9a563facb9d579df` (matches caller-supplied base intent).
- Branch diff: 26 `*.cs`, 2 `*.csproj`, 1 `*.runsettings`, and docs/evidence markdown. C# is the only language with changed source files.
- Remediation delta (commit `8587ae92`): 6 files — `TaskController.cs`, `TaskController.Actions.cs`, `TaskControllerActionsTests.cs`, `TaskControllerFixtures.cs`, `exemption-inventory.2026-07-10T00-01.md`, and the remediation evidence doc.
