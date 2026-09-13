# Phase 0 — Compile-Item Count Baseline

Timestamp: 2026-09-13T15-05
Task: [P0-T12]

Command: pwsh -Command '"UtilitiesCS.csproj Include: " + @(Select-String -Path "UtilitiesCS/UtilitiesCS.csproj" -Pattern "<Compile Include=" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.csproj Element: " + @(Select-String -Path "UtilitiesCS/UtilitiesCS.csproj" -Pattern "<Compile" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.Test.csproj Include: " + @(Select-String -Path "UtilitiesCS.Test/UtilitiesCS.Test.csproj" -Pattern "<Compile Include=" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.Test.csproj Element: " + @(Select-String -Path "UtilitiesCS.Test/UtilitiesCS.Test.csproj" -Pattern "<Compile" -SimpleMatch -CaseSensitive).Count'
EXIT_CODE: 0

UtilitiesCsIncludeCount: 492
UtilitiesCsElementCount: 492
UtilitiesCsTestIncludeCount: 478
UtilitiesCsTestElementCount: 478

Output Summary: the four counts are 492, 492, 478 and 478. Both observed values match the expected
post-merge baseline the plan states: 492 for both counts on `UtilitiesCS/UtilitiesCS.csproj` and 478
for both counts on `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. There is no divergence to report.

## What The Two Patterns Establish

The attribute-form pattern and the bare-element pattern agree on both files. Agreement establishes
that no Update form and no Remove form exists in either file, so a single attribute-form count is a
sound basis for the AC7 comparison that P2-T10 makes.

Agreement does not establish that every Compile element is self-closing, and the weaker reading is
recorded here so that no reviewer draws the stronger one. The Compile item naming the shared assembly
resolver source under the repository-root TestSupport directory, added by the fix for issue #877, is a
multi-line element carrying a Link child and a separate closing tag. Its closing tag is counted by
neither pattern, because the closing-tag text does not contain the opening-tag literal. The two counts
therefore agree at 478 while one of those 478 elements is not self-closing.

## Re-Measurement Note And Reconciliation History, Per D15

This artifact overwrites a superseded capture, which recorded 492, 492, 477 and 477. The test-project
figure has risen by one and the production-project figure is unchanged. The four values above are an
independent measurement of the post-merge tree, not an adjustment of the superseded figures by an
assumed delta.

The full reconciliation history, so that a reviewer can tell a stale figure from a corrected one:

- At authoring time the figures were 491 and 476.
- The first mandated reconciliation raised each by one to 492 and 477, because that merge added
  exactly one Compile item to each of the two project files.
- The second mandated reconciliation, which merged the main branch carrying the fix for issue #877,
  raised only the test-project figure, from 477 to 478. That fix added the Compile item naming the
  shared assembly resolver source to the test project alone and added no item to
  `UtilitiesCS/UtilitiesCS.csproj`, which therefore stays at 492. The measurement above confirms both
  halves of that statement independently.

The research artifact under this feature folder still records 491 and 476. That is correct as a
measurement of the tree at its own timestamp and is deliberately not rewritten; the figures above are
the operative ones.

## Consequence For AC7

P2-T10 requires each of the four counts to be exactly one lower than the corresponding value above,
that is 491, 491, 477 and 477, and requires the tracker-reference count to be zero. A fall of more
than one in either file means a sibling Compile item was dropped; a fall of zero means the item was
not removed.
