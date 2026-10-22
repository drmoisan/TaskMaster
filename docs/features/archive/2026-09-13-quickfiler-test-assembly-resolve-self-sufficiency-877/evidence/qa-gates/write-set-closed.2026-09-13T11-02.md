# Gate: the code write set is exactly five paths — issue #877

Timestamp: 2026-09-13T11-02
Command: four git spans, listed individually below
EXIT_CODE: 0
Output Summary: The anchored diff over the three code directories printed exactly 5 lines, matching the approved write set exactly. The anchored whole-tree diff excluding `docs` and `.claude` printed the SAME 5 lines and nothing else. Both porcelain companions printed zero lines. The write set is closed.

## Span 1 — anchored diff over the three code directories

Command: `git -C <repo-root> diff --name-only main...HEAD -- QuickFiler.Test UtilitiesCS.Test TestSupport`

Output, verbatim, 5 lines:

```
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/SetupAssemblyInitializer.cs
TestSupport/TestAssemblyResolver.cs
UtilitiesCS.Test/TestAssemblyInitializer.cs
UtilitiesCS.Test/UtilitiesCS.Test.csproj
```

This is exactly the approved five-path write set. The newly created `TestSupport/TestAssemblyResolver.cs` is visible to the anchored diff because the Phase 1 commit at [P1-T13] placed it in `HEAD`.

## Span 2 — porcelain companion over the three code directories

Command: `git -C <repo-root> status --porcelain --untracked-files=all -- QuickFiler.Test UtilitiesCS.Test TestSupport`

Output: zero lines. Zero lines is an acceptable result for this span. No class-(b) path exists to permit, because [P2-T4] recorded `Class (b) list: EMPTY`.

## Span 3 — anchored whole-tree diff

Command: `git -C <repo-root> diff --name-only main...HEAD -- ':!docs' ':!.claude'`

Output, verbatim, 5 lines:

```
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/SetupAssemblyInitializer.cs
TestSupport/TestAssemblyResolver.cs
UtilitiesCS.Test/TestAssemblyInitializer.cs
UtilitiesCS.Test/UtilitiesCS.Test.csproj
```

Identical to span 1 and nothing else. This span is what carries the phrase `the diff` in acceptance criterion AC7 beyond the three code directories: no file anywhere in the repository outside `docs` and `.claude` was changed other than the five write-set paths.

## Span 4 — porcelain whole-tree companion

Command: `git -C <repo-root> status --porcelain --untracked-files=all -- ':!docs' ':!.claude'`

Output: zero lines.

## Complementarity

The anchored diff enumerates committed change and is blind to untracked files; porcelain status covers untracked and uncommitted change and goes empty once a change is committed. Each alone is wrong in one state, so both are recorded for every scope.

## Base ref

`main` resolves as a local ref at `e4349a62c0fe6a5daece0b0554a0da5f508129d6`, so no `origin/main...HEAD` substitution was made.
