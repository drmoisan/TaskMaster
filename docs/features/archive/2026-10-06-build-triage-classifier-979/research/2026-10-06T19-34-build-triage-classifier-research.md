<!-- markdownlint-disable-file -->

# Task Research Notes: Build Triage Classifier

## Research Executed

### File Analysis

- `IItemInfo`, `MinedMailInfo`, and mined-mail staging mappings
  - `IItemInfo` includes the Triage field, while `MinedMailInfo` currently neither declares the nullable field nor copies it during staging load.
- Triage classifier and ribbon integration code
  - The classifier uses the A/B/C class contract. Its rebuild path establishes aggregate counts and the token base before classification data is usable. Existing ribbon controller, viewer, and XML patterns provide the menu integration points.

### Code Search Results

- `RebuildClassifier`
  - The existing rebuild operation supplies aggregate-count and token-base initialization that the focused Triage rebuild must reuse.
- `Triage`
  - Triage is present at the item-information boundary but absent from the mined-mail model and copy mappings.

### Project Conventions

- Standards referenced: repository C# and unit-test policy, including MSTest, Moq, FluentAssertions, CSharpier, analyzer build, nullable build, and VSTest coverage.
- Instructions followed: the requested research scope was limited to verified current-state findings supplied for Issue #979.

## Key Discoveries

### Implementation Patterns

`MinedMailInfo` is the persisted/reusable representation needed to rebuild the classifier. It requires a nullable Triage property and all existing creation/copy/staging mappings must carry that value from `IItemInfo`. The rebuild must consume these mined records through a focused Triage classifier API, preserving the classifier's existing A/B/C interpretation rather than deriving another class scheme.

### Behavior Semantics

- A missing Triage value remains nullable and must not be converted into an A, B, or C value during copying or rebuild input preparation.
- Rebuild processes available Triage-labelled mined records according to the existing A/B/C contract.
- Aggregate class counts and token-base setup occur before the rebuilt classifier is made available.
- The user command is located at `TaskMaster -> Settings -> Folder Classifier -> Build Triage Classifier`.

### Requirements Mapping

The change requires: the `MinedMailInfo` nullable property; all construction and staging-load mappings; a focused classifier rebuild entry point that consumes `MinedMailInfo`; and ribbon controller, viewer, and XML wiring for the named command. This preserves the current classifier rebuild responsibilities while making mined mail sufficient input for reconstruction.

## Candidate Approaches

1. Add a dedicated Triage rebuild API that accepts mined mail records, filters nullable labels, initializes aggregate counts/token base, and delegates to the existing classifier mechanisms.
2. Convert mined records back into an item-information shape and invoke the general rebuild workflow.

## Recommended Approach

Implement the dedicated `MinedMailInfo`-based Triage rebuild API. It keeps Triage data in its native mined-mail representation, makes null-label handling explicit, reuses the established aggregate-count/token-base setup, and avoids a lossy or artificial conversion back to an item-information type.

### Rejected Alternatives

Adapting mined mail back to `IItemInfo` adds a conversion layer and obscures whether a missing Triage label is intentionally excluded. It provides no identified benefit over a focused API.

## Testing Implications

- Add unit coverage that `MinedMailInfo` declares nullable Triage and each mined staging/load copy path preserves A, B, C, and null values.
- Add classifier tests covering rebuild from mined mail, A/B/C aggregate counts, token-base initialization, and exclusion of null labels.
- Add focused command tests for the ribbon controller/viewer command path and XML/menu placement at the requested Settings > Folder Classifier location.
- Run the required C# toolchain: CSharpier, analyzer build, nullable build, and VSTest with coverage.
