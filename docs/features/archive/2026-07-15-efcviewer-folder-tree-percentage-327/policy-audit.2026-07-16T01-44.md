# Policy Compliance Audit — efcviewer-folder-tree-percentage (#327)

- Timestamp: 2026-07-16T01-44
- Reviewer: feature-reviewer
- Branch under review: `feature/efcviewer-folder-tree-percentage-327`
- Base branch: `origin/epic/folder-tree-percentage-ui-integration`
- Merge-base: `34ed0422ab47969c5b518e5244e861233ad3552d`
- Head: `5b937c90f83ed4816fb0fb382316c3b0aa9077c4`
- Work mode (from `issue.md`): `full-feature` (AC sources: `spec.md` and `user-story.md`)
- Scope: full branch diff vs base (`git diff origin/epic/folder-tree-percentage-ui-integration...HEAD`)

## Executive Summary

The feature delivers its testable logic as five new host-neutral C# modules under
`UtilitiesCS/OutlookObjects/Folder/`, with WinForms/COM wiring confined to two Designer files,
`EfcViewer3.cs`, and the shared `EfcFormController.cs` (all coverage-exempt per CLAUDE.md). The full
C# toolchain is green in the committed Phase 5 evidence (CSharpier exit 0, analyzers 0/0, nullable
0/0, MSTest 4762/4762). New-code coverage for the code-bearing modules is 96.43%-100%, clearing the
>=90% new-code target and the >=85% line / >=75% branch floors. No banned APIs were introduced in
touched production files. Overall verdict: PASS. Two non-blocking observations are recorded below
(pre-existing file-size condition on the exempt controller; a trivial exempt seam implementation).

Repository-wide line coverage (77.54%) sits below the 85% floor. This is a pre-existing repository
condition, not introduced by #327 (baseline 77.46%, post-change 77.54%, +0.07 pts), and is
dispositioned non-blocking under the CLAUDE.md COM/VSTO testable-denominator exemption. All
change-scoped coverage gates hold, so this condition is not attributable to this feature.

## Policy Reading Order Applied

