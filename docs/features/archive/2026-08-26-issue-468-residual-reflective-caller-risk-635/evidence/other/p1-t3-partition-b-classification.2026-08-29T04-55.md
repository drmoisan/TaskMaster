# Partition B Sweep and Total Classification (P1-T3) — discharges AC-4 and AC-5

- **Issue:** #635
- **Plan task:** [P1-T3]

Timestamp: 2026-08-29T06-27

## Output Summary

The thirteen-identifier sweep over tracked non-`.cs` files, this time including the docs tree and the
.claude tree, returned 2,337 hits. Every hit is assigned to exactly one category by a test derived from
its path alone. 2,319 hits are authored documentation or generated evidence under the docs tree,
18 are agent-memory prose under the .claude tree, and the category "genuine name-based caller of a
removed member" is empty. The two per-category counts sum to the recorded total.

TOTAL: 2337
CAT_D_DOCS: 2319
CAT_E_CLAUDE: 18
CAT_G_OTHER: 0

## Command

Command:

```
pwsh -NoProfile -Command '$h = git grep -n -I -F -e WireUpKeyboardHandler -e AnyOpenDropDownsAsync -e LoadGroups_02cAsync -e LoadGroups_02bAsync -e LoadGroup_03bAsync -e LoadConversationsAndFoldersAsync -e LoadItemGroup -e LoadSequentialAsync -e LoadGroupSequential -e CacheTlpForMove -e SwapTlp -e CaptureTlpTemplate -e _templateTlp -- ":(exclude)*.cs"; Write-Output ("TOTAL=" + $h.Count); Write-Output ("CAT_D_DOCS=" + @($h | Where-Object { $_ -like "docs/*" }).Count); Write-Output ("CAT_E_CLAUDE=" + @($h | Where-Object { $_ -like ".claude/*" }).Count); Write-Output ("CAT_G_OTHER=" + @($h | Where-Object { -not ($_ -like "docs/*") -and -not ($_ -like ".claude/*") }).Count)'
```

Output, verbatim:

```
TOTAL=2337
CAT_D_DOCS=2319
CAT_E_CLAUDE=18
CAT_G_OTHER=0
```

EXIT_CODE: 0

The `pwsh -NoProfile -Command` wrapper exits `0` regardless of the exit code of any command inside it,
so the wrapper's exit code is not asserted. The printed values are the evidence.

## Arithmetic identities asserted against the printed numbers

Identity 1 — the per-category counts sum to the recorded total:

```
CAT_D_DOCS + CAT_E_CLAUDE = 2319 + 18 = 2337 = TOTAL
```

The identity holds. Because the three category tests are exhaustive and mutually exclusive over the
hit set, this identity is equivalent to the statement that every hit received exactly one category and
none was left unassigned.

Identity 2 — the genuine-caller category is empty:

```
CAT_G_OTHER = 0
```

The identity holds.

## The mechanical test by which each hit is assigned its category

The tests are applied in this order and are derived from the path alone, with no reading of hit text:

- Category D is any hit whose path begins docs/ ;
- category E is any hit whose path begins .claude/ ;
- category G, "genuine name-based caller of a removed member", is any hit matched by neither, and it
  must be empty.

No judgment enters the assignment. A third party re-running the printed command against the same
commit obtains the same three counts, because the `-like "docs/*"` and `-like ".claude/*"` predicates
operate on the `path:line:text` string that `git grep -n` prints, whose leading field is the
repository-relative path.

Category D holds two sub-populations that the specification distinguishes but this path test does not
separate: authored documentation prose, and machine-generated historical evidence such as coverage
reports and test-result files under an `evidence/` directory. The distinction does not affect the
classification, because both sub-populations are under the docs tree and neither can resolve a member
by name. Separating them would change no count and no conclusion.

## Corroboration with [P1-T1]

The printed `CAT_G_OTHER` value of `0` is the same population that [P1-T1] measured directly. [P1-T1]
runs a `git grep` whose pathspec adds `":(exclude)docs/*"` and `":(exclude).claude/*"` to the
`":(exclude)*.cs"` used here, so its search set is exactly the set of hits this task assigns to
category G. [P1-T1] measured that set as empty by a zero-hit `git grep` exiting `1`; this task measures
it as empty by a counting predicate over the full unfiltered hit list. The two tasks therefore
corroborate each other by independent routes: one excludes the prose trees before searching, the other
searches everything and partitions afterwards.

## Why no fixed value is asserted for TOTAL

The plan directs that no fixed value be asserted for `TOTAL`, and none is. The base-commit measurement
recorded in the plan was 2,229 hits, of which 2,216 were under the docs tree and 13 under the .claude
tree. The value measured here is 2,337, of which 2,319 are under the docs tree and 18 under the .claude
tree.

The reason the value moves is stated by the plan and is confirmed by the shape of the movement.
`git grep` searches tracked files only, so this item's plan file and its evidence artifacts are outside
this sweep's search set at the moment this task runs: they are untracked until [P4-T1], which runs in
Phase 4. The docs-tree count nevertheless rose by 103 because commits landed on this branch and on the
base branch between the specification's base commit
`b56400ab663a85b6039139d4548f408821e957ce` and the current HEAD
`d6cfb21c2185088847df5f6e209f79f05c6483ce`, adding tracked Markdown that quotes the identifiers.
The .claude-tree count rose by 5 because the agent-memory tree beneath the .claude directory is
tracked and is written by the agents executing this plan as their own bookkeeping.

Neither movement touches the acceptance condition. The acceptance condition is the pair of arithmetic
identities above, both of which are invariant under any number of additional prose hits: a new hit
under either prose tree increments both a category count and the total by one, leaving identity 1
satisfied, and cannot increment `CAT_G_OTHER`, leaving identity 2 satisfied.
