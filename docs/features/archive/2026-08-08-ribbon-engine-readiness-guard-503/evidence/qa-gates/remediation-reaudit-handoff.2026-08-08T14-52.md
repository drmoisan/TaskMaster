# Remediation Cycle 1 — Reaudit Handoff (Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P4-T5]
Post-commit HEAD: **`00bc47bb2d9f82cc4b63b13fbfbd251627e858b1`**
Branch: `bug/ribbon-engine-readiness-guard-503`
Merge-base: `003c5715055d7d1933db68a742531332756e30b2`
Blocking findings entering the reaudit: **0**

## Finding disposition

| Finding | Status | Evidence |
|---|---|---|
| **F1** — vacuous assertion in the AC5 ribbon-XML test | **RESOLVED** | `evidence/regression-testing/f1-assertion-shape.2026-08-08T14-52.md`, `evidence/regression-testing/f1-fail-proof.2026-08-08T14-52.md`, `evidence/regression-testing/f1-mutation-restored.2026-08-08T14-52.md`, `evidence/regression-testing/f1-pass-after-restore.2026-08-08T14-52.md` |
| **F2** — `RibbonExplorer.xml` line growth | **NOT REMEDIABLE AS SPECIFIED — escalated, not fixed** | `evidence/qa-gates/f2-formatter-conflict.2026-08-08T14-52.md`, `evidence/qa-gates/f2-xml-line-count.2026-08-08T14-52.md` (superseded), `evidence/qa-gates/f2-xml-wellformed.2026-08-08T14-52.md`, `evidence/regression-testing/f2-ribbon-xml-tests.2026-08-08T14-52.md` |

### F1 — resolved and proven

The null-conditional operator was removed from `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback`. The attribute is bound to a local and asserted `NotBeNull` before `Value` is dereferenced, so all three required failure conditions reach a real assertion.

Non-vacuity was **demonstrated, not asserted**, by a recorded mutate-build-fail-restore cycle in which the embedded byte content was verified at each step so the proof could not be a stale-assembly false negative:

| Step | Embedded `getEnabled` count | AC5 test | Exit |
|---|---|---|---|
| Green before mutation (P1-T4) | 8 | Passed | 0 |
| **Failing run with the mutation (P1-T7)** | **7** | **Failed** | **1** |
| Restored (P1-T8) | 8 | — | 0 |
| Pass after restore (P1-T10) | 8 | Passed | 0 |

Verbatim failure message from the recorded failing run:

```text
Expected getEnabled not to be <null> because control 'TrainSpam' is engine-backed and must declare a getEnabled callback.
   at FluentAssertions.Primitives.ReferenceTypeAssertions`2.NotBeNull(String because, Object[] becauseArgs)
   at TaskMaster.Test.Ribbon.RibbonExplorerXmlTests.RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback() in ...\RibbonExplorerXmlTests.cs:line 202
```

The permanent tree retains no part of the mutation: `git status --porcelain` is empty and `TaskMaster/Ribbon/RibbonExplorer.xml` is absent from the remediation commit.

### F2 — escalated with measured cause

The pinned edit was made and reverted. CSharpier **1.3.0 formats XML**; `.csharpierignore` excludes `*.csproj`/`*.props`/`*.targets` but not `*.xml`; the default print width is 100 columns. The merge-base single-line form is 78 characters; adding the functionally required `getEnabled="EngineCommand_GetEnabled"` makes it **116 characters**. The formatter therefore mandates the multi-line form, the 12 lines the review called "incidental churn with no functional purpose" are formatter-mandated, and the 527-line target is unreachable while the mandatory format gate must pass.

`RibbonExplorer.xml` remains at **539 lines** and takes a **zero-line diff** from this cycle. All eight `getEnabled="EngineCommand_GetEnabled"` attributes remain present and correct, the resource still parses as valid CustomUI, and the AC5/AC6/AC7/AC8 ribbon-XML tests all pass. Per the executor directive, this newly discovered conflict is recorded for the orchestrator to promote and was **not** fixed; no `.csharpierignore`, `.csharpierrc`, or other gate configuration was modified.

## Acceptance criteria — no state changed

**No acceptance criterion was checked off or unchecked in this cycle.** `git diff -U0 -- .../spec.md | grep -E '^[+-].*- \[[ x]\]'` returns no match, and `git diff --numstat -- .../spec.md` reports `12  0` (twelve added, zero deleted). The only `spec.md` edit is the append-only `### Remediation Cycle 1 — 2026-08-08T14-26` subsection under `## Delivery Notes and Deviations`.

