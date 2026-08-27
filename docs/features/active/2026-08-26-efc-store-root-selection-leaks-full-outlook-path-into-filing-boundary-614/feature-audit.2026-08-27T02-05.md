# Feature Audit: Issue #614 post-remediation cycle 2

**Audit Date:** 2026-08-27
**Feature Folder:** `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`
**Base Branch:** `main` / resolved `origin/main`
**Head Branch:** `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614` at `8188cff9537125255bdd0415ce4b9b701c138c99`
**Work Mode:** `full-bug`
**Audit Type:** Full post-remediation acceptance verification.

## Scope and Baseline

- **Base branch:** `main`; PR collector resolved `origin/main` at `8b70208032519d82fe838009a5ce280f18b277f9`.
- **Head:** `8188cff9537125255bdd0415ce4b9b701c138c99`.
- **Merge base:** `c279d40bddacdba00c29a9724d1b5b17f9ebbc90`.
- **Primary evidence:** `artifacts/pr_context.summary.txt`, generated for this head and base.
- **Secondary evidence:** `artifacts/pr_context.appendix.txt`, full changed-file and commit anchors.
- **Feature evidence:** canonical `evidence/baseline`, `evidence/remediation-baseline`, `evidence/regression-testing`, `evidence/qa-gates`, `evidence/issue-updates`, and `evidence/other` folders.
- **Requirements source:** `spec.md` only. `issue.md:15` persists `- Work Mode: full-bug`, which resolves acceptance tracking to `spec.md` under the repository skill.
- **Scope:** Complete 147-file feature diff, all C# production/test/project changes, feature documentation, and all 26 acceptance criteria. Review coverage was not narrowed. The subsequent user-approved `scope_change` affects only remediation disposition for two documentation/evidence findings.
- **QA standing:** Underlying authoritative final run is CSharpier 1,530 files; analyzer and nullable rebuilds zero errors; 6,586/6,586 tests; 84.8841% line and 78.8692% branch coverage. The final-test artifact's mixed expectation metadata is evaluated separately below.
- **Accepted-risk decision:** Checkpoint requirement `issue-614-approved-documentation-findings-scope-change` records approval at `2026-08-27T02:11:56.137Z`, after this full re-review and before documentation remediation or PR authoring. It excludes exactly the mixed-expectation evidence representation and the #637/#638 change-description reference from remaining remediation scope.

## Acceptance Criteria Inventory

**Authoritative AC source:**

- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/spec.md` — sole source for `full-bug` mode.

### Acceptance criteria

1. **AC1 — Contract type:** pure `ArchiveStemContract`, explicit project include, required API, drive-root decision, under 500 lines.
2. **AC2 — D1:** hierarchy activation cannot store a verbatim out-of-root path; selection remains unchanged and diagnostic emitted.
3. **AC3 — D2:** store-root, cross-store, at/above-root, valid ancestor/child, and leaf activation behavior.
4. **AC4 — D3:** direct-row pass-through guarded; out-of-root row cannot leak; hierarchy lookup cannot fabricate an out-of-root identity.
5. **AC5 — D4:** both filing-boundary overloads validate before concatenation; `GetStem` and `IsDeleteRelevant` use anchored, separator-aware comparisons.
6. **AC6 — D5a:** only derived segments are validated; legitimate dotted/hyphenated filesystem ancestor succeeds.
7. **AC7 — D5b:** invalid filename characters, trailing dot/space, and device names are enforced without banning `.`, `[`, or `]`.
8. **AC8 — D5c:** no drive-prefix substring assumption; UNC and short ancestors are safe.
9. **AC9 — D5d:** ancestor strip is anchored, separator-aware, and case-insensitive.
10. **AC10 — D5e:** converter exception does not leak Outlook or filesystem identifiers.
11. **AC11 — D5f:** remove-illegal-characters option preserves legal characters.
12. **AC12 — D5g:** root resolution uses separator-bounded prefixes; dead `ask` parameter removed or documented.
13. **AC13 — D6:** archive root is validated once and fails explicitly/redacted when unresolvable or cross-store.
14. **AC14 — D7:** OneDrive fallback is explicit/redacted, environment access is injectable, and `MatchBestSpecialFolder` is unchanged.
15. **AC15 — D8:** data-model stem derivation uses the shared contract and covers under-root/root/cross-store/case variants.
16. **AC16 — D9:** filing and creation routes use two scope-specific predicates; filing rejects rooted/sentinel/blank values without a length minimum; creation adds the three-character minimum.
17. **AC17 — Primary regression:** named `ResolvePaths` test has fail-before/pass-after and redaction evidence.
18. **AC18 — Producer companion:** store-root segment activation cannot store the full Outlook path and has fail-before evidence.
19. **AC19 — #609/#439 regression set:** named scenarios remain green and `FolderPredictor.cs` is unchanged.
20. **AC20 — #499 interaction:** rejection preserves prior selection and does not absorb selection-clearing work.
21. **AC21 — Redaction:** no real mailbox, account, host, organization, or user-profile identifier in changed content.
22. **AC22 — Test policy:** MSTest, Moq where collaborators exist, FluentAssertions, AAA, deterministic and isolated tests, no temporary files.
23. **AC23 — Coverage:** changed/new code at least 90%, baseline/post metrics captured, and no confirmed changed-line regression.
24. **AC24 — Full four-step toolchain:** exact commands, exit codes, clean sequence, non-vacuous rebuilds, and coverage-mode test record.
25. **AC25 — Scope/file size:** approved in-scope modifications, named protected paths unchanged, and file-size policy enforced under the recorded existing-file adjudications.
26. **AC26 — Manual validation:** five live-profile steps executed or explicitly recorded as not executable with reasons.

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---:|---|---|---|---|---|
| AC1 | Contract type | PASS | `ArchiveStemContract.cs`; `UtilitiesCS.csproj`; 147 lines; pure API and drive-root rationale | source inspection; compile-include `rg` | 100% reported coverage. |
| AC2 | D1 hierarchy rejection | PASS | `SelectHierarchyPath`; `SegmentActivate_StoreRootAncestor_LeavesSelectionUnchangedAndDiagnoses` | targeted router evidence and full suite | Prior selection remains non-null and unchanged. |
| AC3 | D2 activation matrix | PASS | `BreadcrumbBridgeRouterIssue614Tests` | `p3-t5-router-tests`; full runner | Root, cross-store, root-exact, valid ancestor/child, and leaf covered. |
| AC4 | D3 direct row/lookup guard | PASS with follow-up | `SelectRow`, `ToHierarchyPath`, out-of-root row test | full diff inspection; full runner | At/under-root rooted direct rows remain verbatim but the filing guard rejects them; producer normalization is #637. |
| AC5 | D4 filing boundary | PASS | both `ResolvePaths` overloads, `GetStem`, `IsDeleteRelevant`; four boundary tests and RC-4 | `rc4-getstem` and final runner | Composition test confirms accepted guard values do not throw. |
| AC6 | D5a derived-only validation | PASS | converter and dotted/hyphenated root tests | `p5-t7-converter-tests` | Filesystem ancestor is not passed to segment validator. |
| AC7 | D5b Windows rules | PASS | `FindInvalidSegmentRule`; positive/negative tests for four rule groups | targeted converter evidence | `.`, `[`, and `]` no longer blanket-banned. |
| AC8 | D5c no substring assumption | PASS | converter no longer contains `.Substring(3)`; UNC/short tests | source search and targeted tests | No out-of-range or mangling. |
| AC9 | D5d anchored strip | PASS | `TryMakeArchiveRelative`; repeated/case tests | contract and converter tests | Separator boundary and case behavior pinned. |
| AC10 | D5e redacted exception | PASS | invalid-segment and outside-ancestor message assertions | targeted converter evidence; redaction sweep | No value embedded. |
| AC11 | D5f removal option | PASS with residual | corrected assertion returns `BadName` | `FolderConverterTests` | Behavior sits in an unreachable legacy prompt cluster; tracked as non-blocking residual. |
| AC12 | D5g/dead parameter | PASS | `ResolveOlRoot` uses shared contract; `ask` removed | source inspection; near-miss test | No new API ambiguity. |
| AC13 | D6 archive root | PASS with follow-up | `ArchiveRootPathGuard`; AppGlobals tests; cached property | targeted tests and source inspection | UI propagation gap is distinct promoted issue #638. |
| AC14 | D7 OneDrive | PASS | priority resolver, redacted failure tests, `MatchBestSpecialFolder` absent from diff | source/diff inspection; targeted tests | Static delegate resolver is deterministic. |
| AC15 | D8 data-model stem | PASS | `ToArchiveRelativeStem`; eight tests; two live callers unchanged | targeted tests and source inspection | Root-exact and cross-store reject without leaking. |
| AC16 | D9 predicates | PASS | amended spec; 79-line guard; 25 current guard/composition tests | final runner and source inspection | Rooted inputs rejected; short relative stems accepted; creation minimum retained. |
| AC17 | Primary regression | PASS | named test plus fail-before and pass-after evidence | `p1-t2` and final runner | Message redaction asserted. |
| AC18 | Producer companion | PASS | named store-root activation test plus fail-before evidence | `p1-t4` and final runner | Full root is never stored. |
| AC19 | #609/#439 regression | PASS | must-stay-green set and protected `FolderPredictor.cs` | final runner; `git diff --name-only` | All named scenarios pass. |
| AC20 | #499 non-absorption | PASS | rejection returns before commit; selection unchanged tests; change description | source and test inspection | Existing clearing semantics not widened. |
| AC21 | Redaction | PASS | cycle-2 redaction sweep and reviewer diff scan | recorded regex commands; source inspection | Only approved fabricated placeholders found. |
| AC22 | Test policy | PASS | MSTest/Moq/FluentAssertions; banned-pattern scan empty; mirrored test trees | `rg` banned-pattern check; final runner | No temporary files or live dependencies. |
| AC23 | Coverage | PASS | 84.8758%->84.8841% line; 78.8585%->78.8692% branch; changed contracts 100% | coverage-delta XML analysis | Canonical evidence path is `evidence/qa-gates` under current skill authority. |
| AC24 | Full toolchain/evidence | PARTIAL | underlying clean pass: format/analyzer/nullable/test all exit 0; 6,586/6,586 | `toolchain-clean-pass`; PR-context normalized evidence row | Final test artifact mixes exit 0 with another run's expectation 1, so canonical context reports fail. |
| AC25 | Scope/file size | PASS under approved adjudications | exact code scope; protected paths absent; new/under-limit files comply; three pre-existing over-limit files do not grow | `git diff --check`; size/scope artifacts; checkpoint adjudications | `QuickFiler.Test/packages.config` is the required companion to the test-project log4net reference. |
| AC26 | Manual validation | PASS under criterion escape clause | five NOT EXECUTED entries, each with reason and automated counterpart | `manual-validation.2026-08-26T18-55.md` | No live Outlook profile was available; no step silently omitted. |

## Summary

**Overall Feature Readiness:** PASS WITH ACCEPTED RISK

**Criteria summary:**

- **PASS:** 25 criteria
- **PARTIAL:** 1 criterion (AC24)
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Retained accepted risks:**

1. The final test artifact's successful authoritative run and expected-failure preservation run remain mixed, so canonical PR context continues to normalize the row as fail. The separate 6,586/6,586 exit-0 QA fact remains verified and does not convert that row to pass.
2. The change-description root-resolution follow-up remains #637 although #638 is accurate.

**Lifecycle disposition:**

1. Do not create remediation inputs or a remediation plan for either accepted finding, and do not edit either affected file solely for these findings.
2. Carry both findings and accepted consequences into PR authoring; do not claim the canonical normalized row passed.
3. Run the three review artifact validators. If strict completion validation later rejects the unchanged findings, preserve and report that blocker without implementing the waived changes.

The product implementation is technically ready, and the accepted-risk scope decision permits PR authoring. Full evidence-policy compliance is not claimed.

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules, AC1-AC23, AC25, and AC26 remain checked. AC24 is changed from `[x]` to `[ ]` by this review because its canonical final-test evidence currently normalizes to fail. The accepted-risk disposition permits the overall review to pass without changing this criterion status. The criterion text is unchanged and may be rechecked only if future evidence and fresh PR context establish a passing normalized row.

### AC Status Summary

- Source: `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/spec.md`
- Total AC items: 26
- Checked off (delivered): 25
- Remaining (unchecked): 1
- Items remaining: AC24 — full four-step toolchain evidence representation.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|---|---:|---:|---:|---|
| `spec.md` | 26 | 25 | 1 | Sole checkbox-backed source for `full-bug`; AC24 is unchecked pending evidence normalization. |

No criterion text was added, deleted, or reworded by this review.

No remediation inputs or remediation plan were created because the only remediation-triggering findings are exactly the two findings removed from remaining scope by the recorded user approval.
