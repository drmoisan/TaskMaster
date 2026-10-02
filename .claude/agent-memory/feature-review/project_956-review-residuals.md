---
name: 956-review-residuals
description: '#956 (SortEmail partial split + YesNoToAllPromptSession seam) full-bug review 2026-10-01T22-24 PASS 17/17 AC, 0 blocking, 6 non-blocking; Cobertura <class> Grep needs filename-first pattern; session-cwd artifacts/csharp/coverage.xml is Cobertura so the hook cannot force FAIL from it; DirectoryInfo-inside-try spec inconsistency'
metadata:
  type: project
---

Full-bug review (parallel cohort bugs-2026-09-28, nested worktree `agent-a6974b35cca57abb8`, head `6278c6316`, merge base
`f5b46df63` = origin/main, ancestor of head via a merge commit): PASS, 17/17 AC, 0 blocking, 6 non-blocking (CR-1..CR-6),
3 gaps (G-1..G-3), 6 follow-ups. Bash was limited to `git diff *` / `git log *` from the session cwd (shared object store, so
`git diff --stat <base> <head>` and `git log --format='%H %ct %s'` worked without `-C`); everything else Read/Grep/Glob.
Advertised the 3-`..` hook path from the session cwd as at [[928-review-residuals]]; plain + absolute paths in prose; no mirror.

**Reusable verification points:**
- Grep on a dotnet-coverage Cobertura document: the `<class>` attribute order is `line-rate branch-rate complexity name
  filename`, so a pattern starting `class name="..." filename=` returns nothing. Match `filename="[^"]*<File>\.cs"` alone
  (`-n`), then Read the class region by line number for per-line `hits`. Root element is readable with Read limit 6
  (`lines-covered/lines-valid/branches-*`, `timestamp=` epoch as the clock). `ConvertTo-KoverageCoberturaXml` output folds
  state-machine classes into the parent `<class>` but leaves closure classes (`<>c.<<SortAsync>b__25_3>d`) as separate
  0-line-rate nodes, so a moved uncovered lambda shows up as its own class row.
- Session-cwd `artifacts/csharp/coverage.xml` was a Cobertura document (2026-09-05, 84.83%). The hook's
  `Get-JacocoRepoCoverage` selects `//counter[@type="LINE"]`, which Cobertura lacks, so it returns null and the sub-85 figure
  CANNOT force a FAIL row. Combined with the stale session `pr_context.summary.txt` having no `.cs` bullets (only .md/.yml/
  .csproj), the language checks were fully disarmed from the session cwd; only the three artifact-existence checks were live.
- Verbatim-move review of a partial split: read merge-base header (1-19), the field block, the seam method and the tail
  against the partials; trust the executor's segment census (`PARTITION-EXACT`, `FILE-EXACT`) for the middle; count
  `[ExcludeFromCodeCoverage]` before/after (28 -> 28 here, one removed + one added) as a cheap attribute-parity check.
- CR-1 pattern: a spec that says "exception boundary unchanged (path computed before the inner try)" while its own design
  moves the `new DirectoryInfo` into an adapter called inside the try is internally inconsistent; the AC text as written
  held, so non-blocking with a spec-wording follow-up. Check whether the first `try` already validated the same argument
  (`Directory.CreateDirectory` + `Path.GetDirectoryName`) before calling the nuance unreachable.
- `ReleaseSingleAnswer()` replacing two arm-specific resets (`if Yes -> Empty`, `if No -> Empty`) is equivalent only because
  each arm can hold just the two values its guard admits; state that reasoning explicitly in AC6.

**Follow-ups owed to the orchestrator:** F-1 reconcile spec "Boundaries" bullet with D2 items 3/5; F-2 promote L1-L4, F1-F3
from the spec Rollout list (still present in code); F-3 `Debug.WriteLine` -> logger and drop the no-op outer rethrow in the
try-save core; F-4 trim replicated using blocks; F-5 canonical C# coverage artifact path convention (recurring); F-6
`quality-tiers.yml` absent at repo root (tier gates unevaluable, pre-existing).
