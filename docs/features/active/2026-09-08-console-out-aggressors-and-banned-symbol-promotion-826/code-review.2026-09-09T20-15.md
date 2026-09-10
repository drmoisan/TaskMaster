# Code Review — Issue #826 (console-out aggressors and banned-symbol promotion)

- Date: 2026-09-09
- Branch: `bug/console-out-aggressors-and-banned-symbol-promotion-826-exec`
- Head: `077856c915cccf81d89898d4b3e2537a44b30f3e`
- Base: `dea7b49dae31a9bda8d35ecb73b8c8d646b1a460`
- Reviewer verdict: **ACCEPT** — 0 blocking findings, 5 non-blocking observations

## Scope Reviewed

The full branch diff against the base: 38 paths outside the feature folder plus the feature's own
documentation and 48 evidence artifacts. Reviewed with Read, Grep and Glob only, per the caller's
prohibition on the Bash tool in this unattended run.

## 1. Production Change — `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`

Two statements substituted, 2 added and 2 removed. Both are inside catch clauses that already existed.

```csharp
// line 96, in the else branch of catch (TaskCanceledException)
logger.Warn($"{nameof(GetTableInViewAsync)} timed out on try {counter}");
// line 115, in catch (TimeoutException)
logger.Warn($"{nameof(GetTableInViewAsync)} timed out on try {counter}");
```

**Assessment: correct and minimal.**

- The logging vehicle is right. `logger` is the log4net `ILog` declared in the sibling partial of the same
  `public static partial class OlTableExtensions`, already used at five other sites in this file. No new
  field and no new `using` was required, and none was added.
- The level is right. `Warn` matches this file's own convention for the same class of event — the
  neighbouring warnings cover a failed `GetTableAsync` after maximum attempts and a `COMException`. The
  General Code Change Policy's "match the existing style" rule is satisfied by construction rather than by
  assertion.
- `LogTableTiming` was correctly rejected as the vehicle. It is hard-wired to `logger.Debug`, which would
  bury a fault below the default threshold, and it applies message framing belonging to feature 825's
  instrumentation channel.
- Replacing the literal `"Task"` with `nameof(GetTableInViewAsync)` is a genuine improvement: the message
  now identifies its own origin and survives a rename. The previous text `"Task timed out on try {counter}"`
  was ambiguous in a file that logs from several methods.
- **The inverse constraint was respected.** No catch clause was added, removed or widened. This matters
  concretely: `GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException` pins the behaviour
  that the exception escapes the method, and broadening either catch "to be safe" would have failed it. The
  reviewer confirmed the control flow after each diagnostic — the `counter < 2` retry decision, the
  recursive call's argument list, and the `table = null` fallbacks — is byte-identical to the base.
- Feature 825's ownership boundary held. The deadline window, retry counter, `timeoutSourceFactory` seam
  and `TimeProvider` resolution at lines 60-70 are untouched by this feature.

**Observation CR-1 (non-blocking).** The two messages are now identical, so a reader of the log cannot tell
whether the timeout arrived as a `TaskCanceledException` or a `TimeoutException`. The spec explicitly
permits this ("keeping both messages identical is acceptable and is the minimal change") and leaves
differentiation to the implementer's judgement, so this is not a defect. It is worth noting because the two
paths have different causes — one is a deadline fired by the injected source, the other a cancellation that
was not requested by the caller — and a future diagnosis would benefit from telling them apart. Suggested
follow-up only.

## 2. New Test File — `OlTableExtensionsTimeoutDiagnosticsTests.cs`

235 lines, well inside the 500-line limit. Two test methods, four private helpers.

### The reflective binding is justified, not merely convenient

The file invokes `GetTableInViewAsync` through `MethodInfo.Invoke` rather than calling it directly. The
stated reason is CS1769: the method returns `Task<Outlook.Table>`, and Outlook types are embedded interop
types, so a direct `await` from this assembly is rejected. That is a real compiler restriction on embedded
interop types used as generic type arguments across an assembly boundary, not an avoidable design choice.
The mitigation is handled well:

- The parameter `Type[]` is declared once in a single `SignatureTypes` property, so a future signature change
  is corrected in one place rather than at each call site.
- The binding is asserted, not assumed: `method.Should().NotBeNull("the reflective binding must match the
  current signature")`. If feature 825 or a successor changes the signature, this test fails loudly at the
  binding rather than silently skipping.
- The returned object is type-checked with `BeAssignableTo<Task>()` before being awaited.

This is the right handling of an unavoidable reflection dependency.

### The tests genuinely enter the catch clauses

This was the sharpest question in the review, because a test that asserts only on the injected seam would
prove nothing about the branch. It does not apply here.

