# Feature Audit — Issue #635 Residual Reflective-Caller Risk

- **Issue:** #635
- **Branch:** `bug/issue-468-residual-reflective-caller-risk-635`
- **Base / merge base:** `main` / `b56400ab663a85b6039139d4548f408821e957ce`
- **Head reviewed:** `73bd8082e1776d7957ca0c9a3226b3587e4a658f`
- **Work mode:** `full-bug`
- **AC source:** `spec.md` only
- **Timestamp:** 2026-08-29T06-50
- **Verdict:** GO — 15 of 15 acceptance criteria PASS, 0 blocking findings

## Work Mode Resolution

`issue.md` line 14 carries the marker `- Work Mode: full-bug`. Under the acceptance-criteria-tracking
protocol, `full-bug` resolves the acceptance-criteria source to `spec.md` **only**. `user-story.md` is
deliberately absent from this feature folder; its absence is correct for this work mode and is not
recorded as a defect.

`spec.md` declares 15 acceptance criteria, AC-1 through AC-15, all in `- [x] **AC-n**` checkbox form.

## Verification Method

Every criterion was evaluated against its cited evidence, and every load-bearing measurement in that
evidence was **re-executed at review head** rather than accepted from the artifact. A criterion is
marked PASS only where the cited evidence exists, says what the criterion requires, and reproduces.

