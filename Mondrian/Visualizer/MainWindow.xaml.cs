using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace Visualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string AllProblemsRelativePath = @"..\..\..\..\Problems";

        public MainWindow()
        {
            InitializeComponent();

            string[] problemFiles = Directory.GetFiles(AllProblemsRelativePath, "*.png");
            List<int> problemStrings = problemFiles.Select(file => int.Parse(Path.GetFileNameWithoutExtension(file))).ToList();
            problemStrings.Sort();
            ProblemSelector.ItemsSource = problemStrings;
        }

        private void SelectProblemFromId(List<string> problemStrings, string problemId)
        {
            for (int idx = 0; idx < problemStrings.Count; idx++)
            {
                if (problemStrings[idx].Equals(problemId, StringComparison.OrdinalIgnoreCase))
                {
                    ProblemSelector.SelectedIndex = idx;
                    break;
                }
            }
        }

        private void ResetProblem()
        {
            string filePath = Path.GetFullPath(Path.Combine(AllProblemsRelativePath, $"{ProblemSelector.SelectedItem}.png"));

            // ... Create a new BitmapImage.
            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri(filePath);
            b.EndInit();

            ReferenceImage.Source = b;
        }

        internal void LogMessage(string message)
        {
            ConsoleLogPanel.Text += $"{message}{Environment.NewLine}";
            ConsoleLogScroller.ScrollToBottom();
        }

        private void LogVisualizerMessage(string message)
        {
            LogMessage($"[VIZ] {message}");
        }

        private void ProblemSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetProblem();
        }
    }
}
