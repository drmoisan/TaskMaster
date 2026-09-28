# R3 — The Write Set Moves for a Normalisation-Only Run

- Timestamp: 2026-09-20T08-57-40
- Task: [P3-T3]
- Finding: R3. **This is the load-bearing half of R3.**
- Command: CMD-PESTER-FILTERED,
  `<FILE>` = `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1`,
  `<FILTER>` = `*R3- reports a non-zero write count*`
- EXIT_CODE: 0

## This Task Is Not a Fail-Before and Does Not Claim to Be

Stated plainly so it is not mis-recorded. This test **does not fail before the fix**. It tests the
**composition root**, which already returns `WrittenPath` correctly; the defect R3 reports is the
**workflow's choice of gating quantity**, not a wrong value in the result object.

The red-before evidence for R3 is [P3-T1]'s `R3- gates` assertion, which failed because the
workflow published no `written-count` output at all.

What this task establishes is the other half, and it is the half that decides whether
`written-count` is the **right** quantity to gate on: it demonstrates a run in which the old gate
reads 0 and the new gate reads 1.

## Test Added

`It 'R3- reports a non-zero write count for a run whose only change is a normalisation'`

Drives the composition root through the existing in-memory `Get-RepairFixture` harness with:

- a `packages.config` in the **wrapped multi-line form**, the shape CSharpier produced before
  `.csharpierignore` was widened;
- a project file that **already agrees** with it — the Reference assembly version, the hint-path
  folder segment and the declared package version are all consistent at `1.0.0`;
- no `-CandidateUpgrade`, so no upgrade is applied;
- an assembly-identity map supplying `Contoso.Widgets|1.0.0` at `net472` version `1.0.0.0`, so
  `Resolve-ReferenceAssemblyVersion` confirms the declared version and the Reference line is left
  byte-identical.

The only change the run makes is the normalisation pass reflowing the manifest to the inline form
the NuGet CLI writes.

## Counts Line, Verbatim

```
PESTER Passed=1 Failed=0 Skipped=0 Executed=1 Total=31 NotRun=30
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `Executed` = `Passed + Failed + Skipped` | 1 | **1** | PASS |
| `Passed` | 1 | **1** | PASS |
| `EXIT_CODE` | 0 | **0** | PASS |

## Both Figures, Quoted

Reproduced by an independent direct invocation of the entry point over the same fixture, outside
Pester, so the figures are on the record rather than only implied by a green assertion:

```
RepairCount=0
WrittenPathCount=1
WrittenPath=X:\fixture\App\packages.config
IsSuccess=True
ManifestAfter=<?xml version="1.0" encoding="utf-8"?> | <packages> |   <package id="Contoso.Widgets" version="1.0.0" targetFramework="net481" /> | </packages> |
```

`ManifestAfter` shows the reflow: the four-line wrapped `<package>` element is now one inline
element. The fixture path `X:\fixture\App\packages.config` is a synthetic in-memory key and names
nothing on disk.

## The Silent-Discard Case, Exhibited

| Quantity | Value on this run | What the gate reading it does |
|---|---|---|
| `RepairCount` — the **old** gate | **0** | `if: ... repair-count != '0'` is false. The commit step is skipped, the repair is discarded, the job is green, and the pull request reports "No repairs were applied." |
| `@($result.WrittenPath).Count` — the **new** gate | **1** | `if: ... written-count != '0'` is true. The commit step runs and the repair is pushed. |

That is the review's finding reproduced as a measurement rather than restated as an argument.

**How this task fails.** If `WrittenPath` did **not** move for a normalisation-only run, the count
would be 0, the assertion would fail, and `written-count` would be the wrong quantity to gate on.
The third assertion — that the single written path is the manifest — is what stops the count of 1
from being satisfied by some unrelated write.

## Gate Rule 20 — Verification Route and Residual

**Verified without a live run:** that the composition root's `WrittenPath` is non-empty for a run
whose only change is a normalisation, driven entirely through injected delegates with no
temporary file.

**Unverifiable until the #914 credential exists:** that GitHub evaluates
`steps.repair.outputs.written-count != '0'` as this reasoning expects, and that the commit step
consequently runs and pushes.

## Output Summary

One test added, one executed, one passed, exit 0. `RepairCount=0` and `WrittenPathCount=1` on the
same run, with the written path being the manifest — the state in which the old gate discards the
repair and the new gate pushes it.
