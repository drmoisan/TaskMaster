# [DoNotParallelize] additive-only diff (P5-T3)

Timestamp: 2026-09-02T23-30

Command: `git diff --unified=0 $base -- 'UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs' 'UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs'`

EXIT_CODE: 0

BaseRef re-derivation (D11): `git merge-base origin/main HEAD` returned
`8be5a6aac3b5a82c86241fbbf989fd9118602c56`, which equals the `BaseRef:` recorded by P0-T14 in
`base-ref.2026-09-02T10-30.md`. The diff below is anchored to that ref and therefore covers both
committed and uncommitted state at this point in the plan.

## Full diff

```diff
diff --git a/UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs b/UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs
index f987c549..c28d715a 100644
--- a/UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs
+++ b/UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs
@@ -7,0 +8,7 @@ namespace UtilitiesCS.Test.OutlookObjects.FilterDASL
+    // PrintTree_WritesIndentedTreeToConsole captures and restores Console.Out, which is
+    // process-wide state. Under the class-level parallel scope declared by the Parallelize
+    // attribute at UtilitiesCS.Test/Properties/AssemblyInfo.cs lines 18-21, a sibling test
+    // class's Console.SetOut overrides this class's redirect mid-test and makes the captured
+    // output empty. The assembly attribute, not TaskMaster.runsettings, is what takes effect:
+    // the CI vstest invocation passes no /Settings: argument.
+    [DoNotParallelize]
diff --git a/UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs b/UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs
index 7b73c245..caae5708 100644
--- a/UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs
+++ b/UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs
@@ -8,0 +9,7 @@ namespace UtilitiesCS.Test.ReusableTypeClasses
+    // Main_RunsSampleScenarioWithoutThrowing captures and restores Console.Out, which is
+    // process-wide state. Under the class-level parallel scope declared by the Parallelize
+    // attribute at UtilitiesCS.Test/Properties/AssemblyInfo.cs lines 18-21, a sibling test
+    // class's Console.SetOut overrides this class's redirect mid-test and makes the captured
+    // output empty. The assembly attribute, not TaskMaster.runsettings, is what takes effect:
+    // the CI vstest invocation passes no /Settings: argument.
+    [DoNotParallelize]
```

Output Summary:

- Added lines: 14. Removed lines: 0.
- Every added line is either a `//` comment line or the `[DoNotParallelize]` attribute line. No
  test body, no assertion, and no test-method name is changed in either file, which is what AC13
  requires.
- Both hunk headers are pure insertions (`@@ -7,0 +8,7 @@` and `@@ -8,0 +9,7 @@`), confirming
  no existing line was rewritten.
