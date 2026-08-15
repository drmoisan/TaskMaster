# Phase 0 — Feature Requirement Inputs and Governance Authorization Read ([P0-T2])

Timestamp: 2026-08-10T22-32
Command: (none — analysis artifact)
EXIT_CODE: (none — analysis artifact)

## Requirement inputs read

| # | Path | Role |
|---|---|---|
| 1 | `docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/spec.md` | Authoritative acceptance-criteria source (work mode `full-bug`); per-site replacement tables; Blocks R1-R6; SD1-SD4 |
| 2 | `docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/issue.md` | Issue statement; `- Work Mode: full-bug` marker; AC1-AC13 source of numbering |
| 3 | `docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/research/toolchain-gate-fidelity.2026-08-10T14-40.md` | Research; recommendations D4-D7 |
| 4 | `docs/features/epics/build-ci-coverage-gate-fidelity/epic.md` § "Execution Authorization Required" | Governance-edit authorization |

## The nine pre-existing evidence artifacts, enumerated by filename

Eight under `evidence/baseline/`:

| # | Filename |
|---|---|
| 1 | `baseline-analyzer-step-vacuity.2026-08-10T14-55.md` |
| 2 | `baseline-ci-parity-on-main.2026-08-10T15-05.md` |
| 3 | `baseline-csharpier-documented-command.2026-08-10T14-25.md` |
| 4 | `baseline-csharpier-replacement-forms.2026-08-10T14-45.md` |
| 5 | `baseline-mirror-provenance.2026-08-10T15-30.md` |
| 6 | `baseline-nullable-gate-vacuity.2026-08-10T14-25.md` |
| 7 | `baseline-nullable-pragma-inventory.2026-08-10T14-35.md` |
| 8 | `baseline-powershell-toolchain.2026-08-10T15-40.md` |

One under `evidence/regression-testing/`:

| # | Filename |
|---|---|
| 9 | `negative-path-proof-dry-run.2026-08-10T15-20.md` |

## Governance authorization — verbatim quotation

From `docs/features/epics/build-ci-coverage-gate-fidelity/epic.md` § "Execution Authorization
Required":

> Two child features edit documents that the `policy-compliance-order` skill places under a hard
> constraint: "Do NOT modify policy documents under `.claude/rules/` or `.github/instructions/`."
>
> - `csharp-toolchain-gate-fidelity-512` must edit `CLAUDE.md` and `.claude/rules/csharp.md`.
> - `coverage-threshold-policy-reconciliation-494` must edit `CLAUDE.md`,
>   `.claude/rules/general-unit-test.md`, and `.claude/rules/quality-tiers.md`.
>
> These edits are the substance of issues 494, 509 and 522 — the defect *is* that the governance
> documents are wrong. Planning does not perform them; preparation produces specifications and
> atomic plans that propose them. Executing this epic constitutes the authorization to apply them,
> and the edits must remain scoped to exactly the sites the issues enumerate. No child may edit a
> governance document for any purpose outside its own issue's acceptance criteria, and in
> particular no child may relax a policy in order to make a gate pass.

**Recorded:** the epic names exactly **two** edit targets for this child — `CLAUDE.md` and
`.claude/rules/csharp.md`. No third path is named for `csharp-toolchain-gate-fidelity-512`.

## Why `.claude/skills/csharp-qa-gate/SKILL.md` requires no suspension

The `policy-compliance-order` hard constraint reads, verbatim
(`.claude/skills/policy-compliance-order/SKILL.md`):

> - Do NOT modify policy documents under `.claude/rules/` or `.github/instructions/`.

Its scope is the two named directories. `.claude/skills/` is **not** named. Editing
`.claude/skills/csharp-qa-gate/SKILL.md` therefore does not engage the hard constraint and needs no
suspension from the epic; it is authorized by this feature's own acceptance criteria (AC1, AC2, AC5,
AC6 and AC13, via replacement-table rows 16-19 in `spec.md`), and `issue.md` §
"Governance-Document Authorization" names it explicitly as a file this feature must edit.

Conversely, `.github/instructions/` **is** named in the constraint and is **not** named in the
epic's authorization, so the mirror tree remains under the unsuspended constraint and is excluded
(SD1). The correct response to an unsuspended hard constraint is to stop, not to widen scope.

## Protected and excluded files

Protected — must be byte-identical to `MERGE_BASE` (AC9):

- `CLAUDE.md` § UT2 ("Coverage and Scenarios")
- `.claude/rules/general-unit-test.md`
- `.claude/rules/quality-tiers.md`

Excluded systems and files (SD1 and SD4) — must not appear in `git diff <MERGE_BASE> --name-only`:

- `AGENTS.md`
- `.agents/**`
- `.github/instructions/**`
- `.github/agents/**`
- `.codex/**`
- `.github/workflows/ci.yml`

## Additional scope limitation recorded

No `*.cs`, `*.csproj`, `*.props` or `*.targets` file is modified by this feature. The nullable-debt
burn-down (issue #492) is explicitly out of scope; [P0-T13] measures the debt and does not fix it.

## Output Summary

All four requirement inputs and all nine pre-existing evidence artifacts were read. The epic's
authorization sentence is quoted verbatim above and names exactly two edit targets for this child.
`.claude/skills/csharp-qa-gate/SKILL.md` is confirmed to sit outside the `policy-compliance-order`
hard constraint's stated scope and therefore needs no suspension. The protected and excluded file
lists are recorded and are enforced by [P3-T6], [P5-T13] and [P6-T9].
