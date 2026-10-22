# P0-T2 Base git anchor

Timestamp: 2026-10-02T03-08
Command: git -C <execution-worktree-root> fetch origin main; git -C <execution-worktree-root> merge-base HEAD origin/main; git -C <execution-worktree-root> rev-parse HEAD; git -C <execution-worktree-root> cat-file -t <BASE_SHA>; git -C <execution-worktree-root> status --porcelain --untracked-files=all; git -C <execution-worktree-root> diff --name-only <BASE_SHA> -- .
EXIT_CODE: 0

FETCH-EXIT: 0
BASE_SHA: 860d67bf4fddecb929e0d6c166065fd1ee752feb
P0-START: 66bc53ee1f37118ebc0ffeb79d23292606cabb35
CAT-FILE-TYPE: commit
ORIGIN-MAIN-TIP: 860d67bf4fddecb929e0d6c166065fd1ee752feb (equals BASE_SHA, as the orchestrator stated after the origin/main merge into the branch)

BASE-PORCELAIN (verbatim; taken after P0-T1 had written its artifact and flipped its plan checkbox, so both entries are P0-T1 outputs):

```text
 M docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/plan.2026-10-02T00-16.md
?? docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/baseline/phase0-instructions-read.2026-10-02T03-08.md
```

INHERITED (verbatim):

```text
.claude/agent-memory/atomic-planner/MEMORY.md
.claude/agent-memory/atomic-planner/project_953_fizzler_redirect_sweep_and_ratchet_plan_seams.md
.claude/agent-memory/atomic-planner/project_953_r1_pwsh_refused_and_count_recheck_seams.md
.claude/agent-memory/atomic-planner/project_953_r2_grep_long_line_omission_and_cr_anchor_seams.md
.claude/agent-memory/atomic-planner/project_953_r3_grep_gitignore_directory_path_and_measured_line_lengths.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/evidence/other/preflight-clearance.2026-10-02T03-45.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/issue.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/plan.2026-10-02T00-16.md
docs/features/active/2026-08-04-stale-fizzler-and-unsafe-binding-redirects-953/research/2026-10-02T00-35-fizzler-redirect-remedy-and-redirect-gate-research.md
docs/features/potential/promoted/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md
```

Git emitted one advisory on the diff command: the working copy of the plan file will have LF replaced by CRLF the next time Git touches it (autocrlf advisory; not an error).

Acceptance: BASE_SHA is 40 hexadecimal characters; `git cat-file -t` prints `commit`; no path in BASE-PORCELAIN or INHERITED ends in `.cs`, `.csproj`, `packages.config` or `app.config`, and none lies under `scripts/dependencies/` or `tests/scripts/dependencies/`. The feature-folder paths and `.claude/agent-memory/` paths are recorded, not asserted empty.

Output Summary: BASE_SHA 860d67bf4fddecb929e0d6c166065fd1ee752feb equals the origin/main tip. HEAD 66bc53ee1f37118ebc0ffeb79d23292606cabb35. Write Set is clean at start; INHERITED lists 10 paths (agent-memory and feature-folder documents only).
