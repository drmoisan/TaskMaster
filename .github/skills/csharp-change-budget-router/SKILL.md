---
name: csharp-change-budget-router
description: Budget-first routing contract for C# work: estimate production-file scope, choose small vs large path, and enforce orchestration-first routing for larger changes.
---

# C# Change Budget Router

Canonical guidance for deciding whether work should stay on the small path or escalate to the full C# orchestration workflow.

## When to Use This Skill

Use this skill when:
- Intake starts from a natural-language C# request.
- An agent must decide the execution path before planning or implementation.
- A direct-mode route must reject over-budget requests and switch to orchestrated flow.

## Canonical Routing Rules

1) Estimate rough change budget first based on likely **production C# files** touched.
2) Route:
- `1-3` production files (+ corresponding tests) → **small path** (atomic plan + execution route).
- `>3` production files or `>3` test files → **large path** (orchestration workflow with promotion/research/spec/planning/execution/review).

## Direct-Mode Rejection Rule

If direct implementation is requested but estimated scope is `>3` production files:
- Stop before implementation.
- Return explicit routing instruction to invoke `csharp-orchestrator`.

## Documentation Expectations

Record in response/logs:
- estimated production file count,
- chosen path (`small`/`large`),
- rationale summary (1-3 bullets).
