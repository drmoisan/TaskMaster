# Preflight Validation Note — Issue #262 (F2)

- Timestamp: 2026-07-07T18-05
- Plan: docs/features/active/2026-07-07-folder-settings-store-model-null-262/plan.2026-07-07T18-00.md
- Directive: PREFLIGHT VALIDATION ONLY (re-check after revision)
- Result: PREFLIGHT: ALL CLEAR

## Prior blocking defect (P5-T4) — resolved
Prior plan pointed the executor at issue.md for AC1-AC7, which is impossible: issue.md
`## Acceptance Criteria` contains only AC1-AC6 with different numbering. The revised P5-T4:
- Targets spec.md `## Acceptance Criteria` (verified: AC1-AC7, lines 297-322) as the
  authoritative full-bug AC source, each checked with a per-AC evidence-path annotation
  (AC1->P2-T1/P3-T1/P3-T3; AC2->P2-T2/P3-T1/P3-T3; AC3->P2-T3/P3-T1/P3-T3; AC4->P5-T3;
  AC5->P2-T4/P3-T3; AC6->P1-T3/P5-T1; AC7->P4-T1..P4-T5).
- Reconciles issue.md `## Acceptance Criteria` (verified: AC1-AC6, lines 86-98) to its own
  numbering: issue AC1-AC5 -> spec AC1-AC5; issue AC6 (toolchain) -> spec AC7; spec AC6
  (file-size) is spec-only with no issue.md counterpart. Mapping verified line-by-line.
- Mirrors the checked spec.md section to evidence/issue-updates/issue-262.<timestamp>.md
  with Timestamp / exact text / PostedAs.

## Structural checks
- Headings canonical `### Phase N — <Title>` (Phases 0-5).
- Task IDs sequential per phase.
- Bugfix ordering intact: Phase 2 (Red, fail-before incl. inverted existing test P2-T1
  [expect-fail], fail-before evidence P2-T4) precedes Phase 3 (Green fix P3-T1, pass-after
  P3-T3). All [expect-fail] tasks tagged.
- Evidence paths canonical (<FEATURE>/evidence/<kind>/); no artifacts/ evidence paths.
- Phase 0 policy reads in required order (CLAUDE.md, general-code-change.md,
  general-unit-test.md, csharp.md) + baseline artifacts with required fields; P0-T12
  numeric coverage baseline.
- Phase 4 CLAUDE.md 4-step toolchain (csharpier -> analyzers -> nullable -> vstest coverage)
  with restart-on-change loop; coverage thresholds new-code >= 90%, repo >= 80%.
