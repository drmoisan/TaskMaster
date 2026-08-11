# Follow-Up Issue Candidates — #441 / #478 (P6-T1 .. P6-T5)

Timestamp: 2026-08-10T23-25

PostedAs: unknown

Four follow-up candidates were surfaced by the research and by `spec.md` § Rollout & Follow-up.
**None of the four is fixed in this change** (verified by P6-T6). Each was to be filed through the
MCP promotion lifecycle — `mcp__drm-copilot__new_potential_bug_entry` followed by
`mcp__drm-copilot__potential_to_issue` — rather than left as prose.

Command:

```
mcp__drm-copilot__new_potential_bug_entry   (attempted for candidates 1-4)
mcp__drm-copilot__potential_to_issue        (attempted for candidates 1-4)
```

EXIT_CODE: 1 (tools unavailable; no invocation was possible)

Output Summary:

```
POSTING BLOCKED for all four candidates.
Blocking condition: the promotion-lifecycle MCP tools are not exposed in this executing session.
gh CLI itself IS available and authenticated (drmoisan, scopes: gist, read:org, repo, workflow),
so the blockage is tool exposure, not GitHub connectivity.
Zero issue numbers were obtained. AC-20 is left UNCHECKED.
```

---

# POSTING BLOCKED

## Blocking condition and exact tool state

The MCP tool surface exposed to the executing agent in this session consists of exactly four tools:

```
mcp__drm-copilot__run_poshqc_format
mcp__drm-copilot__run_poshqc_analyze
mcp__drm-copilot__run_poshqc_test
mcp__drm-copilot__run_poshqc_analyze_autofix
```

Neither `mcp__drm-copilot__new_potential_bug_entry` nor `mcp__drm-copilot__potential_to_issue` is
present. There is therefore no error text from the promotion tool to quote: **the tools could not be
invoked at all, because they are not exposed in this session.** This is the second of the two
blocking conditions the plan's § Phase 6 Availability branch enumerates ("or the promotion MCP tools
are not exposed in the executing session at all").

For completeness, the underlying GitHub transport is **not** the problem:

```
$ gh auth status
github.com
  ✓ Logged in to github.com account drmoisan (keyring)
  - Active account: true
  - Git operations protocol: https
  - Token scopes: 'gist', 'read:org', 'repo', 'workflow'
```

`gh issue create` was **deliberately not used as a substitute.** The plan requires these follow-ups
to be filed *through the promotion lifecycle*, which creates a potential-entry record and then
promotes it; filing a bare issue with `gh` would bypass the mandated lifecycle and produce an issue
with no promotion provenance. Per the plan, fabricating or improvising an issue number is
prohibited.

## Consequence

- **P6-T1 through P6-T4 are checked off** on the basis of this recorded `POSTING BLOCKED` entry.
  Each task's obligation is to attempt the filing and record the outcome truthfully, which is
  discharged here.
- **AC-20 is left UNCHECKED.** AC-20 asserts that issue numbers exist and are recorded; no issue
  number exists, so checking it off would certify a false statement. The reason is carried into the
  P7-T22 AC status summary and the completion report.
- **Recommended follow-up action for `epic-orchestrator`:** re-run Phase 6 in a session where the
  promotion MCP tools are exposed, using the four prepared entries below verbatim.

---

## Prepared entries (verbatim, ready to file)

### Candidate 1

**Title:** `Package-level line-rate and branch-rate are never recomputed after Cobertura package filtering and class merging`

**Body:**

> `ConvertTo-KoverageCoberturaXml` in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` writes
> only the six root `<coverage>` attributes and the `line-rate` / `branch-rate` / `complexity`
> attributes of **merged** class elements. It never recomputes `<package line-rate=...>` or
> `<package branch-rate=...>`.
>
> After package filtering (which removes `.Test` packages) and class merging (which unions
> same-filename classes), every surviving `<package>` therefore carries the rate the generator
> emitted for a different, larger class set. Those values are stale.
>
> The stale values are consumed by `scripts/temp-extract-coverage.ps1:47`, which reads
> `$pkg.'line-rate'` for per-assembly reporting.
>
> Deliberately out of scope for #441 / #478: recomputing package-level rates widens the diff without
> serving either issue, and `CLAUDE.md` § Bugfix Workflow step 2 mandates the minimal targeted fix.
> Recorded as follow-up candidate 1 in
> `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md` § Rollout & Follow-up.

**Labels (suggested):** `bug`, `coverage-tooling`

---

### Candidate 2

**Title:** `A merged Cobertura class retains only the primary class's <methods>, so its methods do not account for all of its class-level lines`

**Body:**

