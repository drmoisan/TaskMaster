---
name: maintainer-waiver-recorded-only-in-gitignored-state
description: Maintainer coverage waivers get written to artifacts/orchestration/orchestrator-state.json, which is gitignored in TaskMaster, so they never reach the PR; always check git check-ignore and require the waiver be transcribed into a committed file.
metadata:
  type: project
---

When the orchestrator routes a coverage-threshold decision to the maintainer, the authorization lands
in `artifacts/orchestration/orchestrator-state.json` under a `human_interaction.maintainer_waivers[]`
block. **That file is gitignored in this repo** (`.gitignore:57:artifacts/`), so the waiver exists
nowhere in the committed record.

**Why:** On issue #418 cycle 4 the maintainer waived the file-level coverage floor for
`SVGControl/SvgAssemblyResolver.cs`. The waiver entry was well-formed — `authorized_by`,
`authorization_text`, `scope`, `basis`, and an `orchestrator_disclosure_at_time_of_request` recording
the self-inflicted framing. But `git ls-files --error-unmatch` on the state file errors out. The waiver
would not appear in the PR, would not survive a fresh clone, and the next coverage audit would
re-derive the same finding with no trace that it was ever adjudicated. This inverts the property
`CLAUDE.md` UT2 designs for, which specifies exemptions be applied via `[ExcludeFromCodeCoverage]`
attributes "in source code (**reviewable in PRs**)" or `coverage.config` excludes — both deliberately
reviewer-visible.

**How to apply:**
- Whenever a review cycle closes a finding on the strength of a maintainer waiver, run
  `git check-ignore -v <path>` on whatever file holds it and state the result in the policy audit.
- Require transcription into a committed file — for `minor-audit` the natural home is a subsection
  under the relevant AC in `issue.md`, recording authorizer, date, scope sentence, and basis.
- **Do not recommend converting the waiver into an `[ExcludeFromCodeCoverage]` attribute or a
  `coverage.config` exclude.** A threshold exception keeps the file in the denominator and so does not
  breach `.claude/rules/general-unit-test.md`'s no-exclusion rule; an exclusion would remove the lines
  from the repo-wide figure and *would* breach it. Verify no such attribute/config change is in the
  diff before calling the waiver legitimate.
- Also check the waived residual has a follow-up owner. On #418 the sibling G-1 residual was owned by
  `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md` but the waived file was named
  nowhere in it.
- `scripts/dev_tools/validate_orchestrator_state.py` does **not** exist in TaskMaster, and
  `.claude/rules/orchestrator-state.md` defines its `exception`-requires-`runbook_path` invariant over
  `human_interaction.requirements[]` only — `maintainer_waivers[]` is an undocumented extension that
  nothing validates. Expect a placeholder `runbook_path` there; it is harmless if disclosed.

Related: [[project_orchestrator-state-human-interaction-verifies-scope-change-ratification]] for
cross-checking a ratification claim against the same (gitignored) file, and
[[project_taskmaster-validator-memories-are-cross-repo]] for why the validator script is absent.
