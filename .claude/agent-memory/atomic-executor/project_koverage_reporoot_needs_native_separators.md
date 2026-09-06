---
name: koverage-reporoot-needs-native-separators
description: ConvertTo-KoverageCoberturaXml -RepoRoot strips prefixes by literal StartsWith, so a forward-slash root silently leaves every Cobertura filename absolute and the document stays effectively raw
metadata:
  type: project
---

`ConvertTo-KoverageCoberturaXml -RepoRoot <path>` delegates to `ConvertTo-KoverageRelativePath`, which strips a prefix by **literal `StartsWith`** against `"<root>\"` and `"<root>/"` (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:67-92`). Cobertura `class/@filename` values on Windows use backslashes. So passing `-RepoRoot` as `C:/Users/.../worktree` (forward slashes) matches nothing: **every filename stays host-absolute and the post-processed document is indistinguishable from a raw one**, while the call still succeeds and returns a document.

**Why:** on issue #731 `[P5-T5]` this produced a processed document whose residual absolute-filename count was 563 of 563 classes. The plan's `Cobertura document state:` audit line — value `processed` when no selected `class/@filename` begins with a drive-letter prefix — is what caught it; without that audit line the run would have proceeded on a document that was silently unrelativised. Rates and per-line maps were unaffected (only the relativisation is a no-op; package filtering, closure exemption, merging and rate recomputation all still ran), but the state assertion would have read `raw` and blocked the plan.

**How to apply:** always pass `-RepoRoot (Resolve-Path -LiteralPath '.').Path`, which returns the native backslash form, never a forward-slash spelling of the same path. Recovery is cheap and needs no test re-run: step 9 of the coverage procedure is a pure transformation of the on-disk **raw** document, so re-read the raw file, re-call `ConvertTo-KoverageCoberturaXml` with the native root, and overwrite only the processed path. Keep an audit line that counts drive-letter-prefixed filenames in the selected classes — it is the only cheap signal that distinguishes a genuinely processed document from this failure.

Related: [[koverage-cobertura-postprocessing-shape]], [[csharp-canonical-coverage-artifact-conversion]].
