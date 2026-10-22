# Feature Audit — 2026-09-02-ci-build-infra-debt-730 (Re-audit, remediation cycle 1)

- Timestamp: 2026-09-03T00-33
- Issue: #730
- Branch: `bug/ci-build-infra-debt-730` @ `ff2106fe`
- Base: `origin/main` @ `8be5a6aa` (merge base identical)
- Work Mode: **`full-bug`** (persisted marker at `issue.md` line 12)
- AC source (resolved by work mode): **`spec.md` only**

## Verdict

**PASS — 8 of 8 acceptance criteria verified. Zero blocking findings.**

`spec.md` was **not modified** by either remediation commit (blob `843eaeda` is identical at
`e80f74c9` and at `ff2106fe`), and neither were any of the four shipped configuration files.
Every acceptance criterion was nonetheless re-verified against primary evidence in this session
rather than carried forward from the prior audit.

## AC Source Resolution

The work-mode marker reads `- Work Mode: full-bug`. Per the acceptance-criteria-tracking protocol,
`full-bug` resolves to **`spec.md` only**; `user-story.md` is not an AC source for this mode and
does not exist in this feature folder. The criteria are the 8 checkbox items under the
`## Acceptance Criteria` heading at `spec.md` lines 167–175.

---

## Acceptance Criteria Evaluation

| # | Criterion (abbreviated) | Verdict | Primary evidence |
|---|---|---|---|
| AC1 | Rationale comment above `restore-keys:` in 3 workflow files, no functional change | **PASS** | Comment present at lines 40–55 in each; parse trees identical to base under two parsers |
| AC2 | Root `Directory.Build.props` sets `RxUseUnsupportedPackagesConfig` true with rationale comment | **PASS** | File added, 18 lines, XML valid, one property, comment present |
| AC3 | Rebuild shows zero PackagesConfigCheck warnings across the 5 projects, no new warnings/errors | **PASS** | Pre 5 warnings / 0 errors; post 0 warnings / 0 errors; no new diagnostic codes |
| AC4 | No `.csproj`, `packages.config`, `Directory.Build.targets`, or application source modified | **PASS** | Filtered `git diff --stat` over full branch range returns empty |
| AC5 | No coverage threshold, Pester job, or coverage gate added or changed | **PASS** | Workflow parse trees identical to base; only comment lines inserted |
| AC6 | Rx-dependent MSTest suites pass on re-run | **PASS** | 6095/6095 passed, 0 failed, exit code 0, corroborated by TRX counters |
| AC7 | Full toolchain pass: CSharpier, both msbuild commands, vstest | **PASS** | CSharpier exit 0 over 1576 files; both rebuilds 0 errors; vstest exit 0 |
| AC8 | Before/after transcripts captured under `evidence/` per conventions | **PASS** | 20 evidence artifacts under canonical `evidence/<kind>/` subdirectories |

---

### AC1 — Workflow comment insertion, no functional change

The comment block is present immediately above `restore-keys:` in all three files, at lines 40–55,
and is byte-identical across them.

The "no functional change" half of this criterion is the substantive one and was tested directly,
because the block is indented deeper (12) than the `key:` scalar above it (10). A more-indented
line following a plain scalar can be absorbed into that scalar, which would corrupt the NuGet cache
key with no syntax error and no visible signal in a line diff.

Order-sensitive full parse-tree comparison, base `8be5a6aa` versus head `ff2106fe`, using two
independent implementations:

- **PyYAML 6.0.3** — all three trees identical; resolved `key` is
  `nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}` in both base and head; no `#`
  present in any resolved `key`; `restore-keys` present once in base and once in head.
- **YamlDotNet via powershell-yaml 0.4.12** — all three trees identical under ordinal comparison;
  canonical serialization lengths identical base to head (1365, 1854, 2935).

No YAML key, value, step, job, or ordering differs from base. **PASS.**

### AC2 — `Directory.Build.props`

Added at the repository root, 18 lines, trailing newline present.

- XML loads cleanly; root element `Project`.
- `PropertyGroup` contains exactly one child; `RxUseUnsupportedPackagesConfig` resolves to `true`.
- A 12-line XML comment states the rationale: the repository deliberately keeps its legacy non-SDK
  VSTO / .NET Framework projects on `packages.config`, so the vendor warning is accepted as a known
  trade-off rather than fixed by migration, and the property is the vendor's own documented
  suppression switch.

Import-chain preconditions re-verified: exactly one `Directory.Build.props` is tracked in the
repository, `Directory.Build.targets` is present (which is what demonstrates the auto-import
mechanism is already live here), and **no `NuGet.Config` exists anywhere** that could disturb the
chain. **PASS.**

### AC3 — Warning elimination with no new diagnostics

Measured directly from the four committed build transcripts, not from their summary documents.

| Transcript pair | PackagesConfigCheck mentions | MSBuild summary |
|---|---|---|
| analyzers pre | 10 raw mentions = 5 distinct warnings (MSBuild prints each inline and again in summary) | `5 Warning(s)`, `0 Error(s)` |
| analyzers post | **0** | `0 Warning(s)`, `0 Error(s)` |
| nullable pre | 10 raw mentions = 5 distinct warnings | `5 Warning(s)`, `0 Error(s)` |
| nullable post | **0** | `0 Warning(s)`, `0 Error(s)` |

The five affected projects were confirmed by extracting the `[...csproj]` tag from each inline
warning, and they match the five named in `spec.md` exactly: **QuickFiler, TaskMaster, ToDoModel,
UtilitiesCS, UtilitiesCS.Test**.

The "no new warnings or errors" half was tested separately by comparing the set of distinct
diagnostic codes (`CS`, `MSB`, `CA`, `IDE`, `NU`, `SA` families) between each pre and post log:
**no code appears in a post log that is absent from its pre log**, in either pair. Error count is
0 before and after. **PASS.**

