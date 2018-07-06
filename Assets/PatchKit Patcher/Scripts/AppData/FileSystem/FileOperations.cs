﻿using System;
using System.IO;
using PatchKit.Unity.Patcher.Cancellation;
using PatchKit.Unity.Patcher.Debug;

namespace PatchKit.Unity.Patcher.AppData.FileSystem
{
    // ReSharper disable once InconsistentNaming
    public static class FileOperations
    {
        private static readonly DebugLogger DebugLogger = new DebugLogger(typeof(FileOperations));

        /// <summary>
        /// Copies file from <paramref name="sourceFilePath" /> to <paramref name="destinationFilePath" />.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="destinationFilePath">The destination file path.</param>
        /// <param name="overwrite">if set to <c>true</c> and destination file exists then it is overwritten.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentException"><paramref name="sourceFilePath"/> is null or empty.</exception>
        /// <exception cref="ArgumentException"><paramref name="destinationFilePath"/> is null or empty.</exception>
        /// <exception cref="FileNotFoundException"><paramref name="sourceFilePath"/> doesn't exist.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="destinationFilePath"/> parent directory doesn't exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Unauthorized access.</exception>
        public static void Copy(string sourceFilePath, string destinationFilePath, bool overwrite, CancellationToken cancellationToken)
        {
            Assert.IsFalse(string.IsNullOrEmpty(sourceFilePath), "sourceFilePath");
            Assert.IsFalse(string.IsNullOrEmpty(destinationFilePath), "destinationFilePath");

            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsTrue(Directory.Exists(Path.GetDirectoryName(destinationFilePath)));

            RetryStrategy.TryExecute(() => CopyInternal(sourceFilePath, destinationFilePath, overwrite), cancellationToken);
        }

        private static void CopyInternal(string sourceFilePath, string destinationFilePath, bool overwrite)
        {
            try
            {
                DebugLogger.Log(string.Format("Copying file from <{0}> to <{1}> {2}...",
                    sourceFilePath,
                    destinationFilePath,
                    overwrite ? "(overwriting)" : string.Empty));

                File.Copy(sourceFilePath, destinationFilePath, overwrite);

                DebugLogger.Log("File copied.");
            }
            catch (Exception)
            {
                DebugLogger.LogError("Error while copying file: an exception occured. Rethrowing exception.");
                throw;
            }
        }

        /// <summary>
        /// Deletes file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="FileNotFoundException"><paramref name="filePath"/> doesn't exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Unauthorized access.</exception>
        public static void Delete(string filePath, CancellationToken cancellationToken)
        {
            Assert.IsFalse(string.IsNullOrEmpty(filePath), "filePath");
            Assert.IsTrue(File.Exists(filePath));

            RetryStrategy.TryExecute(() => DeleteInternal(filePath), cancellationToken);
        }

        private static void DeleteInternal(string filePath)
        {
            try
            {
                DebugLogger.Log(string.Format("Deleting file <{0}>.", filePath));

                File.Delete(filePath);

                DebugLogger.Log("File deleted.");
            }
            catch (Exception)
            {
                DebugLogger.LogError("Error while deleting file: an exception occured. Rethrowing exception.");
                throw;
            }
        }

        /// <summary>
        /// Moves file from <paramref name="sourceFilePath" /> to <paramref name="destinationFilePath" />.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="destinationFilePath">The destination file path.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentException"><paramref name="sourceFilePath"/> is null or empty.</exception>
        /// <exception cref="ArgumentException"><paramref name="destinationFilePath"/> is null or empty.</exception>
        /// <exception cref="FileNotFoundException"><paramref name="sourceFilePath"/> doesn't exist.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="destinationFilePath"/> parent directory doesn't exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Unauthorized access.</exception>
        public static void Move(string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken)
        {
            Assert.IsFalse(string.IsNullOrEmpty(sourceFilePath), "sourceFilePath");
            Assert.IsFalse(string.IsNullOrEmpty(destinationFilePath), "destinationFilePath");

            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsTrue(Directory.Exists(Path.GetDirectoryName(destinationFilePath)));

            RetryStrategy.TryExecute(() => MoveInternal(sourceFilePath, destinationFilePath), cancellationToken);
        }

        private static void MoveInternal(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                DebugLogger.Log(string.Format("Moving file from <{0}> to <{1}>.", sourceFilePath, destinationFilePath));

                File.Move(sourceFilePath, destinationFilePath);

                DebugLogger.Log("File moved.");
            }
            catch (Exception)
            {
                DebugLogger.LogError("Error while moving file: an exception occured. Rethrowing exception.");
                throw;
            }
        }
    }
}