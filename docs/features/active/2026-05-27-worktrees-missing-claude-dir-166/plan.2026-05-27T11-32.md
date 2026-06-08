# worktrees-missing-claude-dir (Plan)

- **Issue:** #166
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-05-27T11-32
- **Status:** Ready
- **Version:** 1.0
- **Work Mode:** minor-audit (small-path)
- **Branch:** bug/worktrees-missing-claude-dir-166
- **Base:** development

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QA artifact tasks, and coverage-comparison tasks for each in-scope language when policy requires coverage. If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Record the expected artifact path or location in each evidence-producing task. Do not mark evidence-backed work complete without the artifact.

---

## Scope and Constraints

- **Single production file:** `.gitignore` (repository root) is the only code/config file changed by this plan.
- **Out of scope:** No edits to any `.claude/` file. Committing the now-tracked `.claude/` content is performed later by the orchestrator's pre-review `git add -A` step, not by this plan.
- **Issue #149 invariant (must preserve):** When `.claude/` becomes tracked, the following MUST remain git-ignored:
  - `.claude/settings.local.json` (per-developer local settings)
  - `.claude/agent-memory/` (per-agent learned state, not shared tooling)
- **Required `.gitignore` change:** Remove the bare `.claude` ignore entry (current final line) and replace it with targeted ignores that keep `.claude/settings.local.json` and the `.claude/agent-memory/` subtree ignored, while allowing the rest of `.claude/` (`agents/`, `hooks/`, `rules/`, `skills/`, `settings.json`) to be tracked.

## Testing and Verification Reality (documented exceptions)

- **No MSTest/unit regression test applies.** `.gitignore` behavior is a git-process property. A unit test would require invoking an external `git` process, which the repository General Unit Test Policy (UT4) and C# Unit Test Policy expressly prohibit (no external processes; no temporary files). The Phase 2 regression step is therefore adapted to a deterministic command-based verification using `git check-ignore`, not an MSTest test. This is a documented exception, not a skipped requirement.
- **No C# toolchain applies (documented N/A).** No `.cs`, `.csproj`, `.props`, or `.targets` file changes. CSharpier, msbuild analyzer/nullable builds, and vstest coverage steps are not applicable. This is recorded as a documented N/A in the QA phase, not a skipped requirement. Repository-wide C# coverage is unaffected because no production C# code changes.
- **Verification mechanism:** `git check-ignore <path>` exits 0 and prints a path when the path is ignored; it exits 1 and prints nothing when the path is not ignored.

### Expected verification results AFTER the fix

- `git check-ignore .claude/skills .claude/agents .claude/hooks .claude/rules .claude/settings.json` -> prints nothing (no longer ignored)
- `git check-ignore .claude/settings.local.json` -> prints `.claude/settings.local.json` (still ignored)
- `git check-ignore .claude/agent-memory/orchestrator/MEMORY.md` -> prints `.claude/agent-memory/orchestrator/MEMORY.md` (still ignored)

### Pre-fix state (defect proof, captured in Phase 2)

- `git check-ignore .claude/skills` -> prints `.claude/skills` (proves the directory is currently ignored)

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Confirm the active branch is `bug/worktrees-missing-claude-dir-166` (base `development`) by running `git rev-parse --abbrev-ref HEAD` and `git merge-base HEAD development`; write both values (branch name, current HEAD commit SHA, merge-base SHA) to `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/evidence/baselines/166-branch-commit-baseline.txt`.
- [x] [P0-T2] Read the repository policy files that govern this change and record their paths and the governing clauses in `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/evidence/baselines/166-policy-baseline.txt`: `c:\Users\DanMoisan\repos\TaskMaster\CLAUDE.md` (General Code Change Policy, General Unit Test Policy §UT4), `c:\Users\DanMoisan\repos\TaskMaster\.claude\rules\general-code-change.md`, `c:\Users\DanMoisan\repos\TaskMaster\.claude\rules\general-unit-test.md`.
- [x] [P0-T3] Capture the C# toolchain N/A determination: run `git diff --name-only development...HEAD` (and inspect the staged change set) to confirm no `*.cs`, `*.csproj`, `*.props`, or `*.targets` files are in scope; write the file list and the explicit "C# toolchain N/A — no C# build/test artifacts change" statement to `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/evidence/baselines/166-csharp-toolchain-na.txt`.

### Phase 1 — Scope Lock

