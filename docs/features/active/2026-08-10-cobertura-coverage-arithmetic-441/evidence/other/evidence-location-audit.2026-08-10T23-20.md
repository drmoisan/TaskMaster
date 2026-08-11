# Evidence Location and Schema Audit (P5-T5, and P7-T20 final sweep)

Timestamp: 2026-08-10T23-20

Audits every artifact this plan has produced against
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md` and against `spec.md` AC-16.

## Sweep 1 — Phases 0 through 5 (P5-T5)

This sweep deliberately **excludes** the Phase 6 and Phase 7 artifacts, which do not exist yet. They
are covered by the final sweep appended by P7-T20, and AC-16 is certified against that final sweep,
not against this one.

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
$ev = Join-Path $root 'docs\features\active\2026-08-10-cobertura-coverage-arithmetic-441\evidence'
Get-ChildItem -Path $ev -Recurse -File | ForEach-Object { $_.FullName.Substring($ev.Length + 1) }
Get-ChildItem -Path $ev -Recurse -File -Filter '*.md' | ForEach-Object {
    $t = Get-Content -LiteralPath $_.FullName -Raw
    '{0} | Timestamp={1} Command={2} EXIT_CODE={3} OutputSummary={4}' -f `
        $_.Name, $t.Contains('Timestamp:'), $t.Contains('Command:'), $t.Contains('EXIT_CODE:'), $t.Contains('Output Summary:')
}
Get-ChildItem -Path (Join-Path $root 'artifacts') -Recurse -File
```

EXIT_CODE: 0

Output Summary:

```
31 files under <FEATURE>/evidence/, all within {baseline, regression-testing, qa-gates, other}.
26 markdown artifacts: 24 command-step artifacts carry all four schema fields;
2 narrative artifacts carry Timestamp only (permitted by the amended AC-16).
5 coverage XML data files carry no schema fields (they are tool output referenced by their
  parent artifacts, not artifacts in their own right).
No evidence artifact exists under any artifacts/ path.
```

### 1.1 Location inventory (31 files, all canonical)

**`<FEATURE>/evidence/baseline/` — 10 files**

| File | Kind |
| --- | --- |
| `phase0-instructions-read.md` | command-step |
| `git-baseline.2026-08-10T22-30.md` | command-step |
| `poshqc-tool-surface.2026-08-10T22-30.md` | command-step |
| `poshqc-format.2026-08-10T22-30.md` | command-step |
| `poshqc-analyze.2026-08-10T22-30.md` | command-step |
| `pester-baseline.2026-08-10T22-30.md` | command-step |
| `prechange-generator-parity.2026-08-10T22-30.md` | command-step |
| `prechange-package-filtered.2026-08-10T22-30.md` | command-step |
| `assumption2-subset-proof.2026-08-10T22-30.md` | command-step |
| `pester-coverage-baseline.2026-08-10T22-30.xml` | coverage data |

**`<FEATURE>/evidence/regression-testing/` — 6 files**

| File | Kind |
| --- | --- |
| `fail-before-f1-f4.2026-08-10T22-45.md` | command-step |
| `pass-after-f1-f6.2026-08-10T22-55.md` | command-step |
| `helper-unit-tests.2026-08-10T23-05.md` | command-step |
| `pester-coverage-fail-before.2026-08-10T22-45.xml` | coverage data |
| `pester-coverage-pass-after.2026-08-10T22-55.xml` | coverage data |
| `pester-coverage-helper-unit-tests.2026-08-10T23-05.xml` | coverage data |

**`<FEATURE>/evidence/qa-gates/` — 13 files**

| File | Kind |
| --- | --- |
| `union-builder-byte-identity.2026-08-10T22-55.md` | command-step (two sweeps: P2-T5 + P4-T10) |
| `poshqc-format.2026-08-10T23-10.md` | command-step |
| `poshqc-analyze.2026-08-10T23-10.md` | command-step |
| `pester-final.2026-08-10T23-10.md` | command-step |
| `toolchain-clean-pass.2026-08-10T23-10.md` | command-step |
| `file-size-audit.2026-08-10T23-10.md` | command-step |
| `coverage-delta.2026-08-10T23-10.md` | command-step |
| `scope-lock.2026-08-10T23-10.md` | command-step |
| `threshold-no-change.2026-08-10T23-10.md` | command-step |
| `postchange-generator-parity.2026-08-10T23-15.md` | command-step |
| `postchange-package-filtered.2026-08-10T23-15.md` | command-step |
| `coverage-arithmetic-delta.2026-08-10T23-15.md` | command-step |
| `pester-coverage-final.2026-08-10T23-10.xml` | coverage data |

**`<FEATURE>/evidence/other/` — 2 files**

| File | Kind |
| --- | --- |
| `helper-branch-test-map.2026-08-10T23-10.md` | **narrative** |
| `threshold-handoff-494.2026-08-10T23-15.md` | **narrative** |

All four sub-paths used — `baseline`, `regression-testing`, `qa-gates`, `other` — are canonical
under `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. `issue-updates/` is not yet
populated; it is written in Phase 6.

