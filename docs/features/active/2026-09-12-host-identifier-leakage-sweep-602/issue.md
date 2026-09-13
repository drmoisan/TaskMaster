# Repository-wide host-identifier leakage: absolute user-profile paths, account and host names in tracked files

- Issue: #602
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/602
- Promotion Type: bug
- Work Mode: full-bug
- Short Name: host-identifier-leakage-sweep
- Severity: Medium
- Parallel Run: bugs-2026-09-11
- Status: Active (preparation in progress, execution deferred to the parallel orchestrator)

## Summary

Tracked files across this repository embed developer-machine identifiers: absolute user-profile paths,
the bare account name, and the bare host name. The standing convention is that no file may embed an
absolute host path or a host identifier; portable placeholders are used instead.

Feature `winformspumphost-suite-determinism-511` sanitized its own artifacts and recorded the
convention, but deliberately scoped its change to its own feature folder so as not to break its
three-file scope-lock acceptance criterion. This issue tracks the remainder.

The bulk of the account-name occurrences come from one mechanical source: the MSTest console runner
names its test-result files with an account-and-host prefix by default, and evidence records across
many feature folders cite those filenames verbatim.

## Maintainer Scope Narrowing (2026-09-11)

The issue as filed carries five acceptance criteria. The maintainer narrowed the scope of this
delivery item as follows.

In scope for this item:

- AC1 — no tracked file contains an absolute user-profile path.
- AC2 — no tracked file contains the bare account name or the bare host name.
- AC3, editor-settings half only — the workspace editor settings file uses portable placeholders or
  environment references.

Out of scope for this item:

- AC3, agent-governance-settings half. That settings file is published into this repository by a
  push-down from the upstream governance repository with zero templating, so an edit made here is
  reverted on the next sync. It is tracked upstream and must be corrected there. This item does not
  edit it and does not plan a task touching it.
- AC4 — the explicit results-directory and log-file-name half. A sibling item in the same parallel run
  delivers it. This item is expected to land after that one.
- AC5 — the upstream governance-repository portion, for the same reason as the AC3 exclusion above.

## Why it matters

- A repository-relative artifact that hard-codes one developer's profile path is not reproducible by
  anyone else, and silently documents a machine rather than a procedure.
- Account and host names are gratuitous identifying information in a repository that may be shared or
  made public.
- Because the default test-result naming reintroduces the prefix on every run, text-only cleanup
  regresses. The durable fix, delivered by the sibling item, controls the results directory and the
  log file name.

## Authoring Rules (non-negotiable)

1. Angle-bracket placeholder tokens are acceptable inside Markdown prose. An angle-bracket token must
   never be written into an XML attribute value; that is what corrupted feature 488's test-result XML.
   When a tracked XML file carries a host identifier, delete the file rather than redacting it in
   place.
2. Per the maintainer decision on issue 671 of 2026-09-11, the project has moved to projection-only
   evidence. Deleting tracked raw test-result and coverage XML files is preferred over redacting them,
   and is the intended outcome for that class rather than a fallback.

## Verified Environment Constraints

Re-verified in this worktree on 2026-09-12 at merge commit 2405a829d.

- The Bash allowlist grants only git, gh, pwsh, poetry run, and three library scripts. Every chained
  segment of a command line is checked independently, so a command that changes directory and then
  runs another program is denied.
- This repository has no Python toolchain. There is no developer-tools Python script tree and no
  extensions tree.
- Git Bash rewrites a leading-slash argument into a Windows path before git sees it, so a search for a
  literal beginning with a forward slash returns zero matches and a non-zero exit against a file that
  contains it. Count-based acceptance conditions must use a bracketed character class for a leading
  slash instead.
- Verified this run by direct probe: under agent worktree isolation the Bash tool refuses both a
  command carrying a shell variable expansion and any invocation of the PowerShell executable, so the
  execution child for this item must run non-isolated.
- Staging scope: an unscoped pathspec sweeps a sibling item's queued promotion file onto this branch.
  Every staging and porcelain span must exclude the potential-features directory.

## Re-derived Measurements (2026-09-12, merge commit 2405a829d)

All figures were re-derived in this worktree with tracked-only search, case-insensitive, excluding the
staged promotion directory. They supersede the figures measured on main on 2026-09-11.

| Population | Re-derived |
| --- | --- |
| Tracked files, excluding the staged promotion directory | 15208 |
| Absolute user-profile path, union of all four spellings | 1094 |
| Bare account name, unguarded | 1195 |
| Bare account name with the negative preceding-character guard | 1187 |
| Package-coordinate form | 13 |
| Bare host name, full token | 183 |
| Host-name stem with the trailing digit run removed | 186 |
| Union of guarded account name and host name | 1188 |
| Agent-memory documents carrying an identifier | 9 |
| Tracked filenames carrying the account name | 2 |
| Tracked filenames carrying the host name | 2 |
| Tracked test-result files | 332 |
| Tracked XML under a feature-folder evidence path | 305 |
| Tracked XML outside the features tree | 4 |
| Stray raw test output at the repository root | 1 |

## Acceptance Criteria

The authoritative acceptance-criteria source for this full-bug item is `spec.md` in this folder. The
criteria below are the maintainer-narrowed issue-level statements that `spec.md` refines.

- [ ] AC1: No tracked file contains an absolute user-profile path.
- [ ] AC2: No tracked file contains the bare account name or the bare host name.
- [ ] AC3 (editor-settings half): the workspace editor settings file uses portable placeholders or
      environment references.
- [ ] AC-SCOPE: the delivered diff touches only paths declared in the spec Write Set, and touches
      neither of the two out-of-scope files named in the Maintainer Scope Narrowing section.
- [ ] AC-XML: no angle-bracket placeholder token is introduced into any XML attribute value anywhere
      in the diff.
- [ ] AC-SELF-CONSISTENT: the repository remains self-consistent after the sweep whether or not the
      sibling AC4 item has already landed.

## Links

- GitHub issue: https://github.com/drmoisan/TaskMaster/issues/602
- Related feature folder: `docs/features/active/2026-08-21-winformspumphost-suite-determinism-511`
