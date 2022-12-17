open System
open System.Linq
open Model
open System.Collections.Generic
open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics
open System.Diagnostics

[<Literal>]
let workload : string = @"C:\Users\mukun\source\repos\FSharpAdvent_2022\src\Workloads\SimpleWorkload_1\bin\Debug\net7.0\SimpleWorkload_1.exe"

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

let model = test_model_simpleworkload_1()
let extrema = findOptima model Goal.Min 40
Console.WriteLine( extrema.Input.MinBy(fun e -> e.Y))
