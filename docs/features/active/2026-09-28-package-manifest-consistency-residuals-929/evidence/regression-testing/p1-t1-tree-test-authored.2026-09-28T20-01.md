# P1-T1 — Repository tree consistency test authored

Timestamp: 2026-09-30T10-00
Command: Write tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1; then pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; (BOM, CR byte, non-ASCII byte and line counts); Select-String -Pattern "^\s*It '"'"'" ...; Select-String -Pattern "Start-Sleep","GetTempFileName","New-TemporaryFile","Mock" ...; Select-String for It, Describe or Context names matching AC followed by a digit' (the quote character built from [char]39 for the name checks)
EXIT_CODE: 0
WORKER: atomic-executor authored the file inline; no sub-agent dispatch tool is available to this executor, so the powershell-typed-engineer hand-off was performed within exactly the P1-T1 bounds.
Output Summary:
- File created: tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1
- Encoding: UTF-8 with BOM (BOM=True); line endings LF (0 CR bytes); ASCII content (0 bytes above 127 after the BOM)
- Line count: 152 (at most 200)
- Set-StrictMode -Version Latest; BeforeAll resolves $script:RepoRoot from $PSScriptRoot as ConsistencyVerifier.Tests.ps1 line 4 does and imports scripts/dependencies/ConsistencyVerifier.psm1 with -Force
- One Describe: 'Repository tree consistency (issue 929)'
- It lines matching ^\s*It ': 4, named verbatim:
  1. 'reports no Import element whose package the sibling manifest omits, for every project directory that carries a manifest'
  2. 'names in the SVGControl binding redirects the assembly version the SVGControl project reference declares'
  3. 'passes client-id and not app-id to the create-github-app-token step of the repair workflow'
  4. 'instructs the maintainer to store the Client ID in the secret the repair workflow reads by name'
- Select-String for Start-Sleep, GetTempFileName, New-TemporaryFile and Mock: 0 lines
- It, Describe or Context names matching AC followed by a digit: 0
- Test 1 formats each finding as "<project file leaf name>: line <LineNumber> <PackageFolder>"; test 3 carries -Because text containing client-id and app-id; test 4 carries -Because text containing secret name and client-id.
- The step-block helper re-implements the Get-WorkflowStepBlock approach locally (it is not dot-sourced); both helpers are defined inside BeforeAll.
