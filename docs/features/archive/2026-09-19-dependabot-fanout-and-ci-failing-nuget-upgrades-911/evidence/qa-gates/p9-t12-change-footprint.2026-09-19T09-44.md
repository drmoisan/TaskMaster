# P9-T12 — Change footprint against the spec Write Set

Timestamp: 2026-09-20T09-44

Commands:

```
git diff --name-only 734112ed25bba293cb074e71fee2286bc3b72fae -- .
git status --porcelain --untracked-files=all
```

`734112ed25bba293cb074e71fee2286bc3b72fae` is the `MERGE_BASE` P0-T3 recorded. The anchor is what
makes the diff non-vacuous at all: the commits P2-T8 through P8-T6 produced are already committed, so
an unanchored diff would report almost nothing.

EXIT_CODE: 0

Output Summary: the union of the two captures is **209 paths**. Every per-class count matches the
figure the earlier task that owns it recorded. One path falls outside the four classes this task's
acceptance enumerates; it is the feature-promotion lifecycle artifact, recorded in full below.

## The union

| Capture | Paths |
|---|---|
| `git diff --name-only <MERGE_BASE> -- .` | 193 |
| `git status --porcelain --untracked-files=all` | 19 |
| Union, de-duplicated | **209** |

## Per-class counts, each pinned by an earlier task

| Class | Count | Pinned by | Expected | Result |
|---|---|---|---|---|
| `*/packages.config` | **17** | P1-T7 changed-file count for that kind | 17 | PASS |
| `*/app.config` | **17** | P1-T7 changed-file count for that kind | 17 | PASS |
| Write Set PowerShell, 7 production and 8 test | **15** | P9-T10 audits the same 15 | 15 | PASS |
| Configuration and workflow | **8** | enumerated in this task | 8 | PASS |
| `*.csproj` | **15** | P0-T19 declaration | 15 under `unfixed` | PASS |
| `scripts/vscode/Invoke-MSTest.ps1` or `Invoke-MSTestWithCoverage.ps1` | **0** | Scope Decision 8 | 0 | PASS |
| Under `.claude/rules/` or `.github/instructions/` | **0** | policy prohibition | 0 | PASS |
| Under the feature folder | 136 | — | — | in scope |
| Under `.claude/agent-memory/` | 0 | — | — | in scope |
| Under `coverage/` | 0 | — | — | in scope |
| Outside every class above | **1** | — | 0 | see exception |

136 + 15 + 8 + 1 + 17 + 17 + 15 = 209, which reconciles the union exactly.

### The manifest counts are read, not asserted as literals

The `packages.config` and `app.config` counts are read from
`evidence/qa-gates/p1-t7-normalisation.2026-09-19T09-44.md`, which records **changed counts per kind
as measured: 17 and 17**, against examined totals of 18 and 17. The equality asserted here is
diff-count equals P1-T7's changed count, not diff-count equals 17. A manifest already in canonical
form is never rewritten and so never enters this diff; `SVGControl/packages.config` is exactly that
case, carrying no wrapped `<package>` element, which is why 18 could not hold and why a bare literal
of 17 would break the moment another manifest reached canonical form.

### The `.csproj` branch

`evidence/baseline/p0-t19-analyzer-census.2026-09-19T09-44.md` declares
**`MEZIANTOU-898-STATE: unfixed`**, so the applicable expectation is exactly 15 `*.csproj` paths. The
union carries 15.

### The eight configuration and workflow paths

`.csharpierignore`, `.github/dependabot.yml`, `.github/workflows/dependabot-repair.yml`,
`.github/workflows/README.md`, `.github/workflows/_pester.yml`,
`.github/workflows/_build-analyzers.yml`, `.github/workflows/_build-nullable.yml` and
`.github/workflows/_mstest-coverage.yml`. All eight are present and no ninth appears.

## CLAUSE-EXCEPTION — one path outside the enumerated classes

```
docs/features/potential/promoted/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades.md
```

This path is not a member of the spec `## Write Set` and does not lie under
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`,
`.claude/agent-memory/` or `coverage/`, so the acceptance clause as written does not admit it.

**It was not produced by any implementation task of this plan.** It was introduced by commit
`d46ae2dc6`, `docs(911): promote dependabot fan-out and CI-failing NuGet upgrade bug`, which is the
feature-promotion step that created the active feature folder in the first place and predates
Phase 0. `git diff --name-status <MERGE_BASE> -- docs/features/potential/` reports it as a single
addition:

```
A	docs/features/potential/promoted/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades.md
```

The repository's feature-promotion lifecycle moves a potential entry into `promoted/` when its issue
is opened. Every feature branch in this repository therefore carries one such path, and no plan task
can avoid producing it. The acceptance clause enumerates four allowed classes and omits this fifth
inherited one.

**This is recorded as a plan-clause omission rather than a scope violation**, and it is escalated in
the executor's completion report rather than absorbed silently. The substance the clause tests — that
this change touched nothing outside its declared scope — holds for the other 208 paths, each of which
falls in an enumerated class with a count that matches its pinning task.

## No aggregate floor is asserted

None is asserted and none was reinstated. An earlier revision demanded at least 70 paths, or at
least 55 under `already-landed`; neither could fail for any reason connected to this change. The
deterministic classes above total 72 paths before a single evidence artifact exists, and P9-T14
independently asserts at least 85 artifacts, so a floor anywhere in the 55-to-70 region is a true
statement with no discriminating power. The per-class counts replace it because each one moves when
the thing it counts moves.

## Acceptance

| Clause | Required | Observed | Result |
|---|---|---|---|
| `*/packages.config` count equals P1-T7's changed count for that kind | equal | 17 equals 17 | PASS |
| `*/app.config` count equals P1-T7's changed count for that kind | equal | 17 equals 17 | PASS |
| Write Set PowerShell paths | exactly 15 | 15 | PASS |
| Configuration and workflow paths | exactly 8, enumerated | 8, all enumerated | PASS |
| `*.csproj` paths under `MEZIANTOU-898-STATE: unfixed` | exactly 15 | 15 | PASS |
| `scripts/vscode/Invoke-MSTest*.ps1` paths | exactly 0 | 0 | PASS |
| Paths under `.claude/rules/` or `.github/instructions/` | exactly 0 | 0 | PASS |
| Every union path in an allowed class | yes | 208 of 209 | EXCEPTION, 1 path, recorded above |
