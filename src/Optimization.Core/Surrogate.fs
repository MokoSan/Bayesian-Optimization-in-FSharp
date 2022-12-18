module Surrogate 

open System.Collections.Generic
open System.Linq
open MathNet.Numerics.LinearAlgebra
open System
open Kernel

// TODO: Comment and clean.
let predict (model: GaussianModel) : IEnumerable<EstimationResult> =
    let predictPoint (gaussianProcess : GaussianProcess) (input : double) : EstimationResult = 

        let matchedKernelFunction : (double -> double -> double) = getKernelFunction model

        let kStar : double[] =
            gaussianProcess.ObservedDataPoints
                           .Select(fun dp -> matchedKernelFunction input dp.X)
                           .ToArray()

        let yTrain : double[] =
            gaussianProcess.ObservedDataPoints
                           .Select(fun dp -> dp.Y)
                           .ToArray()

        let ks         : Vector<double> = Vector<double>.Build.Dense kStar
        let f          : Vector<double> = Vector<double>.Build.Dense yTrain

        // Common helper term. 
        let common     : Vector<double> = gaussianProcess.CovarianceMatrix.Inverse().Multiply ks
        // muStar = Kstar^T * K^-1 * f = common dot f
        let mu         : double         = common.DotProduct f
        let confidence : double         = Math.Abs( matchedKernelFunction input input  - common.DotProduct(ks) )

        { Mean = mu; LowerBound = mu - confidence; UpperBound = mu + confidence; Input = input }

    model.Inputs.Select(fun x -> predictPoint model.GaussianProcess x)
