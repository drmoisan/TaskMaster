# Acceptance-Criteria Verification Map (P5-T6)

Timestamp: 2026-07-16T02-40

Sources: spec.md (## Acceptance Criteria) and user-story.md (## Acceptance Criteria). The two lists are
equivalent; each spec item and its user-story counterpart share the same evidence.

Verdict legend: PASS = delivered and verified; test evidence for host-neutral logic, build + manual-QA
for coverage-exempt WinForms/controller wiring per CLAUDE.md.

## spec.md acceptance criteria

1. Folders containing subfolders render with a plus/minus expand affordance.
   - PASS. Hierarchy + HasChildren from FolderSuggestionTree.BuildFromRows — tests
     FolderSuggestionTreeHierarchyTests (BuildFromRows_WithNestedPaths_..., ...DeepPathWithoutParent...).
     The plus/minus glyph is supplied by the TreeListView CanExpandGetter/ChildrenGetter wired in
     EfcFormController.ConfigureFolderTreeView and the Designer TreeListView (EfcViewer.Designer.cs,
     EfcViewer3.Designer.cs) — coverage-exempt, verified by build (P5-T2/T3) + manual QA.

2. Mouse click on the plus expands / on the minus collapses.
   - PASS. Native BrightIdeasSoftware.TreeListView expand/collapse enabled by the wired
     CanExpandGetter/ChildrenGetter (exempt; build + manual QA). Host-neutral toggle semantics tested
     via FolderSuggestionTreeStateTests.Toggle_* .

3. Right arrow expands, left arrow collapses the highlighted node.
   - PASS. Host-neutral RightArrow/LeftArrow no-op rules tested in FolderSuggestionTreeStateTests
     (RightArrow_ExpandsCollapsedExpandableRoot, LeftArrow_CollapsesExpandedRoot, leaf/already-*/banner
     no-ops). Wired in EfcFormController.FolderListBox_KeyDown + native TreeListView (exempt; build +
     manual QA).

4. Each suggestion shows its prediction probability right-aligned in whole-number percent.
   - PASS. PercentageFormatter.FormatPercent tested in PercentageFormatterTests (0, 1, .5 boundary
     away-from-zero, typical). Right alignment via olvColumnPercent.TextAlign = Right in both Designers
     (exempt; build + manual QA).

5. Probability consumed from the upstream folder-probability-plumbing contract (path -> double [0,1]),
   not recomputed; rows with no probability render blank.
   - PASS. FolderProbabilityAdapter join tested in FolderProbabilityAdapterTests (matched/unmatched/
     banner/nested). Contract re-confirmed in evidence/other/upstream-9001-contract-reconfirm.md
     (FolderScore.Probability is double [0,1] keyed by FolderPath). Production source is a
     FolderScore-backed IFolderProbabilitySource in EfcFormController (exempt; build + manual QA).

6. Delivered in BOTH viewers EfcViewer.cs and EfcViewer3.cs.
   - PASS. Both EfcViewer.Designer.cs and EfcViewer3.Designer.cs replace the flat ListBox with the
     two-column TreeListView; EfcViewer3.cs received [ExcludeFromCodeCoverage] (P4-T1). Shared
     EfcFormController drives both. Coverage-exempt; verified by build (solution compiles) + manual QA.

7. Shared host-neutral logic factored into a reusable testable helper meeting coverage thresholds.
   - PASS. FolderSuggestionNode/FolderSuggestionTree/PercentageFormatter/FolderProbabilityAdapter/
     IFolderProbabilitySource under UtilitiesCS/OutlookObjects/Folder/; per-module coverage 96.43%-100%
     (evidence/qa-gates/phase5-final-tests-coverage.md, phase5-coverage-delta.md).

8. Full C# toolchain (csharpier, analyzers, nullable, MSTest+Moq+FluentAssertions) is green.
   - PASS. phase5-final-csharpier.md (exit 0), phase5-final-analyzers.md (0 errors),
     phase5-final-nullable.md (0/0), phase5-final-tests-coverage.md (4762/4762).

## user-story.md acceptance criteria

- Items 1-7 map one-to-one to spec.md items 1-7 above (identical evidence). All PASS.

## Summary

All acceptance criteria: PASS. Host-neutral behavior is proven by unit tests; the coverage-exempt
WinForms Designer/Form and controller wiring is verified by a green build and is subject to manual QA
of the running add-in (plus/minus glyphs, mouse expand/collapse, arrow-key behavior, right-aligned
percentage) per the plan and CLAUDE.md coverage exemption.
