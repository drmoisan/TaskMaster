---
name: project-950-hygiene-blocks-absolute-prefix-and-always-failing-probe-seams
description: "#950 plan v1.0 (no-shell planning session): a caller demand to record the absolute worktree prefix in Command rows collides with the CI hygiene rule B (drive:\\users\\ pattern, scripts/hygiene/Test-RepositoryHygiene.Rules.ps1:21) because plan and evidence are committed; bash cannot carry a single-quoted prefix inside a single-quoted -Command; an always-failing probe gives an ambient-value observation a deterministic ExpectedExitCode; a commit task's own artifact is untracked in that task's porcelain gate; a using-wrap re-indent adds lines that defeat added-line token scans"
metadata:
  type: project
---

Seams from authoring plan 950 (QuickFiler wall-clock waits + R4 raced static), worktree agent-a7805823735145ca4, 2026-10-01.

**Absolute prefix vs hygiene rule B.** The orchestrator asked every artifact's `Command:` row to record the full payload including `Set-Location -LiteralPath '<absolute worktree path>'`. That path contains the profile directory, and the CI hygiene guard rejects any committed line matching `[a-z]:[\\/]+users[\\/]+[a-z0-9_.~-]` (case-insensitive). Plan text is committed too, so the plan itself must never contain the path. Adopted: `WORKTREE` token in plan and artifacts, executor substitutes at run time, and every payload prints `WORKTREE-LEAF: <leaf>` as the falsifiable proof the prefix took effect. Record the adjustment in a "Caller instructions applied with a recorded adjustment" section so it does not read as an omission.

**Quote character.** Payloads with `$` go in outer single quotes through the Bash tool, which cannot embed a single quote; write the prefix with double quotes and say why.

**Ambient-value observation (AC16 SynchronizationContext).** A temporary probe test that ALWAYS fails (`report.Should().Be("PROBE-ALWAYS-FAILS", report)` with the observed value inside `report`) makes ExpectedExitCode deterministic (1) and carries the value in the trx message; a probe that passes-or-fails depending on the value leaves the exit code unknowable at authoring time.

**Commit-task porcelain gate.** The artifact a commit task writes after its commit is untracked, so "no porcelain path under FEATURE except the plan file" fails on its own artifact. Admit the artifact by name in that task.

**Re-indent and added-line scans.** Wrapping R4's body in `using (...) { }` re-indents ~55 lines, so they all appear as added lines in the anchored diff; an added-line scan for `.Wait(` would trip on the untouched `secondCallerStarted.Wait();`. Keep added-line scans to tokens absent from re-indented code (`Thread.Sleep`, `Task.Delay`, `DoNotParallelize`, `Retry(`, `Timeout(` — the `[Timeout]` attribute line sits outside the body) and add a positive-control token the change genuinely adds.

**How to apply:** for any plan whose caller asks for absolute paths in committed evidence, substitute a token plus a leaf observation and cite the hygiene rule; for any "record the observed X once" criterion, use an always-failing probe; for every commit task, admit its own post-commit artifact in the porcelain clause. Related: [[_shared_no_absolute_host_paths]], [[project-927-r9-pwsh-process-directory-and-backslash-collapse-seams]], [[empty-porcelain-clause-is-unsatisfiable]].
