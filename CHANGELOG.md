# Changelog

All notable changes to this project will be documented here.

## v0.1.10 — Jellyfin-Native Config Requests

### High-Level
This release switches the Retreivr dashboard page to Jellyfin-native authenticated API requests and jQuery Mobile page lifecycle hooks. The goal is to make config load/save survive reloads reliably inside the Jellyfin dashboard SPA instead of only appearing to change in the DOM.

### Changed
- Plugin release metadata and packaging are now aligned to `v0.1.10`.

### Fixed
- Plugin API requests now use Jellyfin `ApiClient.ajax(...)` and `ApiClient.getUrl(...)` instead of relying on raw unauthenticated `fetch(...)`.
- The config page now binds refresh behavior through Jellyfin’s `pageshow` lifecycle.
- The controller is explicitly elevation-protected and logs config GET/POST activity for debugging.
- The settings layout now uses stacked labels/inputs instead of the cramped inline grid.

## v0.1.9 — Direct Config Action Fallbacks

### High-Level
This release removes the last dependency on native form submission and fragile page-bound event hooks for the Jellyfin dashboard page. Core actions now have direct callable fallbacks so save, refresh, open-UI, and search actions can still execute even when Jellyfin’s SPA lifecycle interferes with normal event binding.

### Changed
- Plugin release metadata and packaging are now aligned to `v0.1.9`.

### Fixed
- `Save Settings` no longer depends on HTML form submission behavior.
- `Open Retreivr UI`, `Refresh Status`, `Search`, and `Back` now have direct page-action fallbacks exposed on `window`.
- The config page is more resilient when Jellyfin loads the page HTML but skips or delays normal script-bound event attachment.

## v0.1.8 — Config Page Initialization Hardening

### High-Level
This release hardens the Jellyfin plugin dashboard page initialization path so the embedded UI can reliably bind events, load current configuration, and execute save/open actions under Jellyfin’s SPA page lifecycle.

### Changed
- Plugin release metadata and packaging are now aligned to `v0.1.8`.

### Fixed
- Config page initialization now retries safely across Jellyfin page-load timing paths instead of assuming immediate DOM availability.
- `Save Settings`, `Refresh Status`, and `Open Retreivr UI` now bind through a more resilient initialization flow.
- Retreivr Core URLs entered without a scheme are normalized before use in page actions.

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
