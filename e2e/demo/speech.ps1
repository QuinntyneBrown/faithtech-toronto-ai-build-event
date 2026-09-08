param([string]$Manifest, [string]$Directory)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Speech
$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
try {
    $synth.SelectVoice('Microsoft Zira Desktop')
    $synth.Rate = 0
    $lines = Get-Content -LiteralPath $Manifest -Raw | ConvertFrom-Json
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $synth.SetOutputToWaveFile((Join-Path $Directory "$i.wav"))
        $synth.Speak([string]$lines[$i])
        $synth.SetOutputToNull()
    }
} finally { $synth.Dispose() }
