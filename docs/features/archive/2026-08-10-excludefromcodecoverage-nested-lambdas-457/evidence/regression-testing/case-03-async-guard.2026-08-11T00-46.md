# Regression case 3 — keep, async guard

Timestamp: 2026-08-11T00-46
Task: `[P1-T3]` `[expect-fail]`
Test file: `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`
Fully-qualified Pester name:
`Remove-CoberturaExemptClosureCoverage.keeps closure lines whose declaring member exists only as an async state-machine class`

## Fixture (verbatim, inline here-string)

```xml
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T.&lt;Async&gt;d__33" filename="Ns\T.cs" line-rate="1" branch-rate="1" complexity="1">
      <methods><method name="MoveNext" signature="()" line-rate="1" branch-rate="1"><lines><line number="60" hits="1" branch="False" /><line number="61" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="60" hits="1" branch="False" /><line number="61" hits="1" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c__DisplayClass33_1" filename="Ns\T.cs" line-rate="1" branch-rate="1" complexity="1">
      <methods><method name="&lt;Async&gt;b__0" signature="()" line-rate="1" branch-rate="1"><lines><line number="50" hits="1" branch="False" /><line number="51" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="50" hits="1" branch="False" /><line number="51" hits="1" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
```

There is deliberately **no** plain `<method name="Async">` anywhere in the document. The declaring
member is present only as the state-machine class `Ns.T.&lt;Async&gt;d__33`, whose sole method is
`MoveNext`. Both classes carry the same `filename` and resolve to the same declaring type `Ns.T`, so
they share the presence-set key `Ns.T|Ns\T.cs`.

The lambda lines carry `hits="1"` (covered), matching the verified live counter-example
`BreadcrumbPopupUiOperations.<>c__DisplayClass33_1` / `33_2` (`line-rate="1"`) declared inside the
non-exempt async member `CreateAndInstallSurfaceAsync`.

## Assertion (verbatim)

```powershell
Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

$summary = Get-CoberturaCoverageSummary -XmlDocument $doc

# XPath predicates compare against PARSED attribute values, so the unescaped '<' and '>'
# characters are used here even though the fixture text carries the escaped entities.
# The line assertions are scoped to the closure class's own rollup, because each fixture
# line appears twice (once under its <method>, once in the class-level <lines>).
$closureLines = '//class[@name="Ns.T.<>c__DisplayClass33_1"]/lines/line'
@($doc.SelectNodes("$closureLines[@number=`"50`"]")).Count | Should -Be 1
@($doc.SelectNodes("$closureLines[@number=`"51`"]")).Count | Should -Be 1
@($doc.SelectNodes('//class[@name="Ns.T.<>c__DisplayClass33_1"]')).Count | Should -Be 1
$summary.LinesValid | Should -Be '4'
```

Assertion correction recorded: the first draft used the unscoped `//line[@number="50"]`, whose count
is 2 rather than 1 because the plan's Phase 1 preamble requires every `<class>` to carry a
class-level `<lines>` element in addition to its `<methods>/<method>/<lines>`. The assertion was
scoped to the closure class's own rollup, which is stronger — it identifies where the line survived —
and would still fail if the filter removed it. The XPath predicate was likewise corrected from the
XML-escaped `&lt;&gt;` form to the unescaped form, because XPath compares against parsed attribute
values.

The closure lines survive. This is the load-bearing second direction: it fails if the presence set
omits `d__` classes, which is why presence-set source (2) in `[P2-T4]` is mandatory rather than
optional.

Corroboration from `[P0-T12]`: the probe measured `Probe Answer: YES` — the collector does emit a
`Type.<Member>d__<N>` class for an attributed async member
(`QuickFiler.Controllers.QfcItemController.<ToggleExpansionAsync>d__203`). That confirms the shape
modelled by this fixture occurs in real collector output.

## Observed pre-implementation failure

EXIT_CODE: 1

```
[-] keeps closure lines whose declaring member exists only as an async state-machine class
 at <ScriptBlock>, tests\scripts\vscode\Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1:90
 CommandNotFoundException: The term 'Remove-CoberturaExemptClosureCoverage' is not recognized as a
 name of a cmdlet, function, script file, or executable program.
```

Expected `[expect-fail]` reason.

## Passing result

Recorded by `[P3-T1]` (`<FEATURE>/evidence/regression-testing/pass-after-run.2026-08-11T01-30.md`).

- Timestamp: 2026-08-11T01-30
- Result: **Passed** (10ms)
- Test: `Remove-CoberturaExemptClosureCoverage.keeps closure lines whose declaring member exists only as an async state-machine class`
- Run EXIT_CODE: 0 (Passed=29, Failed=0, Skipped=0 across both test files)
