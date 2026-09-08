# Phase 6 — Clean-Pass Declaration for P6-T1 through P6-T6 (P6-T7)

Timestamp: 2026-09-08T08-38

Command: none. This task is a declaration recorded over the six artifacts produced by P6-T1 through P6-T6.

EXIT_CODE: 0

Output Summary:

## The six artifact paths

1. `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/evidence/qa-gates/p6-t1-csharpier-format.2026-09-07T22-12.md`
2. `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/evidence/qa-gates/p6-t2-csharpier-check.2026-09-07T22-12.md`
3. `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/evidence/qa-gates/p6-t3-msbuild-analyzers.2026-09-07T22-12.md`
4. `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/evidence/qa-gates/p6-t4-msbuild-nullable.2026-09-07T22-12.md`
5. `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/evidence/qa-gates/p6-t5-vstest.2026-09-07T22-12.md`
6. `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/evidence/qa-gates/p6-t6-coverage.2026-09-07T22-12.md`

## Clean-pass declaration

All six artifacts were produced in **one uninterrupted pass with no restart between them**, in the order P6-T1, P6-T2, P6-T3, P6-T4, P6-T5, P6-T6.

**Number of restarts that preceded the final pass: 0.** The sequence was executed once. P6-T1 did not rewrite any file, so the phase-head restart rule did not engage; P6-T5's failed count was 0, so the P6-T6 part (b) restart rule did not engage; and the P6-T6 attachment-count comparison found equal counts, so the blended-collection restart rule did not engage.

## The four toolchain stages, in order

1. **Format** — `dotnet tool run csharpier format` over the eight Write Set paths (P6-T1), verified read-only by `dotnet tool run csharpier check .` (P6-T2).
2. **Analyze** — `msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (P6-T3).
3. **Type-check** — `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` (P6-T4).
4. **Test** — `vstest.console.exe` over the three named assemblies with `/EnableCodeCoverage` and `/InIsolation` (P6-T5), with coverage converted and compared (P6-T6).

## The seven AC1 test methods and the P2-T7 artifact

The seven AC1 degradation methods named in D15 items 1 through 7:

1. `FolderArray_RecentsOnlyWithThrowingArchiveRoot_ReturnsEntriesUnchanged`
2. `FolderArray_SuggestionsOnlyWithThrowingArchiveRoot_ReturnsEntriesUnchanged`
3. `FolderArray_SuggestionsAndRecentsWithThrowingArchiveRoot_ReturnsEntriesUnchanged`
4. `FolderRowArray_RecentsOnlyWithThrowingArchiveRoot_ReturnsEntriesUnchanged`
5. `FolderRowArray_SuggestionsOnlyWithThrowingArchiveRoot_ReturnsEntriesUnchanged`
6. `FolderRowArray_SuggestionsAndRecentsWithThrowingArchiveRoot_ReturnsEntriesUnchanged`
7. `FolderArrayAndFolderRowArray_WithThrowingArchiveRoot_ProduceIdenticalText`

Every one of the seven is recorded as `Passed` in the P6-T5 artifact. The scoped green run for them is `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/evidence/regression-testing/p2-t7-defect-a-green.2026-09-07T22-12.md`, which records `EXIT_CODE: 0` with a failed count of 0. Their fail-before evidence is `p1-t6-defect-a-red.2026-09-07T22-12.md`, in which all seven are among the ten recorded failures.

## Figures restated from each artifact, as the later check-off tasks read them

- **P6-T2 exit code: 0.** The read-only formatter gate reported no file needing formatting, over 1613 files.
- **P6-T3:** `0 Error(s)`, `0 Warning(s)`, and a count of **0** lines containing `Skipping target "CoreCompile"` in a log of 69105 lines.
- **P6-T4:** `0 Error(s)`, `0 Warning(s)`, and a count of **0** lines containing `Skipping target "CoreCompile"` in a log of 69300 lines.
- **P6-T5 failed count: 0.** The switches and filter that run used were `/EnableCodeCoverage`, `/InIsolation`, and the D6 hazard filter `TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests`. Total 6675, Passed 6675, Skipped 0.
- **P6-T5 carve-out disposition: a failed count of 0.** The failing set was empty, so it was not a non-empty subset of the carve-out set, the D7 and D18 protocols did not engage, and no scoped re-run was performed or required. No issue number needed to be named for an exercised member, because no member was exercised under either protocol.
- **The amended AC7 carve-out line** that P5-T9 wrote into `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/spec.md`, transcribed exactly:

```
  reporting zero failures other than a carve-out member handled under the issue-803 or issue-780
