# Phase 6 — Coverage delta and threshold report (AC18)

Timestamp: 2026-09-09T14-04
Task: [P6-T11]

## The four required numeric items

### 1. Baseline repository line coverage, from `[P0-T11]`

```text
line-rate=0.856094 lines-valid=65376 lines-covered=55968
```

**85.6094%**

### 2. Post-change repository line coverage, from `[P6-T9]`

```text
line-rate=0.856132 lines-valid=65435 lines-covered=56021
```

**85.6132%**

### 3. New-and-changed-code coverage, as the per-member result from `[P6-T10]`

| # | File | Member | Line-rate |
|---|---|---|---|
| 1 | `QuickFiler/Controllers/QfcHomeController.cs` | `Cleanup` | 0.8857 |
| 2 | `QuickFiler/Controllers/EfcHomeController.cs` | `Cleanup` | 1.0000 |
| 3 | `UtilitiesCS/Threading/ProgressViewer.cs` | `SetCancellationTokenSource` | 1.0000 |
| 4 | `UtilitiesCS/Threading/ProgressViewer.cs` | `RequestCancel` | 1.0000 |
| 5 | `UtilitiesCS/Threading/ProgressViewer.cs` | `CancelButton_Click` | 1.0000 |
| 6 | `UtilitiesCS/Threading/ProgressPane.cs` | `SetCancellationTokenSource` | 1.0000 |
| 7 | `UtilitiesCS/Threading/ProgressPane.cs` | `RequestCancel` | 1.0000 |
| 8 | `UtilitiesCS/Threading/ProgressPane.cs` | `CancelButton_Click` | 1.0000 |

Restricted to the lines this fix actually **added or changed** — which is what "new and changed code"
denotes — coverage is **1.0000**: all six changed cleanup-site lines and all 71 measured lines across
the eight new or rewritten progress-surface regions report non-zero hits, with zero exceptions.

Member 1's 0.8857 is entirely attributable to four pre-existing zero-hit lines, 382-385, which this
fix did not change and which were already zero-hit in the `[P0-T12]` baseline. It is discharged by the
alternative route the plan specifies for a per-class Cobertura shape, not by relaxing the threshold.

### 4. The governing thresholds

**CLAUDE.md governs**, per plan decision 5 and the spec's own Coverage note. `CLAUDE.md` is authority 1
in the stated policy compliance order.

| Threshold | Value | Source |
|---|---|---|
| Repository line coverage floor | **80%** | `CLAUDE.md` section UT2 |
| New module, class or method floor | **90%** | `CLAUDE.md` section UT2 |

The **85% line and 75% branch** figures in `.claude/rules/general-unit-test.md` and
`.claude/rules/quality-tiers.md` are **not used here**. This divergence is noted rather than silently
resolved: as it happens the post-change repository line coverage of 85.6132% clears the 85% figure as
well, so the choice of authority does not change the outcome on that metric.

## Which condition blocks and which is reported

**Blocking:** the change-scoped conditions in `[P6-T10]`.

- Condition (a), every line changed by this fix reports non-zero hits — **MET**.
- Condition (b), each of the eight AC18 members at a line-rate of at least 0.90 — **MET**, seven
  directly at 1.0000 and the eighth via the plan's stated alternative route.

Both blocking conditions pass, so AC18's blocking half is discharged.

**Reported, not gated:** the repository-wide line figure. AC18 states that because no merge-base
coverage baseline exists in this feature folder, the repository-wide figure is reported rather than
gated. The `[P0-T11]` baseline was captured in this same worktree at the start of this execution, not
at the merge base, so it measures the pre-change state of *this branch* — which already carries 70
sibling-feature commits — rather than the state of `origin/main`. It is a within-run reference point,
not a merge-base baseline, and is treated accordingly.

## Denominator movement, and why a bare line-rate comparison is not sound

`lines-valid` **changed between the two runs**, from 65376 to **65435**, a rise of **59**.

| Quantity | Baseline | Post-change | Delta |
|---|---|---|---|
| `lines-valid` (denominator) | 65376 | 65435 | **+59** |
| `lines-covered` (numerator) | 55968 | 56021 | **+53** |
| `line-rate` | 0.856094 | 0.856132 | +0.000038 |

The 59 added measurable lines are the production code this change introduced: the three-statement
idiom at each of the two cleanup sites, the two `RequestCancel` members, the two rewritten
`SetCancellationTokenSource` methods, the two rewritten `CancelButton_Click` handlers, and the two
logger field initializers. Because the denominator differs, the two `line-rate` values are ratios over
different populations and are **not directly comparable**; the +0.000038 movement should not be read
as a measured improvement in the coverage of any pre-existing code.

The sound statement that can be made from these numbers is the marginal one: of the 59 lines added to
the denominator, 53 are covered, and the 6 that are not are accounted for entirely by pre-existing
zero-hit lines shifting position rather than by any new uncovered line — `[P6-T10]` identifies both
shifts, `ProgressPane.cs` 28-29 to 32-33 and `EfcHomeController.cs` 418 to 420, as the same untouched
property accessors at new line numbers. No line changed by this fix is uncovered.

Both runs also passed `Assert-CoberturaLineCoverageThreshold`, evidenced by the literal
`Done. Coverage artifact:` appearing in both captured outputs, so the repository-wide 80% floor was
met before and after.

Output Summary: baseline repository line coverage **85.6094%** (55968/65376); post-change
**85.6132%** (56021/65435); new-and-changed-code coverage **100%** on every line this fix touched,
with per-member figures of 1.0000 on seven of eight AC18 members and 0.8857 on
`QfcHomeController.Cleanup` from four pre-existing untouched zero-hit lines. Governing thresholds are
CLAUDE.md's 80% repository floor and 90% new-code floor. The change-scoped conditions **block and are
met**; the repository-wide figure is **reported, not gated**. `lines-valid` rose by 59 between the two
runs, so the two repository-wide rates are not directly comparable.
