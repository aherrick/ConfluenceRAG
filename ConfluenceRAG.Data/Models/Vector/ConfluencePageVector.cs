// dims are set at runtime based on the embedding model used

namespace ConfluenceRAG.Data.Models.Vector;

public class ConfluencePageVector
{
    public string Key { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public ReadOnlyMemory<float> Vector { get; set; }
}