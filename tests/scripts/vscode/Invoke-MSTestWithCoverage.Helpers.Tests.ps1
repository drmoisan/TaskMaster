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
}