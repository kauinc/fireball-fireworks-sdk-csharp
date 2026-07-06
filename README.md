# Fireball Fireworks SDK

A server-side C# SDK for integrating game servers with the Fireball platform. It sits on your game's backend and acts as the single point of contact for all communication with Fireball — handling player authentication, wallet operations (bets and wins), game session state, client message delivery, jackpots, and multiplayer coordination.

The SDK is designed to run within a Fireball-hosted environment.

<img width="200" alt="FireWorks" src="https://www.staging.fireballserver.com/images/products/FireWorks.png" />

---

## Table of Contents

- [NuGet Package](#nuget-package)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Modules](#modules)
- [For Developers: Releasing a New Version](#for-developers-releasing-a-new-version)

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

In your `Program.cs` or `Startup.cs`, call `AddFireworks()` on your `IServiceCollection`:

```csharp
using Fireball.Fireworks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFireworks();
```

This registers all required singletons and HTTP clients, including retry and timeout policies via Polly.

### 2. Inject `IFireworks`

```csharp
using Fireball.Fireworks;

public class MyGameFunction
{
    private readonly IFireworks _fireworks;

    public MyGameFunction(IFireworks fireworks)
    {
        _fireworks = fireworks;
    }

    public async Task HandleMessage(string messageJson)
    {
        var result = await _fireworks.ParseMessage(messageJson);

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
| **Core** | `Fireball.Fireworks` | Entry point (`IFireworks`), message parsing, validation, configuration |
| **Integration** | `Fireball.Fireworks.IntegrationModule` | Authenticate players, place bets, pay wins, handle disconnects |
| **Session** | `Fireball.Fireworks.SessionModule` | Create, retrieve, update, and close game sessions and game state |
| **Jackpots** | `Fireball.Fireworks.JackpotsModule` | Jackpot contributions and payouts |
| **Messenger** | `Fireball.Fireworks.MessagesModule` | Send messages, session data, and errors to clients |
| **Multiplayer** | `Fireball.Fireworks.MultiplayerModule` | Multiplayer session management and matchmaking |
| **Validation** | `Fireball.Fireworks.Validation` | Message validation attributes and extension methods |
| **Tests** | `Fireball.Fireworks.TestsModule` | RTP testing utilities |

---

## For Developers: Releasing a New Version

Publishing is fully automated via GitHub Actions using [NuGet Trusted Publishing](https://aka.ms/nuget/trusted-publishing) (keyless OIDC — no API keys required).

### 1. Update the version in `Fireball.Fireworks.csproj`

```xml
<Version>1.2.3</Version>
```

Follow [Semantic Versioning](https://semver.org/): `MAJOR.MINOR.PATCH`.

### 2. Commit and push the version bump

```bash
git add Fireball.Fireworks.csproj
git commit -m "bump version to 1.2.3"
git push
```

### 3. Tag the commit and push the tag

```bash
git tag v1.2.3
git push origin v1.2.3
```

Pushing a tag matching `v*.*.*` triggers the `nuget-publish.yml` workflow, which will:

- Build in Release mode
- Pack the `.nupkg`
- Authenticate with NuGet.org via OIDC (no secrets needed)
- Push the package to NuGet.org

### 4. Verify publication

After a few minutes the new version will appear at:

```
https://www.nuget.org/packages/Fireball.Fireworks
```

### Notes

- The `Fireball.Game.Server.Rng` package reference is an external dependency. If it is ever renamed or republished, update the `PackageReference` in `Fireball.Fireworks.csproj` accordingly.
- The project targets **net10.0** only. Consumers must use .NET 10 or later.
