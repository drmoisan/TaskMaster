# P1-T8 — AC3: the normaliser is idempotent over the already-normalised tree

Timestamp: 2026-09-19T13-02

Command: a second `Invoke-ManifestNormalization` run over the tree P1-T7 left behind, using the
same discovery, reader and writer delegates; followed by
`git diff 734112ed25bba293cb074e71fee2286bc3b72fae -- "*/packages.config" "*/app.config"`,
`git diff --numstat <same>` and
`git status --porcelain --untracked-files=all -- "*/packages.config" "*/app.config"`

EXIT_CODE: 0

## Second-run counts

| Measure | Value |
|---|---|
| Examined `packages.config` | **18** |
| Examined `app.config` | **17** |
| Changed `packages.config` | **0** |
| Changed `app.config` | **0** |
| Files whose SHA-256 changed across the second run | **0** |
| Reflowed-element residual after the second run | **0** |

The two per-kind examined counts are the non-vacuity guard this task depends on. A discovery glob
that matched nothing, or that matched only one of the two kinds, would leave the tree untouched and
produce a diff identical to P1-T7's — indistinguishable from a genuinely idempotent renderer by the
diff alone. Both counts are positive and both equal the totals in the tree, so the second run
really did read all 35 files and really did decide that none needed rewriting.

## The diff is unchanged by the second run

`git diff <MERGE_BASE> -- "*/packages.config" "*/app.config"` was captured immediately before and
immediately after the second run and hashed:

```
DIFF-SHA-BEFORE-SECOND-RUN: 19B654509E9EE2E0135725AEF92236D6286FCECCEA3811C14B3DCC3F09B80BCA
DIFF-SHA-AFTER-SECOND-RUN:  19B654509E9EE2E0135725AEF92236D6286FCECCEA3811C14B3DCC3F09B80BCA
DIFF-IDENTICAL: True
```

The two are equal, so **no hunk in the captured diff differs in content from the P1-T7
normalisation output**. The numstat totals are likewise unchanged at 34 files, 1201 added and 6077
deleted.

## Porcelain capture

34 entries, **0 of them outside** the 35-member set P1-T7 recorded. `SVGControl/packages.config`
remains the one set member absent from both the diff and the porcelain, being already canonical
before P1-T7 ran.

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| The second run reports exactly 18 examined `packages.config` files, emitted by the normaliser as an integer | 18 | PASS |
| The second run reports exactly 17 examined `app.config` files, emitted by the normaliser as an integer | 17 | PASS |
| The captured `git diff` lists no hunk whose content differs from the P1-T7 normalisation output | the whole diff text hashes identically before and after the second run | PASS |
| The captured porcelain output lists only paths drawn from the 35-member set P1-T7 recorded | 34 entries, 0 outside | PASS |

**AC3 is checked off in `spec.md`.** The criterion is satisfied in both halves: all 18 manifests
are normalised — 17 rewritten by P1-T7 and one already canonical, all 18 verified reflow-free by
the zero residual — and re-running the normaliser over the result changes nothing.

Output Summary: the second normalisation run examined 18 `packages.config` and 17 `app.config`
files and changed none of them; no file's SHA-256 moved; the reflowed-element residual stayed at 0;
and the merge-base diff hashed identically before and after the run at
`19B654509E9EE2E0135725AEF92236D6286FCECCEA3811C14B3DCC3F09B80BCA`, with the numstat totals
unchanged at 34 files, 1201 added and 6077 deleted. The porcelain companion lists 34 paths, none
outside the 35-member set.
