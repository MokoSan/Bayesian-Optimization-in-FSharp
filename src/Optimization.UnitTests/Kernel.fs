module Kernel 

open NUnit.Framework
open Optimization.Domain
open Optimization.Kernel 

[<Test>]
let ``Squared Exponential Kernel Computation When Left and Right Are Equal Should Return The Variance`` () =
    let variance      : double = 1.
    let parameters    : SquaredExponentialKernelParameters = { LengthScale = 0.1; Variance = variance }
    let kernelCompute : double = squaredExponentialKernelCompute parameters 1 1
    Assert.AreEqual(kernelCompute, variance)

[<Test>]
let ``Squared Exponential Kernel Computation When Left and Right Are Not Equal But Far Apart Should Use The Squared Exponential Kernel Formula`` () =
    let parameters    : SquaredExponentialKernelParameters = { LengthScale = 0.1; Variance = 1. }
    let kernelCompute : double = squaredExponentialKernelCompute parameters 40 1 
    Assert.AreEqual(kernelCompute, 0) 

[<Test>]
let ``Squared Exponential Kernel Computation When Left and Right Are Not Equal But Not Far Apart Should Use The Squared Exponential Kernel Formula`` () =
    let parameters    : SquaredExponentialKernelParameters = { LengthScale = 0.1; Variance = 1. }
    let kernelCompute : double = squaredExponentialKernelCompute parameters 3 4 
    Assert.AreEqual(kernelCompute, 1.9287498479639315E-22) 
