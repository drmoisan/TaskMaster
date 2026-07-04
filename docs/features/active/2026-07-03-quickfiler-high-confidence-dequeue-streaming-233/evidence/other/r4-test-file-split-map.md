Timestamp: 2026-07-03T22-02-04:00
Command: Inspect QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs for issue #233 high-confidence tests and movable helpers.
EXIT_CODE: 0
Output Summary: Identified the issue #233 high-confidence RunAsync startup tests and focused helper block that can move to a separate MSTest class without changing test behavior.

Source File:
- QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs

Baseline Size:
- Current file size before split: 621 lines.

Move Plan:
- Create `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs`.
- Move the following focused helper from `QfcHomeControllerRunAsyncTests`:
  - `ArrangeRunAsyncController`
- Move the following issue #233 high-confidence startup tests:
  - `HighConfidencePreFilterLoader_CanBeOverridden_ForTesting`
  - `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue`
  - `RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload`
  - `RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly`

Retention Plan:
- Keep baseline synchronous and general Run/RunAsync tests in `QfcHomeControllerRunAsyncTests`.
- Keep `Run_HighConfidenceEnabled_DoesNotLoadUnfilteredInitialBatch` in the existing file because it covers synchronous `Run()` behavior and does not depend on the focused async helper block.

Targeted Test Method Names for P2-T3:
- `HighConfidencePreFilterLoader_CanBeOverridden_ForTesting`
- `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue`
- `RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload`
- `RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly`
