﻿using System.Windows;
using TerrLauncherPackCreator.Resources.Localizations;

namespace TerrLauncherPackCreator.Code.Utils
{
    public static class MessageBoxUtils
    {
        public static void ShowError(string text)
        {
            MessageBox.Show(
                text,
                StringResources.ErrorLower,
                MessageBoxButton.OK,
                MessageBoxImage.Exclamation
            );
        }
    }
}
