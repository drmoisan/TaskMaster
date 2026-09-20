# In-Place Corrections Cycle — Item-by-Item Discharge

- Timestamp: 2026-09-20T09-56-30
- Cycle: 2026-09-20T09-42 in-place corrections
- Source: `remediation-inputs.2026-09-20T09-42.md`, items R-C2-1 through R-C2-5
- Command: none; this artifact records the edits and the verification of each
- EXIT_CODE: 0
- ExpectedExitCode: 0

This is not a remediation cycle. The re-audit returned zero remediable blocking findings and a
Go decision. Neither completed plan was reopened and no acceptance criterion in `spec.md` was
amended, added or removed; it stands at 26 criteria with 23 checked.

## R-C2-1 — Major. README binding-redirect claim

**File:** `.github/workflows/README.md`, "Dependabot repair workflow".

The clause "or in an `app.config` binding redirect" was removed from the capability sentence and
replaced with a named, explained limit in a paragraph of its own, so a reader comparing the
README against the script does not conclude the README is merely incomplete. The new paragraph
states three things: that the repair script does carry the reconciliation pass; that the
`workflow_run` step supplies no `-CandidateUpgrade`, so the applied-upgrade set is always empty
and the pass never runs; and the condition under which the class becomes reachable again.

Verified in code from both ends, statically:

| Claim | Evidence |
|---|---|
| The script carries the pass | `scripts/dependencies/Repair-PackageManifestConsistency.ps1:428` calls `Invoke-BindingRedirectReconciliation` |
| The pass is gated on the applied set | line 420: `if ($appConfig.Count -eq 0 -or @($upgrade.Applied).Count -eq 0) { continue }` |
| The workflow supplies no `-CandidateUpgrade` | `.github/workflows/dependabot-repair.yml:79` invokes the script with no arguments |
| The parameter defaults to empty | line 56: `[ValidateNotNull()][hashtable]$CandidateUpgrade = @{}` |

**Gate rule 20.** The four facts above are properties of the source text and are verified
without a run. What remains unverifiable is the behaviour of `dependabot-repair.yml` itself:
that workflow has still never executed, so no observation confirms that the step reaches the
script, that the restore succeeds on the runner, or that an operator reading this README would
in fact encounter a stale redirect. The README paragraph asserts only the reachability property,
which is decidable from the source.

## R-C2-2 — Minor, latent. Reference-version guard agreement

**Files:** `scripts/dependencies/ProjectConsistency.psm1`,
`tests/scripts/dependencies/ProjectConsistency.Tests.ps1`.

`Resolve-ReferenceAssemblyVersion` now builds its pattern with
`[System.Text.RegularExpressions.RegexOptions]::IgnoreCase` and tolerates whitespace on **both**
sides of the `Include` name, not only after the opening quote. The trailing allowance was added
beyond what the finding asked for because `Get-RewrittenReferenceVersionLine` applies `.Trim()`
to the captured name before comparing, which strips both ends; matching only the leading side
would have left the two predicates disagreeing on a trailing-space input and reproduced the same
class of defect one input away.

The added test was observed failing before the fix and passing after:

```
[-] R5- preserves the declared version for an Include whose case differs from the manifest identifier
    at $assemblyVersion | Should -BeExactly '1.0.2'
    Expected: '1.0.2'
    But was:  ''
```

The empty string is exactly the mechanism the finding predicts: a case-sensitive resolver does
not see the declaration, returns empty, and `Invoke-VersionReconciliation` substitutes
`$ManifestVersion`. The test carries a positive control that supplies a version directly and
asserts the same `Include` **is** rewritten, which is what establishes the two guards would
genuinely disagree rather than both decline the input.

Exposure is unchanged and remains zero: no `<Reference Include="...">` element in this
repository diverges in case from its sibling manifest identifier today. The hazard was latent
and the fix is preventive.

## R-C2-3 — Minor. The R9c record made observable

**Files:** `scripts/dependencies/Repair-PackageManifestConsistency.ps1`,
`tests/scripts/dependencies/DependabotConfig.Tests.ps1`.

