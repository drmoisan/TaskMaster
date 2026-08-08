# F13 Research — `QuickFiler/Viewers/WebView2CoreInitializer.cs`

- Epic: #136 `quickfiler-per-file-coverage`, child F13, feature issue #455
- Production file: `QuickFiler/Viewers/WebView2CoreInitializer.cs` (30 lines)
- Current state: class-level `[ExcludeFromCodeCoverage]` at **line 15**; entirely absent from the
  committed Cobertura instrumentation (unmeasured, not covered)
- Research date: 2026-08-07
- Companion artifacts: `00-cross-cutting-context.md`, `08-WebView2BreadcrumbHost.md` (its §4 SDK-type
  evidence and §9 exemption-ground analysis are shared and cited here), `09-WebView2Messenger.md`

## 0. Tooling limitation (read first)

No `Bash`/shell tool was available. No `git`, `gh`, `msbuild`, `vstest` or `csharpier` was executed.
Findings derive from working-tree file content, committed Cobertura evidence, and the Microsoft
WebView2 .NET API reference for package version **1.0.4129.50** (`QuickFiler/packages.config:29`).

---

## 1. Headline verdict

The orchestrator's preliminary finding #1 is **CONFIRMED**, with one important qualification and one
correction.

**Confirmed:** the file is 30 lines containing two expression-bodied members, each a literal single
call into the WebView2 SDK, with no branches, no state, no guards, and no observable behaviour of
its own. Unlike its two siblings, its doc comment (`:9-13`) is **accurate**: it really is a thin
forwarding shim, and it exists precisely so that its callers become routing-testable. Under epic
Shared Design §1's "refactor first, exempt only the irreducible remainder", this file **is** the
remainder. It has already been refactored to the thinnest possible wiring; there is nothing left to
extract.

**Qualification:** executing either member in a unit test is not merely impractical — it would
**violate two separate repository rules**, not just be inconvenient. See §3.

**Correction:** the exemption cannot be justified under `CLAUDE.md` §UT2 as written, because none of
its three enumerated grounds covers a third-party SDK adapter. See §7. This is a governance gap that
F1 must close explicitly; it is not a reason to remove the attribute.

**Disposition: retain `[ExcludeFromCodeCoverage]` at `:15`. Classify `ratified-exempt` in F1's
ledger under a newly-ratified fourth ground. Relocate the existing test file to mirror the
production tree and strengthen it into a seam-contract test.**

---

## 2. Member-by-member testability verdict

| # | Member | Lines | Branches / state / guards | Unmockable SDK type touched | Verdict |
|---|---|---|---|---|---|
| 1 | implicit default constructor | — (no explicit ctor; `:16`) | none | none | **testable today** — already exercised by `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs:19`. Contributes nothing because the type is exempt. |
| 2 | `CreateEnvironmentAsync(string cacheFolder, CoreWebView2EnvironmentOptions options)` | 19-22 | **none** — expression-bodied, single statement `CoreWebView2Environment.CreateAsync(null, cacheFolder, options)` | `CoreWebView2Environment` (static factory; no public constructor) | **irreducible remainder.** Ground: third-party SDK adapter whose single statement requires a live Evergreen WebView2 Runtime **and writes to the filesystem**. |
| 3 | `EnsureCoreWebView2Async(WebView2 control, CoreWebView2Environment environment)` | 25-28 | **none** — expression-bodied, single statement `control.EnsureCoreWebView2Async(environment)` | `Microsoft.Web.WebView2.WinForms.WebView2` (non-virtual instance method on a `Control`-derived type) | **irreducible remainder.** Ground: same, plus it requires a created window handle on an STA thread and starts a browser process. |

**Total coverable lines: 3** (the implicit constructor plus two expression bodies). **Total branches:
0.** If the exemption were simply removed with no other change, the file would report roughly
**33% line coverage** — the constructor covered by the existing smoke test, both members permanently
uncovered — failing the 80% per-file gate with no available remedy short of executing the SDK.

