# No coverage exclusion and no PowerShell branch figure (P8-T7)

Timestamp: 2026-09-14T20-50

## Token search over the eight changed files

Tool: Grep semantics, case-sensitive literal search. The searches are case-sensitive, which is the Grep tool's default.

Files searched (8):

1. `.github/workflows/_mstest-coverage.yml`
2. `.github/workflows/_pester.yml`
3. `.github/workflows/ci.yml`
4. `.github/workflows/README.md`
5. `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`
6. `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
7. `scripts/vscode/Invoke-VSBuild.ps1`
8. `scripts/vscode/Invoke-Restore.ps1`

| Token | Match count across all eight files |
| --- | --- |
| `ExcludeFromCodeCoverage` | **0** |
| `CodeCoverage.ExcludeTests` | **0** |
| `BranchPercent` | **0** |
| `BRANCH` | **0** |

All four tokens record a zero match count.

The uppercase token `BRANCH` is searched deliberately and case-sensitively. The C# branch assertion legitimately reads the lowercase Cobertura attribute names `branch-rate`, `branches-valid` and `branches-covered`, and a case-insensitive search would count those as a PowerShell branch figure and produce a false positive. The case-sensitive search distinguishes them: the lowercase attribute reads are present in `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` and are correct there, while the uppercase JaCoCo counter type `BRANCH`, which is what a PowerShell branch figure would be read from, appears nowhere.

## Repository coverage settings file

`coverage.config` was read without modification. It is not in this delivery's write set and no task in this plan edits it.

Full exclude list, read verbatim from its `ModulePaths/Exclude` element:

```
<ModulePath>.*Deedle.*</ModulePath>
<ModulePath>.*FSharp.*</ModulePath>
<ModulePath>.*Castle\.Core.*</ModulePath>
<ModulePath>.*FluentAssertions.*</ModulePath>
<ModulePath>.*Moq.*</ModulePath>
<ModulePath>.*Microsoft\.Testing.*</ModulePath>
<ModulePath>.*MSTest.*</ModulePath>
```

Seven entries. **Every entry names a third-party module and none names a first-party production source path.** Deedle and FSharp are third-party F# libraries whose instrumentation breaks the tests that depend on them; Castle.Core, FluentAssertions and Moq are third-party test-support libraries; Microsoft.Testing and MSTest are the test platform itself. The file's own header comment states the purpose: to prevent instrumentation from breaking tests that depend on those libraries. No entry matches a path under a first-party project, and no entry was added, removed or altered by this delivery.

## The C# branch assertion is the only branch assertion in the delivery

The delivery adds exactly one branch assertion, `Assert-CoberturaBranchCoverageThreshold` in `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, and it applies to C# coverage only. It reads the document-root `branch-rate` and `branches-valid` attributes of the post-processed Cobertura document.

**No PowerShell branch assertion, threshold or reported figure exists anywhere in the delivery.** The Pester gate step in `.github/workflows/_pester.yml` reads only the `LINE` counter, prints only the LINE percentage and Pester's informational command percentage, and evaluates only the failure count and the LINE percentage. The zero match counts for `BranchPercent` and `BRANCH` above are the mechanical confirmation.

This is settled decision D4 of the specification. Pester measures no branch coverage in any output format; four independent repository sources agree, and the feature-review agent definition directs reviewers not to record a finding for an absent PowerShell branch figure, so emitting one would itself be treated as a policy violation.

## Coverage exclusion policy compliance

No production file is excluded from coverage measurement by this delivery. The Pester configuration sets `CodeCoverage.Path` to the directory `scripts/vscode`, which places every production script in that directory in the denominator, including the two whose measured coverage is zero after the seam fix — `Sync-PackageReferences.ps1` at 0 of 84 and `TestProcessCleanup.ps1` before this delivery's new tests. Leaving them in the denominator at a visible cost is what the Coverage Exclusion Policy in `.claude/rules/general-unit-test.md` requires, and the delivery raised the aggregate above the floor by adding tests rather than by narrowing the denominator.
