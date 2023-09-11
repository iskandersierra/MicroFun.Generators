using MicroFun.Generators.DataModel;

namespace MicroFun.Generators.Templates;

public interface ITemplate
{
    string Generate(IDataModel rootModel, CancellationToken cancel = default);
}
