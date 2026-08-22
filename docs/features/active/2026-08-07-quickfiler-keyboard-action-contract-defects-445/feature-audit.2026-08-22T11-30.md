# Feature Audit — quickfiler-keyboard-action-contract-defects (#445)

- **Artifact:** `feature-audit.2026-08-22T11-30.md`
- **Reviewer:** feature-review agent
- **Date:** 2026-08-22T11-30
- **Branch:** `bug/quickfiler-keyboard-action-contract-defects-445-exec` @ `1292b4c3` vs `origin/epic/quickfiler-suite-determinism-foundation-integration` (merge-base `c551eaba`)
- **Work mode:** `full-bug` — AC source is `spec.md` **only** (21 criteria, AC1–AC21). No `user-story.md` exists; its absence is correct by design for this mode.

All 21 criteria were already checked `[x]` by the executor. Per the review protocol, this audit **verifies** each check-off rather than re-checking; no `spec.md` edit was made by this reviewer. Evidence citations name the executor artifact and, where the fact was cheaply re-derivable, the reviewer's independent re-derivation.

## Per-AC Evaluation Table

| AC | Subject | Verdict | Evidence (executor artifact; reviewer re-derivation) |
|---|---|---|---|
| AC1 | Branch-3 gating applied | **PASS** | Reviewer read of current `KaStringAsync.cs`: branch-3 guard reads `if (Activated && Update is not null)`; branch-1 and branch-2 guards unweakened (all four guard sites carry `Activated &&`). Diff shows the single-conjunct edit. Executor: `ac-status-summary.2026-08-22T10-50.md` (guard counts 2/0 vs baseline 1/1). |
| AC2 | Contract documented in-code | **PASS** | Reviewer read: XML doc on `KeyEquals` states the latch contract (side effects gated on `Activated`; matching probe deliberately does not clear the latch; non-matching probe clears it) and the null/empty argument contract with both `<exception>` elements. Executor: `qa-gates/latch-contract-doc.2026-08-22T09-42.md`. |
| AC3 | Anti-regression: early return preserved | **PASS** | Reviewer read: branch 1 still `return true;` before the trailing `Activated = false`. Named witness test `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue` passes unmodified (`regression-testing/green-after-fix.2026-08-22T09-44.md` row 4; `qa-gates/vstest-final.2026-08-22T10-02.md`). Executor: `qa-gates/early-return-preserved.2026-08-22T09-43.md`. |
| AC4 | Null argument rejected explicitly | **PASS** | Reviewer read: `if (other is null) throw new ArgumentNullException(nameof(other));` is the first statement, above `Key.Contains`. Test (d) asserts `ThrowExactly<ArgumentNullException>().WithParameterName("other")`; red run failed on parameter name `value` (library-internal origin), green run passes — proving the guard, not the library, now throws. |
| AC5 | Empty argument rejected explicitly | **PASS** | Reviewer read: `ArgumentException` with `nameof(other)` and a message stating an empty probe would otherwise match every registered action. Test (c) asserts type and parameter name; red→green recorded. |
| AC6 | `ArgumentOutOfRangeException` path closed | **PASS** | Test (c) covers both instance-state variants in one method, including `Activated = true` with non-null `Update` (the variant that previously evaluated `Substring(-1, 1)`); `ThrowExactly` rejects the derived `ArgumentOutOfRangeException`, so a pass proves the exception is exactly `ArgumentException`. Guard precedes all offset arithmetic (reviewer read). |
| AC7 | `DelegateType` removed from both implementers | **PASS** | Reviewer re-derivation: repo-wide grep for `DelegateType` over `*.cs` returns **0 hits** (outside `.claude`); `IKbdAction.cs` has no such member. Executor: `qa-gates/dead-api-removal.2026-08-22T09-47.md` (baseline 3 → 0). |
| AC8 | Dead `Update` removed from four implementers | **PASS** | Reviewer full-file reads of `KaChar.cs` and `KaKey.cs`: no `Update` property or `_update` field on `KaChar`, `KaCharAsync`, `KaKey`, `KaKeyAsync`. |
| AC9 | `Update` retained on `KaStringAsync` | **PASS** | Reviewer read: property + backing field present; five-argument constructor assigns it; read at both guarded call sites in `KeyEquals`. |
| AC10 | Unused `using` removed from `KaChar.cs` only | **PASS** | Diff removes `using System.Windows.Forms;` from `KaChar.cs`; reviewer read confirms `KaKey.cs` line 6 retains it (`Keys` is its key type). |
| AC11 | Commented-out interface members removed | **PASS** | Reviewer read of `IKbdAction.cs` (16 lines): both comment lines gone; diff shows only the two deletions, so the four live members are byte-identical; no implementer signature changed (rebuilds clean). |
| AC12 | Test renamed | **PASS** | Diff shows exactly the method-name line changed for `KeyEquals_MultiCharNonMatch...` → `...MultiCharNonMatchWhileActivated...`; body unchanged. Passes under the new name pre-fix (red-run row 7) and post-fix. |
| AC13 | Defect-1 regression test, red before / green after | **PASS** | `regression-testing/red-before-fix.2026-08-22T09-38.md`: test (a) **Failed** against unmodified production code with captured message showing `Update` fired with `"a"` while `Activated` was false; `regression-testing/green-after-fix.2026-08-22T09-44.md`: **Passed**. Both recorded under `evidence/` as required (note: red run lives under `evidence/regression-testing/`, adjacent to the `evidence/qa-gates/` directory the AC names; the recording requirement is substantively met). |
| AC14 | Latch-survives-transition test added | **PASS** | New test `KeyEquals_LatchSurvivesMatchThenNonMatchTransition_StillResetsToFirstChar` asserts `updates.Equal({"b","a"})`, `toggled == true`, final `Activated == false` — pinning the AC3 behavior. Passes before and after by design (it pins existing behavior), disclosed in the red-run artifact. |
| AC15 | Defect-2 regression tests, red before / green after | **PASS** | Tests (c) and (d) both **Failed** in the red run (rows 11-12, with failure details) and **Passed** in the green run and final suite; recorded under `evidence/`. |
| AC16 | No pre-existing test deleted or weakened | **PASS** | Test-file diff contains only the AC12 rename and four additions. Final suite: `KaStringAsyncTests` 12/12, `KaCharTests` 10, `KaKeyTests` 9, `KbdActionsTests` 3, `KbdActionsRemainingBranchesTests` 10, all passing (`qa-gates/quickfiler-test-suite.2026-08-22T09-50.md`, `qa-gates/vstest-final.2026-08-22T10-02.md`); repo-wide 6441 = 6437 + 4, no test lost. Reviewer re-derivation: `[TestMethod]` count in `KaStringAsyncTests.cs` is 12. |
| AC17 | No test-project file edit | **PASS** | Reviewer re-derivation: `QuickFiler.Test/QuickFiler.Test.csproj` absent from `git diff --name-only` against the merge-base. |
| AC18 | Scope boundaries respected | **PASS** (with recorded interpretive note) | Reviewer re-derivation: `KbdActions.cs`, `KeyboardHandler.cs`, `QfcCollectionController.cs`, and everything under `docs/features/potential/**` are unchanged; the only `.claude/**` changes are three `.claude/agent-memory/atomic-executor/**` files — executor memory persistence carved out by approved plan task P4-T3 and permitted by the review directive. The AC's literal "no file under `.claude/**`" is stricter than the plan and practice; adjudicated PASS on intent (no rule, hook, skill, or policy file touched). See code-review advisory CR-A2. |
| AC19 | Fourth defect not fixed, and filed | **PASS** | Reviewer read: `Key.Substring(other.Length - 1, 1)` and `Key.Contains(other)` retained in branch 1; the `.Be("b")` assertion in the pre-existing prefix test is unchanged. Follow-up issue verified live: **#583** (OPEN, created 2026-08-22, "KaStringAsync.KeyEquals branch 1 computes a prefix-only Substring offset under a Contains guard"), recorded in spec Rollout & Follow-up. Executor: `issue-updates/followup-substring-defect.2026-08-22T10-46.md`. |
| AC20 | File-size limit respected | **PASS** | Reviewer re-derivation (`awk`): 161 / 79 / 80 / 16 / 279 lines — all under 500; the three shrink requirements met (99→79, 99→80, 18→16). Matches `qa-gates/file-size-audit.2026-08-22T09-54.md` exactly. |
| AC21 | Full C# toolchain green, one uninterrupted pass | **PASS** | Stage artifacts in timestamp order: csharpier check 1517 files / 0 needing formatting (09-53); analyzer rebuild exit 0, 0 errors, 5 warnings = baseline, `CoreCompile` skip 0 / start 100 (09-56); nullable rebuild exit 0, 0 errors, skip 0 / start 111 (09-58); vstest 6441/6441, 0 failed, 0 skipped, `/EnableCodeCoverage /InIsolation`, TestCategory filter (10-02); `qa-gates/final-qc-pass-attestation.2026-08-22T10-42.md` attests no stage failed or rewrote files. |

