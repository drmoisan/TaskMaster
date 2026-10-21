# Manifest Edit Facts (P2-T6)

Timestamp: 2026-10-09T14-17
Command: Grep tool counts (`^`, `\r$`, declaration patterns); git diff --numstat BASE-SHA -- QuickFiler.Test/packages.config UtilitiesCS.Test/packages.config TaskTree.Test/packages.config QuickFiler.Test/QuickFiler.Test.csproj; git status --porcelain -- (same four paths)
EXIT_CODE: 0
Output Summary:
- Line counts: QuickFiler.Test 76, UtilitiesCS.Test 111, TaskTree.Test 70 (expected 76, 111, 70)
- Carriage-return counts: QuickFiler.Test 75, UtilitiesCS.Test 110, TaskTree.Test 69 (expected 75, 110, 69; every inserted line kept CRLF)
- numstat: QuickFiler.Test/packages.config 2 0; UtilitiesCS.Test/packages.config 1 0; TaskTree.Test/packages.config 1 0; QuickFiler.Test/QuickFiler.Test.csproj 0 5 (expected 2 0, 1 0, 1 0, 0 5)
- porcelain: ` M` for all four paths
- QuickFiler.Test.csproj: `Include="Microsoft.Web.WebView2.Core,` count 1 (line 388); 567 lines; `Microsoft.Web.WebView2.1.0.4191.47` folder count 2 (lines 389, 392)
- Result: every value equals the expectation.

## Inserted lines

| File | Line | Text |
|---|---|---|
| QuickFiler.Test/packages.config | 42 | `  <package id="Microsoft.Web.WebView2" version="1.0.4191.47" targetFramework="net481" />` |
| QuickFiler.Test/packages.config | 47 | `  <package id="ObjectListView.Official" version="2.9.1" targetFramework="net481" />` |
| UtilitiesCS.Test/packages.config | 64 | `  <package id="Microsoft.Web.WebView2" version="1.0.4191.47" targetFramework="net481" />` |
| TaskTree.Test/packages.config | 41 | `  <package id="ObjectListView.Official" version="2.9.1" targetFramework="net481" />` |

## Production sibling declarations (re-read in this task)

| Sibling | Line | Text |
|---|---|---|
| QuickFiler/packages.config | 19 | `  <package id="Microsoft.Web.WebView2" version="1.0.4191.47" targetFramework="net481" />` |
| QuickFiler/packages.config | 22 | `  <package id="ObjectListView.Official" version="2.9.1" targetFramework="net481" />` |
| UtilitiesCS/packages.config | 57 | `  <package id="Microsoft.Web.WebView2" version="1.0.4191.47" targetFramework="net481" />` |
| TaskTree/packages.config | 8 | `  <package id="ObjectListView.Official" version="2.9.1" targetFramework="net481" />` |

Each inserted declaration matches its production sibling's id, version and targetFramework, and the folder the existing HintPaths name (`Microsoft.Web.WebView2.1.0.4191.47`, `ObjectListView.Official.2.9.1`).

## numstat

```
0	5	QuickFiler.Test/QuickFiler.Test.csproj
2	0	QuickFiler.Test/packages.config
1	0	TaskTree.Test/packages.config
1	0	UtilitiesCS.Test/packages.config
```

## porcelain

```
 M QuickFiler.Test/QuickFiler.Test.csproj
 M QuickFiler.Test/packages.config
 M TaskTree.Test/packages.config
 M UtilitiesCS.Test/packages.config
```
