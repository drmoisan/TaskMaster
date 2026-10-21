# R2 — Option decision

- Timestamp: 2026-09-02T01-27
- Issue: #678
- Task: [P1-T11]

R2 acceptance clause 1 requires that one of two options be chosen and the choice stated with
its reason.

## Clause 1 — the option chosen, and why

**Option 1 was chosen: align the projection.**

Option 2 — narrowing the documented claim and the test name so neither asserts unconditional
parity — was rejected because it would leave the stated invariant false rather than closing
it. The invariant R2 states is that the carried `PredeterminedFolder` and the `FolderArray`
entries are the *same projection of the same input*, so that `_itemViewer.FolderContains`
matches for every archive-rooted suggestion the predictor can produce.

In the (non-null globals, empty archive root) state the predictor's `FolderArray` entries
**are** separator-stripped, because
`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:845-858` guards only on
`_globals is null` and then forms `archivePrefix = _globals.Ol.ArchiveRootPath + "\\"`
unconditionally, giving a prefix of one separator. An unstripped carried value therefore
cannot match at the `FolderContains` boundary in that state, and the AC12 defect reopens in
exactly that state. Renaming the test would document the gap rather than close it.

The P1-T7 red run recorded that reopening directly, in the Moq invocation list of
`AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder`:
`FolderContains("\Projects\Active")` was probed with the raw value, missed, and the selection
fell back to `SetFolderSelectedIndex(1)`.

## Clause 2 — the parity target was not modified

Parity target: `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:845-858`, the private
member `ProjectSuggestionPath`. **It was not modified.** Scope constraint 1 forbids editing
any file under `UtilitiesCS/`.

Two commands prove it, and both outputs are recorded:

Command A, covering the whole branch:

```
git diff 807fb0bb6e5e49f43efa6b256b05960bf078ca19 -- UtilitiesCS
```

Output: **empty** (no output at all). Expected, because the previous cycle's footprint also
excluded `UtilitiesCS`.

Command B, covering this cycle's uncommitted state:

```
git status --porcelain -- UtilitiesCS
```

Output: **empty** (no output at all). This is the clause that can fail if this cycle edited
the parity target: at this point in the plan P1-T14 has not yet run, so any edit would still
be uncommitted and would appear here. Command A alone would not catch an uncommitted edit,
and command B alone would not catch one the previous cycle had already committed; the two are
complementary and both are required.

## Clause 3 — the two deliberate remaining divergences

Both are **null-safety differences rather than projection differences**: neither changes what
string the projection produces for an input both members can accept.

1. **A null or empty `folderPath` is returned unchanged rather than dereferenced.**
   `ProjectSuggestionPath` does not guard its input because that input comes from
   `Suggestions` and is never null there. `ProjectPredeterminedFolder` is called with
   `_predeterminedFolder`, which can legitimately be null or empty on a row with no carrier,
   so the guard is required. For every non-null, non-empty `folderPath` the two agree.

2. **A non-null globals with a null `Ol` is treated as an empty archive root rather than
   reproducing a null dereference.** `ProjectSuggestionPath` would throw a
   `NullReferenceException` on `_globals.Ol.ArchiveRootPath` in that state. The call site
   passes `_globals is null ? null : (_globals.Ol?.ArchiveRootPath ?? string.Empty)`, which
   maps that state to the empty-root behaviour instead. Reproducing a null dereference in
   UI-thread code would be a defect, not parity.

## Clause 4 — the single existing assertion that was corrected

- File: `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`
- Test: `ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection`
- Line: **222** in the pre-edit file (the expected-value line of the second of six
  assertions, whose call is `ProjectPredeterminedFolder(@"\\Archive\Projects\Active",
  string.Empty)`)
- Before, expected value: `@"\\Archive\Projects\Active"` (the identity)
- After, expected value: `@"\Archive\Projects\Active"` (one leading separator stripped)

This is the one correction scope constraint 4 authorises, and it is named in P1-T6 and
nowhere else. The `git diff HEAD` of that file reports exactly **1** removed line, which is
that assertion's expected-value line, and no other removal anywhere in the file. The
surrounding five assertions, the test name and the `[TestMethod]` attribute are untouched.

After the fix the test name
`ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection` is accurate at the
`(folderPath, archiveRootPath)` level the test actually exercises, so the test is neither
renamed nor weakened.

## Output Summary

Option 1, aligning the projection, was chosen; option 2 was rejected because it would leave
the invariant false in the (non-null globals, empty archive root) state, which the P1-T7 red
run observed directly. The parity target `FolderPredictor.cs:845-858` was not modified, proved
by two commands whose outputs are both empty. Two deliberate divergences remain and both are
null-safety differences. Exactly one existing assertion was corrected, at
`QfcItemController.FolderHandlingTests.Part2.cs:222`, from `@"\\Archive\Projects\Active"` to
`@"\Archive\Projects\Active"`.
