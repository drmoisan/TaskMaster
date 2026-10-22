# Policy Audit (Remediation Cycle 1 Exit Reaudit) — utilitiescs-test-determinism (Issue #811)

- Date: 2026-09-08T16-05
- Branch: `bug/utilitiescs-test-determinism-780-803-594-811`
- Head: `41005c6ba889fd645b1aa4058ba43168a6ff12e4`
- Base: `origin/main` @ `e6fc0e79be93e72bb5007fcd4f4314675470b073` (merge base, confirmed identical)
- Work mode: `full-bug` — `spec.md` is the sole acceptance-criteria source
- Cycle-entry artifacts (not superseded; retained as the cycle-entry record):
  - `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/policy-audit.2026-09-08T11-30.md`
  - `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/code-review.2026-09-08T11-30.md`
  - `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/feature-audit.2026-09-08T11-30.md`
  - `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/remediation-inputs.2026-09-08T11-30.md`

This is a reaudit artifact. It does not restate the passing checks established at
`2026-09-08T11-30`; those remain in force and are referenced rather than repeated. It independently
carries the R-1 disposition, the acceptance-criteria evaluation, and the blocking-finding count.

## Executive Summary

**BLOCKING FINDINGS: 0.**

The single blocking finding from cycle entry (F-1 / R-1) is **CLOSED**. The durable follow-up
artifact `docs/features/potential/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race.md`
(111 lines) now exists on the branch, and every content element that
`remediation-inputs.2026-09-08T11-30.md` required of it was re-verified line-by-line against the
post-change source in this session, not accepted from the closure evidence.

No new blocking finding was introduced by the remediation commit or by the two `origin/main`
merges. The source footprint is unchanged at exactly 20 paths. All non-blocking findings F-2
through F-6 carry forward with their cycle-entry dispositions; two minor documentation-accuracy
observations (F-7, F-8) and one PR-authoring guardrail (F-9) are added, none blocking.

Verdict: **PASS**. The branch is clear to merge on the evidence recorded across both cycles.

## Scope Confirmation

The audited scope is the full branch diff against the resolved base branch, not any plan, task, or
caller-supplied subset.

- Base resolution: `artifacts/pr_context.summary.txt` records base `origin/main @ e6fc0e79`, head
  `41005c6b`, and merge base `e6fc0e79`. Head SHA in the artifact matches the branch HEAD, so the
  PR context is **fresh, not stale**, and no regeneration was required.
- Source footprint independently re-derived from `coverage/review-811-source-final.patch` by
  enumerating every `diff --git` header: **exactly 20 paths** — 12 under `UtilitiesCS.Test/`
  (including `UtilitiesCS.Test.csproj`) and 8 under `UtilitiesCS/`. This matches the pre-merge
  footprint, confirming the two `origin/main` merges did not alter this item's change.
- Documentation footprint independently re-derived from `coverage/review-811-docs-namestatus.txt`:
  57 paths, all status `A`, all under `docs/`.

### Rejected Scope Narrowing

**None detected.** The delegation prompt framed this as a remediation exit gate but explicitly
directed evaluation of any new blocking finding across the whole branch, including anything
introduced by the merges. It did not narrow the file set, exclude a language, or mark any coverage
check inapplicable.

The prompt did assert as fact that "the source footprint is still exactly the same 20 paths" and
that "no source file was touched by the remediation cycle." These assertions were **not accepted on
trust**; both were re-derived independently from the pre-generated patch and the docs name-status
listing, and both are confirmed correct.

## Evidence Location Compliance

- Scanned the full branch diff for files written under `artifacts/baselines/`, `artifacts/qa/`,
  `artifacts/evidence/`, or `artifacts/coverage/`: **zero occurrences** in
  `coverage/review-811-source-final.patch` and zero in the docs name-status listing.
- All feature evidence is under the canonical
  `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/evidence/<kind>/`
  tree, including the new `evidence/other/remediation-cycle-1-r1-closure.md`.
- `EVIDENCE_LOCATION_OVERRIDE_REJECTED`: none. No caller instruction specified a non-canonical
  evidence path.

Verdict: **PASS**.

## 1. General Unit Test Policy Compliance

No test file changed in this cycle. The cycle-entry evaluation stands unchanged. See
`policy-audit.2026-09-08T11-30.md` § 1.

Verdict: **PASS** (carried forward).

## 2. General Code Change Policy Compliance

No source file changed in this cycle; the 500-line file-cap evaluation, error-handling, and naming
assessments are unchanged. See `policy-audit.2026-09-08T11-30.md` § 2.

