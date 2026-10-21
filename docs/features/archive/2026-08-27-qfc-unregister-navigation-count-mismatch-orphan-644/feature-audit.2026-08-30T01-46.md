# Feature Audit — Issue #644 (cycle-exit reaudit)

- **Timestamp:** 2026-08-30T01-46
- **Branch:** `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- **Head:** `85a1939f92f64ebada4e71d19cc095dc2e8e8a26`
- **Base:** `main` at `fa2ddefacf2c08abe18f3e3250d77da804534637` (merge base, verified)
- **Work mode:** `full-bug` (from the `- Work Mode: full-bug` marker in `issue.md`)
- **AC source:** `spec.md` only — `full-bug` resolves to `spec.md` and no `user-story.md` exists or
  is to be created
- **Blocking findings:** **0**

## Verdict summary

| Verdict | Count |
|---|---|
| PASS | 17 |
| PARTIAL | 1 (AC-16) |
| FAIL | 0 |
| **Total** | **18** (AC-0 through AC-17) |

Every verdict below was re-derived in this session. None is inherited from the cycle-entry audit at
`2026-08-29T23-06`, including AC-16, which was re-adjudicated from its underlying facts.

## Acceptance criteria evaluation

| AC | Subject | Verdict | Primary evidence re-derived this session |
|---|---|---|---|
| AC-0 | Phase 0 baselines | PASS | base blobs re-read: production 2437, char-test 500/13 |
| AC-1 | Red before green | PASS | `[P1-T4]` red record; T1 `Passed` in the exit TRX |
| AC-2 | `RemoveBelowThresholdAsync` path | PASS | T1 `outcome="Passed"` |
| AC-3 | `RemoveSpecificControlGroup(int)` path | PASS | T2 `outcome="Passed"` |
| AC-4 | Width-crossing path | PASS | T6 `outcome="Passed"` |
| AC-5 | State transitions | PASS | T3 `outcome="Passed"` |
| AC-6 | Empty-ledger negative case | PASS | T4 `outcome="Passed"` |
| AC-7 | `UnregisterNavigation` no longer reads `_itemGroups` | PASS | T5 `outcome="Passed"`; production diff |
| AC-8 | New test file exists and is compiled | PASS | file present, `Compile Include` added, 6 tests in TRX |
| AC-9 | Amended characterisation tests pass | PASS | all four named tests `Passed` |
| AC-10 | Frozen-file constraints hold | PASS | 499 lines, 13 `[TestMethod]` |
| AC-11 | Digits-file assertion flipped and passing | PASS | assertion, because-string, doc block, 3 `[TestMethod]`, 3 passes |
| AC-12 | `_registeredDigits` fully removed, no CS0414 | PASS | zero occurrences repo-wide; nullable build exit 0 |
| AC-13 | Comment synchronisation, no assertion drift | PASS | diff is comment and string-literal only; test `Passed` |
| AC-14 | Footprint containment | PASS | six code paths; production net +9 |
| AC-15 | Full toolchain pass | PASS | csharpier re-run clean; TRX 1254/1254 |
| **AC-16** | **No coverage regression on changed lines** | **PARTIAL** | **see the dedicated section below** |
| AC-17 | Evidence location | PASS | 58 artifacts, all canonical; zero non-canonical paths in diff |

## Detail for each criterion

**AC-0 — PASS.** The four baseline figures were re-derived against the base blob rather than read
from the artifact. `git show fa2ddefa:QuickFiler/Controllers/QfcCollectionController.cs | awk 'END{print NR}'`
returns 2437, matching the expected value. `git show fa2ddefa:QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs | awk 'END{print NR}'`
returns 500, matching. The `[TestMethod]` counts and the digits-file figures are recorded in
`evidence/baseline/p0-t7-counts.2026-08-29T08-15.md`, and the coverage baseline in
`evidence/baseline/p0-t12-coverage-baseline.2026-08-29T08-15.md` (0.853303, 54800/64221). Line counts
were measured with `awk 'END{print NR}'` rather than `Measure-Object -Line`, which undercounts.

**AC-1 — PASS.** `evidence/regression-testing/p1-t4-expect-fail.2026-08-29T08-15.md` records T1
failing against unmodified production code, before the fix was applied.
`evidence/regression-testing/p2-t5-ledger-green.2026-08-29T08-15.md` records it green after. T1
carries `outcome="Passed"` in this cycle's TRX. Red-before-green is established by artifact.

**AC-2 through AC-7 — PASS.** All six regression tests carry `outcome="Passed"`, read from the TRX by
name rather than from console text:

| AC | Test | Outcome |
|---|---|---|
| AC-2 | `UnregisterNavigation_AfterGroupRemovedThroughRemoveGroupByEntryIdSeam_RemovesEveryRegisteredKey` | Passed |
| AC-3 | `UnregisterNavigation_AfterUnbracketedItemGroupsRemoval_ThenReRegister_DoesNotThrow` | Passed |
| AC-4 | `UnregisterNavigation_AfterTwoDigitRegistrationAndShrinkToNine_LeavesNoCollectionKeys` | Passed |
| AC-5 | `RegisterAndUnregisterNavigation_RepeatedCycles_LeaveRegistryEmpty` | Passed |
| AC-6 | `UnregisterNavigation_WithNoPriorRegistration_DoesNotThrowAndLeavesRegistryUnchanged` | Passed |
| AC-7 | `UnregisterNavigation_AfterItemGroupsSetToNull_DoesNotThrow` | Passed |

AC-7's structural claim was additionally verified against the production diff: `UnregisterNavigation`
contains no `_itemGroups` reference after the change, and the `RegisteredNavigationKeys.Clear()` call
satisfies the "drains the ledger" clause.

**AC-8 — PASS.** `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs` exists
(361 lines, 6 `[TestMethod]`). The csproj diff adds exactly one line:

```
+    <Compile Include="Controllers\QfcCollectionControllerNavigationLedgerTests.cs" />
```

All six tests appear as executed results in the TRX, which is the criterion's stated proof that the
csproj entry took effect.

**AC-9 — PASS.** All four named tests carry `outcome="Passed"`:
`LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix`,
`LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys`,
`SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey`, and the
unchanged `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException`. The
amendments are arrangement-only — three seeding calls replaced by `controller.RegisterNavigation();`
— with no assertion edited.

**AC-10 — PASS.** Re-measured: `QfcCollectionControllerTests.cs` is 499 lines (at or under 500) and
contains exactly 13 `[TestMethod]` attributes, matching the frozen baseline of 13. The file shrank by
one line.

**AC-11 — PASS.** All four clauses re-verified by reading the file:

1. Asserts an empty `"Collection"` key set — line 181-186, `remaining.Should().BeEmpty(...)`.
2. The `because:` string names #644 — "issue #644 replaced the count-bounded removal loop with a
   ledger that replays the recorded registration set verbatim, so no key survives unregistration".
3. Its XML documentation records the residual as closed rather than out of scope — lines 147-151,
   "The single residual `"10"` entry this test used to pin is now closed by issue #644".
4. The file contains exactly 3 `[TestMethod]` attributes and all three tests pass.

All four hold, so AC-11 is PASS. Recorded for completeness: CR-2 and CR-6 both sit in this file and
are *not* AC-11 clause failures. AC-11 requires the residual be recorded as closed, which it is; it
does not require every sentence in the surrounding doc block to be re-synchronized, and neither CR-2
nor CR-6 touches the empty-set assertion or its because-string. They are policy findings under
`CLAUDE.md` C#6.3, recorded in the code review, not AC gaps.

**AC-12 — PASS.** `grep -rn "_registeredDigits"` across the repository returns zero occurrences. The
nullable build recorded in `evidence/qa-gates/p2-t4-nullable-build.2026-08-29T23-23.md` exits 0 with
no CS0414 diagnostic.

**AC-13 — PASS.** The diff for `QfcCollectionControllerDefects468Tests.cs` was read line by line and
contains only XML documentation lines, one `because:` string literal, and one inline comment. No
assertion expression changed. The re-attribution is substantively correct: with `UnregisterNavigation`
no longer reading `_itemGroups`, the `NullReferenceException` now originates at
`_itemGroups[selection - 1]` inside `RemoveSpecificControlGroupAsync`, which is what the amended text
says. `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter` carries
`outcome="Passed"`.

**AC-14 — PASS.** The criterion's substance is that no production file is added, `QuickFiler.csproj`
is unchanged, no interface file is touched, and the production diff is a net addition of no more than
10 lines confined to the field block and the three named members. All four hold:
`git diff --numstat` shows `18 9` for the production file, a net of +9; the diff hunks are confined
to the private field block, `RegisterNavigation`, `UnregisterNavigation`, and
`RegisterNavigationAsyncAction`; no production file is added; `QuickFiler.csproj` does not appear.

A wording note, not a gap: the criterion names base commit `ecdb1c84`, which the review anchor
supersedes, and says the diff "lists only the seven paths enumerated in the Blast Radius section".
The literal seven-path enumeration does not include the 58 evidence artifacts and the audit
documents, which the plan itself mandates be written under the feature folder. The property the
clause guards — that nothing outside the code footprint and this feature folder is touched — was
verified independently against the resolved base and holds:
`git diff --name-only fa2ddefa...HEAD` filtered for paths outside `QuickFiler*` and this feature
folder returns nothing. This is the same divergence recorded as PA-6 in the policy audit and is
non-blocking there.

**AC-15 — PASS.** The four commands ran in one uninterrupted pass with no file rewritten. Two were
re-run or re-read by this audit rather than accepted on report: `dotnet tool run csharpier check .`
exits 0 with `Checked 1562 files in 4658ms` and no unformatted file, and the TRX `Counters` element
reads `total="1254" passed="1254" failed="0"`. The two msbuild gates are taken from
`evidence/qa-gates/p2-t3-analyzer-build.2026-08-29T23-23.md` and
`evidence/qa-gates/p2-t4-nullable-build.2026-08-29T23-23.md`, corroborated by the compile proof set
out in section 3 of the policy audit: the built test assembly postdates the edited source and the
test run postdates the assembly, so the passing results were produced by code containing the
remediation.

**AC-17 — PASS.** All 58 evidence artifacts live under
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/` in
six kind-named subfolders with ISO-8601 timestamps.
`git diff --name-only fa2ddefa...HEAD | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"`
returns no output, so nothing was written to a prohibited location.

