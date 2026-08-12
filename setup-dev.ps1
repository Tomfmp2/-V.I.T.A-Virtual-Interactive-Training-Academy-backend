#Requires -Version 5.1
<#
.SYNOPSIS
    Monta el entorno local de VITA: base PostgreSQL, User Secrets y arranque del API.

.DESCRIPTION
    Ejecutar desde la raíz del repo (donde está Vita.sln).
    Pide confirmación antes de destruir academia_cursos.
    No imprime ni hardcodea contraseñas.

.EXAMPLE
    .\setup-dev.ps1

.EXAMPLE
    .\setup-dev.ps1 -SkipDatabase

.EXAMPLE
    .\setup-dev.ps1 -SkipSecrets -NoRun

.EXAMPLE
    .\setup-dev.ps1 -PsqlPath "C:\Program Files\PostgreSQL\18\bin\psql.exe"

.EXAMPLE
    .\setup-dev.ps1 -SuperUser "admin_pg"

.EXAMPLE
    .\setup-dev.ps1 -Force
#>
[CmdletBinding()]
param(
    [switch]$SkipDatabase,
    [switch]$SkipSecrets,
    [switch]$NoRun,
    [string]$PsqlPath,
    [string]$SuperUser,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

function Stop-WithError {
    param([string]$Message)
    Write-Host ""
    Write-Host "ERROR: $Message" -ForegroundColor Red
    exit 1
}

function ConvertFrom-SecureStringPlain {
    param([System.Security.SecureString]$Secure)
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Secure)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    }
}

function Find-PsqlExecutable {
    param([string]$ExplicitPath)

    if ($ExplicitPath) {
        if (-not (Test-Path -LiteralPath $ExplicitPath)) {
            throw "No se encontró psql en -PsqlPath: $ExplicitPath"
        }
        return (Resolve-Path -LiteralPath $ExplicitPath).Path
    }

    $fromPath = Get-Command psql -ErrorAction SilentlyContinue
    if ($fromPath) {
        return $fromPath.Source
    }

    $candidates = Get-ChildItem -Path "C:\Program Files\PostgreSQL" -Filter "psql.exe" -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.Directory.Name -eq "bin" }

    if (-not $candidates) {
        throw @"
No se encontró psql.exe.
Instala PostgreSQL o pasa la ruta explícita:
  .\setup-dev.ps1 -PsqlPath "C:\Program Files\PostgreSQL\18\bin\psql.exe"
Ver docs/EJECUTAR-EL-BACKEND.md (solución de problemas).
"@
    }

    $sorted = $candidates | Sort-Object {
        $folder = $_.Directory.Parent.Name
        $n = 0
        if ([int]::TryParse($folder, [ref]$n)) { $n } else { 0 }
    } -Descending

    return $sorted[0].FullName
}

# --- a) raíz del repo ---
if (-not (Test-Path -LiteralPath (Join-Path (Get-Location) "Vita.sln"))) {
    Stop-WithError "Ejecuta este script desde la raíz del repo (debe existir Vita.sln). Directorio actual: $(Get-Location)"
}

Write-Host "[0/3] Validando entorno..." -ForegroundColor Cyan

# --- b) SDK ---
try {
    $dotnetVersion = & dotnet --version 2>&1
    if ($LASTEXITCODE -ne 0) { throw "dotnet falló" }
    Write-Host "  SDK .NET: $dotnetVersion"
}
catch {
    Stop-WithError "Falta el SDK .NET 10 (o 'dotnet' no esta en el PATH). Instala el SDK y reintenta."
}

# --- c) psql ---
try {
    $psql = Find-PsqlExecutable -ExplicitPath $PsqlPath
    Write-Host "  psql: $psql"
}
catch {
    Stop-WithError $_.Exception.Message
}

