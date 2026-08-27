# [P5-T28] Acceptance-criteria reconciliation

Timestamp: 2026-08-27T20-19
Command: enumeration of every markdown checkbox in the `## Acceptance Criteria` section of `docs/features/active/quickfiler-keyboard-action-defects-444/spec.md`, read from disk, joined to the plan's acceptance-criteria identifier table by section heading and ordinal
EXIT_CODE: 0
Output Summary: 57 rows enumerated. Read from `spec.md` at the moment of writing: 53 checked, 4
unchecked. The fourth unchecked row is AC-QA-13, which `[P5-T29]` checks off immediately after this
artifact; the terminal unchecked set is therefore exactly the three orchestrator-deferred criteria
AC-472-10, AC-482-11, and AC-482-12. See the addendum at the end of this artifact for the terminal
counts.

## Source and structure

- AC source: `docs/features/active/quickfiler-keyboard-action-defects-444/spec.md`
- Work mode: `full-bug`, so `spec.md` is the **sole** AC source; `user-story.md` is not consulted and
  does not exist in this feature folder.
- Section counts observed on disk: `### Issue #444` 11, `### Issue #472` 10, `### Issue #482` 12,
  `### Upstream contract and scope discipline` 11, `### File-size, toolchain, and coverage` 13.
  Total **57**, matching the plan's stated structure of 11, 10, 12, 11, 13.
- Every `- [ ]` or `- [x]` in that section is an acceptance criterion; no other checkbox appears there.

Criterion text is truncated at 150 characters in the table for width. The authoritative text is in
`spec.md`, unmodified.

## All 57 criteria

