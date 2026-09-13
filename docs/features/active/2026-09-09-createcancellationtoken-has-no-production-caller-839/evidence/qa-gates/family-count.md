# CreateCancellationToken family count (AC9) — issue #839

Timestamp: 2026-09-13T06-18
Command: git -c grep.patternType=fixed grep -n -e "CreateCancellationToken()" -- "QuickFiler/*.cs" "QuickFiler.Test/*.cs"
Command: git -c grep.patternType=fixed grep -n -e "CreateCancellationToken();" -- "QuickFiler/*.cs" "QuickFiler.Test/*.cs"
Command: git -c grep.patternType=fixed grep -n -e "void CreateCancellationToken()" -- "QuickFiler/*.cs" "QuickFiler.Test/*.cs"
EXIT_CODE: 0

## Output Summary

Baseline from [P0-T17], quoted beside this result: 6 family lines, 4 invocation lines, 2 declaration lines. This result: 7, 5, 2. The single added line in each of the first two counts is the inserted invocation at QfcHomeController.cs line 88, and the declaration count is unchanged.

CMD-FAMILY-ALL, exactly 7 lines:

    QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:124:            controller.CreateCancellationToken();
    QuickFiler/Controllers/EfcHomeController.cs:62:            CreateCancellationToken();
    QuickFiler/Controllers/EfcHomeController.cs:126:            home.CreateCancellationToken();
    QuickFiler/Controllers/EfcHomeController.cs:162:            home.CreateCancellationToken();
    QuickFiler/Controllers/EfcHomeController.cs:399:        internal void CreateCancellationToken()
    QuickFiler/Controllers/QfcHomeController.cs:88:            CreateCancellationToken();
    QuickFiler/Controllers/QfcHomeController.cs:466:        internal void CreateCancellationToken()

CMD-FAMILY-CALLS, exactly 5 lines, namely EfcHomeController.cs 62, 126 and 162, QfcHomeController.cs 88, and QfcHomeControllerMetricsTests.cs 124:

    QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:124:            controller.CreateCancellationToken();
    QuickFiler/Controllers/EfcHomeController.cs:62:            CreateCancellationToken();
    QuickFiler/Controllers/EfcHomeController.cs:126:            home.CreateCancellationToken();
    QuickFiler/Controllers/EfcHomeController.cs:162:            home.CreateCancellationToken();
    QuickFiler/Controllers/QfcHomeController.cs:88:            CreateCancellationToken();

CMD-FAMILY-DECLS, exactly 2 lines, QfcHomeController.cs and EfcHomeController.cs 399:

    QuickFiler/Controllers/EfcHomeController.cs:399:        internal void CreateCancellationToken()
    QuickFiler/Controllers/QfcHomeController.cs:466:        internal void CreateCancellationToken()

## What this establishes for AC9

The member gained exactly one production caller and nothing else in the family moved. `QfcHomeController.CreateCancellationToken()` previously had zero production invocations, which is the defect; it now has exactly one, at line 88 inside `Init()`. No declaration was added, removed or duplicated, so the fix did not introduce a second copy of the member. The `EfcHomeController` precedent's three invocations and one declaration are untouched, and the pre-existing test invocation in QfcHomeControllerMetricsTests.cs line 124 is untouched, which is consistent with that file being out of scope.

The declaration in QfcHomeController.cs is reported at line 466 rather than the base tree's 467 because the Decision D1 deletion removed two lines above it and the fix inserted one, a net shift of one line upward. The member itself is unchanged.

## Command-transport note

The three `git grep` spans were addressed to the assigned worktree with a repository-location option in place of a working-directory change, forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one. Each retains the plan's pinned fixed-string engine, the `-n` switch, the `-e` token operand and both quoted pathspecs exactly as written, so the counts are the counts the plan asks for.
