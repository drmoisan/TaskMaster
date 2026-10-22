# QA gate — Type-check gate after the production fix ([P2-T4])

- Issue: #644
- Task: `[P2-T4]`
- Timestamp: 2026-08-29T08-15

Purpose: prove the fix compiles under the type-check gate and introduces no `CS0414`.

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
Working directory: repository root (`<repo-root>`)
Shell: PowerShell (`pwsh -NoProfile`)
EXIT_CODE: 0

`/p:Nullable=enable` was not added and `/t:Build` was not substituted. That compilation actually
ran was verified rather than assumed: **36 `csc.exe` invocations** appear in the captured log.

## msbuild final summary block

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:19.54
```

**0 errors.** The warning count is 5, unchanged from the `[P0-T10]` baseline of 5; all five are
the pre-existing `System.Reactive` `packages.config` advisory, which carries no diagnostic
identifier.

## CS0414 check

Command: fixed-string search for the token `CS0414` over the captured build log.

```
cs0414-hits=0
```

**No occurrence of the token `CS0414` anywhere in the output.**

This is the gate that makes the `[P2-T3]` indivisibility argument load-bearing rather than
theoretical. `[P2-T3]` deleted the `_registeredDigits` field declaration, its assignment in
`RegisterNavigation()`, and the `format` expression in `UnregisterNavigation()` together. Had only
the `format` expression been deleted, the field would have been assigned and never read, the
compiler would have reported CS0414, and `/p:TreatWarningsAsErrors=true` would have promoted it to
an error and failed this build. The build is green and the token is absent, which is consistent
with all three deletions having been made.

Comparison against the `[P0-T10]` pre-change baseline:

| Measure | `[P0-T10]` baseline | `[P2-T4]` after fix |
|---|---|---|
| Exit code | 0 | 0 |
| Errors | 0 | 0 |
| Warnings | 5 | 5 |
| `CS0414` occurrences | 0 | 0 |

Output Summary: The type-check gate is green after the production fix. **EXIT_CODE 0, 0 errors, 5
warnings** (identical to the `[P0-T10]` baseline), and **zero `CS0414`** diagnostics. Both
`[P2-T4]` acceptance clauses hold. This artifact supports AC-12 together with
`evidence/qa-gates/p2-t3-registereddigits-removed.2026-08-29T08-15.md`; the authoritative
final-pass measurement for AC-12 is `[P4-T4]`.
