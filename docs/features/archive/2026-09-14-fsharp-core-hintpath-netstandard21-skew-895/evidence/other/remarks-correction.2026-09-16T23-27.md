# Phase 3 — Comment-Only Remark Correction Verified (Issue #895)

Timestamp: 2026-09-17T01-23
Task: [P3-T2]
WORKTREE-LEAF: agent-a8bc4dc5978785885

Covers `[P3-T1]`, the replacement of the `<remarks>` block on `ProbeApplicationBase` in
`TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`. This is the pre-commit measurement
of AC5's comment-only clause; `[P4-T15]` is the post-commit confirming run.

Command: the `[P3-T2]` payload, run inside a WT-PREAMBLE `pwsh -NoProfile -Command` payload.

EXIT_CODE: 0
ExpectedExitCode: 0

## Output Summary:

```
UNSATISFIABLE_COUNT=1
DISPLAY_NAME_TESTS_COUNT=1
DONOTPARALLELIZE_COUNT=2
TESTMETHOD_COUNT=9
BECAUSE_206_COUNT=1
NETSTANDARDBIND_LINES=470
CHANGED_LINES=18
NON_COMMENT_CHANGED_LINES=0
11	7	TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs
```

The diff, verbatim:

```
@@ -364,7 +364,11 @@ namespace TaskMaster.Test.Bootstrap
-        /// It is deliberately not this test assembly's own output directory. The discriminating
-        /// property is which flavour of <c>FSharp.Core</c> the directory deploys: the directory
-        /// named here deploys the flavour that references the unsatisfiable
-        /// <c>netstandard 2.1.0.0</c> identity, whereas this test assembly's own output
-        /// directory deploys a flavour that never requests it, so a probe rooted there reports
-        /// success whether or not a fix is present. The host base directory is this assembly's
-        /// <c>bin\Debug</c>, so three parent steps reach the repository root.
+        /// It is deliberately not this test assembly's own output directory. This directory is
+        /// retained as the historically failing root: before issue 895 aligned every
+        /// <c>FSharp.Core</c> HintPath on the netstandard2.0 flavour, it deployed the flavour
+        /// whose own reference is <c>netstandard 2.1.0.0</c>, while this test assembly's own
+        /// output directory did not, so a probe rooted here could observe the bind failure and a
+        /// probe rooted there could not. After that alignment every deployed copy references
+        /// <c>netstandard 2.0.0.0</c>, so the Deedle invocation no longer requests the
+        /// <c>2.1.0.0</c> identity from any directory, and the discriminating power of this class
+        /// rests with the display-name tests, which bind that identity directly. The host base
+        /// directory is this assembly's <c>bin\Debug</c>, so three parent steps reach the
+        /// repository root.
```

Every changed line begins with `///`. No test method, assertion, attribute or constant changed.

## PLAN-LITERAL DEVIATION: CHANGED_LINES reads 18 and the numstat reads `11 7`

The task's acceptance names `CHANGED_LINES=22` and the numstat line `13	9`. The measured values
are 18 and `11	7`. The cause was established by reading the diff, not inferred.

`[P3-T1]` replaces a nine-line span, lines 363 to 371 inclusive, with a thirteen-line block, and
both spans open with the identical line `/// <remarks>` and close with the identical line
`/// </remarks>`. Git does not report an unchanged line as changed: it emits those two boundary
lines as context and reports only the interior. The hunk header records this exactly —
`@@ -364,7 +364,11 @@` means seven old lines starting at 364 were replaced by eleven new lines
starting at 364, with line 363 and the closing tag line outside the hunk. Eleven insertions plus
seven deletions is the eighteen changed lines measured.

The plan's arithmetic was derived from the size of the replaced span (13 and 9) rather than from the
diff git actually renders. The two accounts agree on every substantive quantity:

- both give the same net growth, and `NETSTANDARDBIND_LINES=470` is reached identically whether
  computed as 466 plus 13 minus 9 or as 466 plus 11 minus 7;
- the `<remarks>` element that AC5 is worded about survives the edit, which is precisely what the
  plan's R1 correction was made to guarantee. The boundary lines are unchanged because the
  replacement carried them, not because they were dropped;
- the comment-only property, which is the claim AC5 rests on, is measured directly by
  `NON_COMMENT_CHANGED_LINES=0` and is unaffected by which convention the line counts follow.

The consequence for `[P4-T15]`, which gates on "the numstat deletions figure is 9", is that the
figure it will observe is 7, for the same reason. This is recorded there as well.

The plan is not edited. The deviation is escalated in the executor's completion report.

## Acceptance

- `UNSATISFIABLE_COUNT=1`: yes, down from the `[P0-T9]` baseline of 2. The surviving occurrence is
  the line-281 `NegativeControl_WithoutInstall_Netstandard21Throws` summary, which stays true after
  the fix and is deliberately retained; the stale sentence is gone.
- `DISPLAY_NAME_TESTS_COUNT=1`: yes, up from 0.
- `DONOTPARALLELIZE_COUNT=2`: unchanged from `[P0-T9]`. Nothing was serialised and nothing
  de-serialised.
- `TESTMETHOD_COUNT=9`: unchanged from `[P0-T9]`. No test method was added or removed.
- `BECAUSE_206_COUNT=1`: unchanged from `[P0-T9]`. The assertion message at lines 206-207 is
  untouched.
- `NETSTANDARDBIND_LINES=470`: yes, as the plan expects.
- `NON_COMMENT_CHANGED_LINES=0`: yes. The change is comment-only.
- `CHANGED_LINES=22` and numstat `13	9`: NOT AS WRITTEN. Measured 18 and `11	7`. See the deviation
  section above.
