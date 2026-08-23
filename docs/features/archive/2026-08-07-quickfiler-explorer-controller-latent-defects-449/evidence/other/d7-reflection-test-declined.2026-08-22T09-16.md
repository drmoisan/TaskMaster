# D7 — Optional Reflection Contract Test DECLINED (Issue #449, [P6-T13])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command:
```
git grep -n --untracked -F "Contract_ExplConvView_Cleanup_IsNotDeclaredOnTheInterface" -- QuickFiler.Test
```
EXIT_CODE: 1
Output: (empty — no match)

## Decision

The optional reflection test `Contract_ExplConvView_Cleanup_IsNotDeclaredOnTheInterface`, listed as
item 11 in research section 5.5, is **NOT added**. No test of that name exists anywhere under
`QuickFiler.Test`, confirmed by the search above (run with `--untracked` so it covers the new,
not-yet-committed `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs`).

The test would have taken roughly the form:

```csharp
typeof(IQfcExplorerController).GetMethod("ExplConvView_Cleanup").Should().BeNull();
```

## Reasons

1. **It asserts the absence of a member rather than a behaviour.** Every other test added by this
   change asserts something the production code DOES: the destination folder is assigned to the
   captured explorer, the dialog seam is consulted once, the remembered view is applied. This test
   would assert only that a name is not present in a type's metadata. It encodes no behaviour, so a
   reader learns nothing about what the class is supposed to do.

2. **It encodes nothing that the compiler does not already enforce more strongly.**
   `IQfcExplorerController` has exactly one implementer, so the build is already the gate for the
   paired removal: an unpaired edit fails with CS0535. A reflection assertion adds no coverage of any
   executable line and no protection the compiler does not already provide.

3. **It would permanently block a future restoration.** This is the decisive reason. `ExplConvView_Cleanup`
   was removed because it was unimplemented and uncalled, not because the concept is wrong — a future
   change may legitimately want a real conversation-view cleanup member. A test asserting the member's
   absence fails the moment anyone reinstates it, and its failure message would say nothing about why
   restoration is forbidden, because nothing forbids it. The test would function as an unexplained veto
   on future design.

4. **It would be a brittle inversion of the normal test contract.** A test that passes because an API
   does not exist cannot distinguish "deliberately removed" from "accidentally never added", and it
   would also pass if the interface were renamed or deleted outright.

## Recorded substitute

The **[P3-T7] fail-before-exception dossier** is the recorded substitute:
`../regression-testing/fail-before-exception.defect1.2026-08-22T09-16.md`.

It supplies, in auditable form, exactly what the reflection test would have gestured at and more:

- `WhyFailingRunImpossible:` — why no behavioural fail-before test can exist for this defect.
- A full enumeration of all six pre-change references with file, line, and compilation status.
- Proof that the three surviving references are in uncompiled files
  (`QuickFiler/QuickFiler.csproj` carries zero `Compile Include` entries for `Legacy\` or `Notes\`).
- Proof that no file under `QuickFiler.Test` referenced the member.
- The compiler-as-gate argument, discharged by two passing builds.

That dossier records the removal permanently without constraining future code, which is precisely the
property the reflection test lacks.

The same reasoning was applied and recorded for the defect-3 analogue: a reflection assertion that
`typeof(QfcExplorerController).GetMethod("StripTabsCrLf", BindingFlags.NonPublic | BindingFlags.Static)`
is null was likewise rejected, in
`../regression-testing/fail-before-exception.defect3.2026-08-22T09-16.md`, because it asserts the
absence of a private implementation detail.

## Output Summary

The optional reflection contract test `Contract_ExplConvView_Cleanup_IsNotDeclaredOnTheInterface`
(research section 5.5 item 11) is **DECLINED and not added**; `git grep --untracked` over
`QuickFiler.Test` confirms no test of that name exists (EXIT_CODE 1, empty output). It is declined
because it asserts the absence of a member rather than a behaviour, adds nothing the compiler does not
already enforce via CS0535 on the single-implementer interface, and would permanently block a
legitimate future restoration with an unexplained veto. The recorded substitute is the [P3-T7]
fail-before-exception dossier at
`../regression-testing/fail-before-exception.defect1.2026-08-22T09-16.md`.
