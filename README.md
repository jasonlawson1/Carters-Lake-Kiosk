# Team P06-01 — Carters Lake Kiosk

## Interactive Game Hub and Engagement Analysis

**Sponsor:** U.S. Army Corps of Engineers — Carters Lake Visitor Center  
**Course:** SWE 4724 — Capstone Project, Fall 2026  
**Current phase:** Milestone 2 — Core Platform Development and Integration  
**Milestone 2 submission target:** November 8, 2026  
**Overall project target:** December 2026

## Project Overview

This capstone project develops an offline-first, touchscreen-oriented Windows kiosk application for visitors to the Carters Lake Visitor Center. The kiosk will provide a single place to browse and launch educational games, while offering authorized Rangers tools to manage the available games and review local engagement analytics.

The project integrates existing compiled Unity games. Access to their original source code and internal score/completion events is not guaranteed, so the launcher must not assume that a Unity process closing means the visitor completed a game.

## Planned Visitor Experience

1. Browse the available educational games on the touchscreen.
2. Select a game, view its details, and choose **Play**.
3. Play the externally launched Unity game.
4. Return to the launcher when the game process ends.
5. **After** the session, optionally provide an age category and name, or skip the prompt.
6. See a brief thank-you screen and return to the home/game-selection screen.

The post-game information must be associated with the session that just ended, and temporary visitor input must be cleared before the next visitor. Collection, storage, retention, and display of visitor names require an approved privacy decision. No pre-game age/name prompt is part of the current planned flow.

## Technology and Operating Environment

- **Application:** C# / .NET 10, Windows Presentation Foundation (WPF)
- **Games:** Existing compiled Unity Windows applications
- **Local persistence:** SQLite (`Microsoft.Data.Sqlite`)
- **Game metadata in current prototype:** `installedGames.json` generated/used by the UI
- **Target environment:** Windows-based Elo touchscreen kiosk; offline operation is a primary requirement
- **Collaboration:** GitHub for code and pull requests; Trello for milestone tasks and blockers

The checked-in project targets `net10.0-windows`. The final kiosk's runtime compatibility, deployment permissions, and security configuration still need validation on the sponsor's device.

## Current Implementation Snapshot

The following reflects the source files on `main` reviewed in October 2026, **not a verified working release**. Teammates may have additional work on local or unmerged branches.

| Area | Current repository state | Remaining work |
| --- | --- | --- |
| Visitor-facing UI | WPF windows for the main screen and game selection; game artwork and navigation code | Polish, dynamic registry integration, post-game screens, and touchscreen testing |
| Unity launch | Game-selection code includes process-launch logic | Confirm real builds, detect process exit, restore launcher, and handle failures |
| Game registry | UI reads/writes `installedGames.json`; SQLite also defines a `games` table | Agree on authoritative registry and synchronize Ranger/visitor views |
| Ranger tools | Ranger UI and preliminary authentication/game-management code | Sponsor-approved security, permissions, validation, and integration |
| SQLite | `DatabaseService` creates initial local tables | Design and implement complete game-session lifecycle and optional post-game fields |
| Session tracking | `SessionService` exists as a placeholder | Start/end/failure records, duration, restart recovery, and session association |
| Analytics | Early data structures only | Queries, Ranger dashboard, and evidence-based reporting |
| Scoreboards | Early/placeholder views | Depends on available Unity score signals and approved scope |

**Known integration risks:** The current Ranger authentication prototype contains a hardcoded password and must not be treated as production security. Unity process exit alone does not prove completion, and elapsed runtime alone does not prove inactivity. The JSON and SQLite game registries are not yet unified.

## Team and Responsibilities

| Member | Primary ownership |
| --- | --- |
| **Kacy Watkins** | Project lead; architecture, component interfaces, integration, sponsor coordination, and consolidated documentation |
| **Reese Mothershed** | Visitor UI, WPF launcher, dynamic game selection, and post-game screens |
| **Luke Miller** | Unity build audit, launch/return integration, outcome feasibility, and game integration tests |
| **Jason Lawson** | SQLite schema, session lifecycle persistence, analytics queries, and chart data |
| **Jay Jalla** | Ranger authentication/access, game management, Ranger dashboard, and admin workflow tests |

Ownership establishes accountability, not exclusive permission to contribute. Interfaces and integration changes should be coordinated with affected component owners.

## Repository Structure

