# DockerStats
CLI tool to log Docker Stats in CSV format using C# Docker Client SDK.
# Source code build
1. Checkout the DockerStats repo
    ```shell
    git clone https://github.com/madhub/DockerStats.git
    ```  
2. Build self contained executable that work on windows & Linux
      * Windows Build
        ```shell
        // Binary will be published to bin\Release\net6.0\publish\win-x64\
        dotnet publish -c Release -p:PublishProfile=WinProfile
        ```  
    * Linux Build
        ```shell
        // Binary will be published to bin\Release\net6.0\publish\linux-x64 directory
        dotnet publish -c Release -p:PublishProfile=LinuxProfile
        ```  
3. Run command
    ```shell
        DockerStats --help
    ```