---
name: poshqc-bundled-coverage-artifact-reads-zero
description: "run_poshqc_test writes artifacts/pester/powershell-coverage.xml with ZERO covered lines repo-wide (invalid capture); it is the hook's canonical PowerShell path and forces a FAIL row whenever PowerShell is enumerated"
metadata:
  type: project
---

The bundled MCP tool `mcp__drm-copilot__run_poshqc_test` writes producer output to `artifacts/pester/` (`pester-junit.xml`, `powershell-coverage.xml`, `powershell-coverage.koverage.xml`; gitignored via `.gitignore:57`). Observed in the #441 review (2026-08-10): the JaCoCo `powershell-coverage.xml` recorded **0 covered / 16075 missed lines for every file in the repo**, including a file whose committed direct-Invoke-Pester JaCoCo at the same head read 90.59%. The capture is an invalid instrument.

**Why:** `artifacts/pester/powershell-coverage.xml` is exactly the path `validate-feature-review-coverage.ps1` reads for PowerShell repo-wide coverage (`Get-JacocoRepoCoverage`). A 0% reading is below the 85% floor, so once PowerShell is enumerated from `pr_context.summary.txt` the hook REQUIRES a FAIL verdict on a PowerShell coverage row. Pester JaCoCo emits no BRANCH counters, so the branch check returns null and skips (no unconditional block, unlike the C# #328/#398 cases).

**How to apply:** In any TaskMaster PowerShell feature review: (1) check the artifact's actual counters before writing rows; (2) if it reads 0, write the repo-wide row as an honest FAIL against the artifact, disposition non-blocking as a pre-existing tool measurement defect, and adjudicate the real floors from the committed feature-evidence direct-Pester JaCoCo (changed-file, new-code, changed-line regression); (3) recommend filing a tooling follow-up. Executors document the MCP tool's payloads as "non-probative" — the direct `Invoke-Pester` runs are the probative evidence. Also note the #441 executor CORRECTION: an empty `git status --porcelain` does NOT prove the tool wrote nothing, because `artifacts/` is gitignored wholesale. Related: [[coverage-hook-skips-when-no-pr-context-summary]], [[project_stale-untracked-coverage-xml-leftover-false-block]].
