# [P5-T2] AC-U6 non-vacuity mutations: site 1 (seam test plus structural gate) and site 2 (structural gate)

- Issue: #792
- Timestamp: 2026-09-17T20-23
- Command: site 1: CMD-OUTLOOK (`OUTLOOK-CLOSED: true` before each build), then for the mutated tree and again for the restored tree: CMD-BUILD-PLAIN (`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`), CMD-VSTEST, CMD-SCOPED-RUN with `<FILTER>` = `FullyQualifiedName~WebView2BreadcrumbHostIssue792Tests.InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam` and `<task>` = `p5-t2-mutation` / `p5-t2-restored`, then CMD-AC-U6-GATE; restoration by `git checkout -- QuickFiler/Viewers/WebView2BreadcrumbHost.cs`. Site 2 (with site 1 restored): the ViewerSetup edit, CMD-AC-U6-GATE, `git checkout -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, CMD-AC-U6-GATE again. All run from `coverage/plan792-helper.ps1` (the gate block embedded verbatim) with the item worktree as the working directory; consoles in the gitignored `coverage/p5-t2-mutation-build.log`, `coverage/p5-t2-mutation-scoped.log`, `coverage/p5-t2-restore-build.log`, `coverage/p5-t2-restored-scoped.log`; TRX under the gitignored `coverage/test-results/p5-t2-mutation/` and `coverage/test-results/p5-t2-restored/`
- EXIT_CODE: 0
- Output Summary: site 1 mutated: `Test Run Failed.`, `Total tests: 1`, `Failed: 1` (exit 1) on the pre-predicted `AdditionalBrowserArguments` assertion (found `<null>`), gate `PRIMARY-CONSTRUCTION-COUNT: 2`, `AC-U6-STRUCTURAL: FAIL`; site 1 restored: `Test Run Successful.`, `Total tests: 1`, `Passed: 1` (exit 0), gate 1/0/3/3 `PASS`. Site 2 mutated: gate `PRIMARY-CONSTRUCTION-COUNT: 2`, `CONTRACT-READER-COUNT: 2`, `AC-U6-STRUCTURAL: FAIL`; site 2 restored: gate 1/0/3/3 `PASS`. Both files restored byte-identical (SHA-256 equal before mutation and after restoration), BOM state preserved (host: none; ViewerSetup: BOM present throughout), `git diff --numstat HEAD` prints nothing for both.

## Site 1 (`QuickFiler/Viewers/WebView2BreadcrumbHost.cs`)

MUTATION: line 264 (re-derived before the edit; the plan's premise table cites the pre-fix line 250) `CoreWebView2EnvironmentOptions options = WebView2EnvironmentContract.CreateOptions();` became `CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions();`. Needle matched once before and zero after; replacement present once; 382 lines before and after; `git diff --numstat HEAD` read `1 1`. The file carries no BOM at HEAD (`BOM-BEFORE-MUTATION: False`, `BOM-AFTER-MUTATION: False`, `BOM-AFTER-RESTORE: False`).

PREDICTED-FAILING-ASSERTION: `WebView2BreadcrumbHostIssue792Tests.InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam` fails on `capturedOptions.AdditionalBrowserArguments.Should().Be("--incognito ", ...)` (`WebView2BreadcrumbHostIssue792Tests.cs:93-98`): expected `"--incognito "`, actual `<null>` (a parameterless options object carries null arguments); the folder assertion (`:82-87`) and the not-null assertion (`:88-92`) before it still pass. Structural gate predicted `PRIMARY-CONSTRUCTION-COUNT: 2` and `AC-U6-STRUCTURAL: FAIL`.

Mutated build: exit 0, `0 Warning(s)`, exact line `0 Error(s)`; 27 `CoreCompile:` lines (unanchored), 10 `csc.exe` lines (1 producing `QuickFiler.dll`, 1 producing `QuickFiler.Test.dll`); production DLL rewritten 20:23:02 to 20:23:57, test DLL 20:23:05 to 20:24:00.

OBSERVED (first `Error Message` line, verbatim): `Expected capturedOptions.AdditionalBrowserArguments to be "--incognito " because every WebView2 site must share the same incognito browser argument, but found <null>.`

Observed run: `Failed InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam [305 ms]`; `Test Run Failed.`; `Total tests: 1`; `Failed: 1`; exit 1.

Observed gate (mutated):

```
PRIMARY-CONSTRUCTION-COUNT: 2
PRIMARY-SITE: QuickFiler/Viewers/WebView2BreadcrumbHost.cs:264
PRIMARY-SITE: QuickFiler/Viewers/WebView2EnvironmentContract.cs:50
CREATEASYNC-OUTSIDE-ADAPTER: 0
SEAM-CALLER-COUNT: 3
CONTRACT-READER-COUNT: 2
AC-U6-STRUCTURAL: FAIL
```

PREDICTION-MATCHES-OBSERVATION: true (test and gate).

Restoration proof (site 1):

- SHA256-BEFORE-MUTATION: `BD6E5F07AC709BF8C274045C5AC0508E6447A0E686657F742DD0195134BDF0C0` (equal to the gitignored snapshot `coverage/p5-t2-snapshot.cs`)
- SHA256-AFTER-RESTORE: `BD6E5F07AC709BF8C274045C5AC0508E6447A0E686657F742DD0195134BDF0C0`
- RESTORED-IDENTICAL: true; needle count after restore 1, replacement count 0
- RESTORED: `git diff --numstat HEAD -- QuickFiler/Viewers/WebView2BreadcrumbHost.cs` prints nothing; `git diff --no-index --numstat` snapshot vs restored file exit 0

Restored build: exit 0, `0 Warning(s)`, `0 Error(s)`; 27 `CoreCompile:`, 10 `csc.exe` (1 + 1); production DLL 20:23:57 to 20:24:16, test DLL 20:24:00 to 20:24:17. Restored run: `Passed InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam [209 ms]`; `Test Run Successful.`; `Total tests: 1`; `Passed: 1`; `Failed: 0 (omitted category)`; exit 0. Restored gate: `PRIMARY-CONSTRUCTION-COUNT: 1` (`QuickFiler/Viewers/WebView2EnvironmentContract.cs:50`), `CREATEASYNC-OUTSIDE-ADAPTER: 0`, `SEAM-CALLER-COUNT: 3` (`EfcItemController.WebViewEnvironment.cs:46`, `QfcItemController.ViewerSetup.cs:66`, `WebView2BreadcrumbHost.cs:279`), `CONTRACT-READER-COUNT: 3` (`EfcItemController.WebViewEnvironment.cs:40`, `QfcItemController.ViewerSetup.cs:57`, `WebView2BreadcrumbHost.cs:264`), `AC-U6-STRUCTURAL: PASS`.

## Site 2 (`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`), applied with site 1 restored

This half is the site-2 proof cited by [P7-T14]. Per the task text it is gate-only: no build and no test run were performed on the site-2 mutation.

MUTATION: line 57 (re-derived; the plan's premise table cites the pre-fix line 62) `CoreWebView2EnvironmentOptions options = WebView2EnvironmentContract.CreateOptions();` became `CoreWebView2EnvironmentOptions options = new("--incognito ");`. Needle matched once before and zero after; replacement present once; 474 lines before and after; `git diff --numstat HEAD` read `1 1`. The file carries a UTF-8 BOM at HEAD and the BOM was present before the edit, after the edit and after restoration (`True`, `True`, `True`); the edit was written through a BOM-preserving encoder, so the [P4-T2] strip-and-restore incident did not recur.

PREDICTED: `PRIMARY-CONSTRUCTION-COUNT: 2`, `CONTRACT-READER-COUNT: 2`, `AC-U6-STRUCTURAL: FAIL` (the target-typed `new(` form is caught by the gate's second construction pattern `CoreWebView2EnvironmentOptions\s+\w+\s*=\s*new\s*\(`).

OBSERVED gate (mutated):

```
PRIMARY-CONSTRUCTION-COUNT: 2
PRIMARY-SITE: QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:57
PRIMARY-SITE: QuickFiler/Viewers/WebView2EnvironmentContract.cs:50
CREATEASYNC-OUTSIDE-ADAPTER: 0
SEAM-CALLER-COUNT: 3
CONTRACT-READER-COUNT: 2
CONTRACT-READER: QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs:40
CONTRACT-READER: QuickFiler/Viewers/WebView2BreadcrumbHost.cs:264
AC-U6-STRUCTURAL: FAIL
```

PREDICTION-MATCHES-OBSERVATION: true.

Restoration proof (site 2):

- SHA256-BEFORE-MUTATION: `AAD50304873955794E69DFE7957B8550D1F1AAF733ACFC37E7A3ABFC4A6F2D1D` (equal to the gitignored snapshot `coverage/p5-t2-site2-snapshot.cs`)
- SHA256-AFTER-RESTORE: `AAD50304873955794E69DFE7957B8550D1F1AAF733ACFC37E7A3ABFC4A6F2D1D`
- RESTORED-IDENTICAL: true; BOM-AFTER-RESTORE: true; needle count after restore 1, replacement count 0
- RESTORED: `git diff --numstat HEAD -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` prints nothing; `git diff --no-index --numstat` snapshot vs restored file exit 0
- Restored gate: `PRIMARY-CONSTRUCTION-COUNT: 1`, `CREATEASYNC-OUTSIDE-ADAPTER: 0`, `SEAM-CALLER-COUNT: 3`, `CONTRACT-READER-COUNT: 3`, `AC-U6-STRUCTURAL: PASS`

Scoped porcelain (`git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'`) after each restoration: prints nothing.
