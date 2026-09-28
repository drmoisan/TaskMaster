# pr-context-harvests-closing-keywords-from-prose (Issue #886)

- Date captured: 2026-09-13
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/pr-context-harvests-closing-keywords-from-prose/ (Issue #886)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #886
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/886
- Last Updated: 2026-09-13
## Summary

`mcp__drm-copilot__collect_pr_context` produces an "author-asserted autoclose issues" list by scraping `#<number>` tokens out of prose inside a feature's own documents, without distinguishing a citation/reference from a closure intent. On item 871 this produced seven unrelated issue numbers plus one nonsense token, none of which item 871's pull request closes. It did not fire only because GitHub CLI validation was reported unavailable and the pr-author skill's `None` fallback applied — a fallback, not a control.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable; this is the `drm-copilot` MCP `collect_pr_context` tool
- Command/flags used: `mcp__drm-copilot__collect_pr_context` invoked for item 871 (`bug/qfcqueue-enqueue-path-lacks-injectable-seams-871`), base `main`
- Data source or fixture: `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/artifacts/pr_context.summary.txt`

## Steps to Reproduce

1. In the item-871 worktree, read `artifacts/pr_context.summary.txt`.
2. Locate the `===== Close candidates =====` section, `Auto-close issues (author asserted):` subsection.
3. Observe the list: `#620, #678, #724, #727, #731, #781, #784, #871, #ISO-8601`.
4. Grep item 871's own `spec.md`, `issue.md`, and research document for each of those numbers (excluding #871 itself): every one of #620, #678, #724, #727, #731, #781, #784 appears only as a prose citation — e.g. `spec.md:771`: "the move-monitor per-owner invariant issues #731 and #620; the dispatcher synchronization-context hazard issues #781 and #784, which this item neither introduces nor mitigates" — never as a stated closing target.
5. Observe the `#ISO-8601` entry has no issue-number form at all; it is a scraping artifact, most likely from a timestamp-shaped string in the source text being matched by the same `#`-prefixed pattern.
6. Observe the `===== GitHub CLI status =====` section reports "GitHub CLI unavailable: GitHub CLI (gh) is not installed." — while `gh` was independently confirmed working in the same environment during this filing (`gh issue view`, `gh pr view` both succeeded against `drmoisan/TaskMaster`).

## Expected Behavior

The context-collection tool should distinguish a genuine closing-intent statement (e.g. "Closes #871", "Fixes #871") from an ordinary citation or cross-reference appearing in prose (e.g. "issue #731 finding 1", "the dispatcher synchronization-context hazard issues #781 and #784"). It should not include cited-but-not-closed issue numbers in an "author-asserted autoclose" list, and it should not emit a malformed non-numeric token like `#ISO-8601`. It should also correctly detect an installed, working `gh` CLI rather than reporting it unavailable.

## Actual Behavior

The tool's "author-asserted autoclose issues" list for item 871 contained: `#620, #678, #724, #727, #731, #781, #784, #871, #ISO-8601`. Only `#871` is the issue this PR addresses. The other seven are unrelated issues referenced only as citations inside item 871's own spec/issue/research documents, and `#ISO-8601` is not a valid issue reference at all. Had these been emitted as actual GitHub closing keywords in the PR body and had GitHub validation been available, merging the pull request would have closed seven unrelated issues. The `pr-author` skill's body for PR #883 states explicitly: "GitHub CLI validation was unavailable when this body was generated, so no closing keyword is emitted... The context bundle's author-asserted list also harvested several unrelated issue numbers from prose inside the feature documents; emitting closing keywords from that list would have closed issues this PR does not address." Additionally, `artifacts/pr_context.summary.txt` reports "GitHub CLI unavailable: GitHub CLI (gh) is not installed," which is false in this environment: `gh issue view 882` and `gh pr view 883` both succeeded during this same filing session. A further defect, reported by the delegating coordinator and not independently reproduced in this filing, is that `collect_pr_context` can produce a vacuous zero-diff context when invoked against a workspace root whose checked-out branch is not the target branch; this is recorded here for completeness and should be verified independently before being treated as confirmed.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `artifacts/pr_context.summary.txt` (item 871), lines 38-47:
  ```
  Auto-close issues (author asserted):
  - #620
  - #678
  - #724
  - #727
  - #731
  - #781
  - #784
  - #871
  - #ISO-8601
  ```
  and line 10: `GitHub CLI unavailable: GitHub CLI (gh) is not installed. Install from https://cli.github.com/.`

## Impact / Severity

- [x] High
- [ ] Blocker
- [ ] Medium
- [ ] Low

High: the mechanism can cause a merge to close unrelated, unaddressed issues with no warning to the author. It did not fire on item 871 only because of an unrelated tool-availability fallback, which is not a designed safety control.

## Suspected Cause / Notes

The harvester most likely applies a `#\d+`-style (or similarly permissive) regular expression across the full text of every additional context file, including feature `spec.md`/`issue.md`/research documents, without regard to sentence structure or closing-keyword phrasing (`closes`, `fixes`, `resolves`). The `#ISO-8601` token suggests the pattern is not even anchored to digits, since it matched a non-numeric string. The false "GitHub CLI unavailable" report and the reported vacuous zero-diff context (workspace root on the wrong branch) both point at the same tool's environment-detection and ref-resolution logic being unreliable, independent of the closing-keyword defect.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: a closing-keyword extractor that requires an adjacent closing verb (`closes`, `fixes`, `resolves`) immediately before the `#<number>` token, scoped to the PR body/commit messages the tool is actually meant to scan rather than arbitrary feature-document prose; reject non-numeric tokens after `#`.
- [x] Integration scenario to retest: re-run `collect_pr_context` against item 871's actual worktree/branch and confirm the "author asserted" list is empty or contains only `#871`; separately verify `gh` detection against an environment where `gh` is confirmed installed and authenticated.
- [x] Manual verification notes: state plainly, wherever this tool's output is consumed (e.g. by `pr-author`), that safety currently depends on the `gh`-unavailable fallback rather than on any closing-intent validation, so the fallback must not be "fixed away" without first fixing the harvester.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
