# Cranky - .NET API Documentation Coverage Analyzer

Cranky is a command-line tool for analyzing the documentation coverage of public APIs in your .NET projects and solutions. It helps you ensure that your public APIs are adequately documented. This README provides an overview of how to use Cranky and its features.

## Features

Cranky supports the following command-line arguments:

- **-p|--project \<csproj>**: Specify a .csproj file to analyze. Repeatable.

- **-s|--solution \<sln>**: Specify a .sln file to analyze the entire solution. (only csproj projects are analyzed)

- **-x|--exclude \<pattern>**: Specify a pattern to exclude projects from analysis. Supports glob patterns. Repeatable.

- **--github**: Enable output compatible with GitHub Actions.

- **--azure**: Enable output compatible with Azure Pipelines.

- **--json**: Enable output in JSON format.

- **--percentages \<min,accept>**: Specify minimum and acceptable thresholds for coverage as two comma-separated numbers. By default, these values are 50 and 90.

- **-e|--set-exit-code**: Set an exit code other than 0 if coverage is below the minimum. By default, the exit code is not set.

- **--debug**: Enable build output for debugging.

The default output just writes information to stdout (including ANSI color codes).

## JSON Output

When using the `--json` option, Cranky outputs the following JSON:

```json
{
  "Messages": [
    {
      "Type": "info",
      "Message": "Documentation coverage passed \u2705"
    }
  ],
  "Result": {
    "total": 708,
    "documented": 412,
    "undocumented": 296,
    "percent": 58,
    "health": "\u2705",
    "message": "Documentation coverage passed \u2705",
    "badge": "https://img.shields.io/badge/Documentation%20Coverage-58%25-brightgreen"
  }
}
```

## GitHub Actions Integration

When using the `--github` option, Cranky provides the following outputs for use in GitHub actions:

- **total**: The total number of public API members that need documentation.

- **undocumented**: The number of undocumented API members.

- **documented**: The number of documented API members.

- **percent**: A percentage between 0 and 100 showing the total coverage.

- **health**: An emoji to indicate the coverage status (❌ = error, ⚠️ = warning, ✅ = success).

- **badge**: A badge showing the coverage with an indicator color.

- **message**: A text message stating the coverage status.

## Usage

To analyze a project, use the `-p` option and provide the path to the .csproj file. For analyzing a solution, use the `-s` option and specify the .sln file.

Example usage:

```shell
# Analyze a project/solution in the current directory
cranky

# Analyze a solution with custom percentage thresholds
cranky -s MySolution.sln --percentages 40,85

# Analyze a project, output for GitHub Actions and set exit code
cranky -p MyProject.csproj --github --set-exit-code

# Exclude test projects
cranky -s MySolution.sln --exclude *.Tests*
```

## GitHub Actions Workflow

To integrate Cranky with GitHub actions, include the following step in your workflow file:

```yaml
steps:
  - uses: actions/checkout@v4
    with:
      fetch-depth: 0

  - name: Run Cranky
    id: docblocks
    run: dotnet tool update cranky -g && cranky --project ./MyProject.csproj --github --set-exit-code --percentages 0,50

  - name: Report Results
    run: |
      echo "Docblock Coverage: ${{ steps.docblocks.outputs.percent }}%"
```

This will run Cranky with GitHub actions-compatible output.

## License

This project is licensed under the [MIT License](https://github.com/ricardoboss/cranky/blob/main/LICENSE).
