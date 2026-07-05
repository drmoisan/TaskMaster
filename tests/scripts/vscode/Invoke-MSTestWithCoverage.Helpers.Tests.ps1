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