```

  That line names both the issue-803 and the issue-780 protocol.

- **Locator for the `>= 80%` restatement** in that same file: the single line carrying the fixed string `UT2: repository-wide line coverage`. It occurs exactly once in the file. It is recorded here as that fixed string rather than as a line number, because P5-T9 inserted two Write Set entries, a Write Set scoping note, a Write Set parenthetical amendment, and a fourth `### Known local test hazards` item into the same file, all above this line, and each shifts it down. Its full text is:

```
`CLAUDE.md` UT2: repository-wide line coverage `>= 80%` on the testable denominator; any new module,
```

- **P6-T6 numeric coverage figures:** root `line-rate` 0.7367916823028189 (73.679 percent), `lines-covered` 166887, `lines-valid` 226505. Against the P0-T12 baseline of 0.7363901154028049, 166609 and 226251. New accessor file aggregate line rate 1.000000 over 19 lines, from 1 matched class element. Changed-line intersection on `StoreWrapperController.Display.cs`: 2 matched class elements, intersection size 6, zero-hit count 0.
- **P6-T6 named comparison branch: Branch A.** `lines-valid` moved by 254 against a 1 percent tolerance of 2262.51, so the denominators are comparable; the post-change rate of 0.7367916823028189 is at least the baseline rate less 0.005, which is 0.7313901154028049.

## AC7 disposition

**Every clause of AC7 except the `>= 80%` repository-floor clause is satisfied**, and the artifact satisfying each is named:

| AC7 clause | Satisfied by | Observation |
| --- | --- | --- |
| `csharpier format .` then `csharpier check .` reporting zero files needing formatting | P6-T1 and P6-T2 | format rewrote nothing; check exit 0 over 1613 files |
| First `msbuild ... /t:Rebuild ...` reporting `0 Error(s)` and `0 Warning(s)` | P6-T3 | both counts 0 |
| Second `msbuild ... /t:Rebuild ...` reporting `0 Error(s)` and `0 Warning(s)` | P6-T4 | both counts 0 |
| Captured logs containing zero occurrences of `Skipping target "CoreCompile"` | P6-T3 and P6-T4 | 0 and 0, over logs of 69105 and 69300 lines |
| `vstest.console.exe` over the three assemblies with `/EnableCodeCoverage`, `/InIsolation` and the documented hazard filter, reporting zero failures other than a carve-out member | P6-T5 | failed count 0; no carve-out member needed |
| Coverage of the new accessor `>= 90%` | P6-T6 part (a) | 1.000000 |
| Coverage of the latch gate, changed lines not losing coverage | P6-T6 part (b) | intersection size 6, zero-hit count 0 |
| One uninterrupted pass in the stated order | this artifact | 0 restarts |
| Evidence written under the feature `evidence/` folder | all six artifacts | canonical paths listed above |
| Repository line coverage `>= 80%` **on the testable denominator** | **not satisfied — see below** | not computed by this plan |

**The `>= 80%` clause is stated over the testable denominator defined by `CLAUDE.md` UT2** and restated in `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/spec.md` on the single line carrying the fixed string `UT2: repository-wide line coverage`. That denominator excludes VSTO add-in lifecycle classes, WinForms form-derived and Designer-generated code, and Outlook Interop event-handler classes without an injectable seam.

**No task of this plan computes that denominator.** The figure this plan does measure is the **raw root Cobertura `line-rate` over all instrumented code**, which includes every one of those excluded categories.

**P0-T12 recorded that raw figure at 73.639, below 80, before this plan edited any source file**, and carried the line `PRE-EXISTING SUB-80 BASELINE: YES` against it. The post-change raw figure is 73.679, so this change moved it upward by 0.040 percentage points.

AC7 is therefore left unchecked. `acceptance-criteria-tracking` rule 4 requires an acceptance criterion that cannot be fully verified to be left unchecked with its gap documented, and this plan verifies every clause of AC7 except that one. Amending the `>= 80%` clause in `spec.md` is not an available alternative: unlike the AC7 carve-out line that P5-T9 corrects, that clause is a repository policy requirement traceable to `CLAUDE.md` UT2 rather than a factual statement about this workstation, so rewriting it inside a feature spec would waive the policy rather than record an observation.
