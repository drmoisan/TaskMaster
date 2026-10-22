# Code Review — storewrapper-dialog-imprecise-for-genuine-failure (#287)

- Reviewed: 2026-09-01
- Diff basis: `09eae2e85cd586c092fb1977a76cd9e895ec0a3b..564792e57aa2a6f0088d0b4f727bdf86a115c92a`, five files.

## Overall verdict: PASS

## Design

`StoreLaunchReadinessEvaluator.BuildUnavailableMessage`/`BuildUnavailableTitle` (`UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs:56-93`) are pure switch expressions over `StoreLaunchReadinessState`:

- `Ready => throw new ArgumentOutOfRangeException(...)`
- `StoresUnavailable => <transient copy>`
- `_ => <ModelUnavailable / conservative-default copy>`

This is a good, minimal design choice. Collapsing the "genuine `ModelUnavailable`" and "any undefined cast value" cases onto the same discard arm is deliberate (AC5) and correctly documented as the conservative choice in the XML doc and in `spec.md`'s "Selected copy" section — it never claims a retry will succeed for a state the code cannot characterize. The two call sites (`StoreWrapperController.cs:122-123`, `DisabledStoresController.cs:166-167`) were changed identically and symmetrically, matching the spec's "one shared helper, both call sites" rationale and preserving the pre-existing behavioral contract (gate condition, `MessageBoxButtons.OK`, `MessageBoxIcon.Warning`, `[ExcludeFromCodeCoverage]`, early return without constructing a viewer).

No new class was introduced for what is fundamentally two pure functions — correct application of General Code Change Policy §2.2 ("use functions for small, pure helpers"). Placing them on the existing `StoreLaunchReadinessEvaluator` rather than a new file is well-reasoned given the packages.config-era `.csproj` file-registration hazard the spec documents (an unregistered new `.cs` file compiles into nothing) and the file-size headroom constraint on `StoreWrapperController.cs`.

## Error handling

`ArgumentOutOfRangeException` on `Ready` is the correct fail-fast choice per General Code Change Policy §3.1: reaching either method with `Ready` is a caller defect (readiness gate already excludes `Ready` before either method is called), and returning a plausible-looking string for that case would be a silent-failure hazard. Both call sites are structurally guarded by `if (readiness.State != StoreLaunchReadinessState.Ready)`, so the exception path is unreachable in production but exists as an explicit invariant/contract boundary — consistent with the repository's convention (see `MyBoxModeless.cs`/`EngineToggleStateCoordinator.cs` precedents cited in spec.md and independently confirmed present).

## Naming and documentation

- `BuildUnavailableMessage`/`BuildUnavailableTitle` match the established `Build<X>Message` naming convention in this codebase (`MyBoxModeless.BuildMessage`, `EngineToggleStateCoordinator.BuildUnavailableMessage`/`BuildToggleFailedMessage`/`BuildPrimeFailedMessage`/`BuildUnmappedKeyMessage` — spot-checked directly, all present as claimed).
- XML doc comments on both new methods are complete (`<summary>`, `<param>`, `<returns>`, `<exception>`) and describe behavior accurately, including the throw condition.
- The `StoreLaunchReadinessEvaluator` class-level XML doc was extended (not just the two new methods) to record that `ModelUnavailable` is also the terminal post-catch state, citing `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:66-72` — independently verified accurate against the current file contents (the `catch (Exception e)` block at those lines logs an `Error` line and leaves `StoresWrapper` unset with no retry).
- `DisabledStoresController.Launch`'s XML summary was corrected to drop the now-false "shows the same warning as the single-store editor" claim (`DisabledStoresController.cs:154-159`) and describe the actual state-specific behavior — a real doc-accuracy fix, not just AC-checkbox theater.

## Duplicated string literals

`BuildUnavailableMessage` and `BuildUnavailableTitle` each independently hardcode the `ModelUnavailable` copy string (message and title) inline as switch-arm literals, rather than sharing a single source of truth (e.g., a private constant referenced by both methods, or a combined tuple-returning helper each of the two public methods projects from). This is a minor duplication: the `ModelUnavailable` message string appears twice across the file (once as the switch default in `BuildUnavailableMessage`, and its title counterpart once in `BuildUnavailableTitle`), and each string is also duplicated a second time as a literal in its corresponding MSTest assertion (`StoreWrapperController_Tests.Launch.cs`, four separate places). This is consistent with the file's pre-existing pattern (the old single hardcoded literal was likewise duplicated across the two former call sites before this change), and the spec explicitly rejected introducing a tuple/struct return type for `net48`/`LangVersion 12.0` `IsExternalInit`-unavailability reasons (verified true). Not a blocking finding — flagging only because a future edit to either string must remember to update all four occurrences (2 production + repeated in ~6 test assertions across two test files) by hand; no compiler or test failure will catch a partial update to the *message* text alone, since the title and message asserted values are independent literals in the tests.

## Test quality

- Both new test regions (`#region BuildUnavailableMessage and BuildUnavailableTitle (issue #287)` in `StoreWrapperController_Tests.Launch.cs`, and the two new `[TestMethod]`s appended to `DisabledStoresControllerTests.cs`) are well-isolated, single-assertion-focused, and use AAA structure with comments.
- Test names are descriptive and self-documenting (`BuildUnavailableMessage_WhenStateIsUndefinedCast_ReturnsModelUnavailableCopy`, `Launch_ForModelUnavailableAndStoresUnavailable_ShowsDifferentMessages`).
- The extension of the two pre-existing `Launch_When...` tests to also capture and assert `viewer.Text`/`viewer.TextMessage.Text` is a minimal, surgical addition that reuses the existing arrange/act structure rather than duplicating it — good adherence to "prefer the simplest design."
- `Launch_ForModelUnavailableAndStoresUnavailable_ShowsDifferentMessages` uses an ordering-dependent capture pattern (`if (capturedModelUnavailableMessage is null) { ...first... } else { ...second... }`) keyed on invocation order rather than on which state produced which message. This is correct here because the Act step calls `controllerModelUnavailable.Launch()` then `controllerStoresUnavailable.Launch()` in a fixed, non-parallel sequence within the same test method — but it is slightly less self-documenting than keying capture directly on which controller is being invoked (e.g., passing an index or state tag into the invoker). Not a defect; a minor readability nit only, non-blocking.
- Coverage of `BuildUnavailableMessage`/`BuildUnavailableTitle` is exhaustive over the achievable input space per the spec's own test strategy: one test per defined enum member (2), one for the undefined-cast discard arm, one for the `Ready`-throws case, and one differencing test — for both the message and the title method (10 new focused unit tests total across the two methods, matching the spec's "4 outcomes, no unreachable branch" claim).

## Regression discipline

`StoreLaunchReadinessEvaluator.Evaluate` itself was not touched (confirmed by diff — the new methods are pure additions after the existing `Evaluate` method body); the five pre-existing readiness tests in `StoreWrapperController_Tests.Launch.cs` are untouched by the diff hunks (confirmed: diff hunks stop before the `#region EvaluateLaunchReadiness` region). AC12 is satisfied by construction, not merely by a passing test run.

## Findings summary

- Blocking: none.
- Non-blocking: duplicated literal strings across production/test files (documented above, pre-existing pattern, explicitly justified by the `net48` constraint); minor test-readability nit in the differencing test's capture ordering. Neither warrants remediation-inputs or a follow-up issue on its own.
