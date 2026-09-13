Set-StrictMode -Version Latest

BeforeAll {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $helperScriptPath = Join-Path $repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1'
    . $helperScriptPath

    $script:projectionScriptPath = Join-Path $repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.Projection.ps1'

    function Get-NormalizedProjectionText {
        <#
            .SYNOPSIS
            Reduces a projection string to line-feed line endings for comparison.

            .DESCRIPTION
            The projection is joined with the running platform's newline while a here-string literal
            in this file carries whatever line ending the file itself is stored with. Normalising
            both sides makes the exact-shape comparison depend on the characters the projection
            emits rather than on the storage convention of this test file. Every other character,
            including the two-space indentation and the space before each self-closing slash, is
            still compared exactly.
        #>
        [CmdletBinding()]
        [OutputType([string])]
        param(
            [Parameter(Mandatory = $true)]
            [AllowEmptyString()]
            [string]$Text
        )

        return $Text.Replace("`r`n", "`n")
    }

    function Get-ProjectionCounterElement {
        <#
            .SYNOPSIS
            Selects one counter element from a serialised projection by package name and type.

            .DESCRIPTION
            A test-only reader so each assertion names the package and the counter type it is
            interested in rather than repeating an XPath. It parses the projection string the
            function under test returned, which also proves the emitted text is well-formed XML.
        #>
        [CmdletBinding()]
        [OutputType([System.Xml.XmlElement])]
        param(
            [Parameter(Mandatory = $true)]
            [string]$ProjectionXml,

            [Parameter(Mandatory = $true)]
            [string]$PackageName,

            [Parameter(Mandatory = $true)]
            [string]$CounterType
        )

        [xml]$projectionDocument = $ProjectionXml
        $xpath = "/report/package[@name='$PackageName']/counter[@type='$CounterType']"

        return $projectionDocument.SelectSingleNode($xpath)
    }
}

