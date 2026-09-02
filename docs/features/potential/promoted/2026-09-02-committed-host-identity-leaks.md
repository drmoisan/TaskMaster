# committed-host-identity-leaks (Issue #728)

- Date captured: 2026-09-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/committed-host-identity-leaks/ (Issue #728)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #728
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/728
- Last Updated: 2026-09-02
## Summary

Two consolidated findings: a committed project file and five committed agent-memory files leak a real developer account name and/or employer name into version control. Consolidated into one issue rather than two, since both are the same class of defect (plain-text identity leakage in a tracked file, not a runtime/log leak) and fixing them is a single coordinated sweep, not two independent efforts.

## Environment

- OS/version: Windows 11 Pro (repo default)
- Python version: n/a — repo-wide, spans a `.csproj` and Markdown agent-memory files
- Command/flags used: n/a — findings are from direct `git grep`/`git show` inspection
- Data source or fixture: n/a

## Steps to Reproduce

Not applicable — both findings are static content inspections. See "Actual Behavior."

## Expected Behavior

No committed, tracked file should contain a real account name, machine name, or employer name in plain text. This is already an established repo-wide convention (see `.claude/agent-memory/_shared_no_absolute_host_paths.md`) that these two findings violate.

## Actual Behavior

**1. `TaskMaster/TaskMaster.csproj:37` commits a personal publish path.** `<PublishUrl>C:\Users\DanMoisan\OneDrive - The Real Good Food Company\TM\</PublishUrl>` — both the developer's Windows account name and their employer's organization name (via a OneDrive commercial folder name) are committed to version control, visible in every clone, the public GitHub UI, and the full history. This is the same class of leak already tracked by issue #602 (scoped to runtime exception messages), but worse in durability: a log message exists on one machine, while a committed project file republishes the identifiers to every reader indefinitely, and rewriting it out of history is disruptive in a way fixing a log message is not. *(Source: #628.)*

**2. Five `.claude/agent-memory/**` files contain the real account name and/or machine name in plain text**, confirmed by direct `git grep` against `origin/main`:
   - `.claude/agent-memory/epic-orchestrator/feedback_measure_whole_volume_before_blaming_worktrees.md`
   - `.claude/agent-memory/feature-review/project_464-review-residuals.md`
   - `.claude/agent-memory/feature-review/project_488-review-residuals.md`
   - `.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md`
   - `.claude/agent-memory/orchestrator/collect-pr-context-lands-in-main-checkout.md`

   These files should follow the same no-absolute-host-paths convention already documented and applied elsewhere in this same memory tree. *(Source: #685.)*

Note: issue #671 (TRX evidence-hygiene sweep producing malformed XML as a side effect of host-token redaction) is a related but mechanically distinct defect — a *tool* that redacts identity information incorrectly, rather than *un-redacted* identity information sitting in a file. Deliberately left standalone rather than folded in here, since #727 (filed earlier this session) already cross-references #671 by number as the umbrella tracker for that specific sweep-tool defect.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: n/a — see file/line citations above, each confirmed directly against `origin/main` on 2026-09-02.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: no functional defect, but a real, durable, publicly-visible privacy/identity leak in a repository history that (per #628's own text) is disruptive to remove retroactively — the cost of delay compounds, unlike a log message.

## Suspected Cause / Notes

Each finding traces to a specific issue, cited inline above. Both are plain committed-content leaks (not a tool malfunction), so the fix is a straightforward find-and-redact plus a guard against recurrence — consistent with the existing `_shared_no_absolute_host_paths.md` convention this repo already documents and partially enforces elsewhere.

## Proposed Fix / Validation Ideas

- [ ] Replace `TaskMaster.csproj`'s `<PublishUrl>` with a placeholder/relative value or remove the element if the publish profile isn't used
- [ ] Redact the account/machine name from the five named agent-memory files, replacing with the established `<user-profile>`/`<host>` placeholder convention
- [ ] Consider a repo-wide grep-based pre-commit or CI check for the known account/machine name tokens across all tracked files, to prevent recurrence (this finding, #602, and #671 together suggest the leak class recurs rather than being a one-off)
- [ ] Evaluate whether `TaskMaster.csproj`'s history needs rewriting or whether a forward-only fix (current tip clean, history accepted as sunk cost) is the pragmatic choice — a maintainer decision, not a mechanical one

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
