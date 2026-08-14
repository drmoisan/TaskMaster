---
name: 553-ci-split-review-pattern
description: 'Cycle-1 review facts for the #553 CI parallel job split: 1 procedural Blocking (green run), byte-identity independently verified 14/14, benchmark-baselines provenance scoped out, re-audit checklist for the post-PR cycle'
metadata:
  type: project
---

Cycle 1 (2026-08-14T10-21, head 0b016c81) of the #553 CI parallel-job-split review found exactly one Blocking finding: `modified-workflow-needs-green-run` (procedural — branch not yet pushed, no run can exist). The change set itself was clean: byte-identity of all transplanted gate blocks was independently re-verified (14/14 SHA-256 matches vs merge-base ci.yml, script pattern: extract `run: |` blocks + full step blocks, dedent, hash); the new plain `Build solution` step in `_mstest-coverage.yml` carries zero analyzer/warning-promotion properties (gate-neutral); under-gating analysis is favorable because the strict ruleset fail-closes. Checked off spec S1-S5,S7 and user-story U1-U4,U6.

**Why:** the re-audit after the live-PR phases (plan P3-P7) will need to verify the remaining items, and the cycle-1 groundwork should not be redone.

**How to apply:** on the #553 re-audit, verify only the delta: (1) green run whose head SHA equals the then-current branch head (recompute; do not trust cycle-1 head 0b016c81 after new commits); (2) ruleset PUT evidence triple (ruleset-pre-put JSON, PUT payload, post-PUT GET) with exactly five live-captured contexts and `strict_required_status_checks_policy: true` retained — spec S6/S9, US U5/U8; (3) post-split timing evidence via `gh api .../runs/<id>/jobs` (runner parity) — spec S10; (4) whether README L82's `CI / <gate>` wording was fixed (Minor F2) — the correct context form is `<caller job> / <callee job>`, e.g. `format-check / Verify formatting`. Docs/YAML-only diff means Get-ChangedLanguageSet returns empty and the coverage hook runs only the 3 artifact-path checks (verified by dot-source simulation). benchmark-baselines rule: sibling provenance.json NOT required for the 444s baseline because its scope clause limits it to baselines consumed by a benchmark regression gate; reassess only if it gets wired into one. See [[remediation-handoff-skill-conflicts-with-hook]] for the flat-artifact-layout and planner-authors-the-plan conventions applied.
