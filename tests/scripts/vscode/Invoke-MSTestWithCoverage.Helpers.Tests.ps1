Set-StrictMode -Version Latest

BeforeAll {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $helperScriptPath = Join-Path $repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1'
    . $helperScriptPath
}

Describe 'ConvertTo-KoverageCoberturaXml' {
    It 'preserves backslash separators for nested Windows paths while making them workspace-relative' {
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages>
    <package name="QuickFiler.Test" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="QuickFiler.Test.MailItemInfoTests" filename="C:\repo\QuickFiler.Test\Helper Classes\MailItemInfoTests.cs" line-rate="0.5" branch-rate="1" complexity="1">
          <lines>
            <line number="10" hits="1" branch="False" />
            <line number="11" hits="0" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
'@

        # Supply ProjectNames explicitly so this path-normalization test does not
        # depend on the production allowlist, which now excludes '.Test' packages.
        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\' -ProjectNames @('QuickFiler.Test')
        $classNode = $resultXml.SelectSingleNode('//class')
        $sourceNode = $resultXml.SelectSingleNode('//sources/source')

        $classNode.filename | Should -Be 'QuickFiler.Test\Helper Classes\MailItemInfoTests.cs'
        $classNode.filename | Should -Not -Match '/'
        $sourceNode.InnerText | Should -Be '.'
    }

    It 'strips active and stale TaskMaster roots while preserving already relative paths' {
        $worktreeRoot = 'C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57'
        $canonicalRoot = 'C:\Users\DanMoisan\repos\TaskMaster'
        $relativeSource = 'ToDoModel\Data Model\ToDo\ToDoItem.cs'

        ConvertTo-KoverageRelativePath -Path "$canonicalRoot\$relativeSource" -RepoRoot $worktreeRoot -PathSeparator '\' |
            Should -Be $relativeSource
        ConvertTo-KoverageRelativePath -Path "$canonicalRoot\$relativeSource" -RepoRoot $canonicalRoot -PathSeparator '\' |
            Should -Be $relativeSource
        ConvertTo-KoverageRelativePath -Path $relativeSource -RepoRoot $worktreeRoot -PathSeparator '\' |
            Should -Be $relativeSource
    }

    It 'merges duplicate class entries that point to the same source file' {
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages>
    <package name="UtilitiesCS" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="UtilitiesCS.MeetingItemHelper" filename="C:\repo\UtilitiesCS\OutlookObjects\AppointmentItem\MeetingItemHelper.cs" line-rate="0.5" branch-rate="0" complexity="2">
          <methods />
          <lines>
            <line number="10" hits="1" branch="False" />
            <line number="11" hits="0" branch="False" />
          </lines>
        </class>
        <class name="UtilitiesCS.MeetingItemHelper.&lt;&gt;c" filename="C:\repo\UtilitiesCS\OutlookObjects\AppointmentItem\MeetingItemHelper.cs" line-rate="1" branch-rate="1" complexity="3">
          <methods />
          <lines>
            <line number="11" hits="1" branch="False" />
            <line number="12" hits="1" branch="True" condition-coverage="50% (1/2)">
              <conditions>
                <condition number="0" type="jump" coverage="50%" />
              </conditions>
            </line>
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
'@

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\'
        $classNodes = @($resultXml.SelectNodes('//class[@filename="UtilitiesCS\OutlookObjects\AppointmentItem\MeetingItemHelper.cs"]'))
        $line11 = $resultXml.SelectSingleNode('//class[@filename="UtilitiesCS\OutlookObjects\AppointmentItem\MeetingItemHelper.cs"]/lines/line[@number="11"]')
        $line12 = $resultXml.SelectSingleNode('//class[@filename="UtilitiesCS\OutlookObjects\AppointmentItem\MeetingItemHelper.cs"]/lines/line[@number="12"]')

        $classNodes.Count | Should -Be 1
        $classNodes[0].name | Should -Be 'UtilitiesCS.MeetingItemHelper'
        $classNodes[0].complexity | Should -Be '5'
        $line11.hits | Should -Be '1'
        $line12.branch | Should -Be 'True'
        $line12.'condition-coverage' | Should -Be '50% (1/2)'
    }

    It 'normalizes stale TaskMaster roots before merging duplicate production class entries' {
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages>
    <package name="ToDoModel" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="ToDoModel.ToDoItem" filename="C:\Users\DanMoisan\repos\TaskMaster\ToDoModel\Data Model\ToDo\ToDoItem.cs" line-rate="0.5" branch-rate="0" complexity="2">
          <methods />
          <lines>
            <line number="10" hits="1" branch="False" />
            <line number="11" hits="0" branch="False" />
          </lines>
        </class>
        <class name="ToDoModel.ToDoItem.&lt;&gt;c" filename="ToDoModel\Data Model\ToDo\ToDoItem.cs" line-rate="1" branch-rate="0" complexity="3">
          <methods />
          <lines>
            <line number="11" hits="1" branch="False" />
            <line number="12" hits="1" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
'@

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57' -PathSeparator '\'
        $classNodes = @($resultXml.SelectNodes('//class[@filename="ToDoModel\Data Model\ToDo\ToDoItem.cs"]'))
        $line11 = $resultXml.SelectSingleNode('//class[@filename="ToDoModel\Data Model\ToDo\ToDoItem.cs"]/lines/line[@number="11"]')

        $classNodes.Count | Should -Be 1
        $line11.hits | Should -Be '1'
        $resultXml.coverage.'lines-covered' | Should -Be '3'
        $resultXml.coverage.'lines-valid' | Should -Be '3'
        $resultXml.coverage.'line-rate' | Should -Be '1'
    }

    It 'excludes .Test packages from the report and from the aggregate covered/valid line totals' {
        # Regression for Issue #193: test assemblies must not be counted in the
        # numerator (lines-covered) or denominator (lines-valid). The production
        # package (UtilitiesCS) must be retained unchanged.
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages>
    <package name="UtilitiesCS" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="UtilitiesCS.Sample" filename="C:\repo\UtilitiesCS\Sample.cs" line-rate="0.5" branch-rate="0" complexity="1">
          <methods />
          <lines>
            <line number="10" hits="1" branch="False" />
            <line number="11" hits="0" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
    <package name="UtilitiesCS.Test" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="UtilitiesCS.Test.SampleTests" filename="C:\repo\UtilitiesCS.Test\SampleTests.cs" line-rate="1" branch-rate="0" complexity="1">
          <methods />
          <lines>
            <line number="20" hits="1" branch="False" />
            <line number="21" hits="1" branch="False" />
            <line number="22" hits="1" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
'@

        # ProjectNames resolved from the real repo via Get-KoverageProjectAllowlist
        # (default). UtilitiesCS is retained; UtilitiesCS.Test is excluded.
        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\'

        $packageNames = @($resultXml.SelectNodes('//package')) | ForEach-Object { $_.name }
        $packageNames | Should -Contain 'UtilitiesCS'
        $packageNames | Should -Not -Contain 'UtilitiesCS.Test'

        # Only the production package's two lines (one covered) remain. The three
        # covered lines from the test package are excluded from both totals.
        $resultXml.coverage.'lines-covered' | Should -Be '1'
        $resultXml.coverage.'lines-valid' | Should -Be '2'
    }

    It 'counts each source line once when methods repeat the class-level rollup' {
        # Regression for Issue #441. Assert counts, not the rate: the rate does not discriminate.
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
        <class name="Ns.Foo" filename="C:\repo\Ns\Foo.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods><method name="M" signature="()" line-rate="0" branch-rate="0"><lines><line number="10" hits="1" branch="False" /><line number="11" hits="0" branch="False" /><line number="12" hits="1" branch="False" /></lines></method></methods>
          <lines>
            <line number="10" hits="1" branch="False" />
            <line number="11" hits="0" branch="False" />
            <line number="12" hits="1" branch="False" />
          </lines>
        </class>
      </classes></package></packages>
</coverage>
'@

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\' -ProjectNames @('Ns')

        $resultXml.coverage.'lines-valid' | Should -Be '3'
        $resultXml.coverage.'lines-covered' | Should -Be '2'
        $resultXml.coverage.'line-rate' | Should -Be '0.666667'
    }

    It 'counts each branch line once when methods repeat the class-level rollup' {
        # Regression for Issue #441 branch arithmetic. The branch RATIO is unchanged by the
        # double count, so this must assert branches-valid/branches-covered, never branch-rate.
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
        <class name="Ns.Foo" filename="C:\repo\Ns\Foo.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods><method name="M" signature="()" line-rate="0" branch-rate="0"><lines><line number="10" hits="1" branch="False" /><line number="11" hits="0" branch="False" /><line number="12" hits="1" branch="True" condition-coverage="50% (1/2)"><conditions><condition number="0" type="jump" coverage="50%" /></conditions></line></lines></method></methods>
          <lines>
            <line number="10" hits="1" branch="False" />
            <line number="11" hits="0" branch="False" />
            <line number="12" hits="1" branch="True" condition-coverage="50% (1/2)">
              <conditions>
                <condition number="0" type="jump" coverage="50%" />
              </conditions>
            </line>
          </lines>
        </class>
      </classes></package></packages>
</coverage>
'@

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\' -ProjectNames @('Ns')

        $resultXml.coverage.'branches-valid' | Should -Be '2'
        $resultXml.coverage.'branches-covered' | Should -Be '1'
    }

    It 'computes the merged per-file line-rate from the merged rollup alone' {
        # Regression for Issue #478: miniature of the confirmed QfcHomeController.Iteration.cs
        # case. The merged rate must be 3/5 = 0.6, not the blended 6/8 = 0.75.
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
        <class name="Ns.Foo" filename="C:\repo\Ns\Foo.cs" line-rate="0" branch-rate="0" complexity="2">
          <methods><method name="M" signature="()" line-rate="0" branch-rate="0"><lines><line number="56" hits="1" branch="False" /><line number="57" hits="1" branch="False" /><line number="58" hits="1" branch="False" /></lines></method></methods>
          <lines>
            <line number="56" hits="1" branch="False" />
            <line number="57" hits="1" branch="False" />
            <line number="58" hits="1" branch="False" />
          </lines>
        </class>
        <class name="Ns.Foo.&lt;&gt;c" filename="C:\repo\Ns\Foo.cs" line-rate="0" branch-rate="0" complexity="3">
          <methods><method name="N" signature="()" line-rate="0" branch-rate="0"><lines><line number="12" hits="0" branch="False" /><line number="13" hits="0" branch="False" /></lines></method></methods>
          <lines>
            <line number="12" hits="0" branch="False" />
            <line number="13" hits="0" branch="False" />
          </lines>
        </class>
      </classes></package></packages>
</coverage>
'@

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\' -ProjectNames @('Ns')
        $mergedClass = $resultXml.SelectSingleNode('//class[@filename="Ns\Foo.cs"]')
        $mergedLines = @($mergedClass.SelectNodes('./lines/line'))

        $mergedClass.'line-rate' | Should -Be '0.6'
        $mergedLines.Count | Should -Be 5
        (@($mergedLines | ForEach-Object { $_.number }) -join ',') | Should -Be '12,13,56,57,58'
    }

    It 'deduplicates a repeated line number by taking the maximum hits value' {
        # Line 5 appears in two constructor overloads with differing hits and once in the
        # class-level rollup. It must count exactly once, and as covered.
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
        <class name="Ns.Foo" filename="C:\repo\Ns\Foo.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods><method name=".ctor" signature="()" line-rate="0" branch-rate="0"><lines><line number="5" hits="1" branch="False" /></lines></method><method name=".ctor" signature="(int)" line-rate="0" branch-rate="0"><lines><line number="5" hits="0" branch="False" /></lines></method></methods>
          <lines>
            <line number="5" hits="1" branch="False" />
          </lines>
        </class>
      </classes></package></packages>
</coverage>
'@

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\' -ProjectNames @('Ns')

        $resultXml.coverage.'lines-valid' | Should -Be '1'
        $resultXml.coverage.'lines-covered' | Should -Be '1'
    }

    It 'retains method-level lines when the class-level rollup element is absent' {
        # Guard: a bare child-axis switch would silently drop these two lines. Passes both
        # before and after the fix, and pins the union behaviour against that regression.
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
        <class name="Ns.Foo" filename="C:\repo\Ns\Foo.cs" line-rate="0" branch-rate="0" complexity="1">
          <methods><method name="M" signature="()" line-rate="0" branch-rate="0"><lines><line number="20" hits="1" branch="False" /><line number="21" hits="0" branch="False" /></lines></method></methods>
        </class>
      </classes></package></packages>
</coverage>
'@

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\' -ProjectNames @('Ns')

        $resultXml.coverage.'lines-valid' | Should -Be '2'
        $resultXml.coverage.'lines-covered' | Should -Be '1'
    }

    It 'preserves the primary class methods subtree and every hits value when merging' {
        # Locks the decision not to merge or strip <methods>. Reuses the F3 document.
        $inputXml = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
        <class name="Ns.Foo" filename="C:\repo\Ns\Foo.cs" line-rate="0" branch-rate="0" complexity="2">
          <methods><method name="M" signature="()" line-rate="0" branch-rate="0"><lines><line number="56" hits="1" branch="False" /><line number="57" hits="1" branch="False" /><line number="58" hits="1" branch="False" /></lines></method></methods>
          <lines>
            <line number="56" hits="1" branch="False" />
            <line number="57" hits="1" branch="False" />
            <line number="58" hits="1" branch="False" />
          </lines>
        </class>
        <class name="Ns.Foo.&lt;&gt;c" filename="C:\repo\Ns\Foo.cs" line-rate="0" branch-rate="0" complexity="3">
          <methods><method name="N" signature="()" line-rate="0" branch-rate="0"><lines><line number="12" hits="0" branch="False" /><line number="13" hits="0" branch="False" /></lines></method></methods>
          <lines>
            <line number="12" hits="0" branch="False" />
            <line number="13" hits="0" branch="False" />
          </lines>
        </class>
      </classes></package></packages>
</coverage>
'@

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\' -ProjectNames @('Ns')
        $mergedClass = $resultXml.SelectSingleNode('//class[@filename="Ns\Foo.cs"]')
        $methodNodes = @($mergedClass.SelectNodes('./methods/method'))
        $hitsByLine = @($mergedClass.SelectNodes('./lines/line')) | ForEach-Object { '{0}={1}' -f $_.number, $_.hits }

        $methodNodes.Count | Should -Be 1
        $methodNodes[0].name | Should -Be 'M'
        ($hitsByLine -join ',') | Should -Be '12=0,13=0,56=1,57=1,58=1'
    }

    It 'still throws when the document has no packages node' {
        # Error handling: the existing guard must survive the arithmetic rewrite verbatim.
        [xml]$doc = @'
<coverage line-rate="0" branch-rate="0" />
'@

        { Get-CoberturaCoverageSummary -XmlDocument $doc } |
            Should -Throw 'Cobertura XML does not contain a <packages> node.'
    }
}