`ThrowOnFirstCallFactory` throws on its first invocation. The factory is invoked inside
`TimeOutTask.RunWithTimeout` before that method's own `try` opens, so the exception escapes `RunWithTimeout`
carrying its own type and reaches the matching catch clause in `GetTableInViewAsync`. The three assertions
together are only satisfiable if the catch body executed:

- `factoryInvocations == 2` — a second deadline source was constructed, which only happens on the retry
  inside the catch body.
- `getTableCalls == 1` — the first attempt threw before reaching `GetTable`, so the single call belongs to
  the retry.
- `result.Should().BeSameAs(mockTable.Object)` — the method returned normally. Had the exception not been
  caught it would have propagated out of the `await` and failed the test.

The `catch (TaskCanceledException)` test additionally relies on `CancellationToken.None` never being
cancelled, which is what selects the `else` branch carrying the diagnostic rather than the `table = null`
branch. That is stated in an inline comment at the Act step, which is the correct place for it.

Coverage corroborates the reasoning independently: the `<GetTableInViewAsync>d__32` state-machine class rose
from 0.6533 to 0.88 line-rate, and line 96 moved from 0 hits to non-zero.

### Determinism

Clean. The file contains no `Thread.Sleep`, no `Task.Delay`, no `DateTime.Now`, no wall-clock wait, no
temporary file and no external process. The timeout path is forced by an injected throwing factory, which is
the correct deterministic substitute for waiting out a deadline. This is the crucial property for a test file
whose subject is timeout behaviour, and it holds.

The helper deliberately returns the parameterless `new CancellationTokenSource()` and documents why in-code:
it adds no new banned-symbol call site, which would otherwise have been self-inflicted by the same change
that added the ban. That is careful work.

### Why a fresh source per call

The comment explains that `RunWithTimeout` holds the source in a `using` declaration and disposes it at the
end of each attempt, so returning the same instance would make the retry read `Token` on a disposed source.
This is a genuine "why, not what" comment of the kind the policy asks for — it records a non-obvious
lifetime constraint that a future maintainer would otherwise rediscover through a confusing
`ObjectDisposedException`.

### Structure and assertions

Arrange/Act/Assert is explicit. Every assertion carries a `because` reason, so a failure reports the intent
rather than only the values. Both methods have XML doc comments stating the scenario and the expected
control flow. Naming follows the repository's `Method_Condition_ExpectedOutcome` convention.

**Observation CR-2 (non-blocking).** `InvokeGetTableInViewAsync` dereferences `method` immediately after
`method.Should().NotBeNull(...)`. FluentAssertions' `NotBeNull` does not narrow nullability for the
compiler, so under a future `#nullable enable` on this file the subsequent `method.Invoke` would produce a
CS8602. The file carries no `#nullable enable` today and the project defaults to C# 7.3 nullable-agnostic
behaviour, so nothing fails now. Worth knowing if this file is ever enrolled in the nullable migration.

**Observation CR-3 (non-blocking).** `task.GetType().GetProperty("Result").GetValue(task)` dereferences the
`PropertyInfo` without a null check. It cannot be null for a `Task<T>`, and the preceding
`BeAssignableTo<Task>()` plus the successful `await` make the shape certain, so this is safe in practice.
A defensive assertion would make the intent explicit at no cost.

## 3. Item 1 — the 33-file deletion sweep

112 removed lines, 0 added. The distribution matches the plan's partition exactly: ten files lose a whole
initializer, two `TreeNode` files lose 10 lines each, and 21 files lose one or two lines.

**Assessment: correct, and removal was the right design.**

The spec's argument against a restoring scope is sound and worth affirming, because the intuitive fix is the
wrong one. `Console.SetOut` is process-global while the runsettings specify `<Scope>ClassLevel</Scope>` with
`<Workers>0</Workers>`. A save/restore pair running concurrently across 33 classes interleaves: class A saves
the writer class B just installed, then restores B's writer as the supposed original. That is strictly worse
than the current unrestored install, which at least converges to one stable writer. Issue #811 removed this
exact pattern from `NLogTraceWriter_Test` for the same reason. Deleting is also strictly smaller: it removes
lines and adds none.

The two hazards flagged in the spec were both handled:

- **The `TreeNode` CS0169/CS0414 trap.** Deleting only the call would leave the field assigned but never read
  (CS0414); deleting the call and assignment but keeping the field leaves it never used (CS0169). Both are
  compiler warnings, so the `.editorconfig` `suggestion` ceiling does not apply and
  `/p:TreatWarningsAsErrors=true` would promote them to build errors. The field, its assignment, the call and
  the orphaned commented-out `[ClassInitialize]` block were deleted together. Verified: zero whole-word `tw`
  matches remain in either file, and the nullable gate exits 0.
