module TraceCollector

open System
open System.Diagnostics
open System.Net
open System.IO

[<Literal>]
let PERFVIEW_RELEASE_PATH = "https://github.com/microsoft/perfview/releases/download/v3.0.6/PerfView.exe"

let startTrace (traceParameters : string) (path : string) : string * Process =

    let dependenciesDirectory : string = Path.Combine ( Environment.CurrentDirectory, "dependencies" )
    let perfViewPath : string = Path.Combine( dependenciesDirectory, "PerfView.exe" )

    if Directory.Exists ( dependenciesDirectory ) && File.Exists( perfViewPath ) then ()
    else 
        use webclient : WebClient = new WebClient()
        Directory.CreateDirectory dependenciesDirectory |> ignore
        webclient.DownloadFile(PERFVIEW_RELEASE_PATH, perfViewPath) 

    let sessionId : string = Guid.NewGuid().ToString()
    let logPath   : string = path + ".log"
    let command   : string = $"start {traceParameters} /NoGUI  /AcceptEULA /Merge:true /DataFile:{path} /LogFile:{logPath}"

    use startProcess : Process = new Process()
    startProcess.StartInfo.FileName  <- perfViewPath
    startProcess.StartInfo.Arguments <- command
    startProcess.Start() |> ignore

    command, startProcess

let stopTrace (startCommand : string) : unit =

    // Assumption: if you started PerfView.exe, it exists in the same spot.
    let dependenciesDirectory : string = Path.Combine ( Environment.CurrentDirectory, "dependencies" )
    let perfViewPath : string = Path.Combine( dependenciesDirectory, "PerfView.exe" )

    use stopProcess : Process = new Process()
    stopProcess.StartInfo.FileName  <- perfViewPath
    stopProcess.StartInfo.Arguments <- startCommand.Replace("start", "stop") 
    stopProcess.Start() |> ignore
    stopProcess.WaitForExit()
