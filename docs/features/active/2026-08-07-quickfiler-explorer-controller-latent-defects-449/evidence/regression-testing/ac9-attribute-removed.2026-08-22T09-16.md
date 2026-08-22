# AC-9 — `[ExcludeFromCodeCoverage]` Fully Removed (Issue #449, [P5-T7])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command:
```
git grep -n -F "ExcludeFromCodeCoverage" -- QuickFiler/Controllers/QfcExplorerController.cs
```
EXIT_CODE: 1
Output: (empty — no output)

`git grep` returns exit code 1 when there is no match.

## Result

**ZERO matching lines.** No attribute of that name remains anywhere in the file — not at class level,
and not at member level. AC-9 is satisfied.

The search deliberately matches the bare identifier `ExcludeFromCodeCoverage` rather than the
bracketed form. That is the stronger check: it catches a class-level attribute, a member-level
attribute, an attribute inside a combined list, and the fully-qualified
`System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage` spelling. All forms return zero.

## What was removed, and why removal rather than narrowing

Before [P5-T1] the file carried the attribute at line 20 of the merge-base version:

```
    20	    [ExcludeFromCodeCoverage]
    21	    internal class QfcExplorerController : IQfcExplorerController
```

The attribute was pre-existing, added 2026-06-13 in commit `a564add0d`, and was not introduced by this
change. Under decision D5 it is **removed** rather than narrowed onto `OpenQFItem`, which overrides
research section 6.4: the `NotInViewDialogInvoker` seam introduced by [P5-T3] makes BOTH `OpenQFItem`
branches testable, so narrowing the attribute onto that member would have left a now-testable member
unmeasured. The three seam tests from [P5-T9] through [P5-T11] are the direct evidence that the
previously untestable branch is now testable.

The tenth and final `using` directive of the D4 disposition table,
`using System.Diagnostics.CodeAnalysis;`, was the attribute's ONLY consumer and was removed by
[P5-T2] immediately afterwards. The ordering is load-bearing: removing the directive first would have
failed the analyzer build with `CS0246` while the attribute still referenced the type. Exactly six
`using` directives now remain in the file.

## Consequence for coverage measurement

The class-level attribute suppressed every member of the class, including the compiler-generated
`async` state machine for `OpenQFItem` and the lambda display classes, so the class contributed **no
`<class>` element and no lines at all** to the Cobertura report. That is why the baseline
`QfcExplorerController` figure is recorded as **absent from the report** rather than as 0 percent —
see `../baseline/step5-vstest-coverage.2026-08-22T09-16.md` ([P0-T13]).

Removing the attribute brings the class into the report, and therefore into the coverage DENOMINATOR,
for the first time. [P7-T10] records the consequence for the epic NFR honestly, with numbers.
**No blanket class-level `[ExcludeFromCodeCoverage]` is restored** to improve any figure.

## Output Summary

`git grep -n -F "ExcludeFromCodeCoverage" -- QuickFiler/Controllers/QfcExplorerController.cs` returns
**zero matching lines** (EXIT_CODE 1, empty output). No attribute of that name remains at class level
or member level, in bracketed, combined, or fully-qualified form. The pre-existing class-level
attribute at merge-base line 20 was removed by [P5-T1], and its sole consumer directive
`using System.Diagnostics.CodeAnalysis;` was removed by [P5-T2]. AC-9 is satisfied.