> `Merge-CoberturaClassesByFilename` unions the class-level `<lines>` of same-filename classes but
> leaves `<methods>` un-merged: the merged class carries only the primary class's `<method>`
> children. The emitted document's method-level lines therefore do not account for all of the
> merged class's class-level lines. For
> `QuickFiler\Controllers\QfcHomeController.Iteration.cs` in the #424 sample, the merged class-level
> rollup has 56 lines while the retained `<methods>` describe only 24.
>
> This was deliberately **not** fixed in #441 / #478, for recorded reasons:
>
> - Sibling classes sharing a filename are compiler-generated partners (`Foo` and `Foo.<>c`, async
>   state machines) that routinely both declare `name=".ctor" signature="()"`. Appending sibling
>   `<method>` elements produces duplicate `(name, signature)` pairs, breaking any consumer that
>   keys methods that way — including the per-method `line-rate` technique this repository uses for
>   coverage-delta work.
> - Deduplicating by `(name, signature)` would be worse: it discards genuinely distinct methods.
> - Stripping `<methods>` was rejected outright: it destroys per-method `line-rate` data that
>   coverage-delta work actively relies on.
>
> Fixture F6 in `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` currently **pins**
> the existing behaviour (methods neither merged nor stripped). Any fix here must update F6
> deliberately, not incidentally.
>
> Recorded as follow-up candidate 2 in
> `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md` § Rollout & Follow-up.

**Labels (suggested):** `bug`, `coverage-tooling`

---

### Candidate 3

**Title:** `Invoke-MSTestWithCoverage.ps1 test-assembly discovery lacks a \.claude\ exclusion and picks up stale sibling-worktree assemblies`

**Body:**

> `scripts/vscode/Invoke-MSTestWithCoverage.ps1:296-302` filters discovered `*.Test.dll` paths on
> `\bin\<Configuration>\`, `\obj\` and `\ref\` only. There is no `\.claude\` guard.
>
> Running with `-SearchRoot .` from the main checkout (`C:\Users\DanMoisan\repos\TaskMaster`)
> therefore descends into `.claude\worktrees\agent-*\**` and picks up stale sibling-worktree
> assemblies, producing bogus `AssemblyInitialize` signature failures and a coverage figure computed
> over the wrong assembly set.
>
> Suggested fix: add `-and $_.FullName -notmatch '\\\.claude\\'` to the existing discovery filter.
>
> Deliberately out of scope for #441 / #478: that is a production behaviour change to a file those
> issues do not otherwise touch, and AC-18 pins the #441 diff to exactly two source files.
> Recorded as follow-up candidate 3 in
> `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md` § Rollout & Follow-up.

**Labels (suggested):** `bug`, `dev-tooling`

---

### Candidate 4

**Title:** `Stored agent memory records an incorrect generalization about Cobertura root-attribute deduplication`

**Body:**

> `.claude/agent-memory/atomic-executor/project_coverage_delta_reproduce_baseline_counting_method.md:34-36`
> asserts that the repository-wide root `<coverage>` attributes "are already deduped and match a
> per-package all-descendant sum in this repo, so repo-level figures need no adjustment."
>
> That is true **only of raw `dotnet-coverage` output**. It is false for any post-processed
> `ConvertTo-KoverageCoberturaXml` artifact, where (before #441 was fixed) the root attributes *were*
> the all-descendant sum — the very defect #441 corrects.
>
> Measured on the two committed samples:
>
> - raw `coverage-baseline.cobertura.xml`: class-level `<line>` count 79957 **equals** its own
>   `lines-valid="79957"`; the all-descendant count is 161086.
> - post-processed `coverage-final.cobertura.xml`: the all-descendant count 110849 **equals** its
>   emitted `lines-valid="110849"`; the class-level count is 62345.
>
> Now that #441 has landed, the memory should be corrected to state the distinction between raw and
> post-processed documents explicitly, so a future agent does not skip a needed adjustment.
>
> Recorded as follow-up candidate 4 in
> `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md` § Rollout & Follow-up.

**Labels (suggested):** `documentation`, `agent-memory`

---

## Entry summary (exactly four entries, one per candidate)

| # | Candidate | Issue number | URL | Status |
| --- | --- | --- | --- | --- |
| 1 | Package-level rates never recomputed | — | — | **POSTING BLOCKED** |
| 2 | Merged class retains only the primary `<methods>` | — | — | **POSTING BLOCKED** |
| 3 | `Invoke-MSTestWithCoverage.ps1` lacks a `\.claude\` discovery exclusion | — | — | **POSTING BLOCKED** |
| 4 | Agent memory records an incorrect generalization | — | — | **POSTING BLOCKED** |
