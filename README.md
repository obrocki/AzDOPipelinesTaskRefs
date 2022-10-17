# AzDOPipelinesTaskRefs
Scan YAML files for Azure DevOps Pipelines task references.

Based on the scraped information off the [Azure Pipelines task reference article](https://learn.microsoft.com/azure/devops/pipelines/tasks/reference/?view=azure-pipelines), this rudimentary console app with check for any YML task definitions.

## Usage

```dotnetcli
 .\AzDOYmlTasks.exe C:\Code\Pipelines false
```

Expected parameters:

- [`String`] **Root directory of YML files**.

  Subdirectories will be included in the search. E.g. C:\Code\Pipelines. Defaults to current execution directory.

- [`Boolean`] **Console/File output toggle**.

    When parameter is `true` the scan result will be printed out to `output.txt` file. Defaults to `Console` output (false).


The reference list of tasks can be extended by adding name(s) of the task to `outputTasks` list (with addition of any known tasks (line:80 Program.cs).

## Sample Output

```dotnetcli
> .\AzDOYmlTasks.exe C:\Code\Pipelines false
Parsing Azure Pipelines Task Reference
 - AndroidBuild@1
 - AndroidSigning@1

 // full list skipped

 - Xcode@2
 - Xcode@3
 - Xcode@4
 - Xcode@5
 - XcodePackageiOS@0
246 tasks parsed.

Looking up YML files in C:\Code\Pipelines root directory.
 - C:\Code\Pipelines\azure-pipelines-AZSK.yml
 - C:\Code\Pipelines\azure-pipelines-CredScan.yml
 - C:\Code\Pipelines\azure-pipelines-SAST.yml
 - C:\Code\Pipelines\azure-pipelines-SCA.yml
 - C:\Code\Pipelines\azure-pipelines.yml
 - C:\Code\Pipelines\docker-compose.ci.build.yml
 - C:\Code\Pipelines\docker-compose.override.yml
 - C:\Code\Pipelines\docker-compose.yml
 - C:\Code\Pipelines\sauce_browsers.yml
9 files found.

        File: azure-pipelines-AZSK.yml      - task: AzSKSVTs@4
        File: azure-pipelines-CredScan.yml          - task: UsePythonVersion@0
        File: azure-pipelines-CredScan.yml          - task: CmdLine@2
        File: azure-pipelines-CredScan.yml          # - task: CmdLine@2
        File: azure-pipelines-CredScan.yml          - task: PublishPipelineArtifact@1
        File: azure-pipelines-CredScan.yml          - task: PowerShell@2
        File: azure-pipelines-SAST.yml      - task: SonarQubePrepare@4
        File: azure-pipelines-SAST.yml      - task: SonarQubeAnalyze@4
        File: azure-pipelines-SAST.yml      - task: SonarQubePublish@4
        File: azure-pipelines-SCA.yml       - task: WhiteSource Bolt@20
        File: azure-pipelines.yml           - task: DockerCompose@0
        File: azure-pipelines.yml           - task: DockerCompose@0
        File: azure-pipelines.yml           - task: DockerCompose@0
        File: azure-pipelines.yml           - task: DockerCompose@0
        File: azure-pipelines.yml                 - task: SqlAzureDacpacDeployment@1
        File: azure-pipelines.yml                 - task: AzureWebAppContainer@1
        File: azure-pipelines.yml       #           - task: SqlAzureDacpacDeployment@1
        File: azure-pipelines.yml       #           - task: AzureWebAppContainer@1
18 tasks checked.


YML files requiring attention (2)
        File: azure-pipelines-AZSK.yml (1 task(s))
             - task: AzSKSVTs@4
        File: azure-pipelines-SCA.yml (1 task(s))
             - task: WhiteSource Bolt@20


Press any key to exit
```