open System
open System.Linq
open Model
open System.Collections.Generic
open MathNet.Numerics.LinearAlgebra
open System.Diagnostics

[<Literal>]
let workload : string = @"C:\Users\mukun\source\repos\FSharpAdvent_2022\src\Workloads\SimpleWorkload_1\bin\Debug\net7.0\SimpleWorkload_1.exe"

let query(input : double) : double = 

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

let test_model() : GaussianModel =
    let gaussianProcess : GaussianProcess = 
        { 
            SquaredExponentialKernelParameters = { LengthScale = 0.1; Variance = 1 }
            DataPoints       = List<DataPoint>()
            CovarianceMatrix = Matrix<double>.Build.Dense(1, 1)
        }

    //createModel gaussianProcess Trig.Sin -Math.PI Math.PI 300 
    createModel gaussianProcess query 0 10 300

let model = test_model()
let extrema = findOptima model Goal.Min 40
Console.WriteLine( extrema.Input.MinBy(fun e -> e.Y))
