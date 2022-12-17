[<AutoOpen>]
module Domain

open MathNet.Numerics.LinearAlgebra
open System.Collections.Generic

type Goal =
    | Max
    | Min

type GaussianModel =
    {
        GaussianProcess : GaussianProcess 
        Query           : double -> double
        Inputs          : List<double> 
    }
and GaussianProcess = 
    { 
        SquaredExponentialKernelParameters : SquaredExponentialKernelParameters 
        DataPoints                         : List<DataPoint>
        mutable CovarianceMatrix           : Matrix<double>
    }
and DataPoint       = { X : double; Y : double }
and SquaredExponentialKernelParameters = { LengthScale : double; Variance : double }

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
        Input                    : List<DataPoint>
        AcquistionFunctionResult : List<AcquisitionFunctionResult>
        EstimationResult         : List<EstimationResult>
    }
