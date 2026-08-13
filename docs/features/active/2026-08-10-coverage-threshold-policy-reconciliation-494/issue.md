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

## User-Authorized Scope Correction

This section supersedes the earlier governance-document authorization and every receipt-gated
delivery assumption in this issue. The TaskMaster deliverable for every prohibited `CLAUDE.md`
or `.claude/**` change is the existing upstream prompt at
`evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md`.

TaskMaster must not edit `CLAUDE.md`, any `.claude/**` path, or `.agents/skills/**`, and it must
not write to an upstream repository. Future application of the prompt is expressly deferred
outside TaskMaster. No upstream receipt, release, validation, publication, or other external
evidence is required for TaskMaster completion.

The remaining TaskMaster-owned scope is limited to permitted coverage tooling and deterministic
tests. The existing corrected-arithmetic coverage evidence remains an execution input and may
not select or lower a threshold. The issue #512 C# toolchain ownership boundary remains intact.

## Acceptance Criteria

- [ ] AC1 — The existing upstream prompt is retained as the complete TaskMaster deliverable for
      reconciling coverage thresholds in the upstream source of truth; no TaskMaster `CLAUDE.md`
      or `.claude/**` file is changed, and future application is deferred outside TaskMaster.
- [ ] AC2 — The existing upstream prompt explicitly requires the upstream owner to reconcile the
      coverage exclusion/exemption policy; TaskMaster records that deferred requirement without
      editing any local Claude-runtime path.
- [ ] AC3 — The existing upstream prompt explicitly requires one authoritative upstream coverage
      policy source and non-conflicting generated references; TaskMaster records that deferred
      requirement without editing any local Claude-runtime path.
- [ ] AC4 — TaskMaster coverage tooling rejects a valid synthetic Cobertura result below 80%,
      accepts the exact 80% boundary, and has deterministic negative-path evidence under
      `evidence/regression-testing/`; upstream Claude-hook reconciliation remains deferred.
- [ ] AC5 — The existing upstream prompt carries the requirement to ratify or supersede the
      #424/#230 precedent in the authoritative upstream policy; TaskMaster records that deferred
      requirement without editing any local Claude-runtime path.
- [ ] AC6 — The existing upstream prompt carries the requirement to resolve the false
      `quality-tiers.yml`, `tier-classification`, and `docs/ci.research.md` claims at their
      upstream-owned runtime sites; TaskMaster records that deferred requirement without editing
      `.claude/**` or `.agents/skills/**`.
- [ ] AC7 — Corrected-arithmetic remeasurement evidence is retained and validated as an
      execution-time input before the local threshold gate is implemented; it does not select or
      lower a threshold.
- [ ] AC8 — The existing upstream prompt carries the requirement to reconcile the upstream
      feature-review coverage hook's documentation and behavior; TaskMaster records that deferred
      requirement without editing `.claude/hooks/**`.
- [ ] AC9 — Added Pester tests mirror their TaskMaster coverage-tooling subjects, are deterministic,
      and create no temporary files.
- [ ] AC10 — The existing upstream prompt identifies the future affected TaskMaster paths and
      requires upstream coverage-site disposition; TaskMaster records the deferral without editing
      the protected Claude or Codex runtime policy surfaces.

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
