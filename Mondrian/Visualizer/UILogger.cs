using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;

namespace Visualizer
{
    internal class UILogger : LoggerBase
    {
        private readonly MainWindow _mainUi;
        private readonly CancellationToken _cancellationToken;

        // Threading stuff
        private int _skippedFrames = 0;
        private long _rendering = 0;
        private ConcurrentQueue<string> _messages = new ConcurrentQueue<string>();

        private string _statusString;

        // Rendering stuff
        private List<SimpleBlock> _nextImage;
        private int _nextScore;
        private int _nextTotalInstructionCost;

        private Task _renderPump;

        // The solver is blocked on the logger. UI must eventually set this to false to release it.
        internal bool SolverPaused;

        // User requested pause in the visualizer.
        internal bool PauseRequested;

        private object lockobj = new object();

        public UILogger(MainWindow mainUI, CancellationToken cancellationToken, List<Rectangle> selectedRects)
        {
            _mainUi = mainUI;
            _cancellationToken = cancellationToken;
            _renderPump = Task.Run(RenderLoop);
            UserSelectedRectangles = selectedRects ?? UserSelectedRectangles;
        }

        private void RenderLoop()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (Interlocked.Exchange(ref _rendering, 1) == 0)
                {
                    // We can do stuff
                    _mainUi.Dispatcher.BeginInvoke(() =>
                    {
                        bool addDebugMessages = true; // _mainUi.UIDebugSpewCheckbox.IsChecked.Value;
                        int skippedFrames = -1;

                        List<SimpleBlock> toRender;
                        int score;
                        int totalCost;
                        lock (lockobj)
                        {
                            toRender = Interlocked.Exchange(ref _nextImage, null);
                            score = _nextScore;
                            totalCost = _nextTotalInstructionCost;
                        }

                        if (toRender != null)
                        {
                            if (_mainUi.UseOldRenderer)
                            {
                                _mainUi.RenderImage(toRender, score, totalCost);
                            }
                            else
                            {
                                _mainUi.RenderImageFast(toRender, score, totalCost);
                            }
                            skippedFrames = Interlocked.Exchange(ref _skippedFrames, 0);
                        }

                        var messages = new List<string>();
                        while (_messages.TryDequeue(out string message))
                        {
                            messages.Add(message);
                        }

                        if (addDebugMessages && messages.Count > 1)
                        {
                            messages.Insert(0, $"[DBG] Batch of {messages.Count} messages.");
                        }

                        if (addDebugMessages && skippedFrames > 0)
                        {
                            messages.Add($"[DBG] Skipped rendering {skippedFrames} frames.");
                        }

                        if (messages.Any())
                        {
                            _mainUi.LogMessage(string.Join(Environment.NewLine, messages));
                        }

                        if (_statusString != null)
                        {
                            _mainUi.CustomStatusText.Text = _statusString;
                            _statusString = null; // Null out so we don't constantly update.
                        }

                        Interlocked.Exchange(ref _rendering, 0);
                    });
                }

                Task.Delay(20).Wait(); // Need something to stop us from constantly spamming the UI
            }
        }

        public override void Break(bool immediate)
        {
            if (PauseRequested || immediate)
            {
                SolverPaused = true;
                _mainUi.Dispatcher.BeginInvoke(() =>
                {
                    _mainUi.Break();
                });


                while (SolverPaused && !_cancellationToken.IsCancellationRequested)
                {
                    Task.Delay(20).Wait(); // Visualizer will eventually release.
                }
            }
        }

        public override void Render(Picasso image)
        {
            List<SimpleBlock> blocks;
            // Atomically update all data that's going to be rendered.
            lock (lockobj)
            {
                _nextScore = _mainUi.HideScore ? 1 : image.Score;
                _nextTotalInstructionCost = _mainUi.HideScore? 1 : image.TotalInstructionCost;
                blocks = new List<SimpleBlock>(image.AllSimpleBlocks);
            }

            if (Interlocked.Exchange(ref _nextImage, blocks) != null)
            {
                Interlocked.Increment(ref _skippedFrames);
            }
        }

        public override void LogStatusMessage(string logString)
        {
            // You only get one.
            _statusString = logString;
        }

        public override void LogMessage(string logString)
        {
            _messages.Enqueue($"[OUT] {logString}");
        }

        public override void LogError(string logString)
        {
            _messages.Enqueue($"[ERR] {logString}");
        }
    }
}
