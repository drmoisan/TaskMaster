---
name: absence-gate-must-target-the-file-that-carries-it
description: Scoping an absence grep to a file that never contained the token makes a no-op gate; verify the token's actual file set before writing the pathspec
metadata:
  type: feedback
---

Before writing `git grep -c -F "<token>" -- <path>` exits 1 as an acceptance, verify the token is present in THAT path today. A token that is genuinely present somewhere in the repository can still be absent from the specific file the acceptance names.

**Why:** in #823 the draft asserted the removal of `Ignored when null` from BOTH `QuickFiler/Viewers/QfcFormViewer.cs` and `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs`. Only the first file carries it; the registry file phrases the same idea as "A null value is ignored rather than rejected" and "A null value is ignored on the same reasoning". The registry-scoped clause was therefore true before the edit and passed whatever the executor did. The union-scoped clause in the AC was fine — the defect appears only when a per-file task splits the union.

**How to apply:** derive absence tokens per file by reading that file, never by reusing a sibling file's phrasing. Then baseline every one of them in the Phase 0 token-baseline task and require each count >= 1, with an explicit halt if any count is 0 — that halt is what converts "I believe this is false-before" into a measured fact. Watch the token-count arithmetic downstream: raising the baseline task from five tokens to eight also changes its "five `BASELINE-TOKEN:` lines" acceptance and the Literals section's disappear-list.

Related: [[acceptance-edits-must-be-false-before-true-after]], [[zero-hit-grep-gates-need-carveouts]], [[single-numeral-gates-must-name-the-role]].
