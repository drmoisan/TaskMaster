Set-StrictMode -Version Latest

BeforeAll {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $helperScriptPath = Join-Path $repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1'
    . $helperScriptPath
}

Describe 'Merge-CoberturaClassesByFilename' {
    It 'unions the methods of every group member into the merged class' {
        # Issue #733 finding 2: three classes share one file - the declaring class contributing
        # method M, and two distinct closure classes contributing N and O. Cloning the primary
        # class alone drops N and O from the merged report. Distinct group members never
        # legitimately share an identical method name (spec.md Assumptions), so the union needs no
        # deduplication key and each of the three names must survive exactly once.
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
        <class name="Ns.Foo" filename="C:\repo\Ns\Foo.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods><method name="M" signature="()" line-rate="0" branch-rate="0"><lines><line number="10" hits="1" branch="False" /></lines></method></methods>
          <lines><line number="10" hits="1" branch="False" /></lines>
        </class>
        <class name="Ns.Foo.&lt;&gt;c" filename="C:\repo\Ns\Foo.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods><method name="N" signature="()" line-rate="0" branch-rate="0"><lines><line number="20" hits="1" branch="False" /></lines></method></methods>
          <lines><line number="20" hits="1" branch="False" /></lines>
        </class>
        <class name="Ns.Foo.&lt;&gt;c__DisplayClass1_0" filename="C:\repo\Ns\Foo.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods><method name="O" signature="()" line-rate="0" branch-rate="0"><lines><line number="30" hits="0" branch="False" /></lines></method></methods>
          <lines><line number="30" hits="0" branch="False" /></lines>
        </class>
      </classes></package></packages>
</coverage>
'@

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\' -ProjectNames @('Ns')
        $mergedClass = $resultXml.SelectSingleNode('//class[@filename="Ns\Foo.cs"]')
        $methodNames = @(@($mergedClass.SelectNodes('./methods/method')) | ForEach-Object { $_.name })

        $methodNames.Count | Should -Be 3
        ($methodNames -join ',') | Should -Be 'M,N,O'
    }

    It 'takes the higher hits value when the second class seen for a filename is strictly higher' {
        # Issue #733 finding 4: closes a test-coverage gap on the max(hits) merge branch, which the
        # production code already handles correctly. Exactly two classes share one filename, they
        # overlap on exactly one line number, and only the hits value differs, with the
        # second-seen class strictly higher. Any implementation that kept the first-seen value, or
        # that took the last-seen value unconditionally, would be indistinguishable from max()
        # without this asymmetry.
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
        <class name="Ns.Bar" filename="C:\repo\Ns\Bar.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods /><lines><line number="42" hits="1" branch="False" /></lines>
        </class>
        <class name="Ns.BarNested" filename="C:\repo\Ns\Bar.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods /><lines><line number="42" hits="9" branch="False" /></lines>
        </class>
      </classes></package></packages>
</coverage>
'@

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\' -ProjectNames @('Ns')
        $mergedLines = @($resultXml.SelectNodes('//class[@filename="Ns\Bar.cs"]/lines/line'))

        $mergedLines.Count | Should -Be 1
        $mergedLines[0].hits | Should -Be '9'
    }
}
