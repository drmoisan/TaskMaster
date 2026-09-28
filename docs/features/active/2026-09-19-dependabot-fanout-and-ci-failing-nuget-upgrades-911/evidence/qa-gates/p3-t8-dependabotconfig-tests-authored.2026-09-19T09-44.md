# P3-T8 — `tests/scripts/dependencies/DependabotConfig.Tests.ps1` authored

Timestamp: 2026-09-20T00-33

Command — structural measurement from the PowerShell parser's abstract syntax tree:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $p = (Resolve-Path "tests/scripts/dependencies/DependabotConfig.Tests.ps1").Path; ... "IT_COUNT=" ... "IT_AC1_PREFIX=" ... "OUTER_AC_DIGIT=" ... "IMPORT_MODULE_STATEMENTS=" ...'
```

EXIT_CODE: 0

The file was created with the `Write` tool and amended with the `Edit` tool. No heredoc and no
shell redirection was used.

## Verbatim output

```
LINECOUNT=196
PARSE_ERRORS=0
IT_COUNT=5
IT_AC1_PREFIX=5
OUTER_AC_DIGIT=0
IMPORT_MODULE_STATEMENTS=0
NONASCII=0
ANALYZER=0
TEMPFILE_API_HITS=0
--- It names ---
 * AC1- declares exactly one entry under groups
 * AC1- declares applies-to version-updates and a catch-all pattern on that entry
 * AC1- limits open pull requests to one
 * AC1- carries one unqualified Deedle ignore entry
 * AC1- retains the merge-base semver-major pair set element by element
--- Outer ---
 * Dependabot configuration consolidation
 * Grouping and pull-request volume
 * Ignore entries
```

## The five AC1 assertions, one `It` each

| AC1 clause | `It` name |
|---|---|
| exactly one entry under `groups` | `AC1- declares exactly one entry under groups` |
| that entry declares `applies-to: version-updates` and the catch-all pattern | `AC1- declares applies-to version-updates and a catch-all pattern on that entry` |
| `open-pull-requests-limit` equals 1 | `AC1- limits open pull requests to one` |
| a `Deedle` ignore entry with neither a `versions` nor an `update-types` qualifier | `AC1- carries one unqualified Deedle ignore entry` |
| the semver-major pair set equals a literal expected set, element by element | `AC1- retains the merge-base semver-major pair set element by element` |

## No module dependency

`IMPORT_MODULE_STATEMENTS=0`. The file contains no `Import-Module` statement of any kind, so in
particular none naming a module outside `scripts/`. The parse is three text helpers defined in
`BeforeAll` — `Get-DependabotGroupKey`, `Get-DependabotGroupBody` and `Get-DependabotIgnoreEntry` —
each reading the fixed two-space-indented block structure with a line regex. `powershell-yaml` is
deliberately not taken as a dependency: it is not guaranteed present on the `windows-latest`
runner, and an absent module would turn the CI `pester` job red for a reason unrelated to this
configuration.

The suite reads exactly one file, `.github/dependabot.yml`, which is the artefact under test.
`TEMPFILE_API_HITS=0`: no temporary file is created.

## The literal expected set

Declared in `BeforeAll` as `$script:ExpectedSemverMajorPair`, an eight-member array in file order:

| # | Literal declared in the test |
|---|---|
| 1 | `Microsoft.Extensions.*\|version-update:semver-major` |
| 2 | `Microsoft.Bcl.*\|version-update:semver-major` |
| 3 | `System.Text.Json\|version-update:semver-major` |
| 4 | `System.Drawing.Common\|version-update:semver-major` |
| 5 | `Microsoft.Graph*\|version-update:semver-major` |
| 6 | `Apache.Arrow*\|version-update:semver-major` |
| 7 | `Microsoft.Data.Analysis\|version-update:semver-major` |
| 8 | `Microsoft.ML*\|version-update:semver-major` |

These are the eight `dependency-name` values the P0-T22 census recorded at the merge base, in the
same order. The comparison is a per-index `Should -BeExactly` against this literal, preceded by a
count equality, so a reordering, a rename, a dropped entry and an added entry each fail
distinguishably.

## Non-vacuity built into the assertions

Three of the five cases would otherwise be satisfiable by a parser that found nothing, so each
carries its own positive guard:

| Case | Guard |
|---|---|
| catch-all pattern | `$body.Count | Should -BeGreaterThan 0` before any count over the body |
| unqualified Deedle entry | `$entry.Count | Should -BeGreaterThan 1` before the empty-qualifier assertion, so the empty qualifier list is read from a parser that demonstrably found the eight qualified entries too |
| single group key | the key itself asserted non-empty, not only the count |

## Token discipline, per gate rule 11

All five `It` names begin with the token `AC1-`, trailing hyphen included, and `IT_AC1_PREFIX=5`
equals `IT_COUNT=5`. The trailing hyphen is what will keep `AC10-`, `AC11-` and `AC12-` out of the
`*AC1-*` filtered population at P3-T9. `OUTER_AC_DIGIT=0` over the three enumerated outer names,
measured with the regex `AC\d` rather than the bare two letters.

`IT_COUNT` is 5 at this task by design; P3-T10 extends the same file with `AC4-` cases, which is
why the AC1 count is asserted by prefix rather than by file total.

## Trial run

```
PESTER Passed=5 Failed=0 Skipped=0 Total=5
```

Recorded as context. The filtered gated run is P3-T9.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| File at most 500 lines | `<= 500` | 196 | PASS |
| `Import-Module` statements naming a module outside `scripts/` | exactly 0 | 0 `Import-Module` statements at all | PASS |
| `It` names beginning `AC1-` | exactly 5 | 5 | PASS |
| `Describe` and `Context` names matching `AC\d` | exactly 0 | 0, over 3 enumerated names | PASS |
| The literal expected set is the P0-T22 8-member list | 8 names in file order | identical, enumerated above | PASS |
| File parses | no parse error | `PARSE_ERRORS=0` | PASS |

`ANALYZER=0` and `NONASCII=0` are additionally recorded against P4-T2's zero-owned-findings clause.

Output Summary: `tests/scripts/dependencies/DependabotConfig.Tests.ps1` was authored with the
`Write` tool at **196** lines and parses with **0** errors. It declares exactly **5** `It` blocks,
all **5** prefixed `AC1-`, one per AC1 clause, and **0** of its 3 `Describe`/`Context` names match
the regex `AC\d`. It takes **0** module dependencies: the parse is three text helpers defined in
`BeforeAll`, so the CI `pester` job cannot fail on an absent `powershell-yaml`. The literal
expected set is the eight `dependency-name` values the P0-T22 census recorded, in file order,
compared per index. Three of the five cases carry an explicit positive guard so a parser that
found nothing cannot pass them. A trial run reports `Passed=5 Failed=0 Total=5`.
