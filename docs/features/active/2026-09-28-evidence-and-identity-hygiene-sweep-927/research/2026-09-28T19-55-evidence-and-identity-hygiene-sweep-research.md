# Research — evidence and identity hygiene sweep (Issue #927)

- Timestamp: 2026-09-28T19-55
- Issue: #927 (bug, work mode full-bug), branch `bug/evidence-and-identity-hygiene-sweep-927`
- Base measured by the orchestrator: `origin/main` at `177b6d78e`
- Mode: preparation-mode research only; no source, configuration or evidence file was modified.
- Tooling available to this research: file read, content search and glob only. No shell. Every
  statement that needs `git` names the exact command the executor must run.
- Identifier hygiene of this document: the developer account name, the host name, the 8.3
  short-name form of the account and every absolute host path are referred to by category or by
  the placeholders `<repo-root>`, `<user-profile>`, `<user>` and `<host>`. No drive-rooted profile
  path is written contiguously anywhere in this file, so the guard proposed below does not flag
  this file once it lands.

Evidence tags: `[V-file]` verified by reading a tracked file; `[V-grep]` verified by content
search over the working tree; `[V-brief]` figure supplied by the orchestrator's `git ls-files` /
`git grep` measurement (masked); `[V-web]` verified against an external page; `[needs-git]` cannot
be verified without a shell and names the command.

---

## 1. Current-state analysis

### 1.1 CI pipeline facts that constrain the guard

- `.github/workflows/ci.yml` is a pure orchestrator: six `uses:` jobs, no inline steps, caller-owned
  concurrency group (`ci.yml:13-35`) `[V-file]`. The README states the topology rule "zero `needs:`
  edges" and that each callee declares `workflow_call` plus `workflow_dispatch`
  (`.github/workflows/README.md:25-31, 138-142`) `[V-file]`.
- `_pester.yml` runs Pester 5.6.1 pinned on `windows-latest` with an explicit path list:
  `Run.Path = @('tests/scripts/dependencies', 'tests/scripts/vscode')` (`_pester.yml:41`) and
  `CodeCoverage.Path = @('scripts/dependencies', 'scripts/vscode')` (`_pester.yml:45`). The gate
  fails on any failed test and when the JaCoCo `LINE` percentage is below 80 (`_pester.yml:70-71`)
  `[V-file]`. Consequence: a test file placed in a new folder under `tests/scripts/` is **not**
  discovered and a script in a new folder under `scripts/` is **not** in the coverage denominator
  until both arrays are extended. `scripts/dev-tools/run-actionlint.ps1` is the existing example of
  a script outside both lists; it has no test file and is not measured `[V-file]`.
- The README table row for `_pester.yml` (`README.md:23`) says the suite runs "over
  `tests/scripts/vscode`"; the workflow also runs `tests/scripts/dependencies`. This is
  documentation drift to correct when the row is edited for the new folder.
- `_actionlint.yml` lints every workflow file on `ubuntu-latest` (`_actionlint.yml:13-29`), so the
  new callee must be actionlint-clean `[V-file]`.
- `_format-check.yml` (`[V-file]`, 42 lines) is the smallest callee and is the shape to copy:
  `on: workflow_call + workflow_dispatch`, `permissions: contents: read`, one job, checkout with
  `fetch-depth: 1`, `shell: pwsh` steps.
- `.claude/rules/ci-workflows.md` requires a `pwsh` step that deliberately invokes a failing command
  to reset `$LASTEXITCODE` or end with an explicit `exit`. A gate step's own non-zero exit is the
  signal and must propagate (`README.md:271-280`) `[V-file]`.
- `tests/scripts/dependencies/DependabotConfig.Tests.ps1:111-154` enumerates every `*.yml` under
  `.github/workflows` for `setup-nuget` steps. A callee with no such step contributes no record, so
  adding a workflow file does not break that suite `[V-file]`.
- Branch protection: the `main` ruleset (`18572843`) uses
  `strict_required_status_checks_policy: true`; a new required context must be captured from a live
  run and applied by one atomic PUT (`README.md:196-259`) `[V-file]`. Context names take the form
  `<caller job id> / <callee job name>`.
- The Ubuntu 24.04 hosted runner image ships PowerShell 7.6.6 and Git 2.55.0 `[V-web]`, so a
  `pwsh` guard can run on `ubuntu-latest` without an install step.

### 1.2 Repository script and test conventions the guard must match

- Every entry-point script under `scripts/vscode` declares functions and runs its main body only
  when `$MyInvocation.InvocationName -ne '.'` (`Invoke-MSTest.ps1:260`,
  `Invoke-MSTestWithCoverage.ps1:437`, `Invoke-VSBuild.ps1:268`, `Invoke-Restore.ps1:120`,
  `Sync-PackageReferences.ps1:421`, `Install-RepoDotNetSdk.ps1:109`) `[V-grep]`. Tests dot-source
  the script in `BeforeAll` and call the functions directly
  (`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:3-7`) `[V-file]`.
- Large scripts are split into `Verb-Noun.Part.ps1` part files dot-sourced by the main file to stay
  under the 500-line ceiling (`Invoke-MSTestWithCoverage.Helpers.ps1:1-6`,
  `Invoke-MSTestWithCoverage.Projection.ps1:3-7`) `[V-file]`.
