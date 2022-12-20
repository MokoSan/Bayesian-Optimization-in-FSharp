module Optimization.AcquisitionFunctions

open System
open System.Linq
open MathNet.Numerics.Distributions

let expectedImprovement (gaussianProcess  : GaussianProcess) 
                        (predictionResult : PredictionResult) 
                        (goal : Goal) 
                        (explorationParameter : double) : AcquisitionFunctionResult =

    let optimumValue : double =
        match goal with 
        | Goal.Max -> gaussianProcess.ObservedDataPoints.Max(fun l -> l.Y)
        | Goal.Min -> gaussianProcess.ObservedDataPoints.Min(fun l -> l.Y)

    let baseResult : AcquisitionFunctionResult = { Input = predictionResult.Input; AcquisitionScore = 0. }

    if gaussianProcess.ObservedDataPoints.Any(fun d -> d.X = predictionResult.Input) then baseResult 
    else
        let Δ : double = predictionResult.Mean - optimumValue - explorationParameter
        let σ : double = (predictionResult.UpperBound - predictionResult.LowerBound) / 2.
        if σ = 0 then baseResult
        else 
            let z                   : double = Δ / σ 
            let exploitationFactor  : double = Δ * Normal.CDF(0, 1, z)
            let explorationFactor   : double = σ * Normal.PDF(0, 1, z)
            let expectedImprovement : double = exploitationFactor + explorationFactor

            { Input = predictionResult.Input; AcquisitionScore = Math.Max(expectedImprovement, 0) }
