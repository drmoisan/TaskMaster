---
name: iapplicationglobals-member-forces-implementers
description: Adding a member to IApplicationGlobals forces edits to ~7 hand-written test-double implementers beyond any scope lock; Moq mocks auto-implement and need no change
metadata:
  type: project
---

`IApplicationGlobals` (UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs) has multiple hand-written concrete implementers in test projects. Adding any new interface member breaks compilation of ALL of them, forcing edits beyond a plan's scope lock.

Known hand-written implementers (as of #261, 2026-07-07):
- QuickFiler.Test: FakeApplicationGlobals in EfcHomeControllerLifecycleTests.cs, EfcHomeControllerMetricsTests.cs, EfcHomeControllerTests.cs (member style `=> null;`)
- TaskMaster.Test: StubApplicationGlobals in AppOlObjectsCoverageTests.cs, AppOlObjectsTests.cs (`=> throw new NotSupportedException();`), AppToDoObjectsTestDoubles.cs (`=> throw new NotSupportedException();`)
- UtilitiesCS.Test: StubGlobals in EmailIntelligence/EmailDataMiner_TestSupport.cs (`=> throw new NotImplementedException();`)

**Why:** these are `: IApplicationGlobals` classes, not Moq mocks. `Mock<IApplicationGlobals>` / `Mock.Of<IApplicationGlobals>()` auto-implement new members (default/null) and need NO edit.

**How to apply:** when a plan adds a member to IApplicationGlobals but its scope lock omits these 7 files, add a minimal member to each matching that file's existing style. This is a mechanically-necessary consequence of the interface change (complete-and-escalate past preflight), not replanning — record it as a scope addition. A planner SHOULD list these 7 files in the scope lock up front.
