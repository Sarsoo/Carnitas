using Carnitas.Source;

namespace Carnitas.Test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var checkoutManger = new CheckoutManager();
        
        checkoutManger.Checkout("https://github.com/Sarsoo/dotfiles.git", "~/lab");
    }
}