﻿using System;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pipopolam.Net
{
    public class Request
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Task task;

        protected virtual Task Task => task;

        public HttpResponseHeaders Headers { get; protected set; }

        protected Request(CancellationTokenSource cancellationTokenSource)
        {
            this.cancellationTokenSource = cancellationTokenSource;
        }

        public Request(Task<ServiceResponse> task, CancellationTokenSource cancellationTokenSource):
            this(cancellationTokenSource)
        {
            this.task = RequestWrapper(task);
        }

        public TaskAwaiter GetAwaiter() => Task.GetAwaiter();

        private async Task RequestWrapper(Task<ServiceResponse> task)
        {
            ServiceResponse response = await task;

            Headers = response.Headers;
        }

        public void Cancel() => cancellationTokenSource.Cancel();

        public static implicit operator Task(Request request)
        {
            return request.Task;
        }
    }

    public class Request<T> : Request where T: class
    {
        private readonly Task<T> task;

        protected override Task Task => task;

        public T Result => task.Result;

        public Request(Task<ServiceResponse<T>> task, CancellationTokenSource cancellationTokenSource) :
            base(cancellationTokenSource)
        {
            this.task = RequestWrapper(task);
        }

        public new TaskAwaiter<T> GetAwaiter() => task.GetAwaiter();

        private async Task<T> RequestWrapper(Task<ServiceResponse<T>> task)
        {
            ServiceResponse<T> response = await task;

            Headers = response.Headers;

            return response?.Data;
        }

        public static implicit operator Task<T>(Request<T> request)
        {
            return request.task;
        }
    }
}
