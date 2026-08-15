# Regression case 8 — covered closure lines

Timestamp: 2026-08-11T00-56
Task: `[P1-T7]` `[expect-fail]`
Test file: `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`
Fully-qualified Pester name:
`Remove-CoberturaExemptClosureCoverage.removes covered closure lines from both the numerator and the denominator`

## Fixture (verbatim, inline here-string)

```xml
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T" filename="Ns\T.cs" line-rate="0.5" branch-rate="0" complexity="1">
      <methods><method name="Visible" signature="()" line-rate="0.5" branch-rate="0"><lines><line number="10" hits="1" branch="False" /><line number="11" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="10" hits="1" branch="False" /><line number="11" hits="0" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c__DisplayClass42_0" filename="Ns\T.cs" line-rate="1" branch-rate="1" complexity="1">
      <methods><method name="&lt;DisposeProductionSurface&gt;b__0" signature="()" line-rate="1" branch-rate="1"><lines><line number="80" hits="1" branch="False" /><line number="81" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="80" hits="1" branch="False" /><line number="81" hits="1" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
```

This is the verified live `<>c__DisplayClass42_0` / `DisposeProductionSurface` shape: the closure's
lines carry `hits="1"` and are therefore **covered**. The declaring class `Ns.T` carries a plain
`<method name="Visible">` on one covered and one uncovered line, so the post-filter summary has a
non-zero denominator and the reduced numerator is distinguishable. Without that declaring class the
closure class would be wholly removed, the case would duplicate case 5, and the recomputed-rate
assertion would have a zero denominator.

## Assertion (verbatim)

```powershell
$before = Get-CoberturaCoverageSummary -XmlDocument $doc

Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

$after = Get-CoberturaCoverageSummary -XmlDocument $doc

# Before the filter the covered closure lines inflate both totals.
$before.LinesValid | Should -Be '4'
$before.LinesCovered | Should -Be '3'
# After the filter they have left BOTH the numerator and the denominator.
$after.LinesValid | Should -Be '2'
$after.LinesCovered | Should -Be '1'
# The recomputed rate is consistent with the reduced numerator AND denominator.
$after.LineRate | Should -Be '0.5'
```

The before/after pair is the substantive content of this case. `LinesCovered` falls from 3 to 1 and
`LinesValid` from 4 to 2, so the corrected rate is 1/2 = 0.5, **not** 3/2. This is the mechanical
demonstration of the `spec.md` AC 14 requirement that a corrected per-file figure is not
`covered / (valid - n)`: the removed lines leave the numerator too.

Both summaries are read from `Get-CoberturaCoverageSummary`, never from the document-level
attributes, which the filter does not rewrite.

## Observed pre-implementation failure

EXIT_CODE: 1

```
FAIL: removes covered closure lines from both the numerator and the denominator
      => CommandNotFoundException
```

Expected `[expect-fail]` reason.

## Passing result

Recorded by `[P3-T1]` (`<FEATURE>/evidence/regression-testing/pass-after-run.2026-08-11T01-30.md`).

- Timestamp: 2026-08-11T01-30
- Result: **Passed** (38ms)
- Test: `Remove-CoberturaExemptClosureCoverage.removes covered closure lines from both the numerator and the denominator`
- Run EXIT_CODE: 0 (Passed=29, Failed=0, Skipped=0 across both test files)
