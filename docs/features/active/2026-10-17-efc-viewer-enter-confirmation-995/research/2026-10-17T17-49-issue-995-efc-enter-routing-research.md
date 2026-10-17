<!-- markdownlint-disable-file -->

# Task Research Notes: Issue 995 EFC Viewer Enter confirmation and authorized routing repair

## Research Executed

### File Analysis

- `.github/agents/task-researcher.agent.md`: canonical workflow and research template read in full.
- `.agents/skills/execution-environment/SKILL.md` and `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`: read; research belongs in this feature research folder. EVIDENCE_LOCATION_OVERRIDE_REJECTED: artifacts/research/ replaced with docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/research/.
- `artifacts/orchestration/orchestrator-state.json`: read only; receipt `issue-995-research-1` selects native `task-researcher-c3`, `gpt-6.1-sol`, `high`.
- Authority-store runtime attestation referenced by the checkpoint was opened directly: agent `01a14afb-04b5-77e3-9293-6ea2629753b5`; recorded 2026-10-17T17:48:49.7992063+00:00; actual/expected model `gpt-6.1-sol`, effort `high`; `routing_valid: true`, `launch_authority_valid: true`, `provenance_valid: true`. SHA-256 `d6ee92a68bba47e68d1ad163908bacc9d5abe65573218d5c5f5c1eec45ebe235` matches the actual profile file and persisted receipt. Attestation key `10a3059202043dd921cd42ee43f72f20433921691a4945c34981d985f080148b`.

### Code Search Results

- Initial inventory found bridge assets, controller router selection partial, EFC viewer/controller and matching unit test families.

### External Research

Pending authoritative documentation verification.

### Project Conventions

- Research only; production/configuration/checkpoint remain unchanged. Issue 995 is the sole feature identifier.
- Canonical template retained. User authorized autonomous alternative selection.

## Key Discoveries

### Project Structure

Investigation in progress.

### Implementation Patterns

Investigation in progress.

### Complete Examples

Pending source verification.

### API and Schema Documentation

Pending source verification.

### Configuration Examples

Configuration read: C1 gpt-6-luna/low; C2 gpt-6.1-sol/medium; C3 gpt-6.1-sol/high; C4 gpt-6-astra/ultra; C3 elevated gpt-6-astra/high; forced epic personas gpt-6-astra/ultra. Resolver elevated effort is currently max, unlike config high.

### Technical Requirements

Issue acceptance criteria and draft plan read. No acceptance criteria marked complete.

## Recommended Approach

Pending evidence comparison.

## Implementation Guidance

- **Objectives**: Research original EFC defect and all authorized routing branch changes.
- **Key Tasks**: Verify implementation and repository-specific gates.
- **Dependencies**: Source investigation and authoritative documentation.
- **Success Criteria**: Verified findings, selected recommendation, actionable regression/QA strategy; research artifact confirmed on disk.