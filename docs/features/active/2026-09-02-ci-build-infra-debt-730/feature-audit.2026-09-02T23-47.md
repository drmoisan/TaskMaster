# Feature Audit — 2026-09-02-ci-build-infra-debt-730

- Timestamp: 2026-09-02T23-47
- Issue: #730
- Work Mode: `full-bug` (marker at `issue.md:12`)
- AC source (resolved by work mode): `spec.md` § `## Acceptance Criteria`, lines 168-175 — **8 items**
- Branch: `bug/ci-build-infra-debt-730` @ `e80f74c9`
- Baseline: `origin/main` @ `8be5a6aa`

Per the work-mode routing rule, `full-bug` resolves the AC source to `spec.md` **only**.
`user-story.md` is not an AC source in this mode and does not exist in this feature folder, which is
correct for a bug. `issue.md`'s "Proposed Fix / Validation Ideas" checkboxes are scoping notes, not
acceptance criteria, and were not treated as such.

## AC Evaluation

| # | Acceptance criterion (abbreviated) | Verdict |
|---|---|---|
| AC1 | Rationale comment above `restore-keys:` in 3 workflows, no functional change | PASS |
| AC2 | New root `Directory.Build.props` sets `RxUseUnsupportedPackagesConfig` true with rationale comment | PASS |
| AC3 | Both rebuilds show zero Rx warnings across 5 projects, no new warnings or errors vs baseline | PASS |
| AC4 | No `.csproj`, `packages.config`, `Directory.Build.targets`, or application source modified | PASS |
| AC5 | No coverage threshold, Pester job, or coverage gate added or changed | PASS |
| AC6 | Rx-dependent MSTest suites pass on re-run | PASS |
| AC7 | Full toolchain pass: csharpier, both msbuild rebuilds, vstest regression | PASS |
| AC8 | Before/after transcripts captured under the feature folder's `evidence/` | PASS |

**8 of 8 PASS. 0 PARTIAL. 0 FAIL. 0 UNVERIFIED.**

---

## AC1 — Comment present above `restore-keys:` in three workflows, no functional change — PASS

Both halves of this criterion were verified separately.

**Presence and identity.** The 16-line block is present in all three files, positioned immediately
above `restore-keys:` (block occupies lines 40-55; `restore-keys:` is at line 56 in
`_build-analyzers.yml`). `git diff origin/main...HEAD` shows `+16/-0` for each of the three files and
no other change. The inserted text is byte-identical across all three, as the spec requires
("identical in content, per research §1.4").

**No functional change.** Verified empirically rather than by reading the diff, because the block is
indented deeper than the surrounding keys and a more-indented line following a plain scalar can be
folded into that scalar. Parsed the base and head revisions of each file with two independent YAML
implementations (PyYAML and YamlDotNet via `powershell-yaml`) and deep-compared the resulting
documents with key order preserved. **All three parse trees are identical to base.** No key, value,
step, or job was added, removed, reordered, or altered. `key` resolves to
`nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}`, `restore-keys` to
`nuget-${{ runner.os }}-\n`, and `path` to `packages`, in all three files, in both parsers.

The workflows also remain syntactically valid YAML, which satisfies the spec's Test Strategy check
for Finding 1 by direct parse rather than by waiting to observe a CI run.

The comment's factual assertions were checked against the file they annotate: the `Restore solution`
step is the immediately following step, carries no `if:` condition and no `cache-hit` gate, and
precedes the build step. The rationale the comment states is accurate for this repository.

*Related non-blocking observation:* CR-2 (indentation) and CR-3 (path reference will break on
archive) in the code review. Neither affects this verdict.

## AC2 — `Directory.Build.props` at repository root — PASS

Added at the repository root (`Directory.Build.props`, status `A`, 18 lines). Verified:

- Well-formed XML, confirmed by `[xml]` document load. Root element `<Project>`, no `Sdk` attribute,
  which is correct for a non-SDK-style repository.
- Declares exactly one property, `RxUseUnsupportedPackagesConfig`, with value `true`, inside a single
  `<PropertyGroup>` — precisely as the criterion specifies.
- Carries an inline XML comment (lines 2-13) stating the rationale: the vendor emits the warning for
  packages.config projects, this repository deliberately keeps its legacy non-SDK VSTO / .NET 4.8.1
  projects on packages.config rather than migrating, and the property is the package's own documented
  suppression switch. This states the accepted trade-off, which is what the criterion asks for.
