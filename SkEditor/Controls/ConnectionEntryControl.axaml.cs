﻿using System.Diagnostics;
using Avalonia.Controls;
using SkEditor.API;

namespace SkEditor.Controls;

public partial class ConnectionEntryControl : UserControl
{
    public ConnectionEntryControl(ConnectionData connectionData)
    {
        InitializeComponent();

        Expander.Header = connectionData.Name;
        Expander.Description = connectionData.Description;

        var dashboardUrl = connectionData.DashboardUrl;
        if (dashboardUrl == null)
        {
            OpenDashboardButton.IsVisible = false;
        }
        else
        {
            OpenDashboardButton.Click += (_, _) =>
            {
                Process.Start(new ProcessStartInfo(connectionData.DashboardUrl)
                {
                    UseShellExecute = true
                });
            };
        }

        var key = connectionData.OptionKey;
        ApiKeyTextBox.Text = ApiVault.Get().GetAppConfig().GetOptionValue<string>(key);
        ApiKeyTextBox.TextChanged += (_, _) =>
        {
            ApiVault.Get().GetAppConfig().SetOptionValue(key, ApiKeyTextBox.Text);
        };

        Expander.IconSource = connectionData.IconSource;
    }
}