---

## 3. Why executing either member is prohibited, not merely difficult

This is the distinguishing argument for this file and should be stated in the ledger verbatim.

### 3.1 `CreateEnvironmentAsync` would create files on the local filesystem

`CoreWebView2Environment.CreateAsync(null, cacheFolder, options)` creates and populates a user-data
folder at the supplied path. Its production caller passes
`%LocalAppData%\WindowsFormsWebView2` (`WebView2BreadcrumbHost.cs:99-102`). Two repository rules bar
this from a unit test:

- `CLAUDE.md` §UT4: "**Creation and use of temporary files on the local filesystem is expressly
  prohibited** unless explicitly authorized as an exception. Currently approved exceptions: none."
- `.claude/rules/general-code-change.md` § I/O Boundaries: "Use of temporary files within tests is
  strictly prohibited."

There is no path variant that avoids this — the folder is the method's entire purpose.

### 3.2 Both members require an external runtime the test host cannot assume

`CoreWebView2Environment.CreateAsync` locates and launches the Evergreen WebView2 Runtime; a machine
without it throws `WebView2RuntimeNotFoundException`. `CLAUDE.md` §UT4 requires that unit tests "not
depend on external services such as databases, networks, remote APIs, **or external processes**",
and §UT1 requires determinism. A test whose outcome depends on whether the runtime is installed on
the agent or CI runner is non-deterministic by construction.

### 3.3 `EnsureCoreWebView2Async` additionally requires a live control and an STA apartment

It is an instance method on a `Control`-derived `WebView2` that must have a created window handle.
Epic §3's STA last-resort clause permits **in-memory, never-shown WinForms controls** as a last
resort; it does not permit handle creation or browser-process startup, and epic §2 states
explicitly: "Running COM elements on the UI thread is a production-only last resort, never in
tests."

### 3.4 The two members are not mockable and cannot be intercepted

Per `08-WebView2BreadcrumbHost.md` §4: `CoreWebView2Environment` is a `public class` with no public
constructor and no virtual members; `Microsoft.Web.WebView2.WinForms.WebView2` derives from `Control`
with non-virtual members. Moq can intercept neither. There is no interface between this file and the
SDK to interpose on — **this file *is* that interface's implementation.**

---

## 4. Seam design — there is none to add, and adding one is a false economy

The seam this file exists to serve already exists and is already correct:
`QuickFiler/Viewers/IWebViewCoreInitializer.cs` (30 lines, `public interface`, two members mirroring
this class exactly). It is consumed and mocked by:

| Consumer | Site | Owner |
|---|---|---|
| `WebView2BreadcrumbHost` | ctor param `:43`, calls `:108`, `:112` | F13 |
| `BreadcrumbPopupUiOperations` | `BeginInitialization` delegate `:31-35`, `:388` | F13 |
| `BreadcrumbWebViewSurfaceFactory` | `Create(...)` `:164`, `:173`, `:188` | F13 |
| `QfcItemController` | `_webViewInitializer ??= new WebView2CoreInitializer()` at `Controllers/QfcItemController.Initialization.cs:381` | F10 |
| `EfcFormController` | `new WebView2CoreInitializer()` at `Controllers/EfcFormController.cs:838` | F9 |
| `ItemViewer` | `ConfigureBreadcrumbDropDown(CoreWebView2Environment, IWebViewCoreInitializer)`, pinned by `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:31-49` | F14 |

Interposing a second seam beneath this one would only relocate the same two SDK calls one layer
deeper and leave an identically-untestable innermost file — a strictly worse outcome, because it
would add a production file with no coverage benefit. **Recommend no new production file for this
type.**

**Rejected alternative considered:** merging the two forwards into the
`WebView2ControlSurface` adapter proposed in `08-WebView2BreadcrumbHost.md` §3.2. Rejected because
`IWebViewCoreInitializer` has five consumers across four different epic children (F9, F10, F13,
F14), two of which construct `WebView2CoreInitializer` directly. Folding it into an F13-local
adapter would change contracts that three sibling children compile against, breaching the
frozen-signature rule (`00-cross-cutting-context.md` §10) and the epic's no-behaviour-change NFR.

