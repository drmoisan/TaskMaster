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

    It 'synthesises a primary class when every group member carries a compiler-generated name' {
        # The primary selection prefers a group member whose name carries no angle bracket. When
        # every member is compiler-generated there is no such member, and the fallback must take
        # the first member of the group rather than leaving the primary unset. The two names below
        # carry an angle bracket but no '.<>c' marker, so the closure filter leaves them untouched
        # and the merge is the only transform under test.
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
        <class name="Ns.&lt;Alpha&gt;d__1" filename="C:\repo\Ns\Gen.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods><method name="MoveNext" signature="()" line-rate="0" branch-rate="0"><lines><line number="10" hits="1" branch="False" /></lines></method></methods>
          <lines><line number="10" hits="1" branch="False" /></lines>
        </class>
        <class name="Ns.&lt;Beta&gt;d__2" filename="C:\repo\Ns\Gen.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods><method name="MoveNextBeta" signature="()" line-rate="0" branch-rate="0"><lines><line number="20" hits="1" branch="False" /></lines></method></methods>
          <lines><line number="20" hits="1" branch="False" /></lines>
        </class>
      </classes></package></packages>
</coverage>
'@

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\' -ProjectNames @('Ns')
        $mergedClasses = @($resultXml.SelectNodes('//class[@filename="Ns\Gen.cs"]'))

        $mergedClasses.Count | Should -Be 1
        $mergedClasses[0].name | Should -Be 'Ns.<Alpha>d__1'
        @($mergedClasses[0].SelectNodes('./lines/line')).Count | Should -Be 2
    }

    It 'creates a methods element when the primary class has none' {
        # The primary class carries a <lines> rollup but no <methods> element at all, which is the
        # shape a collector emits for a type whose members were all filtered. The merge must
        # create the element before union-appending the other group member's methods, rather than
        # dropping them.
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
        <class name="Ns.Baz" filename="C:\repo\Ns\Baz.cs" line-rate="0" branch-rate="0" complexity="1">
          <lines><line number="5" hits="1" branch="False" /></lines>
        </class>
        <class name="Ns.&lt;Baz&gt;d__0" filename="C:\repo\Ns\Baz.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods><method name="MoveNext" signature="()" line-rate="0" branch-rate="0"><lines><line number="15" hits="1" branch="False" /></lines></method></methods>
          <lines><line number="15" hits="1" branch="False" /></lines>
        </class>
      </classes></package></packages>
</coverage>
'@

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\' -ProjectNames @('Ns')
        $mergedClass = $resultXml.SelectSingleNode('//class[@filename="Ns\Baz.cs"]')
        $methodNames = @(@($mergedClass.SelectNodes('./methods/method')) | ForEach-Object { $_.name })

        $mergedClass.name | Should -Be 'Ns.Baz'
        $methodNames.Count | Should -Be 1
        $methodNames[0] | Should -Be 'MoveNext'
    }

    It 'removes a stale condition-coverage attribute when the winning line carries none' {
        # Two group members contribute the same line number and neither carries a
        # condition-coverage attribute. The merged line must take the higher hits value and must
        # carry no condition-coverage attribute: the merge must not synthesise one, and no stale
        # attribute may survive onto a line whose winning entry has none.
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
        <class name="Ns.Qux" filename="C:\repo\Ns\Qux.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods /><lines><line number="7" hits="1" branch="False" /></lines>
        </class>
        <class name="Ns.&lt;Qux&gt;d__0" filename="C:\repo\Ns\Qux.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods /><lines><line number="7" hits="5" branch="False" /></lines>
        </class>
      </classes></package></packages>
</coverage>
'@

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\' -ProjectNames @('Ns')
        $mergedLines = @($resultXml.SelectNodes('//class[@filename="Ns\Qux.cs"]/lines/line'))

        $mergedLines.Count | Should -Be 1
        $mergedLines[0].hits | Should -Be '5'
        $mergedLines[0].HasAttribute('condition-coverage') | Should -BeFalse
    }

    It 'replaces the condition elements from the winning line' {
        # Both group members contribute the same line number with condition coverage, and the
        # second has the strictly larger denominator so it wins. The merged line must carry the
        # winning entry's condition-coverage attribute and exactly the winning entry's <condition>
        # children: the losing entry's condition elements must not survive alongside them.
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
        <class name="Ns.Zed" filename="C:\repo\Ns\Zed.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods /><lines><line number="9" hits="1" branch="True" condition-coverage="50% (1/2)"><conditions><condition number="0" type="jump" coverage="50%" /></conditions></line></lines>
        </class>
        <class name="Ns.&lt;Zed&gt;d__0" filename="C:\repo\Ns\Zed.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods /><lines><line number="9" hits="1" branch="True" condition-coverage="75% (3/4)"><conditions><condition number="0" type="jump" coverage="75%" /></conditions></line></lines>
        </class>
      </classes></package></packages>
</coverage>
'@

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\' -ProjectNames @('Ns')
        $mergedLines = @($resultXml.SelectNodes('//class[@filename="Ns\Zed.cs"]/lines/line'))

        $mergedLines.Count | Should -Be 1
        $mergedLines[0].GetAttribute('condition-coverage') | Should -Be '75% (3/4)'

        $mergedConditions = @($mergedLines[0].SelectNodes('./conditions/condition'))
        $mergedConditions.Count | Should -Be 1
        $mergedConditions[0].coverage | Should -Be '75%'
    }
}
