# P1-T4 Pester hygiene suite before the production files exist (expect-fail, AC2)

Timestamp: 2026-09-29T17-30
Command: pwsh -NoProfile -Command 'Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/hygiene"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; foreach ($t in $r.Tests) { "IT| " + $t.ExpandedPath + " | " + $t.Result }; if ($r.FailedCount -gt 0 -or $r.PassedCount -eq 0) { exit 1 } else { exit 0 }'
ExpectedExitCode: 1
EXIT_CODE: 1
Output Summary:
- Precondition: pwsh -NoProfile -Command '"PROD-EXISTS=" + (Test-Path "scripts/hygiene")' printed PROD-EXISTS=False.
- Counts line: PESTER Passed=0 Failed=31 Skipped=0 Total=31.
- Discovery found 31 tests in 3 files. Every Describe's BeforeAll dot-source threw CommandNotFoundException because the entry point scripts/hygiene/Test-RepositoryHygiene.ps1 does not exist, and Pester reported every It in each of the six Describe blocks as Failed ("BeforeAll \ AfterAll failed: 6").
- Strict-mode parity run (CI preamble, known execution risk 3): the same configuration with Output.Verbosity "None", preceded by Set-StrictMode -Version Latest and $ErrorActionPreference = "Stop" exactly as .github/workflows/_pester.yml applies them, printed STRICT-PESTER Passed=0 Failed=31 Skipped=0 Total=31 and exited 1.
- Execution notes: every payload was run with a prefix that sets the location and [Environment]::CurrentDirectory to <repo-root> (the item worktree), because pwsh launched from Bash starts in the session checkout.

Strict-mode setting used: plan-verbatim run without a caller preamble (each test file sets Set-StrictMode -Version Latest at file scope); parity run with Set-StrictMode -Version Latest and $ErrorActionPreference = "Stop" in the calling scope.

Failure message (one per Describe, path prefix replaced):

```text
CommandNotFoundException: The term '<repo-root>\tests\scripts\hygiene\..\..\..\scripts\hygiene\Test-RepositoryHygiene.ps1' is not recognized as a name of a cmdlet, function, script file, or executable program.
```

Failed It names (31):

```text
IT| Get-TrackedFileRecord.parses a NUL-separated eol listing into path records | Failed
IT| Get-TrackedFileRecord.flags an index-binary record | Failed
IT| Read-TrackedFileText.decodes UTF-16 little-endian bytes by byte-order mark | Failed
IT| Read-TrackedFileText.decodes UTF-8 bytes without a byte-order mark | Failed
IT| Assert-GitExitCode.throws when the git wrapper reports a non-zero exit | Failed
IT| Find-UserProfilePathMatch.matches a backslash-separated profile path assembled at run time | Failed
IT| Find-UserProfilePathMatch.matches a forward-slash-separated profile path | Failed
IT| Find-UserProfilePathMatch.matches a doubled-backslash profile path | Failed
IT| Find-UserProfilePathMatch.matches a lower-case drive letter and profile parent | Failed
IT| Find-UserProfilePathMatch.matches an upper-case profile parent | Failed
IT| Find-UserProfilePathMatch.matches an eight-dot-three user segment | Failed
IT| Find-UserProfilePathMatch.does not match a user-profile placeholder path | Failed
IT| Find-UserProfilePathMatch.does not match a repo-root placeholder path | Failed
IT| Find-UserProfilePathMatch.does not match a bare drive root | Failed
IT| Find-UserProfilePathMatch.does not match a fixtures root | Failed
IT| Find-UserProfilePathMatch.reports the line number and not the matched text | Failed
IT| Get-RawEvidenceDocumentKind.classifies a trx extension as trx | Failed
IT| Get-RawEvidenceDocumentKind.classifies a coverage root on its own line as cobertura | Failed
IT| Get-RawEvidenceDocumentKind.classifies a results root as dotnet-coverage | Failed
IT| Get-RawEvidenceDocumentKind.classifies a report root with class elements as jacoco-raw | Failed
IT| Get-RawEvidenceDocumentKind.classifies a package-only report root as jacoco-projection | Failed
IT| Get-RawEvidenceDocumentKind.classifies a report root behind a DOCTYPE | Failed
IT| Get-RawEvidenceDocumentKind.classifies a byte-order-mark prefixed document | Failed
IT| Get-RawEvidenceDocumentKind.classifies a ps1 file containing a TestRun element as none | Failed
IT| Invoke-RepositoryHygieneMain.excludes governance-directory records and reports the remaining violation | Failed
IT| Invoke-RepositoryHygieneMain.reports a raw document record as a finding | Failed
IT| Invoke-RepositoryHygieneMain.retains a package-level projection record | Failed
IT| Invoke-RepositoryHygieneMain.returns a non-zero exit decision when findings exist | Failed
IT| Invoke-RepositoryHygieneMain.returns a zero exit decision over clean content | Failed
IT| Invoke-RepositoryHygieneMain.prints path and line only and never the matched text | Failed
IT| Invoke-RepositoryHygieneMain.reports a record whose content reader throws as unreadable | Failed
```
