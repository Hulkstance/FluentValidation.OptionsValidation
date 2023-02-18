# FluentValidation.OptionsValidation

![CI](https://github.com/Hulkstance/FluentValidation.OptionsValidation/actions/workflows/CI.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/vpre/Hulkstance.FluentValidation.OptionsValidation.svg)](https://www.nuget.org/packages/Hulkstance.FluentValidation.OptionsValidation)
[![NuGet](https://img.shields.io/nuget/dt/Hulkstance.FluentValidation.OptionsValidation.svg)](https://www.nuget.org/packages/Hulkstance.FluentValidation.OptionsValidation) 

## Install

You should install [Hulkstance.FluentValidation.OptionsValidation with NuGet](https://www.nuget.org/packages/Hulkstance.FluentValidation.OptionsValidation):

```
Install-Package Hulkstance.FluentValidation.OptionsValidation
```

Or via the .NET Core command line interface:

```
dotnet add package Hulkstance.FluentValidation.OptionsValidation
```

Either commands, from Package Manager Console or .NET Core CLI, will download and install Hulkstance.FluentValidation.OptionsValidation and all required dependencies.

## Usage

```cs
builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);

builder.Services
    .AddOptions<ExampleOptions>()
    .Bind(builder.Configuration.GetSection(ExampleOptions.SectionName))
    .ValidateFluentValidation()
    .ValidateOnStart();
```
