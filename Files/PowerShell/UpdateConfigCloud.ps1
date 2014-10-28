param(
    [string]$configPath,
    [string]$certPath,
    [string]$certPassword,
    [string]$subscriptionId,
    [string]$serviceName,
    [string]$storageAccount,
    [string]$storageAccountKey,
    [string]$Mode
    )

    function dsc_GET() {
        return @{ Configured = Select-String -path $configPath -pattern "cloud:" -allmatches -simplematch -quiet}
    }

    function dsc_SET() {
        Add-Content -path $configPath "`ncloud:`n`tazure:`n`t`tkeystore: $certPath`n`t`tpassword: $certPassword`n`t`tsubscription_id: $subscriptionId`n`t`tservice_name: $serviceName"
        if( $storageAccountKey){ 
            Add-Content -path $configPath "`t`tstorage_account: $storageAccount`n`t`tstorage_key: $storageAccountKey"
        }
    }

    function dsc_TEST() {
            if (Select-String -path $configPath -pattern "cloud:" -allmatches -simplematch -quiet) {
                Write-Verbose "Elastic Search Config already has Cloud settings" -Verbose
                    return $true
            }
            return $false
    }

    &"dsc_$Mode"