**AC19, AC20, and AC21 remain unchecked**, each still beginning `- [ ]`, verified verbatim in `evidence/qa-gates/manual-only-unchecked.2026-08-08T14-52.md`. They are MANUAL-ONLY and require live-Outlook verification; the maintainer checklist at `evidence/manual-verification/ac19-ac21-checklist.2026-08-08T15-00.md` is unchanged and still carries `Status: PENDING MAINTAINER EXECUTION`.

## Out-of-scope items were not touched

None of the following was addressed, re-promoted, or re-litigated:

- Issue **#512** — pre-existing repository-wide nullable debt and the vacuous type-check gate (restated in `evidence/qa-gates/msbuild-nullable.2026-08-08T14-52.md`, not remediated).
- Issue **#510** — the `CS2002` duplicate `<Compile Include>` entry in `UtilitiesCS.Test.csproj` (observed in every analyzer build, not fixed).
- Issue **#508** — the `YieldAsync_WithoutDispatcher_RemainsStrict` order-dependent flake (passed in both the baseline and final runs; no fix made or attempted).
- The residual `engine as SpamBayes` / `.Engine` dereference window.
- The `??=` lazy-initialiser thread-safety observation in `RibbonController.EngineCommands.cs`.
- `TaskMaster\AppGlobals\AppItemEngines.cs` and `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs` — both verified at a zero-line diff against the merge-base, before and after the commit.
- Issues **#504**, **#505**, **#506**, **#507**, **#509**, **#511**.

## Toolchain state at handoff

| Gate | Result |
|---|---|
| CSharpier format (scope-locked) | exit 0, no rewrite |
| CSharpier check (repo-wide) | exit 0 over 1498 files, empty unformatted set |
| MSBuild analyzers | exit 0, 0 errors, 6 warnings all matching the P0-T9 baseline |
| MSBuild nullable | exit 0 (with the #512 limitation restated) |
| Tests with coverage | exit 0, **6338/6338 passed**, 0 failed, 0 skipped |
| First-party LINE coverage | 85.8462% to **85.8561%** (up) |
| First-party BRANCH coverage | 79.2559% to **79.2702%** (up) |
| `TaskMaster` package LINE counter | `missed=1464 covered=3515` at all three measurement points |

All five gate commands ran in one uninterrupted pass with no restart; the earlier aborted attempt and its cause are disclosed in `evidence/qa-gates/toolchain-clean-pass.2026-08-08T14-52.md`.

## New defect recorded for promotion, not fixed

**Plan section 3 rule 6 is factually wrong for this toolchain**, and the F2 finding rests on that error. CSharpier 1.3.0 formats XML and enforces its 100-column print width on `RibbonExplorer.xml`. Recommended orchestrator actions: close F2 as not remediable as specified; correct the rule text in future plans; and, if the 500-line overage is to be addressed at all, route a resource-split issue rather than a reformatting change. Full detail in `evidence/qa-gates/f2-formatter-conflict.2026-08-08T14-52.md`.

## Trailing evidence commit

The plan places the commit task (P4-T3) **before** the post-commit verification tasks (P4-T4, P4-T5), so those two tasks necessarily produce artifacts after `00bc47bb` exists. `evidence/qa-gates/remediation-commit.2026-08-08T14-52.md`, `evidence/qa-gates/zero-line-diff-postcommit.2026-08-08T14-52.md`, this artifact, and the final Phase 4 plan checklist state are therefore committed in a **second, documentation-and-evidence-only** commit immediately following `00bc47bb`, so the worktree ends clean.

That trailing commit contains **no source path**. The remediation commit `00bc47bb` remains the commit whose diff the P4-T4 scope audit was taken over, and the protected-path verification recorded there is unaffected: neither commit touches `TaskMaster/AppGlobals/AppItemEngines.cs`, `UtilitiesCS/Interfaces/IGlobals/IAppItemEngines.cs`, or `TaskMaster/AppGlobals/ApplicationGlobals.cs`.

## Binary outcome

Both findings carry at least one evidence pointer. F1 is marked **resolved**. F2 is marked **not remediable as specified** with its measured cause, rather than reported as resolved or silently dropped. Blocking-finding count entering the reaudit: **0**.
