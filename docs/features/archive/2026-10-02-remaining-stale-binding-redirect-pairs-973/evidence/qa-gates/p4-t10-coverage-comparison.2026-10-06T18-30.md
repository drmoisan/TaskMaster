# P4-T10 coverage comparison, baseline versus final (AC14 as amended, D2)

Timestamp: 2026-10-06T18-30
Command: CMD-COVERAGE-COMPARE STAGE=final (pwsh -NoProfile -Command 'Set-Location -LiteralPath "<execution-worktree-root>"; . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1"); ... Get-CoberturaFirstPartyCoverageReport over coverage\baseline-973.cobertura.xml and coverage\final-973.cobertura.xml ... LINE-PERCENT-DELTA ... BRANCH-PERCENT-DELTA ...'), plus git -C <execution-worktree-root> diff --name-only a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- *.cs, git -C <execution-worktree-root> status --porcelain --untracked-files=all -- *.cs and git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- *.cs
EXIT_CODE: 0
Output Summary: COMPARABILITY: A. Line and branch denominators are equal (65855 and 17078). Baseline 85.36% lines / 79.75% branches; final 85.35% / 79.74%; both deltas -0.01, within the -0.10 tolerance. Final figures are above the 80% line and 75% branch floors. CHANGED-EXECUTABLE-LINES: 0: the .cs diff is the six section 5 files only, porcelain is empty, and the changes are deleted usings, one added `partial`, a verbatim move and two removed blank lines.

## Printed lines

BASELINE-FIRST-PARTY: First-party coverage: lines 56212/65855 (85.36%), branches 13620/17078 (79.75%)
FINAL-FIRST-PARTY: First-party coverage: lines 56207/65855 (85.35%), branches 13618/17078 (79.74%)
LINES-VALID baseline=65855 final=65855 EQUAL=True
BRANCHES-VALID baseline=17078 final=17078 EQUAL=True
LINE-PERCENT-DELTA: -0.01 WITHIN-TOLERANCE=True
BRANCH-PERCENT-DELTA: -0.01 WITHIN-TOLERANCE=True

BASELINE-MATCHES-ARTIFACT: True (evidence/baseline/mstest-coverage-baseline.md reads `First-party coverage: lines 56212/65855 (85.36%), branches 13620/17078 (79.75%)`, figure for figure)
FIRST-PARTY-LINE-PERCENT (P4-T9): 85.35 (at least 80: True)
FIRST-PARTY-BRANCH-PERCENT (P4-T9): 79.74 (at least 75: True)
COMPARABILITY: A

## Changed-line justification

CHANGED-EXECUTABLE-LINES: 0

git -C <execution-worktree-root> diff --name-only a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- *.cs

    UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.ConditionalEngine.cs
    UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs
    UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs
    UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs
    UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs
    UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs

git -C <execution-worktree-root> status --porcelain --untracked-files=all -- *.cs: (empty)

Numstat at PLAN-START-HEAD (P3-T18 figures, CategoryClassifierGroup.cs superseded by Part H):

    106	0	UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.ConditionalEngine.cs
    1	98	UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs
    0	1	UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs
    0	1	UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs
    0	1	UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs
    0	1	UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs

P3-T24 census: MOVE-DIFFERENCES: 0, ADDED-LINES: 1 (evidence/other/category-classifier-group-split-census.md). The only .cs changes are deleted `using` lines, one declaration gaining `partial`, the verbatim move of the 93-line region into the new partial file, and the two blank lines removed ahead of the moved `#region` line. None of these adds or removes a sequence point; the unchanged denominators (65855 lines, 17078 branches) are consistent with that.

Observation (recorded, not gated): the covered counts moved by 5 lines (UtilitiesCS 4, QuickFiler 1) and 2 branches (UtilitiesCS) between two runs over an identical denominator. No changed line is executable, so this is variation between runs in code this item does not change. It is within the D2 tolerance.
