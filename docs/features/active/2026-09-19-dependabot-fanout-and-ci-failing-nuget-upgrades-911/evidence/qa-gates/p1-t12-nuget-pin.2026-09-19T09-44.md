# P1-T12 — NuGet CLI pinned to 7.9.0

Timestamp: 2026-09-19T14-12

Command:
```
git grep -c -F "nuget-version: latest" -- ".github/workflows/"
git grep -n -F "nuget-version: '7.9.0'" -- ".github/workflows/"
git diff --numstat 734112ed25bba293cb074e71fee2286bc3b72fae -- ".github/workflows/_build-analyzers.yml" ".github/workflows/_build-nullable.yml" ".github/workflows/_mstest-coverage.yml"
git status --porcelain --untracked-files=all -- ".github/workflows/"
```

EXIT_CODE: 0

## The edit

Three sites, each replacing the floating selector with the exact three-part version and each gaining
one comment line of rationale immediately above it. The edits were made with the `Edit` tool, not
with `sed` through the Bash tool, per gate rule 15.

| File | Line before edit | Line after edit |
|---|---|---|
| `.github/workflows/_build-analyzers.yml` | 33 | 34 |
| `.github/workflows/_build-nullable.yml` | 33 | 34 |
| `.github/workflows/_mstest-coverage.yml` | 49 | 50 |

Each site now reads:

```
          # Pinned: the tool that rewrites .csproj and app.config during an upgrade must be a known quantity for a given commit, and 7.9.0 is what the floating selector resolved to, so this freezes current behaviour rather than changing it.
          nuget-version: '7.9.0'
```

The pin freezes the behaviour the three workflows already had: `7.9.0` is the version the floating
selector resolved to, so no CI behaviour changes at this commit. What changes is that a later
upgrade of the NuGet CLI becomes a visible, reviewable edit rather than an invisible drift in a
tool that rewrites `.csproj` and `app.config`.

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| Count of lines matching `nuget-version: latest` across `.github/workflows/` is exactly 0 | `git grep -c -F` printed no output and returned exit 1, which is the no-match signal; **0** files, against the 3 P0-T21 recorded | PASS |
| Count of lines matching `nuget-version: '7.9.0'` is exactly 3 | **3** lines across **3** files, enumerated above | PASS |

The positive count of 3 guards the zero: a deletion of the three `with:` blocks, or a rename of the
workflow files, would also drive the `latest` count to 0, and the pinned count would then be 0 too.

## Diff shape

`git diff --numstat <MERGE_BASE>`, with `<MERGE_BASE>` the value `734112ed25bba293cb074e71fee2286bc3b72fae`
P0-T3 recorded:

```
2	1	.github/workflows/_build-analyzers.yml
2	1	.github/workflows/_build-nullable.yml
2	1	.github/workflows/_mstest-coverage.yml
```

Six added and three deleted lines across three files — one replaced selector line plus one comment
line per site, which is exactly the shape the edit should have. A line-ending-only rewrite would
show 0 added and 0 deleted while porcelain still listed three modified files, so the line totals
rather than the file count are what distinguish the real substitution here, per gate rule 15.

`git status --porcelain --untracked-files=all -- ".github/workflows/"` is the companion capture
required by gate rule 8:

```
 M .github/workflows/_build-analyzers.yml
 M .github/workflows/_build-nullable.yml
 M .github/workflows/_mstest-coverage.yml
```

It lists the same three paths and nothing else, so this task created no untracked workflow file and
perturbed no other workflow.

## Note for later positional citations

The inserted comment shifts every line below it in the three files by one. No task after this one
cites a line number in any of the three, verified by searching the plan for `_build-analyzers.yml:`,
`_build-nullable.yml:` and `_mstest-coverage.yml:`: the only citations are at plan line 253 and in
the P0-T21 baseline artifact, both of which describe the pre-edit state and are superseded by this
artifact rather than read forward. `.github/workflows/_pester.yml` is a different file and is
unaffected by this task.

Output Summary: the three `nuget/setup-nuget@v2` sites at `_build-analyzers.yml`, `_build-nullable.yml`
and `_mstest-coverage.yml` now pin `nuget-version: '7.9.0'`, each with a one-line rationale comment.
The floating-selector count is 0, against the 3 P0-T21 recorded, and the pinned-literal count is 3.
The merge-base numstat totals 6 added and 3 deleted lines across the 3 files, matching a
one-replacement-plus-one-comment edit per site.
