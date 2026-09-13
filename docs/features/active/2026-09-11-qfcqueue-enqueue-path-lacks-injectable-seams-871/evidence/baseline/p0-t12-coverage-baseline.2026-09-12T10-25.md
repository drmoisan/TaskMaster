# P0-T12 — Coverage baseline: BLOCKED by a pre-existing tooling defect

**Status: acceptance NOT met. Phase 0 execution stopped at this task.**

Timestamp: 2026-09-13T05-00
Command: pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput docs\features\active\2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871\evidence\baseline\coverage-baseline.2026-09-12T10-25.cobertura.xml
EXIT_CODE: 1
ExpectedExitCode: 0
ThresholdAssertion: PASSED

## Tokens CMD-COVERAGE printed

```
RUNNER-EXIT: 1
COVERAGE-ARTIFACT-WRITTEN
THRESHOLD-ASSERTION: PASSED
```

## Why the acceptance condition is not met

The acceptance condition has three clauses. The first is satisfied and the other two are not.

**Clause 1, satisfied.** The command printed the token `COVERAGE-ARTIFACT-WRITTEN`. A Cobertura
document exists at the stated path.

**Clause 2, NOT satisfied.** The condition requires a filename attribute for each of
`QuickFiler/Controllers/QfcQueue.cs` and `QuickFiler/Controllers/QfcQueue.Enqueue.cs` in the
backslash-separated repository-relative form the post-processor writes. The document that was written
carries those two files as absolute host paths instead. Measured, by enumerating every class element
filename attribute in the document and filtering for the queue type:

```
CLASS-COUNT: 3166
QFCQUEUE-MATCHES:
<absolute-host-worktree-root>\QuickFiler\Controllers\QfcQueue.cs
<absolute-host-worktree-root>\QuickFiler\Controllers\QfcQueue.Enqueue.cs
```

The repository-relative form the condition names, `QuickFiler\Controllers\QfcQueue.cs`, does not
occur as a filename attribute anywhere in the document. The document is the raw output of the
coverage tool, not the post-processed document, so the condition is unsatisfiable from this run
rather than merely unsatisfied by a narrow margin.

**Clause 3, NOT satisfied.** The package-level and class-level figures the condition requires are to
be obtained from the two coverage helper functions, whose contract is the post-processed document.
They cannot be recorded as the condition specifies while clause 2 fails. The raw document-level
figures that are readable are recorded below as an observation, clearly labelled as raw, so that the
run is auditable; they are not the figures the condition asks for and this artifact does not present
them as such.

## Additional self-inconsistency the task text itself directs be reported

The task states: "A `ThresholdAssertion:` of `THREW` paired with an exit code of 0, or of `PASSED`
paired with a non-zero exit code, is a failure of this task and is reported." The observed pair is
`PASSED` with exit code 1, which is the second of those two prohibited combinations. It is reported
here.

The pair is not contradictory once the mechanism is known: the runner never reached its threshold
assertion, so the assertion neither passed nor threw, and the CMD-COVERAGE detector — which infers
the branch by searching the captured output for the assertion's failure message — found no such
message and therefore reported `PASSED`. The detector cannot distinguish "the assertion ran and
passed" from "the assertion never ran". That is a property of the detector, not a second defect.

## Measured mechanism

Every element below was measured in this worktree during this task, not carried over from a report.

1. **Three tests fail under the runner, and the same assembly is fully green without it.** The run
   reported `Total tests: 1394`, `Passed: 1391`, `Failed: 3`. The three failures are
   `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`,
   `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` and
   `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`. All three fail with
   `System.TypeInitializationException` for `Deedle.Reflection`, whose inner cause is
   `System.IO.FileNotFoundException: Could not load file or assembly 'netstandard, Version=2.1.0.0'`.
   P0-T11 ran the same assembly minutes earlier with no settings file and reported 1394 of 1394
   passed at exit code 0, so the three failures are attributable to the run configuration and not to
   the assembly.

