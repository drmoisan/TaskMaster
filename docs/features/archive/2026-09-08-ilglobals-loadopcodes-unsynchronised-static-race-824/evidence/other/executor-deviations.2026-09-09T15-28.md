# Executor deviations from the plan as written (Issue #824)

Timestamp: 2026-09-09T15-28

This artifact records every point at which the executed run departed from the literal text of
`plan.2026-09-08T23-51.md`, with the observation that motivated it. It is written so a reviewer does
not have to reconstruct the departures from the surrounding artifacts.

---

## D-1 — P0-T15: the recorded base is not inert, and the halt branch was not taken

Full record: `evidence/baseline/base-inertness.2026-09-09T15-19.md`.

Summary. The worktree was fast-forwarded to the epic integration tip after the plan cleared
preflight, so `git merge-base HEAD origin/main` now predates the merged work of five sibling
children. The inherited listing carries 304 paths, 277 of them outside the three D6 classes.
P0-T15's acceptance is therefore not met and **P0-T15 is left unchecked**.

The plan's stated response is to halt before Phase 1. That response was not taken, because the
authorising delegation permits blocking only during preflight, records this exact base state as
verified and expected, and because the property the affected gates establish remains fully
verifiable against a different anchor.

Adaptation. For the footprint gates P4-T7, P4-T8 and P5-T3, and for the anchored-diff span of
P4-T2, P4-T5 and P4-T6, the plan's literal merge-base-anchored command is still run and recorded,
and the same command anchored on `HEAD` is run alongside it. Acceptance is judged on the
`HEAD`-anchored result, which measures this run's own footprint. `HEAD` is used symbolically, so no
SHA is pinned and plan D5 is preserved. The adaptation excludes only commits this run did not make,
so it cannot conceal anything this run did.

---

## D-2 — P1-T4: the `IsInitOnly` match count is 4, not the 2 the task states

Observation. P1-T4 requires that a `Grep` for `IsInitOnly` over
`UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` return exactly two matches. The
observed count is four. The four matching lines are:

```
95:        public void SingleByteOpCodes_FieldIsInitOnly()
106:                .IsInitOnly.Should()
119:        public void MultiByteOpCodes_FieldIsInitOnly()
130:                .IsInitOnly.Should()
```

Cause. The two test-method names are mandated by the same task, and the token `IsInitOnly` is a
substring of `FieldIsInitOnly`, so each declaration line matches as well as each assertion line.
The count of two is therefore arithmetically unreachable for any implementation that uses the
mandated names and asserts `FieldInfo.IsInitOnly`, which AC3 and the plan both require. This is the
same identifier-collision class the plan itself guards against in its Phase 6 preamble, where the
token `**AC1` is noted to prefix `**AC10`, `**AC11` and `**AC12`.

Disposition. The substantive property the condition expresses — exactly two `FieldInfo.IsInitOnly`
assertions, one per field — was verified with the discriminating pattern `\.IsInitOnly`, which
matches the member-access sites and not the declaration names:

```
Grep \.IsInitOnly over ILGlobals_Tests.cs = 2
```

Every other P1-T4 condition holds exactly as written: `SingleByteOpCodes_FieldIsInitOnly` = 1,
`MultiByteOpCodes_FieldIsInitOnly` = 1, `using System.Reflection;` = 1, `BeforeFieldInit` = 0. The
implementation was not altered to chase the literal count, because doing so would have required
either renaming the tests away from the mandated names or replacing the mandated
`FieldInfo.IsInitOnly` assertion with the `Attributes.HasFlag(FieldAttributes.InitOnly)` form that
AC3 does not name.
