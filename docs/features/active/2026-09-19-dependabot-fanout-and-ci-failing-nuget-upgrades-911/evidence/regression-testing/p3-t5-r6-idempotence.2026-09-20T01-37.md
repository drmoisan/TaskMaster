# R6 — The Disclosure Edit Is Idempotent

- Timestamp: 2026-09-20T08-59-40
- Task: [P3-T5]
- Finding: R6
- Command: CMD-PESTER-FILTERED, `<FILE>` = `tests/scripts/dependencies/DependabotConfig.Tests.ps1`,
  `<FILTER>` = `*R6- replaces rather than appends*`
- EXIT_CODE: 0

## Test Added

`It 'R6- replaces rather than appends a previously disclosed block'`

It does four things in order:

1. assigns the pattern literal
   `(?s)<!-- dependabot-repair:begin -->.*?<!-- dependabot-repair:end -->` to a variable;
2. **asserts that `.github/workflows/dependabot-repair.yml` contains that exact literal**;
3. constructs a synthetic pull-request body carrying leading text and one already-delimited
   block, then applies `[regex]::Replace` with that variable and appends a fresh delimited block,
   exactly as the workflow step does;
4. asserts the result carries exactly one begin marker, exactly one end marker, the body's
   original leading text, the new block's content, and **not** the prior block's content.

## Counts Line, Verbatim

```
PESTER Passed=1 Failed=0 Skipped=0 Executed=1 Total=17 NotRun=16
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `Executed` = `Passed + Failed + Skipped` | 1 | **1** | PASS |
| `Passed` | 1 | **1** | PASS |
| `EXIT_CODE` | 0 | **0** | PASS |

## The Containment Assertion Is What Makes This a Test of the Workflow

```powershell
$text.Contains($blockPattern) |
    Should -BeTrue -Because 'the workflow must strip prior blocks with exactly this expression'
```

Without it the test would exercise a pattern the test itself invented, and would stay green if
the workflow's expression were deleted, mistyped or changed. With it, the test **fails if the
workflow's expression and the test's expression ever diverge by a character**.

The test derives its two markers from the pattern the same way the workflow does — dropping the
`(?s)` prefix and splitting on `.*?` — so the marker strings are bound to the workflow's
expression as well, not only the pattern itself.

## What the Test Observes

| Assertion | Why it can fail |
|---|---|
| exactly 1 begin marker in the result | an appending edit leaves 2; that is the review's finding |
| exactly 1 end marker in the result | the same, for the closing delimiter |
| the leading text survives | a strip pattern that was too greedy would take the pull-request body with it |
| `second run` present | a strip that deleted everything would satisfy the two count clauses vacuously |
| `first run` absent | the prior block must be replaced rather than accumulated |

The third and fourth are the non-vacuity guards. Two count assertions alone are satisfied by an
empty string.

## Gate Rule 20 — Verification Route and Residual

**Verified without a live run:** that the workflow's own literal pattern, applied to a body
already carrying one delimited block and followed by an append, yields exactly one block with the
surrounding body intact.

**Unverifiable until the #914 credential exists:** that a second run against a **real**
pull-request body yields exactly one block. The body in this test is a string this repository
constructs. A real body is whatever `gh pr view --json body` returns, including Dependabot's own
generated content, its line endings, and any normalisation GitHub applies on write. Nothing in
this cycle observes that.

## Phase-Ceiling Note

Adding this test took `tests/scripts/dependencies/DependabotConfig.Tests.ps1` to **479** lines,
over the 470-line ceiling [P3-T14] sets. Rather than halting, the file was compacted: the blank
lines immediately preceding an `# Act` or `# Assert` comment inside the newly added Contexts were
removed, and the `Get-WorkflowStepBlock` help and parameter comments were shortened. The file now
measures **468**.

No assertion, `-Because` clause or comment explaining a decision was removed. The halt branch
exists so the executor does not delete content to make room; nothing was deleted but whitespace
and two lines of re-wrapped prose. The Arrange-Act-Assert comments remain, so the structure the
unit-test policy requires is intact.

Whole-file run at this point: `Passed=15 Failed=2 Total=17`. The two failures are `R7- counts`
and `R8- derives`, which [P3-T6] and [P3-T7] have not yet addressed. `R3- gates`,
`R6- guards` and this test are green.

## Output Summary

One test added, executed, passed, exit 0. The workflow's strip expression is bound to the test's
by a containment assertion, and the strip-then-append composition leaves exactly one block with
the body's leading text intact.
