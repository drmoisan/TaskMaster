# drm-copilot-upstream-template-and-payload-defects (Issue #932)

- Date captured: 2026-09-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/drm-copilot-upstream-template-and-payload-defects/ (Issue #932)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #932
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/932
- Last Updated: 2026-09-28
## Summary
**Upstream tracker.** Every defect below lives in push-down-owned files or MCP tools from the `drm-copilot` repository. A fix made in TaskMaster would be reverted by the next push-down, so each must be fixed in drm-copilot and arrive here through a push-down. This issue follows the precedent of #691 and closes once all items arrive and are verified here.

1. **#887: `potential_to_issue` silently drops unmatched sections.**
   - `extensions/drm-copilot/src/lib/potential-to-issue/content.ts:39-47`: `BUG_SECTION_HEADINGS` lists only seven headings.
   - `buildBugBody` (lines 212-224) emits only those seven, so `Suspected Cause / Notes`, `Proposed Fix / Validation Ideas` and `Next Step` are dropped with no signal in the receipt.
   - The Python mirror has the same gap: `scripts/dev_tools/potential_to_issue_content.py:15,94`.
   - The last change to that module was 279d4f7c on 2026-08-25, so the defect is live in v1.1.12.
2. **#899: acceptance-criteria authoring conventions are inconsistent.**
   - The promotion scaffold injects checkbox lines outside the acceptance-criteria section: 52 of 86 active specs are affected.
   - The feature template ships `## Definition of Done` instead of `## Acceptance Criteria`: 5 specs are affected.
   - Three label conventions are live at once: `(#NNN)`, numbered, and `AC<n>`.
   - `.claude/skills/acceptance-criteria-tracking/SKILL.md:43-50` permits several headings.
   - The only authoring-time gate is `.claude/hooks/validate-prd-feature-output.ps1:66`.
3. **#885: the "Write Set" acceptance criterion cannot be satisfied.**
   - `.claude/agent-memory/**` is tracked and written by every executing agent.
   - No spec's Write Set can enumerate those files in advance, so the criterion fails on every agent-executed delivery.
   - The spec-authoring and planning prompts (`resolve_atomic_plan_prompt`, the prd-feature template) need a standing `.claude/agent-memory/**` carve-out.
4. **#602 `.claude` portion: host-identifier leakage in push-down-owned files.**
   - About 8 `.claude/**` files contain the developer account name or an absolute user-profile path, including `.claude/settings.json:75` and several `.claude/agent-memory/**` files and `.claude/skills/cleanup-merged-worktrees/SKILL.md`.
   - Replace them with `<repo-root>`, `<user-profile>` or environment references upstream.
5. **#563 residual: coverage floors differ from the maintainer decision.**
   - `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md` and `.claude/hooks/validate-feature-review-coverage.ps1` state 85% line / 75% branch.
   - The maintainer decision recorded on #563 (2026-09-11) is 80% line / 75% branch for C#, and 80% line for PowerShell.
   - Upstream should either make the floor configurable per consuming repository or accept that the review hook disagrees with TaskMaster's CI gate.
6. **#727 sub-finding 5 residual: evidence timestamps.**
   - Evidence artifacts carry `Timestamp:` fields that disagree with their own build banners and commit dates, so inter-gate ordering cannot be established from them.
   - The owning guidance is the push-down-owned `evidence-and-timestamp-conventions` skill.

## Environment
- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable (drm-copilot TypeScript MCP server and its Python mirror, and push-down payload files)
- Command/flags used: static inspection of drm-copilot `main` (v1.1.12 tag 2dce111e and later) and TaskMaster `main` at `177b6d78e` (harness 1.1.12)
- Data source or fixture: files cited above

## Steps to Reproduce
See each item. #887 reproduces on any promotion of a fully filled potential-bug entry: diff the issue body against the source.

## Expected Behavior
Each item is corrected upstream and pushed down. After the push-down, each is verified in TaskMaster:
- a re-promotion carries every source section
- a newly scaffolded spec has checkboxes only inside `## Acceptance Criteria`
- a Write Set criterion passes with agent-memory writes present
- `.claude/**` carries no host identifier
- the review hook's coverage floors match the recorded decision

## Actual Behavior
All six are present on TaskMaster `main` at `177b6d78e` with harness 1.1.12.

## Logs / Screenshots
- [x] Attached minimal logs or snippet
- Snippet: see the verification comments on the consolidated source issues.

## Impact / Severity
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes
Each is a template, tool, or payload defect in drm-copilot. None can be fixed durably in this repository, because push-down overwrites the payload paths.

## Proposed Fix / Validation Ideas
- [ ] Run one drm-copilot change (or one drm-copilot parallel run) covering items 1 to 6.
- [ ] Publish the MCP server. TaskMaster's `.mcp.json` runs `npx -y` against the published package, so the published version is what TaskMaster sessions run.
- [ ] Push down to TaskMaster.
- [ ] Verify each expected behavior in TaskMaster.

## Next Step
- [x] Promote to GitHub issue (bug-report template)
- [ ] Execute in drm-copilot

Consolidates: #885, #887, #899, the `.claude` portion of #602, the upstream residual of #563, and the timestamp portion of #727 sub-finding 5.