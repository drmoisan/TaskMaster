# Policy Audit — Issue #752 (coverage assembly discovery excludes own worktree root)

- Timestamp: 2026-09-03T12-23
- Component: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (PowerShell developer tooling)
- Branch: `bug/coverage-assembly-discovery-excludes-own-worktree-root-752`
- Head: `80d07a1c26122a5cede04edc5833c964d663d8b7`
- Base (merge base with `origin/main`): `87233f867ad60c0a5c0d19b09cc121ae536d7ba1`
- Diff range audited: `87233f867ad60c0a5c0d19b09cc121ae536d7ba1..80d07a1c26122a5cede04edc5833c964d663d8b7` (39 files, +4848/-1)
- Work mode: `full-bug` (from `issue.md` line 12) — `spec.md` `## Acceptance Criteria` is the sole AC source
- Policy read order applied: `CLAUDE.md` -> `.claude/rules/general-code-change.md` -> `.claude/rules/general-unit-test.md` -> `.claude/rules/powershell.md` -> `.claude/rules/quality-tiers.md`

## Executive Summary

The branch delivers a one-line production change and one new 99-line Pester file. The fix at
`scripts/vscode/Invoke-MSTestWithCoverage.ps1:301` replaces an absolute-path `.claude` exclusion
with an anchored match against the candidate path computed relative to `$resolvedSearchRoot`. The
change matches the documented root cause, stays inside the plan's Write Set, preserves the
`Invoke-MSTest.RunSettings.Tests.ps1` regression test byte-identically, and is substantiated by a
fail-before / pass-after evidence pair plus a clean single-pass PowerShell toolchain run.

Verdict: **PARTIAL** — the code change and all six acceptance criteria are satisfied and
independently corroborated, but one blocking repository-hygiene violation is present in a file
inside the branch diff, and two non-blocking FAIL findings are recorded.

Blocking findings in this artifact: **1** (`POL-2`).

## Rejected Scope Narrowing

None. The delegation prompt supplied the merge-base anchoring rule (a legitimate base-resolution
instruction, not a scope narrowing) and pre-briefed the unchecked `[P3-T6]` plan task without
instructing that it be excluded from the audit. The full branch diff against the resolved base was
audited, including the documentation-only preparation commits `5375bcc9` and `df81b27e`, which the
plan's own Write Set does not enumerate.

## Evidence Location Compliance

- `validate_evidence_locations.py` is not present in this repository; the scan was performed
  directly against the branch diff file list.
- Verification: `git -C <repo-root> diff --numstat 87233f86..HEAD` — 33 of the 39 changed paths sit
  under `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/`,
  and every evidence artifact sits under one of the canonical sub-paths `evidence/baseline/`,
  `evidence/regression-testing/`, `evidence/qa-gates/`, `evidence/issue-updates/`, `evidence/other/`.
- Zero paths under `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`,
  `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/`,
  or `artifacts/post-change/` appear in the diff.
