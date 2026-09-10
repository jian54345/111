# InfiniteLoop.CommandGenerator

A standalone Windows Forms command generator for the InfiniteLoop / AscNet development server.

## Requirements

- Windows
- Visual Studio 2022 with .NET desktop development workload
- .NET 8 SDK

## Build

```powershell
dotnet restore
dotnet build -c Release
```

Run:

```powershell
dotnet run --project .\src\InfiniteLoop.CommandGenerator\InfiniteLoop.CommandGenerator.csproj
```

## Default server settings

The first run uses:

```text
Server:
http://127.0.0.1:8080

Endpoint:
 /api/AscNet/command/{command}

Method:
 GET
```

The `{command}` placeholder is URL encoded before the request is sent.

If your local InfiniteLoop branch exposes a different command endpoint or HTTP method, change it in the `选项` tab. The setting is stored beside the executable as `appsettings.local.json`.

## Built-in commands

### Character

```text
/character add all
/character modify all max
/character modify all min
/character reset all
/character modify all level {level}
```

### Equip

```text
/equip modify all level {level}
/equip modify all level max
/equip reset all
```

### Level

```text
/level {level}
/level max
/level reset
```

### Guide

```text
/guide all
```

### System

```text
/save
```

## Batch commands

Commands can be combined with `|`:

```text
/character modify all max | /equip modify all level max | /level max | /save
```

The generator executes batch commands one by one and waits for each HTTP request to complete.

## Important

This tool does not implement game logic. It only generates and optionally sends the command strings that your InfiniteLoop server exposes.

For state-changing commands such as:

```text
/character reset all
/equip reset all
/level reset
```

the application displays a confirmation before batch execution.

## Design

The command list is isolated in:

```text
src/InfiniteLoop.CommandGenerator/Services/CommandCatalog.cs
```

The HTTP client is isolated in:

```text
src/InfiniteLoop.CommandGenerator/Services/AscNetClient.cs
```

This makes it easy to adapt the client to changes in the InfiniteLoop command API without changing the UI.