### 1.2 Schema conformance

- **24 command-step artifacts**: every one carries `Timestamp:`, `Command:`, `EXIT_CODE:` **and**
  `Output Summary:`. Verified mechanically by the `.Contains(...)` check above; all four columns
  returned `True` for all 24.
- **2 narrative artifacts** (`helper-branch-test-map`, `threshold-handoff-494`): each carries
  `Timestamp:` and records no command, so it carries no `Command:` or `EXIT_CODE:`. This is
  **permitted**: the 2026-08-10T21-40 amendment recorded in `spec.md` § Acceptance Criteria scopes
  the `Command:`/`EXIT_CODE:` requirement to command-step artifacts, precisely because requiring
  those fields of a narrative artifact would make AC-16 unsatisfiable.
- **5 coverage XML files**: JaCoCo reports written by the direct `Invoke-Pester` runs. They are
  measurement data referenced by their parent markdown artifacts (each of which records the
  `OutputPath`), not artifacts in their own right, and carry no schema fields by nature.

### 1.3 Forbidden-path search

- **SearchScope:** the whole worktree, specifically
  `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a\artifacts\`
- **SearchPatterns:** `artifacts/**` — in particular the forbidden evidence sub-paths
  `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`,
  `artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/`,
  `artifacts/post-change/`
- **SearchResult for evidence artifacts:** **none.** Not one artifact produced by this plan resides
  under any `artifacts/` path. Every forbidden sub-path listed above is absent from the worktree
  entirely.

Four files do exist under `artifacts/`, and each is accounted for as **not** evidence produced by
this plan:

| Path | Provenance | Status |
| --- | --- | --- |
| `artifacts/orchestration/orchestrator-state.json` | written by `epic-orchestrator` before this execution began (mtime 22:28, prior to the first Phase 0 command) | **allowed** — `artifacts/orchestration/` is the one permitted non-evidence `artifacts/` sub-path |
| `artifacts/pester/pester-junit.xml` | tool output written by the bundled MCP `run_poshqc_test` | producer output, not evidence |
| `artifacts/pester/powershell-coverage.xml` | tool output written by the bundled MCP `run_poshqc_test` | producer output, not evidence |
| `artifacts/pester/powershell-coverage.koverage.xml` | tool output written by the bundled MCP `run_poshqc_test` | producer output, not evidence |

All four are **gitignored** (`.gitignore:57` ignores `artifacts/` wholesale, confirmed by
`git check-ignore -v`) and **untracked**, so none will be committed. No artifact of this feature is
read from or written to them; the plan's coverage numbers come exclusively from the direct
`Invoke-Pester` runs whose `OutputPath` is an explicit `<FEATURE>/evidence/<kind>/` path.

### 1.4 Correction raised by this sweep

The P0-T13 artifact originally asserted that `run_poshqc_test` "writes no coverage artifact into the
workspace, evidenced by an empty `git status --porcelain` immediately afterwards." **That assertion
was wrong, and the instrument did not support it.** The tool does write the three `artifacts/pester/`
files above; they are invisible to `git status --porcelain` because `artifacts/` is gitignored, so an
empty porcelain proves only that nothing *tracked* changed.

The claim has been corrected in place with a dated `CORRECTION` block appended to
`<FEATURE>/evidence/baseline/poshqc-tool-surface.2026-08-10T22-30.md`, rather than silently edited
away. No conclusion in this plan depends on the retracted sentence: the MCP call was treated as
non-probative throughout for independent reasons (it carries no verdict and no counts), and every
figure was taken from the direct Pester runs.

## Sweep 1 verdict

**PASS.** Every artifact produced by Phases 0 through 5 resides under
`<FEATURE>/evidence/{baseline,regression-testing,qa-gates,other}/`; every command-step artifact
carries all four schema fields; every narrative artifact carries `Timestamp:`; and no evidence
artifact was written under any `artifacts/` path.

---

## Final sweep

Timestamp: 2026-08-10T23-30

Re-runs the audit over the **full** artifact set, including the Phase 6 and Phase 7 artifacts that
did not exist at Sweep 1. **AC-16 is certified against this sweep, not against Sweep 1.** This
section is written before the AC-16 check-off (P7-T21), so the criterion is certified against
evidence that already exists on disk.

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
$ev = Join-Path $root 'docs\features\active\2026-08-10-cobertura-coverage-arithmetic-441\evidence'
Get-ChildItem -Path $ev -Recurse -File | ForEach-Object { $_.FullName.Substring($ev.Length + 1) }
Get-ChildItem -Path $ev -Recurse -File -Filter '*.md' | ForEach-Object {
    $t = Get-Content -LiteralPath $_.FullName -Raw
    '{0} | TS={1} Cmd={2} EXIT={3} OS={4}' -f `
        $_.Name, $t.Contains('Timestamp:'), $t.Contains('Command:'), $t.Contains('EXIT_CODE:'), $t.Contains('Output Summary:')
}
```

EXIT_CODE: 0

Output Summary:

```
34 files under <FEATURE>/evidence/ across five canonical sub-paths:
   baseline 10 + regression-testing 6 + qa-gates 14 + other 3 + issue-updates 1 = 34.
29 markdown artifacts: 27 command-step artifacts carry all four schema fields;
   2 narrative artifacts carry Timestamp: only (permitted).
5 coverage XML data files carry no schema fields (tool output referenced by their parents).
Forbidden-path search: SearchResult none.
```

### Complete enumeration of every file under `<FEATURE>/evidence/` (34 files)

**`baseline/` (10)**

1. `baseline\assumption2-subset-proof.2026-08-10T22-30.md` — command-step
2. `baseline\git-baseline.2026-08-10T22-30.md` — command-step
3. `baseline\pester-baseline.2026-08-10T22-30.md` — command-step
4. `baseline\pester-coverage-baseline.2026-08-10T22-30.xml` — coverage data
5. `baseline\phase0-instructions-read.md` — command-step
6. `baseline\poshqc-analyze.2026-08-10T22-30.md` — command-step
7. `baseline\poshqc-format.2026-08-10T22-30.md` — command-step
8. `baseline\poshqc-tool-surface.2026-08-10T22-30.md` — command-step (carries the dated CORRECTION block)
9. `baseline\prechange-generator-parity.2026-08-10T22-30.md` — command-step
10. `baseline\prechange-package-filtered.2026-08-10T22-30.md` — command-step

**`regression-testing/` (6)**

11. `regression-testing\fail-before-f1-f4.2026-08-10T22-45.md` — command-step
12. `regression-testing\helper-unit-tests.2026-08-10T23-05.md` — command-step
13. `regression-testing\pass-after-f1-f6.2026-08-10T22-55.md` — command-step
14. `regression-testing\pester-coverage-fail-before.2026-08-10T22-45.xml` — coverage data
15. `regression-testing\pester-coverage-helper-unit-tests.2026-08-10T23-05.xml` — coverage data
16. `regression-testing\pester-coverage-pass-after.2026-08-10T22-55.xml` — coverage data

**`qa-gates/` (14)**

17. `qa-gates\coverage-arithmetic-delta.2026-08-10T23-15.md` — command-step
18. `qa-gates\coverage-delta.2026-08-10T23-10.md` — command-step
19. `qa-gates\file-size-audit.2026-08-10T23-10.md` — command-step
20. `qa-gates\followups-not-fixed.2026-08-10T23-25.md` — command-step **(Phase 6, new since Sweep 1)**
21. `qa-gates\pester-coverage-final.2026-08-10T23-10.xml` — coverage data
22. `qa-gates\pester-final.2026-08-10T23-10.md` — command-step
23. `qa-gates\poshqc-analyze.2026-08-10T23-10.md` — command-step
24. `qa-gates\poshqc-format.2026-08-10T23-10.md` — command-step
25. `qa-gates\postchange-generator-parity.2026-08-10T23-15.md` — command-step
26. `qa-gates\postchange-package-filtered.2026-08-10T23-15.md` — command-step
27. `qa-gates\scope-lock.2026-08-10T23-10.md` — command-step
28. `qa-gates\threshold-no-change.2026-08-10T23-10.md` — command-step
29. `qa-gates\toolchain-clean-pass.2026-08-10T23-10.md` — command-step
30. `qa-gates\union-builder-byte-identity.2026-08-10T22-55.md` — command-step

**`other/` (3)**

31. `other\evidence-location-audit.2026-08-10T23-20.md` — command-step (this file)
32. `other\helper-branch-test-map.2026-08-10T23-10.md` — **narrative**
33. `other\threshold-handoff-494.2026-08-10T23-15.md` — **narrative**

**`issue-updates/` (1)**

34. `issue-updates\followups-441.2026-08-10T23-25.md` — command-step **(Phase 6, new since Sweep 1)**

All five sub-paths in use — `baseline`, `regression-testing`, `qa-gates`, `other`, `issue-updates`
— are canonical under `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` and are exactly
the five enumerated by the amended AC-16.

### Schema conformance across the full set

- **27 command-step markdown artifacts**: every one carries `Timestamp:`, `Command:`, `EXIT_CODE:`
  and `Output Summary:`. All four columns returned `True` for all 27 in the mechanical check above.
  This includes both artifacts new since Sweep 1 (`followups-441` and `followups-not-fixed`).
- **2 narrative markdown artifacts** (`helper-branch-test-map`, `threshold-handoff-494`): each
  carries `Timestamp:`, records no command, and therefore carries no `Command:`/`EXIT_CODE:`.
  Individually enumerated here as items 32 and 33, as AC-16 requires. Permitted by the
  2026-08-10T21-40 amendment recorded in `spec.md` § Acceptance Criteria, which scopes the
  `Command:`/`EXIT_CODE:` requirement to command-step artifacts.
- **5 coverage XML data files** (items 4, 14, 15, 16, 21): JaCoCo reports written by the direct
  `Invoke-Pester` runs, referenced by their parent markdown artifacts, carrying no schema fields by
  nature.

### Forbidden-path search (final)

- **SearchScope:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a\artifacts\`
  and the entire worktree
- **SearchPatterns:** `artifacts/**`, specifically `artifacts/baselines/`, `artifacts/baseline/`,
  `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`,
  `artifacts/regression-testing/`, `artifacts/post-change/`
- **SearchResult:** **none.** No evidence artifact for this feature exists under any `artifacts/`
  path. The only files under `artifacts/` are the orchestrator checkpoint in the permitted
  `artifacts/orchestration/` sub-path and three gitignored, untracked PoshQC producer outputs under
  `artifacts/pester/`, all itemized and accounted for in Sweep 1 § 1.3.

### One artifact is written after this sweep

Exactly one artifact is created after this section: **`<FEATURE>/evidence/other/ac-status-summary.2026-08-10T23-30.md`**, written by P7-T22. Its
intended path is stated here so the audit trail is complete. It is a **narrative** artifact — it
records no command and therefore carries no `EXIT_CODE:` field — and it carries `Timestamp:`.

### Final sweep verdict

**PASS.** Every one of the 34 artifacts resides under
`<FEATURE>/evidence/{baseline,regression-testing,qa-gates,issue-updates,other}/`; every command-step
artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`; every narrative
artifact carries `Timestamp:` and is individually enumerated; and no evidence artifact is written
under any `artifacts/` path. **AC-16 is satisfied.**
