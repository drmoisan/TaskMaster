# P1-T9 Interim Pester line coverage over scripts/hygiene (advisory)

Timestamp: 2026-09-29T17-39
Command: pwsh -NoProfile -Command 'Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/hygiene"); $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/hygiene"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/pester-coverage.xml"; $r = Invoke-Pester -Configuration $c; ... (the PESTER-CI-SHAPE parsing lines, unchanged)'
EXIT_CODE: 0
Output Summary:
- Final iteration (iter2): PESTER Passed=31 Failed=0 Skipped=0 Total=31; COVERAGE LinePercent=94.06 Covered=95 Total=101.
- Per file (iter2): Test-RepositoryHygiene.Git.ps1 34/37 = 91.89%; Test-RepositoryHygiene.ps1 30/33 = 90.91%; Test-RepositoryHygiene.Rules.ps1 31/31 = 100.00%. All three at or above 90%.
- The remaining missed lines are the three lines of the Invoke-GitExe body (Git.ps1 lines 50 to 52, the only place git is invoked; tests mock the wrapper and never run the executable) and the three lines of the script-entry guard block (Test-RepositoryHygiene.ps1 lines 84 to 86, which do not run when the tests dot-source the script).
- The JaCoCo document stays under the ignored coverage directory; only these figures are committed.

## iter1 (Timestamp 2026-09-29T17-35)

- PESTER Passed=31 Failed=0 Skipped=0 Total=31; COVERAGE LinePercent=83.17 Covered=84 Total=101.
- FILE| hygiene/Test-RepositoryHygiene.Git.ps1 | covered=30 missed=7 (81.08%, below 90).
- FILE| hygiene/Test-RepositoryHygiene.ps1 | covered=30 missed=3 (90.91%).
- FILE| hygiene/Test-RepositoryHygiene.Rules.ps1 | covered=24 missed=7 (77.42%, below 90).
- Missed lines: Git.ps1 50, 51, 52, 79 (malformed-record throw), 138 (empty byte result), 152 (UTF-16 big-endian decode), 157 (UTF-8 with mark decode); Rules.ps1 91 and 92 (coverage and coveragexml extensions), 98 (empty xml content), 106 (no root element), 112 (TestRun root), 113 (CoverageSession root), 122 (unknown root).

## Remediation between iter1 and iter2 (tests added; no exclusion; no production change)

- tests/scripts/hygiene/Test-RepositoryHygiene.Git.Tests.ps1, It "parses a NUL-separated eol listing into path records": added the assertion that a record without the attribute-path tab separator throws "Malformed git ls-files --eol record" (Git.ps1 line 79).
- tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1, It "returns a zero exit decision over clean content": the listing additionally carries clean text delivered as UTF-8-with-mark bytes and as UTF-16 big-endian bytes, an empty byte array, an empty xml record, an xml record with an unrecognised root and an xml record with no element; FindingCount 0 and ExitCode 0 are unchanged expectations (Git.ps1 lines 138, 152, 157; Rules.ps1 lines 98, 106, 122).
- tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1, It "returns a non-zero exit decision when findings exist": the listing additionally carries a TestRun-root xml, a CoverageSession-root xml, a coverage-extension record and a coveragexml-extension record; the expectation becomes FindingCount 5 with four raw-document lines and ExitCode 1 (Rules.ps1 lines 91, 92, 112, 113).
- The shared content delegate in the orchestration test file now returns a byte-array value with the unary comma so the adapter receives it whole.

Deviation recorded: P1-T9 names "the corresponding test file" as the place to add tests. Rules.ps1 coverage was raised through the orchestration test file instead of tests/scripts/hygiene/Test-RepositoryHygiene.Rules.Tests.ps1, because the binding PowerShell batch budget for this session (3 production and 3 test files) has one test slot reserved for tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 (P3-T14), and editing both the rules and the git test files would have exceeded it. No It was added or renamed: the It counts stay at 19, 5 and 7 (31 in total), as P1-T1 to P1-T3 and P6-T3 require. Both edited test files keep their UTF-8 byte-order mark.