Where a recorded figure has drifted since execution, the criterion is assessed against whether the
drift touches an asserted value. In every case in this item, it does not.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence and verification |
|---|---|---|
| AC-1 | **PASS** | `evidence/baseline/p0-t4-identifier-derivation.2026-08-29T04-55.md`. Re-ran `git show --stat 63eebd47`: one source file changed, pure deletion of 241 lines, subject names `_templateTlp`. The 13-row table quotes a removed declaration line per identifier — 12 method declarations plus `-        private TableLayoutPanel _templateTlp;`. `IDENTIFIER_ROWS: 13`. |
| AC-2 | **PASS** | `evidence/other/p1-t1-partition-a-sweep.2026-08-29T04-55.md`. Re-ran the verbatim command at review head: no output, `EXIT: 1`. Command, verbatim output `(no output)`, and exit code are all recorded, with `ExpectedExitCode: 1` declared per file. |
| AC-3 | **PASS** | `evidence/baseline/p0-t5-scope-census.2026-08-29T04-55.md`. Re-measured: `SCOPE_FILES 683`, `AC16_SIX_EXTENSION_SCOPE 153`, `TRACKED_CS 1599`, and the 12-row extension census — all reproduce exactly. `WIDENING_DELTA 530`. The scope is non-empty and its census spans 8 extensions outside AC-16's six, so the AC-2 zero is non-vacuous. |
| AC-4 | **PASS** | `evidence/other/p1-t3-partition-b-classification.2026-08-29T04-55.md`. Recorded `TOTAL=2337`, `CAT_D_DOCS=2319`, `CAT_E_CLAUDE=18`; sum identity holds. Re-ran at review head: `TOTAL=2474`, `2456 + 18 = 2474`. Identity holds at both commits. Categories are path-derived. |
| AC-5 | **PASS** | Same artifact. `CAT_G_OTHER=0` at execution and `0` on re-run. The mechanical test is stated: path begins `docs/` (D), path begins `.claude/` (E), residue (G), applied in that order on the `path:line:text` string, with no reading of hit text. Independently corroborated by `[P1-T1]`, whose pathspec is exactly the category G population and which measures it empty by a different route. |
| AC-6 | **PASS** | `evidence/other/p1-t4-partition-c-enumeration.2026-08-29T04-55.md`. Re-ran: `PARTITION_C_HITS: 31`, `DISTINCT_FILES: 12`. **Enumerated rows counted individually: exactly 31**, numbered 1-31, each with file, line, matched identifier, and category. `CAT_A 2 + CAT_B 28 + CAT_C 1 + CAT_G 0 = 31`. A residual-category probe returned empty, independently confirming `CAT_G: 0`. |
| AC-7 | **PASS** | `evidence/other/p1-t5-untracked-pass.2026-08-29T04-55.md`. `UNTRACKED_FILES=9` with all nine enumerated as `FILE` lines, and `UNTRACKED_HIT_FILES_OUTSIDE_SCOPE=0`. Five hit files are listed with per-file counts before the carve-out test is applied, so the result is auditable and the carve-outs exclude nothing from the enumeration. |
| AC-8 | **PASS** | `evidence/other/p2-t1-reflection-inventory.2026-08-29T04-55.md`. 17 patterns with production and test counts reported separately. Re-measured `QF_PROD_SCOPE_FILES 228`, `QF_TEST_SCOPE_FILES 151`, `GetField( test=172 prod=0`, `GetMethod( test=69 prod=0`, `GetProperty( test=24 prod=0` — exact. The `GetField(`/`GetFields(` family AC-16 omitted is present as rows 7 and 8. All 16 name-resolving patterns record `prod=0`; the combined production sweep exits 1. |
| AC-9 | **PASS** | `evidence/other/p2-t3-variable-argument-closure.2026-08-29T04-55.md`. Eight sites named individually by file and line with API, argument form, and closure statement. All eight verified in source. The stated limit — a name assembled at run time by concatenation or interpolation — is recorded in a dedicated section. See the divergence note below; the criterion is discharged by a superset. |
| AC-10 | **PASS** | `evidence/other/p3-t1-ac16-corrections.2026-08-29T04-55.md`. `AC16_CORRECTIONS: 2`. Correction 1 verified against the commit. Correction 2 verified against the original artifact at `docs/features/active/qfc-collection-controller-defects-468/evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md:205`, which reads "Zero hits anywhere in `QuickFiler.Test`". The superseding occurrence is identified by file, line, and category. |
| AC-11 | **PASS** | `evidence/other/p3-t4-zero-result-audit.2026-08-29T04-55.md`. `ZERO_RESULT_SEARCHES: 37` with 37 enumerated rows; composition sums `1+1+1+1+16+8+1+8 = 37`. Every row carries `SearchScope:`, `SearchPatterns:`, `SearchResult:`, and a measured scope size. Smallest scope 9, largest 10,274; none is zero. |
| AC-12 | **PASS** | `evidence/qa-gates/p4-t2-no-modification-proof.2026-08-29T04-55.md`, plus independent verification. Re-ran `git diff --name-only origin/main...HEAD` over the **final** head: 32 paths, `NON_MD_COUNT: 0`. `git status --porcelain` clean before my writes. No production, test, or build-input file is modified. |
| AC-13 | **PASS** | `evidence/other/p3-t3-decision-record.2026-08-29T04-55.md`. `DECISION: RESIDUAL RISK CLOSED`, with a nine-row table of caller classes proved absent, each citing its evidence, and a dedicated section naming the one class not proved absent. No caller was found, so the name-the-caller branch correctly does not apply. |
| AC-14 | **PASS** | `evidence/regression-testing/fail-before-exception.2026-08-29T04-55.md`. States why a failing run is structurally impossible (no executable change; the assertion is a tautology at both ends) and supplies the non-vacuity measurement as the alternative proof, citing five artifacts by path. Located in the canonical `evidence/regression-testing/` directory as the specification requires. |
| AC-15 | **PASS** | `evidence/qa-gates/p4-t3-toolchain-gate.2026-08-29T04-55.md`. `TOOLCHAIN_BRANCH: 2`, language composition stated, C# and PowerShell gates each marked not applicable with the reason "no in-scope file" and an evidence pointer. Branch selection assessed against the actual 32-path diff, not by re-running the gates: zero paths carry a C# or PowerShell extension, so branch two is correct. |

**Result: 15 PASS, 0 PARTIAL, 0 FAIL, 0 UNVERIFIED.**

