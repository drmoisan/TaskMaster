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

        [xml]$resultXml = ConvertTo-KoverageCoberturaXml -XmlContent $inputXml -RepoRoot 'C:\repo' -PathSeparator '\'
        $classNode = $resultXml.SelectSingleNode('//class')
        $sourceNode = $resultXml.SelectSingleNode('//sources/source')

        $classNode.filename | Should -Be 'QuickFiler.Test\Helper Classes\MailItemInfoTests.cs'
        $classNode.filename | Should -Not -Match '/'
        $sourceNode.InnerText | Should -Be '.'
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
}