Describe 'ConvertTo-JacocoPackageProjection' {
    It 'emits the exact projection document for a multi-package fixture' {
        # AC1. The fixture carries two packages. Alpha.Core holds three distinct line numbers of
        # which two have a nonzero hits value, and one of those is a branch line carrying one of two
        # conditions, so its figures are lines 2 of 3 and branches 1 of 2. Beta.Utility holds two
        # covered lines and no branch data, so its figures are lines 2 of 2 and branches 0 of 0.
        # JaCoCo reports missed rather than valid, so the expected counters are LINE missed 1
        # covered 2 and BRANCH missed 1 covered 1 for the first package, and LINE missed 0 covered 2
        # and BRANCH missed 0 covered 0 for the second.
        [xml]$script:sourceDocument = @'
<coverage line-rate="0.8" branch-rate="0.5" lines-covered="4" lines-valid="5" branches-covered="1" branches-valid="2">
  <packages>
    <package name="Alpha.Core" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="Alpha.Core.Widget" filename="Alpha.Core\Widget.cs" line-rate="0" branch-rate="0" complexity="1">
          <lines>
            <line number="10" hits="1" branch="False" />
            <line number="11" hits="0" branch="False" />
            <line number="12" hits="2" branch="True" condition-coverage="50% (1/2)" />
          </lines>
        </class>
      </classes>
    </package>
    <package name="Beta.Utility" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="Beta.Utility.Helper" filename="Beta.Utility\Helper.cs" line-rate="0" branch-rate="0" complexity="1">
          <lines>
            <line number="20" hits="3" branch="False" />
            <line number="21" hits="4" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
'@

        $projection = ConvertTo-JacocoPackageProjection -XmlDocument $script:sourceDocument

        $expected = @'
<report name="TaskMaster">
  <package name="Alpha.Core">
    <counter type="LINE" missed="1" covered="2" />
    <counter type="BRANCH" missed="1" covered="1" />
  </package>
  <package name="Beta.Utility">
    <counter type="LINE" missed="0" covered="2" />
    <counter type="BRANCH" missed="0" covered="0" />
  </package>
</report>
'@

        (Get-NormalizedProjectionText -Text $projection) |
            Should -BeExactly (Get-NormalizedProjectionText -Text $expected)
    }

    It 'derives missed as valid minus covered for lines and branches independently' {
        # AC2. Covered is strictly less than valid for both lines and branches: the single class
        # holds five distinct line numbers of which three have a nonzero hits value, and its one
        # branch line carries one of four conditions. The expected counters are therefore LINE
        # missed 2 covered 3 and BRANCH missed 3 covered 1. The two derivations are independent:
        # neither missed figure can be produced from the other counter's numbers.
        [xml]$script:sourceDocument = @'
<coverage line-rate="0.6" branch-rate="0.25" lines-covered="3" lines-valid="5" branches-covered="1" branches-valid="4">
  <packages>
    <package name="Gamma.Engine" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="Gamma.Engine.Runner" filename="Gamma.Engine\Runner.cs" line-rate="0" branch-rate="0" complexity="1">
          <lines>
            <line number="1" hits="7" branch="True" condition-coverage="25% (1/4)" />
            <line number="2" hits="1" branch="False" />
            <line number="3" hits="1" branch="False" />
            <line number="4" hits="0" branch="False" />
            <line number="5" hits="0" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
'@

        $packageSummary = Get-CoberturaPackageLineSummary -PackageNode $script:sourceDocument.SelectSingleNode('//package')
        $projection = ConvertTo-JacocoPackageProjection -XmlDocument $script:sourceDocument

        $lineCounter = Get-ProjectionCounterElement -ProjectionXml $projection -PackageName 'Gamma.Engine' -CounterType 'LINE'
        $branchCounter = Get-ProjectionCounterElement -ProjectionXml $projection -PackageName 'Gamma.Engine' -CounterType 'BRANCH'

        $lineCounter.GetAttribute('covered') | Should -BeExactly '3'
        $lineCounter.GetAttribute('missed') | Should -BeExactly '2'
        $branchCounter.GetAttribute('covered') | Should -BeExactly '1'
        $branchCounter.GetAttribute('missed') | Should -BeExactly '3'

        # Re-derived from the shared summariser so the assertion is the subtraction rule itself and
        # not a second copy of the literals above.
        [int]$lineCounter.GetAttribute('missed') |
            Should -Be ([int]$packageSummary.LinesValid - [int]$packageSummary.LinesCovered)
        [int]$branchCounter.GetAttribute('missed') |
            Should -Be ([int]$packageSummary.BranchesValid - [int]$packageSummary.BranchesCovered)
    }

    It 'emits zero line counters for a package with no class elements' {
        # AC8. A package whose classes element holds no class is valid input per the
        # Get-CoberturaPackageLineSummary contract, so the denominator is zero and the LINE counter
        # must still be emitted carrying zero missed and zero covered. The fixture's own stale
        # line-rate attribute is deliberately nonzero so a zero cannot come from copying the input.
        [xml]$script:sourceDocument = @'
<coverage line-rate="0.5" branch-rate="0.5" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages>
    <package name="Delta.Empty" line-rate="0.5" branch-rate="0.5" complexity="1">
      <classes />
    </package>
  </packages>
</coverage>
'@

        $projection = ConvertTo-JacocoPackageProjection -XmlDocument $script:sourceDocument

        $lineCounter = Get-ProjectionCounterElement -ProjectionXml $projection -PackageName 'Delta.Empty' -CounterType 'LINE'
        $lineCounter.GetAttribute('missed') | Should -BeExactly '0'
        $lineCounter.GetAttribute('covered') | Should -BeExactly '0'
    }

    It 'emits a zero BRANCH counter rather than omitting it when no branch data is present' {
        # AC7. Uniformity: a package with line data but no branch attribute anywhere must still
        # carry a BRANCH counter, so a consumer never has to distinguish an absent counter from a
        # zero one. The assertion pins the counter count at two as well as the zero figures,
        # because an implementation that omitted the branch counter would otherwise satisfy a
        # figures-only assertion by emitting nothing at all.
        [xml]$script:sourceDocument = @'
<coverage line-rate="1" branch-rate="0" lines-covered="2" lines-valid="2" branches-covered="0" branches-valid="0">
  <packages>
    <package name="Epsilon.NoBranch" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="Epsilon.NoBranch.Plain" filename="Epsilon.NoBranch\Plain.cs" line-rate="0" branch-rate="0" complexity="1">
          <lines>
            <line number="1" hits="1" branch="False" />
            <line number="2" hits="1" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
'@

        $projection = ConvertTo-JacocoPackageProjection -XmlDocument $script:sourceDocument

        [xml]$projectionDocument = $projection
        @($projectionDocument.SelectNodes("/report/package[@name='Epsilon.NoBranch']/counter")).Count |
            Should -Be 2

        $branchCounter = Get-ProjectionCounterElement -ProjectionXml $projection -PackageName 'Epsilon.NoBranch' -CounterType 'BRANCH'
        $branchCounter | Should -Not -BeNullOrEmpty
        $branchCounter.GetAttribute('missed') | Should -BeExactly '0'
        $branchCounter.GetAttribute('covered') | Should -BeExactly '0'
    }

    It 'emits one package element per source package in document order' {
        # AC4. The three fixture package names are deliberately not in alphabetical order, so a
        # projection that sorted its output would fail this assertion rather than pass it by
        # coincidence. The fifteen-package set of the committed reference instance is not reproduced
        # here, because that instance was converted from a raw collector document rather than from a
        # post-processed one.
        [xml]$script:sourceDocument = @'
<coverage line-rate="1" branch-rate="0" lines-covered="3" lines-valid="3" branches-covered="0" branches-valid="0">
  <packages>
    <package name="Zeta.First" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="Zeta.First.One" filename="Zeta.First\One.cs" line-rate="0" branch-rate="0" complexity="1">
          <lines>
            <line number="1" hits="1" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
    <package name="Alpha.Second" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="Alpha.Second.Two" filename="Alpha.Second\Two.cs" line-rate="0" branch-rate="0" complexity="1">
          <lines>
            <line number="1" hits="1" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
    <package name="Mid.Third" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="Mid.Third.Three" filename="Mid.Third\Three.cs" line-rate="0" branch-rate="0" complexity="1">
          <lines>
            <line number="1" hits="1" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
'@

        $projection = ConvertTo-JacocoPackageProjection -XmlDocument $script:sourceDocument

        [xml]$projectionDocument = $projection
        $packageElements = @($projectionDocument.SelectNodes('/report/package'))
        $packageElements.Count | Should -Be 3

        $emittedNames = @($packageElements | ForEach-Object { $_.GetAttribute('name') })
        $emittedNames[0] | Should -BeExactly 'Zeta.First'
        $emittedNames[1] | Should -BeExactly 'Alpha.Second'
        $emittedNames[2] | Should -BeExactly 'Mid.Third'
    }

    It 'throws the existing missing-packages wording without introducing a second wording' {
        # AC6. The expected message is obtained by invoking the first-party helper on the same
        # fixture and capturing what it throws, rather than by restating the literal here. Restating
        # it would make this test pass even if the two wordings had drifted apart, because the test
        # would then be comparing the projection against a third copy of the text rather than
        # against the wording the repository actually uses.
        [xml]$script:sourceDocument = @'
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" />
'@

        $projectionMessage = $null
        try {
            $null = ConvertTo-JacocoPackageProjection -XmlDocument $script:sourceDocument
        }
        catch {
            $projectionMessage = $_.Exception.Message
        }

        $firstPartyMessage = $null
        try {
            # An explicit ProjectNames value is supplied so the parameter default, which enumerates
            # every project file in the repository, is never evaluated.
            $null = Get-CoberturaFirstPartyCoverageSummary -XmlDocument $script:sourceDocument -ProjectNames @('Alpha.Core')
        }
        catch {
            $firstPartyMessage = $_.Exception.Message
        }

        $firstPartyMessage | Should -Not -BeNullOrEmpty
        $projectionMessage | Should -Not -BeNullOrEmpty
        $projectionMessage | Should -BeExactly $firstPartyMessage
    }
}

