# GitHub Actions Workflows

This directory holds the CI orchestrator and the five callee reusable workflows
it invokes. The split was introduced by issue #553 to replace a single
sequential `quality-gates` job, whose measured wall clock was 444s, with
independent gate jobs that GitHub Actions schedules concurrently and that report
as separate status checks.

## Pipeline overview

`ci.yml` is a pure orchestrator. It declares the workflow `name`, the `on`
triggers, `permissions`, and the workflow-level `concurrency` block, then
references each gate with `uses:`. It contains no inline `steps:`.

| File | Runner | Gate | Timeout |
| --- | --- | --- | --- |
| `ci.yml` | n/a (orchestrator) | Invokes the five callees below | n/a |
| `_actionlint.yml` | `ubuntu-latest` | Downloads actionlint 1.7.7 and lints every workflow file | 10 min |
| `_format-check.yml` | `windows-latest` | `dotnet csharpier check .` | 10 min |
| `_build-analyzers.yml` | `windows-latest` | `msbuild /t:Build` with `EnableNETAnalyzers` and `EnforceCodeStyleInBuild` | 30 min |
| `_build-nullable.yml` | `windows-latest` | `msbuild /t:Rebuild` with `TreatWarningsAsErrors` | 30 min |
| `_mstest-coverage.yml` | `windows-latest` | Plain `msbuild /t:Build`, then `vstest.console.exe` with `/EnableCodeCoverage`; uploads the `test-results` artifact | 30 min |

Structural properties that are deliberate and should not be changed casually:

- **Zero `needs:` edges.** No gate depends on another. The topology shares no
  build output between jobs, so there is no artifact-consumption edge to justify
  an ordering constraint. Every gate runs to completion independently, which is
  what preserves the full diagnostic signal in a single run: a nullable failure
  does not prevent the MSTest result from being reported.
- **The caller owns the concurrency group.** `ci.yml` declares
  `group: ci-${{ github.workflow }}-${{ github.event.pull_request.number || github.ref }}`
  with `cancel-in-progress: true`. Jobs of a called workflow run as part of the
  caller's run and are covered by the caller's group, so a superseded run is
  cancelled as a unit. **The callees declare no `concurrency` block of their
  own.** Callee-level workflow concurrency under `workflow_call` is not clearly
  documented, and the pipeline avoids relying on it rather than depending on
  undefined behavior.
- **Per-job tailored setup.** The msbuild-consuming callees omit
  `setup-dotnet`, the dotnet-tools cache, and `dotnet tool restore`; the format
  callee omits `setup-msbuild`, `setup-nuget`, the `packages` cache, and
  `nuget restore`. Each job installs only what its gate consumes. If a gate ever
  fails because a trimmed setup step was in fact required, restore that specific
  step to that specific callee rather than restoring full setup everywhere.
- **Gate commands are byte-identical to their pre-split forms.** The two msbuild
  invocations (including the `/t:Rebuild` rationale comment and both
  `$LASTEXITCODE` guards), the csharpier invocation, and the vstest invocation
  (including the test-assembly discovery filter and the zero-assembly `throw`)
  were moved, not edited. Treat any change to those blocks as a change to the
  gate's pass criterion.

## Per-stage workflow_dispatch procedure

Every callee declares `workflow_dispatch` in addition to `workflow_call`, so any
single gate can be re-run on its own without re-running the whole pipeline. This
is the intended response to a transient infrastructure failure in one gate.

From the command line:

```
gh workflow run _<name>.yml --ref <branch>
```

For example, to re-run only the MSTest gate against `main`:

```
gh workflow run _mstest-coverage.yml --ref main
gh run list --workflow _mstest-coverage.yml --limit 1 --json databaseId,status,conclusion
gh run watch <run-id> --exit-status
```

The same operation is available in the Actions UI: select the workflow in the
left-hand list and use the **Run workflow** button.

Two caveats:

1. **A standalone dispatch forms its own run.** It is not part of a `ci.yml` run
   and is therefore outside the CI concurrency group. It will not be cancelled by
   a subsequent push, and it will not cancel an in-flight CI run.
2. **A standalone dispatch does not update a pull request's required checks.** It
   produces its own run with its own check; it does not re-report the
   `<caller job> / <callee job>` context that branch protection requires. To turn
   a required context green, re-run the failed job from the pull request's Checks
   tab, which re-runs it within the `ci.yml` run.

The required context names take the form `<caller job id> / <callee job name>` —
the job id used in `ci.yml`, then the `name:` of the job inside the callee. The
five contexts this pipeline reports are, verbatim:

