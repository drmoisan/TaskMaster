---
timestamp: 2026-09-02T20-52
plan: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/plan.2026-09-02T08-58.md
task: P3-T1
ac: AC1
---

# AC1 Verification: _format-check.yml Citation at Line 194

Timestamp: 2026-09-02T20-52

Command: `Select-String -Path CLAUDE.md -Pattern '_format-check\.yml'`

EXIT_CODE: 0

Output Summary: Exactly one match found at LineNumber 194. The matched line contains the citation `.github/workflows/_format-check.yml` in the correct position within the CSharpier pinned-version parity sentence. AC1 PASS.

## Matched Line

```
CLAUDE.md:194:   - Always invoke through `dotnet tool run` so the manifest-pinned version is used. Do not invoke a globally installed `csharpier`: a different global version produces diffs that disagree with `.github/workflows/_format-check.yml`, which runs the pinned version after `dotnet tool restore`.
```

---

**AC1 Status: PASS** — CLAUDE.md line 194 cites `.github/workflows/_format-check.yml` (not ci.yml) for the CSharpier pinned-version claim.
