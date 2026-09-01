# Phase 5 — `IsAltKeyCommand` survives unchanged ([P5-T3])

Timestamp: 2026-09-01T23-27

Command 1:

```
git diff -U0 origin/main...HEAD -- QuickFiler/Controllers/QfcFormKeyHandler.cs QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs
```

Command 2:

```
git status --porcelain -- QuickFiler/Controllers/QfcFormKeyHandler.cs QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs
```

EXIT_CODE: 0 for both.

## Acceptance reading 1 — no removed line contains `IsAltKeyCommand`

The diff contains **zero** lines that begin with a single `-` character. Measured directly by filtering
the diff output for lines matching `^-` and excluding the `---` file-header form:

```
REMOVED_LINE_COUNT=0
REMOVED_WITH_ISALT=0
```

With no removed line at all, no removed line contains `IsAltKeyCommand`. The change to both files is
purely additive: two `using` directives and one new member in the production file, two `using` directives
and seven new test methods in the test file.

`IsAltKeyCommand` itself is untouched, as is the class-level XML summary on lines 8 through 11 of the test
file whose line 9 names that identifier. Had the summary been rewritten to mention the new methods, the
rewrite would have produced a removed line containing `IsAltKeyCommand` and failed this gate; the new
methods carry their own per-method comments instead.

## Acceptance reading 2 — porcelain span

`git status --porcelain -- QuickFiler/Controllers/QfcFormKeyHandler.cs QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`

Output: **nothing**. Neither file is modified, staged or untracked after the `[P5-T1]` commit.

## The full diff

