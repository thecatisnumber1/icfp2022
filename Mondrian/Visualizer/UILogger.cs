using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
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
        private Figure _nextFigure;
        private Task _renderPump;

        public bool Busy => Interlocked.Read(ref _rendering) != 0;

        public UILogger(MainWindow mainUI, CancellationToken cancellationToken)
        {
            _mainUi = mainUI;
            _cancellationToken = cancellationToken;
            _renderPump = Task.Run(RenderLoop);
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

                        Figure toRender = Interlocked.Exchange(ref _nextFigure, null);
                        if (toRender != null)
                        {
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

                        Interlocked.Exchange(ref _rendering, 0);
                    });
                }

                Task.Delay(20).Wait(); // Need something to stop us from constantly spamming the UI
            }
        }

        public void Show(Figure figure)
        {
            if (Interlocked.Exchange(ref _nextFigure, figure) != null)
            {
                Interlocked.Increment(ref _skippedFrames);
            }
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