- Verdict: **PASS**. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` entries were required.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence / isolation | PASS | `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1:21-42` hoists all state into `BeforeEach`; each `It` re-declares only the mocks it overrides. |
| Determinism | PASS | All fixtures are literal `[pscustomobject]` records with fixed paths; no clock, RNG, network, or process is read. |
| Fast execution | PASS | Three `It` blocks, fully mocked; whole-suite run of 95 tests recorded in `evidence/qa-gates/final-clean-pass.2026-09-03T07-23.md`. |
| Arrange-Act-Assert | PASS | Each `It` is mock-setup, then a single `Invoke-MSTestWithCoverageMain` call, then one `Should -Be` (lines 44-57, 59-72, 74-98). |
| Documented intent | PASS | All three `It` names are behavioural sentences; lines 45, 61-62, 76-77 carry a why-comment naming the issue (#752, #733 finding 3, research §7). |
| No temporary files | PASS | Repo-wide grep of the new file for `New-Item`, `New-TemporaryFile`, `GetTempPath`, `GetTempFileName`, `$env:TEMP`, `$env:TMP` returns zero hits; the `check-powershell-test-purity.ps1` forbidden-pattern list at lines 99-117 is fully satisfied. |
| No external dependencies | PASS | `Invoke-VsWhereExe`, `Get-Command`, `Get-ChildItem`, `Get-Content`, `Set-Content`, `Invoke-DotnetCoverageCollection` are all mocked; the executable seam is mocked through its wrapper function, not directly. |
| Test file location mirrors source | PASS | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` -> `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`. |
| Coverage exclusion policy | PASS | No `exclude` entry was added; no production path was removed from measurement. |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | Single-expression in-place predicate edit; no new function, type, or indirection. |
| Separation of concerns | PASS | The change is a pure path computation inside an existing filter; no I/O boundary moved. |
| File size limit (500 lines) | PASS | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` = 350 lines (`evidence/other/predicate-line-shape.2026-09-03T07-23.md`, `LINECOUNT=350`); new test file = 99 lines (`git diff --numstat`). |
| Toolchain loop (format -> lint -> test) | PASS | See section 7. |
| Error handling unchanged | PASS | The `throw` at line 306 is byte-identical; no new catch or suppression introduced. |
| Comment why, not what | PASS | Each `It` carries an issue-referencing rationale comment. |
| Reusability / avoid copy-paste | PARTIAL | See `CR-2` in `code-review.2026-09-03T12-23.md`: roughly 40 lines of the new file duplicate `Invoke-MSTest.RunSettings.Tests.ps1:347-373` and `:416-442`. Justified in `plan.2026-09-03T07-23.md` line 53 by the 500-line cap on the original file; non-blocking. |
| No policy documents modified | PASS | No path under `.claude/rules/` or `.github/instructions/` appears in the diff. |

## 3. Language-Specific Code Change Policy Compliance (PowerShell)

| Requirement (`.claude/rules/powershell.md`) | Verdict | Evidence |
|---|---|---|
| PowerShell 7+ compatibility | PASS | `[System.IO.Path]::GetRelativePath` is a .NET Core / .NET 5+ API; the rules file line 24 mandates PowerShell 7+. Confirmed executable by `evidence/regression-testing/getrelativepath-probe.2026-09-03T07-23.md`, which returns three measured relative paths. |
| No `Invoke-Expression`, no hard-coded credentials or host paths in source | PASS | Neither changed source file contains any. |
| Approved verbs / naming | PASS | No function was added or renamed. |
| Analyzer debt not increased | PASS | `evidence/qa-gates/pssa-diagnostic-set.iter1.2026-09-03T07-23.md` records 16 diagnostics, line-for-line identical to the 16-item baseline; `NEW DIAGNOSTICS: NONE`. |
| Change budget (<= 2 production files, <= 3 test files) | PASS | 1 production file, 1 test file. |
| Wrapper-seam mocking, not executable mocking | PASS | `Mock Invoke-VsWhereExe` with `param([string]$VsWherePath, [string[]]$VsWhereArgs)` matches the production signature at `Invoke-MSTestWithCoverage.ps1:284-286`. |

## 4. Language-Specific Unit Test Policy Compliance (Pester)

| Requirement | Verdict | Evidence |
|---|---|---|
| Pester 5.x, `Describe`/`It` | PASS | `Describe 'Invoke-MSTestWithCoverage assembly discovery'`, three `It` blocks. |
| `*.Tests.ps1` naming | PASS | `Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`. |
| `Set-StrictMode -Version Latest` | PASS | Line 1 of the new file, matching `Invoke-MSTest.RunSettings.Tests.ps1:1`. |
| AST/ScriptBlock import order | PASS | Lines 9-18: parse, assert no parse errors, dot-source the `ScriptBlock`, then dot-source the helpers file — the order mandated by `.claude/rules/powershell.md` lines 85-88. |
| Mock registration before resolution | PASS | All ten seams are registered in `BeforeEach` before any `It` body runs. |
| One behaviour per `It` | PASS | Each `It` asserts exactly one captured assembly list. |

## 5. Test Coverage Detail

Changed languages in the branch diff: **PowerShell only** (`scripts/vscode/Invoke-MSTestWithCoverage.ps1`,
`tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`). No `.cs`, `.py`,
`.ts`, or `.tsx` file is changed on this branch.

| Language | Changed files | Artifact consulted | Line % | Branch % | Verdict |
|---|---|---|---|---|---|
| PowerShell (Pester) | 2 | `evidence/qa-gates/pester-coverage-cleanpass.2026-09-03T07-23.xml` (committed JaCoCo, direct `Invoke-Pester`) | 78.3313 (post) vs 78.3042 (baseline) | not emitted by Pester | **FAIL** — below the 85 percent line floor; see disposition below |
| TypeScript | 0 | `coverage/lcov.info` | — | — | PASS (zero changed files on the branch; no artifact required) |
| Python | 0 | `artifacts/python/lcov.info` | — | — | PASS (zero changed files on the branch; no artifact required) |
| C# / dotnet | 0 | `artifacts/csharp/coverage.xml` | — | — | PASS (zero changed files on the branch; no artifact required) |

Notes on the PowerShell row, stated evidence-first:

- The measurement scope is `scripts/vscode` (11 files), which is the scope both the baseline and
  the post-change runs used, so the delta is like-for-like.
- Baseline 78.3042394014963 percent (`evidence/baseline/pester-coverage-baseline.2026-09-03T07-23.md`),
  post-change 78.3312577833126 percent (`evidence/qa-gates/final-clean-pass.2026-09-03T07-23.md`).
  Delta +0.027 points. No regression on any changed line.
- Changed-line status independently verified by this reviewer: the baseline JaCoCo XML carries a
  `sourcefile name="Invoke-MSTestWithCoverage.ps1"` node spanning lines 727-830 with **no**
  `<line nr="301">` child; the post-change and clean-pass XMLs each carry
  `<line nr="301" mi="0" ci="1" mb="0" cb="0" />` at file offset 805, inside the same node. The
  changed line moved from unmeasured to measured-and-covered.
- Corroborating counter: Pester's analyzed-command total moved 802 -> 803 across the same 11 files,
  which is exactly the one new invocation expression the fix introduces.
- Branch threshold: Pester emits no `BRANCH` counter in JaCoCo output, so there is no branch figure
  to evaluate for this language and no branch gate applies (`.claude/rules/powershell.md` line 64,
  `.claude/rules/quality-tiers.md`). No FAIL is recorded for the absent branch figure.
- The canonical repository artifact `artifacts/pester/powershell-coverage.xml` is a gitignored local
  leftover from a bundled PoshQC run and reads `<counter type="LINE" missed="6403" covered="0" />`
  at report level in this worktree. A zero-covered capture is a known invalid measurement, so the
  committed direct-`Invoke-Pester` JaCoCo artifacts in the feature folder are treated as the
  authoritative figures. Both readings sit below the floor, so the verdict is unchanged either way.
- Disposition: **non-blocking**. The shortfall is pre-existing (recorded before this item changed
  anything in `evidence/baseline/coverage-floor-position.2026-09-03T07-23.md`, `BASELINE AT OR ABOVE
  FLOOR: false`), the delta is positive, the changed line is covered, and a single-line defect fix
  cannot close a 6.7-point repository shortfall it did not cause. Recorded as FAIL because the floor
  is not met, not because this item regressed anything.

## 6. Test Execution Metrics

| Run | Command source | Result | Artifact |
|---|---|---|---|
| Pre-change whole scope | `Invoke-Pester` over `tests/scripts/vscode` | 92 passed / 0 failed / 0 skipped | `evidence/baseline/pester-coverage-baseline.2026-09-03T07-23.md` |
| New suite, pre-fix (RED) | `Invoke-Pester` over the new file only | 1 passed / 2 failed, exit 1, `ExpectedExitCode: 1` | `evidence/regression-testing/pre-fix-new-suite.2026-09-03T07-23.md` |
| Failure-mode proof | filtered single-case run | one `FAILMSG` carrying `No test assemblies found` | same artifact, section `[P1-T10]` |
| New suite, post-fix (GREEN) | identical command | 3 passed / 0 failed | `evidence/regression-testing/post-fix-new-suite.2026-09-03T07-23.md` |
| Preserved original test | filtered run of `Invoke-MSTest.RunSettings.Tests.ps1` | 1 passed / 0 failed / 26 not-run of 27 | `evidence/regression-testing/preserved-original-test.2026-09-03T07-23.md` |
| Post-change whole scope | `Invoke-Pester` over `tests/scripts/vscode` | 95 passed / 0 failed / 0 skipped | `evidence/qa-gates/pester-coverage-postchange.iter1.2026-09-03T07-23.md` |
| Final clean pass | identical command | 95 passed / 0 failed, `CLEAN PASS ITERATION: 1` | `evidence/qa-gates/final-clean-pass.2026-09-03T07-23.md` |

92 + 3 = 95 reconciles exactly; no pre-existing test was lost and no unexpected test appeared.

## 7. Code Quality Checks

| Stage | Tool | Result | Verdict |
|---|---|---|---|
| Format | `mcp__drm-copilot__run_poshqc_format` | `ok: true`; SHA-256 of both Write Set files identical pre- and post-run (`WRITE SET REWRITTEN BY FORMATTER: NONE`, `RESTORED PATHS: NONE`) | PASS |
| Lint | `mcp__drm-copilot__run_poshqc_analyze` | 16 issues post-change against a 16-issue baseline; exit 1 expected and declared (`ExpectedExitCode: 1`) because the repository carries pre-existing unsuppressed Warnings under `scripts/vscode` | PASS |
| Lint (rule-level) | direct `Invoke-ScriptAnalyzer` | diagnostic set line-for-line identical to baseline; `NEW DIAGNOSTICS: NONE`; neither changed file contributes a diagnostic | PASS |
| Type check | no type-check stage exists for this language per `.claude/rules/powershell.md` line 17 | — | — |
| Test | `mcp__drm-copilot__run_poshqc_test` + direct `Invoke-Pester` | `MCP RESULT OK: true`; 95 passed / 0 failed | PASS |
| Loop restart discipline | plan `[P3-T1]`-`[P3-T5]` | completed on iteration 1; no `.iter2` artifacts exist and none are referenced | PASS |

Independent corroboration of the format stage being a no-op: the working tree is clean against
`HEAD` (`git -C <repo-root> diff --stat HEAD` returns empty), and `scripts/vscode/Invoke-MSTestWithCoverage.ps1:301`
on disk is character-identical to the `TRIMMED=` value recorded in `evidence/other/predicate-line-shape.2026-09-03T07-23.md`.

## 8. Gaps and Exceptions

### POL-1 — PASS — Policy read order and threshold conflict recorded

`evidence/baseline/phase0-instructions-read.2026-09-03T07-23.md` records the five-file read order
and a `POLICY CONFLICT NOTED:` line naming both the `CLAUDE.md` 80/90 figures and the
`.claude/rules` 85/75 figures, and states that the stricter 85 percent figure governs. This matches
the reviewer's own reading of the two rule files. No exception claimed.

### POL-2 — FAIL — BLOCKING — Absolute host path committed inside the branch diff

- File and line: `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md:5`
- Introduced by: commit `5375bcc9` (`docs(752): prepare feature folder, research, and spec ...`), which is inside the audited range.
- Violated rule: `.claude/agent-memory/_shared_no_absolute_host_paths.md` lines 8-13 — "No file
  committed to this repository may contain an absolute host path or a host identifier ... Prohibited:
  `C:\Users\<account>\...`". The same file's verification rule at lines 85-87 scopes the sweep to
  "the files your branch changed (`git diff --name-only <base>..HEAD`)", and this file is in that set.
  The plan elevates the same rule to non-negotiable status in its Plan Conventions (line 22).
- Verification command and output: `git -C <repo-root> diff 87233f86..HEAD | grep -n -i -E` filtered
  for added lines carrying the account name, the Windows drive-rooted profile prefix, the POSIX
  profile prefix, or the worktree-parent directory name, returns exactly one added line, the
  `- Worktree: ...` bullet on line 5 of that research artifact. It carries the operator account name
  and the full worktree directory layout.
- Reviewer's assessment of the executor's disposition: `evidence/other/followups.2026-09-03T07-23.md`
  entry 3 discloses the leak by class without quoting it — correct handling of the disclosure — and
  declines repair on the ground that the file is not in the plan's Write Set. That reasoning is
  correct for the executor's scope lock but does not discharge the branch-level obligation: the
  audit scope is the full branch diff, and merging this branch publishes the identifier to
  `origin/main`.
- Remedy: replace the absolute path with `<repo-root>` (or delete the bullet, since the branch name
  it also carries is available from git), then squash-merge so the pre-sanitisation blob does not
  remain reachable in history. Detail in `remediation-inputs.2026-09-03T12-23.md`.

### POL-3 — FAIL — non-blocking — Evidence `Timestamp:` fields are not capture times

- Files: nine committed evidence artifacts whose `Timestamp:` value postdates the commit that
  contains them. Commit `eea3bb9b` was created at `2026-09-03T12:14:51-04:00`
  (`git -C <repo-root> log --format="%h %cI" -4`) and carries every evidence file; commit
  `80d07a1c` (12:15:16) carries only `plan.2026-09-03T07-23.md`.
  - `evidence/qa-gates/pester-coverage-postchange.iter1.2026-09-03T07-23.md` — `Timestamp: 2026-09-03T12-15`
  - `evidence/qa-gates/coverage-delta.2026-09-03T07-23.md` — `12-16`
  - `evidence/qa-gates/runsettings-tests-unmodified.2026-09-03T07-23.md` — `12-17`
  - `evidence/qa-gates/sibling-defect-sweep.2026-09-03T07-23.md` — `12-18`
  - `evidence/qa-gates/final-clean-pass.2026-09-03T07-23.md` — `12-20`
  - `evidence/issue-updates/issue-752.2026-09-03T07-23.md` — `12-24`
  - `evidence/qa-gates/changed-file-audit.2026-09-03T07-23.md` — `12-26`
  - `evidence/other/followups.2026-09-03T07-23.md` — `12-28`
  - `evidence/other/ac-status-summary.2026-09-03T07-23.md` — `12-29`
- Second, independent corroboration from a clock the executor did not author: the JaCoCo writer
  stamps its own wall time into `<report name="Pester (MM/dd/yyyy HH:mm:ss)">`. The three committed
  XMLs read `11:55:15` (baseline, artifact claims `11-56` — consistent), `12:06:34`
  (post-change, artifact claims `12-15` — off by nine minutes), and `12:09:54` (clean pass, artifact
  claims `12-20` — off by ten minutes).
- Violated rule: `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` lines 108-112 define
  `Timestamp:` as a schema field of the recorded run; the plan's own Plan Conventions (line 26)
  state "The real capture time goes in the artifact's own `Timestamp:` field". The recorded values
  are a synthetic monotone one-minute-per-task sequence, not measured times.
- Impact, stated no more strongly than the evidence supports: no measured value is affected. Every
  substantive claim these artifacts label was re-derived by this reviewer from the committed XMLs,
  the git objects, and the working tree, and all agreed. The one derived claim that rests on the
  Timestamps alone is the *ordering* of the format -> analyze -> test stages within the clean pass;
  that ordering is instead carried here by an order-independent observation — the format stage left
  both Write Set files byte-identical (equal SHA-256 pre and post), so no stage sequence could
  change what analyze and test read, and both committed post-change XMLs demonstrably instrument the
  fixed content (they carry the line-301 node the pre-fix content cannot produce).
- Disposition: non-blocking. Recommended follow-up: have evidence-producing tasks emit the capture
  time from the same process that runs the command rather than composing it.

### POL-4 — PARTIAL — PR context artifacts are stale; scope derived from git instead

- `artifacts/pr_context.summary.txt` in the review worktree records `Head SHA: 558c9c42...` and
  `Head ref (resolved): bug/ribbon-engine-toggle-defects-735`; the copy in the session working
  directory records item #707. Neither describes this branch.
- Both copies are tracked files (`git log --oneline -1 -- artifacts/pr_context.summary.txt` returns
  `590d887d docs(735): author the pull request body and provenance receipt`) even though
  `.gitignore:57` ignores `artifacts/`. Regenerating over a tracked file belonging to another item
  would create an unrelated modification in this branch's working tree, so no regeneration was
  performed and the MCP context tool is not reachable from this agent's tool profile.
- Deviation recorded: the audited scope was derived directly from
  `git -C <repo-root> merge-base origin/main HEAD` and `git diff --numstat <base> HEAD`, which is the
  authoritative base-resolution source named in `.claude/skills/pr-base-branch-merge-base`. The
  derived file list (39 paths) is reproduced in section 9.
- Consequence for the changed-language enumeration: neither stale summary lists any source file, so
  an automated reader of those files would enumerate zero changed languages. This audit instead
  enumerates them from the git diff and records an explicit verdict for each in section 5.

### POL-5 — PARTIAL — Policy-audit template asset not resolvable

`.claude/skills/policy-audit-template-usage/SKILL.md` line 18 requires the template to come from
`mcp__drm-copilot__resolve_policy_audit_template_asset`. That MCP server is not exposed to this
agent's tool profile, and `mcp__drm-copilot__validate_orchestration_artifacts` is likewise
unavailable, so step 6's validator run could not be executed. Rather than emit a fully BLOCKED
stub, this artifact was hand-authored preserving all thirteen canonical major headings the skill
enumerates at lines 29-41. Recorded as a deviation, not a claim of template conformance.

### POL-6 — Observation — non-blocking — Plan `[P3-T6]` stop-and-report directive not honoured

`plan.2026-09-03T07-23.md` line 221 classifies "COVERED against a `false` baseline" as a
stop-and-report. That is the combination the run produced. The executor did not stop; it recorded
the divergence in `evidence/qa-gates/coverage-delta.2026-09-03T07-23.md` under the heading
`PLAN BRANCH DIVERGENCE (recorded rather than resolved)`, repeated it in
`evidence/other/followups.2026-09-03T07-23.md` entry 4, left the `[P3-T6]` checkbox unchecked, and
continued to completion. The reviewer's assessment: the plan's stop condition exists to catch an
*unexplained* third combination. The combination here is explained, and this reviewer independently
reproduced the explanation from the committed XMLs (baseline has no line-301 node under that
sourcefile; post-change and clean-pass each have one at `ci="1"`; analyzed commands 802 -> 803). The
outcome is a coverage gain, which neither branch's failure condition covers. Disclosed rather than
concealed, and the checkbox was left honestly unchecked. Non-blocking; the plan's branch enumeration
was incomplete, not the execution.

### POL-7 — Observation — non-blocking — One diff path sits outside the spec Write Set

`docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md`
(+66) appears in the audited range but is not in `spec.md` lines 120-123. It was added by commit
`5375bcc9`, the promotion-lifecycle preparation commit that precedes the plan, and the plan
allow-lists it explicitly with that rationale at lines 251-253. No task in the plan modified it.
Not treated as an unrelated modification by this item.

### POL-8 — PASS — No unrelated file modified by the fix

Verified independently, not accepted from the executor's audit: the merge-base-to-HEAD numstat lists
exactly two source paths, `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (+1/-1) and
`tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` (+99/-0). Every other
path is a feature-folder document or the promotion record above.
`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` is absent from the diff entirely, and
`git -C <repo-root> diff --stat HEAD` over the whole tree returns empty, so nothing is left
uncommitted either.

