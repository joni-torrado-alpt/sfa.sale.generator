# sfa-test

Console application for Unit and Integration Testing

# Some comands

Run all tests:

```
dotnet test
```

Run specific test:

```
dotnet test --filter NewInstallation
```

Run specific test with details on console:

```
dotnet test --filter NewInstallation --logger "console;verbosity=detailed"
```

Set Environment variables for debug:

```
$env:PWDEBUG=1
dotnet test
```

Get Environment variables:

```
gci env:* | sort-object name
```

# Create tests with Playwright Codegen

```
.\playwright-codegen.ps1
```
