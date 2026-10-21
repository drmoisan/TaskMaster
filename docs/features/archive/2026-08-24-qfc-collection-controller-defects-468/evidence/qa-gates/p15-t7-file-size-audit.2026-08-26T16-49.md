# [P15-T7] Post-format file-size audit

Timestamp: 2026-08-26T16-49

Command:

```
wc -l QuickFiler/Controllers/QfcCollectionController.cs \
      QuickFiler/Interfaces/IQfcCollectionController.cs \
      QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs \
      QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs \
      QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs \
      QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs \
      QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs \
      QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs \
      QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs

for c in <each feature commit>; do git show $c:QuickFiler/Controllers/QfcCollectionController.cs | wc -l; done
```

Run **after** the final CSharpier pass (P15-T1) and its repository-wide verification (P15-T2), because
formatting can change line counts. P15-T1 rewrote 0 files, so these figures are identical to the
pre-format ones; they are re-measured here regardless, because the audit's value depends on being
taken after the formatter and not before it.

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Every test file is at or under the 500-line cap. `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
is at 500, exactly at the cap and unchanged from its baseline. `QuickFiler/Controllers/QfcCollectionController.cs`
is **2,437 lines**.

**One acceptance sub-clause is NOT met and is reported as such rather than asserted.** The task's
acceptance asks for the statement that the controller's excess over the cap "is a pre-existing
condition this feature reduces rather than creates." The first half is true and the second half is
false: the excess is pre-existing, but this feature **increased** the file by 88 lines rather than
reducing it. The measurements are below.

## Post-format line counts

### Test files — the 500-line cap

| File | Lines | At or under 500? |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` (existing, changed) | **500** | yes — exactly at the cap |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` (existing, changed) | 155 | yes |
| `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs` (new) | 154 | yes |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` (new) | **494** | yes — 6 lines of headroom |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` (new) | **497** | yes — 3 lines of headroom |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs` (new) | 432 | yes |
| `QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs` (new) | 183 | yes |

All seven satisfy the cap. Three of them are within 6 lines of it, which is worth stating plainly:
`QfcCollectionControllerTests.cs` cannot take another line at all, and the two Defects468 files can
take 6 and 3 respectively. Any future change to these files must extract before it adds. This is the
condition that forced D12's five-file distribution in the first place, and it is now tighter than it
was at planning time.

### Production files

| File | Base (`61edc19b`) | Post-feature | Delta |
|---|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2,349 | **2,437** | **+88** |
| `QuickFiler/Interfaces/IQfcCollectionController.cs` | 118 | 131 | +13 |

`IQfcCollectionController.cs` at 131 lines is well under the cap. Its 13-line growth is the single XML
doc block added for `#469` defect 4.

## The controller's size, measured honestly

### Where the plan's estimate went wrong

The spec's `## Follow-up Candidates` entry 1 projected: "Step 1 removes approximately 241 lines …
net of the additions the other fixes introduce … the realistic post-feature figure is
**2,120-2,180 lines**." The actual figure is **2,437** — roughly 270 lines above the top of that
range.

The dead-code removal landed almost exactly as predicted. Everything after it exceeded the estimate.

### Line count after every commit in the feature

| Commit | Message | Lines | Delta |
|---|---|---|---|
| `61edc19b` | (merge base) | 2,349 | — |
| `63eebd47` | `fix(468)` dead-code removal | **2,108** | **−241** |
| `122dcd8d` | `fix(474-1)` `_parent` retype | 2,108 | 0 |
| `fbe5b3a6` | `fix(286)` reentrancy counter | 2,118 | +10 |
| `d512fcfe` | `fix(469-3)` ordered move collection | 2,140 | +22 |
| `8637aaa8` | `fix(473-2)` cancellation | 2,157 | +17 |
| `137ee307` | `fix(469-1/-2)` diagnostics | 2,167 | +10 |
| `62322433` | `fix(470-2)` insertion count | 2,274 | **+107** |
| `40381135` | `fix(470-1)` negative-index guards | 2,308 | +34 |
| `ffc10ff9` | `fix(470-3)` `SetVisualDigits` | 2,320 | +12 |
| `6cac5a82` | `refactor(471)` `ShrinkByRows` seam | 2,348 | +28 |
| `f733506a` | `fix(471)` sign correction | 2,348 | 0 |
| `97604063` | `refactor(473-1)` drain seam | 2,360 | +12 |
| `505cab92` | `fix(473-1)` atomic bag swap | 2,382 | +22 |
| `613e88c3` | `docs(469-4)` undo-stack contract | 2,400 | +18 |
| `4938779a` | `refactor(474-2)` readiness seam | **2,437** | +37 |

The removal took out 241 lines exactly as forecast. The fourteen commits after it added 329, of which
the single largest is the `#470` defect 2 reconciliation at +107 — the two new pure static helpers
`ResolveConversationInsertions` and `ReconcileInsertionCount` (D6), with their XML documentation and
the six-value diagnostic message.

The estimate's error is that it treated the fixes as small edits. They are not: seven of the thirteen
fixes introduce a guard clause, a helper, or a seam, and every new member carries the XML
documentation the C# policy requires for a non-obvious contract. The three seams alone account for
+77 lines (28 + 12 + 37).

### What this means for the cap

- The excess over the 500-line cap **is** pre-existing. The file was 4.70x the cap at the base commit
  and is 4.87x the cap now. This feature did not create the violation and could not have removed it —
  bringing the file under 500 requires decomposition into at least five types.
- This feature **did not reduce** the excess. It increased the file by 88 lines, or 3.7%. The
  contrary statement the task's acceptance asks for is not written here, because it is not true.
- No file split was performed, which is correct: AC-25 forbids it and the spec says explicitly "**Do
  not propose a file split in this feature**." A split alongside seven defect fixes would make the
  diff unreviewable and destroy per-defect regression attribution.
- The condition is claimed by **open issue #623** (`Feature: quickfiler-500-line-cap-violations`),
  whose body names this file at 2,349 lines and carries the acceptance criterion
  "`QuickFiler/Controllers/QfcCollectionController.cs` is at most 500 lines". That issue's baseline
  figure is now stale by 88 lines and should be updated when it is scheduled.

## Markdown exemption

`.claude/rules/general-code-change.md` exempts Markdown documentation files from the 500-line cap:
"Exceptions: temporary throwaway scripts created and deleted within an agent session; raw text
fixtures for language-processing test data; **Markdown documentation files**."

This exemption covers `docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md`,
`spec.md`, `issue.md`, every artifact under `evidence/`, and the seven new potential entries under
`docs/features/potential/`. None of those is production code, test code, or a reusable script, and
none is measured against the cap.

## Acceptance verification

| Clause | Status |
|---|---|
| the artifact exists | met |
| every test file is at most 500 lines | met — 500, 155, 154, 494, 497, 432, 183 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is at most 500 lines | met — exactly 500 |
| the artifact records the post-feature size of `<CTRL>` | met — **2,437 lines** |
| …together with the statement that its excess over the cap is a pre-existing condition this feature **reduces** rather than creates | **NOT MET.** The excess is pre-existing (2,349 at base, 4.70x the cap), but the feature increased the file by **+88 lines** to 2,437 (4.87x). The "reduces" half of the statement is false and is therefore not asserted. Full per-commit accounting is above. |
