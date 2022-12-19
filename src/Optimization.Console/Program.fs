open System
open Model
open System.Collections.Generic
open MathNet.Numerics.LinearAlgebra
open Microsoft.Diagnostics.Tracing.Analysis
open MathNet.Numerics
open System.Diagnostics
open Charting

// Test paths.
[<Literal>]
let workload : string = @"C:\Users\mukun\source\repos\FSharpAdvent_2022\src\Workloads\SimpleWorkload_1\bin\Debug\net7.0\SimpleWorkload_1.exe"
[<Literal>]
let burstyAllocator : string = @"C:\Users\mukun\source\repos\FSharpAdvent_2022\src\Workloads\BurstyAllocator\bin\Release\net6.0\BurstyAllocator.exe"
[<Literal>]
let highMemory_BurstyAllocator : string = @"C:\Users\mukun\source\repos\FSharpAdvent_2022\src\Workloads\HighMemory_BurstyAllocations\bin\Release\net6.0\HighMemory_BurstyAllocations.exe"

let query_trace (input : double) : double = 

    let p = new Process()
    p.StartInfo.FileName        <- workload
    p.StartInfo.UseShellExecute <- false
    p.StartInfo.Arguments       <- $"--input {int input}"

    let stopWatch = Stopwatch()
    stopWatch.Start()

    p.Start()       |> ignore
    p.WaitForExit() |> ignore

    p.Dispose()

    stopWatch.Stop()

    let result = double stopWatch.ElapsedMilliseconds / 1000.
    printfn "%A -> %A" input result
    result

let test_model_sin() : GaussianModel =
    let gaussianProcess : GaussianProcess = 
        { 
            KernelFunction = SquaredExponentialKernel { LengthScale = 0.1; Variance = 1 }
            ObservedDataPoints                 = List<DataPoint>()
            CovarianceMatrix                   = Matrix<double>.Build.Dense(1, 1)
        }

    let sinObjectiveFunction : ObjectiveFunction = QueryContinuousFunction Trig.Sin
    createModel gaussianProcess sinObjectiveFunction -Math.PI Math.PI 300

let test_model_simpleworkload_1() : GaussianModel =
    let gaussianProcess : GaussianProcess = 
        { 
            KernelFunction = SquaredExponentialKernel { LengthScale = 0.1; Variance = 1 }
            ObservedDataPoints       = List<DataPoint>()
            CovarianceMatrix = Matrix<double>.Build.Dense(1, 1)
        }

    let queryProcessInfo : QueryProcessInfo = { WorkloadPath = workload; ApplyArguments = (fun input -> $"--input {input}") } 
    let queryProcessObjectiveFunction : ObjectiveFunction = (QueryProcessByElapsedTimeInSeconds queryProcessInfo)
    createModel gaussianProcess queryProcessObjectiveFunction 0 5 30

let test_model_burstyallocator() : GaussianModel =
    let gaussianProcess : GaussianProcess = 
        { 
            KernelFunction = SquaredExponentialKernel { LengthScale = 0.1; Variance = 1 }
            ObservedDataPoints       = List<DataPoint>()
            CovarianceMatrix = Matrix<double>.Build.Dense(1, 1)
        }

    let queryProcessByTraceLog : QueryProcessInfoByTraceLog = 
        { 
            WorkloadPath = highMemory_BurstyAllocator 
            ApplyArguments = (fun input -> "") 
            ApplyEnvironmentVariables = (fun input -> [("COMPlus_GCHeapCount", ( (int ( Math.Round(input))).ToString("X") )); ("COMPlus_GCServer", "1")] |> Map.ofList)
            TraceLogApplication = (fun (traceLog : Microsoft.Diagnostics.Tracing.Etlx.TraceLog) ->

                let eventSource : Microsoft.Diagnostics.Tracing.Etlx.TraceLogEventSource = traceLog.Events.GetSource()

                eventSource.NeedLoadedDotNetRuntimes() |> ignore
                eventSource.Process()                  |> ignore

                let burstyAllocatorProcess : TraceProcess = eventSource.Processes() |> Seq.find(fun p -> p.Name.Contains "BurstyAllocations")
                let managedProcess         : TraceLoadedDotNetRuntime = burstyAllocatorProcess.LoadedDotNetRuntime()
                managedProcess.GC.Stats().GetGCPauseTimePercentage()
            )

            TraceParameters = "/GCCollectOnly"
            OutputPath = "./Result_2"
        }

    let queryProcessObjectiveFunction : ObjectiveFunction = QueryProcessByTraceLog queryProcessByTraceLog
    createModelWithDiscreteInputs gaussianProcess queryProcessObjectiveFunction 1 System.Environment.ProcessorCount 300

let model    : GaussianModel      = test_model_sin()
let optima   : OptimaResults      = findOptima model Goal.Max 20 
printfn "Optima: %A" optima.Optima

open Plotly.NET

let chart : GenericChart.GenericChart = chartResult optima.ExplorationResults.FinalResult (Nullable(optima.Optima.X)) "Final Result" 

let charts : GenericChart.GenericChart seq = 
    chartAllResults optima 

saveChart "./Saved.png" chart
saveCharts "./Base" charts

saveGif "./Base" "./Base/Combined.gif"
