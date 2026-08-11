# Regression case 2 — keep (required direction 2)

Timestamp: 2026-08-11T00-46
Task: `[P1-T2]` `[expect-fail]`
Test file: `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`
Fully-qualified Pester name:
`Remove-CoberturaExemptClosureCoverage.keeps closure lines whose declaring member is present in the instrumented method set`

## Fixture (verbatim, inline here-string)

```xml
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T" filename="Ns\T.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="Visible" signature="()" line-rate="0" branch-rate="0"><lines><line number="10" hits="1" branch="False" /><line number="11" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="10" hits="1" branch="False" /><line number="11" hits="1" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c__DisplayClass41_0" filename="Ns\T.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="&lt;Visible&gt;b__0" signature="()" line-rate="0" branch-rate="0"><lines><line number="406" hits="0" branch="False" /><line number="409" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="406" hits="0" branch="False" /><line number="409" hits="0" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
```

This is the case-1 shape with exactly one difference: the closure method is
`&lt;Visible&gt;b__0` rather than `&lt;Exempt&gt;b__0`, so its derived declaring member is `Visible`,
which IS present via the plain `<method name="Visible">` on the non-closure class `Ns.T`.

## Assertion (verbatim)

```powershell
Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

$summary = Get-CoberturaCoverageSummary -XmlDocument $doc

# Scoped to the closure class's own rollup: every fixture carries each line twice (once
# under its <method>, once in the class-level <lines>), so an unscoped //line count would
# be 2 and would not identify WHERE the line survived.
$closureLines = '//class[@name="Ns.T.<>c__DisplayClass41_0"]/lines/line'
@($doc.SelectNodes("$closureLines[@number=`"406`"]")).Count | Should -Be 1
@($doc.SelectNodes("$closureLines[@number=`"409`"]")).Count | Should -Be 1
$summary.LinesValid | Should -Be '4'
```

The closure lines survive and remain in `lines-valid` (4 = `Visible`'s two lines plus the closure's
two). Deleting them would be over-exclusion, which the fail-safe invariant forbids.

### Assertion correction recorded

The first draft asserted `@($doc.SelectNodes('//line[@number="406"]')).Count | Should -Be 1`. That
count is 2, not 1, and the assertion failed against a correct implementation at `[P2-T9]`. The cause
is the fixture shape mandated by the plan's Phase 1 preamble: every `<class>` carries a class-level
`<lines>` element **in addition to** its `<methods>/<method>/<lines>`, so line 406 legitimately
appears twice and the unscoped descendant-axis `//line` counts both copies. The assertion was
scoped to the closure class's own class-level rollup, which is both correct and stronger: it
identifies **where** the line survived rather than merely that some node with that number exists.
No assertion was weakened — the scoped form would still fail if the filter removed the line. The
same correction was applied to case 3.

## Observed pre-implementation failure

EXIT_CODE: 1

```
[-] keeps closure lines whose declaring member is present in the instrumented method set
 at <ScriptBlock>, tests\scripts\vscode\Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1:60
 CommandNotFoundException: The term 'Remove-CoberturaExemptClosureCoverage' is not recognized as a
 name of a cmdlet, function, script file, or executable program.
```

Expected `[expect-fail]` reason. Pester discovered 3 tests; the fixture parsed as XML before the
call was attempted.

## Passing result

Recorded by `[P3-T1]` (`<FEATURE>/evidence/regression-testing/pass-after-run.2026-08-11T01-30.md`).

- Timestamp: 2026-08-11T01-30
- Result: **Passed** (13ms)
- Test: `Remove-CoberturaExemptClosureCoverage.keeps closure lines whose declaring member is present in the instrumented method set`
- Run EXIT_CODE: 0 (Passed=29, Failed=0, Skipped=0 across both test files)