## AC-16 — independent re-adjudication

> **AC-16 (no coverage regression on changed lines).** The repository coverage figure from the AC-15
> step-4 run is greater than or equal to the AC-0 baseline. Changed production lines live in a
> `[ExcludeFromCodeCoverage]` class and are therefore outside the denominator; that fact is stated
> explicitly in the coverage evidence artifact so the gate is not read as vacuously satisfied.

**Verdict: PARTIAL. AC-16 remains unchecked in `spec.md`.**

This verdict was reached from the underlying facts, not inherited. It happens to agree with the
cycle-entry audit; the reasoning below is this audit's own.

### Clause 2 — PASS

The `[ExcludeFromCodeCoverage]` fact is stated explicitly, at length, in
`evidence/qa-gates/p4-t6-coverage-final.2026-08-29T08-15.md`, under its own heading "The changed
production file is absent from all 558 class entries". The attribute was independently confirmed on
line 21 of `QuickFiler/Controllers/QfcCollectionController.cs`. This clause is unambiguously
satisfied, and the gate is therefore not read as vacuously satisfied.

### Clause 1 — not established, and not establishable by the named instrument

Two independent reasons, either sufficient on its own.

**First: the instrument the criterion names produces no such figure.** Clause 1 requires "the
repository coverage figure from the AC-15 step-4 run". The AC-15 step-4 command is
`vstest.console.exe ... /EnableCodeCoverage /InIsolation /Logger:trx ...`. That command emits a TRX
and a binary `.coverage` attachment; it prints no repository-wide percentage and writes no Cobertura
document. The figure the clause compares therefore cannot come from the run the clause names. Every
figure in evidence comes from a substitute instrument,
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`. Substitution is reasonable, but it means the clause
as written was never evaluated against its own stated source.

**Second: the substitute instrument cannot decide the comparison at the resolution demanded.**

| Run | Source state | `lines-covered` | `lines-valid` | Rate | vs baseline |
|---|---|---|---|---|---|
| A — baseline | pre-change | 54800 | 64221 | 0.853303 | — |
| E | final | 54793 | 64221 | 0.853194 | **-0.0109 pts** |
| F | final, byte-identical to E | 54811 | 64221 | 0.853475 | **+0.0172 pts** |

E and F measured the same tree with the same command on the same machine and differ by 18 covered
lines. The E-to-F spread is 0.0281 percentage points; the disputed shortfall at run E is 0.0109
points. The instrument's measured noise is roughly **2.6 times** the delta the clause is asked to
adjudicate, and the two runs **straddle the baseline** — one returns FAIL, the other PASS, on
identical source.

A comparison whose outcome is determined by which of two equally valid runs happens to be taken has
not established the property it was written to establish. Recording PASS would assert a fact the
evidence does not support. Recording FAIL would be equally unsupported, since run F clears the bar.
**PARTIAL is the only verdict the evidence carries.**

I explicitly decline to check AC-16 off on the strength of the recorded orchestrator authorization.
An override is a decision to proceed; it is not a measurement, and it cannot convert an undecidable
comparison into a satisfied one.

### What is nonetheless established

The substantive property AC-16 exists to protect — that this change does not cost the repository
coverage — is well supported, independently of clause 1:

- `lines-valid` is invariant at **64221** across all three runs, so the denominator did not move and
  the entire variation is in the numerator.
- The single changed production file carries `[ExcludeFromCodeCoverage]` (verified on line 21) and
  was confirmed absent from **all 558 `<class>` entries** of the post-processed document. No line
  this change edits sits in either the numerator or the denominator, so the change cannot move the
  figure through that file in either direction.
- Both post-change figures, 85.3194% and 85.3475%, clear the 85% floor, and the recorded branch rate
  of 79.29% clears the 75% floor. `54793 / 64221` and `54811 / 64221` reproduce the recorded rates,
  so the figures are arithmetically self-consistent.
- The whole-repository test result is clean at 6876 of 6876, and the touched assembly is clean at
  1254 of 1254. No test regressed.
- This remediation cycle changed **no production file and no coverage figure**. Confirmed:
  `git diff a2c69aea..85a1939f --name-only -- QuickFiler/Controllers/QfcCollectionController.cs`
  returns empty, and the prior cycle's coverage artifact is byte-identical by SHA-256.

### Classification and recommendation

**Non-blocking.** The gap is an instrument-resolution limitation, not a coverage regression and not a
defect in the change. It does not warrant reopening the cycle: re-running the same harness would
produce a third number without resolving anything, and the executor was correct to escalate rather
than re-run until a run cleared the bar.

Recommendation for the repository, not for this pull request: a no-regression gate stated to two
decimal places needs an instrument whose noise floor is below the threshold it enforces. Either
tighten the harness or restate the criterion as a tolerance band wider than the measured noise.
Recorded so the same undecidable comparison is not re-litigated on the next change.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/spec.md
- Total AC items: 18 (AC-0 through AC-17)
- Checked off (delivered): 17
- Remaining (unchecked): 1
- Items remaining: AC-16 (no coverage regression on changed lines)
```

