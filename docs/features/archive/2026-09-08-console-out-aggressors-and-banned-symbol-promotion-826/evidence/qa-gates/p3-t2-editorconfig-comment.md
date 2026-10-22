# `.editorconfig` tracking-comment amendment (issue #826, [P3-T2])

Timestamp: 2026-09-09T19-18

Command: the surface was re-measured first, so that no unverified figure was written into the
repository, then the two comment lines were replaced. Both ran as `pwsh -NoProfile -Command` blocks
carrying the plan's C2 preamble branch guard:

```
$cs = @(git ls-files "*.cs")
@(Select-String -LiteralPath $cs -CaseSensitive -SimpleMatch "DateTime.Now").Count
@(Select-String -LiteralPath $cs -CaseSensitive -SimpleMatch "DateTime.UtcNow").Count
@(Select-String -LiteralPath $cs -CaseSensitive -SimpleMatch "Random.Shared").Count
@(Select-String -LiteralPath $cs -CaseSensitive -SimpleMatch "Thread.Sleep").Count
@(Select-String -LiteralPath $cs -CaseSensitive -SimpleMatch "Task.Delay").Count
```

EXIT_CODE: 0

## Re-measured textual surface

Measured against tracked `*.cs` in this worktree at implementation time, after the item-2 edit and after
the new test file was created:

| Symbol | Re-measured count |
|---|---|
| `DateTime.Now` | 53 |
| `DateTime.UtcNow` | 20 |
| `Random.Shared` | 5 |
| `Thread.Sleep` | 15 |
| `Task.Delay` | 60 |
| **total** | **153** |

These are the figures written into the comment. They are not the figures `spec.md` records (54, 20, 5,
15, 58 = 152); the plan requires the re-measured values, and the two differ because the tree has moved
since the spec was authored. Textual hits are not diagnostic counts and the two are not comparable.

## Comment block as it now stands

The region running from the line containing `BannedApiAnalyzers 3.3.4` through the line containing
`dotnet_diagnostic.RS0030.severity`:

```
# --- Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4 (RS003x) ---
# RS0030 is held at suggestion. Promotion to warning is blocked by exactly one
# precondition: the pre-existing banned-symbol call sites must be cleared first,
# because toolchain step 3 (msbuild ... /p:TreatWarningsAsErrors=true, mirrored by
# .github/workflows/_build-nullable.yml) promotes every warning to a build error,
# and the analyzer step, which passes no TreatWarningsAsErrors, would not break.
# Verified textual surface, 2026-09-08 (re-measured at implementation time):
# DateTime.Now 53, DateTime.UtcNow 20, Random.Shared 5, Thread.Sleep 15,
# Task.Delay 60 = 153 textual hits. Textual hits are not diagnostics; the earlier
# recorded figure was a diagnostic count and is not comparable to this one.
dotnet_diagnostic.RS0030.severity = suggestion
```

The two lines this task replaced previously read:

```
# RS0030 held at suggestion for initial rollout. Promotion to warning is a
# post-cleanup follow-up (143 existing banned-symbol usages, see issue #181 evidence).
```

The amended comment states the promotion precondition inline, names the gate that would actually break
(the nullable gate, not the analyzer gate), records the re-measured surface, and no longer defers to the
closed tracking issue.

## Gate figures

Region counts, over the region defined above:

| `-SimpleMatch` token | Observed | Required |
|---|---|---|
| `181` | 0 | 0 |
| `TreatWarningsAsErrors` | 2 | at least 1 |
| `_build-nullable.yml` | 1 | at least 1 |
| `2026-09-08` | 1 | at least 1 |

Whole-file counts:

| `-SimpleMatch` token | Observed | Required |
|---|---|---|
| `#181` | 2 | 2, down from the 3 recorded by [P0-T5] |
| `dotnet_diagnostic.RS0030.severity = suggestion` | 1 | 1 |

The two surviving `#181` references are at `.editorconfig` lines 23 and 993 in `Get-Content` numbering,
in the third-party-analyzer-severities header and the naming-preferences header respectively. Neither is
the tracking reference AC13 targets, and neither is in the region this task owns.

## Constraints observed

The authored comment block contains none of the characters `181`, none of the token
`dotnet_diagnostic.` and none of the token `.severity`. The severity value itself was not changed on any
line; [P3-T3] and [P4-T3] prove that independently from the anchored diff.

Output Summary: the tracking comment now states the promotion precondition inline with the re-measured
surface of 153 textual hits and no longer cites closed work. Region `181` count is 0, the three required
region tokens are present, the whole-file `#181` count fell from 3 to 2, and the severity line is intact
at `suggestion`.
