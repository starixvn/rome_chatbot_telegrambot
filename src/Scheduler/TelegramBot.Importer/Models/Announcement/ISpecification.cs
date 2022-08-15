using System.ComponentModel;

namespace TelegramBot.Importer.Models.Announcement;

public enum ConjunctionOperator
{
    And = 1,
    Or = 2
}

public enum SpecificationOperator
{
    In = 1,
    [Description("Not In")]
    NotIn = 2
}

public interface ISpecification<T>
{
    string Id { get; }

    string Name { get; }

    string TypeName { get; }

    bool Combinable { get; }

    dynamic ValueRange { get; set; }

    dynamic Value { get; set; }

    ConjunctionOperator ConjunctionOperator { get; set; }

    SpecificationOperator SpecificationOperator { get; set; }

    ISpecification<T> And(ISpecification<T> specification);

    ISpecification<T> Or(ISpecification<T> specification);

    bool IsSatisfiedBy(T o);
}