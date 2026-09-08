---
name: get-blastradius-overincludes-citations-omits-gitignored-writes
description: Get-BlastRadius extracts every path the plan MENTIONS, so it pulls in read-only citations and explicit non-goals as modules, and it omits gitignored working files the plan will actually write
metadata:
  type: project
---

`Get-BlastRadius` derives its `paths` from plan and spec TEXT, so it cannot distinguish a path
the plan will write from a path the plan cites. Verified 2026-09-08 on issue #810 preparation.

**It over-includes, in two ways that matter for cohort scheduling.**

- *Explicit non-goals become modules.* The #810 plan names `TaskMaster/ThisAddIn.cs` and
  `UtilitiesCS/Threading/UiThread.cs` only to forbid editing them (they were owned by a
  concurrent run on #809), and cites `UtilitiesCS/Threading/ProgressViewer.cs` as a report-only
  finding. The derived `modules` came back `["QuickFiler", "QuickFiler.Test", "TaskMaster",
  "UtilitiesCS"]`. The two extra modules exist **because** the plan promised not to touch them.
  A cohort computation fed that result serializes the item against every TaskMaster and
  UtilitiesCS item for no reason.
- *Extraction artifacts.* The output also carried line-suffixed pseudo-paths
  (`QuickFiler/Controllers/QfcHomeController.cs:389`), a path that does not exist on disk
  (`.config/dotnet-tools.json`, cited by the plan as the file that is NOT the manifest),
  relative fragments missing the feature-folder prefix (`evidence/baseline/p0-t2-...md`,
  `research/research...md`), and wildcard entries (`**/coverage*.xml`, `<feature-folder>/**`).

**It omits gitignored working files the plan does write.** `coverage/810-effective-coverage.config`
and `coverage/810-baseline.cobertura.xml` were both named in the plan's Write Set and both absent
from the derived `paths`, while the sibling `coverage/810-post.cobertura.xml` was present. The
`TestResults/810-*` directories were absent too.

**Why:** the extractor is a text scanner over prose. Presence in the text is the only signal it
has, and the plan's most careful prose — the non-goals list, the "this is not the manifest"
correction, the residual-candidate discussion — is exactly the prose that names paths the plan
will never write.

**How to apply:** run the derivation (a caller may require it, and it is a useful superset), then
reconcile in BOTH directions before reporting. Append every exact path the plan names that the
derivation missed, and state plainly which derived entries are citations rather than writes —
name the mechanism, do not silently drop them, because a caller instruction to "never
re-normalize the widened result" means the derived set is meant to survive. Report the literal
write-set enumeration alongside the derived radius so no path is represented only by a wildcard.
Flag an over-broad `modules` value explicitly: see
[[project_blast_radius_module_map_caps_parallelism]] for how assembly-level modules already cap
parallelism, and [[feedback_project_file_overlap_is_not_contention]] for the same class of false
contention.

Running it at all requires a delegated subagent when you are worktree-isolated; see
[[worktree-isolation-blocks-pwsh-per-agent-type]].