# --- d) base de datos ---
if (-not $SkipDatabase) {
    Write-Host "[1/3] Base de datos (academia_cursos / vita_user)..." -ForegroundColor Cyan
    Write-Warning "Se va a ELIMINAR y recrear la base academia_cursos. Se perderan TODOS los datos locales de esa base. El rol vita_user y otras bases no se tocan."

    $confirm = Read-Host "Escribe SI (en mayúsculas) para continuar"
    if ($confirm -cne "SI") {
        Write-Host "Abortado: no se modificó la base ni los secretos." -ForegroundColor Yellow
        exit 1
    }

    if (-not $SuperUser) {
        $inputUser = Read-Host "Rol superusuario de PostgreSQL [postgres]"
        if ([string]::IsNullOrWhiteSpace($inputUser)) {
            $SuperUser = "postgres"
        }
        else {
            $SuperUser = $inputUser.Trim()
        }
    }

    $pgSecure = Read-Host "Contraseña del rol $SuperUser" -AsSecureString
    $env:PGPASSWORD = ConvertFrom-SecureStringPlain -Secure $pgSecure

    try {
        Write-Host "  Ejecutando setup-local-01-crear-base.sql..."
        & $psql -U $SuperUser -h localhost -d postgres -v ON_ERROR_STOP=1 -f "DB/setup-local-01-crear-base.sql"
        if ($LASTEXITCODE -ne 0) {
            throw "Falló el script DB/setup-local-01-crear-base.sql (exit $LASTEXITCODE). Ver docs/EJECUTAR-EL-BACKEND.md (solución de problemas)."
        }

        Write-Host "  Ejecutando setup-local-02-permisos.sql..."
        & $psql -U $SuperUser -h localhost -d academia_cursos -v ON_ERROR_STOP=1 -f "DB/setup-local-02-permisos.sql"
        if ($LASTEXITCODE -ne 0) {
            throw "Falló el script DB/setup-local-02-permisos.sql (exit $LASTEXITCODE). Ver docs/EJECUTAR-EL-BACKEND.md (solución de problemas)."
        }

        Write-Host "  Verificando permisos de vita_user sobre el esquema public..."
        $sqlCheck = "SELECT has_schema_privilege('vita_user','public','CREATE') AND has_schema_privilege('vita_user','public','USAGE');"
        $check = (& $psql -U $SuperUser -h localhost -d academia_cursos -t -A -c $sqlCheck | Out-String).Trim()
        if ($LASTEXITCODE -ne 0 -or $check -ne "t") {
            throw "vita_user no tiene permisos CREATE/USAGE sobre el esquema public (resultado: '$check'). El paso 02 no surtio efecto. Ver docs/EJECUTAR-EL-BACKEND.md (solucion de problemas)."
        }

        Write-Host "  Base lista." -ForegroundColor Green
    }
    finally {
        Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    }
}
else {
    Write-Host "[1/3] Base de datos: omitido (-SkipDatabase)." -ForegroundColor DarkGray
}

# --- e) secretos ---
$secretKeys = @(
    "Seeds:DemoUsers:AdminPassword",
    "Seeds:DemoUsers:InstructorPassword",
    "Seeds:DemoUsers:StudentPassword"
)

if (-not $SkipSecrets) {
    Write-Host "[2/3] User Secrets del seeder..." -ForegroundColor Cyan
    Write-Host "  Valores: tarjeta Trello 'Seeds en el API'."

    # OJO: sin 2>&1. En PowerShell 5.1, redirigir stderr de un comando nativo
    # con $ErrorActionPreference = "Stop" lanza un error terminante y mata el
    # script. Se baja la preferencia solo alrededor de esta llamada.
    # OJO 2: 'dotnet user-secrets list' imprime los VALORES en texto plano.
    # $listText contiene contraseñas: NUNCA lo escribas en pantalla ni en logs.
    $listText = ""
    $prevEap = $ErrorActionPreference
    try {
        $ErrorActionPreference = "Continue"
        $listText = (& dotnet user-secrets list --project Vita.Api | Out-String)
    }
    catch {
        $listText = ""
    }
    finally {
        $ErrorActionPreference = $prevEap
    }

    foreach ($key in $secretKeys) {
        $exists = $listText -match [regex]::Escape($key)
        if ($exists -and -not $Force) {
            Write-Host "  Ya configurado: $key (usa -Force para sobrescribir)."
            continue
        }

        $label = if ($Force -and $exists) { "Nueva contraseña para $key" } else { "Contraseña para $key" }
        $sec = Read-Host $label -AsSecureString
        $plain = ConvertFrom-SecureStringPlain -Secure $sec
        try {
            & dotnet user-secrets set $key $plain --project Vita.Api | Out-Null
            if ($LASTEXITCODE -ne 0) {
                Stop-WithError "No se pudo guardar el secreto $key."
            }
            Write-Host "  Guardado: $key"
        }
        finally {
            $plain = $null
        }
    }
}
else {
    Write-Host "[2/3] User Secrets: omitido (-SkipSecrets)." -ForegroundColor DarkGray
}

# --- f) cierre ---
Write-Host "[3/3] Arranque..." -ForegroundColor Cyan

if ($NoRun) {
    Write-Host "Omitido (-NoRun). Para levantar:" -ForegroundColor Yellow
    Write-Host "  dotnet restore"
    Write-Host "  dotnet run --project Vita.Api"
    Write-Host "Swagger: http://localhost:5044/swagger"
    exit 0
}

Write-Host "Swagger quedará en http://localhost:5044/swagger (Ctrl+C para detener)."
& dotnet restore
if ($LASTEXITCODE -ne 0) {
    Stop-WithError "dotnet restore fallo."
}

& dotnet run --project Vita.Api
exit $LASTEXITCODE
