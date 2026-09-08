# Code Review — issue #812 (utilitiescs-archive-root-read-and-user-email-retry-801-805)

- Artifact timestamp: 2026-09-08T17-00
- Scope: full branch diff against `origin/main`
- Verdict: **ACCEPT. 0 blocking findings, 5 advisory findings, 0 remediation-required findings.**

---

## 1. What was reviewed and how

Every production and test hunk was read in the diff and then re-read in its post-change form on disk,
so that the review judges the delivered file rather than the patch. Four specific questions posed by
the delegating agent were treated as first-class review objects and each was answered by inspection
plus, where possible, by applying the prohibited edit and confirming a test turns red.

---

## 2. Defect A — the guarded accessor

### 2.1 Does `GetArchiveRootForDisplayOrNull` degrade correctly? — Yes.

`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.ArchiveRoot.cs:48-66`:

```csharp
private string? GetArchiveRootForDisplayOrNull()
{
    try
    {
        // Null-conditional is load-bearing: the navigation-only constructor sets _globals
        // to null!, and the pre-change display read already tolerated that.
        return _globals?.Ol.ArchiveRootPath;
    }
    catch (InvalidOperationException ex)
    {
        logger.Warn( /* … */, ex);
        return null;
    }
}
```

- **Catch breadth is exactly right.** One `catch`, and its type is `InvalidOperationException` — the
  single type `AppOlObjects.ResolveValidatedArchiveRootPath` normalises to. `COMException` and every
  other type propagate. This is verified twice: by reading the file, and by
  `FolderArray_WhenArchiveRootPathThrowsComException_PropagatesComException`, which turns red if the
  catch is widened to `catch (Exception)`.
- **The null return is not an error path.** `ArchiveStemProjection.ToDisplayStem` returns
  `folderPath` unchanged when `archiveRoot is null` (`ArchiveStemProjection.cs:45-48`), which is the
  documented contract at `:32-35`. That dependency is now pinned by `ToDisplayStem_NullRoot_ReturnsInputUnchanged`
  rather than merely implied, which was the correct call: the whole degradation rests on it.
