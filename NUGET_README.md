# Fireball Fireworks SDK

A server-side C# SDK for integrating game servers with the Fireball platform. It sits on your game's backend and acts as the single point of contact for all communication with Fireball — handling player authentication, wallet operations (bets and wins), game session state, client message delivery, jackpots, and multiplayer coordination.

The SDK is designed to run within a Fireball-hosted environment.

---

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Modules](#modules)

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