- Pure functions take document text as a string and never touch disk; fixtures are here-strings
  cast to `[xml]` inside the test (`Invoke-MSTest.TrxSummary.ps1:8-10`, spec #873 "Fixture
  mechanism") `[V-file]`. External executables are reached only through `Invoke-<Tool>Exe -<Tool>Args`
  wrapper seams that tests mock (`.claude/rules/powershell.md`, Design Seams) `[V-file]`.
- The committed-evidence convention is in `CLAUDE.md`, section "Committed Test Evidence Format":
  permitted forms are the package-level JaCoCo projection, the one-line first-party summary, and the
  test-result summary; raw collector and raw test-platform documents are prohibited "in any form,
  including under a feature folder's evidence tree" `[V-file]`.
- The projection shape the repository emits: root `<report name="TaskMaster">`, one `<package>` per
  package, exactly two `<counter>` children (LINE, BRANCH), no class, method, sourcefile or line
  element (`Invoke-MSTestWithCoverage.Projection.ps1:14-81`) `[V-file]`. This is the content
  discriminator between a permitted projection and a raw JaCoCo document.
- `.csharpierignore` already excludes `**/evidence/**`, `*.cobertura.xml`, `*.coverage`,
  `*.coveragexml`, `*.trx` from the formatter (`.csharpierignore:4-8`) `[V-file]`; committed
  projections under evidence trees are therefore not formatter inputs and no formatter change is
  needed for any file this item adds or removes.
- `.gitignore` already ignores `*.coverage`, `*.coveragexml` (`.gitignore:140-141`) and the
  repository `coverage/` directory (`.gitignore:144-145`); it has **no** entry for `*.trx` or for
  any Cobertura name pattern `[V-file]`. `.gitattributes` sets `* text=auto` `[V-file]`;
  `.editorconfig:669` sets `end_of_line = crlf` `[V-file]`.

### 1.3 Prior work this item builds on

- Issue #873 (PR #881) delivered the projection writer, the TRX summary, explicit
  `/ResultsDirectory:` and `LogFileName=` on both argument builders, the `CLAUDE.md` convention
  section, and rule text requiring a per-plan residual scan to include the plan file and to pair a
  zero count with a parse check. It explicitly excluded the historical sweep, an executable
  redaction tool and any `.gitignore` change (spec #873 "Out of scope", lines 69-77) `[V-file]`.
  Its identifier baseline derived the tokens at run time from `$env:USERPROFILE` and
  `$env:COMPUTERNAME` and recorded counts only (`evidence/baseline/p0-t14-identifier-leak-baseline.md`)
  `[V-file]`; that is the precedent for every gate proposed here.
- The 2026-09-11 parallel run planned, for the withdrawn #602 item, a committed
  `scripts/dev-tools/Repair-HostIdentifierLeak.ps1` with a test under `tests/scripts/dev-tools/`
  (`docs/features/parallel/bugs-2026-09-11/parallel.md:50-54`) `[V-file]`. Neither file exists
  (`scripts/dev-tools/` holds only `run-actionlint.ps1`) `[V-grep]`. The orchestrator's constraint
  for this item is the opposite: the redaction helper is a throwaway kept outside the repository;
  only the guard is committed.
- The #602 promoted record already proposed "a repo-wide grep-based pre-commit or CI check" and
  named the placeholder convention `<user-profile>` / `<host>`
  (`docs/features/potential/promoted/2026-09-02-committed-host-identity-leaks.md:67-68`) `[V-file]`.

---

## 2. Question 1 — CI guard design

### 2.1 Implementation language: PowerShell under `scripts/`, Pester under `tests/scripts/`

Compared:

| Option | Fit | Verdict |
|---|---|---|
| PowerShell script under `scripts/<folder>/` with Pester tests under `tests/scripts/<folder>/` | Matches `scripts/vscode` and `scripts/dependencies`; both are already CI-consumed (`_mstest-coverage.yml`, `dependabot-repair.yml`); the only PowerShell coverage gate in CI is `_pester.yml`; the wrapper-seam and in-memory-fixture patterns already exist; `.claude/rules/powershell.md` governs style and the 500-line ceiling. | **Selected** |
| bash under `scripts/bash/` | `scripts/bash/` has library scripts and a `shell-qc.sh` but there is no `tests/scripts/bash/` tree and no CI job runs any bash test or measures bash coverage (`ci.yml` has no bash gate; Glob of `tests/**` returns only `tests/scripts/dependencies` and `tests/scripts/vscode`) `[V-grep]`. A bash guard would be the only ungated, uncovered gate script in the pipeline, which contradicts the coverage policy that every production script is in the denominator. | Rejected |

### 2.2 New files (each well under 500 lines)

| Path | Role | Estimated size |
|---|---|---|
| `scripts/hygiene/Test-RepositoryHygiene.ps1` | Entry point. Dot-sources the two part files; `Invoke-RepositoryHygieneMain` enumerates tracked files through the git wrapper, applies both rules, prints one `HYGIENE <rule> <path>[:<line>]` line per finding plus a `HYGIENE Findings=<n>` summary, and `exit 1` when `n > 0`. Guarded by `if ($MyInvocation.InvocationName -ne '.')`. | ~120 lines |
| `scripts/hygiene/Test-RepositoryHygiene.Rules.ps1` | Pure functions: `Get-UserProfilePathPattern` (returns the regex string), `Find-UserProfilePathMatch -Text` (returns line-numbered matches), `Get-RawEvidenceDocumentKind -RelativePath -Content` (returns one of `trx`, `cobertura`, `dotnet-coverage`, `opencover`, `jacoco-raw`, `jacoco-projection`, `none`). No I/O. | ~150 lines |
| `scripts/hygiene/Test-RepositoryHygiene.Git.ps1` | `Invoke-GitExe -GitArgs <string[]>` wrapper (splats into `git @GitArgs 2>&1`, throws on non-zero `$LASTEXITCODE`), `Get-TrackedFileRecord` (parses `git ls-files --eol -z` into `Path`, `IsBinaryInIndex`), and `Read-TrackedFileText -Path -ReadContent <scriptblock>` adapter whose default reads bytes and decodes by BOM (UTF-8, UTF-16 LE/BE) so a UTF-16 file is scanned rather than skipped. | ~110 lines |
| `tests/scripts/hygiene/Test-RepositoryHygiene.Rules.Tests.ps1` | In-memory rule tests (section 6.3 lists the cases). | ~200 lines |
| `tests/scripts/hygiene/Test-RepositoryHygiene.Git.Tests.ps1` | `ls-files --eol -z` parser tests over literal strings; BOM decoding tests over byte arrays built in the test. | ~120 lines |
| `tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1` | Orchestration tests with `Invoke-GitExe` mocked (mock body `param([string[]]$GitArgs)`) and `-ReadContent` injected; asserts findings and the exit-code decision without any file existing. | ~150 lines |
| `.github/workflows/_hygiene.yml` | Reusable callee (section 2.3). | ~35 lines |

Naming follows the `Verb-Noun.Part.ps1` and `Verb-Noun.Part.Tests.ps1` convention already used in
`scripts/vscode` and `tests/scripts/vscode`. `Test` is an approved verb. The folder name `hygiene`
is new; `scripts/dependencies` is the precedent for a purpose-named folder consumed by a workflow.

Fixture rule for the three test files: a violating fixture must be assembled at run time
(for example `'C:' + '\Users\' + 'alice\repos\x'`) so the tracked test file never contains a
contiguous profile path; otherwise the guard's own tests trip the guard. The same rule applies to
`.trx`/`.xml` fixtures: the raw-document rule is extension-gated (section 3.1), so a `<TestRun>` or
`<coverage>` here-string inside a `.ps1` file is not a finding.

### 2.3 Workflow wiring

`.github/workflows/_hygiene.yml` (shape copied from `_format-check.yml`):

```yaml
name: hygiene

on:
  workflow_call:
  workflow_dispatch:

permissions:
  contents: read

jobs:
  hygiene:
    name: Repository hygiene guard
    runs-on: ubuntu-latest
    timeout-minutes: 10

    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
        with:
          fetch-depth: 1

      - name: Run repository hygiene guard
        shell: pwsh
        run: ./scripts/hygiene/Test-RepositoryHygiene.ps1
```

`ci.yml` gains one job appended after `pester`, in the same form as the other six
(`ci.yml:33-35`):

```yaml
  hygiene:
    name: hygiene
    uses: ./.github/workflows/_hygiene.yml
```

Predicted required-check context: `hygiene / Repository hygiene guard`. Per the README procedure
it must be captured from a live run and added to ruleset `18572843` by one atomic PUT after the PR
is green (`README.md:206-259`). Until that PUT, the context reports but is not required.

`ubuntu-latest` is chosen over `windows-latest` because the guard needs only `git` and `pwsh`
(both present `[V-web]`), starts faster, and `git ls-files` output is separator-normalised on both
platforms. The Pester unit tests still run on `windows-latest` inside `_pester.yml`; the parser and
rule functions are platform-neutral (they operate on strings), so this split is safe.

The gate step relies on the script's own `exit 1`; no `$LASTEXITCODE` reset is needed because the
step has no deliberately-failing nested command (`.claude/rules/ci-workflows.md`).

### 2.4 `_pester.yml` changes and coverage

- `_pester.yml:41` becomes
  `@('tests/scripts/dependencies', 'tests/scripts/hygiene', 'tests/scripts/vscode')`.
- `_pester.yml:45` becomes `@('scripts/dependencies', 'scripts/hygiene', 'scripts/vscode')`.
- Without these two edits `_pester.yml` would **not** run the new tests and would **not** measure
  the new scripts `[V-file]`.
- PowerShell coverage in CI is measured only for the listed `scripts/` folders, at a LINE floor of
  80 (`_pester.yml:71`), which matches `CLAUDE.md` UT2 ("PowerShell line coverage must remain
  `>= 80%`") and not the 85 in `.claude/rules/general-unit-test.md`. `CLAUDE.md` is the file loaded
  into every session and the maintainer's 2026-09-11 decision (#563) is recorded there; the
  executor should plan against 80 in CI and the new-code target of 90 from `CLAUDE.md` UT2.
- Adding `scripts/hygiene` to the denominator lowers the aggregate only by the entry-point block and
  the `git` call inside the wrapper, which are the same uncovered shapes the existing scripts carry.
  Keep both minimal.
- `README.md:15-23` table gains a row for `_hygiene.yml`; the `_pester.yml` row is corrected to name
  all three test folders; the contexts list at `README.md:176-183` gains the seventh entry.

---

## 3. Question 2 — Guard rules

### 3.1 Rule A — raw evidence document (content-classified, extension-gated)

Classification of every tracked `*.xml` under `docs/features` by root element `[V-grep]`:

| Class | Root element / discriminator | Files | Disposition |
|---|---|---|---|
| Test-platform document | extension `.trx`; every one has a `<TestRun` root | 332 | remove |
| Cobertura collector document | `.xml` whose root is `<coverage` | 243 | remove |
| dotnet-coverage native document | `.xml` whose root is `<results>` | 23 | remove |
| Raw JaCoCo (Pester) document | `.xml` whose root is `<report` **and** which contains `<class ` / `<sourcefile ` / `<method ` / `<line ` | 27 | remove (recommended, see note) |
| Package-level JaCoCo projection | `.xml` whose root is `<report` and which contains only `<package>` and `<counter>` children | 18 | keep |
| `*.coverage`, `*.coveragexml`, OpenCover `<CoverageSession` | none tracked | 0 | rule retained for the future |

Total `.xml` under `docs/features`: 311 = 243 + 23 + 27 + 18. Raw documents in total: 625.

Reconciliation with the issue and brief figures:

- The issue's 248 "`*cobertura*.xml`" = 240 files whose **name** contains `cobertura` + 8 files that
  match only through a **directory** segment: the five Pester JaCoCo documents under
  `docs/features/archive/2026-08-10-cobertura-coverage-arithmetic-441/evidence/**` and the three
  under `docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/evidence/**`
  `[V-grep]`. Those 8 are raw Pester documents, which is why the raw-JaCoCo class is included in
  the recommendation rather than left as a name accident.
- 243 Cobertura roots versus 240 cobertura-named files: three Cobertura documents carry no
  `cobertura` in their name (`docs/features/archive/2026-07-03-quickfiler-navigation-key-collision-232/evidence/coverage/2026-07-03T16-58/coverage.xml`,
  `docs/features/archive/2026-07-10-swordfish-raw-usage-cleanup-310/evidence/baseline/baseline-coverage-repository.xml`
  and `.../qa-gates/final-coverage-repository.xml`), and two cobertura-named documents under
  `docs/features/archive/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/**`
  put the root tag on its own line with attributes on the next line `[V-file]`. Both facts mean the
  removal list and the guard must be **content**-based; a `git ls-files` name pattern misses three
  and a naive "`<coverage ` followed by a space" match misses two.
- The 23 `<results>` documents (folders 214, 211, 191, 183, 177, 181) are `dotnet-coverage` native
  XML, not named cobertura, and are outside the issue's count. They are raw collector documents by
  the `CLAUDE.md` rule. Content search for a drive-rooted profile path over them returns 0 files
  `[V-grep]`; they are removed for the policy reason, not for an identifier they carry.
- The 27 raw JaCoCo documents carry no profile path either (0 matches) `[V-grep]`. Five of them are
  named `*.jacoco.xml` (folders 815, 873, 494) yet contain class elements, so the `.jacoco.xml`
  suffix does not identify a projection; only content does.
- 132 of the 332 `.trx` files still contain a drive-rooted profile path `[V-grep]`; two `.trx` file
  **names** follow the default vstest naming `<account>_<host>_<timestamp>_net481.trx` (folders 813
  and 181), so those paths themselves carry both identifiers and `git rm` is the only fix.

Classifier contract (`Get-RawEvidenceDocumentKind`):

1. Extension gate first: `.trx` -> `trx`; `.coverage`/`.coveragexml` -> `dotnet-coverage`; any
   extension other than `.xml` -> `none` (this is what keeps XML here-strings in `.ps1` tests and
   fenced XML in Markdown out of scope).
2. For `.xml`: strip BOM, skip the XML declaration, comments, processing instructions and a
   `<!DOCTYPE ...>` (JaCoCo declares one; `_pester.yml:54-58` records that hazard), take the first
   element name where the name is terminated by whitespace, `>` or `/` (the 139-folder case).
3. Map `coverage` -> `cobertura`, `results` -> `dotnet-coverage`, `TestRun` -> `trx`,
   `CoverageSession` -> `opencover`, `report` -> `jacoco-raw` if the document matches
   `<(class|sourcefile|method|line)[\s>]`, else `jacoco-projection`; anything else -> `none`.
4. Finding if kind is not `none` and not `jacoco-projection`.

Scope: all tracked paths, not only `docs/features`; the extension gate makes that cheap, and
`git ls-files` is the enumeration so ignored working-tree output under `coverage/` is never read.

### 3.2 Rule B — Windows user-profile path (generic, no identifier embedded)

Pattern (case-insensitive):

```text
[a-z]:[\\/]+users[\\/]+[a-z0-9_.~-]
```

- `[a-z]:` — any drive letter, upper or lower case under `IgnoreCase`.
- `[\\/]+` — one or more separators, which covers backslash, forward slash, the doubled
  (JSON-escaped) backslash, and mixed runs; no separate alternative is needed for `\\\\`.
- `users` — the profile parent, any case, so `USERS`, `users`, `Users` all match.
- `[a-z0-9_.~-]` — the **first** character of the user segment must be alphanumeric, underscore,
  dot, tilde or hyphen. `<user>`, `<user-profile>` and `<repo-root>` begin with `<` and therefore
  never match. The 8.3 form (six characters, tilde, digit) begins with a letter and matches.
- The pattern deliberately matches the legacy tokens `redacted-account`, `redacted`, `redacted-user`,
  `user`, `testuser`, `test`, `Public` and any other user segment. The rule has **no allowlist**;
  section 3.4 normalises the documents instead.
- Placeholder forms that start with `<` such as `<drive>:\Users\<segment>` in this document do not
  match because `>` is not a drive letter.
- Dialect: the same string is valid as a .NET regex (`[regex]::new($pattern, 'IgnoreCase')`) and as
  a POSIX ERE for `git grep -i -E` (inside a bracket expression a backslash is literal in ERE and an
  escape in .NET; both read `[\\/]` as the set backslash-or-slash). Keep the pattern inside this
  common subset: no `\d`, no `\b`, no lookaround.

Line reporting: the guard reports `path:line` for the first match per line so a reviewer can act on
the output; it does not print the matched text, which would echo the identifier into CI logs.

### 3.3 Exclusions

- `.claude/**` only. Reason: push-down owned; companion upstream issue #932. Implement as a
  path-prefix test on the `git ls-files` output (`$Path.StartsWith('.claude/')`), not as a git
  pathspec, so the exclusion is unit-testable in memory.
- `.mcp.json` and `.codex/config.toml` need **no** exclusion from either rule. Each contains the
  package scope `@<account>/drm-copilot-mcp` exactly once `[V-grep]` and neither contains a
  drive-rooted profile path (the profile-path search over both returns nothing) `[V-grep]`. They
  trip only the manual bare-account gate in section 8, which excludes them by pathspec.
- No exclusion for `tests/**` or for this feature's folder. The guard's own fixtures are assembled
  at run time (section 2.2) and this document contains no contiguous profile path.

### 3.4 Legacy redaction tokens

Observed forms `[V-brief]`: user segment `redacted-account` (9 files), `redacted` (24),
`redacted-user` (17); host forms `REDACTED-HOST`, `HOST`, `host`, and `&lt;host&gt;` (the last only
inside `.trx`, which are removed). Recommendation: normalise Markdown and other text files to the
canonical placeholders so the guard needs no allowlist:

| Existing form (user segment of a profile path) | Replacement |
|---|---|
| `<drive>:\Users\redacted-account\repos\TaskMaster...` and separator variants | `<repo-root>...` (rule 1 in section 7) |
| `<drive>:\Users\{redacted-account,redacted-user,redacted}\...` | `<user-profile>\...` |
| bare `redacted-account`, `redacted-user` outside a path | `<user>` |
| `REDACTED-HOST` | `<host>` |
| bare `HOST` / `host` | leave unchanged: both are ordinary English words and prose in the tree uses them; a blanket replacement would corrupt sentences. They are not identifiers and do not trip the guard. |
| `&lt;host&gt;` inside an XML-family file that remains | leave unchanged: it is the correctly escaped form of the canonical placeholder; un-escaping it inside XML markup is the corruption the #873 research (R4.3) records. None of the 18 retained projections contains it (0 matches for the profile pattern; the projections carry no attribute that could hold a host). |

The fixture segments `testuser`, `test`, `Test`, `user` in the 13 C# test files are handled by
rewriting the **root** (section 4), not the segment, so the token assertions in those tests keep
their meaning.

---

## 4. Question 3 — Triage of the 13 `*.Test` fixture files and the helper test

No production code keys on the `Users` segment: the only production reference is
`Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)` at
`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs:230`, which reads the environment and parses
nothing `[V-grep]`. Every fixture below treats the path as an opaque string, a substring-matching
input, or a "drive-rooted" example. Therefore **no guard exemption is needed**: rewrite each fixture
root to a non-profile root and keep the user-segment token where a `NotContain` assertion depends
on it. Recommended root: `C:\Fixtures\<segment>\...` (drive-rooted, so "drive-rooted" semantics
survive; no `Users` segment, so the guard is silent). CSharpier must be re-run because the literals
change length and lines near the wrap width may re-flow.

| File:lines | What the test asserts | Profile path semantically required? | Action | Test methods touched |
|---|---|---|---|---|
| `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsOneDriveResolutionTests.cs:17-19` | `ResolveOneDriveRoot` returns the first non-blank of three injected environment values in priority order; `:87` asserts the failure message does **not** contain `testuser` | No. Values are opaque; only the `testuser` token matters for `:87`. | Rewrite three constants to `C:\Fixtures\testuser\OneDrive - Contoso`, `...\OneDrive`, `...\OneDrive - Personal` | `ResolveOneDriveRoot_AllThreeVariablesSet_PicksTheHighestPriority`, `_CommercialUnset_FallsToTheSecondPriority`, `_OnlyPersonalSet_FallsToTheThirdPriority`, `_WhitespaceOnlyValues_AreTreatedAsUnset`, `_NoVariableSet_FailsExplicitlyWithARedactedDiagnostic` |
| `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs:38,44,59-60,66,81,86,123,162,176` | Pure ordinal `string.Contains` longest-value match over an in-memory dictionary | No. Substring semantics only. `:81/:86` need a case mismatch, `:59-60` need a shorter and a longer candidate. | Rewrite roots to `C:\Fixtures\Test\...`; `:59` root-only candidate (drive plus the profile parent, no user segment) -> `C:\Fixtures`; `:81` upper-case profile literal -> `C:\FIXTURES\TEST`; `:86` lowercase form -> `c:\fixtures\test\file.txt` | `MatchBestSpecialFolder_PathContainsKnownValue_ReturnsThatKey`, `_TwoCandidatesContained_LongerValueKeyWins`, `_CaseMismatch_DoesNotMatch_ReturnsNull`, `_NoValueContained_ReturnsNull`, `_EmptyPath_NoValueContained_ReturnsNull`, `_NullPath_ThrowsNullReferenceException` |
| `TaskMaster.Test/AppGlobals/AppAutoFileObjectsFolderPredictorTests.cs:39` | Mock `SpecialFolders["AppData"]` value used to build a store config; opaque | No | Rewrite to `C:\Fixtures\test\AppData` | every test using `CreateMockGlobalsWithAppData()` (AC23 tests in that class) |
| `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs:30-35` | Six special-folder roots for `FilePathHelperConverter` round-trips; opaque | No | Rewrite the doubled-backslash profile prefix (drive, profile parent, `user`) to `C:\\Fixtures\\user\\` (escaped-string form; keep the doubled backslashes) | all tests in the class (setup-level) |
| `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:72,182` | `IsValidFilingSelection` rejects a drive-rooted value; the composition guard iterates candidates | Only "drive-rooted" is required | Rewrite to `C:\Fixtures\testuser\OneDrive - Contoso` | `IsValidFilingSelection_DriveRootedSelection_IsRejected`, `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary` |
| `UtilitiesCS.Test/NewtonsoftHelpers/FilePathHelperConverterTests.cs:175,189,260,266` | Combine / split of a path against a special-folder value; opaque | No | Rewrite to `C:\Fixtures\Test\AppData...` | `ExtractFolderPath_WithKnownSpecialFolder_CombinesRelativePath`, `GetSerializablePath_WithMatchingSpecialFolder_ReturnsNameAndRelativePath` |
| `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs:118` | GWSO store exclusion by case-insensitive token `\Google\Google Workspace Sync\` in the file path | No; only the token substring matters | Replace the three-letter real-account prefix used as the user segment with `testuser` **and** the root with `C:\Fixtures`: `C:\Fixtures\testuser\Google\Google Workspace Sync\sync.ost` | `Init_WhenStoresMatchFilters_ProjectsOnlyIncludedStores` |
| `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperDisableTests.cs:67` | Same token match, mixed case | No | Same rewrite (`...\GOOGLE\Google Apps Sync\sync.ost`) | `InclusionFilters_ExcludeMatchingGwsoPaths_IgnoringCase` |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreFilterAttributionTests.cs:80,152,222,313` | `StoreFilterAttribution.Decide` attributes the GWSO rule from `GwsoTokens` (`:16-20`) | No | Same rewrite at four sites | `Decide_FilePathContainsGwsoToken_WhenGwsoExcluded_ReturnsFalseGwsoFilePath` and the three sibling `Decide_*` tests at those lines |
| `UtilitiesCS.Test/EmailIntelligence/LcppnFolderPredictorStore_Tests.cs:19` | `BuildConfig(AppData)` composes `Bayesian\LcppnFolder.json` under the given root; opaque | No | Rewrite to `C:\Fixtures\test\AppData` | `BuildConfig_TargetsDedicatedFileInBayesianFolder` and any other test reading the `AppData` constant |
| `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs:320,374` | `ResolvePaths` rejects a store-root stem; `:354` asserts the message does not contain `fsAncestor` | No; the `NotContain(fsAncestor)` assertion holds for any distinctive string | Rewrite both to `C:\Fixtures\testuser\OneDrive - Contoso` | `Issue614_ResolvePaths_WithStoreRootStem_RejectsNonRelativeStemWithoutLeakingIdentifiers`, `Issue614_ResolvePathsWithFolder_RejectsStoreRootStemThroughTheFolderOverload` |
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemContractTests.cs:54,136` | `IsFullOutlookPath` is true for a drive-rooted value; `:145-146` assert the message contains neither `testuser` nor `OneDrive` | Only "volume separator in position 1" is required | Rewrite to `C:\Fixtures\testuser\OneDrive - Contoso` (keeps both tokens) | `IsFullOutlookPath_DriveRootedValue_IsTrue`, `RequireArchiveRelativeStem_DriveRootedValue_ThrowsWithoutEmbeddingTheValue` |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterIssue614Tests.cs:18` | `ToFsFolderpath` prefixes the caller's fs root (which carries a space and a hyphen) without validating it | No | Rewrite to `C:\Fixtures\testuser\OneDrive - Contoso` | every test using `OneDriveRoot` (`ToFsFolderpath_DottedAndHyphenatedFilesystemRoot_Succeeds`, `ToFsFolderpath_DerivedSegmentContainingADot_Succeeds`, and the rest of the D5 matrix) |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:41-42,104,124` | `ConvertTo-KoverageRelativePath` strips the repo root **and** the sibling `<parent>\TaskMaster` root (`Invoke-MSTestWithCoverage.Helpers.ps1:74-80` derives the sibling from the leaf name only) | No. The real account profile path is used only as an arbitrary parent; `C:\repo` is already the fixture root elsewhere in the same file (`:31, :84, :172`). | Rewrite `:41` to `C:\repo\TaskMaster-wt-2026-07-04-12-57`, `:42` to `C:\repo\TaskMaster`, `:104` filename to `C:\repo\TaskMaster\ToDoModel\Data Model\ToDo\ToDoItem.cs`, `:124` `-RepoRoot` to `C:\repo\TaskMaster-wt-2026-07-04-12-57` | `strips active and stale TaskMaster roots while preserving already relative paths`, `normalizes stale TaskMaster roots before merging duplicate production class entries` |

Why an exemption mechanism is not recommended: a fixed list of fixture user tokens inside the guard
would make `<drive>:\Users\testuser\...` legal everywhere, including in evidence Markdown, and a
future author could reuse a listed token to mask a real path. Rewriting the roots removes the class
of string from the tree, so the guard rule stays a single regex with no data table.

If the plan nevertheless needs a temporary exemption (for example to land the guard before the C#
batch), the only acceptable mechanism is a **path**-scoped allowlist limited to the 14 files above,
checked in by name, and removed in the same PR; a token allowlist is not acceptable for the reason
above.

---

## 5. Question 4 — `docs/research/...` hit and `test-output.txt`

- `docs/research/2026-08-10-parallel-bug-flighting-and-surface-blockers.md:50, 66, 392` quote the
  upstream governance repository's absolute checkout path (`<user-profile>\repos\drm-copilot`, once
  with `\config\blast-radius.json`) `[V-grep]`. These are prose citations; replace the profile prefix
  with `<user-profile>` and keep the `\repos\drm-copilot...` suffix (rule 2 in section 7 does this
  automatically; rule 1 must not fire because the leaf is `drm-copilot`, not `TaskMaster`).
- `test-output.txt` (repository root) is a captured `VSTest version 18.4.0` console run
  (`test-output.txt:1-4`) `[V-file]`. References to its name across the tree are prose only:
  this feature's issue/spec/promoted record, historical diff-scope notes under
  `docs/features/archive/2026-03-19-utilities-coverage-part-three-87/**` that already list it as
  "artifact, non-issue-87" and as "deleted", one research note citing lines `1606-1609` of it, and
  the withdrawn #602 write-set entry in `docs/features/parallel/bugs-2026-09-11/parallel.md:53`
  `[V-grep]`. No script, test, workflow or task reads it. Safe to `git rm`.

---

## 6. Question 5 — Raw-document removal safety and `.gitignore`

### 6.1 Nothing executable reads the tracked raw documents

- `scripts/**`, `tests/**`, `.github/**` reference `.trx` / `cobertura.xml` only as the names of
  files the tooling **writes** under the ignored `coverage/` directory or as in-memory test
  fixtures (`Invoke-MSTestWithCoverage.ps1:9,282,284`, `_mstest-coverage.yml:103`,
  `tests/scripts/vscode/*.Tests.ps1`) `[V-grep]`. `scripts/temp-extract-coverage.ps1:2-3` reads
  `coverage\coverage.cobertura.xml` (ignored, not tracked) and writes a Markdown file; it does not
  read any evidence tree.
- The two feature-review coverage hooks read four fixed paths, none under a feature evidence tree,
  and read JaCoCo rather than Cobertura or TRX (spec #873, "Backward-compatibility expectations",
  line 181) `[V-file]`.
- Markdown references: 955 `.md` files under `docs/features` mention a `.trx` name and 1,639 mention
  a Cobertura name `[V-grep]`. They are narrative evidence records and will become dangling
  references after removal. Recommendation: tolerate them; do not attempt to rewrite prose links
  (no policy requires link validity in historical evidence, and the redaction pass already touches
  most of these files for identifiers). The plan should state this explicitly so a reviewer does not
  read a dangling link as an incomplete task.

### 6.2 Figures: rely on committed summaries, do not generate retroactive projections

- Every feature folder that holds a raw document also holds Markdown that quotes the figures the run
  produced (the 955/1,639 counts above are the evidence that the prose exists). The issue permits
  relying on the committed summary.
- A retroactive projection is not recommended: `ConvertTo-JacocoPackageProjection` is specified over
  the **post-processed** document and the #873 spec records that a raw collector document "would
  not reconcile" to the projection's reconciliation assertion (spec #873, invariant 2). Producing
  projections from 243 raw Cobertura documents would therefore either fail the reconciliation or
  require re-running the tooling per feature, neither of which this item should take on. The 23
  `<results>` documents have no projection path at all.
- Consequence for the plan: removal is a single `git rm` batch driven by the content classifier's
  list; no per-folder judgement is required.

### 6.3 `.gitignore` additions

Existing coverage: `*.coverage`, `*.coveragexml` (`.gitignore:140-141`), `coverage/*`
(`.gitignore:144`), `[Tt]est[Rr]esult*/` (`.gitignore:39`). Missing: `*.trx` and any Cobertura
name. Recommended lines, placed after `.gitignore:141`:

```gitignore
# Raw test-platform and coverage-collector documents are prohibited committed evidence
# (CLAUDE.md, "Committed Test Evidence Format"). The CI hygiene guard rejects them by content;
# these name patterns only stop an accidental `git add`.
*.trx
*cobertura*.xml
```

`*cobertura*.xml` rather than `*.cobertura.xml` because tracked names include
`baseline.cobertura.2026-08-27T20-01.xml` (folder 501), where the timestamp follows the marker.
`.gitignore` does not affect already-tracked files, so `git rm` must run first; after it, the
patterns prevent re-addition without `-f`. The `<results>` and raw-JaCoCo classes have no reliable
name pattern; the guard's content rule is the only protection for them, which is why the `.gitignore`
comment says so.

Post-change check: `git check-ignore -q docs/features/x/a.trx docs/features/x/b.cobertura.xml`
must exit 0 for both `[needs-git]`.

---

## 7. Question 6 — Redaction mechanics for the throwaway helper

The helper lives outside the repository (scratch directory), reads the identifiers from the
environment at run time, and is never committed. Inputs: the account token as the profile leaf
`Split-Path -Leaf $env:USERPROFILE` (the committed paths embed the profile directory name, which is
what the #873 baseline used; add `$env:USERNAME` as a second token only if it differs; a Git Bash
`basename` of the same variable is wrong on a backslash path, a defect recorded during the earlier
#602 preparation),
`$env:COMPUTERNAME` (host), `$env:USERPROFILE` (profile root), and the 8.3 leaf obtained with
`(New-Object -ComObject Scripting.FileSystemObject).GetFolder($env:USERPROFILE).ShortName`. The
repository root leaf is `TaskMaster`; worktree roots are `<user-profile>\repos\TaskMaster\.claude\worktrees\<id>`
and `<user-profile>\repos\TaskMaster-wt-<stamp>` (the latter shape is quoted in
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:41` `[V-file]`).

### 7.1 Ordered replacement table (longest match first; each rule is a .NET regex built at run time)

Let `SEP = [\\/]{1,2}`, `ACCT = <escaped account>|<escaped 8.3 leaf>|redacted-account|redacted-user|redacted`,
`PROFILE = [A-Za-z]:SEP(?i:users)SEP(?:ACCT)` (account alternation case-insensitive).

| # | Match (regex, run-time built) | Replacement | Notes |
|---|---|---|---|
| 1 | `PROFILE SEP repos SEP TaskMaster(?=[\\/]|$|[^A-Za-z0-9-])` | `<repo-root>` | Handles canonical root, `\.claude\worktrees\<id>` suffix (kept), JSON-escaped `\\` form, forward-slash form, 8.3 form, lowercase drive. `TaskMaster-wt-<stamp>` intentionally does **not** match this rule (the lookahead excludes `-`), so it falls to rule 2 and becomes `<user-profile>\repos\TaskMaster-wt-<stamp>`, which preserves the worktree distinction. |
| 2 | `PROFILE(?![A-Za-z0-9_.-])` | `<user-profile>` | Any other profile-rooted path, including the scratch-directory form `<user-profile>\AppData\Local\Temp\claude\...` and the upstream repo path in `docs/research`. |
| 3 | `(?<![A-Za-z0-9])<escaped account>(?![A-Za-z0-9])` case-insensitive | `<user>` | Bare account, including the hyphen-joined session key `C--Users-<account>-repos-TaskMaster` that appears inside scratch paths (hyphen is a boundary). Excludes `.mcp.json` and `.codex/config.toml` by path (scope string). |
| 4 | `(?<![A-Za-z0-9])<escaped 8.3 leaf>(?![A-Za-z0-9])` case-insensitive | `<user>` | Bare 8.3 form outside a path (rare; harmless if zero). |
| 5 | `(?<![A-Za-z0-9])<escaped host>(?![A-Za-z0-9-])` case-insensitive | `<host>` | Bare host; verify beforehand with `git grep -i -o -h <host>` piped through `Sort-Object -Unique` that the only contexts are host references `[needs-git]`. |
| 6 | `REDACTED-HOST` | `<host>` | Legacy token normalisation. |
| 7 | `(?<![A-Za-z0-9<])(?:redacted-account|redacted-user)(?![A-Za-z0-9>])` | `<user>` | Legacy bare tokens outside a path (rule 2 already consumed the in-path ones). Do not touch bare `redacted`, `HOST`, `host`. |

Ordering guarantees: rule 1 is a strict superstring of rule 2, and rules 2 and 3 are superstrings of
the bare tokens, so applying in this order never leaves a partial replacement such as
`<user-profile>\repos\TaskMaster` where `<repo-root>` was intended.

### 7.2 Idempotence

Every replacement string contains none of the seven match targets, so a second run over the output
performs zero substitutions. The helper must prove this: after writing, re-run the match phase in
dry-run mode and require zero matches; that is also the residual scan (section 7.4).

### 7.3 Encoding and line-ending preservation

- Read each file as **bytes**. Detect a BOM (EF BB BF; FF FE; FE FF); decode with that encoding or
  with UTF-8 (no BOM) otherwise. Re-encode with the **same** encoding and re-emit the same BOM.
  `[System.IO.File]::ReadAllText` plus `WriteAllText` with an explicit `UTF8Encoding($false)` would
  strip a BOM and could mis-decode UTF-16; do not use the defaults.
- Perform substitutions on the decoded string; a regex over the whole text preserves `\r\n`, `\n`,
  and the absence or presence of a trailing newline because nothing except the matched spans is
  touched.
- Whether any hit file is UTF-16 or BOM-prefixed cannot be determined with the tools available here.
  The brief records zero binary-only hits for the account or host under `git grep -a`, which means
  no UTF-16 file currently carries either token (git classifies UTF-16 as binary). The executor
  should still record the encoding distribution of the write set before rewriting:
  `git ls-files --eol -- <write-set> | Select-String 'i/-text'` lists index-binary files
  `[needs-git]`; any listed text file must be handled by the BOM branch above.
- `.gitattributes` `* text=auto` means git normalises line endings in the index; preserving the
  working-tree ending avoids whole-file diffs under a non-default `core.autocrlf` and keeps the
  review diff to the substituted lines.
- XML-family files that remain (the 18 projections) contain no identifiers and are not rewritten.
  If a future run must touch an XML file, the placeholder must be written as `&lt;...&gt;` inside
  markup and the file must be re-parsed afterwards (`[xml]` cast) — the #873 R4.3 corruption record.

### 7.4 Residual scan scope (#727 sub-finding 5)

The residual scan runs over **every tracked file** including plan files and this feature's own folder
(`docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/**`), with only
`.claude/**`, `.mcp.json` and `.codex/config.toml` excluded, and pairs the zero-count with:

1. the guard run (section 8, gate 6), and
2. a parse check over every XML-family file the helper rewrote (expected: none rewritten).

The helper's own output log must be written to the scratch directory, never under the feature
folder, because the log echoes matched paths.

---

## 8. Question 7 — Verification gates (all `[needs-git]`; identifiers from the environment)

Run from the repository root in PowerShell. `$LASTEXITCODE` from `git grep` is 1 when nothing
matches; the gates count lines rather than reading the exit code.

```powershell
$account = Split-Path -Leaf $env:USERPROFILE   # profile leaf; add $env:USERNAME as a second token if it differs
$hostName = $env:COMPUTERNAME
$short = (New-Object -ComObject Scripting.FileSystemObject).GetFolder($env:USERPROFILE).ShortName
$scope = @('--', '.', ':!.claude/', ':!.mcp.json', ':!.codex/config.toml')

# Gate 1 — account name, text files only, case-insensitive fixed string. Baseline 1,215. Expected 0.
@(git grep -I -l -i -F -e $account @scope).Count

# Gate 2 — host name. Baseline 186. Expected 0.
@(git grep -I -l -i -F -e $hostName -- . ':!.claude/').Count

# Gate 3 — 8.3 short form. Baseline 5. Expected 0.
@(git grep -I -l -i -F -e $short -- . ':!.claude/').Count

# Gate 4 — generic profile path, any user segment. Baseline 1,213. Expected 0.
@(git grep -I -l -i -E -e '[a-z]:[\\/]+users[\\/]+[a-z0-9_.~-]' -- . ':!.claude/').Count

# Gate 5 — raw documents by name and by content. Baselines 332 / 240 / 243+23+27+332. Expected 0.
@(git ls-files -- '*.trx' '*cobertura*.xml' '*.coverage' '*.coveragexml').Count
@(git grep -l -E '^<(coverage|results|TestRun)' -- '*.xml' '*.trx').Count
@(git grep -l -E '<(class|sourcefile|method|line)[ >]' -- 'docs/features/*.xml').Count

# Gate 6 — the guard over the real tree exits 0.
pwsh -NoProfile -File ./scripts/hygiene/Test-RepositoryHygiene.ps1; $LASTEXITCODE   # expected 0

# Gate 7 — Pester self-test: the in-memory violation cases fail the rule and the run is green.
Invoke-Pester -Path tests/scripts/hygiene -Output Detailed   # expected: 0 failed

# Gate 8 — ignore rules are live for the removed classes.
git check-ignore -q docs/features/x/a.trx docs/features/x/b.cobertura.xml; $LASTEXITCODE   # expected 0
```

Gate 5's third command is scoped to `docs/features` because `<class ` and `<line ` occur legitimately
in other tracked XML (for example analyzer or designer files); the guard itself is root-element
gated and does not need this scoping.

Baselines must be recorded **before** any change so each gate is observed failing (the "gates can
pass for reasons unrelated to correctness" lesson). Gate 6 is expected to exit 1 before the sweep
with a findings count equal to the sum of the raw-document population (625) plus the profile-path
file count (1,213); record that number too.

Guard self-test cases (Pester, in-memory, no file created):

- `Find-UserProfilePathMatch` returns one match for each of: backslash form, forward-slash form,
  doubled-backslash form, lowercase drive and `users`, upper-case `USERS`, and the 8.3 form; all
  built by concatenation at run time.
- Returns zero matches for `<user-profile>\repos\x`, `<repo-root>\a`, `<drive>:\Users\<user>`,
  a bare `C:\` root, and `C:\Fixtures\testuser\x`.
- `Get-RawEvidenceDocumentKind`: `.trx` -> `trx`; `<coverage` on its own line -> `cobertura`;
  `<results>` -> `dotnet-coverage`; JaCoCo with DOCTYPE and `<class ` -> `jacoco-raw`; JaCoCo with
  only package/counter -> `jacoco-projection`; `.ps1` containing `<TestRun` -> `none`; UTF-8 BOM
  prefixed document still classified.
- Orchestration: mocked `Invoke-GitExe` returns a three-record `ls-files --eol -z` payload
  (`.claude/x.md` with a violation, `docs/a.md` with a violation, `docs/b.jacoco.xml` projection);
  `-ReadContent` supplies the strings; findings count is exactly 1 and names `docs/a.md`; the
  `.claude/` record is excluded; exit decision is non-zero. A second run with clean content yields
  zero findings and a zero exit decision.

---

## 9. Question 8 — Coordination with the other five items of `bugs-2026-09-28`

- The parallel manifest for `bugs-2026-09-28` is not present on this branch (only
  `docs/features/parallel/bugs-2026-09-02`, `-06`, `-11`, `-17`, `bug-families-01`, `bugs-635-440`,
  `bugs-638-644-647` exist) `[V-grep]`, and no sibling `docs/features/active/2026-09-28-*` folder
  other than this one is checked out here. This item cannot edit their branches and does not need to.
- Guard timing on sibling PRs: for `pull_request` events GitHub runs the workflow files of the PR
  **head** ref (`README.md:206-211`). A sibling branch cut before this item merges does not contain
  `_hygiene.yml`, so the guard does not run on it until the branch is updated past `main`. Because
  the ruleset is `strict`, every sibling must update its branch before merging anyway, at which point
  the guard runs on the sibling's merge ref and must be green.
- Residual risk to siblings: their evidence Markdown is written by agents that quote tool banners
  and absolute paths verbatim (issue #927, "Suspected Cause"). Placeholder-only, projection-only
  evidence passes the raw-document rule by construction; the profile-path rule fails on any quoted
  absolute path. Mitigation the orchestrator can apply now: tell each sibling to run
  `pwsh -NoProfile -File ./scripts/hygiene/Test-RepositoryHygiene.ps1` after updating past `main`
  (or, before this item merges, to run gate 4 from section 8) and to use the four placeholders.
- Until the context is added to the ruleset (README procedure, after this PR's first green run), the
  guard reports but does not block. Recommend sequencing: merge this item first, run the ruleset PUT,
  then let siblings update and merge. Merging siblings first is also safe as long as their evidence
  passes gate 4; a sibling that merges non-compliant evidence before the guard is required simply
  becomes a new finding for a follow-up sweep and blocks the next unrelated PR until fixed, which is
  the disruptive case to avoid.
- Nothing in this item touches `TaskMaster.sln`, any `.csproj`, `packages.config` or `app.config`,
  so no mechanically-mergeable conflict with siblings is expected; the 13 C# test files are the
  only source-tree edits and are unlikely to overlap with unrelated bug fixes, but the plan should
  list them in the declared blast radius.

---

## 10. Question 9 — Executability and split

Recommendation: **one pull request, one plan, phased so the guard's own tests are green before the
guard runs over the tree.** Reasoning: the guard cannot be green on CI until the removal and the
redaction have landed on the same ref; splitting into two PRs leaves a window in which new raw
documents or identifiers can be committed with no gate, and doubles the ruleset choreography.

Phases inside the plan (each phase is an executable unit with its own gate):

1. **Phase 0 — baselines.** Record gates 1-5 before any change; record `git ls-files | Measure-Object`
   (expected 16,585 `[V-brief]`); record the encoding survey (section 7.3).
2. **Phase 1 — guard code and tests (PowerShell batch: 3 production + 3 test files, exactly the
   per-batch cap in `.claude/rules/powershell.md`).** Pester green; the guard run over the tree is
   expected to exit 1 with the recorded baseline count — that is the failing regression observation
   the bugfix workflow requires. `_pester.yml` path arrays extended in the same phase.
3. **Phase 2 — raw-document removal and `.gitignore`.** `git rm` driven by the classifier's output
   (run the guard with a `-ListRawDocuments` switch, or drive `git rm` from gate 5's three commands),
   plus `git rm test-output.txt`. Gate 5 -> 0, gate 8 -> 0.
4. **Phase 3 — C# and PowerShell fixture rewrites (14 files).** The C# change-budget router
   applies (13 test files); they are literal-only edits but the C# toolchain still runs in full:
   `csharpier format .`, both `msbuild /t:Rebuild` passes, and the MSTest-with-coverage run whose
   output stays under the ignored `coverage/` directory. Evidence for this phase is the test-result
   summary and the JaCoCo projection only. The 14 touched test methods listed in section 4 must pass.
5. **Phase 4 — redaction sweep with the external helper.** Rules 1-7 over the 1,661-file write set
   (181 feature folders, 6 promoted records, 16 other files `[V-brief]`), then the residual scan over
   the whole tree including the plan file and this folder. Gates 1-4 -> 0.
6. **Phase 5 — `_hygiene.yml`, `ci.yml`, README.** Gate 6 -> exit 0 locally; push; confirm the
   seventh context reports on the PR head; actionlint green.
7. **Phase 6 — ruleset.** After the green run, the maintainer applies the atomic PUT adding
   `hygiene / Repository hygiene guard` (captured from the live run, not typed).

If the orchestrator requires a split for review-size reasons, the only safe cut is between
phase 3 and phase 4 (PR A: guard code + removal + fixtures, with `_hygiene.yml` **not** wired into
`ci.yml`; PR B: redaction + wiring). PR A alone must not wire the guard because the tree still fails
rule B at that point.

Size note for reviewers: the removal diff is 625 deletions of multi-megabyte files and the
redaction diff touches about 1,661 files. The PR body should point reviewers at the gate outputs
rather than the diff.

---

## 11. Candidate approaches (summary) and rejected alternatives

Selected: content-classified raw-document rule + generic profile-path regex, implemented in
PowerShell with an `Invoke-GitExe` wrapper and a `-ReadContent` adapter, enumerating tracked files
via `git ls-files --eol -z`, run on `ubuntu-latest` by a new `_hygiene.yml` callee.

Rejected alternatives (kept brief):

- **Guard implemented as `git grep` invocations only.** Fast, but the regex would be evaluated by
  git's ERE engine in CI and by .NET in the Pester tests, so a dialect difference could produce a
  green unit test and a silent CI miss; the raw-JaCoCo discrimination also needs a second pass.
  The in-process design keeps one regex dialect and lets the same function serve tests and CI.
  `git grep` remains the right tool for the manual gates in section 8, where the identifier is a
  fixed string.
- **Name-based raw-document rule (`*.trx`, `*cobertura*.xml`).** Misses the three unnamed Cobertura
  documents, all 23 `<results>` documents and the 27 raw JaCoCo documents, and cannot tell a
  projection from a raw Pester document that shares the `.jacoco.xml` suffix.
- **Token allowlist for fixture user segments.** Rejected in section 4.
- **Committed redaction script.** The orchestrator constraint is a throwaway helper; a committed
  tool would also have to carry tests whose fixtures are exactly the strings the guard forbids.
- **Retroactive projections for every removed Cobertura document.** Rejected in section 6.2.

---

## 12. Numeric Derivation Evidence

### 12.1 Raw evidence document population under `docs/features` (proposed AC figure: 625 removed, 18 retained)

- Complete Family: every tracked file under `docs/features/**` that is a raw test-platform or raw
  coverage-collector document, plus every package-level JaCoCo projection there.
- Exhaustive Search Scope: all `*.xml` and `*.trx` under `docs/features` (glob, recursive). Other
  extensions (`*.coverage`, `*.coveragexml`) were globbed repository-wide and returned none.
- Inclusion Rules: root element in {`coverage`, `results`, `TestRun`} or extension `.trx`; JaCoCo
  `<report` roots that contain class/sourcefile/method/line elements.
- Exclusion Rules: JaCoCo `<report` roots containing only package/counter elements (retained).
- Primary Search Strategy: content search `^<coverage` over `**/*.xml` (243 files); `^<results>|^<results ` (23);
  `<sourcefile |<class name=` over `**/*.xml` (27); `<TestRun` over `**/*.trx` (332); `<report name=` (45).
- Primary Member Set: 243 + 23 + 27 + 332 raw; 45 - 27 = 18 projections.
- Primary Count: 625 raw, 18 retained, 311 total `.xml`.
- Cross-check Search Strategy: (a) a single combined root search
  `<CoverageDSPriv|<Coverage |<coverage version|<results|<testsuites|<RunSettings|<Project |<summary|<report `
  returning 68 files, whose members are exactly the 45 JaCoCo plus the 23 `<results>` files;
  (b) `<method name=|<line nr=` over `**/*.xml` returning the same 27 file paths as the primary
  JaCoCo-raw search; (c) an unconditional file enumeration of `**/*.trx` returning 332, equal to the
  `<TestRun` set; (d) the earlier line-start-with-space search `^<coverage |^<coverage>|<coverage line-rate`
  returning 241, plus the two 139-folder documents read directly whose root tag sits alone on line 2;
  (e) the orchestrator's name-based figure 240 plus the three non-cobertura-named Cobertura documents
  identified by path.
- Cross-check Member Set: 68 = 45 + 23; 27 = 27 (same paths); 332 = 332 (same paths); 241 + 2 = 243
  and 240 + 3 = 243 (same paths, reconciled by name in section 3.1).
- Cross-check Count: 625 raw, 18 retained.
- Member-set Comparison: identical for every class. The assertion may be proposed.

### 12.2 Fixture files carrying a profile path outside `docs/features` (proposed figure: 13 C# + 1 PowerShell)

- Complete Family: tracked `*.cs` under `*.Test/` and tracked `*.ps1` under `tests/` containing a
  drive-rooted profile path.
- Exhaustive Search Scope: glob `*.Test/**/*.cs` and `tests/**/*.ps1`, regex
  `[A-Za-z]:(\\\\|\\|/)+Users(\\\\|\\|/)+`, case-insensitive.
- Inclusion Rules: any match. Exclusion Rules: none.
- Primary Search Strategy: content search over `*.Test/**/*.cs` (files with matches listed by path)
  and over `{scripts,tests,.github}/**` plus root-level configuration files.
- Primary Member Set: the 13 C# files listed in section 4 and
  `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`.
- Primary Count: 14.
- Cross-check Search Strategy: the orchestrator's `git grep` user-segment table (`[V-brief]`), which
  attributes the non-`docs/features` matches per user segment: `user` (1 file), `testuser` (5),
  `test` (4), three-letter prefix (3), real account (1 PowerShell test file).
- Cross-check Member Set: 1 + 5 + 4 + 3 = 13 C# files, the same paths; plus the same PowerShell file.
- Cross-check Count: 14.
- Member-set Comparison: identical. The assertion may be proposed.

Measurement caveat for 12.1 and 12.2: every `[V-grep]` figure above was produced by a
working-tree content search (ripgrep semantics, BOM-aware, binary files skipped), while the gates
in section 8 use tracked-only `git grep`. The #602 research recorded one-file deltas between the two
tools on the profile-path union at the same commit, attributed to binary-detection differences.
The executor's `git` figures govern; a delta of that size should be reported alongside the
classifier list rather than reconciled by hand.

### 12.3 Identifier baselines (not derivable here)

The account (1,222 / 1,215 outside `.claude`), host (187 / 186) and generic profile-path (1,228 /
1,213) file counts are `[V-brief]` figures measured by `git grep`. This research could not run an
independent second enumeration for the account and host tokens without embedding them, so those
figures are carried as the orchestrator's baseline and must be re-measured by the executor in
Phase 0 with the gate commands in section 8 before being written into any acceptance criterion.

---

## 13. Testing implications

- Guard: Pester 5, in-memory only (section 8 cases). Mock `Invoke-GitExe` with a body
  `param([string[]]$GitArgs)`; never mock `git`. Coverage target for the three new production files:
  90 percent line (new-code target), measured by a direct Pester coverage capture over
  `scripts/hygiene` and recorded as a JaCoCo projection under this feature's `evidence/qa-gates/`.
- C# fixtures: no new tests; the 14 touched test methods are the regression set and must pass under
  the existing MSTest-with-coverage route. Assertions such as `NotContain("testuser")` keep their
  discriminating power because the token is retained.
- Evidence for this item must itself pass the guard: summaries and projections only, placeholders
  only, and the residual scan must cover this folder and the plan file.
- Determinism: the guard reads no environment variable and no wall clock; the tests inject content
  and git output, so results are identical in Terminal and Test Explorer.

## 14. Open items that need a shell

1. Encoding survey of the write set (`git ls-files --eol`).
2. Context check for the bare host token before rule 5 (`git grep -i -o -h`).
3. Pre-change baselines for gates 1-6 and the guard's pre-sweep findings count.
4. Confirmation that no `.trx` or Cobertura document is tracked outside `docs/features`
   (`git ls-files -- '*.trx' ':!docs/features/'` expected empty; the brief states all 572 named
   ones are under `docs/features`).
