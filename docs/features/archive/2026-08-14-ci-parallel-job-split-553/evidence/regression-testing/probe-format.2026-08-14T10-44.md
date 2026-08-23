# Seeded Probe 1 — Formatting Violation — Issue #553

- Timestamp: 2026-08-14T10-44 (local) / 2026-08-14T14:44Z (UTC)
- Task: [P4-T1] `[expect-fail]`
- Expected outcome: **exactly one red gate — `format-check`.** A failing
  `format-check` job is the intended result of this task; it demonstrates fault
  isolation, not a defect.

## Probe

- Probe commit SHA: `5a6068955422f1f114af6332bd1083e1d3a68341`
- Commit message: `probe(553): formatting violation — to be reverted`
- File: `UtilitiesCS/EmailIntelligence/IntelligenceFilters.cs`
- Edit: a formatting-only indentation change on one line, from four leading
  spaces to twelve:

  ```diff
  @@ -7,5 +7,5 @@ using System.Threading.Tasks;

   namespace UtilitiesCS
   {
  -    public class IntelligenceFilters { }
  +            public class IntelligenceFilters { }
   }
  ```

- Diff scope: 1 file changed, 1 insertion, 1 deletion. The change alters only
  leading whitespace. It introduces no compiler diagnostic, so the analyzer,
  nullable, and MSTest jobs must remain green — which is precisely what makes
  this a clean single-gate probe.

Commands:

```
git commit -m "probe(553): formatting violation — to be reverted"
git push origin feature/ci-parallel-job-split-553
gh workflow run ci.yml --ref feature/ci-parallel-job-split-553
gh run watch 31810574239 --interval 20
gh api repos/drmoisan/TaskMaster/actions/runs/31810574239/jobs --jq '.jobs[] | {name, conclusion}'
```

EXIT_CODE: 0 (all observation commands succeeded; the observed run conclusion is
`failure`, which is the expected result for this task)

## Run

- Run: [31810574239](https://github.com/drmoisan/TaskMaster/actions/runs/31810574239)
- Head SHA: `5a6068955422f1f114af6332bd1083e1d3a68341`
- Run conclusion: `failure`

## Per-job conclusions

| Job (check-run context) | Conclusion | Expected |
| --- | --- | --- |
| `format-check / Verify formatting` | **failure** | failure |
| `actionlint / actionlint` | success | success |
| `build-analyzers / Build with analyzers and code style enforcement` | success | success |
| `build-nullable / Build with nullable warnings treated as errors` | success | success |
| `mstest-coverage / Run MSTest suite with coverage` | success | success |

**Output Summary: exactly one red gate (format-check).**

## Step-level attribution inside the failing job

The failure is attributable to the gate command itself, not to setup:

| Step | Conclusion |
| --- | --- |
| Set up job | success |
| Checkout repository | success |
| Setup .NET SDK | success |
| Cache dotnet tools | success |
| Setup CSharpier | success |
| **Verify formatting** (`dotnet csharpier check .`) | **failure** |
| Post Cache dotnet tools | skipped |
| Post Setup .NET SDK | skipped |
| Post Checkout repository | success |
| Complete job | success |

Failing job URL:
<https://github.com/drmoisan/TaskMaster/actions/runs/31810574239/job/94799943550>

## What this demonstrates

1. **Failure isolation works.** The formatting violation reddened one context and
   left the other four to run to completion and report independently. Under the
   pre-split monolith the same violation would have failed the single
   `Format, build, analyze, and test` check and, because `csharpier check` ran
   before the two builds and the test suite, would have prevented the analyzer,
   nullable, and MSTest results from being produced at all.
2. **Attribution is possible from the checks list alone.** The red context names
   the gate; no log inspection is needed to know a formatting rule was violated.
3. **The zero-`needs:` topology holds under failure.** No job was skipped or
   cancelled as a consequence of the format job failing.

## Revert (mandatory)

- Revert commit SHA: `072e19ca1e62c99ab67434ce41ba5d3793ee3257`
- Message: `Revert "probe(553): formatting violation — to be reverted"`
- Command: `git revert --no-edit 5a6068955422f1f114af6332bd1083e1d3a68341`
- Verified restored: line 10 of `IntelligenceFilters.cs` reads
  `    public class IntelligenceFilters { }` (four leading spaces), confirmed
  byte-exactly via `sed -n '10p' ... | cat -A`.
- Pushed to `origin/feature/ci-parallel-job-split-553`.

The probe run was allowed to complete **before** the revert was pushed, so
`cancel-in-progress: true` on the caller's concurrency group did not cancel it.

Net effect of this task on the branch diff: **zero.** Verified in aggregate by
[P5-T3].

## Acceptance ([P4-T1])

- Artifact shows exactly the format gate red and the other four green.
- Revert commit exists on the branch and the file is byte-restored.
- Spec seeded-condition checkbox 3 ("A deliberate formatting violation fails only
  the formatting gate and reports a distinct red check") is checked off with this
  artifact as the evidence pointer.
