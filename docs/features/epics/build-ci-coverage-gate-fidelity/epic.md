---
epic: build-ci-coverage-gate-fidelity
integration_branch: epic/build-ci-coverage-gate-fidelity-integration
created_at: 2026-08-10T14-05
intent:
  epic_type: enabler
  business_outcome_hypothesis: >-
    Repository quality gates report figures that correspond to what they claim to measure, so
    that the 59 remaining open bugs in Lanes B through M can be delivered against evidence that
    is trustworthy rather than against gates that cannot fail.
  leading_indicators:
    - A deliberately introduced nullable violation fails the documented type-check gate.
    - A deliberately introduced coverage regression fails the documented coverage gate.
    - Reported repository-wide lines-valid equals the distinct-line count, not twice it.
    - The documented C# toolchain commands execute successfully against a clean main.
  nfrs:
    - No coverage threshold may be lowered to accommodate a corrected denominator without an
      explicit, recorded decision.
    - Every corrected figure must be accompanied by a re-captured baseline in the same change.
features:
  - issue_num: 441
    feature_folder: cobertura-coverage-arithmetic-441
    depends_on: []
  - issue_num: 457
    feature_folder: excludefromcodecoverage-nested-lambdas-457
    depends_on: [441]
  - issue_num: 512
    feature_folder: csharp-toolchain-gate-fidelity-512
    depends_on: []
  - issue_num: 494
    feature_folder: coverage-threshold-policy-reconciliation-494
    depends_on: [457]
  - issue_num: 394
    feature_folder: utilitiescs-test-cs2002-duplicate-compile-entry-394
    depends_on: []
---

# Epic: Build / CI / Coverage Gate Fidelity

## Goal

Make the repository's build, type-check, and coverage gates report truthfully. Every gate in
scope currently either measures the wrong quantity, cannot fail, or is documented with a command
that does not execute. Until they are corrected, no other bug fix in the backlog can be certified.

## Source

This epic is Lane A of `docs/research/2026-08-10-parallel-bug-flighting-and-surface-blockers.md`
(§4 Lane A, §5 Flight 0, §9 Fallback Path). That document establishes Lane A as the prerequisite
flight: it must run first and must run alone, because the remaining 59 open bugs would otherwise
be certified against gates that cannot fail.

The research document scoped Lane A as ten issues. This epic delivers nine of them. Issue 513 is
excluded; see Non-Goals.

## Scope

Nine open bug issues, grouped into five independently mergeable child features:

| Feature | Issues closed | Primary surface |
| --- | --- | --- |
| `cobertura-coverage-arithmetic-441` | 441, 478 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` |
| `excludefromcodecoverage-nested-lambdas-457` | 457 | `scripts/vscode/Invoke-MSTestWithCoverage*.ps1`, `coverage.config` |
| `csharp-toolchain-gate-fidelity-512` | 492, 509, 512, 522 | `CLAUDE.md`, `.claude/rules/csharp.md`, `.claude/skills/csharp-qa-gate/SKILL.md` |
| `coverage-threshold-policy-reconciliation-494` | 494 | `CLAUDE.md` § UT2, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md` |
| `utilitiescs-test-cs2002-duplicate-compile-entry-394` | 394 | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` |

## Non-Goals

- **Issue 513 (`collect-pr-context-misclassifies-csharp-as-documentation`) is out of scope.**
  The defective classification step is implemented at
  `extensions/drm-copilot/src/lib/pr-context/collector-output.ts` in the separate `drm-copilot`
  governance repository. TaskMaster consumes `collect_pr_context` as an installed MCP extension
  and contains no copy of that source; a grep across this repository returns only two hook files
  that invoke the tool. No branch cut from this repository can close 513. It must be filed and
  fixed upstream in `drm-copilot`.
- **Burning down the nullable debt is out of scope.** Issue 492 states the separation explicitly:
  first make the gate report truthfully, then decide how to burn down the roughly 195 to 220
  `CS86xx` diagnostics it exposes in `UtilitiesCS.csproj`. This epic delivers only the first half.
  The debt burn-down is a follow-on epic sized against whatever figure the corrected gate reports.
- **Re-tuning gate thresholds to accommodate the corrected denominator is out of scope as an
  implicit act.** Feature `coverage-threshold-policy-reconciliation-494` decides the thresholds
  explicitly and on the record. No child may silently lower a threshold to accommodate a number
  that moved.

## Shared Design

Two distinct correctness problems run through this epic, and the decomposition follows them.

**Problem 1 — the coverage figure does not measure what it claims.** Three independent defects
compound in the same PowerShell post-processing module:

- Issue 441: `Get-CoberturaCoverageSummary` and `Merge-CoberturaClassesByFilename` both select
  over the XPath descendant axis `.//lines/line`. Each `<class>` carries its `<line>` nodes twice
  (once nested under each `<method>`, once as a class-level rollup), so every line is counted
  twice. The committed sample's `lines-valid="110849"` equals the raw `<line number=` count,
  confirming the double count.
