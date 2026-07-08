# Research: Canonical Folder Structure for Multi-Feature Epics

- Date: 2026-07-07T19-00
- Author: task research (one-off, not tied to a single feature)
- Question: Is the current epic folder structure the best way to organize a multi-feature
  epic? Compare against SAFe (framework.scaledagile.com) and general docs-as-code practice,
  weighing machine readability, consistency, and human readability.

## 1. Objective & Scope

Evaluate the folder structure introduced for epic #260 (store-lockup-resilience) — the repo's
first epic — against external references and the stated quality dimensions, and recommend whether
to keep it, adjust it, or replace it. This is a governance/structure question, not a code feature;
no implementation is performed here. Any adopted change would touch the `epic-orchestrate` skill,
the `new_active_feature_folder` MCP tool, and the epic-state validator, and must be approved
separately.

## 2. Current-State Analysis (verified)

The current guidance is defined in `.claude/skills/epic-orchestrate/SKILL.md` and realized by the
MCP promotion tooling. For epic #260 it produced artifacts in **two separate trees** plus flat
sibling feature folders:

```
docs/features/epics/store-lockup-resilience/
├── epic-plan.md          # manifest: YAML frontmatter (features[]+depends_on DAG) + narrative
└── epic-status.md        # generated projection of the epic checkpoint
docs/features/active/2026-07-07-store-lockup-resilience-260/
├── issue.md              # epic GitHub issue body
└── initiative.md         # decomposition: child list, dependencies, milestones
docs/features/active/2026-07-07-store-disable-service-261/     (child F1)  ┐
docs/features/active/2026-07-07-folder-settings-store-model-null-262/ (F2) │ flat
docs/features/active/2026-07-07-store-runtime-reenable-263/    (child F3)  │ siblings
docs/features/active/2026-07-07-store-lockup-detect-notify-264/ (child F4) │
docs/features/active/2026-07-07-disabled-stores-settings-ui-265/ (child F5)┘
```

Parent↔child linkage is expressed twice: in `epic-plan.md`'s `features[]` frontmatter
(machine-readable DAG) and in the GitHub "Child Issues" list (#260 → #261–#265). During
implementation a third machine artifact appears: `artifacts/orchestration/epic-orchestrator-state.json`.

Observed strengths:
- **Machine-readable manifest.** `epic-plan.md` frontmatter carries `features[] {feature_folder,
  issue_num, depends_on[]}`, parsed deterministically for wave computation
  (`scripts/dev_tools/epic_wave_computation.py`). This is a genuine strength and should be kept.
- **Flat sibling feature folders.** Each child is an ordinary active feature folder, so it obeys
  the existing `active/ → completed/` lifecycle, gets its own git branch/worktree during the
  parallel wave build, and merges with local diff locality.

Observed weaknesses:
1. **The epic exists in two trees.** Epic-level material is split between `epics/<slug>/`
   (plan+status) and `active/<date>-<slug>-<issue>/` (issue+initiative). "Everything about the
   epic" is not in one place — a human-readability and discoverability cost.
2. **Triplicated decomposition.** The child list + dependencies appear in `epic-plan.md`
   (frontmatter + table), again in `initiative.md` (Decomposition section), and again in
   `epic-status.md` (status table). Three hand-or-semi-maintained copies drift out of sync; I had
   to edit all three when F3's `depends_on` changed during this epic's planning.
3. **Naming inconsistency between the two epic locations.** `epics/` uses a bare slug
   (`store-lockup-resilience`) while `active/` uses `<date>-<slug>-<issue>`
   (`2026-07-07-store-lockup-resilience-260`). Same entity, two naming schemes.
4. **DAG keyed by folder basename.** `depends_on` uses `feature_folder` basenames, which embed a
   date and issue number and change when a feature is promoted `active/ → completed/`. The skill
   already works around this ("… or `docs/features/completed/…` if already promoted"), which is a
   symptom of a brittle key.

## 3. External Guidance

### 3.1 SAFe (Scaled Agile Framework)

SAFe defines a four-tier work-item hierarchy — **Epic → Capability → Feature → Story** — where
"Portfolio Epics are split into Capabilities, Capabilities into Features, and Features into
Stories." An epic is "a significant initiative that requires portfolio-level oversight," and each
epic's canonical artifacts are an **epic hypothesis statement** (four fields: epic description,
business-outcome hypothesis, leading indicators, and NFRs), a **Lean business case**, and an
**MVP** definition. Epics are one of two types — **Business** or **Enabler**.

