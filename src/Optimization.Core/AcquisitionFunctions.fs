module Optimization.AcquisitionFunctions

open System
open System.Linq
open MathNet.Numerics.Distributions

[<Literal>]
let EXPLORATION_PARAMETER : double = 0.01
 
let expectedImprovement (parameters : AcquisitionFunctionRequest) : AcquisitionFunctionResult =

    let gaussianProcess : GaussianProcess = parameters.GaussianProcess

    let optimumValue : double =
        match parameters.Goal with 
        | Goal.Max -> gaussianProcess.ObservedDataPoints.Max(fun l -> l.Y)
        | Goal.Min -> gaussianProcess.ObservedDataPoints.Min(fun l -> l.Y)

    let baseResult : AcquisitionFunctionResult = { Input = parameters.PredictionResult.Input; AcquisitionScore = 0. }

    let predictionResult : PredictionResult = parameters.PredictionResult

    if gaussianProcess.ObservedDataPoints.Any(fun d -> d.X = predictionResult.Input) then baseResult 
    else
        let Δ : double = predictionResult.Mean - optimumValue - EXPLORATION_PARAMETER 
        let σ : double = (predictionResult.UpperBound - predictionResult.LowerBound) / 2.
        if σ = 0 then baseResult
        else 
            let z                   : double = Δ / σ 
            let exploitationFactor  : double = Δ * Normal.CDF(0, 1, z)
            let explorationFactor   : double = σ * Normal.PDF(0, 1, z)
            let expectedImprovement : double = exploitationFactor + explorationFactor

            { Input = predictionResult.Input; AcquisitionScore = Math.Max(expectedImprovement, 0) }
