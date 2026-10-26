# QFC Pending Style Rule (Issue #1007, B-1, P5-T7)

Timestamp: 2026-10-26T00-05
Command: regex search of QuickFiler/Resources/FolderBreadcrumb.html for `\.sub\.active`, with the positions of the `<style>` and `</style>` tags
EXIT_CODE: 0
Output Summary: Exactly 1 match. It sits inside the `<style>` block, on the line just before `</style>`. Its rule declares `background: var(--bc-sel-back)` and `color: var(--bc-sel-fore)`.

## Match and enclosing rule

```
.sub.selected, .sub.active { background: var(--bc-sel-back); color: var(--bc-sel-fore); }
```

| Check | Result |
|---|---|
| Matches for `.sub.active` | 1 |
| Inside the `<style>` block | yes (after `<style>`, immediately before `</style>`) |
| Declarations | `background: var(--bc-sel-back)`; `color: var(--bc-sel-fore)` |
| Theme coverage | both variables are redefined by the `:root[data-theme="dark"]` block and the `prefers-color-scheme: dark` block of the same stylesheet |
