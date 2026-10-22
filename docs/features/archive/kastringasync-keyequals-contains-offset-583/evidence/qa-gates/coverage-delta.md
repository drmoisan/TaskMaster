# Coverage Delta (P5-T7)

- Timestamp: 2026-09-13T02-20

Computed entirely from the two markdown evidence artifacts (coverage-baseline.md at P0-T8,
coverage-postchange.md at P5-T5); no raw coverage document was read (both raw outputs were
deleted by the tasks that produced them).

## (a) Instrumented-run line-rate / branch-rate delta

| | Baseline (P0-T8) | Post-change (P5-T5) | Delta |
|---|---|---|---|
| Root line-rate | 0.4298999577286177 | 0.4300204268507431 | +0.0001204691221254 |
| Root branch-rate | 0.24095967959333 | 0.24095967959333 | 0 |

## (b) KaStringAsync.cs covered/total, before and after

| | Baseline (P0-T8) | Post-change (P5-T5) |
|---|---|---|
| Covered | 60 | 65 |
| Total | 60 | 65 |

Both 100% (60/60 baseline, 65/65 post-change); the 5-line increase in the total is the new
regression test causing 5 additional lines (the multi-line rewritten Update(...) call body) to
be counted as executable, replacing the 1-line pre-fix call.

## (c) Changed-line coverage

Anchored zero-context diff of QuickFiler/Controllers/KaStringAsync.cs against origin/main:

```
@@ -81,2 +81,4 @@ namespace QuickFiler.Controllers
-        /// guard clause at the top of this method rejects both fail-fast, so branch 1's substring
-        /// offset expression is never evaluated with a negative start index.
+        /// guard clause at the top of this method rejects both fail-fast, so branch 1's derived
+        /// offset (Key.IndexOf(other) plus the matched length) is never evaluated with a
+        /// negative start index: IndexOf is non-negative because branch 1 only runs when
+        /// Contains already matched, and other.Length is at least 1 because of the guard above.
@@ -128 +130,6 @@ namespace QuickFiler.Controllers
-                    Update(Key.Substring(other.Length - 1, 1));
+                    Update(
+                        Key.Substring(
+                            Key.IndexOf(other, StringComparison.Ordinal) + other.Length - 1,
+                            1
+                        )
+                    );
```

Added (post-change) line numbers: 81, 82, 83, 84, 130, 131, 132, 133, 134, 135.

Cross-referenced against coverage-postchange.md's per-line hits projection (that file's
executable lines only):

| Added line | Row present in projection? | Hits |
|---|---|---|
| 81 | No (non-executable — doc comment) | excluded |
| 82 | No (non-executable — doc comment) | excluded |
| 83 | No (non-executable — doc comment) | excluded |
| 84 | No (non-executable — doc comment) | excluded |
| 130 | Yes | 1 |
| 131 | Yes | 1 |
| 132 | Yes | 1 |
| 133 | Yes | 1 |
| 134 | Yes | 1 |
| 135 | Yes | 1 |

- Denominator (added lines appearing as a row in the projection): 6 (130-135)
- Numerator (of those, hits >= 1): 6
- Changed-line coverage: 6 / 6 = 100%
- Excluded as non-executable (added lines with no row in the projection): 81, 82, 83, 84 — the
  reworded doc-comment lines from P3-T2, as expected.

## Per-line hits projection comparison (baseline vs. post-change)

Each baseline line number outside the removed hunk range is mapped to its post-change line
number by the net added-minus-removed shift of the hunks that precede it:

- Lines 1-80 (before both hunks): shift 0.
- Lines 83-127 (after hunk 1's removed range 81-82, before hunk 2): shift +2 (hunk 1 removed 2,
  added 4).
