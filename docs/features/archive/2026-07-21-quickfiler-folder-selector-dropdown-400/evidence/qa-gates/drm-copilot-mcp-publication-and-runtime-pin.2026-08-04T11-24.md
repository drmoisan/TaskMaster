Timestamp: 2026-08-04T11-24
Command: npm view '@danmoisan/drm-copilot-mcp@1.0.21' version dist.integrity dist.tarball --json; download `dist.tarball` in memory and calculate SHA-512 SRI with Node.js `crypto.createHash('sha512')`
EXIT_CODE: 0
Output Summary: Independent npm registry publication verified for `@danmoisan/drm-copilot-mcp@1.0.21`. The published registry SRI exactly matched the independently downloaded tarball hash, so the conditional activation requirement is satisfied.

## Immutable package identity

- Package: `@danmoisan/drm-copilot-mcp`
- Version: `1.0.21`
- Tarball: `https://registry.npmjs.org/@danmoisan/drm-copilot-mcp/-/drm-copilot-mcp-1.0.21.tgz`
- Registry SRI: `sha512-6Ehxshezrcz7YIPc1PN0xsiDw0uspHFEBW0qnOi2+lXUzId/4GDY9wZ4hP1RYUYQUsX2l7h728XDAHXq55TO7A==`
- Downloaded tarball SRI: `sha512-6Ehxshezrcz7YIPc1PN0xsiDw0uspHFEBW0qnOi2+lXUzId/4GDY9wZ4hP1RYUYQUsX2l7h728XDAHXq55TO7A==`
- Downloaded bytes: `944656`
- Supplied release merge on `origin/main`: `060747bd0d9e5978ab40a3854345e38c2a6d5369`

## Activation

The independent publication condition passed. `.codex/config.toml` now pins the `drm-copilot` MCP server exactly to `@danmoisan/drm-copilot-mcp@1.0.21`.

Command: Fresh `@modelcontextprotocol/sdk` `StdioClientTransport({ command: 'npx.cmd', args: ['-y', '@danmoisan/drm-copilot-mcp@1.0.21'] })` processes, each connected through `Client`, then calling `validate_orchestration_artifacts`.
EXIT_CODE: 0
Output Summary: Every SDK process identified the server as `drmCopilotExtension` version `1.0.21`; the CRLF compatibility plan, original CRLF plan, recovered remediation plan, and canonical checkpoint all validated with `ok: true` and `isError: false`.

- Compatibility smoke: temporary 88-byte CRLF plan with a completed canonical `[x] [P0-T1]` task validated with `ok: true` and `isError: false`.
- Original CRLF plan: `plan.2026-07-21T10-41.md` SHA-256 `1DB61950096431A5CE25A688F725D8FFC5524AFD67F608DB5DD3AF89416162E2`, 141 CRLF sequences, zero lone LF, and zero lone CR; validated with `ok: true` and `isError: false`.
- Recovered remediation plan: `remediation-plan.2026-07-21T21-37.md` validated with `ok: true` and `isError: false`.
- Checkpoint: `artifacts/orchestration/orchestrator-state.json` validated with `require_codex_topology: true` and `require_codex_model_routing: true`, returning `ok: true` and `isError: false`.
- Exception runbook: `runbooks/issue-400-repository-wide-powershell-coverage-exception.runbook.md` SHA-256 `4765D8F5AF24436E3454D4A974C8229C6F275F7136F0EC65586B34BFBCFBD718`; verified the issue scope, mandatory PoshQC gates, focused wrapper coverage at least 90%, no changed-line coverage regression, and byte-for-byte coverage-policy preservation requirements.
