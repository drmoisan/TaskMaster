Timestamp: 2026-08-31T12:23:19-04:00
Scope: P8-T32 spec-versus-tree reconciliation after the issue #638 merge
Verification basis: direct line-count and symbol searches of the merged working tree, plus the retained P1-T2 and P8-T16 census evidence.

This task does not modify any `spec.md` text. Its only earlier changes to `spec.md` were the P8-T1 through P8-T30 checkbox flips. The acceptance-criteria count remains 30; no criterion was added, removed, split, or text-edited.

## List A — citations already corrected before this execution

Each entry below is a re-verification, not a correction made by this plan.

| Entry | Spec figure | Merged-tree measurement | Agreement | Binding AC clause affected |
|---|---|---|---|---|
| A1 / AC25 (`spec.md:976-983`) | `EfcDataModel.cs` is 485 lines with 15 lines of headroom | 485 lines | Yes | No; the verified value explains the required change-B file split. |
| A2 / AC16 (`spec.md:926`) | `string` overload declaration at `EfcDataModel.cs:303` | declaration at 303 | Yes | No. |
| A3 / AC11 (`spec.md:900-906`) | `EfcDataModel.FilingStem.cs`; `DestinationOlStem` assignment at `EfcDataModel.cs:337` | assignment at 337 | Yes | No. |
| A4 / AC17 (`spec.md:933`) | `OpenOlFolderAsync` at 349-372 and `OpenFsFolderAsync` at 374-396 | 349-372 and 374-396 | Yes | No. |
| A5 / AC15 and AC16 (`spec.md:918-920`, `:926`) | `MAPIFolder` overload at 398-419; declaration at 398 | 398-419; declaration at 398 | Yes | No. |
| A6 / AC15 (`spec.md:920`) | `ToArchiveRelativeStem` call at 407 | call at 407 | Yes | No. |
| A7 / AC16 (`spec.md:928`) | delegation call at 408 | call at 408, spanning 408-414 | Yes | No. |
| A8 / AC15 (`spec.md:918`) | `ToArchiveRelativeStem` at 421-448; declaration at 434 | 421-448; declaration at 434 | Yes | No. |
| A9 / AC16 (`spec.md:925-932`) | 3 declarations and 7 call sites; family stem 23 lines across 6 files; syntax search 10 lines across 5 files | retained P1-T2 and P8-T16 evidence measures exactly those figures | Yes | No. |
| A10 / AC17 (`spec.md:933-937`) | guarded `Globals.Ol.ArchiveRootPath` read at 284 and `UserDiagnosticAction(ArchiveRootUnavailableMessage)` degradation at 358 and 382 are preserved | read at 284; degradations at 358 and 382 | Yes | No. The prior wording asserting that no archive-root read gained guarding/degradation was already false after #638, so the current satisfiable preservation clause remains required. |

## List B — stale citations deliberately left uncorrected

Each discrepancy is either satisfiable as written or outside the acceptance criteria. No binding acceptance-criterion clause is changed.

| Entry | Spec figure | Merged-tree measurement | Agreement | Binding AC clause affected |
|---|---|---|---|---|
| B1 / AC23 (`spec.md:967`; prose `:783`) | `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary` at 167-213 | method spans 167-214 (declaration at 168) | No | No; AC23 requires guard behavior and passing tests, not the endpoint citation. |
| B2 (`spec.md:376`) | #499 clear-on-rebind block at `BreadcrumbBridgeRouter.cs:143-146` | block spans 143-147; AC24's read at 143 and write at 145 are exact | No | No. |
| B3 / AC5 (`spec.md:874`; prose `:127`, `:360`, `:635`, `:1021`) | `folderpath != "Trash to Delete"` comparison at `EfcDataModel.cs:272` | comparison at 316 | No | No; AC5 is behavioral. |
| B4 (implementation/headroom/census prose at `spec.md:401`, `:414-416`, `:582`, `:710`) | 424 lines and 76 lines of headroom | 485 lines and 15 lines of headroom | No | No; the change-B split follows the measured value. |
| B5 (prose `spec.md:119-120`, `:122`, `:284`, `:445`, `:469`, `:514`) | pre-#638 string-overload declaration at 259-265 and assignment at 287 | declaration at 303-309; assignment at 337 | No | No. |
| B6 (`spec.md:313`) | `MoveToFolder` family: 16 lines across 5 files | 23 stem lines across 6 files | No | No; the sentence explicitly describes the pre-#638 research correction. |
| B7 (`spec.md:164-172`) | `Globals.Ol.ArchiveRootPath` benign-degrade item remains a pending #695 non-goal; verbatim `DestinationOlStem` assignments at 308 and 326 | EfcDataModel half shipped in #638; verbatim assignments remain at 364 and 388 | No | No; this is non-AC prose, while the assignment-value preservation still holds. |

AC21 remains recorded as a deliberate spec correction in `evidence/other/p8-t21-spec-correction-record.md`: the superseding issue #614 archive-relative-stem invariant had been applied to `SelectHierarchyPath` and the filing boundary but not to `SelectRow`. It is not a weakened test.