| ID | Section | Ordinal | On-disk state | Evidence pointer | Criterion text (truncated) |
| --- | --- | --- | --- | --- | --- |
| AC-444-01 | Issue #444 | 1 | `- [x]` | `[P1-T22]` | **(Inherited from #468 — verify, do not re-perform.)** A repository-wide search of `*.cs` for the identifier `WireUpKeyboardHandler` returns zero h... |
| AC-444-02 | Issue #444 | 2 | `- [x]` | `[P1-T23]` | **(Inherited from #468 — recorded, not implemented.)** The promoted #444 criterion "The duplicate registration in `QfcCollectionController.cs` is r... |
| AC-444-03 | Issue #444 | 3 | `- [x]` | `[P1-T24]` | The intended `Keys.Down` behaviour for the QuickFiler collection surface is decided and recorded in `## Proposed Fix` of this spec as `SelectNextIt... |
| AC-444-04 | Issue #444 | 4 | `- [x]` | `[P1-T25]` | `KbdActions(IEnumerable<UClass>)` in `QuickFiler/Controllers/KbdActions.cs` throws `ArgumentException` when the supplied sequence contains two or m... |
| AC-444-05 | Issue #444 | 5 | `- [x]` | `[P1-T26]` | The constructor guard compares using `KbdActions.StoredKeyEquals` and not `KeyEquals`. Verified by the new test in `QuickFiler.Test/Controllers/Kbd... |
| AC-444-06 | Issue #444 | 6 | `- [x]` | `[P1-T27]` | The pre-existing characterization test `KbdActionsTests.Add_WhenSourceAndStoredKeysAreDistinct_DoesNotTreatSubstringAsDuplicate` (`QuickFiler.Test/... |
| AC-444-07 | Issue #444 | 7 | `- [x]` | `[P1-T28]` | `new KbdActions<…>(null)` still throws `ArgumentNullException` and not `NullReferenceException`. Verified by a named null test in `QuickFiler.Test/... |
| AC-444-08 | Issue #444 | 8 | `- [x]` | `[P1-T29]` | A duplicate-free seed sequence, and a sequence repeating a `Key` under different `SourceId` values, both construct without throwing. Verified by tw... |
| AC-444-09 | Issue #444 | 9 | `- [x]` | `[P1-T30]` | The `KbdActions` constructor logs via the existing `logger.Error` immediately before throwing, matching the pattern at `QuickFiler/Controllers/KbdA... |
| AC-444-10 | Issue #444 | 10 | `- [x]` | `[P1-T31]` | `RegisterAsyncKeyActions` registers exactly one `("Collection", Keys.Down)` entry bound to `SelectNextItemAsync` and exactly one `("Collection", Ke... |
| AC-444-11 | Issue #444 | 11 | `- [x]` | `[P1-T32]` | The duplicate-guard regression test was observed **failing before** the `KbdActions.cs` change and **passing after**, with both runs recorded in `d... |
| AC-472-01 | Issue #472 | 1 | `- [x]` | `[P2-T17]` | `QfcCollectionController` records the digit width used at registration in a new `private int _registeredDigits` field, assigned inside `RegisterNav... |
| AC-472-02 | Issue #472 | 2 | `- [x]` | `[P2-T18]` | `UnregisterNavigation` selects its key format from `_registeredDigits` and contains **zero** reads of the `Digits` property. Verified by a source s... |
| AC-472-03 | Issue #472 | 3 | `- [x]` | `[P2-T19]` | The format selection is written as `_registeredDigits == 2 ? "00" : ""` so that a controller built via `FormatterServices.GetUninitializedObject` (... |
| AC-472-04 | Issue #472 | 4 | `- [x]` | `[P2-T20]` | Registering at 10 items and unregistering at 9 leaves no orphaned `"0"`-prefixed navigation key other than the single `"10"` entry the loop bound c... |
| AC-472-05 | Issue #472 | 5 | `- [x]` | `[P2-T21]` | The width-fidelity test carries an XML documentation comment attributing the residual `"10"` entry to the separately-promoted count-mismatch defect... |
| AC-472-06 | Issue #472 | 6 | `- [x]` | `[P2-T22]` | The mirror-direction test (register at 9 items with `_digits = 1`, grow to 10, unregister) asserts the same width-fidelity property and passes. |
| AC-472-07 | Issue #472 | 7 | `- [x]` | `[P2-T23]` | The four pre-existing navigation tests in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` (at `:409`, `:430`, `:452`, `:474` as of ba... |
| AC-472-08 | Issue #472 | 8 | `- [x]` | `[P2-T24]` | `QuickFiler/Interfaces/IQfcCollectionController.cs` is not modified. Verified by `git status` / the branch diff showing the path absent. |
| AC-472-09 | Issue #472 | 9 | `- [x]` | `[P2-T25]` | The #472 regression test was observed **failing before** the `QfcCollectionController.cs` change and **passing after**, with both runs recorded in ... |
| AC-472-10 | Issue #472 | 10 | `- [ ]` | DEFERRED — `[P5-T25]` | The unbracketed-removal count-mismatch defect described in `### Downstream notes` item 3 is promoted through the feature-promotion lifecycle into a... |
| AC-482-01 | Issue #482 | 1 | `- [x]` | `[P3-T20]` | A new `private void SyncExpandedRegistrations(bool expanded)` exists in `QuickFiler/Controllers/QfcItemController.Navigation.cs`, carries no `[Excl... |
| AC-482-02 | Issue #482 | 2 | `- [x]` | `[P3-T21]` | Both `ToggleExpansion(Enums.ToggleState)` and `ToggleExpansionAsync(Enums.ToggleState)` delegate expansion registration to `SyncExpandedRegistratio... |
| AC-482-03 | Issue #482 | 3 | `- [x]` | `[P3-T22]` | Both `ToggleState` overloads retain their existing accessibility, `virtual` modifier, parameter list, return type, and `[System.Diagnostics.CodeAna... |
| AC-482-04 | Issue #482 | 4 | `- [x]` | `[P3-T23]` | The sequence `ToggleExpansionAsync(On)` → `ToggleExpansion(Off)` → `ToggleExpansionAsync(On)` completes without throwing `ArgumentException`. Verif... |
| AC-482-05 | Issue #482 | 5 | `- [x]` | `[P3-T24]` | After either `ToggleState` overload completes with `_expanded == true`, both `_kbdHandler.CharActions` and `_kbdHandler.CharActionsAsync` hold exac... |
| AC-482-06 | Issue #482 | 6 | `- [x]` | `[P3-T25]` | After either `ToggleState` overload completes with `_expanded == false`, both registries hold zero `'B'` and zero `'D'` entries for `ItemHelper.Ent... |
| AC-482-07 | Issue #482 | 7 | `- [x]` | `[P3-T26]` | Two consecutive `ToggleState.On` calls on the same overload do not throw. Verified by the named idempotence test. |
| AC-482-08 | Issue #482 | 8 | `- [x]` | `[P3-T27]` | `SyncExpandedRegistrations` is exercised directly for both `true` and `false` through `QfcItemControllerTestSupport.InvokeNonPublic` by a named tes... |
| AC-482-09 | Issue #482 | 9 | `- [x]` | `[P3-T28]` | The #482 end-to-end test constructs no `System.Threading.Timer`: `ItemHelper.UnRead` is `false` in the arrangement, established explicitly rather t... |
| AC-482-10 | Issue #482 | 10 | `- [x]` | `[P3-T29]` | The #482 regression test was observed **failing before** the `QfcItemController.Navigation.cs` change (as `ArgumentException` on the third step) an... |
| AC-482-11 | Issue #482 | 11 | `- [ ]` | DEFERRED — `[P5-T26]` | The deliberate behaviour widening — `'B'`/`'D'` responding after a synchronous expansion and Alt+`B`/Alt+`D` after an asynchronous one — is stated ... |
| AC-482-12 | Issue #482 | 12 | `- [ ]` | DEFERRED — `[P5-T27]` | The correction to #482's filed trigger and severity (the filed `QfcCollectionController.cs:1439` trigger is unreachable; the live trigger is Right ... |
| AC-SCOPE-01 | Upstream contract and scope discipline | 1 | `- [x]` | `[P5-T11]` | The `### Upstream contract (exhaustive) — required by features 464 and 489` section of this spec matches the delivered code exactly: every ADDED, C... |
| AC-SCOPE-02 | Upstream contract and scope discipline | 2 | `- [x]` | `[P5-T12]` | `QuickFiler/Controllers/KeyboardHandler.cs` is not modified. |
| AC-SCOPE-03 | Upstream contract and scope discipline | 3 | `- [x]` | `[P5-T13]` | `QuickFiler/Interfaces/IQfcCollectionController.cs` is not modified. |
| AC-SCOPE-04 | Upstream contract and scope discipline | 4 | `- [x]` | `[P5-T14]` | None of the following nine `QfcItemController` partials is modified: `QfcItemController.cs`, `QfcItemController.Conversation.cs`, `QfcItemControlle... |
| AC-SCOPE-05 | Upstream contract and scope discipline | 5 | `- [x]` | `[P5-T15]` | The branch diff's production-file list is a subset of exactly three paths: `QuickFiler/Controllers/KbdActions.cs`, `QuickFiler/Controllers/QfcItemC... |
| AC-SCOPE-06 | Upstream contract and scope discipline | 6 | `- [x]` | `[P5-T16]` | `KbdActions.Remove` retains its `bool` return and its silent `false` for an absent pair, and no `TryRemove`-style member is added. Verified by the ... |
| AC-SCOPE-07 | Upstream contract and scope discipline | 7 | `- [x]` | `[P5-T17]` | No member is added to, removed from, or re-signed on any `public` type, so this feature contributes no public-API change. Verified by the branch di... |
| AC-SCOPE-08 | Upstream contract and scope discipline | 8 | `- [x]` | `[P5-T18]` | Sibling #484's downstream note proposing a timer-factory seam at `QfcItemController.Navigation.cs:223-224` is explicitly declined in this spec, and... |
| AC-SCOPE-09 | Upstream contract and scope discipline | 9 | `- [x]` | `[P5-T19]` | Phase 0 re-derived every `QfcCollectionController.cs` and `QuickFiler.Test.csproj` anchor by member name or element text against the actual branch ... |
| AC-SCOPE-10 | Upstream contract and scope discipline | 10 | `- [x]` | `[P5-T20]` | `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` passes: no `System.Windows.Forms.Form`-derived type was added to the test assembly. |
| AC-SCOPE-11 | Upstream contract and scope discipline | 11 | `- [x]` | `[P5-T21]` | The new test file is registered in `QuickFiler.Test/QuickFiler.Test.csproj` by a single `<Compile Include>` line inserted between the `Controllers\... |
| AC-QA-01 | File-size, toolchain, and coverage | 1 | `- [x]` | `[P4-T14]` | No production or test file **added** by this feature exceeds **500 lines**, and every pre-existing file changed by this feature is either at or bel... |
| AC-QA-02 | File-size, toolchain, and coverage | 2 | `- [x]` | `[P4-T15]` | `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is unchanged by this feature: its line count and its `[TestMethod]` count are identic... |
| AC-QA-03 | File-size, toolchain, and coverage | 3 | `- [x]` | `[P4-T16]` | `dotnet tool run csharpier check .` reports zero unformatted files in the final toolchain pass. |
| AC-QA-04 | File-size, toolchain, and coverage | 4 | `- [x]` | `[P4-T17]` | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` comp... |
| AC-QA-05 | File-size, toolchain, and coverage | 5 | `- [x]` | `[P4-T18]` | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` completes with zero errors in the... |
| AC-QA-06 | File-size, toolchain, and coverage | 6 | `- [x]` | `[P4-T19]` | `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` completes with ... |
| AC-QA-07 | File-size, toolchain, and coverage | 7 | `- [x]` | `[P5-T22]` | All four toolchain steps passed in a single final pass with no step auto-fixing files, and the commands actually run are stated in the completion r... |
| AC-QA-08 | File-size, toolchain, and coverage | 8 | `- [x]` | `[P4-T20]` | `SyncExpandedRegistrations` reaches `>= 90%` line coverage as a new member (`CLAUDE.md` §UT2). Verified from the coverage report produced by the fi... |
| AC-QA-09 | File-size, toolchain, and coverage | 9 | `- [x]` | `[P4-T21]` | The new duplicate-guard branch in `QuickFiler/Controllers/KbdActions.cs` is covered on **both** the throwing and non-throwing paths. Verified from ... |
| AC-QA-10 | File-size, toolchain, and coverage | 10 | `- [x]` | `[P4-T22]` | A Phase 0 coverage baseline was captured and recorded in `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/`, and the ... |
| AC-QA-11 | File-size, toolchain, and coverage | 11 | `- [x]` | `[P5-T23]` | The coverage-policy conflict between `CLAUDE.md` §UT2 (`>= 80%` / `>= 90%`) and `.claude/rules/general-unit-test.md` plus `.claude/rules/quality-ti... |
| AC-QA-12 | File-size, toolchain, and coverage | 12 | `- [x]` | `[P5-T24]` | No acceptance condition in the atomic plan claims a coverage increase attributable to changes in `QuickFiler/Controllers/QfcCollectionController.cs... |
| AC-QA-13 | File-size, toolchain, and coverage | 13 | `- [ ]` | `[P5-T29]` | All evidence artifacts produced by this feature are written under `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/<kind>/` pe... |

