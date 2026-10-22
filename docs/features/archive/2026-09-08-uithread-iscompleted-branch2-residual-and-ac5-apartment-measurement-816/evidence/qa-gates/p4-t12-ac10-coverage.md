# P4-T12 — AC10 coverage projection

Timestamp: 2026-09-13T23-49

Source document: the Cobertura document written by P4-T11 into the gitignored coverage directory.
Baseline figures are cited from `evidence/baseline/p0-t16-coverage-baseline.md`.

## Per-file line coverage for `UtilitiesCS/Threading/UiThread.cs`

| Figure | Baseline (P0-T16) | Post-change (P4-T11) |
|---|---|---|
| Covered lines | 121 | **130** |
| Total lines | 126 | **133** |
| Line rate | 96.03% | **97.74%** |

**97.74% is at or above the 80 percent floor in CLAUDE.md**, so the figure is not below the floor
and it is not omitted. The rate also rose relative to the baseline.

### The derivation, identical to the rule stated in P0-T16

1. Take every `class` element whose `filename` attribute ends with the two path segments `Threading`
   and `UiThread.cs`. Three matched, the same three as at baseline: `UtilitiesCS.UiThread`,
   `UtilitiesCS.UiThread.SynchronizationContextAwaiter` and
   `UtilitiesCS.UiThread.SynchronizationContextAwaiter.<>c`. (Their `filename` attributes are
   absolute host paths and are not reproduced here; each ends with
   `<repo-root>\UtilitiesCS\Threading\UiThread.cs`.)
2. Union their `line` elements by the `number` attribute, so a member split across several class
   elements is counted once. The union has 133 distinct numbers.
3. Count distinct numbers whose `hits` attribute is greater than zero as covered: 130. The
   remainder, 3, are uncovered.
4. Line rate = 130 / 133 = 97.74% to two decimal places.

The total rose from 126 to 133 because the added conjunct occupies more executable lines than the
single-line condition it replaced.

## The three anchor lines of the hardened exit

Identified by content rather than by pre-change line numbers, because the added conjunct shifts
them.

| Anchor | Identification rule | Resolved line | Hits |
|---|---|---|---|
| 1 | Lowest-numbered line containing the substring `ReferenceEquals(_context, _uiSyncContext)` | 183 | **1** |
| 2 | Lowest-numbered line containing the substring `Dispatcher.FromThread(Thread.CurrentThread)` | 186 | **1** |
| 3 | The second line in the file whose trimmed text is exactly `return true;` | 191 | **1** |

Each of the three has a matching `line` element in the Cobertura document, so none is recorded as
non-executable. The two `return true;` lines in the file are at 162 and 191; 162 is the
ambient-identity exit and 191 is the body of the hardened exit.

**PASS condition:** the third anchor, the second `return true;` line, carries a line element with
hits **1**, which is greater than zero. P0-T16 recorded that same line — line 178 in the pre-change
numbering — **uncovered at baseline, with hits 0**. This is therefore a real and falsifiable
transition from uncovered to covered, not a restatement. The contingency in which P0-T16 had
recorded it already covered did not arise.

## No line changed by this delivery lost coverage

Derivation, as stated in the plan: the P2-T7 and P4-T15 diffs establish that the only removed lines
are the two named there — the one-line comment and the one-line condition — and that every other
line of the file is unchanged in content, so a covered baseline line maps to the same content in the
post-change file at a line number shifted by a constant after the condition.

Enumeration of any line whose content is unchanged, whose baseline hits were greater than zero, and
whose post-change hits are zero:

| Line | Content | Baseline hits | Post-change hits |
|---|---|---|---|
| *(none)* | | | |

**The enumeration is empty.**

The mechanical check behind that: the post-change uncovered set is exactly `{38, 39, 40}`, and the
baseline uncovered set was `{38, 39, 40, 177, 178}`. Every post-change uncovered line was already
uncovered at baseline, so no line moved from covered to uncovered. Lines 38-40 are the body of the
`if (onLockupDetected is not null)` guard inside `UiThread.Init`, which this delivery does not touch
and which no test in either assembly exercises; they were uncovered before and after.

The two lines that left the uncovered set are exactly the two lines of the hardened exit's body,
which is the intended effect of the two new negative tests and the positive twin.

## New-member coverage floor

**This delivery adds no new production member.** The change is confined to one condition inside an
existing property accessor; no new type, method, property, field or constructor is added to any
production assembly. The 90 percent floor for newly added members therefore has an **empty
denominator on the production side**.

The applicable production gate is consequently the 80 percent line floor together with coverage of
the hardened exit. Both are met: 97.74% against the 80 percent floor, and all three anchor lines of
the hardened exit covered.

## FAIL determination

The per-file figure is recorded and is not omitted; it is 97.74%, which is not below 80 percent; and
the third anchor is recorded covered with hits 1, not uncovered. No FAIL condition is met.
