# [P1-T4] AC-3 live-member non-regression

Timestamp: 2026-08-26T08-45

Command: `grep -nF '<literal>' QuickFiler/Controllers/QfcCollectionController.cs` — once per literal.
Command: `git diff --stat 61edc19b -- QuickFiler/Controllers/QfcCollectionController.cs`
Command: `git diff -U0 61edc19b -- QuickFiler/Controllers/QfcCollectionController.cs`

`<MERGE_BASE>` = `61edc19befcf6c4e95b5acd32542f2dcdab41b78`, as recorded by P0-T10. The form
`git diff <MERGE_BASE> -- <path>` is used with **no** `..HEAD`, so the still-uncommitted P1-T2 edit
is in scope.

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

**All five live literals are still present (two hits each). The diff against `<MERGE_BASE>` changes
241 lines, which is non-zero. The changed-line set intersected with each of the five live member
bodies is empty.**

---

## Part 1 — the five live literals are still present

| # | Literal | Hits | Line numbers (post-edit) | Roles |
|---|---|---|---|---|
| 1 | `AnyOpenDropDowns(` | **2** | 1081, 1091 | call site in `CustomReturnKeyHandler`; declaration |
| 2 | `LoadItemGroupsAndViewers_02` | **2** | 286, 613 | call site; declaration |
| 3 | `LoadConversationsAndFolders_04` | **2** | 294, 629 | call site; declaration |
| 4 | `LoadSequential_5` | **2** | 631, 634 | call site in `LoadConversationsAndFolders_04`; declaration |
| 5 | `ActivateQueuedTlp` | **2** | 258, 663 | call site; declaration |

Verbatim:

```
1081:            var anyOpen = AnyOpenDropDowns(true, Token);
1091:        internal bool AnyOpenDropDowns(bool close, CancellationToken token)
 286:            LoadItemGroupsAndViewers_02(listMailItems, template);
 613:        public void LoadItemGroupsAndViewers_02(IList<MailItem> items, RowStyle template)
 294:            LoadConversationsAndFolders_04();
 629:        public void LoadConversationsAndFolders_04()
 631:            LoadSequential_5();
 634:        public void LoadSequential_5()
 258:            ActivateQueuedTlp(tlp);
 663:        internal void ActivateQueuedTlp(TableLayoutPanel tlp)
```

Each returns **at least one** hit, as required. Each in fact returns two: a declaration and a live
caller, which additionally confirms none of the five became orphaned by the removal.

D3 is satisfied specifically: `AnyOpenDropDowns(bool, CancellationToken)` — the non-async overload —
survives at `:1091` and is still called at `:1081`, while `AnyOpenDropDownsAsync` is gone (P1-T3
row 2). Only the `Async` overload was dead.

---

## Part 2 — the diff is non-empty

```
$ git diff --stat 61edc19b -- QuickFiler/Controllers/QfcCollectionController.cs
 QuickFiler/Controllers/QfcCollectionController.cs | 241 ----------------------
 1 file changed, 241 deletions(-)
```

**Total changed-line count: 241. This is non-zero.** An empty diff cannot satisfy this gate, and the
diff is not empty.

Composition: **241 deletions, 0 insertions**. Verified independently — `git diff -U0 ... | grep -c
"^+[^+]"` returns `0`. The edit adds no line anywhere in the file.

---

## Part 3 — empty changed-line set for each of the five live member bodies

Because the diff contains **zero** insertions, the changed-line set is exactly the set of deleted
old-line numbers. `git diff -U0` reports it exactly, in hunk headers:

```
@@ -70      +69,0   @@   ->  old line 70
@@ -402     +400,0  @@   ->  old line 402
@@ -587,20  +584,0  @@   ->  old lines 587-606
@@ -635,105 +612,0  @@   ->  old lines 635-739
@@ -761,37  +633,0  @@   ->  old lines 761-797
@@ -827,32  +662,0  @@   ->  old lines 827-858
@@ -865,11  +668,0  @@   ->  old lines 865-875
@@ -1254,21 +1046,0 @@   ->  old lines 1254-1274
@@ -1324,6  +1095,0 @@   ->  old lines 1324-1329
@@ -1991,7  +1756,0 @@   ->  old lines 1991-1997
```

Sum of hunk lengths: 1 + 1 + 20 + 105 + 37 + 32 + 11 + 21 + 6 + 7 = **241**, matching `--stat`.

Every `+N,0` new-side length confirms each hunk is a pure deletion.

### The five live member spans at `<MERGE_BASE>`

Read from `git show 61edc19b:QuickFiler/Controllers/QfcCollectionController.cs`:

| Live member | Span at `<MERGE_BASE>` | Boundary evidence |
|---|---|---|
| `LoadItemGroupsAndViewers_02` | `:740-754` | `740: public void LoadItemGroupsAndViewers_02(...)`; `755:` blank |
| `LoadConversationsAndFolders_04` | `:756-759` | `756: public void LoadConversationsAndFolders_04()`; `759: }` |
| `LoadSequential_5` | `:798-825` | `798: public void LoadSequential_5()`; `825: }`; `826:` blank |
| `ActivateQueuedTlp` | `:859-863` | `859: internal void ActivateQueuedTlp(...)`; `863: }`; `864:` blank |
| `AnyOpenDropDowns` (non-async) | `:1319-1322` (plus its `#351` comment at `:1316-1318`) | `1319: internal bool AnyOpenDropDowns(...)`; `1322: }`; `1323:` blank |

### Intersection, member by member

| Live member | Span | Nearest deleted range below | Nearest deleted range above | Intersection |
|---|---|---|---|---|
| `LoadItemGroupsAndViewers_02` | 740-754 | 635-**739** | **761**-797 | **empty** |
| `LoadConversationsAndFolders_04` | 756-759 | 635-**739** | **761**-797 | **empty** |
| `LoadSequential_5` | 798-825 | 761-**797** | **827**-858 | **empty** |
| `ActivateQueuedTlp` | 859-863 | 827-**858** | **865**-875 | **empty** |
| `AnyOpenDropDowns` (+comment) | 1316-1322 | 1254-**1274** | **1324**-1329 | **empty** |

Every live member sits strictly in a gap between two deleted ranges. Each gap closes to within one
line on at least one side — `739 | 740`, `797 | 798`, `858 | 859`, `1322 | 1323 | 1324` — which is the
expected shape: the dead members were interleaved with the live ones, and the deletion boundaries
land exactly on the blank separator lines between them. **No deleted old-line number falls inside any
of the five spans, and no line was inserted into any of them.**

Conclusion: the five live members are byte-for-byte identical to their `<MERGE_BASE>` text. Their
line numbers shifted (renumbering), but their content did not change.

---

## Acceptance verification

- All five searches return at least one hit each (two each, in fact).
- The artifact records the total changed-line count of the diff — **241** — and states it is
  **non-zero**.
- The artifact shows an **empty** changed-line set for each of the five member bodies, derived from
  `git diff -U0` hunk headers plus the measured `<MERGE_BASE>` spans.

Result: PASS. AC-3 is satisfied.
