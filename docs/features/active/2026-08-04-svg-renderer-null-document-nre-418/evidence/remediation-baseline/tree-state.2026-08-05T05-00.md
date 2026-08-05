# Tree State and Three Invariants — Remediation Cycle 2 Entry

- Task: `[P0-T5]`
- Timestamp: 2026-08-04T23-30
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`
- EXIT_CODE: 0 (all five commands returned 0)

## HEAD — recorded as an observation, not asserted

```
Command: git rev-parse HEAD
EXIT_CODE: 0
Output:  dc00cf1daab06b2e5d3a43881b41caded3dcfbf6
```

**This value is an observation only.** No task in this plan expects a particular HEAD SHA. Per this
plan's § Branch note, a SHA pin would rot on every commit that touches this plan file, so the gate is
the three invariants below instead. HEAD `dc00cf1d` is a descendant of `a62391f7`; the 14 commits'-worth
of paths that differ between them are enumerated under invariant (c) and are all Markdown.

## Invariant (a) — `git status --porcelain`

### Cycle-entry measurement (the invariant's subject)

Measured as the first command of this session, before any file in this cycle was written, during
`[P0-T1]`:

```
Command: git status --porcelain
EXIT_CODE: 0
Output:  (empty — zero lines)
```

**PASS.** The tree was clean at cycle entry. There is no carried-in permitted-dirt set and none was
needed: no other agent's file was present and therefore none was reverted or otherwise acted on.

### Re-measurement at the time of writing this artifact

```
Command: git status --porcelain
EXIT_CODE: 0
Output:
 M docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-plan.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/ac-source-check.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/cycle-inputs-read.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/phase0-instructions-read.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/toolchain-bootstrap.2026-08-05T05-00.md
