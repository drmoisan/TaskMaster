# [P7-T1] Repository-Wide CSharpier Format

Timestamp: 2026-09-08T10-18
Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0
Output Summary: Two passes were required. The first pass rewrote one file, `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs`, growing it from 173 to 175 lines. That triggered a loop restart per D4. The second pass changed nothing and both observation spans came back identical, so the tree is formatter-clean.

## Pass 1

Verbatim printed `Formatted` line:

```
Formatted 1615 files in 7299ms.
```

PATH_SETS_IDENTICAL: True
DIFFSTAT_IDENTICAL: False

The porcelain path set was unchanged at 46 lines before and after, which is the expected blindness of that span: the rewritten file was already reported as an added path before the pass and was still reported as one after it. The diffstat span is what detected the rewrite. Two rows moved:

```
before:  .../Viewers/BreadcrumbPopupOwnerRegistryTests.cs   |  173 +++
after:   .../Viewers/BreadcrumbPopupOwnerRegistryTests.cs   |  175 +++

before:  46 files changed, 3571 insertions(+), 301 deletions(-)
after:   46 files changed, 3573 insertions(+), 301 deletions(-)
```

The rewrite was a two-line growth in one assertion, where CSharpier split a member chain that exceeded the 100-column print width at its indentation onto separate lines. No other file in the repository moved.

Because a pair differed, this step changed files, so the loop restarted from [P7-T1] per D4 rather than proceeding.

## Pass 2

Verbatim printed `Formatted` line:

```
Formatted 1615 files in 2676ms.
```

PATH_SETS_IDENTICAL: True
DIFFSTAT_IDENTICAL: True

Both spans are byte-identical before and after the invocation, with the porcelain span at 46 lines on both sides and every diffstat row unchanged, including the summary row `46 files changed, 3573 insertions(+), 301 deletions(-)`. This is the final pass and it satisfies the task's acceptance condition that the final pass report both flags `True`.

## Why the exit code is not the observation

`format` rewrites tracked source and still exits 0 after rewriting, and prints a `Formatted <count> files in <duration>ms.` line whether or not it changed anything. Pass 1 and pass 2 both exited 0 and both printed such a line, while pass 1 changed a file and pass 2 did not; the exit code and the printed line are therefore identical across a repairing run and a clean one, and only the before-and-after span comparison distinguishes them.

## Effect on the rest of the loop

The analyzer, nullable and test gates of this phase had not yet run when pass 1 rewrote the file, so no earlier gate result was invalidated by the rewrite. [P7-T2] through [P7-T5] run against the tree as pass 2 left it.
