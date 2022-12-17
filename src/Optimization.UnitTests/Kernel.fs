module Optimization

open NUnit.Framework
open Kernel 

[<SetUp>]
let Setup () =
    ()

[<Test>]
let ``Squared Exponential Kernel Computation When Left and Right Are Equal Should Return The Variance`` () =
    let variance      : double = 1.
    let parameters    : SquaredExponentialKernelParameters = { LengthScale = 0.1; Variance = variance }
    let kernelCompute : double = squaredExponentialKernelCompute parameters 1 1
    Assert.AreEqual(kernelCompute, variance)

[<Test>]
// TODO: Fix.
let ``Squared Exponential Kernel Computation When Left and Right Are Not Equal Should Use The Squared Exponential Kernel Formula`` () =
    let parameters    : SquaredExponentialKernelParameters = { LengthScale = 0.1; Variance = 1. }
    let kernelCompute : double = squaredExponentialKernelCompute parameters 1 1
    Assert.AreEqual(kernelCompute, 1.)
