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

                var viewBuilder = new ResultViewBuilder();

                ComponentsGrid.ItemsSource = viewBuilder.BuildComponents(response);
                BomsGrid.ItemsSource = viewBuilder.BuildBoms(response);
                BomLinesGrid.ItemsSource = viewBuilder.BuildBomLines(response);
                MessagesGrid.ItemsSource = viewBuilder.BuildMessages(response);

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