One point re-confirmed because the remediation cycle could have breached it: the CLAUDE.md Bugfix
Workflow minimal-fix requirement. R-1 deliberately introduced **no** source change; the `ILGlobals`
race is documented for separate RED-first work rather than opportunistically fixed on this branch.
That is the correct disposition and is verified by the unchanged 20-path footprint.

Verdict: **PASS** (carried forward).

## 3. Language-Specific Code Change Policy Compliance (C#)

No C# file changed in this cycle. Toolchain gate evidence from the entry cycle
(`evidence/qa-gates/p7-t1` through `p7-t5`) remains the governing record: csharpier format and
check, msbuild analyzers, msbuild nullable, and vstest all passed in a single final pass.

Verdict: **PASS** (carried forward).

## 4. Language-Specific Unit Test Policy Compliance (C#)

MSTest, Moq, and FluentAssertions usage unchanged. See `policy-audit.2026-09-08T11-30.md` § 4.

Verdict: **PASS** (carried forward).

## 5. Test Coverage Detail

Coverage is mandatory for every language with changed files in the branch diff. **C# is the only
language with changed files** (20 paths, all `.cs` or `.csproj`). Its verdict is stated explicitly
below and is not deferred, waived, or marked informational.

Note on the changed-language source: `artifacts/pr_context.summary.txt` § "Changed files overview"
reports `Core logic changes: 0 files` and classifies all 57 listed paths as
`Docs/templates/agents/tooling`, despite 20 C# source paths in the branch diff. This is the known
recurring pr-context classifier defect. It was **not** allowed to narrow this audit's coverage
scope; the changed-language set was derived from the actual diff.

Figures re-derived independently in this session from the Cobertura root elements, not quoted from
the entry-cycle artifact:

| Artifact | line-rate | lines | branch-rate | branches |
|---|---|---|---|---|
| `coverage/p7-final.cobertura.xml` (post) | 0.8604239666249645 | 172626 / 200629 | 0.6639784946236559 | 21489 / 32364 |
| `coverage/p0-baseline.cobertura.xml` (baseline) | 0.8601092896174863 | 172353 / 200385 | 0.6630576006929406 | 21434 / 32326 |

- **C# line coverage 86.0424% — PASS** (floor >= 85%; baseline 86.0109%, delta +0.0315 pt, no regression).
- **C# branch coverage 66.3978% — FAIL** (floor >= 75%; baseline 66.3058%, delta +0.0920 pt). Pre-existing
  repository-wide condition, improved by this change, not introduced by it. Disposition
  non-blocking under F-2; see § 8.
- C# canonical coverage artifact `artifacts/csharp/coverage.xml` is **absent — FAIL** on the
  artifact-presence rule. Coverage substance was nonetheless verified directly from the committed
  Cobertura pair above, so the disposition is procedural and non-blocking under F-5.
- Changed-line coverage on the 8 production files: no regression; see
  `evidence/qa-gates/p7-t6-coverage-delta.md`.
- PowerShell, Python, TypeScript: **zero changed files in the branch diff**, so no coverage
  obligation arises for these languages.

## 6. Test Execution Metrics

Unchanged from cycle entry. The AC4 ten-run record is at
`evidence/regression-testing/p8-t6-ac4-ten-run.md`: nine of ten runs clean, run 7 carrying the
single `ILGlobals` race failure that R-1 documents. No test was re-run to green to mask it.

## 7. Code Quality Checks

Unchanged from cycle entry. See `code-review.2026-09-08T16-05.md` for this cycle's delta review.

## 8. Gaps and Exceptions

### F-1 — CLOSED (was BLOCKING) — The `ILGlobals` static race now has a durable follow-up artifact

The cycle-entry rationale for blocking was, verbatim: the defect "currently exists only as prose
inside `<FEATURE>/evidence/`, which is archived at merge."

That condition no longer holds. `docs/features/potential/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race.md`
is a tracked, committed file in the repository's canonical follow-up intake queue, outside the
feature folder that is archived at merge. A search of `docs/features/potential/` for `ILGlobals`,
`SDIL`, and `MethodBodyReader` returned zero matches at cycle entry and now returns exactly one
file: the new entry.

**Content verification.** Each element required by `remediation-inputs.2026-09-08T11-30.md` was
re-verified against the post-change source in this session:

