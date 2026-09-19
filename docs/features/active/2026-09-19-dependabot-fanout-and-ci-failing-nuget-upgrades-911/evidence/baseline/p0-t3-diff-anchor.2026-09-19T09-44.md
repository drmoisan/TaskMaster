# P0-T3 — Diff Anchor Pin

Timestamp: 2026-09-19T12-18

Command:
```
git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/dependabot-911" fetch origin main
git -C <W> rev-parse origin/main
git -C <W> rev-parse main
git -C <W> rev-parse HEAD
git -C <W> merge-base origin/main HEAD
git -C <W> cat-file -t 734112ed25bba293cb074e71fee2286bc3b72fae
git -C <W> merge-base --is-ancestor 734112ed25bba293cb074e71fee2286bc3b72fae HEAD
git -C <W> rev-list --count 734112ed25bba293cb074e71fee2286bc3b72fae..HEAD
```

EXIT_CODE: 0

## Pinned anchor

```
MERGE_BASE = 734112ed25bba293cb074e71fee2286bc3b72fae
```

Every diff, merge-base, footprint and scope check in this plan substitutes this 40-character value
for `<MERGE_BASE>`. No gate anchors to bare `main` or to `origin/main`.

## Recorded values

| Measurement | Value |
|---|---|
| `git fetch origin main` | exit 0; `branch main -> FETCH_HEAD` |
| `git rev-parse origin/main` | `734112ed25bba293cb074e71fee2286bc3b72fae` |
| `git rev-parse main` | `734112ed25bba293cb074e71fee2286bc3b72fae` |
| `git rev-parse HEAD` | `8b0afe2c48060804ded103db62a4c3e5eceef8f9` |
| `MERGE_BASE` | `734112ed25bba293cb074e71fee2286bc3b72fae` (40 hex characters) |
| `git cat-file -t <MERGE_BASE>` | `commit` |
| `git merge-base --is-ancestor <MERGE_BASE> HEAD` | EXIT_CODE 0 |
| `git rev-list --count <MERGE_BASE>..HEAD` | `8` |

## Acceptance evaluation

- `MERGE_BASE` is 40 hexadecimal characters — measured length 40. PASS.
- `git cat-file -t <MERGE_BASE>` prints `commit`. PASS.
- The ancestor check against **HEAD** returns `EXIT_CODE: 0`. PASS.
- `<MERGE_BASE>` differs from `git rev-parse HEAD` (`734112ed2…` against `8b0afe2c4…`). PASS.
- `git rev-list --count <MERGE_BASE>..HEAD` is an integer greater than 0 — measured **8**. PASS.

**Observation on the count.** The plan records this figure as "measured at 4 at the time this plan
was written". It is 8 now. The acceptance condition is "an integer greater than 0", which 8
satisfies; the 4 is a parenthetical record of an earlier measurement rather than an asserted value,
and the growth is fully accounted for by the four further documentation commits the plan itself
describes (plan revisions 4 through 7 and the AC12 spec amendment). The eight commits are:

```
8b0afe2c4 docs(911): plan revision 7 - anchor the branch discriminator, drop the inert footprint floor
8f0257116 docs(911): plan revision 6 - 898 merge-order branch selector and verifier surface
bf9a6d2b9 docs(911): amend spec AC12 to the analyzer folder preserve rule
0f714dfb3 docs(911): plan revisions 4 and 5 - analyzer folder preservation and byte-exact path rewrites
d43a0b226 docs(911): plan revision 2 resolving eight blocking preflight defects
32594c3cb docs(911): add the atomic plan and the cold-restore failing control
ea99d66f6 docs(911): correct the diagnosis and add spec, research and runbook
d46ae2dc6 docs(911): promote dependabot fan-out and CI-failing NuGet upgrade bug
```

`bf9a6d2b9` is the AC12 spec amendment P1-T1 verifies, and `d43a0b226` is the plan-file sync commit
P0-T24 verifies; both are present on this branch.

**Why this count is the change-relevant figure and can fail.** It is what makes every
`<MERGE_BASE>` diff in this plan non-vacuous. A branch sitting exactly on the merge-base would
report 0, meaning no commit has landed to diff against, and the gate would fail.

## The two assertions deliberately not used, recorded as observations

Neither can fail, so neither is an acceptance condition. Both were measured and are recorded here.

- `git merge-base --is-ancestor <MERGE_BASE> origin/main` returned **EXIT_CODE 0**. This is true by
  the definition of a merge-base and carries no information.
- `git rev-list --count main..origin/main` returned **0**. Local `main` and `origin/main` are the
  same commit `734112ed25bba293cb074e71fee2286bc3b72fae` in this worktree, so the count is 0 by
  construction.

## Basis of the diff-anchor prohibition

The prohibition on anchoring a gate to `origin/main` rests on `origin/main` being a **moving** ref:
it can advance mid-run, so a gate anchored to it is not reproducible and two tasks in the same run
can compare against different trees. It does **not** rest on any staleness of local `main`, which
this measurement shows is identical to `origin/main` here. The three-dot form `PINNED...HEAD`
remains prohibited as a substitute: when the pinned ref is an ancestor of HEAD — which the ancestor
check above confirms it is — the three-dot form degenerates to the two-dot diff and inherits the
same reproducibility defect.

Output Summary: MERGE_BASE pinned to `734112ed25bba293cb074e71fee2286bc3b72fae`, 40 hex characters,
object type `commit`, ancestor of HEAD (exit 0), distinct from HEAD `8b0afe2c4…`, with 8 commits in
`<MERGE_BASE>..HEAD` against a required minimum of 1. `origin/main` and local `main` are identical
at `734112ed2…`. All five acceptance clauses hold.
