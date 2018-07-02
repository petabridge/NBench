// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace NBench
{
    /// <summary>
    /// Exposed to the end-user by <see cref="BenchmarkContext"/> so they can add
    /// diagnostic messages to the output of a benchmark.
    /// </summary>
    public interface IBenchmarkTrace
    {
        /// <summary>
        /// Write a debug event to the NBench output
        /// </summary>
        /// <param name="message">The message we're going to write to output.</param>
        void Debug(string message);

        /// <summary>
        /// Write a info event to the NBench output
        /// </summary>
        /// <param name="message">The message we're going to write to output.</param>
        void Info(string message);

        /// <summary>
        /// Write a warning to the NBench output
        /// </summary>
        /// <param name="message">The message we're going to write to output.</param>
        void Warning(string message);

        /// <summary>
        /// Write an error to the NBench output
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> raised during the benchmark.</param>
        /// <param name="message">The message we're going to write to output.</param>
        void Error(Exception ex, string message);

        /// <summary>
        /// Write an error to the NBench output
        /// </summary>
        /// <param name="message">The message we're going to write to output.</param>
        void Error(string message);
    }
}

