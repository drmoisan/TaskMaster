# Policy Audit — `build-ci-coverage-gate-fidelity` epic fan-in

- **Timestamp:** 2026-08-15T05-11
- **Base branch:** `main` @ `0569ac0b489f5867f514c00ccb2f65314c19cb16`
- **Head:** `epic/build-ci-coverage-gate-fidelity-integration` @ `22b5de02325331b0dcd660222dba33a1f1b66450`
- **Merge base (recomputed):** `0569ac0b489f5867f514c00ccb2f65314c19cb16` — matches the caller-supplied value.
- **Diff range audited:** `git diff origin/main...HEAD` — **404 files**.
- **Working tree:** clean (`git status --porcelain` empty).

## Executive Summary

Verdict: **PARTIAL**. Zero Blocking findings. Five Major findings, seven Minor.

The composed diff is internally coherent and the three hand-resolved Markdown conflicts at
`fb8eff9b` dropped no incoming hunk from `main`. The epic's arithmetic, closure-filter, CS2002 and
toolchain-fidelity lanes are delivered and verifiable. The coverage-threshold lane (#494) did **not**
remove the always-loaded policy contradiction it was chartered to remove; the contradiction was
deferred upstream by a committed scope correction, and a new executable gate at the lower of the two
contradictory numbers was added in its place. That is the epic's principal residual.

## Scope Determination

Scope was derived from `git diff --numstat origin/main...HEAD` directly, not from the PR context
summary's classification, per the caller's instruction and per the recurring misclassification
defect.

| Extension | Files |
|---|---|
| `.md` | 375 |
| `.xml` | 19 (all committed coverage evidence) |
| `.ps1` | 8 |
| `.json` | 1 (`.vscode/tasks.json`) |
| `.csproj` | 1 (`UtilitiesCS.Test/UtilitiesCS.Test.csproj`) |

**No `.cs` files are changed in this diff.** The `UtilitiesCS/Threading/TimeOutTask.cs` and
`UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs` edits visible in an early session
snapshot arrived from `main` through merge `fb8eff9b` and are therefore absent from the three-dot
diff. Changed-language set for coverage purposes: **PowerShell**.

`artifacts/pr_context.summary.txt` records Head `22b5de02` and Base `0569ac0b` and is therefore
**current**, not stale as the caller expected. Its `Core logic changes: 8 files` line counts only the
eight `.ps1` files and omits `.vscode/tasks.json` and `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
(Minor m-6). The omission does not affect the coverage hook's language enumeration, because neither
`.json` nor `.csproj` maps to a language in `Get-ChangedLanguageSet`.

## Rejected Scope Narrowing

**None detected.** The caller prompt explicitly assigned scope determination to this agent, supplied
the full 404-file diff range, and directed that the full composed diff plus the merge-conflict
resolutions be audited. No instruction attempted to restrict the audit to a plan, task, phase, file
subset, or language subset, and no instruction asked that a toolchain or coverage check be skipped.

For completeness: the caller's characterisation of the PR context artifacts as "stale by two commits"
was checked and found incorrect (they are at the current head). Proceeding on the full diff was
correct regardless.

## Evidence Location Compliance

**PASS.** Every evidence artifact in the diff resides under `<FEATURE>/evidence/<kind>/`.

- Scan for `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/` in
  the branch diff: **zero matches**.
- All 19 changed `.xml` coverage artifacts live under
  `docs/features/active/<feature>/evidence/{baseline,qa-gates,regression-testing}/`.
- `scripts/dev_tools/validate_evidence_locations.py` **does not exist** in this repository, so the
  scripted check could not be run; the equivalent path scan above was performed instead. This is
  consistent with the recorded finding that several validators named in the shared skills are not
  present in TaskMaster.

No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events occurred.

## 1. Coverage Verification

### 1.1 Per-language repository-wide rows

Coverage is verified from pre-existing committed artifacts; no coverage run was re-executed.

- **PowerShell repository-wide line coverage: 71.51% (502 of 702 lines) against the 85% floor — FAIL.**
- **C# repository-wide line coverage: 85.55% (53381 of 62401 lines) against the 85% floor — PASS.**
- **C# repository-wide branch coverage: 79.04% (12546 of 15872 branches) against the 75% floor — PASS.**
- **PowerShell branch coverage: the runner emits no branch counter — see 1.4 for the recorded verification status.**

Canonical artifact paths `artifacts/pester/powershell-coverage.xml` and `artifacts/csharp/coverage.xml`
are both **absent** from the working tree. Verification was performed against committed
`<FEATURE>/evidence/` artifacts, which is the established substitution for this repository.

| Language | Artifact used | Root figure |
|---|---|---|
| PowerShell | `.../494/evidence/baseline/powershell-baseline.jacoco.xml` (JaCoCo, 11 classes, full script tree) | LINE missed 200 / covered 502 |
| C# | `.../494/evidence/baseline/coverage-remeasurement-run{1,2,3}.corrected.cobertura.xml` | `line-rate` 0.855451 / 0.855627 / 0.855547 |

### 1.2 PowerShell per-file figures for changed production files

| File | Status | Lines covered/total | Rate | Verdict |
|---|---|---|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | new | 93/93 | 100.00% | PASS (>= 85% and >= 90% new-code) |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | modified | 185/204 | 90.69% | PASS |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | modified | 87/97 | 89.69% | PASS |
| `scripts/vscode/Invoke-VSBuild.ps1` | modified | 36/43 | 83.72% | Below floor — see disposition |

Changed-production-file aggregate: **401/437 = 91.76%**.

### 1.3 Disposition of the PowerShell repository-wide FAIL

The 71.51% figure is recorded as FAIL and is **dispositioned non-blocking**. Basis:

1. The entire shortfall is attributable to five scripts with zero or near-zero coverage, **none of
   which appear in this branch's diff**: `scripts/temp-extract-coverage` (0/58),
   `scripts/vscode/TestProcessCleanup` (0/29), `scripts/vscode/Invoke-Restore` (0/16),
   `scripts/dev-tools/run-actionlint` (0/9), `scripts/vscode/Install-RepoDotNetSdk` (3/33).
   Together: 3 covered of 145.
2. The branch **improves** the metric. Removing the new `ClosureFilter.ps1` contribution (93 covered
   of 93) from the numerator and denominator yields 409/609 = 67.16% — the approximate pre-branch
   position. The branch therefore moves repository-wide PowerShell line coverage from roughly 67.2%
   to 71.5%.
3. Zero regression on changed lines. `#512` evidence
   (`evidence/qa-gates/powershell-coverage-delta.2026-08-11T00-45.md`) records changed-line coverage
   of 100% for `Invoke-VSBuild.ps1` (3 of 3 analyzed changed commands executed) and a missed-command
   set identical to its baseline.
4. `Invoke-VSBuild.ps1` at 83.72% carries seven uncovered commands, all in the un-seamed MSBuild
   invocation tail, all present in the pre-change baseline, and none in an edited region.

No remediation-inputs artifact is produced for this row.

### 1.4 PowerShell branch coverage

The verification status of this metric is recorded as **UNVERIFIED**, for the following concrete
reason. Pester 5.6.1 implements a command-based measurement model that has no branch counter, so no
such figure can be produced. `#512` evidence enumerates both the
`PesterConfiguration.CodeCoverage` and result-object `CodeCoverage` property lists at run time and
neither contains a branch metric. The committed JaCoCo artifacts contain zero `<counter type="BRANCH">`
elements, corroborating this. No branch figure can be produced from this runner.

## 2. Merge-Conflict Resolution Audit (`fb8eff9b`)

Parents: `^1` = `85ff0c34` (integration side), `^2` = `0569ac0b` (`main` side). Each of the three
hand-resolved files was diffed against **both** parents.

### 2.1 `.claude/rules/csharp.md` — PASS

The resolution keeps the integration branch's structured list form and re-adds `main`'s
`dotnet tool restore` sentence, which the integration side lacked. Nothing present in `^2` was
dropped. Both new workflow references were independently verified against the tree:

- `_build-analyzers.yml:50` — `& msbuild $env:SOLUTION_PATH /t:Build /m ...` — the claim that CI's
  analyzer job retains `/t:Build /m` is **true**.
- `_build-nullable.yml:57` — `& msbuild $env:SOLUTION_PATH /t:Rebuild /m ...` — the claim that CI's
  nullable job uses `/t:Rebuild /m` is **true**.
- `.github/workflows/ci.yml` post-split contains **no** `msbuild` invocation; it is a caller that
  dispatches five reusable workflows. The resolution's re-pointing of both rationales was therefore
  a necessary correction, not a drift.

### 2.2 `.claude/skills/csharp-qa-gate/SKILL.md` — PASS

The merge result is a strict superset of `^2`: the `^2`-to-merge diff is `+2 / -0` (the
`CoreCompile` skip-count evidence paragraph from the integration side). The `^1`-to-merge diff shows
`main`'s `dotnet tool restore` step-1 wording and its `/t:Rebuild /m` rationale paragraph were both
taken. Nothing was dropped from either parent.

### 2.3 `.claude/agent-memory/feature-review/MEMORY.md` — PASS with Minor

Index entry counts: `^1` 57, `^2` 65, merge 58. Link-target set difference:

- **Present in `^2`, absent from merge — 9 targets:** `code-review-findings-table-header.md`,
  `code-review-required-headings.md`, `feature-audit-checkoff-heading-case.md`,
  `feature-audit-requires-summary-heading.md`, `policy-audit-comparison-line-schema.md`,
  `policy-audit-numeric-new-code-coverage.md`, `policy-audit-required-structure.md`,
  `policy-audit-section7-row-label-parser.md`, `policy-audit-validator-uses-full-template.md`.
- **Present in `^1`, absent from merge — zero.**
- `main`'s single new entry (`project_553_ci_split_review_pattern.md`) **is present** in the merge
  result.

The nine dropped index lines are not information loss: they were deliberately consolidated on the
integration side into the `taskmaster-validator-memories-are-cross-repo` entry, which names all nine
files explicitly, and all nine files still exist on disk. Recorded as Minor m-1 (the nine files are
now reachable only through the consolidating entry rather than through their own index lines).

**Conclusion for section 2: no incoming hunk from `main` was silently dropped by any of the three
resolutions.**

## 3. Always-Loaded Policy Document Consistency

This is the class of defect the epic was chartered to remove. Current state of the tree:

| Document | Repo-wide line | Branch | New code |
|---|---|---|---|
| `CLAUDE.md:303` | `>= 80%` | not stated | `>= 90%` |
| `.claude/rules/csharp.md:44` | `>= 80%` | not stated | — |
| `.claude/rules/general-unit-test.md:23-24` | `>= 85%` all tiers | `>= 75%` all tiers | — |
| `.claude/rules/quality-tiers.md:33-34` | `>= 85%` | `>= 75%` | — |

**The contradiction is still present and unmodified by this branch.** `#494`'s spec records it
verbatim in its Context table and resolves it in decisions D1 through D7 — but the
"User-Authorized Scope Correction" at `spec.md:25-55` supersedes every delivery instruction that
would edit `CLAUDE.md`, any non-memory Claude runtime path, or `.agents/skills/**`. The reconciliation
was therefore deferred to an upstream owner via
`evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md`, and seven of
`#494`'s ten acceptance criteria (AC1, AC2, AC3, AC5, AC6, AC8, AC10) are satisfied by that prompt
carrying a requirement rather than by any in-repo reconciliation.

What did land in-repo is `Assert-CoberturaLineCoverageThreshold`, which enforces **80%**. See
Major M-1: the repository now has two live numeric coverage gates enforcing different floors.

**Verdict for section 3: PARTIAL.** The contradiction was neither removed nor relocated — it was
retained and partially operationalised at the lower number.

## 4. Toolchain and Command Fidelity

| Check | Result |
|---|---|
| CSharpier pinned version claim (`CLAUDE.md:188` says 1.2.6) | PASS — `dotnet-tools.json` pins `1.2.6`, `rollForward: false` |
| `.csharpierignore` excludes `*.csproj`/`*.props`/`*.targets` as `CLAUDE.md:188` claims | PASS — all three present |
| Documented analyzer command uses `/t:Rebuild /m` | PASS — `CLAUDE.md:202`, `.claude/rules/csharp.md:15`, `.vscode/tasks.json` `-Target Rebuild` |
| Documented nullable command omits `/p:Nullable=enable` | PASS — removed from `CLAUDE.md`, `csharp.md`, `tasks.json`; `Invoke-VSBuild.ps1` switch made inert with a warning |
| `CLAUDE.md` references to `.github/workflows/ci.yml` | **FAIL — three stale references, Major M-2** |
| `.agents/`, `.github/instructions/`, `.github/agents/`, `AGENTS.md` mirror sites | Still prescribe `csharpier .` and `/t:Build ... /p:Nullable=enable`; enumerated with rationale under `#512` SD1 and deferred to follow-up issue #535 — Minor m-7 |

`#512` AC6 is satisfied as written: its final sentence permits deliberately-unchanged sites provided
they are enumerated with rationale, and
`evidence/qa-gates/site-inventory-reconciled.2026-08-11T00-18.md` enumerates all of them with a
four-ground rationale and a filed follow-up issue. This audit verified the enumeration against the
current tree and found the hit sets unchanged.

## 5. General Code Change Policy

| Rule | Result |
|---|---|
| 500-line file limit | PASS — largest changed file is `Helpers.Tests.ps1` at 498 lines; `Helpers.ps1` at 491. Both under the limit with under 10 lines of headroom (Minor m-5) |
| Test files mirror production tree | PASS — `scripts/vscode/X.ps1` → `tests/scripts/vscode/X.Tests.ps1` for all four changed production scripts; no colocation |
| No temporary files in tests | PASS — `#457` AC and `#441` AC-16 both assert this; spot-check of the two new test files found no temp-file creation |
| Policy documents not modified by this review | PASS — this agent made no edits to any file |
| Public API compatibility | PASS — `Invoke-VSBuild.ps1` retains the `-EnableNullable` switch as an inert, warning-emitting parameter so existing callers still bind |

## 6. CI and Green-Run Status

`.github/workflows/**` is **not modified** by this branch relative to `main` (the reusable workflow
files arrived from `main` through the merge). The `modified-workflow-needs-green-run` rule therefore
does not fire on workflow-change grounds.

However, **no green CI run exists for the current head.** `epic-status.md` records this honestly: the
last green integrated-tree run (31493339489) was against `c7d398c2`, and two changes landed after it —
`#494` (merge `85ff0c34`) and the `main` merge (`fb8eff9b`). Recorded as Major M-5.

## 7. Findings Summary

| ID | Severity | Finding |
|---|---|---|
| M-1 | Major | Coverage-threshold contradiction not removed; two live gates now enforce 80% and 85% on the same metric |
| M-2 | Major | `CLAUDE.md` retains three stale `.github/workflows/ci.yml` references after the CI-split merge |
| M-3 | Major | Threshold assertion runs before `Set-Content`, leaving an un-post-processed artifact on failure |
| M-4 | Major | Repository-wide PowerShell line coverage 71.51% is below the 85% floor (dispositioned non-blocking) |
| M-5 | Major | No green CI run covers the current head |
| m-1 | Minor | Nine feature-review memory files unindexed after the MEMORY.md resolution |
| m-2 | Minor | `csharp-qa-gate/SKILL.md` "CI may retain `/t:Build`" is imprecise post-split |
| m-3 | Minor | `#494` `.raw.`/`.corrected.` Cobertura pairs are byte-identical; the `.raw.` label is inaccurate |
| m-4 | Minor | Two committed figures disagree for `Invoke-VSBuild.ps1` coverage (85.71% vs 83.72%) |
| m-5 | Minor | `Helpers.ps1` (491) and `Helpers.Tests.ps1` (498) leave under 10 lines of 500-limit headroom |
| m-6 | Minor | PR context summary "Core logic changes: 8 files" omits the `.json` and `.csproj` changes |
| m-7 | Minor | Mirror policy surfaces still prescribe the superseded commands (deferred, issue #535) |

**Blocking: 0. Major: 5. Minor: 7.**

## Appendix A — Verification Commands

All commands were read-only.

```
git rev-parse HEAD
git merge-base origin/main HEAD
git diff --numstat origin/main...HEAD
git diff fb8eff9b^1 fb8eff9b -- <each conflicted path>
git diff fb8eff9b^2 fb8eff9b -- <each conflicted path>
git show fb8eff9b^2:.claude/agent-memory/feature-review/MEMORY.md
grep -n 'msbuild\|/t:' .github/workflows/_build-analyzers.yml .github/workflows/_build-nullable.yml
grep -n '85\.0\|75\.0\|BranchFloor' .claude/hooks/validate-feature-review-coverage.ps1
cmp coverage-remeasurement-run{1,2,3}.raw.cobertura.xml ...corrected...
```

Coverage figures were computed by parsing the committed JaCoCo and Cobertura artifacts with
`xml.etree.ElementTree`; no coverage generation was re-run.
