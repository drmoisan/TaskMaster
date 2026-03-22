---
name: make-skill-template
description: 'Create new Claude skills from prompts or by duplicating this template. Use when asked to "create a skill", "make a new skill", "scaffold a skill", or when building specialized AI capabilities with bundled resources. Generates SKILL.md files with proper frontmatter, directory structure, and optional scripts/references/assets folders.'
user-invocable: true
allowed-tools: Read, Write, Edit, Bash, Grep, Glob
---

# Make Skill Template

A meta-skill for creating new Claude Skills. Use this skill when you need to scaffold a new skill folder, generate a SKILL.md file, or help users understand the Claude Skills specification.

## When to Use This Skill

- User asks to "create a skill", "make a new skill", or "scaffold a skill"
- User wants to add a specialized capability to their Claude setup
- User needs help structuring a skill with bundled resources
- User wants to duplicate this template as a starting point

## Prerequisites

- Understanding of what the skill should accomplish
- A clear, keyword-rich description of capabilities and triggers
- Knowledge of any bundled resources needed (scripts, references, assets, templates)

## Creating a New Skill

### Step 1: Create the Skill Directory

Create a new folder with a lowercase, hyphenated name under `.claude/skills/`:

```
.claude/skills/<skill-name>/
└── SKILL.md          # Required
```

### Step 2: Generate SKILL.md with Frontmatter

Every skill requires YAML frontmatter with `name` and `description`:

```yaml
---
name: <skill-name>
description: '<What it does>. Use when <specific triggers, scenarios, keywords users might say>.'
user-invocable: true           # false = background-only; true = appears in / menu
disable-model-invocation: true # true = only user can invoke; false = Claude auto-invokes too
model: sonnet                  # opus, sonnet, or haiku (optional override)
allowed-tools: Read, Write, Edit, Bash, Grep, Glob, Agent  # optional
---
```

#### Frontmatter Fields

| Field | Required | Description |
|-------|----------|-------------|
| `name` | **Yes** | Lowercase letters/numbers/hyphens; must match folder name |
| `description` | **Yes** | Describes WHAT it does AND WHEN to use it; drives auto-discovery |
| `user-invocable` | No | `false` = background knowledge only; `true` (default) = appears in `/` menu |
| `disable-model-invocation` | No | `true` = only user can invoke via `/name`; Claude won't auto-trigger |
| `model` | No | Override model: `opus`, `sonnet`, or `haiku` |
| `allowed-tools` | No | Tools pre-approved when this skill is active |
| `argument-hint` | No | Hint for autocomplete (e.g., `[issue-number]`) |
| `context` | No | `fork` = run in isolated subagent |
| `agent` | No | Which subagent type with `context: fork`: `Explore`, `Plan`, `general-purpose` |

#### Description Best Practices

**CRITICAL**: The `description` is the PRIMARY mechanism for automatic skill discovery. Include:
1. **WHAT** the skill does (capabilities)
2. **WHEN** to use it (triggers, scenarios, file types)
3. **Keywords** users might mention in prompts

**Good example:**
```yaml
description: 'Toolkit for reviewing C# changes. Use when asked to review, audit, or verify C# implementation quality, test coverage, or policy compliance.'
```

**Poor example:**
```yaml
description: 'C# review helpers'
```

### Step 3: Write the Skill Body

After the frontmatter, add markdown instructions. Recommended sections:

| Section | Purpose |
|---------|---------|
| `## When to Use This Skill` | Reinforces description triggers |
| `## Prerequisites` | Required tools, dependencies |
| `## Step-by-Step Workflows` | Numbered steps for tasks |
| `## Constraints` | What the skill must not do |
| `## Output Format` | Expected outputs |

### Step 4: Add Optional Directories (If Needed)

| Folder | Purpose | When to Use |
|--------|---------|-------------|
| `references/` | Documentation Claude reads | API references, schemas, guides |
| `templates/` | Starter files Claude modifies | Scaffolds to extend |
| `scripts/` | Executable code | Automation that performs operations |

## Skill Behavior Modes

| Configuration | User invokes | Claude auto-invokes |
|---|---|---|
| Default | Yes | Yes (based on description) |
| `disable-model-invocation: true` | Yes | No |
| `user-invocable: false` | No | Yes (background knowledge) |

## Validation Checklist

- [ ] Folder name is lowercase with hyphens under `.claude/skills/`
- [ ] `name` field matches folder name exactly
- [ ] `description` explains WHAT and WHEN (10-1024 characters)
- [ ] `description` is wrapped in single quotes
- [ ] Body content describes clear instructions

## Quick Start: Duplicate This Template

1. Copy the `make-skill-template/` folder to `.claude/skills/<your-skill-name>/`
2. Rename to your skill name (lowercase, hyphens)
3. Update `SKILL.md`:
   - Change `name:` to match folder name
   - Write a keyword-rich `description:`
   - Replace body content with your instructions
4. Add bundled resources as needed
