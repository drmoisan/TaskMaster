---
timestamp: 2026-09-02T20-52
plan: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/plan.2026-09-02T08-58.md
task: P3-T3
ac: AC3
---

# AC3 Verification: _build-nullable.yml Citation at Line 210

Timestamp: 2026-09-02T20-52

Command: `Select-String -Path CLAUDE.md -Pattern '_build-nullable\.yml'`

EXIT_CODE: 0

Output Summary: Exactly one match found at LineNumber 210. The matched line contains both the citation `.github/workflows/_build-nullable.yml` and the parenthetical step name "Build with nullable warnings treated as errors". AC3 PASS.

## Matched Line

```
CLAUDE.md:210:   - This is character-for-character the command in `.github/workflows/_build-nullable.yml` (step "Build with nullable warnings treated as errors"). Two properties of it are load-bearing and must not be "restored":
```

## Verification Results

- Citation `.github/workflows/_build-nullable.yml` present: YES
- Parenthetical step name "Build with nullable warnings treated as errors" retained: YES
- No additional text changes on this line: VERIFIED

---

**AC3 Status: PASS** — CLAUDE.md line 210 cites `.github/workflows/_build-nullable.yml` (not ci.yml) for the nullable `TreatWarningsAsErrors` claim, retaining the step-name parenthetical "Build with nullable warnings treated as errors".
