# QA Gate — Coverage delta, baseline vs post-change (P7-T6)

Timestamp: 2026-08-27T23-31

Command: parse `FF/evidence/baseline/baseline.cobertura.2026-08-27T20-01.xml` and
`FF/evidence/qa-gates/postchange.cobertura.2026-08-27T23-31.xml`, aggregating every `<class>` element
that shares a `filename` attribute; then `git diff --unified=0 BASELINE_SHA -- <owned production paths>`

EXIT_CODE: 0

Output Summary: every clause of the acceptance passes. Repository line-rate delta **+0.0068 pp**, all
four per-file deltas at or above 0.00 pp, and new/changed-line coverage **100.0000%**.

`BASELINE_SHA` is `4f238289090e4c97ca505511a5a73e8092dce0f9`.

## (a) Baseline repository rates

| Metric | Value |
| --- | --- |
| `line-rate` | 85.1380% (54387 / 63881) |
| `branch-rate` | 79.2096% (12927 / 16320) |

## (b) Post-change repository rates

| Metric | Value |
| --- | --- |
| `line-rate` | 85.1448% (54439 / 63937) |
| `branch-rate` | 79.2202% (12943 / 16338) |

| Delta | Value | Acceptance | Verdict |
| --- | --- | --- | --- |
| repository `line-rate` | **+0.0068 pp** | at or above 0.00 pp | PASS |
| repository `branch-rate` | +0.0106 pp | not gated | informational |

Repository line coverage is **85.1448%**, above the 85% floor in `.claude/rules/general-unit-test.md`.
Coverage moved UP, not down.

## (c) Per-file line-rate, owned production files

| File | Baseline | Post-change | Delta | Verdict |
| --- | --- | --- | ---: | --- |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | 98.3122% (233/237) | 98.3122% (233/237) | 0.0000 pp | PASS |
| `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | 99.0196% (202/204) | 100.0000% (209/209) | +0.9804 pp | PASS |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 100.0000% (294/294) | 100.0000% (306/306) | 0.0000 pp | PASS |
| **SR-1 split pair, COMBINED** | 100.0000% (280/280) | 100.0000% (295/295) | 0.0000 pp | PASS |

Every delta is at or above the -0.50 pp floor. None is negative, so no measurement-noise exemption is
claimed.

The split pair is reported as one combined row because P2-T1 relocates lines between
`BreadcrumbBridgeCoordinator.cs` and `BreadcrumbBridgeCoordinator.Suggestions.cs`, so a per-file
comparison across the split has no common denominator and the new file has no baseline row by
construction. Its post-change value is (covered lines of both files) / (coverable lines of both files).

### Remediation applied to reach this result

The FIRST post-merge measurement put the combined split pair at 98.9726% (289/292), a delta of
**-1.0274 pp**, which FAILED the -0.50 pp floor, and put new/changed-line coverage at 96.5116%. The
three uncovered lines were the `if (!ran) { _upgradeLifetime.Abandon(lease); }` block in `AddItems`,
unreachable on a single thread because a freshly begun lease is current by construction. An
`AddItemsCore` seam was extracted, mirroring the already-ratified `SetSuggestionsCore` seam (SR-5), and
a deterministic supersession test now drives the skip path. Full rationale, including why the block was
not simply deleted, is in `FF/evidence/qa-gates/addItemsCore-seam.2026-08-27T23-31.md`.

## (d) New and changed line coverage

For every line added or modified in the five owned production files per
`git diff --unified=0 BASELINE_SHA -- <owned paths>`, checked against `hits` in the post-change
Cobertura:

| File | Added lines | Coverable | Covered | Uncovered |
| --- | ---: | ---: | ---: | --- |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | 39 | 12 | 12 | none |
| `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | 52 | 12 | 12 | none |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 36 | 14 | 14 | none |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 0 | 0 | 0 | none |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` | 123 | 51 | 51 | none |
| **Total** | **250** | **89** | **89** | **none** |

**New/changed-line coverage = 89 / 89 = 100.0000%**, against an acceptance floor of 90.00% (AC-32,
second half). PASS.

"Added lines" counts every line in an added or modified hunk; "coverable" counts the subset the
Cobertura instrument reports as executable, which excludes braces, declarations, comments and XML doc
lines.

## Acceptance summary

| Clause | Required | Observed | Verdict |
| --- | --- | --- | --- |
| all four sections present with numeric values | yes | yes | PASS |
| repository line-rate delta | at or above 0.00 pp | +0.0068 pp | PASS |
| each of four per-file deltas | at or above -0.50 pp | 0.0000, +0.9804, 0.0000, 0.0000 | PASS |
| new/changed-line coverage | at or above 90.00% | 100.0000% | PASS |