```
actionlint / actionlint
format-check / Verify formatting
build-analyzers / Build with analyzers and code style enforcement
build-nullable / Build with nullable warnings treated as errors
mstest-coverage / Run MSTest suite with coverage
```

Do not hand-write these strings when editing branch protection; capture them from
a live run as described in the next section.

## Branch-protection rename procedure

Splitting or renaming a gate changes the check-run context names that branch
protection requires. The `main` ruleset (id `18572843`) uses
`strict_required_status_checks_policy: true`, so a required context that never
reports blocks merging. That property is fail-closed and is the reason the
procedure below over-blocks rather than under-gates at every step.

Follow this sequence exactly.

1. **Open the pull request and let it run.** For `pull_request` events GitHub
   executes the workflow files from the pull request's head ref, so the pull
   request that changes the pipeline exercises the new pipeline and reports the
   new contexts. Expect the pull request to be blocked by the old required
   context, which can no longer report. That is the fail-closed state and is
   correct.

2. **Confirm the run is green, then capture the exact context names from the live
   head SHA.** Do not assume or hand-write the strings. For jobs of a called
   reusable workflow the context name takes the form
   `<caller job name> / <callee job name>`, and a wrong string is the single most
   likely way to botch the migration.

   ```
   gh api repos/drmoisan/TaskMaster/commits/<head-sha>/check-runs --jq '.check_runs[].name'
   ```

3. **Apply one atomic PUT.** Fetch the current ruleset, build the new body from
   its writable fields only, replace the entire required-contexts array in the
   same request, and retain `strict_required_status_checks_policy: true`.

   ```
   gh api repos/drmoisan/TaskMaster/rulesets/18572843 > ruleset-current.json
   # Build ruleset-new.json from the writable fields of ruleset-current.json:
   #   name, target, enforcement, bypass_actors, conditions, rules
   # Replace the required_status_checks rule's parameters.required_status_checks
   # with the complete new set of {"context": "<name>"} entries captured in step 2.
   gh api --method PUT repos/drmoisan/TaskMaster/rulesets/18572843 --input ruleset-new.json
   ```

   The read-only fields returned by GET — `id`, `node_id`, `created_at`,
   `updated_at`, `_links`, `source`, `source_type`, `current_user_can_bypass` —
   are not part of the payload and must be stripped.

   **A two-step remove-then-add edit is prohibited.** Removing the old context
   before adding the replacements leaves a window in which fewer gates are
   required than intended, which is the only way this procedure can under-gate.

4. **Merge immediately.** Between the PUT and the merge, every other open pull
   request still runs the old pipeline from its own head ref, so it reports the
   old context and lacks the new ones and is blocked until it updates its branch
   past the merge. This is over-blocking, not under-gating, but it is disruptive,
   so keep the interval short. Because `strict` requires the branch to be up to
   date with `main`, update or rebase the branch first if needed.

5. **Verify by GET.** Confirm the ruleset holds exactly the intended contexts and
   that `strict_required_status_checks_policy` is still `true`.

   ```
   gh api repos/drmoisan/TaskMaster/rulesets/18572843
   ```

Record the pre-PUT ruleset JSON, the PUT payload, and the post-PUT GET response
as evidence.

**Rollback.** A single PUT restoring the previous contexts set reverts the merge
policy. Reverting the workflow change itself is an ordinary revert pull request.
The two are independent: reverting the workflows without restoring the contexts
leaves the new contexts required but never reported, which blocks all merges.

## Rules

Two repository rules govern changes in this directory. Read them before editing a
workflow file.

- **`.claude/rules/ci-workflows.md`** — governs `pwsh` steps. A step whose `run:`
  block intentionally invokes a command expected to fail must not let the
  residual non-zero `$LASTEXITCODE` propagate to GitHub Actions; it must reset the
  exit code explicitly or terminate the success path with an explicit `exit 0`.
  No step in this pipeline currently uses that pattern, so no reset is present or
  required. The rule becomes load-bearing the moment someone adds a negative-path
  self-validation step. Note that this does not apply to the gate commands
  themselves: for a gate, a non-zero exit **is** the signal, and the
  `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` guards on the msbuild steps
  exist to propagate it, not to suppress it.

- **`.claude/rules/benchmark-baselines.md`** — governs performance baselines. A
  baseline must be captured in the same runner environment class it is compared
  against; a developer workstation capture must not be compared against a hosted
  runner. When measuring this pipeline's duration, collect from a GitHub-hosted
  run with the same method used for the baseline of record:

  ```
  gh api repos/drmoisan/TaskMaster/actions/runs/<run-id>/jobs
  ```
