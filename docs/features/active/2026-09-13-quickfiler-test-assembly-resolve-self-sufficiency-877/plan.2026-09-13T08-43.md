# 2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency (Plan)

- **Issue:** #877
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-13T10-05
- **Status:** Ready for preflight
- **Version:** 1.0
- **Work Mode:** minor-audit (from `issue.md` metadata block, line 7)
- **Requirements source:** `docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877/issue.md` only. There is no `spec.md` and no `user-story.md`, and none may be created. The AC source is the explicit `## Acceptance Criteria` section of `issue.md`, which holds 8 items. That section begins at line 185 as the plan is written, but [P1-T11] inserts lines above it, so the line numbers go stale mid-run; every task that touches an acceptance criterion locates it by text, never by line number, per the AC locator rule before [P2-T18].
- **Worktree (binding):** `C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation`. Every path in this plan is relative to that worktree unless written absolute. Address git as `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" <subcommand>`. Do not write in any sibling worktree.
- **Task counts (mechanical, `^- \[ \] \[P\d+-T\d+\]`):** Phase 0 = 13, Phase 1 = 13, Phase 2 = 28. Total = 54.

---

## THE ACCEPTANCE RULE THAT MATTERS MOST — read this before executing anything

**M3 is the ONLY discriminator for this fix. A passing M2 run and a passing full suite prove NOTHING about it.**

One run shape, M2, was observed twice with an identical command and an identical total, and the two observations disagreed:

| Run shape | First observation | Second observation | Same run shape? | Stable? |
|---|---|---|---|---|
| M2 — `QuickFiler.Test.dll` with `/Settings:` | Total 1395: 1392 passed, 3 FAILED | Total 1395: 1395 passed, 0 failed | YES — both total 1395 | NO |
| M6 — zero-batch class plus one unrelated class, no runsettings | Total 9: 9 passed, 0 failed | Total 13: 10 passed, 3 FAILED | NO — 9 against 13 | not determinable |
| M3 — zero-batch class ALONE, no runsettings | Total 3: 3 failed | Total 3: 3 failed | YES — both total 3 | YES |

M2 alone carries the nondeterminism demonstration, and it is sufficient on its own. Both M2 observations selected the same 1395 tests, so the run shape was identical and only the outcome differed. M6 must NOT be described as a run that disagreed with itself: its two observations have different totals, 9 against 13, so they did not select the same set of classes and are not directly comparable. The committed artifact `evidence/other/m6-order-control.2026-09-13T09-16.md` records the Total 13 observation; the Total 9 observation comes from a prior session. M6 remains useful only as the single-run demonstration that the zero-batch class fails when it is scheduled first, which is what its committed artifact actually shows.

This is exactly what the mechanism in `issue.md` `## Verified Mechanism` predicts. Whether the `netstandard, Version=2.1.0.0` bind succeeds depends on which class happens to touch `SVGControl.SvgRenderer` first, and the run *shape* does not fix that. A multi-class run can therefore pass or fail for reasons unrelated to the change under test. M3 remains the sole discriminator, and M2's demonstrated nondeterminism is on its own sufficient to make M2 and the full suite non-probative.

Consequences that bind the executor:

1. M3 must FAIL before the change and PASS after. The fail-before half is already recorded and is cited, not re-run (Phase 0, [P0-T8]).
2. M2 and the `UtilitiesCS.Test` suite run are **REGRESSION checks only and are explicitly non-probative**. They can neither confirm nor refute the fix. The executor MUST state this explicitly, in those terms, in its own final report.
3. M3 must be run **at least three separate times** after the fix, each with a distinct `/ResultsDirectory:` and a distinct `LogFileName=`. The executor MUST report the repeat count and, for each run, the totals and the exit code.
4. If M3 does not pass after the fix, **STOP AND REPORT**. Do not escalate into any prohibited change. Do not retrofit an explanation onto a partial pass. Do not declare success because M2 passed.

---

## Approved design (locked — do not redesign, do not substitute an alternative)

The write set is exactly five paths. Nothing else may be modified.

1. **NEW** `TestSupport/TestAssemblyResolver.cs` at the repository root. The directory does not exist yet; `TestSupport/**` matches nothing in the tree today.
2. `QuickFiler.Test/QuickFiler.Test.csproj` — one additive `<Compile>` item.
3. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — one additive `<Compile>` item.
4. `QuickFiler.Test/SetupAssemblyInitializer.cs` — one added call plus its comment.
5. `UtilitiesCS.Test/TestAssemblyInitializer.cs` — rewired to the shared type; the moved members deleted.

Both test projects are legacy non-SDK projects with explicit `Compile Include` items, so nothing is auto-globbed and the link must be declared by hand in each.

#### PROHIBITED, absolute

- Do **NOT** modify `SVGControl` (any file). Three near-identical resolvers will exist afterwards and that is accepted; consolidating into production code is a separate decision with a different blast radius, tracked as issue #879.
- Do **NOT** modify `scripts/vscode/TaskMaster.cli.runsettings`.
- No `Workers` change. No removal or weakening of the `Parallelize` block. No `[DoNotParallelize]`. No dropping or bypassing `/Settings:`. No retries, no timing tolerance, no sleeps, no test reordering, no `[ClassInitialize]` front-running hack.
- Do not create `artifacts/csharp/coverage.xml`.
- Project-file edits stay MINIMAL and STRICTLY ADDITIVE: `QuickFiler.Test/QuickFiler.Test.csproj` sits in parked parallel item 871's declared blast radius, and `UtilitiesCS.Test` files sit in 872's.

#### RECOMMENDATION (record only — must NOT be substituted for the approved fix)

A declarative `<bindingRedirect>` for `netstandard` in both `app.config` files would probably also satisfy the bind. It is recorded here as a note for a future reader and is explicitly **not** in scope for this change. Do not implement it. Do not substitute it. The approved fix is the `[AssemblyInitialize]` resolver install described above.

#### Language-version constraint discovered during planning (load-bearing)

`QuickFiler.Test/QuickFiler.Test.csproj` declares **no** `<LangVersion>` element (verified: the property group at lines 10-31 has none, and a repository-wide search of `*.csproj` for `LangVersion` returns 13 hits, none of them in `QuickFiler.Test.csproj`). It targets `v4.8.1`, so it compiles at the **C# 7.3** default. This is documented in-repo at `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs` lines 20-24: "QuickFiler.Test declares no LangVersion element and targets v4.8.1, so it compiles at the 7.3 default ... Do not introduce target-typed `new`, `is not null`, switch expressions, or nullable reference annotations here: they surface as CS8370 at build time, not at edit time."

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` line 18 declares `<LangVersion>Latest</LangVersion>`.

The source block being lifted uses `_resolving ??= new HashSet<string>(...)` at `UtilitiesCS.Test/TestAssemblyInitializer.cs` line 61. **`??=` is C# 8 and is CS8370 under C# 7.3.** The shared file therefore MUST spell that as an explicit null test. The spelling below is required, not optional. Adding a `<LangVersion>` element to `QuickFiler.Test.csproj` is **not** permitted: the project-file edits are capped at one additive `<Compile>` item each.

#### Exact content of the new shared file

Author `TestSupport/TestAssemblyResolver.cs` with exactly this content. The line breaks inside the XML doc are load-bearing: two acceptance conditions assert single-line tokens that must not be split.

```
using System;
using System.Collections.Generic;
using System.Reflection;