1. `CLAUDE.md` (standing instructions)
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md` (C# is the only changed source language)

## 1. General Code Change Policy

| Check | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | Host-neutral tree/formatter/adapter kept small and single-purpose; controller wiring delegates to helpers rather than duplicating logic. |
| Reusability (shared logic factored out) | PASS | Tree state, hierarchy building, visible-row projection, percentage formatting, and the probability join live once in `UtilitiesCS/OutlookObjects/Folder/`; both viewers consume them via the shared controller. |
| Separation of concerns (pure logic vs I/O/UI) | PASS | Pure in-memory model in UtilitiesCS; all WinForms/COM contact isolated in `EfcFormController` and the two Designers. |
| Error handling fail-fast | PASS | `FolderProbabilityAdapter` throws `ArgumentNullException` on null source/tree; state transitions no-op safely on null nodes and banners. |
| Naming conventions | PASS | PascalCase types/members, camelCase locals; descriptive names throughout. |
| Banned APIs (RS0030 BannedSymbols) | PASS | `git grep` over touched production files (`UtilitiesCS/OutlookObjects/Folder/`, `EfcFormController.cs`, `EfcViewer3.cs`) found no `DateTime.Now/UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`, `Guid.NewGuid`, or `DateTimeOffset.Now/UtcNow`. |
| File size <= 500 lines | PARTIAL (non-blocking) | See Observation OBS-1. `EfcFormController.cs` is 1122 lines at head (1014 at baseline); pre-existing over-limit file extended by exempt wiring. All new files are well under 500 (largest is `FolderSuggestionTree.cs` at 253). |
| Public API compatibility | PASS | No public API removed; new public types are additive. |
| Dependencies | PASS | Reuses the already-referenced `BrightIdeasSoftware` (ObjectListView 2.9.1) `TreeListView`; no new package added. |

### OBS-1 (non-blocking): controller file exceeds the 500-line limit

`QuickFiler/Controllers/EfcFormController.cs` is 1122 lines at head, up from 1014 at baseline. The
500-line limit was already exceeded before this feature; #327 did not cause the file to cross the
threshold. The +108 net lines are WinForms/COM wiring (TreeListView configuration, bind/rebind,
selection caching, key handling) that legitimately belongs in the coverage-exempt controller, and
the genuinely testable logic was correctly placed in the host-neutral helpers. Recommendation
(future, non-blocking): extract cohesive controller regions to reduce the file below the limit. Not
attributable as a new violation of this feature.

## 2. General Unit Test Policy

| Check | Verdict | Evidence |
|---|---|---|
| Framework (MSTest) | PASS | All four new test files use `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. |
| Mocking (Moq) / Assertions (FluentAssertions) | PASS | `FolderProbabilityAdapterTests` uses `Mock<IFolderProbabilitySource>` (MockBehavior.Strict); all tests assert with FluentAssertions. |
| Arrange-Act-Assert | PASS | All new tests use explicit AAA sectioning. |
| Determinism (no wall-clock, no RNG, no sleeps) | PASS | No time/RNG/sleep dependencies; pure in-memory inputs. |
| No temp files / network / COM / external services | PASS | Host-neutral logic is tested directly; no filesystem, network, or Outlook Interop. |
| Independence / isolation | PASS | Each test builds its own tree; no shared mutable state. |
| Scenario completeness (positive/negative/edge/error) | PASS | Hierarchy (nested, deep-without-parent, per-section isolation, order preservation, empty, single, null); state (leaf/already-expanded/already-collapsed/banner/null no-ops, visible-row projection); formatter (0, 1, typical, below-midpoint, at-midpoint away-from-zero, small-midpoint, null); adapter (matched/unmatched/banner-never-queried/nested/null-guards). |
| Test file location mirrors source | PASS | `UtilitiesCS.Test/OutlookObjects/Folder/` mirrors `UtilitiesCS/OutlookObjects/Folder/`. |
| Coverage exclusion policy (no production path excluded from measurement) | PASS | Exemptions are applied via `[ExcludeFromCodeCoverage]` on WinForms Form-derived / Designer / COM-bound controller types, consistent with the CLAUDE.md COM/VSTO exemption; no host-neutral production file is excluded. |

### 2.1 Coverage Verification (evidence-based; no coverage regeneration performed)

Coverage was verified by reading the executor's committed evidence under
`docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/qa-gates/`
(`phase5-final-tests-coverage.md` and `phase5-coverage-delta.md`). The reviewer did not regenerate
coverage and did not write `artifacts/csharp/coverage.xml`.

### C# Coverage

- C# new/changed-code line coverage: PASS. The five new host-neutral modules are covered at
  96.43%-100% line coverage, clearing the >=90% new-code target and the >=85% line floor:
  FolderSuggestionNode 100% line / 100% branch; FolderSuggestionTree 98.45% line / 96.43% branch
  (2 uncovered defensive guard lines); PercentageFormatter 100% / 100%; FolderProbabilityAdapter
  100% / 100%.
- C# changed-line no-regression coverage: PASS. The only changed/added production lines under
  measurement are the five new modules (96.43%-100%). All other modified files carry
  `[ExcludeFromCodeCoverage]` and are outside the coverage denominator; no previously-covered
  production line lost coverage.
- IFolderProbabilitySource: interface-only; it declares no executable lines and is legitimately
  omitted from measurement per `.claude/rules/general-unit-test.md`.

Numeric repo-wide line-coverage comparison (identical tooling for both points):

- Baseline: 77.4641% (109085/140820 lines) — `evidence/baseline/phase0-baseline-tests-coverage.md`.
- Post-change: 77.5388% (109553/141288 lines) — `evidence/qa-gates/phase5-final-tests-coverage.md`.
- Disposition: repository-wide line coverage moved +0.0747 pts (increase); no regression. Repository
  branch coverage moved 52.9436% -> 53.1184% (+0.1748 pts, increase).