```diff
diff --git a/QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs b/QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs
index 31a820bc..1609efcd 100644
--- a/QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs
+++ b/QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs
@@ -3,0 +4 @@ using Microsoft.VisualStudio.TestTools.UnitTesting;
+using Moq;
@@ -4,0 +6 @@ using QuickFiler.Controllers;
+using QuickFiler.Interfaces;
@@ -65,0 +68,142 @@ namespace QuickFiler.Controllers.Tests
+
+        // Issue #663. The QuickFiler form's ProcessCmdKey dispatches the parameterless
+        // ToggleKeyboardDialogAsync() overload, which accepts no key data, so the only gesture that
+        // dispatch can encode is a bare Alt press. ClaimsAltChord therefore accepts the Alt modifier
+        // only when the key-code half of the value, keyData & Keys.KeyCode, is Keys.Menu or
+        // Keys.None. Every other Alt chord is a mnemonic or a system chord and must fall through to
+        // the base implementation.
+
+        // Positive case, synthetic shape: the bare Keys.Alt value a unit test supplies masks to
+        // Keys.None in its key-code half.
+        [TestMethod]
+        public void ClaimsAltChord_WithBareAltFlagAndHandler_ReturnsTrue()
+        {
+            // Arrange
+            var handler = new Mock<IQfcKeyboardHandler>();
+
+            // Act
+            var result = QfcFormKeyHandler.ClaimsAltChord(handler.Object, Keys.Alt);
+
+            // Assert
+            result
+                .Should()
+                .BeTrue(
+                    "bare Alt is the only chord the keyboard-navigation dialog toggle services"
+                );
+        }
+
+        // Positive case, physical-keyboard shape: a real bare Alt press arrives with the Alt
+        // modifier flag set and Keys.Menu, documented as "The ALT key", in its key-code half.
+        [TestMethod]
+        public void ClaimsAltChord_WithMenuKeyCodeAndAltFlag_ReturnsTrue()
+        {
+            // Arrange
+            var handler = new Mock<IQfcKeyboardHandler>();
+
+            // Act
+            var result = QfcFormKeyHandler.ClaimsAltChord(handler.Object, Keys.Menu | Keys.Alt);
+
+            // Assert
+            result
+                .Should()
+                .BeTrue(
+                    "a physical bare Alt press carries the Keys.Menu key code with the Alt flag"
+                );
+        }
+
+        // Negative case, the one real mnemonic on this surface: the hosted ItemViewer and
+        // ItemViewerExpanded controls each carry a "&Move Options" menu item.
+        [TestMethod]
+        public void ClaimsAltChord_WithAltM_ReturnsFalse()
+        {
+            // Arrange
+            var handler = new Mock<IQfcKeyboardHandler>();
+
+            // Act
+            var result = QfcFormKeyHandler.ClaimsAltChord(handler.Object, Keys.Alt | Keys.M);
+
+            // Assert
+            result
+                .Should()
+                .BeFalse(
+                    "Alt+M is the Move Options mnemonic on the hosted item viewers and must reach the base implementation"
+                );
+        }
+
+        // Negative case, system chord: Alt+F4 reaches ProcessCmdKey as WM_SYSKEYDOWN before the
+        // default window procedure can translate it into the close command.
+        [TestMethod]
+        public void ClaimsAltChord_WithAltF4_ReturnsFalse()
+        {
+            // Arrange
+            var handler = new Mock<IQfcKeyboardHandler>();
+
+            // Act
+            var result = QfcFormKeyHandler.ClaimsAltChord(handler.Object, Keys.Alt | Keys.F4);
+
+            // Assert
+            result
+                .Should()
+                .BeFalse("Alt+F4 is the standard window-close chord and must not be consumed here");
+        }
+
+        // Negative case, vestigial chord: no keyboard registry on this surface is keyed on an
+        // Alt-modified arrow value, so claiming Alt+arrow discards a key the form will not act on.
+        [TestMethod]
+        public void ClaimsAltChord_WithAltLeft_ReturnsFalse()
+        {
+            // Arrange
+            var handler = new Mock<IQfcKeyboardHandler>();
+
+            // Act
+            var result = QfcFormKeyHandler.ClaimsAltChord(handler.Object, Keys.Alt | Keys.Left);
+
+            // Assert
+            result
+                .Should()
+                .BeFalse("Alt+arrow is vestigial on this surface and must fall through unclaimed");
+        }
+
+        // Negative case, no Alt modifier at all. Two inputs are asserted in one body: a bare letter
+        // key, and Keys.Control, whose key-code half is Keys.None and which would be accepted by a
+        // predicate that inspected only the key-code half without first testing the Alt flag.
+        [TestMethod]
+        public void ClaimsAltChord_WithoutAltFlag_ReturnsFalse()
+        {
+            // Arrange
+            var handler = new Mock<IQfcKeyboardHandler>();
+
+            // Act
+            var withLetterKey = QfcFormKeyHandler.ClaimsAltChord(handler.Object, Keys.M);
+            var withControlModifier = QfcFormKeyHandler.ClaimsAltChord(
+                handler.Object,
+                Keys.Control
+            );
+
+            // Assert
+            withLetterKey
+                .Should()
+                .BeFalse("a bare letter key carries no Alt flag and is not the dialog gesture");
+            withControlModifier
+                .Should()
+                .BeFalse(
+                    "Keys.Control carries no Alt flag even though its key-code half is Keys.None"
+                );
+        }
+
+        // Negative case, unwired handler: with nothing to dispatch to, the chord is not claimed and
+        // reaches the base implementation unchanged.
+        [TestMethod]
+        public void ClaimsAltChord_WithNullHandler_ReturnsFalse()
+        {
+            // Arrange
+            IQfcKeyboardHandler handler = null;
+
+            // Act
+            var result = QfcFormKeyHandler.ClaimsAltChord(handler, Keys.Alt);
+
+            // Assert
+            result
+                .Should()
+                .BeFalse("with no handler wired there is nothing to claim the chord for");
+        }
diff --git a/QuickFiler/Controllers/QfcFormKeyHandler.cs b/QuickFiler/Controllers/QfcFormKeyHandler.cs
index 5572a5b6..a27c7c47 100644
--- a/QuickFiler/Controllers/QfcFormKeyHandler.cs
+++ b/QuickFiler/Controllers/QfcFormKeyHandler.cs
@@ -1,0 +2 @@ using System.Windows.Forms;
+using QuickFiler.Interfaces;
@@ -18,0 +20,18 @@ namespace QuickFiler.Controllers
+
+        /// <summary>
+        /// Decides whether the QuickFiler form's <c>ProcessCmdKey</c> override should claim the
+        /// supplied key chord for the keyboard-navigation dialog.
+        /// </summary>
+        /// <param name="handler">The keyboard handler the form dispatches to, or <see langword="null"/>.</param>
+        /// <param name="keyData">The key data reported by <c>ProcessCmdKey</c>.</param>
+        /// <returns><see langword="true"/> when the chord is claimed; otherwise <see langword="false"/>.</returns>
+        internal static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)
+        {
+            if (handler is null || !keyData.HasFlag(Keys.Alt))
+            {
+                return false;
+            }
+
+            Keys keyCode = keyData & Keys.KeyCode;
+            return keyCode == Keys.Menu || keyCode == Keys.None;
+        }
```

Output Summary: The anchored diff of the two `QfcFormKeyHandler` files against `origin/main` contains zero
removed lines, so no removed line contains `IsAltKeyCommand`, and the porcelain span over the same two
paths prints nothing. `IsAltKeyCommand` keeps its signature, body and semantics, and the four existing
`IsAltKeyCommand_*` tests are unmodified; `[P4-T5]` separately records all four as passing. AC-8 holds.