namespace TaskMaster.TestSupport
{
    /// <summary>
    /// Installs a process-wide AppDomain.CurrentDomain.AssemblyResolve fallback that matches a
    /// requested assembly on simple name plus public key token. Linked into QuickFiler.Test and
    /// UtilitiesCS.Test from this single file so the two copies cannot drift.
    /// <para>
    /// Reason 1, binding redirects are not applied. vstest's testhost
    /// does not reliably honour binding redirects from the test assembly's .dll.config, depending
    /// on AppDomain mode, so a reference recorded at one version but deployed at another raises
    /// FileNotFoundException (for example ExCSS 4.2.3 vs 4.3.1, or
    /// System.Threading.Tasks.Extensions 4.2.0.1 vs 4.2.4.0). Production resolves these through
    /// TaskMaster.exe.config and is unaffected.
    /// </para>
    /// <para>
    /// Reason 2, an unsatisfiable netstandard bind. Both test projects redirect FSharp.Core to
    /// 11.0.0.0, and FSharp.Core 11.0.0.0 references
    /// netstandard, Version=2.1.0.0, PublicKeyToken=cc7b13ffcd2ddd51
    /// The requirement enters the closure through that FSharp.Core redirect and NOT through
    /// Deedle, which asks only for netstandard 2.0.0.0. Nothing on this machine satisfies it: the
    /// GAC holds only v4.0_2.0.0.0__cc7b13ffcd2ddd51 and no config file in the repository contains
    /// the string netstandard. The two versions share a public key token, so a handler matching on
    /// simple name plus token resolves the request while the default binder cannot.
    /// </para>
    /// <para>
    /// Do not delete this handler because Reason 1 looks obsolete. Reason 2 stands on its own:
    /// without the fallback, any test that touches Deedle fails at static-initializer time, and
    /// the CLR caches a failed type initializer for the lifetime of the process.
    /// </para>
    /// </summary>
    internal static class TestAssemblyResolver
    {
        /// <summary>
        /// Attaches the fallback to the current AppDomain. Call this from an assembly's
        /// AssemblyInitialize method so that it runs before any test in that assembly.
        /// Attaching twice in one process is harmless: the handler is a pure lookup with no
        /// state beyond a thread-local re-entrance guard.
        /// </summary>
        public static void Install()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveByNameAndKey;
        }

        [ThreadStatic]
        private static HashSet<string> _resolving;