Describe 'Get-KoverageProjectAllowlist' {
    It 'excludes projects that resolve to a .Test assembly name' {
        # Regression for Issue #193: no allowlist entry may match a '.Test' suffix.
        $allowlist = @(Get-KoverageProjectAllowlist)

        $allowlist | Should -Not -BeNullOrEmpty
        ($allowlist | Where-Object { $_ -match '\.Test$' }) | Should -BeNullOrEmpty
    }

    It 'retains non-test production projects in the allowlist' {
        # AC4: production packages must remain available for retention.
        $allowlist = @(Get-KoverageProjectAllowlist)

        $allowlist | Should -Contain 'UtilitiesCS'
    }

    It 'applies the .Test exclusion to the project-file base-name fallback' {
        # Exercises the fallback branch (no <AssemblyName> element): the resolved
        # name comes from the project file base name, and the '.Test' suffix
        # exclusion must still apply. Get-ChildItem / Get-Content are mocked so
        # the test is deterministic and touches no disk.
        Mock -CommandName Get-ChildItem -MockWith {
            @(
                [pscustomobject]@{ FullName = 'C:\fake\Sample\Sample.csproj'; Name = 'Sample.csproj' }
                [pscustomobject]@{ FullName = 'C:\fake\Sample.Test\Sample.Test.csproj'; Name = 'Sample.Test.csproj' }
            )
        }
        Mock -CommandName Get-Content -MockWith {
            # No <AssemblyName> element forces the base-name fallback path.
            '<Project Sdk="Microsoft.NET.Sdk"></Project>'
        }

        $allowlist = @(Get-KoverageProjectAllowlist -RepoRoot 'C:\fake')

        $allowlist | Should -Contain 'Sample'
        $allowlist | Should -Not -Contain 'Sample.Test'
    }
}

