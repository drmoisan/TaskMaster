# Branch-Protection Ruleset Migration — Issue 553

- Performed: 2026-08-14T15:58:00Z
- Ruleset: `main`, id `18572843`, repository `drmoisan/TaskMaster`
- Authorization: explicitly granted by the repository owner before execution. The
  migration task was marked ORCHESTRATOR CONFIRMATION REQUIRED in the plan of record
  (`[P6-T3]`) and was not executed autonomously.
- Head SHA the new context names were captured from: `d83bf377a7f435fdede49220057df68de2f44641`
- Supporting green run: https://github.com/drmoisan/TaskMaster/actions/runs/31814562839

## Why the migration was required

Splitting the monolithic `quality-gates` job retires the context
`Format, build, analyze, and test`. Extracting `actionlint` into a callee workflow
additionally renames its context from the bare `actionlint` to `actionlint / actionlint`,
because a called workflow reports as `<caller job id> / <callee job name>`.

Both previously required contexts therefore stop reporting. With
`strict_required_status_checks_policy: true`, a PR that cannot report a required
context blocks rather than merges, so the pre-migration state over-blocks and never
under-gates. The migration was still necessary to make any merge to `main` possible.

## Before

```
enforcement: active
strict_required_status_checks_policy: true
required contexts (2):
  - actionlint
  - Format, build, analyze, and test
rule types: deletion, non_fast_forward, required_status_checks, pull_request
bypass_actors: []
conditions: { ref_name: { include: ["~DEFAULT_BRANCH"], exclude: [] } }
```

Captured at `ruleset-pre.json`.

## After

```
enforcement: active
strict_required_status_checks_policy: true
required contexts (5):
  - actionlint / actionlint
  - build-analyzers / Build with analyzers and code style enforcement
  - build-nullable / Build with nullable warnings treated as errors
  - format-check / Verify formatting
  - mstest-coverage / Run MSTest suite with coverage
rule types: deletion, non_fast_forward, required_status_checks, pull_request
```

Captured at `ruleset-post.json` by an independent `GET` issued after the `PUT`,
not from the `PUT` response body.

## Method

A single atomic `PUT` carrying the full writable object, never a partial patch:

```
gh api --method PUT repos/drmoisan/TaskMaster/rulesets/18572843 --input ruleset-new.json
```

The payload (`ruleset-new.json`) was built by projecting the pre-PUT object to its six
writable fields — `name`, `target`, `enforcement`, `bypass_actors`, `conditions`,
`rules` — and replacing only
`rules[type=required_status_checks].parameters.required_status_checks`.

The eight read-only fields returned by `GET` were stripped: `id`, `node_id`,
`created_at`, `updated_at`, `_links`, `source`, `source_type`,
`current_user_can_bypass`.

Each new context carries `integration_id: 15368`, matching the value both prior
contexts carried (the GitHub Actions app).

## Pre-PUT verification — five checks, all PASS

| # | Check | Result |
| --- | --- | --- |
| 1 | All eight read-only fields absent from the payload | PASS |
| 2 | Exactly five contexts, verbatim from the captured name list, in order | PASS |
| 3 | `strict_required_status_checks_policy` retained as `true` | PASS |
| 4 | Payload differs from the pre-PUT projection **only** in the contexts array (both sides compared with the contexts array nulled) | PASS |
| 5 | All four rule types preserved in order | PASS |

Check 4 is the load-bearing one: it proves the `PUT` could not silently drop the
`deletion`, `non_fast_forward`, or `pull_request` rules, or alter `bypass_actors` or
`conditions`, while replacing the contexts.

## Post-PUT verification

Set equality against the expected five contexts: **PASS**. Zero missing, zero
unexpected. `enforcement` still `active`, `strict` still `true`, all four rule types
present.

## Under-gating analysis

No window existed in which `main` could be merged to without a gate:

- Before the `PUT`, the two old contexts were required and unreportable, so merges
  blocked.
- The `PUT` was atomic: a single request replaced the old set with the complete new
  set. There was no intermediate state in which the old contexts were removed but the
  new ones not yet added.
- After the `PUT`, all five gates are required and all five report.

The `pull_request` rule, the `deletion` rule, and the `non_fast_forward` rule were
untouched throughout, as proven by pre-PUT check 4 and the post-PUT rule-type listing.

## Effect on other open pull requests

Any PR whose head predates the workflow split runs the old pipeline from its own head
ref, reports the old contexts, and cannot report the five new ones. Such a PR is
blocked until it updates its branch past the merged split, at which point it acquires
the new workflow files and reports the new contexts on its next run. This is
over-blocking, not under-gating.

## Rollback

A single `PUT` of `ruleset-pre.json`'s writable projection restores the previous
required-context set. Reverting the workflow change itself is an ordinary revert PR.

## Artifacts

- `ruleset-pre.json` — full `GET` response before the migration
- `ruleset-new.json` — the exact payload sent
- `ruleset-post.json` — full `GET` response after the migration
