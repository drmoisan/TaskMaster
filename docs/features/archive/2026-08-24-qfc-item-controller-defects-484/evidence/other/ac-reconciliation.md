# Acceptance-criteria reconciliation

Timestamp: 2026-08-26T14-20
Task: [P8-T13]

AC source (work mode `full-bug`): `docs/features/active/qfc-item-controller-defects-484/spec.md`.
`user-story.md` is intentionally absent and its absence is not a blocker.

## Counts

Command:

```
grep -c '^- \[' docs/features/active/qfc-item-controller-defects-484/spec.md
grep -c '^- \[x\]' docs/features/active/qfc-item-controller-defects-484/spec.md
grep -c '^- \[ \]' docs/features/active/qfc-item-controller-defects-484/spec.md
```

EXIT_CODE: 0

| Metric | Value |
|---|---|
| Acceptance-criterion checkboxes | **50** |
| Checked `- [x]` | **50** |
| Unchecked `- [ ]` | **0** |

## No criterion text was modified

Command:

```
git diff --stat 61edc19befcf6c4e95b5acd32542f2dcdab41b78 -- docs/features/active/qfc-item-controller-defects-484/spec.md
```

EXIT_CODE: 0

```
 .../active/qfc-item-controller-defects-484/spec.md | 100 ++++++++++-----------
 1 file changed, 50 insertions(+), 50 deletions(-)
```

Exactly 50 lines changed, each a `- [ ]` to `- [x]` flip. Filtering the diff to lines that are neither a
removed `- [ ]` line nor an added `- [x]` line returns no output, which establishes that no criterion
text was altered, added, or removed.

## Outcome

**Pass: 50 of 50 checked.** The authorized `[P8-T4]` exception branch does **not** apply, because the
`[P0-T9]` baseline unformatted-file set was empty and `[P7-T2]` also reported an empty set with
`EXIT_CODE: 0`; the csharpier criterion is therefore checked unconditionally on its own evidence rather
than left unchecked under the exception. No criterion is left unchecked, so this task's outcome is not
remediation-required.

## Recorded divergences between `spec.md` prose and the delivered measurements

Two criteria carry a descriptive sub-clause whose figure the delivered state does not match. In both
cases the criterion's binding requirement is satisfied and evidenced; only a projection embedded in the
prose diverged. `spec.md` text is unmodifiable under this task, so both divergences are recorded here
and in the cited evidence artifact rather than corrected in place.

### D-1 — File-size criterion, projected per-file distribution

Verbatim sub-clause: "the two owned test files that receive no added lines,
`QfcItemController.FocusAndThemeTests.cs` and `QfcItemController.ViewerSetupTests.cs`, are verified at
their unchanged 497 and 474 lines."

Delivered: `QfcItemController.FocusAndThemeTests.cs` is unchanged at 497 as stated;
`QfcItemController.ViewerSetupTests.cs` is **498**, because it received the #484
`Cleanup_NullsMailActions_AndSaveParametersRebindsIt` test under an authorized constraint C2 capacity
rule 3 relocation. The relocation was forced: the planned home
`QfcItemController.MailActionsTests.cs` entered Phase 4 at 459 lines rather than its 184-line C2
baseline, leaving 41 lines of headroom, of which `[P4-T1]` and `[P4-T2]` consumed 39.

Binding requirement — "Every production and test file touched by this feature is at most 500 lines after
the change. All nine owned files are recorded with their post-change line counts" — **satisfied**; see
`docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/file-sizes-final.md`, which
records all nine values (maximum 499) and carries the same divergence note.

### D-2 — Coverage criterion, expected zero rate for the notifier default

Verbatim sub-clause: the default `MoveFailureNotifier` delegate `text => MessageBox.Show(text)` "so its
measured line rate is zero."

Delivered: the measured line rate is **1 (100 percent)**, and the source line
`QuickFiler/Controllers/QfcItemController.MailActions.cs:31` records `hits="1"`. The reason is a
measurement artefact, not a behavioural one: the lambda is a single-line property initializer, so
constructing the delegate registers a hit on the same source line that holds its body. The body itself
is never invoked by any test, exactly as the criterion describes, because every `MoveMailAsync`
failure-path test replaces the notifier through the seam.

Binding requirement — every new production member reaches at least 90 percent line coverage except the
three named carve-outs — **satisfied**; all five named members measure 100 percent, and the delivered
state is more favourable than the prose predicted. The relocation still reduces no changed line's
coverage: the baseline Cobertura records `hits="0"` on each of `MailActions.cs:119`, `:120`, and `:121`
at `<BASE_SHA>`. See
`docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/coverage-delta.md`, which
records the measured value verbatim.

## Per-issue evidence index

| Issue | Fail-before artifact | Pass-after artifact |
|---|---|---|
| #480 | `480-sync-tightened-fail.md`, `480-async-fail.md` | `480-pass.md` |
| #481 | `481-empty-bodies-fail.md`, `481-unguarded-fail.md`, `fail-before-exception.webresourcerequested-detach.md` | `481-pass.md` |
| #483 | `483-fail.md` | `483-pass.md` |
| #484 | `484-fail.md` | `484-pass.md` |
| #485 | `485-fail.md` | `485-pass.md` |

All paths are under
`docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/`.

Output Summary: 50 of 50 acceptance criteria checked, zero remaining, and no criterion text modified.
Outcome is a pass. Two descriptive sub-clause divergences (D-1 file-size projection, D-2 notifier
coverage expectation) are recorded above and in their cited evidence artifacts; neither affects a
binding requirement.
