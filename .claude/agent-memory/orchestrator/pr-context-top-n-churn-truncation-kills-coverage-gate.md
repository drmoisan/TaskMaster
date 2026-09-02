---
name: pr-context-top-n-churn-truncation-kills-coverage-gate
description: The real mechanism by which the PR-context bundle disables C# coverage enforcement is a top-10-by-churn truncation, and a feature's own large evidence artifacts are what push the source files out of the list
metadata:
  type: project
---

Known previously as "the bundle classifies changed C# source as documentation". That is real, but it
is only one of TWO independently sufficient mechanisms, and the second one is the more insidious
because a well-run item causes it. Measured directly on issue #670, 2026-09-02.

**The gate.** `.claude/hooks/validate-feature-review-coverage.ps1`:

- `Get-ChangedLanguageSet` (around :121-138) derives the language set ONLY from lines of
  `artifacts/pr_context.summary.txt` matching `^\s*-\s+(\S+)\s+\(\+\d+/-\d+\)\s*$` — a path with a
  churn suffix — then switches on the extension (`.cs` -> CSharp, `.ps1|.psm1` -> PowerShell, etc).
- At :425-427, `if ($changedLanguages.Count -eq 0) { return @{ Ok = $true } }`. An empty set is a
  silent PASS. Nothing is printed.

**Mechanism 1 (known).** The bundle's `===== Changed files overview =====` reported
`Core logic changes: 0 files` and bucketed all 61 changed paths under
`Docs/templates/agents/tooling`, even though five were C# source including a new production file.

**Mechanism 2 (new, and self-inflicted).** That section enumerates only the TOP TEN paths BY CHURN.
On #670 the ten were, in order, `postchange.cobertura.xml` (+194037), `baseline.cobertura.xml`
(+193975), then `spec.md`, the research note, `policy-audit`, `plan`, `feature-audit`,
`code-review`, an analyzer-paths artifact, and `issue.md`. The largest C# change was 41 lines. So
even with correct classification, no `.cs` path could reach the parsed list.

**Why this matters.** The better your evidence discipline, the larger your committed Cobertura
documents, and the more certainly your own source files are truncated out of the only input the
coverage gate reads. The gate silently self-disables on exactly the items that generate the most
evidence.

**How to apply.** Never let the bundle tell you whether coverage enforcement ran. Derive coverage
yourself from the committed Cobertura documents with the per-`<line>` expression, and state the
figures in the checkpoint and the PR body. To check whether the gate was live on a given branch:

```
grep -c -E '^[[:space:]]*-[[:space:]]+[^[:space:]]+[[:space:]]+\(\+[0-9]+/-[0-9]+\)[[:space:]]*$' artifacts/pr_context.summary.txt
```

then confirm at least one matching line ends in a source extension. Zero source extensions means the
gate returned a vacuous pass.

Related: [[pr-context-summary-unreliable-gh-and-classification]],
[[csharp-coverage-denominator-two-figures]], [[preflight-catches-vacuous-gates]].
