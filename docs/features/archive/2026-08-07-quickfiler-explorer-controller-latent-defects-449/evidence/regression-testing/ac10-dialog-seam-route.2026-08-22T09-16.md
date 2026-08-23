# AC-10 — Dialog Call Routed Through the Seam (Issue #449, [P5-T8])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command:
```
git grep -n -F "MessageBox.Show" -- QuickFiler/Controllers/QfcExplorerController.cs
```
EXIT_CODE: 0

Full output (verbatim, complete):
```
QuickFiler/Controllers/QfcExplorerController.cs:63:            (text, caption, buttons, icon) => MessageBox.Show(text, caption, buttons, icon);
```

## Result

**Exactly ONE matching line**, at line 63. AC-10's count condition is satisfied.

## The one match lies inside the seam's default initialiser, NOT in `OpenQFItem`

Surrounding context:

```
    49	        // Injectable seam for the not-in-view prompt. The branch it guards calls a modal WinForms
    50	        // dialog, which cannot be exercised in a headless unit test: the dialog blocks on user input
    51	        // and requires a message pump. Tests replace this delegate with a stub that records the
    52	        // arguments and returns the DialogResult under test. The delegate type is written fully
    53	        // qualified as System.Func<...> so the seam does not resurrect the `using System;` directive
    54	        // that was removed as orphaned, matching the file's existing fully-qualified style for
    55	        // log4net.ILog and System.Reflection.MethodBase above.
    56	        internal System.Func<
    57	            string,
    58	            string,
    59	            MessageBoxButtons,
    60	            MessageBoxIcon,
    61	            DialogResult
    62	        > NotInViewDialogInvoker { get; set; } =
    63	            (text, caption, buttons, icon) => MessageBox.Show(text, caption, buttons, icon);
```

Line 63 is the production DEFAULT of the `NotInViewDialogInvoker` auto-property declared at line 62.
It is a class-member initialiser, structurally outside any method body. `OpenQFItem` begins at line
145, so the match sits 82 lines above it and cannot be inside it.

The body of `OpenQFItem` now calls the seam instead:

```
   167	                DialogResult result = NotInViewDialogInvoker(
   168	                    "Selected message is not in view. Would you like to open it?",
   169	                    "Error",
   170	                    MessageBoxButtons.YesNo,
   171	                    MessageBoxIcon.Error
   172	                );
```

## The four argument values are byte-identical to the pre-change call

Only the invocation TARGET changed. The user-visible dialog text, caption, buttons, and icon are
unchanged, so the production behaviour a user sees is identical.

| Position | Pre-change (merge-base line 168) | Post-change (line 167) | Identical? |
| --- | --- | --- | --- |
| 1 text | `"Selected message is not in view. Would you like to open it?"` | same | yes |
| 2 caption | `"Error"` | same | yes |
| 3 buttons | `MessageBoxButtons.YesNo` | same | yes |
| 4 icon | `MessageBoxIcon.Error` | same | yes |
| invocation target | `MessageBox.Show` | `NotInViewDialogInvoker` | **changed — the only change** |

The argument order is unchanged, and the seam's delegate signature
`System.Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>` matches that order
positionally, so the compiler enforces the correspondence.

`mailItem.Display()` at line 175 needs no seam, because `MailItem` is already mocked throughout this
repository's tests.

## Note on the exact-count gate

The explanatory comment at lines 49-55 was originally worded using the literal token `MessageBox.Show`
when describing why the seam exists. That wording made this search return **two** lines and would have
failed AC-10's "exactly one" condition on a COMMENT rather than on real code. The comment was
rephrased to "the dialog blocks on user input", which preserves its full meaning — [P5-T3] requires
the comment to record that the not-in-view branch calls a modal WinForms dialog that cannot be
exercised in a headless unit test, and it still does — without weakening the gate. The gate was not
relaxed; the comment was corrected so the gate measures production code.

## Corroboration by test

The three seam tests recorded in `phase5-seam-tests.2026-08-22T09-16.md` confirm the route
behaviourally, not merely textually. `OpenQFItem_WhenItemNotSelectableInView_InvokesDialogSeamOnce`
asserts the substituted stub is invoked exactly once with those four argument values, which is only
possible if `OpenQFItem` calls the seam rather than the dialog API directly. The seam default at line
63 is never exercised by any test, so no dialog is ever displayed and no message pump is required.

## Output Summary

`git grep -n -F "MessageBox.Show" -- QuickFiler/Controllers/QfcExplorerController.cs` returns
**exactly one matching line**, at line 63, and that line lies within the default initialiser of the
`NotInViewDialogInvoker` auto-property declared at line 62 — **not** in the body of `OpenQFItem`,
which begins at line 145. `OpenQFItem` calls the seam at line 167 with all four argument values
byte-identical to the pre-change call, so only the invocation route changed and the user-visible
dialog is unchanged. AC-10 is satisfied.
