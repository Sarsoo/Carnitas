namespace Carnitas.Workflow.Stage;

public static class StageContext
{
    public static AsyncLocal<string> CheckoutPath { get; } = new();
}