Timestamp: 2026-08-25T12-47
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue609_" /InIsolation
EXIT_CODE: 0
Output Summary: All four Issue #609 tests passed: direct row selection, ancestor activation, immediate-child activation, and EmailFilerConfig single-prefix construction.

# Fail-before exception dossier

WhyFailingRunImpossible: The added deterministic regression tests pass before any production edit because `BreadcrumbBridgeRouter` already constructs the full hierarchy value only for `ResolveLeafKeyAsync` and converts hierarchy selection values back to the archive-relative filing target. `EmailFilerConfig` already prefixes the archive root exactly once when its supplied stem is archive-relative.

## Alternative proof

The successful direct-row, ancestor, and immediate-child tests prove the router boundary with `\\mailbox@example.com\Archive` and `Clients\North`; the configuration test proves the single-root output `\\mailbox@example.com\Archive\Clients\North` and `C:\Mail\Clients\North`. No production change is required under P2-T1.