**Residual forwarders keep a class-level `[ExcludeFromCodeCoverage]`** (the existing attribute at
`:15`), not method-level. The file contains no lambdas today, so the lambda-suppression asymmetry
documented in `08-WebView2BreadcrumbHost.md` §3.4 does not bite here — but keeping the attribute at
class level preserves the epic-wide convention that artifact recommends and guards against a future
edit introducing one.

---

## 5. What `WebView2CoreInitializerTests.cs` can actually assert

This directly answers the brief's question. Current file:
`QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` (25 lines, one test asserting
construction yields a non-null `IWebViewCoreInitializer`).

Because the production type carries `[ExcludeFromCodeCoverage]`, **no test can contribute a single
instrumented line to it.** The test's value is therefore entirely contractual, not numeric. What
remains legitimately assertable:

| Assertable | How | Value |
|---|---|---|
| The type constructs and is assignable to `IWebViewCoreInitializer` | existing test, `:17-23` | Catches accidental removal of the interface implementation, which would break five consumers |
| `CreateEnvironmentAsync` exists with return type `Task<CoreWebView2Environment>` and parameters `(string, CoreWebView2EnvironmentOptions)` | reflection over `typeof(IWebViewCoreInitializer)` | Pins the seam shape that F9/F10/F13/F14 mock. A silent parameter reorder would otherwise surface only as a runtime failure in production |
| `EnsureCoreWebView2Async` exists with return type `Task` and parameters `(WebView2, CoreWebView2Environment)` | reflection | Same |
| `WebView2CoreInitializer` **does** carry `ExcludeFromCodeCoverageAttribute` | reflection | Makes the ratified exemption machine-checked, so a future edit that adds logic to this file trips a test rather than silently hiding it. This is the **inverse** of the existing precedent at `ItemViewerBreadcrumbDropDownContractTests.cs:102-130`, which asserts the *absence* of the attribute on `BreadcrumbDropDownOpenCoordinator` |
| The type is `sealed` and `public` | reflection | Prevents a subclass from inheriting the exemption |

**Not assertable, by design:**

- Any behaviour of `CreateEnvironmentAsync` — invoking it writes to `%LocalAppData%` (§3.1) and
  requires the Evergreen runtime (§3.2).
- Any behaviour of `EnsureCoreWebView2Async` — requires a live control handle and starts a browser
  process (§3.3).
- Argument validation — there is none to assert (see defect F2 in §9).

**Is a reflection contract test permitted here?** Yes. `epic.md:521-522` prohibits "shape-assertion
tests written purely to manufacture coverage" for files in the `interface-only / not-measured`
bucket. That prohibition does not apply: this file is `ratified-exempt`, not `interface-only`, and
because it is exempt these tests manufacture **zero** coverage by construction. They exist to pin a
cross-child contract, which is their legitimate purpose.

---

## 6. Concurrency and ordering

**Nothing to report.** The file contains no `Interlocked`, no `Volatile`, no `lock`, no
`CancellationToken`, no `async`/`await`, no `async void`, no `TaskCompletionSource`, no event
subscribe/unsubscribe, no disposal, and no mutable state of any kind. Both members return the SDK's
`Task` directly without awaiting.

One consequence worth recording for the planner: because `CreateEnvironmentAsync` does not `await`,
a synchronous throw from `CoreWebView2Environment.CreateAsync` (for example
`WebView2RuntimeNotFoundException` raised before the task is produced) propagates **synchronously**
to the caller rather than as a faulted task. `WebView2BreadcrumbHost.InitializeAsync:108` awaits it
inside an `async` method, so the distinction is invisible there; but
`BreadcrumbPopupUiOperations.BeginInitializationAsync` (`:141-145`) explicitly guards against a null
return and wraps the call in `RunAsync`, where a synchronous throw is caught by the dispatcher rather
than surfacing as a faulted task. Behaviour is preserved either way; noted so a future refactor does
not "helpfully" add `async`/`await` here and change the failure timing.

