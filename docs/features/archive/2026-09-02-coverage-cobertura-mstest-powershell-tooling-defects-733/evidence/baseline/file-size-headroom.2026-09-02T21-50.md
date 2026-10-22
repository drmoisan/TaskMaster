# Phase 0 — File Size and Headroom Baseline (P0-T4)

Timestamp: 2026-09-02T21-50

Task: [P0-T4]

Ceiling: 500 lines per file, per the File Size Limit section of
.claude/rules/general-code-change.md and the Coding Standards section of
.claude/rules/powershell.md.

## Measurement Method

Two commands were run per file:

Command: `wc -l < <file>`
Command: `tr -cd '\n' < <file> | wc -c` and `tail -c 1 <file> | od -An -c`
EXIT_CODE: 0 (both)

Every one of the seven files ends with a trailing newline byte. For a file that ends with a
newline, the newline count equals the number of content lines, so the figures below are
content-line counts. A line-numbered viewer that renders a phantom empty line after the final
newline reports one more than this for the same file; that accounts for the plan's own
492-line figure for Invoke-MSTestWithCoverage.Helpers.ps1 against the 491 measured here. The
two figures describe the same file under two counting conventions and do not conflict. The
smaller headroom (the plan's, treating the file as 492 lines) is carried forward below as the
conservative value.

## Measured Line Counts and Headroom

| File | Lines | Headroom (500 - lines) |
|---|---|---|
| scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 491 | 9 |
| scripts/vscode/Invoke-MSTestWithCoverage.ps1 | 349 | 151 |
| scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | 389 | 111 |
| scripts/vscode/Invoke-MSTest.ps1 | 131 | 369 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | 498 | 2 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | 443 | 57 |
| tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 | 459 | 41 |

## Explicit Flag — Invoke-MSTestWithCoverage.Helpers.ps1

scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 is at or within 9 lines of the 500-line
ceiling: 491 lines measured here, 492 lines as measured during the planning pass, leaving 9
lines of headroom on the measured figure and only 8 lines on the planning-pass figure. Either
way there is not enough room to add a new function with its comment-based help inline.

Phase 1 addresses this by extracting the new `Get-CoberturaPackageLineSummary` helper into a
new sibling production file, scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1, rather
than adding it inline to Invoke-MSTestWithCoverage.Helpers.ps1 (plan tasks P1-T8 and P1-T9).
The 500-line ceiling is a hard, non-negotiable repository constraint that takes precedence
over spec.md's stated file-placement preference where the two conflict; spec.md's substantive
requirement (one new pure per-package rate helper, reused by both the document-level
summarizer and the merge function) is honored in full, and only its file placement is
adjusted.

## Second Constraint — Invoke-MSTestWithCoverage.Helpers.Tests.ps1

tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 is measured at 498 lines,
leaving 2 lines of headroom. This file is not named in the plan's stated size rationale, but
it is a binding constraint on Phase 1: P1-T5 and P1-T6 add new It cases to it, and P1-T4
edits an existing test in it. The plan already routes the new `Get-CoberturaPackageLineSummary`
Describe block (P1-T1, P1-T2) to the separate new file
tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1, which removes the
largest planned addition from this file. P1-T14's size check is the gate that confirms the
remaining additions still fit, and P1-T14's own acceptance text prescribes the remedy
(extracting the most recently added self-contained Describe block into a further sibling file)
if the resulting count exceeds 500.

## Output Summary

All seven files measured. Every one is currently at or under the 500-line ceiling. Two files
carry material size pressure into Phase 1:
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 at 491 lines (9 lines of headroom,
8 on the planning-pass count) and
tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 at 498 lines (2 lines of
headroom). Both are handled by Phase 1's split of the new helper and its Describe block into
sibling PackageRate files, with P1-T14 as the confirming gate.
