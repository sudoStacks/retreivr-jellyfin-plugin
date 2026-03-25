# Contributing

## Scope

This repository should stay aligned to the Retreivr Section 3 ecosystem plan:

- Resolution API is the integration boundary
- availability states must remain:
  - `verified`
  - `pending`
  - `not_found`
  - `local_only`
- Jellyfin is a consumer of the network, not a replacement for Retreivr Core

## Development Rules

- Keep plugin behavior deterministic.
- Prefer MusicBrainz-backed item identity over fuzzy media matching.
- Do not embed Retreivr Core business logic in the plugin.
- Add source-level comments only where the plugin host/runtime behavior is non-obvious.

## Build

```bash
dotnet restore
dotnet build Retreivr.Jellyfin.Plugin.sln -c Release
dotnet publish src/Retreivr.Jellyfin.Plugin/Retreivr.Jellyfin.Plugin.csproj -c Release -o artifacts/publish
```

## Release

Tagged releases are built automatically by GitHub Actions.