```

Recorded for completeness and disclosed rather than suppressed. **Every one of these five paths is a
product of this cycle's own Phase 0 tasks**, not carried-in dirt:

| Path | Produced by | Authorized by |
|---|---|---|
| `remediation-plan.2026-08-05T05-00.md` (` M`) | checkbox check-offs for `[P0-T1]`..`[P0-T4]` | Scope Lock — "this file; checkbox state and preflight revision only" |
| `evidence/remediation-baseline/toolchain-bootstrap.2026-08-05T05-00.md` | `[P0-T1]` | Scope Lock — `evidence/**` |
| `evidence/remediation-baseline/phase0-instructions-read.2026-08-05T05-00.md` | `[P0-T2]` | Scope Lock — `evidence/**` |
| `evidence/remediation-baseline/ac-source-check.2026-08-05T05-00.md` | `[P0-T3]` | Scope Lock — `evidence/**` |
| `evidence/remediation-baseline/cycle-inputs-read.2026-08-05T05-00.md` | `[P0-T4]` | Scope Lock — `evidence/**` |

Zero `.cs`, `.csproj`, `packages.config`, or `app.config` path appears, and neither prior plan file
appears. The invariant's purpose — that this cycle does not begin on top of an unexplained modification
— is satisfied. `[P1-T7]` re-runs the same check after the Phase 1 edits with an explicit expected set.

## Invariant (b) — both prior plan files show an empty diff, and both are read-only this cycle

```
Command: git diff --stat HEAD -- docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md
EXIT_CODE: 0
Output:  (empty — zero lines)
```

```
Command: git diff --stat HEAD -- docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-plan.2026-08-05T01-50.md
EXIT_CODE: 0
Output:  (empty — zero lines)
```

**PASS, both.** `plan.2026-08-04T14-36.md` (complete at 46/46) and
`remediation-plan.2026-08-05T01-50.md` (complete at 40/40) are unmodified.

**Both files are read-only for the whole of this cycle.** No task in this plan may modify either. They
are cited only for reference: `plan.2026-08-04T14-36.md` for the `Svg` reference precedent at its
`[P1-T4]`, and `remediation-plan.2026-08-05T01-50.md` for its `evidence/qa-gates/*.2026-08-05T01-50.md`
series, which is this cycle's comparison basis. `[P2-T12]` re-confirms both diffs are empty at exit.

## Invariant (c) — the substantive gate: no source or build-configuration difference from `a62391f7`

```
Command: git diff --name-only a62391f7 HEAD
EXIT_CODE: 0
Output (14 paths):
.claude/agent-memory/atomic-executor/MEMORY.md
.claude/agent-memory/atomic-executor/project_418_plan_rationale_clauses_are_evidence.md
.claude/agent-memory/atomic-planner/MEMORY.md
.claude/agent-memory/atomic-planner/csharpierignore-scope-packages-config.md
.claude/agent-memory/atomic-planner/never-pin-head-sha-as-plan-expectation.md
.claude/agent-memory/atomic-planner/stale-build-output-is-not-evidence-of-existence.md
.claude/agent-memory/feature-review/MEMORY.md
.claude/agent-memory/feature-review/project_langversion-missing-test-projects-cs8630.md
.claude/agent-memory/feature-review/project_vstest-argument-order-transitive-dep.md
docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/code-review.2026-08-04T22-28.md
docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/feature-audit.2026-08-04T22-28.md
docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/policy-audit.2026-08-04T22-28.md
docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-inputs.2026-08-04T22-28.md
docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-plan.2026-08-05T05-00.md
```

Numeric verification rather than visual inspection:

```
Command: git diff --name-only a62391f7 HEAD | grep -E '(\.cs|\.csproj|packages\.config|app\.config)$' | wc -l
Output:  0
Command: git diff --name-only a62391f7 HEAD | wc -l
Output:  14
Command: git diff --name-only a62391f7 HEAD | sed 's/.*\.//' | sort | uniq -c
Output:  14 md
```

**PASS.** All 14 differing paths are `.md`: nine agent-memory files (three agents writing memory at
end-of-turn) and five feature-documentation files. **Zero** paths end `.cs`, `.csproj`,
`packages.config`, or `app.config`.

### Why this is the precondition Design Decision 5 depends on

The `evidence/qa-gates/*.2026-08-05T01-50.md` series was captured in and committed as `a62391f7` and
records the end state of that commit's source tree. Reuse of that series as this cycle's comparison
basis is valid for any HEAD whose source and build-configuration tree is identical to `a62391f7`'s.
Invariant (c) measures exactly that identity, and it holds at **0** differing source or
build-configuration paths. Documentation and agent-memory commits do not change the inputs to the
formatting, analyzer, nullable, or coverage gates, so those recorded figures are unaffected and are
directly comparable. `[P0-T10]` and `[P0-T11]` transcribe them on this basis.

Invariant (c) is the gate that fails exactly when it should: had any commit between `a62391f7` and the
executing HEAD touched a `.cs`, `.csproj`, `packages.config`, or `app.config` file, the reuse argument
would have failed and this plan would have required re-planning with a fresh full baseline rather than
patching. That condition did not arise.

## Verdict

All three invariants PASS. No halt condition fires. Execution may proceed to `[P0-T6]` and, in due
course, to `[P1-T1]`.

## Output Summary

HEAD observed at `dc00cf1daab06b2e5d3a43881b41caded3dcfbf6`. Invariant (a): `git status --porcelain`
was empty at cycle entry; the five paths present at the time of writing are all this cycle's own Phase 0
products and are disclosed with their authorizing Scope Lock clause. Invariant (b): the diff is empty
for both `plan.2026-08-04T14-36.md` and `remediation-plan.2026-08-05T01-50.md`, both of which are
read-only for this entire cycle. Invariant (c): `git diff --name-only a62391f7 HEAD` yields 14 paths,
all `.md`, with **0** matching `.cs`/`.csproj`/`packages.config`/`app.config`, so the source and
build-configuration tree is identical to the tree the `2026-08-05T01-50` evidence series was captured
against. The `2026-08-05T01-50` series is therefore a valid comparison basis for this cycle.
