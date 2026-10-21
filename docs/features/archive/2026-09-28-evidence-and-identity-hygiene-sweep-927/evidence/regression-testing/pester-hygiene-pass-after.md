# P1-T8 Pester hygiene suite with the production files present (AC3)

Timestamp: 2026-09-29T17-35
Command: pwsh -NoProfile -Command 'Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/hygiene"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; foreach ($t in $r.Tests) { "IT| " + $t.ExpandedPath + " | " + $t.Result }; if ($r.FailedCount -gt 0 -or $r.PassedCount -eq 0) { exit 1 } else { exit 0 }'
EXIT_CODE: 0
Output Summary:
- Counts line: PESTER Passed=31 Failed=0 Skipped=0 Total=31.
- All thirty-one It blocks passed: the nineteen rules tests, the five git tests and the six orchestration tests named in spec.md, plus the unreadable-branch test.
- Strict-mode parity run (CI preamble, known execution risk 3): Set-StrictMode -Version Latest and $ErrorActionPreference = "Stop" in the calling scope, exactly as .github/workflows/_pester.yml applies them, same configuration with Output.Verbosity "None": STRICT-PESTER Passed=31 Failed=0 Skipped=0 Total=31, exit 0.
- Execution note: the payload was run with a prefix that sets the location and [Environment]::CurrentDirectory to <repo-root> (the item worktree).

Strict-mode setting used: plan-verbatim run without a caller preamble (each test file sets Set-StrictMode -Version Latest at file scope; the entry point sets it at script scope); parity run with Set-StrictMode -Version Latest and $ErrorActionPreference = "Stop" in the calling scope.

```text
IT| Get-TrackedFileRecord.parses a NUL-separated eol listing into path records | Passed
IT| Get-TrackedFileRecord.flags an index-binary record | Passed
IT| Read-TrackedFileText.decodes UTF-16 little-endian bytes by byte-order mark | Passed
IT| Read-TrackedFileText.decodes UTF-8 bytes without a byte-order mark | Passed
IT| Assert-GitExitCode.throws when the git wrapper reports a non-zero exit | Passed
IT| Find-UserProfilePathMatch.matches a backslash-separated profile path assembled at run time | Passed
IT| Find-UserProfilePathMatch.matches a forward-slash-separated profile path | Passed
IT| Find-UserProfilePathMatch.matches a doubled-backslash profile path | Passed
IT| Find-UserProfilePathMatch.matches a lower-case drive letter and profile parent | Passed
IT| Find-UserProfilePathMatch.matches an upper-case profile parent | Passed
IT| Find-UserProfilePathMatch.matches an eight-dot-three user segment | Passed
IT| Find-UserProfilePathMatch.does not match a user-profile placeholder path | Passed
IT| Find-UserProfilePathMatch.does not match a repo-root placeholder path | Passed
IT| Find-UserProfilePathMatch.does not match a bare drive root | Passed
IT| Find-UserProfilePathMatch.does not match a fixtures root | Passed
IT| Find-UserProfilePathMatch.reports the line number and not the matched text | Passed
IT| Get-RawEvidenceDocumentKind.classifies a trx extension as trx | Passed
IT| Get-RawEvidenceDocumentKind.classifies a coverage root on its own line as cobertura | Passed
IT| Get-RawEvidenceDocumentKind.classifies a results root as dotnet-coverage | Passed
IT| Get-RawEvidenceDocumentKind.classifies a report root with class elements as jacoco-raw | Passed
IT| Get-RawEvidenceDocumentKind.classifies a package-only report root as jacoco-projection | Passed
IT| Get-RawEvidenceDocumentKind.classifies a report root behind a DOCTYPE | Passed
IT| Get-RawEvidenceDocumentKind.classifies a byte-order-mark prefixed document | Passed
IT| Get-RawEvidenceDocumentKind.classifies a ps1 file containing a TestRun element as none | Passed
IT| Invoke-RepositoryHygieneMain.excludes governance-directory records and reports the remaining violation | Passed
IT| Invoke-RepositoryHygieneMain.reports a raw document record as a finding | Passed
IT| Invoke-RepositoryHygieneMain.retains a package-level projection record | Passed
IT| Invoke-RepositoryHygieneMain.returns a non-zero exit decision when findings exist | Passed
IT| Invoke-RepositoryHygieneMain.returns a zero exit decision over clean content | Passed
IT| Invoke-RepositoryHygieneMain.prints path and line only and never the matched text | Passed
IT| Invoke-RepositoryHygieneMain.reports a record whose content reader throws as unreadable | Passed
```
