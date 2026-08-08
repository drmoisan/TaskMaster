# iwebviewcoreinitializer-contract-defects (Issue #477)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/iwebviewcoreinitializer-contract-defects/ (Issue #477)
- Work Mode: full-bug
- Discovered during: preparation research for issue #455 (epic #136, child F13)

- Issue: #477
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/477
- Last Updated: 2026-08-08
## Summary

`IWebViewCoreInitializer` documents itself as a 1:1 forward to the WebView2 SDK, and its
implementation `WebView2CoreInitializer` is coverage-exempt on that basis. Neither claim survives
reading the code: the implementation silently hard-codes an SDK parameter, and it validates none of
its four arguments. Because the type is `[ExcludeFromCodeCoverage]`, no test exercises either
problem.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in
- Package: `Microsoft.Web.WebView2` 1.0.4129.50 (`QuickFiler/packages.config:29`)

## Defect 1 — the "1:1" contract is false; a parameter is silently hard-coded

`QuickFiler/Viewers/WebView2CoreInitializer.cs:19-22`:

```csharp
public Task<CoreWebView2Environment> CreateEnvironmentAsync(
    string cacheFolder,
    CoreWebView2EnvironmentOptions options
) => CoreWebView2Environment.CreateAsync(null, cacheFolder, options);
```

The SDK signature is `CreateAsync(string browserExecutableFolder, string userDataFolder,
CoreWebView2EnvironmentOptions options)`. The seam drops `browserExecutableFolder` and passes
`null` unconditionally, pinning every caller to the Evergreen runtime with no way to select a
fixed-version distribution.

That is a real product constraint expressed as an undocumented literal. The interface doc at
`QuickFiler/Viewers/IWebViewCoreInitializer.cs:10-11` describes the member as a 1:1 forward, which
it is not. The same doc claim is what the coverage exemption at `WebView2CoreInitializer.cs:15`
rests on.

## Defect 2 — no argument validation on any parameter

Neither member validates anything:

```csharp
public Task<CoreWebView2Environment> CreateEnvironmentAsync(string cacheFolder, CoreWebView2EnvironmentOptions options)
    => CoreWebView2Environment.CreateAsync(null, cacheFolder, options);          // :22

public Task EnsureCoreWebView2Async(WebView2 control, CoreWebView2Environment environment)
    => control.EnsureCoreWebView2Async(environment);                             // :28
```

A null `control` at `:28` produces a bare `NullReferenceException` with no parameter name. A null or
empty `cacheFolder` at `:22` is forwarded to the SDK, which surfaces a less specific failure than a
guard would. Every other seam in this area guards its arguments — `WebView2Messenger.cs:38-39`,
`WebView2BreadcrumbHost.cs:45-46`, `BreadcrumbPopupUiOperations.cs:71-77` — so this file is the
outlier, not the convention.

`CLAUDE.md` §C#4 requires validating constructor and method preconditions and failing fast with
explicit exceptions.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Defect 1's impact is latent but architectural: the repository cannot adopt a fixed-version WebView2
distribution without editing this file, and nothing documents that. Defect 2's impact is
diagnostic — failures surface as `NullReferenceException` without a parameter name, which is harder
to triage from a production log.

Severity is Medium because neither misbehaves in the current happy path.

## Note on the coverage exemption

The exemption on `WebView2CoreInitializer` is otherwise **justified** and should be retained: both
members require the Evergreen runtime (an external process) and `CreateEnvironmentAsync` creates a
user-data folder on disk. Executing either in a unit test is prohibited by `CLAUDE.md` §UT4
("Creation and use of temporary files on the local filesystem is expressly prohibited... approved
exceptions: none") and by the ban on external dependencies — not merely difficult. That is the
correct reason to exempt it.

But the exemption's *stated* rationale ("1:1 forwarding") is the part that is false, and the
exemption is what has kept both defects unexamined. Adding the guards in Defect 2 does not make the
type testable and does not change the exemption verdict.

## Suggested Remediation

1. Add explicit `ArgumentNullException` / `ArgumentException` guards to both members, matching the
   convention already used by the sibling seams.
2. Either surface `browserExecutableFolder` as a parameter on `IWebViewCoreInitializer`, or document
   the `null` as a deliberate Evergreen-only decision in the interface XML doc.
3. Correct the "1:1 forward" wording in `IWebViewCoreInitializer.cs:10-11` and in the exemption
   rationale at `WebView2CoreInitializer.cs:8-14`, restating the exemption on the accurate ground
   (external runtime plus filesystem side effect).

## Why this is not fixed under epic #136

Epic #136 child F13 (issue #455) carries a hard no-behavior-change NFR, and adding throwing guards
changes observable behavior on a null argument. Item 3 is doc-only and may be folded into F13's own
change if the ledger rationale is being rewritten there anyway.

## Related

- Issue #455 — F13, breadcrumb drop-down and WebView2 host coverage (where this was found).
- Issue #432 — F1 coverage ledger; the ratified rationale for this exemption should use the accurate
  ground, not the "1:1 forward" claim.
- Issue #136 — parent epic.

## Next Step

- [ ] Promote to GitHub issue
- [ ] Confirm whether fixed-version WebView2 distribution is a product requirement