## Totals as read from `spec.md`

| Measure | Value |
| --- | --- |
| Total criteria | **57** |
| Checked (`- [x]`) | **53** |
| Unchecked (`- [ ]`) | **4** |
| Checked + unchecked | 53 + 4 = **57** |

## The unchecked set, with the reason for each

| ID | Reason unchecked | Deferral artifact |
| --- | --- | --- |
| AC-472-10 | Conjunctive criterion. The potential entry and GitHub issue #644 both exist; the PR-body clause cannot be satisfied from inside this branch. | `evidence/issue-updates/p5-t25-ac472-10-deferred.2026-08-27T20-16.md` |
| AC-482-11 | Requires a statement in the integration pull-request body, which this preparation run does not create. | `evidence/issue-updates/p5-t26-ac482-11-deferred.2026-08-27T20-16.md` |
| AC-482-12 | Conjunctive criterion. The spec half is satisfied at branch head; the pull-request half is outstanding. | `evidence/issue-updates/p5-t27-ac482-12-deferred.2026-08-27T20-17.md` |
| AC-QA-13 | **Not a deferral.** Unchecked only because `[P5-T29]`, the task that checks it off, runs immediately after this artifact is written. Discharged in the addendum below. | none — see addendum |

**No criterion is unchecked for any other reason.** In particular:

