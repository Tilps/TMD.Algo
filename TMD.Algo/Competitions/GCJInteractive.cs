#region License

/*
Copyright (c) 2026, the TMD.Algo authors.
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of TMD.Algo nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#endregion

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace TMD.Algo.Competitions
{
    /// <summary>
    /// Bidirectional line-oriented communication channel for interactive Code Jam style problems.
    /// Supports both standard console I/O and in-process paired test execution.
    /// </summary>
    public sealed class GCJInteractiveChannel : IDisposable
    {
        private readonly BlockingCollection<string>? incoming;
        private readonly BlockingCollection<string>? outgoing;
        private readonly CancellationToken cancellationToken;
        private readonly TextReader? reader;
        private readonly TextWriter? writer;

        private GCJInteractiveChannel(
            BlockingCollection<string> incoming,
            BlockingCollection<string> outgoing,
            CancellationToken cancellationToken)
        {
            this.incoming = incoming;
            this.outgoing = outgoing;
            this.cancellationToken = cancellationToken;
        }

        private GCJInteractiveChannel(TextReader reader, TextWriter writer)
        {
            this.reader = reader;
            this.writer = writer;
        }

        /// <summary>
        /// Creates a channel bound to standard input and standard output, flushing after every written line.
        /// </summary>
        /// <returns>A new <see cref="GCJInteractiveChannel"/> backed by <see cref="Console.In"/> and <see cref="Console.Out"/>.</returns>
        public static GCJInteractiveChannel FromConsole() => new(Console.In, Console.Out);

        internal static (GCJInteractiveChannel SolverChannel, GCJInteractiveChannel JudgeChannel) CreatePair(
            BlockingCollection<string> judgeToSolver,
            BlockingCollection<string> solverToJudge,
            CancellationToken cancellationToken) =>
            (new GCJInteractiveChannel(judgeToSolver, solverToJudge, cancellationToken),
                new GCJInteractiveChannel(solverToJudge, judgeToSolver, cancellationToken));

        /// <summary>
        /// Reads the next line from the peer, or returns <see langword="null"/> on end-of-stream.
        /// </summary>
        /// <returns>The next line of text, or <see langword="null"/> if the peer finished writing.</returns>
        public string? ReadLine()
        {
            if (incoming is not null)
            {
                if (incoming.TryTake(out string? line, Timeout.Infinite, cancellationToken))
                    return line;
                cancellationToken.ThrowIfCancellationRequested();
                return null;
            }

            return reader!.ReadLine();
        }

        /// <summary>
        /// Reads the next required line from the peer, throwing <see cref="EndOfStreamException"/> if the stream has ended.
        /// </summary>
        /// <returns>The next line of text.</returns>
        public string ReadRequiredLine()
        {
            string? line = ReadLine();
            return line ?? throw new EndOfStreamException("Unexpected end of interactive input.");
        }

        /// <summary>
        /// Writes a line to the peer and flushes immediately.
        /// </summary>
        /// <param name="line">Line content (without trailing newline).</param>
        public void WriteLine(string line)
        {
            ArgumentNullException.ThrowIfNull(line);
            if (outgoing is not null)
            {
                if (outgoing.IsAddingCompleted)
                    throw new InvalidOperationException("Cannot write after writing has been completed.");
                outgoing.Add(line, cancellationToken);
                return;
            }

            writer!.WriteLine(line);
            writer.Flush();
        }

        /// <summary>
        /// Signals to the peer that no further lines will be written.
        /// Safe to call multiple times.
        /// </summary>
        public void CompleteWriting()
        {
            if (outgoing is not null)
                outgoing.CompleteAdding();
            else
                writer!.Flush();
        }

        /// <summary>
        /// Completes the outgoing side of the channel.
        /// </summary>
        public void Dispose() => CompleteWriting();
    }

    /// <summary>
    /// Runs an interactive solver and judge concurrently over paired <see cref="GCJInteractiveChannel"/> instances
    /// with deadlock-safe cancellation and timeout handling.
    /// </summary>
    public static class GCJInteractive
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Executes <paramref name="solver"/> and <paramref name="judge"/> concurrently until both finish or an error/timeout occurs.
        /// </summary>
        /// <param name="solver">Solver delegate receiving its interactive channel.</param>
        /// <param name="judge">Judge delegate receiving its interactive channel.</param>
        /// <param name="timeout">Maximum duration before cancelling both sides. Defaults to 30 seconds.</param>
        public static void Run(
            Action<GCJInteractiveChannel> solver,
            Action<GCJInteractiveChannel> judge,
            TimeSpan? timeout = null)
        {
            ArgumentNullException.ThrowIfNull(solver);
            ArgumentNullException.ThrowIfNull(judge);

            TimeSpan effectiveTimeout = timeout ?? DefaultTimeout;
            if (effectiveTimeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive.");

            using CancellationTokenSource cts = new(effectiveTimeout);
            using BlockingCollection<string> judgeToSolver = new();
            using BlockingCollection<string> solverToJudge = new();

            var (solverChannel, judgeChannel) = GCJInteractiveChannel.CreatePair(judgeToSolver, solverToJudge, cts.Token);

            Exception? solverError = null;
            Exception? judgeError = null;

            Task solverTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    solver(solverChannel);
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested)
                {
                }
                catch (Exception ex)
                {
                    Volatile.Write(ref solverError, ex);
                    cts.Cancel();
                }
                finally
                {
                    solverChannel.CompleteWriting();
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            Task judgeTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    judge(judgeChannel);
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested)
                {
                }
                catch (Exception ex)
                {
                    Volatile.Write(ref judgeError, ex);
                    cts.Cancel();
                }
                finally
                {
                    judgeChannel.CompleteWriting();
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            Task.WaitAll(solverTask, judgeTask);

            if (judgeError is not null)
                ExceptionDispatchInfo.Capture(judgeError).Throw();
            if (solverError is not null)
                ExceptionDispatchInfo.Capture(solverError).Throw();
            if (cts.IsCancellationRequested)
                throw new TimeoutException($"Interactive session exceeded timeout of {effectiveTimeout.TotalSeconds:0.###} seconds.");
        }
    }
}
