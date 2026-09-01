---
name: project-662-banner-prefix-revision-round-seams
description: Issue #662 round-2 revision seams — xml scope gate hits the plan's own cobertura evidence, post-format line numbers cannot be pinned, a delta's "first sentence" often means two, and test steps pulled into a restart loop need a baseline-relative failure definition
metadata:
  type: project
---

Applying the round-1 preflight delta to the issue #662 atomic plan surfaced five seams that generalize beyond this issue.

**1. A scope-boundary `git diff`/`git status` gate scoped by `'*.xml'` matches the plan's own coverage evidence.** The delta's P2-T23 asked for `git diff <base> --name-only -- '*.cs' '*.csproj' '*.props' '*.targets' '*.xml' 'packages.config'` and required the union with porcelain status to be exactly the four in-scope source files. But the same plan writes `coverage-baseline.cobertura.xml` and `coverage-postchange.cobertura.xml` under `<FEATURE>/evidence/`, and an earlier phase commits them, so the diff lists them and the union can never equal four. Fix: append `':(exclude)<feature-folder-path>'` to both pathspecs. The exclusion does not blind the check to a formatter rewrite, because `.csharpierignore:4` is `**/evidence/**`.

**Why:** the `*.xml` operand was added to catch a CSharpier 1.2.6 rewrite of a non-`.cs` file; nobody re-checked what else in the tree ends in `.xml` after the plan's own tasks run.

**How to apply:** whenever a plan gates its change set with a suffix pathspec, enumerate the files the plan itself creates with that suffix before writing the gate.

**2. Post-format line numbers cannot be pinned as literals when earlier tasks shift them.** The delta named `EfcSelectionGuard.cs:49`, `:75` and `FolderSuggestionTree.cs:197` as "post-format line numbers" for a changed-code coverage figure. Those are the *pre-change* numbers: P1-T4 replaces a one-line XML doc with a multi-line one above `:49`, P1-T6 deletes a declaration above `:197`, and CSharpier then wraps the rewritten reader. Fix: identify each changed statement by its enclosing member and the token it contains, instruct the executor to resolve the line number from the post-format file, and require the resolved numbers to be recorded. Pre-change numbers stay valid in the Phase 0 baseline task, which runs before any edit — say so explicitly so the two tasks are not conflated.

**3. A delta that says "replace the first sentence" often means the first two.** Twice the replacement prose subsumed the sentence after the one it named. Replacing only the literal first sentence left the plan carrying "If any of *the four* fails ... restart" immediately after "If any of *those eight* fails ... restart", and left a duplicated `git add`/`git status` sequence in two commit tasks. Read the replacement for what it subsumes, not only for what the instruction names, and report the reading in the handoff.

**4. Pulling test steps into a QC restart loop needs a per-step failure definition, and it must be baseline-relative.** The delta's Phase 2 preamble extended the restart loop to cover the two full-assembly test runs, but those tasks' acceptance was "records EXIT_CODE and transcribes counters", which always holds — so the loop could never restart. Adding "any failure restarts" would instead loop forever when the baseline already carries failures. The satisfiable form is: restart when `failed` **exceeds** the same assembly's Phase 0 baseline `failed`; when equal and non-zero, do not restart and let the AC task record `REMEDIATION-REQUIRED`.

**5. `TaskMaster.runsettings` at the repository root carries the Code Coverage collector; `scripts/vscode/TaskMaster.cli.runsettings` deliberately does not.** The CLI variant is MSTest `<Parallelize>` only, documented as such at `Invoke-MSTestWithCoverage.ps1:20-26`. Pair `/EnableCodeCoverage` only with the root file, or the run instruments `Deedle` and `FSharp.Core` (referenced by both `QuickFiler.Test/packages.config` and `UtilitiesCS.Test/packages.config`). A `vstest` run passing no `/EnableCodeCoverage` may keep the CLI variant — no collector is activated, so no exclusion list is needed.

Related: [[absolute-counts-in-shared-files-go-stale]], [[observation-scope-must-match-blast-radius]], [[reference_vstest_scoped_run_command]], [[project_501_r3_preflight_seams]], [[project_662_banner_prefix_arity_plan_seams]].
