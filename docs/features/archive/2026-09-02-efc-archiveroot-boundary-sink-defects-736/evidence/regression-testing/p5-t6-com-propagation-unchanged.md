# P5-T6 — The COM-propagation contract test was not edited

Timestamp: 2026-09-04T00-17

Command:

```
git add -A
git status --porcelain
git diff --cached -U0 origin/main -- QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs
```

EXIT_CODE: 0

## The post-change line span of the frozen method

`MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates` occupies post-change lines
**251 through 264** of `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` (signature line
through closing brace). Its `[TestMethod]` attribute and XML summary sit immediately above at lines
244 through 250, so the widest span attributable to the method is **244 through 264**.

## The recorded diff

```
diff --git a/QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs b/QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs
index 992f9741..d904abf1 100644
--- a/QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs
+++ b/QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs
@@ -12,0 +13 @@ using UtilitiesCS;
+using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
@@ -167,3 +168,5 @@ namespace QuickFiler.Test.Controllers
-        /// Expected outcome: the archive root is read exactly once. The move still fails deeper
-        /// in the filer with a null reference, because the test mail helper carries no folder
-        /// information; that is the barrier that stops any second archive-root read.
+        /// Expected outcome: the archive root is read exactly once. The move stops deliberately at
+        /// the filer-invocation seam, which <see cref="TestableEfcDataModel"/> overrides, so the
+        /// only assertion is the invariant this test exists to pin. It no longer depends on an
+        /// incidental collaborator crash several frames downstream, whose future absence would have
+        /// failed this test with a message pointing at the wrong subsystem (issue #699).
@@ -179 +181,0 @@ namespace QuickFiler.Test.Controllers
-            Func<Task> act = () => MoveAsync(dataModel);
@@ -182 +184 @@ namespace QuickFiler.Test.Controllers
-            await act.Should().ThrowAsync<NullReferenceException>();
+            await MoveAsync(dataModel);
@@ -386,0 +389,8 @@ namespace QuickFiler.Test.Controllers
+
+            // The deliberate stop that replaces the incidental downstream dereference. The base
+            // body constructs the real filer, which this test has no fixture for; overriding it
+            // ends the success path at the seam instead of several frames later.
+            protected internal override Task<bool> InvokeFilerAsync(
+                EmailFilerConfig config,
+                IList<MailItemHelper> mailHelpers
+            ) => Task.FromResult(true);
```

**No hunk header names a new-file line range intersecting 244-264.** The five hunks touch new-file
lines 13, 168-172, an insertion point at 181 carrying no added line, 184, and 389-396. Every one of
those falls outside the frozen method's span, above it or below it.

## The porcelain span makes the claim non-vacuous

`git status --porcelain` lists the file as modified:

```
M  QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs
```

The file **was** edited by this phase, so the empty-intersection result above is a real observation
about where those edits landed rather than a consequence of an untouched file.

## The frozen method's assertion line, quoted verbatim

```
            await act.Should().ThrowAsync<COMException>();
```

The assertion is unchanged: the test still injects a `COMException` at the `Mock<IOlObjects>` seam —
which sits above the layer this fix touches — and still asserts that it propagates. Issue #638's
rejection of widening `EfcDataModel.TryGetArchiveRoot`'s catch to `COMException` therefore still
holds, and P5-T5 records the method passing.

Output Summary: the frozen COM-propagation method occupies post-change lines 251-264 (244-264
including its attribute and summary). The anchored, index-reading diff over the file produced five
hunks at new-file lines 13, 168-172, 181, 184, and 389-396 — none intersecting that span — while the
porcelain span lists the file as modified, so the empty intersection is a real observation. The
method's assertion line is unchanged at `await act.Should().ThrowAsync<COMException>();`.
