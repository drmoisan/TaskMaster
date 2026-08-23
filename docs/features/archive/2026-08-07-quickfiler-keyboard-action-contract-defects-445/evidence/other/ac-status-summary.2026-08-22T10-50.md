# Phase 6 — Acceptance-Criteria Status Summary (Issue #445)

Timestamp: 2026-08-22T10-50

Command:
```bash
grep -c '^- \[x\] \*\*AC' docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/spec.md
grep -c '^- \[ \] \*\*AC' docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/spec.md
```
Run from `WS`.

EXIT_CODE: 0

---

## Acceptance Criteria Status

```
Source: docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/spec.md
Total AC items: 21
Checked off (delivered): 21
Remaining (unchecked): 0
Items remaining: (none)
```

Work mode is `full-bug`, so `spec.md` is the **sole** acceptance-criteria source. No `user-story.md` exists and none was created. Only `- [ ]` was changed to `- [x]`; no criterion text was altered and no criterion was added or removed. The counts above were measured on disk after the edits, not asserted: 21 lines match `^- \[x\] \*\*AC` and 0 lines match `^- \[ \] \*\*AC`.

---

## Per-criterion verdict and evidence

| AC | Subject | Verdict | Evidence |
|---|---|---|---|
| AC1 | Branch-3 gating applied | **PASS** | P2-T2: `if (Activated && Update is not null)` count 2 (baseline 1); `if (Update is not null)` count 0 (baseline 1). No other guard weakened or reordered. |
| AC2 | Contract documented in-code | **PASS** | `evidence/qa-gates/latch-contract-doc.2026-08-22T09-42.md`; `latch` 6 (baseline 0), `///` 49 (baseline 0). All three mandated clauses present; prohibited unqualified claim absent. |
| AC3 | Anti-regression, early return preserved | **PASS** | `evidence/qa-gates/early-return-preserved.2026-08-22T09-43.md`; `return true;` exactly 1 and `Activated = false` exactly 1, both at baseline. `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue` passed unmodified in P2-T6. |
| AC4 | Null argument rejected explicitly | **PASS** | P2-T1: `ArgumentNullException(nameof(other))` count 1. `KeyEquals_NullProbe_ThrowsArgumentNullExceptionNamingOther` Failed pre-fix (parameter name `value`), Passed post-fix (`other`). |
| AC5 | Empty argument rejected explicitly | **PASS** | P2-T1: `nameof(other)` count 2. `KeyEquals_EmptyProbe_ThrowsArgumentExceptionNamingOther` Passed in P2-T6. Message documents that an empty probe would otherwise match every registered action. |
| AC6 | `ArgumentOutOfRangeException` path closed | **PASS** | P2-T6: the two-variant empty-probe test Passed on both variants. `ThrowExactly<ArgumentException>` rejects the derived `ArgumentOutOfRangeException`, so passing proves the exception is now exactly `ArgumentException`. |
| AC7 | `DelegateType` removed from both implementers | **PASS** | `evidence/qa-gates/dead-api-removal.2026-08-22T09-47.md`; repository-wide `DelegateType` over `*.cs` is 0 (baseline 3). No `DelegateType` member added to `IKbdAction.cs` (count 0). |
| AC8 | Dead `Update` removed from four implementers | **PASS** | P3-T4 and P3-T7: `_update` is 0 in `KaChar.cs` and 0 in `KaKey.cs` (baseline 6 each). Repo-wide `public Action<string> Update` fell from 5 to 1. |
| AC9 | `Update` retained on `KaStringAsync` | **PASS** | P3-T9: `_update` in `KaStringAsync.cs` still exactly 3; `Update = update;` still 1 in the five-argument constructor; read sites in `KeyEquals` intact. |
| AC10 | Unused `using` removed from `KaChar.cs` only | **PASS** | P3-T2 and P3-T5: `using System.Windows.Forms;` is 0 in `KaChar.cs` and 1 in `KaKey.cs`. `Keys` is 0 in `KaChar.cs`, confirming the directive became unused there. |
| AC11 | Commented-out interface members removed | **PASS** | P3-T8 counts, plus the `IKbdAction.cs` diff containing two deleted comment lines and **no added line**, so the four live members are byte-identical. P3-T11 clean compile proves no implementer signature changed. |
| AC12 | Test renamed | **PASS** | P1-T1: old name count 0, new name count 1. Body unchanged; the test passed pre-fix (P1-T9) and post-fix (P2-T6). |
| AC13 | Defect-1 regression test, red before, green after | **PASS** | `evidence/regression-testing/red-before-fix.2026-08-22T09-38.md` (Failed, captured `{"a"}`) and `green-after-fix.2026-08-22T09-44.md` (Passed). |
| AC14 | Latch-survives-transition test added | **PASS** | P2-T6: `KeyEquals_LatchSurvivesMatchThenNonMatchTransition_StillResetsToFirstChar` Passed, asserting the `"b"` then `"a"` sequence. |
| AC15 | Defect-2 regression tests, red before, green after | **PASS** | Both regression-testing artifacts. The P6-T16 contingency did NOT apply: the null-probe test is recorded as **Failed** before the fix (`Expected exception with parameter name "other", but found "value"`), not Passed, so both defect-2 tests have genuine red-before witnesses. |
| AC16 | No pre-existing test deleted or weakened | **PASS** | `evidence/qa-gates/quickfiler-test-suite.2026-08-22T09-50.md`; 907/907 passed, `KaCharTests` 10, `KaKeyTests` 9, `KbdActionsTests` 3, `KbdActionsRemainingBranchesTests` 10, each equal to baseline, and each of those four files reports 0 `git status` lines. |
| AC17 | No test-project file edit | **PASS** | P4-T1: `git status --porcelain` and `git diff --name-only` both report 0 lines for `QuickFiler.Test/QuickFiler.Test.csproj`. |
| AC18 | Scope boundaries respected | **PASS** | P4-T2 and P4-T3: 0 lines for `KbdActions.cs`, `KeyboardHandler.cs`, `QfcCollectionController.cs`; 0 for `docs/features/potential`; 0 for `.claude` excluding `agent-memory`. The unscoped `.claude` status was additionally empty. |
| AC19 | Fourth defect not fixed, and filed | **PASS** | P4-T4 retention counts all 1 (`Key.Substring(other.Length - 1, 1)`, `Key.Contains(other)`, `Be("b"`). Issue **#583** filed: https://github.com/drmoisan/TaskMaster/issues/583, recorded in the spec's Rollout & Follow-up. |
| AC20 | File-size limit respected | **PASS** | `evidence/qa-gates/file-size-audit.2026-08-22T09-54.md`; all five files below 500 (161, 79, 80, 16, 279) and the three required to shrink did: 99→79, 99→80, 18→16. |
| AC21 | Full C# toolchain green in one uninterrupted pass | **PASS** | `csharpier-check` (1517 checked, 0 needing format, exit 0), `msbuild-analyzers` (0 errors, 5 warnings = baseline, skip count 0), `msbuild-nullable` (0 errors, skip count 0), `vstest-final` (6441/6441 passed, 0 failed), `final-qc-pass-attestation` (P5-T1 rewrote 0 files, no stage failed). |