- No criterion was left unchecked on a conditional branch of `[P4-T14]`, `[P4-T19]`, or `[P4-T22]`.
  All three tasks completed on their check-off branch, so none of the three
  `REMEDIATION-REQUIRED` artifacts those branches would have written exists:
  - `[P4-T14]` — clause (a) and clause (b) of AC-QA-01 both hold per `[P4-T3]`; no
    `REMEDIATION-REQUIRED: AC-QA-01` was recorded.
  - `[P4-T19]` — `[P4-T6]` recorded a failed count of `0`, so AC-QA-06 was checked off and no
    `REMEDIATION-REQUIRED: AC-QA-06` was recorded.
  - `[P4-T22]` — `[P4-T11]` recorded both repository-wide deltas at `+0.09` and both changed-file
    line rates not lower than baseline, so AC-QA-10 was checked off and no
    `REMEDIATION-REQUIRED: AC-QA-10` was recorded.
- AC-482-08 is **checked**. `[P3-T27]` recorded it as deferred to Phase 4 by construction and
  `[P4-T20]` checked it off against the `[P4-T9]` measurement of `line-rate = 1`. It is checked in
  every outcome and is not a member of the authorized-branch set.

## Acceptance

- The artifact enumerates 57 rows — met.
- The checked count plus the unchecked count read from `spec.md` equals `57` — met (53 + 4).
- The unchecked set is exactly the three orchestrator-deferred criteria, plus any criterion left
  unchecked on an explicitly authorized conditional branch of `[P4-T14]`, `[P4-T19]`, or `[P4-T22]` —
  met at the terminal state recorded in the addendum below. At the moment of writing the set also
  contains AC-QA-13, whose only reason for being unchecked is that `[P5-T29]` had not yet run; no
  authorized conditional branch was taken by any of the three named tasks.
