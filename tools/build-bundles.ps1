param(
    [string]$ProjectDir = (Get-Location).Path
)

$ErrorActionPreference = "Stop"

$configPath = Join-Path $ProjectDir "bundleconfig.json"
if (-not (Test-Path $configPath)) {
    throw "bundleconfig.json not found at $configPath"
}

$bundles = Get-Content $configPath -Raw | ConvertFrom-Json

function Minify-Css {
    param([string]$Content)

    $minified = [System.Text.RegularExpressions.Regex]::Replace(
        $Content,
        "/\*[\s\S]*?\*/",
        "",
        [System.Text.RegularExpressions.RegexOptions]::Singleline)

    $minified = [System.Text.RegularExpressions.Regex]::Replace($minified, "\s+", " ")
    $minified = [System.Text.RegularExpressions.Regex]::Replace($minified, "\s*([{}:;,>])\s*", '$1')
    return $minified.Trim()
}

function Minify-Js {
    param([string]$Content)

    $minified = [System.Text.RegularExpressions.Regex]::Replace(
        $Content,
        "(?ms)^\s*//.*?$",
        "")

    $minified = [System.Text.RegularExpressions.Regex]::Replace($minified, "\r?\n{2,}", "`n")
    $minified = [System.Text.RegularExpressions.Regex]::Replace($minified, "[ \t]{2,}", " ")
    return $minified.Trim()
}

foreach ($bundle in $bundles) {
    $parts = foreach ($inputFile in $bundle.inputFiles) {
        $inputPath = Join-Path $ProjectDir $inputFile
        if (-not (Test-Path $inputPath)) {
            throw "Bundle input not found: $inputPath"
        }

        [System.IO.File]::ReadAllText($inputPath)
    }

    $combined = [string]::Join([Environment]::NewLine, $parts)
    $outputPath = Join-Path $ProjectDir $bundle.outputFileName
    $outputDir = Split-Path $outputPath -Parent

    if (-not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Path $outputDir | Out-Null
    }

    $result = if ($bundle.type -eq "css") {
        Minify-Css -Content $combined
    }
    elseif ($bundle.type -eq "js") {
        Minify-Js -Content $combined
    }
    else {
        throw "Unsupported bundle type: $($bundle.type)"
    }

    [System.IO.File]::WriteAllText($outputPath, $result, [System.Text.UTF8Encoding]::new($false))
    Write-Host "Bundled $($bundle.outputFileName)"
}
