# Phase 0 — Compile-Item Count Baseline

Timestamp: 2026-09-13T05-14
Task: [P0-T12]

Command: pwsh -Command '"UtilitiesCS.csproj Include: " + @(Select-String -Path "UtilitiesCS/UtilitiesCS.csproj" -Pattern "<Compile Include=" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.csproj Element: " + @(Select-String -Path "UtilitiesCS/UtilitiesCS.csproj" -Pattern "<Compile" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.Test.csproj Include: " + @(Select-String -Path "UtilitiesCS.Test/UtilitiesCS.Test.csproj" -Pattern "<Compile Include=" -SimpleMatch -CaseSensitive).Count; "UtilitiesCS.Test.csproj Element: " + @(Select-String -Path "UtilitiesCS.Test/UtilitiesCS.Test.csproj" -Pattern "<Compile" -SimpleMatch -CaseSensitive).Count'
EXIT_CODE: 0

UtilitiesCsIncludeCount: 492
UtilitiesCsElementCount: 492
UtilitiesCsTestIncludeCount: 477
UtilitiesCsTestElementCount: 477

Output Summary: all four counts were measured and all four match the plan's expected baseline exactly. The
expectation is 492 for both counts on `UtilitiesCS/UtilitiesCS.csproj` and 477 for both counts on
`UtilitiesCS.Test/UtilitiesCS.Test.csproj`. There is no divergence to report.

The command printed:

```
UtilitiesCS.csproj Include: 492
UtilitiesCS.csproj Element: 492
UtilitiesCS.Test.csproj Include: 477
UtilitiesCS.Test.csproj Element: 477
```

## Why The Two Patterns Are Both Counted

The two patterns agreeing establishes that every Compile element in each file uses the `Include`
attribute and that no `Update` or `Remove` form exists. That is what makes a single attribute-form count a
sound basis for the AC7 comparison: if an `Update` or `Remove` form existed, the element count would exceed
the include count and a post-change include-count comparison could miss a dropped sibling.

## Relationship To The Figures Recorded At Authoring Time

These two expectations were 491 and 476 when the plan was authored and were each raised by one when the
mandated reconciliation merge brought the current main branch into this item's branch: that merge added
exactly one Compile item to each of the two project files. The research artifact under this feature folder
still records 491 and 476, which is correct as a measurement of the tree at its own timestamp and is
deliberately not rewritten. The figures above are the operative ones.

The same merge also raised the QuickFiler test assembly's executed-test count by one relative to the figure
D14 records, which P0-T9 notes. The two observations have the same cause.

## AC7 Expectation For Phase 2

AC7 requires each of the two counts to fall by exactly one relative to the base commit, so the Phase 2
expectations are 491 on `UtilitiesCS/UtilitiesCS.csproj` and 476 on
`UtilitiesCS.Test/UtilitiesCS.Test.csproj`, with the include count and the element count still agreeing in
each file.

## Baseline Timing

Both project files are unmodified at the time of this measurement. Phase 1 has not started, and
`git status --porcelain --untracked-files=all -- "*.csproj"` produced no output earlier in Phase 0.
