# Fireball Fireworks SDK

Fireball™ Fireworks is the world's most high-performance and feature-rich Remote Games Server SDK, built upon almost two decades of battle-hardened experience. Its open architecture allows game studios to easily deploy and maintain game logic, and enables operators to simply integrate into the action.

---

## Table of Contents

- [NuGet Package](#nuget-package)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Modules](#modules)
- [For Developers: Build & Publish Guide](#for-developers-build--publish-guide)

---

## NuGet Package

| Property | Value |
|---|---|
| Package ID | `Fireball.Fireworks` |
| Target Framework | .NET 10.0 |
| Authors | KAU Inc. |
| License | MIT |

---

## Installation

```bash
dotnet add package Fireball.Fireworks
```

Or via the NuGet Package Manager in Visual Studio:

```
Install-Package Fireball.Fireworks
```

---

## Quick Start

### 1. Register dependencies

In your `Program.cs` or `Startup.cs`, call `AddFireballDependencies()` on your `IServiceCollection`:

```csharp
using Fireball.Fireworks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFireballDependencies();
```

This registers all required singletons and HTTP clients, including retry and timeout policies via Polly.

### 2. Inject `IFireball`

```csharp
using Fireball.Fireworks;

public class MyGameFunction
{
    private readonly IFireball _fireball;

    public MyGameFunction(IFireball fireball)
    {
        _fireball = fireball;
    }

    public async Task HandleMessage(string messageJson)
    {
        var result = await _fireball.ParseMessage(messageJson);

        if (result.IsSuccess)
        {
            // Handle message by name
            switch (result.MessageName)
            {
                case FireballConstants.MessagesNames.SESSION:
                    // session established
                    break;
                // ...
            }
        }
    }
}
```

---

## Modules

| Module | Namespace | Description |
|---|---|---|
| **Core** | `Fireball.Fireworks` | Entry point (`IFireball`), message parsing, validation, configuration |
| **Integration** | `Fireball.Fireworks.IntegrationModule` | Authenticate players, place bets, pay wins, handle disconnects |
| **Session** | `Fireball.Fireworks.SessionModule` | Create, retrieve, update, and close game sessions and game state |
| **Jackpots** | `Fireball.Fireworks.JackpotsModule` | Jackpot contributions and payouts |
| **Messenger** | `Fireball.Fireworks.MessagesModule` | Send messages, session data, and errors to clients |
| **Multiplayer** | `Fireball.Fireworks.MultiplayerModule` | Multiplayer session management and matchmaking |
| **Validation** | `Fireball.Fireworks.Validation` | Message validation attributes and extension methods |
| **Tests** | `Fireball.Fireworks.TestsModule` | RTP testing utilities |

---

## For Developers: Build & Publish Guide

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- NuGet API key for the `Fireball.Fireworks` package (obtain from the team)

### 1. Update the version

Edit `Fireball.Fireworks.csproj` and bump the three version fields consistently:

```xml
<Version>1.7.5</Version>
<ReleaseVersion>1.7.5</ReleaseVersion>
<PackageVersion>1.7.5</PackageVersion>
```

Follow [Semantic Versioning](https://semver.org/): `MAJOR.MINOR.PATCH`.

### 2. Build in Release mode

```bash
dotnet build Fireball.Fireworks.sln --configuration Release
```

Verify there are no errors before proceeding.

### 3. Pack the NuGet package

```bash
dotnet pack Fireball.Fireworks.csproj --configuration Release --no-build --output ./nupkg
```

The `.nupkg` file will be written to `./nupkg/`.

### 4. Inspect the package (optional but recommended)

Use [NuGet Package Explorer](https://github.com/NuGetPackageExplorer/NuGetPackageExplorer) or the CLI to verify the package contents:

```bash
dotnet nuget verify ./nupkg/Fireball.Fireworks.*.nupkg
```

### 5. Push to NuGet.org

```bash
dotnet nuget push ./nupkg/Fireball.Fireworks.*.nupkg \
  --api-key <YOUR_API_KEY> \
  --source https://api.nuget.org/v3/index.json
```

Replace `<YOUR_API_KEY>` with your NuGet.org API key. Never commit API keys to source control.

### 6. Verify publication

After a few minutes, the new version should appear at:

```
https://www.nuget.org/packages/Fireball.Fireworks
```

### Notes

- The `<PackOnBuild>true</PackOnBuild>` flag in the `.csproj` will also produce a `.nupkg` automatically on every build. The explicit `dotnet pack` step above is still preferred for release builds to control output location.
- The `Fireball.Game.Server.Rng` package reference is an external dependency. If it is ever renamed or republished, update the `PackageReference` in `Fireball.Fireworks.csproj` accordingly.
- The project targets **net10.0** only. Consumers must use .NET 10 or later.
