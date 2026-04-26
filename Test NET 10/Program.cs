using System.Globalization;
using System.Numerics;
using Test_NET_10;

CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
INumberParser numberParser = new FlexibleNumberParser();
IConsoleUiService consoleUi = new ConsoleUiService(numberParser);
string preferencesPath = Path.Combine(
	Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
	"Test_NET_10",
	"preferences.json");
IAnimationPreferenceStore animationPreferenceStore = new JsonAnimationPreferenceStore(preferencesPath);

consoleUi.ShowBanner();

AnimationSpeedProfile selectedAnimationProfile;

if (!animationPreferenceStore.TryLoad(out selectedAnimationProfile))
{
	AnimationSpeedProfile[] animationProfiles =
	[
		AnimationSpeedProfile.Fast,
		AnimationSpeedProfile.Normal,
		AnimationSpeedProfile.Cinematic
	];

	int animationProfileIndex = consoleUi.ReadSelection(
		animationProfiles,
		"Choisis le rythme d'animation.",
		GetAnimationProfileLabel);

	if (animationProfileIndex < 0)
	{
		consoleUi.ShowCancellation();
		return;
	}

	selectedAnimationProfile = animationProfiles[animationProfileIndex];
	animationPreferenceStore.Save(selectedAnimationProfile);
}

consoleUi.ConfigureAnimation(selectedAnimationProfile);

string[] numberTypes = ["int", "float", "double", "decimal"];

while (true)
{
	int numberTypeIndex = consoleUi.ReadSelection(
		numberTypes,
		"Choisis le type de nombre.");

	if (numberTypeIndex < 0)
	{
		consoleUi.ShowCancellation();
		return;
	}

	string selectedNumberType = numberTypes[numberTypeIndex];
	string animationProfileLabel = GetAnimationProfileLabel(selectedAnimationProfile);

	Func<Task<bool>> runCalculationAsync = BuildScenario(selectedNumberType, animationProfileLabel, culture, consoleUi);
	bool shouldContinue = await runCalculationAsync();

	if (!shouldContinue)
	{
		return;
	}
}

static Func<Task<bool>> BuildScenario(
	string selectedNumberType,
	string animationProfileLabel,
	CultureInfo culture,
	IConsoleUiService consoleUi)
{
	switch (selectedNumberType)
	{
		case "int":
			IOperationFactory<int> intFactory = new IntegerOperationFactory<int>();
			return () => RunCalculationAsync(intFactory, selectedNumberType, animationProfileLabel, culture, consoleUi);
		case "float":
			IOperationFactory<float> floatFactory = new NumericOperationFactory<float>();
			return () => RunCalculationAsync(floatFactory, selectedNumberType, animationProfileLabel, culture, consoleUi);
		case "double":
			IOperationFactory<double> doubleFactory = new NumericOperationFactory<double>();
			return () => RunCalculationAsync(doubleFactory, selectedNumberType, animationProfileLabel, culture, consoleUi);
		case "decimal":
			IOperationFactory<decimal> decimalFactory = new NumericOperationFactory<decimal>();
			return () => RunCalculationAsync(decimalFactory, selectedNumberType, animationProfileLabel, culture, consoleUi);
		default:
			throw new InvalidOperationException("Type de nombre non pris en charge.");
	}
}

static async Task<bool> RunCalculationAsync<T>(
	IOperationFactory<T> operationFactory,
	string selectedNumberType,
	string animationProfileLabel,
	CultureInfo culture,
	IConsoleUiService consoleUi)
	where T : struct, INumber<T>
{
	T a = consoleUi.ReadNumber<T>("Saisis le premier nombre: ", culture);

	OperationEnum[] operations = operationFactory.GetAvailableOperations();
	int operationIndex = consoleUi.ReadSelection(
		operations,
		"Choisis l'operation.",
		GetSymbol);

	if (operationIndex < 0)
	{
		consoleUi.ShowCancellation();
		return false;
	}

	OperationEnum selectedOperation = operations[operationIndex];

	consoleUi.ShowSelectionSummary(
		selectedNumberType,
		selectedOperation,
		culture,
		GetSymbol(selectedOperation),
		animationProfileLabel);

	T b = consoleUi.ReadNumber<T>("Saisis le second nombre: ", culture);

	if (RequiresNonZeroDivisor(selectedOperation) && b == T.Zero)
	{
		consoleUi.ShowError("Impossible de diviser par 0.");
		return true;
	}

	T result = await consoleUi.RunWithStatusAsync(
		$"Execution de l'operation {selectedOperation}...",
		() =>
		{
			IOperation<T> operation = operationFactory.Create(selectedOperation, a, b);
			return Task.FromResult(operation.GetResult());
		});

	consoleUi.ShowResult(a, GetSymbol(selectedOperation), b, result, culture);
	return true;
}

static bool RequiresNonZeroDivisor(OperationEnum operation)
{
	return operation is OperationEnum.Divide or OperationEnum.Modulo;
}

static string GetSymbol(OperationEnum operation)
{
	return operation switch
	{
		OperationEnum.Addition => "+",
		OperationEnum.Subtract => "-",
		OperationEnum.Divide => "/",
		OperationEnum.Multiple => "*",
		OperationEnum.Modulo => "%",
		_ => "?"
	};
}

static string GetAnimationProfileLabel(AnimationSpeedProfile profile)
{
	return profile switch
	{
		AnimationSpeedProfile.Fast => "Rapide",
		AnimationSpeedProfile.Normal => "Normal",
		AnimationSpeedProfile.Cinematic => "Cinematique",
		_ => "Inconnu"
	};
}

