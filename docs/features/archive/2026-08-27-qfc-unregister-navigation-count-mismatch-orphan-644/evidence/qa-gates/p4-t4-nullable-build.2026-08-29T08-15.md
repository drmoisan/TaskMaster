# QA gate — Type-check gate ([P4-T4])

- Issue: #644
- Task: `[P4-T4]`
- Timestamp: 2026-08-29T08-15

**Restarted pass.** This artifact records the re-run triggered by the `[P4-T8]` net-line finding
described in `evidence/qa-gates/p4-t1-csharpier-format.2026-08-29T08-15.md`.

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
Working directory: repository root (`<repo-root>`)
Shell: PowerShell (`pwsh -NoProfile`)
EXIT_CODE: 0

This is character-for-character the command in `.github/workflows/ci.yml`, apart from the
deliberate local substitution of `/t:Rebuild` for CI's `/t:Build`.

## Command-shape constraints honoured

- **`/p:Nullable=enable` was not added.** No project in this repository carries a `<Nullable>`
  element and there is no `Directory.Build.props`, so that property is a solution-wide opt-in that
  would conscript every file which has never adopted the `#nullable enable` pragma. CI omits it
  deliberately, and omitting it loses no enforcement over any file that has opted in.
- **`/t:Build` was not substituted.** MSBuild's up-to-date check does not invalidate on a
  command-line `/p:` change, so a warm `/t:Build` returns exit 0 having skipped `CoreCompile` on
  every project, and the gate could not fail. Verified that compilation actually ran:

```
T4_csc=36
```

## msbuild final summary block

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:16.93
```

## Acceptance

| Clause | Required | Observed | Verdict |
|---|---|---|---|
| Exit code | 0 | **0** | PASS |
| Occurrences of the token `CS0414` | 0 | **0** | PASS |

Both clauses hold, so the loop does **not** restart from `[P4-T1]`.

## CS0414 statement

Command: fixed-string search for the token `CS0414` over the captured build log.

```
T4_cs0414=0
```

This is the gate that makes the `[P2-T3]` indivisibility argument enforceable rather than
rhetorical. `[P2-T3]` deleted three things in one edit: the
`var format = _registeredDigits == 2 ? "00" : "";` expression, the `_registeredDigits = digits;`
assignment in `RegisterNavigation()`, and the `private int _registeredDigits;` field declaration.
Deleting only the `format` expression would have left the field assigned and never read — CS0414 —
which `/p:TreatWarningsAsErrors=true` promotes to a build error. The build is green and the token
is absent across 36 compilations.

Together with `evidence/qa-gates/p2-t3-registereddigits-removed.2026-08-29T08-15.md`, which records
`git grep -F -n '_registeredDigits' -- '*.cs'` producing no output and exiting 1, this artifact is
the authoritative evidence for **AC-12**.

## Comparison across the run

| Measure | `[P0-T10]` baseline | `[P2-T4]` | `[P4-T4]` final |
|---|---|---|---|
| Exit code | 0 | 0 | 0 |
| Errors | 0 | 0 | 0 |
| Warnings | 5 | 5 | 5 |
| `CS0414` occurrences | 0 | 0 | 0 |
| `csc.exe` invocations | 36 | 36 | 36 |

Output Summary: Type-check gate **green**. EXIT_CODE **0**, **0 errors, 5 warnings** (identical to
the `[P0-T10]` baseline; all five are the pre-existing `System.Reactive` `packages.config`
advisory), and **zero occurrences of the token `CS0414`**. 36 `csc.exe` invocations confirm
`/t:Rebuild` recompiled every project. No loop restart required.