The load-bearing insight for folder design: SAFe's containment is a **logical/backlog** hierarchy
(managed on a Portfolio Kanban), **not a filesystem layout**. SAFe never prescribes that a
feature's artifacts be physically nested inside an epic's; the parent-child relationship is a
data relationship (an epic *references* its features). This directly supports keeping child
feature folders as flat siblings linked by a manifest rather than physically nested.

What SAFe suggests the repo is *missing*: a first-class, structured **epic hypothesis / intent**
record (business-outcome hypothesis + leading indicators + NFRs). The current `epic-plan.md`
narrative captures goal/scope/non-goals but not a machine-checkable hypothesis/outcome section.

### 3.2 Docs-as-code / monorepo practice

Monorepo guidance (Nx, Backstage mkdocs-monorepo) finds that **flat structures evolve to nested
grouping as scale grows**, because nesting "minimizes the time developers spend navigating the
folder tree." Countervailing: keeping docs as independent units close to their code enables
folder-based ownership (GitHub CODEOWNERS) and independent lifecycle. For our case the two pull in
opposite directions: nesting children under the epic would aid *navigation/grouping* but break the
*independent active→completed lifecycle* and per-feature worktree isolation that the wave build
depends on. ADR/RFC conventions (one immutable, numbered file per decision in a flat directory
with an index) reinforce the pattern actually worth adopting: **flat item files + one machine
index**, not deep nesting.

## 4. Evaluation Scorecard

Rating each candidate against the requested dimensions (✓ strong, ~ adequate, ✗ weak):

| Dimension | A. Current (dual-tree + flat + 3 docs) | B. Nested children under epic | C. Flat-only, epic in `active/` | D. Recommended (single epic home + flat + one manifest) |
|---|---|---|---|---|
| Machine readability | ✓ manifest frontmatter | ~ path implies DAG but fragile | ✓ | ✓ manifest keyed by issue_num |
| Consistency | ✗ two trees, 3 copies, 2 name schemes | ~ | ✓ epic == active item | ✓ one epic home, one manifest |
| Human readability | ~ epic split across trees | ✓ grouping, ✗ deep nesting | ~ epic lost in flat sea | ✓ dedicated epic home + linked children |
| Git / worktree ergonomics | ✓ flat children isolate | ✗ nested children collide, no clean per-feature worktree | ✓ | ✓ flat children unchanged |
| Lifecycle (active→completed) | ✓ children move independently | ✗ epic folder can't partially complete | ✓ | ✓ children independent; epic completes as a unit |
| Scalability (many features) | ~ flat sea, manifest saves it | ✓ grouped, ✗ churn | ~ | ✓ manifest is the grouping index |
| Drift risk | ✗ 3 decomposition copies | ~ | ✓ | ✓ single source, status generated |

## 5. Recommended Approach

**Keep the two correct core decisions and fix the two weaknesses.** The core of the current design
— a machine-readable manifest plus flat, independently-lifecycled sibling feature folders — is
correct and is corroborated by both SAFe (logical, not physical, containment) and docs-as-code
practice (flat items + one index). The problems are duplication and fragmentation, not the flat
model. Recommended structure (option D):

```
docs/features/epics/<epic-slug>/
├── epic.md               # SINGLE epic home: YAML frontmatter (manifest DAG + SAFe intent)
│                         #   + narrative (goal/scope/non-goals/shared-design). Also the source
│                         #   the epic GitHub issue body is generated from.
└── epic-status.md        # GENERATED projection only (never hand-edited)
docs/features/active/<date>-<feature-slug>-<issue>/   # child features: flat siblings, unchanged
    └── … issue.md, spec.md, user-story.md, plan.<ts>.md, research/, evidence/
```

Concrete changes from today's structure:
1. **Collapse to one epic home.** Merge `initiative.md`'s decomposition and the epic `issue.md`
   into `epic-plan.md` (rename to `epic.md`), living under `docs/features/epics/<epic-slug>/`. Do
   not create a separate `active/<date>-<epic>-<issue>/` folder for the epic. This removes the
   dual-tree and the naming inconsistency, and eliminates two of the three decomposition copies.
2. **Keep `epic-status.md` strictly generated** from the epic checkpoint; it is the only place
   status is written, and it is never the source of the DAG.
3. **Key the manifest DAG by stable `issue_num`, not folder basename.** Treat `feature_folder` as a
   resolvable hint (and allow it to point into `completed/`). This removes the active→completed
   path-drift workaround.
