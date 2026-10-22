# Fixture-change census (issue #968, tasks P2-T1 to P2-T6)

Timestamp: 2026-10-03T02-58
Command: pwsh -NoProfile -Command '<CMD-LINECOUNT payload>' with FILES `"QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs"` (the first of four payloads; then CMD-TOKEN-COUNT on FIX with the P0-T12 FIX list, CMD-SPAN-TOKEN-COUNT on ENSURE and on SCOPE with the P0-T12 lists; each the Command Reference macro executed verbatim with PREFIX expanded and WORKTREE substituted), followed by three git calls
Canonical command: CMD-LINECOUNT, CMD-TOKEN-COUNT and CMD-SPAN-TOKEN-COUNT on FIX; git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs; git -C WORKTREE diff HEAD -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs; git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229 (every payload)
- LINES QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs = 375
- FIX tokens (`_pinCount`, `_fixtureInstalledParked`, `lock (FieldLock)`, `CompareExchange(`, `return new EnsureScope(`, `leaks exactly`, `A scope that installed nothing`, `pins for the process lifetime`, `install-ownership flag`, `installed nothing carries`): 4, 4, 5, 2, 1, 0, 0, 2, 1, 0
- ENSURE: SPAN: 143-167; `_pinCount++` 1, `_fixtureInstalledParked = true;` 1, `lock (FieldLock)` 1, `return new EnsureScope(` 1
- SCOPE: SPAN: 273-316; `CompareExchange(` 0, `lock (FieldLock)` 1, `_pinCount--` 1, `_fixtureInstalledParked = false;` 1, `DispatcherField.SetValue(null, null);` 1
- numstat (exit 0): `48	15	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`
- porcelain (exit 0):
  - ` M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`
  - ` M QuickFiler.Test/QuickFiler.Test.csproj`
  - `?? QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs`
- P2-T1: one line contains `private static int _pinCount;` and one contains `private static bool _fixtureInstalledParked;`, both in the static field block above the issue #743 counters; the inserted comment contains `only while FieldLock is held` and does not contain `lock (FieldLock)`.
- P2-T2: `install-ownership flag` 1; the class doc ends with the new paragraph followed by `/// </summary>`.
- P2-T3: `leaks exactly` 0; `pins for the process lifetime` 2.
- P2-T4 and P2-T5: as the ENSURE and SCOPE values above; `A scope that installed nothing` 0.

## FIELDLOCK-ENCLOSURE

Reading the transcribed diff below: the two new fields are `private static` members of `UiThreadDispatcherFixture` (diff hunk 2). The two `lock (FieldLock)` blocks that use them are (1) the block in `EnsureDispatcher` (hunk 4), which contains `_pinCount++;` and `_fixtureInstalledParked = true;`, and (2) the block in `EnsureScope.Dispose` (hunk 6), which contains `_pinCount--;`, `_pinCount == 0`, `&& _fixtureInstalledParked` and `_fixtureInstalledParked = false;`. The three non-declaration occurrences of each new field therefore all lie inside one of those two blocks, and the F-FIELDS comment names neither identifier. The scope class contains no `CompareExchange` call (SCOPE `CompareExchange(` 0): the last-release null write `DispatcherField.SetValue(null, null);` is made inline in the same critical section as the decrement (the AC9 reading).

## Transcribed diff (git -C WORKTREE diff HEAD -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs)

