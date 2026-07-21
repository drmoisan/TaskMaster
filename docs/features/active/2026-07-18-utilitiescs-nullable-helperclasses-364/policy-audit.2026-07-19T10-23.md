# Policy Compliance Audit — Issue #364 (utilitiescs-nullable-helperclasses)

- Timestamp: 2026-07-19T10-23
- Reviewer: feature-reviewer
- Work Mode: full-feature (from `issue.md` `- Work Mode: full-feature`)
- Branch: `feature/utilitiescs-nullable-helperclasses-364`
- Base (diff target): `origin/epic/utilitiescs-nullable-remediation-integration`
- Merge-base: `6d4da8bb4d881dc26c421440464ce5575e3fb15f`
- Head: `2edda572b87593446c2ef5546eef71f660a0a35f`
- Diff command: `git diff origin/epic/utilitiescs-nullable-remediation-integration...HEAD`
- PR context artifacts: regenerated this session (were absent). `artifacts/pr_context.summary.txt`,
  `artifacts/pr_context.appendix.txt`. Reconstructed manually from `git diff --numstat` (no
  `collect_pr_context` tool available in-session).

## Executive Summary

The branch delivers per-file `#nullable enable` opt-in nullable-reference-type remediation across
`UtilitiesCS/HelperClasses/`. The change is annotation and null-safety only; no production behavior
change was observed in the reviewed diffs. All maintainer-mandated hard constraints were verified
respected. The full toolchain passes for the in-scope obligations. One pre-existing, out-of-scope
condition (a full-solution `TreatWarningsAsErrors` build exiting 1) is adjudicated below as a
pre-existing, out-of-scope condition that the maintainer scope lock forbids remediating in this
child, not a blocking in-scope defect.

Overall policy verdict: PASS. Blocking findings: 0.

## Scope Narrowing

No caller instruction attempted to narrow the audit scope below the full branch diff. The task
prompt scoped the review to the full `origin/epic/...integration...HEAD` diff, which is the
authoritative scope. Nothing to reject.

## Changed-File Scope Verification

- Command: `git diff --name-only <base>...HEAD | grep -E '\.(cs|csproj|sln)$' | grep -v 'UtilitiesCS/HelperClasses/'`
  → NONE. No C# source, project, or solution file outside `UtilitiesCS/HelperClasses/` was changed.
- Command: `git diff --name-only <base>...HEAD | grep -iE 'csproj|\.sln|Designer'` → only the
  `csproj-no-nullable` evidence markdown matched; no actual `.csproj`, `.sln`, or `Designer.cs`
  file is in the diff.
- Only non-source, non-doc files in the diff: `.claude/agent-memory/atomic-executor/MEMORY.md` and
  `.claude/agent-memory/atomic-executor/project_364_nullable_gate_preexisting_blockers.md`
  (executor memory notes; not production code).