Repository-wide line coverage of 77.54% is below the 85% floor. This is a pre-existing repository
condition (present at baseline, 77.46%) and is dispositioned non-blocking under the CLAUDE.md
COM/VSTO testable-denominator exemption. It is not introduced by #327: the feature increased the
repository figure. The change-scoped gates that bind this feature (new-code >=90% and no-regression
on changed lines) both hold, so the repository-wide floor is treated as an authority-scoped
pre-existing exception rather than a finding attributable to this feature.

## 3. C# Code Change Policy

| Check | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `evidence/qa-gates/phase5-final-csharpier.md`: `csharpier check .` exit 0, 1352 files, no differences. |
| .NET analyzers (EnableNETAnalyzers/EnforceCodeStyleInBuild) | PASS | `evidence/qa-gates/phase5-final-analyzers.md`: build succeeded, 0 warnings, 0 errors. |
| Nullable / TreatWarningsAsErrors | PASS | `evidence/qa-gates/phase5-final-nullable.md`: build succeeded, 0 warnings, 0 errors; `#nullable enable` on all new files. |
| MSTest execution | PASS | `evidence/qa-gates/phase5-final-tests-coverage.md`: 4762 tests, 4762 passed, 0 failed, exit 0. |
| Strong contracts / null-safety | PASS | Explicit types at public boundaries; nullable annotations on `Probability`/`FolderSuggestionNode?`; guard clauses in the adapter. |
| Framework/mocking/assertion selection (MSTest/Moq/FluentAssertions) | PASS | Confirmed in the four new test files. |

## 4. Evidence Location Compliance

All evidence artifacts are written under
`docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/<kind>/`
(`baseline/`, `qa-gates/`, `other/`), which is the canonical location per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. A scan of the branch diff found no
files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or
`artifacts/coverage/`. Verdict: PASS. No violations.

## 5. Out-of-Scope-Lock Edit Assessment

The executor modified `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs`, which is
listed in the plan Scope Lock only among the modified set indirectly through the FolderListBox
retype. Assessment: acceptable, mechanically necessary. Retyping `FolderListBox` from
`System.Windows.Forms.ListBox` to `BrightIdeasSoftware.TreeListView` in both Designers breaks
compilation (CS0029) of the test's `viewer.FolderListBox = new ListBox()` assignment. The test was
updated to construct a `TreeListView` and to inject the selected `FolderSuggestionNode` via
reflection (`SetPrivateField(formController, "_selectedNode", ...)`), because `TreeListView`
selection requires a native window handle unavailable in a headless run. The updated test drives the
same input contract the unit under test reads (`SelectedFolder => _selectedNode?.FullPath`) and does
not depend on live UI, COM, or temp files, so it remains deterministic and policy-compliant. Residual
note carried to the code review: the `SelectionChanged -> _selectedNode` wiring itself is exempt
controller glue exercised by build + manual QA, not by this unit test.

## 6. Rejected Scope Narrowing

None. The audit covered the full branch diff versus the resolved base branch. The caller's framing of
the repository-wide coverage floor as a pre-existing authority-scoped exception is a disposition of a
pre-existing condition, not a narrowing of the audit scope; every changed C# file was reviewed and an
explicit C# coverage verdict is recorded above. No plan/task/phase narrowing was applied. The plan
contains no injected directive requiring rejection.

## 7. Coverage Verdict Summary (per changed language)

| Language | Changed files | New/changed-code coverage | Repo-wide (evidence) | Verdict |
|---|---|---|---|---|
| C# / .NET | Yes | 96.43%-100% line coverage on new modules (>=90% target met) | 77.54% line (pre-existing below-floor, dispositioned non-blocking via exemption; +0.07 pts, no regression) | PASS |
| TypeScript | No | n/a | n/a | No changed files |
| Python | No | n/a | n/a | No changed files |
| PowerShell | No | n/a | n/a | No changed files |

## Overall Policy Verdict: PASS

No blocking findings. Two non-blocking observations (OBS-1 file size; a trivial exempt seam, see
code review). Blocking count contribution from this artifact: 0.
