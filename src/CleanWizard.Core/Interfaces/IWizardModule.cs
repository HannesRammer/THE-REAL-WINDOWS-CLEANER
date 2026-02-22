namespace CleanWizard.Core.Interfaces;

public interface IWizardModule
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string Icon { get; }
    int Order { get; }
    IReadOnlyList<IStep> Steps { get; }
    Task InitializeAsync();
}
