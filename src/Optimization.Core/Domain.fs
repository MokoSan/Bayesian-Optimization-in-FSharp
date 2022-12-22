[<AutoOpen>]
module Optimization.Domain

open MathNet.Numerics.LinearAlgebra
open System.Collections.Generic
open Microsoft.Diagnostics.Tracing.Etlx

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
        ObservedDataPoints       : List<DataPoint>
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
and Goal =
    | Max
    | Min

// Results.
and DataPoint         = { X : double; Y : double }
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
