---
timestamp: 2026-09-02T08-58
plan: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/plan.2026-09-02T08-58.md
task: P0-T5
---

# CLAUDE.md Pre-Fix Citation Baseline

Timestamp: 2026-09-02T08-58

Command: `Select-String -Path CLAUDE.md -Pattern 'ci\.yml'`

EXIT_CODE: 0

Output Summary: Three matches found at lines 194, 202, and 210, each containing the citation `.github/workflows/ci.yml`.

## Pre-Fix Line Text

### Line 194 (CSharpier section)

Full line text:
```
   - Always invoke through `dotnet tool run` so the manifest-pinned version is used. Do not invoke a globally installed `csharpier`: a different global version produces diffs that disagree with `.github/workflows/ci.yml`, which runs the pinned version after `dotnet tool restore`.
```

Citation token to be replaced: `.github/workflows/ci.yml`
Target replacement: `.github/workflows/_format-check.yml`

### Line 202 (Analyzer section)

Full line text:
```
   - Use `/t:Rebuild`, not `/t:Build`. Analyzer diagnostics are produced during compilation, and MSBuild's incremental up-to-date check compares timestamps without invalidating on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and runs no analyzers. `.github/workflows/ci.yml` uses `/t:Build /m` for its analyzer step because a runner checkout is always cold; a local working tree is not.
```

Citation token to be replaced: `.github/workflows/ci.yml`
Target replacement: `.github/workflows/_build-analyzers.yml`

### Line 210 (Nullable section)

Full line text:
```
   - This is character-for-character the command in `.github/workflows/ci.yml` (step "Build with nullable warnings treated as errors"). Two properties of it are load-bearing and must not be "restored":
```

Citation token to be replaced: `.github/workflows/ci.yml`
Target replacement: `.github/workflows/_build-nullable.yml`
Note: Retain the parenthetical step name "Build with nullable warnings treated as errors"

## Verification: .claude/rules/csharp.md Search

Command: `Select-String -Path .claude/rules/csharp.md -Pattern 'ci\.yml|workflows'`

EXIT_CODE: 0 (no matches)

Output Summary: Zero matches found in .claude/rules/csharp.md for either 'ci.yml' or 'workflows'. This confirms that .claude/rules/csharp.md requires no edit and is out of scope per AC5.

---

This baseline establishes the pre-fix state for all three citations and serves as the false-before evidence for AC1–AC4 and the baseline for AC5.
