﻿using System;
using System.IO;
using System.Net;
using JetBrains.Annotations;
using PatchKit.Logging;
using PatchKit.Network;
using PatchKit.Unity.Patcher.Cancellation;
using PatchKit.Unity.Patcher.Debug;

namespace PatchKit.Unity.Patcher.AppData.Remote.Downloaders
{
    public sealed class BaseHttpDownloader : IBaseHttpDownloader
    {
        private readonly ILogger _logger;

        private const int BufferSize = 1024;
        
        private readonly string _url;
        private readonly int _timeout;
        private readonly IHttpClient _httpClient;

        private readonly byte[] _buffer;

        private bool _downloadHasBeenCalled;
        private BytesRange? _bytesRange;

        public event DataAvailableHandler DataAvailable;

        public BaseHttpDownloader(string url, int timeout) :
            this(url, timeout, new DefaultHttpClient(), PatcherLogManager.DefaultLogger)
        {
        }

        public BaseHttpDownloader([NotNull] string url, int timeout, [NotNull] IHttpClient httpClient, [NotNull] ILogger logger)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentException("Value cannot be null or empty.", "url");
            if (timeout <= 0) throw new ArgumentOutOfRangeException("timeout");
            if (httpClient == null) throw new ArgumentNullException("httpClient");
            if (logger == null) throw new ArgumentNullException("logger");

            _url = url;
            _timeout = timeout;
            _httpClient = httpClient;
            _logger = logger;

            _buffer = new byte[BufferSize];

            ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, errors) => true;
            ServicePointManager.DefaultConnectionLimit = 65535;
        }

        public void SetBytesRange(BytesRange? range)
        {
            _bytesRange = range;
        }

        public void Download(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Downloading...");
                _logger.LogTrace("url = " + _url);
                _logger.LogTrace("bufferSize = " + BufferSize);
                _logger.LogTrace("bytesRange = " + (_bytesRange.HasValue
                                     ? _bytesRange.Value.Start + "-" + _bytesRange.Value.End
                                     : "(none)"));
                _logger.LogTrace("timeout = " + _timeout);

                Assert.MethodCalledOnlyOnce(ref _downloadHasBeenCalled, "Download");

                var request = new HttpGetRequest
                {
                    Address = new Uri(_url),
                    Range = _bytesRange,
                    Timeout = _timeout
                };

                using (var response = _httpClient.Get(request))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    _logger.LogDebug("Received response from server.");
                    _logger.LogTrace("statusCode = " + response.StatusCode);

                    if (IsStatusSuccess(response.StatusCode))
                    {
                        _logger.LogDebug("Successful response. Reading response stream...");

                        ReadResponseStream(response.ContentStream, cancellationToken);

                        _logger.LogDebug("Stream has been read.");
                    }
                    else if (IsStatusClientError(response.StatusCode))
                    {
                        _logger.LogError("Download data is not available.");
                        throw new DownloadDataNotAvailableException(_url);
                    }
                    else
                    {
                        _logger.LogError("Invalid server response.");
                        throw new DownloadServerErrorException(_url, response.StatusCode);
                    }
                }

                _logger.LogDebug("Downloading finished.");
            }
            catch (WebException webException)
            {
                _logger.LogError("Downloading has failed.", webException);
                throw new DownloadConnectionFailureException(_url);
            }
            catch (Exception e)
            {
                _logger.LogError("Downloading has failed.", e);
                throw;
            }
        }

        private void ReadResponseStream(Stream responseStream, CancellationToken cancellationToken)
        {
            int bufferRead;
            while ((bufferRead = responseStream.Read(_buffer, 0, BufferSize)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                OnDataAvailable(_buffer, bufferRead);
            }
        }

        private bool IsStatusSuccess(HttpStatusCode statusCode)
        {
            return (int) statusCode >= 200 && (int) statusCode <= 299;
        }

        private bool IsStatusClientError(HttpStatusCode statusCode)
        {
            return (int) statusCode >= 400 && (int) statusCode <= 499;
        }

        private void OnDataAvailable(byte[] data, int length)
        {
            var handler = DataAvailable;
            if (handler != null) handler(data, length);
        }
    }
}