# Regression case 7 — state machine untouched

Timestamp: 2026-08-11T00-56
Task: `[P1-T6]` `[expect-fail]`
Test file: `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`
Fully-qualified Pester name:
`Remove-CoberturaExemptClosureCoverage.leaves an async state-machine class untouched even when its member has no plain method`

## Fixture (verbatim, inline here-string)

```xml
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T.&lt;Foo&gt;d__1" filename="Ns\T.cs" line-rate="0.5" branch-rate="0.25" complexity="2">
      <methods><method name="MoveNext" signature="()" line-rate="0.5" branch-rate="0.25"><lines><line number="70" hits="1" branch="False" /><line number="71" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="70" hits="1" branch="False" /><line number="71" hits="0" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
```

There is deliberately no plain `<method name="Foo">` anywhere. Non-zero, non-identical `line-rate`
(0.5) and `branch-rate` (0.25) values are used so an accidental rate recomputation would be visible.

## Assertion (verbatim)

```powershell
# XPath predicates compare against PARSED attribute values, hence the unescaped '<Foo>'.
$beforeXml = $doc.SelectSingleNode('//class[@name="Ns.T.<Foo>d__1"]').OuterXml

Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

$after = $doc.SelectSingleNode('//class[@name="Ns.T.<Foo>d__1"]')

# Retained unchanged: the <lines> set and both rate attributes survive byte-for-byte.
$after | Should -Not -BeNullOrEmpty
$after.OuterXml | Should -Be $beforeXml
$after.'line-rate' | Should -Be '0.5'
$after.'branch-rate' | Should -Be '0.25'
@($after.SelectNodes('./lines/line')).Count | Should -Be 2
```

`Ns.T.&lt;Foo&gt;d__1` carries no `.<>c` marker, so `Test-CoberturaClosureClassName` rejects it
before any derivation is attempted and the class is never a candidate for mutation. The `OuterXml`
equality assertion pins the documented async residual in both directions: the class must not be
removed, and it must not be silently rewritten.

## Authoring correction recorded

The first draft of this assertion used the XPath predicate
`//class[@name="Ns.T.&lt;Foo&gt;d__1"]`, carrying the XML-escaped entities. That predicate matched
nothing, because XML entities are resolved at parse time and an XPath predicate compares against the
PARSED attribute value `Ns.T.<Foo>d__1`. The test consequently failed with `PropertyNotFoundException`
(`.OuterXml` on `$null` under `Set-StrictMode -Version Latest`) rather than the expected
`CommandNotFoundException`. The predicate was corrected to the unescaped form and the same correction
was applied to cases 3, 4 and 5, and to the two `Should -Contain` / `Should -Not -Contain` method-name
literals in case 4. The escaped forms remain in the fixture text, where they are required. Recording
this here because a `Should -Not -Contain '&lt;Exempt&gt;b__0_0'` assertion against an escaped literal
would have passed vacuously against any implementation.

## Observed pre-implementation failure

EXIT_CODE: 1

```
FAIL: leaves an async state-machine class untouched even when its member has no plain method
      => CommandNotFoundException
```

Expected `[expect-fail]` reason: `CommandNotFoundException` on
`Remove-CoberturaExemptClosureCoverage`.

## Passing result

Recorded by `[P3-T1]` (`<FEATURE>/evidence/regression-testing/pass-after-run.2026-08-11T01-30.md`).

- Timestamp: 2026-08-11T01-30
- Result: **Passed** (7ms)
- Test: `Remove-CoberturaExemptClosureCoverage.leaves an async state-machine class untouched even when its member has no plain method`
- Run EXIT_CODE: 0 (Passed=29, Failed=0, Skipped=0 across both test files)
