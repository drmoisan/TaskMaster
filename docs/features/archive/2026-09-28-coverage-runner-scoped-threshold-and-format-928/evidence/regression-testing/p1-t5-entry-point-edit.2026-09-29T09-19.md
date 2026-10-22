# P1-T5 Entry-Point Edit

Timestamp: 2026-09-29T09-19
Task: P1-T5
Command: Edit tool (three edits to scripts/vscode/Invoke-MSTestWithCoverage.ps1); Grep over the entry point; git diff --numstat 177b6d78e -- scripts/vscode/Invoke-MSTestWithCoverage.ps1; git diff --numstat 177b6d78e -- scripts/vscode
EXIT_CODE: 0

Edits applied (all inside Invoke-MSTestWithCoverageMain):
1. After the summary part-file dot-source, a two-line comment and the line `. (Join-Path $ScriptRoot 'Invoke-MSTestWithCoverage.Scope.ps1')` (now line 313).
2. The two assertion statements replaced by the eleven-line block from the Production Specification: the three-line comment, `if (Test-CoverageRunIsScoped -RepoRoot $repoRoot -ResolvedSearchRoot $resolvedSearchRoot)` with one `Write-Warning` built from two string literals joined with `+`, and an else arm holding the two existing assertion statements unchanged in text and order.
3. The help block gained a `.DESCRIPTION` paragraph and a `.PARAMETER SearchRoot` paragraph. The parameter paragraph contains the words `scoped`, `repository root` and `skipped`, and states the definition (full-path comparison, ordinal case-insensitive, trailing separators ignored; omitted value, dot and dot-backslash are unscoped; a scoped run skips the threshold assertions and writes exactly one warning naming the search root; an unscoped run keeps enforcing the 80 percent line and 75 percent branch floors unchanged).

Output Summary:
- Grep `\.DESCRIPTION` count: 6 (5 at the base anchor; expected 6).
- Grep `\.PARAMETER SearchRoot` count: 1 (line 284).
- Grep `Test-CoverageRunIsScoped -RepoRoot \$repoRoot -ResolvedSearchRoot \$resolvedSearchRoot` count: 1 (line 407).
- Grep `Assert-CoberturaLineCoverageThreshold -CoberturaXml \$processedXmlContent` count: 1 (line 412).
- Grep `Assert-CoberturaBranchCoverageThreshold -CoberturaXml \$processedXmlContent` count: 1 (line 413).
- The line-assertion call (line 412) is on the line immediately before the branch-assertion call (line 413).
- git diff --numstat 177b6d78e -- scripts/vscode/Invoke-MSTestWithCoverage.ps1: `29	2	scripts/vscode/Invoke-MSTestWithCoverage.ps1` (at least 20 added, exactly 2 deleted: met).
- git diff --numstat 177b6d78e -- scripts/vscode (verbatim; evaluated before P1-T7):
  `49	0	scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1`
  `29	2	scripts/vscode/Invoke-MSTestWithCoverage.ps1`
  Exactly two paths, the entry point and the new part file. Git also printed a line-ending advisory for the new part file (LF in the working copy, to be normalised to CRLF by the `* text=auto` attribute); it is an advisory, not a listed path.
- Derived entry-point size: 439 + 29 - 2 = 466 newline-terminated lines, under the 500-line ceiling.
