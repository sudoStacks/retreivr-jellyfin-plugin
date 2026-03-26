# Changelog

All notable changes to this project will be documented here.

## v0.1.5 — Configuration Persistence + Live Plugin Wiring

### High-Level
This release stabilizes the first real Jellyfin testing path. The plugin now uses its own configuration endpoints, resolves backend clients against live plugin configuration instead of startup snapshots, and preserves the in-plugin search and acquisition surface for production testing against the latest Retreivr core.

### Added
- Plugin-owned configuration provider so runtime clients always read the latest saved settings.
- Plugin API endpoints for explicit config load/save and backend status checks.
- Main-menu and user-menu page exposure for the Retreivr plugin inside Jellyfin.

### Fixed
- Settings persistence no longer depends on Jellyfin’s generic plugin-config round-trip.
- Resolution API and Retreivr Core clients now read live configuration instead of stale startup-time values.
- In-plugin search, album drill-in, and queue actions are aligned with the current Retreivr API surface for coordinated testing.
