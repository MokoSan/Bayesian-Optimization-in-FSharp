[<AutoOpen>]
module Optimization.Domain

open MathNet.Numerics.LinearAlgebra
open System.Collections.Generic
open Microsoft.Diagnostics.Tracing.Etlx

type Goal =
    | Max
    | Min

// Helper Types.
type WorkloadPath   = string
type ApplyArguments = string -> string

type GaussianModel    =
    {
        GaussianProcess     : GaussianProcess
        ObjectiveFunction   : ObjectiveFunction
        Inputs              : double list
    }
and GaussianProcess   = 
    { 
        KernelFunction                     : KernelFunction
        // TODO: Create a separate abstraction handling the min, max, updation and access of this. 
        ObservedDataPoints                 : List<DataPoint>
        mutable CovarianceMatrix           : Matrix<double>
    }
and KernelFunction    =
    | SquaredExponentialKernel of SquaredExponentialKernelParameters
and SquaredExponentialKernelParameters = { LengthScale : double; Variance : double }
and DataPoint         = { X : double; Y : double }
and ObjectiveFunction =
    | QueryContinuousFunction            of (double -> double)
    | QueryProcessByElapsedTimeInSeconds of QueryProcessInfo
    | QueryProcessByTraceLog             of QueryProcessInfoByTraceLog
and QueryProcessInfo  = 
    {
        WorkloadPath   : WorkloadPath
        ApplyArguments : ApplyArguments
    }
and QueryProcessInfoByTraceLog  = 
    {
        WorkloadPath              : WorkloadPath
        ApplyArguments            : ApplyArguments
        ApplyEnvironmentVariables : double   -> Map<string, string>
        TraceLogApplication       : TraceLog -> double
        TraceParameters           : string
        OutputPath                : string
    }
    
type ModelResult = 
    { 
        ObservedDataPoints : IReadOnlyList<DataPoint>
        AcquisitionResults : IReadOnlyList<AcquisitionFunctionResult>
        PredictionResults  : IReadOnlyList<PredictionResult>
    }
and AcquisitionFunctionResult = { Input : double; AcquisitionScore: double }
and PredictionResult =
    { 
        Input      : double
        Mean       : double
        LowerBound : double
        UpperBound : double
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
