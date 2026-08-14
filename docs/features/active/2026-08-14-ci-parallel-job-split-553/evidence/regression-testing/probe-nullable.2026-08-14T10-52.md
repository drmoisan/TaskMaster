# Seeded Probe 2 — Nullable Violation — Issue #553

- Timestamp: 2026-08-14T10-52 (local) / 2026-08-14T14:52Z (UTC)
- Task: [P4-T2] `[expect-fail]`
- Expected outcome: **exactly one red gate — `build-nullable`.** A failing
  `build-nullable` job is the intended result of this task.

## Precondition verified before committing

The task requires a production `*.cs` file carrying `#nullable enable` in a
project whose `.csproj` does **not** set `TreatWarningsAsErrors`, so that the
violation is an error only under the nullable gate's explicit
`/p:TreatWarningsAsErrors=true`.

- File: `UtilitiesCS/EmailIntelligence/IntelligenceFilters.cs` — line 1 is
  `#nullable enable`.
- Project: `UtilitiesCS/UtilitiesCS.csproj`.
- `Select-String`/`grep` for `TreatWarningsAsErrors` in that csproj returns
  exactly **one** hit, and it is inside an XML comment on line 1299:
  `<!-- Issue #181: analyzer-only references (first-party scope). Severities are
  set to suggestion in .editorconfig so none break the nullable
  TreatWarningsAsErrors build. -->`. **No `<TreatWarningsAsErrors>` property
  element is set.** The precondition holds.
- The csproj also carries no `<Nullable>` element, consistent with the repo's
  per-file `#nullable enable` opt-in convention documented in the nullable gate's
  own rationale comment.

## Probe

- Probe commit SHA: `fc4f2be6dbc79f627c10961660312e6a9da5e2a8`
- Commit message: `probe(553): nullable violation — to be reverted`
- Edit: replaced the empty class body with a single method returning a null
  literal from a non-nullable return type:

  ```diff
  -    public class IntelligenceFilters { }
  +    public class IntelligenceFilters
  +    {
  +        public static string ProbeValue() => null;
  +    }
  ```

- The edit is written in canonical CSharpier style so it does not also trip the
  formatting gate; the green `format-check` result below confirms it did not.

Commands:

```
git commit -m "probe(553): nullable violation — to be reverted"
git push origin feature/ci-parallel-job-split-553
gh workflow run ci.yml --ref feature/ci-parallel-job-split-553
gh run watch 31811211865 --interval 20
gh api repos/drmoisan/TaskMaster/actions/runs/31811211865/jobs --jq '.jobs[] | {name, conclusion}'
```

EXIT_CODE: 0 (observation commands succeeded; observed run conclusion `failure`,
the expected result)

## Dispatch-race correction (recorded for auditability)

The first dispatch after the probe push produced run **31811124719**, whose
`head_sha` was `072e19ca` — the *previous* head. `gh workflow run --ref <branch>`
resolved the branch ref before the push had replicated, so that run would have
executed the clean post-revert tree and reported a misleading GREEN.

Action taken: run 31811124719 was **cancelled** (`gh run cancel 31811124719`,
final conclusion `cancelled`), the remote tip was re-confirmed with
`git ls-remote --heads origin feature/ci-parallel-job-split-553` =
`fc4f2be6...`, and a fresh dispatch produced run **31811211865** whose `head_sha`
is `fc4f2be6...`. Only the second run is evidence.

**Verifying that the dispatched run's `head_sha` equals the intended probe SHA
before watching is now a standing step for every probe in this phase.** A probe
observed on the wrong commit is worse than no probe: it produces a confident
false negative.

## Run

- Run: [31811211865](https://github.com/drmoisan/TaskMaster/actions/runs/31811211865)
- Head SHA: `fc4f2be6dbc79f627c10961660312e6a9da5e2a8` (verified to equal the probe SHA)
- Run conclusion: `failure`

## Per-job conclusions

| Job (check-run context) | Conclusion | Expected |
| --- | --- | --- |
| `build-nullable / Build with nullable warnings treated as errors` | **failure** | failure |
| `actionlint / actionlint` | success | success |
| `format-check / Verify formatting` | success | success |
| `build-analyzers / Build with analyzers and code style enforcement` | success | success |
| `mstest-coverage / Run MSTest suite with coverage` | success | success |

**Output Summary: exactly one red gate (build-nullable).**

## Compiler diagnostic (from the failing job log)

```
IntelligenceFilters.cs(12,46): error CS8603: Possible null reference return.
```

The only failing step in the job was the gate step itself,
`Build with nullable warnings treated as errors`; every setup step succeeded.

Failing job URL:
<https://github.com/drmoisan/TaskMaster/actions/runs/31811211865/job/94802025964>

## What this demonstrates

1. **The nullable gate still enforces what it enforced before the split.** The
   same `/t:Rebuild` + `/p:TreatWarningsAsErrors=true` command, transplanted
   byte-identically, promoted CS8603 to an error and failed the build.
2. **The gates are genuinely independent in enforcement semantics, not just in
   scheduling.** The identical source produced only a *warning* in the analyzer
   job and the MSTest job's plain build — both stayed green — because neither
   sets `TreatWarningsAsErrors`. This is the intended separation: the analyzer
   gate and the nullable gate enforce different criteria over the same code, and
   the MSTest job's new plain build deliberately promotes nothing.
3. **`/t:Rebuild` is doing real work.** The violation was caught on a full
   recompile, which is exactly the scenario the in-file rationale comment
   preserves.

## Revert (mandatory)

- Revert commit SHA: `9415ad31f01ef4df48783472df88b34b17a02484`
- Message: `Revert "probe(553): nullable violation — to be reverted"`
- Command: `git revert --no-edit fc4f2be6dbc79f627c10961660312e6a9da5e2a8`
- Verified restored: the file is byte-identical to its pre-probe state.
  `git diff 0b016c81 HEAD -- UtilitiesCS/EmailIntelligence/IntelligenceFilters.cs`
  returns **0 lines**, which also confirms probe 1's revert remains intact.
- Pushed; remote tip is `9415ad31`.

The probe run completed before the revert was pushed, so `cancel-in-progress`
did not cancel it.

## Acceptance ([P4-T2])

- Artifact shows exactly the nullable gate red and the other four green.
- Revert commit exists on the branch and the file is byte-restored.
- Spec seeded-condition checkbox 4 ("A deliberate nullable violation fails only
  the nullable gate") is checked off with this artifact as the evidence pointer.
