---
name: promotion-hook-matches-commit-message-text
description: enforce-promotion-mcp-only.ps1 scans the whole Bash command string, so the GitHub-CLI issue-creation phrase inside a git commit message body is denied as if you were running it
metadata:
  type: project
---

`.claude/hooks/enforce-promotion-mcp-only.ps1` matches its denial regex against the ENTIRE Bash
command string, not against the executable being invoked. A `git commit -m "..."` whose message
body merely *describes* the GitHub-CLI issue-creation phrase is denied with
`PROMOTION_MCP_ONLY_BLOCKED`, even though the command creates no issue and calls no `gh`.

**Why:** the hook is a substring/regex guard over the command text. It cannot distinguish a command
that performs the banned action from a commit message, a heredoc, or a `--body-file` argument that
quotes it. Observed 2026-08-25 while committing a plan fix whose whole subject was that a plan task
had been calling that command and must stop.

**How to apply:** when a commit or PR body needs to discuss that command, paraphrase it — write
"invoked the GitHub CLI to open follow-up issues directly" rather than the literal verb phrase. The
same caution applies to any Bash-tool command whose text quotes a banned pattern for documentation
purposes. This is the mirror image of [[closing-keyword-fires-inside-negation]]: both hooks read
text without reading intent, so a *negated* or *descriptive* mention fires the guard exactly as a
real one would.

Related: [[potential-to-issue-creates-github-issue]] records the substantive rule the hook enforces
(the MCP promotion tool opens the issue itself, so the CLI call was never needed).
