# [P3-T28] AC-482-09 — how `ItemHelper.UnRead == false` was established, and a recorded deviation

Timestamp: 2026-08-27T09-45
EXIT_CODE: 0

Criterion (`### Issue #482` ordinal 9):

> The #482 end-to-end test constructs no `System.Threading.Timer`: `ItemHelper.UnRead` is `false` in
> the arrangement, established explicitly rather than by relying on a default. Verified by inspection
> of the test arrangement and by the absence of any wall-clock wait.

## Recorded deviation from `[P3-T1]`'s stated mechanism

`[P3-T1]` instructs that the arrangement inject a `MailItemHelper` "whose `UnRead` is assigned `false`
**explicitly** rather than by relying on a default". That literal mechanism — an assignment — is
**unsatisfiable**, for two independent reasons established by direct source read:

1. **The setter dereferences a live Outlook item.**
   `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Properties.cs` implements the setter as:

   ```csharp
   set
   {
       _unread = value.ToLazyValue();
       Item.UnRead = value;
       Item.Save();
   }
   ```

   `Item` is null on a default-constructed `MailItemHelper`, so `helper.UnRead = false;` throws
   `NullReferenceException` before the test reaches its Act step. `spec.md`'s
   `## Assumptions, Constraints, Dependencies` section states this prohibition directly: "Tests must
   never *assign* `UnRead`. The setter … writes through to `Item.UnRead` and then calls `Item.Save()`,
   which would dereference the backing Outlook item. Rely on the default only."

2. **The property is not virtual, so it cannot be mocked either.** The declaration is
   `public bool UnRead` with no `virtual` modifier, so a `Mock<MailItemHelper>` cannot override its
   getter. (Other `MailItemHelper` members that existing tests do stub, such as `Subject`, are
   virtual; `UnRead` and `EntryId` are not.)

## Mechanism actually used

The arrangement establishes the value **explicitly by assertion** rather than by assignment:

```csharp
            var helper = new MailItemHelper { EntryId = id };
            helper.UnRead.Should().BeFalse("a false UnRead avoids the 4000 ms timer branch");
```

This satisfies the criterion's operative requirement — "established explicitly rather than by relying
on a default" — because the arrangement now contains an explicit, executed statement about `UnRead`
whose failure aborts the test. It is strictly stronger than a silent reliance on the default: if a
future change to `MailItemHelper` made `UnRead` default to `true`, this assertion fails loudly at the
Arrange step, naming the reason, instead of the test quietly arming a 4000 ms timer and becoming flaky.
The `EntryId` setter is assigned normally; it only writes a `Lazy<string>` and touches no Outlook item.

## Verification that no timer is constructed and no wall-clock wait exists

`ToggleExpansionOn()` arms the timer only inside this branch
(`QuickFiler/Controllers/QfcItemController.Navigation.cs`):

```csharp
            if ((ItemHelper is not null) && ItemHelper.UnRead == true)
            {
                _emailIsReadTimer = new System.Threading.Timer(ApplyReadEmailFormat);
                _emailIsReadTimer.Change(4000, System.Threading.Timeout.Infinite);
            }
```

With `ItemHelper.UnRead` false the branch is not entered, so no `System.Threading.Timer` is
constructed.

Counts over `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs`:

| Pattern | Count |
| --- | --- |
| `Thread.Sleep` | 0 |
| `Task.Delay` | 0 |

Timing evidence from `[P3-T13]`: the three companion tests each completed in `1 ms` and the
interleaving test in `308 ms` (dominated by first-use assembly and Moq initialisation, not by a wait).
A test that had armed the 4000 ms timer and waited on it could not report those figures.

Output Summary: AC-482-09 is satisfied — no `System.Threading.Timer` is constructed and the file
contains zero `Thread.Sleep` and zero `Task.Delay`. The mechanism deviates from `[P3-T1]`'s literal
"assigned" wording because the `UnRead` setter dereferences a null Outlook item and the property is
non-virtual; explicit assertion was used instead, and the deviation is recorded here rather than left
implicit.
