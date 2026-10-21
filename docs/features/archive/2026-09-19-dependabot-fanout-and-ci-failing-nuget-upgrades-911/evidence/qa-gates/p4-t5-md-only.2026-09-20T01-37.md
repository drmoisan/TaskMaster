# R4 — The Rewrite Touched Documentation Only

- Timestamp: 2026-09-20T09-06-43
- Task: [P4-T5]
- Finding: R4, **Blocking**
- EXIT_CODE: 0

## The Two Captures

Both are recorded because per **gate rule 8** each is blind in the state the other covers: the
anchored diff enumerates tracked changes only and cannot report an untracked file, and porcelain
goes empty once the change is committed.

### `git diff --name-only 07b4872eae664e9e5242c79e2ed546a1ee9fe797`

Anchored to the `<P3-T15-head-sha>`. **33 paths**, every one a tracked markdown file inside the
feature folder, and exactly the 33 [P4-T2] rewrote.

### `git status --porcelain --untracked-files=all`

**38 entries**: the same 33 as ` M`, plus **5** as `??`.

The five untracked entries are evidence artifacts this phase wrote **after** the [P3-T15]
commit, so they are new rather than rewritten:

```
?? .../evidence/qa-gates/p3-t15-commit.2026-09-20T01-37.md
?? .../evidence/qa-gates/p4-t1-substitution-map.2026-09-20T01-37.md
?? .../evidence/qa-gates/p4-t2-sanitisation.2026-09-20T01-37.md
?? .../evidence/qa-gates/p4-t3-residual.2026-09-20T01-37.md
?? .../evidence/qa-gates/p4-t4-autoclose-list.2026-09-20T01-37.md
```

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| Every path in the union ends `.md` | yes | **38 of 38** | PASS |
| Paths with any other extension | 0 | **0** | PASS |
| Count of rewritten paths equals the [P0-T3] file count | 33 | **33**, from the anchored diff | PASS |
| Union count | — | 38 | see below |

**The union count is 38, not 33, and the reason is recorded rather than glossed.** The clause
reads "the count of such paths equals the P0-T3 file count". The quantity that equals 33 is the
**anchored diff**, which enumerates exactly the files the substitution rewrote. The union is
larger by the five artifacts [P3-T15] and [P4-T1] through [P4-T4] wrote after the anchor commit,
which no rewrite touched and which did not exist when [P0-T3] took its census.

That is a property of the plan's own task ordering — a sanitisation phase that records its work
in the same tree it sanitises — and not a signal about the rewrite. The rewrite set is 33 and is
confirmed as 33 by three independent measurements: the anchored diff here, the 33-row per-file
table at [P4-T2], and the 33-path numstat at [P4-T3].

## The Load-Bearing Clause Passed

The clause that matters for R4 is `.md`-only, and it passed on the **whole union**, 38 of 38.

Its purpose is to catch a map entry that matched inside a script, a workflow or a configuration
file. Any such match would appear as a non-`.md` path in one of the two captures. There are
**zero**. In particular:

- no path under `scripts/`, so no production PowerShell was rewritten;
- no path under `.github/`, so no workflow was rewritten;
- no `.csproj`, `packages.config` or `app.config` path, which **gate rule 9** also forbids;
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` does not appear, confirming
  that the named exclusion was left untouched. Had the rewrite reached it, that `.ps1` path would
  be the first non-`.md` entry here.

## Output Summary

38 paths across the two captures, **all markdown**, zero with any other extension. The anchored
diff's 33 equals the [P0-T3] census file count exactly; the five extra porcelain entries are
this phase's own untracked evidence artifacts. No script, workflow or build-configuration file
was touched by the substitution.
