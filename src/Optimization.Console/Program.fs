open System
open Model
open System.Linq
open System.Collections.Generic
open MathNet.Numerics.LinearAlgebra
open Microsoft.Diagnostics.Tracing.Analysis
open MathNet.Numerics
open System.Diagnostics


[<Literal>]
let workload : string = @"C:\Users\mukun\source\repos\FSharpAdvent_2022\src\Workloads\SimpleWorkload_1\bin\Debug\net7.0\SimpleWorkload_1.exe"

[<Literal>]
let burstyAllocator : string = @"C:\Users\mukun\source\repos\FSharpAdvent_2022\src\Workloads\BurstyAllocator\bin\Debug\net6.0\BurstyAllocator.exe"

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
            SquaredExponentialKernelParameters = { LengthScale = 0.1; Variance = 1 }
            ObservedDataPoints                 = List<DataPoint>()
            CovarianceMatrix                   = Matrix<double>.Build.Dense(1, 1)
        }

    let sinObjectiveFunction : ObjectiveFunction = QueryContinuousFunction Trig.Sin
    createModel gaussianProcess sinObjectiveFunction -Math.PI Math.PI 300 

let test_model_simpleworkload_1() : GaussianModel =
    let gaussianProcess : GaussianProcess = 
        { 
            SquaredExponentialKernelParameters = { LengthScale = 0.1; Variance = 1 }
            ObservedDataPoints       = List<DataPoint>()
            CovarianceMatrix = Matrix<double>.Build.Dense(1, 1)
        }

    let queryProcessInfo : QueryProcessInfo = { WorkloadPath = workload; ApplyArguments = (fun input -> $"--input {input}") } 
    let queryProcessObjectiveFunction : ObjectiveFunction = (QueryProcessByElapsedTimeInSeconds queryProcessInfo)
    createModel gaussianProcess queryProcessObjectiveFunction 0 5 300 

let test_model_burstyallocator() : GaussianModel =
    let gaussianProcess : GaussianProcess = 
        { 
            SquaredExponentialKernelParameters = { LengthScale = 0.1; Variance = 1 }
            ObservedDataPoints       = List<DataPoint>()
            CovarianceMatrix = Matrix<double>.Build.Dense(1, 1)
        }

    let queryProcessByTraceLog : QueryProcessInfoByTraceLog = 
        { 
            WorkloadPath = burstyAllocator 
            ApplyArguments = (fun input -> "") 
            EnvironmentVariables = (fun input -> [("COMPlus_GCHeapCount", (int input).ToString() )] |> Map.ofList) 
            TraceLogApplication = (fun (traceLog : Microsoft.Diagnostics.Tracing.Etlx.TraceLog) ->

                let eventSource = traceLog.Events.GetSource()

                eventSource.NeedLoadedDotNetRuntimes() |> ignore
                eventSource.Process()                  |> ignore

                let burstyAllocator = eventSource.Processes() |> Seq.find(fun p -> p.Name.Contains "BurstyAllocator")
                let managedProcess : TraceLoadedDotNetRuntime = burstyAllocator.LoadedDotNetRuntime()
                managedProcess.GC.Stats().MeanPauseDurationMSec
            )
            TraceParameters = "/GCCollectOnly"
            OutputPath = "./Result"
        } 

    let queryProcessObjectiveFunction : ObjectiveFunction = QueryProcessByTraceLog queryProcessByTraceLog
    createModel gaussianProcess queryProcessObjectiveFunction 1 5 300 

let model = test_model_burstyallocator()
let extrema = findOptima model Goal.Min 10 
printfn "%A" ( extrema.ObservedDataPoints.MinBy(fun e -> e.Y ))
