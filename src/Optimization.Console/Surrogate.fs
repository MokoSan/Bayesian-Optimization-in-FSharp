module Surrogate 

open System.Collections.Generic
open System.Linq
open MathNet.Numerics.LinearAlgebra
open System
open Kernel

// TODO: Comment and clean.
let estimateAtRange (model: GaussianModel) : List<EstimationResult> = 
    let estimateAtPoint (gaussianProcess : GaussianProcess) (input : double) : EstimationResult = 

        let kStar : double[] =
            gaussianProcess.DataPoints
                           .Select(fun dp -> squaredExponentialKernelCompute gaussianProcess.SquaredExponentialKernelParameters input dp.X)
                           .ToArray()

        let yTrain : double[] =
            gaussianProcess.DataPoints
                           .Select(fun dp -> dp.Y)
                           .ToArray()

        let ks         : Vector<double> = Vector<double>.Build.Dense kStar
        let f          : Vector<double> = Vector<double>.Build.Dense yTrain
        let common     : Vector<double> = gaussianProcess.CovarianceMatrix.Inverse().Multiply ks
        let mu         : double         = common.DotProduct f
        let confidence : double         = Math.Abs(-common.DotProduct(ks) + squaredExponentialKernelCompute gaussianProcess.SquaredExponentialKernelParameters input input )

        { Mean = mu; LowerBound = mu - confidence; UpperBound = mu + confidence; Input = input }

    model.Inputs.Select(fun x -> estimateAtPoint model.GaussianProcess x).ToList()
