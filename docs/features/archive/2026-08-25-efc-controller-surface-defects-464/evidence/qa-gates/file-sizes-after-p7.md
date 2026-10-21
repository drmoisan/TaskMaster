# [P7-T15] Post-Phase-7 file sizes

Timestamp: 2026-08-28T01-26
Task: [P7-T15]
Command: `wc -l` over the six files this feature owns, after `dotnet tool run csharpier format` had
settled their formatting
EXIT_CODE: 0

## Delivered line counts

| File | Delivered | Gate | Verdict |
|---|---|---|---|
| `QuickFiler/Controllers/EfcFormController.cs` | **1189** | at most **1193** | PASS, 4 lines of headroom |
| `QuickFiler/Controllers/EfcItemController.cs` | **1117** | strictly fewer than **1170** | PASS, 53 lines of headroom |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | **485** | at most 500 | PASS |
| `QuickFiler.Test/Controllers/EfcItemControllerTests.cs` | **470** | at most 500 | PASS |
| `QuickFiler.Test/Controllers/EfcItemController.CleanupTests.cs` | **260** | at most 500 | PASS |
| `QuickFiler.Test/Controllers/EfcViewerTests.cs` | **104** | at most 500 | PASS (Phase 8 extends it) |

## Size gate for `EfcFormController.cs` — which figure is used, and why

The plan's constraint C2 states the gate as "at most 1204 — the 1084-line merge-base count plus at most
120 net lines". **The real merge-base count is 1073, not 1084**, verified on the actual execution base
`38f097898639b054428188c9c5e266e54972c259`. The base-drift addendum therefore directs the executor to
hold the **stricter derived gate of 1193** (1073 + 120), so that both readings of the criterion pass.

This artifact records the true merge-base figure alongside the delivered one:

| Measure | Value |
|---|---|
| True merge-base count at `BASELINE_SHA` | **1073** |
| Delivered count after Phase 7 | **1189** |
| Net delta | **+116** |
| Stricter derived gate held | **1193** (1073 + 120) |
| Plan's literal gate | 1204 |

+116 is within the 120-line allowance, so the file passes under both the stricter derived gate and the
plan's literal one.

### Net delta by remedy

| Remedy | Members added or changed | Approx. net lines |
|---|---|---|
| RC1 (#460 A/C, #464 A, #465 A) | guarded `Cleanup()`, guarded `ActiveTheme`, `LoadTheme`, `DarkMode` | +26 |
| RC3-B (#464 B) | `BoundaryErrorSink` plus five extracted `internal async Task` members, minus five `throw;` | +42 |
| RC3-C (#464 C) | `try`/`catch` around `PopulateFolderCombobox` | +8 |
| RC8 (#465 B) | `MatchesForSearchText`, hoisted control read | +14 |
| RC9 (#465 C) | `TrashRowText`, `WithTrashRow`, `ApplyDeleteGesture`, `BindSourceFolderRows`, minus the `ActionDeleteAsync` body and the `BindFolderRows` write-back | +19 |
| RC7 (#465 D) | `IsBannerRow`, `IsSelectableFolder`, recomposed `ActionOkAsync` guard | +17 |
| Comment compression (see below) | none — comments only | −10 |
| **Total** | | **+116** |

## Constraint C2 overflow rule — applied, and exactly what was done

Constraint C2 states: "If, at any file-size checkpoint, that file is projected above 480 lines, the
executor consolidates near-identical test methods into `[DataTestMethod]` rows and records the
consolidation in the checkpoint artifact. The executor must **not** create a fifth test file and must
not add a fourth `Compile Include` entry."

**The projection did exceed 480 during Phase 7.** After `[P7-T4]` the file stood at 492 lines with three
`[P7-T9]` tests still to add, projecting roughly 530 — over the 500 hard ceiling.

**What was done, stated precisely:** the overflow was resolved by **compressing prose** — shortening the
`because` strings and explanatory comments in the tests this feature added, which collapsed multi-line
FluentAssertions call chains back onto one or two lines. That removed 59 lines from the file
(492 → 433) before the remaining three tests were added. No test method was deleted, renamed, merged, or
weakened, and no assertion was removed.

**Why prose compression rather than the `[DataTestMethod]` consolidation C2 names.** Every candidate
group of near-identical methods in this file is individually required by name:

- `FormDarkMode_...`, `FormActiveTheme_...` and `FormLoadTheme_...` are each required by `[P5-T1]` and
  are each cited by a separate `spec.md` criterion already checked off (`spec.md:954`, `:955`, `:956`).
- `IsBannerRow_ClassifiesByTheFourCharacterPrefix`,
  `IsBannerRow_NullOrShortRow_ReturnsFalseWithoutThrowing` and
  `IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically` are all three required
  by `[P7-T9]` acceptance ("all three method names exist").

Merging any such group into one `[DataTestMethod]` would delete a method name that a completed task or a
checked criterion depends on. The one genuine consolidation opportunity in the file was already taken:
`AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow` is a single `[DataTestMethod]` carrying five
`[DataRow]` attributes rather than five separate methods.

The two hard prohibitions in the overflow rule were both honoured: **no fifth test file was created**,
and **no fourth `Compile Include` entry was added**. `QuickFiler.Test/QuickFiler.Test.csproj` carries
exactly the three `Efc*` entries this feature declared.

The deviation from the rule's named mechanism is recorded here rather than absorbed.

## Remaining headroom

`EfcFormControllerTests.cs` ends Phase 7 at **485**, above the 480 checkpoint threshold but 15 lines
below the hard 500 ceiling. **Phase 8 adds no test to this file** — its five `ClaimsAltChord` tests go to
`QuickFiler.Test/Controllers/EfcViewerTests.cs`, which stands at 104 — so the file does not grow again
in the remainder of this batch.

`EfcFormController.cs` ends at **1189**, 4 lines below its gate. Phase 8 edits only
`QuickFiler/Viewers/EfcViewer.cs`, so this file does not grow again in the remainder of this batch
either.

Output Summary: PASS. All six owned files are within their gates. `EfcFormController.cs` is 1189 against
the stricter derived gate of 1193 (true merge base 1073, net delta +116 within the 120-line allowance);
`EfcItemController.cs` is 1117, strictly below 1170; all four test files are under 500. The constraint C2
overflow rule was triggered and is recorded: the overflow was resolved by prose compression rather than
by the `[DataTestMethod]` consolidation the rule names, because every near-identical group in the file is
individually required by name; no fifth test file and no fourth `Compile Include` entry were created.
