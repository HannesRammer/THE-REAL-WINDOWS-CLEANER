# Update UI Layout and Styling

The goal is to analyze the current WPF layout of `CleanWizard` and make it look "awesome, simple, and easy", while removing everything that feels "weird" or unprofessional.

## Proposed Changes

Currently, the application relies on deeply nested controls, hardcoded colors scattered throughout the views (especially in the sidebar), and emojis for icons (which look unprofessional and vary by OS).

I will implement a cleaner, Windows 11-inspired Fluent design style entirely within the existing WPF framework without adding heavy third-party dependencies, keeping it lightweight and fast.

### CleanWizard.App/Styles
Here I will define a sleek, modernized dark palette and unify control styles.
#### [MODIFY] [Colors.Dark.xaml](file:///d:/projects/Projects/theRealWindowsCleaner/src/CleanWizard.App/Styles/Colors.Dark.xaml)
- Update to a more modern, cohesive dark theme palette (inspired by fluent design rather than pure blacks).
- Add missing brushes that were previously hardcoded in the views (e.g., sidebar active states, specific backgrounds).

#### [MODIFY] [Styles.xaml](file:///d:/projects/Projects/theRealWindowsCleaner/src/CleanWizard.App/Styles/Styles.xaml)
- Refine Button and Card styles for better hover states and modern corner radii.
- Add an explicit `IconTextBlockStyle` that uses `FontFamily="Segoe Fluent Icons"` so we can use real Windows icons instead of emojis.

### CleanWizard.App/Views (and MainWindow)
Here I will clean up the actual layout files, removing the "weird stuff".
#### [MODIFY] [MainWindow.xaml](file:///d:/projects/Projects/theRealWindowsCleaner/src/CleanWizard.App/MainWindow.xaml)
- Remove all hardcoded colors (`#1A1A2E`, `#AAAACC`, `#333355`, `#243149`) and replace them with `StaticResource` references to the updated theme.
- Enhance the sidebar typography and alignment so it looks simpler and less cluttered.

#### [MODIFY] [SystemCheckView.xaml](file:///d:/projects/Projects/theRealWindowsCleaner/src/CleanWizard.App/Views/SystemCheckView.xaml)
- Replace all emojis (`🪟`, `⚡`, `🧠`, `💾`, `📁`, `🚀`, `📈`, `🛡️`) with sleek Segoe Fluent Icons.
- Clean up the grid layout of the cards so the margins and paddings are completely uniform and balanced.
- (Optional but recommended) Do the same for `WizardView.xaml` and `SummaryView.xaml` if they contain similar hardcoded elements.

## Verification Plan

### Automated Tests
- `dotnet build .\CleanWizard.sln --configuration Release` => Ensures XAML is valid and no resources are missing.

### Manual Verification
- Once the changes are applied, you can run the application (`dotnet run --project .\src\CleanWizard.App` or via Visual Studio) and verify if the new interface feels significantly cleaner, more modern, and free of "weird" emojis and hardcoded colors.
