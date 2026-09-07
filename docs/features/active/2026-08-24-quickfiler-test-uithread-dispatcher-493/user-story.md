# quickfiler-test-uithread-dispatcher (#493) — user story placeholder

> **This file carries no acceptance criteria and is not an acceptance-criteria source.**
>
> Work Mode for this feature is `full-bug`. Per
> `.claude/skills/acceptance-criteria-tracking/SKILL.md` § AC Source Resolution, the authoritative
> acceptance-criteria source for a `full-bug` feature is `spec.md` **only**, and `user-story.md` is
> intentionally absent by default (see also `.claude/skills/atomic-plan-contract/SKILL.md`
> § Mode-Specific Mandatory Plan Gates: "`full-bug` plans MUST enforce spec-driven expectations
> (`spec.md` required, `user-story.md` optional/absent by default)").
>
> This placeholder exists solely because the `PreToolUse` gate
> `.claude/hooks/enforce-feature-folder-order.ps1` requires `issue.md`, `spec.md`, **and**
> `user-story.md` to exist before any write to `plan.md`, without consulting the
> `- Work Mode:` marker in `issue.md`. That unconditional requirement contradicts the
> `full-bug` mode rule above. The gate is enforced by a `.claude/` runtime file that this
> repository receives by push-down from `drm-copilot` and does not own, so the contradiction is
> recorded here rather than worked around by editing the hook.
>
> **Do not add acceptance criteria to this file.** Every acceptance criterion for #493 lives in
> `docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md` § Acceptance Criteria
> (AC-1 through AC-10) and is checked off there.

## Narrative

As a developer running the `QuickFiler.Test` suite, I want every mutation of the process-wide
`UtilitiesCS.Threading.UiThread._dispatcher` static made by this test assembly's owned files to be
atomic and to have a restore path, so that tests remain independent and order-insensitive under the
class-level parallelization the repository runsettings force, and so that the #230 deadlock — one
class's write reverting the static to a parked, never-pumped dispatcher while another class awaits
a dispatcher operation — cannot recur.

## Requirements source

- `docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md` (authoritative)
- `docs/features/active/quickfiler-test-uithread-dispatcher-493/issue.md` (constraints, scope lock)
- `docs/features/potential/promoted/2026-08-07-uithread-dispatcher-static-swap-no-restore.md`
