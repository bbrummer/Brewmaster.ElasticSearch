<Query Kind="Program">
  <Connection>
    <ID>84a05001-6daa-45cb-a099-2b6cda22c67c</ID>
    <Persist>true</Persist>
    <Server>(localdb)\v11.0</Server>
    <Database>schedeventstore</Database>
    <ShowServer>true</ShowServer>
  </Connection>
  <NuGetReference>Brewmaster.TemplateSDK.Contracts</NuGetReference>
  <Namespace>Brewmaster.TemplateSDK.Contracts.Fluent</Namespace>
  <Namespace>Brewmaster.TemplateSDK.Contracts.Models</Namespace>
</Query>

void Main()
{
	var template = WithTemplateExtensions
                .CreateTemplate("Brewmaster.ElasticSearch", "Elastic Search Cluster")
				.WithAffinityGroup("{{AffinityGroup}}", "{{Region}}")
				.WithStorageAccount("{{DiskStore}}")
				.WithCloudService("{{CloudService}}","Brewmaster Elastic Search",
					cs=>cs.WithDeployment(null,d=>
					d.UsingDefaultDiskStorage("{{DiskStore}}")
						.WithVirtualMachine("{{ServerNamePrefix}}","{{VmSize}}","es-avset",vm=>
											vm.WithWindowsConfigSet("vmadmin")
											.WithNewDataDisk("disk0",100)
											.UsingConfigSet("ElasticSearchServer")))
				)
				.WithCredential("vmadmin","{{AdminName}}","{{AdminPassword}}")
				.WithConfigSet("ElasticSearchServer", "Elastic Search Server",
                          r =>
                          r.WithEndpoint("HTTP", 9200, 9200,
                                         new EndpointLoadBalancerProbe
                                             {
                                                 Name = "http",
                                                 Protocol = "Http",
                                                 Path = "/",
                                                 IntervalInSeconds = 15,
                                                 TimeoutInSeconds = 31
                                             })
                           .WithEndpoint("HTTPS", 443, 443,
                                         new EndpointLoadBalancerProbe
                                             {
                                                 Name = "https",
                                                 Protocol = "Tcp",
                                                 IntervalInSeconds = 15,
                                                 TimeoutInSeconds = 31
                                             })
                           .UsingConfiguration("InstallElasticSearch"));
						   
	template.Configurations = new[]
                {
                    new Brewmaster.TemplateSDK.Contracts.Models.Configuration
                        {
                            Name = "InstallElasticSearch",
                            Resources = new []
                                {
								new GenericResource("xFormatDisks")
                                        {
                                            Name = "FormatRawDisks",
											ImportModule= "xAzureDataDisks",
											ImportTypeName = "ADITI_xFormatDisks",
                                            Args = new Dictionary<string, string>
                                                {
													{"FirstDriveLetter", "F"}
                                                },
                                        },
								new GenericResource("File")
                                        {
                                            Name = "SetupFolder",
                                            Args = new Dictionary<string, string>
                                                {
													{"Type", "Directory"},
                                                    {"DestinationPath", @"C:\Setup"},
                                                    {"Ensure", "Present"},
                                                },
                                        },
								new ScriptResource
                                        {
                                            Name = "DownloadJRE",
                                            Credential = "vmadmin",
											TestScript =
													@"if (Test-Path -LiteralPath ""C:\setup\jdk1.8.0_05.zip"" -PathType Leaf)
{Write-Verbose ""C:\setup\jdk1.8.0_05.zip already exists."" -Verbose
return $true}
return $false",
											SetScript =
													@"Write-Verbose ""Downloading JRE. This can take around 20 mins."" -Verbose
Invoke-WebRequest 'http://apselasticsearchdev.blob.core.windows.net/brewmasterinstallers/jdk1.8.0_05.zip' -OutFile ""C:\setup\jdk1.8.0_05.zip"""
											,
											GetScript =
													@"return @{ Downloaded = Test-Path -LiteralPath ""C:\setup\jdk1.8.0_05.zip"" -PathType Leaf }",
											Requires = new[] {"[File]SetupFolder"}
										},
								new ScriptResource
                                        {
                                            Name = "DownloadElasticSearch",
                                            Credential = "vmadmin",
											TestScript =
													@"if (Test-Path -LiteralPath ""C:\setup\elasticsearch-1.1.1.zip"" -PathType Leaf)
{Write-Verbose ""C:\setup\jdk1.8.0_05.zip already exists."" -Verbose
return $true}
return $false",
											SetScript =
													@"Invoke-WebRequest 'https://download.elasticsearch.org/elasticsearch/elasticsearch/elasticsearch-1.1.1.zip' -OutFile ""C:\setup\elasticsearch-1.1.1.zip"""
											,
											GetScript =
													@"return @{ Downloaded = Test-Path -LiteralPath ""C:\setup\elasticsearch-1.1.1.zip"" -PathType Leaf }",
											Requires = new[] {"[File]SetupFolder"}
										},
								new ScriptResource
                                        {
                                            Name = "DownloadPluginHead",
                                            Credential = "vmadmin",
											TestScript =
													@"if (Test-Path -LiteralPath ""C:\setup\elasticsearch-head-master.zip"" -PathType Leaf)
{Write-Verbose ""C:\setup\elasticsearch-head-master.zip already exists."" -Verbose
return $true}
return $false",
											SetScript =
													@"Invoke-WebRequest 'https://github.com/mobz/elasticsearch-head/archive/master.zip' -OutFile ""C:\setup\elasticsearch-head-master.zip"""
											,
											GetScript =
													@"return @{ Downloaded = Test-Path -LiteralPath ""C:\setup\elasticsearch-head-master.zip"" -PathType Leaf }",
											Requires = new[] {"[File]SetupFolder"}
										},
								new ScriptResource
                                        {
                                            Name = "DownloadPluginAzure",
                                            Credential = "vmadmin",
											TestScript =
													@"if (Test-Path -LiteralPath ""C:\setup\elasticsearch-cloud-azure-2.2.0.zip"" -PathType Leaf)
{Write-Verbose ""C:\setup\elasticsearch-cloud-azure-2.2.0.zip already exists."" -Verbose
return $true}
return $false",
											SetScript =
													@"Invoke-WebRequest 'http://download.elasticsearch.org/elasticsearch/elasticsearch-cloud-azure/elasticsearch-cloud-azure-2.2.0.zip' -OutFile ""C:\setup\elasticsearch-cloud-azure-2.2.0.zip"""
											,
											GetScript =
													@"return @{ Downloaded = Test-Path -LiteralPath ""C:\setup\elasticsearch-cloud-azure-2.2.0.zip"" -PathType Leaf }",
											Requires = new[] {"[File]SetupFolder"}
										},
								new ScriptResource
                                        {
                                            Name = "DownloadCertificate",
                                            Credential = "vmadmin",
											TestScript =
													@"if (Test-Path -LiteralPath ""C:\setup\azurecert.pfx"" -PathType Leaf)
{Write-Verbose ""C:\setup\azurecert.pfx already exists."" -Verbose
return $true}
return $false",
											SetScript =
													@"Invoke-WebRequest '{{AzureCertificateUrl}}' -OutFile ""C:\setup\azurecert.pfx"""
											,
											GetScript =
													@"return @{ Downloaded = Test-Path -LiteralPath ""C:\setup\azurecert.pfx"" -PathType Leaf }",
											Requires = new[] {"[File]SetupFolder"}
										},
								new GenericResource("Archive")
										{
											Name = "UnpackJRE",
											Args = new Dictionary<string, string>
                                                {
													{"Path" , @"C:\setup\jdk1.8.0_05.zip"},
													{"Destination" , @"%ProgramFiles%"},
													{"Ensure" , "Present"}
												},
  											Requires = new[] {"[Script]DownloadJRE"}
										},
								new GenericResource("Archive")
										{
											Name = "UnpackElasticSearch",
											Args = new Dictionary<string, string>
                                                {
													{"Path" , @"C:\setup\elasticsearch-1.1.1.zip"},
													{"Destination" , @"%ProgramFiles%"},
													{"Ensure" , "Present"}
												},
  											Requires = new[] {"[Script]DownloadElasticSearch"}
										},
								new GenericResource("Environment")
										{
											Name = "SetJavaHome",
											Args = new Dictionary<string, string>
                                                {
													{"Name" , "JAVA_HOME"},
													{"Value" , @"%ProgramFiles%\jdk1.8.0_05\"},
													{"Ensure" , "Present"}
												},
  											Requires = new[] {"[Archive]UnpackJRE"}
										},
								new ScriptResource
                                        {
                                            Name = "InstallElasticSearchService",
                                            Credential = "vmadmin",
											TestScript =
													@"if (Get-WmiObject -Class Win32_Service -Filter ""Name='elasticsearch-service-x64'"")
{Write-Verbose ""elasticsearch-service-x64 already exists."" -Verbose
return $true}
return $false",
											SetScript =
													@"$servicebat = ""$env:ProgramFiles\elasticsearch-1.1.1\bin\service.bat""
$servicebatargs = @(""install"")
Write-Verbose ""Installing Elastic Search Service ($servicebat $servicebatargs)"" -Verbose
Start-Process -FilePath $servicebat -ArgumentList $servicebatargs -UseNewEnvironment -Wait -RedirectStandardOutput $env:BrewmasterDir\Logs\elasticsearchservice.log",
											GetScript =
													@"return @{ ServiceInstalled = Get-WmiObject -Class Win32_Service -Filter ""Name='elasticsearch-service-x64'"" }",
											Requires = new[] {"[Environment]SetJavaHome"}
										},
								new ScriptResource
                                        {
                                            Name = "EnableFirewallPort9200",
                                            Credential = "vmadmin",
											TestScript =
													@"if ((Get-NetFirewallRule | Where-Object { $_.Name -eq 'ElasticSearch9200' }) -ne $null)
{Write-Verbose ""Firewall Rule ElasticSearch9200 already exists."" -Verbose
return $true}
return $false",
											SetScript =
													@"New-NetFirewallRule -Name ElasticSearch9200 -DisplayName ""Elastic Search Port 9200"" -Direction Inbound -LocalPort 9200 -Protocol TCP -Action Allow",
											GetScript =
													@"return @{ Enabled = (Get-NetFirewallRule | Where-Object { $_.Name -eq 'ElasticSearch9200' }) -ne $null }",
											Requires = new[] {"[Script]InstallElasticSearchService"}
										},
								new ScriptResource
                                        {
                                            Name = "EnableFirewallPort9300",
                                            Credential = "vmadmin",
											TestScript =
													@"if ((Get-NetFirewallRule | Where-Object { $_.Name -eq 'ElasticSearch9300' }) -ne $null)
{Write-Verbose ""Firewall Rule ElasticSearch9300 already exists."" -Verbose
return $true}
return $false",
											SetScript =
													@"New-NetFirewallRule -Name ElasticSearch9300 -DisplayName ""Elastic Search Port 9300"" -Direction Inbound -LocalPort 9300 -Protocol TCP -Action Allow",
											GetScript =
													@"return @{ Enabled = (Get-NetFirewallRule | Where-Object { $_.Name -eq 'ElasticSearch9300' }) -ne $null }",
											Requires = new[] {"[Script]InstallElasticSearchService"}
										},
								new ScriptResource
                                        {
                                            Name = "InstallPluginHead",
                                            Credential = "vmadmin",
											TestScript =
													@"if (Test-Path -LiteralPath ""$env:ProgramFiles\elasticsearch-1.1.1\plugins\head"" -PathType Container)
{Write-Verbose ""Elastic Search Head Plugin already installed."" -Verbose
return $true}
return $false",
											SetScript =
													@"$pluginbat = ""$env:ProgramFiles\elasticsearch-1.1.1\bin\plugin.bat""
$pluginbatargs = @(""-install mobz/elasticsearch-head -url file:///c:\Setup\elasticsearch-head-master.zip -verbose"")
Write-Verbose ""Installing Elastic Search Head Plugin ($pluginbat $pluginbatargs)"" -Verbose
Start-Process -FilePath $pluginbat -ArgumentList $pluginbatargs -UseNewEnvironment -Wait -RedirectStandardOutput $env:BrewmasterDir\Logs\headpluginlog.log",
											GetScript =
													@"return @{ Installed = Test-Path -LiteralPath ""$env:ProgramFiles\elasticsearch-1.1.1\plugins\head"" -PathType Container }",
											Requires = new[] {"[Script]InstallElasticSearchService"}
										},
								new ScriptResource
                                        {
                                            Name = "InstallPluginAzure",
                                            Credential = "vmadmin",
											TestScript =
													@"if (Test-Path -LiteralPath ""$env:ProgramFiles\elasticsearch-1.1.1\plugins\cloud-azure"" -PathType Container)
{Write-Verbose ""Elastic Search Head Plugin already installed."" -Verbose
return $true}
return $false",
											SetScript =
													@"$pluginbat = ""$env:ProgramFiles\elasticsearch-1.1.1\bin\plugin.bat""
$pluginbatargs = @(""-install elasticsearch/elasticsearch-cloud-azure/2.2.0 -url file:///c:\Setup\elasticsearch-cloud-azure-2.2.0.zip -verbose"")
Write-Verbose ""Installing Elastic Search Azure Plugin ($pluginbat $pluginbatargs)"" -Verbose
Start-Process -FilePath $pluginbat -ArgumentList $pluginbatargs -UseNewEnvironment -LoadUserProfile -Wait -RedirectStandardOutput $env:BrewmasterDir\Logs\azurepluginlog.log",
											GetScript =
													@"return @{ Installed = Test-Path -LiteralPath ""$env:ProgramFiles\elasticsearch-1.1.1\plugins\cloud-azure"" -PathType Container }",
											Requires = new[] {"[Script]InstallElasticSearchService"}
										},
								new ScriptResource
                                        {
                                            Name = "UpdateConfigCloud",
                                            Credential = "vmadmin",
											TestScript =
													@"if (Select-String -path ""$env:ProgramFiles\elasticsearch-1.1.1\config\elasticsearch.yml"" -pattern ""cloud:"" -allmatches -simplematch -quiet)
{Write-Verbose ""Elastic Search Config already has Cloud settings"" -Verbose
return $true}
return $false",
											SetScript =
													@"Add-Content ""$env:ProgramFiles\elasticsearch-1.1.1\config\elasticsearch.yml"" ""`ncloud:`n`tazure:`n`t`tkeystore: C:/Setup/azurecert.pfx`n`t`tpassword: {{AzureCertificatePassword}}`n`t`tsubscription_id: {{AzureSubscriptionId}}`n`t`tservice_name: {{CloudService}}`n`t""",
											GetScript =
													@"return @{ Configured = Select-String -path ""$env:ProgramFiles\elasticsearch-1.1.1\config\elasticsearch.yml"" -pattern ""cloud:"" -allmatches -simplematch -quiet }",
											Requires = new[] {"[Script]InstallPluginAzure"}
										},
								new ScriptResource
                                        {
                                            Name = "UpdateConfigDiscovery",
                                            Credential = "vmadmin",
											TestScript =
													@"if (Select-String -path ""$env:ProgramFiles\elasticsearch-1.1.1\config\elasticsearch.yml"" -pattern ""discovery:"" -allmatches -simplematch -quiet)
{Write-Verbose ""Elastic Search Config already has Discovery settings"" -Verbose
return $true}
return $false",
											SetScript =
													@"Add-Content ""$env:ProgramFiles\elasticsearch-1.1.1\config\elasticsearch.yml"" ""`ndiscovery:`n`t`ttype: azure""",
											GetScript =
													@"return @{ Configured = Select-String -path ""$env:ProgramFiles\elasticsearch-1.1.1\config\elasticsearch.yml"" -pattern ""discovery:"" -allmatches -simplematch -quiet }",
											Requires = new[] {"[Script]InstallPluginAzure"}
										},
								new ScriptResource
                                        {
                                            Name = "UpdateConfigDataPath",
                                            Credential = "vmadmin",
											TestScript =
													@"if (Select-String -path ""$env:ProgramFiles\elasticsearch-1.1.1\config\elasticsearch.yml"" -pattern ""path.data: F:/"" -allmatches -simplematch -quiet)
{Write-Verbose ""Elastic Search Config already has Discovery settings"" -Verbose
return $true}
return $false",
											SetScript =
													@"Add-Content ""$env:ProgramFiles\elasticsearch-1.1.1\config\elasticsearch.yml"" ""`npath.data: F:/""",
											GetScript =
													@"return @{ Configured = Select-String -path ""$env:ProgramFiles\elasticsearch-1.1.1\config\elasticsearch.yml"" -pattern ""path.data: F:/"" -allmatches -simplematch -quiet }",
											Requires = new[] {"[xFormatDisks]FormatRawDisks"}
										},
								new GenericResource("Service")
										{
											Name = "ConfigureElasticSearchService",
											Args = new Dictionary<string, string>
                                                {
													{"Name" , "elasticsearch-service-x64"},
													{"StartupType" , "Automatic"},
													{"State" , "Running"}
												},
  											Requires = new[] {"[Script]UpdateConfigDiscovery"}
										},
								}
						}
				};
				
	template = template.WithParameter("Region", ParameterType.String, "Name of Azure region.", "AzureRegionName")
                .WithParameter("AffinityGroup", ParameterType.String, "Name of Azure affinity group.",
                               "AzureAffinityGroupName")
                .WithParameter("CloudService", ParameterType.String, "Name of the Azure Cloud Service.",
                               "AzureCloudServiceName")
                .WithParameter("DiskStore", ParameterType.String, "Name of Azure disk storage account.",
                               "AzureStorageName")
                .WithParameter("VMSize", ParameterType.String, "Size of the server VMs.", "AzureRoleSize",
                               p => p.WithDefaultValue("Small"))
                .WithParameter("AdminName", ParameterType.String, "Name of local administrator account.", "username",
                               p => p.WithLimits(1, 64))
                .WithParameter("AdminPassword", ParameterType.String, "Password of local administrator account.",
                               "password",
                               p => p.WithLimits(8, 127), maskValue: true)
                .WithParameter("ServerNamePrefix", ParameterType.String, "Name prefix for ElasticSearch servers.",
                               p => p.WithDefaultValue("esn")
                                     .WithRegexValidation(@"^[a-zA-Z][a-zA-Z0-9-]{1,13}$",
                                                          "Must contain 3 to 14 letters, numbers, and hyphens. Must start with a letter."))
				.WithParameter("AzureCertificateUrl", ParameterType.String, "URL to Azure certificate.",
                               "string",
                               p => p.WithLimits(8, 127))
				.WithParameter("AzureCertificatePassword", ParameterType.String, "Password for the certificate file.",
                               "password",
                               p => p.WithLimits(8, 127), maskValue: true)										  
				.WithParameter("AzureSubscriptionId", ParameterType.String, "Subscription Id.",
                               "Guid",
                               p => p.WithLimits(8, 127))			
                .WithParameter("NumberOfElasticSearchServers", ParameterType.Number, "Number of ElasticSearch servers.", "integer",
                               p => p.WithDefaultValue("2")
                                     .WithLimits(2, 100)
                                     .WithRegexValidation(@"^\d+$", "Must enter a positive integer between 2 and 100."))
				.WithParameter("DataDiskSize", ParameterType.Number, "Size of Data disk(GB).", "integer",
                               p => p.WithDefaultValue("100")
                                     .WithLimits(2, 1024)
                                     .WithRegexValidation(@"^\d+$", "Must enter a positive integer between 2 and 1024."));
									 
	template.Save(@"E:\Git_Local\Brewmaster.ElasticSearch\Brewmaster.ElasticSearch");
}