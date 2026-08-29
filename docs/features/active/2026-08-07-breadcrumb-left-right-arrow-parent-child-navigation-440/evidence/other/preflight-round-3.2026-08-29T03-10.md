# Preflight Round 3 — Issue #440

Timestamp: 2026-08-29T03-10
Reviewer: atomic-executor under `DIRECTIVE: PREFLIGHT VALIDATION ONLY`
Plan under review: `plan.2026-08-29T00-22.md` in this feature folder
Signal: `PREFLIGHT: REVISIONS REQUIRED`
Convergence: `CONVERGENCE: FURTHER ROUNDS LIKELY`

The reviewer executed no plan task and modified no file. All 55 tasks remain unchecked.

## Confirmations

Round-2 defects A, B, C, D and E are confirmed genuinely closed in the plan text. The seven latent sibling invalidations the planner found and fixed on its own initiative in the round-2 revision are each confirmed correct and complete, including the rewiring of the seven Phase 5 check-offs, each of which now names a span that exists and can produce the evidence it claims.

The reviewer additionally verified defect A's literal against a **successful** run rather than only the failed log the orchestrator used. In `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/regression-testing/p3-t10-greenflip-build.msbuild.txt`, which records 0 errors and 5 warnings, `(Rebuild target(s))` occurs 40 times, the first at line 4, and `Skipping target "CoreCompile"` occurs 0 times. The short literal `(Rebuild target)` occurs 10 times, all after `Build succeeded.` at line 10636. The plan's location claim is exactly right, and no count for the short literal is asserted anywhere in the plan, so the earlier 12-versus-40 measurement discrepancy is not load-bearing.

Also re-derived and confirmed: the three file line counts (248 / 235 / 495); every literal-count precondition at exactly 1; the two-red-tests conclusion by an exhaustive sweep; the expect-fail trace; the P3-T2 and P3-T3 test-count floors; all ten named results and all fourteen filter class names; the wrapper-script line citations; and scope containment at exactly three backticked source files.

## Defects requiring revision

### 1. P0-T7 cannot detect the live analyzer version skew, and P0-T11 and P0-T12 are unsatisfiable on this worktree (blocking)

P0-T7 checks for the presence of a `Meziantou.Analyzer.*` directory and a `Roslynator.Analyzers.*` directory. That glob is version-agnostic, and the failure mode present at BASE is version skew, which the glob cannot see.

**The orchestrator verified this independently and the measurements below are its own.** A NuGet bump moved `packages.config` and the `Import` and `Error Condition` lines without moving the hand-written `Analyzer Include` paths:

- `packages.config` pins `Meziantou.Analyzer` version `3.0.174` and `Roslynator.Analyzers` version `4.16.1`. Confirmed by reading `UtilitiesCS/packages.config` lines 17 to 22 and 107 to 112.
- Across all project files, `Analyzer Include` items reference `Meziantou.Analyzer.3.0.156` in 16 items and `Roslynator.Analyzers.4.16.0` in 64 items. Confirmed at `UtilitiesCS/UtilitiesCS.csproj` lines 1303 to 1307. The 64 project-file references to `Meziantou.Analyzer.3.0.174` are the `Import` and `Error Condition` restore-check lines, which were bumped.
- Neither the repository-root `packages` directory nor `.dotnet-sdk` exists in this worktree.

Because a restore installs only the `packages.config` versions, the directories `packages\Meziantou.Analyzer.3.0.156` and `packages\Roslynator.Analyzers.4.16.0` will not exist, and a fresh-worktree `/t:Rebuild` fails with `error CS0006: Metadata file could not be found`. P0-T7's acceptance would pass while the build is broken; P0-T11 then cannot meet its `EXIT_CODE: 0` acceptance and the phase halts by its own text. Because `/t:Rebuild` cleans the output directories before failing, P0-T12, P0-T13 and P0-T14 are equally unreachable. The plan as written cannot reach Phase 1.

The remedy is a bootstrap action, not a repository change. Provisioning the two missing versions into the repository-root `packages` directory is invisible to every gate in this plan: `git check-ignore -v` returns `.gitignore:191:**/[Pp]ackages/*` for a path under it. No project file and no `packages.config` may be edited; repairing the 80 stale version strings is a solution-wide change belonging to its own issue, and making it here would break AC-10 and AC-12.

The round-3 reviewer supplied full replacement text for P0-T7's task body and acceptance. It requires enumerating every distinct `Analyzer Include` package directory prefix across the solution, confirming each exists under `packages`, recording the referenced-but-missing count both before and after provisioning with the gate requiring the after count to be 0, and recording that `git status --porcelain` scoped to project and packages-config files produces empty output, which is what proves the reconciliation touched no project file. That empty-output assertion is confirmed true at BASE, where the only dirty tracked paths are under `.claude/agent-memory/`.

### 2. P3-T5's acceptance asserts a false `.gitignore` fact (blocking)

The final sentence of P3-T5's acceptance states that the repository-root `.dotnet-sdk` directory is not covered by `.gitignore`. **The orchestrator disproved this**: `git check-ignore -v .dotnet-sdk/dotnet.exe` returns `.gitignore:350:.dotnet*/`.

The span itself is correct; only its justification is wrong. This matters because the sentence sits inside an acceptance clause and would therefore be transcribed verbatim into the task's evidence artifact. It is the same class of defect round 2 identified in the `New-Item` rationale and explicitly forbade carrying into evidence, and it entered the plan through round 2's own replacement text, which no reviewer had checked.

The reviewer supplied replacement text giving the two true reasons for choosing an anchored `git diff` on the fourth span — that its file list is produced by the same command shape as the first span and remains valid after the commit, whereas porcelain status goes empty — and noting that untracked bootstrap and build output cannot make it fail because `git diff` reports only tracked paths, with each such path matched by `.gitignore` at lines 350, 191, 26 and 27. The orchestrator confirmed every one of those line numbers and patterns.

## Non-blocking observations recorded without a delta

1. Global rule 9's first sentence says every `git diff`, `git status` and `git grep` gate is anchored to BASE and scoped with an explicit pathspec. A `git status` cannot be anchored to a ref, and four status spans are not. The operative half, the explicit pathspec, holds everywhere and no gate is affected. Rewording is optional.
2. P5-T10's claim that no porcelain entry carries an untracked status field is trivially true after the preceding `git add -A`, which P3-T5's acceptance already states. The add and rename halves remain discriminating, so the check-off is sound.
3. P0-T14 produces an artifact no later task cites. It is legitimate provenance for AC-6 and AC-7; no change needed.

## Repository defect identified, outside this issue's scope

The analyzer version skew in defect 1 is a genuine repository defect that affects every fresh worktree and every clean CI-cold build, not a condition specific to issue #440. Its correct remedy is to realign the 80 stale `Analyzer Include` version strings with `packages.config`, which is a solution-wide change touching most project files. That is out of scope here and is deliberately not attempted: this plan works around it with a gitignored bootstrap provisioning step. The orchestrator reported it to the calling run for promotion into its own issue rather than promoting it directly, because the preparation-mode directive for issue #440 scoped this run's promotion actions to the already-promoted item.
