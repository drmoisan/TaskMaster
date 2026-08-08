# Phase 4 — MANUAL-ONLY Criteria Remain Unchecked (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P4-T2]
Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; Select-String -Path 'docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\spec.md' -Pattern '\*\*AC19|\*\*AC20|\*\*AC21'"`
EXIT_CODE: 0

## Output Summary

The three criterion lines, quoted verbatim (truncated at 120 characters for readability; the leading checkbox marker is the material part and is shown in full):

```text
- [ ] **AC19 (R5b, A3) — MANUAL-ONLY.** In a live Outlook Explorer, immediately after add-in reload and before initializ...
- [ ] **AC20 (R5b, A2, A3) — MANUAL-ONLY.** In the same live session, after `InitAsync()` completes, each of the eight c...
- [ ] **AC21 (R5c, A4) — MANUAL-ONLY.** In a live Outlook session, Office visually greys the eight buttons during initia...
```

A fourth match is the prose line under `## Delivery Notes and Deviations`, which is not a criterion:

```text
**AC19, AC20, and AC21 remain unchecked by design.** They are MANUAL-ONLY and require a live Outlook profile. The mainta...
```

| Criterion | Marker | Required |
|---|---|---|
| AC19 | `- [ ] **AC19` | `- [ ] **AC19` |
| AC20 | `- [ ] **AC20` | `- [ ] **AC20` |
| AC21 | `- [ ] **AC21` | `- [ ] **AC21` |

All three still begin `- [ ]`. **None was checked off in this cycle.**

Per plan section 3 rule 11 and the criteria's own text, these must never be checked off on the strength of unit tests, source inspection, or any automated artifact produced by this cycle. They require recorded live-Outlook verification against a running Outlook process and a live mail profile, which `.claude/rules/general-unit-test.md` forbids automated tests from depending on. The maintainer checklist at `evidence/manual-verification/ac19-ac21-checklist.2026-08-08T15-00.md` carries `Status: PENDING MAINTAINER EXECUTION` and is unchanged by this cycle.

## No acceptance criterion changed state in this cycle

Beyond the three MANUAL-ONLY criteria, `git diff -U0 -- .../spec.md | grep -E '^[+-].*- \[[ x]\]'` returns **no match**, confirming that **no** `- [ ]` or `- [x]` marker anywhere in `spec.md` changed state. The only `spec.md` edit is the append-only P4-T1 subsection: `git diff --numstat` reports `12  0` — twelve lines added, **zero deleted**.

The `- [ ] Blocker` / `- [x] High` / `- [ ] Medium` / `- [ ] Low` markers under `## Impact / Severity` are severity markers, not acceptance criteria, and are likewise unmodified.

Binary outcome satisfied: all three lines still begin `- [ ] **AC19`, `- [ ] **AC20`, and `- [ ] **AC21`.
