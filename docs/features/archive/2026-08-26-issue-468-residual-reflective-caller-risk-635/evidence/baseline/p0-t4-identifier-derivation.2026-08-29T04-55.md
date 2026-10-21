# Commit-Level Identifier Derivation (P0-T4) — discharges AC-1

- **Issue:** #635
- **Plan task:** [P0-T4]
- **Removal commit:** `63eebd47`

Timestamp: 2026-08-29T06-24

## Output Summary

Commit `63eebd47` resolves in this worktree. Its subject is
`fix(468): remove unreachable load paths and the dead _templateTlp field`. Its only source-file change
is `QuickFiler/Controllers/QfcCollectionController.cs`, with 241 lines removed and no lines added to
that file. The diff of that file carries a removed declaration line for each of the thirteen
identifiers: twelve method declarations and one field declaration. The search set for this item is
therefore derived from the commit rather than from the twelve-identifier AC-16 list.

IDENTIFIER_ROWS: 13

## Command 1

Command: `git show --stat 63eebd47`

EXIT_CODE: 0

Output, verbatim:

```
commit 63eebd47ee29402cccb4868b1ac579ce42202626
Author: Dan Moisan <drmoisan@gmail.com>
Date:   Wed Aug 26 08:56:22 2026 -0400

    fix(468): remove unreachable load paths and the dead _templateTlp field

    Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>

    Claude-Session: https://claude.ai/code/session_01Mic58ikwEhpXsTnhz9FShE

 QuickFiler/Controllers/QfcCollectionController.cs  |  241 -
 .../p0-t14-tests-coverage.2026-08-26T08-25.md      |   46 +-
 .../baseline/p0-t16-commit.2026-08-26T08-25.md     |   74 +
 ...t1-reflective-caller-search.2026-08-26T08-25.md |  253 +
 ...p1-t3-dead-identifier-sweep.2026-08-26T08-45.md |   98 +
 ...4-live-member-nonregression.2026-08-26T08-45.md |  138 +
 .../qa-gates/p1-t5-format.2026-08-26T08-45.md      |   70 +
 .../qa-gates/p1-t6-analyzers.2026-08-26T08-45.md   |  100 +
 .../qa-gates/p1-t7-nullable.2026-08-26T08-45.md    |   69 +
 .../qa-gates/p1-t8-suite.2026-08-26T08-45.md       |  165 +
 .../evidence/qa-gates/p1-t8/p1-t8.trx              | 6609 ++++++++++++++++++++
 .../plan.2026-08-24T09-39.md                       |   18 +-
 .../qfc-collection-controller-defects-468/spec.md  |    6 +-
 13 files changed, 7631 insertions(+), 256 deletions(-)
```

Exactly one file with a source extension appears in the stat, and its change is a pure deletion of 241
lines. Every other changed path in the commit is Markdown or a TRX evidence artifact under the issue
#468 feature folder.

## Command 2

Command: `git show 63eebd47 -- QuickFiler/Controllers/QfcCollectionController.cs`

EXIT_CODE: 0

The full diff is 241 removed lines and is not reproduced in full here. The removed lines that mention
any of the thirteen identifiers were extracted from that diff with the following filter, which reads
the same command's output and prints only removed lines naming one of the thirteen:

```
pwsh -NoProfile -Command 'git show 63eebd47 -- QuickFiler/Controllers/QfcCollectionController.cs | Select-String -Pattern "^-" | Where-Object { $_.Line -match "WireUpKeyboardHandler|AnyOpenDropDownsAsync|LoadGroups_02cAsync|LoadGroups_02bAsync|LoadGroup_03bAsync|LoadConversationsAndFoldersAsync|LoadItemGroup|LoadSequentialAsync|LoadGroupSequential|CacheTlpForMove|SwapTlp|CaptureTlpTemplate|_templateTlp" } | ForEach-Object { Write-Output $_.Line }'
```

Filter output, verbatim:

