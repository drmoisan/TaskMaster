# P3-T7 — `.github/dependabot.yml` consolidated to one catch-all group

Timestamp: 2026-09-20T00-26

Commands:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $p = (Resolve-Path ".github/dependabot.yml").Path; $lines = [System.IO.File]::ReadAllLines($p); ... "GROUP_KEY_COUNT=" ... "GROUPBY_LINES=" ... "OPEN_PR_LIMIT=" ... "IGNORE_ENTRY_COUNT=" ... "SEMVER_MAJOR_ORDER=" ... "DEEDLE_ENTRIES=" ...'

git -C <W> diff --numstat 734112ed25bba293cb074e71fee2286bc3b72fae -- .github/dependabot.yml
```

EXIT_CODE: 0

The file was rewritten with the `Write` tool, amended with the `Edit` tool, and its CRLF line
endings were restored byte-exactly with `[System.IO.File]::ReadAllText` and `WriteAllText`. No
`sed` was used, per gate rule 15.

## Verbatim measurement output

```
CRLF=45
GROUPBY_LINES=0
GROUP_KEY_COUNT=1 :: all-nuget-updates
OPEN_PR_LIMIT= open-pull-requests-limit: 1
SEMVER_MAJOR_GREP=8
IGNORE_ENTRY_COUNT=9
  Microsoft.Extensions.* [update-types]
  Microsoft.Bcl.* [update-types]
  System.Text.Json [update-types]
  System.Drawing.Common [update-types]
  Microsoft.Graph* [update-types]
  Apache.Arrow* [update-types]
  Microsoft.Data.Analysis [update-types]
  Microsoft.ML* [update-types]
  Deedle []
SEMVER_MAJOR_COUNT=8
SEMVER_MAJOR_ORDER=Microsoft.Extensions.* | Microsoft.Bcl.* | System.Text.Json | System.Drawing.Common | Microsoft.Graph* | Apache.Arrow* | Microsoft.Data.Analysis | Microsoft.ML*
DEEDLE_ENTRIES=1 QUALIFIERS=[]
APPLIES_TO=1
CATCHALL_PATTERN=1
```

```
$ git diff --numstat 734112ed25bba293cb074e71fee2286bc3b72fae -- .github/dependabot.yml
15      32      .github/dependabot.yml
```

## Ordered semver-major list, compared element by element against P0-T22

| # | P0-T22 recorded | Measured now | Match |
|---|---|---|---|
| 1 | `Microsoft.Extensions.*` | `Microsoft.Extensions.*` | yes |
| 2 | `Microsoft.Bcl.*` | `Microsoft.Bcl.*` | yes |
| 3 | `System.Text.Json` | `System.Text.Json` | yes |
| 4 | `System.Drawing.Common` | `System.Drawing.Common` | yes |
| 5 | `Microsoft.Graph*` | `Microsoft.Graph*` | yes |
| 6 | `Apache.Arrow*` | `Apache.Arrow*` | yes |
| 7 | `Microsoft.Data.Analysis` | `Microsoft.Data.Analysis` | yes |
| 8 | `Microsoft.ML*` | `Microsoft.ML*` | yes |

Eight entries, same names, same order, each still carrying
`update-types: ["version-update:semver-major"]`. The eight-line block and its explanatory comment
were carried across unchanged, which is visible in the diff size: 15 added and 32 deleted against a
63-line original, the deletions being the three removed topic groups and their pattern lists.

## The `group-by` zero, and the three positive assertions that guard it

`GROUPBY_LINES=0`. Per gate rule 2 that absence is paired with three positive assertions, each of
which a vacuous measurement would also fail:

| Positive guard | Required | Measured |
|---|---|---|
| Group keys under `groups:` | exactly 1 | 1, named `all-nuget-updates` |
| `applies-to: version-updates` declarations | 1 | 1 |
| Catch-all `"*"` pattern entries | 1 | 1 |
| `open-pull-requests-limit` | `1` | `1` |
| `version-update:semver-major` occurrences | 8 | 8 |

A measurement that read no file would report 0 for all of them.

A drafting note recorded rather than absorbed: the first version of the explanatory comment above
the group contained the literal `group-by` while describing the keys being removed, which made
`GROUPBY_LINES` read **1**. The measurement is a plain line search and does not distinguish a
comment from a key, so the comment was reworded to describe the removed keys without naming the
token. The clause is satisfied by the file as written, not by narrowing the search.

## The Deedle entry

```
      - dependency-name: "Deedle"
```

Exactly **1** entry, with **no** qualifier keys beneath it: neither `versions` nor `update-types`,
and no other indented key. The qualifier list measured for that entry is empty, while all eight
semver-major entries measure `update-types`, so the parser that produced the empty list is
demonstrably able to see a qualifier when one is present.

The entry carries a comment recording why Deedle is ignored outright rather than at a single
update type: its published packages target `netstandard2.1` and later only, which `net481` cannot
consume at any version.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| Group keys | exactly 1 | 1 (`all-nuget-updates`) | PASS |
| Lines matching `group-by` | exactly 0 | 0 | PASS |
| `open-pull-requests-limit` | `1` | `1` | PASS |
| `Deedle` ignore entries | exactly 1, unqualified | 1, qualifier list empty | PASS |
| Ordered semver-major `dependency-name` list | equals the P0-T22 8-member list, element by element | identical, 8 of 8 | PASS |
| The group declares `applies-to: version-updates` and the catch-all pattern | both | both present, 1 each | PASS |

Output Summary: `.github/dependabot.yml` now declares exactly **1** group, `all-nuget-updates`,
carrying `applies-to: version-updates` and the catch-all pattern `"*"`, with
`open-pull-requests-limit: 1` replacing 10. All four inert per-dependency partition keys are gone:
the file contains **0** lines matching `group-by`, guarded by the four positive counts above. The
`ignore` block carries **9** entries — the original **8** `version-update:semver-major` entries in
the same order, matched element by element against the P0-T22 census, plus **1** new unqualified
`Deedle` entry with no `versions` and no `update-types` key beneath it. The change is 15 added and
32 deleted against `MERGE_BASE`, and CRLF line endings were preserved at 45 of 45 lines.
