# QA gate — #468 defects file changed in comments and string literals only ([P4-T9])

- Issue: #644
- Task: `[P4-T9]`
- Timestamp: 2026-08-29T08-15

## Diff anchor substitution (recorded local_execution_override: `diff_anchor_substitution`)

- Plan's literal anchor: `ecdb1c84ba8541ab67042985919cfed4df768c01`
- Substituted anchor actually run: `e968a1a8804b7641380d4489c496662824d45767`

Rationale, as authorized by the parent orchestrator: this run merged the current `origin/main` tip
into the feature branch before execution, and `e968a1a8804b7641380d4489c496662824d45767` is that
merge commit, i.e. the true pre-change state of this run. The plan's literal anchor predates the
merged fix for issue #638, so anchoring there would list every path that fix brought in, which this
task's acceptance clauses were not written to admit. The substitution narrows the diff to this
change; it is not a widening of any acceptance clause.

## Commands and results

Command: `git diff e968a1a8804b7641380d4489c496662824d45767 -- QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs`
EXIT_CODE: 0

```diff
diff --git a/QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs b/QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs
index 84abaa83..4c7bbc27 100644
--- a/QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs
+++ b/QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs
@@ -151,13 +151,15 @@ namespace QuickFiler.Controllers.Tests

         /// <summary>
         /// Issue #286. The reentrancy counter must be restored when
-        /// <c>RemoveSpecificControlGroupAsync</c> throws at the very first statement after the
+        /// <c>RemoveSpecificControlGroupAsync</c> throws early in its body, just after the
         /// <c>Interlocked.Increment</c>. An uninitialized controller leaves <c>_itemGroups</c>
-        /// <c>null</c>, so <c>UnregisterNavigation()</c> raises
-        /// <see cref="NullReferenceException"/> there. Expected outcome: the exception propagates
-        /// and the private static counter is back at its pre-call value. Before the fix the
-        /// decrement was the method's last statement and unreachable after a throw, leaking the
-        /// counter for the life of the process.
+        /// <c>null</c>; since issue #644 replaced the count-bounded unregister loop with a key
+        /// ledger, <c>UnregisterNavigation()</c> no longer reads that field and completes, so the
+        /// <see cref="NullReferenceException"/> now originates one statement later, at the
+        /// <c>_itemGroups[selection - 1]</c> dereference inside
+        /// <c>RemoveSpecificControlGroupAsync</c>. Expected outcome is unchanged: the exception
+        /// propagates and the counter is back at its pre-call value. Before the fix the decrement
+        /// was the method's last statement and unreachable after a throw, leaking the counter.
         /// </summary>
         [TestMethod]
         public async Task RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter()
@@ -173,8 +175,8 @@ namespace QuickFiler.Controllers.Tests
             // Assert
             await act.Should()
                 .ThrowAsync<NullReferenceException>(
-                    because: "UnregisterNavigation() is the first statement after the increment and "
-                        + "it dereferences the null _itemGroups field"
+                    because: "the null _itemGroups field is dereferenced at _itemGroups[selection - 1] "
+                        + "inside RemoveSpecificControlGroupAsync, so the decrement must run on that path"
                 );
             ReadReentrancyCounter()
                 .Should()
@@ -200,8 +202,10 @@ namespace QuickFiler.Controllers.Tests
             QfcCollectionController controller =
                 QfcCollectionControllerTestSupport.CreateUninitializedController();

-            // A real (empty) KbdActions instance rather than a mock: UnregisterNavigation calls
-            // Remove(...) on it directly, and it must succeed so the throw lands later in the body.
+            // A real (empty) KbdActions instance rather than a mock. Since issue #644 replaced the
+            // count-bounded loop with a ledger, UnregisterNavigation iterates an empty ledger here
+            // and calls Remove zero times; the real instance is retained so the arrangement stays
+            // valid, not because UnregisterNavigation still calls Remove on it.
             Mock<IQfcKeyboardHandler> keyboardHandler = new Mock<IQfcKeyboardHandler>(
                 MockBehavior.Loose
             );
```

Command: `git status --porcelain -- QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs`
EXIT_CODE: 0

```
(no output)
```

The porcelain companion is empty because the edit was committed by an earlier segment of this
resumed run. The anchored diff above is therefore the complete record of the change to this file.

## Mechanical verification of the acceptance clauses

The four token and shape clauses were evaluated by filtering the diff to its added and removed
lines only — every line beginning with `+` or `-`, excluding the `+++` and `---` file headers — and
counting over that filtered set rather than by reading. Measured values:

- Changed lines in the diff: 24 (14 added, 10 removed).
- Occurrences of the token `Should()` among changed lines: **0**.
- Occurrences of the token `ThrowAsync` among changed lines: **0**.
- Occurrences of the token `[TestMethod]` among changed lines: **0**.

All three tokens appear in the hunks only as unchanged context lines (`await act.Should()`,
`.ThrowAsync<NullReferenceException>(`, and the `[TestMethod]` attribute), which the clause permits
because it constrains added and removed lines only.

Added-line shape. Filtering the 14 added lines to those that are neither an XML documentation line
beginning with `///` nor a `//` comment line leaves exactly **2** lines:

```
                    because: "the null _itemGroups field is dereferenced at _itemGroups[selection - 1] "
                        + "inside RemoveSpecificControlGroupAsync, so the decrement must run on that path"
```

Both are string-literal lines inside the `because:` argument, which is the third shape the clause
admits. The remaining 12 added lines are 8 `///` XML documentation lines and 4 `//` comment lines.
No added line is executable code.

Required literals in the added text:

- `RemoveSpecificControlGroupAsync` — present on **3** added lines.
- `_itemGroups[selection - 1]` — present on **2** added lines, one in the rewritten XML
  documentation block and one in the rewritten `because:` string, matching the exactly-two
  expectation `[P3-T5]` recorded.

Test outcome clause. `evidence/qa-gates/p4-t5-vstest-final.2026-08-29T08-15.md` line 74 records:

```
Passed :: RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter
```

## Acceptance evaluation

1. **No added or removed line contains `Should()`, `ThrowAsync`, or `[TestMethod]`** — PASS. All
   three measured counts are 0.
2. **Every added line is a `///` line, a `//` line, or a `because:` string-literal line** — PASS.
   12 comment lines and 2 `because:` string-literal lines; 0 lines of any other shape.
3. **Added text names `RemoveSpecificControlGroupAsync` and `_itemGroups[selection - 1]`** — PASS.
   3 and 2 added-line occurrences respectively.
4. **`RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter` appears with
   outcome `Passed` in the `[P4-T5]` TRX** — PASS, recorded above.

Output Summary: The change to the #468 defects file is confined to comments and string literals.
Against the substituted anchor `e968a1a8804b7641380d4489c496662824d45767` the diff carries 24
changed lines, 14 added and 10 removed, spread over three hunks: the XML documentation block of
`RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter`, that test's
`because:` argument, and the two-line `//` comment inside the sibling
`RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter`. Measured over the
added and removed lines only, the tokens `Should()`, `ThrowAsync`, and `[TestMethod]` occur 0 times
each; all three appear solely as unchanged context. Of the 14 added lines, 12 are `///` or `//`
comment lines and the remaining 2 are the string-literal continuation lines of the `because:`
argument, so no executable line was added. The added text names `RemoveSpecificControlGroupAsync`
on 3 lines and `_itemGroups[selection - 1]` on 2. No assertion, asserted exception type, or counter
assertion changed. The `[P4-T5]` TRX records the affected test as `Passed`. All four acceptance
clauses pass.
