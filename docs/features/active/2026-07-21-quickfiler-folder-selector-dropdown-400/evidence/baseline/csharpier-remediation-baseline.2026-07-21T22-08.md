Timestamp: 2026-07-21T22-08Z
Command: `csharpier check .`
EXIT_CODE: 1
Output Summary: CSharpier checked 1,432 files. The only baseline delta is `coverage.config` line-ending normalization. The command was non-mutating; `git status --short -- coverage.config` and `git diff -- coverage.config` remain empty. This is recorded as baseline formatting debt and is not authorization for a Phase 0 edit.

```text
Checked 1432 files in 4353ms.
Error .\coverage.config - Was not formatted.
  The file contained different line endings than formatting it would result in.
```

Current worktree SHA-256 for `coverage.config`: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`.
