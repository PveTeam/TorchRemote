using Json.Schema.Generation;
using Json.Schema.Generation.Intents;
using Torch.Views;

namespace TorchRemote.Plugin.Refiners;

public class DisplayAttributeRefiner : ISchemaRefiner
{
    public bool ShouldRun(SchemaGenerationContextBase context)
    {
        return context.GetAttributes().OfType<DisplayAttribute>().Any();
    }

    public void Run(SchemaGenerationContextBase context)
    {
        foreach (var displayAttribute in context.GetAttributes().OfType<DisplayAttribute>())
        {
            if (!string.IsNullOrEmpty(displayAttribute.Name))
                context.Intents.Add(new TitleIntent(displayAttribute.Name));

            if (!string.IsNullOrEmpty(displayAttribute.Description))
                context.Intents.Add(new DescriptionIntent(displayAttribute.Description));

            if (displayAttribute.ReadOnly)
                context.Intents.Add(new ReadOnlyIntent(displayAttribute.ReadOnly));
        }
    }
}