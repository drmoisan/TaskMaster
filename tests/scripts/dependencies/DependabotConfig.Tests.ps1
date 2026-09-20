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
}