Describe 'Assert-JacocoProjectionReconciliation' {
    It 'returns without throwing when the projection totals equal the source root attributes' {
        # AC5, positive path. The fixture's root declares four covered lines of five valid, and the
        # two packages contribute LINE covered 2 and 2 and LINE missed 1 and 0, so the summed
        # projection figures are covered 4 and valid 5 and both equalities hold exactly.
        [xml]$script:sourceDocument = @'
<coverage line-rate="0.8" branch-rate="0.5" lines-covered="4" lines-valid="5" branches-covered="1" branches-valid="2">
  <packages>
    <package name="Alpha.Core" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="Alpha.Core.Widget" filename="Alpha.Core\Widget.cs" line-rate="0" branch-rate="0" complexity="1">
          <lines>
            <line number="10" hits="1" branch="False" />
            <line number="11" hits="0" branch="False" />
            <line number="12" hits="2" branch="True" condition-coverage="50% (1/2)" />
          </lines>
        </class>
      </classes>
    </package>
    <package name="Beta.Utility" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="Beta.Utility.Helper" filename="Beta.Utility\Helper.cs" line-rate="0" branch-rate="0" complexity="1">
          <lines>
            <line number="20" hits="3" branch="False" />
            <line number="21" hits="4" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
'@

        $projection = ConvertTo-JacocoPackageProjection -XmlDocument $script:sourceDocument

        { Assert-JacocoProjectionReconciliation -XmlDocument $script:sourceDocument -ProjectionXml $projection } |
            Should -Not -Throw
    }

    It 'throws naming the expected and the observed totals when the projection disagrees' {
        # AC5, negative path. The first package's LINE counter is rewritten from two covered to one
        # covered, so the summed projection covered total becomes 3 against the root's declared 4.
        # The assertion must throw and must name both figures, so a failure is diagnosable from the
        # message alone.
        [xml]$script:sourceDocument = @'
<coverage line-rate="0.8" branch-rate="0.5" lines-covered="4" lines-valid="5" branches-covered="1" branches-valid="2">
  <packages>
    <package name="Alpha.Core" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="Alpha.Core.Widget" filename="Alpha.Core\Widget.cs" line-rate="0" branch-rate="0" complexity="1">
          <lines>
            <line number="10" hits="1" branch="False" />
            <line number="11" hits="0" branch="False" />
            <line number="12" hits="2" branch="True" condition-coverage="50% (1/2)" />
          </lines>
        </class>
      </classes>
    </package>
    <package name="Beta.Utility" line-rate="0" branch-rate="0" complexity="1">
      <classes>
        <class name="Beta.Utility.Helper" filename="Beta.Utility\Helper.cs" line-rate="0" branch-rate="0" complexity="1">
          <lines>
            <line number="20" hits="3" branch="False" />
            <line number="21" hits="4" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
'@

        $projection = ConvertTo-JacocoPackageProjection -XmlDocument $script:sourceDocument
        $alteredProjection = $projection.Replace(
            '<counter type="LINE" missed="1" covered="2" />',
            '<counter type="LINE" missed="1" covered="1" />')

        # Guard: if the substitution matched nothing the rest of the test would assert against an
        # unmodified projection and pass for the wrong reason.
        $alteredProjection | Should -Not -BeExactly $projection

        $thrownMessage = $null
        try {
            Assert-JacocoProjectionReconciliation -XmlDocument $script:sourceDocument -ProjectionXml $alteredProjection
        }
        catch {
            $thrownMessage = $_.Exception.Message
        }

        $thrownMessage | Should -Not -BeNullOrEmpty
        $thrownMessage | Should -Match 'expected 4\b'
        $thrownMessage | Should -Match 'observed 3\b'
    }
}