---

## 7. Exemption-ground analysis — the governance gap

`CLAUDE.md` §UT2 enumerates exactly three grounds: (a) VSTO add-in lifecycle classes; (b) WinForms
**form-derived** classes and Designer-generated code; (c) Outlook Interop event-handler classes
depending on `Application` / `MailItem` / `Store` / `MAPIFolder` **without an injectable seam**.

`WebView2CoreInitializer` matches **none**:

- (a) it is not a VSTO lifecycle class, entry point, ribbon handler, or COM registration utility;
- (b) it is `sealed class WebView2CoreInitializer : IWebViewCoreInitializer` — it derives from
  nothing, is not a form, and is not Designer-generated. It *takes* a WinForms type as a parameter,
  which is not the same as being form-derived;
- (c) it imports no `Microsoft.Office.Interop.Outlook` type — `using` directives at `:1-4` are
  `System.Diagnostics.CodeAnalysis`, `System.Threading.Tasks`, `Microsoft.Web.WebView2.Core`,
  `Microsoft.Web.WebView2.WinForms`.

Meanwhile `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy states flatly that "No
production file may be excluded from coverage measurement", and prescribes refactoring to the
thinnest possible wiring — after which "the entry point's uncovered lines then represent a real and
visible cost in the coverage metric". Read literally and alone, that rule says: remove the
attribute, accept 33%, and let the visible cost apply pressure to keep the file minimal (it already
is, at 30 lines).

Epic Shared Design §1 (`epic.md:206-225`) reconciles the two as "refactor first, **exempt only the
irreducible remainder**", and F1's ledger provides a `ratified-exempt` bucket. That reconciliation
is the operative authority for this child, and this file is the epic's cleanest example of the
irreducible remainder. **But the reconciliation does not itself supply a ground; §UT2 does, and
§UT2's three grounds do not reach WebView2.**

### Recommendation to F1 (binding on all three WebView2 files)

Ratify a **fourth exemption ground**, worded to be narrow and testable:

> **(d) Third-party SDK adapter types** in which *every* member is a single call into a
> vendor-supplied API that requires a live external runtime process, a created window handle, or
> filesystem side effects to execute — and where an interface seam over that adapter already exists
> and is consumed by non-exempt callers. The adapter must contain **zero branches and zero mutable
> state**; the presence of either disqualifies it and requires extraction instead.

`WebView2CoreInitializer` satisfies (d) exactly: two members, zero branches, zero state, seam
(`IWebViewCoreInitializer`) present and consumed by five callers. The proposed
`WebView2ControlSurface` (`08-WebView2BreadcrumbHost.md` §3.2) and `CoreWebView2MessageChannel`
(`09-WebView2Messenger.md` §3.2) are designed to satisfy the same test. The current
`WebView2BreadcrumbHost` and `WebView2Messenger` do **not** satisfy it — both have branches and
state — which is exactly why their exemptions must be removed. Ground (d) is therefore
self-policing: it grants the exemption only to files that have already been reduced to a remainder.

**If F1 declines to ratify (d),** the fallback is to classify this file `testable`, remove the
attribute, and record a single documented, per-file exception at ~33% with §3's two rule citations
as the rationale. That is a worse outcome (it puts a permanent red row in the capstone's per-file
report) but it is defensible. The planner should route this decision to F1 rather than deciding it
inside F13.

---

## 8. Recommended test-case list

One file, relocated and modestly extended. All cases are contract assertions; none contributes
coverage.

### `QuickFiler.Test/Viewers/WebView2CoreInitializerTests.cs` (~90 lines) — **relocated from `Controllers/`**

The move is required by `.claude/rules/general-unit-test.md` § Test File Location ("Test files must
live in a `tests/` directory tree that mirrors the production source structure") — the production
file is `QuickFiler/Viewers/WebView2CoreInitializer.cs` but the test currently sits under
`QuickFiler.Test/Controllers/`. This is defect L8 in `00-cross-cutting-context.md` §9 and is
in-scope for F13's own execution on the F4 precedent (`epic.md:556-558`).

| # | Test | Asserts |
|---|---|---|
| K1 | `Construction_YieldsAnIWebViewCoreInitializer` | existing test, preserved verbatim (`:17-23`) |
| K2 | `Seam_DeclaresCreateEnvironmentAsyncWithExpectedSignature` | reflection over `IWebViewCoreInitializer`: return `Task<CoreWebView2Environment>`, parameters `(string cacheFolder, CoreWebView2EnvironmentOptions options)` |
| K3 | `Seam_DeclaresEnsureCoreWebView2AsyncWithExpectedSignature` | reflection: return `Task`, parameters `(WebView2 control, CoreWebView2Environment environment)` |
| K4 | `Adapter_ImplementsEverySeamMember` | every `IWebViewCoreInitializer` member has a matching public method on `WebView2CoreInitializer` (guards against an explicit-interface-implementation regression) |
| K5 | `Adapter_IsRatifiedExemptFromCodeCoverage` | `typeof(WebView2CoreInitializer)` carries `ExcludeFromCodeCoverageAttribute`. Machine-checks the ledger decision; inverse of the existing precedent at `ItemViewerBreadcrumbDropDownContractTests.cs:102-130` |
| K6 | `Adapter_IsSealed` | prevents a subclass silently inheriting the exemption |

**Explicitly out of scope:** any test that invokes `CreateEnvironmentAsync` or
`EnsureCoreWebView2Async`. If a plan task proposes one, it is a policy violation under §3.1-§3.3 and
must be rejected.

---

## 9. 500-line and csproj impact

### Production

| File | Before | After | 500-line | Ledger bucket |
|---|---|---|---:|---|
| `QuickFiler/Viewers/WebView2CoreInitializer.cs` | 30 | 30 (unchanged) | OK | **`ratified-exempt`** under proposed ground (d), §7 |
| `QuickFiler/Viewers/IWebViewCoreInitializer.cs` | 30 | 30 (unchanged) | OK | `interface-only / not-measured` — **no** `[ExcludeFromCodeCoverage]` (`epic.md:509-522`) |

**No new production file. No `QuickFiler/QuickFiler.csproj` change for this file.** This is the one
F13 target that adds nothing to the shared csproj, reducing fan-in conflict surface.

### Test

`QuickFiler.Test/QuickFiler.Test.csproj` requires one **modified** entry, not an added one:

- Remove `<Compile Include="Controllers\WebView2CoreInitializerTests.cs" />` at **line 150**
- Add `<Compile Include="Viewers\WebView2CoreInitializerTests.cs" />` into the breadcrumb block at
  lines 60-89

The file is CRLF-terminated and uses an explicit compile list (107 entries). **Preserve CRLF** — use
the `Edit` tool or `perl -0777` with explicit `\r\n`; a git-bash `sed -i` strips it and guarantees a
merge conflict (`epic.md:611-612`).

Because line 150 sits well outside the 60-89 breadcrumb block that every other F13 and F12 test edit
touches, this particular edit is unlikely to conflict at fan-in.

---

## 10. Latent defects (report only — orchestrator promotes via the MCP lifecycle)

| ID | Location | Impact | Confidence |
|---|---|---|---|
| **F1** | `WebView2CoreInitializer.cs:19-28` | **No argument validation on any parameter.** `cacheFolder`, `options`, `control`, and `environment` are all forwarded unchecked. A null `control` produces a `NullReferenceException` from inside the adapter rather than a fail-fast `ArgumentNullException` naming the offending parameter, and a null `cacheFolder` surfaces as an opaque SDK error. Contradicts `CLAUDE.md` §C#4.1 ("Fail fast with explicit exceptions when invariants are violated") and § General 3.3 ("Enforce invariants at construction/initialization time"). **Note the interaction with the exemption:** adding guards would make ~2 lines of this file testable, raising it from ~33% to roughly 50% — still below the 80% gate, so it does not rescue the file's classification. It is a quality fix, not a coverage fix, and it is a behaviour change excluded by the epic's no-behaviour-change NFR. Promote as a standalone issue. | High (textual) |
| **F2** | `IWebViewCoreInitializer.cs:10-11` and `WebView2CoreInitializer.cs:9-13` | Both doc comments claim the adapter "forwards **1:1** to the WebView2 SDK", but `CreateEnvironmentAsync` does not: it drops the SDK's first parameter, passing a hard-coded `null` for `browserExecutableFolder` (`:22`). Callers cannot influence the browser executable folder through this seam and nothing documents that. A documentation/contract accuracy defect, not a runtime defect. | High (textual) |
| **F3** | `WebView2CoreInitializer.cs:15` (and `WebView2BreadcrumbHost.cs:29`, `WebView2Messenger.cs:20`) | The three exemptions cite grounds that do not exist in `CLAUDE.md` §UT2 (§7). Governance defect in the exemption ledger. **In-scope for F1 to ratify, not for promotion as a bug.** | High (textual) |

---

## 11. Deviations from the delegation brief

| # | Brief claim | Finding |
|---|---|---|
| 1 | "`WebView2CoreInitializer.cs` appears genuinely irreducible … Likely a true adapter-tier remainder. Confirm" | **CONFIRMED.** 30 lines, two expression-bodied members, zero branches, zero state, zero guards, three coverable lines. Its doc comment is the only one of the three that matches its code. |
| 2 | Implicit framing that the obstacle is difficulty | **Refined.** The obstacle is not difficulty but **prohibition**: executing `CreateEnvironmentAsync` would create filesystem artifacts, barred outright by `CLAUDE.md` §UT4 ("Currently approved exceptions: none") and `.claude/rules/general-code-change.md`, and both members depend on an external runtime process, barred by §UT4 and non-deterministic under §UT1. This is a stronger argument than "hard to test" and should be the wording in the ledger. |
| 3 | "explain what `WebView2CoreInitializerTests.cs` can possibly assert given the class is exempt and takes SDK types" | **Answered in §5.** It can assert construction, interface assignability, both seam signatures by reflection, adapter/seam member parity, presence of the exemption attribute, and sealedness — six contract assertions contributing **zero** coverage by construction. It cannot assert any behaviour of either member. The epic's shape-assertion prohibition (`epic.md:521-522`) does not apply because it is scoped to the `interface-only / not-measured` bucket, and this file is `ratified-exempt`. |
| 4 | Implicit premise that CLAUDE.md §UT2 supplies the exemption ground | **Refuted.** §UT2's three grounds are VSTO lifecycle, WinForms form-derived/Designer, and Outlook Interop without a seam. This file is none of the three: it derives from nothing, is not Designer-generated, and imports no Outlook type (`using` directives at `:1-4`). §7 proposes a narrow, self-policing fourth ground (d) for F1 to ratify, plus a documented fallback if F1 declines. **This is the single most consequential correction in this artifact** — the plan must not assume §UT2 already covers these files. |
| 5 | (brief §5) Any seam must avoid deepening dependence on F12-owned code | **Satisfied trivially.** This file has no cross-file dependency at all beyond the SDK and its own interface. It references neither `BreadcrumbPopupLifecycleOperations` (`BreadcrumbItemViewerLifecycleCoordinator.cs:355`) nor `BreadcrumbNavigationSubscription` (`:337`), and the recommendation adds no new production file. |
| 6 | (brief §4) Two parallel WebView2 hosting paths | **This file spans both** — it is the one type common to the EfcViewer path (`EfcFormController.cs:838`) and the ItemViewer/QFC path (`QfcItemController.Initialization.cs:381`). That shared status is the load-bearing reason not to fold it into either path's local adapter (§4). |
