# Line-5 Baseline, Recorded by Class Only — Remediation R-1, Issue #752

- Timestamp: 2026-09-03T23-42
- Task: `[P0-T5]`
- Target file: `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md`

Command:

1. `pwsh -NoProfile -Command` reading the target file with `Get-Content -LiteralPath`, reporting
   `@(Get-Content -LiteralPath <path>).Count`, testing the line-5 prefix, and computing the SHA-256
   of the UTF-8 bytes of line 5.
2. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md`

EXIT_CODE:

1. `0`
2. `0`

Output Summary:

- `LINE_COUNT: 184`. Measured as `@(Get-Content -LiteralPath <path>).Count`. It was not derived by
  splitting the raw file text on a newline character: the file is LF-terminated and that method
  reports 185, one greater than the true count.
- Line 5 begins with the literal prefix `- Worktree: `. Verified by `String.StartsWith`, which
  returned `True`.
- `LINE5_SHA256: efe7ce1c8bbaf7fbde1369b498573096ed70738c40f3358dbd5ca93fef03bacf` — 64 hexadecimal
  characters, over the UTF-8 bytes of line 5 as read. A cryptographic digest is a one-way transform
  and is not a quoted value under the no-quoting rule.
- Per-token hit classes for line 5, from the single File-mode invocation (command 2). The invocation
  printed exactly one `FILECOUNT:` line and one `FILEMATCH:` line:

```
FILEMATCH: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md | LINE: 5 | TOKENS: account,parentdir,winprofile
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md | COUNT: 1
```

  Read as token classes: line 5 carries the account-name token, the worktree-parent directory-name
  token, and the Windows user-profile path prefix. It carries neither the forward-slash Windows
  user-profile variant nor the POSIX user-profile path segment.

Acceptance checks:

- `LINE_COUNT:` is 184 — satisfied.
- Exactly one `FILECOUNT:` line was printed by the File-mode invocation, reporting `COUNT: 1`.
- The single `FILEMATCH:` line reports `LINE: 5`.
- `LINE5_SHA256:` is a 64-character hexadecimal digest.
- This artifact contains no reproduction of the line's value. Every substituted token is described by
  class only.
