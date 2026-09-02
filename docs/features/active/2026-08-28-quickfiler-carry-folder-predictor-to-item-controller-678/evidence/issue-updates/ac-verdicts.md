# P2-T13 — Per-criterion verdict register

Timestamp: 2026-09-02T00-34

PostedAs: unknown

**Reason for `PostedAs: unknown`:** no GitHub posting is performed by this plan. The plan contains no
task that posts to issue #678, the executor was given no instruction to post, and `gh` was not
invoked. This artifact is the local mirror of the criterion state; whether and when it reaches the
GitHub issue is decided by the orchestrator that owns the pull request.

## Verdict register — 23 rows, one per criterion

| AC | Verdict | Supporting evidence artifact |
|---|---|---|
| AC1 | **PASS** | evidence/other/carrier-chain.md |
| AC2 | **PASS** | evidence/other/carrier-chain.md |
| AC3 | **PASS** | evidence/baseline/carrier-construction-sites.md |
| AC4 | **PASS** | evidence/other/leg-a.md |
| AC5 | **PASS** | evidence/other/leg-a.md |
| AC6 | **PASS** | evidence/other/leg-b.md |
| AC7 | **PASS** | evidence/regression-testing/ac16-green.md |
| AC8 | **PASS** | evidence/regression-testing/ac16-green.md |
| AC9 | **PASS** | evidence/regression-testing/ac9-negative-guard.md |
| AC10 | **PASS** | evidence/other/carrier-chain.md |
| AC11 | **PASS** | evidence/regression-testing/ac12-path-normalisation.md |
| AC12 | **PASS** | evidence/regression-testing/ac12-path-normalisation.md |
| AC13 | **PASS** | evidence/other/test-reconciliation.md |
| AC14 | **PASS** | evidence/other/carrier-chain.md |
| AC15 | **PASS** | evidence/other/change-description.md |
| AC16 | **PASS** | evidence/regression-testing/ac16-red.md |
| AC17 | **PASS** | evidence/other/test-reconciliation.md |
| AC18 | **PASS** | evidence/other/test-reconciliation.md |
| AC19 | **PASS** | evidence/qa-gates/final-toolchain-pass.md |
| AC20 | **PARTIAL — NOT SATISFIED** | evidence/qa-gates/coverage-delta.md |
| AC21 | **PASS** | evidence/qa-gates/file-size-audit.md |
| AC22 | **PASS** | evidence/other/out-of-scope-register.md |
| AC23 | **PASS** | evidence/qa-gates/scope-confinement.md |

23 rows and no more. Every path is relative to
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/`.

## The only edit made to the `## Acceptance Criteria` section of `issue.md`

**The only edit is the checkbox transition `- [ ]` to `- [x]`**, performed one criterion at a time
per the `acceptance-criteria-tracking` skill, and only on criteria whose supporting evidence artifact
exists and verifies. **No criterion text was reworded, added or removed.**

That is proved rather than asserted. `git diff` of `issue.md` reports **22 insertions and 22
deletions**; a filter for any added or removed line that is *not* of the form
`- [ ] AC<n>.` or `- [x] AC<n>.` returns **0**. An independent check normalises `- [x] AC` back to
`- [ ] AC` in both the pre-edit and post-edit texts and compares them byte-for-byte: the result is
`normalised_identical=True`, so the two files differ in nothing but those checkbox characters.

### A correction made during this task, recorded

The first check-off attempt used `Get-Content` / `Set-Content -Encoding UTF8`, which round-tripped
the file's non-ASCII characters incorrectly and altered four lines **outside** the acceptance-criteria
section: a section sign at line 124 and three em-dashes at lines 154, 155 and 159 were replaced with
ASCII substitutes. That is an unintended edit to `issue.md` and it was caught by the byte-comparison
check above rather than allowed to stand.

The file was restored with `git checkout --` and the check-off redone with byte-level UTF-8 I/O
(`System.IO.File.ReadAllBytes` / `WriteAllBytes` with a no-BOM `UTF8Encoding`), plus a single
occurrence-scoped replacement of the six checkbox characters rather than a line rewrite. The
`normalised_identical=True` result above is from the corrected run.

## Which criteria were checked off

**Checked off (22):** AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8, AC9, AC10, AC11, AC12, AC13, AC14,
AC15, AC16, AC17, AC18, AC19, AC21, AC22, AC23.

**Left unchecked (1): AC20.**

### Why AC20 is left unchecked

AC20 states: "Coverage does not regress on the changed lines and every new or modified member reaches
at least 90% line coverage. Baseline and post-change coverage figures are recorded numerically. No
`[ExcludeFromCodeCoverage]` attribute is added or removed anywhere in the change."

Three of its four clauses hold:

- **No regression on the changed lines — holds.** Repository-wide line coverage moved from 85.3973 %
  to 85.4119 %, branch coverage from 79.4239 % to 79.4494 %. Every non-exempt file's added executable
  lines are 100 % covered except `QfcQueue.Enqueue.cs`, whose uncovered lines are relocated
  pre-existing code that was equally uncovered before the move; the combined `QfcQueue` surface rose
  from 41.47 % to 44.90 %. No file shows a reduction unexplained by a line deletion in that file.
- **Baseline and post-change figures recorded numerically — holds.** `evidence/baseline/coverage-baseline.md`
  and `evidence/qa-gates/coverage-post-change.md`, six attributes each, no placeholders.
- **No attribute added or removed — holds.** `evidence/qa-gates/exclude-attribute-invariant.md`: 0
  added, 0 removed over a diff of 1679 added and 619 removed lines, corroborated by an attribute
  census of 46 on each side.

**The fourth clause fails.** Two modified members are below the 90 % threshold:

| Member | Coverage |
|---|---:|
| `QfcQueue.EnqueueAsync` | 0/46 = 0.00 % |
| `QfcQueue.LoadControllersViewersAsync` | 0/24 = 0.00 % |

Both gained a parameter, so the clause applies to them. Both are COM- and WinForms-bound —
`EnqueueAsync` clones a `TableLayoutPanel` through the UI-idle marshal, `LoadControllersViewersAsync`
dequeues a real `ItemViewer` — and the repository unit-test policy prohibits a test requiring a real
window. Neither is in a class carrying `[ExcludeFromCodeCoverage]`, so no exemption applies, and
AC20 forbids adding one.

The shortfall was reduced rather than accepted where that was possible: the two statements
`LoadControllersViewersAsync` gained both delegate to members at 100 % (`ResolveCarriedHandler`
14/14 and the `ItemControllerFactory` production default 11/11), and the factory seam was narrowed
mid-execution from a concrete `QfcItemGroup` parameter to the `IItemViewer` interface specifically so
its default could be invoked headlessly, taking that member from 8.33 % to 100 %. The two relocated
members cannot be reached the same way without a headless seam over `AddAsync` and the UI-idle
marshal, which no acceptance criterion authorises.

**AC20 is therefore recorded as PARTIAL and its checkbox is left `- [ ]`.** It is not dispositioned
into a pass.
