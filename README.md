# Retreivr - Music Search and Acquisition

Jellyfin plugin scaffold for the Retreivr resolution network.

## Purpose

This plugin is the first external consumer of the Retreivr Resolution API. Its job is to turn MusicBrainz-linked media inside Jellyfin into:

- availability indicators
- instant resolved-source playback eligibility
- download handoff into Retreivr Core
- future artist radio and music-video flows

The integration boundary is the Resolution API, not Retreivr internals.

## Current Scaffold

Included now:

- Jellyfin plugin project structure
- plugin configuration model
- embedded config page
- typed Resolution API client
- typed Retreivr Core client
- availability service aligned to Retreivr Section 3 status model:
  - `verified`
  - `pending`
  - `not_found`
  - `local_only`
- plugin controller endpoints for:
  - recording resolve
  - bulk resolve
  - Jellyfin item availability by item id
  - Jellyfin item bulk availability
  - Retreivr Core download enqueue
- MusicBrainz provider-id extraction for Jellyfin items
- GitHub Actions build workflow
- GitHub Actions tagged-release packaging workflow
- roadmap docs for immediate next implementation phases

Not yet implemented:

- real Jellyfin UI injection for item badges
- playback interception / “play resolved source” flow
- artist radio UX
- music video browsing integration

## Configuration

The plugin is designed to connect to a Retreivr Resolution API endpoint such as:

`http://retreivr.local:8000`

Optional auth is supported via bearer token / API key if you enable that on the Resolution API.

Retreivr Core dispatch can point at the same node or a separate Retreivr Core URL.

## Plugin API Surface

Current controller routes:

- `GET /Plugins/Retreivr/health`
- `GET /Plugins/Retreivr/resolve/recording/{mbid}`
- `POST /Plugins/Retreivr/resolve/bulk`
- `GET /Plugins/Retreivr/items/{itemId}/availability`
- `POST /Plugins/Retreivr/items/availability/bulk`
- `POST /Plugins/Retreivr/items/{itemId}/download`

## Immediate Functional Scope

With the current scaffold, the next implementation pass can begin wiring:

1. bulk availability lookups for MBID-backed Jellyfin items
2. status badges for `verified / pending / not_found / local_only`
3. a “Resolve via Retreivr” action that opens or calls the Retreivr node
4. a “Download via Retreivr” action using the plugin controller
5. a “Play resolved source” path for `verified` and `local_only`

## Release Automation

- normal pushes do not run CI
- manual builds are available through `workflow_dispatch`
- tags matching `v*` build and package the plugin automatically
- the release workflow stamps the assembly/build manifest version from the tag before packaging
- the repository manifest is at `manifest.json` and is intended to be served from the repo root on `main`

## Jellyfin Repository URL

Once the GitHub repo exists at `sudoStacks/retreivr-jellyfin-plugin`, the repository URL to paste into Jellyfin will be:

`https://raw.githubusercontent.com/sudoStacks/retreivr-jellyfin-plugin/main/manifest.json`

Installation path in Jellyfin:

1. Dashboard
2. Plugins
3. Repositories
4. Add repository
5. Paste the manifest URL above
6. Save
7. Open `Catalog`
8. Find `Retreivr`
9. Install
10. Restart Jellyfin

## Local Build Note

This repo was scaffolded in an environment without `dotnet`, so the source tree is in place but it has not been compiled here. Build and package steps should be run on a machine with the appropriate .NET SDK installed.
