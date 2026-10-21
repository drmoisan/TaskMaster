# P4-T7 — Post-fix occurrence count of the literal `Parent Changed`

Timestamp: 2026-08-28T00-46
Command: (git grep -F -n "Parent Changed" -- QuickFiler/Viewers/ | Measure-Object).Count
EXIT_CODE: 0
ExpectedExitCode: 0

MatchCount: 0
BaselineMatchCount: 2

## Acceptance

The recorded count is **0**, against the **2** recorded in P3-T4. The two pre-fix matches were
`QuickFiler/Viewers/ItemViewer.cs:168` and `QuickFiler/Viewers/ItemViewerExpanded.cs:156`; both were
the single-statement bodies of the `L0v2h2_WebView2_ParentChanged` members that P4-T1 and P4-T3
deleted, so both matches went with the members. This discharges the AC13 zero-match assertion whose
fail-before record is P3-T4.

## Why the count idiom is load-bearing, and what `EXIT_CODE: 0` means here

This task is the case the plan's rationale anticipated. A **passing** gate here has zero matches, and
a bare `git grep` exits `1` when nothing matches. Recording that `1` would normalize this artifact to
`fail` under the evidence schema even though the gate passed, and no `ExpectedExitCode:` could be
declared unconditionally in advance because the correct expectation differs before and after the fix.

Wrapping the search in `(… | Measure-Object).Count` moves the assertion onto the recorded integer.
Measured directly, the wrapped PowerShell statement completes successfully with zero matches:

```
COUNT=0
STATEMENT_SUCCESS=True      # $? immediately after the statement
ERRCOUNT=0                  # $Error.Count, under $ErrorActionPreference='Stop'
RESIDUAL_LASTEXITCODE=1     # git grep's own native exit code, still visible in $LASTEXITCODE
```

`EXIT_CODE: 0` above is the wrapped statement's outcome: it succeeded, raised no error, and produced
the integer the gate asserts on. The residual `$LASTEXITCODE=1` is recorded here in full for
auditability — PowerShell leaves that automatic variable holding the last *native* command's code
regardless of how the pipeline is consumed, so it is not the wrapped statement's exit code and is
precisely the value the plan's idiom exists to stop this artifact from asserting on. Reading it as
the gate would invert a passing result.

Output Summary: The literal `Parent Changed` occurs **0** times under `QuickFiler/Viewers/` after the
Phase 4 deletions, down from the **2** matches recorded at P3-T4, and the wrapped statement completes
successfully with `EXIT_CODE: 0`. Both original matches were the bodies of the deleted
`L0v2h2_WebView2_ParentChanged` members, so no console diagnostic remains and no replacement was
introduced. The residual `$LASTEXITCODE=1` from `git grep`'s zero-match convention is recorded but is
not the gate; the gate is the recorded integer, which is the reason the plan mandates the
`(… | Measure-Object).Count` form for this task and not for P3-T4.