| Required element | Entry location | Verified against source | Result |
|---|---|---|---|
| Mechanism: `ILGlobals.cs:123` reassigns to all-default `OpCode[0x100]`, fill loop at `:137` | Actual Behavior 1 | `ILGlobals.cs` lines 123 and 137 read directly | Correct |
| Lines 117-118 declare both tables as plain mutable `public static` | Actual Behavior 1 | Lines 117-118 read: `public static OpCode[] multiByteOpCodes` / `singleByteOpCodes`, no modifier | Correct |
| No `lock`, no `Lazy<T>`, no `volatile`, no static constructor | Actual Behavior 1 | Full class body 110-149 inspected; none present | Correct |
| Reader sites `MethodBodyReader.cs:105` and `:110`, unsynchronised | Actual Behavior 2 | Lines 105 and 110 read the two tables directly, no guard | Correct |
| `ILGlobals_Tests.cs` calls `LoadOpCodes()` at lines 15, 26, 37, 47 | Actual Behavior 3 | All four call sites confirmed at exactly those lines | Correct |
| `MethodBodyReader_Tests.cs:364` inside private `CreateReader`, which every test routes through | Actual Behavior 3 | `CreateReader` at 362-366 with `LoadOpCodes()` at 364. All 14 `[TestMethod]`s route through it: 10 call `CreateReader`/`CreateReaderWithoutBody` directly, the other 5 via `ConstructCustomInstructions` at line 373 | Correct — claim independently falsification-tested and upheld |
| Neither class carries `[DoNotParallelize]` | Actual Behavior 3 | Grep for `DoNotParallelize` returns zero matches in both files | Correct |
| Assembly `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]` at `AssemblyInfo.cs:18-21` | Actual Behavior 3 | Confirmed verbatim at lines 18-21 | Correct |
| Observed AC4 run-7 signature and why it indicates a partial table | Actual Behavior 4 | `GetBodyCode_ReturnsConcatenatedInstructions` at line 62 asserts `Contain("ldstr")` at line 74. `OperandType.InlineBrTarget == 0` is the default enum value, and the `InlineBrTarget` branch at `MethodBodyReader.cs:119-123` stores a numeric operand with no `ResolveString`. 1879067923 = 0x70004D13, inside the 0x70 String-token range | Correct |
| Frequency statement with base-rate caveat | Actual Behavior 5 | Matches the ten-run evidence; explicitly declines to claim a base rate | Correct |
| Three candidate fixes in preference order | Proposed Fix | Immutable publication, then `lock`, then `[DoNotParallelize]` marked interim-only | Correct and in the required order |
| Impact statement | Impact / Severity | States the `mstest-coverage` intermittent-failure harm and the misdiagnosis risk | Correct |

The entry additionally carries the promotion-tooling section headings unchanged, so it is
well-formed for the bug-report template mapping.

**Disposition: CLOSED.** See the explicit adjudication of the withheld issue-creation half below.

### F-1a — Adjudication: does the durable artifact alone discharge the finding?

The R-1 acceptance criteria had two halves. The first (author the entry with specified content) is
met and verified above. The second (create a GitHub issue and record its number) was deliberately
withheld; `evidence/other/remediation-cycle-1-r1-closure.md` § "GitHub Issue Creation — Deferred"
records the reason, and the entry's `## Next Step` promotion checkbox is left unchecked.

**I accept this as closed, not still blocking.** Reasoning, stated plainly:

1. The blocking rationale I recorded was specifically about **durability past merge**, not about
   the existence of a GitHub issue number. `docs/features/potential/` is tracked in version control
   and is not archived at merge, so the stated defect condition is fully remedied.
2. Filing the issue is not this agent's action to take. The parent delegation prompt directs that
   follow-up defects be enumerated for the operator, and the promotion route is hook-gated. A
   reviewer holding a cycle open for an action the reviewer is instructed not to perform, and that
   the tooling gates, would deadlock the cycle.
3. The unchecked promotion checkbox is the repository's normal representation of a pending
   promotion, not an anomaly. Of the entries under `docs/features/potential/`, unchecked promotion
   boxes are the prevailing state — including entries already relocated into
   `docs/features/potential/promoted/`, which retain an unchecked box. The checkbox therefore
   carries no reliable signal of non-promotion, and its state cannot bear a merge gate.
4. Precedent within this same branch is consistent: the two sibling follow-ups filed by plan task
   P8-T9 (`2026-09-08-etl-deadline-mechanics-follow-ups.md` and
   `2026-09-08-console-out-aggressors-and-banned-symbol-promotion.md`) are likewise unpromoted.
   Treating the third differently would be arbitrary.

**Outstanding operator action (not a merge gate):** promote all three
`docs/features/potential/2026-09-08-*` entries to GitHub issues through the standard promotion
lifecycle, and record the resulting issue number in each entry.

