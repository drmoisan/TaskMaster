---
name: durable-feature-script-triggers-python-coverage-gate
description: A one-off audit/fix script copied into <FEATURE>/scripts/*.py per the atomic-planner's durable-script-copy convention still trips the mandatory per-language Python coverage gate
metadata:
  type: project
---

Issue #354 (stale-app-config-binding-redirects): the atomic-planner's `durable-script-copy-into-feature-folder` memory directs copying a proven-correct scratchpad script into `<FEATURE>/scripts/<name>.py` before referencing it in plan tasks, so it survives past the authoring session. That convention is sound for plan durability, but it means the script is a **permanently-committed** `.py` file in the branch diff, not a throwaway. `validate-feature-review-coverage.ps1`'s `Get-ChangedLanguageSet` matches any `.py` bullet line regardless of where it lives (feature folder vs `src/`), so it unconditionally requires a mandatory Python coverage row. In practice no `artifacts/python/lcov.info` exists for a single-script one-off tool, so the correct, honest verdict is **FAIL — coverage artifact absent**, carried into remediation-inputs, even when the script's actual behavior was independently re-verified via a standalone re-run and is functionally correct.

**Why:** The mandatory coverage rule in the feature-review SKILL has no carve-out for "tooling scripts committed to a feature folder rather than production `src/`." Treating it as exempt would be an unauthorized scope-narrowing.

**How to apply:** When a feature-folder diff includes a new `.py`/`.ps1`/`.ts`/`.cs` file under `scripts/` (not just under conventional production paths), still classify it as a changed-file language and require the coverage row. Independently verify the script's *correctness* (re-run it, diff its output, run Black/Ruff/Pyright directly) as strong corroborating evidence for the code-review, but do not let that substitute for the missing coverage artifact in the policy-audit verdict. See [[project_coverage-hook-label-substring-false-positive]] for the adjacent Test-LanguageCoverageRow line-matching mechanics used to write a compliant FAIL row.
