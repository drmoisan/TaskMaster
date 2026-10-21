# P6-T3 PowerShell test pass (PoshQC MCP, Ruling 1)

## iter1

Timestamp: 2026-09-29T22-16
Command: `git status --porcelain -- "*.csproj"` (before); mcp__drm-copilot__run_poshqc_test with workspace_root `<repo-root>` (the item worktree root) and scan_folders ["scripts/dependencies", "scripts/hygiene", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/hygiene", "tests/scripts/vscode"]; `git status --porcelain -- "*.csproj"` (after); then the plan's read-only JUnit derivation payload over artifacts/pester/pester-junit.xml, run from the item worktree root: pwsh -NoProfile -Command '$p = "artifacts/pester/pester-junit.xml"; $fi = Get-Item -LiteralPath $p; "JUNIT-AGE-SECONDS=" + ...; foreach ($t in $hyg) { "IT| " + (Split-Path -Leaf $t.classname) + " | " + $t.name + " | " + $t.status }' (verbatim as plan revision 1.16 states it, prefixed only by a Set-Location to the worktree root assembled from the user-profile environment variable).
EXIT_CODE: 0
Output Summary:
- POSHQC MCP AVAILABLE ok=true. Summary: "Ran bundled PoshQC test against '<repo-root>' with 6 selected scan folder(s)."
- EXIT_CODE 0 is the derived result (ok=true, failures=0, errors=0, Failed=0). The tool reports no numeric exit code and no counts; the counts below are derived from the JUnit document the tool wrote, as the P3-T17 artifact derived its counts.
- JUNIT-AGE-SECONDS=20 (less than 600: the document was written by this task's call)
- JUNIT-ROOT tests=373 failures=0 errors=0 disabled=0
- PESTER Passed=373 Failed=0 Skipped=0 Total=373 (derived from the document; JUNIT-ROOT tests=373 equals Total=373)
- Expected Passed=373 (the CI baseline PESTER Passed=342 of run 36591948327 on main at c4ff0e2be plus the 31 hygiene It blocks); recorded, not gated here. The P0-T15 direct-run figure 320 is recorded for reference only. The Passed= floor is gated on the CI figure in P6-T38.
- HYGIENE Passed=31 Failed=0 Skipped=0 Total=31
- Project-file porcelain before: no line. After: no line. The two spans are identical; no project file was restored.
- IT| line count: 31, each ending | Passed. Per file: Test-RepositoryHygiene.Rules.Tests.ps1 19, Test-RepositoryHygiene.Git.Tests.ps1 5, Test-RepositoryHygiene.Tests.ps1 7 (the P1-T8 inventory). Each of the thirty It names spec.md lists, plus the unreadable test, appears in a third field.
- The JUnit document and artifacts/pester/powershell-coverage.xml stay under the ignored artifacts/ path. No coverage figure is read from them; the authoritative coverage figure is the CI Pester job's, recorded by P6-T38 (Ruling 1).

IT enumeration (leaf file name | Describe.It name | status):

```text
IT| Test-RepositoryHygiene.Git.Tests.ps1 | Get-TrackedFileRecord.parses a NUL-separated eol listing into path records | Passed
IT| Test-RepositoryHygiene.Git.Tests.ps1 | Get-TrackedFileRecord.flags an index-binary record | Passed
IT| Test-RepositoryHygiene.Git.Tests.ps1 | Read-TrackedFileText.decodes UTF-16 little-endian bytes by byte-order mark | Passed
IT| Test-RepositoryHygiene.Git.Tests.ps1 | Read-TrackedFileText.decodes UTF-8 bytes without a byte-order mark | Passed
IT| Test-RepositoryHygiene.Git.Tests.ps1 | Assert-GitExitCode.throws when the git wrapper reports a non-zero exit | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Find-UserProfilePathMatch.matches a backslash-separated profile path assembled at run time | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Find-UserProfilePathMatch.matches a forward-slash-separated profile path | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Find-UserProfilePathMatch.matches a doubled-backslash profile path | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Find-UserProfilePathMatch.matches a lower-case drive letter and profile parent | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Find-UserProfilePathMatch.matches an upper-case profile parent | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Find-UserProfilePathMatch.matches an eight-dot-three user segment | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Find-UserProfilePathMatch.does not match a user-profile placeholder path | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Find-UserProfilePathMatch.does not match a repo-root placeholder path | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Find-UserProfilePathMatch.does not match a bare drive root | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Find-UserProfilePathMatch.does not match a fixtures root | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Find-UserProfilePathMatch.reports the line number and not the matched text | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Get-RawEvidenceDocumentKind.classifies a trx extension as trx | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Get-RawEvidenceDocumentKind.classifies a coverage root on its own line as cobertura | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Get-RawEvidenceDocumentKind.classifies a results root as dotnet-coverage | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Get-RawEvidenceDocumentKind.classifies a report root with class elements as jacoco-raw | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Get-RawEvidenceDocumentKind.classifies a package-only report root as jacoco-projection | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Get-RawEvidenceDocumentKind.classifies a report root behind a DOCTYPE | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Get-RawEvidenceDocumentKind.classifies a byte-order-mark prefixed document | Passed
IT| Test-RepositoryHygiene.Rules.Tests.ps1 | Get-RawEvidenceDocumentKind.classifies a ps1 file containing a TestRun element as none | Passed
IT| Test-RepositoryHygiene.Tests.ps1 | Invoke-RepositoryHygieneMain.excludes governance-directory records and reports the remaining violation | Passed
IT| Test-RepositoryHygiene.Tests.ps1 | Invoke-RepositoryHygieneMain.reports a raw document record as a finding | Passed
IT| Test-RepositoryHygiene.Tests.ps1 | Invoke-RepositoryHygieneMain.retains a package-level projection record | Passed
IT| Test-RepositoryHygiene.Tests.ps1 | Invoke-RepositoryHygieneMain.returns a non-zero exit decision when findings exist | Passed
IT| Test-RepositoryHygiene.Tests.ps1 | Invoke-RepositoryHygieneMain.returns a zero exit decision over clean content | Passed
IT| Test-RepositoryHygiene.Tests.ps1 | Invoke-RepositoryHygieneMain.prints path and line only and never the matched text | Passed
IT| Test-RepositoryHygiene.Tests.ps1 | Invoke-RepositoryHygieneMain.reports a record whose content reader throws as unreadable | Passed
```
