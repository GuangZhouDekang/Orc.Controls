﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionStringEditViewModel.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.Controls
{
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Collections;
    using Catel.IoC;
    using Catel.MVVM;
    using Catel.Services;
    using Catel.Threading;

    public class ConnectionStringEditViewModel : ViewModelBase
    {
        private readonly IMessageService _messageService;
        private readonly IConnectionStringBuilderService _connectionStringBuilderService;
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly ITypeFactory _typeFactory;

        private bool _isServersInitialized = false;
        private bool _isDatabasesInitialized = false;

        public ConnectionStringEditViewModel(SqlConnectionString connectionString, IMessageService messageService,
            IConnectionStringBuilderService connectionStringBuilderService, IUIVisualizerService uiVisualizerService, ITypeFactory typeFactory)
        {
            Argument.IsNotNull(() => connectionStringBuilderService);
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => typeFactory);
            Argument.IsNotNull(() => messageService);

            _messageService = messageService;
            _connectionStringBuilderService = connectionStringBuilderService;
            _uiVisualizerService = uiVisualizerService;
            _typeFactory = typeFactory;

            ConnectionString = connectionString;
            DbProvider = connectionString?.DbProvider;

            InitServers = new Command(() => InitServersAsync(), () => !IsServersRefreshing);
            RefreshServers = new Command(() => RefreshServersAsync(), () => !IsServersRefreshing);
            
            InitDatabases = new Command(() => InitDatabasesAsync(), () => !IsDatabasesRefreshing);
            RefreshDatabases = new Command(() => RefreshDatabasesAsync(), CanInitDatabases);

            TestConnection = new Command(OnTestConnection);
            ShowAdvancedOptions = new Command(OnShowAdvancedOptions);
        }

        public ConnectionStringProperty DataSource => ConnectionString?.Properties["Data Source"];
        public ConnectionStringProperty UserId => ConnectionString?.Properties["User ID"];
        public ConnectionStringProperty Password => ConnectionString?.Properties["Password"];
        public ConnectionStringProperty IntegratedSecurity => ConnectionString?.Properties["Integrated Security"];
        public ConnectionStringProperty InitialCatalog => ConnectionString?.Properties["Initial Catalog"];

        public bool CanLogOnToServer => Password != null || UserId != null;

        public bool IsServerListVisible { get; set; } = false;
        public bool IsDatabaseListVisible { get; set; } = false;
        public override string Title => "Connection properties";
        public SqlConnectionString ConnectionString { get; private set; }
        public DbProvider DbProvider { get; set; }

        private void OnDbProviderChanged()
        {
            var dbProvider = DbProvider;
            if (dbProvider == null)
            {
                return;
            }

            ConnectionString = _connectionStringBuilderService.GetConnectionString(dbProvider);
        }

        public Command RefreshServers { get; }
        public Command InitServers { get; }
        public bool IsServersRefreshing { get; private set; } = false;

        public Command TestConnection { get; }
        public Command ShowAdvancedOptions { get; }
        
        private void OnShowAdvancedOptions()
        {
            var advancedOptionsViewModel = _typeFactory.CreateInstanceWithParametersAndAutoCompletion<ConnectionStringAdvancedOptionsViewModel>();

            _uiVisualizerService.ShowDialog(advancedOptionsViewModel);
        }

        public ConnectionState ConnectionState { get; private set; } = ConnectionState.NotTested;

        private void OnTestConnection()
        {
            ConnectionState = _connectionStringBuilderService.GetConnectionState(ConnectionString);

            _messageService.ShowAsync(ConnectionState.ToString());
        }
        
        private void OnSelectedServerChaged()
        {
            _isDatabasesInitialized = false;
            InitDatabases.RaiseCanExecuteChanged();
        }

        public bool CanInitDatabases()
        {
            return !IsDatabasesRefreshing;
        }

        public Command RefreshDatabases { get; }
        public Command InitDatabases { get; }
        public bool IsDatabasesRefreshing { get; private set; } = false;

        public FastObservableCollection<string> Servers { get; } = new FastObservableCollection<string>();
        public FastObservableCollection<string> Databases { get; } = new FastObservableCollection<string>();
       
        public bool UseSqlServerAuthentication { get; set; }

        private Task InitServersAsync()
        {
            if (_isServersInitialized)
            {
                return TaskHelper.Completed;
            }

            return RefreshServersAsync();
        }

        private Task RefreshServersAsync()
        {
            IsServersRefreshing = true;

            Servers.Clear();

            return TaskHelper.RunAndWaitAsync(() =>
            {
                var servers = _connectionStringBuilderService.GetSqlServers();
                Servers.AddItems(servers);

                IsServersRefreshing = false;
                _isServersInitialized = true;
                IsServerListVisible = true;
            });
        }

        private Task InitDatabasesAsync()
        {
            if (_isDatabasesInitialized)
            {
                return TaskHelper.Completed;
            }

            return RefreshDatabasesAsync();            
        }

        private Task RefreshDatabasesAsync()
        {
            IsDatabasesRefreshing = true;

            Databases.Clear();
            
            if (UseSqlServerAuthentication)
            {
                
         //       ConnectionStringBuiler.Password = Password;
         //    ConnectionStringBuiler.UserID = UserName;
            }

         //   ConnectionStringBuiler..IntegratedSecurity = !UseSqlServerAuthentication;

            return TaskHelper.RunAndWaitAsync(() =>
            {
                var databases = _connectionStringBuilderService.GetDatabases(ConnectionString);
                Databases.AddItems(databases);

                IsDatabasesRefreshing = false;
                _isDatabasesInitialized = true;
                IsDatabaseListVisible = true;
            });
        }
    }
}