- Every unchecked row names its deferral artifact — met for the three deferred criteria. AC-QA-13
  names no deferral artifact because it is not deferred; the addendum records its check-off.
- No criterion is unchecked for any other reason — met.

## Addendum: terminal state after [P5-T29] (2026-08-27T20-21)

`[P5-T29]` ran immediately after this artifact was written and checked off AC-QA-13 against the
evidence-location audit `evidence/qa-gates/p5-t29-evidence-locations.2026-08-27T20-20.md`. Re-read
from `spec.md` after that check-off:

| Measure | Value |
| --- | --- |
| Total criteria | **57** |
| Checked (`- [x]`) | **54** |
| Unchecked (`- [ ]`) | **3** |
| Checked + unchecked | 54 + 3 = **57** |

The terminal unchecked set is **exactly** the three orchestrator-deferred criteria:

| ID | Deferral artifact |
| --- | --- |
| AC-472-10 | `evidence/issue-updates/p5-t25-ac472-10-deferred.2026-08-27T20-16.md` |
| AC-482-11 | `evidence/issue-updates/p5-t26-ac482-11-deferred.2026-08-27T20-16.md` |
| AC-482-12 | `evidence/issue-updates/p5-t27-ac482-12-deferred.2026-08-27T20-17.md` |

Row AC-QA-13 in the 57-row table above records `- [ ]`, which was its true on-disk state at the
moment of writing; it is now `- [x]`. The table is left as written rather than back-dated, because an
enumeration artifact should record what it observed. This addendum is the terminal reading.

No criterion was left unchecked on any authorized conditional branch of `[P4-T14]`, `[P4-T19]`, or
`[P4-T22]`, so the terminal unchecked set contains no member from that category and equals the three
deferrals exactly.
