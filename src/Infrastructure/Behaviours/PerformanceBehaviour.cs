﻿using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Behaviours
{
    public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly Stopwatch _timer;
        private readonly ILogger<TRequest> _logger;
        private readonly ICurrentUser _currentUser;

        public PerformanceBehaviour(
            ILogger<TRequest> logger,
            ICurrentUser currentUser)
        {
            _timer = new Stopwatch();

            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            _timer.Start();

            var response = await next();

            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var requestName = typeof(TRequest).Name;

                _logger.LogWarning(
                    "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}",
                    requestName,
                    elapsedMilliseconds,
                    _currentUser.Id,
                    _currentUser.Name,
                    request);
            }

            return response;
        }
    }
}