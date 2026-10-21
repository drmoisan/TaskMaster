# P1-T1 — Spec Write-Set Amendment and AC12 Verification

Timestamp: 2026-09-19T12-05

Command: `git diff HEAD -- docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`, `git diff --numstat HEAD -- <same>`, `git status --porcelain --untracked-files=all -- <same>`, plus read-only `Grep` measurements over `spec.md`

EXIT_CODE: 0

**TASK STATUS: COMPLETE.** Every clause of this task's acceptance holds, measured against the
revision 12.1 plan text, whose diff clause is anchored to `HEAD`.

## Part 1 — the `## Write Set` amendment (performed)

Three backticked entries were added, each with one sentence naming the reason recorded in the plan's
Scope Decision 1 and Scope Decision 5, and the first citing P0-T23 as its evidence.

| Path | Subsection | Reason recorded |
|---|---|---|
| `.github/workflows/_pester.yml` | Configuration and workflows | Scope Decision 1; cites the P0-T23 measurement of line 41 and line 45 |
| `scripts/dependencies/ConsistencyVerifier.psm1` | Production PowerShell | Scope Decision 5, the unconditional module split |
| `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | Tests | Scope Decision 5, the verifier's module-level suite |

Backticked-entry count **within the `## Write Set` section** (lines 564 to 669), which is the scope
the acceptance names:

| Path | Entries |
|---|---|
| `.github/workflows/_pester.yml` | **1** |
| `scripts/dependencies/ConsistencyVerifier.psm1` | **1** |
| `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | **1** |

Total backticked bullet entries in the section rose from 70 to **73**.

A whole-file count returns 2 for the first two paths. Those second occurrences are pre-existing and
outside the section: `.github/workflows/_pester.yml` is backticked in `## Context` where the
single-directory scoping is first described, and `scripts/dependencies/ConsistencyVerifier.psm1` is
backticked inside AC12 where the missing-segment reported class names the aggregating module. The
acceptance is scoped to the `## Write Set` section, where each path appears exactly once.

## Part 2 — AC12 verification (read-only; no criterion text edited)

The coordinator amended `spec.md` and committed it at `bf9a6d2b9` before execution began. This half
of the task measures and does not edit. AC12 occupies lines 431 to 465.

| Verification clause | Measured | Verdict |
|---|---|---|
| AC12 contains no clause **requiring** a higher or highest Roslyn-qualified folder be selected | Every mention of folder ordering is a prohibition: line 440 requires an item's folder segment be left unchanged when a higher folder is offered, and line 464 states the criterion fails for any implementation that orders or maximises over Roslyn-qualified folder names. No requiring clause is present. | PASS |
| AC12 contains **exactly one** statement of the preserve rule | One, at lines 437 to 439: the repair preserves the existing intermediate folder segment and moves only the version segment, and the restored package is enumerated solely to confirm the preserved segment still exists, never to select a folder. Lines 445 to 453 give the measured rationale and restate no rule. | PASS |
| AC12 contains **exactly one** statement of the missing-segment reported class | One, at lines 455 to 459: no guess, item left unmodified, a record naming the project, the item, the missing segment and the segments the listing does offer, aggregated as a distinct non-fatal class. | PASS |
| The `## Risks & Mitigations` bullet naming AC12 contains `preserve rule` and not `selection rule` | Line 680 reads that AC12 pins the **preserve rule** against an injected listing. The phrase `selection rule` does not occur in that bullet. | PASS |
| Criterion lines in `## Acceptance Criteria` | **26**, AC1 through AC26 with no gap and no duplicate, at lines 340, 351, 360, 367, 376, 382, 390, 401, 408, 415, 423, 431, 467, 473, 480, 486, 494, 501, 507, 515, 523, 530, 536, 544, 550 and 555 | PASS |

AC12 is present and amended. `AC12 AMENDMENT ABSENT` is **not** reported.

### The one permitted `selection rule` occurrence

A whole-file search returns **exactly 1** occurrence, at **line 449**, inside AC12:

```
`roslyn4.7` and `roslyn5.0`. A selection rule would therefore rewrite all **80** analyzer items
```

That is the prohibition statement explaining why such a rule would be incorrect, not a
specification of one. The plan directs that no zero-count assertion be written against it, and none
is. The measured location matches the line the plan records.

## Part 3 — the diff measurement, anchored to `HEAD`

```
git diff --numstat HEAD -- docs/features/active/2026-09-19-.../spec.md
12      0       docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md
```

Three hunks, headers as emitted with `-U0`:

```
@@ -577,0 +578,5 @@ Configuration and workflows:
@@ -586,0 +592,4 @@ Production PowerShell:
@@ -596,0 +606,3 @@ Tests:
```

All three sit inside `## Write Set`, which spans lines 564 to 669. **No hunk touches any criterion
line**: the `## Acceptance Criteria` section ends at line 563, above the first hunk, and the highest
criterion line is 555.

Measured shape: **12 added, 0 deleted, 0 criterion lines touched** — identical to the shape the
plan records.

Porcelain companion, per gate rule 8:

```
 M docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md
```

### Why the anchor is `HEAD` and not `<MERGE_BASE>`

`spec.md` does not exist at `734112ed25bba293cb074e71fee2286bc3b72fae`:
`git ls-tree <MERGE_BASE> -- docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`
returns zero entries, and a merge-base diff over the file is one whole-file addition hunk of 701
lines in which all 26 criterion lines appear as additions. That anchor makes the no-criterion-hunk
clause unsatisfiable whatever the executor does. `HEAD` is the last commit before this task runs, so
the diff isolates this task's edit alone. This was reported on the previous run and is now fixed in
plan revision 12.1; the other nine merge-base spans in the plan are over files that exist at the
base and correctly keep that anchor.

## Acceptance evaluation

| Clause | Verdict |
|---|---|
| Exactly one backticked Write Set entry for each of the three paths | PASS |
| Each new entry carries one sentence naming the recorded reason, the first citing P0-T23 | PASS |
| Every AC12 verification clause | PASS |
| Criterion count is exactly 26 | PASS |
| No criterion text edited by this task | PASS — 12 added lines, 0 deleted, all inside `## Write Set` |
| `git diff HEAD -- spec.md` confined to the Write Set section, no hunk touching a criterion line, paired with a porcelain capture | PASS — 12 added, 0 deleted, 0 criterion lines touched |

Output Summary: the three Write Set entries are present, one backticked entry each inside
`## Write Set`, raising the section from 70 to 73 entries, each carrying its reason sentence and the
first citing P0-T23. All five AC12 verification clauses hold, the criterion count is 26, the single
permitted `selection rule` occurrence sits at line 449 inside AC12, and the `## Risks & Mitigations`
bullet naming AC12 carries `preserve rule` at line 680. This task's edit measures 12 added lines and
0 deleted across three hunks, all inside `## Write Set`, touching no criterion line, confirmed
against `HEAD` with a porcelain companion. Task complete.
