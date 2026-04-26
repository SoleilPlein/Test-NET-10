# Test NET 10

Calculatrice console generique en .NET 10 avec UI interactive Spectre.Console, architecture orientee SOLID et tests xUnit.

## Fonctionnalites

- Choix du rythme d'animation: `Rapide`, `Normal`, `Cinematique`
- Memorisation automatique du dernier profil d'animation (stockage local)
- Choix du type numerique: `int`, `float`, `double`, `decimal`
- Choix d'operation: `+`, `-`, `/`, `*` (+ `%` pour les entiers)
- Parsing flexible des decimaux (`12,5` et `12.5`)
- Gestion des cas invalides (division/modulo par zero)
- Rendu console stylise: bandeau, resume, spinner, panels de resultat/erreur

## Architecture

### SOLID applique

- SRP: separation UI (`ConsoleUiService`), parsing (`NumberParser`), fabrique metier (`OperationFactory`)
- OCP: ajout d'operations via factory sans modifier le flux principal
- DIP: `Program` depend d'abstractions (`IConsoleUiService`, `INumberParser`, `IOperationFactory<T>`)

### Structure

- `Test NET 10/Program.cs`: orchestration de l'application
- `Test NET 10/ConsoleUiService.cs`: UI console Spectre + animations
- `Test NET 10/NumberParser.cs`: parsing numerique flexible
- `Test NET 10/Operation.cs`: operations generiques
- `Test NET 10/OperationFactory.cs`: factories d'operations
- `Test_NET_10.Tests/UnitTest1.cs`: tests unitaires

## Prerequis

- SDK .NET 10 (preview)

## Execution locale

Depuis la racine du repo:

```bash
dotnet run --project "Test NET 10/Test NET 10.csproj"
```

## Tests

```bash
dotnet test "Test NET 10.slnx"
```

## CI

Une pipeline GitHub Actions est fournie dans `.github/workflows/ci.yml`:

- restore
- build (Release)
- test (Release)