- Verdict: PASS. The maintainer scope lock ("no changes to files outside
  `UtilitiesCS/HelperClasses/`") is respected for all source, project, and solution files.

## Hard-Constraint Verification

| # | Constraint | Method | Result | Verdict |
|---|---|---|---|---|
| 1 | Per-file `#nullable enable` on each in-scope file; zero CS86xx per opted-in file under pragma-only build | `grep -rl '#nullable enable' UtilitiesCS/HelperClasses/` = 42; total `.cs` = 43; only `DvgForm.Designer.cs` lacks the pragma. Isolated `UtilitiesCS.csproj` build exits 0 with 0 CS86xx (evidence `final-nullable-build`) | 42 of 43 opted-in; 0 CS86xx | PASS |
| 2 | No project/solution `<Nullable>` element in `UtilitiesCS.csproj` | `grep -nE '<Nullable>' UtilitiesCS/UtilitiesCS.csproj` → no match; csproj not in diff | none present | PASS |
| 3 | FileSystem adapter root boundaries use behavior-preserving `!` with `// why`; latent root-throws flagged not fixed | Reviewed `PhysicalFileInfoAdapter.cs` / `PhysicalDirectoryInfoAdapter.cs` / `FilePathHelper.cs` diffs; every `!` carries a `// why` comment; latent throw flagged in `evidence/other/maintainer-flags.2026-07-19T09-35.md` | correct `!` + comments; flagged | PASS |
| 4 | `PhysicalFileInfoAdapter` injectable-delegate seam preserved exactly | `grep` confirms the four seam fields, both ctors, and all `?? throw` guards present; `git diff` shows no `+`/`-` line touches any seam field, ctor, or guard (only the pragma and two root-boundary annotations changed) | seam byte-unchanged | PASS |
| 5 | `DvgForm.Designer.cs` not hand-edited / not pragma-annotated | Not present in diff (`git diff --stat ... DvgForm.Designer.cs` empty); 0 `#nullable` pragmas in the file | unchanged, oblivious | PASS |
| 6 | `PrettyPrint.cs` 500-line breach flagged not fixed | File is 680 lines (was 677); flagged in `evidence/other/maintainer-flags.2026-07-19T10-05.md`; not split | flagged, not fixed | PASS |
| 7 | Interfaces under `UtilitiesCS/Interfaces/IHelperClasses/` stay oblivious | No file under `UtilitiesCS/Interfaces/` in the diff | untouched | PASS |

## Toolchain Compliance (C#)

Evidence source: `evidence/qa-gates/final-*.2026-07-19T10-07.md`.

| Stage | Command (as evidenced) | Result | Verdict |
|---|---|---|---|
| Format (CSharpier) | `csharpier check .` | EXIT 0; 1406 files checked, 0 unformatted | PASS |
| Analyzer / codestyle build | `msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT 0; 0 errors; 16 pre-existing non-HelperClasses warnings; 0 CS86xx in HelperClasses | PASS |
| Type-check (pragma-only, per critical deviation) | `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild ... /p:BuildProjectReferences=false` (isolated, authoritative for #364) | EXIT 0; 0 CS86xx across the project and HelperClasses | PASS |
| Test + coverage | scoped `dotnet-coverage collect ... UtilitiesCS.Test.dll` | EXIT 0; 4511/4511 tests passed | PASS |

The pragma-only type-check deviation (no `/p:Nullable=enable`) is authorized by spec Constraints &
Risks item (1) for this child only and is not resolved by editing `.claude/rules/*` (confirmed: no
`.claude/rules/*` file is in the diff).

### Adjudication — Full-Solution TreatWarningsAsErrors exit 1

The plan-literal full-solution build
`msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`)
exits 1. Evidence-first analysis:

- The failure cause is 4x `CS0649` (field-never-assigned) in the vendored `SVGControl/SvgImageSelector.cs`
  (fields dated to a 2023 WIP commit), promoted to errors by `TreatWarningsAsErrors`. SVGControl builds
  early in the solution order (it is a project reference of UtilitiesCS), so the solution build halts
  there. Additional pre-existing out-of-scope promotions (CS0618 obsolete-API and CS0168 unused-variable
  in `UtilitiesCS/EmailIntelligence/` and `Extensions/`) are recorded in the baseline evidence.
- This is identical in cause to the P0-T4 baseline captured BEFORE any #364 edit
  (`evidence/baseline/nullable-build-baseline.2026-07-19T08-51.md`, EXIT 1, same SVGControl CS0649).
  The condition surfaced only because commit `20d163ac` changed the nullable gate from `/t:Build`
  (a silent no-op) to `/t:Rebuild` (a genuine recompile).
- None of these promotions is a CS86xx nullable diagnostic, and none originates in
  `UtilitiesCS/HelperClasses/`. The #364 annotation work introduced ZERO new diagnostics.
- All the promoted warnings live in files outside `UtilitiesCS/HelperClasses/`, which the maintainer
  scope lock (spec Non-Goals) forbids this child from modifying.

Adjudication: this is classification (b) — a PRE-EXISTING, OUT-OF-SCOPE condition that the scope
lock forbids remediating in this child. It is NOT a blocking in-scope defect. The in-scope
toolchain obligation (pragma-only type-check to zero CS86xx across all opted-in HelperClasses files)
passes (isolated build EXIT 0). The condition is flagged for the maintainer / epic capstone.

## Evidence Location Compliance

- Command: `git diff --name-only <base>...HEAD | grep -E 'artifacts/(baselines|qa|evidence|coverage)/'`
  → NONE.
- All evidence artifacts are written under the canonical
  `docs/features/active/2026-07-18-utilitiescs-nullable-helperclasses-364/evidence/{baseline,qa-gates,regression-testing,other}/`
  path. No non-canonical `artifacts/baselines|qa|coverage|evidence` writes exist in the diff.
- `scripts/dev_tools/validate_evidence_locations.py` is not present in this repository (checked);
  the manual scan above is used instead.
- Verdict: PASS.

## Coverage Verification

Changed-language set (from `artifacts/pr_context.summary.txt`): C# only. No TypeScript, Python, or
PowerShell files changed on this branch.

Coverage artifact used: the feature-evidence Cobertura
`evidence/qa-gates/final-coverage.2026-07-19T10-07.cobertura.xml` (post-change) compared against
`evidence/baseline/coverage-baseline.2026-07-19T08-51.cobertura.xml` (baseline). Both captured with
the identical scoped invocation (UtilitiesCS.Test, `coverage.config`, `/InIsolation`,
`TestCategory!=LiveOutlook`), so the figures are directly comparable.

Coverage rows (explicit PASS/FAIL per language with changed files):

- C# changed-code coverage PASS: the modified `UtilitiesCS/HelperClasses/` files report 92.08% line
  coverage (9027/9803), up from the 92.07% baseline (8989/9763). This clears the 85% line floor for
  modified files with zero regression on the changed lines. All 4511 UtilitiesCS tests pass. Verdict PASS.
- C# repo-scoped coverage row PASS-with-note: the scoped root figure is 72.07% line (98304/136399)
  and 48.45% branch (12289/25366), flat versus the identical baseline root figure (72.07% line /
  48.45% branch). This scoped single-assembly measurement instruments other loaded first-party
  modules and deflates the denominator; it is a pre-existing figure unchanged by this annotation-only
  feature (delta within measurement noise), so it introduces no regression. Verdict PASS on the
  no-regression basis.
- TypeScript coverage row: no changed files on this branch (zero `.ts`/`.tsx` in the diff); coverage
  measurement not applicable to TypeScript for this branch.
- Python coverage row: no changed files on this branch (zero `.py` in the diff); coverage
  measurement not applicable to Python for this branch.
- PowerShell coverage row: no changed files on this branch (zero `.ps1`/`.psm1` in the diff);
  coverage measurement not applicable to PowerShell for this branch.

Procedural note (non-blocking): the canonical repo-wide C# JaCoCo artifact
`artifacts/csharp/coverage.xml` is not present in this local worktree. Per repository practice,
local full-assembly C# collection is blocked by a known Moq binding-redirect issue, so the per-feature
Cobertura is the local evidence and the true repo-wide first-party figure is produced by the PR CI
run. The substantive coverage obligation (changed-code line coverage above floor, no regression) is
independently verified from the feature-evidence Cobertura, so this canonical-artifact absence is a
procedural item satisfied at merge by the PR CI run; it is not a blocking code defect for this child.

New vs modified file determination: `git diff --name-status <base>...HEAD -- '*.cs'` shows all
changed `.cs` files are Modified (status M); there are NO added `.cs` files. The 90%-new-file tier
therefore does not apply; the 85%-line / no-regression modified-file tier applies and is met.

Coverage verdict: PASS (C#).

## Policy Verdict Summary

| Area | Verdict |
|---|---|
| Scope lock (no files outside HelperClasses) | PASS |
| No project/solution `<Nullable>` element | PASS |
| Per-file `#nullable enable` + zero CS86xx | PASS |
| Adapter root-boundary `!` + `// why`; latent throw flagged | PASS |
| PhysicalFileInfoAdapter seam preserved | PASS |
| DvgForm.Designer.cs untouched | PASS |
| PrettyPrint.cs 500-line breach flagged not fixed | PASS |
| Toolchain (format/analyzer/type-check/test) | PASS |
| Full-solution TWAE exit 1 | Pre-existing, out-of-scope (non-blocking) |
| Evidence location compliance | PASS |
| Coverage (C#) | PASS |

Blocking findings in this artifact: 0.
