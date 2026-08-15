---
name: epic-fanin-artifact-path-and-hook-regex
description: Epic fan-in reviews must ALSO write audits to a docs/features/active/ folder (hook regex rejects docs/features/epics/); plus UNVERIFIED is a narrowing word and "Pester" alone satisfies both hook tests
metadata:
  type: project
---

Two hard constraints in `.claude/hooks/validate-feature-review-coverage.ps1` that bit during the
`build-ci-coverage-gate-fidelity` epic fan-in review (2026-08-15).

**1. Artifact paths must live under `docs/features/active/`.** `Get-ReviewArtifactInfo` (line 96)
matches `^docs/features/active/(?<Folder>.+)/<stem>\.(?<Timestamp>\d{4}-\d{2}-\d{2}T\d{2}-\d{2})\.md$`.
A caller that directs an epic review to write into `docs/features/epics/<epic>/` produces artifacts
the hook rejects as "outside the required location", blocking termination.

**Why:** the regex hard-codes `active`; there is no epic branch in it.

**How to apply:** honor the caller's epic-folder request, then ALSO copy the three files into one of
the epic's child folders under `docs/features/active/` and advertise THOSE paths in the
`*-path:` tokens. All three advertised paths must share the same folder AND the same timestamp.

**2. The narrowing regex includes `\bUNVERIFIED\b` and `\bN/A\b`.** Full pattern:
`(informational only|context only|out of plan scope|out of scope|not applicable|\bN/A\b|\bUNVERIFIED\b)`.
Worse, for PowerShell the label list is `('PowerShell','powershell','pester')` and the
coverage-keyword list is `(coverage|lcov|line[s]?\s+hit|pester)` — so **any line containing "Pester"
satisfies BOTH tests by itself** and becomes a coverage row.

**Why:** a legitimate "branch coverage is UNVERIFIED because Pester 5.6.1 has no branch counter"
sentence trips a false FAIL, even though recording UNVERIFIED with a reason is what the review
contract demands.

**How to apply:** split the disclosure across two lines so the word `UNVERIFIED` never shares a
physical line with a language label plus a coverage keyword. Then simulate before finalizing:
set `$env:CLAUDE_HOOK_INPUT = (@{output=$text} | ConvertTo-Json)` and invoke the hook script
directly — dot-sourcing does not work (the script returns early when `$MyInvocation.InvocationName
-eq '.'`, and `-SourceOnly` is not a real parameter).

Related: [[project_coverage-hook-label-substring-false-positive]],
[[project_coverage-hook-label-plus-verdict-same-line-507]],
[[project_build-ci-coverage-gate-fidelity-epic-outcome]]