- **The naming trap.** `QfcHomeControllerCleanupTests.cs` is sibling-owned and is absent from the diff, while
  the four similarly named `QfcHomeController*` files and two `QfcFormController*` files that belong to the
  population were all included. Matching was done on the full name, as the spec required.

Verified post-state: repository-wide `Console.SetOut(` returns exactly two hits, both documented exclusions —
`TaskMaster/ThisAddIn.cs:103` (production, out of scope) and
`UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs:31` (a commented-out
call that installs nothing). None of the 33 modified files retains a `DebugTextWriter` reference.

**Observation CR-4 (non-blocking).** Unused `using` directives left behind by the deletions were not removed.
This is explicitly permitted: the spec records that no project sets `GenerateDocumentationFile`, so IDE0005 is
not emitted by a command-line build and an orphaned `using` cannot fail any gate. The spec further instructs
that reviewers must not treat it as a defect either way. Recorded for completeness only, with no action
implied.

## 4. Item 3 — `BannedSymbols.txt` and `.editorconfig`

`BannedSymbols.txt` grows 7 → 15 lines. All eight added DocIDs are present exactly once, each carrying a
`;` message naming `TimeProvider`, in the existing file's format with no leading whitespace. Three distinct
messages are used, correctly grouped by symbol family rather than copied verbatim across all eight.

The two documented exclusions hold and are correctly reasoned:

- **`TimeoutAfter` is not banned.** It is not a BCL member but a repository-local extension method in
  `UtilitiesCS/Threading/TimeOutTask.cs`, two of whose overloads accept a `TimeProvider` and are the
  documented determinism seam. Banning it would ban the remedy that every existing message points callers
  toward. Verified: zero `TimeoutAfter` occurrences in the delivered file.
- **The parameterless `WaitHandle.WaitOne()` is not banned.** It is a deterministic handshake on a signal
  rather than a wall-clock deadline, 12 of the 13 current call sites use it as the repository's own
  cross-thread idiom, and adding it would contribute 12 more unfixable entries to the backlog that already
  blocks promotion. Verified: every delivered `WaitOne` DocID carries parentheses with a parameter list.

The `.editorconfig` change is comment-only. `dotnet_diagnostic.RS0030.severity = suggestion` remains, exactly
once, and the anchored diff contains zero changed lines carrying `dotnet_diagnostic.` or `.severity`. The new
comment is a clear improvement over what it replaced: it no longer defers to closed issue #181, it states the
promotion precondition inline, and it names the gate that would actually break — the nullable gate, which
passes `TreatWarningsAsErrors`, not the analyzer gate, which does not. That inversion is counter-intuitive
and worth having written down.

The recorded surface figures were independently confirmed by the reviewer: `DateTime.Now` 53 and `Task.Delay`
60 at head, matching the comment.

**Observation CR-5 (non-blocking).** The honesty note is accurate and should not be lost at merge: adding a
DocID while severity is `suggestion` produces **zero build enforcement**. It changes IDE squiggles only. The
value delivered is that the list is pre-staged so the eventual promotion becomes a one-line severity change
rather than a list-design exercise. Any PR description claiming this change makes timing-hack constraints
build-enforced would be wrong. The executor's evidence carries this note forward correctly; the PR body should
too.

## 5. Cross-Cutting Quality

| Dimension | Assessment |
|---|---|
| Simplicity | Strong. Deletion, a two-statement substitution, and eight appended lines. No new abstraction, no indirection. |
| Reusability | Appropriate. The test file factors its shared setup into four small helpers; nothing else in the change has reusable surface. |
| Separation of concerns | Improved. The diagnostic moves off process-global console state onto an injectable, observable logging channel. |
| Error handling | Unchanged by design and verified unchanged. No catch added, removed, widened or narrowed. |
| Naming | Improved at the two changed lines via `nameof`. |
| Comments | Good. The added comments explain why (interop restriction, source lifetime, branch selection), not what. |
| Public API stability | No change. |
| File sizes | Added file 235 lines. Thirteen pre-existing test files remain over 500 lines; this change only reduces line counts. |
| Formatting | CSharpier clean, proven by a 1658-file SHA-256 comparison showing 0 rewrites rather than by exit code alone. |

## 6. Verdict

**ACCEPT. 0 blocking findings.**

Five non-blocking observations are recorded: CR-1 (identical messages on two distinguishable paths), CR-2
(latent CS8602 if the test file is enrolled in the nullable migration), CR-3 (undefended `PropertyInfo`
dereference), CR-4 (orphaned `using` directives, explicitly permitted), CR-5 (the zero-enforcement honesty
note must survive into the PR description).

The change is small, bounded, correctly reasoned and well evidenced. The two most likely ways it could have
gone wrong — breaking the nullable gate through a partial `TreeNode` edit, and shipping eight DocIDs that
silently resolve to nothing — were both anticipated and both positively disproven rather than assumed away.
