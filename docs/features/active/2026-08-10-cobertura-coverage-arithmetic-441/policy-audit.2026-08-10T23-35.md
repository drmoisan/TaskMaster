# Policy Compliance Audit — 2026-08-10-cobertura-coverage-arithmetic-441

- **Timestamp:** 2026-08-10T23-35
- **Reviewer:** feature-review agent
- **Branch under review:** `bug/cobertura-coverage-arithmetic-441`
- **Head:** `3b8d43fb90a3be7dc0bc9e5624509e473a56ca80`
- **Diff base:** `edf3d34cb9cd455bd3c1d9f5ee363b825632073c` (recorded pre-change baseline; verified an ancestor of HEAD via `git merge-base HEAD edf3d34c` = `edf3d34c`)
- **Work mode:** `full-bug` (persisted marker in `issue.md`; AC source is `spec.md` only)
- **Applicable policies:** `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/powershell.md`
- **Overall verdict:** PASS — 0 blocking findings, 5 non-blocking findings

## Executive Summary

The branch fixes two compounding Cobertura arithmetic defects (#441 descendant-axis double count, #478 blended merge denominator) in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, with regression tests in `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`. The full diff against `edf3d34c` spans 118 files, but exactly two are source files; the remainder are feature documents, evidence, epic-preparation documents for sibling features (already-merged integration-branch content inside the diff window), promoted potential entries, and agent-memory markdown. Every policy gate examined passes. This reviewer independently reproduced the headline correctness oracle, the pre-change defective figures, the package-filtered A/B figures, the fail-before demonstration for F1-F4, the byte-identity of the protected union builder, the analyzer no-new-findings gate, and the 19/19 test result. Five non-blocking findings are recorded (two Minor, three Informational); none requires remediation before merge.

## Scope and Baseline

- Diff command adjudicated: `git diff edf3d34c..HEAD` (full branch diff; 118 files, +17251/-34).
- Source files changed (full list after excluding `docs/**` and `.claude/agent-memory/**`):
  1. `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (+132/-34 net; 357 -> 455 lines)
  2. `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` (+246/-0; 222 -> 468 lines)
- Verified independently: no changes under `.github/workflows/**`, no changes to any hook, skill, or policy document, no `.ts`/`.tsx`/`.py`/`.cs` files changed anywhere in the diff.
- The caller-supplied base `edf3d34c` was accepted after verifying it is an ancestor of HEAD; the epic-preparation commits between the base and the two feature commits (`b52874d6`, `3b8d43fb`) are documentation-only and were included in the adjudicated diff rather than excluded.
- PR context artifacts were absent at review start; the `collect_pr_context` MCP tool is not exposed in this session, so `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` were hand-authored by this reviewer from `git diff edf3d34c..HEAD --numstat` in the canonical locations per `pr-context-artifacts`.

No scope narrowing was attempted by the caller. The caller directive restricts the toolchain to PowerShell, which matches the observed fact that the branch diff contains PowerShell source changes only, and it explicitly instructs full-diff adjudication.

## 1. Toolchain Gates (PowerShell: format -> analyze -> test)

### 1.1 Formatting (PoshQC / Invoke-Formatter)

- **Verdict: PASS.**
- Evidence: `evidence/qa-gates/poshqc-format.2026-08-10T23-10.md` records a clean final pass — SHA-256 content hashes of both in-scope files identical before and after the formatter, and `git status --porcelain` listings identical before and after (no out-of-scope file touched). Baseline artifact `evidence/baseline/poshqc-format.2026-08-10T22-30.md` recorded zero pre-existing drift.
- The dual-instrument approach (content hash + porcelain diff) is sound: it closes the gap where an already-modified file would not change its porcelain line when rewritten.

### 1.2 Linting (PSScriptAnalyzer)

- **Verdict: PASS (no-new-findings gate per spec Amendment 1).**
- Independent re-run by this reviewer at HEAD: `Invoke-ScriptAnalyzer` returns exactly **1 finding** on `Invoke-MSTestWithCoverage.Helpers.ps1` — `PSUseSingularNouns` / Warning / line 140 / `Get-CoberturaLineConditionCoverageParts` — and **0 findings** on the test file.
- This is the single baselined pre-existing finding recorded in `evidence/baseline/poshqc-analyze.2026-08-10T22-30.md`, keyed on `(ScriptName, RuleName, Severity, Message)` with `Line` as observation only. The move from line 146 to 140 equals the 6-line contraction from the P2-T2 loop replacement and is not a new finding. Per the accepted spec amendment, clearing it would require renaming an exported function the spec marks Unmodified; it is correctly left in place. Zero new analyzer findings introduced.
- The new function `Get-CoberturaClassLineSummary` uses an approved verb and a singular noun and introduces no analyzer debt.

### 1.3 Type checking

- Skipped by rule: `.claude/rules/powershell.md` § Toolchain step 3 states type checking is skipped for PowerShell. The `CLAUDE.md` C# `/p:Nullable=enable` command is not a gate for this change (zero C# files in the diff; the command itself is the known defect tracked as issue #522 and was directed not to be raised as a finding).

### 1.4 Testing (Pester v5)

- **Verdict: PASS.**
- Independent re-run by this reviewer at HEAD: `Invoke-Pester` on `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` — **Total=19 Passed=19 Failed=0 Skipped=0**. Matches `evidence/qa-gates/pester-final.2026-08-10T23-10.md`.
- The MCP `run_poshqc_test` payloads are recorded in evidence as non-probative (the tool returns no counts); all probative figures come from direct `Invoke-Pester` runs. That distinction is documented in each test-gate artifact.

## 2. Coverage Verification (PowerShell)

Changed languages in the branch diff: **PowerShell only** (confirmed from `artifacts/pr_context.summary.txt` and independently from `git diff --name-only`). TypeScript, Python, and C# each have zero changed files in the branch diff, so no coverage verdict is required for them.

Floors applied per `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`: line >= 85%, branch >= 75%, new code >= 90% (`CLAUDE.md` UT2), no regression on changed lines.

| # | Scope | Figure | Floor | Verdict |
| --- | --- | --- | --- | --- |
| 2.1 | PowerShell changed production file line coverage, `Invoke-MSTestWithCoverage.Helpers.ps1` (committed Pester JaCoCo `evidence/qa-gates/pester-coverage-final.2026-08-10T23-10.xml`, re-summed by this reviewer) | 183/202 = **90.59%** | >= 85% | **PASS** |
| 2.2 | PowerShell new-code line coverage, `Get-CoberturaClassLineSummary` lines 161-259 (same artifact, re-summed) | 39/40 = **97.50%** | >= 90% | **PASS** |
| 2.3 | PowerShell changed-line regression check: baseline 146/165 = 88.48% (`evidence/baseline/pester-coverage-baseline.2026-08-10T22-30.xml`, re-summed) -> 183/202 = 90.59%; the single uncovered statement (line 220) is new code that never existed in the baseline denominator, so no previously covered changed line lost coverage | +2.11 pp | no regression | **PASS** |
| 2.4 | PowerShell branch-behavior coverage: Pester's JaCoCo writer emits zero BRANCH counters, so no numeric branch percentage exists in any artifact; branch adequacy is adjudicated from direct tests — all three `condition-coverage` precedence branches plus the zero-input boundary each have a dedicated passing unit test (`evidence/other/helper-branch-test-map.2026-08-10T23-10.md`, corroborated by reading the four `It` blocks at tests file lines 402-467) | 3/3 precedence branches + boundary tested | >= 75% | **PASS** (with one untested statement-level branch recorded as finding NF-1) |
| 2.5 | PowerShell repo-wide line coverage read from the canonical artifact `artifacts/pester/powershell-coverage.xml` | 0/16075 lines covered = **0.00%** | >= 85% | **FAIL** — invalid capture; see disposition below |

### Disposition of row 2.5 (FAIL, non-blocking)

The canonical `artifacts/pester/powershell-coverage.xml` (written 2026-08-10 22:58 by the bundled `mcp__drm-copilot__run_poshqc_test` tool; gitignored producer output, never committed) records **zero covered lines for every file in the repository**, including `Invoke-MSTestWithCoverage.Helpers.ps1`, which the committed direct-Pester JaCoCo evidence for this same head demonstrates at 90.59%. A reading of literal zero for a file with a passing 19-test suite exercising it is a measurement defect of the bundled tool's coverage capture, not a property of this branch: the diff contains no change to any coverage-capture tooling, hook, or configuration. The FAIL verdict on row 2.5 is therefore recorded honestly against the artifact as it exists, and is dispositioned **non-blocking** for this feature on the following basis:

1. The probative, committed coverage evidence (rows 2.1-2.3) adjudicates every floor relevant to the changed code and passes each one; this reviewer re-summed all three XML artifacts independently rather than trusting the recorded prose.
2. The zero-reading capture defect pre-exists this branch and sits in shared MCP tooling whose fidelity is the subject of this very epic (`build-ci-coverage-gate-fidelity`); fixing producer tooling inside this bugfix would violate the minimal-fix mandate of `CLAUDE.md` § Bugfix Workflow step 2 and the feature's own scope boundary.
3. The executor's dated CORRECTION block in `evidence/baseline/poshqc-tool-surface.2026-08-10T22-30.md` already documents that these `artifacts/pester/` files are producer output invisible to `git status --porcelain`, and that no feature evidence is read from or written to them.

**Recommended follow-up (non-blocking):** file a tooling issue through the promotion lifecycle for "`run_poshqc_test` bundled coverage capture records zero covered lines repo-wide," or fold the observation into sibling feature #512 (toolchain gate fidelity). Recorded as finding NF-2.

## 3. General Code Change Policy

| Gate | Verdict | Evidence |
| --- | --- | --- |
| Bugfix workflow order (failing regression test first, then minimal fix, then verify) | PASS | Fail-before at 22-45 precedes the production commit; `git diff --name-only edf3d34c -- scripts` empty at fail-before time (`evidence/regression-testing/fail-before-f1-f4.2026-08-10T22-45.md`); this reviewer reproduced F1-F4 failing against the baseline `Helpers.ps1` extracted from `edf3d34c` (4 arithmetic failures with the exact recorded wrong values 6/4, 4/2, '0.75', 3/2) |
| Minimal, targeted fix; no opportunistic refactor | PASS | Production diff confined to the defective summary loop, one new pure helper, and the merge-rate block; protected union builder byte-identical (verified by `cmp`, old lines 217-268 == new lines 311-362); deeper design problems filed as issues #529-#532 instead of widening scope |
| 500-line file ceiling | PASS | `Helpers.ps1` 455 lines; test file 468 lines (both measured with `awk NR`; baseline 357 and 222 respectively — neither file crosses the ceiling) |
| Fail fast / error handling | PASS | Pre-existing `<packages>` guard retained verbatim and pinned by a new test; new helper validates mandatory typed parameter; `GetAttribute` used deliberately for StrictMode safety with an explanatory comment |
| Naming, docs, comments | PASS | Full comment-based help on the new helper; comments explain why (StrictMode rationale, duplication rationale), not what |
| Dependencies | PASS | No new dependency introduced |
| Public API compatibility | PASS | `Get-CoberturaCoverageSummary`, `Merge-CoberturaClassesByFilename`, `ConvertTo-KoverageCoberturaXml` signatures unchanged; one new exported helper added |
| Reusability | PASS | The dedup arithmetic is factored once into `Get-CoberturaClassLineSummary` and consumed by both call sites; the two rate-rounding expressions are duplicated deliberately with an in-code rationale (spec constrains the change to exactly one new helper; existing assertions depend on identical rounding). Recorded as accepted-design observation NF-5 |

## 4. General Unit Test Policy

| Gate | Verdict | Evidence |
| --- | --- | --- |
| Independence / isolation / determinism / speed | PASS | All 11 new `It` blocks operate on inline single-quoted here-string XML fixtures; no wall clock, no RNG, no ordering dependence; full suite runs in seconds |
| No temporary files, no external dependencies | PASS | Reviewer grep for `TestDrive|New-Item|Set-Content|Out-File|Remove-Item|[System.IO.File]` over the test file: zero matches; every new fixture is an in-memory `[xml]` cast. The only `Mock` in the file is in a pre-existing test (allowlist fallback), untouched by this diff |
| Arrange-Act-Assert and documented intent | PASS | Each new test carries a scenario comment naming the issue or branch it pins |
| Scenario completeness | PASS with gap noted | Positive, negative (throw guard retained), boundary (class with neither `lines` nor `methods`), and all three condition-coverage precedence branches are covered; the `max(hits)` update assignment (`Helpers.ps1:220`) is exercised by no test — finding NF-1 |
| Tests in `tests/` tree mirroring source | PASS | `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` mirrors `scripts/vscode/` |
| Existing tests treated as spec | PASS | Test-file diff is +246/-0 (verified by numstat and by counting deletion lines: 0); all 8 pre-existing `It` blocks unmodified and passing, including the `lines-valid | Should -Be '3'` assertion |

## 5. PowerShell Rules (`.claude/rules/powershell.md`)

| Gate | Verdict | Evidence |
| --- | --- | --- |
| Toolchain order format -> analyze -> test, restart on change | PASS | `evidence/qa-gates/toolchain-clean-pass.2026-08-10T23-10.md` records a single clean pass; formatter changed nothing so no restart was owed |
| PowerShell 7+ compatibility | PASS | Analyzer clean under repo settings; no PS7-incompatible construct introduced |
| Advanced functions, CmdletBinding, mandatory typed parameters | PASS | `Get-CoberturaClassLineSummary` uses `[CmdletBinding()]`, `[OutputType]`, `[Parameter(Mandatory = $true)][System.Xml.XmlElement]` |
| No global state; pure helper | PASS | Helper is pure (no I/O, mutates nothing in the source document; class-level `.Contains`/hashtable use is local) |
| Approved verbs / singular nouns | PASS | New function compliant; the one plural-noun warning is the baselined pre-existing finding on an out-of-scope exported function |
| Change budget (<= 2 production files direct-mode) | PASS | 1 production file + 1 test file |
| Mock sparingly; no executable mocking | PASS | Zero mocks in new tests |
| No analyzer debt deferral, no weakened assertions, no sleeps | PASS | Zero new findings; fail-before values assert counts (not rates) precisely because rates do not discriminate — assertions were strengthened, not weakened |

## 6. Scope-Boundary and Threshold Invariants (blocking if violated — all held)

| Invariant | Verdict | Evidence |
| --- | --- | --- |
| No coverage threshold re-tuned, lowered, or relaxed | **HELD** | `git diff --name-only edf3d34c -- CLAUDE.md .claude/rules coverage.config` re-run by this reviewer: empty output. The 85.0317%-vs-85% margin (0.03 pp) is recorded purely as fact and handed off to #494 in `evidence/other/threshold-handoff-494.2026-08-10T23-15.md`, which states explicitly that nothing in it is acted upon |
| No edit to `CLAUDE.md` or `.claude/rules/**` | **HELD** | Same empty diff; sibling ownership (#512, #494) respected |
| AC-18 two-source-file boundary | **HELD** | Full-diff enumeration minus `docs/**` and `.claude/agent-memory/**` yields exactly the two named files; `Invoke-MSTestWithCoverage.ps1` unchanged including its missing `\.claude\` discovery exclusion (now filed as issue #531) |
| AC-8 union builder byte-identity | **HELD** | Reviewer `cmp` of `edf3d34c` lines 217-268 vs HEAD lines 311-362: byte-identical |

## 7. Evidence Location Compliance

- Reviewer scan of the full branch diff for files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`: **zero occurrences.** No committed file in the diff lives under any `artifacts/` path.
- All 35 evidence files reside under `<FEATURE>/evidence/{baseline,regression-testing,qa-gates,issue-updates,other}/` — the canonical layout per `evidence-and-timestamp-conventions`.
- `scripts/dev_tools/validate_evidence_locations.py` does not exist in this repository (consistent with prior review findings that several validator scripts named in agent instructions are not ported to TaskMaster); the scan above was performed manually with `git diff --name-only` and a recursive listing instead.
- Every command-step evidence artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; the three narrative artifacts (`ac-status-summary`, `helper-branch-test-map`, `threshold-handoff-494`) carry `Timestamp:` only, as permitted by the accepted AC-16 amendment, and each is enumerated (or declared in advance) in the final sweep of `evidence/other/evidence-location-audit.2026-08-10T23-20.md`.
- The untracked `artifacts/pester/*.xml` files are producer tool output (gitignored via `.gitignore:57`), not evidence artifacts, per the executor's CORRECTION block — see § 2.5 disposition and finding NF-2.

## 8. Executor Self-Reported Items (both evaluated)

1. **AC-20 completed by the orchestrator, not the executor.** Verified genuine and honest. The executor's `POSTING BLOCKED` record is preserved verbatim (tool surface of exactly four PoshQC tools, no promotion tools; `gh` deliberately not used as a lifecycle bypass; AC-20 left unchecked rather than falsified). The orchestrator's RESOLUTION section (2026-08-10T23-50) appends rather than rewrites, names the actor and mechanism (`new_potential_bug_entry` -> `potential_to_issue`), and records issues #529-#532. This reviewer confirmed all four issues exist and are OPEN on GitHub with matching titles, and that the four promoted potential entries are committed under `docs/features/potential/promoted/`. The `ac-status-summary` superseding update is clearly marked and retains the original narrative. The blocked-then-resolved audit trail is legible and truthful. **PASS.**
2. **P0-T13 CORRECTION block.** Adequate. The retracted sentence ("writes no coverage artifact") is quoted verbatim, the reason the original instrument was invalid is explained (`.gitignore:57` hides `artifacts/` from porcelain), the correction is dated and attributed, and the claim that no downstream conclusion depended on it holds: every coverage figure cited in evidence comes from direct `Invoke-Pester` runs whose `OutputPath` is an explicit `<FEATURE>/evidence/<kind>/` path (verified in each artifact's recorded command), and no feature artifact reads from `artifacts/pester/`. The correction-not-silent-edit practice matches the evidence-integrity expectation. **PASS.** (The corrected fact also surfaces the row 2.5 observation — the very files the tool writes contain the zero-coverage reading — reinforcing that the correction was material and worth recording.)

## Findings Register (all non-blocking)

| ID | Severity | Finding | Disposition |
| --- | --- | --- | --- |
| NF-1 | Minor | The `max(hits)` update assignment at `Helpers.ps1:220` is uncovered: no test presents a duplicate line number whose later entry carries strictly larger hits, so the stated "max(hits)" resolution rule is pinned only for the first-entry-wins ordering | Recommend one added fixture (class rollup `hits="0"`, method entry `hits="1"`, assert `CoveredLines = 1`); does not gate merge — new-code coverage 97.50% clears every floor and the executor recorded the gap explicitly |
| NF-2 | Minor | Canonical repo-wide PowerShell coverage artifact (`artifacts/pester/powershell-coverage.xml`, producer output of `run_poshqc_test`) records zero covered lines repo-wide — an invalid capture contradicted by committed direct-Pester evidence at the same head | Row 2.5 FAIL recorded against the artifact; dispositioned non-blocking (pre-existing tool measurement defect, outside this bugfix's mandated minimal scope); recommend filing a tooling follow-up or folding into #512 |
| NF-3 | Info | `evidence/qa-gates/poshqc-analyze.2026-08-10T23-10.md` states "The 167 added lines introduced no analyzer debt" for the test file; by final QA the test file had 246 added lines (167 fixture lines plus 79 helper-test lines) | Prose figure is stale; the finding-set arithmetic (0 findings) is correct and was independently re-verified at HEAD; no action required |
| NF-4 | Info | The final sweep records 34 evidence files / 2 narrative artifacts; the current tree holds 35 and 3 because `ac-status-summary.2026-08-10T23-30.md` was written after the sweep (its path was declared in advance inside the sweep, as the sweep itself states) and the AC-20 RESOLUTION was later appended to an already-enumerated artifact | Compliant via the declared-in-advance mechanism; noted for completeness |
| NF-5 | Info | The merged-class `line-rate`/`branch-rate` rounding expressions duplicate the equivalent expressions in `Get-CoberturaCoverageSummary` rather than sharing a second helper | Accepted design: in-code comment records the rationale (spec constrains the change to exactly one new helper; existing `Should -Be '1'`-style assertions depend on identical rounding and the `'0'` zero-denominator fallback); no action required |

## Appendix A — Reviewer-Executed Verification Commands

1. `git merge-base HEAD edf3d34c` -> `edf3d34c` (base is ancestor).
2. `git diff edf3d34c..HEAD --stat` / `--name-only` (full-diff enumeration; source-file isolation).
3. `git diff --name-only edf3d34c -- CLAUDE.md .claude/rules coverage.config` -> empty (threshold invariant).
4. Dot-source HEAD `Helpers.ps1`; `Get-CoberturaCoverageSummary` over the #424 raw baseline document -> 79957 / 56124 / 23109 / 13472 (equals the document's own root attributes; AC-1 oracle reproduced exactly).
5. Dot-source `edf3d34c` `Helpers.ps1` (extracted to scratchpad); same run -> 161086 / 113219 / 46218 / 26944 (AC-2 pre-change figures reproduced exactly; each strictly greater than its post-fix counterpart).
6. `ConvertTo-KoverageCoberturaXml` at HEAD over the #424 `coverage-final.cobertura.xml` -> lines-valid 62345, lines-covered 53013, line-rate 0.850317 (AC-3 reproduced exactly).
7. `Invoke-Pester` at HEAD -> 19/19 passed.
8. `Invoke-Pester` of the HEAD test file against the `edf3d34c` production file in a scratchpad tree -> F1, F2, F3, F4 fail with the recorded pre-fix arithmetic values; F5, F6 and the throw guard pass; three additional failures are environment artifacts of the scratchpad tree (the three pre-existing tests that omit `-ProjectNames` resolve an empty project allowlist there, verified directly), matching the executor's in-repo fail-before run of 14 tests / 4 failures.
9. `cmp` of old lines 217-268 vs new lines 311-362 -> byte-identical (AC-8).
10. `grep` for `.//lines/line` (0 matches) and `$classSummaryXml` (0 matches) in HEAD `Helpers.ps1` (AC-7, AC-9).
11. `Invoke-ScriptAnalyzer` on both changed files -> 1 baselined pre-existing warning, 0 on tests (AC-15 analyzer leg).
12. Re-summed the three committed JaCoCo artifacts (baseline, fail-before, final) for rows 2.1-2.3; confirmed line 220 (`mi=1 ci=0`) is the single uncovered new statement.
13. Field-scan of all 30 evidence markdown artifacts for `Timestamp:` / `Command:` / `EXIT_CODE:` / `Output Summary:` (AC-16 schema).
14. `gh issue view 529 530 531 532` -> all four OPEN with matching titles (AC-20).
15. `awk NR` line counts: Helpers 455, tests 468, baselines 357 and 222 (AC-19 and file ceiling).
