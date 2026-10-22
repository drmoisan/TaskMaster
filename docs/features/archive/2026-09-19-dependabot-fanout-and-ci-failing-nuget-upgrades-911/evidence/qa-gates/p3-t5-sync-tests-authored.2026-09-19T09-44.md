# P3-T5 — `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` authored

Timestamp: 2026-09-20T00-14

Command — structural measurement taken from the PowerShell parser's own abstract syntax tree:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $p = (Resolve-Path "tests/scripts/vscode/Sync-PackageReferences.Tests.ps1").Path; $t = [System.IO.File]::ReadAllText($p); "LINECOUNT=" + ([System.IO.File]::ReadAllLines($p)).Count; $tok = $null; $err = $null; $ast = [System.Management.Automation.Language.Parser]::ParseInput($t, [ref]$tok, [ref]$err); "PARSE_ERRORS=" + $err.Count; $calls = $ast.FindAll({ param($n) $n -is [System.Management.Automation.Language.CommandAst] }, $true); ... ; "IT_COUNT=" + $its.Count; "IT_AC7_PREFIX=" + @($its | Where-Object { $_ -cmatch "^AC7-" }).Count; "OUTER_AC_DIGIT=" + @($outer | Where-Object { $_ -match "AC\d" }).Count; "MOCK_CALLS=" + $mocks'
```

EXIT_CODE: 0

The file was created with the `Write` tool and amended with the `Edit` tool. No heredoc and no
shell redirection was used.

## Verbatim output

```
LINECOUNT=185
PARSE_ERRORS=0
IT_COUNT=6
IT_AC7_PREFIX=6
OUTER_AC_DIGIT=0
MOCK_CALLS=0
TEMPFILE_API_HITS=0
NONASCII=0
ANALYZER=0
--- It names ---
 * AC7- resolves net481 through the shared module when net481 is present
 * AC7- resolves net48 through the shared module when net481 is absent
 * AC7- resolves netstandard2.0 when the package ships netstandard2.1 and netstandard2.0
 * AC7- resolves no selection for the three unconsumable asset sets
 * AC7- declares no ordering of its own, so an asset set the deleted array would have resolved returns no selection
 * AC7- repairs a stale hint path to the asset folder the shared module selects
--- Outer names ---
 * Sync-PackageReferences framework selection parity with the shared module
 * Selection cases resolved through the script wrapper
 * Absence of any ordering local to the script
 * End-to-end repair driven through the injected seam
