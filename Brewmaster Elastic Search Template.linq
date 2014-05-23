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
                .CreateTemplate("Brewmaster.ElasticSearch", "Elastic Search Farm")
				.WithAffinityGroup("{{AffinityGroup}}", "{{Region}}")
				.WithStorageAccount("{{DiskStore}}")
				.WithCloudService("{{CloudService}}","Brewmaster Elastic Search",
					cs=>cs.WithDeployment(null,d=>
					d.UsingDefaultDiskStorage("{{DiskStore}}")
						.WithVirtualMachine("{{ServerNamePrefix}}","{{VmSize}}","es-avset",vm=>
											vm.WithWindowsConfigSet("vmadmin")
											.WithNewDataDisk("disk0",40)
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
													@"Invoke-WebRequest 'http://apselasticsearchdev.blob.core.windows.net/brewmasterinstallers/jdk1.8.0_05.zip' -OutFile ""C:\setup\jdk1.8.0_05.zip"""
											,
											GetScript =
													@"return @{ JDKDownloaded = Test-Path -LiteralPath ""C:\setup\jdk1.8.0_05.zip"" -PathType Leaf }",
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
													@"return @{ ESDownloaded = Test-Path -LiteralPath ""elasticsearch-1.1.1.zip"" -PathType Leaf }",
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
Start-Process -FilePath $servicebat -ArgumentList $servicebatargs -UseNewEnvironment -Wait",
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
								new GenericResource("Service")
										{
											Name = "ConfigureElasticSearchService",
											Args = new Dictionary<string, string>
                                                {
													{"Name" , "elasticsearch-service-x64"},
													{"StartupType" , "Automatic"},
													{"State" , "Running"}
												},
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
$pluginbatargs = @(""-install mobz/elasticsearch-head"")
Write-Verbose ""Installing Elastic Search Head Plugin ($pluginbat $pluginbatargs)"" -Verbose
Start-Process -FilePath $pluginbat -ArgumentList $pluginbatargs -UseNewEnvironment -Wait",
											GetScript =
													@"return @{ Installed = Test-Path -LiteralPath ""$env:ProgramFiles\elasticsearch-1.1.1\plugins\head"" -PathType Container }",
											Requires = new[] {"[Service]ConfigureElasticSearchService"}
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
                .WithParameter("ServerNamePrefix", ParameterType.String, "Name prefix for web servers.",
                               p => p.WithDefaultValue("es")
                                     .WithRegexValidation(@"^[a-zA-Z][a-zA-Z0-9-]{1,13}$",
                                                          "Must contain 3 to 14 letters, numbers, and hyphens. Must start with a letter."))
                .WithParameter("NumberOfWebServers", ParameterType.Number, "Number of web servers.", "integer",
                               p => p.WithDefaultValue("2")
                                     .WithLimits(2, 100)
                                     .WithRegexValidation(@"^\d+$", "Must enter a positive integer between 2 and 100."));
									 
	template.Save(@"E:\Git_Local\Brewmaster.ElasticSearch\Brewmaster.ElasticSearch");
}