- Issue 478: `Merge-CoberturaClassesByFilename` unions the class-level `<lines>` of classes
  sharing a filename correctly but never merges their `<methods>` subtrees, then recomputes
  `line-rate` over the same descendant axis. The emitted rate blends the correct union with only
  the primary class's method-level lines and matches neither denominator.
- Issue 457: a method-level `[ExcludeFromCodeCoverage]` does not suppress lambdas declared inside
  the attributed member, because the compiler hoists them into a closure type that does not
  inherit the attribute. Files adopting the repository's preferred "thin exempt production
  forwarder" seam carry a permanent, invisible coverage ceiling.

**Problem 2 — the documented gate commands do not execute what they claim.** Four defects share
the same six documentation sites:

- Issue 512: `/t:Build` lets MSBuild's incremental up-to-date check skip `CoreCompile`, so a
  command-line `/p:` change alone never reaches the compiler. The gate returns exit 0 without
  running. `.github/workflows/ci.yml` already documents this behavior in-line and uses
  `/t:Rebuild` for its own step; the policy command does not.
- Issue 492: the same masking, measured against `UtilitiesCS.csproj`, which a forced rebuild
  shows carries 195 nullable errors the gate never reports.
- Issue 522: `/p:Nullable=enable` is deliberately absent from CI, because the repository uses
  per-file `#nullable enable` opt-in. Forcing it solution-wide produces roughly 200 to 414 errors
  that are red on a clean `main`. The documented gate therefore can never pass and manufactures
  false blocking findings; two deliveries on 2026-08-08 (#507, #508) required human override.
- Issue 509: `CLAUDE.md` documents `dotnet tool run csharpier .`, which is CSharpier v0 syntax.
  `dotnet-tools.json` pins 1.2.6, which requires the `format` subcommand. CI already uses the
  correct form (`ci.yml:93`).

All four defects edit the same regions of the same three files: `CLAUDE.md` lines 185-206 and
381-401, `.claude/rules/csharp.md` lines 14-16 and 83, and `.claude/skills/csharp-qa-gate/SKILL.md`
line 32. They are therefore one feature, not four.

## Decomposition Rationale

Grouping is driven by file-level contention and by shared verification cost, not by issue count.

- **441 and 478 are one feature.** Both correct the rate recomputation inside
  `Merge-CoberturaClassesByFilename`; 478's own text describes itself as "distinct from, and
  additional to, issue #441". Two fixes to the same expression cannot be separate parallel
  features. Both also require the same repository-wide coverage baseline re-capture, which is the
  dominant cost; performing it twice would be waste.
- **457 is a separate feature that depends on 441.** Its mechanism is different — which lines
  enter the denominator at all, rather than how they are counted — but it edits the same module.
  The dependency is genuine rather than stylistic: 457's fix cannot be verified against a
  denominator that is still double-counted, and its acceptance evidence is a corrected per-file
  rate that only exists after 441/478 land.
- **492, 509, 512 and 522 are one feature.** They edit overlapping lines of the same three
  governance files. Delivering them separately would guarantee merge conflicts on every one of
  the six sites and would leave the toolchain block internally inconsistent between merges. The
  research document reaches the same conclusion for 492/512/522 independently ("resolve them as
  a set rather than individually"); 509 is added because it edits step 1 of the same documented
  toolchain block that 512 and 522 rewrite at step 3.
- **494 depends on 457.** Its acceptance criteria require that tooling enforce the agreed
  thresholds and that a deliberately introduced coverage regression fail the gate. A threshold
  cannot be ratified against a figure that is still wrong, and the figure is not correct until
  both 441/478 (how lines are counted) and 457 (which lines are counted) have landed. The edge to
  457 carries the 441 dependency transitively.
- **494 does not depend on the toolchain feature.** Both edit `CLAUDE.md`, but in disjoint
  sections — § UT2 coverage policy versus the C# toolchain command block. Disjoint sections merge
  cleanly, and an ordering edge here would cost a wave for no correctness gain.
- **394 is standalone.** Removing one of two identical `<Compile Include>` items from
  `UtilitiesCS.Test.csproj` (both confirmed present) shares no surface with any other feature.

## Wave Assignment

Computed by longest-path layering: `wave(f) = 0` when `depends_on` is empty, otherwise
`1 + max(wave(d))`. The graph is cycle-free and every `depends_on` entry resolves within
`features[]`.

| Wave | Features |
| --- | --- |
| 0 | `cobertura-coverage-arithmetic-441`, `csharp-toolchain-gate-fidelity-512`, `utilitiescs-test-cs2002-duplicate-compile-entry-394` |
| 1 | `excludefromcodecoverage-nested-lambdas-457` |
| 2 | `coverage-threshold-policy-reconciliation-494` |

Critical path length is three waves: 441 to 457 to 494.

## Complexity Assessment

| Feature | Band | Rationale |
| --- | --- | --- |
| `cobertura-coverage-arithmetic-441` | C3 | `cross_module_contract_change` floor. The coverage figure is a contract consumed by every gate, every committed baseline, and epic #136's fifteen per-file children. |
| `excludefromcodecoverage-nested-lambdas-457` | C3 | `cross_module_contract_change` floor. Alters which lines enter the coverage denominator repository-wide. |
| `csharp-toolchain-gate-fidelity-512` | C3 | `cross_module_contract_change` floor. The documented toolchain is the contract every agent session and contributor executes. |
| `coverage-threshold-policy-reconciliation-494` | C3 | `cross_module_contract_change` floor. Reconciles three governance documents that currently contradict each other on both thresholds and exclusion policy, and encodes the result in tooling. |
| `utilitiescs-test-cs2002-duplicate-compile-entry-394` | C1 | `single_file_localized_edit` and `mechanical_rename_or_move`. Delete one duplicate `<Compile>` item; no behavior change. |

## Execution Authorization Required

Two child features edit documents that the `policy-compliance-order` skill places under a hard
constraint: "Do NOT modify policy documents under `.claude/rules/` or `.github/instructions/`."

- `csharp-toolchain-gate-fidelity-512` must edit `CLAUDE.md` and `.claude/rules/csharp.md`.
- `coverage-threshold-policy-reconciliation-494` must edit `CLAUDE.md`,
  `.claude/rules/general-unit-test.md`, and `.claude/rules/quality-tiers.md`.

These edits are the substance of issues 494, 509 and 522 — the defect *is* that the governance
documents are wrong. Planning does not perform them; preparation produces specifications and
atomic plans that propose them. Executing this epic constitutes the authorization to apply them,
and the edits must remain scoped to exactly the sites the issues enumerate. No child may edit a
governance document for any purpose outside its own issue's acceptance criteria, and in
particular no child may relax a policy in order to make a gate pass.

## Risks and Coordination Notes

- **Baseline invalidation across the in-flight epic #136.** Feature
  `cobertura-coverage-arithmetic-441` changes every reported coverage figure in the repository.
  Twenty-one unmerged branches from the QuickFiler per-file coverage epic
  (`epic/quickfiler-per-file-coverage-integration` and its children) gate on per-file line rates
  computed by the defective code. None of them touches any file this epic modifies, so there is
  no merge conflict, but their committed coverage evidence will not reproduce against the
  corrected arithmetic. This is a sequencing consideration outside this epic's dependency graph
  and cannot be modeled as a wave. Decide before merging this epic to `main` whether epic #136
  lands first or re-baselines afterward.
- **`quality-tiers.yml` does not exist.** `.claude/rules/quality-tiers.md` states that a
  `quality-tiers.yml` at the repository root maps every project to a tier and that CI fails on an
  unclassified project. Neither the file nor that CI stage exists. Feature
  `coverage-threshold-policy-reconciliation-494` must resolve this as part of reconciling the
  documents rather than assuming the file is present.
- **Threshold movement is expected, not exceptional.** Correcting the denominator will move every
  reported figure. Feature 494 owns the decision about what the thresholds become; features 441
  and 457 own re-capturing baselines but must not re-tune thresholds.
