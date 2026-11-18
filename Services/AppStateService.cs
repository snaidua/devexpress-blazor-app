using DevExpressBlazorApp.Data;
using DevExpressBlazorApp.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevExpressBlazorApp.Services;

public enum ToastNotificationType
{
    Info,
    Success,
    Warning,
    Danger
}

public class ToastNotificationItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ToastNotificationType Type { get; set; } = ToastNotificationType.Info;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ErpNotificationItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "bell";
    public string Category { get; set; } = "Operations";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AppStateService : IDisposable
{
    private readonly IDbContextFactory<ErpDbContext> _contextFactory;
    private readonly DashboardQueryService _dashboardQueryService;
    private readonly ILogger<AppStateService> _logger;
    private Timer? _sessionTimer;

    public event Action? OnChange;
    public event Action<ToastNotificationItem>? OnToastRequested;

    // Session & User Telemetry
    public AppUser? CurrentUser { get; private set; }
    public AppRole? CurrentRole { get; private set; }
    public AppCompany? CurrentCompany { get; private set; }
    public AppLocation? CurrentLocation { get; private set; }
    public List<AppCompany> AvailableCompanies { get; private set; } = new();
    public List<AppLocation> AvailableLocations { get; private set; } = new();

    public string CurrentTheme { get; private set; } = "blazing-berry";
    public bool SidebarExpanded { get; private set; } = true;
    public DateTime SessionStartTime { get; private set; } = DateTime.UtcNow;
    public TimeSpan SessionDuration => DateTime.UtcNow - SessionStartTime;

    // Footer Live Telemetry
    public string FooterStatusText { get; private set; } = "System Ready";
    public string FooterStatusLevel { get; private set; } = "Normal"; // Normal, Busy, Success, Warning, Error

    // Dynamic Badges & Notifications
    public int UnreadNotificationsCount => Notifications.Count(n => !n.IsRead);
    public List<ErpNotificationItem> Notifications { get; private set; } = new();
    public Dictionary<string, int> DynamicBadgeCounts { get; private set; } = new();

    // Global Alert Banner
    public bool IsAlertBannerVisible { get; private set; }
    public string AlertBannerMessage { get; private set; } = string.Empty;
    public string AlertBannerType { get; private set; } = "info"; // info, success, warning, danger
    public bool AlertBannerDismissible { get; private set; } = true;

    // Progress Indicator (0 - 100%)
    public bool IsProgressBarVisible { get; private set; }
    public int ProgressPercentage { get; private set; }
    public string ProgressBarText { get; private set; } = string.Empty;

    // Loading Panel
    public bool IsLoadingPanelVisible { get; private set; }
    public string LoadingPanelMessage { get; private set; } = "Processing request...";

    // Global Modal
    public bool IsModalVisible { get; private set; }
    public string ModalTitle { get; private set; } = string.Empty;
    public string ModalMessage { get; private set; } = string.Empty;
    public string? ModalTechnicalDetails { get; private set; }
    public string ModalType { get; private set; } = "Info"; // Confirmation, Info, Warning, Error
    public Func<Task>? ModalConfirmCallback { get; private set; }
    public string ModalConfirmText { get; private set; } = "OK";
    public string ModalCancelText { get; private set; } = "Cancel";

    public AppStateService(
        IDbContextFactory<ErpDbContext> contextFactory,
        DashboardQueryService dashboardQueryService,
        ILogger<AppStateService> logger)
    {
        _contextFactory = contextFactory;
        _dashboardQueryService = dashboardQueryService;
        _logger = logger;

        StartSessionTimer();
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            AvailableCompanies = await context.AppCompanies.Where(c => c.IsActive).ToListAsync();
            AvailableLocations = await context.AppLocations.Where(l => l.IsActive).ToListAsync();

            // Load default Admin user or first active user
            CurrentUser = await context.AppUsers
                .Include(u => u.Role)
                .Include(u => u.DefaultCompany)
                .Include(u => u.DefaultLocation)
                .Include(u => u.Preference)
                .FirstOrDefaultAsync(u => u.IsActive);

            if (CurrentUser != null)
            {
                CurrentRole = CurrentUser.Role;
                CurrentCompany = CurrentUser.DefaultCompany ?? AvailableCompanies.FirstOrDefault();
                CurrentLocation = CurrentUser.DefaultLocation ?? AvailableLocations.FirstOrDefault();

                if (CurrentUser.Preference != null && !string.IsNullOrEmpty(CurrentUser.Preference.ThemeName))
                {
                    CurrentTheme = CurrentUser.Preference.ThemeName;
                }
            }

            // Populate initial ERP mock notification feed
            Notifications = new List<ErpNotificationItem>
            {
                new() { Id = 1, Title = "High-Value Order Received", Description = "Lockheed Martin: INV-2026-9041 ($184,500) created", Category = "Sales", IsRead = false, CreatedAt = DateTime.UtcNow.AddMinutes(-15) },
                new() { Id = 2, Title = "Inventory Reorder Triggered", Description = "Micro-Sensor Lot #882 reached safety threshold", Category = "Warehouse", IsRead = false, CreatedAt = DateTime.UtcNow.AddMinutes(-45) },
                new() { Id = 3, Title = "Quarterly Audit Lock", Description = "FY2026-Q3 ledger closing scheduled in 48 hours", Category = "Finance", IsRead = true, CreatedAt = DateTime.UtcNow.AddHours(-3) }
            };

            // Query dynamic badge counts via Dapper
            DynamicBadgeCounts = await _dashboardQueryService.GetNavMenuDynamicBadgeCountsAsync();

            SetFooterStatus("System Ready", "Normal");
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize AppStateService");
            SetFooterStatus("Initialized with fallback", "Warning");
        }
    }

    public void ToggleSidebar()
    {
        SidebarExpanded = !SidebarExpanded;
        NotifyStateChanged();
    }

    public void SetFooterStatus(string text, string level = "Normal")
    {
        FooterStatusText = text;
        FooterStatusLevel = level;
        NotifyStateChanged();
    }

    public async Task SwitchCompanyAsync(int companyId)
    {
        var company = AvailableCompanies.FirstOrDefault(c => c.Id == companyId);
        if (company != null)
        {
            CurrentCompany = company;
            ShowToast($"Active enterprise tenant switched to {company.Name}", "Tenant Changed", ToastNotificationType.Success);
            SetFooterStatus($"Active Company: {company.Code}", "Normal");
            NotifyStateChanged();
        }
        await Task.CompletedTask;
    }

    public async Task SwitchLocationAsync(int locationId)
    {
        var loc = AvailableLocations.FirstOrDefault(l => l.Id == locationId);
        if (loc != null)
        {
            CurrentLocation = loc;
            ShowToast($"Active operating facility switched to {loc.Name}", "Location Changed", ToastNotificationType.Info);
            SetFooterStatus($"Active Location: {loc.Code}", "Normal");
            NotifyStateChanged();
        }
        await Task.CompletedTask;
    }

    public async Task SetThemeAsync(string themeName)
    {
        CurrentTheme = themeName;

        try
        {
            if (CurrentUser != null)
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var pref = await context.AppUserPreferences.FirstOrDefaultAsync(p => p.UserId == CurrentUser.Id);
                if (pref == null)
                {
                    pref = new AppUserPreference
                    {
                        UserId = CurrentUser.Id,
                        ThemeName = themeName,
                        UpdatedAt = DateTime.UtcNow
                    };
                    context.AppUserPreferences.Add(pref);
                }
                else
                {
                    pref.ThemeName = themeName;
                    pref.UpdatedAt = DateTime.UtcNow;
                }
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist user theme preference");
        }

        ShowToast($"DevExpress theme changed to '{themeName}'", "Theme Applied", ToastNotificationType.Success);
        NotifyStateChanged();
    }

    // Toast Notifications
    public void ShowToast(string message, string title = "ERP System", ToastNotificationType type = ToastNotificationType.Info)
    {
        var item = new ToastNotificationItem
        {
            Title = title,
            Message = message,
            Type = type,
            Timestamp = DateTime.UtcNow
        };
        OnToastRequested?.Invoke(item);
    }

    // Global Alert Banner
    public void ShowAlertBanner(string message, string alertType = "info", bool dismissible = true)
    {
        AlertBannerMessage = message;
        AlertBannerType = alertType;
        AlertBannerDismissible = dismissible;
        IsAlertBannerVisible = true;
        NotifyStateChanged();
    }

    public void DismissAlertBanner()
    {
        IsAlertBannerVisible = false;
        NotifyStateChanged();
    }

    // Global Confirmation & Dialog Modals
    public void ShowConfirmationModal(
        string title,
        string message,
        Func<Task> onConfirm,
        string confirmButtonText = "Confirm",
        string cancelButtonText = "Cancel")
    {
        ModalTitle = title;
        ModalMessage = message;
        ModalType = "Confirmation";
        ModalConfirmCallback = onConfirm;
        ModalConfirmText = confirmButtonText;
        ModalCancelText = cancelButtonText;
        ModalTechnicalDetails = null;
        IsModalVisible = true;
        NotifyStateChanged();
    }

    public void ShowAlertModal(string title, string message, string type = "Info")
    {
        ModalTitle = title;
        ModalMessage = message;
        ModalType = type;
        ModalConfirmCallback = null;
        ModalConfirmText = "Close";
        ModalCancelText = string.Empty;
        ModalTechnicalDetails = null;
        IsModalVisible = true;
        NotifyStateChanged();
    }

    public void CloseModal()
    {
        IsModalVisible = false;
        ModalConfirmCallback = null;
        ModalTechnicalDetails = null;
        NotifyStateChanged();
    }

    // Progress Bar
    public void ShowProgressBar(int percentage, string text = "")
    {
        IsProgressBarVisible = true;
        ProgressPercentage = Math.Clamp(percentage, 0, 100);
        ProgressBarText = text;
        NotifyStateChanged();
    }

    public void HideProgressBar()
    {
        IsProgressBarVisible = false;
        ProgressPercentage = 0;
        ProgressBarText = string.Empty;
        NotifyStateChanged();
    }

    // Full-screen Loading Panel
    public void ShowLoading(string message = "Processing ERP Transaction...")
    {
        LoadingPanelMessage = message;
        IsLoadingPanelVisible = true;
        SetFooterStatus(message, "Busy");
        NotifyStateChanged();
    }

    public void HideLoading()
    {
        IsLoadingPanelVisible = false;
        SetFooterStatus("System Ready", "Normal");
        NotifyStateChanged();
    }

    // Centralized Global Error Handling
    public async Task HandleGlobalErrorAsync(Exception ex, string? customUserMessage = null)
    {
        HideLoading();
        HideProgressBar();
        SetFooterStatus("System Error", "Error");

        var userContext = $"{CurrentUser?.Username ?? "Anonymous"} | Role: {CurrentRole?.Name ?? "N/A"} | Company: {CurrentCompany?.Code ?? "N/A"}";
        
        // Asynchronously log to SQL Server SystemLogs via Dapper
        await _dashboardQueryService.LogSystemErrorAsync(
            level: "FATAL",
            message: ex.Message,
            exception: ex.ToString(),
            stackTrace: ex.StackTrace,
            source: ex.Source ?? "Blazor Server Circuit",
            userContext: userContext);

        ModalTitle = "System Exception Intercepted";
        ModalMessage = customUserMessage ?? "An unhandled exception occurred during ERP transaction execution. The error has been captured and dispatched to SQL Server SystemLogs.";
        ModalTechnicalDetails = $"{ex.GetType().FullName}: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
        ModalType = "Error";
        ModalConfirmCallback = null;
        ModalConfirmText = "Acknowledge";
        ModalCancelText = string.Empty;
        IsModalVisible = true;

        NotifyStateChanged();
    }

    public void MarkAllNotificationsAsRead()
    {
        foreach (var item in Notifications)
        {
            item.IsRead = true;
        }
        NotifyStateChanged();
    }

    private void StartSessionTimer()
    {
        _sessionTimer = new Timer(_ =>
        {
            try
            {
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Session timer tick exception caught");
            }
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    private void NotifyStateChanged()
    {
        if (OnChange == null) return;

        foreach (var handler in OnChange.GetInvocationList().Cast<Action>())
        {
            try
            {
                handler.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Exception in AppState OnChange subscriber");
            }
        }
    }

    public void Dispose()
    {
        _sessionTimer?.Dispose();
    }
}

