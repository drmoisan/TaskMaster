# Zero-Result Search Audit (P3-T4) — discharges AC-11

- **Issue:** #635
- **Plan task:** [P3-T4]

Timestamp: 2026-08-29T06-38

## Output Summary

Every search in this item whose recorded result is zero is enumerated below, and each row carries all
four required fields: `SearchScope:`, `SearchPatterns:`, `SearchResult:`, and a measured scope size.
Thirty-seven zero results were produced by this item and thirty-seven rows are recorded. No zero result
in this item rests on an unstated or empty search set.

ZERO_RESULT_SEARCHES: 37

## Composition of the count

| Group | Producing task | Rows |
|---|---|---|
| Partition A sweep | [P1-T1] | 1 |
| Partition B category G count | [P1-T3] | 1 |
| Partition C category G count | [P1-T4] | 1 |
| Untracked outside-scope count | [P1-T5] | 1 |
| Production inventory rows | [P2-T1] | 16 |
| Zero test-tree inventory rows | [P2-T1] | 8 |
| Class L5 count | [P2-T2] | 1 |
| Surface-check rows | [P2-T4] | 8 |
| **Total** | | **37** |

`1 + 1 + 1 + 1 + 16 + 8 + 1 + 8 = 37`

Every enumerated row is present. No producing task recorded a blocker, so no reduced count applies.

The measured scope size for every row scoped to a QuickFiler tree is the corresponding value recorded
by [P2-T1]: `QF_PROD_SCOPE_FILES=228` for the production tree and `QF_TEST_SCOPE_FILES=151` for the
test tree.

---

## Row 1 — [P1-T1] Partition A sweep

SearchScope: tracked files matching `":(exclude)*.cs" ":(exclude)docs/*" ":(exclude).claude/*"` — every tracked file that is not a `.cs` file, is not under the docs tree, and is not under the .claude tree.
SearchPatterns: the thirteen identifiers, supplied as thirteen `-e` operands to `git grep -n -I -F`.
SearchResult: none. `git grep` exited `1`, and the artifact declares `ExpectedExitCode: 1`.
Measured scope size: 683 tracked files, from [P0-T5] `SCOPE_FILES=683`.

## Row 2 — [P1-T3] Partition B category G count

SearchScope: tracked files matching `":(exclude)*.cs"`, partitioned after the fact; category G is the residue of that hit set after removing every hit whose path begins docs/ or .claude/.
SearchPatterns: the thirteen identifiers, then the ordered path-derived category tests D, E, G.
SearchResult: none. `CAT_G_OTHER=0` over a total of 2,337 hits.
Measured scope size: 10,274 tracked non-`.cs` files, from [P0-T5] `TRACKED_NON_CS=10274`. The residue of that scope outside the two prose trees is the 683 files of row 1.

## Row 3 — [P1-T4] Partition C category G count

SearchScope: tracked files matching `"*.cs"`.
SearchPatterns: the thirteen identifiers, then the ordered category tests A, B, C, G applied to each of the 31 printed hits.
SearchResult: none. `CAT_G: 0`, with the other three categories summing to 31.
Measured scope size: 1,599 tracked `.cs` files, from [P0-T5] `TRACKED_CS=1599`.

## Row 4 — [P1-T5] untracked outside-scope count

SearchScope: the untracked, unignored files of this worktree, enumerated by `git ls-files --others --exclude-standard`, each read with `Select-String -LiteralPath`.
SearchPatterns: the thirteen identifiers, matched with `-SimpleMatch`.
SearchResult: none outside scope. `UNTRACKED_HIT_FILES_OUTSIDE_SCOPE=0`; five hit files were found and all five are this item's own evidence artifacts under its own feature folder.
Measured scope size: 9 untracked, unignored files, from [P1-T5] `UNTRACKED_FILES=9`, enumerated individually in that artifact.

---

## Rows 5 through 20 — [P2-T1] production inventory, sixteen name-resolving patterns

Each of the sixteen rows below shares the following scope and result fields, and differs only in its
pattern:

