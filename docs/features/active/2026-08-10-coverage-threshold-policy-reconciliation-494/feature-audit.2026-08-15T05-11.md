# Feature Audit — `build-ci-coverage-gate-fidelity` epic fan-in

- **Timestamp:** 2026-08-15T05-11
- **Baseline:** `main` @ `0569ac0b489f5867f514c00ccb2f65314c19cb16`
- **Head:** `epic/build-ci-coverage-gate-fidelity-integration` @ `22b5de02325331b0dcd660222dba33a1f1b66450`

## Summary

All 67 acceptance criteria across the five child features are checked off in their source documents,
and this audit found no criterion checked off against a false statement. Sixty-five are confirmed
PASS. Two are PASS-with-qualification, both in `#494`, where the criterion as finally written is
satisfied but the criterion text was narrowed by a committed scope correction such that satisfying it
does not deliver the outcome the epic manifest scoped for that lane.

No AC checkbox was changed by this audit; all were already `[x]` and all evaluated to PASS.

## Scope and Baseline

The audit compares the composed branch against `main` at the recomputed merge base `0569ac0b`, which
matches the caller-supplied SHA. The diff is 404 files: 375 Markdown, 19 coverage-evidence XML, 8
PowerShell, 1 `.vscode/tasks.json`, 1 `.csproj`. No `.cs` file is changed.

## Work-Mode Resolution

