# [P2-T11] Production surface audit

Timestamp: 2026-08-11T01-26
Command: `git status --porcelain -uall` (with `.claude/agent-memory/` filtered out);
`git status --porcelain -uall -- scripts/vscode tests/scripts/vscode`;
`git diff -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
EXIT_CODE: 0

A `<MERGE_BASE>..HEAD` diff is deliberately NOT used: this plan contains no commit task, so that
diff is empty and the check would be vacuous. The changed-file set is computed from
`git status --porcelain -uall` at the repository root.

## Production surface — exactly as specified

| Path | State | Change |
|---|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | `??` (new) | new file |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | ` M` | two edits |

No third production file. No `.cs` file, no `coverage.config`, no `*.runsettings`, no `CLAUDE.md`,
and nothing under `.claude/rules/` is modified.

## Restricted changed-file listing (verbatim)

`git status --porcelain -uall -- scripts/vscode tests/scripts/vscode`:

```
 M scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
?? scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1
```

**This four-line listing is the value `[P3-T10]` re-measures and compares byte-for-byte.** The
whole-repository set is not used for that comparison, because it necessarily grows between this task
and `[P3-T10]` as Phase 3 writes new evidence artifacts under `<FEATURE>/evidence/`.

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` is absent from this listing, as required. No format
run has rewritten it: the `[P0-T6]` baseline format run measured
`FORMAT_SCAN_GRANULARITY: file-honored` and rewrote no file, so the restore branch was not taken and
is not needed here.

`tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Unit.Tests.ps1` is correctly absent:
the `[P1-T12]` pre-authorized split was not taken.

## Whole-repository changed-file set (`.claude/agent-memory/` filtered out)

```
 M docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/plan.2026-08-10T14-08.md
 M scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/async-d-state-machine-probe.2026-08-11T00-38.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/branch-commit-baseline.2026-08-11T00-14.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/coverage-baseline-extract.2026-08-11T00-30.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/coverage-collection.2026-08-11T00-30.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/csharp-build.2026-08-11T00-26.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/dependency-441-verification.2026-08-11T00-02.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/helpers-module-size.2026-08-11T00-14.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/nuget-restore.2026-08-11T00-24.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/pester-coverage.2026-08-11T00-20.xml
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/phase0-feature-documents-read.2026-08-11T00-12.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/phase0-instructions-read.2026-08-11T00-06.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/poshqc-analyze.2026-08-11T00-18.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/poshqc-format.2026-08-11T00-16.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/poshqc-test.2026-08-11T00-20.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/other/production-file-size.2026-08-11T01-24.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/regression-testing/case-01-exclude.2026-08-11T00-44.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/regression-testing/case-02-keep.2026-08-11T00-46.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/regression-testing/case-03-async-guard.2026-08-11T00-46.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/regression-testing/case-04-mixed-closure.2026-08-11T00-50.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/regression-testing/case-05-whole-class-removal.2026-08-11T00-50.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/regression-testing/case-06-pre-merge-ordering.2026-08-11T01-06.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/regression-testing/case-07-state-machine-untouched.2026-08-11T00-56.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/regression-testing/case-08-covered-closure-lines.2026-08-11T00-56.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/regression-testing/case-09-unit-purity.2026-08-11T01-02.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/regression-testing/case-10-idempotence.2026-08-11T01-02.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/regression-testing/expect-fail-run.2026-08-11T01-08.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/regression-testing/fixture-purity-audit.2026-08-11T01-10.md
?? docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/regression-testing/pester-coverage.2026-08-11T01-08.xml
?? scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1
```

Every path is accounted for:

- 2 production PowerShell files (1 new, 1 modified)
- 2 test PowerShell files (1 new, 1 modified)
- 1 feature document: the plan itself, carrying only `- [ ]` -> `- [x]` checkbox ticks
- 26 evidence artifacts under `<FEATURE>/evidence/`

The two `pester-coverage.<timestamp>.xml` files under `evidence/baseline/` and
`evidence/regression-testing/` are the expected feature-folder evidence written by the `[P0-T8]` and
`[P1-T11]` direct Pester runs, accounted for as such and not as unaccounted paths.

`ls coverage.xml` at the repository root returns "No such file or directory". A repo-root
`coverage.xml` would mean a direct Pester run omitted the mandatory `CodeCoverage.OutputPath`
redirection; none did.

`.claude/agent-memory/` is filtered out of the listing per the plan, because it is tracked and may be
written independently of this feature.

## The "exactly two edits" measurement (spec AC 13)

`git diff -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (verbatim):

```diff
diff --git a/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 b/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
index 2af80765..e3db4157 100644
--- a/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
+++ b/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
@@ -1,4 +1,5 @@
 Set-StrictMode -Version Latest
+. (Join-Path $PSScriptRoot 'Invoke-MSTestWithCoverage.ClosureFilter.ps1')

 function Get-KoverageProjectAllowlist {
     [CmdletBinding()]
@@ -423,6 +424,7 @@ function ConvertTo-KoverageCoberturaXml {
         $classNode.filename = ConvertTo-KoverageRelativePath -Path $classNode.filename -RepoRoot $RepoRoot -PathSeparator $PathSeparator
     }

+    Remove-CoberturaExemptClosureCoverage -XmlDocument $xml
     Merge-CoberturaClassesByFilename -XmlDocument $xml

     if (-not $xml.SelectSingleNode('//sources')) {
```

| Measure | Value |
|---|---|
| Added lines (`+`, excluding the `+++` header) | **2** |
| Removed lines (`-`, excluding the `---` header) | **0** |
| Baseline format hunks to exclude | none — `[P0-T6]` recorded `baseline format diff: empty` |

The two added lines are exactly the ones the plan specifies:

1. `. (Join-Path $PSScriptRoot 'Invoke-MSTestWithCoverage.ClosureFilter.ps1')` (edit 1, `[P2-T8]`)
2. `Remove-CoberturaExemptClosureCoverage -XmlDocument $xml` (edit 2, `[P2-T9]`)

A third added line, or any removed line, would fail this task. This diff measurement is the only
measurement in the plan that establishes spec AC 13's "exactly two edits";
`git status --porcelain -uall` reports only that the file is modified.

### Hard ordering criterion verified from the diff

The second hunk shows the call inserted immediately AFTER the `//class[@filename]`
path-normalization loop (whose closing `}` is the context line above the blank line) and immediately
BEFORE `Merge-CoberturaClassesByFilename -XmlDocument $xml`. The resulting order inside
`ConvertTo-KoverageCoberturaXml` is:

remove non-allowlisted `<package>` -> normalize `//class[@filename]` ->
**`Remove-CoberturaExemptClosureCoverage`** -> `Merge-CoberturaClassesByFilename` -> inject
`<sources>` -> `Get-CoberturaCoverageSummary` and write the document-level rate attributes.

No other call site of `Get-CoberturaCoverageSummary` is changed.

## Output Summary

Changed-file set is exactly 2 production PowerShell files, 2 test PowerShell files, the plan file
(checkbox ticks only), and 26 evidence artifacts. No `.cs`, no `coverage.config`, no `*.runsettings`,
no `CLAUDE.md`, nothing under `.claude/rules/`. The helpers-module diff is exactly 2 added lines and
0 removed lines, with no baseline format hunks to exclude. The hard ordering criterion is verified
from the diff. No repo-root `coverage.xml`.
