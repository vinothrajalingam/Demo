using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace ARCO.EndPoint.eCard.ErrorHandling
{
    public class PassthroughHandler : IExceptionHandler
    {
		private readonly IExceptionHandler _innerHandler;
		private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(PassthroughHandler));

		public PassthroughHandler(IExceptionHandler innerHandler)
		{
			if (innerHandler == null)
				throw new ArgumentNullException(nameof(innerHandler));
			_innerHandler = innerHandler;
		}

		public IExceptionHandler InnerHandler
		{
			get { return _innerHandler; }
		}

		public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
		{
			Handle(context);

			return Task.FromResult<object>(null);
		}

		public void Handle(ExceptionHandlerContext context)
		{
			_log.Error("There was an unexpected error", context.Exception);
			context.Result = new InternalServerErrorResult(context.Request);
		}
	}
}