```diff
diff --git a/QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs b/QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
index 1eaa87064..3b7f9e1f1 100644
--- a/QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
+++ b/QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
@@ -28,6 +28,19 @@ namespace QuickFiler.Controllers.Tests
     /// that carry no <c>[Timeout]</c>, so making them wait on a gate another test class holds for a
     /// whole test body would convert a bounded failure elsewhere into an unbounded hang there.
     /// </para>
+    /// <para>
+    /// Issue #968: ensure pins are reference counted. A pin counter and an install-ownership flag
+    /// live under <c>FieldLock</c>. The first pin on a <c>null</c> field seeds the parked dispatcher
+    /// and sets the flag; the last release writes <c>null</c> back only when the flag is set and the
+    /// field still holds the parked instance, then clears the flag. A discarded scope therefore
+    /// pins for the process lifetime and leaves the parked dispatcher installed, so every caller
+    /// disposes its scope, and every pin is acquired and released while its caller holds a
+    /// transaction, which keeps the count at zero whenever a transaction is acquired. Residual: a
+    /// transaction that installs over a pinned parked value and restores it after the last pin
+    /// released leaves the parked value installed with zero pins and the flag set; the next pin
+    /// cycle reverts it. No test in this assembly installs over a pinned value, so the residual is
+    /// documented rather than exercised.
+    /// </para>
     /// </summary>
     internal static class UiThreadDispatcherFixture
     {
@@ -37,6 +50,11 @@ namespace QuickFiler.Controllers.Tests
         private static readonly FieldInfo DispatcherField = ResolveDispatcherField();
         private static Dispatcher _parkedDispatcher = null;
 
+        // Issue #968: the count of live ensure scopes and whether the fixture itself seeded the parked
+        // dispatcher into a null field. Both are read and written only while FieldLock is held.
+        private static int _pinCount;
+        private static bool _fixtureInstalledParked;
+
         // Issue #743 AC1 observable: three monotonic counters over TransactionGate. A contended
         // acquisition is one that observed CurrentCount == 0 immediately before waiting. In a serial
         // run no live holder can exist when a test begins its transaction, so a non-zero contended
@@ -114,10 +132,13 @@ namespace QuickFiler.Controllers.Tests
         }
 
         /// <summary>
-        /// Seeds the static with the parked dispatcher only when it is currently <c>null</c>, and
-        /// returns a scope whose <c>Dispose</c> conditionally reverts that seeding. Never acquires
-        /// <c>TransactionGate</c> and never blocks on anything a caller must release. Disposing the
-        /// returned scope is optional: a discarded scope leaks exactly as the pre-fix helper did.
+        /// Takes one counted pin on the shared static (issue #968). The first pin on a <c>null</c>
+        /// field seeds the parked dispatcher and records that the fixture owns the seeding; a pin
+        /// taken while the field is non-null installs nothing. Disposing the returned scope releases
+        /// the pin, and the field reverts to <c>null</c> only on the last release, only when the
+        /// fixture owns the seeding, and only when the field still holds the parked instance. Never
+        /// acquires <c>TransactionGate</c> and never blocks on anything a caller must release. A
+        /// discarded scope pins for the process lifetime, so every caller disposes its scope.
         /// </summary>
         internal static IDisposable EnsureDispatcher()
         {
@@ -127,14 +148,15 @@ namespace QuickFiler.Controllers.Tests
 
             lock (FieldLock)
             {
+                _pinCount++;
                 if (DispatcherField.GetValue(null) == null)
                 {
                     DispatcherField.SetValue(null, parked);
-                    return new EnsureScope(parked);
+                    _fixtureInstalledParked = true;
                 }
             }
 
-            return new EnsureScope(null);
+            return new EnsureScope(parked);
         }
 
         /// <summary>
@@ -241,19 +263,21 @@ namespace QuickFiler.Controllers.Tests
         }
 
         /// <summary>
-        /// The scope returned by <see cref="EnsureDispatcher"/>. Reverts the seeding only when the
-        /// static still holds the exact instance this scope installed. A scope that installed nothing
-        /// carries <c>null</c> and is a no-op, which is what keeps a discarded scope from clobbering a
-        /// value some other owner installed in the meantime.
+        /// The scope returned by <see cref="EnsureDispatcher"/>: one counted pin. Disposal is
+        /// idempotent and performs the decrement and the conditional revert inline in one
+        /// <c>FieldLock</c> critical section, so no other pin can interleave between them. The revert
+        /// writes <c>null</c> only when this release brings the count to zero, the fixture itself
+        /// seeded the parked dispatcher, and the field still holds that instance; a value some other
+        /// owner installed in the meantime is left in place.
         /// </summary>
         private sealed class EnsureScope : IDisposable
         {
-            private readonly Dispatcher _installed;
+            private readonly Dispatcher _parked;
             private bool _disposed = false;
 
-            internal EnsureScope(Dispatcher installed)
+            internal EnsureScope(Dispatcher parked)
             {
-                _installed = installed;
+                _parked = parked;
                 _disposed = false;
             }
 
@@ -266,9 +290,18 @@ namespace QuickFiler.Controllers.Tests
 
                 _disposed = true;
 
-                if (_installed != null)
+                lock (FieldLock)
                 {
-                    UiThreadDispatcherFixture.CompareExchange(_installed, null);
+                    _pinCount--;
+                    if (
+                        _pinCount == 0
+                        && _fixtureInstalledParked
+                        && ReferenceEquals(DispatcherField.GetValue(null), _parked)
+                    )
+                    {
+                        DispatcherField.SetValue(null, null);
+                        _fixtureInstalledParked = false;
+                    }
                 }
             }
         }
```
