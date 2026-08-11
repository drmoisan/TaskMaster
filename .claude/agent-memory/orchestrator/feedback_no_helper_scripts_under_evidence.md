---
name: no-helper-scripts-under-evidence
description: Tell executors never to commit a helper .ps1/.py/.ts script under a feature's evidence/ tree; feature-review's language-set match is extension-only and path-blind, so it forces a mandatory-coverage FAIL
metadata:
  type: feedback
---

Instruct every `atomic-executor` delegation to capture a helper script's OUTPUT into the evidence `.md` artifact and NOT to commit the script file itself anywhere under `<FEATURE>/evidence/`.

**Why:** On #394 (utilitiescs-test-cs2002-duplicate-compile-entry) the executor retained a 27-line `duplicate-sweep.ps1` under `<FEATURE>/evidence/baseline/` "for reproducibility". `feature-review`'s `Get-ChangedLanguageSet` matches on file extension alone and is completely path-blind — it does not care that the file sits under a docs/evidence tree rather than production source. One retained `.ps1` therefore put PowerShell into the branch's changed-language set, which triggers the mandatory coverage-verification rule, which has no artifact and no Pester test behind it. Result: a FAIL and a full remediation cycle (planner + executor + reaudit) on a branch whose only real change was deleting one line from a `.csproj`. The plan had only ever required capturing the script's output.

**How to apply:** Put the prohibition in the delegation prompt, not just in the plan. When a plan task needs a non-trivial shell/XML/parsing step, the task should say "run the command and record its command text and complete output in the evidence artifact" — never "write a script and commit it". If a helper script has already been committed, the proportionate remediation is `git rm` it (the sibling `.md` artifact already carries its logic and output verbatim), not building out Pester/PoshQC/coverage tooling for a one-off audit helper. Applies equally to `.py`, `.ts`, `.psm1`, `.psd1`. See [[feature-review-coverage-85-floor-trap]] for the adjacent coverage-gate trap.
