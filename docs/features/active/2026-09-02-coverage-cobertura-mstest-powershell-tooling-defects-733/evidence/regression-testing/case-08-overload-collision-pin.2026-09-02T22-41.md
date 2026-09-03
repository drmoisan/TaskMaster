# Case 08 — Bare-name overload collision, safe under-exclusion pin (P3-T3)

Timestamp: 2026-09-02T22-41

Task: [P3-T3]

Not tagged [expect-fail]. Per this plan's Phase 3 scope note, findings 5 and 6 are documentation
clarifications with no production behavior change, and this test pins the CURRENT, safe,
under-exclusion collision behavior rather than a fix. It therefore must pass immediately against
unchanged production code, which is what was observed.

## Change Made

Added one new It, "retains a closure whose bare member name collides with a non-exempt overload",
to the existing Describe 'Remove-CoberturaExemptClosureCoverage' block in
tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1.

Fixture: a declaring class `Ns.T` carrying a single plain method named `Overloaded` (representing
the non-exempt overload, which is the only one of the pair that emits a `<method>` element,
because the exempt overload emits none), plus a sibling closure class
`Ns.T.<>c__DisplayClass1_0` carrying the method `<Overloaded>b__0`.

Assertions after `Remove-CoberturaExemptClosureCoverage` runs: the closure class still exists, its
line 20 survives in its own class-level rollup, its single method is retained, and the document
summary reports LinesValid '2' and LinesCovered '1' — that is, the closure's uncovered line
remains in the denominator.

The It carries a comment citing issue #733 finding 6, stating that the failure direction pinned
here is the safe under-exclusion one (the exempt overload's lambda lines stay in the denominator
permanently uncovered) and not the forbidden over-exclusion one, and cross-referencing the P3-T2
docstring addendum for why a signature-based re-key is not proposed.

## Command

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with `Run.Path` =
tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 (absolute path within the
item worktree), `Run.PassThru = $true`, `Output.Verbosity = "Detailed"`, `Filter.FullName` =
"*retains a closure whose bare member name collides*", then the explicit trailing branch
`if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 0

## Observed Result

```
Describing Remove-CoberturaExemptClosureCoverage
  [+] retains a closure whose bare member name collides with a non-exempt overload 139ms (120ms|18ms)
```

Counts: Passed 1, Failed 0, Skipped 0 (11 NotRun, excluded by the name filter), Total 12.
Pester version 5.6.1.

## Output Summary

The new pinning test passes on its first run against unmodified production code, confirming the
documented collision behavior: the non-exempt overload's plain `<method>` element admits the bare
name `Overloaded` into the presence set, so the exempt overload's closure resolves as present and
its coverage is retained rather than removed. That is the safe under-exclusion direction. No
production code was changed by this task. Absolute host paths naming the item worktree were
replaced with their repository-relative equivalents in the captured Pester output.
