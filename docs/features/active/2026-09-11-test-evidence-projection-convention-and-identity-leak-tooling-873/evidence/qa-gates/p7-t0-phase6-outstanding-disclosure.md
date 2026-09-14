# Phase 6 Outstanding — Disclosure For This Phase 7 Pass

Timestamp: 2026-09-13T07-17
Task: none. This artifact is required by the delegation that authorised this Phase 7 pass, not by a
numbered task in the plan.

## What is being disclosed

The atomic-plan contract expects the final QA loop to run last. In this delivery it does not. This
Phase 7 pass was taken with Phase 6 tasks P6-T2, P6-T3, P6-T4 and P6-T5 outstanding. P6-T1 is
complete. Phase 6 will run after this Phase 7 pass rather than before it.

The four outstanding tasks are the two end-to-end runs of the coverage entry point, the default-name
scan over their output, and the AC23 check-off that depends on all three. They are blocked on a
pre-existing defect that this delivery did not introduce and that this Phase 7 pass was not
authorised to repair.

The consequence for this pass is stated plainly rather than minimised: the Phase 7 gates below were
measured against a tree in which no end-to-end run of the coverage entry point has yet been observed.
AC23 is therefore recorded as OUTSTANDING in the P7-T14 acceptance summary and its checkbox in
`spec.md` is left unmarked.

## Determination 1 — can Phase 6 modify any TRACKED file?

Yes. Phase 6 can and will modify tracked files.

This determination was derived by reading the P6-T2 through P6-T5 task text in
`docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/plan.2026-09-12T10-26.md`
and by reading `scripts/vscode/Invoke-MSTestWithCoverage.ps1` end to end, not by inference from the
plan's prose about itself.

The specific tracked paths Phase 6 would modify:

| Path | How Phase 6 modifies it | Which task |
|---|---|---|
| `docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p6-t2-default-output-run.md` | created | P6-T2 |
| `docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p6-t3-external-output-run.md` | created | P6-T3 |
| `docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p6-t4-default-name-scan.md` | created | P6-T4 |
| `docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/spec.md` | AC23's checkbox changed from `- [ ]` to `- [x]` | P6-T5 |
| `docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/plan.2026-09-12T10-26.md` | four checkboxes marked | P6-T2 through P6-T5 |

Every one of those five paths sits beneath this feature folder.

## Determination 2 — can Phase 6 modify a tracked file that ANY Phase 7 gate measures?

No, for Phase 6 as the plan writes it. The reasoning is recorded below so a reader can check it
rather than take it.

The gate-measured populations are: the PowerShell scripts and tests under `scripts/vscode` and
`tests/scripts/vscode`, which P7-T1, P7-T2, P7-T3, P7-T7 and P7-T8 measure; the C# sources, which
P7-T4, P7-T5 and P7-T6 measure; and the project files, which P7-T5 and P7-T6 measure. None of the
five paths in the table above belongs to any of those three populations. All five are Markdown under
the feature folder, and Markdown is not an input to CSharpier, to MSBuild, to PSScriptAnalyzer or to
Pester.

That leaves the runtime writes the two end-to-end runs perform. Reading
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, the entry point writes to exactly four places, and
all four resolve beneath the repository `coverage` directory:

1. The resolved coverage output path, from the `CoverageOutput` parameter joined against the
   repository root, whose parent directory the entry point creates if absent.
2. A derived coverage settings file. `Get-DerivedCoverageSettingsPath` places it adjacent to the
   requested output, and the entry point throws unless the derived file's directory equals the output
   directory, so it cannot escape that directory. It is removed in a `finally` block.
3. The projection, written to the output file's directory.
4. The resolved results directory and the test-result summary beside it, from the `ResultsDirectory`
   parameter joined against the repository root. The results directory is also where the test console
   writes its trx document, because this delivery passes `/ResultsDirectory:` explicitly.

P6-T2 leaves both parameters at their defaults, `coverage\coverage.cobertura.xml` and
`coverage\test-results`. P6-T3 points both beneath `coverage\tm873-external-output`. Every resulting
path is confirmed ignored:

```
Command: git check-ignore -v coverage/coverage.cobertura.xml coverage/test-results/mstest-coverage-run.trx coverage/tm873-external-output/coverage.cobertura.xml

.gitignore:144:coverage/*	coverage/coverage.cobertura.xml
.gitignore:144:coverage/*	coverage/test-results/mstest-coverage-run.trx
.gitignore:144:coverage/*	coverage/tm873-external-output/coverage.cobertura.xml
```

The entry point reads `coverage.config` and the runsettings document and writes neither. It does not
invoke the repository build wrapper and does not invoke `Sync-PackageReferences.ps1`, so nothing on
its path rewrites a project file hint path. It resolves the test console through vswhere and runs
`dotnet-coverage` directly.

Conclusion for determination 2, stated without softening: Phase 6 as written cannot modify any
tracked file that a Phase 7 gate measures, so this Phase 7 pass does not need a confirming re-run on
account of Phase 6 running afterwards.

## The one case that would change that conclusion, stated rather than omitted

The conclusion above is about Phase 6 as the plan writes it. It is not a claim that nothing arising
from Phase 6 could ever require a Phase 7 re-run.

If either end-to-end run fails in a way that is traced to a defect in this delivery's own wiring, the
repair would land in `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which P7-T1, P7-T2, P7-T3 and
P7-T8 all measure. In that case this Phase 7 pass would be invalidated and the format, analyze, test
and file-size gates would need to be re-run against the repaired tree. The C# gates would not, since
that file is not a compilation input.

No task in Phase 6 describes such a repair, and the plan's Phase 6 flakiness rule directs the
executor to record and report a pre-existing failure rather than modify anything. The case is
recorded here because it is the only route by which a Phase 7 gate's input could change, and leaving
it unstated would make the conclusion above read stronger than the evidence supports.

## What this pass does and does not certify

Certified by this pass: the PowerShell format, analyzer and test gates; the C# format, analyzer and
nullable gates; per-file new-code coverage for the two new part files; the file-size ceiling and the
helpers growth bound; and the changed-file inventory. AC15, AC20, AC21 and AC22 are checked off on
that evidence.

Not certified by this pass: AC23, which requires the two end-to-end observations Phase 6 has not
made. It is recorded as OUTSTANDING and its checkbox is not marked.