`spec.md` was **not modified** by this audit. AC-16 stays `- [ ]` because this audit's verdict is
PARTIAL, not PASS. No other checkbox changed state; `spec.md` still carries 21 checked and 5
unchecked checkbox lines in total, of which 18 are AC items and the other eight are bug-report
template fields.

## Original defect: is it actually fixed?

Assessed against `issue.md` and the spec Context, independently of the AC list.

The reported defect was that `UnregisterNavigation` bounded its loop with the live `_itemGroups.Count`
while `RemoveSpecificControlGroup(int)` mutated `_itemGroups` outside any unregister/register
bracket, so unregistration stopped short and orphaned `KbdActions` registrations.

**Fixed, and fixed at the right level.** `UnregisterNavigation` no longer reads `_itemGroups` at all;
it replays a recorded ledger. The defect is not merely guarded against on the two known paths — the
dependency that produced it is removed, so a future third unbracketed mutation path cannot
reintroduce it. Both reported reachability paths are covered by dedicated tests: the
`RemoveBelowThresholdAsync` route through the `_removeGroupByEntryId` seam (T1) and the direct
unbracketed `_itemGroups` removal reached by the `'R'` char action and `PopOutControlGroup` (T2). The
expected behavior in `issue.md` — "a subsequent registration succeeds" — is pinned by T2, which
re-registers and asserts no `ArgumentException`.

The fix also strengthens #472 rather than undoing it: the residual `"10"` entry that #472's test
previously pinned as an accepted orphan is now removed, and that test's assertion was flipped from
`Equal(new[] { "10" })` to `BeEmpty()`.

## Blocking findings

**Total blocking findings: 0.**

No acceptance criterion is FAIL. The single PARTIAL is non-blocking for the reasons set out above.
No remediation-inputs artifact is produced.
