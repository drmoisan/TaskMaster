# P2-T4 — CSharpier check

Timestamp: 2026-09-19T15-11

Command: CMD-CSHARPIER-CHECK.

```
dotnet tool run csharpier check .
```

Run from the execution worktree `C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911`. Invoked
through `dotnet tool run` so the manifest-pinned 1.2.6 is used, never a global install.

EXIT_CODE: 0

## Verbatim summary line

```
Checked 1623 files in 4210ms.
```

`N` is **1623**, an integer. Per gate rule 6, `Checked N files` is the **scanned** count, not a
finding count and not a rewrite count.

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| `EXIT_CODE: 0` | 0 | PASS |
| The verbatim `Checked N files in Xms.` line recorded with `N` an integer | `Checked 1623 files in 4210ms.` | PASS |
| Zero files reported with findings | no file line printed; the summary line is the entire output | PASS |
| No normalised `packages.config` or `app.config` reported | none reported | PASS |

## The scanned count is the positive measurement that the new patterns match

The stated failing condition is that a normalised manifest is reported, which would mean the
`.csharpierignore` patterns P1-T2 added do not match. A bare "nothing was reported" observation
cannot distinguish that from a check that scanned nothing, so the scanned count is compared against
the baseline:

| Run | Scanned count |
|---|---|
| P0-T13 baseline, before the `.csharpierignore` additions | **1658** |
| This run, after them | **1623** |
| Difference | **35** |

Thirty-five is exactly the manifest population this change normalised: **18** `packages.config`
plus **17** `app.config`, the same 35-member set P1-T7 examined and P1-T8 re-examined. The two
patterns therefore removed precisely those files from the scan and nothing else — a pattern that
matched too broadly would have removed more than 35, and one that matched nothing would have left
the count at 1658 with the manifests reported as findings.

`.csharpierignore` as it now stands is 18 lines. The two additions are at lines 16 and 18, each
preceded by its one-line rationale comment at 15 and 17:

```
15: # The repository adopts the inline form the NuGet CLI writes these manifests in, so the formatter no longer owns them.
16: **/packages.config
17: # The repository adopts the inline form the NuGet CLI writes these binding-redirect files in, so the formatter no longer owns them.
18: **/app.config
```

The block was **appended**, not inserted. Line 4 is still `**/evidence/**`, which P2-T7, P9-T7 and
gate rule 12 all cite by that line number, and lines 1 through 14 are unchanged from what P0-T21
recorded.

## Relationship to the P1-T3 live control

The count arithmetic above establishes that the manifests left the scan. P1-T3 separately
established that the check is live against the files that remain: it transiently reflowed
`UtilitiesCS/packages.config` and `UtilitiesCS/app.config` and transiently perturbed
`UtilitiesCS/Extensions/EnumExtensions.cs`, and the check named the `.cs` file while naming neither
manifest. The two observations are complementary — one shows the exclusion is real, the other shows
the checker still fires on a genuine C# defect — and together they exclude the reading that a
zero-finding result means the tool did nothing.

Output Summary: CMD-CSHARPIER-CHECK returned EXIT_CODE 0 with
`Checked 1623 files in 4210ms.` and no file reported with findings. The scanned count is 35 below
the 1658 P0-T13 recorded, which is exactly the 18 `packages.config` plus 17 `app.config` the
`.csharpierignore` additions at lines 16 and 18 now exclude. Line 4 of that file is still
`**/evidence/**`, so the three sites that cite it by line number remain valid.
