using HomeBook.Client.Models;
using HomeBook.Frontend.Module.Finances.Enums;
using HomeBook.Frontend.Module.Finances.Mappings;
using HomeBook.Frontend.Module.Finances.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HomeBook.Frontend.Module.Finances.Pages.Saving;

public partial class Add : ComponentBase
{
    private bool _useQuickAdd = false;
    private int _stepIndex = 0;
    private MudStepper _stepper = null!;
    private MudForm? _formStepName;
    private MudForm? _formStepGoal;
    private MudForm? _formStepPlan;
    private bool isStepNameValid = false;
    private bool isStepGoalValid = false;
    private bool isStepPlanValid = false;
    private SavingGoalDetailViewModel _model { get; } = new();
    private AddSavingGoalSummaryViewModel? _summaryVM = null;

    private MudForm? _formStepBasicData;
    private MudForm? _formStep2;
    private bool _iconDialogOpen;

    protected readonly List<string> SuggestedIcons =
    [
        Icons.Material.Filled.Savings,
        Icons.Material.Filled.AttachMoney,
        Icons.Material.Filled.TravelExplore,
        Icons.Material.Filled.Home,
        Icons.Material.Filled.Computer
    ];

    protected readonly List<string> AllIcons =
    [
        Icons.Material.Filled.Savings,
        Icons.Material.Filled.AttachMoney,
        Icons.Material.Filled.TravelExplore,
        Icons.Material.Filled.Home,
        Icons.Material.Filled.Computer,
        Icons.Material.Filled.PhoneIphone,
        Icons.Material.Filled.CarRental,
        Icons.Material.Filled.School,
        Icons.Material.Filled.Favorite,
        Icons.Material.Filled.Flight,
        Icons.Material.Filled.Coffee,
        Icons.Material.Filled.Monitor,
        Icons.Material.Filled.FitnessCenter,
        Icons.Material.Filled.Book,
        Icons.Material.Filled.Pets
    ];

    protected async Task NextStep()
    {
        CancellationToken cancellationToken = CancellationToken.None;
        _stepIndex++;

        int maxSteps = _stepper.Steps.Count;
        bool isLast = (_stepIndex + 1) == maxSteps;

        if (!isLast)
            return;

        _summaryVM = ViewModelFactory.CreateAddSavingGoalSummaryViewModel();
        _summaryVM.Name = _model.Name;
        _summaryVM.Color = _model.Color ?? "#ff5722";
        _summaryVM.IconName = _model.IconName ?? Icons.Material.Filled.Savings;
        _summaryVM.TargetAmount = _model.TargetAmount;

        if (_model.TargetDate is null)
            return;

        // calculate with target date
        string? token = await AuthenticationService.GetTokenAsync(cancellationToken);
        CalculatedSavingResponse? response = await BackendClient.Modules.Homebook.Finances.Calculations.Savings
            .PostAsync(new CalculateSavingRequest
                {
                    TargetAmount = Convert.ToDouble(_model.TargetAmount),
                    TargetDate = _model.TargetDate,
                    InterestRateOption = (int)_model.InterestRateOption,
                    InterestRate = Convert.ToDouble(_model.InterestRate),
                    TargetSimpleRate = true,
                },
                x =>
                {
                    x.Headers.Add("Authorization", $"Bearer {token}");
                },
                cancellationToken);

        if (response is null)
        {
            // display error
            return;
        }

        // _summaryVM.TargetDate = _model.TargetDate;
        _summaryVM.InterestRateOption = _model.InterestRateOption;
        _summaryVM.InterestRate = _model.InterestRate;

        _summaryVM.TargetDate = DateTime.UtcNow.AddMonths(response.MonthsNeeded!.Value);
        _summaryVM.AmountPerMonth = Convert.ToDecimal(response.MonthlyPayment);
        _summaryVM.NumberOfMonths = response.MonthsNeeded!.Value;
        _summaryVM.AmountsWithInterests = (response.Amounts ?? []).Select(x => Convert.ToDecimal(x!.Value)).ToArray();
        _summaryVM.Interests = (response.Interests ?? []).Select(x => Convert.ToDecimal(x!.Value)).ToArray();

        StateHasChanged();
    }

    protected void PrevStep() => _stepIndex = 0;

    protected void SelectIcon(string icon)
    {
        _model.IconName = icon;
        _iconDialogOpen = false;
    }

    protected async Task SaveAsync()
    {
        if (_summaryVM is null)
            return;

        CancellationToken cancellationToken = CancellationToken.None;

        string? token = await AuthenticationService.GetTokenAsync(cancellationToken);
        await BackendClient.Modules.Homebook.Finances.SavingGoals
            .PostAsync(
                _summaryVM.ToRequest(),
                x =>
                {
                    x.Headers.Add("Authorization", $"Bearer {token}");
                },
                cancellationToken);

        // Beispiel: await SavingGoalService.CreateOrUpdateSavingGoalAsync(Model);
        Snackbar.Add("Sparziel erfolgreich erstellt!", Severity.Success);

        NavigationManager.NavigateTo("/Finances/Savings/Overview");
    }

    public async Task SwitchToQuickAdd()
    {
        _useQuickAdd = true;
        StateHasChanged();
    }
}
