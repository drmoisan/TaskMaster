# collect-pr-context-misclassifies-csharp-as-documentation (Issue #513)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/collect-pr-context-misclassifies-csharp-as-documentation/ (Issue #513)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #513
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/513
- Last Updated: 2026-08-08
## Summary

The PR-context collector writes an `artifacts/pr_context.summary.txt` whose "Changed files overview" classifies changed C# source files as documentation, reporting "Core logic changes: 0 files" on a branch that changes 30 `.cs` files. Because a downstream coverage hook enumerates languages from that summary, the misclassification can cause the C# coverage gate to be skipped rather than enforced.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: `mcp__drm-copilot__collect_pr_context` (drm-copilot MCP extension)
- Command/flags used: `collect_pr_context` with `base=main` against branch `bug/quickfiler-search-keystroke-focus-steal-438`
- Data source or fixture: `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`

## Steps to Reproduce

1. Check out a branch whose diff against `main` contains a substantial number of changed `.cs` files (30 in the observed case, alongside a larger number of Markdown evidence artifacts).
2. Run `collect_pr_context` with `base=main`.
3. Read the "Changed files overview" section of `artifacts/pr_context.summary.txt` and compare it against `artifacts/pr_context.appendix.txt` and `git diff --name-only main..HEAD`.

## Expected Behavior

The summary classifies changed files by language accurately, so that a reviewer or an automated hook reading only the summary reaches the same conclusion about which languages changed as it would from the raw diff. A branch changing 30 C# files reports non-zero core logic changes and enumerates CSharp.

## Actual Behavior

The summary reported **"Core logic changes: 0 files"** and classified all 30 changed C# files as documentation. Observed twice in one session on issue #438 (cycle-1 and cycle-2 feature reviews), each time corrected in place by the reviewing agent appending the true `- <path> (+N/-N)` enumeration so the coverage hook would enumerate CSharp.

The appendix, by contrast, contained the correct file list. The defect is in the summary's classification step, not in diff collection.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no raw pre-correction summary was retained, because both occurrences were corrected in place during review. A fresh capture should accompany the fix; the defect reproduces on any branch with a large Markdown-to-C# file ratio.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

This is a silent-skip defect in a quality gate, which is the most dangerous shape a tooling bug can take. A coverage hook that enumerates languages from the summary will conclude no C# changed and skip the C# coverage gate entirely, reporting a clean pass while measuring nothing. It was caught here only because a reviewing agent independently cross-checked the appendix against the raw diff. On a branch reviewed with less scrutiny, a real coverage regression could merge unexamined. The severity is driven by the failure mode, not by frequency.

## Suspected Cause / Notes

Observed during orchestration of issue #438 on 2026-08-08; feature-review recorded it as a recurring generator defect seen repeatedly since issue #171, and confirmed via `gh` search that no tracking issue exists.

- The classifier appears to mis-bucket by proportion or by a truncated sample rather than by file extension: the observed branch carried far more `.md` evidence artifacts than `.cs` files, and the summary's overview is separately known to truncate its file list.
- The appendix is correct, so diff collection is sound; only the summary's language classification is wrong.
- Related known summary defects worth checking at the same time: the overview truncates its changed-files list, and the collector can report "GitHub CLI unavailable" even when `gh` is present and working in the calling shell (both observed on #438).

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: classify by file extension deterministically, with a test asserting that a synthetic diff of N `.cs` files plus M `.md` files reports N core-logic C# files for any N >= 1, including cases where M greatly exceeds N.
- [ ] Integration scenario to retest: run the collector against a branch with a high Markdown-to-C# ratio and assert the summary and appendix agree on the changed-language set.
- [ ] Manual verification notes: add a consistency assertion so the summary and appendix cannot disagree about which languages changed, and make the coverage hook fail closed — if the summary reports zero changed languages while the diff is non-empty, that should be an error rather than a silent skip.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
