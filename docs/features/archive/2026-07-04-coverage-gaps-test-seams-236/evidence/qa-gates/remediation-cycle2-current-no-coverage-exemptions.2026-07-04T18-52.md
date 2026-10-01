# Remediation Cycle 2 Current No Coverage Exemptions

Timestamp: 2026-07-04T20:44:55.5977859-04:00
Command: Select-String target files and coverage configuration for coverage exclusions; git diff -- coverage.config TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings
EXIT_CODE: 0

Output Summary:
- No coverage configuration diff was detected for coverage.config, TaskMaster.runsettings, or scripts/vscode/TaskMaster.cli.runsettings.
- Search output is recorded below for audit; existing coverage-related text is not treated as a new exemption unless present in the configuration diff.

Search Output:
```text
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:3:  dotnet-coverage settings file.
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:4:  Excludes third-party and F#/mixed-mode assemblies from instrumentation to
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:5:  prevent coverage from breaking tests that depend on those libraries
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:8:  in Invoke-MSTestWithCoverage.ps1.
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:11:  <CodeCoverage>
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:13:      <Exclude>
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:21:      </Exclude>
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:23:  </CodeCoverage>
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.runsettings:11:      <DataCollector friendlyName="Code Coverage">
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.runsettings:13:          <CodeCoverage>
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.runsettings:15:              <Exclude>
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.runsettings:23:              </Exclude>
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.runsettings:25:          </CodeCoverage>
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:20:    # Test projects must be excluded from the coverage allowlist so that
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:71:    # Coverage produced from sibling worktrees can retain the canonical
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:98:function Get-CoberturaCoverageSummary {
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:128:                if ($line.branch -eq 'True' -and $line.HasAttribute('condition-coverage') -and $line.'condition-coverage' -match '\(([0-9]+)/([0-9]+)\)') {
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:146:function Get-CoberturaLineConditionCoverageParts {
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:154:    if ($LineNode.HasAttribute('condition-coverage') -and $LineNode.'condition-coverage' -match '\(([0-9]+)/([0-9]+)\)') {
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:221:                    $candidateCoverage = Get-CoberturaLineConditionCoverageParts -LineNode $lineNode
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:226:                            Covered = $candidateCoverage.Covered
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:227:                            Total   = $candidateCoverage.Total
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:241:                        $candidateCoverage.Total -gt $existing.Total -or
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:242:                        ($candidateCoverage.Total -eq $existing.Total -and $candidateCoverage.Covered -gt $existing.Covered)
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:244:                        $existing.Covered = $candidateCoverage.Covered
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:245:                        $existing.Total = $candidateCoverage.Total
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:247:                        if ($lineNode.HasAttribute('condition-coverage')) {
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:248:                            $existingNode.SetAttribute('condition-coverage', $lineNode.GetAttribute('condition-coverage'))
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:250:                        elseif ($existingNode.HasAttribute('condition-coverage')) {
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:251:                            $existingNode.RemoveAttribute('condition-coverage')
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:270:            $classSummaryXml = [xml]"<coverage><packages><package><classes /></package></packages></coverage>"
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:273:            $classSummary = Get-CoberturaCoverageSummary -XmlDocument $classSummaryXml
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:336:        $coverageNode = $xml.SelectSingleNode('/coverage')
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:337:        $packagesElement = $xml.SelectSingleNode('/coverage/packages')
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:338:        $coverageNode.InsertBefore($sourcesNode, $packagesElement) | Out-Null
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:341:    $coverageSummary = Get-CoberturaCoverageSummary -XmlDocument $xml
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:342:    $xml.coverage.SetAttribute('line-rate', $coverageSummary.LineRate)
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:343:    $xml.coverage.SetAttribute('branch-rate', $coverageSummary.BranchRate)
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:344:    $xml.coverage.SetAttribute('lines-covered', $coverageSummary.LinesCovered)
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:345:    $xml.coverage.SetAttribute('lines-valid', $coverageSummary.LinesValid)
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:346:    $xml.coverage.SetAttribute('branches-covered', $coverageSummary.BranchesCovered)
<user-profile>\repos\TaskMaster-wt-2026-07-04-12-57\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1:347:    $xml.coverage.SetAttribute('branches-valid', $coverageSummary.BranchesValid)
```

Coverage Config Diff:
```text
```
