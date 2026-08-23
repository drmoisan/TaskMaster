# Fail-Before Exception Dossier — Defect 1, `ExplConvView_Cleanup` (Issue #449, [P3-T7])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`
Merge-base SHA: `c551eabab0aa0a6b1a284252811a2e1de819634e`

Command:
```
git grep -n -F "ExplConvView_Cleanup" c551eabab0aa0a6b1a284252811a2e1de819634e -- "*.cs"
```
EXIT_CODE: 0

The command is pinned to the merge-base SHA so it reproduces the PRE-CHANGE hit set on demand, rather
than describing a tree state that this plan has already altered. Re-running it reproduces the
`SearchResult:` below verbatim.

## WhyFailingRunImpossible

The remedy removes a member that no compiled production or test code calls, so there is no observable
behaviour whose change a test could detect. A test asserting the member's absence would assert the
non-existence of an API rather than a behaviour, and would permanently block restoration.

Expanded: the member's entire compiled body was `throw new NotImplementedException();`. Nothing in any
compiled assembly invoked it, so no input to any public or internal API could cause that line to
execute. Before the change, no test could observe the throw without calling the member directly — and
a test that calls a member solely to observe that it throws `NotImplementedException` asserts the
absence of an implementation, not a behaviour. After the change the member does not exist, so the same
test could not even compile. There is consequently no assertion that is meaningful both before and
after, which is what a fail-before/pass-after pair requires.

Two candidate mechanisms were considered and rejected:

1. **Reflection contract test** —
   `typeof(IQfcExplorerController).GetMethod("ExplConvView_Cleanup").Should().BeNull()`. This does
   genuinely fail before and pass after, and the general policy lists "Contract / schema tests" as a
   category. It is rejected because it asserts the ABSENCE of a member: it encodes no behaviour and
   would permanently block a future restoration of the member by failing the moment anyone reinstated
   it. [P6-T13] records the same decision for the test-suite side, and
   `../other/d7-reflection-test-declined.<timestamp>.md` is its artifact.
2. **`NotThrow` assertion** —
   `System.Action act = () => controller.ExplConvView_Cleanup(); act.Should().NotThrow<NotImplementedException>();`
   would be the correct fail-before test **if** the decision had been to IMPLEMENT the member. The
   ratified decision (D1) is to REMOVE it, so this mechanism does not apply: after removal the call
   does not compile.

The recommended and adopted gate is the **compiler**: the interface has exactly one implementer, so
removing the member from the interface forces the paired removal of the implementation or the build
fails with CS0535. That gate was exercised and passed — see
`phase3-analyzer-build.2026-08-22T09-16.md` (EXIT_CODE 0) and
`phase3-nullable-build.2026-08-22T09-16.md` (EXIT_CODE 0).

## Absence-of-caller proof

SearchScope: the entire repository at merge-base `c551eabab0aa0a6b1a284252811a2e1de819634e`, all
tracked `*.cs` files. Additionally, the post-change working tree under `QuickFiler.Test` including
untracked files (`git grep --untracked -n -F "ExplConvView_Cleanup" -- QuickFiler.Test`).

SearchPatterns: `ExplConvView_Cleanup` (fixed string, `-F`).

SearchResult: **six** pre-change hits, enumerated in full with file, line, and compilation status:

| # | File | Line | Kind | Compiled? | Disposition |
| --- | --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Controllers/QfcExplorerController.cs` | 60 | `//PRIORITY:` comment | YES | removed by [P3-T2] |
| 2 | `QuickFiler/Controllers/QfcExplorerController.cs` | 61 | implementation declaration | YES | removed by [P3-T2] |
| 3 | `QuickFiler/Interfaces/IQfcExplorerController.cs` | 12 | interface declaration | YES | removed by [P3-T1] |
| 4 | `QuickFiler/Legacy/QuickFileController.cs` | 673 | call site | **NOT COMPILED** | retained |
| 5 | `QuickFiler/Legacy/QuickFileController.cs` | 851 | declaration | **NOT COMPILED** | retained |
| 6 | `QuickFiler/Notes/notes_interfaces.cs` | 58 | duplicate interface declaration | **NOT COMPILED** | retained |

