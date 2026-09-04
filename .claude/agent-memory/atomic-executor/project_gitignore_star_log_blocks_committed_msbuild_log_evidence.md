---
name: gitignore-star-log-blocks-committed-msbuild-log-evidence
description: .gitignore:84 is *.log, so an evidence artifact named *.min.log is never staged by git add -A; a plan that designates it the retained AC evidence silently ships without it
metadata:
  type: project
---

A plan that writes an msbuild `/fl` file-logger artifact into `<FEATURE>/evidence/<kind>/` and calls it
the committed evidence for a non-vacuity acceptance criterion cannot deliver it if the file name ends in
`.log`. `.gitignore` line 84 is the bare pattern `*.log`, no negation anywhere in the file un-ignores it,
and `git add -A` skips it silently, so the delivery commit contains the `.md` summary and not the log it
cites.

**Do not assert "no `.log` is tracked in this repository" — that stopped being true on 2026-09-03.** Issue
#730 force-added four of them under `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/`, so
`git ls-files '*.log'` now returns four rows. They are tracked *despite* matching line 84, because once a
path is in the index the pattern no longer applies. Two consequences. First, a plan that recites the old
zero-row measurement as a re-derived fact is asserting something the tree contradicts. Second,
`git check-ignore -v <path>` returns **exit 1 (not ignored)** for those four, because check-ignore skips
indexed paths by default; only `git check-ignore -v --no-index <path>` reports `.gitignore:84:*.log`. Probe
with a path that does *not* exist and is *not* indexed if you want the pattern's own answer.

**Why:** the gate that would catch this normally reads "the minimal log exists at the evidence path named
in the command", which is a filesystem existence check. An ignored file exists on disk and is absent from
the commit, so that clause passes in exactly the state the criterion fails. The terminal
`git status --porcelain` clean-tree gate also passes, because ignored files never appear in porcelain.
Nothing in the plan's own gate set can observe the gap.

**How to apply:** during preflight, run `git check-ignore -v --no-index <path>` on every evidence artifact
extension a plan names, not just the ones that look risky. `.trx` and `.txt` under `docs/` are clear;
`.log` is not. The compliant fix is to rename the artifact so its final extension is not `.log` (for
example `.min.log.txt`, verified not ignored) — editing `.gitignore` is usually the wrong repair because
`.gitignore` is not in a ratified Write Set and adding a negation to it breaches the scope-containment AC.
Pair the rename with an acceptance clause that asserts the path is *tracked* (appears in `git ls-files` or
in the anchored `origin/main...HEAD` name-only diff), not merely that it exists.

**The tracked-status clause needs `git add -N` in front of it, or it is itself vacuous.** `git ls-files`
reads the index, and a freshly written file has no index entry whether or not it is ignored, so the clause
prints nothing for a *compliant* path too. Measured on git 2.53.0.windows.1 in a real worktree:
`git add -N <path matched by *.log>` exits **1**, prints "The following paths are ignored by one of your
.gitignore files", and creates no index entry, so the following `git ls-files --` prints nothing;
`git add -N <path ending .log.txt>` exits **0** and `git ls-files --` prints the path. `-N`
(`--intent-to-add`) does **not** bypass the ignore check — it takes the same `add_files()` path as a plain
`git add`, only `-f` suppresses it — so the un-forced add is a genuine discriminator that records the path
in the index without staging content. Assert `EXIT_CODE: 0` on the `add -N` step specifically, not just on
the task. Re-running it on an already-tracked or already-staged path is a no-op returning 0, so the gate
survives a QC-loop restart; a missing file makes it exit 128, which is also a correct failure.

Related: [[project_vstest_trx_evidence_needs_sanitisation_task]],
[[project_msbuild_log_token_search_matches_csc_command_line]],
[[project_preflight_checkoff_cites_later_task_artifact]].
