using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;
using CoreImage = Core.Image;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Rectangle = System.Drawing.Rectangle;
using Image = System.Drawing.Image;
using System.Threading;
using Mondrian;
using System.Threading.Tasks;
using Core;

namespace Visualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string AllProblemsRelativePath = @"..\..\..\..\..\Problems";
        private long _runCount = 0;

        // Solver running in the visualizer goo
        private Task _solverTask;
        private CancellationTokenSource _tokenSource;

        private int _problemWidth;
        private int _problemHeight;

        public MainWindow(string[] args)
        {
            InitializeComponent();

            string[] problemFiles = Directory.GetFiles(AllProblemsRelativePath, "*.png");
            List<int> problemStrings = problemFiles.Select(file => int.Parse(Path.GetFileNameWithoutExtension(file))).ToList();
            problemStrings.Sort();
            ProblemSelector.ItemsSource = problemStrings;

            string[] solverList = Solvers.Names();
            SolverSelector.ItemsSource = solverList;

            var otherArgs = new List<string>();
            // Parse args
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("/Problem", StringComparison.OrdinalIgnoreCase))
                {
                    string problemId = args[++i];
                    SelectProblemFromId(problemStrings, problemId);
                    continue;
                }

                otherArgs.Add(args[i]);
            }

            ArgumentsTextBox.Text = string.Join(' ', otherArgs);

            // Have some default selected
            if (ProblemSelector.SelectedIndex < 0)
            {
                ProblemSelector.SelectedIndex = 0;
            }

            if (SolverSelector.SelectedIndex < 0)
            {
                SolverSelector.SelectedIndex = 0;
            }
        }

        private void SelectProblemFromId(List<int> problemStrings, string problemId)
        {
            for (int idx = 0; idx < problemStrings.Count; idx++)
            {
                if (problemStrings[idx].ToString().Equals(problemId, StringComparison.OrdinalIgnoreCase))
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

            _problemWidth = (int)b.Width;
            _problemHeight = (int)b.Height;

            CoreImage ci = Mondrian.Problems.GetProblem(int.Parse(ProblemSelector.SelectedItem.ToString()));

            RenderImage(ci);
        }

        public void RenderImage(CoreImage image)
        {
            PixelFormat pf = PixelFormat.Format32bppArgb;
            int bitDepth = Image.GetPixelFormatSize(pf) / 8; // Bits -> bytes

            int width = image.Width;
            int height = image.Height;
            // Convert the array into a pure byte array, 32bpp ARGB.
            byte[] imageBytes = new byte[width * height * bitDepth];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Core.RGBA pixel = image[x, y];

                    int baseIndex = (x + y * height) * bitDepth;
                    // TODO: Abuse struct
                    imageBytes[baseIndex + 2] = (byte)pixel.R;
                    imageBytes[baseIndex + 1] = (byte)pixel.G;
                    imageBytes[baseIndex] = (byte)pixel.B;
                    imageBytes[baseIndex + 3] = (byte)pixel.A;
                }
            }

            // Copied from http://mapw.elte.hu/elek/bmpinmemory.html
            Bitmap b = new Bitmap(image.Width, image.Height, pf);
            BitmapData bmpData = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, pf);
            IntPtr ptr = bmpData.Scan0;
            Int32 psize = bmpData.Stride * height;
            System.Runtime.InteropServices.Marshal.Copy(imageBytes, 0, ptr, psize);
            b.UnlockBits(bmpData);

            // Copied from https://stackoverflow.com/questions/94456/load-a-wpf-bitmapimage-from-a-system-drawing-bitmap
            OutputImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                b.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(width, height));
        }

        public void RenderImage(Picasso image)
        {
            PixelFormat pf = PixelFormat.Format32bppArgb;
            int bitDepth = Image.GetPixelFormatSize(pf) / 8; // Bits -> bytes

            Bitmap b = new Bitmap(_problemWidth, _problemHeight, pf);
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

        private void SolverSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetSolverButtons();
        }

        private void ResetSolverButtons()
        {
            SolverRunButton.IsEnabled = true;
            SolverStopButton.IsEnabled = false;
            if (Interlocked.Read(ref _runCount) == 0)
            {
                SolverResetButton.IsEnabled = false;
            }
        }

        private void SolverRunButton_OnClick(object sender, RoutedEventArgs e)
        {
            Solvers.Solver solver = Solvers.GetSolver(SolverSelector.SelectedItem.ToString());

            SolverRunButton.IsEnabled = false;

            SolverSelector.IsEnabled = false;
            ProblemSelector.IsEnabled = false;

            _tokenSource = new CancellationTokenSource();

            LoggerBase logger = new UILogger(this, _tokenSource.Token);

            // Load image etc.
            CoreImage ci = Problems.GetProblem(int.Parse(ProblemSelector.SelectedItem.ToString()));

            _solverTask = Task.Run(() =>
            {
                Interlocked.Increment(ref _runCount);

                solver.Invoke(new Picasso(ci), new AI.AIArgs(), logger);

                // Solver has finished, but if they ran to completion, the UI logger might still be pumping. Kill it with our token.
                if (!_tokenSource.IsCancellationRequested)
                {
                    _tokenSource.Cancel();
                }

                // Cleanup UI on the main thread.
                Dispatcher.BeginInvoke(() =>
                {
                    LogVisualizerMessage("Done!");
                    SolverSelector.IsEnabled = true;
                    ProblemSelector.IsEnabled = true;

                    ResetSolverButtons();
                });
            });
        }
    }
}
