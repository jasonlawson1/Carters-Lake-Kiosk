# Development Workflow

This document defines the Git and GitHub conventions used by the
Carters Lake Kiosk development team. These conventions are intended
to keep repository activity consistent, traceable, and easy to review.

## Branch Naming

Branches should use lowercase kebab-case and begin with a prefix
describing the type of work being performed.

### Branch Types

| Prefix | Purpose | Example |
|---|---|---|
| `feature/` | New functionality | `feature/game-launcher` |
| `fix/` | Bug fixes | `fix/launcher-black-screen` |
| `docs/` | Documentation changes | `docs/requirements-update` |
| `test/` | Test-related work | `test/scorekeeping-service` |
| `refactor/` | Code restructuring without changing intended behavior | `refactor/analytics-service` |
| `chore/` | Repository, configuration, or maintenance work | `chore/update-gitignore` |

Branch names should clearly describe the work being performed.

Examples:

`feature/visitor-game-selection`

`feature/admin-game-management`

`fix/score-persistence`

`docs/system-architecture`

Avoid vague branch names such as:

`my-branch`

`updates`

`new-stuff`

`test1`

## Commit Messages

Commit messages should use the following format:

`type: short description`

### Commit Types

| Type | Purpose |
|---|---|
| `feat` | Adds or changes functionality |
| `fix` | Corrects a defect |
| `docs` | Documentation-only change |
| `test` | Adds or modifies tests |
| `refactor` | Restructures code without changing intended behavior |
| `chore` | Repository, configuration, or maintenance work |

Examples:

`feat: add visitor game selection screen`

`fix: return to launcher after game closes`

`docs: add initial architecture diagrams`

`test: add score persistence tests`

`refactor: separate analytics collection logic`

`chore: configure repository settings`

Commit messages should briefly describe the change made. Avoid vague
messages such as "updates," "changes," "stuff," or "fixed it."

## Pull Request Workflow

Development work should normally follow this process:

1. Begin with the current `main` branch.
2. Create a branch using the team's branch naming convention.
3. Make changes and create logical commits.
4. Push the working branch to GitHub.
5. Open a pull request into `main`.
6. Complete the repository pull request template.
7. Assign at least one team member other than the author to review the PR.
8. Address review comments or requested changes.
9. Obtain at least one approval before merging.
10. Merge the approved pull request into `main`.
11. Delete the merged branch when it is no longer needed.
12. Update the associated Trello card as appropriate.

Team members should not approve their own pull requests.

## Traceability

When applicable, development work should be traceable between project
artifacts.

The intended relationship is:

Sponsor Need → Requirement → Trello Card → Branch/Code → Pull Request → Test

Pull requests should reference the associated Trello card and applicable
requirement IDs when those IDs are available.

Example:

Requirement: `FR-VIS-03`

Branch: `feature/visitor-game-selection`

Pull Request:
- Trello Card: Touchscreen Visitor Interface
- Requirement ID: FR-VIS-03

## Main Branch

The `main` branch represents the team's integrated codebase.

Once repository permissions allow branch protection to be configured,
the team intends to require pull requests and peer review before normal
development changes are merged into `main`.

Direct commits to `main` should be avoided after the initial repository
setup is complete.

## Repository Security

Credentials, passwords, access tokens, private keys, and other sensitive
configuration information must not be committed to the repository.

Security and access-control implementation decisions for the kiosk
system will be documented separately as sponsor and operational
requirements are clarified.