Describe 'Invoke-MSTestWithCoverage.Projection.ps1 counting-rule delegation' {
    It 'delegates the counting rule to the per-package helper and re-derives nothing' {
        # AC3. The projection must not carry a second implementation of the de-duplicating counting
        # rule. Three mechanical properties of the file's abstract syntax tree stand for that: the
        # shared per-package summariser is invoked; no string anywhere in the file names the
        # condition-coverage attribute, which is the only attribute a re-derived branch count could
        # read; and the file declares no hashtable literal, which is the shape a per-line-number map
        # takes in the existing summariser.
        $parseErrors = $null
        $parseTokens = $null
        $scriptAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:projectionScriptPath,
            [ref]$parseTokens,
            [ref]$parseErrors)

        @($parseErrors).Count | Should -Be 0

        $commandAsts = $scriptAst.FindAll(
            { param($node) $node -is [System.Management.Automation.Language.CommandAst] },
            $true)
        @($commandAsts | Where-Object { $_.GetCommandName() -eq 'Get-CoberturaPackageLineSummary' }).Count |
            Should -BeGreaterThan 0

        $conditionCoverageStrings = $scriptAst.FindAll(
            {
                param($node)
                ($node -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
                $node.Value -like '*condition-coverage*') -or
                ($node -is [System.Management.Automation.Language.ExpandableStringExpressionAst] -and
                $node.Value -like '*condition-coverage*')
            },
            $true)
        @($conditionCoverageStrings).Count | Should -Be 0

        $hashtableAsts = $scriptAst.FindAll(
            { param($node) $node -is [System.Management.Automation.Language.HashtableAst] },
            $true)
        @($hashtableAsts).Count | Should -Be 0

        $indexAssignments = $scriptAst.FindAll(
            {
                param($node)
                $node -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                $node.Left -is [System.Management.Automation.Language.IndexExpressionAst]
            },
            $true)
        @($indexAssignments).Count | Should -Be 0
    }
}
