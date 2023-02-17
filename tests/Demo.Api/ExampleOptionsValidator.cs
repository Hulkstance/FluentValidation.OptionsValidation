﻿using FluentValidation;

namespace Demo.Api;

public class ExampleOptionsValidator : AbstractValidator<ExampleOptions>
{
    public ExampleOptionsValidator()
    {
        RuleFor(x => x.LogLevel)
            .IsEnumName(typeof(LogLevel));

        RuleFor(x => x.Counter)
            .InclusiveBetween(1, 10);
    }
}