- **The warning is redacted correctly.** The message names the rule and the consequence and contains
  no archive-root path and no mailbox address; the exception is passed as the second argument so a
  maintainer retains the detail. This follows the `EfcDataModel` / `AppOlObjects.ArchiveRoot.cs`
  (#602) precedent.
- **Warning volume is bounded.** The accessor appears exactly 4 times in `FolderPredictor.cs`, once
  per projection helper, each hoisted above its loop. A per-element read would have produced up to 5
  warnings per suggestion projection; three read-count tests pin the bound at 2, 2 and 1.

### 2.2 Are the five functional reads genuinely still unguarded and still throwing? — Yes.

Independently re-derived, not accepted from the evidence artifact. `Grep` over the post-change
`FolderPredictor.cs`:

- `_globals.Ol.ArchiveRootPath` occurs at exactly 5 sites: `:305` (`FindFolder`, `emailSearchRoots`
  seed), `:376` (`FindFolderRows`, `emailSearchRoots` seed), `:687` (`CreateFolder`, `olAncestor`),
  `:752` (`CreateFolderAsync`, `olAncestor`), `:909` (`LoopFolders`, `olAncestor`).
- `_globals?.Ol.ArchiveRootPath` occurs 0 times in that file.
- A `Grep` for `\btry\b|\bcatch\b` over the entire file returns **no matches**. There is no `try` or
  `catch` anywhere in `FolderPredictor.cs`, so no functional read can be inside a guarded region.
  This is a stronger proof than a per-member scan and it is unambiguous.
- None of the five is a call to `GetArchiveRootForDisplayOrNull`.

The pre-change counts were 7 and 1, so both conditions flip across the change and neither can pass
vacuously. `FindFolder_WithNullEmailSearchRootsAndThrowingArchiveRoot_StillThrowsInvalidOperationException`
pins `:305` behaviourally: line 305 is the first executable statement of `FindFolder`, so the
`InvalidOperationException` the test observes is deterministically the one from that read, not an
incidental exception from later in the method.

The five reads are correctly classified. Substituting a null root at any of them changes which
folders are searched (`:305`, `:376`) or where a folder is created (`:687`, `:752`, `:909`) — a
silent behaviour change, not graceful degradation. Leaving them throwing is the right call.

### 2.3 Is the `ProjectSuggestionPath` relocation behaviour-preserving? — Yes, on the body; the read
count is intentionally reduced.

Old body:

```csharp
return ArchiveStemProjection.ToDisplayStem(folderPath, _globals?.Ol.ArchiveRootPath)!;
```

New body:

```csharp
return ArchiveStemProjection.ToDisplayStem(folderPath, archiveRoot)!;
```

The expression, the null-forgiving operator and its CS8603 rationale comment are carried over
verbatim; only the root's provenance moves from an inline read to a parameter. Two consequences, both
intended and both spec-mandated:

1. The archive root is now read once per helper invocation instead of once per element. That is the
   AC2 invariant, and it is the reason the relocation exists.
2. `AddSuggestions` at `:815` changes from a method group (`.Select(ProjectSuggestionPath)`) to a
   lambda (`.Select(x => ProjectSuggestionPath(x, r))`). Semantically equivalent for this use.

The relocation is also what keeps `FolderPredictor.cs` at or below its pre-change size: the two
hoists add 1 line each and the removed method plus its blank line takes 7, netting 1002 → 997. No
stale one-argument call site remains anywhere in the repository (verified by repo-wide grep; the only
other occurrences are two prose comments in `QuickFiler`).

---

## 3. Defect B — the latch

### 3.1 Is the latch correct? — Yes.

`UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs:48-56`:

```csharp
if (
    Current is not null
    && Current.UserEmailAddress is null
    && !_userEmailRetryAttempted
)
{
    _userEmailRetryAttempted = true;
    Current.RefreshUserEmailAddress();
}
```

- **Ordering is right.** The latch is set *before* the call, so a throw out of
  `RefreshUserEmailAddress` still consumes the single attempt. Setting it after the call would leave
  the bound unenforced on the exact failure path the change exists to bound. (In practice
  `GetSmtpAddressFromStore` catches `COMException` per step and cannot escape, but the ordering is
  correct regardless of that, which is the right way to write it.)
- **The latch supplements the null check rather than replacing it.** The `Current.UserEmailAddress is
  null` conjunct is retained, so a populated address still costs zero UI-thread latency. This is
  pinned by `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_NeverInvokesExchangeUserLookup`.
- **It is per instance, not static and not per store.** `private bool _userEmailRetryAttempted;` at
  `StoreWrapperController.cs:105`. `PopulateWithCurrent_OnASecondControllerOverTheSameFailingStore_RetriesOnceMore`
  shares one `StoreWrapper` between two controllers and asserts `Times.Exactly(2)`, so a `static`
  latch would fail it.
- **No reset, and nothing was added to `Launch()`.** Verified: the only hunk in
  `StoreWrapperController.cs` is `@@ -95,0 +96,11 @@`, a pure insertion consuming zero pre-image
  lines, which therefore cannot intersect the `Launch()` span. `[ExcludeFromCodeCoverage]` still
  immediately precedes `public void Launch()`. The decision to place no reset in `Launch()` is
  well-reasoned: an exempt member emits no `<method>` element in Cobertura, so a reset there would be
  neither executable nor observable under unit test — "proven to work but never proven to be wired".
- **The load-bearing assumption is documented in code, not only in the spec.** The XML doc on the
  field names `RibbonController.FolderStoresSettings` as the sole construction site and states that
  reusing a controller would silently reduce the bound to once per lifetime. That is exactly the
  right mitigation for an invariant that lives in a different file.

### 3.2 Do the three existing #797 tests still hold? — Yes.

The diff for `StoreWrapperController_Tests.Display.cs` contains no change to any of the three
methods' Arrange, Act or Assert; the only edit inside them is the 3-line Arrange comment in
`PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_DoesNotRetryLookup`. The new helper
`CreateDisplayFailingSmtpRootFolderWithUser` is a **sibling** of `CreateDisplayFailingSmtpRootFolder`
rather than a modification of it, and the new-helper hunk is a pure insertion at pre-image position
64, consuming no pre-image line. That is the right call: changing the original helper's return type
would have rippled into three tests the change is obliged not to disturb. The mild duplication
between the two helpers is a deliberate and correctly-reasoned trade, documented in the helper's own
XML doc.

All three are recorded `Passed` in the final run.

---

## 4. Test quality

The new tests pin behaviour; none passes vacuously. Section 1 of `policy-audit.2026-09-08T17-00.md`
records the per-method analysis, including the prohibited-edit reasoning for the five methods that
pass both before and after by design. Summary: 11 of 16 have recorded fail-before evidence, and each
of the remaining 5 turns red under a specific, plausible regression of the delivered code.

Two fixture details are worth calling out as good practice:

- Suggestion scores are seeded strictly descending (`1000 - index * 100`) with an in-code explanation
  that equal scores would fall back to an ordinal key tie-break and invalidate the expected
  sequences. That is a real determinism hazard, identified and closed rather than discovered later.
- Every seeded path is archive-rooted (`\\ArchiveRoot\Suggested\One`, and so on) with a comment
  stating why: a resolvable root *would* project each of them to a shorter stem, so byte-identical
  pass-through is an observable outcome rather than a coincidence. Without this the identity
  assertions would be weak.

---

## 5. Findings

### CR-1 (Advisory) — the latch is per controller, not per store; the spec's accepted-behaviour paragraph understates its reach

`_userEmailRetryAttempted` is keyed on the controller, so the **first** failing store re-selection in
a dialog session consumes the session's only retry. If a profile carries two or more stores whose
startup SMTP lookup failed, the second and subsequent such stores are denied the retry they would
have received, and their labels render `Email address unavailable: …` for the life of that dialog.

This is exactly what AC4 asks for, and the field's XML doc states the per-instance bound accurately,
so the **code is correct and self-describing**. The gap is in the spec's Decision B1
accepted-behaviour-change paragraph, which describes only "re-selecting the **same** store no longer
re-attempts within one dialog session". The delivered bound is broader than that sentence.

Impact is bounded: reaching this state requires at least two stores that each failed their Init-time
lookup, and the remedy (close and reopen the dialog) is the same one already documented.

Recommendation, non-blocking: state the cross-store consequence in the PR body, or in a follow-up
consider keying the latch on the store (for example a `HashSet<StoreWrapper>` of attempted stores),
which would preserve the "one blocking COM chain per store per dialog open" reading without
reintroducing the unbounded per-re-selection retry. Do not change it in this branch — AC4 pins the
per-instance bound and the tests pin it against exactly this kind of drift.

### CR-2 (Observation) — `_globals?.Ol` guards the outer reference only

`return _globals?.Ol.ArchiveRootPath;` short-circuits on a null `_globals` but dereferences `Ol`
unconditionally, so a non-null `_globals` with a null `Ol` raises `NullReferenceException` past the
guard. This is **character-for-character the pre-change expression** at the old `ProjectSuggestionPath`
site, so preserving it is the correct reading of "behaviour-preserving relocation" and changing it
would have been unrequested scope. Noted only so the shape is on the record and is not later mistaken
for an oversight introduced here.

A related, benign asymmetry: the two recents sites previously read `_globals.Ol.ArchiveRootPath`
non-conditionally and now tolerate a null `_globals` through the accessor. The widening is
unreachable, because `AddRecents` and `AddRecentRows` both dereference `_globals.AF.RecentsList`
before the read, so a null `_globals` throws earlier regardless. No behaviour change in practice.

### CR-3 (Advisory) — Defect A's user-visible outcome is not restored end to end; the follow-up filing is owed

Verified still true at head. `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:231-234`:

```csharp
string predetermined = ProjectPredeterminedFolder(
    _predeterminedFolder,
    _globals is null ? null : (_globals.Ol?.ArchiveRootPath ?? string.Empty)
);
```

This sits inside `AssignFolderComboBox` (`:191`), 21 lines after `_folderHandler.FolderArray` at
`:212` and 10 lines after `_folderHandler.FolderRowArray` at `:221`, in the same call frame and
inside no `try`. The `?.` guards a null `Ol`, not a throwing property, and `??` never runs because
the property throws before it is reached. **The net effect of this change on the QuickFiler path is
that the throw site moves from `:212` to `:233`, not that it disappears.**

This is spec Non-Goals item 2 and Risk 3 — correctly identified, correctly scoped out, and correctly
disclosed, with AC1 verified at the `FolderPredictor` unit level as the spec itself states. It is not
a defect in this change. The spec's Rollout section commits to filing the follow-up defect **before
closing #812**; that filing could not be confirmed from the working tree and is recorded here as
**owed**, alongside the two other follow-ups the same section names (#797 CR-2 `SerializeNow`
UI-thread file I/O and unbounded write-lock wait; the non-blocking Outlook COM read from #797
Non-Goals item 8).

### CR-4 (Observation) — the logged warning is asserted only indirectly

No test observes `logger.Warn`. The stated reason is sound — direct observation requires mutating the
process-global log4net repository, which UT4 forbids, and the injected-sink alternative was rejected
along with the rest of the `OutlookFolderHierarchyProvider` precedent for independent reasons. The
review verified by inspection that the new file contains exactly one `catch`, exactly one
`logger.Warn`, and a message free of paths and addresses; the read-count tests bound the warning
count. This is the correct mitigation given the constraint, recorded so the audit trail shows the
gap was reasoned about rather than missed.

### CR-5 (Observation) — pre-existing structural debt carried forward, not worsened

`FolderPredictor.cs` remains at 997 lines against a 500-line cap. The partial-part remedy is the
established containment in this exact class (documented at `FolderPredictor.IFolderSearchHandler.cs:4-9`)
and this change applies it again, reducing the file by 5 lines. `UtilitiesCS.Test/…/FolderPredictorTests.cs`
(1067 lines) and `StoreWrapperController_Tests.Launch.cs` (480 lines) were correctly kept out of the
diff so neither grows. No action requested in this branch.

---

## 6. Things the change got right that are worth recording

- **The scope correction over `issue.md` was necessary, not gold-plating.** `issue.md` named two read
  sites; the spec's call-graph analysis established that `ProjectSuggestionPath` reaches the property
  *before* the recents projection whenever the suggestion set is non-empty, so guarding only the two
  named sites would have left the defect observable in the common case. The four-site fix is the
  minimal correct fix, not a widened one.
- **The `issue.md` AC2 premise was contradicted rather than implemented.** `issue.md` proposed a flag
  "reset in `Launch`". The spec established from the single production construction site that no
  reset is needed and that a reset placed in an `[ExcludeFromCodeCoverage]` member would be
  unobservable. Correcting an upstream requirement on evidence, and saying so, is the right
  behaviour.
- **Both fixes have genuine RED-first evidence** with exact failing sets, not merely green runs.
- **The prose corrections mark themselves as dated amendments referencing #812** rather than silently
  rewriting history, and the five historical #797 artifacts are demonstrably untouched. The #797 AC6
  passage's internal self-contradiction is explicitly reconciled rather than half-fixed.
- **Evidence hygiene.** Absolute host paths and account/machine tokens are kept out of every committed
  artifact, including a recorded repair-and-re-run when the first sweep found a single prose mention.
  The `.trx` and `.coverage` attachments stay in the git-ignored tree.

---

## 7. Verdict

**ACCEPT — 0 blocking findings.** The delivered code does what the spec says, the tests pin it, the
toolchain evidence is complete and demonstrably non-vacuous, and the two coverage FAIL rows are
pre-existing repository conditions this branch improves. CR-1 and CR-3 warrant a sentence each in the
PR body; CR-3's follow-up issue is owed before #812 is closed.
