using RedisKeyMap.Application.Abstractions;
using RedisKeyMap.Application.Configuration;
using RedisKeyMap.Application.Requests;
using RedisKeyMap.Application.Results;
using RedisKeyMap.Core.Diffing;
using RedisKeyMap.Core.Policies;

namespace RedisKeyMap.Application.UseCases;

public sealed class CheckSnapshotsUseCase(ISnapshotReader reader, IDiffReportWriter writer)
{
    public async Task<OperationResult> ExecuteAsync(CheckRequest request, AppConfiguration configuration, CancellationToken cancellationToken)
    {
        var baseline = await reader.ReadAsync(request.BaselinePath, cancellationToken).ConfigureAwait(false);
        var current = await reader.ReadAsync(request.CurrentPath, cancellationToken).ConfigureAwait(false);
        var diff = new SnapshotComparer().Compare(baseline, current, request.AllowConfigMismatch, request.AllowSourceMismatch);
        if (!diff.IsCompatible)
        {
            return new(OperationStatus.OperationalFailure, Diff: diff, Message: string.Join(Environment.NewLine, diff.CompatibilityErrors));
        }

        var policy = new PolicyEvaluator().Evaluate(diff, current, configuration.Policies);
        if (request.ReportPath is not null)
        {
            await writer.WriteAsync(diff, policy, request.ReportPath, cancellationToken).ConfigureAwait(false);
        }

        return new(policy.Passed ? OperationStatus.Success : OperationStatus.PolicyViolation, Diff: diff, Policy: policy);
    }
}