SearchScope: tracked files matching the pathspec `QuickFiler/*` — the QuickFiler production tree.
SearchResult: none. The row printed `prod=0`.
Measured scope size: 228 tracked files, from [P2-T1] `QF_PROD_SCOPE_FILES=228`.

The per-row pattern fields:

- Row 5 — SearchPatterns: the fixed string `GetMethod(`.
- Row 6 — SearchPatterns: the fixed string `GetMethods(`.
- Row 7 — SearchPatterns: the fixed string `GetMember(`.
- Row 8 — SearchPatterns: the fixed string `GetMembers(`.
- Row 9 — SearchPatterns: the fixed string `GetProperty(`.
- Row 10 — SearchPatterns: the fixed string `GetProperties(`.
- Row 11 — SearchPatterns: the fixed string `GetField(`.
- Row 12 — SearchPatterns: the fixed string `GetFields(`.
- Row 13 — SearchPatterns: the fixed string `GetEvent(`.
- Row 14 — SearchPatterns: the fixed string `InvokeMember(`.
- Row 15 — SearchPatterns: the fixed string `Type.GetType(`.
- Row 16 — SearchPatterns: the fixed string `Activator.CreateInstance`.
- Row 17 — SearchPatterns: the fixed string `Assembly.CreateInstance`.
- Row 18 — SearchPatterns: the fixed string `Assembly.Load`.
- Row 19 — SearchPatterns: the fixed string `Delegate.CreateDelegate`.
- Row 20 — SearchPatterns: the fixed string `CallByName`.

Restated per row so that each carries all four fields explicitly:

| Row | SearchScope: | SearchPatterns: | SearchResult: | Measured scope size |
|---|---|---|---|---|
| 5 | `QuickFiler/*` | `GetMethod(` | none, `prod=0` | 228 |
| 6 | `QuickFiler/*` | `GetMethods(` | none, `prod=0` | 228 |
| 7 | `QuickFiler/*` | `GetMember(` | none, `prod=0` | 228 |
| 8 | `QuickFiler/*` | `GetMembers(` | none, `prod=0` | 228 |
| 9 | `QuickFiler/*` | `GetProperty(` | none, `prod=0` | 228 |
| 10 | `QuickFiler/*` | `GetProperties(` | none, `prod=0` | 228 |
| 11 | `QuickFiler/*` | `GetField(` | none, `prod=0` | 228 |
| 12 | `QuickFiler/*` | `GetFields(` | none, `prod=0` | 228 |
| 13 | `QuickFiler/*` | `GetEvent(` | none, `prod=0` | 228 |
| 14 | `QuickFiler/*` | `InvokeMember(` | none, `prod=0` | 228 |
| 15 | `QuickFiler/*` | `Type.GetType(` | none, `prod=0` | 228 |
| 16 | `QuickFiler/*` | `Activator.CreateInstance` | none, `prod=0` | 228 |
| 17 | `QuickFiler/*` | `Assembly.CreateInstance` | none, `prod=0` | 228 |
| 18 | `QuickFiler/*` | `Assembly.Load` | none, `prod=0` | 228 |
| 19 | `QuickFiler/*` | `Delegate.CreateDelegate` | none, `prod=0` | 228 |
| 20 | `QuickFiler/*` | `CallByName` | none, `prod=0` | 228 |

---

## Rows 21 through 28 — [P2-T1] test-tree inventory rows that printed zero

Each of the eight rows below shares the following scope and result fields:

SearchScope: tracked files matching the pathspec `QuickFiler.Test/*` — the QuickFiler test tree.
SearchResult: none. The row printed `test=0`.
Measured scope size: 151 tracked files, from [P2-T1] `QF_TEST_SCOPE_FILES=151`.

Restated per row so that each carries all four fields explicitly:

