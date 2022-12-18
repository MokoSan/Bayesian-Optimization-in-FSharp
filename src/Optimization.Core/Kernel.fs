module Kernel

open System

let squaredExponentialKernelCompute (parameters : SquaredExponentialKernelParameters) (left : double) (right : double) : double = 
    if left = right then parameters.Variance
    else
        let squareDistance : double = Math.Pow((left - right), 2)
        parameters.Variance * Math.Exp( -squareDistance / ( parameters.LengthScale * parameters.LengthScale * 2. ))

// I ♥ Partial Application.
let getKernelFunction (model : GaussianModel) : double -> double -> double =
    match model.GaussianProcess.KernelFunction with
    | SquaredExponentialKernel parameters ->
        squaredExponentialKernelCompute parameters
