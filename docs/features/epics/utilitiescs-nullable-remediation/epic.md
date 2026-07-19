---
epic: utilitiescs-nullable-remediation
integration_branch: epic/utilitiescs-nullable-remediation-integration
created_at: 2026-07-18T21-30
intent:
  epic_type: enabler
  business_outcome_hypothesis: >-
    The CI nullable gate (repaired by PR #361 to use /t:Rebuild) can be genuinely enforced
    without permanently blocking future PRs, by remediating the pre-existing CS86xx nullable
    debt under a per-file #nullable enable opt-in architecture.
  leading_indicators:
    - The nullable-gate build passes on the integration branch under per-file pragma enforcement.
    - Each child feature merges independently without cross-blocking non-opted-in files.
  nfrs:
    - No behavior changes; null-annotation and null-safety remediation only.
    - No reduction in coverage on changed lines.
features:
  - issue_num: 363
    feature_folder: utilitiescs-nullable-extensions
    depends_on: []
  - issue_num: 364
    feature_folder: utilitiescs-nullable-helperclasses
    depends_on: []
  - issue_num: 9003
    feature_folder: utilitiescs-nullable-reusabletypes
    depends_on: []
  - issue_num: 9004
    feature_folder: utilitiescs-nullable-newtonsofthelpers
    depends_on: []
  - issue_num: 9005
    feature_folder: utilitiescs-nullable-threading
    depends_on: []
  - issue_num: 9006
    feature_folder: utilitiescs-nullable-svgcontrol
    depends_on: []
  - issue_num: 9007
    feature_folder: utilitiescs-nullable-outlook-folder-store
    depends_on: [utilitiescs-nullable-extensions, utilitiescs-nullable-helperclasses]
  - issue_num: 9008
    feature_folder: utilitiescs-nullable-outlook-mailitem-item
    depends_on: [utilitiescs-nullable-extensions, utilitiescs-nullable-helperclasses]
  - issue_num: 9009
    feature_folder: utilitiescs-nullable-email-parsing
    depends_on: [utilitiescs-nullable-extensions]
  - issue_num: 9010
    feature_folder: utilitiescs-nullable-email-classifier
    depends_on: [utilitiescs-nullable-extensions]
  - issue_num: 9011
    feature_folder: utilitiescs-nullable-dialogs-misc
    depends_on: [utilitiescs-nullable-extensions, utilitiescs-nullable-helperclasses]
  - issue_num: 9012
    feature_folder: utilitiescs-nullable-ci-capstone
    depends_on:
      - utilitiescs-nullable-extensions
      - utilitiescs-nullable-helperclasses
      - utilitiescs-nullable-reusabletypes
      - utilitiescs-nullable-newtonsofthelpers
      - utilitiescs-nullable-threading
      - utilitiescs-nullable-svgcontrol
      - utilitiescs-nullable-outlook-folder-store
      - utilitiescs-nullable-outlook-mailitem-item
      - utilitiescs-nullable-email-parsing
      - utilitiescs-nullable-email-classifier
      - utilitiescs-nullable-dialogs-misc
---

# Epic: UtilitiesCS Nullable-Reference-Type Remediation

> **Placeholder issue numbers.** The `issue_num` values `9001`-`9012` in the frontmatter are
> provisional placeholders assigned at manifest-authoring time, before child promotion. Each is
> back-filled with the real GitHub issue number from its child's promotion receipt as preparation
> completes. `depends_on` edges are expressed as `feature_folder` basenames (a form the
> `epic-orchestrate` schema resolves via the `feature_folder` index), so the dependency graph
> remains valid while `issue_num` values are still placeholders.

## Goal

Remediate the pre-existing nullable-reference-type debt (~2131 CS86xx diagnostics across ~234
files) that the CI nullable gate was silently failing to catch, so that the repaired gate can be
genuinely enforced going forward without permanently blocking future PRs. This is null-annotation
and null-safety remediation only; no behavior changes.

## Base Branch and Relationship to PR #361

This epic is premised on the nullable-gate repair delivered by **PR #361**
(`fix/ci-nullable-gate-masking`), which changes the CI step to `msbuild /t:Rebuild ...
/p:Nullable=enable /p:TreatWarningsAsErrors=true` so the gate performs a genuine recompile instead
of a silently-skipped incremental build. At epic-planning time **PR #361 is OPEN, not yet merged
to `main`**. The nullable-gate fix therefore does not exist on `origin/main`.

Consequently, the integration branch `epic/utilitiescs-nullable-remediation-integration` is based
on the PR #361 head commit `20d163ac` (which is `origin/main` plus exactly the one gate-repair
commit), **not** on `origin/main`. This ensures every child feature is prepared and later executed
against the real, repaired gate. If PR #361 merges to `main` before the epic's final
integration-to-`main` PR, that merge is clean because `20d163ac` is a direct descendant of
`origin/main`. This is a deliberate, documented deviation from the default "branch off
`origin/main`" rule, required by the epic's premise.

## Scope

- All CS86xx nullable-diagnostic remediation across `UtilitiesCS/` (~40 subdirectories), grouped
  into cohesive, independently mergeable clusters.
- `SVGControl/` (a separate `net481` WinForms control project) is included because the current
  solution-level nullable gate covers it too; it is remediated and opted in on the same per-file
  basis.
- Finalization of the CI nullable-gate enforcement mechanism to match the per-file opt-in
  architecture (the capstone child).

