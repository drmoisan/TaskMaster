---
name: epic-child-nullable-fanin-debt-deferred
description: In the utilitiescs-nullable-remediation epic, the per-child #nullable gate is scoped to the child's own branch/files; cross-child CS86xx fan-in debt accumulates on the integration branch and is deferred to the Wave-2 CI capstone — do not over-remediate into sibling files
metadata:
  type: project
---

For the `utilitiescs-nullable-remediation` epic (per-file `#nullable enable` opt-in), a child's AC1 nullable gate is verified on the child's OWN branch over the child's OWN in-scope files, BEFORE fan-in. After merging the integration branch into the child branch, the combined UtilitiesCS project emits NEW cross-child CS86xx — sibling `#nullable enable` annotations on shared helpers propagate into your pragma-enabled files and vice versa. This is expected fan-in debt, not a defect in the child's deliverable.

**Why:** #372 (email-classifier) merged clean on its own branch (executor final-nullable-pragma-gate.md = 0 CS86xx). After fan-in the merged branch showed 76 CS86xx; the integration tip aa154796 ALONE already carried 15 CS86xx (in sibling-owned EmailParsingSorting/SubjectMap/People files outside #372 scope), proving prior siblings (#374/#385/#371/#375-residuals) all merged despite cross-child fan-in debt. The plan's Open Questions note explicitly defers the global-flag-vs-per-file-pragma conflict to the Wave-2 CI capstone.

**How to apply:** For an epic-child orchestrator, the merge criterion is feature-review blocking_count==0 + CLEAN/MERGEABLE PR (child->integration PRs get zero CI). Do NOT run a whole-project post-merge nullable gate as a blocking check and do NOT remediate cross-child propagation errors — that expands scope into sibling-owned files (scope-lock violation) and is structurally the Wave-2 capstone's job. Record the measured cross-child debt (integration-tip-alone count and merged count) in the checkpoint under a `cross_child_nullable_fanin_debt` block and report it to the epic-orchestrator/capstone. Scoped verification gate that surfaces it: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168`. See also [[parallel-epic-children-name-collisions]] (the CS0101/CS0104 analogue) and [[epic-child-pr-gate-gotchas]].
