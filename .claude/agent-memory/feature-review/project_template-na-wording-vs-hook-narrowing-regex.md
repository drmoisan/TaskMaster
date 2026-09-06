---
name: template-na-wording-vs-hook-narrowing-regex
description: The canonical policy-audit template mandates `N/A - out of scope` on Coverage Evidence Checklist lines, which is exactly the phrase the SubagentStop hook rejects as scope narrowing — safe only for languages with ZERO changed files
metadata:
  type: project
---

Two TaskMaster gates give directly opposing instructions about the same text, and both are live.

- The canonical policy-audit template's **Coverage Evidence Checklist** requires the lines
  `TypeScript baseline coverage artifact:`, `TypeScript post-change coverage artifact:`,
  `PowerShell baseline coverage artifact:`, `PowerShell post-change coverage artifact:` (plus C#
  and Python equivalents) and spells the out-of-scope value as `[path or N/A - out of scope]`.
- `.claude/hooks/validate-feature-review-coverage.ps1` rejects any line that carries a language
  label AND a coverage keyword AND any of `informational only|context only|out of plan scope|out
  of scope|not applicable|\bN/A\b|\bUNVERIFIED\b` — but **only for languages present in
  `changedLanguages`**, derived from `artifacts/pr_context.summary.txt`.

**Why they coexist without conflict, usually:** the template wording is only used for languages
with zero changed files, and the hook only polices languages with changed files. The two sets are
disjoint, so a checklist line reading `PowerShell baseline coverage artifact: N/A - out of scope`
is safe precisely when it is true.

**The trap:** on a branch that changes BOTH C# and PowerShell files, you may NOT write
`PowerShell baseline coverage artifact: N/A - out of scope`. The hook will block termination. Give
a real artifact path, or non-narrowing wording such as
`artifacts/pester/powershell-coverage.xml (absent; recorded FAIL in section 5.2)`. The same holds
for any language whose files appear in the diff.

**How to apply:** after drafting, simulate rather than reason about it. Dot-source the hook and run
`Test-LanguageCoverageRow` once per language with the language forced in as changed. On #781 that
stress run correctly returned `Ok=False` for PowerShell/Python/TypeScript on exactly these template
lines while the real invocation returned `Ok=True`, which is the expected signal — not a defect —
because those three languages had zero changed files. Confirm the real
`Invoke-FeatureReviewCoverageValidation` returns `Ok=True` and read any forced-run failure as a
"this would break if that language were in scope" warning.

Related: [[taskmaster-validator-memories-are-cross-repo]] (the template structure is mandatory
because the orchestrator runs the validator), [[policy-audit-required-structure]],
[[coverage-hook-label-substring-false-positive]].