## Non-Goals

- No behavior changes, refactors, or feature work. Null-annotation and null-safety only.
- No global force-enable of nullable at the project or solution level as the enforcement
  mechanism (see Shared Design). A project-level `<Nullable>enable</Nullable>` flip is considered
  only as an optional, separately-gated capstone step once every file is opted in and clean.
- No editing of `.claude/rules/*` (policy prohibits it); the rules-vs-convention conflict is
  flagged for the maintainer rather than resolved by editing policy.

## Shared Design

### Per-file `#nullable enable` opt-in (confirmed architecture)

`UtilitiesCS.csproj` has no `<Nullable>` element (nullable OFF at project level). The organic
repository convention is per-file opt-in: a subset of `.cs` files already carry `#nullable enable`
pragmas. The confirmed architecture continues that convention:

- Each file that is remediated receives a `#nullable enable` pragma and is brought to zero CS86xx
  diagnostics under that pragma.
- Files that are not yet remediated remain non-opted-in and are not cross-blocking.
- This makes each child feature independently mergeable and CI-passable. A global force-enable
  would make no child mergeable until all ~234 files were fixed simultaneously.

### CI gate enforcement change (capstone)

Because enforcement moves to per-file pragmas, the gate must stop overriding `-p:Nullable=enable`
globally at the solution level. The capstone child revises the PR #361 gate step to rely on each
file's own pragma under `/t:Rebuild /p:TreatWarningsAsErrors=true`, so opted-in files are
enforced while non-opted-in files are not cross-blocking. The capstone verifies genuine
enforcement (a deliberately-introduced null defect in an opted-in file fails the gate) and flags
the rules-vs-convention conflict for the maintainer.

### Rules-vs-convention conflict (flagged, not resolved)

`.claude/rules/csharp.md` documents the toolchain as forcing `/p:Nullable=enable` globally, which
conflicts with the codebase's per-file opt-in convention. Policy prohibits editing
`.claude/rules/*`. This conflict is FLAGGED for the maintainer (consistent with the
coverage-threshold conflict-handling precedent), not silently resolved.

## Decomposition and Waves

Clusters follow `UtilitiesCS/` subdirectory cohesion. Shared, cross-module clusters
(Extensions, HelperClasses, ReusableTypes, NewtonsoftHelpers) are remediated first so that
downstream Outlook/Email/Dialogs clusters consume already-annotated contracts. Dependency edges
reflect real annotation-contract consumption only.

Wave assignment uses longest-path layering (`wave = 0` when `depends_on` is empty, else
`1 + max(wave(dep))`):

### Wave 0 (no dependencies)

| feature_folder | cluster | est. files | complexity |
| --- | --- | --- | --- |
| utilitiescs-nullable-extensions | `Extensions/` | ~12 | C3 |
| utilitiescs-nullable-helperclasses | `HelperClasses/` (incl. FileSystem, ThemeHelpers, root) | ~15 | C3 |
| utilitiescs-nullable-reusabletypes | `ReusableTypeClasses/` (incl. TimedActions, NewSmartSerializable) | ~12 | C3 |
| utilitiescs-nullable-newtonsofthelpers | `NewtonsoftHelpers/` | ~14 | C3 |
| utilitiescs-nullable-threading | `Threading/` | ~15 | C3 |
| utilitiescs-nullable-svgcontrol | `SVGControl/` (separate net481 WinForms project) | ~14 | C2 |

### Wave 1 (consume wave-0 shared annotations)

| feature_folder | cluster | depends_on | est. files | complexity |
| --- | --- | --- | --- | --- |
| utilitiescs-nullable-outlook-folder-store | `OutlookObjects/Folder` + `OutlookObjects/Store` | extensions, helperclasses | ~29 | C2 |
| utilitiescs-nullable-outlook-mailitem-item | `OutlookObjects/MailItem` + Item + Conversation + Attachment + Table | extensions, helperclasses | ~25 | C2 |
| utilitiescs-nullable-email-parsing | `EmailIntelligence/EmailParsingSorting` + SubjectMap + Ctf | extensions | ~18 | C2 |
| utilitiescs-nullable-email-classifier | `EmailIntelligence/Bayesian` + ClassifierGroups + Flags | extensions | ~18 | C3 |
| utilitiescs-nullable-dialogs-misc | `Dialogs/` + remaining small subdirs (catch-all) | extensions, helperclasses | ~16 | C2 |

### Wave 2 (capstone)

| feature_folder | cluster | depends_on | est. files | complexity |
| --- | --- | --- | --- | --- |
| utilitiescs-nullable-ci-capstone | CI nullable-gate finalization + rules-conflict flag + optional project-level Nullable | all eleven remediation children | ~3 | C2 |

## Complexity Rationale

- **C3 (opus-tier)** clusters alter contracts consumed across module boundaries
  (`cross_module_contract_change` floor signal): Extensions, HelperClasses, ReusableTypes,
  NewtonsoftHelpers. Threading is floored C3 by `concurrency_or_ordering`. Email-classifier is
  floored C3 by `classifier_or_model_logic` (it touches T1 classifier modules), though the change
  is annotation-only with no scoring change.
- **C2 (sonnet-tier)** clusters are file-local null guards/annotations in adapter/UI code
  (Outlook adapters, Email parsing, Dialogs, SVGControl) and the small CI capstone.

Each band is a reviewed starting assessment; each child orchestrator re-runs its own
model-selection step during preparation.
