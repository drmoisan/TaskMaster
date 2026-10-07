# P8-T1 Pre-Merge Facts

Timestamp: 2026-10-06T17-52
Command: git status --porcelain --untracked-files=all; git fetch origin main; git rev-parse HEAD; git rev-parse origin/main; git merge-base HEAD origin/main; git merge-base --is-ancestor origin/main HEAD; git diff --numstat 94287369908cc920b21b0e3256314f988ad7d2f5 origin/main -- QuickFiler.Test/QuickFiler.Test.csproj; git diff 94287369908cc920b21b0e3256314f988ad7d2f5 origin/main -- QuickFiler.Test/QuickFiler.Test.csproj; git diff --name-only 94287369908cc920b21b0e3256314f988ad7d2f5 origin/main (each issued as one git -C <worktree> invocation)
EXIT_CODE: 0 (scoped to `git fetch origin main`; equal to FETCH-EXIT)
ITERATION: 1
Output Summary: authorized; worktree clean; fetch succeeded; merge base is the Phase 0 to 7 anchor; origin/main is not contained in HEAD (a merge is required); main adds three Compile Include lines to QuickFiler.Test/QuickFiler.Test.csproj and removes none; QuickFiler.Test/QuickFiler.Test.csproj is the only Write Set path main touched; main touched no UtilitiesCS.Test/EmailIntelligence/ file and no SortEmail_ file.

AUTHORIZATION: PHASE 7 RE-REVIEW: PASS (orchestrator authorization for Phase 8). Re-review artifacts: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/{policy-audit,code-review,feature-audit}.2026-10-06T17-40.md, committed at 061784a95; zero blocking findings.

CLEAN-BEFORE-MERGE: 0
FETCH-EXIT: 0
HEAD-AT-P8-T1: 061784a958a03fe43102aa60738c90be58725a7d
ORIGIN-MAIN-SHA: f8ea1b5dcc6514bc0088bc80965c188bfd717557
MERGE-BASE-BEFORE: 94287369908cc920b21b0e3256314f988ad7d2f5
MAIN-IS-ANCESTOR-EXIT: 1
MAIN-QFT-NUMSTAT: 3	0	QuickFiler.Test/QuickFiler.Test.csproj

MAIN-QFT-HUNK:

```
diff --git a/QuickFiler.Test/QuickFiler.Test.csproj b/QuickFiler.Test/QuickFiler.Test.csproj
index 9fd57ab0b..9022b82f3 100644
--- a/QuickFiler.Test/QuickFiler.Test.csproj
+++ b/QuickFiler.Test/QuickFiler.Test.csproj
@@ -201,6 +201,7 @@
     <Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixture.cs" />
     <Compile Include="Controllers\QfcItemController.SeamMarshallingTests.cs" />
     <Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs" />
+    <Compile Include="Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs" />
     <Compile Include="Controllers\QfcItemController.InitializationTests.cs" />
     <Compile Include="Controllers\QfcItemController.InitializationTests.Part2.cs" />
     <Compile Include="Controllers\QfcItemController.InitializationTests.Part3.cs" />
@@ -226,6 +227,8 @@
     <Compile Include="Controllers\QfcQueueTests.cs" />
     <Compile Include="TestSupport\WinFormsPumpHost.cs" />
     <Compile Include="TestSupport\DedicatedWorkerThread.cs" />
+    <Compile Include="TestSupport\SynchronousBackgroundWorker.cs" />
+    <Compile Include="TestSupport\ArmingFakeTimeProvider.cs" />
     <Compile Include="TestSupport\WinFormsPumpHostTests.cs" />
     <Compile Include="NoLiveFormInTestAssemblyTests.cs" />
     <Compile Include="Helper Classes\ConversationResolverTests.cs" />
```

MAIN-QFT-HUNK-PLUS-LINES: 3 (exactly the three PD-17 lines; no additional `+` line)
MAIN-QFT-HUNK-MINUS-LINES: 0 (no `-` line other than the `---` header)

MAIN-CHANGED-PATHS: 280
MAIN-TOUCHED-WRITE-SET:
- QuickFiler.Test/QuickFiler.Test.csproj

MAIN-TOUCHED-TEST-DIRS:
- QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs
- QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs
- QuickFiler.Test/Controllers/QfcDatamodelTests.cs
- QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs
- QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs
- QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs
- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs

(No row under UtilitiesCS.Test/EmailIntelligence/; no SortEmail_ file; EfcDataModelArchiveRootTests.cs not named.)

## Acceptance

1. AUTHORIZATION quotes the line: PASS
2. CLEAN-BEFORE-MERGE: 0: PASS
3. FETCH-EXIT: 0: PASS
4. MERGE-BASE-BEFORE equals 94287369908cc920b21b0e3256314f988ad7d2f5: PASS
5. MAIN-IS-ANCESTOR-EXIT: 1: PASS
6. MAIN-QFT-HUNK carries the three expected `+` lines and no `-` line: PASS
7. MAIN-TOUCHED-WRITE-SET lists QuickFiler.Test/QuickFiler.Test.csproj (no other member): PASS
