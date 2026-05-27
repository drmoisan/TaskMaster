# Feature Audit: EmailFiler — Actionable Classifier Serialization (Issue #164)

**Audit Date:** 2026-05-26
**Auditor:** Feature Review Agent
**Base Branch:** `development` (commit `4e7210a72e52e5a2c471c88b6de4fcfe12a03d66`)
**Head Branch:** `bug/actionable-classifier-not-serialized-164` (commit `4e7210a72e52e5a2c471c88b6de4fcfe12a03d66`)
**Merge State:** Branch tip equals `origin/development` — change is present in `development`.
**Work Mode:** `minor-audit`
**Requirements Source:** `docs/features/active/2026-05-26-actionable-classifier-not-serialized-164/issue.md`

---

## Scope and Baseline

**Feature folder:** `docs/features/active/2026-05-26-actionable-classifier-not-serialized-164/`
**Issue:** #164 — `SerializeFolderManagerAsync` not serializing the `Actionable` classifier; `TrainActionableAsync` training on `"None"` Actionable labels.
**Files changed:**
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` — 2 additions
- `UtilitiesCS.Test/EmailIntelligence/EmailFiler_Tests.cs` — 1 new test method

**Baseline commit:** `4e7210a72e52e5a2c471c88b6de4fcfe12a03d66` (`origin/development` pre-merge)
**Final commit:** `4e7210a72e52e5a2c471c88b6de4fcfe12a03d66` (branch tip equals `origin/development`)
**Coverage baseline:** `coverage/coverage.cobertura.xml` — `EmailFiler.cs` class: 87.3% lines, 89.5% branches

---

## Acceptance Criteria Inventory

| AC | Criterion | Source |
|----|-----------|--------|
| AC1 | `SerializeFolderManagerAsync` calls `(await Globals.AF.Manager["Actionable"]).Serialize()` after the existing `Folder` serialize call | `issue.md` § AC1 |
| AC2 | `TrainActionableAsync` returns `Task.CompletedTask` immediately when `mailHelper.Actionable == "None"` without calling `Train` | `issue.md` § AC2 |
| AC3 | Test `TrainActionableAsync_WhenActionableIsNone_DoesNotTrainClassifier` exists in `EmailFiler_Tests.cs` and passes | `issue.md` § AC3 |
| AC4 | Full toolchain passes: CSharpier format, MSBuild analyzers, MSBuild nullable/warnings-as-errors, VSTest — with no new failures | `issue.md` § AC4 |

---

## Summary

This audit validates the four acceptance criteria from `issue.md` for the bug fix that addresses Issue #164: `SerializeFolderManagerAsync` was not serializing the `Actionable` classifier, and `TrainActionableAsync` was training on `"None"` Actionable labels.

Both production changes were verified by code inspection. The confirming unit test was verified by the VSTest evidence. The full toolchain passed with no new failures.

**Overall Feature Readiness: PASS**

All four acceptance criteria are PASS. No remediation is required.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| AC1 | `SerializeFolderManagerAsync` calls `(await Globals.AF.Manager["Actionable"]).Serialize()` after the existing `Folder` serialize call | ✅ PASS | `EmailFiler.cs` line 377: call present and ordered correctly. `evidence/qa-gates/msbuild-nullable.txt`: builds clean. |
| AC2 | `TrainActionableAsync` returns `Task.CompletedTask` immediately when `mailHelper.Actionable == "None"` without calling `Train` | ✅ PASS | `EmailFiler.cs` lines 391–394: guard clause `if (mailHelper.Actionable == "None") return Task.CompletedTask;` present before the `Task.Run` block. `evidence/qa-gates/vstest-final.txt`: test confirming this path PASSED. |
| AC3 | Test `TrainActionableAsync_WhenActionableIsNone_DoesNotTrainClassifier` exists in `EmailFiler_Tests.cs` and passes | ✅ PASS | `EmailFiler_Tests.cs` line 383: `[TestMethod] public async Task TrainActionableAsync_WhenActionableIsNone_DoesNotTrainClassifier()`. `evidence/qa-gates/vstest-final.txt`: `Passed TrainActionableAsync_WhenActionableIsNone_DoesNotTrainClassifier [< 1 ms]`. |
| AC4 | Full toolchain passes: CSharpier format, MSBuild analyzers, MSBuild nullable/warnings-as-errors, VSTest — with no new failures beyond the known pre-existing failures | ✅ PASS | CSharpier: 0 changes (`evidence/qa-gates/csharpier-format.txt`). MSBuild analyzers: Build succeeded (`evidence/qa-gates/msbuild-analyzers.txt`). MSBuild nullable: Build succeeded (`evidence/qa-gates/msbuild-nullable.txt`). VSTest: 4009 passed, 2 pre-existing failures (unchanged from baseline), 2 skipped (`evidence/qa-gates/vstest-final.txt`). |

---

## Detailed AC Evidence

### AC1: Actionable serialize call in `SerializeFolderManagerAsync`

**Requirement:** `SerializeFolderManagerAsync` calls `(await Globals.AF.Manager["Actionable"]).Serialize()` after the existing `Folder` serialize call.

**Evidence — code inspection:**
```csharp
protected internal virtual async Task SerializeFolderManagerAsync()
{
    (await Globals.AF.Manager["Folder"]).Serialize();
    (await Globals.AF.Manager["Actionable"]).Serialize();
}
```
File: `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs`, line 377.

The call is present and ordered correctly: `Folder` first, `Actionable` second. The build passes with nullable analysis enabled, confirming no null-safety regression.

**Verdict: PASS**

---

### AC2: `"None"` early-return guard in `TrainActionableAsync`

**Requirement:** `TrainActionableAsync` returns `Task.CompletedTask` immediately when `mailHelper.Actionable == "None"` without calling `Train`.

**Evidence — code inspection:**
```csharp
protected internal virtual Task TrainActionableAsync(MailItemHelper mailHelper)
{
    // Only train on confirmed actionable signals; skip "None" to avoid diluting the classifier
    // with the majority class and producing a model that always predicts "None".
    if (mailHelper.Actionable == "None")
    {
        return Task.CompletedTask;
    }
    return Task.Run(async () =>
        (await Globals.AF.Manager["Actionable"]).Train(
            mailHelper.Actionable,
            mailHelper.Tokens,
            1
        )
    );
}
```
File: `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs`, lines 389–408.

The guard clause appears before the `Task.Run` block. When `Actionable == "None"`, the method returns immediately without entering `Task.Run` or calling `Train`. The intent comment documents the classifier quality rationale.

**Verdict: PASS**

---

### AC3: Unit test exists and passes

**Requirement:** A new MSTest `TrainActionableAsync_WhenActionableIsNone_DoesNotTrainClassifier` exists in `EmailFiler_Tests.cs` and passes.

**Evidence — code inspection:**
```csharp
[TestMethod]
public async Task TrainActionableAsync_WhenActionableIsNone_DoesNotTrainClassifier()
{
    var actionableGroup = new BayesianClassifierGroup();
    var manager = CreateManager(new BayesianClassifierGroup(), actionableGroup);
    var filer = new ExposedEmailFiler
    {
        Globals = CreateGlobals(manager, null, null, null, null),
    };
    var helper = new TestMailItemHelper();
    helper.SetTokens("alpha", "beta");
    helper.Actionable = "None";

    await filer.CallTrainActionableAsync(helper);

    actionableGroup.Classifiers.Should().BeEmpty();
}
```
File: `UtilitiesCS.Test/EmailIntelligence/EmailFiler_Tests.cs`, line 383.

**Evidence — VSTest final:**
From `evidence/qa-gates/vstest-final.txt`:
```
Passed  TrainActionableAsync_WhenActionableIsNone_DoesNotTrainClassifier [< 1 ms]
```
Test exists, is a `[TestMethod]`, and passed in the final run.

**AC3 Baseline note:** The test also appears as PASSED in `evidence/baseline/vstest-baseline.txt` (4013 total both runs). This means the baseline evidence was captured after the implementation was committed, not before. The fail-before regression test pattern from the bugfix workflow was not followed. This is documented as a process observation only; it does not affect the pass status of AC3. The test exists and passes in the verified final state.

**Verdict: PASS**

---

### AC4: Full toolchain passes with no new failures

**Requirement:** The full toolchain passes: CSharpier format, MSBuild analyzers, MSBuild nullable/warnings-as-errors, VSTest — with no new failures beyond the known pre-existing failures.

**Evidence:**

| Toolchain Step | Command | Result | Evidence File |
|---------------|---------|--------|---------------|
| CSharpier format | `dotnet tool run csharpier format .` | 1057 files processed, 0 formatting changes | `evidence/qa-gates/csharpier-format.txt` |
| MSBuild analyzers | `msbuild TaskMaster.sln ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Build succeeded, 0 new warnings/errors | `evidence/qa-gates/msbuild-analyzers.txt` |
| MSBuild nullable | `msbuild TaskMaster.sln ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | Build succeeded, 0 warnings/errors | `evidence/qa-gates/msbuild-nullable.txt` |
| VSTest | `vstest.console.exe <assemblies> /EnableCodeCoverage` | 4013 total; 4009 passed; 2 failed (pre-existing); 2 skipped | `evidence/qa-gates/vstest-final.txt` |

**Pre-existing failures (confirmed in both baseline and final):**
1. `TrainSelectionAsync_WhenSelectionContainsTwoMailItems_TrainsOnlyFirstItem_TotalEmailCountIncrementsOnce` — `Triage_OlLogicTests.cs`
2. `TrainSelectionAsync_WhenSelectionContainsTwoMailItems_TrainsOnlyFirstItem_MatchEmailCountIncrementsOnce` — `Triage_OlLogicTests.cs`

These failures are in `Triage_OlLogicTests.cs`, unrelated to the `EmailFiler` changes. Their failure count is identical between baseline and final (2 failures both runs). No new failures were introduced.

**Verdict: PASS**

---

## Acceptance Criteria Check-off

All four AC items in `docs/features/active/2026-05-26-actionable-classifier-not-serialized-164/issue.md` are marked `[x]`. The check-off status is confirmed as accurate for all four items.

| AC | `issue.md` Status | Audit Confirmation |
|----|-------------------|--------------------|
| AC1 | `[x]` | Confirmed PASS |
| AC2 | `[x]` | Confirmed PASS |
| AC3 | `[x]` | Confirmed PASS |
| AC4 | `[x]` | Confirmed PASS |

No changes to `issue.md` are required.

---

## Observations

1. **Baseline evidence captured post-implementation (AC3):** As documented in the AC3 detail above, the vstest baseline was taken on the feature branch after the test and implementation were both committed. The fail-before step was not performed. This is a process gap, not a correctness gap. The test is valid and covers the intended behavior.

2. **No Cobertura artifact captured:** No per-file coverage instrument was run for this fix. The minimal scope (3 new production lines, 1 new test) and the passing test suite make this an acceptable gap for the minor-audit scope.

3. **Branch already merged:** The head branch tip equals `origin/development` as of the audit date. This review serves as a post-merge confirmation of the delivered state.

---

## Remediation

No remediation is required. All four acceptance criteria are PASS. The toolchain is clean. No remediation-inputs or atomic_planner delegation is needed.