## 9. Summary of Changes

Source changes (2 files):

| Path | Delta | Nature |
|---|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | +1/-1 | Line 301 predicate: `$_.FullName -notmatch '\\\.claude\\'` becomes `([System.IO.Path]::GetRelativePath($resolvedSearchRoot, $_.FullName)) -notmatch '(^\|\\\\)\.claude\\\\'` |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` | +99/-0 | New Pester file, one `Describe`, ten-seam `BeforeEach`, three `It` blocks |

Documentation and evidence (37 files): `issue.md`, `spec.md`, `plan.2026-09-03T07-23.md`,
`research/research-findings.2026-09-03T00-00.md`, 32 evidence artifacts under the five canonical
`evidence/` sub-paths, and one promotion record.

## 10. Compliance Verdict

**PARTIAL.**

| Verdict class | Count | Items |
|---|---|---|
| Blocking FAIL | 1 | POL-2 |
| Non-blocking FAIL | 2 | POL-3, POL-4 (coverage floor row in section 5 is the second FAIL; POL-3 is the third) |
| PARTIAL | 2 | POL-4, POL-5 |
| Observations | 3 | POL-6, POL-7, and the reusability note carried as CR-2 |
| PASS | all remaining sections | — |

Restating the coverage row explicitly so it is not lost in the table above: the PowerShell line
coverage row in section 5 carries a **FAIL** verdict at 78.3313 percent against the 85 percent
floor, with a non-blocking disposition on the grounds recorded there.

Remediation-loop entry is required for POL-2 only.

## Appendix A: Test Inventory

New tests in `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`:

1. Line 44 — `includes an assembly directly beneath a search root that is itself under a .claude worktree segment`. Root `C:\repo\.claude\worktrees\agent-7`; one candidate directly beneath it; asserts inclusion. Fails against the pre-fix predicate.
2. Line 59 — `excludes a nested sibling worktree beneath a non-dot-claude search root`. Root `C:\repo\.`; two candidates, one nested under `.claude\worktrees\agent-1`; asserts only the root-level one survives. Passes both before and after; this is the case the regex anchor exists to preserve.
3. Line 74 — `retains the root-level assembly and excludes a further-nested worktree beneath a dot-claude search root`. Root `C:\repo\.claude\worktrees\agent-7` with a further-nested `.claude\worktrees\agent-9` candidate; asserts both behaviours hold for one root. Fails against the pre-fix predicate. This is the case a naive "disable the clause when the root is under `.claude`" fix cannot pass.

Preserved, unmodified: `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:416-442`,
blob `4b168b07967b692fdb0574aefd7a5734dfeb0d9c`, confirmed absent from the branch diff.

## Appendix B: Toolchain Commands Reference

PowerShell order per `.claude/rules/powershell.md` line 20 — format, then analyze, then test; no
type-check stage:

1. `mcp__drm-copilot__run_poshqc_format` with `scan_folders` `scripts/vscode`, `tests/scripts/vscode`
2. `mcp__drm-copilot__run_poshqc_analyze` with the same `scan_folders`
3. `Invoke-ScriptAnalyzer -Path "scripts/vscode" -Recurse` / `-Path "tests/scripts/vscode" -Recurse` (rule-level comparison the MCP tool cannot supply)
4. `mcp__drm-copilot__run_poshqc_test` with `scan_folders` `tests/scripts/vscode`
5. `Invoke-Pester` with `CodeCoverage.Path = "scripts/vscode"`, `OutputFormat = "JaCoCo"` (numeric figures the MCP tool cannot supply)

Reviewer verification commands used in this audit:

- `git -C <repo-root> merge-base origin/main HEAD`
- `git -C <repo-root> diff --numstat 87233f86..HEAD`
- `git -C <repo-root> diff --stat HEAD` (clean-tree check)
- `git -C <repo-root> log --format="%h %cI %aI %s" -4` (timestamp falsification)
- repo-wide grep for `-notmatch|-notlike|-match|-like` followed by a quoted `.claude` across `*.ps1`, `*.psm1`, `*.psd1` (sibling-defect re-derivation)