| Row | SearchScope: | SearchPatterns: | SearchResult: | Measured scope size |
|---|---|---|---|---|
| 21 | `QuickFiler.Test/*` | `GetMembers(` | none, `test=0` | 151 |
| 22 | `QuickFiler.Test/*` | `GetProperties(` | none, `test=0` | 151 |
| 23 | `QuickFiler.Test/*` | `InvokeMember(` | none, `test=0` | 151 |
| 24 | `QuickFiler.Test/*` | `Type.GetType(` | none, `test=0` | 151 |
| 25 | `QuickFiler.Test/*` | `Assembly.CreateInstance` | none, `test=0` | 151 |
| 26 | `QuickFiler.Test/*` | `Assembly.Load` | none, `test=0` | 151 |
| 27 | `QuickFiler.Test/*` | `Delegate.CreateDelegate` | none, `test=0` | 151 |
| 28 | `QuickFiler.Test/*` | `CallByName` | none, `test=0` | 151 |

These are the eight test-tree rows of the [P2-T1] inventory that print zero: `GetMembers(`,
`GetProperties(`, `InvokeMember(`, `Type.GetType(`, `Assembly.CreateInstance`, `Assembly.Load`,
`Delegate.CreateDelegate` and `CallByName`. The remaining nine test-tree rows print non-zero values and
are not zero results, so they are not audited here.

---

## Row 29 — [P2-T2] class L5 count

SearchScope: the 39 `System.Reflection` occurrences enumerated by [P2-T2] over the tracked files matching the pathspec `QuickFiler/*`.
SearchPatterns: the fixed string `System.Reflection` under `git grep -F`, then the five ordered class tests L1 through L5, with L5 defined as a call site taking a member-name argument.
SearchResult: none. `L5: 0`, with L1 26, L2 3, L3 3 and L4 7 summing to the 39 occurrences printed by [P2-T1] for that pattern.
Measured scope size: 228 tracked files, from [P2-T1] `QF_PROD_SCOPE_FILES=228`, over which 39 occurrences were found and classified.

---

## Rows 30 through 37 — [P2-T4] surface-check rows

Each of the eight rows below shares the following scope and result fields:

SearchScope: tracked files matching the pathspec `QuickFiler/*` — the QuickFiler production tree.
SearchResult: none. The row printed `prod=0`.
Measured scope size: 228 tracked files, from [P2-T1] `QF_PROD_SCOPE_FILES=228`.

Restated per row so that each carries all four fields explicitly:

| Row | SearchScope: | SearchPatterns: | SearchResult: | Measured scope size |
|---|---|---|---|---|
| 30 | `QuickFiler/*` | `DataBindings.Add` | none, `prod=0` | 228 |
| 31 | `QuickFiler/*` | `DisplayMember` | none, `prod=0` | 228 |
| 32 | `QuickFiler/*` | `ValueMember` | none, `prod=0` | 228 |
| 33 | `QuickFiler/*` | `DataPropertyName` | none, `prod=0` | 228 |
| 34 | `QuickFiler/*` | `[Serializable` | none, `prod=0` | 228 |
| 35 | `QuickFiler/*` | `DataContract` | none, `prod=0` | 228 |
| 36 | `QuickFiler/*` | `JsonProperty` | none, `prod=0` | 228 |
| 37 | `QuickFiler/*` | `XmlElement` | none, `prod=0` | 228 |

---

## Completeness statement

Every enumerated row above carries `SearchScope:`, `SearchPatterns:`, `SearchResult:`, and a measured
scope size. Every measured scope size is greater than zero: the smallest is 9, the untracked file set
of row 4, and the largest is 10,274, the tracked non-`.cs` corpus of row 2. No zero result in this item
therefore rests on an empty or unstated search set.

Only searches whose recorded result is zero are in scope for this audit. Searches in this item that
returned a non-zero result are outside it and are not enumerated above: the [P1-T2] control with 13
hits, the [P2-T4] COM-visibility search with 1 hit, the nine non-zero test-tree rows of the [P2-T1]
inventory, the `System.Reflection` production row with 39 occurrences, and the counting commands of
[P0-T5], [P1-T3], [P1-T4] and [P1-T5], whose printed totals are all greater than zero.
