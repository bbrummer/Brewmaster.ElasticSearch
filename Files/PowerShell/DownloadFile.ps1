
function TestScript() {
    param(
        [string]$diskPath
         ) 
    if (Test-Path -LiteralPath $diskPath -PathType Leaf){
        Write-Verbose "$diskPath already exists." -Verbose
        return $true
        }
    return $false
}

function GetScript() {
    param(
        [string]$diskPath
         ) 
    return @{ Downloaded = Test-Path -LiteralPath $diskPath -PathType Leaf }
}

function SetScript() {
    param(
        [string]$diskPath,
        [string]$downloadUrl
         ) 
    Write-Verbose "Downloading from $downloadUrl to $diskPath." -Verbose
    Invoke-WebRequest $downloadUrl -OutFile $diskPath
}
