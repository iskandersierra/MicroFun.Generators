namespace MicroFun.Generators.DataModel;

public interface IDataGraph
{
    IDataNode Root { get; }
}

public enum DataNodeType
{
    Element,
    Collection,
}

public interface IDataNode
{
    DataNodeType NodeType { get; }
}

public interface IDataElement : IDataNode
{
    string ElementType { get; }
    IReadOnlyDictionary<string, IDataAttribute> Attributes { get; }
}

public interface IDataCollection : IDataNode
{
    IReadOnlyList<IDataIndex> Items { get; }
}

public enum DataItemType
{
    Value,
    Child,
    Reference,
}

public interface IDataItem
{
    DataItemType ItemType { get; }
}

public interface IDataAttribute : IDataNode
{
    string Label { get; }
    bool IsChild { get; }
    object Value { get; }
    IDataNode Node { get; }
}
