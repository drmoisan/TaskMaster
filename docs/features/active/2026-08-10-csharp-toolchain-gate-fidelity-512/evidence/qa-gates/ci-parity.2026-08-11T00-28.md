# AC5 — CI-parity comparison of the corrected documented commands ([P5-T15])

Timestamp: 2026-08-11T00-28
Command: (none — analysis artifact)
EXIT_CODE: (none — analysis artifact)

`.github/workflows/ci.yml` is **not modified** by this feature (SD4). It is the reference
implementation the documented commands converge onto.

---

## Step 1 — format

**CI (`ci.yml`, job step `Verify formatting`, preceded by `Setup CSharpier`):**

```yaml
- name: Setup CSharpier
  shell: pwsh
  run: dotnet tool restore

- name: Verify formatting
  shell: pwsh
  run: dotnet csharpier check .
```

**Documented (after this feature):**

- Apply: `dotnet tool run csharpier format .`
- Verify, read-only, CI parity: `dotnet tool run csharpier check .`
- Prerequisite: `Run dotnet tool restore once per clone or worktree before the first invocation.`

**Consistency.** `dotnet csharpier check .` and `dotnet tool run csharpier check .` resolve the
**same manifest-pinned CSharpier 1.2.6** — CI's form works because `dotnet tool restore` runs first
(`ci.yml` `Setup CSharpier`), and the documented form states that restore prerequisite explicitly.
Both were measured at `EXIT_CODE: 0` with `Checked 1517 files`
(`baseline-csharpier-replacement-forms.2026-08-10T14-45.md` forms A and B; and [P5-T2] for the
adopted form).

