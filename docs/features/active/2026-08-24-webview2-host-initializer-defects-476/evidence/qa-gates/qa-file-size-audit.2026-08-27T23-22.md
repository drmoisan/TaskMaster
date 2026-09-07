# File Size Audit ([P4-T6])

Timestamp: 2026-08-27T23-22

Command:

```
for f in QuickFiler/Viewers/WebView2BreadcrumbHost.cs QuickFiler/Viewers/WebView2CoreInitializer.cs QuickFiler/Viewers/IWebViewCoreInitializer.cs QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs; do printf "%5d  %s\n" "$(grep -c '' "$f")" "$f"; done
```

(run from the workspace root, after the `[P4-T1]` formatter step)

EXIT_CODE: 0

## Rows

| # | File | Line count | Limit | Within limit |
| --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | 368 | 500 | Yes |
| 2 | `QuickFiler/Viewers/WebView2CoreInitializer.cs` | 103 | 500 | Yes |
| 3 | `QuickFiler/Viewers/IWebViewCoreInitializer.cs` | 66 | 500 | Yes |
| 4 | `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` | 173 | 500 | Yes |
| 5 | `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` | 440 | 500 | Yes |
| 6 | `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs` | 201 | 500 | Yes |

## Output Summary

- Exactly six rows, one per file named by `[P4-T1]`.
- Every recorded line count is 500 or fewer. The largest is
  `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` at 440 lines, 60 lines below the
  500-line ceiling in `.claude/rules/general-code-change.md` and `CLAUDE.md` C#5.
- `grep -c ''` counts physical lines including the final line, so a file whose last line lacks a
  trailing newline is still counted. Counts were taken after the CSharpier apply in `[P4-T1]`,
  because the formatter changes line counts.
- Decisions Record item 6 anticipated this audit: the contract assertions and the pump-hosted
  behavioural tests were split into two files precisely so that neither would approach the ceiling.
  At 201 and 440 lines respectively, both remain under it.