Incidental confirmation of sanitization quality: these transcripts render paths as `<repo-root>`
throughout, consistent with the 0 account-name matches recorded in the policy audit.

### AC4 — No project or source file modified

`git diff --stat 8be5a6aa..ff2106fe` restricted to `*.csproj`, `*packages.config`,
`Directory.Build.targets`, and `*.cs` returns **empty output** over the entire branch range.

Independently corroborated by the extension census of all 34 changed files: `.md` 26, `.log` 4,
`.yml` 3, `.props` 1. No other extension appears. **PASS.**

### AC5 — No coverage gate changed

`_mstest-coverage.yml` is one of the three files touched, so this criterion needed positive proof
rather than an absence argument. The order-sensitive parse-tree comparison for that file shows the
head tree is **identical to base**, which means no threshold value, job, step, or condition was
added, removed, or altered. The only textual delta is comment lines, which do not appear in the
parse tree at all. No Pester job exists in these workflows. **PASS.**

### AC6 — MSTest regression re-run

Scoped to the two Rx-dependent assemblies (`UtilitiesCS.Test`, `QuickFiler.Test`), resolved using
the same `vswhere` path that `_mstest-coverage.yml` uses, and run with `/InIsolation`.

- Console: `Test Run Successful.`, `Total tests: 6095`, `Passed: 6095`
- TRX `ResultSummary/Counters`: `total=6095 executed=6095 passed=6095 failed=0 error=0 timeout=0 aborted=0 notExecuted=0`
- Process exit code 0

Two independent sources agree. The evidence artifact also documents why no `Failed:` line appears
(vstest omits it on a fully passing run) and confirms the six lines containing the substring
"Failed" are test *names* prefixed `Passed`, which is the correct reading. **PASS.**

### AC7 — Full toolchain pass

| Stage | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier check .` | Exit 0; `Checked 1576 files in 6100ms`; no drift |
| Analyze | `msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 Warning(s), 0 Error(s) |
| Type-check | `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` | 0 Warning(s), 0 Error(s) |
| Test | `vstest.console.exe ... /EnableCodeCoverage /InIsolation` | 6095/6095, exit 0 |

Both msbuild invocations use `/t:Rebuild` (not `/t:Build`), which matters because a warm `/t:Build`
skips `CoreCompile` and runs no analyzers, making the gate vacuous. The transcripts show 36
`csc.exe` invocations each, confirming real compilation occurred. Neither command carries
`/p:Nullable=enable`, correctly matching the CI command. **PASS.**

### AC8 — Evidence captured under the canonical location

20 evidence artifacts are committed under
`docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/`, using the canonical
subdirectories `baseline/` (7), `qa-gates/` (12), and `other/` (1). Both required before/after
transcript pairs are present. No evidence file was written to `artifacts/baselines/`,
`artifacts/qa/`, `artifacts/coverage/`, or `artifacts/evidence/`. **PASS.**

---

## Remediation Impact on Acceptance Criteria

The caller asked for confirmation that the remediation did not disturb the AC basis. Verified by
blob-SHA comparison between the pre-remediation commit `e80f74c9` and HEAD `ff2106fe`:

| Artifact | Role | Blob at `e80f74c9` | Blob at `ff2106fe` | Changed? |
|---|---|---|---|---|
| `spec.md` | AC source | `843eaeda` | `843eaeda` | No |
| `issue.md` | work-mode marker | `51efa7ed` | `51efa7ed` | No |
| `msbuild-analyzers-pre.log` | AC3 evidence | `fd757e3e` | `fd757e3e` | No |
| `msbuild-nullable-pre.log` | AC3 evidence | `a3222fcd` | `a3222fcd` | No |
| `msbuild-analyzers-post.log` | AC3 evidence | `5ec06350` | `5ec06350` | No |
| `msbuild-nullable-post.log` | AC3 evidence | `178241d4` | `178241d4` | No |
| 3 workflow files, `Directory.Build.props` | AC1, AC2, AC5 | not in either commit's file list | — | No |

The remediation commits touched 10 paths, all documentation or evidence under the feature folder.
No AC source and no AC evidence artifact was altered. The 8/8 PASS result therefore rests on the
same evidence base as the prior audit, re-measured in this session.

---

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-02-ci-build-infra-debt-730/spec.md (Work Mode: full-bug)
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none
```

All 8 items were already marked `[x]` in `spec.md` by the executor. Each was independently verified
as PASS in this audit, so no checkbox required a change and none was modified by this reviewer.

## Open Non-Blocking Observations

Carried into PR authoring; none gates the merge.

| ID | Severity | Item | Recommended handling |
|---|---|---|---|
| CR-8 / PA-2 | Low | Superseded blob with the account name reachable in 4 earlier branch commits | **Squash-merge the PR** |
| CR-2 | Low | Comment block indented 12 versus surrounding keys at 10 (cosmetic; proven inert) | Optional dedent, or defer |
| CR-3 | Low | 4 comment references to `docs/features/active/...` will break when the folder is archived | Optional: drop the path, keep "issue #730" |
| PA-3 | Low | Tracked `artifacts/pr_context.*` are stale (belong to issue #564); pre-existing on `main` | Regenerate during PR authoring for this branch |
| CR-5, CR-6, CR-7 | Info | Tool paths in logs; root props scope; comment duplication | No action recommended |

## Conclusion

All 8 acceptance criteria pass. The remediation closed the single blocking finding without altering
the AC source, the AC evidence, or any shipped configuration file. **Zero blocking issues remain**;
this remediation cycle can be closed and the branch can proceed to PR authoring, to be
squash-merged.
