Set-StrictMode -Version Latest

BeforeAll {
    $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
    $script:ConfigPath = Join-Path $script:RepoRoot '.github/dependabot.yml'
    $script:ConfigLine = [System.IO.File]::ReadAllLines($script:ConfigPath)

    # The parse below is deliberately text-based and takes no module dependency. The
    # powershell-yaml module is not guaranteed present on the windows-latest runner, and an
    # absent module would turn the CI pester job red for a reason unrelated to this config.
    # The forms asserted here are a fixed two-space-indented block, which a line parser reads
    # deterministically.

    function Get-DependabotGroupKey {
        <#
        .SYNOPSIS
            Returns the group keys declared directly under the groups mapping.
        #>
        param(
            [Parameter(Mandatory = $true)]
            [string[]]$Line
        )

        $key = [System.Collections.Generic.List[string]]::new()
        $inGroups = $false
        foreach ($text in $Line) {
            if ($text -match '^\s{4}groups:\s*$') {
                $inGroups = $true
                continue
            }
            if (-not $inGroups) { continue }
            if ($text -match '^\s{0,4}\S') { break }
            if ($text -match '^\s{6}([A-Za-z0-9_.-]+):\s*$') { $key.Add($Matches[1]) }
        }

        return $key.ToArray()
    }

    function Get-DependabotGroupBody {
        <#
        .SYNOPSIS
            Returns the lines belonging to the named group, excluding its key line.
        #>
        param(
            [Parameter(Mandatory = $true)][string[]]$Line,
            [Parameter(Mandatory = $true)][string]$GroupKey
        )

        $body = [System.Collections.Generic.List[string]]::new()
        $inGroup = $false
        foreach ($text in $Line) {
            if ($text -match ('^\s{{6}}{0}:\s*$' -f [regex]::Escape($GroupKey))) {
                $inGroup = $true
                continue
            }
            if (-not $inGroup) { continue }
            if ($text -match '^\s{0,6}\S') { break }
            $body.Add($text)
        }

        return $body.ToArray()
    }

    function Get-DependabotIgnoreEntry {
        <#
        .SYNOPSIS
            Returns one record per ignore entry, carrying its qualifier keys and update types.
        #>
        param(
            [Parameter(Mandatory = $true)]
            [string[]]$Line
        )

        $entry = [System.Collections.Generic.List[pscustomobject]]::new()
        $index = -1
        for ($i = 0; $i -lt $Line.Count; $i++) {
            if ($Line[$i] -match '^\s{4}ignore:\s*$') { $index = $i }
        }
        if ($index -lt 0) { return $entry.ToArray() }

        for ($i = $index + 1; $i -lt $Line.Count; $i++) {
            if ($Line[$i] -notmatch '^\s{6}- dependency-name:\s*"([^"]+)"') { continue }
            $name = $Matches[1]
            $qualifier = [System.Collections.Generic.List[string]]::new()
            $updateType = [System.Collections.Generic.List[string]]::new()
            $j = $i + 1
            while ($j -lt $Line.Count -and $Line[$j] -match '^\s{8}([A-Za-z-]+):\s*(.*)$') {
                $qualifierName = $Matches[1]
                $qualifierValue = $Matches[2]
                $qualifier.Add($qualifierName)
                if ($qualifierName -eq 'update-types') {
                    $listMatch = [regex]::Match($qualifierValue, '^\[(.*)\]$')
                    if ($listMatch.Success) {
                        foreach ($part in ($listMatch.Groups[1].Value -split ',')) {
                            $updateType.Add($part.Trim().Trim('"'))
                        }
                    }
                }
                $j++
            }
            $entry.Add([pscustomobject]@{
                    Name       = $name
                    Qualifier  = $qualifier.ToArray()
                    UpdateType = $updateType.ToArray()
                })
        }

        return $entry.ToArray()
    }

    $script:WorkflowDirectory = Join-Path $script:RepoRoot '.github/workflows'
    $script:RepairWorkflowPath = Join-Path $script:WorkflowDirectory 'dependabot-repair.yml'
    $script:WorkflowReadmePath = Join-Path $script:WorkflowDirectory 'README.md'
    $script:CompositionRootPath = Join-Path $script:RepoRoot 'scripts/dependencies/Repair-PackageManifestConsistency.ps1'

    function Get-SetupNuGetStep {
        <#
        .SYNOPSIS
            Returns one record per workflow step that uses the setup-nuget action.
        .DESCRIPTION
            Enumeration is over the workflow YAML files only. Each record carries the
            declared nuget-version value, or the empty string when the step declares none,
            so a step that reverted to the floating selector is reported rather than
            silently skipped.
        #>
        param(
            [Parameter(Mandatory = $true)]
            [string]$Directory
        )

        $step = [System.Collections.Generic.List[pscustomobject]]::new()
        foreach ($file in (Get-ChildItem -LiteralPath $Directory -Filter '*.yml' -File | Sort-Object Name)) {
            $line = [System.IO.File]::ReadAllLines($file.FullName)
            for ($i = 0; $i -lt $line.Count; $i++) {
                if ($line[$i] -notmatch '^\s*uses:\s*\S*setup-nuget@') { continue }
                $version = ''
                for ($j = $i + 1; $j -lt $line.Count; $j++) {
                    if ($line[$j] -match '^\s*-\s') { break }
                    $versionMatch = [regex]::Match($line[$j], "^\s*nuget-version:\s*['`"]?([^'`"\s]+)['`"]?\s*$")
                    if ($versionMatch.Success) {
                        $version = $versionMatch.Groups[1].Value
                        break
                    }
                }
                $step.Add([pscustomobject]@{
                        File         = $file.Name
                        LineNumber   = $i + 1
                        NuGetVersion = $version
                    })
            }
        }

        return $step.ToArray()
    }

    function Get-WorkflowStepBlock {
        <#
        .SYNOPSIS
            Returns one workflow step's lines, from its name line to the next step's name line.
        #>
        param(
            # AllowEmptyString is required: a mandatory [string[]] rejects a blank element, and a workflow file is full of blank lines.
            [Parameter(Mandatory = $true)][AllowEmptyString()][string[]]$Line,
            [Parameter(Mandatory = $true)][string]$StepName
        )

        $block = [System.Collections.Generic.List[string]]::new()
        $inStep = $false
        foreach ($text in $Line) {
            if ($text -match '^\s+-\s+name:\s*(.+?)\s*$') {
                if ($inStep) { break }
                if ($Matches[1] -eq $StepName) { $inStep = $true; $block.Add($text); continue }
            }
            if ($inStep) { $block.Add($text) }
        }

        return $block.ToArray()
    }

    # The literal expected set, in file order, as recorded by the P0-T22 census of the
    # merge-base configuration. Declared here so the comparison is against a fixed list
    # rather than against whatever the file happens to contain.
    $script:ExpectedSemverMajorPair = @(
        'Microsoft.Extensions.*|version-update:semver-major',
        'Microsoft.Bcl.*|version-update:semver-major',
        'System.Text.Json|version-update:semver-major',
        'System.Drawing.Common|version-update:semver-major',
        'Microsoft.Graph*|version-update:semver-major',
        'Apache.Arrow*|version-update:semver-major',
        'Microsoft.Data.Analysis|version-update:semver-major',
        'Microsoft.ML*|version-update:semver-major'
    )
}

Describe 'Dependabot configuration consolidation' {

    Context 'Grouping and pull-request volume' {

        It 'AC1- declares exactly one entry under groups' {
            # Arrange / Act
            $groupKey = @(Get-DependabotGroupKey -Line $script:ConfigLine)

            # Assert
            $groupKey.Count | Should -Be 1 -Because 'the four topic groups are consolidated into one catch-all group'
            $groupKey[0] | Should -Not -BeNullOrEmpty -Because 'the single group must be named'
        }

        It 'AC1- declares applies-to version-updates and a catch-all pattern on that entry' {
            # Arrange
            $groupKey = @(Get-DependabotGroupKey -Line $script:ConfigLine)
            $groupKey.Count | Should -Be 1 -Because 'the body lookup below is meaningful only for a single group'

            # Act
            $body = @(Get-DependabotGroupBody -Line $script:ConfigLine -GroupKey $groupKey[0])

            # Assert
            $body.Count | Should -BeGreaterThan 0 -Because 'an empty body would satisfy any absence clause vacuously'
            @($body | Where-Object { $_ -match '^\s+applies-to:\s*version-updates\s*$' }).Count |
                Should -Be 1 -Because 'the group applies to version updates'
            @($body | Where-Object { $_ -match '^\s+-\s+"\*"\s*$' }).Count |
                Should -Be 1 -Because 'the group pattern is the catch-all'
        }

        It 'AC1- limits open pull requests to one' {
            # Arrange / Act
            $limit = @($script:ConfigLine | Where-Object { $_ -match '^\s*open-pull-requests-limit:\s*(\d+)\s*$' })

            # Assert
            $limit.Count | Should -Be 1 -Because 'the key is declared exactly once'
            $limit[0] | Should -Match 'open-pull-requests-limit:\s*1\s*$' -Because 'the fan-out fix caps the ecosystem at one open pull request'
        }
    }

    Context 'Ignore entries' {

        It 'AC1- carries one unqualified Deedle ignore entry' {
            # Arrange / Act
            $entry = @(Get-DependabotIgnoreEntry -Line $script:ConfigLine)
            $deedle = @($entry | Where-Object { $_.Name -eq 'Deedle' })

            # Assert
            $entry.Count | Should -BeGreaterThan 1 -Because 'a parser that found nothing would report an empty qualifier list for every entry'
            $deedle.Count | Should -Be 1 -Because 'Deedle is ignored exactly once'
            $deedle[0].Qualifier | Should -BeNullOrEmpty -Because 'an unqualified entry ignores every update type, including the ones a qualifier would re-admit'
        }

        It 'AC1- retains the merge-base semver-major pair set element by element' {
            # Arrange
            $entry = @(Get-DependabotIgnoreEntry -Line $script:ConfigLine)

            # Act
            $actualPair = @(
                $entry |
                    Where-Object { $_.Qualifier -contains 'update-types' } |
                        ForEach-Object { '{0}|version-update:semver-major' -f $_.Name })

            # Assert
            $actualPair.Count | Should -Be $script:ExpectedSemverMajorPair.Count -Because 'no entry may be dropped or added'
            for ($i = 0; $i -lt $script:ExpectedSemverMajorPair.Count; $i++) {
                $actualPair[$i] |
                    Should -BeExactly $script:ExpectedSemverMajorPair[$i] -Because "entry $i must match the merge-base set in name and order"
            }
        }
    }

    Context 'NuGet CLI version pinning' {

        It 'AC4- enumerates at least one workflow step that uses the setup-nuget action' {
            # Arrange / Act
            $step = @(Get-SetupNuGetStep -Directory $script:WorkflowDirectory)

            # Assert: the greater-than-zero clause is what stops a broken enumerator from
            # satisfying the pinning assertion below over an empty set.
            $step.Count | Should -BeGreaterThan 0 -Because 'an enumerator that found no step would make every per-step assertion vacuously true'
            @($step | ForEach-Object { $_.File } | Sort-Object -Unique).Count |
                Should -BeGreaterThan 0 -Because 'the enumerated steps must come from real workflow files'
        }

        It 'AC4- pins every setup-nuget step to an exact three-part version literal' {
            # Arrange
            $step = @(Get-SetupNuGetStep -Directory $script:WorkflowDirectory)
            $step.Count | Should -BeGreaterThan 0 -Because 'the per-step assertions below are meaningful only over a non-empty set'

            # Act / Assert
            foreach ($declared in $step) {
                $declared.NuGetVersion |
                    Should -Match '^\d+\.\d+\.\d+$' -Because "$($declared.File) line $($declared.LineNumber) must declare an exact three-part nuget-version, not a floating selector"
            }
        }
    }

    Context 'Repair workflow static validity' {

        It 'AC17- restricts its work to head branches under the Dependabot branch prefix' {
            # Arrange
            $line = [System.IO.File]::ReadAllLines($script:RepairWorkflowPath)

            # Act
            $restriction = @($line | Where-Object {
                    $_ -match "startsWith\(github\.event\.workflow_run\.head_branch,\s*'dependabot/'\)"
                })

            # Assert: a positive match on the named expression, so an absent restriction fails
            # rather than passing for want of anything to find.
            $line.Count | Should -BeGreaterThan 0 -Because 'the workflow file must be readable for the assertion below to mean anything'
            $restriction.Count | Should -BeGreaterThan 0 -Because 'the job must not run for a completed CI run on any other branch'
        }

        It 'AC17- does not use the base-context variant of the pull-request trigger' {
            # Arrange
            $text = [System.IO.File]::ReadAllText($script:RepairWorkflowPath)

            # Act
            $prohibited = ([regex]::Matches($text, 'pull_request' + '_target')).Count
            $declared = ([regex]::Matches($text, 'workflow_run:')).Count

            # Assert: the positive count is what distinguishes a file declaring the intended
            # trigger from a file that declares no trigger at all.
            $declared | Should -BeGreaterThan 0 -Because 'the workflow must declare the workflow_run trigger it was designed around'
            $prohibited | Should -Be 0 -Because 'the base-context pull-request trigger is rejected on security grounds'
        }

        It 'AC26- records a NuGet pin in the workflow README equal to every workflow literal' {
            # Arrange: the README literal, read from the pinned-tool paragraph of the repair
            # workflow section, and every nuget-version literal the workflow files declare.
            $readme = [System.IO.File]::ReadAllText($script:WorkflowReadmePath)
            $documented = @([regex]::Matches($readme, '`(?<value>\d+\.\d+\.\d+)`') |
                    ForEach-Object { $_.Groups['value'].Value } |
                        Where-Object { $_ -match '^\d+\.\d+\.\d+$' })
            $step = @(Get-SetupNuGetStep -Directory $script:WorkflowDirectory)

            # Act
            $declared = @($step | ForEach-Object { $_.NuGetVersion } | Sort-Object -Unique)
            $pinned = @($documented | Where-Object { $declared -contains $_ } | Sort-Object -Unique)

            # Assert: the comparison runs over a non-empty set in both directions, so neither an
            # empty README nor a broken step enumerator can satisfy it vacuously.
            $step.Count | Should -BeGreaterThan 0 -Because 'an empty workflow set would make the equality below vacuous'
            $declared.Count | Should -Be 1 -Because 'every workflow step must pin the same NuGet CLI version'
            $pinned.Count | Should -Be 1 -Because "the README must record the pinned version $($declared -join ', ') that the workflow files declare"
            foreach ($record in $step) {
                $record.NuGetVersion |
                    Should -BeExactly $pinned[0] -Because "$($record.File) line $($record.LineNumber) must declare the version the README records"
            }
        }

        It 'AC17- declares the write permissions the repair and the disclosure need' {
            # Arrange
            $line = [System.IO.File]::ReadAllLines($script:RepairWorkflowPath)

            # Act
            $contents = @($line | Where-Object { $_ -match '^\s*contents:\s*write\s*$' })
            $pullRequests = @($line | Where-Object { $_ -match '^\s*pull-requests:\s*write\s*$' })

            # Assert
            $contents.Count | Should -BeGreaterThan 0 -Because 'the job pushes a commit onto the Dependabot branch'
            $pullRequests.Count | Should -BeGreaterThan 0 -Because 'the job edits the pull-request body and its labels'
        }
    }

    Context 'Repair workflow gating, disclosure and identity' {

        It 'R3- gates the commit step on the write-set count rather than the repair count' {
            # Arrange
            $line = [System.IO.File]::ReadAllLines($script:RepairWorkflowPath)
            $step = @(Get-WorkflowStepBlock -Line $line -StepName 'Commit and push the repair onto the Dependabot branch')
            # Act
            $ifLine = @($step | Where-Object { $_ -match '^\s+if:\s' })
            # Assert
            $step.Count | Should -BeGreaterThan 0 -Because 'an empty step block would make every clause below vacuous'
            $ifLine.Count | Should -Be 1 -Because 'the commit step carries exactly one condition'
            @($line | Where-Object { $_ -like '*written-count=*' }).Count |
                Should -BeGreaterThan 0 -Because 'the repair step must publish the write-set count as a step output'
            $ifLine[0] | Should -BeLike '*written-count*' -Because 'the push gate must read the quantity that moves whenever any file is written'
            $ifLine[0] | Should -Not -BeLike '*repair-count*' -Because 'RepairCount excludes normalisation and binding-redirect writes, so a run whose only writes fall in those classes would go green and skip the push'
        }

        It 'R6- guards the disclosure step and replaces a delimited block' {
            # Arrange
            $line = [System.IO.File]::ReadAllLines($script:RepairWorkflowPath)
            $text = [System.IO.File]::ReadAllText($script:RepairWorkflowPath)
            $step = @(Get-WorkflowStepBlock -Line $line -StepName 'Disclose the repairs on the pull request')
            # Act
            $ifLine = @($step | Where-Object { $_ -match '^\s+if:\s' })
            # Assert
            $step.Count | Should -BeGreaterThan 0 -Because 'an empty step block would make every clause below vacuous'
            $ifLine.Count | Should -Be 1 -Because 'the disclosure step must be guarded'
            $ifLine[0] | Should -BeLike '*written-count*' -Because 'a run that wrote nothing has nothing to disclose'
            $ifLine[0] | Should -BeLike '*skip-count*' -Because 'AC20 requires the skipped block whenever the run recorded a skip, even with no write'
            $text | Should -BeLike '*<!-- dependabot-repair:begin -->*' -Because 'the disclosure must be delimited so a later run replaces it instead of appending'
            $text | Should -BeLike '*<!-- dependabot-repair:end -->*' -Because 'a block needs both delimiters to be replaceable'
        }

        It 'R7- counts beyond-known-weak repairs with the analyzer exclusion alone' {
            # Arrange
            $text = [System.IO.File]::ReadAllText($script:RepairWorkflowPath)
            # Assert
            $text.Length | Should -BeGreaterThan 0 -Because 'an empty file would satisfy both containment clauses below vacuously'
            $text | Should -BeLike "*Where-Object { `$_ -ne 'Analyzer' }*" -Because 'the binding-redirect clause is unreachable under the configured trigger and is removed'
            $text | Should -BeLike '*not reachable from the workflow_run trigger*' -Because 'the reachability decision must be recorded at the line it explains, so a later author who supplies -CandidateUpgrade is told'
        }

        It 'R8- derives the commit identity from the token step outputs' {
            # Arrange: the prohibited address is composed rather than typed, so this test file
            # is not itself a match for a repository search for the literal.
            $text = [System.IO.File]::ReadAllText($script:RepairWorkflowPath)
            $handWritten = 'dependabot-repair' + '[bot]' + '@users.noreply.github.com'
            # Assert
            $text.Length | Should -BeGreaterThan 0 -Because 'an empty file would satisfy the absence clause below vacuously'
            $text | Should -BeLike '*steps.app-token.outputs.app-slug*' -Because 'the app slug must come from the token step rather than a literal'
            $text | Should -BeLike '*users/*' -Because 'the bot user id is resolved through the users API, the numeric part being the bot user id and not the app id'
            $text.Contains($handWritten) | Should -BeFalse -Because 'a hand-written noreply address matches no account, so the commit author login would resolve to null and AC18 could not hold'
        }

        It 'R6- replaces rather than appends a previously disclosed block' {
            # Arrange: the pattern is the one the workflow itself uses, and the containment
            # assertion below is what makes this a test of the workflow rather than of a
            # pattern this test invented. It fails if the two ever diverge by a character.
            $blockPattern = '(?s)<!-- dependabot-repair:begin -->.*?<!-- dependabot-repair:end -->'
            $text = [System.IO.File]::ReadAllText($script:RepairWorkflowPath)
            $text.Contains($blockPattern) |
                Should -BeTrue -Because 'the workflow must strip prior blocks with exactly this expression'

            # The markers are derived from the pattern the same way the workflow derives them.
            $marker = $blockPattern.Substring(4) -split '\.\*\?'
            $leading = 'Bumps Contoso.Widgets from 1.0.0 to 2.0.0.'
            $existing = $leading + "`n`n" + $marker[0] + "`n## Repairs applied`n- first run`n" + $marker[1]

            # Act: strip any prior block, then append a fresh one, exactly as the step does.
            $stripped = [regex]::Replace($existing, $blockPattern, '').TrimEnd()
            $updated = $stripped + "`n`n" + $marker[0] + "`n## Repairs applied`n- second run`n" + $marker[1]
            # Assert
            ([regex]::Matches($updated, [regex]::Escape($marker[0]))).Count |
                Should -Be 1 -Because 'a second run must leave exactly one opening marker, not two'
            ([regex]::Matches($updated, [regex]::Escape($marker[1]))).Count |
                Should -Be 1 -Because 'a second run must leave exactly one closing marker, not two'
            $updated | Should -BeLike "*$leading*" -Because 'the strip must remove only the delimited block, leaving the pull-request body intact'
            $updated | Should -BeLike '*second run*' -Because 'the fresh block must be present, so the strip did not simply delete everything'
            $updated | Should -Not -BeLike '*first run*' -Because 'the prior block must be gone rather than accumulated'
        }
    }

    Context 'Default manifest lister visibility' {

        It 'R9c- records the enumerated directory count in the default manifest lister' {
            # Arrange: read the composition root and isolate the default lister block, which
            # runs from its assignment line to the first closing brace in column one. This is
            # a text assertion over production source and is described as such: it observes
            # what the file says, not what a run of it does.
            $line = [System.IO.File]::ReadAllLines($script:CompositionRootPath)
            $start = -1
            for ($i = 0; $i -lt $line.Count; $i++) {
                if ($line[$i] -match '^\$script:DefaultFileLister\s*=\s*\{') { $start = $i; break }
            }
            $start | Should -BeGreaterThan -1 -Because 'an unfound block would make both assertions below vacuous'
            $end = -1
            for ($j = $start + 1; $j -lt $line.Count; $j++) {
                if ($line[$j] -eq '}') { $end = $j; break }
            }
            $end | Should -BeGreaterThan $start -Because 'an undelimited block would make both assertions below vacuous'
            # Act
            $block = ($line[$start..$end] -join [System.Environment]::NewLine)
            # Assert
            $block | Should -BeLike '*Write-Verbose*' -Because 'a shortfall in one-level-deep manifest discovery must be observable in the run log'
            $block | Should -BeLike '*enumerated director*' -Because 'the verbose record must name the enumerated directory count, which is the quantity a shortfall shows up in'
        }
    }
}
