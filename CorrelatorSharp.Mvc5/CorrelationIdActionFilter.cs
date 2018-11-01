using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CorrelatorSharp.Mvc5
{
    public class CorrelationIdActionFilter : IActionFilter, IResultFilter
    {
        private static readonly string CorrelationIdHttpHeader = Headers.CorrelationId;
        private static readonly string CorrelationParentIdHttpHeader = Headers.CorrelationParentId;
        private static readonly string CorrelationNameHttpHeader = Headers.CorrelationName;

        private const string ITEM_KEY = "__Correlator";

        /// <summary>
        /// Looks for "X-Correlation-" type http headers and either uses them or generates new ones, where applicable.
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.IsChildAction)
            {
                return;
            }

            OpenScope(filterContext.RequestContext?.HttpContext);
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.IsChildAction)
            {
                return;
            }

            if (filterContext.Canceled || (filterContext.Exception != null && !filterContext.ExceptionHandled))
            {
                CloseScope(filterContext.HttpContext);
            }
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {

        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (filterContext.IsChildAction)
            {
                return;
            }

            CloseScope(filterContext.HttpContext);
        }

        private static void OpenScope(HttpContextBase context)
        {
            if (context == null)
            {
                return;
            }

            string correlationScopeName = null;
            string correlationId = null;
            string parentCorrelationId = null;

            var headers = context.Request?.Headers;
            if (headers != null)
            {
                var correlationIds = headers.GetValues(CorrelationIdHttpHeader);
                if (correlationIds?.Any() ?? false)
                {
                    correlationId = correlationIds.First();
                }

                var correlationIdParents = headers.GetValues(CorrelationParentIdHttpHeader);
                if (correlationIdParents?.Any() ?? false)
                {
                    parentCorrelationId = correlationIdParents.First();
                }

                var correlationScopeNames = headers.GetValues(CorrelationNameHttpHeader);
                if (correlationScopeNames?.Any() ?? false)
                {
                    correlationScopeName = correlationScopeNames.First();
                }
            }

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            if (string.IsNullOrWhiteSpace(parentCorrelationId) == false)
            {
                ActivityScope.Create(null, parentCorrelationId);
                context.Items[ITEM_KEY] = ActivityScope.Child(correlationScopeName, correlationId);
                return;
            }

            context.Items[ITEM_KEY] = ActivityScope.Create(null, correlationId);
        }

        private static void CloseScope(HttpContextBase context)
        {
            var scope = context?.Items[ITEM_KEY] as ActivityScope;
            if (scope == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(scope.ParentId))
            {
                context.Response.AddHeader(CorrelationIdHttpHeader, scope.Id);
                scope.Dispose();
            }

            var parentId = scope.ParentId;

            context.Response.AddHeader(CorrelationIdHttpHeader, scope.Id);
            scope.Dispose();

            var parent = ActivityTracker.Find(parentId);
            if (parent == null)
            {
                return;
            }

            context.Response.AddHeader(CorrelationParentIdHttpHeader, scope.ParentId);
            parent?.Dispose();
        }
    }
}
