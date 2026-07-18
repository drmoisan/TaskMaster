# Policy Audit — folder-hierarchy-live-provider (Issue #350)

- Timestamp: 2026-07-18T08-23
- Feature branch: feature/folder-hierarchy-live-provider-350 (HEAD 158ccb84)
- Base branch (three-dot merge-base): origin/epic/folder-tree-breadcrumb-redesign-integration (merge-base 3d322cfe)
- Work Mode: full-feature (AC sources: spec.md and user-story.md)
- Reviewer scope: full branch diff versus the resolved base. C# is the only language with changed files.

## Executive Summary

Overall verdict: PASS. Zero Blocking findings.

The branch adds one shared upstream contract (`IFolderHierarchyProvider`), one immutable DTO
(`FolderBreadcrumbSegment`), one facade implementation (`OutlookFolderHierarchyProvider`), and one
additive pure helper method (`FolderTreeSnapshotQueries.GetAncestorChain`), plus MSTest/Moq/
FluentAssertions unit tests. All four core-policy layers (CLAUDE.md, general-code-change,
general-unit-test, C# code/unit-test policies) are satisfied. The wave-0 mergeability boundary is
intact. Coverage is verified from the executor's Cobertura evidence and clears every applicable floor.

## Rejected Scope Narrowing

None. The caller directive is consistent with the full-branch-diff scope invariant. The coverage-hook
note in the directive instructs verification of C# coverage from the executor's Cobertura evidence
rather than regenerating a repo-wide `artifacts/csharp/coverage.xml`. That is an evidence-source
instruction, not a scope narrowing: C# coverage remains fully in scope and receives an explicit PASS
verdict below.

## Scope and Baseline

- Diff scope: `git diff origin/epic/folder-tree-breadcrumb-redesign-integration...HEAD`.
- Languages with changed files: C# only (9 changed files: 4 production `.cs`, 3 test `.cs`, 2 `.csproj`).
- New production files: `FolderBreadcrumbSegment.cs`, `IFolderHierarchyProvider.cs`,
  `OutlookFolderHierarchyProvider.cs`.
- Modified production file: `FolderTreeSnapshotQueries.cs` (additive `GetAncestorChain` only).
- New test files: `FolderBreadcrumbSegmentTests.cs`, `FolderTreeSnapshotQueriesAncestorChainTests.cs`,
  `OutlookFolderHierarchyProviderTests.cs`.

## 1. Policy Compliance Verdicts

### 1.1 General Code Change Policy

| Dimension | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | Provider is a thin read-only projection over the existing snapshot; no new indirection layer. |
| Reusability | PASS | Reuses `IOutlookFolderTreeService` / `FolderTreeSnapshot` rather than adding a second COM seam. |
| Extensibility | PASS | Public surface is the `IFolderHierarchyProvider` interface; consumers depend on the contract. |
| Separation of concerns | PASS | Pure ancestor-walk logic (`GetAncestorChain`) is separated from async snapshot acquisition. |
| Error handling (fail fast) | PASS | Constructors throw `ArgumentNullException` on null `key` / `treeService`; `GetAncestorChain` throws on null snapshot. |
| File size <= 500 lines | PASS | Largest changed file is a 282-line test; all production files are 53-97 lines. |
| Naming | PASS | PascalCase types/members, camelCase locals; descriptive names. |
| Public API / compatibility | PASS | Additive only; no existing public API altered or removed. |
| Dependencies | PASS | No new packages; `<Compile Include>` registrations only. |
| I/O boundary isolation | PASS | Live Outlook query stays behind the injected `IOutlookFolderTreeService` interface; provider is host-neutral and unit-tested without COM. |

### 1.2 General Unit Test Policy / C# Unit Test Policy

| Dimension | Verdict | Evidence |
|---|---|---|
| Framework MSTest | PASS | `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting` in all three test files. |
| Mocking Moq | PASS | `Mock<IOutlookFolderTreeService>` in `OutlookFolderHierarchyProviderTests`. |
| Assertions FluentAssertions | PASS | `Should()...` assertions throughout. |
| Independence / isolation | PASS | Each test builds its own in-memory snapshot; no shared mutable state. |
| Determinism | PASS | Deterministic cancellation via pre-cancelled `CancellationTokenSource`; no `Thread.Sleep`/`Task.Delay`/wall clock. |
| No external deps / no temp files | PASS | No live Outlook, COM, network, filesystem, or temp files. |
| AAA structure + intent docs | PASS | Explicit Arrange/Act/Assert comments and descriptive test names / class summaries. |
| Scenario completeness | PASS | Positive, negative (null/unknown key, empty set, not-found path), edge (leaf==root, cycle, duplicate names, duplicate paths), and cancellation flows present. |

#### 1.2.1 Coverage (per-language)

C# coverage (line coverage and branch coverage), verified from the executor Cobertura evidence at
`evidence/qa-gates/final-tests-coverage.md` and `evidence/qa-gates/coverage-delta.md`:

- C# repo-wide line coverage:
  - Baseline: 88.49% (35834/40496).
  - Post-change: 88.49% (35905/40573).
  - Change: +0.007 pt (no regression on the measured first-party denominator).
  - New/changed-code coverage: 96.92% (126/130 combined new production lines).
  - Disposition: PASS. Repo-wide line coverage 88.49% clears the 85% floor (and the CLAUDE.md 80% floor); new-code 96.92% clears the 90% new-code target.
  - Evidence: `evidence/qa-gates/final-tests-coverage.md`, `evidence/qa-gates/coverage-delta.md`.
- C# repo-wide branch coverage:
  - Baseline: 82.21% (8257/10044).
  - Post-change: 82.21% (8279/10070).
  - Change: +0.006 pt (no regression).
  - New/changed-code coverage: `OutlookFolderHierarchyProvider.cs` 83.33% branch; `FolderBreadcrumbSegment.cs` 100% branch; `GetAncestorChain` fully hit.
  - Disposition: PASS. Repo-wide branch coverage 82.21% clears the 75% branch floor; new-code branch coverage clears 75%.
  - Evidence: `evidence/qa-gates/coverage-delta.md`.

Coverage-artifact provenance note (procedural, non-blocking): the canonical repo-wide JaCoCo path
`artifacts/csharp/coverage.xml` is intentionally not present. vstest emits Cobertura, and generating a
trimmed repo-wide JaCoCo file at this path would be parsed against a 85% hard floor by the coverage
hook and could misrepresent the measured figure. C# line coverage and C# branch coverage are instead
verified from the per-feature Cobertura evidence captured during execution (report
`DanMoisan_MEGALODON4_2026-07-18.08_13_41.cobertura.xml`), which is the required evidence-verification
model. The numeric figures above are the authoritative C# coverage result and the verdict is PASS.

### 1.3 C# Code Change Policy

| Dimension | Verdict | Evidence |
|---|---|---|
| Format (csharpier) | PASS | `evidence/qa-gates/final-csharpier.md`: `csharpier check .` exit 0, no remaining diffs. |
| Lint / .NET analyzers | PASS | `evidence/qa-gates/final-analyzers.md`: build exit 0, 0 errors, zero new analyzer warnings on touched files. |
| Type-check / nullable | PASS | `evidence/qa-gates/final-nullable.md`: `TreatWarningsAsErrors=true` build exit 0, 0 warnings/0 errors. |
| MSTest test gate | PASS | `evidence/qa-gates/final-tests-coverage.md`: 4344/4344 passed, exit 0, +23 new tests. |
| net48 DTO constraint | PASS | `FolderBreadcrumbSegment` is a plain `sealed class` with constructor-set get-only properties; no `init`/`record`/`record struct`. |
| No new COM code / no new exemption | PASS | Diff contains no new `[ExcludeFromCodeCoverage]` and no `Microsoft.Office.Interop.Outlook` reference; dependency is the `IOutlookFolderTreeService` interface only. |

## 2. Scope-Lock / Wave-0 Mergeability Boundary

| Requirement | Verdict | Evidence |
|---|---|---|
| `FolderSuggestionTree.BuildFromRows` still exists, not deleted | PASS | Not in diff; present per `evidence/qa-gates/scope-boundary-check.md`. |
| `FolderHierarchyBuilder.Build` still exists, not deleted | PASS | Not in diff; present per scope-boundary evidence. |
| `EfcFormController.cs` not rewired | PASS | Not in the branch diff (`git diff --name-only` confirms absence). |
| `ItemViewer.FolderSearch.cs` not rewired | PASS | Not in the branch diff. |
| feature-324 probability plumbing untouched | PASS | No scoring/probability file in diff. |
| `FolderBreadcrumbSegment` probability-free | PASS | No `Probability`/`Score` member; only `Key`, `DisplayName`, `FolderPath`, `HasChildren`. |

## 3. GetAncestorChain Invariant Compliance

| Invariant | Verdict | Evidence |
|---|---|---|
| Root-first / leaf-last ordering | PASS | Walk collects leaf-to-root, then `chain.Reverse()`; verified by `GetAncestorChain_MultiLevel_...`. |
| Last element equals requested leaf | PASS | `chain.Last().Key.Should().Be(leafKey)` in single/multi-level tests. |
| Adjacent `child.ParentKey == parent.Key` | PASS | Asserted in `GetAncestorChain_MultiLevel_ReturnsRootFirstLeafLastWithLinkedPairs`. |
| Leaf == root -> single element | PASS | `GetAncestorChain_RootOnlyLeaf_ReturnsSingleElementChain`. |
| Null snapshot -> ArgumentNullException | PASS | Guard clause + `GetAncestorChain_NullSnapshot_ThrowsArgumentNullException`. |
| Null / unknown key -> empty list (never null) | PASS | `Array.Empty<...>()` return + two dedicated tests. |
| Identity by `FolderTreeNodeKey` | PASS | `GetAncestorChain_DuplicateDisplayNamesAtDifferentDepths_DistinguishedByKey`. |
| Cycle guard (visited-set) | PASS | `visited.Add` termination + `GetAncestorChain_CyclicParentKey_ReturnsPartialChainWithoutHanging`. |

## 4. Evidence Location Compliance

No changed file is written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or
`artifacts/coverage/`. All feature evidence resides under the canonical
`docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/<kind>/` path
(`baseline/`, `qa-gates/`). Branch-diff scan for banned evidence paths returned none. Verdict: PASS.

## 5. Tonality / Policy-Document Integrity

- No policy document under `.claude/rules/` or `.github/instructions/` was modified. PASS.
- Artifact wording is professional, factual, and evidence-first. PASS.

## 6. Assumptions

- PR context artifacts were absent at review start; a summary and appendix were regenerated from
  `git diff --numstat` against the resolved base so language enumeration reflects the actual diff.
- C# coverage figures are taken from the executor's committed Cobertura evidence, treated as the
  authoritative verification per the required evidence-verification model.

## 7. Coverage Verdict Roll-up

| Language | Changed files | Line coverage | Branch coverage | Verdict |
|---|---|---|---|---|
| C# coverage | 9 | 88.49% repo-wide / 96.92% new | 82.21% repo-wide / 83.33% new | PASS |

No other language has changed files on the branch.
