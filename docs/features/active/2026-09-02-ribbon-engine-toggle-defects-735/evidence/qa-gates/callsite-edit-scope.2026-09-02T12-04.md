# Finding 2 — Call-Site Edit Scope (P2-T11)

Timestamp: 2026-09-03T02-17
Task: [P2-T11]
Command: `git diff --unified=0 (git merge-base origin/main HEAD) -- TaskMaster/Ribbon/RibbonController.Intelligence.cs`
EXIT_CODE: 0
Merge base re-derived at run time: `a679cd082819af6788cd0fb35f4366786fab87e3`

## Every hunk lies inside the Spam Manager region

Region boundaries in the post-change file (444 lines):

| Region | Opens | Closes |
|---|---|---|
| SettingsMenu | 27 | 84 |
| Folder Classifier | 86 | 139 |
| BayesianPerformance | 141 | 186 |
| **Spam Manager** | **188** | **282** |
| Triage | 284 | 423 |

Hunk headers produced by the diff, with their post-change line spans:

```
@@ -205,0 +206,25 @@
@@ -217   +242   @@
@@ -219,6 +244,5 @@
@@ -226,5 +250,13 @@
@@ -232   +264   @@
```

Every post-change span falls in 206..264, which is strictly inside the Spam Manager region's
188..282. No hunk touches any other region.

## The preamble and the confirmation dialog are unchanged and in their original order

Neither appears as a changed line in any hunk. In the post-change file they read:

```
 233:             if (SynchronizationContext.Current is null)
 234:                 SynchronizationContext.SetSynchronizationContext(
 235:                     new WindowsFormsSynchronizationContext()
 236:                 );
 237:             var response = MessageBox.Show(
 238:                 "Are you sure you want to clear the Spam Manager? This cannot be undone",
 239:                 "Clear Spam Manager",
 240:                 MessageBoxButtons.YesNo
 241:             );
```

The synchronization-context preamble still comes first and the confirmation dialog still comes
second, exactly as before. Reordering the not-ready notice ahead of the confirmation prompt was
considered and rejected by the spec: it would change user-visible behavior on the already-working
path for no defect-driven reason.

## The method body no longer dereferences the globals chain

`ClearSpamManagerAsync` now spans lines 231..265. Token counts taken over that span:

| Token | Occurrences in the method body |
|---|---|
| `Globals.AF` | **0** |
| `Globals.Engines` | **0** |
| `Globals?.AF` | 0 |
| `Globals?.Engines` | 0 |
| `Globals` (any use) | 0 |
| `?.` (null-conditional) | **0** |

All three pre-existing unguarded dereferences are gone: the one inside the condition that opened the
guarded block, and the two inside the block. The engine-touching statements moved verbatim into the
deferred lambda and now use the `manager` and `engines` parameters the gate resolved.

## No inline ad-hoc null guard was introduced

The null-conditional count inside the method is zero. The single `is null` occurrence in the method
is line 233, `if (SynchronizationContext.Current is null)`, which is the PRE-EXISTING
synchronization-context preamble — it is unchanged, appears in no hunk, and is not a guard on an
optional dependency. No `is null` guard and no null-conditional operator was ADDED.

This matters because an inline guard was explicitly disrecommended by the maintainer on the
predecessor issue: it would sit permanently inside the containing type's coverage-exempt region and
so could never be tested. Routing through the gate instead puts the decision in a host-neutral,
fully covered class.

## Out-of-scope members are untouched

| Member group | Location in the post-change file | Touched by any hunk |
|---|---|---|
| The eight QuickFiler-settings members (`IsMoveEntireConversationActive` 29, `ToggleMoveEntireConversation` 31, `IsSaveAttachmentsActive` 36, `ToggleSaveAttachments` 38, `IsSavePicturesActive` 43, `ToggleSavePictures` 45, `IsSaveEmailCopyActive` 48, `ToggleSaveEmailCopy` 50) | SettingsMenu region, lines 29-50 | No — every hunk is at 206 or later |
| The three not-implemented bound handlers (`TestSpamVerbose` 267, `SpamMetrics` 272, `SpamInvestigateErrors` 277) | Spam Manager region, lines 267-277 | No — the last hunk ends at 264 |

Both groups are recorded by the spec as separate follow-ups, not as part of this change.

Output Summary: All five diff hunks fall within post-change lines 206-264, inside the Spam Manager
region 188-282. The synchronization-context preamble and the confirmation dialog are unchanged and
in their original order. The method body contains zero `Globals.AF`, zero `Globals.Engines` and zero
null-conditional operators, and no `is null` guard was added. The eight QuickFiler-settings members
and the three not-implemented members are untouched by any hunk.
