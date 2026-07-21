# Final QC — Scope Guards

Timestamp: 2026-07-19T06-35

## Files over 500 lines — NOT split
| File | Line count (post-change) | Split? |
|---|---|---|
| Bayesian/BayesianClassifierShared.cs | 1016 | NO (single file; was ~1008, +pragma/annotations) |
| Bayesian/BayesianClassifierGroup.cs | 518 | NO (was ~515) |
| ClassifierGroups/Categories/CategoryClassifierGroup.cs | 525 | NO (was ~523) |
| Flags/FlagParser.cs | 634 | NO (was ~633) |
| Bayesian/Performance/BayesianPerformanceMeasurement.cs | 1548 | NO (was ~1537) |

Each remains a single file; none was split. Line-count growth is from the `#nullable enable` pragma, nullable annotations, and justified `!`/comment additions only.

## FolderHierarchyNode shape preserved
`grep 'sealed record FolderHierarchyNode'` -> line 18: `public sealed record FolderHierarchyNode`. No `init` accessor present (grep for `\binit\b` returns none). The get-only auto-properties set in the `[JsonConstructor]` constructor and the `?? throw new ArgumentNullException` guards are unchanged.

## No new init / record / record struct introduced
Command: `git diff df2235bc -- 'UtilitiesCS/EmailIntelligence/**/*.cs' | grep '^+' | grep -E 'record struct|{ get; init;|init;'`

Result: ZERO — no `init` accessor, positional `record`, or `record struct` was introduced anywhere in the remediated set (the existing `record` DTOs in BayesianMetricTypes.cs and the existing `sealed record FolderHierarchyNode` are unchanged in shape).

**All scope guards SATISFIED** (AC3/AC5 scope compliance).
