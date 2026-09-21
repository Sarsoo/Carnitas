using Carnitas.Options;
using LibGit2Sharp;
using Microsoft.Extensions.Options;

namespace Carnitas.Source;

public interface ICheckoutManager
{
    void Checkout(string url, string workingDirectory);
}

public class CheckoutManager(IOptions<WorkerOptions> options) : ICheckoutManager
{
    public void Checkout(string url, string workingDirectory)
    {
        using var repo = new Repository(Repository.Clone(url, workingDirectory));
    }
}