# Seeded Probe 3 — Deliberate Test Failure — Issue #553

- Timestamp: 2026-08-14T11-01 (local) / 2026-08-14T15:01Z (UTC)
- Task: [P4-T3] `[expect-fail]`
- Expected outcome: **exactly one red gate — `mstest-coverage`.** A failing
  `mstest-coverage` job is the intended result of this task.

## Probe

- Probe commit SHA: `a55ccdfc2e7980125ab36c76a84c2d59b29fb8e6`
- Commit message: `probe(553): deliberate test failure — to be reverted`
- File: `UtilitiesCS.Test/Extensions/ExtToChar_Tests.cs`
- Test: `ExtToChar_CurrentlyExposesNoPublicMethods`
- Edit: inverted the single assertion.

  ```diff
  -            publicStaticMethods.Should().BeEmpty();
  +            publicStaticMethods.Should().NotBeEmpty();
  ```

Target selection rationale — the test satisfies every constraint the task sets:

- **Fast and deterministic.** It is a pure reflection query over
  `typeof(ExtToChar)` with no I/O, no Outlook interop, no timing dependency, and
  no shared mutable state. Inverting the assertion fails deterministically
  because the reflected method array genuinely is empty.
- **Not `TestCategory=LiveOutlook`.** The test class carries no `TestCategory`
  attribute at all, so it is not excluded by the gate's
  `/TestCaseFilter:"TestCategory!=LiveOutlook"` and is guaranteed to execute.
- **CSharpier-clean and free of new compiler diagnostics.** A one-token change
  to an existing FluentAssertions call; `NotBeEmpty()` is a valid method on the
  same assertion type. The green `format-check`, `build-analyzers`, and
  `build-nullable` results below confirm both properties.

Commands:

```
git commit -m "probe(553): deliberate test failure — to be reverted"
git push origin feature/ci-parallel-job-split-553
git ls-remote --heads origin feature/ci-parallel-job-split-553   # confirm tip == probe SHA
gh workflow run ci.yml --ref feature/ci-parallel-job-split-553
gh run watch 31811867381 --interval 20
gh api repos/drmoisan/TaskMaster/actions/runs/31811867381/jobs --jq '.jobs[] | {name, conclusion}'
```

EXIT_CODE: 0 (observation commands succeeded; observed run conclusion `failure`,
the expected result)

The dispatched run's `head_sha` was verified to equal the probe SHA **before**
watching, applying the standing step adopted after the dispatch race recorded in
`probe-nullable.2026-08-14T10-52.md`. No race occurred this time.

## Run

- Run: [31811867381](https://github.com/drmoisan/TaskMaster/actions/runs/31811867381)
- Head SHA: `a55ccdfc2e7980125ab36c76a84c2d59b29fb8e6` (verified == probe SHA)
- Run conclusion: `failure`

## Per-job conclusions

| Job (check-run context) | Conclusion | Expected |
| --- | --- | --- |
| `mstest-coverage / Run MSTest suite with coverage` | **failure** | failure |
| `actionlint / actionlint` | success | success |
| `format-check / Verify formatting` | success | success |
| `build-analyzers / Build with analyzers and code style enforcement` | success | success |
| `build-nullable / Build with nullable warnings treated as errors` | success | success |

**Output Summary: exactly one red gate (mstest-coverage).**

## Test-run detail (from the failing job log)

```
Failed ExtToChar_CurrentlyExposesNoPublicMethods
Total tests: 6435
Passed:  6434
Failed:     1
```

The isolation is exact at the test level as well as the gate level: of 6435
executed tests, the single failure is the probed test. The only failing step in
the job was the gate step `Run MSTest suite with coverage`; every setup step and
the job's own plain build succeeded.

Failing job URL:
<https://github.com/drmoisan/TaskMaster/actions/runs/31811867381/job/94804173833>

## What this demonstrates

1. **The MSTest gate still enforces test outcomes after the split**, using the
   byte-identical vstest invocation with `/EnableCodeCoverage /InIsolation
   /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`.
2. **Test discovery is intact under the job's own plain build.** 6435 tests were
   discovered and executed from assemblies this job built itself, with no
   inherited build output from any other job. The transplanted discovery filter
   (match `\bin\Debug\`, exclude `\obj\` and `\ref\`) and the zero-assembly
   `throw` guard behaved correctly: had discovery regressed to zero assemblies,
   the guard would have thrown rather than reporting a false pass.
3. **A test failure no longer masks the other gates.** The formatting, analyzer,
   and nullable results were all produced and reported green in the same run.

## Revert (mandatory)

- Revert commit SHA: `ad28ea81e85ed09399feb4275828d00efeccc790`
- Message: `Revert "probe(553): deliberate test failure — to be reverted"`
- Command: `git revert --no-edit a55ccdfc2e7980125ab36c76a84c2d59b29fb8e6`
- Verified restored: line 23 reads `publicStaticMethods.Should().BeEmpty();`
- Pushed to `origin/feature/ci-parallel-job-split-553`.

## Cumulative probe-neutrality check (all three probes)

```
git diff --name-only 0b016c81 HEAD -- '*.cs' '*.csproj' '*.props' '*.targets' '**/packages.config' '**/app.config'
```

returns **0 files**. All three probe commits are fully cancelled by their
reverts; the branch carries no residual C# or project-file change relative to the
pre-probe workflow commit. [P5-T3] repeats this check against the merge base as
the formal verification.

Commit sequence on the branch:

```
ad28ea81 Revert "probe(553): deliberate test failure — to be reverted"
a55ccdfc probe(553): deliberate test failure — to be reverted
9415ad31 Revert "probe(553): nullable violation — to be reverted"
fc4f2be6 probe(553): nullable violation — to be reverted
072e19ca Revert "probe(553): formatting violation — to be reverted"
5a606895 probe(553): formatting violation — to be reverted
0b016c81 ci(#553): split monolithic quality-gates job into parallel reusable workflows
```

Each probe is immediately followed by its own revert; no probe commit is left
uncancelled at any point after its run completed.

## Acceptance ([P4-T3])

- Artifact shows exactly the MSTest gate red and the other four green.
- Revert commit exists on the branch and the assertion is restored.
- Spec seeded-condition checkbox 5 ("A deliberate test failure fails only the
  MSTest gate") is checked off with this artifact as the evidence pointer.