**Deliberate difference:** the documented form spells `tool run` explicitly, where CI uses the
implicit `dotnet csharpier`. **In-line rationale location:** `CLAUDE.md` § C#1 item 1, final bullet
("Always invoke through `dotnet tool run` so the manifest-pinned version is used. Do not invoke a
globally installed `csharpier`: a different global version produces diffs that disagree with
`.github/workflows/ci.yml`, which runs the pinned version after `dotnet tool restore`."), and
`.claude/rules/csharp.md` § Toolchain item 1. The reason is measured: a global CSharpier 1.3.0 is
present on this machine, and version skew between it and the pinned 1.2.6 can produce diffs that
disagree with CI.

---

## Step 2 — analyzer

**CI (`ci.yml`, job step `Build with analyzers and code style enforcement`):**

```powershell
& msbuild $env:SOLUTION_PATH /t:Build /m /p:Configuration=Debug `
    "/p:Platform=Any CPU" `
    /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

**Documented (after this feature), ANALYZE:**

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

**Token-by-token comparison:**

| Token | CI | Documented | Same |
|---|---|---|---|
| solution | `$env:SOLUTION_PATH` | `TaskMaster.sln` | equivalent (solution token) |
| target | `/t:Build` | **`/t:Rebuild`** | **NO — the one deliberate difference** |
| parallelism | `/m` | `/m` | yes |
| configuration | `/p:Configuration=Debug` | `/p:Configuration=Debug` | yes |
| platform | `"/p:Platform=Any CPU"` | `"/p:Platform=Any CPU"` | yes (identical spelling) |
| analyzers | `/p:EnableNETAnalyzers=true` | `/p:EnableNETAnalyzers=true` | yes |
| code style | `/p:EnforceCodeStyleInBuild=true` | `/p:EnforceCodeStyleInBuild=true` | yes |

**The one deliberate difference, stated with its rationale.** The documented ANALYZE uses
`/t:Rebuild` where CI uses `/t:Build`. **This is deliberate**: a CI runner checkout is always cold,
so `/t:Build` there is a genuine compile; a local working tree is warm, so `/t:Build` there is not.
Measured: DOC-ANALYZE warm returned exit 0 in **2.8 s** with `CoreCompile` skipped on **18 of 18**
projects ([P0-T10]), against ANALYZE at exit 0 with **0** skips in 20.3 s ([P5-T3]).

**In-line rationale locations (one per edited site):**

| Site | Rationale text |
|---|---|
| `CLAUDE.md` § C#1 item 2, bullet after the command | "Use `/t:Rebuild`, not `/t:Build`. ... `.github/workflows/ci.yml` uses `/t:Build /m` for its analyzer step because a runner checkout is always cold; a local working tree is not." |
| `.claude/rules/csharp.md` § Toolchain item 2, continuation bullet | R4: "Use `/t:Rebuild` so the step always performs a genuine recompile; a warm `/t:Build` skips `CoreCompile` and runs no analyzers. CI uses `/t:Build /m` because a runner checkout is cold." |
| `.claude/rules/csharp.md` § Toolchain item 3, first continuation bullet | R4 (same text, applied to the type-check step) |

`CLAUDE.md` § CUT3 and § "C# Toolchain (run in this exact order)" are condensed command lists whose
authoritative expansion is § C#1; `.claude/skills/csharp-qa-gate/SKILL.md` is a procedure that cites
the same commands. The rationale is carried in full at the two normative sites above.

---

## Step 3 — type-check

**CI (`ci.yml`, job step `Build with nullable warnings treated as errors`), with its six-line
in-workflow comment:**

```powershell
# Use /t:Rebuild (not /t:Build) so this step always performs a genuine full
# recompile. Enforcement now relies entirely on each file's own #nullable
# enable pragma (the repo's per-file opt-in convention; UtilitiesCS.csproj and
# SVGControl.csproj carry no project-level <Nullable> element) plus
# /p:TreatWarningsAsErrors=true. MSBuild's incremental up-to-date check does
# not invalidate on this command-line property change alone, so a plain
# /t:Build would silently skip recompilation and never enforce this gate.
& msbuild $env:SOLUTION_PATH /t:Rebuild /m /p:Configuration=Debug `
    "/p:Platform=Any CPU" `
    /p:TreatWarningsAsErrors=true
```

**Documented (after this feature), TYPECHECK:**

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

| Token | CI | Documented | Same |
|---|---|---|---|
| solution | `$env:SOLUTION_PATH` | `TaskMaster.sln` | equivalent (solution token) |
| target | `/t:Rebuild` | `/t:Rebuild` | yes |
| parallelism | `/m` | `/m` | yes |
| configuration | `/p:Configuration=Debug` | `/p:Configuration=Debug` | yes |
| platform | `"/p:Platform=Any CPU"` | `"/p:Platform=Any CPU"` | yes |
| warnings-as-errors | `/p:TreatWarningsAsErrors=true` | `/p:TreatWarningsAsErrors=true` | yes |
| `/p:Nullable=enable` | **absent** | **absent** | yes |

**The documented type-check command is character-for-character identical to CI's, modulo the solution
token.** There is **no** deliberate difference at step 3.

**In-line rationale locations:** `CLAUDE.md` § C#1 item 3 ("This is character-for-character the
command in `.github/workflows/ci.yml` (step 'Build with nullable warnings treated as errors'). Two
properties of it are load-bearing and must not be 'restored': ... **Do not add
`/p:Nullable=enable`** ... **Do not use `/t:Build`** ..."), and `.claude/rules/csharp.md` § Toolchain
item 3 continuation bullet (sentence R5).

---

## Independent corroboration on `main`'s tip

`FEATURE/evidence/baseline/baseline-ci-parity-on-main.2026-08-10T15-05.md` records that on `main`'s
tip (`a682c7a2`) all three of these CI steps **succeeded**:

| Conclusion | Step |
|---|---|
| success | `Verify formatting` (`dotnet csharpier check .`) |
| success | `Build with analyzers and code style enforcement` |
| success | `Build with nullable warnings treated as errors` (`/t:Rebuild /m`, no `/p:Nullable=enable`) |
| failure | `Run MSTest suite with coverage` — outside this feature's scope, see [P6-T9] |

## Output Summary

AC5 is satisfied. The documented **format** command resolves the same manifest-pinned CSharpier 1.2.6
as `ci.yml:93`, differing only in the explicit `tool run` spelling, whose rationale is stated in-line
at both normative sites. The documented **type-check** command is character-for-character identical
to `ci.yml`'s, modulo the solution token, with `/p:Nullable=enable` deliberately absent from both.
The documented **analyzer** command carries exactly **one** deliberate difference from CI
(`/t:Rebuild` vs `/t:Build`), whose cold-runner-versus-warm-tree rationale is stated in-line at
`CLAUDE.md` § C#1 item 2 and `.claude/rules/csharp.md` § Toolchain items 2 and 3.