The `Write-Verbose` call in `$script:DefaultFileLister` is now
`Write-Information ... -InformationAction Continue`, the pattern
`scripts/vscode/Sync-PackageReferences.ps1` already uses for its own summary. The explicit
action is stated on the call, so visibility does not depend on the caller and the workflow
invocation needs no change. The comment was rewritten to say what the code does and why the
verbose stream was rejected.

The route not taken was adding `-Verbose` to the workflow step. It was rejected because it makes
the remedy's visibility a property of the caller, which is the failure this finding is about,
and because it would also surface the two unrelated `Write-Verbose` diagnostics at lines 127 and
405 that nothing asked for.

The existing test at `DependabotConfig.Tests.ps1` pinned the literal `Write-Verbose` and
asserted, in its `-Because` clause, the observability property the code did not have. It was
updated rather than left to fail: it now requires `Write-Information`, requires
`-InformationAction Continue`, and requires the absence of `Write-Verbose` in that block.

**Gate rule 20.** What is verified without a run: the source now calls `Write-Information` with
an explicit action, and the information stream is observed reaching a caller in the Pester run,
where `Sync-PackageReferences: All HintPaths are up to date` appears in the test output. What
remains unverifiable: that the record appears in a GitHub Actions run log for
`dependabot-repair.yml`, because that workflow has never executed. The property relied on —
that `-InformationAction Continue` emits irrespective of the caller's preference — is a
documented cmdlet behaviour and was observed locally, not inferred.

Scope is unchanged: all 18 `.csproj` in this repository sit at depth 1, so no project is skipped
today and the record exists for a future nested project.

## R-C2-4 — Minor. Coverage of line 410

**File:** `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`.

One test added, driving `Invoke-PackageReferenceSync` through a seam that lists one manifest
whose sibling project needs no repair. It asserts `ExaminedCount`, `FixedCount`, that the
`WriteText` delegate was never called, and the summary text captured from the information
stream.

The finding's line number was correct and its description of that line was not. Line 410 is the
zero-fix arm, not the non-zero-fix arm; the non-zero arm is 407 and was already covered. The
discrepancy and its consequence are recorded in
`evidence/qa-gates/rc2-t3-pester-coverage.2026-09-20T09-42.md`. The coverage outcome the finding
predicted, 105 of 127 at 82.68 percent, was reached.

## R-C2-5 — Minor. P3-T10 deletion table arithmetic

**File:**
`evidence/qa-gates/p3-t10-workflow-footprint.2026-09-20T01-37.md`.

The `[P3-T4]` row is now 1, and the prose reads "replaces one line rather than two". The column
sums to 5 and agrees with the total row and with the measured numstat.

Re-measured rather than taken from the review:
`git diff 794d34f02..HEAD -- .github/workflows/dependabot-repair.yml` emits exactly five deleted
content lines. The `WriteAllText` call is among them; `$updated = Join-Path $env:RUNNER_TEMP
'pr-body.md'` is not, and is unchanged context.

A dated corrigendum paragraph was added rather than rewriting the artifact silently, because the
artifact is a record of a prior cycle and a reader should be able to see that its arithmetic was
corrected and on what basis. The artifact's conclusion, its numstat and its decision to report
the clause unmet rather than adjust the change are untouched.

## File Size

No file in the write set exceeds the 500-line limit. The two closest are
`tests/scripts/dependencies/ProjectConsistency.Tests.ps1` at 494 and
`scripts/dependencies/Repair-PackageManifestConsistency.ps1` at 475.
`scripts/dependencies/ConsistencyVerifier.psm1` was **not** modified and remains at 499.

## Output Summary

All five corrections applied. R-C2-2 carries observed fail-before and pass-after evidence.
R-C2-1 and R-C2-3 are verified statically from both ends, with the limits of that verification
stated under gate rule 20. R-C2-4 exposed a factual error in the finding's description of line
410, which is recorded rather than propagated. R-C2-5 was re-measured before correction. No
acceptance criterion was amended and neither completed plan was reopened.
