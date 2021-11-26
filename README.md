# MR.Analyzers.Records

[![CI](https://github.com/mrahhal/MR.Analyzers.Records/actions/workflows/ci.yml/badge.svg)](https://github.com/mrahhal/MR.Analyzers.Records/actions/workflows/ci.yml)
[![NuGet version](https://badge.fury.io/nu/MR.Analyzers.Records.svg)](https://www.nuget.org/packages/MR.Analyzers.Records)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)

Analyzers relating to records.

## Usage

Add the following in any ItemGroup in your csproj:
```xml
<PackageReference Include="MR.Analyzers.Records" Version="1.0.0" PrivateAssets="All" />
```

## Rules

- `MRR1000`: ConvertClassToRecord: Analyzer/Fixer for converting a class to a record
