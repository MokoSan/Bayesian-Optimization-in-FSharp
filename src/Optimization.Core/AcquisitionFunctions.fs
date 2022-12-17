module AcquisitionFunctions

open System
open System.Linq
open MathNet.Numerics.Distributions

let expectedImprovement (gaussianProcess : GaussianProcess) 
                        (estimationResult : EstimationResult) 
                        (goal : Goal) 
                        (explorationParameter : double) : AcquisitionFunctionResult =

    let optimumValue : double =
        match goal with 
        | Goal.Max -> gaussianProcess.ObservedDataPoints.Max(fun l -> l.Y)
        | Goal.Min -> gaussianProcess.ObservedDataPoints.Min(fun l -> l.Y)

    let baseResult : AcquisitionFunctionResult = { X = estimationResult.Input; Y = 0. }

    if gaussianProcess.ObservedDataPoints.Any(fun d -> d.X = estimationResult.Input) then baseResult 
    else
        let delta : double = estimationResult.Mean - optimumValue - explorationParameter
        // TODO: Fix. Should I actually be computing the sigma??
        let sigma : double = estimationResult.UpperBound - estimationResult.LowerBound
        if sigma = 0 then baseResult
        else 
            let z                   : double = delta / sigma
            let exploitationFactor  : double  = delta * Normal.CDF(0, 1, z)
            let explorationFactor   : double  = sigma * Normal.PDF(0, 1, z)
            let expectedImprovement : double = exploitationFactor + explorationFactor

            { X = estimationResult.Input; Y = Math.Max(expectedImprovement, 0)}
