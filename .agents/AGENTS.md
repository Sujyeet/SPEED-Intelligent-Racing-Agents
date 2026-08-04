# RESEARCH_ML — Agent Rules & Documentation Protocol

## Documentation Rules for Every Change

For every feature, mechanic update, refactor, or bug fix completed in this repository, the agent MUST automatically update/generate two dedicated, professional documentation files in the project workspace:

### 1. `CHANGELOG_MECHANICS.md` (Code and Mechanics Changes)
Documents all gameplay mechanics and code modifications with high precision:
- **What Changed**: File, class, method, and state transitions (exact delta from previous implementation to new implementation).
- **Why**: Game design intent, architecture goal, or feature requirement.
- **How**: The exact technical implementation details, design patterns, and flow.
- **Why Not Alternatives**: Trade-off analysis explaining why alternative technical approaches were rejected in favor of the chosen design.

### 2. `ROOT_CAUSE_RESOLUTIONS.md` (Issues & Root Cause Log)
Documents all bug fixes, runtime crashes, and performance/engine issues:
- **Issue Description**: Concise summary of the error, crash log, warning, or bug.
- **Root Cause Analysis**: In-depth technical breakdown of the exact failure mechanism (e.g. netcode ownership, unspawned states, null event delegates).
- **Resolution Strategy**: Code and system changes made to permanently eliminate the root cause without symptom-patching.
- **Verification Method**: Empirical runtime proof (Unity console logs, playtest verification) confirming resolution.

### 3. `GAME_DEVELOPMENT_BLUEPRINT.md` (Living Production Master Plan)
Whenever the user proposes a new game idea, mechanic, feature, or improvement, the agent MUST automatically update `GAME_DEVELOPMENT_BLUEPRINT.md` in the project root:
- **Feature Title & Description**: Overview of proposed mechanic.
- **Suggested Phase**: Phase placement (Phase 1 through Phase 5).
- **Technical Design**: Class structure, code snippets, or system architecture.
- **Potential Issues & Mitigation**: Expected technical risks and prevention strategy.
- **Trade-off Analysis**: Why this design was selected over alternative implementations.

---
*Tone & Formatting Requirements:*
- No emojis, no long dashes, no AI fluff or filler words.
- Clean, structured, highly professional senior game developer documentation suitable for commercial portfolios, research logs, and PhD reviews.