```

## The four AC7 selection cases, each asserted for parity

Each of the first four `It` blocks calls the script's own `Resolve-PackageAssetFolder` and the
module's `Select-CompatibleAssetFolder` over the **same** offered asset set, asserts the two agree,
and then asserts the agreed answer is the one AC7 names. Parity alone would be satisfied by two
functions that are both wrong; the second assertion is what pins the value.

| AC7 case | Offered asset set | Asserted value |
|---|---|---|
| returns `net481` when present | `net45`, `netstandard2.0`, `net481` | `net481` |
| returns `net48` when `net481` absent | `net45`, `net48`, `netstandard2.0` | `net48` |
| returns `netstandard2.0` when offered with `netstandard2.1` | `netstandard2.1`, `netstandard2.0` | `netstandard2.0` |
| returns no selection | three sub-forms: `netstandard2.1` alone; `net6.0`/`netcoreapp3.1`; the empty set | empty in all three, each also asserted equal to the module's answer |

## The script declares no ordering of its own

The fifth `It` exercises the asset set `net6.0`, `netstandard2.1`. The deleted `$tfmPreference`
array listed `netstandard2.1` as its second-to-last member, so a surviving fixed ordering returns
`netstandard2.1` for this set; the correct answer is no selection. That is the point at which a
local ordering would diverge from the module, and the assertion is that the script returns no
selection.

The non-selection is not asserted alone. The same `It` carries a positive control over
`net6.0`, `netstandard2.1`, `net472`, asserting the same resolver returns `net472`. A resolver that
had been broken into returning nothing for every input would pass the first assertion and fail the
second.

The sixth `It` drives the whole entry point, `Invoke-PackageReferenceSync`, over a seam whose
package no longer ships the asset folder the project was bound to. It asserts the written project
text binds to `lib\net472\`, does **not** contain `netstandard2.1`, and carries the reconciled
`Version=2.0.0.0`, alongside an examined count of 1, a fixed count of 1 and exactly one write.

## Boundaries are mocked at the wrapper-function seam only

`MOCK_CALLS=0`. The suite calls Pester's `Mock` command zero times and therefore mocks no
executable, real or otherwise. Every external boundary is supplied as an in-memory delegate table
handed to the script's own `-Seam` parameter, built by the two `BeforeAll` helpers `Get-AssetSeam`
and `Get-RepairSeam`. Per the Pester 5 scoping rule, both helpers are defined inside `BeforeAll`.

`TEMPFILE_API_HITS=0` across the pattern set `New-Item`, `Out-File`, `Set-Content`, `Add-Content`,
`GetTempFileName`, `GetTempPath`, `TestDrive`, `env:TEMP`, `env:TMP`, `New-TemporaryFile`. The
end-to-end case's writes land in an in-memory hashtable passed as the write sink. The zero is
guarded by the positive observation that the suite performs three filesystem reads in `BeforeAll`
— resolving the repository root, importing the module and dot-sourcing the script under test — and
by `IT_COUNT=6`, so a run that resolved no file is distinguishable.

## Fixture correction made during authoring, recorded rather than absorbed

The end-to-end case initially asserted `net472` against a seam in which every probe except the
stale `1.0.0` path resolved. It failed, and the failure was correct: `Get-HintPathRepair` prefers
the project's **existing** asset folder at the corrected version before asking the module to
select, which is the production behaviour the merge-base script also had. Under that seam the
existing `net45` folder resolved at version `2.0.0`, so no module selection was needed and none
happened.

The fixture, not the production code, was wrong: it modelled a package that still shipped the old
asset folder at the new version, which is not the state the case is about. The seam now reports
that version `2.0.0` no longer ships a `net45` asset, and the offered set is `netstandard2.1`,
`net472`. The case then exercises the module-selection path it was written to exercise. No
production code and no acceptance was changed to make it pass.

## Trial run

```
PESTER Passed=6 Failed=0 Skipped=0 Total=6
```

Recorded as context. The gated run over both AC7 suites is P3-T6.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| File at most 500 lines | `<= 500` | 185 | PASS |
| `It` blocks | at least 5 | 6 | PASS |
| `Describe` and `Context` names matching `AC\d` | exactly 0 | 0, over 4 enumerated outer names | PASS |
| Every AC-bearing `It` name begins `AC7-` | all | 6 of 6 | PASS |
| External boundaries mocked at the wrapper-function seam | delegate table only | `MOCK_CALLS=0`; both seams injected through `-Seam` | PASS |
| No real executable mocked | none | none; no `Mock` call exists | PASS |
| No temporary file created | none | 0 temp/write API hits | PASS |
| File parses | no parse error | `PARSE_ERRORS=0` | PASS |

`ANALYZER=0` and `NONASCII=0` are additionally recorded, because a non-ASCII byte without a
byte-order mark raises `PSUseBOMForUnicodeEncodedFile` and would have counted against P4-T2's
zero-owned-findings clause.

Output Summary: `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` was authored with the
`Write` tool at **185** lines and parses with **0** errors. It declares **6** `It` blocks, all
**6** prefixed `AC7-`, against a required minimum of 5, and **0** of its 4 `Describe`/`Context`
names match the regex `AC\d`. The first four cases assert that the script's
`Resolve-PackageAssetFolder` returns exactly what the module's `Select-CompatibleAssetFolder`
returns for each AC7 selection case and that the agreed value is the one AC7 names; the fifth
shows the script has no ordering of its own, with a positive `net472` control alongside; the sixth
drives the entry point end-to-end and asserts the written project text binds to `net472`, never to
`netstandard2.1`. Boundaries are injected as an in-memory delegate table: **0** Pester `Mock`
calls, **0** temporary-file API calls, **0** PSScriptAnalyzer findings. A trial run reports
`Passed=6 Failed=0 Total=6`. One fixture error found during authoring is recorded above rather
than absorbed.
