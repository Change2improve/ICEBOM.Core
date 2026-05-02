using System;
using System.IO;
using System.Windows;

using ICEBOM.Core.App.Services;
using ICEBOM.Core.App.ViewModels;
using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Models.Odoo;
using ICEBOM.Core.Domain.Normalizers;
using ICEBOM.Core.Domain.Odoo.Auth;
using ICEBOM.Core.Domain.Odoo.Services;
using ICEBOM.Core.Domain.Repositories;
using ICEBOM.Core.Domain.Services;

using Microsoft.Win32;

namespace ICEBOM.Core.App
{
    public partial class MainWindow : Window
    {
        private string? _selectedJsonPath;

        private string? _selectedConfigPath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectJson_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Seleccionar ICEBOM_Request.json",
                Filter = "Archivos JSON (*.json)|*.json|Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedJsonPath = dialog.FileName;
                SelectedFileText.Text = _selectedJsonPath;
                ResultText.Text = string.Empty;
            }
        }

        private async void ProcessJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_selectedJsonPath))
                {
                    MessageBox.Show(
                        "Selecciona primero un archivo JSON.",
                        "ICEBOM Core",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                var requestReader = new JsonRequestReader();
                var request = requestReader.Read(_selectedJsonPath);

                var configPath = !string.IsNullOrWhiteSpace(_selectedConfigPath)
                    ? _selectedConfigPath
                    : Path.Combine(AppContext.BaseDirectory, "Config", "customer_config.json");

                var configReader = new CustomerConfigReader();
                var customerConfig = configReader.Read(configPath);

                ICEBOMSettingsMapper.ApplyCustomerConfig(request, customerConfig);
                ICEBOMSettingsMapper.ApplyFunctionalTypeAliases(request, customerConfig);
                ICEBOMSettingsMapper.ApplyDefaultFunctionalType(request, customerConfig);

                var unitNormalizer = new ICEBOMUnitNormalizer(customerConfig.Units);
                var fakeRepository = new FakeOdooRepository(customerConfig.FakeOdoo);
                var traceService = new ICEBOMTraceService();

                ICEBOMProcessor processor;
                ICEBOMResponse response;

                if (customerConfig.OdooMode.Mode == ICEBOMOdooModeEnum.Real)
                {
                    var authenticator = new OdooAuthenticator();

                    traceService.Add(
                        "OdooLogin",
                        "Odoo",
                        customerConfig.OdooConnection.Database,
                        $"Intentando login en Odoo. Url='{customerConfig.OdooConnection.Url}', Database='{customerConfig.OdooConnection.Database}', User='{customerConfig.OdooConnection.Username}'.");

                    var loginResult = await authenticator.AuthenticateAsync(customerConfig.OdooConnection);

                    if (!loginResult.Success || loginResult.Session == null)
                    {
                        traceService.Add(
                            "OdooLogin",
                            "Odoo",
                            customerConfig.OdooConnection.Database,
                            $"Login FAIL en Odoo. Url='{customerConfig.OdooConnection.Url}', Database='{customerConfig.OdooConnection.Database}', User='{customerConfig.OdooConnection.Username}'. {loginResult.UserMessage}");

                        MessageBox.Show(
                            loginResult.UserMessage,
                            "Error conectando con Odoo",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        return;
                    }

                    traceService.Add(
                        "OdooLogin",
                        "Odoo",
                        customerConfig.OdooConnection.Database,
                        $"Login OK en Odoo. Url='{customerConfig.OdooConnection.Url}', Database='{customerConfig.OdooConnection.Database}', User='{customerConfig.OdooConnection.Username}', UserId={loginResult.UserId}.");

                    var executionOptions = new OdooExecutionOptions
                    {
                        DryRun = customerConfig.OdooMode.DryRun
                    };

                    var asyncRepository = OdooRepositoryFactory.Create(
                        customerConfig.OdooMode,
                        customerConfig.FakeOdoo,
                        loginResult.Session,
                        executionOptions);

                    var odooSyncService = new OdooSyncService(
                        asyncRepository,
                        executionOptions,
                        traceService);

                    processor = new ICEBOMProcessor(
                        fakeRepository,
                        unitNormalizer,
                        customerConfig.BusinessRules,
                        customerConfig.UnsupportedFeatures,
                        asyncRepository,
                        odooSyncService,
                        traceService);

                    response = await processor.ProcessAsync(request);
                }
                else
                {
                    processor = new ICEBOMProcessor(
                        fakeRepository,
                        unitNormalizer,
                        customerConfig.BusinessRules,
                        customerConfig.UnsupportedFeatures,
                        traceService);

                    response = processor.Process(request);
                }

                response.Meta.CustomerName = customerConfig.CustomerName;
                response.Meta.CustomerConfigVersion = customerConfig.ConfigVersion;
                response.Meta.CustomerConfigPath = configPath;

                response.Meta.CreateMissingProducts = customerConfig.SyncPolicy.CreateMissingProducts;
                response.Meta.UpdateExistingProducts = customerConfig.SyncPolicy.UpdateExistingProducts;
                response.Meta.CreateMissingBoms = customerConfig.SyncPolicy.CreateMissingBoms;
                response.Meta.UpdateExistingBoms = customerConfig.SyncPolicy.UpdateExistingBoms;
                response.Meta.AllowProductVariants = customerConfig.SyncPolicy.AllowProductVariants;

                if (!customerConfig.Execution.IncludeTrace)
                    response.Trace.Clear();

                string traceLogPath = string.Empty;

                if (customerConfig.Execution.IncludeTrace && response.Trace.Count > 0)
                {
                    var traceLogWriter = new TraceLogWriter();
                    traceLogPath = traceLogWriter.Write(_selectedJsonPath, response);
                }

                var responseWriter = new ResponseWriter();
                var responsePath = responseWriter.Write(_selectedJsonPath, response);

                var viewBuilder = new ResultViewBuilder();

                ConfigGrid.ItemsSource = viewBuilder.BuildConfigItems(customerConfig);
                ComponentsGrid.ItemsSource = viewBuilder.BuildComponents(response);
                BomsGrid.ItemsSource = viewBuilder.BuildBoms(response);
                BomLinesGrid.ItemsSource = viewBuilder.BuildBomLines(response);
                MessagesGrid.ItemsSource = viewBuilder.BuildMessages(response);
                TraceGrid.ItemsSource = viewBuilder.BuildTrace(response);

                ResultText.Text =
                    $"Cliente config: {customerConfig.CustomerName}\n" +
                    $"Versión config: {customerConfig.ConfigVersion}\n\n" +
                    $"Odoo mode: {customerConfig.OdooMode.Mode}\n" +
                    $"Dry-run: {customerConfig.OdooMode.DryRun}\n\n" +
                    $"ExecutionId: {response.Meta.ExecutionId}\n\n" +
                    $"Estado: {response.Summary.Status}\n" +
                    $"Componentes: {response.Summary.TotalComponents}\n" +
                    $"BOMs: {response.Summary.TotalBoms}\n" +
                    $"Errores: {response.Summary.ErrorsCount}\n" +
                    $"Avisos: {response.Summary.WarningsCount}\n\n" +
                    $"Response guardado en:\n{responsePath}" +
                    $"{(string.IsNullOrWhiteSpace(traceLogPath) ? "" : $"\n\nTrace guardado en:\n{traceLogPath}")}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error procesando JSON",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SelectConfig_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Seleccionar customer_config.json",
                Filter = "Archivos JSON (*.json)|*.json|Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedConfigPath = dialog.FileName;
                SelectedConfigText.Text = _selectedConfigPath;
                ResultText.Text = string.Empty;
            }
        }
    }
}