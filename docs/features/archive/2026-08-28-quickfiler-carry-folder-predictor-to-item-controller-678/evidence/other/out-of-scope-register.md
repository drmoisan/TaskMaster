# P1-T12 — AC22 out-of-scope register

Timestamp: 2026-09-01T23-38

Each of the six items the plan's Scope boundary section places out of scope carries a verdict of
`CONFIRMED-DEFECT` or `NOT-CONFIRMED`, the file and line the verdict rests on, and, where confirmed,
the promotion route it is handed to.

## Promotion route for this run

Every `CONFIRMED-DEFECT` item below is referred by exactly this route:

```
Deferred to a single consolidated follow-up issue filed by the parallel orchestrator from a separate
branch after this PR merges.
```

That route is the named owner: the parallel orchestrator files the issue, from a branch that is not
this one, after this pull request merges. **No promotion MCP tool was run, no potential entry was
created, and no GitHub issue was opened from this branch.** Opening one here would put an
out-of-scope artifact into this change's footprint and would violate AC23.

---

## 1. The synchronous `QfcItemController.LoadFolderHandler` predictor-initialisation defect

**Verdict: CONFIRMED-DEFECT.**

Evidence: `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:27-55`. Both branches of the
synchronous `LoadFolderHandler` assign `_folderHandler = _folderPredictorFactory(...)` and **never
call `InitAsync`**:

- `:31-35`, the `varList is null` branch, constructs with `FolderPredictor.InitOptions.FromField`.
- `:44-48`, the `else` branch, constructs with `FolderPredictor.InitOptions.FromArrayOrString`.

The asynchronous `LoadFolderHandlerAsync` at `:57` does call `fp.InitAsync(...)` in both of its
branches (`:90-93` and `:134-137`). The synchronous method therefore leaves the handler in whatever
state the constructor produced, which is not the state the async path guarantees. `PopulateFolderComboBox`
at `:154` calls the synchronous method and then `AssignFolderComboBox`, which reads
`_folderHandler.FolderArray` and `_folderHandler.Suggestions`.

**Reachability: LIVE.** `PopulateFolderComboBox` is reachable from production UI code, not only from
tests. It is a public member of the item controller and is the synchronous counterpart of
`PopulateFolderComboBoxAsync`.

**Not changed by this branch.** The carried-handler adoption was added to `LoadFolderHandlerAsync`
only. `LoadFolderHandler` is byte-identical to its base-ref text.

**Referral route:** Deferred to a single consolidated follow-up issue filed by the parallel
orchestrator from a separate branch after this PR merges.

---

## 2. De-exempting any `[ExcludeFromCodeCoverage]` class

**Verdict: NOT-CONFIRMED as a defect within this change's scope.**

Evidence: the three classes this change touches that carry the attribute are
`FolderScoringService` (`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:198`),
`QfcCollectionController` (`QuickFiler/Controllers/QfcCollectionController.cs:21`) and
`QfcDatamodel` (`QuickFiler/Controllers/QfcDatamodel.cs:25`). Each carries a justification recording
that its body is COM-bound or WinForms-bound.

There is a genuine, standing tension between `CLAUDE.md`, which ratifies a COM/VSTO/WinForms
coverage exemption applied through this attribute, and
`.claude/rules/general-unit-test.md`, whose Coverage Exclusion Policy states that no production file
may be excluded from coverage measurement. That tension is a policy question, not a defect in this
code, and it is recorded in `evidence/baseline/phase0-instructions-read.md` rather than resolved
here.

**No attribute was added or removed anywhere in this change.** Proved by P2-T8.

**Referral route:** not applicable; no defect confirmed.

---

## 3. Splitting oversized files

**Verdict: CONFIRMED-DEFECT (pre-existing), and deliberately not fixed here.**

Evidence, measured by Derivation D8 at the current head of this branch:

| File | Lines | Over the 500-line limit by |
|---|---:|---:|
| QuickFiler/Controllers/QfcCollectionController.cs | 2336 | 1836 |
| QuickFiler/Controllers/QfcQueue.cs | 505 | 5 |
| QuickFiler.Test/Controllers/QfcFormControllerTests.cs | 792 | 292 |

All three were already over the limit at the base ref (2446, 610 and 827 respectively). **All three
are smaller after this change than before it**, because this change relocated whole members out of
them into new partial parts rather than extending them.

That relocation is not the split this item refers to. It moved only the members this change had to
edit, which is what the plan's file-size section mandates; a proper split would redistribute each
file by responsibility. `QfcQueue.cs` at 505 is five lines over and could be brought under the limit
by moving one more member, but doing so would touch code this change has no other reason to edit.

**Reachability: LATENT.** An oversized file is a maintainability defect, not a runtime one. Nothing
misbehaves because of it.

**Referral route:** Deferred to a single consolidated follow-up issue filed by the parallel
orchestrator from a separate branch after this PR merges.

---

## 4. Adding `InitAsync` to `IFolderSearchHandler`

**Verdict: NOT-CONFIRMED.**

