using System;
using System.Threading;

namespace NBench.Tracing
{
    /// <summary>
    ///     An event instance produced by a <see cref="IBenchmarkTrace" />
    /// </summary>
    public abstract class TraceMessage
    {
        public const string TraceIndicator = "[NBench]";

        protected TraceMessage(string message, TraceLevel level)
        {
            Message = message;
            Level = level;
            Timestamp = DateTime.UtcNow;
            ThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public DateTime Timestamp { get; }

        public string Message { get; }

        public int ThreadId { get; }

        public TraceLevel Level { get; }

        public override string ToString()
        {
            return
                $"{TraceIndicator}[{Level.ToString().ToUpperInvariant()}][{Timestamp}][Thread {ThreadId.ToString().PadLeft(4, '0')}] {Message}";
        }
    }

    public class Error : TraceMessage
    {
        public Error(string message) : base(message, TraceLevel.Error)
        {
        }

        public Error(Exception cause, string message) : base(message, TraceLevel.Error)
        {
            Cause = cause;
        }

        public Exception Cause { get; }

        public override string ToString()
        {
            return base.ToString() + Environment.NewLine + $"Cause: {Cause?.ToString() ?? "Unknown"}";
        }
    }

    public class Warning : TraceMessage
    {
        public Warning(string message) : base(message, TraceLevel.Warning)
        {
        }
    }

    /// <summary>
    ///     <see cref="Info" /> events
    /// </summary>
    public class Info : TraceMessage
    {
        public Info(string message) : base(message, TraceLevel.Info)
        {
        }
    }

    /// <summary>
    ///     <see cref="Debug" /> events
    /// </summary>
    public class Debug : TraceMessage
    {
        public Debug(string message) : base(message, TraceLevel.Debug)
        {
        }
    }
}
