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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

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
        private int _problemId;
        private List<Core.Rectangle> _selectedRects;
        private UILogger _loggerInstance;

        private int _problemWidth;
        private int _problemHeight;

        // Special request fun zone
        // Area select
        private DrawingPoint? _areaSelectOrigin;
        private bool _leftMouseDown;

        // Options
        private bool _showSelectedRectOnTop;
        private bool _debugSpew;

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
                if (args[i].Equals("-problem", StringComparison.OrdinalIgnoreCase) || args[i].Equals("-p", StringComparison.OrdinalIgnoreCase))
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

                if (args[i].Equals("-sot", StringComparison.OrdinalIgnoreCase))
                {
                    SelectedRectOnTopCheckbox.IsChecked = true;
                }

                if (args[i].Equals("-vdbg", StringComparison.OrdinalIgnoreCase))
                {
                    UIDebugSpewCheckbox.IsChecked = true;
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

        private void ResetProblem(bool clearSelectedRects = false)
        {
            string filePath = Path.GetFullPath(Path.Combine(AllProblemsRelativePath, $"{ProblemSelector.SelectedItem}.png"));

            // ... Create a new BitmapImage.
            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri(filePath);
            b.EndInit();

            ReferenceImage.Source = b;

            _problemId = int.Parse(ProblemSelector.SelectedItem.ToString());
            CoreImage ci = Problems.GetProblem(_problemId);
            InitialConfig initialConfig = InitialConfigs.GetInitialConfig(_problemId);
            _problemWidth = ci.Width;
            _problemHeight = ci.Height;

            _problem = new Picasso(ci, initialConfig);

            if (clearSelectedRects)
            {
                _selectedRects = new List<Core.Rectangle>();
                RectStack.ItemsSource = _selectedRects;
                SelectedRectCanvas.Children.Clear();
            }

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
            ScoreStatusText.Text = $"Score: {score:n0}. Total instruction cost: {totalInstructionCost:n0}. Instruction % of score: {(totalInstructionCost / (double)score):P}";
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
            if (UIDebugSpewCheckbox.IsChecked.HasValue && UIDebugSpewCheckbox.IsChecked.Value)
            {
                LogMessage($"[VIZ] {message}");
            }
        }

        private void ProblemSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetProblem(true);
        }

        private void SolverSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetSolverButtons();
        }

        private void ResetSolverButtons()
        {
            SolverRunButton.IsEnabled = true;
            SolverResumeButton.IsEnabled = false;
            SolverBreakButton.IsEnabled = false;
        }

        private void SolverRunButton_OnClick(object sender, RoutedEventArgs e)
        {
            Solvers.Solver solver = Solvers.GetSolver(SolverSelector.SelectedItem.ToString());

            SolverRunButton.IsEnabled = false;
            SolverBreakButton.IsEnabled = true; // Allow debug breaks

            SolverSelector.IsEnabled = false;
            ProblemSelector.IsEnabled = false;

            _tokenSource = new CancellationTokenSource();

            _loggerInstance = new UILogger(this, _tokenSource.Token, _selectedRects);

            // This should probably happen *after* the task runs
            ResetProblem(); // Wipe all state.

            _solverTask = Task.Run(() =>
            {
                Interlocked.Increment(ref _runCount);

                solver.Invoke(_problem, new AI.AIArgs(_problemId), _loggerInstance);

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
            DrawingPoint cursorPosition = sender == MouseLayer ? e.GetPosition(ManualDrawCanvas) : e.GetPosition(OutputImage);
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

                DrawingPoint cursorPosition = sender == MouseLayer ? e.GetPosition(ManualDrawCanvas) : e.GetPosition(OutputImage);

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
                if (result.Width == 0 || result.Height == 0)
                {
                    LogVisualizerMessage("Ignoring zero width/height rectangle");
                }
                else
                {
                    LogVisualizerMessage($"Selected from {startPosition} to {endPosition}");
                    LogVisualizerMessage($"Resulting rect: {result.BottomLeft}, {result.TopRight}");

                    _selectedRects.Insert(0, result);
                    DrawSelectedRects(true);
                }

                _areaSelectOrigin = null;
                return;
            }
        }

        private void ManualMove_OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DrawingPoint cursorPosition = sender == MouseLayer ? e.GetPosition(ManualDrawCanvas) : e.GetPosition(OutputImage);

            Core.Point gridPosition = cursorPosition.FromViewportToModel(_problemHeight);
            CursorPositionText.Text = gridPosition.ToString();

            ManualDrawCanvas.Children.Clear();

            // Draw crosshairs
            // Horizontal
            var horizontalRect = new System.Windows.Shapes.Rectangle();
            horizontalRect.Stroke = new SolidColorBrush(Colors.Purple);
            horizontalRect.StrokeThickness = 0.5;
            horizontalRect.Opacity = 1.0;
            horizontalRect.Width = _problemWidth;
            horizontalRect.Height = 0.5;
            System.Windows.Controls.Canvas.SetLeft(horizontalRect, 0);
            System.Windows.Controls.Canvas.SetTop(horizontalRect, cursorPosition.Y - 0.25);

            ManualDrawCanvas.Children.Add(horizontalRect);
            // Vertical
            var verticalRect = new System.Windows.Shapes.Rectangle();
            verticalRect.Stroke = new SolidColorBrush(Colors.Purple);
            verticalRect.StrokeThickness = 0.5;
            verticalRect.Opacity = 1.0;
            verticalRect.Width = 0.5;
            verticalRect.Height = _problemHeight;
            System.Windows.Controls.Canvas.SetLeft(verticalRect, cursorPosition.X - 0.25);
            System.Windows.Controls.Canvas.SetTop(verticalRect, 0);

            ManualDrawCanvas.Children.Add(verticalRect);


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
            SolverBreakButton.IsEnabled = false;
        }

        private void SolverResumeButton_OnClick(object sender, RoutedEventArgs e)
        {
            SolverBreakButton.IsEnabled = true;
            SolverResumeButton.IsEnabled = false;
            _loggerInstance.SolverPaused = false;
            _loggerInstance.PauseRequested = false;
        }

        private void SolverBreakButton_OnClick(object sender, RoutedEventArgs e)
        {
            LogVisualizerMessage("Pause requested");
            _loggerInstance.PauseRequested = true;
        }

        private void Execute_Undo(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (_selectedRects != null)
            {
                if (_selectedRects.Count > 0)
                {
                    _selectedRects.RemoveAt(0);
                    DrawSelectedRects(true);
                }
            }
        }

        private void DrawSelectedRects(bool reloadList = false)
        {
            // Figure out which rectangle to highlight
            Core.Rectangle selected = null;
            if (RectStack.SelectedIndex != -1)
            {
                selected = RectStack.SelectedItem as Core.Rectangle;
            }

            SelectedRectCanvas.Children.Clear();
            System.Windows.Shapes.Rectangle selectedRect = null;
            foreach (Core.Rectangle rect in _selectedRects)
            {
                // Draw a box
                var stackRect = new System.Windows.Shapes.Rectangle();

                if (selected == rect)
                {
                    stackRect.Stroke = new SolidColorBrush(Colors.Red);
                    stackRect.Fill = new SolidColorBrush(Colors.Salmon);
                    selectedRect = stackRect;
                }
                else
                {
                    stackRect.Stroke = new SolidColorBrush(Colors.Green);
                    stackRect.Fill = new SolidColorBrush(Colors.LightGreen);
                }

                stackRect.StrokeThickness = 0.1;
                stackRect.Opacity = 0.80;
                // Be less stupid about this...
                stackRect.Width = Math.Abs(rect.Width);
                stackRect.Height = Math.Abs(rect.Height);
                System.Windows.Controls.Canvas.SetLeft(stackRect, rect.Left);
                System.Windows.Controls.Canvas.SetTop(stackRect, _problemHeight - rect.Top);

                // No value/false OR it's not the selected one
                if ((!SelectedRectOnTopCheckbox.IsChecked.HasValue || !SelectedRectOnTopCheckbox.IsChecked.Value)
                    || selected != rect)
                {
                    SelectedRectCanvas.Children.Add(stackRect);
                }
            }

            // One was selected AND we're told to render it on top
            if (selectedRect != null && (SelectedRectOnTopCheckbox.IsChecked.HasValue && SelectedRectOnTopCheckbox.IsChecked.Value))
            {
                SelectedRectCanvas.Children.Add(selectedRect);
            }

            if (reloadList)
            {
                RectStack.ItemsSource = null;
            }
            RectStack.ItemsSource = _selectedRects;
        }

        private void Execute_SaveStack(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            long now = DateTime.UtcNow.Ticks;
            string filePath = Path.Combine(@"..\..\quicksaves", $"{_problemId}_{now}.json");
            LogVisualizerMessage($"Saving state to {filePath}");
            Quicksave.SaveRects(_problemId.ToString(), _selectedRects, filePath);
        }

        private void Execute_RestoreStack(object sender, ExecutedRoutedEventArgs e)
        {
            (string problemId, List<Core.Rectangle> rects, string fileName) = Quicksave.RestoreMostRecentStackFromDirectory(@"..\..\quicksaves", _problemId);

            if (problemId == null || rects?.Count == 0)
            {
                LogVisualizerMessage("Nothing to restore");
                return;
            }

            LogVisualizerMessage($"Restoring from {fileName}");

            _selectedRects = rects;
            DrawSelectedRects(true);
        }

        private void RectStack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DrawSelectedRects();

            OrderRectStackUp.IsEnabled = _selectedRects.Count > 1 && RectStack.SelectedIndex > 0;
            OrderRectStackDown.IsEnabled = _selectedRects.Count > 1 && RectStack.SelectedIndex < _selectedRects.Count - 1; // -1 because we can't move the last one down
        }

        private void RectStack_KeyUp(object sender, KeyEventArgs e)
        {
            if (RectStack.SelectedIndex == -1)
            {
                return;
            }

            int selectedIndex = RectStack.SelectedIndex;
            Core.Rectangle selected = RectStack.SelectedItem as Core.Rectangle;

            if (e.Key == Key.Delete)
            {
                _selectedRects.Remove(selected);

                DrawSelectedRects(true);

                return;
            }

            Core.Rectangle newRect;
            if (e.Key == Key.W)
            {
                // Top up 1 px
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    // Clamp top to top of canvas
                    newRect = new Core.Rectangle(selected.BottomLeft, new Core.Point(selected.Right, Math.Min(_problemHeight, selected.Top + 1)));
                }
                // Bottom up 1 px
                else if (Keyboard.IsKeyDown(Key.RightShift))
                {
                    // Clamp bottom to disallow lines
                    newRect = new Core.Rectangle(new Core.Point(selected.Left, Math.Min(selected.Bottom + 1, selected.Top - 1)), selected.TopRight);
                }
                else
                {
                    // Nudge but shrink if it hits the top and don't allow lines
                    Core.Point newTopRight = new Core.Point(selected.Right, Math.Min(_problemHeight, selected.Top + 1));
                    newRect = new Core.Rectangle(new Core.Point(selected.Left, Math.Min(selected.Bottom + 1, newTopRight.Y - 1)), newTopRight);
                }

                SwapSelectedRect(newRect, selectedIndex);
                return;
            }

            if (e.Key == Key.S)
            {
                // Top down 1 px
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    // Clamp top to disallow lines
                    newRect = new Core.Rectangle(selected.BottomLeft, new Core.Point(selected.Right, Math.Max(selected.Bottom + 1, selected.Top - 1)));
                }
                // Bottom down 1 px
                else if (Keyboard.IsKeyDown(Key.RightShift))
                {
                    // Clamp bottom to bottom of canvas
                    newRect = new Core.Rectangle(new Core.Point(selected.Left, Math.Max(0, selected.Bottom - 1)), selected.TopRight);
                }
                else
                {
                    // Nudge but shrink if it hits the bottom and don't allow lines
                    Core.Point newBottomLeft = new Core.Point(selected.Left, Math.Max(0, selected.Bottom - 1));
                    newRect = new Core.Rectangle(newBottomLeft, new Core.Point(selected.Right, Math.Max(newBottomLeft.Y + 1, selected.Top - 1)));
                }

                SwapSelectedRect(newRect, selectedIndex);
                return;
            }

            if (e.Key == Key.A)
            {
                // Left edge left 1 px
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    // Clamp left to left side of canvas
                    newRect = new Core.Rectangle(new Core.Point(Math.Max(0, selected.Left - 1), selected.Bottom), selected.TopRight);
                }
                // Right edge left 1 px
                else if (Keyboard.IsKeyDown(Key.RightShift))
                {
                    // Clamp to left to prevent lines
                    newRect = new Core.Rectangle(selected.BottomLeft, new Core.Point(Math.Max(selected.Left + 1, selected.Right - 1), selected.Top));
                }
                else
                {
                    // Nudge but shrink if it hits the left and don't allow lines
                    Core.Point newBottomLeft = new Core.Point(Math.Max(0, selected.Left - 1), selected.Bottom);
                    newRect = new Core.Rectangle(newBottomLeft, new Core.Point(Math.Max(newBottomLeft.X + 1, selected.Right - 1), selected.Top));
                }

                SwapSelectedRect(newRect, selectedIndex);
                return;
            }

            if (e.Key == Key.D)
            {
                // Left edge right 1 px
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    // Clamp to right to prevent lines
                    newRect = new Core.Rectangle(new Core.Point(Math.Min(selected.Left + 1, selected.Right - 1), selected.Bottom), selected.TopRight);
                }
                // Right edge right 1 px
                else if (Keyboard.IsKeyDown(Key.RightShift))
                {
                    // Clamp to right size of canvas
                    newRect = new Core.Rectangle(selected.BottomLeft, new Core.Point(Math.Min(selected.Right + 1, _problemWidth), selected.Top));
                }
                else
                {
                    // Nudge but shrink if it hits the right and don't allow lines
                    Core.Point newTopRight = new Core.Point(Math.Min(selected.Right + 1, _problemWidth), selected.Top);
                    newRect = new Core.Rectangle(new Core.Point(Math.Min(selected.Left + 1, newTopRight.X - 1), selected.Bottom), newTopRight);
                }

                SwapSelectedRect(newRect, selectedIndex);
                return;
            }
        }

        private void SwapSelectedRect(Core.Rectangle newRect, int selectedIndex)
        {
            _selectedRects.RemoveAt(selectedIndex);
            _selectedRects.Insert(selectedIndex, newRect);
            RectStack.ItemsSource = null;
            RectStack.ItemsSource = _selectedRects;
            RectStack.SelectedIndex = selectedIndex;
        }

        private void OrderRectStackUp_OnClick(object sender, RoutedEventArgs e)
        {
            // Swap current with current - 1
            int currentIndex = RectStack.SelectedIndex;
            Core.Rectangle prev = _selectedRects[currentIndex - 1];
            _selectedRects.RemoveAt(currentIndex - 1);
            _selectedRects.Insert(currentIndex, prev);
            RectStack.ItemsSource = null;
            RectStack.ItemsSource = _selectedRects;
            RectStack.SelectedIndex = currentIndex - 1; // Automatically redraws
        }

        private void OrderRectStackDown_OnClick(object sender, RoutedEventArgs e)
        {
            // Swap current with current + 1
            int currentIndex = RectStack.SelectedIndex;
            Core.Rectangle next = _selectedRects[currentIndex + 1];
            _selectedRects.RemoveAt(currentIndex + 1);
            _selectedRects.Insert(currentIndex, next);
            RectStack.ItemsSource = null;
            RectStack.ItemsSource = _selectedRects;
            RectStack.SelectedIndex = currentIndex + 1; // Automatically redraws
        }

        private void SelectedRectOnTopCheckbox_Toggled(object sender, RoutedEventArgs e)
        {
            DrawSelectedRects();
        }

        private void ManualMove_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ManualDrawCanvas.Children.Clear();
        }
    }
}