4. **Add a structured SAFe-style intent block** to the manifest frontmatter — `epic_type:
   business|enabler`, `business_outcome_hypothesis`, `leading_indicators[]`, `nfrs[]` — so epic
   intent is machine-checkable, not just prose. This is the one net-new idea from SAFe worth
   importing.
5. **Keep flat child feature folders exactly as they are** (worktree isolation, independent
   lifecycle, per-feature CODEOWNERS all depend on it).

Rejected alternatives (brief):
- **B. Physically nest children under the epic** (`active/<epic>/<feature>/`). Rejected: it breaks
  the per-feature git worktree/branch model the wave build uses, prevents a feature from moving to
  `completed/` independently of its epic, and worsens merge locality — costs that outweigh the
  navigation benefit, which the manifest already provides.
- **C. Put the epic in `active/` like any other item and drop the `epics/` tree.** Attractive for
  "an epic is just an active work item with children," but it (a) removes the stable, greppable
  `epics/<slug>/` manifest anchor that the skill/hooks/validator hard-code, and (b) buries the epic
  in the flat feature sea, hurting discoverability. Preferred only if the team wants maximal
  uniformity over a distinct epic namespace.
- **Status quo (A).** Functional and already shipped for #260, but carries the three-copy drift
  risk and dual-tree fragmentation documented in §2; not optimal.

## 6. Answer to the Question

The current structure is **good but not optimal.** Its two foundational choices — a machine-readable
manifest and flat, independently-lifecycled feature folders — are the right ones and are validated
by SAFe (containment is logical, not physical) and by docs-as-code norms (flat items + one index).
Its two defects are avoidable duplication (decomposition in three files) and fragmentation (the epic
living in two trees with two naming schemes). Consolidating to a single epic home (`epics/<slug>/epic.md`
+ generated `epic-status.md`), keying the DAG by `issue_num`, and adding a structured SAFe intent block
resolves those defects while preserving the strengths. For epic #260 the migration is low-cost and
non-urgent: fold `initiative.md` into `epic-plan.md`, treat `epic-status.md` as generated-only, and
(optionally) retire the `active/2026-07-07-store-lockup-resilience-260/` epic folder.

## 7. Testing / Validation Implications

Structure changes are validated by tooling, not unit tests of product code:
- The epic-state validator (`scripts/dev_tools/validate_epic_orchestrator_state.py`) and
  `epic_wave_computation.py` already parse the manifest; keying by `issue_num` requires updating
  their key resolution and their tests.
- A cheap guard worth adding: a lint check that fails if `epic-status.md` is edited by hand (i.e.
  differs from a regeneration from the checkpoint), enforcing "status is generated, plan is source."
- No temporary files; deterministic parsing; consistent with repo policy.

## 8. Sources

- [Epic — Scaled Agile Framework](https://framework.scaledagile.com/epic)
- [Features and Capabilities — Scaled Agile Framework](https://framework.scaledagile.com/features-and-capabilities/)
- [Story — Scaled Agile Framework](https://scaledagileframework.com/story/)
- [Enterprise Backlog Structure and Management — Scaled Agile Framework](https://framework.scaledagile.com/enterprise-backlog-structure-and-management)
- [Epic Hypothesis Statement template (SAFe)](https://framework.scaledagile.com/wp-content/uploads/2025/03/Epic-Hypothesis-Statement.docx)
- [Lean Business Case template (SAFe)](https://framework.scaledagile.com/wp-content/uploads/2025/03/Lean-Business-Case-V7.docx)
- [Folder Structure — Nx](https://nx.dev/docs/concepts/decisions/folder-structure)
- [Backstage mkdocs-monorepo-plugin](https://github.com/backstage/mkdocs-monorepo-plugin)

## 9. File References

- `.claude/skills/epic-orchestrate/SKILL.md` — current epic structure guidance (manifest path,
  epic-plan vs epic-status, integration lifecycle).
- `docs/features/epics/store-lockup-resilience/epic-plan.md`, `epic-status.md` — current manifest + status.
- `docs/features/active/2026-07-07-store-lockup-resilience-260/{issue.md, initiative.md}` — current
  dual-tree epic artifacts.
- `scripts/dev_tools/epic_wave_computation.py`, `scripts/dev_tools/validate_epic_orchestrator_state.py`
  — manifest consumers that would change if the DAG key or paths change.
