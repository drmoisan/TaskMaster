# Regression case 1 — exclude (required direction 1)

Timestamp: 2026-08-11T00-44
Task: `[P1-T1]` `[expect-fail]`
Test file: `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`
Fully-qualified Pester name:
`Remove-CoberturaExemptClosureCoverage.drops closure lines whose declaring member is absent from the instrumented method set`

Path note: the test file mirrors the production path `scripts/vscode/`;
`tests/scripts/powershell/` is not used, per `spec.md` § Test Strategy.

## Fixture (verbatim, inline here-string; no temporary file, no on-disk fixture, no `.cs` source)

```xml
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T" filename="Ns\T.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="Visible" signature="()" line-rate="0" branch-rate="0"><lines><line number="10" hits="1" branch="False" /><line number="11" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="10" hits="1" branch="False" /><line number="11" hits="1" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c__DisplayClass41_0" filename="Ns\T.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="&lt;Exempt&gt;b__0" signature="()" line-rate="0" branch-rate="0"><lines><line number="406" hits="0" branch="False" /><line number="409" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="406" hits="0" branch="False" /><line number="409" hits="0" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
```

Fixture conformance to the plan's Phase 1 preamble: class names are fully qualified
(`Ns.T.&lt;&gt;c__DisplayClass41_0` carries the `.<>c` marker; its declaring class is `Ns.T`), the
method name uses the escaped `&lt;Exempt&gt;b__0` form as emitted by the collector, and every
`<class>` carries a class-level `<lines>` element in addition to its `<methods>/<method>/<lines>`.

## Assertion (verbatim)

```powershell
Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

# The recomputed summary is read from Get-CoberturaCoverageSummary, never from the
# document-level attributes: the filter mutates the tree but does not rewrite them.
$summary = Get-CoberturaCoverageSummary -XmlDocument $doc

@($doc.SelectNodes('//line[@number="406"]')).Count | Should -Be 0
@($doc.SelectNodes('//line[@number="409"]')).Count | Should -Be 0
$summary.LinesValid | Should -Be '2'
$summary.LinesCovered | Should -Be '2'
```

Neither closure line survives, and the recomputed `lines-valid` counts only `Visible`'s two lines.

## Observed pre-implementation failure

EXIT_CODE: 1 (Pester: Passed=0, Failed=1)

```
[-] drops closure lines whose declaring member is absent from the instrumented method set
 at <ScriptBlock>, tests\scripts\vscode\Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1:29
 CommandNotFoundException: The term 'Remove-CoberturaExemptClosureCoverage' is not recognized as a
 name of a cmdlet, function, script file, or executable program.
```

This is the expected `[expect-fail]` reason: `CommandNotFoundException` on
`Remove-CoberturaExemptClosureCoverage`. It is not a Pester discovery error, a here-string syntax
error, or a malformed-XML harness error — Pester discovered 1 test and the fixture parsed as XML
before the call was attempted.

## Passing result

Recorded by `[P3-T1]` (`<FEATURE>/evidence/regression-testing/pass-after-run.2026-08-11T01-30.md`).

- Timestamp: 2026-08-11T01-30
- Result: **Passed** (155ms)
- Test: `Remove-CoberturaExemptClosureCoverage.drops closure lines whose declaring member is absent from the instrumented method set`
- Run EXIT_CODE: 0 (Passed=29, Failed=0, Skipped=0 across both test files)