### F-2 — Non-blocking (carried forward) — Repository branch coverage below the 75% floor

66.3978% against a 75% floor. Pre-existing (baseline 66.3058%) and improved by this change.
Recorded as FAIL in § 5 per the coverage rule; disposition remains non-blocking, since gating a
three-defect determinism bugfix on a repository-wide branch-coverage programme is not proportionate.
The unreconciled CLAUDE.md 80%/no-branch versus `.claude/rules` 85%/75% conflict also persists and
is a documentation defect independent of this branch.

### F-3 — Non-blocking (carried forward) — One test performs two Acts

`EnumerateTable_WritesFormattedOutputToSuppliedWriterAndMovesToStart`. Unchanged.

### F-4 — Non-blocking (carried forward) — `MessageBoxInvoker` remains a mutated process-wide static

Unchanged. No concurrent reader today; the invariant remains unenforced.

### F-5 — Non-blocking (partially resolved) — Canonical coverage artifact absent

**Improved this cycle.** `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`
are now present and fresh (head SHA matches branch HEAD). `artifacts/csharp/coverage.xml` remains
absent; coverage substance is verified from `coverage/p7-final.cobertura.xml`. Procedural only.

### F-6 — Non-blocking (carried forward) — AC4 literal condition not met

Correctly left unchecked. Re-evaluate after the `ILGlobals` race is fixed under its own issue.

### F-7 — Non-blocking (new) — Source-path split misstated in the closure evidence

`evidence/other/remediation-cycle-1-r1-closure.md` line 17-18 states the unchanged footprint is
"11 under `UtilitiesCS.Test/`, 9 under `UtilitiesCS/`". The correct split is **12 and 8**
(`UtilitiesCS.Test/UtilitiesCS.Test.csproj` falls under the test project, and a `UtilitiesCS.Test/`
path does not match a `UtilitiesCS/**` glob). The total of 20 and the document's conclusion — that
this cycle introduced no source change — are both correct and independently confirmed. Documentation
accuracy only; no impact on the closure claim.

### F-8 — Non-blocking (new) — Minor imprecision in the entry's operand description

The entry says the `InlineBrTarget` branch "stores a raw metadata token". The branch at
`MethodBodyReader.cs:119-123` actually stores `ReadInt32(...) + position`, that is, the token plus
the current IL offset. Because `position` is a small offset within a short method body, the stored
value remains inside the 0x70 String-token range, so the entry's diagnostic conclusion is unaffected
and correct. Worth a one-line correction if the entry is promoted, so a future investigator
reconciling the printed value against a token table is not misled by an off-by-`position` delta.

### F-9 — Non-blocking (new) — Autoclose candidate list is over-broad

`artifacts/pr_context.summary.txt` § "Close candidates" lists author-asserted autoclose issues
`#181, #520, #592, #594, #779, #780, #798, #802, #803, #809, #811, #812` and a spurious `#SHA-256`.
This list is scraped from document text, not authored intent. Per `spec.md`, only **#780, #803,
#594** are superseded and only **#811** is the item itself; **#592 is explicitly out of scope and
must remain open**, and #181, #520, #798, #779 are related prior work, not closable here. No PR body
exists yet, so nothing is committed — this is a guardrail for PR authoring, not a branch defect.

## 9. Summary of Changes

This cycle: one new file (`docs/features/potential/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race.md`,
111 lines), one new evidence file (`evidence/other/remediation-cycle-1-r1-closure.md`), and the
commit of the four previously untracked `2026-09-08T11-30` audit artifacts. Two `origin/main` merges
were absorbed, the most recent bringing sibling item 810 (PR #819, QuickFiler only), which does not
intersect this item's 20 source paths.

Zero source files changed in this cycle.

## 10. Compliance Verdict

| Area | Verdict |
|---|---|
| Scope confirmation (full branch vs base) | PASS |
| Evidence location compliance | PASS |
| General unit test policy | PASS |
| General code change policy | PASS |
| C# code change policy | PASS |
| C# unit test policy | PASS |
| C# line coverage (86.0424%) | PASS |
| C# branch coverage (66.3978%) | FAIL — pre-existing, improved, non-blocking |
| C# coverage artifact presence | FAIL — procedural, substance verified, non-blocking |
| Acceptance criteria (4 of 5 met, AC4 correctly open) | PASS |
| **Blocking findings** | **0** |

**Overall: PASS.** Remediation cycle 1 exit criteria are satisfied.
