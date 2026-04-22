# How I Built This with Spec Kit

This document describes the steps I followed to design and implement the Weather Advisor application using [Spec Kit](https://github.com/github/spec-kit) — a toolkit for Spec-Driven Development (SDD) with AI coding assistants.

---

## 1. Installation

Spec Kit is distributed as a Python CLI tool called `specify`. The recommended way to install it is via [uv](https://docs.astral.sh/uv/), a fast Python package manager.

**Install uv** (Windows):

```powershell
powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"
```

**Verify uv is working:**

```bash
uv --version
```

**Install Spec Kit:**

```bash
uv tool install specify-cli --from git+https://github.com/github/spec-kit.git
```

> Tip: Pin a specific release tag for stability (e.g. `@v0.7.4`). Check the [Releases page](https://github.com/github/spec-kit/releases) for the latest tag.

**Verify Spec Kit is installed:**

```bash
specify version
```

---

## 2. Add to Your Project

**Create a brand-new project:**

```bash
specify init <PROJECT_NAME>
```

This scaffolds a new directory with all the Spec Kit structure already in place.

**Or initialize inside an existing repository** (run from the repo root):

```bash
specify init --here --ai copilot
```

This writes the agent command files and `.specify/` configuration into the current directory without creating a new folder. The `--ai copilot` flag tells Spec Kit to install GitHub Copilot-compatible slash commands under `.github/copilot/`. If you use a different AI coding tool (Cursor, Claude, etc.), replace `copilot` with the appropriate agent name — see the [Supported AI Coding Agent Integrations](https://github.github.io/spec-kit/reference/integrations.html) page for the full list.

For this project I used **VS Code with GitHub Copilot**, so all the commands below are invoked as `/speckit.*` slash commands in the Copilot Chat panel.

---

## 3. Create the Feature

The Spec-Driven workflow moves through a structured sequence of commands. Each one produces or refines an artifact in `specs/<feature-folder>/`. The commands build on each other — do not skip steps, and do not rush past clarification before the spec is precise enough.

### `speckit.constitution`

**What it does:** Creates (or updates) the project's governing principles — coding standards, architectural decisions, testing expectations, and non-negotiable constraints. These principles are referenced by every subsequent command and act as a guardrail for AI-generated content.

**What to provide:** Describe your core non-negotiables in plain language. Think about code quality, testing standards, UX consistency, performance requirements, security policies, and anything else you want enforced throughout the project.

```
/speckit.constitution Create principles focused on: separation of concerns (frontend
must not call external APIs directly), no persistence for demo projects, API-first
contracts before implementation, and 100% unit-testable business logic.
```

**Pitfalls:**
- Being too vague produces toothless principles that the AI will ignore. Be specific.
- Avoid generic buzzwords ("write clean code"). Prefer actionable constraints ("controllers must contain no business logic").
- The constitution can be updated later, but changing it mid-implementation can invalidate earlier artifacts. Invest time here upfront.

---

### `speckit.specify`

**What it does:** Turns your natural-language description into a formal feature specification (`spec.md`). This includes user stories, acceptance scenarios, priority rankings, and functional requirements. Focus on the *what* and the *why*, not the technology.

**What to provide:** Describe the feature from a user perspective. What problem does it solve? Who uses it? What are the key workflows and edge cases?

```
/speckit.specify Build a weather advisor web app that helps users decide whether
planned outdoor activities (Running, Cycling, Picnic, Walking) are suitable based
on current weather for a city they enter. Show temperature, wind, and precipitation.
Give a clear Suitable / Caution / Not Recommended verdict with an explanation.
```

**Pitfalls:**
- Do not mention the tech stack here — that comes in `speckit.plan`. Mixing "what" with "how" degrades spec quality.
- Omitting edge cases (city not found, API timeout, missing fields) leads to gaps that surface late in implementation. Think through failure modes now.
- The spec is the source of truth for all downstream artifacts. Ambiguity here propagates into the plan, tasks, and code.

---

### `speckit.clarify`

**What it does:** Reviews the current spec for underspecified areas and asks up to 5 targeted clarifying questions. Your answers are encoded back into `spec.md`. This is an optional but highly recommended step — run it **before** `speckit.plan`.

**What to provide:** Simply invoke it; the agent identifies ambiguities automatically. Answer each question with as much precision as you can.

```
/speckit.clarify
```

For this project, clarification nailed down the tech stack (React + .NET), units of measurement (metric only), the exact set of supported activities, and the API timeout value — all decisions that would have required disruptive spec revisions later.

**Pitfalls:**
- Skipping clarification is the most common cause of rework. A vague spec produces a vague plan, which produces buggy or incomplete tasks.
- If the agent asks about something you haven't decided yet, make the decision now and commit to it. Deferring decisions to implementation is how scope creep starts.

---

### `speckit.checklist [topic]`

**What it does:** Generates a custom quality checklist that validates the completeness, clarity, and consistency of your spec (or a specific topic). Think of it as "unit tests for English" — it catches missing requirements before any code is written.

**What to provide:** Optionally specify a topic to focus on (e.g., `error handling`, `security`, `accessibility`). Without a topic it reviews the spec broadly.

```
/speckit.checklist error handling
/speckit.checklist
```

**Reviewing the checklist:** Once generated, you can work through it in two ways:
- **Manually** — read each item yourself and decide whether it is satisfied, partially addressed, or missing entirely.
- **Ask the AI** — paste the checklist into the chat and ask Copilot to evaluate each item against the current `spec.md`. It will flag any gaps and suggest what needs to be added.

Either way, fix every identified gap in the spec before moving on. Do not carry unresolved checklist items into planning.

**Pitfalls:**
- Treat checklist findings as actionable. If a requirement is missing, add it to the spec before moving on — do not plan around gaps.
- Running checklist only once at the end is less valuable than running it after `speckit.specify` and again after `speckit.clarify`.

---

### `speckit.plan`

**What it does:** Takes the approved spec and produces a technical implementation plan (`plan.md`). This is where you introduce the tech stack, architecture, key dependencies, and a constitution compliance check. The plan bridges user intent (spec) and actionable tasks.

**What to provide:** Describe your technology choices, architectural constraints, and any non-obvious decisions. Be explicit about libraries, testing frameworks, and infra.

```
/speckit.plan React (TypeScript) SPA frontend + .NET 10 Web API backend. Use
Open-Meteo (free, no API key). Testing: Vitest + React Testing Library for
frontend, xUnit + Moq for backend. No auth, no database. Metric units only.
```

**Pitfalls:**
- Introducing technology that conflicts with the constitution (e.g., adding a database when "no persistence" is a principle) will produce a plan that fails the constitution gate. Resolve the conflict before continuing.
- Vague architecture descriptions produce vague plans. Say "controllers own only HTTP routing; services own business logic" rather than "use a layered architecture".
- The plan should produce a `contracts/api.md` as a Phase 1 output. If it doesn't, explicitly ask for it before running `speckit.tasks` — tasks that reference undefined endpoints will be ambiguous.

---

### `speckit.tasks`

**What it does:** Breaks the implementation plan into a dependency-ordered, actionable task list (`tasks.md`). Each task is scoped to a single coherent unit of work that the AI can execute independently.

**What to provide:** No extra input is normally needed — the agent reads `spec.md` and `plan.md`. If you have specific ordering preferences or want to split/merge phases, mention them.

```
/speckit.tasks
```

**Pitfalls:**
- Run `speckit.analyze` immediately after (see below) to catch inconsistencies before implementation begins.
- Tasks that reference data structures not defined in `data-model.md` or endpoints not in `api.md` will be ambiguous. Make sure those artifacts exist first.
- Do not manually edit `tasks.md` to add or remove large chunks — use `speckit.analyze` findings and the spec to drive revisions, so traceability is maintained.

---

### `speckit.analyze`

**What it does:** Performs a cross-artifact consistency and coverage check across `spec.md`, `plan.md`, and `tasks.md`. It catches misalignments — missing tasks for specified requirements, tasks that have no spec basis, or plan decisions not reflected in tasks — before a single line of code is written.

**What to provide:** No input needed. Run it after `speckit.tasks` and before `speckit.implement`.

```
/speckit.analyze
```

**Pitfalls:**
- Findings are not cosmetic. A finding that says "FR-008 (API timeout handling) has no corresponding task" means that requirement will not be implemented. Fix the gap before proceeding.
- If `speckit.analyze` surfaces many gaps, it usually means the spec or plan needs to be more explicit — go back and refine rather than patching tasks manually.

---

### `speckit.implement`

**What it does:** Executes all tasks in `tasks.md` in dependency order, using the spec, plan, and contracts as authoritative context. This is where the actual code gets written.

**What to provide:** No extra input is needed for a standard run. The agent uses all accumulated artifacts as context.

```
/speckit.implement
```

**Pitfalls:**
- Do not start implementation until `speckit.analyze` produces a clean (or explicitly accepted) result. Implementing against an inconsistent task list produces drift between the spec and the code from day one.
- If implementation gets interrupted, `tasks.md` tracks completion state (`[x]` / `[ ]`). You can resume safely — the agent will pick up incomplete tasks.
- Resist the urge to vibe-code details that "the spec didn't cover". If something is missing, update the spec first (even retroactively), so the artifact trail stays honest.

---

## Further Reading

- [Spec Kit documentation](https://github.github.io/spec-kit/)
- [Complete Spec-Driven Development methodology](https://github.com/github/spec-kit/blob/main/spec-driven.md)
- [Supported AI Coding Agent Integrations](https://github.github.io/spec-kit/reference/integrations.html)
- [Specify CLI Reference](https://github.github.io/spec-kit/reference/overview.html)
- [Community Extensions](https://speckit-community.github.io/extensions/)
