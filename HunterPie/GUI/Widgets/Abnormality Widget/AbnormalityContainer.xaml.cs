﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets.Abnormality_Widget {
    /// <summary>
    /// Interaction logic for AbnormalityContainer.xaml
    /// </summary>
    public partial class AbnormalityContainer : Widget {

        Dictionary<string, Parts.AbnormalityControl> ActiveAbnormalities = new Dictionary<string, Parts.AbnormalityControl>();
        AbnormalityTraySettings AbnormalityWidgetSettings;
        Player Context;
        int AbnormalityTrayIndex;

        public AbnormalityContainer(Player context, int TrayIndex) {
            InitializeComponent();
            BaseWidth = Width;
            BaseHeight = Height;
            AbnormalityTrayIndex = TrayIndex;
            ApplySettings();
            SetWindowFlags();
            SetContext(context);
        }

        public override void ApplySettings() {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                this.WidgetActive = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Enabled;
                this.Top = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Position[1];
                this.Left = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Position[0];
                this.BuffTray.Orientation = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Orientation == "Horizontal" ? Orientation.Horizontal : Orientation.Vertical;
                int BuffTrayMaxSize = Math.Max(UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].MaxSize, 0);
                if (this.BuffTray.Orientation == Orientation.Horizontal) {
                    this.BuffTray.MaxWidth = BuffTrayMaxSize == 0 ? int.MaxValue : BuffTrayMaxSize;
                } else {
                    this.BuffTray.MaxHeight = BuffTrayMaxSize == 0 ? int.MaxValue : BuffTrayMaxSize;
                }
                base.ApplySettings();
            }));
        }

        private void SaveSettings() {
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].MaxSize = BuffTray.Orientation == Orientation.Horizontal ? (int)BuffTray.MaxWidth : (int)BuffTray.MaxHeight;
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Orientation = BuffTray.Orientation == Orientation.Horizontal ? "Horizontal" : "Vertical";
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].Scale = DefaultScaleX;
        }

        private void SetContext(Player ctx) {
            Context = ctx;
            HookEvents();
        }

        public override void EnterWidgetDesignMode() {
            base.EnterWidgetDesignMode();
            this.ResizeMode = ResizeMode.CanResizeWithGrip;
            this.SettingsButton.Visibility = Visibility.Visible;
            RemoveWindowTransparencyFlag();
        }

        public override void LeaveWidgetDesignMode() {
            base.LeaveWidgetDesignMode();
            this.ResizeMode = ResizeMode.CanResize;
            SizeToContent = SizeToContent.WidthAndHeight;
            this.SettingsButton.Visibility = Visibility.Collapsed;
            ApplyWindowTransparencyFlag();
            SaveSettings();
        }

        #region Game events

        private void HookEvents() {
            Context.Abnormalities.OnNewAbnormality += OnPlayerNewAbnormality;
            Context.Abnormalities.OnAbnormalityRemove += OnPlayerAbnormalityEnd;
        }

        private void UnhookEvents() {
            Context.Abnormalities.OnNewAbnormality -= OnPlayerNewAbnormality;
            Context.Abnormalities.OnAbnormalityRemove -= OnPlayerAbnormalityEnd;
        }

        private void OnPlayerAbnormalityEnd(object source, AbnormalityEventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() => {
                this.ActiveAbnormalities.Remove(args.Abnormality.InternalID);
                this.RedrawComponent();
            }));
        }

        private void OnPlayerNewAbnormality(object source, AbnormalityEventArgs args) {
            if (!UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[AbnormalityTrayIndex].AcceptedAbnormalities.Contains(args.Abnormality.InternalID)) return;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() => {
                this.WidgetHasContent = true;
                Parts.AbnormalityControl AbnormalityBox = new Parts.AbnormalityControl(args.Abnormality);
                this.ActiveAbnormalities.Add(args.Abnormality.InternalID, AbnormalityBox);
                this.RedrawComponent();
            }));
        }

        #endregion

        #region Rendering

        private void RedrawComponent() {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                this.BuffTray.Children.Clear();
                if (this.ActiveAbnormalities.Count == 0) {
                    this.WidgetHasContent = false;
                }
                ChangeVisibility();
                foreach (Parts.AbnormalityControl Abnorm in ActiveAbnormalities.Values) {
                    this.BuffTray.Children.Add(Abnorm);
                }
            }));
        }

        #endregion


        #region Window events

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            this.UnhookEvents();
        }

        private void OnMouseEnter(object sender, MouseEventArgs e) {
            this.MouseOver = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                this.MoveWidget();
            } else if (e.RightButton == MouseButtonState.Pressed) {

            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e) {
            if (this.MouseOver) {
                if (e.Delta > 0) {
                    //ScaleWidget(DefaultScaleX + 0.05, DefaultScaleY + 0.05);
                } else {
                    //ScaleWidget(DefaultScaleX - 0.05, DefaultScaleY - 0.05);
                }
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e) {
            this.MouseOver = false;
        }

        private void OnSizeChange(object sender, SizeChangedEventArgs e) {

            // This means the user didn't resize the widget
            if (this.BuffTray.ActualWidth + 4 == e.NewSize.Width && this.BuffTray.ActualHeight + 4 == e.NewSize.Height) return;
            // Only resize if in design mode
            if (!this.InDesignMode) return;
            // Resize depending on the orientation
            if (this.BuffTray.Orientation == Orientation.Horizontal) {
                if (e.NewSize.Width < 40) return;
                this.BuffTray.MaxWidth = e.NewSize.Width;
            } else {
                if (e.NewSize.Height < 40) return;
                this.BuffTray.MaxHeight = e.NewSize.Height;
            }
        }

        private void OnSettingsButtonClick(object sender, MouseButtonEventArgs e) {
            if (this.AbnormalityWidgetSettings == null || this.AbnormalityWidgetSettings.IsClosed) {
                AbnormalityWidgetSettings = new AbnormalityTraySettings(this, this.AbnormalityTrayIndex);
                AbnormalityWidgetSettings.Show();
            }
        }
        #endregion


    }
}
