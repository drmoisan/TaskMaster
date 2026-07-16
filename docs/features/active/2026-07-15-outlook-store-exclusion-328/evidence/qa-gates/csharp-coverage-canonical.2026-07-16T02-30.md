# R1 — Canonical C# Coverage Artifact (JaCoCo) Verification (Issue #328)

Timestamp: 2026-07-16T02-30
Command:
- `python cobertura_to_jacoco.py` (deterministic Cobertura -> JaCoCo conversion; source =
  `evidence/qa-gates/final-coverage.2026-07-15T18-45.cobertura.xml`; output = `artifacts/csharp/coverage.xml`)
- `. ./.claude/hooks/validate-feature-review-coverage.ps1; Get-JacocoRepoCoverage -Path 'artifacts/csharp/coverage.xml'`
- `. ./.claude/hooks/validate-feature-review-coverage.ps1; Get-JacocoBranchCoverage -Path 'artifacts/csharp/coverage.xml'`
- `Test-Path artifacts/csharp/coverage.xml`

EXIT_CODE: 0

## Output Summary

- `Test-Path artifacts/csharp/coverage.xml`: True
- `Get-JacocoRepoCoverage` (first-party LINE %): 70.45
- `Get-JacocoBranchCoverage` (first-party BRANCH %): 67.11
- JaCoCo `//counter[@type="LINE"]` elements: 6; `//counter[@type="BRANCH"]` elements: 6
- Both parser results are non-null numeric ([double]); the coverage hook
  (`.claude/hooks/validate-feature-review-coverage.ps1`) reads the artifact directly.

## Disposition — P1-T2 explicitly-authorized alternative applies

The deterministic first-party aggregate line coverage (70.45%) does NOT clear the 85% line floor. Per
the plan's P1-T2 acceptance, the explicitly-authorized alternative applies because pre-existing
out-of-scope low-coverage first-party assemblies inflate the denominator beyond issue #328's
incremental contribution:

- The Cobertura source was produced by the delivered plan's vstest run over `UtilitiesCS.Test`,
  `TaskMaster.Test`, and `ToDoModel.Test` only (policy-audit §2). The first-party assemblies
  `QuickFiler` (0.00%, 7189 lines), `Tags` (0.00%, 758 lines), and `TaskVisualization` (0.83%, 1573
  lines) appear in the report at ~0% solely because their `.Test` projects were not part of this
  coverage collection — not because their production code is genuinely uncovered. These three
  assemblies contribute 9520 uncovered denominator lines that issue #328 did not touch.
- Issue #328's changes live in `UtilitiesCS` and `ToDoModel`. `UtilitiesCS` — the assembly containing
  the bulk of the #328 code (`StoresWrapper`, `StoreFilterAttribution`, `StoreWrapperController`,
  `StoreWrapper`) — is at 88.33% line / 82.00% branch, clearing both floors. Every touched non-exempt
  first-party class clears the 85% line floor at >= 95% (policy-audit §5.2/§5.3; coverage-delta).
- Per policy-audit §5.4, a stable repo-wide first-party C# line-coverage number is not recomputable
  from a single local `dotnet-coverage` run (documented denominator/instrumentation nondeterminism);
  the authoritative repo-wide gate is the PR CI coverage run.

Alternative disposition recorded: the PR CI coverage run is the authoritative repo-wide C# coverage
gate for this feature per policy-audit §5.4. The canonical `artifacts/csharp/coverage.xml` is present
and hook-parseable in this branch (verified above). CI workflow-run URL: PENDING — branch
`feature/outlook-store-exclusion-328` currently has no open PR and no branch-triggered workflow run
(`gh pr list --head feature/outlook-store-exclusion-328 --state all` and
`gh run list --branch feature/outlook-store-exclusion-328` both returned empty on 2026-07-16T02-30);
the concrete CI run URL is produced when the PR is opened. This remediation does not open the PR
(out of scope). The canonical artifact's presence is the deliverable that resolves the AC12/US-AC4
"canonical artifact absent" finding; the repo-wide aggregate remains authoritatively deferred to CI.

## Auditable package manifest

Included (first-party production assemblies present in the Cobertura report):

| Package | LINE covered/total | LINE % | BRANCH covered/total | BRANCH % |
|---|---|---|---|---|
| QuickFiler        | 0/7189      | 0.00  | 0/1398    | 0.00  |
| Tags              | 0/758       | 0.00  | 0/190     | 0.00  |
| TaskMaster        | 1689/2507   | 67.37 | 329/538   | 61.15 |
| TaskVisualization | 13/1573     | 0.83  | 0/400     | 0.00  |
| ToDoModel         | 991/1831    | 54.12 | 239/508   | 47.05 |
| UtilitiesCS       | 34929/39544 | 88.33 | 8088/9864 | 82.00 |
| **First-party total** | **37622/53402** | **70.45** | **8656/12898** | **67.11** |

Excluded — vendored / third-party NuGet packages (not first-party production): `Deedle`,
`FluentAssertions`, `FSharp.Core`, `log4net`, `Mono.Reflection`, `SVGControl`, `System.Interactive`,
`System.Linq.Async`. (The plan names `Deedle`/`FSharp.Core`/`Swordfish`/`SVGControl`; `Swordfish` is
not present in this report — it was removed in the prior Swordfish-removal epic.)

Excluded — `*.Test` assemblies: `TaskMaster.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`.

Conversion method: per-`<line>` dedup by `(filename, line-number)` — a line is covered if any
occurrence has `hits > 0`; branches read from Cobertura `condition-coverage="p% (a/b)"` (`a` covered
conditions, `b` total conditions), deduped by line taking the max covered/total. One JaCoCo
`<counter type="LINE">` and one `<counter type="BRANCH">` emitted per package (single aggregation
level, so the hook's `//counter` sum does not double-count).