---

## Notes on the two conditional criteria

Two acceptance criteria carried explicit contingencies in the plan that would have required leaving them unchecked. **Neither contingency was triggered.**

**AC15 (P6-T16 contingency).** The plan instructs leaving AC15 unchecked "if the P1-T9 artifact records the null-probe test as Passed before the fix rather than Failed". The P1-T9 artifact records it as **Failed**, with the verbatim message `Expected exception with parameter name "other", but found "value"`. The pre-fix exception type was already `ArgumentNullException` (the `ThrowExactly` type assertion passed); the parameter name was the discriminator, and it was the only red-before lever available for the null case, exactly as research section 4.3 predicted. AC15 is therefore checked on a genuine red-before/green-after pair.

**AC21 (P6-T22 contingency).** The plan instructs leaving AC21 unchecked if the P5-T6 Failed count is non-zero even when the failing set is a subset of the baseline. The P5-T6 **Failed count is 0**, so the contingency does not arise. All four stage exit codes are 0, the `Skipping target "CoreCompile"` count is 0 in both MSBuild logs, and the P5-T6 gate passed. There are no surviving pre-existing failures to name.

---

## Blocking coverage gates

Both blocking coverage gates recorded in `evidence/qa-gates/coverage-delta.2026-08-22T10-40.md` **PASS**:

- Newly-added production line coverage: **100.00%** (12 of 12 added executable lines have hits >= 1), against a `>= 90%` requirement.
- No changed line covered at baseline is uncovered after the change: **no regression** on any of the four production files.

The repository-wide figures (line 70.60%, branch 58.75%) remain below both threshold sets (CLAUDE.md UT2's 80%, and 85%/75% from `general-unit-test.md` and `quality-tiers.md`). That shortfall was measured before any edit in this plan, is pre-existing and unadjudicated, is explicitly not resolved by this issue, and is not a blocking gate for this bugfix. Both rates moved marginally upward as a result of this change.

---

## Scope of this plan

The plan ends here. **No pull-request, PR-body, CI-monitoring, or merge task is in scope**, and none was performed.

Output Summary: The acceptance-criteria status block is `Source:` `docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/spec.md`, `Total AC items: 21`, `Checked off (delivered): 21`, `Remaining (unchecked): 0`, `Items remaining:` none. All 21 criteria AC1 through AC21 were individually evaluated against named evidence artifacts and all 21 PASS; the per-criterion verdict table above cites the artifact establishing each. Counts were measured on disk after editing (21 lines match the checked pattern, 0 match the unchecked pattern), not asserted. Only `- [ ]` was changed to `- [x]`; no criterion text was altered and none was added or removed. Neither of the two plan-defined contingencies triggered: the P6-T16 AC15 contingency did not apply because the null-probe test is recorded as Failed before the fix, and the P6-T22 AC21 contingency did not apply because the P5-T6 Failed count is 0. Both blocking coverage gates pass at 100% newly-added-line coverage and no changed-line regression. The follow-up issue required by AC19 is **#583**. The plan's scope ends at this summary; no pull request was opened, no PR body authored, no CI monitored, and nothing merged.
