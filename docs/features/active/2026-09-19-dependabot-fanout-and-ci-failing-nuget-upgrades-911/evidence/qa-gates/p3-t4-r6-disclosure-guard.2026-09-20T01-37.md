# R6 — The Disclosure Step Is Guarded and Replaces a Delimited Block

- Timestamp: 2026-09-20T08-58-40
- Task: [P3-T4]
- Finding: R6, Major
- EXIT_CODE: 0

## The New `if:` Line, Verbatim

`.github/workflows/dependabot-repair.yml:120`:

```
        if: steps.repair.outputs.written-count != '0' || steps.repair.outputs.skip-count != '0'
```

The guard names `skip-count` as well as `written-count` because **AC20 requires the skipped block
whenever the run recorded a skip**, and a run that skipped an incompatible package without
writing anything must still disclose. A guard on `written-count` alone would have suppressed
exactly that disclosure.

## The Full Rewritten Body-Composition Block, Verbatim

`.github/workflows/dependabot-repair.yml:114-148`:

```yaml
      - name: Disclose the repairs on the pull request
        # A run that neither wrote nor skipped anything has nothing to disclose, and the
        # unguarded step appended a "No repairs were applied." block on every completed CI
        # run on the branch. skip-count is named as well as written-count because AC20
        # requires the skipped block whenever the run recorded a skip, and a run that skipped
        # an incompatible package without writing anything must still disclose.
        if: steps.repair.outputs.written-count != '0' || steps.repair.outputs.skip-count != '0'
        shell: pwsh
        env:
          GH_TOKEN: ${{ steps.app-token.outputs.token }}
        run: |
          $number = @(gh pr list --head $env:HEAD_BRANCH --state open --json number --jq '.[].number')
          if ($number.Count -eq 0) {
            Write-Warning "No open pull request for branch $env:HEAD_BRANCH; nothing to disclose."
            exit 0
          }
          $existing = gh pr view $number[0] --json body --jq '.body'
          $report = [System.IO.File]::ReadAllText('${{ steps.repair.outputs.report-path }}')
          # Replace rather than append. The repair push triggers a new CI run whose completion
          # re-fires this workflow, so an appending edit grows the body without bound over the
          # life of the pull request. Stripping any prior delimited block first makes the edit
          # idempotent: one block, however many times the workflow runs.
          $blockPattern = '(?s)<!-- dependabot-repair:begin -->.*?<!-- dependabot-repair:end -->'
          # The two markers are derived from the strip pattern rather than written a second
          # time, so the block this step emits and the block it strips cannot drift apart.
          # Substring(4) drops the (?s) option prefix; the split is on the .*? between them.
          $marker = $blockPattern.Substring(4) -split '\.\*\?'
          $stripped = [regex]::Replace($existing, $blockPattern, '').TrimEnd()
          $block = $marker[0] + "`n" + $report + "`n" + $marker[1]
          $updated = Join-Path $env:RUNNER_TEMP 'pr-body.md'
          [System.IO.File]::WriteAllText($updated, ($stripped + "`n`n" + $block))
          gh pr edit $number[0] --body-file $updated
          if ('${{ steps.repair.outputs.beyond-known-weak }}' -ne '0') {
            gh pr edit $number[0] --add-label 'deps:autofixed'
          }
```

## Literal Counts

| Literal | Required | Measured | Result |
|---|---|---|---|
| `<!-- dependabot-repair:begin -->` | exactly 1 | **1** | PASS |
| `<!-- dependabot-repair:end -->` | exactly 1 | **1** | PASS |
| `(?s)<!-- dependabot-repair:begin -->.*?<!-- dependabot-repair:end -->` | exactly 1 | **1** | PASS |

## How the Exactly-Once Clause Was Met

Recorded because a first form of this edit failed it, and the failure is instructive rather than
incidental.

The obvious composition writes the strip pattern **and** writes the two markers again when it
builds the fresh block:

```powershell
$stripped = [regex]::Replace($existing, '(?s)<!-- dependabot-repair:begin -->.*?<!-- dependabot-repair:end -->', '')
$block = "<!-- dependabot-repair:begin -->`n" + $report + "`n<!-- dependabot-repair:end -->"
```

That form measured **2** occurrences of each marker and 1 of the pattern, failing the exactly-once
clause. The two clauses are jointly unsatisfiable in that shape: [P3-T5] requires the pattern
literal to be present verbatim, and the pattern literal necessarily contains both markers, so any
second written occurrence takes the count to 2.

The delivered form derives the emitted markers from the strip pattern by dropping the `(?s)`
option prefix and splitting on the `.*?` between them. Each marker is therefore written **once**,
inside the pattern, and used twice.

That is not only a way to satisfy the count. It makes the anti-drift property structural: the
block this step **emits** and the block it **strips** are built from one literal, so they cannot
diverge by a character. [P3-T5] asserts the same property across the workflow-to-test boundary;
this derivation asserts it within the step.

## Gate Rule 20 — Verification Route and Residual

**Verified without a live run:**

- by [P3-T1]'s `R6- guards` assertion, which failed before this edit because the disclosure step
  carried zero `if:` lines, and which [P3-T8] re-runs green;
- by [P3-T5], which applies the workflow's own literal pattern to a synthetic body already
  carrying one delimited block and asserts the result carries exactly one begin marker and
  exactly one end marker, with the body's original leading text intact.

**Unverifiable until the #914 credential exists:** that a second run against a **real**
pull-request body yields exactly one block. Nothing here calls `gh pr view` or `gh pr edit`;
[P3-T5] exercises the strip-and-append expression against a string this repository constructs,
not against a body GitHub returned. The shape of a real body — its leading content, its line
endings, any markdown GitHub normalises — is not observed by any check in this cycle.

## Output Summary

The disclosure step is guarded on `written-count` or `skip-count`, and the body edit strips any
prior delimited block before appending a fresh one. Each of the three literals appears exactly
once, with the emitted markers derived from the strip pattern so the two cannot drift.
