---
name: project-coverage-firstparty-denominator-method
description: How the #197 COM/VSTO coverage-exemption feature computes the "production-only first-party" coverage rate from the deduped Cobertura
metadata:
  type: project
---

The #197 coverage-exemption feature's "production-only first-party deduped" coverage rate
(the 59.03% baseline / 71.73% assembly-exclude / 71.65% class-level figures) is computed by
counting `<line>` elements across ALL `<package>` elements in the Koverage-deduped first-party
Cobertura, INCLUDING the two vendored packages `Swordfish.NET.General` and `SVGControl` (held
constant per design memo §2.6). The `<package>` elements carry only a `line-rate` attribute, not
`lines-valid`/`lines-covered` attributes, so per-line counting is the only reproducible method.

**Why:** matching this exact method is what reproduces the prior `coverage-delta.md` figure of
51,594 lines-valid / 37,010 covered / 71.73% from
`artifacts/csharp/coverage-firstparty.postexemption.cobertura.xml`. Excluding the two vendored
packages instead gives a different number (74.49% for the same artifact), so the
vendored-included convention is the authoritative one for this feature's deltas.

**How to apply:** when re-measuring or comparing coverage on the `feature/csharp-coverage-uplift`
branch, regenerate the deduped Cobertura via `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
(dotnet-coverage collect with `coverage.config` + inner vstest
`/Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation`, then Koverage post-process),
then sum `<line>` hits/totals across every package. The pipeline strips `.Test` packages
(Issue #193) automatically. See [[project_runsettings_datacollector_default_enabled]] for the
related runsettings/data-collector interaction.
