module Kernel

open System

let squaredExponentialKernelCompute (parameters : SquaredExponentialKernelParameters) (left : double) (right : double) : double = 
    if left = right then parameters.Variance
    else
        let squareDistance : double = Math.Pow((left - right), 2)
        parameters.Variance * Math.Exp( -squareDistance / ( parameters.LengthScale * parameters.LengthScale * 2. ))