The `- Work Mode:` marker was read from each child `issue.md`. All five resolve to **`full-bug`**,
so per `acceptance-criteria-tracking` the sole AC source for each child is its `spec.md`. `#494`'s
`spec.md:16-18` independently reaffirms this ("only the `## Acceptance Criteria` section in this
`spec.md` is the sole acceptance-criteria source").

| Feature | Issue | Work mode | AC source | AC count |
|---|---|---|---|---|
| `2026-08-10-cobertura-coverage-arithmetic-441` | 441 | `full-bug` | `spec.md` | 20 |
| `2026-08-10-csharp-toolchain-gate-fidelity-512` | 512 | `full-bug` | `spec.md` | 13 |
| `2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394` | 394 | `full-bug` | `spec.md` | 8 |
| `2026-08-10-excludefromcodecoverage-nested-lambdas-457` | 457 | `full-bug` | `spec.md` | 16 |
| `2026-08-10-coverage-threshold-policy-reconciliation-494` | 494 | `full-bug` | `spec.md` | 10 |

## Acceptance Criteria Inventory

Total 67. Checked in source: 67. Unchecked: 0.

## Acceptance Criteria Evaluation

Verification was performed against the current tree and the committed evidence. Criteria are grouped;
each group states the independent check this audit performed rather than restating the criterion.

### `#394` — 8 criteria — all PASS

| Criterion | Verdict | Independent verification |
|---|---|---|
| Exactly one `PercentageFormatterTests.cs` `<Compile Include>` | PASS | Count at HEAD = 1; at merge base = 2 |
| Duplicate sweep across every item type | PASS | `grep -oE '<Compile Include="[^"]+"' \| sort \| uniq -d` returns nothing |
| Diff touches only the `.csproj` (single-line deletion) plus docs | PASS | `git diff --numstat` shows `0 1 UtilitiesCS.Test/UtilitiesCS.Test.csproj` |
| Fail-before CS2002 evidence, post-change zero CS2002, test count 7, toolchain pass, docs updated | PASS | Recorded in `evidence/`; two review cycles with remediation on this child |

### `#441` — 20 criteria — all PASS

| Criterion group | Verdict | Independent verification |
|---|---|---|
| AC-6 helper contract: `Get-CoberturaClassLineSummary` exists as a pure function | PASS | `Helpers.ps1:162`; no I/O, no source-document mutation |
| AC-7 / AC-9 defect removed at its one site; synthetic-document delegation replaced | PASS | The `$classSummaryXml` block is gone; `Merge-CoberturaClassesByFilename` now calls the helper directly (`Helpers.ps1:365`) |
| AC-8 correct site untouched | PASS | The union builder retains its prior structure in the diff |
| AC-11 / AC-13 six fixtures plus direct helper-branch tests | PASS | `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` grew 222 → 498 lines |
| AC-17 no threshold re-tuned by this child | PASS | This child's own diff contains no `CLAUDE.md` or `.claude/rules/` change; the composed diff's changes to those files originate in `#512` |
| AC-19 file ceiling | PASS | `Helpers.ps1` = 491 lines, under 500 (Minor: 9 lines headroom) |
| AC-15 toolchain green (amended to no-new-findings) | PASS | Amendment recorded pre-execution; the pre-existing `PSUseSingularNouns` finding is on `Get-CoberturaLineConditionCoverageParts` |
| AC-1..5, 10, 12, 14, 16, 18, 20 | PASS | Recorded numerically in `evidence/`; AC-3's post-fix `lines-valid = 62345` is corroborated by the `#494` remeasurement figure of 62401 on a later tree |

### `#457` — 16 criteria — all PASS

| Criterion group | Verdict | Independent verification |
|---|---|---|
| Closure filter is a pure XML-to-XML transform, new file only | PASS | `Invoke-MSTestWithCoverage.ClosureFilter.ps1`, 389 lines, new |
| Invoked inside `ConvertTo-KoverageCoberturaXml` after path rewrite | PASS | `Helpers.ps1:427`, immediately before `Merge-CoberturaClassesByFilename` |
| Unrecognized compiler-generated name shapes are **retained** | PASS | Fail-safe direction confirmed by the criterion text and the regression suite |
| Ten regression cases, deterministic, no temporary files | PASS | `ClosureFilter.Tests.ps1`, 443 lines |
| No coverage threshold changed by this feature | PASS | This child's diff contains no threshold constant |
| New-file coverage | PASS | 93/93 executable lines = 100.00% |
| Baseline re-captured, residuals handed off, probe recorded | PASS | `evidence/baseline/` and `evidence/qa-gates/` |

### `#512` — 13 criteria — all PASS

| Criterion | Verdict | Independent verification |
|---|---|---|
| AC1 documented format command executes | PASS | `dotnet-tools.json` pins CSharpier 1.2.6, which requires the `format`/`check` subcommand; all `.claude`-side sites now use it |
| AC2/AC3 type-check command performs a genuine recompile | PASS | `/t:Rebuild /m` adopted in `CLAUDE.md`, `.claude/rules/csharp.md`, `.claude/skills/csharp-qa-gate/SKILL.md`, `.vscode/tasks.json` |
| AC4 negative-path proof | PASS | Recorded in `evidence/regression-testing/` |
| AC5 documented commands consistent with CI | PASS with note | Substantively true, but AC5 names `.github/workflows/ci.yml` as the comparator and the CI split moved those steps into reusable workflows. The comparator moved after the criterion was verified |
| AC6 complete site inventory reconciled | PASS | Re-verified against the current tree: pattern (a) 10 hits, pattern (b) 9 hits, pattern (c) 14 hits, all either SD1-allowlisted with a four-ground rationale and follow-up #535, or one of the four anticipated residual classes |
| AC8 no policy requirement relaxed | PASS | No coverage numeral was lowered by this child |
| AC9 `CLAUDE.md` § UT2 and the two rule files | PASS | Unmodified by this child |
| AC7, AC10, AC11, AC12, AC13 | PASS | Verification step, rationale correction, baseline and final-QC evidence, measured nullable-debt figure, and the recorded analyzer-vacuity decision are all present |

### `#494` — 10 criteria — 8 PASS, 2 PASS-with-qualification

| Criterion | Verdict | Independent verification |
|---|---|---|
| AC4 tooling rejects below 80%, accepts exactly 80%, negative-path evidence | PASS | `Assert-CoberturaLineCoverageThreshold` (`Helpers.ps1:459-491`) validates presence, numeric form, and the 0-1 range before comparing; five `It` blocks cover missing, non-numeric, below-80, exactly-80, and above-80 |
| AC7 corrected-arithmetic remeasurement retained and validated | PASS | Three runs: line rate 0.855451 / 0.855627 / 0.855547, `lines-valid` identical at 62401. This satisfies D5's own reproducibility tolerance (spread 0.0176 pp against a 1.0 pp allowance; denominator spread 0% against a 0.5% allowance) |
| AC9 Pester tests mirror their subjects, deterministic | PASS | Tests sit at the mirrored `tests/scripts/vscode/` path; no temporary files |
| AC1, AC2, AC3, AC5, AC6, AC8, AC10 | **PASS-with-qualification** | Each is satisfied by the upstream prompt artifact *carrying a requirement*, not by any in-repo reconciliation. See below |

**Qualification on `#494`.** The "User-Authorized Scope Correction" at `spec.md:25-55` supersedes every
delivery instruction that would edit `CLAUDE.md`, any non-memory Claude runtime path, or
`.agents/skills/**`, and designates
`evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md` as the complete
local deliverable for all of them. Seven criteria were rewritten to be satisfied by that prompt
carrying a requirement. As finally written, each is met. The consequence is that decisions D1 through
D7 — which resolve the 80-vs-85 contradiction, name `CLAUDE.md` § UT2 as the single authority,
establish the cite-do-not-restate convention, and remove the false `quality-tiers.yml` claims — are
specified but not applied in this repository.

The epic manifest scopes `#494`'s primary surface as `CLAUDE.md` § UT2,
`.claude/rules/general-unit-test.md`, and `.claude/rules/quality-tiers.md`. **None of those three
files is modified by this branch.** The lane's acceptance criteria are met; the lane's scoped surface
is untouched.

**Authorization verification.** The scope correction states that it is user-authorized. This audit
could not corroborate that from the repository: `artifacts/orchestration/` contains only
`epic-orchestrator-state.json`, which carries **no `human_interaction` block**, and the child
`#494` orchestrator state is not present in this worktree (the child worktree was removed after
merge; the checkpoint is gitignored). The authorization is therefore recorded as **UNVERIFIED** in
this repository, with that specific reason. The committed spec text, reviewed across five `#494`
review cycles, is the best available evidence and is accepted for the purpose of AC evaluation.

## Epic-Level Outcome Against the Manifest

The epic manifest states four leading indicators. Evaluated against the composed branch:

| Leading indicator | Verdict | Basis |
|---|---|---|
| A deliberately introduced nullable violation fails the documented type-check gate | PASS | `#512` AC4 negative-path proof; `/t:Rebuild` adopted so `CoreCompile` cannot be skipped |
| A deliberately introduced coverage regression fails the documented coverage gate | PARTIAL | `Assert-CoberturaLineCoverageThreshold` now fails a below-80% run, which is new capability. It fails at 80%, while two always-loaded rule files state 85%, and the review hook fails at 85.0 |
| Reported repository-wide `lines-valid` equals the distinct-line count, not twice it | PASS | 62401 against the pre-fix 110849 for the comparable document; `#441` AC-1 reproduces a source document's own root attributes exactly |
| The documented C# toolchain commands execute successfully against a clean `main` | PASS | `#512` evidence; independently re-verified that the commands' claimed CI counterparts exist, though `CLAUDE.md` still names the pre-split workflow file |

The epic manifest also states two NFRs:

- *No coverage threshold may be lowered to accommodate a corrected denominator without an explicit,
  recorded decision.* **PASS.** No threshold was lowered. `#494` D1's reasoning explicitly addresses
  the point and no measurement figure is an input to it. The corrected repository-wide C# figure
  (85.55%) clears both the 80% and the 85% floors, so no accommodation was needed.
- *Every corrected figure must be accompanied by a re-captured baseline in the same change.*
  **PASS.** `#441`, `#457`, and `#494` each carry `evidence/baseline/` captures.

## Acceptance Criteria Check-off

No checkbox required a change. All 67 criteria were already `[x]` in their `spec.md` source files and
all 67 evaluate to PASS or PASS-with-qualification. No criterion was found checked off against a
statement this audit could falsify.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md`,
  `.../2026-08-10-csharp-toolchain-gate-fidelity-512/spec.md`,
  `.../2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/spec.md`,
  `.../2026-08-10-excludefromcodecoverage-nested-lambdas-457/spec.md`,
  `.../2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md`
- Total AC items: 67
- Checked off (delivered): 67
- Remaining (unchecked): 0
- Items remaining: none

## Residuals Carried Forward

1. The 80-vs-85 coverage contradiction remains in the always-loaded policy set and is now enforced at
   two different floors by two live gates.
2. `CLAUDE.md`'s three `ci.yml` references are stale after the CI split.
3. The threshold assertion's ordering relative to `Set-Content`.
4. Mirror policy surfaces (`.agents/`, `.github/instructions/`, `.github/agents/`, `AGENTS.md`) still
   prescribe the superseded commands; tracked as issue #535.
5. No green CI run covers the current head.
6. Nine issues (441, 478, 492, 509, 512, 522, 394, 457, 494) remain open and close only when the
   integration PR merges into `main`, which makes that PR's closing-keyword list correctness-critical.
