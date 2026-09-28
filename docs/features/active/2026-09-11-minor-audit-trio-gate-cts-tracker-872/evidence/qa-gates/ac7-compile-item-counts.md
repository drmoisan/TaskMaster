# AC7 — Compile-Item Counts After The Deletion

Timestamp: 2026-09-13T15-41
Task: [P2-T10]

Verdict: PASS

Command: pwsh -Command '"UtilitiesCS.csproj Include: " + @(Select-String -Path "UtilitiesCS/UtilitiesCS.csproj" -Pattern "<Compile Include=" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.csproj Element: " + @(Select-String -Path "UtilitiesCS/UtilitiesCS.csproj" -Pattern "<Compile" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.Test.csproj Include: " + @(Select-String -Path "UtilitiesCS.Test/UtilitiesCS.Test.csproj" -Pattern "<Compile Include=" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.Test.csproj Element: " + @(Select-String -Path "UtilitiesCS.Test/UtilitiesCS.Test.csproj" -Pattern "<Compile" -SimpleMatch -CaseSensitive).Count; "TrackerReferences: " + @(Select-String -Path "UtilitiesCS/UtilitiesCS.csproj","UtilitiesCS.Test/UtilitiesCS.Test.csproj" -Pattern "ProgressTrackerAsync" -SimpleMatch -CaseSensitive).Count'
EXIT_CODE: 0

## Measured Counts Against The P0-T12 Baseline

| Field | Baseline (P0-T12) | Post-change | Fall | Required |
|---|---|---|---|---|
| UtilitiesCsIncludeCount | 492 | 491 | 1 | exactly 1 |
| UtilitiesCsElementCount | 492 | 491 | 1 | exactly 1 |
| UtilitiesCsTestIncludeCount | 478 | 477 | 1 | exactly 1 |
| UtilitiesCsTestElementCount | 478 | 477 | 1 | exactly 1 |

TrackerReferences: 0

Output Summary: each of the four counts is exactly one lower than its baseline value, and no reference
to the deleted type's file name survives in either project file. A fall of more than one would mean a
sibling Compile item was dropped; a fall of zero would mean the item was not removed. Neither occurred.

## Why The Two Patterns Are Both Recorded

The attribute-form count and the bare-element count agree at every measurement, before and after. That
agreement establishes that no `Update` or `Remove` form of the Compile element exists in either file,
which is what makes a single attribute-form count a sound basis for the comparison.

Per P0-T12 the agreement no longer establishes that every Compile element is self-closing: the resolver
item that the #877 fix added to the test project is a multi-line element carrying a `Link` child and a
separate closing tag, and that closing tag is counted by neither pattern because the closing-tag text
does not contain the opening-tag literal. The two counts therefore agree at 477 while one of the 477
elements is not self-closing. That weaker reading is restated here so that a reviewer does not draw the
stronger one from the agreement.

## Derivation Integrity

The command printed five non-zero-or-expected values and no error. Had the quoting of the pattern
arguments been mangled, `Select-String` would have written a parameter-binding error to the error
stream and the counts would have come back as 0 while the surrounding statements still printed. Three
of the five values are 491 or 477 rather than 0, and the one value that is 0 — `TrackerReferences:` —
is corroborated independently: P2-T11 records that both named files are deleted from the tree, and the
analyzer and nullable rebuilds in P2-T3 and P2-T4 both succeeded, which they could not have done had
either project file still named a source file that no longer exists.
