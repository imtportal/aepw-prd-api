using AEPW.Application.DTOs.Evidence;
using AEPW.Application.Exceptions;
using AEPW.Application.Interfaces.Repositories.Evidence;
using AEPW.Application.Services.Evidence;
using AEPW.Domain.Models.Evidence;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AEPW.Application.Tests.Services.Evidence;

public sealed class EvidenceReviewServiceTests
{
    [Fact]
    public async Task GetReviewDetailsAsync_ForwardsEvidenceAndUserIds()
    {
        var repository = new FakeEvidenceReviewRepository();
        var service = CreateService(repository);

        await service.GetReviewDetailsAsync(123, 42);

        Assert.Equal(123, repository.RequestedEvidenceId);
        Assert.Equal(42, repository.RequestedUserId);
    }

    [Fact]
    public async Task GetReviewDetailsAsync_RejectsInvalidEvidenceAndUserIds()
    {
        var repository = new FakeEvidenceReviewRepository();
        var service = CreateService(repository);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.GetReviewDetailsAsync(0, 42));
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.GetReviewDetailsAsync(123, 0));

        Assert.Null(repository.RequestedUserId);
    }

    [Fact]
    public async Task SaveExtractedFieldsAsync_ForwardsEveryFieldAndUpdatedBy()
    {
        var repository = new FakeEvidenceReviewRepository();
        var service = CreateService(repository);
        var request = new SaveEvidenceReviewExtractedFieldsRequestDto
        {
            EvidenceId = 123,
            UpdatedBy = 42,
            Fields =
            [
                new EvidenceReviewExtractedFieldRequestDto
                {
                    EvidenceDetailId = 10,
                    Value = "edited"
                },
                new EvidenceReviewExtractedFieldRequestDto
                {
                    EvidenceDetailId = 20,
                    Value = null
                }
            ]
        };

        var result = await service.SaveExtractedFieldsAsync(123, request);

        Assert.True(result);
        Assert.Equal(123, repository.SavedEvidenceId);
        Assert.Equal(42, repository.SavedBy);
        Assert.Equal(
            [
                new EvidenceReviewExtractedField(10, 100, "edited", null, false, 2, 0.9),
                new EvidenceReviewExtractedField(20, 200, null, null, false, 8, 0.8)
            ],
            repository.SavedFields);
    }

    [Fact]
    public async Task SaveExtractedFieldsAsync_RejectsRouteAndBodyEvidenceIdMismatch()
    {
        var service = CreateService(new FakeEvidenceReviewRepository());

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveExtractedFieldsAsync(123, CreateRequest(evidenceId: 124)));
    }

    [Fact]
    public async Task SaveExtractedFieldsAsync_RejectsInvalidEvidenceAndUpdatedByIds()
    {
        var service = CreateService(new FakeEvidenceReviewRepository());

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveExtractedFieldsAsync(0, CreateRequest()));
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveExtractedFieldsAsync(123, CreateRequest(evidenceId: 0)));
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveExtractedFieldsAsync(123, CreateRequest(updatedBy: 0)));
    }

    [Fact]
    public async Task SaveExtractedFieldsAsync_RejectsMissingOrEmptyFields()
    {
        var service = CreateService(new FakeEvidenceReviewRepository());

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveExtractedFieldsAsync(123, new SaveEvidenceReviewExtractedFieldsRequestDto
            {
                EvidenceId = 123,
                UpdatedBy = 42,
                Fields = null
            }));
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveExtractedFieldsAsync(123, CreateRequest(fields: [])));
    }

    [Fact]
    public async Task SaveExtractedFieldsAsync_RejectsInvalidDuplicateAndUnknownDetailId()
    {
        var service = CreateService(new FakeEvidenceReviewRepository());

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveExtractedFieldsAsync(123, CreateRequest(
                field: new EvidenceReviewExtractedFieldRequestDto { EvidenceDetailId = 0, Value = "x" })));
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveExtractedFieldsAsync(123, CreateRequest(fields:
            [
                new EvidenceReviewExtractedFieldRequestDto { EvidenceDetailId = 10, Value = "a" },
                new EvidenceReviewExtractedFieldRequestDto { EvidenceDetailId = 10, Value = "b" }
            ])));
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveExtractedFieldsAsync(123, CreateRequest(
                field: new EvidenceReviewExtractedFieldRequestDto { EvidenceDetailId = 999, Value = "x" })));
    }

    [Fact]
    public async Task SaveExtractedFieldsAsync_PropagatesPersistenceFailure()
    {
        var expectedException = new InvalidOperationException("Persistence failed.");
        var repository = new FakeEvidenceReviewRepository { SaveException = expectedException };
        var service = CreateService(repository);

        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveExtractedFieldsAsync(123, CreateRequest()));

        Assert.Same(expectedException, actualException);
    }

    private static EvidenceReviewService CreateService(IEvidenceReviewRepository repository) =>
        new(repository, NullLogger<EvidenceReviewService>.Instance);

    private static SaveEvidenceReviewExtractedFieldsRequestDto CreateRequest(
        long evidenceId = 123,
        long updatedBy = 42,
        IReadOnlyList<EvidenceReviewExtractedFieldRequestDto?>? fields = null,
        EvidenceReviewExtractedFieldRequestDto? field = null) =>
        new()
        {
            EvidenceId = evidenceId,
            UpdatedBy = updatedBy,
            Fields = fields ?? [field ?? new EvidenceReviewExtractedFieldRequestDto
            {
                EvidenceDetailId = 10,
                Value = "edited"
            }]
        };

    private sealed class FakeEvidenceReviewRepository : IEvidenceReviewRepository
    {
        public long? RequestedEvidenceId { get; private set; }
        public long? RequestedUserId { get; private set; }
        public long? SavedEvidenceId { get; private set; }
        public long? SavedBy { get; private set; }
        public IReadOnlyList<EvidenceReviewExtractedField>? SavedFields { get; private set; }
        public Exception? SaveException { get; init; }

        public Task<EvidenceReviewData?> GetEvidenceReviewAsync(
            long evidenceId,
            long userId,
            CancellationToken cancellationToken = default) =>
            GetEvidenceReviewAsyncCore(evidenceId, userId);

        private Task<EvidenceReviewData?> GetEvidenceReviewAsyncCore(long evidenceId, long userId)
        {
            RequestedEvidenceId = evidenceId;
            RequestedUserId = userId;

            return Task.FromResult<EvidenceReviewData?>(new EvidenceReviewData
            {
                EvidenceId = evidenceId,
                ScalarFields =
                [
                    new EvidenceReviewFieldRow
                    {
                        EvidenceFieldId = 100,
                        EvidenceDetailId = 10,
                        EvidenceKey = "firstProperty",
                        PageNumber = 2,
                        ConfidenceScore = 0.9
                    },
                    new EvidenceReviewFieldRow
                    {
                        EvidenceFieldId = 200,
                        EvidenceDetailId = 20,
                        EvidenceKey = "secondProperty",
                        PageNumber = 8,
                        ConfidenceScore = 0.8
                    }
                ]
            });
        }

        public Task SaveExtractedFieldsAsync(
            long evidenceId,
            IReadOnlyList<EvidenceReviewExtractedField> fields,
            long updatedBy,
            string? evidencedValue,
            CancellationToken cancellationToken = default)
        {
            if (SaveException is not null)
            {
                throw SaveException;
            }

            SavedEvidenceId = evidenceId;
            SavedBy = updatedBy;
            SavedFields = fields;
            return Task.CompletedTask;
        }
    }
}
