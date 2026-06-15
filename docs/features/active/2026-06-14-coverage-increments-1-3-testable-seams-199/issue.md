# coverage-increments-1-3-testable-seams (Issue #199)

- Date captured: 2026-06-14
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/coverage-increments-1-3-testable-seams/ (Issue #199)

- Issue: #199
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/199
- Last Updated: 2026-06-14
- Work Mode: full-feature

## Problem / Why

The COM/VSTO coverage exemption (#197) made the coverage metric meaningful by scoping the denominator to genuinely-testable code, but the measured production-only rate (71.65%) is still below the 80% floor. The coverage roadmap (`artifacts/research/csharp-coverage-roadmap.2026-06-12.md` §6) identifies concrete, low-risk unit-test increments on already-testable seams that raise covered code toward the floor. This entry implements Increments 1–3.

## Proposed Behavior

Add MSTest unit tests (MSTest + Moq + FluentAssertions) for the testable seams that #197 deliberately preserved in the denominator. No production behavior change is expected; introduce the smallest seam only if strictly required and flag it.

- **Increment 1 — ToDoModel** (`ToDoModel.Test`): `ToDoLoader.SetAndSave<T>` (all overloads, read-only guard, null objectSetter/objectSaver paths); `IDList.GetNextToDoID(string)` (base case, ID-already-present loop, length-boundary); `ProjectEntry` (`SetProjectId` happy/null/malformed, `CompareTo` cases); `BaseChanger` remaining uncovered branches.
- **Increment 2 — QuickFiler** (`QuickFiler.Test`): keyboard-action value objects (`KaChar`, `KaKey`, `KaStringAsync`, `KaCharAsync`, `KaKeyAsync`), `KbdActions<>` remaining branches, `FilerQueue`, `QfcQueue` pure queue-management paths.
- **Increment 3 — TaskMaster** (`TaskMaster.Test`): `AppStagingFilenames` (injected settings stub), `AppFileSystemFolderPaths.MatchBestSpecialFolder` (pure LINQ), `AppQuickFilerSettings` remaining pure properties.

## Acceptance Criteria (early draft)

- [ ] Increment 1 tests added and passing; ToDoModel covered-line count increases for the named seams.
- [ ] Increment 2 tests added and passing; QuickFiler covered-line count increases for the named seams.
- [ ] Increment 3 tests added and passing; TaskMaster covered-line count increases for the named seams.
- [ ] All tests follow the General + C# Unit Test Policy (MSTest, Moq, FluentAssertions, AAA, deterministic, no temp files, no external dependencies).
- [ ] Full C# toolchain green (csharpier, analyzers, nullable, MSTest); no coverage regression on changed lines; new/changed code targets >= 90%.
- [ ] Production-only coverage re-measured and recorded; net increase vs the 71.65% post-#197 baseline.
- [ ] No exempted (COM/VSTO/WinForms) code is un-exempted or tested via live Outlook/WinForms.

## Constraints & Risks

- Tests only target seams #197 left measured; do not add tests that require a live Outlook process or WinForms message loop.
- Prefer no production change; if a minimal injectable seam is unavoidable (e.g., a `MyBox`/settings wrapper already present in source), use the smallest one and flag it.
- Large test-authoring effort across three assemblies; phase by increment.
- Determinism: no temp files, no mutable global state, no timing/sleep hacks (avoid the flaky-timing pattern tracked in #191/#176).

## Test Conditions to Consider

- [ ] Positive, negative, edge, and error scenarios per the General Unit Test Policy.
- [ ] Pure-logic/arithmetic boundaries (IDList base-36, BaseChanger, MatchBestSpecialFolder).
- [ ] Queue state transitions (FilerQueue, QfcQueue).

## Next Step

- [ ] Promote to GitHub issue (refactor template, full mode)
- [ ] Create active feature folder from the template