- Content matches what `spec.md` §Proposed Fix describes.
- File terminates with a newline and is 18 lines, under the 500-line limit.
- No collision: `git grep -i RxUseUnsupportedPackagesConfig HEAD -- "*.csproj" "*.props" "*.targets"
  "*.config"` returns matches only inside this new file, confirming the spec's stated invariant that
  the property is not already set anywhere.

## AC3 — Zero Rx warnings, no new warnings or errors versus baseline — PASS

This criterion was the subject of a measurement-method defect found and corrected during execution
(MSBuild's file logger prints each warning twice at `verbosity=normal` — once inline, once in the
end-of-build summary). The correction was re-derived independently rather than accepted.

Measured directly from the four committed logs:

| Log | `Build succeeded.` | Rx token, whole-log | Rx token, inline only | MSBuild `N Warning(s)` | MSBuild `N Error(s)` |
|---|---|---|---|---|---|
| `msbuild-analyzers-pre.log` | 1 | 10 | **5** | **5** | 0 |
| `msbuild-nullable-pre.log` | 1 | 10 | **5** | **5** | 0 |
| `msbuild-analyzers-post.log` | 1 | 0 | **0** | **0** | 0 |
| `msbuild-nullable-post.log` | 1 | 0 | **0** | **0** | 0 |

