# P1-T11 — Issue #903 manifest entries

Timestamp: 2026-09-19T14-05

Command: `Edit` tool insertion into `ToDoModel.Test/packages.config` (performed on the earlier
round and left in place); verification this round by
`(Get-Content -LiteralPath ToDoModel.Test/packages.config).Count`,
`Select-String -LiteralPath ToDoModel.Test/packages.config -Pattern "Deedle" -SimpleMatch`,
`Select-String -LiteralPath ToDoModel.Test/packages.config -Pattern "FSharp.Core" -SimpleMatch`,
`git grep -n -F -e "Deedle.3.0.0" -e "FSharp.Core.11.0.100" -- "ToDoModel.Test/ToDoModel.Test.csproj"`,
`git status --porcelain --untracked-files=all -- ToDoModel.Test/packages.config`, and the
post-normalisation re-derivation recorded below.

EXIT_CODE: 0

**TASK STATUS: COMPLETE.** All four acceptance clauses hold against plan revision 13, which
re-anchors the line-count identity to the post-normalisation figure P1-T7 produces rather than to
P0-T20's superseded pre-normalisation 172.

## The edit

Two entries in `ToDoModel.Test/packages.config`, at the file's existing alphabetical positions and
in the canonical inline form P1-T7 established:

```
  7: <package id="Deedle" version="3.0.0" targetFramework="net481" />
  9: <package id="FSharp.Core" version="11.0.100" targetFramework="net481" />
```

`Deedle` sits between `Castle.Core` and `FluentAssertions`; `FSharp.Core` sits between
`FluentAssertions` and `Meziantou.Analyzer`. That is the ordering the NuGet CLI produces and the
ordering the other 17 manifests already carry.

Encoding preserved: the file still opens with a UTF-8 byte-order mark and contains 0 bare LF line
endings. Re-rendering the edited file through `ConvertTo-PackagesConfigText` over its own parse
returns it byte-identical, so the insertion is canonical by the same definition P1-T7 and P1-T8
used.

## Post-normalisation baseline — re-derived from the tree

Plan revision 13 amends P1-T7 to record this file's post-normalisation line count. **P1-T7 had
already run when that clause was added, and its artifact
`evidence/qa-gates/p1-t7-normalisation.2026-09-19T09-44.md` carries per-file before-and-after
SHA-256 hashes but no line count.** Per the coordinator's instruction, the figure was re-derived
directly from the current tree rather than by re-running the normalisation, which would have been
a no-op in any case (P1-T8 measured the normaliser idempotent).

Two independent derivations agree.

| Derivation | Command | Result |
|---|---|---|
| Current file with the two #903 entries removed | `(Get-Content ToDoModel.Test/packages.config \| Where-Object { $_ -notmatch "id=.Deedle." -and $_ -notmatch "id=.FSharp\.Core." }).Count` | **71** lines, of which **68** are `<package ` lines |
| Canonical inline structure over the merge-base package population | `git show <MERGE_BASE>:ToDoModel.Test/packages.config` carries **68** `<package ` elements across **172** lines; canonical inline form is 1 XML declaration + 1 `<packages>` + 68 package lines + 1 `</packages>` | **71** |

`<MERGE_BASE>` is `734112ed25bba293cb074e71fee2286bc3b72fae`, the value P0-T3 recorded.

The second derivation is the non-vacuity guard on the first: the first alone would agree with the
measured post-edit count by construction, whereas the second reaches 71 from the merge-base
population without reading the current file's length at all.

POST-NORMALISATION-LINE-COUNT: 71 (re-derived this round; not re-measured by re-running the
normaliser)

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| The file's line count after the edit equals the **post-normalisation** count P1-T7 records for this file plus exactly 2 | post-normalisation **71**, post-edit **73** — exactly plus 2 | PASS |
| A `Select-String` for `Deedle` returns exactly 1 match, against the 0 P0-T20 recorded | **1**, against 0 | PASS |
| A `Select-String` for `FSharp.Core` returns exactly 1 match, against the 0 P0-T20 recorded | **1**, against 0 | PASS |
| The two version literals equal the folder segments `Deedle.3.0.0` and `FSharp.Core.11.0.100` read from `ToDoModel.Test/ToDoModel.Test.csproj:93` and `:96` | `3.0.0` matches `Deedle.3.0.0` at line 93; `11.0.100` matches `FSharp.Core.11.0.100` at line 96 | PASS |
| `git status --porcelain --untracked-files=all -- ToDoModel.Test/packages.config` lists the file as modified | ` M ToDoModel.Test/packages.config` | PASS |

The two `<HintPath>` line citations were re-measured this round rather than carried forward,
because P1-T9 rewrote an `<Analyzer Include>` line in the same file. P1-T9's substitution is
one-for-one on a single line, so nothing below it moves, and `git grep -n` confirms lines 93 and 96
still carry the two HintPaths verbatim.

## Why the line-count identity still discriminates

The clause exists to make a reflowed multi-line insertion fail rather than pass. Against the
post-normalisation baseline of 71 the post-edit count is 73, one line per entry. A reflowed
insertion of the kind the clause is written to catch renders each `<package>` element across 5
lines, which would have produced 81. The clause therefore separates the correct form from the
incorrect one by 8 lines, and revision 13's re-anchoring preserves that discrimination while
removing the unsatisfiable 174 demand.

## Non-vacuity

The two positive match counts of 1 each stand against the 0 and 0 P0-T20 recorded for the same two
patterns in the same file, so neither is an absence-shaped assertion. The version equality is
checked against two `<HintPath>` lines whose existence P0-T20 recorded verbatim with line numbers.
The post-normalisation baseline is derived twice by independent routes rather than asserted.

Output Summary: the two #903 entries are present in `ToDoModel.Test/packages.config` in canonical
inline form at their alphabetical positions with `targetFramework="net481"`, preserving the
byte-order mark and CRLF endings. The file measures 73 lines against a re-derived post-normalisation
baseline of 71, exactly plus 2. `Deedle` and `FSharp.Core` each match exactly once against the 0 and
0 P0-T20 recorded; the two versions agree with the `Deedle.3.0.0` and `FSharp.Core.11.0.100` folder
segments at `ToDoModel.Test/ToDoModel.Test.csproj:93` and `:96`; and porcelain lists the file
modified. All four acceptance clauses pass.
