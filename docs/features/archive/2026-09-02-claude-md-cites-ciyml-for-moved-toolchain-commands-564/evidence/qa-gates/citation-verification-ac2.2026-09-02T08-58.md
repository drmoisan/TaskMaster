---
timestamp: 2026-09-02T20-52
plan: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/plan.2026-09-02T08-58.md
task: P3-T2
ac: AC2
---

# AC2 Verification: _build-analyzers.yml Citation at Line 202

Timestamp: 2026-09-02T20-52

Command: `Select-String -Path CLAUDE.md -Pattern '_build-analyzers\.yml'`

EXIT_CODE: 0

Output Summary: Exactly one match found at LineNumber 202. The matched line contains the citation `.github/workflows/_build-analyzers.yml` in the correct position within the analyzer `/t:Build /m` sentence. AC2 PASS.

## Matched Line

```
CLAUDE.md:202:   - Use `/t:Rebuild`, not `/t:Build`. Analyzer diagnostics are produced during compilation, and MSBuild's incremental up-to-date check compares timestamps without invalidating on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and runs no analyzers. `.github/workflows/_build-analyzers.yml` uses `/t:Build /m` for its analyzer step because a runner checkout is always cold; a local working tree is not.
```

---

**AC2 Status: PASS** — CLAUDE.md line 202 cites `.github/workflows/_build-analyzers.yml` (not ci.yml) for the analyzer `/t:Build /m` claim.
