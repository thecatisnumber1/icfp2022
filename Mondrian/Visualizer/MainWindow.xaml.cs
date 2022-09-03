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
using DrawingPoint = System.Windows.Point;
using System.Threading;
using Mondrian;
using System.Threading.Tasks;
using Core;
using System.Windows.Media;

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
        private Picasso _problem;
        private Stack<Core.Rectangle> _selectedRects;
        private UILogger _loggerInstance;

        private int _problemWidth;
        private int _problemHeight;

        // Special request fun zone
        // Area select
        private DrawingPoint? _areaSelectOrigin;
        private bool _leftMouseDown;

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
                if (args[i].Equals("-problem", StringComparison.OrdinalIgnoreCase))
                {
                    string problemId = args[++i];
                    SelectProblemFromId(problemStrings, problemId);
                    continue;
                }

                if (args[i].Equals("-a", StringComparison.OrdinalIgnoreCase))
                {
                    string solverName = args[++i];
                    SelectSolverFromName(solverList, solverName);
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

        private void SelectSolverFromName(string[] solverList, string solverName)
        {
            for (int idx = 0; idx < solverList.Length; idx++)
            {
                if (solverList[idx].ToString().Equals(solverName, StringComparison.OrdinalIgnoreCase))
                {
                    SolverSelector.SelectedIndex = idx;
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

            int problemNum = int.Parse(ProblemSelector.SelectedItem.ToString());
            CoreImage ci = Problems.GetProblem(problemNum);
            InitialConfig initialConfig = InitialConfigs.GetInitialConfig(problemNum);
            _problemWidth = ci.Width;
            _problemHeight = ci.Height;

            _problem = new Picasso(ci, initialConfig);
            _selectedRects = new Stack<Core.Rectangle>();

            RenderImage(_problem.AllSimpleBlocks.ToList(), 1, 1);
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

        // Yup. Putting a field here. Deal with it.
        private HashSet<SimpleBlock> previouslyRenderedBlocks;
        private Bitmap renderedBitmap;

        public void RenderImageFast(List<SimpleBlock> blocks)
        {
            PixelFormat pf = PixelFormat.Format32bppArgb;
            int bitDepth = Image.GetPixelFormatSize(pf) / 8; // Bits -> bytes
            int width = _problemWidth;
            int height = _problemHeight;

            if (renderedBitmap == null)
            {
                renderedBitmap = new Bitmap(width, height, pf);
            }

            previouslyRenderedBlocks ??= new HashSet<SimpleBlock>();
            List<SimpleBlock> unrendered = blocks.Where(b => !previouslyRenderedBlocks.Contains(b)).ToList();

            LogVisualizerMessage($"Full size: {blocks.Count}. Unrendered size: {unrendered.Count()}");

            BitmapData bmpData = renderedBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, pf);

            foreach (SimpleBlock block in unrendered)
            {
                // Render
                unsafe
                {
                    // Get starting pointer
                    // Pointer of 0,0 + X offset (w/ bitdepth) + Y offset (stride handles bit depth; it's width * bit depth)
                    byte* ptr = (byte*)bmpData.Scan0 + (bitDepth * block.BottomLeft.X) + (block.BottomLeft.Y * bmpData.Stride);
                    for (int x = 0; x < block.Size.X; x++)
                    {
                        byte* drawptr = ptr;
                        for (int y = 0; y < block.Size.Y; y++)
                        {
                            *(drawptr++) = (byte)block.Color.B;
                            *(drawptr++) = (byte)block.Color.G;
                            *(drawptr++) = (byte)block.Color.R;
                            *(drawptr++) = (byte)block.Color.A;
                        }
                        ptr += bmpData.Stride;
                    }
                }
            }

            renderedBitmap.UnlockBits(bmpData);


            // Throw away anything in the set. Because doing an action makes a new block, we don't want to
            // be holding onto the reference.
            previouslyRenderedBlocks = blocks.ToHashSet();

            // While we can muck with it all we want, we still need to create a new source every time...
            OutputImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                renderedBitmap.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(width, height));
        }

        public void RenderImage(List<SimpleBlock> blocks, int score, int totalInstructionCost)
        {
            PixelFormat pf = PixelFormat.Format32bppArgb;
            int bitDepth = Image.GetPixelFormatSize(pf) / 8; // Bits -> bytes
            int width = _problemWidth;
            int height = _problemHeight;

            RGBA[] pixels = BlocksToRGBAArray(blocks, width, height);

            byte[] imageBytes = new byte[width * height * bitDepth];
            for (int i = 0; i < pixels.Length; i++ )
            {
                Core.RGBA pixel = pixels[i];

                int baseIndex = i * bitDepth;
                imageBytes[baseIndex + 2] = (byte)pixel.R;
                imageBytes[baseIndex + 1] = (byte)pixel.G;
                imageBytes[baseIndex] = (byte)pixel.B;
                imageBytes[baseIndex + 3] = (byte)pixel.A;
            }

            Bitmap b = new Bitmap(width, height, pf);
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

            // Make sure to copy this into other implementations.
            ScoreStatusText.Text = $"Score: {score:n0}. Total instruction cost: {totalInstructionCost:n0}. Instruction % of score: { totalInstructionCost / (double)score * 100.0}";
        }

        private static RGBA[] BlocksToRGBAArray(List<SimpleBlock> blocks, int width, int height)
        {
            RGBA[] frame = new RGBA[width * height];
            foreach (var block in blocks)
            {
                var frameTopLeft = new Core.Point(block.BottomLeft.X, height - block.TopRight.Y);
                var frameBottomRight = new Core.Point(block.TopRight.X, height - block.BottomLeft.Y);

                for (var y = frameTopLeft.Y; y < frameBottomRight.Y; y++)
                {
                    for (var x = frameTopLeft.X; x < frameBottomRight.X; x++)
                    {
                        frame[y * width + x] = block.Color;
                    }
                }
            }

            return frame;
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
            SolverResumeButton.IsEnabled = false;
        }

        private void SolverRunButton_OnClick(object sender, RoutedEventArgs e)
        {
            Solvers.Solver solver = Solvers.GetSolver(SolverSelector.SelectedItem.ToString());

            SolverRunButton.IsEnabled = false;

            SolverSelector.IsEnabled = false;
            ProblemSelector.IsEnabled = false;

            _tokenSource = new CancellationTokenSource();

            _loggerInstance = new UILogger(this, _tokenSource.Token, _selectedRects);

            // Load image etc.
            ResetProblem(); // Wipe all state.

            _solverTask = Task.Run(() =>
            {
                Interlocked.Increment(ref _runCount);

                solver.Invoke(_problem, new AI.AIArgs(), _loggerInstance);

                // This will totally screw me over later, but it lets a final Render call go through.
                Task.Delay(50).Wait();

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

        private void ManualMove_OnMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_leftMouseDown)
            {
                return;
            }

            _leftMouseDown = true;

            // Necessary to deal with hit-testing and rounding garbage
            DrawingPoint cursorPosition = e.GetPosition(ManualDrawCanvas);
            Core.Point gridPosition = cursorPosition.FromViewportToModel(_problemHeight);
            CursorPositionText.Text = $"{gridPosition} Mouse down";

            // Only overwrite this if it's not already set.
            _areaSelectOrigin ??= cursorPosition;
        }

        private void ManualMove_OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _leftMouseDown = false;

            if (_areaSelectOrigin != null)
            {
                // Log area
                ManualDrawCanvas.Children.Clear();

                DrawingPoint cursorPosition = e.GetPosition(ManualDrawCanvas);
                Core.Point endPosition = cursorPosition.FromViewportToModel(_problemHeight);
                Core.Point startPosition = _areaSelectOrigin.Value.FromViewportToModel(_problemHeight);

                // Also clamp
                Core.Point bottomLeft = new Core.Point(
                    Math.Min(_problemWidth, Math.Max(0, Math.Min(startPosition.X, endPosition.X))),
                    Math.Min(_problemHeight, Math.Max(0, Math.Min(startPosition.Y, endPosition.Y))));
                Core.Point topRight = new Core.Point(
                    Math.Min(_problemWidth, Math.Max(0, Math.Max(startPosition.X, endPosition.X))),
                    Math.Min(_problemHeight, Math.Max(0, Math.Max(startPosition.Y, endPosition.Y))));

                Core.Rectangle result = new Core.Rectangle(bottomLeft, topRight);
                LogVisualizerMessage($"Selected from {startPosition} to {endPosition}");
                LogVisualizerMessage($"Resulting rect: {result.BottomLeft}, {result.TopRight}");

                _selectedRects.Push(result);
                LogVisualizerMessage($"Stack size: {_selectedRects.Count}");

                _areaSelectOrigin = null;
                return;
            }
        }

        private void ManualMove_OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DrawingPoint cursorPosition = e.GetPosition(ManualDrawCanvas);
            Core.Point gridPosition = cursorPosition.FromViewportToModel(_problemHeight);
            CursorPositionText.Text = gridPosition.ToString();

            ManualDrawCanvas.Children.Clear();

            // Handle area select
            if (_areaSelectOrigin != null)
            {
                DrawingPoint origin = _areaSelectOrigin.Value;
                // Draw a box
                var selectionRect = new System.Windows.Shapes.Rectangle();
                selectionRect.Stroke = new SolidColorBrush(Colors.Navy);
                selectionRect.StrokeThickness = 0.1;
                selectionRect.Fill = new SolidColorBrush(Colors.LightSkyBlue);
                selectionRect.Opacity = 0.40;
                // Be less stupid about this...
                selectionRect.Width = Math.Abs(cursorPosition.X - origin.X);
                selectionRect.Height = Math.Abs(cursorPosition.Y - origin.Y);
                System.Windows.Controls.Canvas.SetLeft(selectionRect, Math.Min(cursorPosition.X, origin.X));
                System.Windows.Controls.Canvas.SetTop(selectionRect, Math.Min(cursorPosition.Y, origin.Y));

                ManualDrawCanvas.Children.Add(selectionRect);

                return;
            }
        }

        internal void Break()
        {
            SolverResumeButton.IsEnabled = true;
        }

        private void SolverResumeButton_OnClick(object sender, RoutedEventArgs e)
        {
            SolverResumeButton.IsEnabled = false;
            _loggerInstance.Paused = false;
        }
    }
}
