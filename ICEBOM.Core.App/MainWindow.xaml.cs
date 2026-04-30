using System.Windows;

using ICEBOM.Core.Domain.Services;

using Microsoft.Win32;
using ICEBOM.Core.App.ViewModels;
using ICEBOM.Core.App.Services;

namespace ICEBOM.Core.App
{
    public partial class MainWindow : Window
    {
        private string? _selectedJsonPath;

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

        private void ProcessJson_Click(object sender, RoutedEventArgs e)
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

                var processor = new ICEBOMProcessor();
                var response = processor.Process(request);

                var responseWriter = new ResponseWriter();
                var responsePath = responseWriter.Write(_selectedJsonPath, response);

                ComponentsGrid.ItemsSource = response.Components.Select(c => new ComponentResultView
                {
                    InternalId = c.InternalId,
                    Reference = c.Reference,
                    Status = c.Status,
                    Action = c.Action.ToString(),
                    ErrorsText = string.Join(" | ", c.Errors.Select(e => e.Message)),
                    WarningsText = string.Join(" | ", c.Warnings.Select(w => w.Message))
                }).ToList();

                BomsGrid.ItemsSource = response.Boms.Select(b => new BomResultView
                {
                    BomId = b.BomId,
                    ProductReference = b.ProductReference,
                    Status = b.Status,
                    Action = b.Action.ToString(),
                    ErrorsText = string.Join(" | ", b.Errors.Select(e => e.Message)),
                    WarningsText = string.Join(" | ", b.Warnings.Select(w => w.Message))
                }).ToList();

                var messages = new List<MessageView>();

                foreach (var c in response.Components)
                {
                    messages.AddRange(c.Errors.Select(e => new MessageView
                    {
                        Type = "Error",
                        Code = e.Code,
                        Message = e.Message,
                        Source = $"Component: {c.InternalId}"
                    }));

                    messages.AddRange(c.Warnings.Select(w => new MessageView
                    {
                        Type = "Warning",
                        Code = w.Code,
                        Message = w.Message,
                        Source = $"Component: {c.InternalId}"
                    }));
                }

                foreach (var b in response.Boms)
                {
                    messages.AddRange(b.Errors.Select(e => new MessageView
                    {
                        Type = "Error",
                        Code = e.Code,
                        Message = e.Message,
                        Source = $"BOM: {b.BomId}"
                    }));

                    messages.AddRange(b.Warnings.Select(w => new MessageView
                    {
                        Type = "Warning",
                        Code = w.Code,
                        Message = w.Message,
                        Source = $"BOM: {b.BomId}"
                    }));
                }

                MessagesGrid.ItemsSource = messages;

                ResultText.Text =
                    $"Estado: {response.Summary.Status}\n" +
                    $"Componentes: {response.Summary.TotalComponents}\n" +
                    $"BOMs: {response.Summary.TotalBoms}\n" +
                    $"Errores: {response.Summary.ErrorsCount}\n" +
                    $"Avisos: {response.Summary.WarningsCount}\n\n" +
                    $"Response guardado en:\n{responsePath}";
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
    }
}