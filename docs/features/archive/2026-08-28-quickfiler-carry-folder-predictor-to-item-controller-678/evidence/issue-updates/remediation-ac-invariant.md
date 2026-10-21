# P2-T11 — `issue.md` acceptance-criteria invariant, remediation cycle 1

Timestamp: 2026-09-02T01-42

PostedAs: unknown

**Reason for `PostedAs: unknown`:** this plan performs no GitHub posting. No issue body was
updated and no comment was created, so there is no GitHub URL and no `IssueUpdatedAt` to
record. This artifact is a local invariant record rather than a mirror of a posted update.

## Clause 1 — SHA-256 digest, byte-identical to `R_ISSUE_DIGEST`

Command: `Get-FileHash -Algorithm SHA256 -LiteralPath <issue.md>`

| Source | Digest |
|---|---|
| `R_ISSUE_DIGEST`, recorded by P0-T3 | `A34C27BB10D2081018E659FFB472D5A7FC9433232BC09FEF837E13FF46E0DD4C` |
| Recomputed now, at the end of the cycle | `A34C27BB10D2081018E659FFB472D5A7FC9433232BC09FEF837E13FF46E0DD4C` |

**Byte-identical.** `issue.md` was not modified by this cycle in any way: no criterion text
was edited, no criterion was added or removed, and no checkbox was transitioned.

The digest comparison is used in place of a base-ref-anchored diff because the previous cycle
already modified `issue.md` relative to `807fb0bb6e5e49f43efa6b256b05960bf078ca19`, so an
anchored diff is non-empty before this cycle does anything and cannot isolate this cycle. A
whole-file digest captured at P0-T3 and recomputed here is the only comparison that does.

## Clause 2 — acceptance-criteria line count

Lines matching `^- \[[ x]\] AC`: **23**, equal to the 23 recorded by P0-T3.

## Clause 3 — checked and unchecked split

| Split | P0-T3 | Now |
|---|---|---|
| Checked (`- [x] AC`) | 22 | **22** |
| Unchecked (`- [ ] AC`) | 1 | **1** |

Equal on both counts.

## Clause 4 — the single unchecked line, re-read verbatim

Line **115**:

```
- [ ] AC20. Coverage does not regress on the changed lines and every new or modified member reaches at least 90% line coverage. Baseline and post-change coverage figures are recorded numerically. No `[ExcludeFromCodeCoverage]` attribute is added or removed anywhere in the change.
```

**Byte-identical** to the AC20 line P0-T3 recorded, at the same line number.

AC20 remains unchecked, as the plan's scope-boundary constraint 2 requires. This cycle does
not attempt it: NB-4 (AC20 per-member coverage) is explicitly deferred out of this cycle by
the remediation inputs, and the reviewer established that the criterion as authored is
unsatisfiable for two COM-bound `QfcQueue` members, because reaching 90 percent on them needs
a seam no criterion authorises while the only alternative is an attribute AC20 itself forbids.
That is a criterion defect and is deferred, not a delivery gap in this cycle.

For the record, and without any bearing on the checkbox: the coverage work this cycle did
perform is recorded in `evidence/qa-gates/remediation-coverage-delta.md`, which shows
changed-line coverage of 34/34 (100.00%) for this cycle's own lines, all seven new or modified
non-exempt members at or above 90 percent, and zero `[ExcludeFromCodeCoverage]` attributes
added or removed.

## Clause 5 — supporting context

- Work-mode marker `- Work Mode: minor-audit` still occurs exactly once, at line 13.
- Heading `## Acceptance Criteria` still occurs exactly once, at line 62.
- `issue.md` is still 186 lines.
- Neither `spec.md` nor `user-story.md` exists in the feature folder, which is the expected
  state for work mode `minor-audit`. SearchScope: the feature root. SearchPatterns: `spec.md`,
  `user-story.md`. SearchResult: none.

## Output Summary

All five clauses hold. The `issue.md` digest is byte-identical to `R_ISSUE_DIGEST`, so the
file is unchanged by this cycle. 23 acceptance criteria, split 22 checked and 1 unchecked; the
single unchecked line is AC20 at line 115, byte-identical to the P0-T3 record. `PostedAs:
unknown` because this plan performs no GitHub posting.
