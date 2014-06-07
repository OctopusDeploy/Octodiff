
function Assert-That ($val, $error) {
    if ($val -ne $true) {
        Write-Error "Test failed: $error"
    }
}

function Generate-RandomFile ($maxSizeInKb) {
    $size = Get-Random -maximum $maxSizeInKb
    $name = [System.Guid]::NewGuid().ToString() + ".txt"
    $name = (Join-Path (Resolve-Path .) $name)
    $data = New-Object System.IO.StreamWriter $name
    $rand = new-object System.Random
    for ($i = 0; $i -le $size; $i++) {
        for ($j = 0; $j -le 1020; $j++){
            $c = [char]$rand.Next(65, 90)
            $data.Write($c)
        }
        $data.WriteLine()
    }
    $data.Close()
}


$here = Split-Path -Parent $MyInvocation.MyCommand.Path

function Run-OctodiffSimulation($iterations) {
    cd $here\..\Octodiff\bin

    mkdir .\Temp -ErrorAction SilentlyContinue
    Remove-Item -Recurse -Force .\temp

    mkdir .\Temp -ErrorAction SilentlyContinue | Out-Null
    pushd .\Temp

    for ($i = 1; $i -le $iterations; $i++) { 
        $name = "Package" + $i  
        mkdir $name -ErrorAction SilentlyContinue | Out-Null
        pushd $name  | Out-Null
        for ($j = 0; $j -lt $i; $j++) {
            Generate-RandomFile ($i * 200)
        }
        popd 

        $original = join-path (resolve-path .) ($name + "_orig.nupkg")
        dir $name | ToZip $original 

        pushd $name  | Out-Null
        for ($j = 0; $j -lt $i / 8; $j++) {
            Generate-RandomFile ($i * 100)
        }
        popd

        $newfile = join-path (resolve-path .) ($name + "_new.nupkg")
        dir $name | ToZip $newfile 

        $watch = [System.Diagnostics.Stopwatch]::StartNew()
        $sigfile = $original + ".octosig"
        & ..\Octodiff.exe signature $original $sigfile
        Assert-That ($LASTEXITCODE -eq 0) "Error creating signature: exit code $LASTEXITCODE"
        
        $deltafile = $original + ".octodelta"
        & ..\Octodiff.exe delta $sigfile $newfile $deltafile
        Assert-That ($LASTEXITCODE -eq 0) "Error creating delta: exit code $LASTEXITCODE"
        
        $outfile = $newfile + "_2"
        & ..\Octodiff.exe patch $original $deltafile $outfile
        Assert-That ($LASTEXITCODE -eq 0) "Error applying delta: exit code $LASTEXITCODE"

        
        Write-Host "Scenario: $i"
        Write-Host " Original size:   $((get-item $original).Length / 1024)K"
        Write-Host " New size:        $((get-item $newfile).Length / 1024)K"
        Write-Host " Signature size:  $((get-item $sigfile).Length / 1024)K"
        Write-Host " Delta size:      $((get-item $deltafile).Length / 1024)K"
        Write-Host " Time taken:      $($watch.ElapsedMilliseconds)ms"
    }
    popd
}


Set-StrictMode -V 1.0
[Reflection.Assembly]::LoadWithPartialName("WindowsBase") | Out-Null
function ToZip($fileName, $relativeBaseDirectory=$null, [switch] $appendToZip=$false, $verbose=$true)
{
    begin
    {
        $zipCreated = { (Get-Variable -ErrorAction SilentlyContinue -Name zipFile) -ne $null }

        $mode = [System.IO.FileMode]::Create
        if ($appendToZip)
        {
            $mode = [System.IO.FileMode]::Open
        }
        $zipFile = [System.IO.Packaging.Package]::Open($fileName, $mode)
    }
    process
    {
        if  ((&$zipCreated) -and ([System.IO.File]::Exists($_.FullName) -eq $true))
        {
             
            $zipFileName = $_.FullName
            if ($relativeBaseDirectory -ne $null)           
            {
                #$directoryName = [System.IO.Path]::GetDirectoryName($_.FullName)
                $zipFileName = $_.FullName.SubString($relativeBaseDirectory.Length, $_.FullName.Length-$relativeBaseDirectory.Length)               
            }
             
             
            $destFilename = [System.IO.Path]::Combine(".\\", $zipFileName)
            #$destFilename = $destFilename.Replace(" ", "_")
            $uri = New-Object Uri -ArgumentList ($destFilename, [UriKind]::Relative)
            $uri = [System.IO.Packaging.PackUriHelper]::CreatePartUri($uri)
                          
            if ($zipFile.PartExists($uri))
            {
                $zipFile.DeletePart($uri);
            }
             
            $part = $zipFile.CreatePart($uri, [string]::Empty, [System.IO.Packaging.CompressionOption]::Normal)
            $dest = $part.GetStream()
             
            $srcStream = New-Object System.IO.FileStream -ArgumentList ($_.FullName, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read)
            try
            {
                $srcStream.CopyTo($dest)
            }
            finally
            {
                $srcStream.Close()
            }           
        }
    }
    end
    {
        if  (&$zipCreated)
        {
            $zipFile.Close()
        }
    }
}

clear
Run-OctodiffSimulation -iterations 5
