# [P14-T9] Seam audit — the three AC-20 seams are behaviour-preserving

Timestamp: 2026-08-26T16-35

Command:

```
# suite counters, read from the committed evidence artifacts
grep -o 'total="[0-9]*" executed="[0-9]*" passed="[0-9]*" failed="[0-9]*"' <artifact>
# seam commit path lists
git show --name-only --format='%H %s' <sha>
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Three seams were introduced by this feature. Each was landed in its own commit, containing
`QuickFiler/Controllers/QfcCollectionController.cs` and nothing else, and each was bracketed by a full
`QuickFiler.Test` suite run immediately before and immediately at the seam. **All three show an
identical passed count before and after**, which is the measurement AC-20 requires.

| Seam | Seam-only commit | Passed before | Passed at the seam | Identical? |
|---|---|---|---|---|
| `ShrinkByRows` | `6cac5a822883d70da3e2cede927185435398f66d` | **958** | **958** | yes |
| `DrainBackgroundLoadingTasksAsync` | `97604063f029109096f405ac9ed82fc6062cb781` | **962** | **962** | yes |
| readiness predicate `TryGetMoveReadiness` with its `_notifyNotReady` delegate | `4938779a7a4092da1de24e7b62a0c05c5272831e` | **964** | **964** | yes |

---

## Seam 1 — `ShrinkByRows` (issue #471)

- **Seam-only commit:** `6cac5a822883d70da3e2cede927185435398f66d` —
  `refactor(471): extract the shared panel-height arithmetic behind ShrinkByRows`.
  Path list: `QuickFiler/Controllers/QfcCollectionController.cs` alone. Zero docs paths.
- **Before (P9-T4, end of Phase 9):**
  `evidence/qa-gates/p9-t4-suite.2026-08-26T11-03.md` —
  `total="958" executed="958" passed="958" failed="0"`.
- **At the seam (P10-T2):**
  `evidence/qa-gates/p10-t2-seam-suite.2026-08-26T11-13.md` —
  `total="958" executed="958" passed="958" failed="0"`.
- **Identical passed count: 958 = 958.**

Per decision D8, the seam deliberately lands **preserving the current inverted sign**:
`ShrinkByRows(Size current, float templateHeight, int removalCount)` returns
`new Size(current.Width, current.Height - (int)Math.Round(templateHeight * removalCount, 0))` and is
used at both the shrink site (positive count) and the grow site (negative count). That is why the
seam commit changes no observable behaviour. The sign correction is a separate later edit, made in
exactly one place — the argument passed at the `EliminateSpaceForItems` site — and it lands in
`f733506a`, not here.

## Seam 2 — `DrainBackgroundLoadingTasksAsync` (issue #473 defect 1)

- **Seam-only commit:** `97604063f029109096f405ac9ed82fc6062cb781` —
  `refactor(473): extract DrainBackgroundLoadingTasksAsync from the duplicated drain sites`.
  Path list: `QuickFiler/Controllers/QfcCollectionController.cs` alone. Zero docs paths.
- **Before (P10-T11, end of Phase 10):**
  `evidence/qa-gates/p10-t11-suite.2026-08-26T11-22.md` —
  `total="962" executed="962" passed="962" failed="0"`.
- **At the seam (P11-T2):**
  `evidence/qa-gates/p11-t2-seam-suite.2026-08-26T11-24.md` —
  `total="962" executed="962" passed="962" failed="0"`.
- **Identical passed count: 962 = 962.**

The seam extracts the drain logic from two duplicated call sites into one member, reproducing the
prior behaviour exactly. The atomic bag swap that closes the drain window is the separate fix commit
`505cab92`.

## Seam 3 — the readiness predicate and its notification delegate (issue #474 defect 2)

- **Seam-only commit:** `4938779a7a4092da1de24e7b62a0c05c5272831e` —
  `refactor(474): split the move-readiness evaluation from its notification`.
  Path list: `QuickFiler/Controllers/QfcCollectionController.cs` alone. Zero docs paths.
- **Before (P12-T4, end of Phase 12):**
  `evidence/qa-gates/p12-t4-suite.2026-08-26T11-37.md` —
  `total="964" executed="964" passed="964" failed="0"`.
- **At the seam (P13-T2):**
  `evidence/qa-gates/p13-t2-seam-suite.2026-08-26T11-40.md` —
  `total="964" executed="964" passed="964" failed="0"`.
- **Identical passed count: 964 = 964.**

`internal bool TryGetMoveReadiness(out string notifications)` carries exactly the prior readiness
evaluation logic, and the private delegate `_notifyNotReady` defaults to the exact prior modal call
with the same message, caption, buttons, and icon. The `ReadyForMove` property became a call of the
predicate followed by the delegate on the false path. `MessageBox.Show` appears exactly once in the
file and only inside the delegate's default. `TryGetMoveReadiness` was deliberately **not** added to
`IQfcCollectionController`, so the interface member set is unchanged.

---

## Why an identical passed count is the right measurement

A seam is behaviour-preserving if it changes no observable outcome. The suite is the largest
observation available: 958 to 964 assertions across the whole `QuickFiler` controller surface. An
identical passed count with a failed count of zero on both sides means no assertion changed its
verdict across the seam commit.

The count also rises monotonically between seams — 958, then 962, then 964 — because the intervening
fix commits each add tests. That rise is the reason the comparison must be made against the run
immediately preceding each seam rather than against a single fixed baseline: a comparison against a
stale figure would show a difference caused by test additions rather than by the seam.

## Acceptance verification

- The artifact exists.
- Three seams are named: `ShrinkByRows`, `DrainBackgroundLoadingTasksAsync`, and the readiness
  predicate with its `_notifyNotReady` delegate.
- Each records an identical before-and-after passed count: 958 = 958, 962 = 962, 964 = 964.
- Each names its seam-only commit SHA, and each of those three commits carries exactly one path.
