# [P9-T6] Feature #476 cross-feature invariant — the `WebView2BreadcrumbHost` construction

Timestamp: 2026-08-28T01-50
Task: [P9-T6]
Command: source inspection of the delivered `QuickFiler/Controllers/EfcFormController.cs` against
`git show 002335989830ba9f3ad802858ef0b794f6281750:QuickFiler/Controllers/EfcFormController.cs`, plus
`git diff --unified=0 002335989830ba9f3ad802858ef0b794f6281750 -- QuickFiler/Controllers/EfcFormController.cs`
EXIT_CODE: 0

## Delivered location

| Measure | Value |
|---|---|
| Pre-change line range at `BASELINE_SHA` | `:834-837` |
| **Delivered start line** | **`:918`** |
| **Delivered end line** | **`:921`** |
| Enclosing method, pre-change | `ConfigureBreadcrumbControl()` (declared `:832`) |
| **Enclosing method, delivered** | **`ConfigureBreadcrumbControl()` (declared `:916`)** |

The construction moved down 84 lines because earlier remedies (RC1 guards, the five RC3 boundary
extractions, RC8 and RC9 helpers) added members above it. It did **not** move relative to its enclosing
method: it is still the first statement of `ConfigureBreadcrumbControl`, two lines after that method's
declaration, exactly as before.

## Byte-identity

The four delivered lines and the four pre-change lines were compared as exact strings:

```
'            _breadcrumbHost = new WebView2BreadcrumbHost('
'                _formViewer.BreadcrumbWebView,'
'                new WebView2CoreInitializer()'
'            );'
```

- Identical **including** leading whitespace: **True**
- Identical after stripping leading and trailing whitespace: **True**

The condition asks for identity "apart from leading whitespace"; the stronger result holds — the four
lines are byte-identical including their indentation. Neither the argument order, the argument
expressions, the constructor call shape, nor the assignment target changed.

## Diff-hunk check

```
git diff --unified=0 002335989830ba9f3ad802858ef0b794f6281750 -- QuickFiler/Controllers/EfcFormController.cs
```

Counting the added and removed lines of that diff that mention any of `WebView2BreadcrumbHost`,
`_formViewer.BreadcrumbWebView` or `new WebView2CoreInitializer()` returns **0**. No hunk adds or removes
any line of the construction. The gate is run against `BASELINE_SHA` here rather than against the
merge-corrected base because `EfcFormController.cs` was not touched by either merged sibling, so the two
bases give the same file content and the as-written command is exact.

Feature #476 (`bug/webview2-host-initializer-defects-476`) merged into this base and depends on this
construction site. It is intact.

Output Summary: PASS. The `new WebView2BreadcrumbHost(...)` construction is delivered at
`EfcFormController.cs:918-921`, still the first statement of `ConfigureBreadcrumbControl()`, and is
byte-identical to its pre-change text at `:834-837` including leading whitespace. The unified diff
against `BASELINE_SHA` contains zero added or removed lines belonging to that construction, so feature
#476's dependency is not moved or reshaped.
