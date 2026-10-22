# P7-T13 — No-Temporary-Files Review Of The Seven Write Set Test Files

Timestamp: 2026-09-13T07-22
Task: [P7-T13]

This review discharges AC22's final clause: that no test in the seven test files the Write Set
carries creates, writes or deletes a file on disk, and that no fixture is loaded from a path. The
seven-file population is the one P0-T16 verified against the spec's Write Set.

## Files reviewed

1. `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1`
2. `tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1`
3. `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`
4. `tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1`
5. `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`
6. `tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1`
7. `tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1`

FILES_REVIEWED: 7

## Scan 1 — filesystem-touching cmdlets

Command: pwsh -NoProfile -Command '<for each of the seven files, emit every line matching the pattern below with its file leaf name and line number>'

Pattern: `Set-Content|Out-File|Add-Content|New-Item|Remove-Item|Copy-Item|Move-Item|New-TemporaryFile|\[IO\.File\]|\[System\.IO\.File\]|Export-|TEMP|Get-Content`

The pattern deliberately includes the read cmdlet and the temporary-file cmdlet as well as the write
and delete cmdlets, so a fixture read from a path and a temporary-file creation would both be caught
rather than only a write.

Every hit, classified:

| File leaf | Line | Form | Classification |
|---|---|---|---|
| Invoke-MSTest.Main.Tests.ps1 | 51 | comment text | comment, not a call |
| Invoke-MSTest.ResultsDirectory.Tests.ps1 | 102 | `Mock Get-Content { throw ... }` | mock declaration |
| Invoke-MSTest.ResultsDirectory.Tests.ps1 | 104 | `Mock Set-Content {}` | mock declaration |
| Invoke-MSTest.ResultsDirectory.Tests.ps1 | 105 | `Mock Remove-Item {}` | mock declaration |
| Invoke-MSTest.ResultsDirectory.Tests.ps1 | 114, 115 | `Should -Invoke ... -Times 0 -Exactly` | assertion on a mock |
| Invoke-MSTest.RunSettings.Tests.ps1 | 236, 239, 243 | `Mock -CommandName ... -MockWith { ... }` | mock declarations |
| Invoke-MSTest.RunSettings.Tests.ps1 | 304, 307, 308, 311, 326, 327, 330 | `Should -Invoke -CommandName ...` | assertions on mocks |
| Invoke-MSTest.RunSettings.Tests.ps1 | 375, 381, 383 | `Mock ...` | mock declarations |
| Invoke-MSTest.RunSettings.Tests.ps1 | 410, 433 | `Should -Invoke Set-Content -Times n -Exactly` | assertions on a mock |
| Invoke-MSTest.RunSettings.Tests.ps1 | 490, 491, 492 | `Mock ...` | mock declarations |
| Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 | 54, 56 | `Mock ...` | mock declarations |
| Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1 | 193, 194, 196, 197, 200 | `Mock ...` | mock declarations |

`tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1` and
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1` produced no hit of any kind.

REAL_FILESYSTEM_CALLS_FOUND: 0

Every occurrence is a `Mock` declaration, a `Should -Invoke` assertion against a mock, or comment
text. A `Mock` declaration replaces the real cmdlet for the scope of the block, so the production
code's write and delete calls are intercepted and nothing reaches disk. A `Should -Invoke` assertion
reads the mock's recorded invocation count and performs no I/O of its own. No test file calls any of
these cmdlets outside a mock.

This distinction is the load-bearing one for this review: a scan that merely counted occurrences of
`Set-Content` would report 12 hits across three files and would look like a violation, when in fact
the presence of those mocks is what guarantees no file is written.

## Scan 2 — fixture loading and code-under-test reads

Command: pwsh -NoProfile -Command '<for each of the seven files, emit every line matching ParseFile, ParseInput or a dot-source of a variable>'

| File leaf | Line | Form | Target |
|---|---|---|---|
| Invoke-MSTest.Main.Tests.ps1 | 12 | dot-source | `scripts/vscode/Invoke-MSTest.ps1` |
| Invoke-MSTest.ResultsDirectory.Tests.ps1 | 14 | `Parser::ParseFile` | `scripts/vscode/Invoke-MSTest.ps1` |
| Invoke-MSTest.ResultsDirectory.Tests.ps1 | 19 | dot-source | `scripts/vscode/Invoke-MSTest.ps1` |
| Invoke-MSTest.RunSettings.Tests.ps1 | 11 | dot-source | `scripts/vscode/Invoke-MSTest.ps1` |
| Invoke-MSTest.RunSettings.Tests.ps1 | 17, 22 | `Parser::ParseFile` then dot-source of the parsed script block | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` |
| Invoke-MSTest.TrxSummary.Tests.ps1 | 6 | dot-source | `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` |
| Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 | 9, 13, 18 | dot-source and `Parser::ParseFile` | the two entry points |
| Invoke-MSTestWithCoverage.Projection.Tests.ps1 | 6 | dot-source | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` |
| Invoke-MSTestWithCoverage.Projection.Tests.ps1 | 457 | `Parser::ParseFile` | `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` |
| Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1 | 13, 18 | `Parser::ParseFile` then dot-source of the parsed script block | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` |

FIXTURES_LOADED_FROM_A_PATH: 0

Every path-bearing read in the seven files targets a production script under `scripts/vscode`. Two
distinct forms appear and neither is a fixture load:

- A dot-source of the script under test, which is how the test file brings the functions it exercises
  into scope. Without it there would be nothing to test.
- A `Parser::ParseFile` over the script under test, which is how the abstract-syntax-tree assertions
  in AC3, AC4, AC5 and AC13 read the production file's structure. The subject of those assertions is
  the production file itself, so reading it is the assertion rather than a fixture load.

Every actual test fixture in these files is an in-memory here-string assigned to a script-scoped
variable inside a `BeforeAll` block or an `It` block. No fixture is read from disk, and no fixture
file exists anywhere in this delivery's footprint.

## Scan 3 — temporary files

The pattern in Scan 1 includes `New-TemporaryFile` and `TEMP`. Neither matched in any of the seven
files.

TEMPORARY_FILES_CREATED: 0

This is the repository's strictest test rule: the General Unit Test Policy prohibits creation and use
of temporary files in tests outright, with no currently approved exceptions. The delivery claims no
exception.

## Corroborating runtime observation

The P7-T3 pass-2 Pester run over the whole test folder was bracketed by a porcelain status taken
immediately before and immediately after. The two listings are byte-identical. A test that created,
wrote or deleted a tracked file, or that left an untracked file behind anywhere outside an ignored
tree, would have changed the second listing. This is a runtime check complementary to the static scans
above: the scans prove no such call is written, and the bracketing proves no such effect occurred.

## Output Summary

Seven files reviewed. REAL_FILESYSTEM_CALLS_FOUND: 0, FIXTURES_LOADED_FROM_A_PATH: 0,
TEMPORARY_FILES_CREATED: 0. Every match for a filesystem cmdlet is a mock declaration, an assertion
against a mock, or comment text; every path-bearing read targets a production script under
`scripts/vscode` as the code under test rather than as a fixture; and the P7-T3 pre- and post-run
porcelain listings are identical. AC22's no-temporary-files clause holds.
