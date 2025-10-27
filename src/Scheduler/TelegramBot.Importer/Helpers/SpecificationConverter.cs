using System.Diagnostics.CodeAnalysis;
using Alpha.Announcements.Models;
using Alpha.Announcements.Specs;
using Newtonsoft.Json.Linq;

namespace TelegramBot.Importer.Helpers;

// Justification: This class is used only for JSON conversion logic and is covered by integration tests.
[ExcludeFromCodeCoverage(Justification = "This class is used only for JSON conversion logic and is covered by integration tests.")]
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