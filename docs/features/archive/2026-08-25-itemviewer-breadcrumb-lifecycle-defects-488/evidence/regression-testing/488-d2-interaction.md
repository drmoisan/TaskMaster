# D2 — Interaction With the Controller Theme Tests ([P2-T6])

Timestamp: 2026-08-28T05-36

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession|FullyQualifiedName~ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily|FullyQualifiedName~ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam" "/Logger:trx;LogFileName=488-d2-interaction.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p2-t6-d2-interaction
```

EXIT_CODE: 0

| Test | Outcome |
| --- | --- |
| `ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession` | **Passed** |
| `ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily` | **Passed** |
| `ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam` | **Passed** |

Total tests 3, Passed 3, **Failed 0**. `Test Run Successful.`

## Why the pooled-reuse "no stale pooled theme is replayed" assertion survives the retained-theme replay

This is the highest-risk interaction in the change-set and the reasoning is recorded here explicitly
rather than inferred from the green result.

`ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession` exercises a
pooled-reuse sequence and closes with the assertion labelled in its own source as
"Assert no stale pooled theme is replayed":

```csharp
                // Act pooled reuse with a changed theme and synchronous readiness
                scope.Viewer.ResetBreadcrumb();
                darkMode = false;
                ...
                // Assert no stale pooled theme is replayed
                reused.Should().BeTrue();
                ThemeMessages(reusedSurface).Should().Equal(Theme("light"));
```

The concern the assertion guards against is that a **stale** theme — the `"dark"` value from the first
session — reaches the surface after the pooled viewer is reused with `darkMode = false`.

**The retained value is overwritten before the re-attach, so the replay carries the current theme
rather than a stale one.** The sequence is: the first session sets the theme to `"dark"`, so
`_retainedTheme` holds `"dark"`; the test then calls `ResetBreadcrumb()` and flips `darkMode` to
`false`; the controller's re-attach path issues a fresh `SetBreadcrumbTheme("light")`, and
`BreadcrumbItemViewerLifecycleCoordinator.SetTheme` assigns `_retainedTheme = theme` **before** its two
forwarding calls, so the retained value becomes `"light"` at that moment. Any subsequent replay in
`ConfigureHost`'s newly-adopted branch therefore carries `"light"`.

The assertion is an exact-sequence equality against `Theme("light")`, so it would have failed on a
stale `"dark"` replay and would equally have failed on a duplicated `"light"` entry. It records
exactly one `"light"` message, which is what the delivered behaviour produces.

Two structural properties reinforce this. First, the replay is confined to the **newly-adopted**
branch; the `UpdateRequestProviders` branch performs no theme call, so a reconfigure with the same
host adds nothing. Second, the assignment of `_retainedTheme` precedes the forwards inside `SetTheme`,
so the retained value can never lag the value the caller most recently requested.

## Unmodified-file check (required by the plan's unmodified-file rule)

Command:

```
git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs
```

**Output: no lines.**

The file is byte-identical to its state at `BASE_SHA` `12465043e052fce66a1861bf1ddd037a1aa81afc`. This
is what establishes the second conjunct of the two criteria `[P2-T11]` and `[P2-T12]` flip, each of
which says the named tests pass **unmodified**. A `Passed` outcome alone would also be produced by a
test that had been edited into passing; the empty diff rules that out. All three tests named in this
task live in that one file.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p2-t6-d2-interaction/488-d2-interaction.trx`

Output Summary: EXIT_CODE 0, failed count **0**, all three named tests `Passed`. The pooled-reuse
"no stale pooled theme is replayed" assertion survives because `_retainedTheme` is overwritten by the
subsequent `SetBreadcrumbTheme("light")` before the re-attach, so the replay carries the current theme.
`git diff --name-only <BASE_SHA> -- QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs`
produces **no output lines**.
