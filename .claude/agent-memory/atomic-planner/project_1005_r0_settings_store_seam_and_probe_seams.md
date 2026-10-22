---
name: project-1005-r0-settings-store-seam-and-probe-seams
description: #1005 R0 planning seams - SettingsSaving probe needs lock around add/remove, "<=500 and smaller" AC contradiction for already-oversized files, IDList null-ctor overload ambiguity, coverage script already applies parallel runsettings
metadata:
  type: project
---

Planned #1005 (parallel test failures: DfDeedle wall clock + shared user.config) on 2026-10-22.

- `ApplicationSettingsBase.SettingsSaving` has explicit add/remove over a plain delegate field (not atomic); concurrent probe tests on one `Settings.Default` must `lock (settings)` around `+=`/`-=` only, which does not serialize tests.
- An AC saying "every Write Set file <= 500 lines AND file X (already 848) is smaller" is unsatisfiable for X; the planner amended spec AC7 to an explicit exception rather than leaving the executor to interpret it.
- `new IDList(null)` is ambiguous once an `IDList(ISettingsStore)` overload exists; null-guard tests must cast `(ISettingsStore)null` or use the two-arg overload.
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` already applies `TaskMaster.cli.runsettings` (Workers 0, ClassLevel), writes `coverage.cobertura.jacoco.xml` and `mstest-coverage-run.summary.txt`, and retains the post-processed Cobertura only in `coverage/`; copy it to SCRATCH for a later baseline comparison.
- `docs/features/templates/policy_audit/` does not exist in TaskMaster; self-review tasks record the template as a deviation (same as #1002).
- A test helper not in the spec Write Set was added by amending the spec Write Set in the same planning pass, so the scope fence has one source.

**Why:** these were the non-obvious points found while re-deriving citations; each would otherwise surface as a preflight or review defect.
**How to apply:** reuse for any later settings-persistence or parallel-isolation plan in TaskMaster. Related: [[reference-invoke-mstest-with-coverage-script]], [[acceptance-criteria-are-amended-by-planners-not-executors]].
