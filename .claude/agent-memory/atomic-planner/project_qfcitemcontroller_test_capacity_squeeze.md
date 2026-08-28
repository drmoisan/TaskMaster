---
name: project-qfcitemcontroller-test-capacity-squeeze
description: The four QfcItemController owned test files have only ~471 lines of aggregate 500-line headroom, so a multi-defect feature must compute the test-capacity budget in Phase 0 before assigning any test to a file
metadata:
  type: project
---

For work confined to `QuickFiler.Test/Controllers/QfcItemController.{FocusAndTheme,EventWiring,ViewerSetup,MailActions}Tests.cs`,
compute the aggregate 500-line headroom BEFORE assigning tests to files. Measured 2026-08-24:
497 / 374 / 474 / 184 lines, i.e. 3 / 126 / 26 / 316 spare, **471 aggregate and only 468 usable**
(the 3 lines in `FocusAndThemeTests.cs` cannot hold a test method).

**Why:** #484 (closes #480/#481/#483/#484/#485) needs roughly 443 lines of new MSTest+Moq+FluentAssertions
tests under CSharpier's 100-column wrapping — a ~25-line aggregate margin. The usual escape hatches are
all closed by the spec: no `.csproj` edit (the alphabetically-ordered `Compile Include` group at
`QuickFiler.Test.csproj:57-175` is shared with sibling epic children), so no new test file and no
`.Part2.cs`; and the changed-file set is itself an acceptance criterion. Moving existing tests between
owned files creates no capacity because the aggregate is the binding constraint.

**How to apply:**
1. Make the per-file 500-line cap the binding acceptance on every test-adding task, and state the
   aggregate arithmetic (headroom, planned addition, margin) in a Phase 0 budget artifact.
2. Treat the per-group file assignment as a starting allocation, and explicitly permit relocation to a
   different OWNED test file under a header comment naming the issue. A rigid per-file mandate is not
   plannable at a 25-line margin.
3. Mandate the compaction levers up front: one shared private arrange helper per test group, and a
   `[DataTestMethod]` with one `[DataRow]` per case where every case asserts the same outcome shape.
   Record that each `[DataRow]` surfaces as its own TRX result, which is what satisfies a spec clause
   reading "each case has its own regression test".
4. Define the escalation as a recorded blocker artifact — never a `.csproj` edit, a new file, or a
   file left above 500 lines.
5. Do NOT refactor the two pre-existing headless real-`ItemViewer` tests in `EventWiringTests.cs`
   (lines 229-309 and 319-372) to reclaim lines: an acceptance criterion counts real-`ItemViewer`
   constructions across the four files (baseline 4, expected 5), and extracting a shared fixture
   changes that arithmetic.

Sizing reference for the same area: a compact seam-and-inject MSTest case is ~13-18 lines; a
16-assertion Moq `VerifyRemove` block is ~52; a headless real-`ItemViewer` fixture test is ~80.

Related: [[literal-call-clauses-block-file-size-tightening]], [[project_400_partial_class_headroom_placement]],
[[feedback_postformat_file_size_audit]].
