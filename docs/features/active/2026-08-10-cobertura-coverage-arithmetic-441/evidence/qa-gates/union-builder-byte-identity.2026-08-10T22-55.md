# Union-Builder Byte Identity (P2-T5, re-verified post-format by P4-T10)

Timestamp: 2026-08-10T22-55

`spec.md` AC-8 and § Non-Goals item 5 require the merge union builder at pre-change
`Invoke-MSTestWithCoverage.Helpers.ps1:217-268` — including the child-axis selection
`$classNode.SelectNodes('./lines/line')` at pre-change `:219` — to remain byte-identical.
Editing `:219` would destroy the working half of the #478 merge.

## P2-T5 — pre-format verification

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
git diff HEAD -U0 -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | Select-String '^@@'
Select-String -LiteralPath 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1' -SimpleMatch `
    -Pattern 'foreach ($lineNode in @($classNode.SelectNodes(''./lines/line'')))'
```

EXIT_CODE: 0

Output Summary:

```
--- git diff HEAD -U0 hunk headers ---
@@ -122,11 +122,5 @@ function Get-CoberturaCoverageSummary {
@@ -166,0 +161,100 @@ function Get-CoberturaLineConditionCoverageParts {
@@ -270,4 +364,8 @@ function Merge-CoberturaClassesByFilename {
@@ -275,2 +373,2 @@ function Merge-CoberturaClassesByFilename {

--- union-builder literal occurrences ---
LITERAL foreach ($lineNode in @($classNode.SelectNodes('./lines/line')))
occurrences=1
    :313: foreach ($lineNode in @($classNode.SelectNodes('./lines/line'))) {
```

### Hunk old-side range analysis

The `HEAD` operand makes this check immune to staging.

| Hunk header | Old-side range | Intersects 217-268? |
| --- | --- | --- |
| `@@ -122,11 +122,5 @@` | 122-132 | no (ends at 132) |
| `@@ -166,0 +161,100 @@` | insertion after 166 (zero old lines) | no |
| `@@ -270,4 +364,8 @@` | 270-273 | no (starts at 270) |
| `@@ -275,2 +373,2 @@` | 275-276 | no |

**No hunk's old-side range intersects pre-change lines 217-268.** The union builder is untouched.
The nearest hunk on either side stops at old line 166 and resumes at old line 270, so lines 167-269
— which contain the entire union builder plus the blank line after it — are unmodified.

The literal `foreach ($lineNode in @($classNode.SelectNodes('./lines/line')))` still occurs
**exactly once**, now at line 313 (shifted down by the 100-line helper insertion at old line 166,
which is a pure relocation, not an edit).

---

## P4-T10 — post-format re-verification

Re-run **after** the P4-T1 PoshQC format step, to confirm the formatter did not reflow any line in
the protected range.

Timestamp: 2026-08-10T23-10

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
git diff HEAD -U0 -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | Select-String '^@@'
Select-String -LiteralPath 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1' -SimpleMatch `
    -Pattern 'foreach ($lineNode in @($classNode.SelectNodes(''./lines/line'')))'
```

EXIT_CODE: 0

Output Summary:

```
--- post-format hunk headers ---
@@ -122,11 +122,5 @@ function Get-CoberturaCoverageSummary {
@@ -166,0 +161,100 @@ function Get-CoberturaLineConditionCoverageParts {
@@ -270,4 +364,8 @@ function Merge-CoberturaClassesByFilename {
@@ -275,2 +373,2 @@ function Merge-CoberturaClassesByFilename {

LITERAL foreach ($lineNode in @($classNode.SelectNodes('./lines/line')))
occurrences=1
    :313: foreach ($lineNode in @($classNode.SelectNodes('./lines/line'))) {
```

The four hunk headers are **byte-identical to the P2-T5 pre-format capture**, and the union-builder
literal still occurs exactly once at line 313. No hunk's old-side range (122-132, insertion after
166, 270-273, 275-276) intersects pre-change lines **217-268**.

This is consistent with P4-T1, which recorded that the formatter changed **zero** files: the SHA-256
hash of `Invoke-MSTestWithCoverage.Helpers.ps1` was identical before and after the format call, so
no line anywhere in the file — inside or outside the protected range — was reflowed. No restoration
was needed and no restart from P4-T1 was triggered.

**AC-8 verdict: the union builder at pre-change `:217-268`, including the child-axis selection
`$classNode.SelectNodes('./lines/line')` at pre-change `:219`, is byte-identical to `main`.**
