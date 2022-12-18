[<AutoOpen>]
module Domain

open MathNet.Numerics.LinearAlgebra
open System.Collections.Generic
open Microsoft.Diagnostics.Tracing.Etlx

type Goal =
    | Max
    | Min

// Helper Types.
type WorkloadPath   = string
type ApplyArguments = string -> string
type LengthScale    = double
// TODO: Add constrain to make this non-negative.
type Variance       = double

type GaussianModel    =
    {
        GaussianProcess   : GaussianProcess
        ObjectiveFunction : ObjectiveFunction
        Inputs            : double list
    }
and GaussianProcess   = 
    { 
        KernelFunction                     : KernelFunction
        // TODO: Create a separate abstraction handling the min, max, updation and access of this. 
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
        WorkloadPath              : WorkloadPath
        ApplyArguments            : ApplyArguments
        ApplyEnvironmentVariables : double -> Map<string, string>
        TraceLogApplication       : TraceLog -> double
        TraceParameters           : string
        OutputPath                : string
    }
and KernelFunction =
    | SquaredExponentialKernel of SquaredExponentialKernelParameters
    
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
