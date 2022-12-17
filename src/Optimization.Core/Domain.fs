[<AutoOpen>]
module Domain

open MathNet.Numerics.LinearAlgebra
open System.Collections.Generic
open Microsoft.Diagnostics.Tracing.Etlx

type Goal =
    | Max
    | Min

type WorkloadPath   = string
type ApplyArguments = string -> string

type GaussianModel    =
    {
        GaussianProcess   : GaussianProcess 
        ObjectiveFunction : ObjectiveFunction 
        Inputs            : double list
    }
and GaussianProcess   = 
    { 
        SquaredExponentialKernelParameters : SquaredExponentialKernelParameters 
        ObservedDataPoints                 : List<DataPoint>
        mutable CovarianceMatrix           : Matrix<double>
    }
and DataPoint         = { X : double; Y : double }
and SquaredExponentialKernelParameters = { LengthScale : double; Variance : double }
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
        WorkloadPath         : WorkloadPath
        ApplyArguments       : ApplyArguments
        EnvironmentVariables : double -> Map<string, string>
        TraceLogApplication  : TraceLog -> double
        TraceParameters      : string
        OutputPath           : string
    }

type EstimationResult =
    { 
        Mean       : double
        LowerBound : double
        UpperBound : double
        Input      : double
    }

type AcquisitionFunctionResult = { Input : double; AcquisitionResult : double }

type ModelResult = 
    { 
        ObservedDataPoints       : IReadOnlyList<DataPoint>
        AcquistionFunctionResult : IReadOnlyList<AcquisitionFunctionResult>
        EstimationResult         : IReadOnlyList<EstimationResult>
    }
