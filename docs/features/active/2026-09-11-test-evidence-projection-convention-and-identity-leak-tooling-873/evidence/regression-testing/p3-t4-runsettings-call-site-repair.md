# P3-T4 — Coverage-Family Call Sites Repaired in the Shared Test File

Timestamp: 2026-09-13T06-03
Task: [P3-T4]

`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` is repaired in the same phase as the
signature change that breaks it, so no phase gate runs a test path this plan itself broke.

## Splatted argument sets declared

Four distinct argument sets, declared in the file's `BeforeAll` block. One set does not serve all ten
call sites:

| Set | Call sites | Why distinct |
|---|---|---|
| `$script:builderArgument` | the five `Get-DotnetCoverageArgumentList` sites | all five share one argument set |
| `$script:collectionArgument` | the three lifecycle `Invoke-DotnetCoverageCollection` sites | supplies the eight-assembly fixture and the fixture runsettings path |
| `$script:nonzeroExitArgument` | the nonzero-exit-code error path | differs in the vstest path and the runsettings path |
| `$script:derivedEqualsCanonicalArgument` | the derived-equals-canonical error path | differs additionally in the output path |

The three derived sets are per-site clones of the first with only their differing keys overridden,
which is the second form the task permits. A clone is taken rather than the base being mutated,
because all four sets are live in the same file. No call site supplies any parameter both inside its
splatted hashtable and explicitly on the same invocation; supplying one parameter by both routes is a
parameter-binding error in PowerShell rather than an override.

The lifecycle block's `BeforeEach` now reads `$script:canonicalCoverageConfig`,
`$script:coverageOutput`, `$script:fakeVsTestPath`, `$script:fakeRunSettingsPath` and
`$script:fakeTestAssemblies` from `$script:collectionArgument`, so the assertions in that block and
the call sites they judge cannot drift apart.

## Mock body and fixtures

- The explicit mock body for `Invoke-DotnetCoverageCollection` now declares seven parameters, adding
  `ResultsDirectory` and `LogFileName`, so the mock signature continues to match production exactly.
- A reader mock filtered to the test-result path
  (`Mock Get-Content -ParameterFilter { $LiteralPath -eq $script:coverageTrxPath }`) was added to the
  coverage-main describe block's setup. It answers with an in-memory here-string whose root declares
  the default TeamTest namespace, so the entry point's test-result read reaches the summary writer
  rather than the non-fatal warning branch. No file is written.
- The mocked post-processor return value in that describe block now carries a root `lines-covered`
  attribute of 4 and a root `lines-valid` attribute of 5 over one package holding one class with five
  distinct line numbers, each carrying a hits attribute, four with a value greater than zero and the
  fifth with the value 0, and none carrying a branch or condition-coverage attribute. The package
  therefore returns covered 4 and valid 5, the projection emits a LINE counter with covered 4 and
  missed 1 and a BRANCH counter with covered 0 and missed 0, and both reconciliation equalities hold
  exactly. The value remains an in-memory string literal.
- Both mocks in that block that return a post-processor value were updated: the `BeforeEach` mock and
  the block-local override inside `passes the generated Cobertura result to the threshold evaluator
  before completing successfully`, whose pinned exact-string assertion carries the same document.
- The override inside `persists the post-processed Cobertura document before the threshold assertion
  can throw on a sub-threshold run` is unchanged, because the threshold assertion throws before the
  projection write on that path.
- No describe block was added to this file.

## Exact-count writer assertions

| Path | Test | Writes |
|---|---|---|
| Happy path | `collects and post-processes coverage on the fully mocked main happy path` | 3 — the post-processed Cobertura document, the projection and the summary |
| Sub-threshold path | `persists the post-processed Cobertura document before the threshold assertion can throw on a sub-threshold run` | 1 — the threshold assertion throws before the projection write, the summary write and the discard |

Both are `Should -Invoke Set-Content -Times N -Exactly`, so each is an exact count rather than a
lower bound.

## Pester run limited to this file

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 with Run.PassThru and Detailed output, printing the passed, failed and skipped counts and ending with an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

```
Describing Invoke-MSTestWithCoverage derived settings
 Context Derived coverage settings lifecycle
   [+] retains canonical module exclusions and adds the test assembly exclusion exactly once
   [+] uses the derived settings path and preserves all eight test assemblies after the vstest boundary
   [+] removes the derived settings after successful collection without writing the canonical file
   [+] removes the derived settings after failed collection without writing the canonical file

Describing Invoke-MSTestWithCoverageMain
   [+] uses only mocked discovery and builds the vswhere command for the main happy path
   [+] does not start coverage collection when NoExecute is supplied
   [+] collects and post-processes coverage on the fully mocked main happy path
   [+] passes the generated Cobertura result to the threshold evaluator before completing successfully
   [+] fails when the search root cannot be found
   [+] persists the post-processed Cobertura document before the threshold assertion can throw on a sub-threshold run
   [+] excludes assemblies discovered under a .claude worktree segment

Describing Invoke-MSTestWithCoverage isolated error paths
   [+] fails when coverage settings have no module exclusion node
   [+] fails when coverage settings repeat the test assembly exclusion
   [+] fails when the derived path equals the canonical coverage path
   [+] fails when dotnet coverage returns a nonzero exit code
PESTER_COUNTS passed=28 failed=0 skipped=0
```

PASSED: 28
FAILED: 0
SKIPPED: 0

The test named `passes the generated Cobertura result to the threshold evaluator before completing
successfully` is recorded as passed.

## Line count against the repository ceiling

Command: pwsh -NoProfile -Command '<count the lines of tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1>'
EXIT_CODE: 0

```
491
```

491 is at most 500. The file absorbed two extra arguments at ten call sites, a reader mock, two
fixture replacements and a two-parameter mock-body extension while shrinking from its Phase 0
post-format baseline, because splatting removed five continuation lines at every converted site. No
test was deleted: discovery reports the same 28 tests as before the change.

## Anchored diff and porcelain status

Command: git diff refs/base-anchor-873 --stat -- tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
EXIT_CODE: 0

```
 .../vscode/Invoke-MSTest.RunSettings.Tests.ps1     | 167 ++++++++++-----------
 1 file changed, 81 insertions(+), 86 deletions(-)
```

Command: git status --porcelain --untracked-files=all
EXIT_CODE: 0

```
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
?? docs/features/active/.../evidence/qa-gates/p3-t1-batch-open.md
?? docs/features/active/.../evidence/regression-testing/p3-t2-collection-forwarding.md
?? docs/features/active/.../evidence/regression-testing/p3-t3-entry-point-wiring.md
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1
```

The anchored diff and the porcelain status agree that the file is changed. The two mechanisms are
complementary: the anchored diff enumerates tracked changes and the porcelain status additionally
shows the untracked file this phase creates. The feature-folder paths are elided to the feature
folder only, to keep the artifact free of long repeated prefixes; the full paths are the canonical
evidence paths this plan names.

## Output Summary

EXIT_CODE: 0. Four distinct splatted argument sets replace ten inline call sites; the seven-parameter
mock body, the filtered test-result reader mock and the reconcilable post-processor fixture are in
place; the happy-path writer assertion is exactly 3 and the sub-threshold one stays exactly 1; 28
passed, 0 failed, 0 skipped; the file measures 491 lines against the 500 ceiling; the anchored diff
and the porcelain status both show the file as changed.
