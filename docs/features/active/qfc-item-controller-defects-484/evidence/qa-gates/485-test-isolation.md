# Issue #485 — Regression Tests Construct No Controller, Viewer, Helper, or WebView2 Type

Timestamp: 2026-08-26T09-16
Task: [P2-T13]

Command: inspection of every test method in the #485 group in
`QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` (lines 214-320), plus a search of that
line range for the identifiers `MailController`, `ItemViewer`, `MailItemHelper`, `CoreWebView2`, and
`new QuickFiler`.
EXIT_CODE: 0

Search result: the only line in the range matching any of those identifiers is line 216, which is the
group header **comment** stating the constraint. No code line in the group names any of them.

## Per-method confirmation

| Test method (or `[DataRow]` group) | Constructs a controller? | Constructs an `ItemViewer`? | Constructs a `MailItemHelper`? | Names any `CoreWebView2*` type? |
|---|---|---|---|---|
| `TryResolveCidResource_RejectsUnusableUri_ReturnsFalseWithNullOutputs` (3 rows) | No | No | No | No |
| `TryResolveCidResource_WithNullMap_ReturnsFalse` | No | No | No | No |
| `TryResolveCidResource_WithMapMiss_ReturnsFalse` | No | No | No | No |
| `TryResolveCidResource_WithNullAttachmentData_ReturnsFalse` | No | No | No | No |
| `TryResolveCidResource_WithKnownExtension_ReturnsPayloadAndMimeType` | No | No | No | No |
| `TryResolveCidResource_WithUnrecognisedExtension_ReturnsOctetStream` | No | No | No | No |

## Why this holds

`TryResolveCidResource` is `internal static`, so it is invoked as
`QfcItemController.TryResolveCidResource(...)` with no instance at all;
`[assembly: InternalsVisibleTo("QuickFiler.Test")]`
(`QuickFiler/Properties/AssemblyInfo.cs:5`) makes it reachable from the test assembly.

Every argument is a plain value:

- the requested URI is a `string` literal;
- the content-id map is built by the shared arrange helper
  `QfcItemControllerTestSupport.BuildContentIdMap(contentId, data, extension)`, which constructs a
  `Mock<IAttachment>` and passes it through the real, host-neutral
  `CidImageResolver.BuildContentIdMap`. `IAttachment` is a plain public interface
  (`UtilitiesCS/Interfaces/IEmailIntelligence/IAttachment.cs`) and `CidImageResolver` is documented as
  performing no I/O and having no COM or WebView2 dependency;
- both results are `out` locals of type `byte[]` and `string`.

The SDK response construction — the only part that needs a live WebView2 runtime — stays outside the
tested unit, in the lambda adapter inside the `[ExcludeFromCodeCoverage]` `InitializeWebViewAsync`.

The three helpers used by the group (`AssertRequestIgnored`, `AssertRequestServed`, `MapWith`) are
`private static` members of the test class and likewise construct none of the four forbidden kinds.

Output Summary: All six #485 test methods, including the three `[DataRow]` cases, run without
constructing a controller, an `ItemViewer`, a `MailItemHelper`, or any `CoreWebView2` type.
