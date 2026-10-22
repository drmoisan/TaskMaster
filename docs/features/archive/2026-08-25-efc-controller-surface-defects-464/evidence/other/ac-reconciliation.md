# [P11-T15] Acceptance-criteria reconciliation

Timestamp: 2026-08-28T02-14
Task: [P11-T15]
Command: checkbox census of `docs/features/active/efc-controller-surface-defects-464/spec.md`, plus a
line-by-line comparison of the delivered file against its state before this batch to prove no criterion
text was modified
EXIT_CODE: 0

## Census

| Measure | Value |
|---|---|
| Acceptance-criterion checkboxes in `spec.md` | **74** |
| Reading `- [x]` | **74** |
| Reading `- [ ]` | **0** |

The file contains exactly 74 acceptance-criterion checkboxes, which is the count `spec.md` §`Acceptance
Criteria` itself declares (`spec.md:909`: "**74 criteria**, distributed as: #459 — 4, #460 — 7, #461 — 4,
#463 — 4, #464 — 12, #465 — 11, #466 — 8, #467 — 7, cross-cutting — 17").

## No criterion text was modified

The delivered `spec.md` was compared line by line against its state at the start of this batch. The file
has the same line count (1168) before and after. Exactly **21** lines differ, and for every one of the 21
the difference is `- [ ] ` becoming `- [x] ` with the remainder of the line byte-identical. The count of
changed lines whose change is **not** a pure checkbox flip is **0**.

The 21 flipped lines, by line number: 926, 979, 990, 1000, 1009, 1013, 1014, 1015, 1017 (Phase 9), and
1004, 1005, 1006, 1007, 1008, 1010, 1011, 1012, 1016, 1018, 1019, 1020 (Phase 11).

## Outcome: PASS at 74 of 74

**Authorised exception 1 does not apply.** It is conditioned on the `[P0-T9]` baseline unformatted set
being **non-empty**. `[P0-T9]` recorded `BASELINE_UNFORMATTED: (none)`, cardinality 0, and `[P10-T3]`
recorded `EXIT_CODE: 0` over 1549 files with an empty unformatted set. The exception's precondition is
therefore false, the criterion beginning "`dotnet tool run csharpier check .` reports no formatting
differences" is satisfied outright, and `[P11-T2]` checked it off on its own evidence rather than under
the exception.

The plan states the pass outcome as 74 of 74 when the exception does not apply. **74 of 74 are checked.**

No criterion was left unchecked, so there is no criterion whose verbatim text, reason and gap-documenting
artifact need recording here, and this task's outcome is **not** remediation-required.

**No criterion was checked to clear a gate.** Every one of the 21 flips this batch made cites a
specific evidence artifact produced in this batch and recording a real measurement:

| Criterion line | Evidence artifact |
|---|---|
| 926, 979, 1000, 1014 | `evidence/qa-gates/sibling-ownership.md` |
| 990 | `evidence/qa-gates/project-files.md` |
| 1009 | `evidence/qa-gates/pre-existing-tests.md` |
| 1013 | `evidence/qa-gates/viewersetup-scope.md` |
| 1015 | `evidence/qa-gates/webview2host-invariant.md` |
| 1017 | `evidence/qa-gates/interface-stability.md` |
| 1004 | `evidence/baseline/phase0-instructions-read.md` and the four baseline command artifacts |
| 1005 | `evidence/qa-gates/csharpier-check.md` |
| 1006 | `evidence/qa-gates/msbuild-analyzers.md` |
| 1007 | `evidence/qa-gates/msbuild-nullable.md` |
| 1008 | `evidence/qa-gates/quickfiler-test-final.md` |
| 1010, 1011, 1012 | `evidence/qa-gates/file-sizes-final.md` |
| 1016 | `evidence/qa-gates/exemption-audit.md` |
| 1018, 1019 | `evidence/qa-gates/test-policy-audit.md` |
| 1020 | `evidence/other/user-story-absence.md` |

### No criterion is structurally uncheckable

A scan of all 74 criteria for wording that depends on the pull request, on a reviewer's sign-off in the
PR, or on the merge itself returns nothing. In particular, no criterion is worded as "closed by the
merge", so none had to be left unchecked on the ground that merging into an integration branch does not
close a GitHub issue. The four criteria whose text contains the token `merge` or `closed` are
`spec.md:949`, `:965`, `:1011` and `:1012`; each refers to the merge-**base** line count or to closure by
a code deletion this feature performed, not to the merge event.

## The three instrument substitutions, restated as recorded deviations rather than gaps

### 1. `BoundaryErrorSink` in place of direct `logger.Error` verification (decision D10)

Every RC3 fault boundary calls
`internal System.Action<string, System.Exception> BoundaryErrorSink { get; set; }`, defaulted in its
declaration at `EfcFormController.cs:127-128` to a delegate whose entire body is a single
`logger.Error(message, exception)` call on the pre-existing static logger. The five `[DataRow]` results
of `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow` inject a counting delegate and assert exactly
one invocation per boundary; `BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing` exercises the
default delegate. `[P6-T15]` records by source inspection that the default body is that one call and
nothing else, and the `spec.md` criterion at `:959` was amended at preflight to name this instrument
explicitly.

**Correction to decision D10's stated premise, recorded rather than repeated.** D10 justifies the
substitution on the ground that the test project carries no log4net reference. **That premise is false on
this execution base**: `QuickFiler.Test/QuickFiler.Test.csproj:214-215` carries a
`<Reference Include="log4net, Version=3.3.2.0, ...">` with a `HintPath` to
`..\packages\log4net.3.3.2\lib\net462\log4net.dll`, added by merged upstream work. The false premise is
recorded here and is deliberately **not** restated as fact anywhere in this feature's artifacts.

The substitution was nevertheless retained, and remains the right instrument on its merits rather than by
necessity:

- It mirrors the established in-repo seam-and-default pattern that merged feature #484 uses for
  `MoveFailureNotifier` — an injectable delegate with a production default that every failure-path test
  replaces — which the upstream-constraints briefing names as the pattern RC3 should follow rather than
  reinvent.
- It observes the boundary as a deterministic call count rather than through a shared, process-wide
  log4net appender, which is a mutable global that concurrent test classes contend for.
- Changing instrument at Phase 11 would invalidate the fail-before evidence recorded for `[P6-T5]` and
  `[P6-T8]` and would rewrite an already-amended `spec.md` criterion.

### 2. Direct invocation of `ToggleExpansionOn` and `ToggleExpansionOff` in place of awaiting the WPF dispatcher (constraint C4 item 2)

`AsyncExpansionPath_OnOffOn_LeavesCharActionsKeysUnchanged` drives the two members directly rather than
awaiting a WPF dispatcher round trip. The pump-less MSTest host has no dispatcher to pump, and awaiting
one there does not complete. The property under test — that an on/off/on expansion cycle leaves the
`CharActions` key set unchanged — is a property of the two members, not of the marshalling, so the direct
invocation exercises exactly the logic the criterion names.

### 3. `IsSelectableFolder` and `IsBannerRow` in place of `IsValidSelection` and `ActionOkAsync`'s guard

The criterion at `spec.md:977` reads:

> A named test asserts a row of exactly three `=` characters and a row of exactly four `=` characters
> classify identically in `IsValidSelection` and in `ActionOkAsync`'s guard.

It names those two sites literally, and no task in this plan invokes either: `ActionOkAsync` shows a
`MessageBox` on its rejection path, which the headless test policy forbids driving. `[P7-T9]`'s
`IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically` exercises the two helpers
instead.

The substitution is sound because the two named sites **are** the two helpers after `[P7-T8]` and
`[P7-T11]`:

- `IsValidSelection` is a single expression over `IsSelectableFolder`, delivered at
  `EfcFormController.cs:1155` as `internal bool IsValidSelection => IsSelectableFolder(SelectedFolder);`
- `ActionOkAsync`'s guard is a composition over `IsBannerRow`, whose delivered expression `[P7-T13]`
  records as the source-level proof of that identity.

The two named sites and the two exercised helpers are therefore the same classification logic, not merely
similar logic.

**`ActionDeleteAsync` is not a substitution and is not listed as one.** `[P7-T4]`'s
`ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows` awaits `ActionDeleteAsync()` itself
twice, so the criterion beginning "A named test drives `ActionDeleteAsync` twice" is satisfied by its
literal instrument.

## Further deviations recorded in this batch, none of which leaves a criterion unchecked

1. **Base drift — `BASELINE_SHA` no longer isolates this feature's diff.** A mandated integration merge
   (`25924673`, second parent `38f09789`) placed merged siblings #476 and #501 inside
   `BASELINE_SHA..HEAD`, so the Phase 9 scope gates were additionally evaluated against `38f09789`, the
   merged integration tip, which `git merge-base` confirms is an ancestor of `HEAD`. Both results are
   recorded in every affected artifact. Recorded in `evidence/qa-gates/changed-file-set.md`.
2. **Stale plan figure — `QfcItemController.ViewerSetup.cs` "still 430".** The file is 499 and was 499 at
   `BASELINE_SHA`; the substantive one-added/one-deleted constraint holds. Recorded in
   `evidence/qa-gates/file-sizes-final.md` and, at Phase 0, in
   `evidence/baseline/file-sizes-and-exemptions.md`.
3. **Stale plan figure — `EfcFormController.cs` merge-base count.** The true count is 1073, not 1084, so
   the stricter derived gate of 1193 was held. Delivered 1189, net delta +116, itemised per remedy.
4. **`MoveMailAsync` is not implemented by `EfcItemController` on this base**, contrary to the
   upstream-constraints briefing. This feature removed nothing; the member was never there. Recorded in
   `evidence/qa-gates/interface-stability.md`.
5. **Manual check 1 is `MANUAL_CHECK_DEFERRED`** — see below.
6. **The seven follow-up items are recorded but not promoted** — see below.
7. **The RC7 residual is reported and deliberately not fixed** — see below.

## Outstanding items — recorded, and none of them a criterion

These do not affect the 74-of-74 outcome, because no acceptance criterion asserts any of them. They are
recorded here so they are handed off rather than lost.

| Item | Status | Artifact |
|---|---|---|
| Manual check 1 — Alt+F and Alt+M open the two menus in a live Outlook host | **`MANUAL_CHECK_DEFERRED`**, not recorded as a pass; no Outlook process is running and the test policy forbids substituting a live form | `evidence/other/manual-validation.md` |
| Promotion of the six `spec.md` follow-up items plus the seventh discovered on this base | **NOT CREATED**; the `potential_to_issue` MCP tool is absent from this agent's tool set and writing a potential document would breach the `[P11-T17]` scope gate. Duplicate check returned empty for all seven | `evidence/other/followup-promotions.md` |
| RC7 residual — `EfcSelectionGuard.BannerPrefix` is a third arity variant (three `=`) and the comment near `EfcFormController.cs:325` describes a four-`=` rejection the guard does not implement | Reported, deliberately **not fixed**; the file belongs to merged sibling #614 and is outside this feature's owned set | `evidence/qa-gates/sibling-ownership.md`, `evidence/other/followup-promotions.md` |
| 14 aggregate-run test failures observed at the Phase 8 boundary in three `QfcItemController.*` files | Base-introduced load-driven flakiness; **all pass** in the `[P10-T6]` final run (0 failed) and in the `[P10-T7]` repository-wide run (0 failed) | `evidence/qa-gates/quickfiler-test-final.md`, `evidence/qa-gates/postmerge-quickfiler-test.md` |

Output Summary: PASS. `spec.md` contains exactly **74** acceptance-criterion checkboxes, **74 of 74** now
read `- [x]`, and **0** remain unchecked. No criterion text was modified: exactly 21 lines differ from the
pre-batch state and every one of the 21 is a pure `- [ ]` to `- [x]` flip. Authorised exception 1 does
**not** apply, because `BASELINE_UNFORMATTED` is empty and `[P10-T3]` exited 0 with an empty set, so the
outcome is the 74-of-74 pass form and **not** remediation-required. The three instrument substitutions are
restated as recorded deviations; `ActionDeleteAsync` is correctly not among them. Decision D10's stated
premise is false on this base — `QuickFiler.Test.csproj:214` does carry a log4net reference — and that
correction is recorded rather than the false premise repeated; the substitution is retained on its merits.
Four outstanding non-criterion items are handed off: the deferred manual check, the seven unpromoted
follow-ups, the RC7 residual, and the base-introduced test flakiness that did not recur.