```
-        private TableLayoutPanel _templateTlp;
-            //await LoadGroups_02bAsync(items, template, _tlpStates);
-        public async Task LoadGroups_02cAsync(
-        public async Task LoadGroups_02bAsync(
-                    (mailItem, i) => LoadGroup_03bAsync(template, mailItem, i, digits, tlpStates)
-        private async Task<QfcItemGroup> LoadGroup_03bAsync(
-        public async Task LoadConversationsAndFoldersAsync()
-                .ForEachAsync(async x => await LoadItemGroup(x.i, x.grp));
-        internal async Task LoadItemGroup(int i, QfcItemGroup group)
-        public async Task LoadSequentialAsync()
-                .ForEachAsync(async x => await LoadGroupSequential(x.i, x.grp));
-        public async Task LoadGroupSequential(int i, QfcItemGroup grp)
-        internal void CacheTlpForMove()
-        internal void SwapTlp(TableLayoutPanel tlp)
-            CacheTlpForMove();
-        public void WireUpKeyboardHandler()
-        internal async Task<bool> AnyOpenDropDownsAsync(bool close, CancellationToken token)
-        internal void CaptureTlpTemplate()
-            _templateTlp = _formViewer.L1v0L2L3v_TableLayout.Clone();
-            _templateTlp.Name = "TemplateTableLayout";
```

Twenty removed lines mention one of the thirteen. Thirteen of them are declarations, one per
identifier; the remaining seven are removed call sites, a removed commented-out call, and two removed
assignments to the removed field, all of which were removed in the same commit and are recorded here
for completeness rather than as declarations.

## The thirteen-row derivation table

Rows are in the order the specification's Context table lists the identifiers, which is the plan's
preamble order.

| # | Identifier | Kind | Removed line that declares it, verbatim from the diff |
|---|---|---|---|
| 1 | `WireUpKeyboardHandler` | method | `-        public void WireUpKeyboardHandler()` |
| 2 | `AnyOpenDropDownsAsync` | method | `-        internal async Task<bool> AnyOpenDropDownsAsync(bool close, CancellationToken token)` |
| 3 | `LoadGroups_02cAsync` | method | `-        public async Task LoadGroups_02cAsync(` |
| 4 | `LoadGroups_02bAsync` | method | `-        public async Task LoadGroups_02bAsync(` |
| 5 | `LoadGroup_03bAsync` | method | `-        private async Task<QfcItemGroup> LoadGroup_03bAsync(` |
| 6 | `LoadConversationsAndFoldersAsync` | method | `-        public async Task LoadConversationsAndFoldersAsync()` |
| 7 | `LoadItemGroup` | method | `-        internal async Task LoadItemGroup(int i, QfcItemGroup group)` |
| 8 | `LoadSequentialAsync` | method | `-        public async Task LoadSequentialAsync()` |
| 9 | `LoadGroupSequential` | method | `-        public async Task LoadGroupSequential(int i, QfcItemGroup grp)` |
| 10 | `CacheTlpForMove` | method | `-        internal void CacheTlpForMove()` |
| 11 | `SwapTlp` | method | `-        internal void SwapTlp(TableLayoutPanel tlp)` |
| 12 | `CaptureTlpTemplate` | method | `-        internal void CaptureTlpTemplate()` |
| 13 | `_templateTlp` | field | `-        private TableLayoutPanel _templateTlp;` |

Rows 1 through 12 are method declarations. Row 13 is a field declaration, `private TableLayoutPanel`,
which is the thirteenth removed member the AC-16 build-input search omitted. [P3-T1] records that
omission as correction 1.

Two declarations in the table end with an open parenthesis because the parameter list continues on the
following removed line; those are rows 3, 4 and 5. The identifier and its accessibility and return
type are complete on the quoted line in every case.

BLOCKER: none. Commit `63eebd47` resolves in this worktree as
`63eebd47ee29402cccb4868b1ac579ce42202626`.
