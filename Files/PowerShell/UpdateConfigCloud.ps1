function GetScript() {
    param(
        [string]$configPath
        )
    return @{ Configured = Select-String -path $configPath -pattern "cloud:" -allmatches -simplematch -quiet}
}

function SetScript() {
    param(
        [string]$configPath,
        [string]$certPath,
        [string]$certPassword,
        [string]$subscriptionId,
        [string]$serviceName,
        [string]$storageAccount,
        [string]$storageAccountKey
         )
    Write-Verbose "Configuring azure plugin in $configPath" -Verbose
    Add-Content -path $configPath "`ncloud:`n`tazure:`n`t`tkeystore: $certPath`n`t`tpassword: $certPassword`n`t`tsubscription_id: $subscriptionId`n`t`tservice_name: $serviceName"
    if( $storageAccountKey){ 
        Write-Verbose "Storage key present; Configuring snapshot repository:  $configPath" -Verbose
        Add-Content -path $configPath "`t`tstorage_account: $storageAccount`n`t`tstorage_key: $storageAccountKey"
    }
}

function TestScript() {
    param(
        [string]$configPath
         )
    if (Select-String -path $configPath -pattern "cloud:" -allmatches -simplematch -quiet) {
        Write-Verbose "Elastic Search Config already has Cloud settings" -Verbose
        return $true
    }
    return $false
}

