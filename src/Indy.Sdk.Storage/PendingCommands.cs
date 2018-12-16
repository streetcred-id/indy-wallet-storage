﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Streetcred.Indy.Sdk
{
    /// <summary>
    /// Holder for pending commands.
    /// </summary>
    internal static class PendingCommands
    {
        /// <summary>
        /// The next command handle to use.
        /// </summary>
        private static int _nextCommandHandle = 0;

        /// <summary>
        /// Gets the next command handle.
        /// </summary>
        /// <returns>The next command handle.</returns>
        public static int GetNextCommandHandle()
        {
            return Interlocked.Increment(ref _nextCommandHandle);
        }

        /// <summary>
        /// Gets the map of command handles and their task completion sources.
        /// </summary>
        private static readonly IDictionary<int, object> TaskCompletionSources = new ConcurrentDictionary<int, object>();

        /// <summary>
        /// Adds a new TaskCompletionSource to track.
        /// </summary>
        /// <typeparam name="T">The type of the TaskCompletionSource result.</typeparam>
        /// <param name="taskCompletionSource">The TaskCompletionSource to track.</param>
        /// <returns>The command handle to use for tracking the task completion source.</returns>
        public static int Add<T>(TaskCompletionSource<T> taskCompletionSource)
        {
            Debug.Assert(taskCompletionSource != null, "A task completion source is required.");

            var commandHandle = GetNextCommandHandle();
            TaskCompletionSources.Add(commandHandle, taskCompletionSource);
            return commandHandle;
        }

        /// <summary>
        /// Gets and removes a TaskCompletionResult from tracking.
        /// </summary>
        /// <typeparam name="T">The type of the TaskCompletionResult that was tracked.</typeparam>
        /// <param name="commandHandle">The command handle used for tracking the TaskCompletionResult.</param>
        /// <returns>The TaskCompletionResult associated with the command handle.</returns>
        public static TaskCompletionSource<T> Remove<T>(int commandHandle)
        {
            Debug.Assert(TaskCompletionSources.ContainsKey(commandHandle),
                $"No task completion source is currently registered for the command with the handle '{commandHandle}'.");

            var taskCompletionSource = TaskCompletionSources[commandHandle];
            TaskCompletionSources.Remove(commandHandle);
            var result = taskCompletionSource as TaskCompletionSource<T>;

            Debug.Assert(result != null,
                $"No  task completion source of the specified type is registered for the command with the handle '{commandHandle}'.");

            return result;
        }
    }
}