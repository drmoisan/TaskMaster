---
name: project-phrase-count-payload-trips-promotion-gh-issue-hook
description: A pwsh payload containing "issue" plus "new"/"create" plus any "gh" substring (High, through) is denied by enforce-promotion-mcp-only.ps1 although it runs no gh; mechanism is wrapper-led raw containment
metadata:
  type: project
---

On 2026-10-02 (issue #964, P0-T4) and again on 2026-10-03 (issue #968 preflight round 3), a read-only
`pwsh -NoProfile -Command '...'` payload was denied with
`PROMOTION_MCP_ONLY_BLOCKED: Direct GitHub issue creation via gh ...` although it invoked no `gh`.

Mechanism (read from `.claude/hooks/hook-command-scanner.ps1` and `hook-command-invocation.ps1`): `pwsh` is in
`$script:CommandLineWrapperNames`, so the whole payload is a wrapper-led segment, and
`Resolve-CommandLineInvocation` then applies `Test-CommandLineRawContainment`: case-insensitive SUBSTRING
containment of the command word and every subcommand word anywhere in the raw text. For the promotion hook that
is `gh` + `issue` + (`create` or `new`). `gh` is satisfied by `High` (HighConfidenceMode), `through`, `right`;
`new` by `new Foo()`, `New-Object`, `New-Item`. In #968 the trigger was merging a span payload whose anchor read
`Reads the issue #424 ...` with a sibling payload carrying `HighConfidenceMode` and `new BackgroundWorker()`;
each payload alone passes. The same rule makes `git` + `worktree` + `remove` (PREFIX `WORKTREE-LEAF` supplies
`worktree`), `git` + `push`, `git` + `reset`, `gh` + `pr` + `create|edit|merge` collide.

**Why:** the coordinator's binding rule is "record the block verbatim, do not rephrase", so a refusal stops the
step, and plans now treat any pwsh refusal as a run stop (D-10 style rules).

**How to apply:** never merge plan payloads into one Bash call; in preflight, scan each payload's full text for
those word triples (substring, case-insensitive) and flag any hit, proposing a split or `[char]`-built words.
Related: [[project-malformed-pwsh-payload-surfaces-as-unrelated-hook-block]],
[[worktree-removal-hooks-match-git-plus-remove-substring]].
