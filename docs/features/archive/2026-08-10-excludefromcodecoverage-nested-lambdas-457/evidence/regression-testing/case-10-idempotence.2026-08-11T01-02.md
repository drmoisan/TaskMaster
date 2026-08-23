# Regression case 10 — idempotence

Timestamp: 2026-08-11T01-02
Task: `[P1-T9]` `[expect-fail]`
Test file: `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`
Fully-qualified Pester name:
`Cobertura closure name derivation.is idempotent and silent when applied twice to the same document`

## Fixture (verbatim, inline here-string)

Reuses the case-4 shape, per the plan ("any fixture the filter modifies").

```xml
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T" filename="Ns\T.cs" line-rate="1" branch-rate="1" complexity="1">
      <methods><method name="Visible" signature="()" line-rate="1" branch-rate="1"><lines><line number="10" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="10" hits="1" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c" filename="Ns\T.cs" line-rate="0.5" branch-rate="0" complexity="3">
      <methods><method name="&lt;Exempt&gt;b__0_0" signature="()" line-rate="0" branch-rate="0"><lines><line number="20" hits="0" branch="False" /></lines></method><method name="&lt;Visible&gt;b__1_0" signature="()" line-rate="1" branch-rate="0"><lines><line number="21" hits="1" branch="False" /></lines></method><method name=".ctor" signature="()" line-rate="1" branch-rate="0"><lines><line number="22" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="20" hits="0" branch="False" /><line number="21" hits="1" branch="False" /><line number="22" hits="1" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
```

## Assertion (verbatim)

```powershell
$originalXml = $doc.OuterXml
$firstErrors = $null
$firstWarnings = $null
$firstInformation = $null
$secondErrors = $null
$secondWarnings = $null
$secondInformation = $null

$firstOutput = @(
    Remove-CoberturaExemptClosureCoverage -XmlDocument $doc `
        -ErrorVariable firstErrors `
        -WarningVariable firstWarnings `
        -InformationVariable firstInformation 4>&1
)
$afterFirstPass = $doc.OuterXml

$secondOutput = @(
    Remove-CoberturaExemptClosureCoverage -XmlDocument $doc `
        -ErrorVariable secondErrors `
        -WarningVariable secondWarnings `
        -InformationVariable secondInformation 4>&1
)
$afterSecondPass = $doc.OuterXml

# The first pass really does change the document, so idempotence is non-vacuous.
$afterFirstPass | Should -Not -Be $originalXml
# The second pass produces no further change.
$afterSecondPass | Should -Be $afterFirstPass
# Both invocations together emit nothing on any stream (verbose is merged into $*Output).
($firstOutput.Count + $secondOutput.Count) | Should -Be 0
(@($firstErrors).Count + @($secondErrors).Count) | Should -Be 0
(@($firstWarnings).Count + @($secondWarnings).Count) | Should -Be 0
(@($firstInformation).Count + @($secondInformation).Count) | Should -Be 0
```

Two properties are asserted in this single `It`:

1. **Idempotence.** `OuterXml` after two passes is identical to `OuterXml` after one. The additional
   `$afterFirstPass | Should -Not -Be $originalXml` assertion makes the property non-vacuous: it
   proves the filter genuinely modified the document, so the equality on the second pass is a real
   idempotence result and not the trivial equality of an untouched document.
2. **Silence.** The two invocations together emit zero objects on the success stream and zero records
   on the error, warning, verbose and information streams. Verbose (stream 4) is merged into the
   captured success output, so a verbose record would raise `$firstOutput.Count` and fail the
   assertion. This is part of the evidence for spec AC 11.

## Observed pre-implementation failure

EXIT_CODE: 1

```
FAIL: is idempotent and silent when applied twice to the same document
      => CommandNotFoundException : The term 'Remove-CoberturaExemptClosureCoverage' is not
         recognized as a name of a cmdlet, function, script file, or executable program.
```

Expected `[expect-fail]` reason.

## Passing result

Recorded by `[P3-T1]` (`<FEATURE>/evidence/regression-testing/pass-after-run.2026-08-11T01-30.md`).

- Timestamp: 2026-08-11T01-30
- Result: **Passed** (12ms)
- Test: `Cobertura closure name derivation.is idempotent and silent when applied twice to the same document`
- Run EXIT_CODE: 0 (Passed=29, Failed=0, Skipped=0 across both test files)