## Checked Criteria Whose Evidence Was Challenged

The review directive required that a checked criterion whose evidence does not support it be reported
as a blocking finding. Four criteria were examined specifically for the "could not have failed" shape:

- **AC-2 (the zero result).** Could have failed. Its scope is measured at 683 files and the `[P1-T2]`
  control proves that scope reaches real content, including an extensionless file and a `.bak` file
  that no extension-based search could reach. A vacuous pathspec would have shown zero control hits.
- **AC-4/AC-5 (total classification).** Could have failed. `CAT_G_OTHER` is a residue category: any hit
  outside the two prose trees increments it. The Partition A sweep tests exactly that population by an
  independent route and also finds it empty.
- **AC-6 (enumeration).** Could have failed. The row count was verified by counting, and the residual
  probe I ran independently would have surfaced any hit outside the three named category tests.
- **AC-12 (Markdown-only diff).** Could have failed, and is the one criterion whose subject is the
  branch itself. Verified over all 32 final paths.

None of the four is vacuous. No criterion is checked without supporting evidence.

## Recorded Divergences

Both were recorded by the executor rather than silently resolved, and both dispositions are judged
correct. Full reasoning is in `policy-audit.2026-08-29T06-50.md` section 13.

1. **AC-9 names six sites; the derivation yields eight.** Seven variable-argument `GetField(` sites plus
   one `GetMethod(` site, so no six-element subset is identifiable with the specification's six. All
   eight are enumerated with closure statements, which over-satisfies the criterion. Editing the
   approved specification is prohibited by the acceptance-criteria-tracking protocol, so recording in
   evidence was the only compliant route. AC-9 is discharged by a superset. Carried forward as a
   maintainer amendment request (NB-4).

2. **Reference drift from `b56400ab` to `d6cfb21c`.** `TRACKED_TOTAL` 11866 to 11873 and Partition B
   `TOTAL` 2229 to 2337, both explicitly non-asserted reference values. No asserted value moved. This
   review supplies a third data point at head: `TRACKED_TOTAL` is now 11895 and Partition B `TOTAL` is
   now 2474, while `SCOPE_FILES` is still 683, `AC16_SIX_EXTENSION_SCOPE` still 153, `TRACKED_CS` still
   1599, Partition C still 31, and both Partition B identities still hold. The asserted values are
   structurally immune to the drift because the Partition A pathspec excludes both trees into which
   this branch writes. Blocking on drift in a deliberately non-asserted value would have been wrong.

## Baseline Relationship

The item is an evidence-producing audit that discharges follow-up candidate 9 of the issue #468
specification and the verification obligation of issue #468 AC-16. Relative to the `main` baseline:

- **Behavior delta: none.** No executable line changes; nothing in the product behaves differently.
- **Coverage delta: none.** No production or test code is touched, so no changed line can regress.
- **Documentation delta:** 18 evidence artifacts, a specification, a plan, a research document, an
  issue record, and 6 agent-memory files.
- **Obligation delta:** issue #468 AC-16's residual-risk gap is closed on measured evidence, with one
  named residual class (runtime-assembled member names) that is explicitly not closed.

## Acceptance Criteria Status

- Source: `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md`
- Total AC items: 15
- Checked off (delivered): 15
- Remaining (unchecked): 0
- Items remaining: none

All 15 criteria were already checked off by the executor. This review verified each against its cited
evidence and confirms every check-off is supported. No checkbox was changed by this review, because
none required changing.

## Verdict

**GO.** 15 of 15 acceptance criteria PASS. 0 blocking findings. 8 non-blocking observations are
recorded in `code-review.2026-08-29T06-50.md`; none of them affects a conclusion, and three of them
(NB-2, NB-3, NB-5) identify claims whose supporting measurement was narrower than the claim, in each
case verified by this review to hold under the broader measurement.

No remediation inputs artifact is produced, because no finding requires remediation.
