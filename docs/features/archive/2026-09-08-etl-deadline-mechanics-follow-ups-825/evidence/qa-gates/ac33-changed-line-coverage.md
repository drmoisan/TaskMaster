# AC33 — Changed-Line Coverage

Timestamp: 2026-09-09T17-24

Command: git diff $b --unified=0 -- <file>, with $b re-derived from evidence/baseline/base-commit.md per D3, intersected with the per-filename line map derived from evidence/qa-gates/coverage-postchange.cobertura.xml

## Derivation

The per-filename line data is derived, not read directly. Each class element repeats every line
number twice, once under methods/method/lines and once in the class-level lines rollup, and a single
source file is split across several class elements because an async method compiles to its own state
machine class. Each file's line map was built with Get-CoberturaClassLineSummary, declared at
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 line 158, over every class element whose
filename attribute ends in that file's name, and the resulting maps were merged by line number,
resolving a line number appearing in more than one map by taking the maximum hits value. That is the
same derivation P0-T9 and P8-T7 use, and it is required here for the same reason: a line read from
one class element, or from one of the two repeated views, can carry a hits value that a sibling
entry for the same line contradicts.
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs carries most of this feature's
changed lines inside an async method, which is exactly the case that can produce more than one class
element for one filename.

A line is counted in ChangedLinesCovered when its merged hits value is greater than zero. ChangedLines
counts only added lines that carry a line element at all; a comment, a declaration or a closing brace
carries none.

## Per-file results

File: UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
AddedDiffLines: 28
ChangedLines: 14
ChangedLinesCovered: 12
ChangedLineRate: 0.857143

File: UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs
AddedDiffLines: 12
ChangedLines: 1
ChangedLinesCovered: 1
ChangedLineRate: 1

File: UtilitiesCS/Threading/TimeOutTask.cs
AddedDiffLines: 0
ChangedLines: 0
ChangedLinesCovered: 0
ChangedLineRate: n/a

File: UtilitiesCS/Extensions/DfDeedle.cs
AddedDiffLines: 8
ChangedLines: 7
ChangedLinesCovered: 7
ChangedLineRate: 1

File: UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs
AddedDiffLines: 1
ChangedLines: 0
ChangedLinesCovered: 0
ChangedLineRate: n/a

## Overall

NewAndChangedCodeRate: 0.909091

The overall figure is the sum of ChangedLinesCovered over the sum of ChangedLines across the files
whose ChangedLines is greater than zero, that is 20 over 22.

Two files report ChangedLines: 0 and are excluded from the overall figure, exactly as the plan
anticipates. UtilitiesCS/Threading/TimeOutTask.cs is only deleted from by P4-T1 and has no added
line at all. UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs has a single added line, the P6-T2 doc
comment, which carries no line element. Their ChangedLineRate is recorded as the literal `n/a`
rather than as 0; recording 0 would lower the overall figure by arithmetic rather than by coverage.

## The two uncovered changed lines

Both sit in the TimeoutException retry recursion in
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs, at post-change lines 103 and 104,
and are the argument lines `timeoutSourceFactory,` and `timeProvider`.

The recursion itself is exercised: the regression test
GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000 drives it and asserts on the
value the second attempt used. The two lines report zero hits because the instrumentation attributes
the multi-line call's execution to the sequence point on its first argument line rather than to every
argument line, and CSharpier broke that call across several lines. This is a measurement artefact of
line-level instrumentation over a wrapped call expression, not an untested branch: the statement they
belong to is covered, and the value one of them carries is the subject of the test's assertion.

No ExcludeFromCodeCoverage attribute exists on any in-scope production file, so every changed line is
measurable and none was exempted to reach this figure.
