# Feature Audit — Issue #644 (remediation cycle 2, exit reaudit)

- **Timestamp:** 2026-08-30T13-10
- **Branch:** `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- **Head:** `4572fef5`
- **Base:** `main` at `fa2ddefacf2c08abe18f3e3250d77da804534637`
- **Work mode:** `full-bug` (marker at `issue.md` line 14)
- **Authoritative AC source:** `spec.md` only. No `user-story.md` exists for this feature and none is
  required; the `acceptance-criteria-tracking` protocol resolves `full-bug` to `spec.md` alone.

## Baseline

The baseline for this audit is the resolved merge base `fa2ddefa`. Span citations use the recorded
`diff_anchor_substitution` anchor `e968a1a8`; the two produce identical numstats over `QuickFiler`
and `QuickFiler.Test`, so the choice does not affect any verdict below.

Phase 0 baselines re-derived in this session and compared against the values `spec.md` expected:

| Figure | Spec expected | Measured at base | Measured at head |
|---|---|---|---|
| `QfcCollectionControllerTests.cs` lines | 500 | 500 | 499 |
| `QfcCollectionControllerTests.cs` `[TestMethod]` | 13 | 13 | 13 |
| `QfcCollectionControllerNavigationDigitsTests.cs` lines | 226 | 226 | 226 |
| `QfcCollectionControllerNavigationDigitsTests.cs` `[TestMethod]` | 3 | 3 | 3 |
| `QfcCollectionController.cs` lines | 2437 | 2437 | 2446 |

Every expected figure is confirmed by measurement. No discrepancy.

## Acceptance criteria evaluation

18 criteria, AC-0 through AC-17.

| AC | Subject | Verdict | How verified in this session |
|---|---|---|---|
| AC-0 | Phase 0 baselines re-derived and recorded | PASS | Baseline table above re-derived with `awk 'END{print NR}'` and `grep -c` at base and head; evidence `baseline/p0-t7-counts` and `baseline/p0-t12-coverage-baseline` |
| AC-1 | Red before green for T1 | PASS | Recorded failing at `[P1-T4]` against unmodified production code; passing at `[P2-T5]` and in this audit's own run |
| AC-2 | `RemoveBelowThresholdAsync` path leaves no key | PASS | T1 re-run by name: `UnregisterNavigation_AfterGroupRemovedThroughRemoveGroupByEntryIdSeam_RemovesEveryRegisteredKey` — Passed |
| AC-3 | Unbracketed `_itemGroups` removal, then re-register, no throw | PASS | T2 re-run: `UnregisterNavigation_AfterUnbracketedItemGroupsRemoval_ThenReRegister_DoesNotThrow` — Passed |
| AC-4 | Width-crossing path leaves no residual | PASS | T6 re-run: `UnregisterNavigation_AfterTwoDigitRegistrationAndShrinkToNine_LeavesNoCollectionKeys` — Passed |
| AC-5 | Repeated register/unregister cycles | PASS | T3 re-run: `RegisterAndUnregisterNavigation_RepeatedCycles_LeaveRegistryEmpty` — Passed |
| AC-6 | Empty-ledger negative case | PASS | T4 re-run: `UnregisterNavigation_WithNoPriorRegistration_DoesNotThrowAndLeavesRegistryUnchanged` — Passed |
| AC-7 | `UnregisterNavigation` no longer reads `_itemGroups` | PASS | T5 re-run: `UnregisterNavigation_AfterItemGroupsSetToNull_DoesNotThrow` — Passed; corroborated by reading the member, which references only the ledger |
| AC-8 | New test file exists and is compiled | PASS | File present at 361 lines; `<Compile Include="Controllers\QfcCollectionControllerNavigationLedgerTests.cs" />` present in the csproj; all six tests appear as executed results in this audit's run |
| AC-9 | Amended characterisation tests pass | PASS | Four tests re-run by name, all Passed, including the `*Key 2 SourceId Collection*` reported-repro test and the unchanged duplicate-registration test |
| AC-10 | Frozen-file constraints hold | PASS | 499 lines (base 500) and exactly 13 `[TestMethod]`, both re-measured |
| AC-11 | Digits-file assertion flipped and passing | PASS | Assertion is `BeEmpty` with a `because:` naming issue #644 at lines 183-186; the `<summary>` at 147-151 records the residual as closed by #644; 3 `[TestMethod]`; all three tests in the file Passed |
| AC-12 | `_registeredDigits` fully removed, no CS0414 | PASS | `git grep "_registeredDigits" -- '*.cs'` returns zero occurrences; the warnings-as-errors rebuild exits 0 with no CS0414, executed at this head and recorded in `evidence/other/resume-toolchain-verification.2026-08-30T13-20.md` |
| AC-13 | Comment synchronisation with no assertion drift | PASS | The `Defects468` diff is XML doc, one `because:` string, and one inline comment; no assertion edit; `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter` Passed |
| AC-14 | Footprint containment | PASS, with a recorded divergence | Substantive clauses hold exactly: one production file modified, none added, `QuickFiler.csproj` unchanged, no interface file touched, production net +9 lines inside the stated bound of 10. The literal enumeration clause diverges — see the note below and PA-9 |
| AC-15 | Full toolchain pass, one uninterrupted run | PASS | All four gates executed at this exact head by the resuming orchestrator with `/t:Rebuild` on both msbuild gates; format and test gates independently re-run here |
| AC-16 | No coverage regression on changed lines | **PARTIAL** | Left unchecked. Adjudication and accepted residual risk below |
| AC-17 | Evidence location | PASS | `git diff --name-only` scan finds no path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`; all evidence sits under the feature folder's `evidence/` tree in six kind-named subfolders with ISO-8601 timestamps |

### Note on AC-14

