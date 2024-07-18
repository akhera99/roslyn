param (
    [string]$SourceBranch
)

function CreateMirrorBranch {
    param (
        [string]$internalRepo,
        [string]$publicRepo,
        [string]$targetBranch
    )

    git remote add internal $internalRepo
    git remote add public $publicRepo
    git fetch internal
    git fetch public
    git push internal $targetBranch
}

function AddDarcPrefix {
    return "darc/$SourceBranch"
}

$publicRepo = "https://github.com/dotnet/roslyn.git"
$internalRepo = "https://dnceng.visualstudio.com/internal/_git/dotnet-roslyn"
$targetBranch = AddDarcPrefix

CreateMirrorBranch -internalRepo $internalRepo -publicRepo $publicRepo -targetBranch $targetBranch