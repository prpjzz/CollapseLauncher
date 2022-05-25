﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

using Windows.Graphics;

using WinRT;
using WinRT.Interop;

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Composition.SystemBackdrops;

using static CollapseLauncher.InnerLauncherConfig;
using static Hi3Helper.Logger;
using static Hi3Helper.Shared.Region.LauncherConfig;

namespace CollapseLauncher
{
    public sealed partial class MainWindow : Window
    {
        public void SetThemeParameters()
        {
            switch (m_currentBackdrop)
            {
#if MICA
                case BackdropType.Mica:
                    {
                        (Application.Current.Resources["PagesSolidAcrylicBrush"] as AcrylicBrush).TintOpacity = 0f;
                        (Application.Current.Resources["PagesSolidAcrylicBrush"] as AcrylicBrush).TintLuminosityOpacity = 0f;
                        (Application.Current.Resources["DialogAcrylicBrush"] as AcrylicBrush).TintOpacity = 0f;
                        (Application.Current.Resources["DialogAcrylicBrush"] as AcrylicBrush).TintLuminosityOpacity = 0.75f;
                        (Application.Current.Resources["NavigationBarBrush"] as AcrylicBrush).TintOpacity = 0f;
                        (Application.Current.Resources["NavigationBarBrush"] as AcrylicBrush).TintLuminosityOpacity = 0f;
                    }
                    break;
#endif
                case BackdropType.DefaultColor:
                    {
                        if (CurrentRequestedAppTheme == ApplicationTheme.Dark)
                        {
                            Application.Current.Resources["NavigationBarBrush"] = new AcrylicBrush
                            {
                                TintColor = new Windows.UI.Color { A = 244, R = 34, G = 34, B = 34 },
                                TintOpacity = 1f,
                                TintLuminosityOpacity = 0f
                            };
                            Application.Current.Resources["PagesSolidAcrylicBrush"] = new AcrylicBrush
                            {
                                TintColor = new Windows.UI.Color { A = 244, R = 34, G = 34, B = 34 },
                                TintOpacity = 1f,
                                TintLuminosityOpacity = 0f
                            };
                            Application.Current.Resources["DialogAcrylicBrush"] = new AcrylicBrush
                            {
                                TintColor = new Windows.UI.Color { A = 244, R = 34, G = 34, B = 34 },
                                TintOpacity = 0.4f,
                                TintLuminosityOpacity = 0.5f
                            };
                        }
                    }
                    break;
            }
        }
    }
}