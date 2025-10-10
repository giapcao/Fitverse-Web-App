using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.KycRecords.Query;

public sealed record ListAllKycRecordsQuery(int PageNumber = 1, int PageSize = 10)
    : PagedQuery<KycRecordDto>(PageNumber, PageSize);