Describe 'Get-CoberturaClassLineSummary' {
    It 'retains the candidate condition-coverage when its total is greater' {
        # Precedence branch 1: the method-level denominator (4) exceeds the class-level one (2).
        [xml]$doc = @'
<class name="Ns.Foo" filename="Foo.cs">
  <methods><method name="M" signature="()"><lines><line number="5" hits="1" branch="True" condition-coverage="50% (2/4)" /></lines></method></methods>
  <lines>
    <line number="5" hits="1" branch="True" condition-coverage="100% (2/2)" />
  </lines>
</class>
'@

        $summary = Get-CoberturaClassLineSummary -ClassNode $doc.SelectSingleNode('//class')

        $summary.TotalBranches | Should -Be 4
        $summary.CoveredBranches | Should -Be 2
    }

    It 'retains the candidate condition-coverage when totals tie and its covered count is greater' {
        # Precedence branch 2: equal denominators (2), method-level covered (1) beats class-level (0).
        [xml]$doc = @'
<class name="Ns.Foo" filename="Foo.cs">
  <methods><method name="M" signature="()"><lines><line number="5" hits="1" branch="True" condition-coverage="50% (1/2)" /></lines></method></methods>
  <lines>
    <line number="5" hits="1" branch="True" condition-coverage="0% (0/2)" />
  </lines>
</class>
'@

        $summary = Get-CoberturaClassLineSummary -ClassNode $doc.SelectSingleNode('//class')

        $summary.TotalBranches | Should -Be 2
        $summary.CoveredBranches | Should -Be 1
    }

    It 'retains the existing condition-coverage when neither precedence condition holds' {
        # Precedence branch 3: method-level total (2) is smaller, so the class-level 2/4 is kept.
        [xml]$doc = @'
<class name="Ns.Foo" filename="Foo.cs">
  <methods><method name="M" signature="()"><lines><line number="5" hits="1" branch="True" condition-coverage="100% (2/2)" /></lines></method></methods>
  <lines>
    <line number="5" hits="1" branch="True" condition-coverage="50% (2/4)" />
  </lines>
</class>
'@

        $summary = Get-CoberturaClassLineSummary -ClassNode $doc.SelectSingleNode('//class')

        $summary.TotalBranches | Should -Be 4
        $summary.CoveredBranches | Should -Be 2
    }

    It 'returns zero totals for a class with neither a lines nor a methods element' {
        # Boundary: valid input per the helper contract; must yield zeros and must not throw.
        [xml]$doc = @'
<class name="Ns.Foo" filename="Foo.cs" />
'@
        $classNode = $doc.SelectSingleNode('//class')

        { Get-CoberturaClassLineSummary -ClassNode $classNode } | Should -Not -Throw
        $summary = Get-CoberturaClassLineSummary -ClassNode $classNode

        $summary.TotalLines | Should -Be 0
        $summary.CoveredLines | Should -Be 0
        $summary.TotalBranches | Should -Be 0
        $summary.CoveredBranches | Should -Be 0
    }
}
