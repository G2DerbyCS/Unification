using System;
using Unification.Models.Enums;

namespace Unification.Models.Interfaces
{
    public interface IMetadataContainer
    {
        Uri              Datasource             { get; }
        DatasourceFormat DatasourceFormat       { get; }
        string[]         MetadataFields         { get; }

        string   Metadata(string MetadataFeild);
    }
}
