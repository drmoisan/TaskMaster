# 2026-08-10-coverage-threshold-policy-reconciliation-494 (Issue)

- **Issue:** #494
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/494
- **Type:** bug
- **Owner:** drmoisan
- **Last Updated:** 2026-08-10T14-10
- **Status:** Prepared (epic wave 2)
- Work Mode: full-bug
- **Epic:** build-ci-coverage-gate-fidelity
- **Integration branch:** epic/build-ci-coverage-gate-fidelity-integration
- **Depends on:** #457 (which depends on #441/#478)
- **Promoted from:** `docs/features/potential/promoted/2026-08-07-conflicting-coverage-thresholds-across-policy-docs.md`

## Problem

Two always-loaded authoritative policy documents state different coverage thresholds, and
neither defers to the other.

| Source | Line coverage | Branch coverage | New/changed code |
|---|---|---|---|
| `CLAUDE.md` § UT2 (Coverage and Scenarios) | >= 80% repo-wide | not stated | >= 90% for new modules/classes/methods |
| `.claude/rules/general-unit-test.md` (Coverage Requirements) | >= 85% all tiers | >= 75% all tiers | not stated |
| `.claude/rules/quality-tiers.md` (gate matrix) | >= 85% | >= 75% | not stated |

The conflict is not only numeric. `CLAUDE.md` § UT2 carries a COM/VSTO/WinForms "testable
denominator" exemption applied via `[ExcludeFromCodeCoverage]` and `coverage.config` assembly
excludes. `.claude/rules/general-unit-test.md` carries a "no production file may be excluded
from coverage measurement" clause and instructs feature-review agents to treat any `exclude`
entry matching a production source path as **Blocking**. These two clauses cannot both stand
as written.

`CLAUDE.md`'s own Policy Compliance Order places itself first, which would make 80/90
authoritative, but that precedence rule does not resolve the exclusion-policy contradiction.

## Impact

`CLAUDE.md` instructs agents to halt and notify the user on any conflicting instruction. A
conflict embedded in the policy documents themselves puts every agent in an unresolvable
position on nearly every code change. In practice agents have improvised rather than halting:
issue #424 established an in-repo precedent (no-regression against a captured baseline plus a
90% changed-line bar as blocking, with raw repo-wide figures reported non-blocking), and
issue #230 / PR #479 applied that precedent by analogy in plan decisions D5 and D12. A
precedent carried between runs by agent memory and prior-plan archaeology is not a policy.

## Verified Current State (captured 2026-08-10, integration branch `edf3d34c`)

These facts were verified directly against the working tree and constrain the fix.

1. **The divergence is wider than the three named documents.** Threshold statements split into
   two camps across the governance surface:
   - 80/90 camp: `CLAUDE.md:297,303,304`; `AGENTS.md:372,373`; `.claude/rules/csharp.md:39,40`;
     `.claude/rules/python.md:16,88,89`; `.claude/rules/typescript.md:42,43`;
     `.claude/skills/csharp-qa-gate/SKILL.md:46`; `.claude/skills/python-qa-gate/SKILL.md:46`.
   - 85/75 camp: `.claude/rules/general-unit-test.md:23,24`; `.claude/rules/quality-tiers.md:33,34,51`;
     `.claude/rules/powershell.md:63,64`; `.claude/skills/powershell-qa-gate/SKILL.md:45`;
     `.claude/skills/feature-review-workflow/SKILL.md:112-114`.
   - A parallel mirror bundle under `.agents/skills/` restates both camps.
2. **The only live numeric enforcement is a review hook, and it is internally inconsistent.**
   `.claude/hooks/validate-feature-review-coverage.ps1` hard-codes a 85.0 line floor (line 313)
   and a 75.0 branch floor (line 323), while its own `.SYNOPSIS` block (line 29) documents the
   behavior as "below 80 percent". Code and comment disagree inside one file.
3. **The coverage runner enforces no threshold at all.** `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
   contains no coverage-threshold comparison; its only failure path is `throw "MSTest with
   coverage failed with exit code $coverageExitCode"` on the test process exit code. AC4
   therefore requires new gate logic, not the edit of an existing constant.
4. **`quality-tiers.yml` does not exist and neither does the `tier-classification` CI stage.**
   `.claude/rules/quality-tiers.md:21` asserts both. The repository root has no
   `quality-tiers.yml`; `.github/workflows/` contains only `ci.yml` and
   `codex-web-setup-test.yml`. The same false claim is duplicated at
   `.agents/skills/quality-tiers/SKILL.md:27`.
5. **`coverage.config` excludes no production assembly today.** It excludes only third-party and
   F#/mixed-mode modules (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing,
   MSTest). The CLAUDE.md exemption mechanism is a stated permission that is not currently
   exercised against any production assembly.
6. **`tests/scripts/powershell/` does not exist.** Any Pester test this feature adds creates that
   tree for the first time.

## Upstream Dependency (governing numbers must be decided after the denominator is corrected)

This feature is wave 2 of the epic and executes after:

- **#441/#478** — corrects *how* lines are counted (`Get-CoberturaCoverageSummary` and
  `Merge-CoberturaClassesByFilename` double-count via the `.//lines/line` descendant axis; the
  merged-class rate blends a union denominator with primary-methods-only lines).
- **#457** — corrects *which* lines are counted (lambdas hoisted into compiler-generated closure
  types out of `[ExcludeFromCodeCoverage]` members are removed from the denominator).

Every coverage figure in the repository will therefore differ by execution time. The governing
thresholds must be decided against the corrected measurement, captured at execution time, and
must not be hard-coded from a figure measured before those features land.

## Governance-Document Authorization

