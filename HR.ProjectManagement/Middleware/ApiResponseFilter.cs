using HR.ProjectManagement.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HR.ProjectManagement.Middleware;

public class ApiResponseFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult)
        {
            var value = objectResult.Value;

            // Check if it's already an ApiResponse or ApiErrorResponse
            if (value is ApiResponse || value is ApiErrorResponse)
            {
                await next();
                return;
            }

            // Check if it's a PagedResponse and wrap it
            if (value != null && value.GetType().IsGenericType &&
                value.GetType().GetGenericTypeDefinition() == typeof(PagedResponse<>))
            {
                // Use reflection to safely access PagedResponse properties without type casting issues
                var pageNumberProp = value.GetType().GetProperty("PageNumber");
                var pageSizeProp = value.GetType().GetProperty("PageSize");
                var totalCountProp = value.GetType().GetProperty("TotalCount");
                var dataProp = value.GetType().GetProperty("Data");

                if (pageNumberProp != null && pageSizeProp != null && totalCountProp != null && dataProp != null)
                {
                    var pageNumber = (int)pageNumberProp.GetValue(value)!;
                    var pageSize = (int)pageSizeProp.GetValue(value)!;
                    var totalCount = (int)totalCountProp.GetValue(value)!;
                    var data = dataProp.GetValue(value);

                    var pagination = PaginationMetadata.FromPagedResponse(pageNumber, pageSize, totalCount);
                    var response = ApiResponse<object>.OkResponse(data ?? new object(), "Success", pagination);
                    context.Result = new ObjectResult(response)
                    {
                        StatusCode = objectResult.StatusCode
                    };
                }
            }
            // Handle simple responses (GET by ID, DELETE, etc.)
            else if (value != null)
            {
                var response = ApiResponse<object>.OkResponse(value, "Success");
                context.Result = new ObjectResult(response)
                {
                    StatusCode = objectResult.StatusCode
                };
            }
            else
            {
                // Handle empty responses (like DELETE)
                context.Result = new ObjectResult(ApiResponse.OkResponse("Success"))
                {
                    StatusCode = objectResult.StatusCode
                };
            }
        }

        await next();
    }
}