# Jellyfin Plugin Setup

## Backend Requirement

This plugin requires a running Retreivr server. The plugin does not perform acquisition or MusicBrainz resolution by itself. It talks to:

- the Retreivr Resolution API
- selected Retreivr Core endpoints for music search and queueing

## Fastest Retreivr Install

Use the Retreivr core release bundle from:

- `https://github.com/sudoStacks/retreivr`

Download the release asset:

- `retreivr-docker-starter-<tag>.zip`

That package contains:

- `docker-compose.yml`
- `.env.example`
- `config/config.json.example`
- runtime setup notes

After extracting the starter bundle:

1. Copy `.env.example` to `.env`
2. Copy `config/config.json.example` to `config/config.json`
3. Adjust paths, tokens, and settings
4. Start Retreivr with `docker compose up -d`

## Plugin Settings

If Jellyfin and Retreivr are on the same Docker network, use the Retreivr container hostname and port.

Common examples:

- `http://retreivr:8000`
- `http://192.168.1.50:8000`
- `https://retreivr.example.com`

For the common single-node setup:

- `Resolution API Base URL` = Retreivr server root
- `Retreivr Core Base URL` = same Retreivr server root

Leave the API key fields blank unless you explicitly enabled auth on the Retreivr server.

## Health Checks

Before testing the plugin, confirm these endpoints load from the Jellyfin host:

- `<retreivr-base-url>/health`
- `<retreivr-base-url>/stats`

The plugin page also includes a backend status panel that checks the configured URLs directly.

## First Test Flow

1. Open Jellyfin Admin Dashboard
2. Open the Retreivr plugin settings page
3. Enter both base URLs
4. Save
5. Confirm both backends show as reachable
6. Search for an artist from the plugin page
7. Open albums
8. Open tracks or queue a full album

## Current Scope

The current plugin UI is a minimal first-pass operator surface inside Jellyfin. It supports:

- backend status
- open Retreivr UI
- basic music search
- album drilldown
- track drilldown
- track enqueue
- album enqueue

Future Jellyfin-native item badges, playback hooks, and richer library integration will build on top of this.
