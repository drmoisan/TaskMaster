# Issue #995 MCP Package Pin Repair

Timestamp: 2026-10-17T22-40
Command: npm view @danmoisan/drm-copilot-mcp@1.1.21 version dist.tarball dist.integrity --json
EXIT_CODE: 0
Output Summary: Release 1.1.21 exists. Its published executable uses gpt-6.1-sol for C2/C3 routing. The repository MCP pin was updated from 1.1.20 to 1.1.21; TOML parsing and exact launch-argument checks pass.

## Authorized Scope and Plan

The user authorized the workflow repair with “Do it”. The delegated prerequisite consists of verifying published release 1.1.21, changing only the MCP version pin in `.codex/config.toml`, parsing the configuration, and writing this evidence. This is independent of execution of the EFC draft plan. No EFC production code, canonical plan, checkpoint, npm cache, or MCP server source was modified by this repair.

Phase: `S3_authorized_dependency_update`
Delegation ID: `issue-995-mcp-upgrade-1`

## Launch Attestation Verified Before Mutation

- Agent ID: `01a14c04-df85-7aa2-9eee-010cfc6d6b62`
- Session ID: `01a14bf0-40df-7783-80f8-860b69d2e605`
- Attestation key: `85aa6dad7c73f7afa2374e608cdbca6755ee4368ea98332af53a8dbb2977f4a3`
- Agent/profile: `atomic-executor-c3`
- Actual and expected model: `gpt-6.1-sol`
- Actual and expected reasoning effort: `high`
- Profile path: `.codex/agents/atomic-executor-c3.toml`
- Profile SHA-256: `2460c2bdf20fa8b88136c16492ee3a810441f60be697432b1975882e8832d46a`
- `routing_valid`, `launch_authority_valid`, and `provenance_valid`: `true`
- Attestation recorded at: `2026-10-17T22:39:12.8652430+00:00`
- Persisted routing receipt matched this profile, model, effort, and profile hash.

## Published Package Verification

Registry command: `npm view @danmoisan/drm-copilot-mcp@1.1.21 version dist.tarball dist.integrity --json`
Observed exit code: `0`
Version: `1.1.21`
Tarball: `https://registry.npmjs.org/@danmoisan/drm-copilot-mcp/-/drm-copilot-mcp-1.1.21.tgz`
Integrity: `sha512-kppfQEt1G7GlRqXwW2WSHki5+J5WHnWTw2+BcV5/uS+xqKUErCPgCyf1t6igy6ILu7FWUUa8kKgmDYPCtuF/cQ==`

The tarball was fetched and inspected in memory, without writing package files or modifying npm cache. The package bundles the routing module into `package/out/mcp-server.js`; it does not contain a separate `package/out/lib/validate/orchestrator-state-codex-model-routing.js` file. Its source marker is `// ../../extensions/drm-copilot/src/lib/validate/orchestrator-state-codex-model-routing.ts`.

Verification command:

```powershell
python -c "import base64,hashlib,io,tarfile,urllib.request; data=urllib.request.urlopen('https://registry.npmjs.org/@danmoisan/drm-copilot-mcp/-/drm-copilot-mcp-1.1.21.tgz').read(); digest='sha512-'+base64.b64encode(hashlib.sha512(data).digest()).decode(); assert digest=='sha512-kppfQEt1G7GlRqXwW2WSHki5+J5WHnWTw2+BcV5/uS+xqKUErCPgCyf1t6igy6ILu7FWUUa8kKgmDYPCtuF/cQ=='; archive=tarfile.open(fileobj=io.BytesIO(data),mode='r:gz'); source=archive.extractfile('package/out/mcp-server.js').read().decode(); index=source.index('var BASE_PROFILES ='); before=source.rfind('\n// ',0,index); end=source.index('var C3_ELEVATED_PROFILE =',index); routing=source[index:end]; assert routing.count('gpt-6.1-sol')==2; assert 'gpt-5.6-terra' not in source; print('Integrity verified: '+digest); print('Bundled module marker: '+source[before:source.index('\n',before+1)].strip()); print(routing); print('No gpt-5.6-terra references in published executable')"
```

Observed exit code: `0`
Output: integrity matches registry metadata; C2 uses `gpt-6.1-sol` with `medium` effort; C3 uses `gpt-6.1-sol` with `high` effort; no `gpt-5.6-terra` references appear in the executable.

Initial read-only inspection attempts exited `1` because they assumed a separate module file and then an incorrect source-marker prefix. Package-member enumeration and source inspection identified the bundled executable and its correct marker; the final check above passed.

## Exact Configuration Change

File: `.codex/config.toml`

Before:

```toml
args = ["-y", "@danmoisan/drm-copilot-mcp@1.1.20"]
```

After:

```toml
args = ["-y", "@danmoisan/drm-copilot-mcp@1.1.21"]
```

## Configuration Validation

Command:

```powershell
python -c "import pathlib,tomllib; path=pathlib.Path('.codex/config.toml'); config=tomllib.loads(path.read_text()); server=config['mcp_servers']['drm-copilot']; assert server['command']=='npx'; assert server['args']==['-y','@danmoisan/drm-copilot-mcp@1.1.21']; assert server['required'] is True; print('TOML parse PASS; required MCP command and args verified')"
```

Observed exit code: `0`
Output: `TOML parse PASS; required MCP command and args verified`

Command: `git diff --check -- .codex/config.toml`
Observed exit code: `0`
Output: no whitespace errors; Git reported its existing LF-to-CRLF working-copy warning.

Command: `git diff -- .codex/config.toml`
Observed exit code: `0`
Output: one-line version-pin substitution only.

No language source files were changed by this prerequisite; no language formatter, linter, compiler, or unit-test suite was run for it. The parent must reconnect or launch a fresh MCP-capable worker and verify the live validator before approving or executing the EFC plan. Existing server connections can retain 1.1.20.

## Repair Checklist

- [x] Verify current launch attestation and persisted routing receipt.
- [x] Verify published 1.1.21 release and tarball integrity.
- [x] Verify C2/C3 model routing in the published executable.
- [x] Update only the repository MCP package pin.
- [x] Parse TOML, assert launch arguments, and check whitespace/diff.
- [x] Record canonical evidence for issue #995.

REPAIR_STATUS: PASS