The double-counting is real and the correction is right: the naive whole-log count reads 10, the
inline count reads 5, and MSBuild's own once-emitted summary line independently reads 5. Three
methods agree that the true baseline is 5. The chosen method (count strictly before the single
`Build succeeded.` line; read totals from MSBuild's own summary line) produces the correct figure.

`Build FAILED.` appears zero times in all four logs; `Build succeeded.` appears exactly once in each,
so the de-duplicating slice is unambiguous.

**Five affected projects confirmed by name.** Extracting the `[...csproj]` tag from each inline
warning in both baseline logs yields exactly: `QuickFiler.csproj`, `TaskMaster.csproj`,
`ToDoModel.csproj`, `UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj` — matching the five projects the
criterion names. Both post-change logs yield an empty set.

**No new warnings or errors.** Total warnings fell 5 -> 0; total errors unchanged 0 -> 0. Since the
post-change total is zero, no new warning of any kind can have been introduced — this is a stronger
result than a delta comparison, because it forecloses the case where five warnings disappear and
five different ones appear.

**The result is not vacuous.** A warm `/t:Build` would report success while skipping compilation
entirely. Both commands correctly use `/t:Rebuild`, and I confirmed real compilation occurred by
counting compiler invocations: 36 `csc.exe` invocations in each of the four logs, with 52/69/67/56
`CoreCompile:` target entries. The solution genuinely recompiled in every run, so the absence of the
warning reflects the property taking effect, not a skipped build.

## AC4 — No project, package, or application source file modified — PASS

`git diff --name-status origin/main...HEAD` over all 27 entries contains zero files with extension
`.csproj`, zero `packages.config`, zero `Directory.Build.targets`, and zero `.cs`. The only non-document
changes are the three workflow files (`M`) and `Directory.Build.props` (`A`).

`Directory.Build.targets` exists at the repository root and is untouched; it was read to confirm no
interaction with the new `.props` file (it sets `SignManifests`/`SignAssembly` and defines one target,
`SetTaskMasterManifestCert` — disjoint from the new property).

## AC5 — No coverage threshold, Pester job, or coverage gate changed — PASS

The only workflow file that carries the MSTest coverage job, `_mstest-coverage.yml`, has a parse tree
identical to its `origin/main` revision under order-sensitive deep comparison. No job, step, key, or
value changed; the diff is 16 comment lines. No coverage threshold value, Pester job, or coverage gate
was added, removed, or modified anywhere in the branch. No coverage configuration file appears in the
diff at all. This scope belongs to #561/#562/#563 and remains untouched here.

## AC6 — Rx-dependent MSTest suites pass on re-run — PASS

`vstest.console.exe` run over `UtilitiesCS.Test.dll` and `QuickFiler.Test.dll` after the
`Directory.Build.props` addition, exit code 0.

Rather than accept the prose summary, the run's own TRX result file was parsed directly.
`ResultSummary` `outcome="Completed"` with `Counters`:

```
total=6095 executed=6095 passed=6095 failed=0 error=0 timeout=0 aborted=0 notExecuted=0
```

This independently confirms the artifact's reported 6095/6095/0. The TRX run window
(23:14:36 to 23:15:24, elapsed 47.35s) matches the artifact's stated `47.3269 Seconds` and precedes
the commit at 23:34:42, so the evidence is contemporaneous with the delivery.

The evidence artifact correctly declined to quote the TRX and coverage attachment paths on the
grounds that they embed the operator account name, and confirmed the `TestResults/` directory is
excluded by `.gitignore:39` (`[Tt]est[Rr]esult*/`). I verified that ignore rule independently, and
confirmed no TRX or coverage attachment entered the change set. This is correct handling.

## AC7 — Full toolchain pass — PASS

All four stages have committed evidence, in the required order, all exit code 0:

1. `dotnet tool run csharpier check .` — exit 0, "Checked 1576 files". Corroborated against
   `.csharpierignore`, which explicitly excludes `*.props`, so the new file is correctly outside the
   formatter's scope rather than merely reported as clean.
2. `msbuild ... /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — exit 0,
   0 warnings, 0 errors. Command matches CLAUDE.md character-for-character.
3. `msbuild ... /t:Rebuild ... /p:TreatWarningsAsErrors=true` — exit 0, 0 warnings, 0 errors. Command
   matches CLAUDE.md. Correctly omits `/p:Nullable=enable`, which CLAUDE.md explicitly warns must not
   be added.
4. `vstest.console.exe` regression re-run — exit 0, 6095/6095 passed.

No C# source changed, so this is regression re-verification rather than new-test authorship, exactly
as the criterion states.

The vstest invocation adds `/InIsolation` and `/TestCaseFilter:"TestCategory!=LiveOutlook"` and is
scoped to the two Rx-dependent assemblies. This is a disclosed deviation from the bare CLAUDE.md
command, consistent with the spec's Test Strategy and with `_mstest-coverage.yml`'s own resolution
logic. It is documented in the evidence artifact rather than concealed, and does not weaken the
criterion, which asks specifically about the Rx-dependent assemblies.

## AC8 — Evidence captured under the feature folder — PASS

19 evidence artifacts are committed under
`docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/`, split across the canonical
`baseline/` (7) and `qa-gates/` (12) subdirectories, per the evidence-and-timestamp-conventions skill.
Before and after rebuild transcripts are present for both msbuild commands:

- `evidence/baseline/msbuild-analyzers-pre.log` + `.2026-09-02T23-03.md`
- `evidence/baseline/msbuild-nullable-pre.log` + `.2026-09-02T23-04.md`
- `evidence/qa-gates/msbuild-analyzers-post.log` + `.2026-09-02T23-11.md`
- `evidence/qa-gates/msbuild-nullable-post.log` + `.2026-09-02T23-12.md`

Workflow-diff verification output is captured in `evidence/qa-gates/diff-build-analyzers`,
`diff-build-nullable`, and `diff-mstest-coverage`. All artifact filenames use the required
`yyyy-MM-ddTHH-mm` timestamp form. No evidence was written to a non-canonical location.

The `.log` files required `git add -f` because `.gitignore:84` carries a blanket `*.log` rule; this is
recorded in the plan and the files are present in the committed tree, so the force-add was applied
correctly.

*Note:* CR-1 (blocking) concerns `research/research.2026-09-02T09-15.md`, which sits under `research/`
rather than `evidence/`. It is a research document, not an evidence transcript, so it is outside the
set this criterion governs and does not change the AC8 verdict.

---

## Relationship of the Blocking Finding to Acceptance

CR-1 / PA-1 (absolute host path with operator account name in the research document) is a
repository-wide artifact-hygiene rule violation. It maps to **no acceptance criterion** in `spec.md` —
the spec's Acceptance Criteria section contains no hygiene criterion — so it does not cause any AC to
fail. It must still be cleared before merge because the no-absolute-host-path rule applies to every
committed artifact independently of a feature's own AC list.

All eight acceptance criteria are genuinely met by the delivered change.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-02-ci-build-infra-debt-730/spec.md
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none
```

All eight items were already marked `[x]` in `spec.md` by the executor. This review verified each one
independently against the branch diff and the committed evidence, and all eight verifications
succeeded, so every existing check-off is warranted. No checkbox required a change, and none was
modified by this review. No phantom criteria were added.