```text
Carters-Lake-Kiosk/
├── App.xaml / App.xaml.cs       # WPF application entry and setup
├── Capstone UI.csproj           # .NET / WPF project
├── Capstone UI.slnx             # Solution
├── Views/                       # Visitor, Ranger, and leaderboard screens
├── Models/                      # Game and preliminary admin models
├── Services/                    # Database, session, and keyboard helpers
├── Images/                      # UI artwork and game screenshots
├── docs/                        # Project and development documentation
├── .github/                     # Pull request template
├── .gitignore
└── README.md
```

The UI may create `installedGames.json` at runtime. Compiled Unity game folders are not included in the supplied source ZIP; paths in the prototype must be verified against the local game installation before launching.

## Getting Started (Development PC)

**Prerequisites:** Windows, a .NET 10 SDK capable of building WPF applications, and Visual Studio or another compatible .NET development environment.

1. Clone the repository and open `Capstone UI.slnx` in a compatible IDE.
2. Restore the project's NuGet packages.
3. Build the WPF application. From PowerShell in the repository root, you can try:

   ```powershell
   dotnet restore "Capstone UI.csproj"
   dotnet build "Capstone UI.csproj"
   dotnet run --project "Capstone UI.csproj"
   ```

4. If testing actual games, obtain the sponsor-provided Unity builds through an approved team channel and verify the configured executable paths.
5. Test on a development PC first. A successful desktop run does **not** establish successful operation on the physical kiosk.

These are intended development commands based on the checked-in project configuration; they have not been validated in this README update. The app currently creates its SQLite file under the Windows user's local application-data folder (`CapstoneUI/gamehub.db`).

## Milestone 2 Integration Goal

The first integrated vertical slice should demonstrate:

1. A registered game appears in the WPF launcher.
2. Selecting **Play** launches an actual supplied Unity executable.
3. The launcher detects process exit or launch failure and returns safely.
4. A game-session record captures the correct game, timestamps, duration, and only outcomes supported by observable evidence.
5. The visitor can optionally enter age category and name **after** the game or skip the prompt.
6. Any submitted optional information is associated with the just-ended session; temporary input is cleared before the next session.
7. The team records executed tests and known limitations.

For outcomes, use **Unknown** where the Unity executable does not expose reliable completion or early-exit signals. Inactivity and timeout reporting require independently verifiable detection.

## Development Workflow

1. Confirm task ownership and acceptance criteria in Trello.
2. Create a focused feature or fix branch from the latest appropriate base.
3. Implement the change and run relevant tests.
4. Commit with a descriptive message and push the branch to GitHub.
5. Open a pull request using `.github/pull_request_template.md`.
6. Request review from another teammate, particularly for changes to shared interfaces.
7. Resolve feedback and merge approved work into `main`.
8. Update Trello and provide diagrams, screenshots, or test evidence where appropriate.

`main` should represent reviewed, integrated work. Work on local or unmerged branches should be disclosed at team meetings so integration status is visible. Additional workflow guidance is in [`docs/development-workflow.md`](docs/development-workflow.md).

## Testing and Documentation

Component owners should provide reproducible tests with **steps, expected results, actual results, and environment**, along with relevant screenshots and design notes for the Software Design Document (SDD). Prioritize launch/return, database persistence, consecutive visitor sessions, Ranger access control, and error recovery. Track defects and retest results in Trello.

The final SDD should distinguish **implemented behavior**, **proposed design**, **known limitations**, and **pending sponsor decisions**. Physical kiosk testing must be documented separately from development-PC testing.

## Security and Privacy

- Do not commit passwords, tokens, secrets, restricted sponsor files, or real visitor data.
- The existing hardcoded Ranger password is a **prototype limitation requiring remediation**, not an approved authentication solution.
- Ranger access and device lockdown must follow sponsor/Army security requirements; do not assume an approach is approved until confirmed.
- Collect only approved visitor information. Age category and name are optional **post-game** inputs in the current plan.
- Do not assume permission to display names publicly or use them on a leaderboard.
- Keep the application functional offline; no remote reporting or USB export is assumed approved.

## Open Decisions and Dependencies

- Choose a single authoritative game registry and a contract for launcher/Ranger updates.
- Agree on session IDs, event interfaces, optional post-game fields, and database schema.
- Confirm how compiled Unity games can be exited and which outcomes can be detected reliably.
- Confirm approved Ranger authentication, name-data retention, and leaderboard/score feasibility with the sponsor.
- Validate .NET runtime, input behavior, game executables, and kiosk deployment on the physical device.

## Sponsor and Academic Context

Developed for the **U.S. Army Corps of Engineers — Carters Lake Visitor Center** as a **Kennesaw State University SWE 4724 Capstone Project (Fall 2026)**.
