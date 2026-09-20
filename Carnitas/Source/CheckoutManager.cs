using LibGit2Sharp;

namespace Carnitas.Source;

public class CheckoutManager
{
    public void Checkout(string url, string workingDirectory)
    {
        using var repo = new Repository(Repository.Clone(url, workingDirectory));
    }
}