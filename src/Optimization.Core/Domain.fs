[<AutoOpen>]
module Optimization.Domain

open System
open MathNet.Numerics.LinearAlgebra
open System.Collections.Generic
open Microsoft.Diagnostics.Tracing.Etlx

type DataPoint = { X : double; Y : double }
type Goal =
    | Max
    | Min

// Class that handles the optima calculation in O(1) and O(1) Contains behavior for inputs.
[<Sealed>]
type ObservationDataPoints (goal : Goal) = 
    let observations         : List<DataPoint> = List<DataPoint>()
    let distinctObservations : HashSet<double> = HashSet<double>()
    let mutable optima : double = nan

    member this.Goal with get()         : Goal                     = goal
    member this.Observations with get() : IReadOnlyList<DataPoint> = observations
    member this.Optima with get()       : double                   = optima
    member this.Count with get()        : int                      = observations.Count

    // When we add, compute the optima.
    member this.Add (dataPoint : DataPoint) : unit =
        let updateOptima : double =
            match goal with
            | Max -> Math.Max(dataPoint.Y, optima)
            | Min -> Math.Min(dataPoint.Y, optima)

        optima <- updateOptima
        observations.Add dataPoint           |> ignore
        distinctObservations.Add dataPoint.X |> ignore

    // Check if the input already exists.
    member this.Contains(input : double) : bool =
        distinctObservations.Contains input 

type GaussianModel =
    {
        GaussianProcess     : GaussianProcess
        ObjectiveFunction   : ObjectiveFunction
        Inputs              : double list
        AcquisitionFunction : AcquisitionFunction
    }
and GaussianProcess = 
    { 
        KernelFunction           : KernelFunction
        ObservedDataPoints       : ObservationDataPoints 
        mutable CovarianceMatrix : Matrix<double>
    }

// Objective Function.
and ObjectiveFunction =
    | QueryContinuousFunction            of (double -> double)
    | QueryProcessByElapsedTimeInSeconds of QueryProcessInfo
    | QueryProcessByTraceLog             of QueryProcessInfoByTraceLog
and QueryProcessInfo  = 
    {
        WorkloadPath   : string 
        ApplyArguments : string -> string 
    }
and QueryProcessInfoByTraceLog  = 
    {
        WorkloadPath              : string 
        ApplyArguments            : string   -> string 
        ApplyEnvironmentVariables : double   -> Map<string, string>
        TraceLogApplication       : TraceLog -> double
        TraceParameters           : string
        OutputPath                : string
    }

// Kernel Function.
and KernelFunction =
    | SquaredExponentialKernel of SquaredExponentialKernelParameters
and SquaredExponentialKernelParameters = { LengthScale : double; Variance : double }

// Acquisition Function.
and AcquisitionFunction = 
    | ExpectedImprovement
and AcquisitionFunctionRequest =
    {
        GaussianProcess  : GaussianProcess
        PredictionResult : PredictionResult
        Goal             : Goal
    }
and AcquisitionFunctionResult = { Input : double; AcquisitionScore: double }

// Results.
and PredictionResult =
    { 
        Input      : double
        Mean       : double
        LowerBound : double
        UpperBound : double
    }
    
type ModelResult = 
    { 
        ObservedDataPoints : IReadOnlyList<DataPoint>
        AcquisitionResults : IReadOnlyList<AcquisitionFunctionResult>
        PredictionResults  : IReadOnlyList<PredictionResult>
    }
and ExplorationResults  =
    {
        IntermediateResults : IReadOnlyList<IntermediateResult> 
        FinalResult         : ModelResult
    }
and OptimaResults       = 
    {
        ExplorationResults : ExplorationResults
        Optima             : DataPoint
    }
and IntermediateResult  = 
    {
        Result    : ModelResult
        NextPoint : double
        Iteration : int
    }