2. **The runner imposes class-level parallelism that the repository pipeline does not.** The runner
   appends its own CLI runsettings to the inner test command at line 76 of
   scripts/vscode/Invoke-MSTestWithCoverage.ps1, resolved at line 33. That settings file contains
   nothing but MSTest parallelisation:

   ```
   <RunSettings>
     <MSTest>
       <Parallelize>
         <Workers>0</Workers>
         <Scope>ClassLevel</Scope>
       </Parallelize>
     </MSTest>
   </RunSettings>
   ```

   The run log confirms the setting took effect: `Test Parallelization enabled for ... (Workers: 24,
   Scope: ClassLevel)`. The repository pipeline passes no settings file.

3. **The runner throws before it post-processes.** Verified by line number in the runner script:
   the terminating `throw` is at line 236, the Koverage post-processing call is at line 341, and the
   document-level threshold assertion is at line 344. A non-zero child exit code therefore ends the
   run before the document is ever rewritten into the repository-relative form, and before the
   threshold assertion is evaluated. The terminating message is reproduced verbatim below.

   RUNNER-TERMINATED: MSTest with coverage failed with exit code 1

4. **The document on disk is consistent with that ordering.** Its document-level figures are the raw,
   unfiltered ones rather than the first-party denominator the post-processor produces:

   ```
   RawDocumentLineRate: 0.20137719396729165
   RawDocumentLinesCovered: 16143
   RawDocumentLinesValid: 80163
   ```

   The 80163 valid lines include third-party packages. The document carries 11 package elements, of
   which `QuickFiler` is one, and the remainder include log4net, Mono.Reflection, SVGControl,
   Microsoft.IO.RecyclableMemoryStream, System.Linq.Async and System.Interactive. A post-processed
   document scoped to the first-party denominator would not present those.

## Assessment

This is a pre-existing defect in repository tooling, reachable by any item that runs the coverage
runner in this worktree. It is not caused by any change this item has made: at the point of this task
the only tracked edits in the worktree are the Phase 0 check-off marks in the plan file and the Phase
0 evidence artifacts, and no production or test source file has been touched.

The plan's own prose about the artifact surviving a throw describes the threshold assertion at line
344. It does not anticipate the earlier throw at line 236, which is what occurred.

## Actions deliberately not taken

- The runner script and the CLI runsettings file were not edited. Both are outside this item's Write
  Set, and the runsettings file is in no item's declared change radius.
- The coverage document was not post-processed by hand to manufacture the filename form the
  condition names. Doing so would record a baseline measured from a run in which three tests failed,
  which would understate coverage for the affected class and corrupt the figure that later phases
  compare against.
- This task was not recorded as skipped or as passing. Its plan checkbox remains unchecked.

## Artifact hygiene

The raw Cobertura document was written to the path the plan states, as
coverage-baseline.2026-09-12T10-25.cobertura.xml in this directory, so that any acceptance condition
reading it is satisfiable as written. It is deliberately left unstaged: under the repository decision
recorded on issue 671 only projections may be committed, and a Cobertura document written under a
feature evidence directory is not matched by the repository ignore file, so staging it would commit
raw coverage output.

Output Summary: The coverage runner exited 1. Three QuickFiler tests failed with a Deedle
`netstandard 2.1` type-initialiser failure caused by the class-level parallelism the runner's own
settings file imposes; the same assembly passed 1394 of 1394 in P0-T11 without that settings file.
The runner's throw at line 236 precedes its Koverage post-processing at line 341, so the document on
disk is raw: its class filename attributes are absolute host paths rather than the repository-relative
backslash form this task requires, and its document-level rate of 0.201 is an unfiltered denominator.
Acceptance clause 1 is met; clauses 2 and 3 are not, and the observed `PASSED` plus exit code 1 pair
is one the task text explicitly designates a failure to be reported. Phase 0 execution stopped here.
