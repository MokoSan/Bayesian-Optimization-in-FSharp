open Optimization.Domain
open System
open Plotly.NET

open Optimization.Charting
open Optimization.Model
open System.IO
open Microsoft.Diagnostics.Tracing.Analysis

let WORKLOAD_PATH : string = Path.Combine( __SOURCE_DIRECTORY__, "../../src/Workloads/HighMemory_BurstyAllocations/bin/Release/net6.0/HighMemory_BurstyAllocations.exe")
let basePath = Path.Combine( __SOURCE_DIRECTORY__, "resources", "Traces_All")

let iterations : int = System.Environment.ProcessorCount
let resolution : int = System.Environment.ProcessorCount

let getHighMemoryBurstyAllocationsModel() : GaussianModel =
    let gaussianProcess : GaussianProcess = createProcessWithSquaredExponentialKernel { LengthScale = 0.1;  Variance = 1. }

    let queryProcessByTraceLog : QueryProcessInfoByTraceLog = 
        { 
            WorkloadPath = WORKLOAD_PATH 
            ApplyArguments = (fun input -> "") 
            ApplyEnvironmentVariables = (fun input -> [("COMPlus_GCHeapCount", ( (int ( Math.Round(input))).ToString("X") )); ("COMPlus_GCServer", "1")] |> Map.ofList)
            TraceLogApplication = (fun (traceLog : Microsoft.Diagnostics.Tracing.Etlx.TraceLog) ->

                let eventSource : Microsoft.Diagnostics.Tracing.Etlx.TraceLogEventSource = traceLog.Events.GetSource()

                eventSource.NeedLoadedDotNetRuntimes() |> ignore
                eventSource.Process()                  |> ignore

                let burstyAllocatorProcess : TraceProcess = eventSource.Processes() |> Seq.find(fun p -> p.Name.Contains "HighMemory_BurstyAllocations")
                let managedProcess         : TraceLoadedDotNetRuntime = burstyAllocatorProcess.LoadedDotNetRuntime()
                managedProcess.GC.Stats().GetGCPauseTimePercentage()
            )

            TraceParameters = "/GCCollectOnly"
            OutputPath      = basePath
        }

    let queryProcessObjectiveFunction : ObjectiveFunction = QueryProcessByTraceLog queryProcessByTraceLog
    createModelWithDiscreteInputs gaussianProcess queryProcessObjectiveFunction 1 System.Environment.ProcessorCount resolution

let model    : GaussianModel      = getHighMemoryBurstyAllocationsModel()
let optima   : OptimaResults      = findOptima model Goal.Min iterations
printfn "Optima: GC Pause Time Percentage is minimized when the input is %A at %A" optima.Optima.X optima.Optima.Y

let charts : GenericChart.GenericChart seq = chartAllResults optima
// Save the Charts and the Gif.
saveCharts basePath charts

saveGif basePath (Path.Combine(basePath, "./Combined.gif"))

// Check to see if we got the % correct.
let pathsToTraces : string seq = Directory.GetFiles(basePath, "*.etlx")

let getGCPauseTimePercentage(pathOfTrace : string) : double =
    use traceLog : Microsoft.Diagnostics.Tracing.Etlx.TraceLog = new Microsoft.Diagnostics.Tracing.Etlx.TraceLog(pathOfTrace)
    let eventSource : Microsoft.Diagnostics.Tracing.Etlx.TraceLogEventSource = traceLog.Events.GetSource()

    eventSource.NeedLoadedDotNetRuntimes() |> ignore
    eventSource.Process()                  |> ignore

    let burstyAllocatorProcess : TraceProcess = eventSource.Processes() |> Seq.find(fun p -> p.Name.Contains "HighMemory_BurstyAllocations")
    let managedProcess         : TraceLoadedDotNetRuntime = burstyAllocatorProcess.LoadedDotNetRuntime()
    managedProcess.GC.Stats().GetGCPauseTimePercentage()

printfn "| Heap Count | Percentage Pause Time In GC |"
printfn "| ---------- | --------------------------- |"
pathsToTraces
|> Seq.iter(fun p -> (
    printfn "|%A | %A |" (Path.GetFileNameWithoutExtension(p)) (getGCPauseTimePercentage p)
))
