# [P6-T5] Stale-comment and banned-symbol sweep over the write-set production files

- Issue: #792
- Timestamp: 2026-09-17T20-56
- Command: (1) `Select-String -SimpleMatch 'until CoreWebView2InitializationCompleted fires'` across the six `QuickFiler/Controllers/EfcFormController*.cs` files; (2) `Select-String -Pattern 'Thread\.Sleep\(|Task\.Delay\('` across the twenty production write-set `.cs` files; (3) `Select-String -SimpleMatch '"WindowsFormsWebView2"'` and (4) `Select-String -SimpleMatch '"--incognito "'` across the 183 files enumerated by `git ls-files -- ':(glob)QuickFiler/**/*.cs'`; run from `coverage/plan792-helper.ps1` with the item worktree as the working directory (the helper's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` is the worktree proof and passed; HEAD `9ac987a969d1f4786d296fee25817c8e5dde9233`)
- EXIT_CODE: 0
- Output Summary: `SWEEP-1-STALE-COMMENT-COUNT: 0`; `SWEEP-2-BANNED-WAIT-COUNT: 0`; `SWEEP-3-WindowsFormsWebView2-COUNT: 1` at `QuickFiler/Viewers/WebView2EnvironmentContract.cs:30`; `SWEEP-4-incognito-COUNT: 1` at `QuickFiler/Viewers/WebView2EnvironmentContract.cs:24`. All four counts as stated by the task.

## Sweep 1 — stale comment

Enumeration (`EFCFORMCONTROLLER-FILES: 6`): `EfcFormController.Actions.cs`, `EfcFormController.Breadcrumb.cs`, `EfcFormController.cs`, `EfcFormController.EventHandlers.cs`, `EfcFormController.Helpers.cs`, `EfcFormController.SetupAndProperties.cs` (all under `QuickFiler/Controllers/`).

SWEEP-1-STALE-COMMENT-COUNT: 0

Positive control: `-SimpleMatch 'InitializeBreadcrumbHostAsync'` over the same six-file enumeration returned 2 hits (`SWEEP-1-CONTROL-InitializeBreadcrumbHostAsync-COUNT: 2`), so the enumeration reaches the files that carry the breadcrumb initialization code and a zero for the stale phrase is a true absence. An independent Grep for the same phrase over every `.cs` file in the worktree also returned no match, and a Grep for the bare token `CoreWebView2InitializationCompleted` over `QuickFiler/**/*.cs` returned 19 hits in 6 files (none of them an `EfcFormController*` file), confirming the token family is searchable and the stale phrase no longer exists anywhere.

## Sweep 2 — banned wall-clock waits

Enumeration: the twenty production write-set `.cs` paths listed in the plan's Write set (`PRODUCTION-WRITE-SET-FILES: 20`, `PRODUCTION-WRITE-SET-MISSING: 0`).

SWEEP-2-BANNED-WAIT-COUNT: 0

Positive controls: (a) widening the same alternation to `Thread\.Sleep\(|Task\.Delay\(|Task\.CompletedTask` over the same twenty files returned 1 hit (`SWEEP-2-CONTROL-widened-alternation-COUNT: 1`), so the alternation form and file enumeration fire when a listed term is present; (b) a looser pattern `Task\.Run\(|Task\.FromResult\(|await ` over the same twenty files returned 187 hits; (c) an independent Grep for `Thread\.Sleep\(|Task\.Delay\(` over all of `QuickFiler/**/*.cs` returned 5 `Task.Delay(` hits, every one in a file outside the write set (`QfcFormController.EventHandlers.cs:348`, `QfcItemController.EventWiring.cs:137`, `QfcQueue.cs:77`, `:141`, `:238`), so the pattern itself matches live code and the zero over the write set is a property of those twenty files, not of the pattern.

## Sweep 3 — `"WindowsFormsWebView2"` (quoted literal)

Enumeration: `git ls-files -- ':(glob)QuickFiler/**/*.cs'` (`QUICKFILER-LS-FILES-CS-COUNT: 183`; a PowerShell `-Path` wildcard does not recurse, so the enumeration is fed to `-LiteralPath`).

SWEEP-3-WindowsFormsWebView2-COUNT: 1

HIT: `QuickFiler/Viewers/WebView2EnvironmentContract.cs:30` — `internal const string UserDataFolderName = "WindowsFormsWebView2";` (the contract)

Control: the unquoted token `WindowsFormsWebView2` over the same enumeration also returns exactly 1 hit at the same line, so no unquoted or interpolated duplicate of the folder name survives in `QuickFiler/`. An independent Grep for the quoted literal over `QuickFiler/**/*.cs` returned the same single line.

## Sweep 4 — `"--incognito "` (quoted literal, trailing space)

SWEEP-4-incognito-COUNT: 1

HIT: `QuickFiler/Viewers/WebView2EnvironmentContract.cs:24` — `internal const string AdditionalBrowserArguments = "--incognito ";` (the contract)

Control: the unquoted token `--incognito` over the same enumeration also returns exactly 1 hit at the same line, so no other spelling of the argument (with or without the trailing space) survives in `QuickFiler/`; the former target-typed `new("--incognito ")` at `QfcItemController.ViewerSetup.cs` and the `IncognitoArgument` literal in `EfcItemController.cs` reported by [P0-T14] are gone (the alias now reads `WebView2EnvironmentContract.AdditionalBrowserArguments`). An independent Grep for the quoted literal over `QuickFiler/**/*.cs` returned the same single line.
