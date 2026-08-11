---
name: project-457-closure-filter-plan-seams
description: Planning seams for issue #457 (ExcludeFromCodeCoverage nested lambdas) — raw-vs-post-processed Cobertura corpus, the in-place overwrite in Invoke-MSTestWithCoverage.ps1, and the pre-merge ordering constraint
metadata:
  type: project
---

Planning facts for the Cobertura closure-filter work (`scripts/vscode/Invoke-MSTestWithCoverage*.ps1`), verified 2026-08-10 while planning issue #457.

**`Invoke-MSTestWithCoverageMain` overwrites its own raw collector output in place.** It collects to `$resolvedOutputPath`, then reads that file and `Set-Content`s the post-processed result back to the same path. There is no hook between collection and post-processing, so a plan task cannot obtain a fresh raw Cobertura artifact from a normal pipeline run without a production change.

**A post-processed artifact cannot answer any question about compiler-generated class names.** `Merge-CoberturaClassesByFilename` groups `<class>` by `filename` and keeps only the primary (first name without `<`), so `Type.<Member>d__<N>` state machines and `<>c__DisplayClass` closures are collapsed and their names disappear. Any probe about `d__` / `b__` naming must read a *raw* corpus (absolute `filename` attributes, closure classes still siblings). The known committed raw corpus is `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml`.

**Attributed async members do exist in-repo**, so the "does the collector emit a `d__` class for an `[ExcludeFromCodeCoverage] async` member" probe is answerable without a scratch C# build — e.g. `UtilitiesCS/Extensions/AsyncSerialization.cs`, `TaskVisualization/AutoCreateProject.cs`, `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs`.

**Why:** Planning a probe against the wrong corpus produces a task that cannot be completed, and a plan that assumes the raw artifact survives the run is wrong about the script's own control flow.

**How to apply:** When a plan needs closure/state-machine class names as evidence, route the task to a raw corpus explicitly and record `Corpus:` in the artifact. When a plan inserts a transform into `ConvertTo-KoverageCoberturaXml`, place it after the `//class[@filename]` normalization loop and before `Merge-CoberturaClassesByFilename` — after the merge the required linkage no longer exists, so it is a correctness constraint, not an ordering preference. `Invoke-MSTestWithCoverage.Helpers.ps1` was 357 lines pre-#441, which is why #457 adds a separate `Invoke-MSTestWithCoverage.ClosureFilter.ps1` rather than growing it. See [[evidence-path-normalization]] and [[reference_invoke_mstest_with_coverage_script]].
