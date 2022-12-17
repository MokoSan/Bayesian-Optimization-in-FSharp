[<AutoOpen>]
module Domain

open MathNet.Numerics.LinearAlgebra
open System.Collections.Generic

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
and QueryProcessInfo  = 
    {
        WorkloadPath   : WorkloadPath
        ApplyArguments : ApplyArguments
    }

type EstimationResult =
    { 
        Mean       : double
        LowerBound : double
        UpperBound : double
        Input      : double
    }

type AcquisitionFunctionResult = { X : double; Y : double }

type ModelResult = 
    { 
        Input                    : IReadOnlyList<DataPoint>
        AcquistionFunctionResult : IReadOnlyList<AcquisitionFunctionResult>
        EstimationResult         : IReadOnlyList<EstimationResult>
    }