## Baseline-Relative Verification

- **Test delta:** baseline 6437/0/0 → final 6441/0/0; +4 = exactly the four new tests; per-class deltas localize entirely to `KaStringAsyncTests` (8 → 12). The baseline failing set was empty, so the final gate was an absolute zero-failure requirement — met.
- **Coverage delta:** repo-wide line 70.5971% → 70.6037%, branch 58.7406% → 58.7469% (both upward); per-file `KaStringAsync.cs` 49/49 → 60/60 (100%), `KaChar.cs` 28/33 → 28/28, `KaKey.cs` 28/33 → 28/28; new-code line coverage 12/12 = 100%; zero changed-line regression. The repo-wide shortfall against the 80%/85% floors is pre-existing and dispositioned in `policy-audit.2026-08-22T11-30.md` section 5.
- **Warning delta:** 5 → 5 (identical codeless third-party System.Reactive advisories); zero new warnings.

### Acceptance Criteria Status

```
Source: docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/spec.md
Total AC items: 21
Checked off (delivered): 21
Remaining (unchecked): 0
Items remaining: (none)
```

Reviewer verification of the check-off state: 21 lines match `- [x] **AC` and 0 lines match `- [ ] **AC` in `spec.md`; the branch diff to `spec.md` consists solely of the 21 checkbox flips plus the follow-up-issue record (#583) — no criterion text was altered, added, or removed. No reviewer check-off was needed (all items already checked and all verified PASS).

## Overall Determination

**All 21 acceptance criteria PASS. Zero blocking findings. Ready to merge** into `epic/quickfiler-suite-determinism-foundation-integration`, subject to the epic's own integration gates.