Verbatim command output:
```
c551eabab0aa0a6b1a284252811a2e1de819634e:QuickFiler/Controllers/QfcExplorerController.cs:60:        //PRIORITY: Implement ExplConvView_Cleanup
c551eabab0aa0a6b1a284252811a2e1de819634e:QuickFiler/Controllers/QfcExplorerController.cs:61:        public void ExplConvView_Cleanup()
c551eabab0aa0a6b1a284252811a2e1de819634e:QuickFiler/Interfaces/IQfcExplorerController.cs:12:        void ExplConvView_Cleanup();
c551eabab0aa0a6b1a284252811a2e1de819634e:QuickFiler/Legacy/QuickFileController.cs:673:                ExplConvView_Cleanup();
c551eabab0aa0a6b1a284252811a2e1de819634e:QuickFiler/Legacy/QuickFileController.cs:851:        public void ExplConvView_Cleanup()
c551eabab0aa0a6b1a284252811a2e1de819634e:QuickFiler/Notes/notes_interfaces.cs:58:        void ExplConvView_Cleanup();
```

Three of the six hits are the removals; three survive. Every surviving hit is in an uncompiled file.
The only pre-change CALL SITE in the entire repository is hit #4, and it is not compiled — so the
compiled surface had **zero** callers.

### Supporting fact — the `Legacy/` and `Notes/` hits are NOT COMPILED

Command: `grep -c 'Compile Include="Legacy' QuickFiler/QuickFiler.csproj` -> EXIT_CODE 1, output `0`
Command: `grep -c 'Compile Include="Notes' QuickFiler/QuickFiler.csproj` -> EXIT_CODE 1, output `0`

`QuickFiler/QuickFiler.csproj` contains **zero** `Compile Include` entries for either directory. These
are legacy non-SDK `packages.config` projects, which enumerate every compiled source file explicitly
rather than globbing, so a file with no `Compile Include` entry is never passed to the compiler. Hits
#4, #5, and #6 are therefore inert text, not compile-time references. Hit #4 in particular is a call
inside `QuickFileController.cs`, which is itself uncompiled, so it resolves against that file's own
local declaration at hit #5 rather than against the interface this change edits.

### Mock-setup proof

Command: `git grep -n --untracked -F "ExplConvView_Cleanup" -- QuickFiler.Test`
EXIT_CODE: 1
Output: (no match)

**No file under `QuickFiler.Test` references the member.** `--untracked` is used so the search covers
`QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs`, which this plan created and which is not
yet committed. There is no mock setup, no `Verify`, no `VerifySet`, and no reflection assertion
anywhere in the test suite naming the member, so no test could have observed its removal.

### Compiler proof

`IQfcExplorerController` has exactly **one** implementer, `QfcExplorerController`. The build therefore
enforces the paired edit: removing the interface member without removing the implementation leaves a
harmless public method, while removing the implementation without removing the interface member fails
with CS0535. Both the analyzer build and the nullable build were run after the paired removal and both
returned EXIT_CODE 0 with 0 errors, so the pair is complete and self-consistent and no compiled caller
broke.

## Post-change confirmation

`ac1-cleanup-references.2026-08-22T09-16.md` records the post-change search: three hits remain, all in
the uncompiled files above, and **zero** in either compiled file.

## Output Summary

A behavioural fail-before run for defect 1 is structurally impossible, because the removed member had
zero compiled callers and a body consisting only of `throw new NotImplementedException();` — there is
no behaviour that differs before and after, and the only tests that could distinguish the states assert
the absence of an API rather than a behaviour. The absence proof enumerates all **six** pre-change
hits: three are the removals themselves, and the remaining three (`Legacy/QuickFileController.cs:673`
and `:851`, `Notes/notes_interfaces.cs:58`) are provably NOT COMPILED because
`QuickFiler/QuickFiler.csproj` carries zero `Compile Include` entries for those directories. No file
under `QuickFiler.Test` references the member. The compiler is the adopted gate and it passed
(EXIT_CODE 0 on both the analyzer and nullable builds).
