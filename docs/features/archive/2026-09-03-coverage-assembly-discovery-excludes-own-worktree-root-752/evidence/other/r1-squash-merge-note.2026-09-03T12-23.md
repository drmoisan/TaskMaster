# Merge-Mode Record — Remediation R-1, Issue #752

- Timestamp: 2026-09-03T23-45
- Task: `[P1-T4]`

Command: N/A (documentation record)

EXIT_CODE: 0

Output Summary:

**Requirement.** The pull request for branch
`bug/coverage-assembly-discovery-excludes-own-worktree-root-752` must be merged with a **squash
merge**. A merge commit or a rebase merge does not satisfy this requirement.

**Reason.** The sanitising commits this remediation makes remove the identifier from the branch tip,
but they do not remove it from the branch's history. The pre-sanitisation blob remains reachable
through an earlier commit on this branch, so a merge that preserves the branch's individual commits
would publish that blob into the history of `main`, where it would remain retrievable by any reader
of the repository. Squashing collapses the branch into a single commit whose tree is the sanitised
tip, which is what keeps the identifier out of `main`'s history. This restates required-remediation
step 4 of `remediation-inputs.2026-09-03T12-23.md`.

**Ownership.** Performing the merge is the orchestrator's or the maintainer's action. It is not an
executable step of this plan, and no task in this plan performs or attempts it. This record exists so
that the requirement is carried in a committed artifact rather than only in an execution summary,
and so that it is visible to whoever configures the merge.

This record quotes no removed value. The identifier it refers to is described by class only: an
absolute host path carrying an account-name token, a worktree-parent directory-name token, and a
Windows user-profile path prefix.
