---
name: pr-author
description: Write a GitHub-ready pull request body file plus a SHA-256 provenance receipt from the canonical PR-context bundle, with strict verification and auto-close rules. Use before any gh pr create / gh pr edit --body-file.
allowed-tools:
  - Read
  - Write
  - "Bash(git log *)"
  - "Bash(pwsh *)"
---

# PR Author Skill

Produce a GitHub-ready Pull Request body **as a file** from the standard PR context files, and emit a sibling provenance receipt so the `enforce-pr-author-skill` PreToolUse hook will allow the subsequent `gh pr create`/`gh pr edit`.

The `enforce-pr-author-skill.ps1` hook blocks any `gh pr create`/`gh pr edit --body-file` whose body is not a canonical, hash-verified, fresh pr-author artifact. This skill produces exactly that artifact pair.

## Inputs

- `artifacts/pr_context.summary.txt` — PR context summary (primary)
- `artifacts/pr_context.appendix.txt` — PR context appendix (full baseline diff, commits, changed files)
- The target PR number `<N>` (the issue or PR number this body is for)
- Optional user directives

If the context artifacts are missing or stale, refresh them first per `pr-context-artifacts` and `pr-base-branch-merge-base` (resolve the base branch, then run `mcp__drm-copilot__collect_pr_context`). The receipt's `created_at` MUST be strictly newer than the context summary's last-write time, so always (re)generate the context bundle before authoring the body.

Only reference files that are listed under "Additional context files" in the context bundle. Do not cite or summarize files outside that enumeration.

## Required Output Artifacts (canonical, hook-enforced)

1. `artifacts/pr_body_<N>.md` — the PR body in GitHub-flavored Markdown. The path MUST match `artifacts/pr_body_<N>.md` exactly (hook Case D).
2. `artifacts/pr_body_<N>.receipt.json` — a sibling provenance receipt (hook Cases E/G/F/H) with exactly:
   ```json
   {
     "number": <N>,
     "sha256": "<lowercase hex SHA-256 of pr_body_<N>.md>",
     "created_at": "<UTC ISO-8601 timestamp, e.g. 2026-06-08T14:30:00.000Z>"
   }
   ```
   - `number` must equal `<N>` from the body filename (Case G).
   - `sha256` must equal the SHA-256 of the body file's bytes, lowercase hex (Case F).
   - `created_at` must be strictly newer than the last-write time of `artifacts/pr_context.summary.txt` (Case H).

### Authoring sequence (do these in order)

1. Write the body to `artifacts/pr_body_<N>.md` using the structure below.
2. Compute the hash and write the receipt deterministically with PowerShell (do not hand-compute the hash):
   ```powershell
   $n = <N>
   $body = "artifacts/pr_body_$n.md"
   $hash = (Get-FileHash -LiteralPath $body -Algorithm SHA256).Hash.ToLowerInvariant()
   $receipt = [ordered]@{
     number     = $n
     sha256     = $hash
     created_at = (Get-Date).ToUniversalTime().ToString("o")
   }
   $receipt | ConvertTo-Json | Set-Content -LiteralPath "artifacts/pr_body_$n.receipt.json" -Encoding utf8
   ```
3. Do not edit `artifacts/pr_body_<N>.md` after writing the receipt; any edit invalidates the hash (Case F). If the body must change, rewrite the body then re-run step 2.
4. Create/edit the PR with the canonical body: `gh pr create --base <base> --head <head> --title "<title>" --body-file artifacts/pr_body_<N>.md` (or `gh pr edit <N> --body-file artifacts/pr_body_<N>.md`).

## Body Content Requirements

1. The file contains only the PR body in GitHub-flavored Markdown.
2. Use clear headings, consistent structure, and concise bullets.
3. Do not invent tests or results; if not in context, state "Not verified in this PR."
4. If scope is large, include a Review Guide with suggested review order.

## PR Body Structure

Use these sections in this order:

1. **Summary** — 3–7 bullets, most important first; lead with the primary product/feature/architecture change
2. **Why** — motivation, constraints, root cause; rely on embedded feature-doc excerpts and PR Intent fields only
3. **What Changed** — grouped by theme (core feature, tooling/CI, tests, docs/templates)
4. **Architecture / How It Fits Together** — short wiring description with components, entry points, control flow
5. **Verification** — "Completed" (from context) and "Recommended" (commands to run)
6. **Backward Compatibility / Migration Notes** — breaking changes, removals, renamed paths
7. **Risks and Mitigations** — realistic risks with mitigations and rollback notes
8. **Review Guide** — suggested order, noisy mechanical moves, large diffs
9. **Follow-ups** — known TODOs, deferred cleanup, next PRs
10. **GitHub Auto-close** — `- Closes #NNN` only from verified autoclose lists; do not invent issue numbers

(The suggested title is passed to `gh ... --title`, not embedded in the body file.)

## Issue/PR Reference Rules

- Only mention an issue/PR number if it appears verbatim in the provided context.
- Do not treat PR numbers as issues.
- Auto-close bullets must use exactly `- Closes #NNN` format, sourced only from "Issues to autoclose" or "Author-asserted autoclose issues."
- If GitHub validation is unavailable/unverified, do not emit `Closes`; use the fallback `None` bullet.
