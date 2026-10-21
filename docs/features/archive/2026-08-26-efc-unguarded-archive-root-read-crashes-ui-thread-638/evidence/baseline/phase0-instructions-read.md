# Phase 0 — Policy Instructions Read (Issue 638)

Timestamp: 2026-08-29T12-15

Policy Order:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`

Files read (explicit list):

- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`

## [P0-T1] `CLAUDE.md`

Read in full. Four embedded policies recorded: General Code Change Policy, General Unit
Test Policy, C# Code Change Policy, C# Unit Test Policy. The
`## C# Toolchain (run in this exact order)` section names four commands, quoted verbatim:

1. `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`; always via `dotnet tool run`, never a global install)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

If any step fails, fix and restart from step 1.

## [P0-T2] `.claude/rules/general-code-change.md`

Read in full. Recorded for [P0-T5]:

- **File Size Limit**: no production code, test code, or reusable script file may exceed
  **500 lines**. Exceptions are throwaway agent-session scripts, raw text fixtures for
  language-processing test data, and Markdown documentation files.
- **Mandatory Toolchain Loop**: seven stages in order — formatting, linting, type
  checking, architecture-boundary tests, unit tests, contract/schema compatibility checks,
  integration tests. Restart from step 1 if any stage fails or auto-fixes any file.

## [P0-T3] `.claude/rules/general-unit-test.md`

Read in full. Recorded:

- **Coverage Exclusion Policy**: no production file may be excluded from coverage
  measurement; every production source file is in the denominator. Permitted `exclude`
  entries cover build output, test files and test infrastructure, non-production config
  files and `node_modules/**` only. Any `exclude` entry matching a production source path
  is a Blocking finding for feature-review agents.
- **Test File Location clause**: test files must live in a `tests/` directory tree
  mirroring the production source structure; colocation in the production source tree is
  not permitted.
- **Note on D1**: scope decision D1 in the plan supersedes the `tests/` mirroring clause
  for C# in this repository. `.claude/skills/policy-compliance-order/SKILL.md` ranks
  `CLAUDE.md` above `.claude/rules/general-unit-test.md`; the unit-test policies embedded
  in `CLAUDE.md` impose no `tests/` mirroring requirement; and the General Code Change
  Policy requires matching existing repository style, which for every C# test project here
  is a sibling `<Project>.Test` project. The new test file therefore lives at
  `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`.

## [P0-T4] `.claude/rules/csharp.md`

Read in full. Recorded: the same four toolchain commands with `/t:Rebuild` required
locally; the prohibition on `/p:Nullable=enable`; MSTest + Moq + FluentAssertions as the
test stack; repository-wide line coverage `>= 80%` and `>= 90%` for any new module, class
or method; coverage regression on changed lines is a blocking finding; the DI-seam
preference order (interface seam, then injectable delegate seam, then adapter seam) which
authorizes the `Action<string>` delegate seam this plan introduces; the five-package
analyzer stack and its severity-first ordering invariant
(`.editorconfig` severities at `suggestion` before any `<Analyzer Include>` wiring); and
the prohibition on weakening assertions or relaxing test expectations to make tests pass.
