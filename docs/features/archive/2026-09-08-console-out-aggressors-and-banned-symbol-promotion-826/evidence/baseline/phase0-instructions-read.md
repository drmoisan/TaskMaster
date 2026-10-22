# Phase 0 — Policy and context instructions read (issue #826, [P0-T1])

Timestamp: 2026-09-09T19-00

Policy Order: the repository policy reading order defined by `.claude/skills/policy-compliance-order/SKILL.md`
was applied — `CLAUDE.md` first, then the cross-language code-change policy, then the cross-language
unit-test policy, then the tier system, then tonality, then the language-specific C# rule file. The
feature's own requirement documents were read after the policy set, and the epic manifest last. The
nine paths read, in that order, are the numbered list immediately below; it is the content of this
`Policy Order:` field.

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/quality-tiers.md`
5. `.claude/rules/tonality.md`
6. `.claude/rules/csharp.md`
7. `docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/spec.md` (read in full)
8. `docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/research/console-out-and-banned-symbol-residuals.2026-09-08T23-58.md` (read in full)
9. `docs/features/epics/review-residuals-2026-09-08/epic.md` (read only; this plan performs no edit to it)

## Binding constraints extracted from the read set

- C# toolchain order is format, analyzer build, nullable build, test; any failure or auto-fix restarts
  the loop from step 1.
- `/t:Rebuild` is mandatory on both msbuild gates; a warm `/t:Build` skips `CoreCompile` and cannot fail.
- `/p:Nullable=enable` is never added to the nullable gate.
- No policy document under `.claude/rules/` or `.github/instructions/` may be modified.
- No coverage threshold, analyzer severity or policy requirement may be lowered to make a gate pass, and
  no production file may be added to a coverage exclusion list.
- Acceptance criteria for this `full-bug` feature come from `spec.md` only; no `user-story.md` exists and
  none may be created.

EXIT_CODE: 0

Output Summary: All nine documents were read in the stated order. No conflicting instruction was found
between the policy set, the feature spec, the research record and the epic manifest. Execution proceeds
to [P0-T2].