The criterion's substantive clauses — no production file added, `QuickFiler/QuickFiler.csproj`
unchanged, no interface file touched, and a production net addition of no more than 10 lines confined
to the private field block and the three navigation members — all hold, measured this session.

The literal enumeration clause ("lists only the seven paths enumerated in the Blast Radius section")
does not hold, and did not hold at either predecessor audit either: roughly 85 feature-folder process
artifacts sit outside the seven, and the branch tip adds 15 `.claude/agent-memory/**` paths on top of
them. `spec.md` explicitly carves out feature-folder process artifacts from the code diff; it does not
mention agent memory, because it predates the checkpoint commit that added it.

AC-14 is evaluated PASS on the property it exists to guard, and the divergence is recorded as PA-9 in
the policy audit at this timestamp rather than being left implicit. It is not a real gap in delivery:
no code path, build input, or product behaviour is affected.

### AC-16 — adjudicated PARTIAL, left unchecked

**Verdict: PARTIAL.** Not PASS, not FAIL. The criterion remains `- [ ]` in `spec.md` at line 707.

**Second clause — holds.** The changed production lines live in an `[ExcludeFromCodeCoverage]` class
and that fact is stated explicitly in the coverage evidence, which is exactly what the clause
requires.

**First clause — undecidable at the instrument's measured resolution.** The clause asks for a `>=`
decision at two decimal places. Two runs of a byte-identical tree, same command, same machine, no
intervening edit of any kind, produced 54793 and 54811 covered lines against an identical
`lines-valid` of 64221 — 85.3194% and 85.3475%. They **straddle** the 85.3303% baseline. The
instrument's root-level noise is therefore about 0.03 percentage points, roughly three times the
0.011-point shortfall the gate was asked to adjudicate. A comparison whose sign is determined by which
of two equally valid measurements of an identical tree happens to be taken is not adjudicating the
property it was written to adjudicate.

Selecting the favourable run is expressly rejected as a basis: an executor free to choose the run it
is judged against cannot fail.

**Independent corroboration, and its exact limit.** The sole changed production file,
`QuickFiler/Controllers/QfcCollectionController.cs`, carries `[ExcludeFromCodeCoverage]` and appears
in 0 of the 558 class entries of the post-processed document, so it sits in neither the numerator nor
the denominator. The invariant `lines-valid` of 64221 across an +18/-9 edit is the mechanical
confirmation. This corroborates that the change cannot move the figure *through the excluded file*; it
does not extend to the figure as a whole, because exclusion removes the type's own lines rather than
its effect on measured collaborators.

**Non-circularity, re-verified here rather than accepted.** The attribute was not added or widened by
this change:

```
$ git show e968a1a8:QuickFiler/Controllers/QfcCollectionController.cs | grep -n "ExcludeFromCodeCoverage"
21:    [ExcludeFromCodeCoverage]
```

It is present at the anchor, and the production diff's hunks begin at lines 117, 1175 and 1183, none
of them near line 21.

**Accepted residual risk, stated explicitly.** A real regression smaller than the roughly 0.03-point
measured noise floor would be indistinguishable from noise by this gate. That risk is accepted for
merge. It is bounded by two facts: the absolute floors are decidable at this resolution and both
final-state runs clear them (85.32% and 85.35% against the 85% line floor; 79.29% against the 75%
branch floor), and the fix's proof rests on six regression tests that fail against unmodified
production code, not on the coverage figure.

**Disposition.** PARTIAL, left unchecked, residual risk judged acceptable for merge. Carried forward
unchanged from the two predecessor audits. No coverage measurement was re-run by this audit, and this
PARTIAL is **not** a blocking finding.

**Follow-up worth filing separately:** reformulate the repository coverage no-regression gate either
with an explicit tolerance above the measured noise floor, or as a same-session per-file changed-lines
comparison, so a criterion of this shape becomes decidable in future.

## Behavioural verification against the reported defect

The spec's Expected behaviour is: "Unregistration removes exactly the set of navigation keys that
registration added, regardless of any `_itemGroups` mutation that occurred in between. A subsequent
registration succeeds, and a navigation keypress resolves against exactly one handler."

All three reaches named in the root-cause analysis are covered by a passing test that fails against
the unmodified code:

| Reach | Test | Pre-fix outcome |
|---|---|---|
| `RemoveBelowThresholdAsync` via the `RemoveGroupByEntryId` seam | T1 | leaves `"10"` |
| `RemoveSpecificControlGroup(int)` — synchronous `'R'` char action and `PopOutControlGroup(int)` | T2 | `ArgumentException` on re-registration |
| Width crossing at the 9/10 boundary | T6 | leaves `"10"` |

The "exactly one handler" half of the expected behaviour is asserted directly by T2's
`OnlyHaveUniqueItems` check. The post-`Cleanup` latent failure mode is closed by T5, which is also the
regression guard the spec's Risks section names against a future change reintroducing an
`_itemGroups`-derived bound.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/spec.md
- Total AC items: 18
- Checked off (delivered): 17
- Remaining (unchecked): 1
- Items remaining: AC-16 (no coverage regression on changed lines) — evaluated PARTIAL; the
  comparison clause is undecidable at the instrument's measured noise floor and the residual risk is
  accepted for merge
```

No acceptance criterion was newly checked off or unchecked by this audit. AC-16 is deliberately left
unchecked, consistent with the recorded adjudication and with both predecessor audits.

## Verdict

**17 PASS, 1 PARTIAL, 0 FAIL. 0 blocking findings.**

The delivered change satisfies the bug's expected behaviour, closes all three unbracketed reaches
named in the root-cause analysis, respects the #468 freeze on the characterisation file, and passes
the full toolchain at the branch tip. The single PARTIAL is adjudicated, accepted, and not blocking.

**Recommendation: GO** — open the pull request.
