# P0-T22 — Dependabot Configuration Census

Timestamp: 2026-09-19T23-14

Commands:

```
git grep -c "group-by:" -- ".github/dependabot.yml"
git grep -c -i "Deedle" -- ".github/dependabot.yml"
git grep -c "version-update:semver-major" -- ".github/dependabot.yml"
```

together with a direct read of `.github/dependabot.yml`.

EXIT_CODE: 0

## 1. Group keys under `groups:`

**4.**

| # | Group key | Line |
|---|---|---|
| 1 | `analyzers-dev-deps` | 10 |
| 2 | `test-frameworks` | 18 |
| 3 | `microsoft-extensions-and-bcl` | 27 |
| 4 | `graph-identity-telemetry` | 33 |

## 2. `group-by:` lines

**4**, one under each group, at lines 17, 26, 32 and 41, each with the value `"dependency-name"`.

These are the keys the plan describes as inert: `group-by` is not a Dependabot grouping option, so
the four have no effect on how updates are batched. P3-T7 removes all four.

## 3. `open-pull-requests-limit`

**10**, at line 8. P3-T7 sets it to `1`.

## 4. Ordered list of `dependency-name` values carrying `version-update:semver-major`

**8 entries**, recorded in file order:

| # | `dependency-name` | Line |
|---|---|---|
| 1 | `Microsoft.Extensions.*` | 47 |
| 2 | `Microsoft.Bcl.*` | 49 |
| 3 | `System.Text.Json` | 51 |
| 4 | `System.Drawing.Common` | 53 |
| 5 | `Microsoft.Graph*` | 55 |
| 6 | `Apache.Arrow*` | 57 |
| 7 | `Microsoft.Data.Analysis` | 59 |
| 8 | `Microsoft.ML*` | 61 |

`git grep -c "version-update:semver-major"` returns 8, matching the 8 enumerated entries.

**This 8-member list, in this order, is the literal expected set that
`tests/scripts/dependencies/DependabotConfig.Tests.ps1` declares for AC1.** It is also the list
P3-T7 must retain unchanged and in the same order, compared element by element.

## 5. Ignore entries naming `Deedle`

**0.** The case-insensitive search over the whole file returned no match.

## Non-vacuity

The single zero in section 5 is guarded by four positive counts, per gate rule 2: 4 group keys, 4
`group-by:` lines, 8 `semver-major` ignore entries enumerated by name and line, and the
`open-pull-requests-limit` value of 10. A search that resolved no file would have returned 0 for
all five and failed on the four positives.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| Group keys under `groups:` | 4 | 4 | PASS |
| `group-by:` lines | 4 | 4 | PASS |
| `open-pull-requests-limit` | `10` | 10 | PASS |
| Ordered `semver-major` `dependency-name` list | the 8 names, in file order | identical, 8 names in file order | PASS |
| Ignore entries naming `Deedle` | 0 | 0 | PASS |

Output Summary: `.github/dependabot.yml` declares **4** groups — `analyzers-dev-deps`,
`test-frameworks`, `microsoft-extensions-and-bcl`, `graph-identity-telemetry` — each carrying a
`group-by: "dependency-name"` line, **4** in total, all inert. `open-pull-requests-limit` is **10**.
The `version-update:semver-major` ignore block carries exactly **8** `dependency-name` values, in
file order: `Microsoft.Extensions.*`, `Microsoft.Bcl.*`, `System.Text.Json`,
`System.Drawing.Common`, `Microsoft.Graph*`, `Apache.Arrow*`, `Microsoft.Data.Analysis`,
`Microsoft.ML*`. That ordered list is the expected set AC1's test declares and the set P3-T7 must
retain. There are **0** ignore entries naming `Deedle`; P3-T7 adds one, unqualified.
