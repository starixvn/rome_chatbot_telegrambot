using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;
using TelegramBot.Importer.Models.Announcement;

namespace TelegramBot.Importer.Helpers;

[ExcludeFromCodeCoverage]
public class SpecificationConverter : JsonCreationConverter<ISpecification<IParam>>
{
    protected override Type GetTargetType(Type type, JObject obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj), "Object cannot be null");
        }

        return Type.GetType(obj.GetValue("TypeName", StringComparison.OrdinalIgnoreCase).ToString());
    }
}