The `policy-compliance-order` hard constraint against modifying `.claude/rules/` documents is
suspended for this feature only, and only for the coverage-threshold and coverage-exclusion
content this issue enumerates, because the defect is that those documents contradict each other.

Hard limits:

- A threshold may be changed only by an explicit, recorded decision with stated rationale in
  `spec.md`. A corrected denominator moving a measured figure is not by itself a justification
  for lowering a bar. If the reconciled number is lower than a number in a current document, the
  spec must say so in those words and justify it.
- Out of bounds for this feature (owned by sibling feature `csharp-toolchain-gate-fidelity-512`):
  the C# toolchain command block in `CLAUDE.md`, `.claude/rules/csharp.md`, and
  `.claude/skills/csharp-qa-gate/SKILL.md`.

## Acceptance Criteria

- [ ] AC1 — A single set of coverage thresholds appears in `CLAUDE.md` § UT2,
      `.claude/rules/general-unit-test.md`, and `.claude/rules/quality-tiers.md`, with no numeric
      disagreement between them.
- [ ] AC2 — The exclusion/exemption policy is stated once, in the document named authoritative,
      and does not contradict itself across those documents. The COM/VSTO/WinForms
      testable-denominator exemption and the "no production file may be excluded" clause are
      reconciled into one rule.
- [ ] AC3 — The documents name which source is authoritative for coverage policy, so a future
      divergence is resolvable by rule rather than by precedent. Non-authoritative documents cite
      the authority rather than restating a number that can drift.
- [ ] AC4 — Tooling enforces the agreed thresholds, and a deliberately introduced coverage
      regression fails the gate. This negative-path proof is the acceptance evidence; a
      demonstration that the gate returns non-zero on a synthetic below-threshold input is
      required, captured under `evidence/regression-testing/`.
- [ ] AC5 — The #424 / #230 improvised precedent (no-regression against a captured baseline plus
      a 90% changed-line bar as blocking, raw repo-wide figures non-blocking) is either ratified
      as the written rule or explicitly superseded, in writing, in the authoritative document.
- [ ] AC6 — The `quality-tiers.yml` / `tier-classification` / `docs/ci.research.md` claims are
      resolved: either the referenced files and the classification stage are authored, or the
      claims are removed. No governance document asserts a file that is absent. **Scope widened
      2026-08-10T16-10:** this criterion covers every site carrying the claim, not
      `.claude/rules/quality-tiers.md` alone. Verified sites are
      `.claude/rules/quality-tiers.md:9,20` and `.claude/rules/general-code-change.md:29` (both
      always-loaded), plus the `.agents/skills/quality-tiers/SKILL.md:27` snapshot copy. Rationale
      for widening: resolving one always-loaded rule file while leaving the identical false claim
      live in another reproduces the exact defect this feature exists to remove. Any T1-T4
      reference left dangling by the removal (`.claude/rules/architecture-boundaries.md`,
      `.claude/rules/powershell.md`, `.claude/rules/general-code-change.md`) must be given an
      explicit disposition rather than left pointing at deleted content.
- [ ] AC7 — The governing threshold numbers are validated against coverage re-measured under the
      post-#441/#478 and post-#457 arithmetic, with the re-measurement captured as evidence
      **before** any number is written into a governance document, and with the numbers treated as
      an input refreshed at execution time rather than a figure hard-coded from a measurement
      taken during preparation. **Disambiguation 2026-08-10T16-10:** this criterion governs
      sequencing and evidence, and does not make the measurement the selector of the threshold.
      The governing numbers are decided on the governance-provenance grounds recorded in `spec.md`
      D1; the re-measurement validates them, supplies context, and identifies which assemblies
      fail. If the re-measurement contradicts D1, the `spec.md` Risk 2 path applies: halt and
      escalate. Silently re-tuning a threshold to match a measured figure is prohibited by the
      epic non-goal and is not a permitted way to satisfy this criterion.
- [ ] AC8 — `.claude/hooks/validate-feature-review-coverage.ps1` is internally consistent: its
      documented behavior and its enforced constants state the same numbers, and those numbers
      equal the reconciled thresholds.
- [ ] AC9 — Any Pester tests added live at `tests/scripts/powershell/<Name>.Tests.ps1`, are
      deterministic, and create no temporary files.
- [ ] AC10 — Threshold-stating sites outside this feature's edit scope (`AGENTS.md`,
      `.claude/rules/python.md`, `.claude/rules/typescript.md`, `.claude/rules/powershell.md`,
      the `*-qa-gate` skills, the `.agents/skills/` mirror, and the 512-owned C# documents) are
      enumerated in the spec with a recorded disposition: aligned here, deferred to a named
      follow-up, or explicitly declared non-normative under the AC3 authority rule.

## Out of Scope

- Re-tuning any threshold implicitly to accommodate a moved figure (epic non-goal).
- The C# toolchain command block, `.claude/rules/csharp.md`, and
  `.claude/skills/csharp-qa-gate/SKILL.md` (owned by feature `csharp-toolchain-gate-fidelity-512`).
- Burning down the nullable debt (epic non-goal).
- The `/p:Nullable=enable` type-check command is a known defect (issue #522, fixed by sibling
  feature 512) producing roughly 200-414 spurious errors on a clean `main`. It is not a blocking
  gate for this feature.

## Verification Notes

- Enforcing tooling is `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, both modified first by features #441
  and #457. Anchor all locators on function/symbol names, never on absolute line numbers.
- Baseline evidence: `evidence/baseline/`. Final-QC evidence: `evidence/qa-gates/`.
  Negative-path gate proof: `evidence/regression-testing/`.
