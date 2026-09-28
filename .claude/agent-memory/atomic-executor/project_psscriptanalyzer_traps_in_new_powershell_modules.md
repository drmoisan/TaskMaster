---
name: psscriptanalyzer-traps-in-new-powershell-modules
description: Four PSScriptAnalyzer findings that a zero-owned-findings gate will produce on freshly authored .psm1/.Tests.ps1 in this repo, and the fix for each
metadata:
  type: project
---

Writing new PowerShell here under a "0 findings in owned files" gate reliably produces these
four. Measured on #911 Batch C: 12 owned findings on first analyze, all of these classes.

1. **`PSUseShouldProcessForStateChangingFunctions` on a pure function.** The rule fires on
   the *verb*, and `New`, `Set`, `Remove`, `Start`, `Stop`, `Restart`, `Reset` and `Update`
   are all on its list. `Update-PackageFolderSegment` was a pure string transform and still
   fired. Fix by naming the function for what it **returns** — `Get-RewrittenPackageFolderLine`
   — not by adding `SupportsShouldProcess` to a function with no state to change. This bites
   internal helpers too; the rule does not care about `Export-ModuleMember`.
2. **`PSReviewUnusedParameter` when the parameter is used only inside a nested scriptblock.**
   A `[regex]::Replace(..., { param($m) ... $PackageId ... })` MatchEvaluator, or a
   `Where-Object { $_.Name -eq $AssemblyName }`, does not count as a use. Bind the parameter
   to a local at statement level first (`$targetName = $AssemblyName`) and let the closure
   capture the local. In test fixtures whose delegate signature is the caller's contract but
   which consult only one parameter, add `$null = $PackageVersion`.
3. **`PSUseBOMForUnicodeEncodedFile` from a single em dash.** Repo PowerShell files carry no
   byte-order mark, so one U+2014 in a comment trips the rule. Keep new `.psm1`/`.ps1` pure
   ASCII rather than adding a mark and diverging from every sibling file. Scan with a
   per-character `[int]$ch -gt 126` loop; the finding reports no line number, so the rule
   name is the only clue.
4. **`PSUseOutputTypeCorrectly` on an array return.** `[OutputType([pscustomobject])]` with
   `return $list.ToArray()` fires, and so does `[OutputType([pscustomobject[]])]` with a bare
   `return @()`, because `@()` is `object[]`. Declare the array type **and** cast every
   return expression: `return [pscustomobject[]]@()`.

**How to apply:** the MCP `run_poshqc_analyze` tool reports a count only. Enumerate tuples
with `Invoke-ScriptAnalyzer -Path <folder> -Recurse` over the same folders — the direct run
reproduced the tool's total at both 25 and 13 on this run, so it is a faithful oracle. Run
it as a micro-action right after authoring a module, not at the phase's analyze gate: each
fix restarts the toolchain loop at the format step.

Corollary measured the same run: a function returning `[string[]]` with **one** element is
unrolled by PowerShell to a scalar string, so a test asserting `$result[0]` indexes the
string and reads its first character. The failure message reads `Expected ... but got .`,
which looks like a production bug and is not. Wrap with `@($result)[0]`.

**Confirmed again on #911 Batch D, and it detonated six tasks downstream.** No analyze step ran
between the Batch D test file being authored at P7-T2 and the Phase 9 analyze gate at P9-T2, so five
owned findings surfaced after every other gate had passed and forced a full loop restart. The two
classes were classes 1 and 2 above, both in a **`.Tests.ps1`** rather than a module: the in-memory
fixture builders `New-RepairFixture` and `New-StandardFixture` fired class 1 on the `New` verb
despite returning nothing but a hashtable, renamed to `Get-`; and three fixture parameters used only
inside `.GetNewClosure()` scriptblocks fired class 2, fixed by reading each into a body-level local
that the closure then captures. Neither fix touches an assertion. The gate is worth running as a
micro-action after authoring a **test** file too, not only a module.

Related: [[powershell-bom-required]], [[project_poshqc_analyze_exit1_on_warning]],
[[project_poshqc_format_strips_bom_and_crlf_only_when_it_rewrites]],
[[project_pester5_helper_function_must_live_in_beforeall]].
