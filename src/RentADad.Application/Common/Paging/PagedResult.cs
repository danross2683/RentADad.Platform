using System.Collections.Generic;

namespace RentADad.Application.Common.Paging;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
