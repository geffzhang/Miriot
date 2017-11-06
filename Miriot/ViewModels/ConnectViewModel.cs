﻿using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Miriot.Common;
using Miriot.Core.ViewModels;
using Miriot.Model;
using Miriot.Services;
using Miriot.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Miriot
{
    public class ConnectViewModel : CustomViewModel
    {
        private string _message;

        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        public RomeRemoteSystem SelectedRemoteSystem
        {
            get { return _selectedSystem; }
            set { Set(ref _selectedSystem, value); }
        }

        private RelayCommand<RomeRemoteSystem> _selectCommand;
        public RelayCommand<RomeRemoteSystem> SelectCommand
        {
            get
            {
                if (_selectCommand == null) _selectCommand = new RelayCommand<RomeRemoteSystem>(OnSelect);
                return _selectCommand;
            }
        }

        private RelayCommand _connectCommand;
        public RelayCommand ConnectCommand
        {
            get
            {
                if (_connectCommand == null) _connectCommand = new RelayCommand(OnConnect);
                return _connectCommand;
            }
        }

        private RelayCommand<string> _sendCommand;
        public RelayCommand<string> SendCommand
        {
            get
            {
                if (_sendCommand == null) _sendCommand = new RelayCommand<string>(OnCommand);
                return _sendCommand;
            }
        }

        public ObservableCollection<RomeRemoteSystem> RemoteSystems { get; set; }

        private IRomeService _romeService;
        private readonly IDispatcherService _dispatcherService;
        private readonly INavigationService _navigationService;
        private RomeRemoteSystem _selectedSystem;

        public ConnectViewModel(
            IRomeService romeService, 
            IDispatcherService dispatcherService,
            INavigationService navigationService)
        {
            _romeService = romeService;
            _dispatcherService = dispatcherService;
            _navigationService = navigationService;
            RemoteSystems = new ObservableCollection<RomeRemoteSystem>();
        }

        public async Task InitializeAsync()
        {
            _romeService.Added = OnAdded;
            await _romeService.InitializeAsync();
        }

        private void OnAdded(RomeRemoteSystem obj)
        {
            _dispatcherService.Invoke(() =>
            {
                RemoteSystems.Add(obj);
            });
        }

        private void OnRemoved(RomeRemoteSystem obj)
        {
            _dispatcherService.Invoke(() =>
            {
                RemoteSystems.Remove(obj);
            });
        }

        private void OnSelect(RomeRemoteSystem sys)
        {
            SelectedRemoteSystem = sys;
        }

        private void OnCommand(string obj)
        {
            _romeService.SendCommandAsync(SelectedRemoteSystem, obj);
        }

        private async void OnConnect()
        {
            Message = "Connecting...";

            var success = await _romeService.ConnectAsync(SelectedRemoteSystem);

            if (success)
            {
                Message = "Retrieving user...";

                var user = await _romeService.GetRemoteUserAsync(SelectedRemoteSystem);

                if (user == null)
                    Message = "Unable to retrieve user";
                else
                {
                    _navigationService.NavigateTo(PageKeys.Settings, user);
                }
            }
            else
            {
                Message = "Unable to connect";
            }
        }
    }
}
