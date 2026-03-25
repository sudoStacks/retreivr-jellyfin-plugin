# Retreivr - Music Search and Acquisition

Jellyfin plugin for the Retreivr resolution and acquisition network.

## Network Role

This plugin connects Jellyfin to a running Retreivr server so users can search music, inspect availability, and send acquisition work to Retreivr without leaving Jellyfin.

The plugin is not a downloader by itself. Retreivr Core remains the backend worker and Resolution API host.

## Quick Setup

The fastest way to get Retreivr running is the core release asset package:

- Retreivr core repo: `https://github.com/sudoStacks/retreivr`
- Release asset: `retreivr-docker-starter-<tag>.zip`

That starter bundle contains the compose file, env example, config example, and runtime notes needed to launch Retreivr quickly with Docker.

Detailed setup notes for Jellyfin + Retreivr are in [docs/SETUP.md](docs/SETUP.md).

Once Retreivr is running, configure this plugin with the Retreivr server root URL. In the common single-node setup, both fields should point to the same base URL:

- `Resolution API Base URL`: `http://RETREIVR_HOST:8000`
- `Retreivr Core Base URL`: `http://RETREIVR_HOST:8000`

Optional API keys are only needed if you enabled auth on the Retreivr server.

## Current Scope

Included now:

- backend status / health view inside the plugin page
- `Open Retreivr UI` button using the configured core URL
- basic in-plugin music search UI
- artist cards with album drilldown
- album cards with track drilldown and full-album download
- track cards with direct enqueue into Retreivr Core
- typed Resolution API client
- typed Retreivr Core client
- availability service aligned to Retreivr Section 3 status model:
  - `verified`
  - `pending`
  - `not_found`
  - `local_only`
- plugin controller endpoints for:
  - health and backend status
  - recording resolve
  - bulk resolve
  - Jellyfin item availability by item id
  - Jellyfin item bulk availability
  - proxied music search / album / track flows
  - Retreivr Core download enqueue
- MusicBrainz provider-id extraction for Jellyfin items
- GitHub Actions tagged-release packaging workflow

## Purpose

This plugin is the first external consumer of the Retreivr Resolution API. Its job is to turn MusicBrainz-linked media inside Jellyfin into:

- availability indicators
- instant resolved-source playback eligibility
- download handoff into Retreivr Core
- future artist radio and music-video flows

The integration boundary is the Resolution API and selected Retreivr Core APIs, not Retreivr internals.

Not yet implemented:

- real Jellyfin UI injection for item badges
- playback interception / “play resolved source” flow
- artist radio UX
- music video browsing integration

## Configuration

For most installs:

- `Resolution API Base URL`: `http://retreivr.local:8000`
- `Retreivr Core Base URL`: `http://retreivr.local:8000`

Optional auth is supported via bearer token / API key if you enable that on the Retreivr server.

## Plugin API Surface

Current controller routes:

- `GET /Plugins/Retreivr/health`
- `GET /Plugins/Retreivr/status`
- `GET /Plugins/Retreivr/resolve/recording/{mbid}`
- `POST /Plugins/Retreivr/resolve/bulk`
- `GET /Plugins/Retreivr/music/search`
- `GET /Plugins/Retreivr/music/albums/search`
- `GET /Plugins/Retreivr/music/albums/{releaseGroupMbid}/tracks`
- `POST /Plugins/Retreivr/music/albums/{releaseGroupMbid}/download`
- `POST /Plugins/Retreivr/music/enqueue`
- `GET /Plugins/Retreivr/items/{itemId}/availability`
- `POST /Plugins/Retreivr/items/availability/bulk`
- `POST /Plugins/Retreivr/items/{itemId}/download`

## Immediate Functional Scope

With the current plugin page, you can already:

1. confirm backend connectivity from Jellyfin
2. open the configured Retreivr UI directly
3. search artists/albums/tracks through Retreivr
4. browse albums for an artist
5. inspect track listings for an album
6. queue track or album downloads into Retreivr

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

This repo was scaffolded in an environment without `dotnet`, so local compilation did not happen here. The intended packaging path is the tagged GitHub Actions release workflow.
