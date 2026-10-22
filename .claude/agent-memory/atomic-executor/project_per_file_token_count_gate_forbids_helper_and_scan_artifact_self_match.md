---
name: per-file-token-count-gate-forbids-helper-and-scan-artifact-self-match
description: Exact per-file token-count gates (e.g. "_settingsStore.Save() = 9 in file X") forbid extracting the repeated statement into a helper; an evidence artifact that quotes its own forbidden-token search pattern makes the scan self-match
metadata:
  type: project
---

Two gate-shape traps seen while executing issue #1005 (settings-store seam).

1. A plan's after-search asserted an exact per-file count of a statement token (Q2: `_settingsStore.Save()` 9 in AppStagingFilenames.cs, matching "each saving setter followed by `_settingsStore.Save()`"). Extracting the write+save into a private `Persist` helper is cleaner, but it collapses the count to 1 and fails the gate. Before refactoring repeated statements, grep the plan for count assertions on that token and keep the inlined form. The same applies to literal argument forms: a gate that asserts `timeProvider: new FakeTimeProvider()` fails if the value is first assigned to a local.

2. A "no host path in evidence" scan (drive letter, Users segment, account name, AppData) matched its own artifact, because the artifact's Command field quoted the regex, which contains the account name. Describe the pattern in words in the artifact and re-run the scan after writing it.

**Why:** both defects pass every build and test and surface only when the textual gate runs, so they cost a rewrite late in the plan.

**How to apply:** before writing implementation, read the plan's search-definition section for per-file counts and literal forms. Write scan artifacts without quoting the forbidden tokens, then re-run the scan over the evidence tree once the artifact exists. Related: [[project_banned_api_zero_hit_gate_hits_doc_comments]].