- Line 128 (inside hunk 2's removed range): no post-change counterpart — excluded from this
  comparison, covered by (c) via its replacement lines 130-135.
- Lines >= 129 (after both hunks): shift +7 (hunk 1 net +2, hunk 2 net +5: removed 1, added 6).

| Baseline line | Baseline hits | Mapped post-change line | Post-change hits |
|---|---|---|---|
| 12 | 1 | 12 | 1 |
| 14 | 1 | 14 | 1 |
| 15 | 1 | 15 | 1 |
| 16 | 1 | 16 | 1 |
| 17 | 1 | 17 | 1 |
| 18 | 1 | 18 | 1 |
| 19 | 1 | 19 | 1 |
| 20 | 1 | 20 | 1 |
| 21 | 1 | 21 | 1 |
| 22 | 1 | 22 | 1 |
| 23 | 1 | 23 | 1 |
| 24 | 1 | 24 | 1 |
| 25 | 1 | 25 | 1 |
| 26 | 1 | 26 | 1 |
| 27 | 1 | 27 | 1 |
| 32 | 1 | 32 | 1 |
| 33 | 1 | 33 | 1 |
| 39 | 1 | 39 | 1 |
| 40 | 1 | 40 | 1 |
| 46 | 1 | 46 | 1 |
| 47 | 1 | 47 | 1 |
| 50 | 1 | 50 | 1 |
| 53 | 1 | 53 | 1 |
| 54 | 1 | 54 | 1 |
| 107 | 1 | 109 | 1 |
| 110 | 1 | 112 | 1 |
| 111 | 1 | 113 | 1 |
| 112 | 1 | 114 | 1 |
| 115 | 1 | 117 | 1 |
| 116 | 1 | 118 | 1 |
| 117 | 1 | 119 | 1 |
| 118 | 1 | 120 | 1 |
| 119 | 1 | 121 | 1 |
| 120 | 1 | 122 | 1 |
| 121 | 1 | 123 | 1 |
| 122 | 1 | 124 | 1 |
| 125 | 1 | 127 | 1 |
| 126 | 1 | 128 | 1 |
| 127 | 1 | 129 | 1 |
| 128 | 1 | (removed hunk range — no counterpart; covered by (c)) | n/a |
| 129 | 1 | 136 | 1 |
| 131 | 1 | 138 | 1 |
| 132 | 1 | 139 | 1 |
| 133 | 1 | 140 | 1 |
| 134 | 1 | 141 | 1 |
| 135 | 1 | 142 | 1 |
| 136 | 1 | 143 | 1 |
| 137 | 1 | 144 | 1 |
| 138 | 1 | 145 | 1 |
| 139 | 1 | 146 | 1 |
| 140 | 1 | 147 | 1 |
| 141 | 1 | 148 | 1 |
| 142 | 1 | 149 | 1 |
| 143 | 1 | 150 | 1 |
| 144 | 1 | 151 | 1 |
| 145 | 1 | 152 | 1 |
| 150 | 1 | 157 | 1 |
| 151 | 1 | 158 | 1 |
| 157 | 1 | 164 | 1 |
| 158 | 1 | 165 | 1 |

Every mapped line retains hits >= 1; no baseline line with hits >= 1 maps to a post-change hits
of 0. No regression on changed lines.

## Companion git-status porcelain listing

```
 M docs/features/active/kastringasync-keyequals-contains-offset-583/plan.2026-09-12T10-25.md
?? docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/coverage-postchange.md
?? docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/kbdactions-postchange.md
```

No untracked file escapes the anchored diff's tracked-file scope: the only tracked change is
the plan file itself (this task's own check-off work in progress), and the two untracked paths
are this phase's own evidence artifacts, both within this feature's evidence subtree.

## Output Summary

Root line-rate rose by +0.0001204691221254 (0.4298999577286177 -> 0.4300204268507431); root
branch-rate unchanged (0.24095967959333). KaStringAsync.cs covered/total: 60/60 baseline, 65/65
post-change (both 100%). Changed-line coverage: 6/6 = 100% over a 6-line denominator (added
lines 130-135), with lines 81-84 (P3-T2 doc-comment reword) correctly excluded as
non-executable. Projection comparison: every one of the 59 comparable baseline lines (excluding
line 128, inside the removed hunk range and covered by (c)) maps to a post-change line with an
identical hits value of 1 — no regression on any previously-covered line. Companion
git-status porcelain listing confirms no untracked path outside this feature's evidence
subtree.
