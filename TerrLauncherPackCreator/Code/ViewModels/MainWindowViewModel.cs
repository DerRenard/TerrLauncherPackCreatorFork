﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows.Controls;
using CommonLibrary.CommonUtils;
using JetBrains.Annotations;
using MVVM_Tools.Code.Commands;
using TerrLauncherPackCreator.Code.Implementations;
using TerrLauncherPackCreator.Code.Interfaces;
using TerrLauncherPackCreator.Code.Utils;
using TerrLauncherPackCreator.Pages.PackCreation;
using TerrLauncherPackCreator.Resources.Localizations;

namespace TerrLauncherPackCreator.Code.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private class ProgressManager : ViewModelBase, IProgressManager
        {
            private const int UpdateTextDelayMs = 500;
            // ReSharper disable once NotAccessedField.Local
            private readonly Timer _updateTextTimer;

            private string _initialText;
            private string[] _steps;
            private int _step;

            #region backing fields
            private string _text;
            private int _currentProgress;
            private int _maximumProgress;
            private int _remainingFilesCount;
            private bool _isIndeterminate;
            #endregion

            public string Text
            {
                get => _text;
                set
                {
                    if (!SetProperty(ref _text, value))
                        return;

                    _initialText = value;
                    _steps = new[]
                    {
                        _initialText,
                        _initialText + ".",
                        _initialText + "..",
                        _initialText + "...",
                    };
                }
            }
            public int CurrentProgress
            {
                get => _currentProgress;
                set => SetProperty(ref _currentProgress, value);
            }
            public int MaximumProgress
            {
                get => _maximumProgress;
                set => SetProperty(ref _maximumProgress, value);
            }
            public int RemainingFilesCount
            {
                get => _remainingFilesCount;
                set => SetProperty(ref _remainingFilesCount, value);
            }
            public bool IsIndeterminate
            {
                get => _isIndeterminate;
                set => SetProperty(ref _isIndeterminate, value);
            }

            public ProgressManager()
            {
                _updateTextTimer = new Timer(state =>
                {
                    if (_steps == null)
                        return;

                    if (_step >= _steps.Length)
                        _step = 0;

                    _text = _steps[_step];

                    _step++;

                    OnPropertyChanged(nameof(Text));
                }, null, 0, UpdateTextDelayMs);
            }
        }

        public PackCreationViewModel PackCreationViewModel { get; }

        public string WindowTitle { get; }
        public int CurrentStep
        {
            get => _currentStep;
            set => SetProperty(ref _currentStep, value);
        }
        private int _currentStep;

        public double InitialWindowWidth { get; }
        public double InitialWindowHeight { get; }

        [NotNull]
        public Page[] StepsPages { get; }

        [NotNull]
        public IProgressManager LoadProgressManager { get; }
        [NotNull]
        public IProgressManager SaveProgressManager { get; }
        [NotNull]
        public IFileConverter FileConverter { get; }

        [NotNull]
        public ObservableCollection<IProgressManager> ProgressManagers { get; }
        [NotNull]
        public IPackProcessor PackProcessor { get; }
        [NotNull]
        public ITempDirsProvider TempDirsProvider { get; }
        [NotNull]
        public AuthorsFileProcessor AuthorsFileProcessor { get; }

        public IActionCommand GoToPreviousStepCommand { get; }
        public IActionCommand GoToNextStepCommand { get; }

        public MainWindowViewModel()
        {
            WindowTitle = Assembly.GetEntryAssembly().GetName().Name;
            _currentStep = 1;
            InitialWindowWidth = ValuesProvider.AppSettings.MainWindowWidth;
            InitialWindowHeight = ValuesProvider.AppSettings.MainWindowHeight;

            GoToPreviousStepCommand = new ActionCommand(
                GoToPreviousStepCommand_Execute, 
                GoToPreviousStepCommand_CanExecute
            );
            GoToNextStepCommand = new ActionCommand(
                GoToNextStepCommand_Execute,
                GoToNextStepCommand_CanExecute
            );

            LoadProgressManager = new ProgressManager {Text = StringResources.LoadingProgressStep};
            SaveProgressManager = new ProgressManager {Text = StringResources.SavingProcessStep};
            FileConverter = new FileConverter();

            PackProcessor = new PackProcessor(
                LoadProgressManager,
                SaveProgressManager,
                FileConverter
            );
            TempDirsProvider = new TempDirsProvider(Paths.TempDir);
            TempDirsProvider.DeleteAll();
            
            AuthorsFileProcessor = new AuthorsFileProcessor();
            PackCreationViewModel = new PackCreationViewModel(PackProcessor, AuthorsFileProcessor);

            StepsPages = new Page[]
            {
                new PackCreationStep1(PackCreationViewModel), 
                new PackCreationStep2(PackCreationViewModel), 
                new PackCreationStep3(PackCreationViewModel), 
                new PackCreationStep4(PackCreationViewModel), 
                new PackCreationStep5(PackCreationViewModel) 
            };

            ProgressManagers = new ObservableCollection<IProgressManager>
            {
                LoadProgressManager,
                SaveProgressManager
            };

            PropertyChanged += OnPropertyChanged;
        }

        private bool GoToPreviousStepCommand_CanExecute()
        {
            return CurrentStep > 1;
        }

        private void GoToPreviousStepCommand_Execute()
        {
            CurrentStep--;
        }

        private bool GoToNextStepCommand_CanExecute()
        {
            return CurrentStep < StepsPages.Length;
        }

        private void GoToNextStepCommand_Execute()
        {
            CurrentStep++;
        }

        public void OnWindowClosed(int actualWidth, int actualHeight)
        {
            var appSettings = ValuesProvider.AppSettings;
            appSettings.MainWindowWidth = actualWidth;
            appSettings.MainWindowHeight = actualHeight;
            try
            {
                AppUtils.SaveAppSettings(appSettings);
            }
            catch (Exception ex)
            {
                MessageBoxUtils.ShowError($"{StringResources.CantSaveAppSettings} {ex.Message}");
            }
        }
        
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CurrentStep):
                    GoToPreviousStepCommand.RaiseCanExecuteChanged();
                    GoToNextStepCommand.RaiseCanExecuteChanged();
                    break;
            }
        }
    }
}
