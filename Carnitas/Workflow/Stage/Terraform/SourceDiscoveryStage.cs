using Carnitas.Workflow.Output;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Sarsoo.Terraform.Source.FileSystem;

namespace Carnitas.Workflow.Stage.Terraform;

public class SourceDiscoveryTerraformStage(
    IRootModuleReporter? rootModuleReporter,
    ILogger<SourceDiscoveryTerraformStage> logger
)
    : StageBuilder<SourceDiscoveryTerraformStage>, IStage
{
    private string? _repositoryId;

    public SourceDiscoveryTerraformStage WithRepositoryId(string? repositoryId)
    {
        _repositoryId = repositoryId;
        return this;
    }

    protected override void CreateCommand()
    {
    }

    public override string Name => "Discover Sources";
    public override bool Retryable => true;

    public override async Task<IStageResult> Run(CancellationToken ct = default)
    {
        try
        {
            var root = BasePath;

            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            {
                logger.LogError("Source discovery stage {Id} has no checkout to scan", Id);
                return new StageResult(Id, StageState.Failure);
            }

            var relativePaths = Discover(root, ct);

            logger.LogInformation("Discovered {Count} root modules in {Root}", relativePaths.Count, root);

            if (rootModuleReporter is not null)
            {
                await rootModuleReporter
                    .ReportRootModules(Id, _repositoryId ?? string.Empty, relativePaths, ct)
                    .ConfigureAwait(false);
            }

            return new StageResult(Id, StageState.Success);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Source discovery stage {Id} failed", Id);
            return new StageResult(Id, StageState.Failure);
        }
    }

    private static IReadOnlyList<string> Discover(string root, CancellationToken ct)
    {
        using var repository = new Repository(root);

        var tips = repository.Branches
            .Where(b => b.IsRemote && !b.FriendlyName.EndsWith("/HEAD", StringComparison.Ordinal))
            .Select(b => b.Tip)
            .Append(repository.Head.Tip)
            .Where(t => t is not null)
            .DistinctBy(t => t!.Sha)
            .ToList();

        var paths = new HashSet<string>(StringComparer.Ordinal);

        foreach (var tip in tips)
        {
            ct.ThrowIfCancellationRequested();

            Commands.Checkout(repository, tip!);

            foreach (var directory in SourceResolver.FindSourceDirectories(root))
            {
                paths.Add(Path.GetRelativePath(root, directory).Replace('\\', '/'));
            }
        }

        return paths
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();
    }
}
