# P0-T16 — Coverage Applicability Determination (Issue #751)

Timestamp: 2026-09-03T14-30

## Statement 1 — The coverage obligations that apply to C#

| Obligation | Source |
|---|---|
| Repository-wide line coverage must remain `>= 80%`; any new module, class, or method must target `>= 90%`; code changes must not reduce coverage for the lines that were changed. | `CLAUDE.md` § UT2 ("Coverage and Scenarios"), `CLAUDE.md:303`, `:310`, `:311` |
| Line coverage must remain `>= 85%` across all tiers; branch coverage `>= 75%` for languages whose tooling measures it; changes must not reduce coverage for the lines that were changed; test files are excluded from the denominator. | `.claude/rules/general-unit-test.md` § "Coverage Requirements", `:23`, `:24`, `:25`, `:28` |
| Uniform gate matrix across T1-T4: line coverage `>= 85%`, branch coverage `>= 75%`, and "No regression on changed lines". | `.claude/rules/quality-tiers.md` § "Uniform-vs-Tier-Dependent Gate Matrix", `:33`, `:34`, `:35` |

Coverage validation is therefore required for C# in this repository, and the atomic plan contract's Coverage
Evidence Contract requires explicit numeric baseline and final-QC coverage capture.

## Statement 2 — What this plan changes

This plan changes **three lines**, all of them in `TaskMaster.Test`:

1. `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` — one inserted barrier assertion
   (P2-T1).
2. `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` — one replaced counter assertion
   (P2-T2).
3. `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` — one replaced increment
   statement (P2-T3), at zero net line change.

No production file is changed.

## Statement 3 — Test files are excluded from the coverage denominator

Test files are excluded from the denominator by policy: `.claude/rules/general-unit-test.md:28` directs
"Configure coverage tooling to exclude test files (e.g., `tests/`) so metrics reflect application code, not
tests." `CLAUDE.md:302` states the same requirement.

The exclusion is also mechanical for the script route. `scripts/vscode/Invoke-MSTestWithCoverage.ps1:99-113`
injects the module exclusion `.*\.Test\.dll$` into the effective coverage settings, verified against the
current tree:

```
 99:     $testAssemblyPattern = '.*\.Test\.dll$'
...
109:     if ($existingTestExclusions.Count -eq 0) {
110:         $testAssemblyExclusion = $settings.CreateElement('ModulePath')
111:         $testAssemblyExclusion.InnerText = $testAssemblyPattern
112:         $null = $excludeNode.AppendChild($testAssemblyExclusion)
113:     }
```

All three lines this plan changes live in `TaskMaster.Test.dll`, which that pattern matches.

## Statement 4 — The changed-production-line set is empty

Spec AC4 (`spec.md:338-339`) forbids any production-file change: it requires
`TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs` to be byte-identical to its state at branch point
`f8414ee9` and requires the branch diff to contain no production-assembly file. **P4-T8 is its mechanical
gate.**

Consequently the set of changed production lines on this branch is empty, and the "no regression on changed
lines" requirement (`.claude/rules/quality-tiers.md:35`; `.claude/rules/general-unit-test.md:25`;
`CLAUDE.md:311`) has an **empty subject** and cannot be violated. P4-T11 re-derives this against the actual
branch diff after the change rather than restating it.

## Statement 5 — Why a numeric pair is not obtainable *through* `Invoke-MSTestWithCoverage.ps1`

A numeric pair obtained **through `scripts/vscode/Invoke-MSTestWithCoverage.ps1`** is not obtainable on a
suite carrying any pre-existing failure. Verified against the current tree:

- `:327` calls `Invoke-DotnetCoverageCollection`:

  ```
  327:     Invoke-DotnetCoverageCollection `
  328:         -OutputPath $resolvedOutputPath `
  329:         -CoverageConfig $coverageConfig `
  ```

- That function throws when the inner run exits non-zero, at `:235-237`:

  ```
  234:         $coverageExitCode = [int]$LASTEXITCODE
  235:         if ($coverageExitCode -ne 0) {
  236:             throw "MSTest with coverage failed with exit code $coverageExitCode"
  237:         }
  ```

- Because the throw propagates, the Koverage post-processing at `:339-342` and the threshold assertion at
  `:344` never execute, so no Cobertura file is produced:

  ```
  339:     Write-Output 'Post-processing coverage XML for Koverage compatibility...'
  340:     $xmlContent = Get-Content $resolvedOutputPath -Raw -Encoding UTF8
  341:     $processedXmlContent = ConvertTo-KoverageCoberturaXml -XmlContent $xmlContent -RepoRoot $repoRoot
  342:     Set-Content -Path $resolvedOutputPath -Value $processedXmlContent -Encoding UTF8 -NoNewline
  343: 
  344:     Assert-CoberturaLineCoverageThreshold -CoberturaXml $processedXmlContent
  ```

The accurate scope of this limitation is narrow. It concerns the script route only. It says nothing about
the `.coverage` attachments themselves, which the collector writes regardless of test outcomes.

## Statement 6 — Where this plan's numeric pair comes from

This plan therefore obtains its numeric pair from the `.coverage` attachments produced directly by the
P0-T14 and P4-T5 vstest runs. Every vstest invocation in this plan passes `/EnableCodeCoverage`, matching
`.github/workflows/_mstest-coverage.yml:99`, so each of those runs emits a `.coverage` attachment under its
own `/ResultsDirectory`. No additional full-suite collection is run, so the capture adds no second traversal
of the suite and does not depend on the suite being free of pre-existing failures.

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` is deliberately **not** invoked for the numeric pair.

## Recorded decision — no absolute coverage floor is asserted by this plan

**No absolute coverage floor is asserted by this plan.** This is a recorded decision, not an omission.

The repository floors are:

- `>= 80%` line coverage — `CLAUDE.md` § UT2, `CLAUDE.md:303`.
- `>= 85%` line and `>= 75%` branch coverage — `.claude/rules/general-unit-test.md` § "Coverage
  Requirements", `:23-24`.

Both are repository-wide standing obligations that this plan neither raises, lowers, nor supersedes. A
three-line, test-only change has no mechanism by which it could move a repository-wide figure toward or away
from any of them: the changed lines are excluded from the denominator by Statement 3, and the set of changed
production lines is empty by Statement 4.

The applicable obligation for a change of this shape is the **no-regression** obligation, which P4-T11 and
P4-T12 discharge.

## Numeric pair is captured, not waived

The numeric baseline/post-change coverage pair is **captured** by P0-T17 (baseline, from the P0-T14
attachment) and P4-T12 (post-change, from the P4-T5 attachment), and compared by P4-T12. It is explicitly
not waived by this determination. If a conversion cannot be performed, the corresponding artifact records the
exact error or the observed attachment count under the header `COVERAGE_CAPTURE_BLOCKED` and no number is
fabricated; in that case the completion report states the coverage criterion as remediation-required rather
than PASS.
