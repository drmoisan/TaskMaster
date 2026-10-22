# P4-T8 — Line-count refresh for the two grown files (Phase 4, loop iteration 1)

Timestamp: 2026-08-28T04-17
Task: [P4-T8]
LoopIteration: 1
Command: (Get-Content -LiteralPath <path>).Count over the two edited files, and git diff --name-only REM_BASE..HEAD
EXIT_CODE: 0

**These figures supersede `FEATURE/evidence/qa-gates/p11-t11-final-line-counts.2026-08-28T02-31.md`
for these two files, and only for these two files.** Every other line count in that artifact remains
current, because this remediation edited no other source file.

REM_BASE for this task is `d77ac2126ec62a37e18a9e20ef220571dc2e4ec2`, as recorded in
`FEATURE/evidence/remediation-baseline/rem1-phase0-repo-state.2026-08-28T03-40.md`.

## Measured line counts

Measured with `(Get-Content -LiteralPath <path>).Count`, under
`$ErrorActionPreference = 'Stop'`; the measuring block reported `$?` = `True` and `$Error.Count` = `0`.

| File | Was (p11-t11) | Now | Delta | Ceiling headroom |
|---|---:|---:|---:|---:|
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 483 | **484** | +1 | 16 |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` | 81 | **105** | +24 | 395 |

`EventWiring.cs` measures **exactly 484** — the P0-T3 baseline of 483 plus the single detachment
statement P2-T1 added, and no other line. That exactness is itself a check: it confirms the production
edit is the one line it was supposed to be and nothing more.

`EventWiringTests.Part2.cs` measures 105 against the plan's estimate of "roughly 104". The 24 added
lines are one blank separator, a six-line XML doc comment, the `[TestMethod]` attribute, and the
sixteen-line test method itself.

Both files are **at most 500 lines**, comfortably so: 16 spare and 395 spare respectively.

## The parent test file is absent from the remediation diff

`git diff --name-only d77ac212..HEAD` returns 19 paths. An exact-match search of that list for
`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` returns **0** matches: the file is
**absent**.

Its line count is unchanged at **499** of the 500-line ceiling, one spare — which is precisely why the
new test was routed into the `Part2.cs` continuation file instead. The 484-owned test
`UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` inside it is neither renamed nor edited, and
it passed unmodified in both P2-T3 and P4-T6.

Only two source paths appear in the remediation diff:

```
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs
QuickFiler/Controllers/QfcItemController.EventWiring.cs
```

Every other entry is under `docs/features/active/itemviewer-surface-defects-489/` — the plan file,
`spec.md`, the amended handoff record, and this cycle's evidence artifacts. No `.csproj`, no `.props`,
no `.targets`, no `.config`.

## Acceptance

| P4-T8 condition | Result |
|---|---|
| Both files are at most 500 lines | **Yes** — 484 and 105 |
| `EventWiring.cs` measures exactly 484 | **Yes** — 484 |
| The parent test file is absent from the remediation diff | **Yes** — 0 exact matches in the 19-path diff |

Output Summary: Line counts refreshed for the two files this remediation grew.
`QuickFiler/Controllers/QfcItemController.EventWiring.cs` measures **exactly 484** (was 483, +1 for the
single added detachment) and
`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` measures **105** (was 81,
+24 for the one added test method with its doc comment); both are inside the 500-line ceiling with 16
and 395 lines spare. `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` is **absent**
from `git diff --name-only REM_BASE..HEAD` and remains at 499 lines, untouched. These two figures
supersede `p11-t11-final-line-counts.2026-08-28T02-31.md` for these two files only. `EXIT_CODE: 0`.
