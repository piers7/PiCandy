// FAKE build script for PiCandy

// Note that FAKE has a few operators that are aren't documented very well
// !! is a file pattern match, *results of which are IEnumerable*
// @@ does Path.Combine

// include Fake lib
#r @"packages\FAKE\tools\FakeLib.dll"

open Fake
open Fake.Testing.NUnit3
open System.IO
open System

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

let stringAsOption = function 
    | "" | null -> None 
    | x -> Some(x)
let getBuildParamOrNone = getBuildParam >> stringAsOption
let defaultOption value defaultValue = value |> function | None -> defaultValue | _ -> value 
//let deployTarget = getBuildParamOrDefault "deployTarget" "\\pierspi2\dev"
let deployTarget = getBuildParamOrDefault "deployTarget" @"\\pizero1\pi\dev"
/// option coallese operator
let (|?) = defaultArg
/// dictionary build helper 
let (=>) a b = a,box b

// Build values (some of which are parameterised)
let solutionName = "PiCandy.sln"
let solutionDir = __SOURCE_DIRECTORY__
let outputDir = solutionDir @@ "bin"
let tempDir = solutionDir @@ ".temp"
let packagesDir = solutionDir @@ "packages"
let buildConfig = getBuildParamOrDefault "buildConfig" "Debug"  // eg build.cmd buildConfig=Release
let buildNumber = getBuildParamOrNone "buildNumber" |> defaultOption <| TeamCityBuildNumber
let buildTag = getBuildParam "buildTag" |> function
        | "master" -> None    // special case, otherwise impossible to pass empty tag on cmdline
        | "" | null -> Some Environment.MachineName
        | other -> Some other 

// Read release notes & version info from RELEASE_NOTES.md
// key properties - AssemblyVersion and NugetVersion (semver)
// let releaseDetails = 
//     File.ReadLines "RELEASE_NOTES.md" 
//     |> ReleaseNotesHelper.parseReleaseNotes

// printfn "%s is building %s on %s, wordlength = %d" Environment.UserName (releaseDetails.SemVer.ToString()) Environment.MachineName (IntPtr.Size)
// TeamCityHelper.SetBuildNumber releaseDetails.NugetVersion

/// helper to locate files relative to solution dir
/// note 'includes' parameter (string list) is globbed - ie can be wildcarded
let files includes = 
  { BaseDirectory = solutionDir
    Includes = includes
    Excludes = [] } 


/// Cleans project outputs (and ensures folders exist)
Target "Clean" (fun _ ->
    !! outputDir
    ++ tempDir
    // ++ (solutionDir @@ "*/bin/")
    // -- (solutionDir @@ "packages/**")
    |> CleanDirs
)

/// Cleans all project output directories. Not normally needed (and VS tends to lock files)
Target "CleanAll" (fun _ -> 
    !! (solutionDir @@ "*/*.*proj")
    |> Seq.map (System.IO.Path.GetDirectoryName >> (fun dir -> dir @@ "bin"))
    |> CleanDirs
)

Target "Build" (fun _ ->
    let buildProperties = 
        [
            "Configuration", buildConfig;
            // for safety's sake we override project-specific out dirs
            // so that we always know where the output artefacts are
            "OutputPath", sprintf @"bin\%s" buildConfig;
        ]

    files [solutionName]
    |> MSBuild "" "Build" buildProperties
    |> ignore
)

Target "Test" (fun _ ->
    // Find all testing projets, and test their primary output assembly
    // (prevents double-execution of test assemblies if reused)
    files [ @".*Tests\*Tests.*proj" ]
        |> Seq.choose (fun projectPath ->
            let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath);
            let projectDir = System.IO.Path.GetDirectoryName(projectPath);
            let expectedAssemblyName = projectDir @@ "bin" @@ buildConfig @@ projectName + ".dll"
            match File.Exists(expectedAssemblyName) with
                | true -> Some(expectedAssemblyName)
                | false -> None
        )
    |> NUnit3 (fun p -> { p with 
                            // OutputDir = outputDir @@ "TestConsoleMessages.txt"; // badly documented - captures console output to *file path*
                            ResultSpecs = [outputDir @@ "TestResults.xml"];
                            // When running in TeamCity, don't halt build on failing tests (let TeamCity deal with that)
                            ErrorLevel = match TeamCityVersion with | Some(_) -> DontFailBuild | _ -> p.ErrorLevel;
                        })
)

Target "Deploy" (fun _ ->
    [
        (!! @"PiCandy.ServerHost\bin\debug\**", deployTarget @@ "\PiCandy.ServerHost-bin");
        (!! @"RpiWs2812OpcServer\bin\debug\**", deployTarget @@ "\RpiWs2812OpcServer-bin");
    ]
    |> Seq.iter (fun (source,target) -> 
        printfn "Deploying to %s" target
        source |> FileHelper.CopyFiles target
    )
)

Target "Default" DoNothing
Target "All" DoNothing
Target "NoOp" DoNothing

// Dependencies
// Actually I don't find this syntax particularly easy to wrap my head around
"Clean"
    ==> "Build"
    ==> "Default"

"Clean" 
    ==> "Build" 
    ==> "Test"
    ==> "All"

"Clean" 
    ==> "Build" 
    ==> "Deploy"

// start build
RunTargetOrDefault "Default"