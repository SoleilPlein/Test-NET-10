using System.Numerics;
using Spectre.Console;

namespace Test_NET_10;

public enum AnimationSpeedProfile
{
    Fast,
    Normal,
    Cinematic
}

public interface IConsoleUiService
{
    int ReadSelection<T>(T[] options, string title, Func<T, string>? labelSelector = null);

    T ReadNumber<T>(string prompt, IFormatProvider formatProvider) where T : struct, INumber<T>;

    void ShowBanner();

    void ShowCancellation();

    void ShowSelectionSummary(
        string selectedNumberType,
        OperationEnum selectedOperation,
        IFormatProvider formatProvider,
        string symbol,
        string? animationProfile = null);

    void ShowError(string message);

    void ShowResult<T>(T a, string symbol, T b, T result, IFormatProvider formatProvider);

    Task<TResult> RunWithStatusAsync<TResult>(string status, Func<Task<TResult>> action, int? minimumSpinnerMs = null);

    void ConfigureAnimation(AnimationSpeedProfile profile);
}

public sealed class ConsoleUiService(INumberParser numberParser) : IConsoleUiService
{
    private readonly INumberParser numberParser = numberParser;
    private const int CancelIndex = -1;
    private int minimumSpinnerMs = 650;

    public int ReadSelection<T>(T[] options, string title, Func<T, string>? labelSelector = null)
    {
        if (options.Length == 0)
        {
            throw new ArgumentException("At least one option is required.", nameof(options));
        }

        var prompt = new SelectionPrompt<int>()
            .Title($"[bold]{EscapeMarkup(title)}[/]\n[grey]Raccourcis:[/] [white]Haut/Bas[/] naviguer, [white]Entree[/] valider, [white]Annuler[/] quitter")
            .PageSize(Math.Min(options.Length + 1, 10))
            .UseConverter(index => index == CancelIndex
                ? "[red]Annuler[/]"
                : EscapeMarkup(labelSelector?.Invoke(options[index]) ?? options[index]?.ToString() ?? string.Empty));

        List<int> choices = [.. Enumerable.Range(0, options.Length), CancelIndex];
        prompt.AddChoices(choices);

        return AnsiConsole.Prompt(prompt);
    }

    public void ShowBanner()
    {
        AnsiConsole.Clear();

        var panel = new Panel("[bold cyan]Test NET 10 Console Lab[/]\n[grey]Calculatrice generique avec experience interactive[/]")
        {
            Border = BoxBorder.Rounded,
            Header = new PanelHeader("Session", Justify.Center),
            Padding = new Padding(1, 0, 1, 0)
        };

        var helpPanel = new Panel(
            "[white]Haut/Bas[/] : naviguer\n[white]Entree[/] : valider\n[white]Annuler[/] : quitter\n[white]Ctrl+C[/] : arret immediat")
        {
            Border = BoxBorder.Rounded,
            Header = new PanelHeader("Aide rapide", Justify.Center),
            Padding = new Padding(1, 0, 1, 0)
        };

        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddRow(panel, helpPanel);

        AnsiConsole.Write(grid);
        AnsiConsole.Write(new Rule("[grey]Initialisation[/]").RuleStyle("grey"));
    }

    public void ShowCancellation()
    {
        AnsiConsole.MarkupLine("[yellow]Operation annulee.[/]");
    }

    public void ShowSelectionSummary(
        string selectedNumberType,
        OperationEnum selectedOperation,
        IFormatProvider formatProvider,
        string symbol,
        string? animationProfile = null)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[grey]Parametre[/]")
            .AddColumn("[white]Valeur[/]");

        table.AddRow("Type", $"[cyan]{EscapeMarkup(selectedNumberType)}[/]");
        table.AddRow("Operation", $"[cyan]{EscapeMarkup(selectedOperation.ToString())} ({EscapeMarkup(symbol)})[/]");
        table.AddRow("Culture", $"[cyan]{EscapeMarkup(formatProvider.ToString() ?? string.Empty)}[/]");

        if (!string.IsNullOrWhiteSpace(animationProfile))
        {
            table.AddRow("Animation", $"[cyan]{EscapeMarkup(animationProfile)}[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public void ConfigureAnimation(AnimationSpeedProfile profile)
    {
        minimumSpinnerMs = profile switch
        {
            AnimationSpeedProfile.Fast => 250,
            AnimationSpeedProfile.Normal => 650,
            AnimationSpeedProfile.Cinematic => 1400,
            _ => minimumSpinnerMs
        };
    }

    public void ShowError(string message)
    {
        var panel = new Panel($"[red]{EscapeMarkup(message)}[/]")
        {
            Border = BoxBorder.Rounded,
            Header = new PanelHeader("Erreur", Justify.Center)
        };

        AnsiConsole.Write(panel);
    }

    public void ShowResult<T>(T a, string symbol, T b, T result, IFormatProvider formatProvider)
    {
        string equation = string.Format(formatProvider, "{0} {1} {2} = {3}", a, symbol, b, result);

        var panel = new Panel($"[bold green]{EscapeMarkup(equation)}[/]")
        {
            Border = BoxBorder.Rounded,
            Header = new PanelHeader("Resultat", Justify.Center)
        };

        AnsiConsole.Write(panel);
    }

    public async Task<TResult> RunWithStatusAsync<TResult>(
        string status,
        Func<Task<TResult>> action,
        int? minimumSpinnerMs = null)
    {
        TResult? result = default;
        bool hasValue = false;
        int effectiveMinimumSpinnerMs = minimumSpinnerMs ?? this.minimumSpinnerMs;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Binary)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync(EscapeMarkup(status), async _ =>
            {
                DateTime startedAt = DateTime.UtcNow;
                TResult localResult = await action();

                int elapsedMs = (int)(DateTime.UtcNow - startedAt).TotalMilliseconds;
                int remainingMs = effectiveMinimumSpinnerMs - elapsedMs;

                if (remainingMs > 0)
                {
                    await Task.Delay(remainingMs);
                }

                result = localResult;
                hasValue = true;
            });

        if (!hasValue)
        {
            throw new InvalidOperationException("The operation did not produce a value.");
        }

        return result!;
    }

    public T ReadNumber<T>(string prompt, IFormatProvider formatProvider) where T : struct, INumber<T>
    {
        while (true)
        {
            string input = AnsiConsole.Ask<string>($"[cyan]{EscapeMarkup(prompt)}[/]");

            if (numberParser.TryParseFlexible(input, formatProvider, out T value))
            {
                return value;
            }

            AnsiConsole.MarkupLine(
                $"[red]Valeur invalide pour le type {EscapeMarkup(typeof(T).Name)}, recommence (virgule ou point acceptes pour les decimaux).[/]");
        }
    }

    private static string EscapeMarkup(string value)
    {
        return Markup.Escape(value);
    }
}