        private static Assembly ResolveByNameAndKey(object sender, ResolveEventArgs args)
        {
            var requested = new AssemblyName(args.Name);
            byte[] requestedKey = requested.GetPublicKeyToken();

            foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
            {
                var loadedName = loaded.GetName();
                if (
                    !string.Equals(
                        loadedName.Name,
                        requested.Name,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }
                if (PublicKeyTokensEqual(loadedName.GetPublicKeyToken(), requestedKey))
                {
                    return loaded;
                }
            }

            // Fall back to a simple-name load from the probing path. Re-entrance guard
            // prevents infinite recursion when Assembly.Load itself fails and re-raises
            // AssemblyResolve on this thread. Written as an explicit null test rather than a
            // null-coalescing assignment because QuickFiler.Test declares no LangVersion
            // element and compiles at the C# 7.3 default, where that operator is CS8370.
            if (_resolving == null)
            {
                _resolving = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            if (!_resolving.Add(requested.Name))
            {
                return null;
            }
            try
            {
                var byName = Assembly.Load(new AssemblyName(requested.Name));
                if (
                    byName != null
                    && PublicKeyTokensEqual(byName.GetName().GetPublicKeyToken(), requestedKey)
                )
                {
                    return byName;
                }
            }
            catch
            {
                // Swallow - return null so default resolution can run.
            }
            finally
            {
                _resolving.Remove(requested.Name);
            }

            return null;
        }

        private static bool PublicKeyTokensEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null)
            {
                return a == b || (a != null && a.Length == 0) || (b != null && b.Length == 0);
            }
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
```

Do **not** add `#nullable enable` to this file. The source it is lifted from carries no nullable context, and the method returns `null` on three paths; enabling the context would raise CS86xx, which `/p:TreatWarningsAsErrors=true` promotes to errors. Do **not** use a `<see cref=...>` element anywhere in the doc comment.

This is a **relocation, not a new module**. The resolution semantics are byte-identical in effect to the current `UtilitiesCS.Test` implementation at `UtilitiesCS.Test/TestAssemblyInitializer.cs` lines 31-107: loaded-assembly match on simple name plus public key token first, then a re-entrance-guarded simple-name `Assembly.Load` fallback, then `null`. The only textual change is the `??=` to explicit-null-test rewrite forced by C# 7.3, which has identical semantics.

#### Exact project-file additions

`QuickFiler.Test/QuickFiler.Test.csproj` — insert immediately after line 229 (`<Compile Include="SetupAssemblyInitializer.cs" />`, the last item in that group) and immediately before line 230 (`</ItemGroup>`):

```
    <Compile Include="..\TestSupport\TestAssemblyResolver.cs">
      <Link>TestSupport\TestAssemblyResolver.cs</Link>
    </Compile>
```

`QuickFiler.Test` already has a physical `TestSupport\` folder holding `WinFormsPumpHost.cs` (line 216) and `WinFormsPumpHostTests.cs` (line 217). The filename differs, so the `Link` value above does not collide.

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` — insert immediately after line 79 (`<Compile Include="TestAssemblyInitializer.cs" />`), inside the `ItemGroup` that opens at line 72:

```
    <Compile Include="..\TestSupport\TestAssemblyResolver.cs">
      <Link>TestSupport\TestAssemblyResolver.cs</Link>
    </Compile>
```

`.csproj` files are excluded from CSharpier by `.csharpierignore` lines 12-14, so these elements will not be reformatted by the format step.

#### Exact edit to `QuickFiler.Test/SetupAssemblyInitializer.cs`

Replace the body of `AssemblyInit` (currently lines 16-20) with:

```
        {
            // Issue #877: install this assembly's own AssemblyResolve fallback before any test
            // class runs, so QuickFiler.Test no longer depends on an unrelated class touching
            // SVGControl.SvgRenderer first. See TestSupport/TestAssemblyResolver.cs.
            global::TaskMaster.TestSupport.TestAssemblyResolver.Install();

            // Set up windows forms for hd resolution
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        }
```

The two existing `System.Windows.Forms.Application` calls are neither removed nor reordered relative to each other. The `Install()` call is placed first so the resolver is attached at the earliest possible point. The four existing `using` directives at lines 1-4 are left untouched. The call is written `global::`-qualified so that no new `using` directive is needed and so that name resolution cannot be captured by any enclosing `QuickFiler` namespace.

#### Exact end state of `UtilitiesCS.Test/TestAssemblyInitializer.cs`

The whole file becomes:

```
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test
{
    /// <summary>
    /// Installs the shared process-wide AssemblyResolve fallback before any test in this assembly
    /// runs. The resolver logic, and the full explanation of why it is required, live in
    /// TestSupport/TestAssemblyResolver.cs, which is linked into this project and into
    /// QuickFiler.Test so the two copies cannot drift.
    /// </summary>
    [TestClass]
    public static class TestAssemblyInitializer
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            global::TaskMaster.TestSupport.TestAssemblyResolver.Install();
        }
    }
}
```

The `[TestClass]` attribute, the `static class` form, the type name, the `[AssemblyInitialize]` attribute, and the `Initialize(TestContext)` signature are all preserved exactly. The `using System;`, `using System.Collections.Generic;`, `using System.Reflection;` and `using System.Threading;` directives at lines 1-4 are removed because no type from those namespaces remains referenced in the file after the deletion. `_resolving`, `ResolveByNameAndKey` and `PublicKeyTokensEqual` (lines 31-107) are deleted from this file; they now live only in the shared file.

---

## Coverage position (recorded, not fabricated)

`CLAUDE.md` governs the coverage thresholds for this repository: C# line `>= 80%`, new modules/classes/methods `>= 90%`, and no reduction in coverage for changed lines. The `85%` / `75%` figures in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` are **not** authoritative here.

This change touches **zero production source files**. The write set is one new test-support source file, two edited test-assembly source files, and two test-project files. Every one of them lives in the test tree, and the coverage tooling is configured to exclude test files from the coverage denominator. A coverage delta run therefore cannot produce a meaningful figure attributable to this change, and is not the gate for it.

Phase 0 records this position in an artifact ([P0-T9]). No coverage command is planned, no coverage number is asserted anywhere in this plan, and `artifacts/csharp/coverage.xml` is not produced. The gate for this change is the M3 discriminator plus the three toolchain gates.

---

## Operating rules for every command in this plan

**Bash discipline.** Never use `cd`. No `&&`, `;` or `|` chaining. Single commands only, first token `git`, `pwsh`, `poetry` or `gh`. Never invoke `grep`, `sed`, `awk`, `cat`, `head`, `tail`, `find`, `cp`, `mv`, `rm` or `echo` through Bash; use the Grep, Read, Glob, Edit and Write tools with absolute paths. Run msbuild, dotnet, csharpier and vstest as one `pwsh -NoProfile -Command` invocation each, with the worktree path inside the command string.

**pwsh quoting.** Outer SINGLE quotes on the `-Command` payload, inner double quotes. An outer-double-quoted payload is eaten by the calling shell.

**Working directory (mandatory, verified).** A `pwsh -NoProfile -Command` invocation started by this executor begins in the COORDINATOR SESSION worktree `C:/Users/DanMoisan/repos/TaskMaster-wt/2026-09-12T10-15`, NOT in the 877 worktree. That session worktree contains its own `TaskMaster.sln`, so an unqualified `msbuild TaskMaster.sln` builds the wrong checkout and returns a green result that says nothing about this change, and an unqualified `dotnet tool run csharpier format .` reformats the coordinator's tree. Every `pwsh -NoProfile -Command` payload in this plan MUST therefore begin with the literal prefix

`Set-Location -LiteralPath 'C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation'; `

and every artifact MUST record the full payload, including that prefix, in its `Command:` row. A task whose artifact records a payload without the prefix has not satisfied its acceptance condition. The two build-lock payloads at the bullets below are the only exception: they reference the lock files by absolute path and read nothing from the worktree.

**Build lock.** EVERY `msbuild`, `dotnet build`, `dotnet tool restore`, `dotnet tool run csharpier` and `vstest.console.exe` invocation is wrapped in acquire/release. Acquire immediately before that single command; release immediately after it returns, including on failure.

- Acquire: `pwsh -NoProfile -Command '& ([scriptblock]::Create((Get-Content -Raw "C:/Users/DanMoisan/repos/TaskMaster-wt/parallel-build-lock/acquire.txt"))) 877'`
- Release: `pwsh -NoProfile -Command '& ([scriptblock]::Create((Get-Content -Raw "C:/Users/DanMoisan/repos/TaskMaster-wt/parallel-build-lock/release.txt"))) 877'`
- These two payloads are the only ones in this plan that are NOT prefixed with `Set-Location`: they name the lock files by absolute path and read nothing from any worktree.
- Never kill a waiter by matching processes on a command line. Cancel your own with `cancel-waiter.txt` and `-Item 877`.

**C# toolchain, in CLAUDE.md order, against `TaskMaster.sln` in the 877 worktree.**

1. `dotnet tool restore`, then `dotnet tool run csharpier format .`, verified with `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. vstest spans, below.

`/t:Rebuild` is mandatory. A warm `/t:Build` skips `CoreCompile` on every project and runs no analyzers, returning a vacuous exit 0.

**msbuild assertion rule.** Do NOT assert on a bare count of the string `error`. A SUCCESSFUL msbuild run prints the substring `error` roughly 35 times in this repository (package paths, target names, `ErrorText` properties). Assert on the exit code together with an **anchored** match for a line that, after leading whitespace is trimmed, is exactly `0 Error(s)`. A substring search for `0 Error(s)` also matches `10 Error(s)`, so the match must be anchored to the whole trimmed line.

**vstest resolution (mandatory, verified).** `vstest.console.exe` is NOT on PATH on this machine: `Get-Command vstest.console.exe` returns nothing, so every vstest span written below as a bare `vstest.console.exe ...` fails to resolve unless the executable is located first. Resolve it through vswhere exactly as `scripts/vscode/Invoke-MSTest.ps1` line 93 does: `-latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`, with the vswhere path `Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'` (line 169). Every vstest payload in Phase 2 therefore has this shape, and where a task below writes `vstest.console.exe <args>`, read it as `& $vstest <args>` inside this payload:

`Set-Location -LiteralPath 'C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation'; $vstest = & (Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe') -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; & $vstest <args>`

Because that payload contains `$`, it MUST be passed with outer SINGLE quotes on `-Command`; an outer-double-quoted payload has `$vstest` eaten by the calling shell. Record the resolved executable path in each vstest artifact's `Output Summary:`.

**Scratch directory for test results.** All `/ResultsDirectory:` values live under `C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation/TestResults/877/`. That tree is gitignored by `.gitignore` line 39 (`[Tt]est[Rr]esult*/`), so raw `.trx` files written there are never committed. This is the concrete scratch root for this plan; do not substitute another.

**Evidence hygiene (issue #671 decision).** Commit PROJECTIONS only, never a raw `.trx` and never a `.cobertura.xml`. Every vstest span sets an explicit `/ResultsDirectory:` and an explicit `LogFileName=` inside `/Logger:trx`. Redact absolute host paths to repository-relative form in every artifact. No helper scripts under `evidence/`.

**Evidence location (non-overridable).** Every evidence path resolves to `docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877/evidence/<kind>/`. `artifacts/baseline*`, `artifacts/qa*`, `artifacts/evidence`, `artifacts/coverage` and `artifacts/regression-testing` are FORBIDDEN.

**Artifact schema.** Every command-step artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:` as column-start rows. Use `ExpectedExitCode:` where a non-zero exit is the expected outcome. The expectation field is **per file**: one artifact carries exactly one expectation, so a step needing a non-zero expectation gets its own artifact file. `<ts>` in a filename below means the capture time in `yyyy-MM-ddTHH-mm`.

**Git scope.** `.claude/agent-memory/**` is a TRACKED path in this repository and the executor may write to it mid-run. Every `git diff`, `git status` and `git log` assertion in this plan is therefore pathspec-scoped. Never widen one of them to the whole tree.

---

### Phase 0 — Baseline capture

- [ ] [P0-T1] Read `C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation/CLAUDE.md` in full. Acceptance: the file has been read in this session and the reader can name the four-step C# toolchain order it specifies.
- [ ] [P0-T2] Read `C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation/.claude/rules/general-code-change.md` in full. Acceptance: the file has been read in this session and the reader can name the 500-line file-size limit it specifies.
- [ ] [P0-T3] Read `C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation/.claude/rules/general-unit-test.md` in full. Acceptance: the file has been read in this session and the reader can name its Coverage Exclusion Policy.
- [ ] [P0-T4] Read `C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation/.claude/rules/csharp.md` in full. Acceptance: the file has been read in this session.
- [ ] [P0-T5] Read `C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation/.claude/rules/tonality.md` in full. Acceptance: the file has been read in this session.
- [ ] [P0-T6] Read `docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877/issue.md` in full, including `## Verified Mechanism`, `## Refuted Explanations` and `## Out of Scope (non-negotiable)`. Acceptance: the file has been read in this session, and the reader can state that the `## Acceptance Criteria` section holds exactly 8 `- [ ]` items.
- [ ] [P0-T7] Write `evidence/baseline/phase0-instructions-read.<ts>.md` recording the policy reads. Acceptance: the file exists at `docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877/evidence/baseline/` with a name beginning `phase0-instructions-read.`, and it contains a column-start `Timestamp:` row, a column-start `Policy Order:` row, and an explicit list naming all six files read in [P0-T1] through [P0-T6] in that order.
- [ ] [P0-T8] Write `evidence/baseline/preexisting-evidence-index.<ts>.md` CITING the four already-committed artifacts without re-running any of them. Acceptance: the file exists, carries `Timestamp:`, `Command:` (value: `read-only verification, no command executed`), `EXIT_CODE: 0` and `Output Summary:`, and names all four of these repository-relative paths, recording for each that it was read and found schema-valid: `evidence/regression-testing/m3-fail-before.2026-09-13T09-14.md` (`EXIT_CODE: 1`, `ExpectedExitCode: 1`, Total 3 / Failed 3), `evidence/baseline/m2-suite-before.2026-09-13T09-15.md`, `evidence/other/m6-order-control.2026-09-13T09-16.md`, `evidence/baseline/mechanism-static-facts.2026-09-13T09-17.md`. The artifact must state in prose that the M3 fail-before half of the fail-before/pass-after pair is satisfied by citation and is NOT re-run.
- [ ] [P0-T9] Write `evidence/baseline/coverage-applicability.<ts>.md` recording the coverage position. Acceptance: the file exists, carries `Timestamp:`, `Command:` (value: `no command executed, static scope determination`), `EXIT_CODE: 0` and `Output Summary:`, names all five write-set paths, states that zero of them are production source files, states that `CLAUDE.md` governs and sets C# line coverage at `>= 80%` and new modules, classes and methods at `>= 90%` with no reduction on changed lines, states that `CLAUDE.md` sets NO branch-coverage threshold and that the 85% line and 75% branch figures in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` are not authoritative for this item, and states that no coverage command is run and no `artifacts/csharp/coverage.xml` is produced. The three governing threshold figures named in the preceding sentence are the only numbers permitted in this artifact; it must report NO measured or estimated coverage percentage for this change, because none is collected.
- [ ] [P0-T10] Run `dotnet tool restore` and then `dotnet tool run csharpier check .` against the 877 worktree as two separate lock-acquire / command / lock-release cycles, and write ONE artifact per command, because `ExpectedExitCode:` is a per-FILE field and these two commands can produce different exit codes. Write `evidence/baseline/dotnet-tool-restore-before.<ts>.md` for the restore and `evidence/baseline/csharpier-check-before.<ts>.md` for the check, each with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Acceptance: both artifacts exist; the restore artifact records exit 0; the check artifact's `Output Summary:` records the observed exit code and, if that exit code is non-zero, enumerates every repository-relative path the tool reported on a line containing the token `Was not formatted`. That enumeration is the baseline CLASS (b) LIST consumed by [P2-T4], [P2-T14], [P2-T15], [P2-T16] and [P2-T28]; if the check exit code is 0 the class-(b) list is EMPTY and the check artifact must say `Class (b) list: EMPTY` in those words. A non-zero check exit does not block Phase 0; it is recorded, and if non-zero, set `ExpectedExitCode:` in the check artifact ONLY, to that same observed value, so that artifact normalizes to pass.
- [ ] [P0-T11] Acquire the build lock, run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` against the 877 worktree in one `pwsh -NoProfile -Command` invocation, release the lock, and write `evidence/baseline/msbuild-analyzers-before.<ts>.md`. Acceptance: the artifact exists with all four schema rows; the `Output Summary:` records the observed exit code, the count of warnings reported on the msbuild summary line, and whether a line whose trimmed text is exactly `0 Error(s)` was present. If the exit code is non-zero, record the first ten diagnostic lines verbatim and set `ExpectedExitCode:` to the observed value; a red analyzer baseline is recorded, not repaired, and it caps what [P1-T8] and [P2-T5] can demand. Also record the output of `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" rev-parse HEAD` as an informational `Head:` row. Do not gate any later task on that SHA value.
- [ ] [P0-T12] Acquire the build lock, run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` against the 877 worktree in one `pwsh -NoProfile -Command` invocation, release the lock, and write `evidence/baseline/msbuild-nullable-before.<ts>.md`. Acceptance: the artifact exists with all four schema rows; the `Output Summary:` records the observed exit code and whether a line whose trimmed text is exactly `0 Error(s)` was present. If the exit code is non-zero, record the first ten diagnostic lines verbatim and set `ExpectedExitCode:` to the observed value; a red baseline here is recorded, not repaired, and it caps what [P2-T6] can demand.
- [ ] [P0-T13] Commit and push the Phase 0 evidence. Run `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" add -A -- docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877`, then a single `git commit`, then `git push origin HEAD`. Acceptance: `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" status --porcelain --untracked-files=all -- docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877` prints zero lines, and `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" ls-files -- docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877/evidence/baseline` lists at least 9 files: the seven Phase 0 baseline artifacts created by [P0-T7] through [P0-T12], whose filenames begin `phase0-instructions-read.`, `preexisting-evidence-index.`, `coverage-applicability.`, `dotnet-tool-restore-before.`, `csharpier-check-before.`, `msbuild-analyzers-before.` and `msbuild-nullable-before.`, plus the two pre-existing baseline artifacts `m2-suite-before.2026-09-13T09-15.md` and `mechanism-static-facts.2026-09-13T09-17.md`; and the push exits 0.

Phase 0 mechanical count of `^- \[ \] \[P0-T\d+\]` matches: 13, numbered `[P0-T1]` through `[P0-T13]`.

---

### Phase 1 — Implementation

Phase 1 makes the five-file change and records the two nondeterminism corrections. No test run happens in Phase 1 beyond the compile gate; all test spans are in Phase 2.

- [ ] [P1-T1] Create `C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation/TestSupport/TestAssemblyResolver.cs` with exactly the content given in the "Exact content of the new shared file" section above. Acceptance: the file exists; it contains a line whose trimmed text is exactly `internal static class TestAssemblyResolver`; it contains a line whose trimmed text is exactly `namespace TaskMaster.TestSupport`; it contains zero occurrences of the token `??=`; it contains zero occurrences of the token `#nullable`; and its total line count is under 500.
- [ ] [P1-T2] Verify the two AC5 explanation tokens are present, each on a single line, in `TestSupport/TestAssemblyResolver.cs`. Acceptance: exactly one line of that file contains the token `does not reliably honour binding redirects`, and exactly one line of that file contains the token `netstandard, Version=2.1.0.0, PublicKeyToken=cc7b13ffcd2ddd51`. Both tokens are quoted verbatim here and are created by [P1-T1]. If either search returns zero matches, the comment was rewrapped and [P1-T1] must be corrected before proceeding.
- [ ] [P1-T3] Verify the new file contains none of the prohibited-change vocabulary, so that the Phase 2 zero-hit diff gates are not self-defeated. Acceptance: `TestSupport/TestAssemblyResolver.cs` contains zero occurrences of each of these four tokens: `DoNotParallelize`, `Workers`, `Thread.Sleep`, `Task.Delay`.
- [ ] [P1-T4] Add the additive `<Compile>` item to `QuickFiler.Test/QuickFiler.Test.csproj`, inserted immediately after the `SetupAssemblyInitializer.cs` item and before the `ItemGroup` close, using the exact three-line element given above. Acceptance: exactly 2 lines of `QuickFiler.Test/QuickFiler.Test.csproj` contain the token `TestAssemblyResolver.cs`; exactly 1 line of it contains the token `..\TestSupport\TestAssemblyResolver.cs`; and exactly 1 line of it still contains the token `TestSupport\WinFormsPumpHost.cs`.
- [ ] [P1-T5] Add the additive `<Compile>` item to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, inserted immediately after the `TestAssemblyInitializer.cs` item inside the `ItemGroup` that opens at line 72, using the exact three-line element given above. Acceptance: exactly 2 lines of `UtilitiesCS.Test/UtilitiesCS.Test.csproj` contain the token `TestAssemblyResolver.cs`; exactly 1 line of it contains the token `..\TestSupport\TestAssemblyResolver.cs`; and exactly 1 line of it still contains the token `<Compile Include="TestAssemblyInitializer.cs" />`.
- [ ] [P1-T6] Edit `QuickFiler.Test/SetupAssemblyInitializer.cs` to add the resolver install, per the "Exact edit" section above. Acceptance: exactly 1 line of that file contains the token `global::TaskMaster.TestSupport.TestAssemblyResolver.Install();`; exactly 1 line still contains the token `System.Windows.Forms.Application.EnableVisualStyles();`; exactly 1 line still contains the token `System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);`; and in the file's line order the `EnableVisualStyles` line precedes the `SetCompatibleTextRenderingDefault` line.
- [ ] [P1-T7] Rewrite `UtilitiesCS.Test/TestAssemblyInitializer.cs` to the exact end state given above, deleting the three moved members. Acceptance: exactly 1 line of that file contains the token `global::TaskMaster.TestSupport.TestAssemblyResolver.Install();`; the file contains zero occurrences of the token `ResolveByNameAndKey`; zero occurrences of the token `PublicKeyTokensEqual`; zero occurrences of the token `_resolving`; exactly 1 line contains the token `[AssemblyInitialize]`; exactly 1 line contains the token `[TestClass]`; and exactly 1 line contains the token `public static void Initialize(TestContext context)`.
- [ ] [P1-T8] Acquire the build lock, run the analyzer build `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` against the 877 worktree in one `pwsh -NoProfile -Command` invocation, release the lock, and write `evidence/qa-gates/msbuild-analyzers-phase1.<ts>.md`. Acceptance: the artifact exists with all four schema rows; the observed exit code equals the exit code recorded in `evidence/baseline/msbuild-analyzers-before.<ts>.md` from [P0-T11] or is lower; if that baseline was 0 the exit code here must be 0 and a line whose trimmed text is exactly `0 Error(s)` must be present; if that baseline was non-zero this build must report no diagnostic naming any of the five write-set paths and the artifact must record the baseline-relative comparison explicitly; and in either case the `Output Summary:` explicitly confirms that `QuickFiler.Test.csproj` compiled, which is the evidence that the shared file's C# 7.3 spelling is accepted at the project's default language version. If the build reports CS8370, the `??=` rewrite in [P1-T1] was not applied; fix [P1-T1] and re-run this task rather than editing any `.csproj` language property.
- [ ] [P1-T9] Append the second, disagreeing M2 observation to `evidence/baseline/m2-suite-before.2026-09-13T09-15.md` WITHOUT introducing any additional column-start `Timestamp:`, `Command:` or `EXIT_CODE:` row and WITHOUT introducing an `ExpectedExitCode:` row, which this artifact does not currently carry and must not acquire: its recorded `EXIT_CODE: 0` would then normalize to fail. Acceptance: the file still contains exactly 1 line beginning at column 1 with the token `EXIT_CODE:`, exactly 1 line beginning at column 1 with the token `Timestamp:`, and exactly 1 line beginning at column 1 with the token `Command:`; the file contains zero column-start `ExpectedExitCode:` rows; a new section heading `## Prior-session observation, disagreeing` is present; that section records, as bulleted prose and not as schema rows, that a prior session ran the same command shape, with the same total of 1395 tests, and observed 1392 passed with 3 FAILED and a non-zero exit, and that this prior-session run is the one the M-matrix above labels the FIRST observation while the headline row of this artifact records the later one, and states that the two observations disagree; and the section states in terms that M2 is therefore NONDETERMINISTIC across runs and is a regression check only, not a discriminator.
- [ ] [P1-T10] Append the prior-session M6 observation to `evidence/other/m6-order-control.2026-09-13T09-16.md` under the same no-new-schema-rows constraint, recording it as a DIFFERENT run shape rather than as a disagreement. Acceptance: the file still contains exactly 1 column-start `EXIT_CODE:` row, exactly 1 column-start `ExpectedExitCode:` row, exactly 1 column-start `Timestamp:` row and exactly 1 column-start `Command:` row; a new section heading `## Prior-session observation, not the same run shape` is present; that section records, as bulleted prose and not as schema rows, that a prior session observed Total 9, 9 passed, 0 failed with a zero exit, against the Total 13, 10 passed, 3 FAILED already recorded in the headline row; it states that the differing totals, 9 against 13, mean the two runs did not select the same set of tests and are therefore NOT directly comparable; and it contains exactly 1 line containing the token `M6 is not a same-command flip`. The section must NOT claim that M6 disagreed with itself and must contain zero occurrences of the token `NONDETERMINISTIC across runs`.
- [ ] [P1-T11] Update the `### M-matrix` table and the paragraph immediately following it in `docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877/issue.md` (table at lines 99-103, prose at lines 105-109) to record the M2 nondeterminism and, separately and differently, the M6 non-comparability. Acceptance: within the `### M-matrix` section, exactly 1 line contains the token `NONDETERMINISTIC across runs` and that line is the M2 row; exactly 1 line contains the token `prior-session run of a different shape, Total 9 against Total 13` and that line is the M6 row; the section contains zero further occurrences of either token; the prose following the table contains exactly 1 line containing the token `M3 is the stable discriminator`; the prose states that a passing M2 or full-suite run is non-probative for this fix and that the M2 disagreement alone is sufficient to establish that, and makes no claim that M6 disagreed with itself; and no `- [ ]` item in the `## Acceptance Criteria` section is edited by this task. This task adds lines to `issue.md` ABOVE the `## Acceptance Criteria` section, so that section's line numbers WILL shift; [P2-T18] through [P2-T25] therefore locate each criterion by its text and never by a line number.
- [ ] [P1-T12] Verify the write set is closed and record the Phase 1 source gates. Acceptance: `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" status --porcelain --untracked-files=all -- SVGControl SVGControl.Test scripts` prints zero lines, and `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" status --porcelain --untracked-files=all -- QuickFiler.Test UtilitiesCS.Test TestSupport` prints exactly 5 lines, naming exactly `QuickFiler.Test/QuickFiler.Test.csproj`, `QuickFiler.Test/SetupAssemblyInitializer.cs`, `TestSupport/TestAssemblyResolver.cs`, `UtilitiesCS.Test/TestAssemblyInitializer.cs` and `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. `--untracked-files=all` lists every untracked file individually and does NOT collapse an untracked directory to a bare directory entry, so no `git add -N` preparation is needed or permitted here; if a bare `TestSupport/` entry appears, the `--untracked-files=all` flag was dropped from the command and must be restored rather than worked around. Record both spans, their outputs, and the token-gate results from [P1-T1] through [P1-T7] in `evidence/qa-gates/phase1-source-gates.<ts>.md` with all four schema rows. That artifact must additionally carry a `## Sharing mechanism justification` section, which is the record AC3 requires. It states: that the resolver is shared from the single file `TestSupport/TestAssemblyResolver.cs` at the repository root, linked into both test projects with `<Compile Include="..\TestSupport\TestAssemblyResolver.cs">` plus a `<Link>` element; that a link was chosen over duplication so the two copies cannot drift; that a hand-written `<Compile>` item is required in each project because both are legacy non-SDK projects with explicit items and no globbing; that the type is `internal` and neither test project grants the other `InternalsVisibleTo`, so the two compiled copies cannot produce a cross-assembly ambiguity at the `global::`-qualified call sites; and that `SVGControl` is not modified and the scope is confined to the two test projects.
- [ ] [P1-T13] Commit and push the Phase 1 change. Stage with `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" add -A -- QuickFiler.Test UtilitiesCS.Test TestSupport docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877`, then a single `git commit`, then `git push origin HEAD`. Acceptance: `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" status --porcelain --untracked-files=all -- QuickFiler.Test UtilitiesCS.Test TestSupport docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877` prints zero lines; `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" ls-files -- TestSupport/TestAssemblyResolver.cs` prints exactly one line; and the push exits 0.

Phase 1 mechanical count of `^- \[ \] \[P1-T\d+\]` matches: 13.

---

### Phase 2 — Final QC loop

Phase 2 runs the full CLAUDE.md-order toolchain, then the discriminating M3 runs, then the two non-probative regression runs, then AC check-off, then the terminal commit and push.

**Restart rule.** If any of [P2-T1] through [P2-T6] fails, or if the format step changes any file, fix the cause and restart the loop from [P2-T1]. Do not proceed past [P2-T6] while any of [P2-T1] through [P2-T6] has failed its own acceptance condition. A non-zero exit at [P2-T5] or [P2-T6] that equals the corresponding [P0-T11] or [P0-T12] baseline and names no write-set path SATISFIES that task's acceptance and is not a red step for this rule: a pre-existing red baseline cannot be cleared by restarting the loop, and this plan forbids repairing it.

- [ ] [P2-T1] Acquire the build lock, run `dotnet tool restore` against the 877 worktree in one `pwsh -NoProfile -Command` invocation, release the lock, and write `evidence/qa-gates/dotnet-tool-restore.<ts>.md`. Acceptance: the artifact exists with all four schema rows and the exit code is 0.
- [ ] [P2-T2] Acquire the build lock, run `dotnet tool run csharpier format .` against the 877 worktree in one `pwsh -NoProfile -Command` invocation, release the lock, and write `evidence/qa-gates/csharpier-format.<ts>.md`. This is a write-mode command that exits 0 whether or not it rewrote files, so its exit code alone is not the observation. Acceptance: the artifact exists with all four schema rows; the exit code is 0; the `Output Summary:` quotes verbatim the tool's own summary line, which on this pinned version 1.2.6 has the shape `Formatted N files in Xms` where N is a processed count and not a rewritten count (do not treat N as a defect signal and do not loop on it); and the `Output Summary:` additionally records the full output of `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" status --porcelain --untracked-files=all -- QuickFiler.Test UtilitiesCS.Test TestSupport` taken immediately after the command returns.
- [ ] [P2-T3] Acquire the build lock, run `dotnet tool run csharpier check .` against the 877 worktree in one `pwsh -NoProfile -Command` invocation, release the lock, and write `evidence/qa-gates/csharpier-check.<ts>.md`. Acceptance: the artifact exists with all four schema rows; the exit code is 0; and zero lines of the captured output contain the token `Was not formatted`.
- [ ] [P2-T4] Verify the repo-wide format pass did not reach outside the write set. Acceptance: `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" status --porcelain --untracked-files=all -- ':!docs' ':!.claude'` prints only paths that are either (a) one of the five write-set paths, or (b) a path that the `evidence/baseline/csharpier-check-before.<ts>.md` artifact from [P0-T10] already enumerated as unformatted at baseline. Record the resulting path list and its classification in `evidence/qa-gates/format-scope-check.<ts>.md` with all four schema rows. Any path in neither class fails this task and must be restored with `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" checkout -- <that path>` before restarting the loop at [P2-T1]. Class-(b) paths are NOT restored: restoring them would reintroduce the drift that [P2-T3] has just required to be absent, and [P2-T3] would fail on the next iteration. They are carried forward instead. The artifact must enumerate them under a heading `## Class (b) pre-existing format drift repaired by this run`, or state `Class (b) list: EMPTY` when [P0-T10] recorded a clean baseline. [P2-T15] permits exactly those enumerated paths in addition to the five write-set paths, [P2-T16] excludes them from its token search, and [P2-T28] stages them in a separate commit that names them as pre-existing CSharpier drift repaired incidentally by the mandatory repo-wide `format .`. [P2-T14] is the one exception and permits none of them: a class-(b) path lying under `SVGControl` or `SVGControl.Test` is a STOP AND REPORT at [P2-T14], because `issue.md` `## Out of Scope (non-negotiable)` forbids modifying `SVGControl` at all and carrying such a path forward would commit that modification.
- [ ] [P2-T5] Acquire the build lock, run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` against the 877 worktree in one `pwsh -NoProfile -Command` invocation, release the lock, and write `evidence/qa-gates/msbuild-analyzers-final.<ts>.md`. Acceptance: the artifact exists with all four schema rows, and the observed exit code equals the exit code recorded in `evidence/baseline/msbuild-analyzers-before.<ts>.md` from [P0-T11] or is lower. If that baseline was 0, the final must be 0 and a line whose trimmed text is exactly `0 Error(s)` must be present in the captured output. If that baseline was non-zero, the final must report no diagnostic naming any of the five write-set paths, and the artifact must record the baseline-relative comparison explicitly. Do not assert on any count of the bare substring `error`.
- [ ] [P2-T6] Acquire the build lock, run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` against the 877 worktree in one `pwsh -NoProfile -Command` invocation, release the lock, and write `evidence/qa-gates/msbuild-nullable-final.<ts>.md`. Acceptance: the artifact exists with all four schema rows, and the observed exit code equals the exit code recorded in `evidence/baseline/msbuild-nullable-before.<ts>.md` from [P0-T12] or is lower. If the baseline was 0, the final must be 0 and a line whose trimmed text is exactly `0 Error(s)` must be present. If the baseline was non-zero, the final must report no diagnostic naming any of the five write-set paths, and the artifact must record the baseline-relative comparison explicitly. Additionally record in `Output Summary:` whether `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` and `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` both exist after this build. If either is absent, the rebuild did not produce the assemblies the Phase 2 vstest spans load; the executor STOPS AND REPORTS at this task and does not run [P2-T7].
- [ ] [P2-T7] M3 RUN 1 of 3 — the primary and only discriminator. Acquire the build lock, run in one `pwsh -NoProfile -Command` invocation against the 877 worktree: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~QfcInitEmailQueueZeroBatchTests" /InIsolation /ResultsDirectory:C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation/TestResults/877/m3-post-1 /Logger:"trx;LogFileName=m3-post-1.trx"`, release the lock, then write `evidence/regression-testing/m3-pass-after-run1.<ts>.md`. No `/Settings:` is passed and none may be added. Acceptance: the artifact exists with all four schema rows; `EXIT_CODE: 0`; the captured console output contains the token `Test Run Successful.`; and the `Output Summary:` records, read from `m3-post-1.trx`, total 3, passed 3, failed 0, together with the three test names `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`, `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` and `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`, each with outcome Passed. The raw `.trx` stays in the gitignored scratch tree and is not committed.
- [ ] [P2-T8] M3 RUN 2 of 3. Same command as [P2-T7] with `/ResultsDirectory:C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation/TestResults/877/m3-post-2` and `/Logger:"trx;LogFileName=m3-post-2.trx"`, under acquire/release, then write `evidence/regression-testing/m3-pass-after-run2.<ts>.md`. Acceptance: identical to [P2-T7], read from `m3-post-2.trx`.
- [ ] [P2-T9] M3 RUN 3 of 3. Same command as [P2-T7] with `/ResultsDirectory:C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation/TestResults/877/m3-post-3` and `/Logger:"trx;LogFileName=m3-post-3.trx"`, under acquire/release, then write `evidence/regression-testing/m3-pass-after-run3.<ts>.md`. Acceptance: identical to [P2-T7], read from `m3-post-3.trx`.
- [ ] [P2-T10] Write the M3 repetition summary `evidence/regression-testing/m3-pass-after-summary.<ts>.md`. Acceptance: the artifact exists with all four schema rows; it states the repeat count as `3`; it tabulates, for each of the three runs, the `/ResultsDirectory:` leaf name, the total, the passed count, the failed count and the exit code; it cites `evidence/regression-testing/m3-fail-before.2026-09-13T09-14.md` as the fail-before half of the pair; and it contains exactly 1 line containing the token `M3 is the stable discriminator`. If any of the three runs did not pass, this task instead records STOP AND REPORT, the executor halts immediately, and NO task from [P2-T11] through [P2-T28] is attempted: no acceptance criterion is checked off, no further evidence artifact is written, and no commit or push is made beyond the Phase 1 commit already landed at [P1-T13]. No later task in this plan re-enables progress past this branch; any sentence elsewhere that appears to describe behaviour under STOP AND REPORT is describing an unreachable state, not authorising continuation. The executor's final report states the halt, the per-run totals, and the per-run exit codes.
- [ ] [P2-T11] M2 REGRESSION — NON-PROBATIVE. Acquire the build lock, run in one `pwsh -NoProfile -Command` invocation against the 877 worktree: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /ResultsDirectory:C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation/TestResults/877/m2-post /Logger:"trx;LogFileName=m2-post.trx"`, release the lock, then write `evidence/regression-testing/m2-suite-after.<ts>.md`. Acceptance: the artifact exists with all four schema rows; `EXIT_CODE: 0`; the `Output Summary:` records the total, passed and failed counts read from `m2-post.trx`, and records that the runsettings in force were `Workers=0` and `Scope=ClassLevel`; and the artifact contains exactly 1 line containing the token `regression check only and non-probative`. This run is order-dependent and proves nothing about the fix either way. Handling when it is not green: this plan records at the M-matrix above that this exact command shape was observed once at 1392 passed / 3 FAILED and once at 1395 passed / 0 failed, both totalling 1395, so a red result here is a known-possible outcome and is NOT by itself evidence of a regression from this change. If the exit code is non-zero, re-run the identical command up to two further times, each with a distinct `/ResultsDirectory:` leaf and a distinct `LogFileName=`, and record every attempt's totals and exit code in the artifact. If any attempt exits 0, record `EXIT_CODE:` as that attempt's value, declare no `ExpectedExitCode:`, and enumerate the red attempts as prose. If all three attempts are red, record the observed non-zero value with no `ExpectedExitCode:`, compare the failing test names against the three `QfcInitEmailQueueZeroBatchTests` names and against `evidence/baseline/m2-suite-before.2026-09-13T09-15.md`, and STOP AND REPORT. The executor halts immediately at this task and NO task from [P2-T12] through [P2-T28] is attempted: AC6 is never reached at [P2-T23], no acceptance criterion is checked off, no further evidence artifact is written, and no commit or push is made beyond the Phase 1 commit already landed at [P1-T13]. No later task in this plan re-enables progress past this branch. The executor's final report states the halt, the per-attempt totals and the per-attempt exit codes. Do not modify any test, any runsettings, or any write-set file to make this run green.
- [ ] [P2-T12] UtilitiesCS.Test REGRESSION — proves the moved logic still behaves identically in its original home. Acquire the build lock, run in one `pwsh -NoProfile -Command` invocation against the 877 worktree: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /ResultsDirectory:C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation/TestResults/877/utilities-post /Logger:"trx;LogFileName=utilities-post.trx"`, release the lock, then write `evidence/regression-testing/utilitiescs-suite-after.<ts>.md`. Acceptance: the artifact exists with all four schema rows, and the `Output Summary:` records the total, passed and failed counts read from `utilities-post.trx`. The recorded `EXIT_CODE:` must be `0`, EXCEPT on the pre-existing-failure branch stated below, where a non-zero `EXIT_CODE:` accompanied by an `ExpectedExitCode:` row carrying that same value satisfies this task instead. KNOWN ENVIRONMENTAL CAVEAT on this machine: the shell-icon test classes `ShellUtilities_Tests` and `ShellUtilitiesStatic_Tests` (in `UtilitiesCS.Test/HelperClasses/ShellUtilities_Tests.cs` and `UtilitiesCS.Test/HelperClasses/ShellUtilitiesStatic_Tests.cs`) stall vstest via `SHGetFileInfo`, and this reproduces on `main`. If the run hangs, re-run once with the filter changed to `/TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities"`, record the substituted filter verbatim in the artifact's `Command:` row, and record the exclusion and its pre-existing nature in `Output Summary:`. KNOWN FLAKINESS CAVEAT: `UtilitiesCS.Test` carries intermittent failures tracked in-repo as issue #811, consolidating #780 (`TryAddValuesAsync_UpdatesExistingValue`), #803 and #594 (`DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`) and two `Console.Out` races, all of which reproduce under parallel class workers on `main` and are out of scope here. If the exit code is non-zero, re-run the identical command up to two further times, each with a distinct `/ResultsDirectory:` leaf and a distinct `LogFileName=`, and record every attempt's totals and exit code in the artifact. If any attempt exits 0, record `EXIT_CODE:` as that attempt's value, declare no `ExpectedExitCode:`, and enumerate the red attempts as prose. If all three attempts are red, compare the failing test names against the #811 list named above: if every failure is on that list, record the observed non-zero value with `ExpectedExitCode:` set to that same value and record the pre-existing attribution in `Output Summary:`; if any failure is not on that list, STOP AND REPORT — the executor halts immediately at this task and no task from [P2-T13] through [P2-T28] is attempted. Do not treat the stall as a regression from this change and do not modify any `UtilitiesCS.Test` test file to work around it.
- [ ] [P2-T13] Verify `scripts/vscode/TaskMaster.cli.runsettings` is unmodified. The base ref for every diff span in [P2-T13] through [P2-T16] is the literal `main...HEAD`, whose three-dot form diffs the merge base of `main` and `HEAD` against `HEAD`. If `main` is not resolvable as a local ref in this worktree, substitute `origin/main...HEAD` in all four tasks and record the substitution in each artifact. Acceptance: `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" diff --name-only main...HEAD -- scripts/vscode/TaskMaster.cli.runsettings` prints zero lines, and its porcelain companion `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" status --porcelain --untracked-files=all -- scripts/vscode/TaskMaster.cli.runsettings` also prints zero lines. Record both spans and both results in `evidence/qa-gates/runsettings-unmodified.<ts>.md` with all four schema rows.
- [ ] [P2-T14] Verify `SVGControl` is unmodified. Acceptance: `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" diff --name-only main...HEAD -- SVGControl SVGControl.Test` prints zero lines, and its porcelain companion `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" status --porcelain --untracked-files=all -- SVGControl SVGControl.Test` prints zero lines. If either span is non-empty solely because [P2-T2]'s mandatory repo-wide `format .` repaired a path under `SVGControl` or `SVGControl.Test` that [P0-T10] had already enumerated as unformatted at baseline, this task FAILS and the executor STOPS AND REPORTS at this task: no task from [P2-T15] through [P2-T28] is attempted, no acceptance criterion is checked off, and no commit or push is made beyond the Phase 1 commit already landed at [P1-T13]. Do not restore the path, because [P2-T3] would then fail on the next loop iteration, and do not carry it forward, because `issue.md` forbids modifying `SVGControl`. The conflict between the mandatory repo-wide format command and that prohibition is not resolvable inside this plan and is reported rather than worked around. Record both spans and both results in `evidence/qa-gates/svgcontrol-unmodified.<ts>.md` with all four schema rows.
- [ ] [P2-T15] Verify the code write set is exactly five paths. The Phase 1 commit at [P1-T13] already placed all five in `HEAD`, so the anchored diff can see the newly created file; the porcelain companion covers any delta that [P2-T2] left uncommitted. Acceptance: `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" diff --name-only main...HEAD -- QuickFiler.Test UtilitiesCS.Test TestSupport` prints exactly 5 lines, and that list is exactly `QuickFiler.Test/QuickFiler.Test.csproj`, `QuickFiler.Test/SetupAssemblyInitializer.cs`, `TestSupport/TestAssemblyResolver.cs`, `UtilitiesCS.Test/TestAssemblyInitializer.cs`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`; and `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" status --porcelain --untracked-files=all -- QuickFiler.Test UtilitiesCS.Test TestSupport` prints only paths drawn from that same five-path set plus any class-(b) path enumerated by [P2-T4] that lies under one of those three directories, with zero lines also acceptable. Additionally, `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" diff --name-only main...HEAD -- ':!docs' ':!.claude'` must print exactly those same five write-set paths and nothing else, and `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" status --porcelain --untracked-files=all -- ':!docs' ':!.claude'` must print only paths drawn from the five-path set plus the class-(b) list enumerated by [P2-T4]. That whole-tree pair is what carries AC7's phrase `the diff` beyond the three code directories; it is satisfiable because the same anchored span printed zero lines before Phase 1 began. Record all four spans and their outputs in `evidence/qa-gates/write-set-closed.<ts>.md` with all four schema rows.
- [ ] [P2-T16] Verify the code diff contains none of the prohibited constructs. Acceptance: the output of `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" diff main...HEAD -- QuickFiler.Test UtilitiesCS.Test TestSupport` contains zero lines containing the token `DoNotParallelize`, zero lines containing the token `Workers`, zero lines containing the token `Thread.Sleep`, and zero lines containing the token `Task.Delay`. If the porcelain companion in [P2-T15] printed a non-empty list, additionally search each listed working-tree file that is one of the five write-set paths for the same four tokens and require zero hits there too; class-(b) paths enumerated by [P2-T4] are excluded from that search, because a pre-existing occurrence in an unrelated file is not a construct this change introduced. The pathspec deliberately excludes `docs` and `.claude`, so the plan's and the issue's own discussion of those prohibited constructs cannot satisfy or unsatisfy this gate. Record the spans and the four zero counts in `evidence/qa-gates/prohibited-constructs-absent.<ts>.md` with all four schema rows. Note in that artifact that `Thread.Sleep` and `Task.Delay` are additionally banned repository-wide by the BannedApiAnalyzers entries at `BannedSymbols.txt` lines 4-7, which the analyzer gate at [P2-T5] enforces independently.
- [ ] [P2-T17] Verify the file-size limit holds for every file in the write set. Acceptance: each of `TestSupport/TestAssemblyResolver.cs`, `QuickFiler.Test/SetupAssemblyInitializer.cs` and `UtilitiesCS.Test/TestAssemblyInitializer.cs` has a line count strictly under 500 as measured AFTER the CSharpier pass in [P2-T2]. Record the three measured counts in `evidence/qa-gates/file-size-audit.<ts>.md` with all four schema rows.

**AC locator rule.** [P1-T11] edits `issue.md` above the `## Acceptance Criteria` section and changes that section's line numbers, so [P2-T18] through [P2-T25] locate each criterion by a verbatim single-line token drawn from the criterion's own text and never by a line number. In each task below, locate the unique `- [ ]` line WITHIN the `## Acceptance Criteria` section that contains the quoted token, change only its leading `- [ ]` to `- [x]`, and leave every character after the checkbox byte-identical. If a token matches zero lines or more than one line inside that section, STOP AND REPORT rather than guessing. Matches outside that section are ignored.

- [ ] [P2-T18] Check off AC1 in `issue.md`, locating it by the token `installs an `AssemblyResolve` fallback in its own`. Change only `- [ ]` to `- [x]`; do not alter the criterion text. Acceptance: that single line now begins `- [x]`, its text after the checkbox is byte-identical to its prior text, and the evidence cited for it in the executor's report is [P1-T6] plus [P1-T1].
- [ ] [P2-T19] Check off AC2 in `issue.md`, locating it by the token `PRIMARY GUARD:`. Acceptance: that single line now begins `- [x]`, its text after the checkbox is byte-identical to its prior text, and the evidence cited is the three M3 runs [P2-T7] through [P2-T9] and the summary [P2-T10]. This task is reachable only when [P2-T10] recorded three passing M3 runs; under the STOP AND REPORT branch the executor has already halted at [P2-T10] and this task is never attempted.
- [ ] [P2-T20] Check off AC3 in `issue.md`, locating it by the token `The resolver logic is shared between`. Acceptance: that single line now begins `- [x]`, its text after the checkbox is byte-identical to its prior text, and the evidence cited is [P1-T1], [P1-T4], [P1-T5], [P2-T14], and the `## Sharing mechanism justification` section of the [P1-T12] artifact, which is the record of the justification AC3 requires.
- [ ] [P2-T21] Check off AC4 in `issue.md`, locating it by the token `behaviour is unchanged in effect`. Acceptance: that single line now begins `- [x]`, its text after the checkbox is byte-identical to its prior text, and the evidence cited is [P1-T7] and [P2-T12].
- [ ] [P2-T22] Check off AC5 in `issue.md`, locating it by the token `The explanatory comment states why the resolver exists`. Acceptance: that single line now begins `- [x]`, its text after the checkbox is byte-identical to its prior text, and the evidence cited is [P1-T2].
- [ ] [P2-T23] Check off AC6 in `issue.md`, locating it by the token `The full `QuickFiler.Test` suite passes with`. Acceptance: that single line now begins `- [x]`, its text after the checkbox is byte-identical to its prior text, and the evidence cited is [P2-T11], with the executor's report restating that this run is a regression check only and is non-probative for the fix itself.
- [ ] [P2-T24] Check off AC7 in `issue.md`, locating it by the token `is unmodified, and the diff contains no`. Acceptance: that single line now begins `- [x]`, its text after the checkbox is byte-identical to its prior text, and the evidence cited is [P2-T13], [P2-T15] and [P2-T16].
- [ ] [P2-T25] Check off AC8 in `issue.md`, locating it by the token `The C# toolchain passes in CLAUDE.md order`. Acceptance: that single line now begins `- [x]`, its text after the checkbox is byte-identical to its prior text, and the evidence cited is [P2-T2], [P2-T3], [P2-T5], [P2-T6] and [P2-T11].
- [ ] [P2-T26] Emit the AC status summary in the required format from the `acceptance-criteria-tracking` skill, and mirror it to `evidence/issue-updates/ac-status-summary.<ts>.md`. Acceptance: the summary names `docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877/issue.md` as the source, states `Total AC items: 8`, states the checked-off count, states the remaining count, and lists the text of any remaining unchecked criterion. The counts must be derived by counting `- [x]` and `- [ ]` lines within the `## Acceptance Criteria` section of `issue.md`, not asserted from this plan.
- [ ] [P2-T27] Write the non-probative-status declaration into the executor's own final report and mirror it to `evidence/other/discriminator-statement.<ts>.md`. Acceptance: the artifact exists with all four schema rows and contains, in prose, all four of these statements: that M3 run alone with no runsettings is the only discriminator for this fix; that the M2 run at [P2-T11] and the `UtilitiesCS.Test` run at [P2-T12] are regression checks only and are explicitly non-probative; that M3 was executed 3 times after the fix with the per-run totals and exit codes listed; and that M2 was observed to disagree with itself across two runs of identical shape, both totalling 1395 tests, which is why it cannot serve as the gate, while M6's two observations have different totals, 9 against 13, are therefore not a same-command flip, and are not offered as one.
- [ ] [P2-T28] Commit and push the Phase 2 evidence and the AC check-offs. If [P2-T4] enumerated a non-empty class-(b) list, first stage exactly those paths and make one separate commit whose message names them as pre-existing CSharpier drift repaired incidentally by the mandatory repo-wide `format .`; if the class-(b) list is EMPTY, skip that commit. Then stage with `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" add -A -- QuickFiler.Test UtilitiesCS.Test TestSupport docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877`, then a single `git commit`, then `git push origin HEAD`. Acceptance: `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" status --porcelain --untracked-files=all -- QuickFiler.Test UtilitiesCS.Test TestSupport docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877 ':!docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877/plan.2026-09-13T08-43.md'` prints zero lines; `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" ls-files -- docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877/evidence/regression-testing` lists at least the four M3 artifacts plus the two suite artifacts plus the pre-existing `m3-fail-before.2026-09-13T09-14.md`; and the push exits 0. The plan file is excluded from the porcelain span above because marking THIS task `[x]` necessarily dirties it after the commit; that is a fixpoint, not a failure. After marking this task `[x]`, make one further commit staging only `docs/features/active/2026-09-13-quickfiler-test-assembly-resolve-self-sufficiency-877/plan.2026-09-13T08-43.md`, push it, and report in the final report the output of `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" status --porcelain --untracked-files=all -- ':!.claude'`, which must be empty. The gitignored `TestResults/877/` tree contributes nothing to any of these commits; confirm no `.trx` path appears in `git -C "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation" log --name-only --pretty=format: main..HEAD`.

Phase 2 mechanical count of `^- \[ \] \[P2-T\d+\]` matches: 28.

---

## Stop conditions

- If M3 does not pass after the fix, STOP AND REPORT at [P2-T10]. Do not attempt any prohibited change. Do not reinterpret a passing M2 as success.
- If the analyzer or nullable build turns red on a write-set file, fix the write-set file. Do not add a `<LangVersion>` element, do not add a suppression, and do not widen the project-file edits past the single `<Compile>` item each.
- If `csharpier format .` rewrites a file outside the write set that was not already listed as unformatted in the [P0-T10] baseline, restore that file and restart the loop at [P2-T1].
- If the `UtilitiesCS.Test` run stalls, apply only the named `ShellUtilities` filter substitution described in [P2-T12].