- [x] [P1-T1] Confirm the only production file to be edited is `c:\Users\DanMoisan\repos\TaskMaster\.gitignore` and that no `.claude/` files will be edited by this plan; record the locked scope statement in `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/evidence/baselines/166-scope-lock.txt`.
- [x] [P1-T2] Read the current `c:\Users\DanMoisan\repos\TaskMaster\.gitignore` and confirm line 351 is the bare `.claude` entry to be replaced; record the exact current final-line text in `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/evidence/baselines/166-scope-lock.txt`.

### Phase 2 — Defect Verification (must fail first)

- [x] [P2-T1] [expect-fail] Run `git check-ignore .claude/skills .claude/agents .claude/hooks .claude/rules .claude/settings.json` against the unmodified `.gitignore` and confirm every listed path is printed (all currently ignored); save the verbatim command output to `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/evidence/regression/166-pre-fix-check-ignore.txt`. This is the deterministic command-based repro that replaces an MSTest regression test (documented exception: external git process prohibited in unit tests).
- [x] [P2-T2] [expect-fail] Run `git check-ignore .claude/settings.local.json .claude/agent-memory/orchestrator/MEMORY.md` and confirm both paths are printed (already ignored, as required by Issue #149); append the verbatim output to `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/evidence/regression/166-pre-fix-check-ignore.txt` to establish the pre-state for the invariant that must be preserved.

### Phase 3 — Minimal Fix

- [x] [P3-T1] In `c:\Users\DanMoisan\repos\TaskMaster\.gitignore`, replace the bare final-line `.claude` entry with the targeted block below; make no other edits to the file:

  ```gitignore
  # .claude/ agentic environment is tracked so it materializes in git worktrees (Issue #166).
  # Keep per-developer and per-agent state out of version control (Issue #149).
  .claude/settings.local.json
  .claude/agent-memory/
  ```

  Acceptance: the bare `.claude` line is gone, exactly the four lines above replace it, and the rest of the file is byte-identical to its prior content.

### Phase 4 — Verification Loop and QA Gate

- [x] [P4-T1] Run `git check-ignore .claude/skills .claude/agents .claude/hooks .claude/rules .claude/settings.json` and confirm it prints nothing and exits non-zero (no longer ignored); save the verbatim output and exit code to `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/evidence/qa/166-post-fix-check-ignore-allowed.txt`.
- [x] [P4-T2] Run `git check-ignore .claude/settings.local.json` and confirm it prints `.claude/settings.local.json`; run `git check-ignore .claude/agent-memory/orchestrator/MEMORY.md` and confirm it prints `.claude/agent-memory/orchestrator/MEMORY.md`; save both verbatim outputs to `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/evidence/qa/166-post-fix-check-ignore-still-ignored.txt`. This verifies the Issue #149 invariant is preserved.
- [x] [P4-T3] Confirm the now-tracked `.claude/` content is visible to git by running `git add -n .claude` (dry run) and confirming `.claude/agents/`, `.claude/hooks/`, `.claude/rules/`, `.claude/skills/`, and `.claude/settings.json` appear while `.claude/settings.local.json` and `.claude/agent-memory/` do not; save the verbatim dry-run output to `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/evidence/qa/166-git-add-dryrun.txt`.
- [x] [P4-T4] Record the QA toolchain determination for this change in `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/evidence/qa/166-toolchain-summary.txt`: (a) Formatter — N/A (no source files; `.gitignore` is not CSharpier/Prettier-formatted); (b) Linter — N/A (no analyzer-bearing source changes); (c) Type check — N/A (no C# build; documented in P0-T3); (d) Tests — adapted to `git check-ignore` command verification per the documented unit-test-policy exception; (e) Coverage — N/A (no production C# code changed, repository-wide coverage unaffected). State each as a documented N/A, not a skipped requirement.

### Phase 5 — Documentation & Status

- [x] [P5-T1] Update `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/issue.md` to record the implemented fix (targeted `.gitignore` ignores), the preserved Issue #149 invariant, and the documented MSTest/C#-toolchain N/A exceptions, with references to the Phase 2 and Phase 4 evidence artifact paths.

### Phase 6 — PR & Handoff

- [x] [P6-T1] Prepare PR notes for branch `bug/worktrees-missing-claude-dir-166` into base `development`: summary (remove bare `.claude` ignore; add targeted `.claude/settings.local.json` and `.claude/agent-memory/` ignores so the agentic environment is tracked and materializes in worktrees), risk (committing `.claude/` content occurs in the orchestrator pre-review `git add -A` step, not in this `.gitignore` change), and validation performed (pre/post `git check-ignore` evidence artifacts under `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/evidence/`). Record the PR note draft path in the PR context artifacts location.