Evidence: `UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs:14-39` declares exactly four
members: `FolderArray`, `Suggestions`, `FolderRowArray` and `FindFolder`. Its own documentation
comment at `:10-12` records why `InitAsync` is deliberately absent — construction goes through an
injectable `Func<IApplicationGlobals, object, FolderPredictor.InitOptions, FolderPredictor>` factory
with a **concrete** return type, precisely because `LoadFolderHandlerAsync` needs
`FolderPredictor.InitAsync`, which is not part of the narrow consuming surface.

The absence is a deliberate design decision with a recorded rationale, not an oversight. This change
does not need it either: the carried handler is already initialised, which is the point of carrying
it. Adding `InitAsync` would also be a change under `UtilitiesCS/`, which AC23 forbids.

**Referral route:** not applicable; no defect confirmed.

---

## 5. Deleting the dormant post-display filter

**Verdict: CONFIRMED-DEFECT (dead code), and deliberately retained.**

Evidence: `QfcHighConfidencePreFilter.FilterAsync` is reachable only through
`QfcHomeController.HighConfidencePreFilterLoader`, whose default value is set at
`QuickFiler/Controllers/QfcHomeController.cs:239-241`. A scan of `QuickFiler/` for
`HighConfidencePreFilterLoader` finds that declaration and its default initialiser and **no
invocation of the delegate anywhere in production code**. The remaining matches for
`QfcHighConfidencePreFilter.FilterAsync` are a log-message literal at
`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:76` and two `<see cref="...">` documentation
references at `:102` and `:194`.

The class is therefore dead production code carried for a decision that issue #233 reversed when it
moved high-confidence enforcement from post-display filtering to dequeue-time gating.

**Reachability: LATENT.** Dormant by construction. It cannot execute, so it cannot misbehave; the
cost is carried code and reader confusion.

**Retained deliberately.** AC13 requires it to remain dormant and requires the `Times.Never`
assertions that pin its dormancy to be preserved verbatim. Deleting it would delete those pins.
Its `QfcPreScoredItem` construction site at `:90` was updated by P1-T4 solely so the file compiles
after the constructor widened, and so it populates the new member; that is not an activation.

**Referral route:** Deferred to a single consolidated follow-up issue filed by the parallel
orchestrator from a separate branch after this PR merges.

---

## 6. Consolidating the duplicated `MailItemHelper.FromMailItemAsync` calls

**Verdict: CONFIRMED-DEFECT.**

Evidence: eight call sites of `MailItemHelper.FromMailItemAsync` exist under `QuickFiler/`:

| File | Line |
|---|---:|
| QuickFiler/Controllers/QfcCollectionController.cs | 362 |
| QuickFiler/Controllers/QfcFormController.Actions.cs | 47 |
| QuickFiler/Controllers/QfcFormController.Actions.cs | 242 |
| QuickFiler/Controllers/QfcHighConfidencePreFilter.cs | 210 |
| QuickFiler/Controllers/QfcItemController.ViewerSetup.cs | 387 |
| QuickFiler/Helper Classes/ConversationResolver.cs | 102 |
| QuickFiler/Helper Classes/ConversationResolver.cs | 192 |

(The eighth match, `QfcHighConfidencePreFilter.cs:191`, is a `<see cref>` in documentation, not a
call.)

The duplication that matters for this issue is the pair at
`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:210`, inside `FolderScoringService.ScoreAsync`,
and `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:387`, inside the item controller's own
helper load. Both build a `MailItemHelper` for the same mail item on the high-confidence path, so an
accepted item is marshalled from COM twice.

**Reachability: LIVE.** Both call sites execute on the high-confidence path in production.

**Not changed by this branch, and not fixed by it.** This change removes the duplicated *scoring*
pass by carrying the initialised handler; it does not remove the duplicated *helper construction*,
because the two calls request different data (`loadAll` differs) and consolidating them would mean
carrying the helper as well, which is a wider change than any acceptance criterion authorises.
`QfcItemController.ViewerSetup.cs:387` is unchanged by this branch.

**Referral route:** Deferred to a single consolidated follow-up issue filed by the parallel
orchestrator from a separate branch after this PR merges.

---

## Acceptance conditions

1. **Each of the six items carries a verdict with the file and line it rests on.** Three
   `CONFIRMED-DEFECT` (items 1, 5, 6), one `CONFIRMED-DEFECT (pre-existing)` (item 3), two
   `NOT-CONFIRMED` (items 2, 4). Every verdict cites at least one file and line.
2. **Each `CONFIRMED-DEFECT` item carries a referral record naming the promotion route.** All four
   name the same literal route, stated once at the head and repeated per item, so the follow-up
   carries a named owner rather than being left unassigned.
3. **No source file outside the change footprint required by AC1 through AC18 was modified for any of
   the six.** None of these items was fixed. `QfcItemController.FolderHandling.cs:27-55`
   (`LoadFolderHandler`), `UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs`,
   `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:387` and every
   `[ExcludeFromCodeCoverage]` attribute in the repository are unchanged. The formal footprint proof
   is P2-T11, and the attribute-invariant proof is P2-T8.
